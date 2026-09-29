// ============================================================================
//  第 09 课 · 示例代码
//  集合：List、Dictionary、HashSet
//  ★ 本课解决第 03 课留下的问题：数组长度不能变
// ============================================================================

using System.Diagnostics;

namespace Lesson09Demo
{
    record Student(int Id, string Name, int Grade);

    class Program
    {
        static void Main()
        {
            Console.WriteLine("========== 第 09 课：集合 ==========");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 1：★ List 解决了数组的什么问题 ----");

            // ❌ 数组：长度固定
            int[] arr = new int[3];
            arr[0] = 88; arr[1] = 92; arr[2] = 76;
            Console.WriteLine($"   数组：加了 3 个就满了，arr.Length = {arr.Length}，不能再加第 4 个");

            // ✅ List：想加多少加多少
            var list = new List<int>();
            list.Add(88);
            list.Add(92);
            list.Add(76);
            list.Add(81);                       // ★ 继续加，没问题
            list.Add(95);
            Console.WriteLine($"   List：加了 5 个，list.Count = {list.Count}   ← 自动扩容");
            Console.WriteLine("   ★ List<T> 才是 Python 的 list，数组不是");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 2：List 的常用操作 ----");

            var fruits = new List<string> { "苹果", "香蕉" };
            Console.WriteLine($"   初始：{string.Join(", ", fruits)}");

            fruits.Add("橘子");
            Console.WriteLine($"   Add(\"橘子\")       → {string.Join(", ", fruits)}");

            fruits.AddRange(new[] { "葡萄", "西瓜" });
            Console.WriteLine($"   AddRange(两种)     → {string.Join(", ", fruits)}");

            fruits.Remove("香蕉");
            Console.WriteLine($"   Remove(\"香蕉\")    → {string.Join(", ", fruits)}");

            fruits.RemoveAt(0);
            Console.WriteLine($"   RemoveAt(0)        → {string.Join(", ", fruits)}");

            Console.WriteLine($"   索引访问 fruits[0]  = {fruits[0]}");
            Console.WriteLine($"   倒数第一 fruits[^1] = {fruits[^1]}");
            Console.WriteLine($"   包含\"葡萄\"？        = {fruits.Contains("葡萄")}");
            Console.WriteLine($"   \"西瓜\"的索引        = {fruits.IndexOf("西瓜")}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 3：Count 还是 Length？（容易搞混）----");

            int[] a = new int[5];
            var l = new List<int> { 1, 2, 3 };
            string s = "hello";

            Console.WriteLine($"   数组     a.Length      = {a.Length}    ← 数组用 Length");
            Console.WriteLine($"   字符串   s.Length      = {s.Length}    ← 字符串也用 Length");
            Console.WriteLine($"   List     l.Count       = {l.Count}    ← List 用 Count");
            Console.WriteLine("   ★ 记法：数组和字符串叫\"长度\"，集合叫\"数量\"");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 4：List 的扩容机制 ----");

            var grow = new List<int>();
            Console.WriteLine($"   初始：Count = {grow.Count}, Capacity = {grow.Capacity}");
            for (int i = 1; i <= 9; i++)
            {
                grow.Add(i);
                if (i <= 1 || grow.Capacity != i)
                {
                    Console.WriteLine($"   Add 第 {i} 个后：Count = {grow.Count}, Capacity = {grow.Capacity}");
                }
            }
            Console.WriteLine("   ★ 容量按 4, 8, 16... 翻倍增长，每次扩容都要复制数据");
            Console.WriteLine("   ★ 如果知道大概多少个元素，用 new List<int>(100) 预分配更高效");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 5：★ Dictionary 键值对 ----");

            var ages = new Dictionary<string, int>
            {
                ["张三"] = 25,
                ["李四"] = 30
            };

            ages["王五"] = 28;                       // 新增
            ages["张三"] = 26;                       // 覆盖

            Console.WriteLine($"   字典内容：{string.Join(", ", ages.Select(kv => $"{kv.Key}={kv.Value}"))}");
            Console.WriteLine($"   直接读取 ages[\"张三\"] = {ages["张三"]}");
            Console.WriteLine();

            // ⚠️ 读不存在的键会抛异常
            try
            {
                int bad = ages["不存在的人"];
                Console.WriteLine($"   {bad}");
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("   ages[\"不存在的人\"] → KeyNotFoundException！");
            }

            // ★ 安全读取
            if (ages.TryGetValue("赵六", out int value))
            {
                Console.WriteLine($"   TryGetValue(\"赵六\") 成功 → {value}");
            }
            else
            {
                Console.WriteLine("   TryGetValue(\"赵六\") 返回 false（和 int.TryParse 一个套路）");
            }
            Console.WriteLine();

            Console.WriteLine("   遍历 Dictionary：");
            foreach (var kv in ages)
            {
                Console.WriteLine($"      {kv.Key,-6} → {kv.Value}");
            }
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 6：★ 实战：统计词频 ----");

            string text = "apple banana apple cherry banana apple";
            Console.WriteLine($"   文本：{text}");

            var counts = new Dictionary<string, int>();
            foreach (string word in text.Split(' '))
            {
                if (counts.TryGetValue(word, out int c))
                {
                    counts[word] = c + 1;            // 已经有，加 1
                }
                else
                {
                    counts[word] = 1;                // 第一次见
                }
            }

            Console.WriteLine("   统计结果：");
            foreach (var kv in counts)
            {
                Console.WriteLine($"      {kv.Key,-8} {kv.Value} 次");
            }
            Console.WriteLine("   ★ 这是 Dictionary 最经典的用法");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 7：★ HashSet 自动去重 ----");

            var visitors = new HashSet<string>();

            Console.WriteLine($"   访客 张三：{(visitors.Add("张三") ? "新增" : "已存在")}");
            Console.WriteLine($"   访客 李四：{(visitors.Add("李四") ? "新增" : "已存在")}");
            Console.WriteLine($"   访客 张三：{(visitors.Add("张三") ? "新增" : "已存在")}   ← 重复的被拒绝");
            Console.WriteLine($"   不重复访客数：{visitors.Count}");
            Console.WriteLine();

            // 集合运算
            var setA = new HashSet<int> { 1, 2, 3, 4 };
            var setB = new HashSet<int> { 3, 4, 5, 6 };
            Console.WriteLine($"   A = {{{string.Join(",", setA)}}}");
            Console.WriteLine($"   B = {{{string.Join(",", setB)}}}");

            var inter = new HashSet<int>(setA); inter.IntersectWith(setB);
            Console.WriteLine($"   交集 A ∩ B = {{{string.Join(",", inter)}}}");

            var union = new HashSet<int>(setA); union.UnionWith(setB);
            Console.WriteLine($"   并集 A ∪ B = {{{string.Join(",", union)}}}");

            var diff = new HashSet<int>(setA); diff.ExceptWith(setB);
            Console.WriteLine($"   差集 A - B = {{{string.Join(",", diff)}}}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 8：★★ 性能对照（为什么选对集合很重要）----");

            const int N = 100_000;          // 10 万个元素
            const int trials = 1000;        // 查找 1000 次

            // 准备数据
            var bigList = new List<int>(N);
            var bigDict = new Dictionary<int, int>(N);
            var bigSet = new HashSet<int>(N);
            for (int i = 0; i < N; i++)
            {
                bigList.Add(i);
                bigDict[i] = i;
                bigSet.Add(i);
            }
            int target = N - 1;             // 找最后一个（List 的最坏情况）

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < trials; i++) bigList.Contains(target);
            sw.Stop();
            double listMs = sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            for (int i = 0; i < trials; i++) bigDict.ContainsKey(target);
            sw.Stop();
            double dictMs = sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            for (int i = 0; i < trials; i++) bigSet.Contains(target);
            sw.Stop();
            double setMs = sw.Elapsed.TotalMilliseconds;

