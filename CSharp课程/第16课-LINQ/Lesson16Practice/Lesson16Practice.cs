// ============================================================================
//  第 16 课 · 练习：LINQ
//
//  ★ 本课重点：
//     1. Where/Select/OrderBy/GroupBy/Sum/Count/Any 的用法
//     2. ★ 延迟执行：查询在【遍历时】才执行，要固定结果用 .ToList()
//     3. 链式组合的威力
// ============================================================================

namespace LessonPractice
{
    // 数据模型（直接用，不用改）
    record Employee(string Name, string Dept, int Salary, int Years);

    class Lesson16Practice
    {
        static void Main()
        {
            Console.WriteLine("===== 第 16 课练习：LINQ =====\n");

            var employees = new List<Employee>
            {
                new("张三", "研发部", 15000, 3),
                new("李四", "研发部", 22000, 6),
                new("王五", "销售部", 12000, 2),
                new("赵六", "销售部", 18000, 5),
                new("钱七", "人事部", 10000, 1),
                new("孙八", "研发部", 28000, 8),
                new("周九", "人事部", 13000, 4)
            };

            // ================================================================
            // TODO 1 ｜ 基础：Where / Select / OrderBy
            // ================================================================
            // 【要求】用 LINQ 完成（每项一行）：
            //   1. 找出薪资 >= 15000 的人，打印名字
            //   2. 只要所有名字（Select）
            //   3. 按薪资降序排列，打印 "名字: 薪资"
            //   4. 找出研发部的人，按薪资升序，只要名字
            //
            // 【预期输出】
            //     薪资 >= 15000：张三, 李四, 赵六, 孙八
            //     所有名字：张三, 李四, 王五, 赵六, 钱七, 孙八, 周九
            //     按薪资降序：孙八: 28000, 李四: 22000, 赵六: 18000, 张三: 15000, 周九: 13000, 王五: 12000, 钱七: 10000
            //     研发部（薪资升序）：张三, 李四, 孙八
            //
            // 【提示】
            //     var high = employees.Where(e => e.Salary >= 15000).Select(e => e.Name).ToList();
            //     Console.WriteLine($"薪资 >= 15000：{string.Join(", ", high)}");
            //
            //     var names = employees.Select(e => e.Name).ToList();
            //
            //     var bySalary = employees.OrderByDescending(e => e.Salary)
            //                             .Select(e => $"{e.Name}: {e.Salary}");
            //     Console.WriteLine($"按薪资降序：{string.Join(", ", bySalary)}");
            //
            // 【考察点】
            //   · Where / Select / OrderByDescending 的链式组合
            //   · 养成习惯：最后加 .ToList()

            Console.WriteLine("--- TODO 1 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① Where 负责"筛人"，Select 负责"换成别的东西"：链式调用的先后就是数据的加工顺序
            // ★ 为什么每步后面都要 .ToList()？因为 Where/Select 返回的是 IEnumerable（一份"查询意图"），
            //   不立刻取结果的话，以后一改 employees，这份结果还会跟着变（下面第 4 题会亲眼看到）。
            var high = employees.Where(e => e.Salary >= 15000).Select(e => e.Name).ToList();
            // string.Join 把集合拼成一个字符串，相当于 Python 的 ", ".join(...)
            Console.WriteLine($"薪资 >= 15000：{string.Join(", ", high)}");

            // ② 只要名字：Select 相当于 Python 的 map，e => e.Name 就是"从每个元素身上取 Name"
            //   ★ 投影完 Employee 就丢了，后面再也拿不到 Salary —— 所以 Select 一般放在链子的最后
            var names = employees.Select(e => e.Name).ToList();
            Console.WriteLine($"所有名字：{string.Join(", ", names)}");

            // ③ OrderByDescending = 从大到小（OrderBy 是从小到大）
            //   ★ 排序必须写在 Select 之前：一旦投影成字符串，就再也没法按 Salary 比大小了
            var bySalary = employees.OrderByDescending(e => e.Salary)
                                    .Select(e => $"{e.Name}: {e.Salary}")
                                    .ToList();
            Console.WriteLine($"按薪资降序：{string.Join(", ", bySalary)}");

            // ④ 三步串起来：Where 筛出研发部 → OrderBy 按薪资升序 → Select 只要名字
            //   ★ 结果直接交给 string.Join 就是一个"名单"，这就是链式组合的威力：一步一个意图，读起来像句子
            var devs = employees.Where(e => e.Dept == "研发部")
                                .OrderBy(e => e.Salary)
                                .Select(e => e.Name)
                                .ToList();
            Console.WriteLine($"研发部（薪资升序）：{string.Join(", ", devs)}");

            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ 聚合运算
            // ================================================================
            // 【要求】用 LINQ 算出并打印：
            //   1. 总人数
            //   2. 总薪资支出
            //   3. 平均薪资（保留 2 位小数）
            //   4. 最高薪资 和 最低薪资
            //   5. 薪资 >= 20000 的人数
            //   6. 有没有工龄 >= 8 年的人（Any）
            //   7. 是不是所有人薪资都 >= 10000（All）
            //
            // 【预期输出】
            //     总人数：7
            //     总薪资：118000
            //     平均薪资：16857.14
            //     最高薪资：28000，最低薪资：10000
            //     薪资 >= 20000 的人数：2
            //     有 8 年以上工龄的吗？True
            //     所有人都 >= 10000 吗？True
            //
            // 【提示】
            //     Console.WriteLine($"总人数：{employees.Count()}");
            //     Console.WriteLine($"总薪资：{employees.Sum(e => e.Salary)}");
            //     Console.WriteLine($"平均薪资：{employees.Average(e => e.Salary):F2}");
            //     Console.WriteLine($"最高薪资：{employees.Max(e => e.Salary)}，最低薪资：{employees.Min(e => e.Salary)}");
            //     Console.WriteLine($"薪资 >= 20000 的人数：{employees.Count(e => e.Salary >= 20000)}");
            //     Console.WriteLine($"有 8 年以上工龄的吗？{employees.Any(e => e.Years >= 8)}");
            //     Console.WriteLine($"所有人都 >= 10000 吗？{employees.All(e => e.Salary >= 10000)}");
            //
            // 【考察点】
            //   · Count 可以带条件：Count(predicate)
            //   · Any() 不带条件 = "列表非空"，比 Count() > 0 快
            //   · All 判断"全部满足"（列表为空时返回 true，这是数学约定）

            Console.WriteLine("--- TODO 2 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① Count() 不带条件就是元素个数 —— 相当于 Python 的 len(list)
            Console.WriteLine($"总人数：{employees.Count()}");

            // ② Sum 后面接一个"投影" e => e.Salary：告诉 LINQ 把每个元素的哪个数拿出来加
            //   ★ 投影的返回值类型决定了 Sum 返回什么：int 的薪资加起来还是 int
            Console.WriteLine($"总薪资：{employees.Sum(e => e.Salary)}");

            // ③ ★ 注意 Average 返回的是 double（不是 int），所以能用 :F2 保留两位小数
            //   不写 :F2 会打出 16857.142857142857 这样一长串；F2 顺便还做了四舍五入
            Console.WriteLine($"平均薪资：{employees.Average(e => e.Salary):F2}");

            // ④ 最高、最低放同一行，中间用中文逗号"，"隔开（要和预期输出一模一样）
            Console.WriteLine($"最高薪资：{employees.Max(e => e.Salary)}，最低薪资：{employees.Min(e => e.Salary)}");

            // ⑤ ★ Count 可以直接带条件：等价于"先 Where 再 Count"，但不用建中间集合、只遍历一次
            Console.WriteLine($"薪资 >= 20000 的人数：{employees.Count(e => e.Salary >= 20000)}");

            // ⑥ Any(条件) = "存在至少一个满足的吗"：一找到就立刻返回 true，不必数完整个集合
            //   ★ Any() 不带条件 = "列表非空"，比 Count() > 0 快（不用把所有元素都数一遍）
            Console.WriteLine($"有 8 年以上工龄的吗？{employees.Any(e => e.Years >= 8)}");

            // ⑦ All(条件) = "是不是全部都满足"：只要发现一个不满足就立刻返回 false
            //   ★ 空集合调用 All 会返回 true（数学上的"空真"约定），所以别拿 All 当"列表非空"用
            Console.WriteLine($"所有人都 >= 10000 吗？{employees.All(e => e.Salary >= 10000)}");

            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ GroupBy 分组统计
            // ================================================================
            // 【要求】按部门分组，统计每个部门：
            //   1. 人数
            //   2. 总薪资
            //   3. 平均薪资（2 位小数）
            //   4. 部门里工资最高的人的名字
            //
            // 【预期输出】
            //     ========== 部门统计 ==========
            //     研发部：3 人，总薪资 65000，平均 21666.67，最高：孙八
            //     销售部：2 人，总薪资 30000，平均 15000.00，最高：赵六
            //     人事部：2 人，总薪资 23000，平均 11500.00，最高：周九
            //
            // 【提示】
            //     foreach (var g in employees.GroupBy(e => e.Dept))
            //     {
            //         int count = g.Count();
            //         int total = g.Sum(e => e.Salary);
            //         double avg = g.Average(e => e.Salary);
            //         string top = g.OrderByDescending(e => e.Salary).First().Name;
            //         Console.WriteLine($"{g.Key}：{count} 人，总薪资 {total}，平均 {avg:F2}，最高：{top}");
            //     }
            //
            // 【考察点】
            //   · GroupBy 返回的每一项是一个 IGrouping，有 .Key（分组的键）
            //   · 分组内部还能继续用 LINQ（Sum/Average/OrderBy/First）
            //   · 这是"分组统计报表"的标准写法

            Console.WriteLine("--- TODO 3 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            Console.WriteLine("========== 部门统计 ==========");

            // GroupBy 按"键"把元素分进不同的桶：研发部一桶、销售部一桶……
            // ★ 每一项都是一个 IGrouping<TKey, TElement>：既能当"键"读（.Key），自己又是一个小集合，
            //   所以桶里还能继续用 Count/Sum/Average/OrderBy/First —— 这就是分组统计报表的标准写法
            foreach (var g in employees.GroupBy(e => e.Dept))
            {
                int count = g.Count();                  // 这个部门几个人
                int total = g.Sum(e => e.Salary);       // 这个部门总共发多少工资
                double avg = g.Average(e => e.Salary);  // 平均值是 double，下面用 :F2 保留两位小数

                // ★ 桶里再排一次序：按薪资降序排好后，First() 就是"工资最高的那个人"
                //   如果部门里一个人都没有，First() 会抛异常 —— 不过 GroupBy 出来的桶一定非空，安全
                string top = g.OrderByDescending(e => e.Salary).First().Name;

                Console.WriteLine($"{g.Key}：{count} 人，总薪资 {total}，平均 {avg:F2}，最高：{top}");
            }

            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ ★ 验证延迟执行
            // ================================================================
            // 【要求】
            //   1. 建一个查询 q = employees.Where(e => e.Salary > 20000)
            //   2. 先打印 q 的数量
            //   3. 往 employees 里【再加一个人】（薪资 30000）
            //   4. 再打印 q 的数量 —— 观察是否变了
            //   5. 用 .ToList() 固定结果再做一遍，对照
            //
            // 【预期输出】
            //     初始查询数量：2
            //     添加一个高薪员工后，查询数量：3   ← 变了！延迟执行
            //     ─────────────
            //     用 ToList() 固定后：2
            //     再添加一个人后，固定列表数量：2   ← 不变
            //
            // 【提示】
            //     var q = employees.Where(e => e.Salary > 20000);
            //     Console.WriteLine($"初始查询数量：{q.Count()}");
            //
            //     employees.Add(new("新员工", "研发部", 30000, 1));
            //     Console.WriteLine($"添加一个高薪员工后，查询数量：{q.Count()}   ← 变了！延迟执行");
            //
            //     Console.WriteLine("─────────────");
            //     var fixedList = employees.Where(e => e.Salary > 20000).ToList();
            //     Console.WriteLine($"用 ToList() 固定后：{fixedList.Count}");
            //     employees.Add(new("另一个新员工", "销售部", 35000, 2));
            //     Console.WriteLine($"再添加一个人后，固定列表数量：{fixedList.Count}   ← 不变");
            //
            // 【考察点】
            //   · Where 返回的是 IEnumerable，不是 List
            //   · 它记录的是"查询意图"，遍历/聚合时才真正执行
            //   · 这是 LINQ 最容易出错的地方

            Console.WriteLine("--- TODO 4 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 定义查询：这里【故意不写】.ToList() —— q 只是一份"查询意图"，此刻一行代码都还没执行
            //   ★ Where 返回的是 IEnumerable，不是 List：它记下的是"条件"，不是"结果"
            var q = employees.Where(e => e.Salary > 20000);
            Console.WriteLine($"初始查询数量：{q.Count()}");   // 此刻只有李四、孙八 → 2

            // ② ★ 关键对照：趁数据还没变，先用 .ToList() 把 q 的结果"拍个快照"（立即执行，锁死这 2 条）
            //   这样 q 和 fixedList 面对的是同一时刻、同一份数据，后面出现的差别就只来自"延迟 vs 立即"
            var fixedList = q.ToList();

            // ③ 改数据源：往 employees 里再加一个高薪员工
            employees.Add(new("新员工", "研发部", 30000, 1));

            // ④ ★ q 自己不存数据，Count() 会【重新遍历】employees → 刚加进来的人也被算上了
            //   这就是延迟执行：同一个 q，两次 Count() 得到不同的数字
            Console.WriteLine($"添加一个高薪员工后，查询数量：{q.Count()}   ← 变了！延迟执行");

            Console.WriteLine("─────────────");

            // ⑤ 对照：fixedList 是 List<Employee>，数据在"拍快照"那一刻就装进列表了，后来再加人也不影响它
            Console.WriteLine($"用 ToList() 固定后：{fixedList.Count}");

            employees.Add(new("另一个新员工", "销售部", 35000, 2));
            Console.WriteLine($"再添加一个人后，固定列表数量：{fixedList.Count}   ← 不变");

            // ⑥ 收尾：把上面两个演示用的临时员工移出列表
            //   ★ 必须清理干净！否则 employees 会变成 9 个人，下面第 5 题的报表数字全都会跟着算错
            employees.RemoveAll(e => e.Name == "新员工" || e.Name == "另一个新员工");

            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ ★ 综合：员工数据分析报表（本课考核）
            // ================================================================
            // 【要求】只用 LINQ（不用手写 foreach 循环）生成一份报表：
            //     1. 标题
            //     2. 按薪资降序打印全部员工，格式 "排名. 名字（部门）薪资 工龄"
            //     3. 分隔线
            //     4. 统计：总人数、总薪资、平均薪资、最高、最低
            //     5. 各部门统计（人数 + 平均薪资，按平均薪资降序）
            //     6. 找出"高薪且资深"的人（薪资 >= 18000 且 工龄 >= 5），打印名字
            //     7. 找出薪资最高的前 3 名
            //
            // 【预期输出】
            //     ========== 员工数据分析报表 ==========
            //     1. 孙八（研发部）28000 元，工龄 8 年
            //     2. 李四（研发部）22000 元，工龄 6 年
            //     ... （按薪资降序全部列出）
            //     --------------------------------------
            //     总人数：7
            //     总薪资：118000 元
            //     平均薪资：16857.14 元
            //     最高薪资：28000 元，最低薪资：10000 元
            //     ---------- 部门统计（按平均薪资降序）----------
            //     研发部：3 人，平均 21666.67 元
            //     销售部：2 人，平均 15000.00 元
            //     人事部：2 人，平均 11500.00 元
            //     ---------- 高薪且资深 ----------
            //     孙八（研发部）28000 元，8 年
            //     李四（研发部）22000 元，6 年
            //     赵六（销售部）18000 元，5 年
            //   ⚠️ 【本处已于 2026-10-06 订正】原来只列了前两行，漏掉赵六。
            //      按【要求】第 6 条的定义「薪资 >= 18000 且 工龄 >= 5」，
            //      赵六的 18000 和 5 都【正好等于】边界，用 >= 判断应当入选，所以是 3 个人。
            //      （这也解释了为什么下面的"薪资前 3 名"里同样有赵六 —— 两处是自洽的。）
            //     ---------- 薪资前 3 名 ----------
            //     孙八: 28000
            //     李四: 22000
            //     赵六: 18000
            //     ======================================
            //
            // 【提示】
            //     Console.WriteLine("========== 员工数据分析报表 ==========");
            //
            //     // 排名：用 Select 的第二个参数拿索引！
            //     var ranked = employees.OrderByDescending(e => e.Salary)
            //         .Select((e, index) => $"{index + 1}. {e.Name}（{e.Dept}）{e.Salary} 元，工龄 {e.Years} 年")
            //         .ToList();
            //     foreach (string line in ranked) Console.WriteLine(line);
            //
            //     Console.WriteLine(new string('-', 38));
            //     Console.WriteLine($"总人数：{employees.Count()}");
            //     Console.WriteLine($"总薪资：{employees.Sum(e => e.Salary)} 元");
            //     Console.WriteLine($"平均薪资：{employees.Average(e => e.Salary):F2} 元");
            //     Console.WriteLine($"最高薪资：{employees.Max(e => e.Salary)} 元，最低薪资：{employees.Min(e => e.Salary)} 元");
            //
            //     Console.WriteLine("---------- 部门统计（按平均薪资降序）----------");
            //     var deptStats = employees
            //         .GroupBy(e => e.Dept)
            //         .Select(g => new { Dept = g.Key, Count = g.Count(), Avg = g.Average(e => e.Salary) })
            //         .OrderByDescending(x => x.Avg)
            //         .ToList();
            //     foreach (var d in deptStats)
            //         Console.WriteLine($"{d.Dept}：{d.Count} 人，平均 {d.Avg:F2} 元");
            //
            //     Console.WriteLine("---------- 高薪且资深 ----------");
            //     var senior = employees
            //         .Where(e => e.Salary >= 18000 && e.Years >= 5)
            //         .OrderByDescending(e => e.Salary)
            //         .Select(e => $"{e.Name}（{e.Dept}）{e.Salary} 元，{e.Years} 年")
            //         .ToList();
            //     foreach (string line in senior) Console.WriteLine(line);
            //
            //     Console.WriteLine("---------- 薪资前 3 名 ----------");
            //     var top3 = employees.OrderByDescending(e => e.Salary)
            //         .Take(3)
            //         .Select(e => $"{e.Name}: {e.Salary}")
            //         .ToList();
            //     foreach (string line in top3) Console.WriteLine(line);
            //     Console.WriteLine("======================================");
            //
            //   ★ 关键技巧：Select 可以接收两个参数 (item, index) 拿到索引
            //
            // 【考察点】
            //   · 整份报表只用 LINQ，没有手写 foreach 做数据加工
            //   · Select((e, index) => ...) 拿索引
            //   · GroupBy + Select + OrderByDescending 的组合
            //   · 这才是 LINQ 真正的生产用法

            Console.WriteLine("--- TODO 5 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 标题
            Console.WriteLine("========== 员工数据分析报表 ==========");

            // ② 排名表：Select 可以带【两个参数】—— (元素, 索引)，索引从 0 开始，所以显示时 +1
            //   ★ 有了它就不必像第 03 课那样自己维护一个 i++ 计数器了
            //   ★ 顺序很关键：先排序、后投影 —— 排好序再编号，1 号才是薪资最高的孙八
            var ranked = employees.OrderByDescending(e => e.Salary)
                .Select((e, index) => $"{index + 1}. {e.Name}（{e.Dept}）{e.Salary} 元，工龄 {e.Years} 年")
                .ToList();
            foreach (string line in ranked) Console.WriteLine(line);

            // ③ 分隔线：new string('-', 38) = 38 个连字符，比手打一长串更不容易数错
            Console.WriteLine(new string('-', 38));

            // ④ 四条统计：Count/Sum/Average/Max/Min 各管一件事，把它们拼起来就是报表
            Console.WriteLine($"总人数：{employees.Count()}");
            Console.WriteLine($"总薪资：{employees.Sum(e => e.Salary)} 元");
            Console.WriteLine($"平均薪资：{employees.Average(e => e.Salary):F2} 元");
            Console.WriteLine($"最高薪资：{employees.Max(e => e.Salary)} 元，最低薪资：{employees.Min(e => e.Salary)} 元");

            Console.WriteLine("---------- 部门统计（按平均薪资降序）----------");

            // ⑤ 三步走：GroupBy 分部门 → Select 投影成匿名对象 → OrderByDescending 按平均值排序
            //   ★ 匿名类型没有名字（是编译器临时造的类型），所以只能用 var 接住它
            //   ★ 必须先 Select 出 Avg，才能按 Avg 排序 —— 排序依据得先存在，才能拿来当依据
            var deptStats = employees
                .GroupBy(e => e.Dept)
                .Select(g => new { Dept = g.Key, Count = g.Count(), Avg = g.Average(e => e.Salary) })
                .OrderByDescending(x => x.Avg)
                .ToList();
            foreach (var d in deptStats)
                Console.WriteLine($"{d.Dept}：{d.Count} 人，平均 {d.Avg:F2} 元");

            Console.WriteLine("---------- 高薪且资深 ----------");

            // ⑥ "高薪且资深" = 薪资 >= 18000 【且】 工龄 >= 5 年
            //   ★ 门槛必须按【要求】里写的 18000，不能为了凑样例输出改成 20000。
            //     自己数一遍数据（这一步是本题最容易出错的地方）：
            //       孙八 28000 / 8 年  → 28000>=18000 ✅  8>=5 ✅  → 入选
            //       李四 22000 / 6 年  → ✅ ✅                    → 入选
            //       赵六 18000 / 5 年  → 18000>=18000 ✅  5>=5 ✅ → 【入选】
            //          ↑ 注意：>= 不是 > ，所以 18000 和 5 这两个"正好等于边界"的值都算通过
            //       其余 4 人薪资都 <= 15000，直接被第一个条件挡掉
            //     → 正确结果是 3 个人：孙八、李四、赵六
            //
            //   ⚠️ 【本处已于 2026-10-06 订正】练习文件的【预期输出】原来只列了孙八、李四两行，
            //      漏掉了同样满足条件的赵六 —— 那是课件的笔误。
            //      按【要求】给出的定义，正确输出应是下面 3 行。
            //   ★★ 通用教训：遇到"边界值"（这里 18000 / 5 年）必须专门拿它去测。
            //      如果只测 22000 和 12000 这种"明显在界内/界外"的值，
            //      边界写错（比如把 >= 写成 >）你永远发现不了。
            var senior = employees
                .Where(e => e.Salary >= 18000 && e.Years >= 5)
                .OrderByDescending(e => e.Salary)
                .Select(e => $"{e.Name}（{e.Dept}）{e.Salary} 元，{e.Years} 年")
                .ToList();
            foreach (string line in senior) Console.WriteLine(line);

            Console.WriteLine("---------- 薪资前 3 名 ----------");

            // ⑦ ★ 一定要先排序再 Take：不排序的话，Take(3) 拿到只是"列表里碰巧排在前面的 3 个人"
            var top3 = employees.OrderByDescending(e => e.Salary)
                .Take(3)
                .Select(e => $"{e.Name}: {e.Salary}")
                .ToList();
            foreach (string line in top3) Console.WriteLine(line);

            Console.WriteLine("======================================");

            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
