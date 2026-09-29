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




            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
