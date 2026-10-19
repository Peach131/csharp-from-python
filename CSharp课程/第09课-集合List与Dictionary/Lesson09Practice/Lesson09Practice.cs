// ============================================================================
//  第 09 课 · 练习：集合 List、Dictionary、HashSet
//
//  ★ 本课重点：
//     1. List<T> 是可变的（数组不是）—— 元素个数用 .Count
//     2. Dictionary 按键 O(1) 查找；读不存在的键要用 TryGetValue
//     3. HashSet 自动去重
// ============================================================================

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ TODO 5 用的类定义写在这里 ↓↓↓
    // ================================================================

    // ────────────────────────────────────────────────────────────────
    // TODO 5 ｜ Student：用 record 当"数据载体"
    // ────────────────────────────────────────────────────────────────
    // ★ 这一行代码，编译器会替你生成：
    //   · 3 个 { get; init; } 只读属性（Id / Name / Grade）
    //   · 一个构造函数 Student(int Id, string Name, int Grade)
    //   · ToString() → 输出 "Student { Id = 1001, Name = 张三, Grade = 88 }"
    //   · Equals / GetHashCode → 【按值比较】，所以它可以直接当 Dictionary 的键
    //   ★ 对比第 05 课写的 30 行 class Student：用 record 一行就够，
    //     而且不会漏写 Equals 导致"内容一样却比不相等"。
    // ★ 为什么这里不用给 Name 写 = ""？
    //   位置参数 Name 会被写进构造函数里赋值，编译器能证明它退出构造函数时非 null，
    //   所以 <Nullable>enable</Nullable> 下不会报 CS8618。
    record Student(int Id, string Name, int Grade);


    class Lesson09Practice
    {
        static void Main()
        {
            Console.WriteLine("===== 第 09 课练习：集合 =====\n");

            // ================================================================
            // TODO 1 ｜ List 基础操作
            // ================================================================
            // 【要求】创建一个 List<string>，依次完成：
            //   1. 加入 "苹果"、"香蕉"、"橘子"
            //   2. 再加入 "葡萄"、"西瓜"（用 AddRange 一次加两个）
            //   3. 删除 "香蕉"
            //   4. 删除第 1 个元素（索引 0）
            //   5. 每一步都打印当前列表和元素个数
            //
            // 【预期输出】
            //     加入 3 种水果    ：苹果, 香蕉, 橘子        （Count = 3）
            //     AddRange 两种    ：苹果, 香蕉, 橘子, 葡萄, 西瓜 （Count = 5）
            //     删除"香蕉"       ：苹果, 橘子, 葡萄, 西瓜   （Count = 4）
            //     删除索引 0       ：橘子, 葡萄, 西瓜        （Count = 3）
            //
            // 【语法提示】
            //     var fruits = new List<string> { "苹果", "香蕉", "橘子" };
            //     Console.WriteLine($"加入 3 种水果    ：{string.Join(", ", fruits)}    （Count = {fruits.Count}）");
            //
            //     fruits.AddRange(new[] { "葡萄", "西瓜" });
            //     fruits.Remove("香蕉");
            //     fruits.RemoveAt(0);
            //
            // 【考察点】
            //   · List 的 Count（不是 Length）
            //   · string.Join(", ", list) 一行输出列表内容

            Console.WriteLine("--- TODO 1 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 集合初始化器 { ... }：创建的同时就把 3 个元素装进去
            //   ★ 类型写的是 List<string> 而不是 string[] ——
            //     数组建好后长度就固定了，List 才能继续 Add。
            var fruits = new List<string> { "苹果", "香蕉", "橘子" };

            // ★ string.Join(分隔符, 集合) 会遍历集合、用分隔符把元素粘成【一个】字符串，
            //   比自己写 for + 判断"是不是最后一个"省事得多。
            // ★★ 这里用的是 fruits.Count（集合用 Count），写成 fruits.Length 会编译报错。
            Console.WriteLine($"加入 3 种水果    ：{string.Join(", ", fruits)}        （Count = {fruits.Count}）");

            // ② AddRange：一次加一批。参数只要是"可枚举的序列"就行，
            //   所以临时 new 一个数组 new[] { ... } 传进去最方便（元素类型推断为 string）。
            //   对比第 03 课：数组想"追加"只能新建更大的数组再搬数据，List 一句搞定。
            fruits.AddRange(new[] { "葡萄", "西瓜" });
            Console.WriteLine($"AddRange 两种    ：{string.Join(", ", fruits)} （Count = {fruits.Count}）");

            // ③ Remove 按【值】删：删掉第一个等于 "香蕉" 的元素。
            //   它其实会返回 bool（到底删掉没有），这里用不上返回值，所以不接。
            fruits.Remove("香蕉");
            Console.WriteLine($"删除\"香蕉\"       ：{string.Join(", ", fruits)}   （Count = {fruits.Count}）");

            // ④ RemoveAt 按【索引】删：删掉索引 0，也就是当前的第一个元素"苹果"。
            //   ★★ 坑：删完之后后面的元素会整体【向前补位】，
            //     所以"索引 0"指的是删除那一刻的第 1 个位置，和元素本身是谁无关。
            fruits.RemoveAt(0);
            Console.WriteLine($"删除索引 0       ：{string.Join(", ", fruits)}        （Count = {fruits.Count}）");

            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ List 的查找和统计
            // ================================================================
            // 【要求】给定成绩 List，用循环算出：
            //   1. 及格（>= 60）的分数，存进一个新的 List 并打印
            //   2. 总分、平均分、最高分、最低分
            //   3. 用 Contains 判断 95 分是否存在
            //
            // 【数据】new List<int> { 88, 45, 92, 76, 59, 95, 81 }
            //
            // 【预期输出】
            //     全部成绩：88, 45, 92, 76, 59, 95, 81
            //     及格成绩：88, 92, 76, 95, 81
            //     总分：536
            //     平均分：76.57
            //     最高分：95
            //     最低分：45
            //     有 95 分吗？True
            //
            // 【提示】
            //     var scores = new List<int> { 88, 45, 92, 76, 59, 95, 81 };
            //     var passed = new List<int>();
            //     int total = 0, max = scores[0], min = scores[0];
            //
            //     foreach (int s in scores)
            //     {
            //         if (s >= 60) passed.Add(s);        // ★ 往新 List 里加
            //         total += s;
            //         if (s > max) max = s;
            //         if (s < min) min = s;
            //     }
            //     Console.WriteLine($"平均分：{(double)total / scores.Count:F2}");
            //
            // 【考察点】注意平均分用的是 scores.Count，不是 Length

            Console.WriteLine("--- TODO 2 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 数据源：List<int>，7 个成绩
            var scores = new List<int> { 88, 45, 92, 76, 59, 95, 81 };

            // ② 准备装"及格成绩"的新 List。
            //   ★ 它是空的，长度会随着 Add 长大 —— 这正是数组做不到的事：
            //     用数组的话，你得先猜"大概有几个及格"再开数组，还得自己记用了几个格子。
            var passed = new List<int>();

            // ③ 求最大值/最小值时，初值取【第一个元素】而不是 0：
            //   如果全是负数，从 0 开始比就会得出最大值 0（一个根本不存在的分数）。
            //   取 scores[0] 保证初值一定是真实数据，后面的元素挨个和它比就够了
            //   （第一个元素和自己比，不会更新，所以直接从它开始遍历也没问题）。
            int total = 0, max = scores[0], min = scores[0];

            // ④ 一次遍历同时算三件事：筛选、累加、找极值。
            //   数据只被扫一遍 = O(n)，比"筛一遍、求和一遍、找最大一遍"更符合直觉也更省。
            foreach (int score in scores)
            {
                if (score >= 60) passed.Add(score);   // ★ 往新 List 里加，自然保持原来的先后顺序
                total += score;                       // 累加，等价于 total = total + score
                if (score > max) max = score;
                if (score < min) min = score;
            }

            // ⑤ 一行输出整个 List：string.Join 会把 int 自动转成字符串再拼接
            Console.WriteLine($"全部成绩：{string.Join(", ", scores)}");
            Console.WriteLine($"及格成绩：{string.Join(", ", passed)}");
            Console.WriteLine($"总分：{total}");

            // ⑥ ★★ 平均分必须【先转 double 再除】：
            //   total 和 scores.Count 都是 int，两个 int 相除叫"整数除法"，
            //   小数部分会被直接丢掉（536 / 7 = 76，不是 76.57）。
            //   (double)total 把左边变成 double 后，右边也会自动提升成 double，才是真除法。
            //   后面的 :F2 是"小数点后保留 2 位"，输出 76.57。
            Console.WriteLine($"平均分：{(double)total / scores.Count:F2}");
            Console.WriteLine($"最高分：{max}");
            Console.WriteLine($"最低分：{min}");

            // ⑦ Contains 判断"在不在"，返回 bool（True / False 首字母大写，这是 C# 的写法）
            Console.WriteLine($"有 95 分吗？{scores.Contains(95)}");

            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ ★ Dictionary 词频统计
            // ================================================================
            // 【要求】统计一段文本里每个单词出现的次数：
            //   1. 用 Split(' ') 拆分成单词数组
            //   2. 用 Dictionary<string,int> 统计
            //   3. 打印每个单词和次数
            //   4. 找出出现次数最多的单词
            //
            // 【文本】"cat dog cat bird dog cat fish bird cat"
            //
            // 【预期输出】
            //     原文：cat dog cat bird dog cat fish bird cat
            //     ---------- 词频统计 ----------
            //     cat  4 次
            //     dog  2 次
            //     bird 2 次
            //     fish 1 次
            //     出现最多的是：cat（4 次）
            //
            // 【提示】
            //     string text = "cat dog cat bird dog cat fish bird cat";
            //     string[] words = text.Split(' ');
            //
            //     var counts = new Dictionary<string, int>();
            //     foreach (string w in words)
            //     {
            //         if (counts.TryGetValue(w, out int c)) counts[w] = c + 1;
            //         else counts[w] = 1;
            //     }
            //
            //     // 找最大值
            //     string topWord = "";
            //     int topCount = 0;
            //     foreach (var kv in counts)
            //     {
            //         Console.WriteLine($"{kv.Key,-5}{kv.Value} 次");
            //         if (kv.Value > topCount) { topCount = kv.Value; topWord = kv.Key; }
            //     }
            //     Console.WriteLine($"\n出现最多的是：{topWord}（{topCount} 次）");
            //
            // 【考察点】
            //   · TryGetValue 的用法（不能直接 counts[w] 读，会抛异常）
            //   · Dictionary 的遍历和"找最大值"的组合

            Console.WriteLine("--- TODO 3 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 原始文本：单词之间用【单个空格】分隔，正好能直接 Split(' ')
            //   ★ Split(' ') 的参数是 char（单引号），返回的是 string[] 数组。
            string text = "cat dog cat bird dog cat fish bird cat";
            string[] words = text.Split(' ');

            Console.WriteLine($"原文：{text}");
            Console.WriteLine("---------- 词频统计 ----------");

            // ② Dictionary<string,int>：键=单词，值=出现次数。
            //   为什么不建两个 List（一个存单词、一个存次数）？
            //   因为"加 1"之前得先知道这个单词在不在 —— List 只能靠 Contains/IndexOf 遍历找，
            //   Dictionary 用键一次定位，这就是它 O(1) 的价值。
            var counts = new Dictionary<string, int>();

            foreach (string w in words)
            {
                // ③ ★★ 本课最关键的写法：TryGetValue = "试着取出来"。
                //   · 键存在 → 返回 true，并把值写进 out 参数 c
                //   · 键不存在 → 返回 false，c 被赋成 int 的默认值 0
                //   合并两件事（判断 + 取值）只查一次哈希表；
                //   而 ContainsKey 再 counts[w] 要查两次。
                //   绝对不要直接 int c = counts[w]：键不存在会抛 KeyNotFoundException 把程序打崩。
                if (counts.TryGetValue(w, out int c))
                {
                    counts[w] = c + 1;    // ④ 键已存在 → 覆盖成新值（字典的"改"）
                }
                else
                {
                    counts[w] = 1;        // ⑤ 键不存在 → 右边这种写法会顺带【新增】这个键
                }
            }

            // ⑥ 遍历字典：每个元素是一个 KeyValuePair（键值对），kv.Key / kv.Value 取两半。
            //   ★ 格式里的 {kv.Key,-5} 是"左对齐、宽度至少 5"：
            //     不够 5 个字符就在右边补空格，所以 cat/dog 后面 2 格、bird 后面 1 格，
            //     后面的次数就都从同一列开始了。
            //   ★ 顺序是按"第一次遇到该单词"的先后（cat → dog → bird → fish），运行结果稳定。
            string topWord = "";
            int topCount = 0;
            foreach (var kv in counts)
            {
                Console.WriteLine($"{kv.Key,-5}{kv.Value} 次");

                // ⑦ "找最大值"的老套路：边遍历边更新。
                //   ★ 一边打印一边比，就不用为了找冠军再遍历第二遍。
                //   ★ 严格大于 > 还有个副作用：次数并列时保留【先遇到】的那个单词。
                if (kv.Value > topCount)
                {
                    topCount = kv.Value;
                    topWord = kv.Key;
                }
            }

            Console.WriteLine($"出现最多的是：{topWord}（{topCount} 次）");

            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ HashSet 去重和集合运算
            // ================================================================
            // 【要求】
            //   1. 有两个班级的选课名单，用 HashSet 找出：
            //        · 两个班都选了的课（交集）
            //        · 至少一个班选了的课（并集）
            //        · 只有 A 班选了、B 班没选的（差集）
            //   2. 统计一共有多少门不同的课
            //
            // 【数据】
            //     A 班：语文, 数学, 英语, 物理
            //     B 班：数学, 英语, 化学, 生物
            //
            // 【预期输出】
            //     A 班课程：语文, 数学, 英语, 物理
            //     B 班课程：数学, 英语, 化学, 生物
            //     ---------- 集合运算 ----------
            //     两个班都选（交集）：数学, 英语
            //     至少一个班选（并集）：语文, 数学, 英语, 物理, 化学, 生物
            //     只有 A 班选（差集）：语文, 物理
            //     不同的课程总数：6
            //
            // 【提示】
            //     var classA = new HashSet<string> { "语文", "数学", "英语", "物理" };
            //     var classB = new HashSet<string> { "数学", "英语", "化学", "生物" };
            //
            //     var both = new HashSet<string>(classA);
            //     both.IntersectWith(classB);            // ★ 注意：先复制一份再运算（会修改自己）
            //
            //     var all = new HashSet<string>(classA);
            //     all.UnionWith(classB);
            //
            //     var onlyA = new HashSet<string>(classA);
            //     onlyA.ExceptWith(classB);
            //
            //   ⚠️ 为什么每次都要 new HashSet<string>(classA)？
            //      因为这些方法会【原地修改】调用它的那个集合。
            //      不复制的话，做完交集 classA 就变了。
            //
            // 【考察点】
            //   · IntersectWith / UnionWith / ExceptWith 都是原地修改
            //   · HashSet 的并集天然就是"去重后的总数"

            Console.WriteLine("--- TODO 4 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① HashSet 初始化器和 List 长得很像，区别是：重复元素加不进去，Count 自动是"去重后"的数量。
            //   题里的名单每班内部本来就不重复，所以两个 Count 都是 4。
            var classA = new HashSet<string> { "语文", "数学", "英语", "物理" };
            var classB = new HashSet<string> { "数学", "英语", "化学", "生物" };

            Console.WriteLine($"A 班课程：{string.Join(", ", classA)}");
            Console.WriteLine($"B 班课程：{string.Join(", ", classB)}");
            Console.WriteLine("---------- 集合运算 ----------");

            // ② 交集：两个班都选的课。
            //   ★★ 为什么不直接写 classA.IntersectWith(classB)？
            //     因为 IntersectWith / UnionWith / ExceptWith 都是【原地修改】调用它的集合 ——
            //     直接改 classA，classA 就被毁掉了，后面算并集、差集全都错。
            //     所以固定套路是：new HashSet<string>(classA) 先复制一份，拿副本去算。
            var both = new HashSet<string>(classA);
            both.IntersectWith(classB);       // 副本里只留下"classB 也有"的元素
            Console.WriteLine($"两个班都选（交集）：{string.Join(", ", both)}");

            // ③ 并集：至少一个班选的课。UnionWith 会自动去重，
            //   所以"并集里剩下几门" = "一共有几门不同的课"，不用再手工数。
            var all = new HashSet<string>(classA);
            all.UnionWith(classB);
            Console.WriteLine($"至少一个班选（并集）：{string.Join(", ", all)}");

            // ④ 差集：A 有、B 没有的课。注意方向 —— classB.ExceptWith(classA) 得到的是另一组结果。
            var onlyA = new HashSet<string>(classA);
            onlyA.ExceptWith(classB);
            Console.WriteLine($"只有 A 班选（差集）：{string.Join(", ", onlyA)}");

            // ⑤ HashSet 天生不存重复元素，"元素个数"就是"不同课程的种数"
            //   ★ 这里又用到 Count（集合）而不是 Length（数组/字符串）
            Console.WriteLine($"不同的课程总数：{all.Count}");

            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ ★ 综合：学生成绩管理系统（本课考核）
            // ================================================================
            // 【背景】用 List 存所有学生，用 Dictionary 建索引实现"按学号秒查"。
            //        这是真实项目里最常见的组合。
            //
            // 【要求】
            //   1. 在上面定义 record Student(int Id, string Name, int Grade);
            //   2. 创建一个 List<Student>，装入 5 个学生：
            //        1001 张三 88
            //        1002 李四 92
            //        1003 王五 76
            //        1004 赵六 95
            //        1005 钱七 55
            //   3. 打印所有学生（用 foreach）
            //   4. 用 Dictionary<int, Student> 建立"学号 → 学生"的索引
            //   5. 用 TryGetValue 查询学号 1003 和 9999
            //   6. 统计：总分、平均分、最高分、及格人数
            //   7. 用 HashSet<string> 或者 List 统计"有哪些不同的分数档"（可以简化：只统计及格/不及格名单）
            //
            // 【预期输出】
            //     ========== 学生名单 ==========
            //     1001  张三  88
            //     1002  李四  92
            //     1003  王五  76
            //     1004  赵六  95
            //     1005  钱七  55
            //     ========== 按学号查询 ==========
            //     学号 1003 → 王五，成绩 76
            //     学号 9999 → 查无此人
            //     ========== 统计 ==========
            //     总分：406
            //     平均分：81.20
            //     最高分：95
            //     及格人数：4
            //     不及格名单：钱七
            //
            // 【提示】
            //     record Student(int Id, string Name, int Grade);
            //
            //     var students = new List<Student>
            //     {
            //         new Student(1001, "张三", 88),
            //         // ... 剩下 4 个
            //     };
            //
            //     // 建索引
            //     var byId = new Dictionary<int, Student>();
            //     foreach (var s in students) byId[s.Id] = s;
            //
            //     // 查询
            //     if (byId.TryGetValue(1003, out var found))
            //         Console.WriteLine($"学号 1003 → {found.Name}，成绩 {found.Grade}");
            //     else
            //         Console.WriteLine("学号 1003 → 查无此人");
            //
            //     // 统计（一次遍历搞定多个指标）
            //     int total = 0, max = students[0].Grade, passCount = 0;
            //     var failedNames = new List<string>();
            //     foreach (var s in students)
            //     {
            //         total += s.Grade;
            //         if (s.Grade > max) max = s.Grade;
            //         if (s.Grade >= 60) passCount++;
            //         else failedNames.Add(s.Name);
            //     }
            //
            // 【考察点】
            //   · List 存全部 + Dictionary 做索引，这是标准组合
            //   · 一次遍历同时算多个统计量
            //   · record 自动生成 ToString/Equals，很适合做数据载体

            Console.WriteLine("--- TODO 5 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 用【位置构造函数】创建 record：参数顺序 = Student(int Id, string Name, int Grade) 的声明顺序。
            //   ★ List<Student> 装的是"学生对象本身"，不是"分数的拷贝"，
            //     以后想加年龄/班级只要改 Student 一行定义，不用再开一个数组。
            var students = new List<Student>
            {
                new Student(1001, "张三", 88),
                new Student(1002, "李四", 92),
                new Student(1003, "王五", 76),
                new Student(1004, "赵六", 95),
                new Student(1005, "钱七", 55),
            };

            // ② 建索引：Dictionary<int, Student>，键 = 学号，值 = 学生对象。
            //   ★ byId[s.Id] = s 这种"下标赋值"的写法：键不存在就新增，键存在就覆盖 ——
            //     所以即使学号写重了也不会抛异常（这正是它比 Add 方便的地方）。
            //   ★ 只建一次索引，之后每次按学号查都是 O(1)。这就是 List（存全部、保顺序）
            //     + Dictionary（做索引、秒查）的经典组合。
            var byId = new Dictionary<int, Student>();
            foreach (var s in students)
            {
                byId[s.Id] = s;
            }

            Console.WriteLine("========== 学生名单 ==========");
            // ③ 用 foreach 打印名单：不需要索引，foreach 的语义就是"把每个元素挨个给我"。
            //   ★ 插值里可以直接放对象，record 的 ToString 是自动生成的，
            //     但课程要求的是"学号 姓名 成绩"这种自定义格式，所以逐个属性取值拼。
            foreach (var s in students)
            {
                Console.WriteLine($"{s.Id}  {s.Name}  {s.Grade}");
            }

            Console.WriteLine("========== 按学号查询 ==========");

            // ④ ★★ 查询必须用 TryGetValue，不能写 byId[1003]：
            //   · 键存在 → 返回 true，out var found 接收取到的学生对象
            //   · 键不存在（比如 9999）→ 返回 false，found 是 null；
            //     如果硬写 byId[9999]，会抛 KeyNotFoundException 直接把程序打崩。
            //   ★ out var 是"就地声明变量"的简写，编译器会推断出 found 的类型是 Student。
            if (byId.TryGetValue(1003, out var found))
            {
                Console.WriteLine($"学号 1003 → {found.Name}，成绩 {found.Grade}");
            }
            else
            {
                Console.WriteLine("学号 1003 → 查无此人");
            }

            if (byId.TryGetValue(9999, out var missing))
            {
                // ★ 这个分支永远不会执行（字典里没有 9999）。
                //   写出来是为了说明"查到了怎么用"——编译器也因为这里用到了 missing，
                //   不会提示"变量声明了没用"。
                Console.WriteLine($"学号 9999 → {missing.Name}，成绩 {missing.Grade}");
            }
            else
            {
                Console.WriteLine("学号 9999 → 查无此人");
            }

            Console.WriteLine("========== 统计 ==========");

            // ⑤ 一次遍历同时算出多个统计量：总分、最高分、及格人数、不及格名单。
            //   ★ 变量名叫 sumScore / topGrade，是为了不和上面 TODO 2 的 total / max 重名 ——
            //     TODO 1~5 全写在同一个 Main 方法里，同一个【方法体作用域】内不允许重复声明。
            //   ★ 最高分的初值取 students[0].Grade —— 和 TODO 2 同理，
            //     取 0 的话一旦所有人都不及格，最高分就会莫名其妙变成 0。
            //   ★ 如果 students 是空列表，students[0] 会越界；真实项目里要先判 Count > 0，
            //     这里数据是写死的 5 个学生，所以直接取第 0 个。
            int sumScore = 0, topGrade = students[0].Grade, passCount = 0;

            // ⑥ failedNames 用 List<string> 收集"不及格的人名"：
            //   人数事先不知道，所以不能用固定长度的数组，必须用能自动变长的 List。
            var failedNames = new List<string>();

            foreach (var stu in students)
            {
                sumScore += stu.Grade;                       // 累加总分
                if (stu.Grade > topGrade) topGrade = stu.Grade;   // 边遍历边更新最高分

                if (stu.Grade >= 60)
                {
                    passCount++;                             // 及格：计数器 +1
                }
                else
                {
                    failedNames.Add(stu.Name);               // 不及格：把名字收进名单
                }
            }

            Console.WriteLine($"总分：{sumScore}");

            // ⑦ 平均分：又是那个坑 —— sumScore 和 students.Count 都是 int，
            //   先 (double)sumScore 转成 double，除法才会保留小数；:F2 保留两位。
            Console.WriteLine($"平均分：{(double)sumScore / students.Count:F2}");
            Console.WriteLine($"最高分：{topGrade}");
            Console.WriteLine($"及格人数：{passCount}");

            // ⑧ 打印不及格名单。
            //   ★ 标准写法就是这一行：failedNames 已经是上面遍历时筛出来的结果，
            //     string.Join 用 ", " 把它拼成一个字符串。
            //   ★ 顺便核一下题面的数字（这一步对你自查很有用）：
            //     数据是 5 个人 → students.Count = 5
            //     总分 88+92+76+95+55 = 406          → 与题面"总分：406"一致 ✅
            //     平均分 406 / 5 = 81.20             → 与题面一致 ✅
            //     55 < 60，所以不及格的就是钱七        → 与题面"不及格名单：钱七"一致 ✅
            //     及格 4 人（88/92/76/95）            → 与题面"及格人数：4"一致 ✅
            //   → 题面所有数字自洽，没有笔误。
            //   ★★ 这里有个通用教训：当数字看起来"对不上"时，
            //     先回去数清【基数】（本题是人数），再判断是不是真矛盾。
            Console.WriteLine($"不及格名单：{string.Join(", ", failedNames)}");

            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
