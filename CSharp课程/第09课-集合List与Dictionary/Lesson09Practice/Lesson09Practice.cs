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




            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