            Console.WriteLine($"   在 {N:N0} 个元素里查找 {trials} 次：");
            Console.WriteLine($"      List.Contains       = {listMs,8:F2} ms   O(n)");
            Console.WriteLine($"      Dictionary.Contains = {dictMs,8:F2} ms   O(1)");
            Console.WriteLine($"      HashSet.Contains    = {setMs,8:F2} ms   O(1)");
            if (dictMs > 0.01)
            {
                Console.WriteLine($"   → List 比 Dictionary 慢了约 {listMs / dictMs:F0} 倍");
            }
            Console.WriteLine("   ★ 实用判断：如果你写了一个 foreach 只为\"找东西在不在\"，");
            Console.WriteLine("     那八成应该换成 Dictionary 或 HashSet");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 9：对象集合（List<Student>）----");

            var students = new List<Student>
            {
                new Student(1001, "张三", 88),
                new Student(1002, "李四", 92),
                new Student(1003, "王五", 76)
            };

            Console.WriteLine("   用 List<Student> 存对象：");
            foreach (var stu in students)
            {
                Console.WriteLine($"      学号 {stu.Id}：{stu.Name}，成绩 {stu.Grade}");
            }
            Console.WriteLine();

            // 按学号建字典，实现 O(1) 查找
            var byId = new Dictionary<int, Student>();
            foreach (var stu in students) byId[stu.Id] = stu;

            Console.WriteLine("   用 Dictionary<int, Student> 实现按学号秒查：");
            if (byId.TryGetValue(1002, out var found))
            {
                Console.WriteLine($"      学号 1002 → {found.Name}，成绩 {found.Grade}");
            }
            Console.WriteLine("   ★ 这是真实项目里的常见组合：List 存全部，Dictionary 做索引");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson09Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
