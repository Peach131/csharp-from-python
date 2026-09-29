// ============================================================================
//  第 16 课 · 示例代码
//  LINQ
// ============================================================================

using System.Diagnostics;

namespace Lesson16Demo
{
    record Student(string Name, int Grade, string City);

    class Program
    {
        static void Main()
        {
            Console.WriteLine("========== 第 16 课：LINQ ==========\n");

            var students = new List<Student>
            {
                new("张三", 88, "重庆"),
                new("李四", 92, "成都"),
                new("王五", 76, "重庆"),
                new("赵六", 95, "北京"),
                new("钱七", 55, "成都")
            };

            Console.WriteLine("   数据：");
            foreach (var s in students)
                Console.WriteLine($"      {s.Name}  {s.Grade}  {s.City}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 1：Where 筛选 ----");

            var passed = students.Where(s => s.Grade >= 60).ToList();
            Console.WriteLine($"   及格的学生（{passed.Count} 人）：{string.Join(", ", passed.Select(s => s.Name))}");

            var chongqing = students.Where(s => s.City == "重庆").ToList();
            Console.WriteLine($"   重庆的学生：{string.Join(", ", chongqing.Select(s => s.Name))}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 2：Select 转换 ----");

            var names = students.Select(s => s.Name).ToList();
            Console.WriteLine($"   只要名字：{string.Join(", ", names)}");

            var grades2x = students.Select(s => s.Grade * 2).ToList();
            Console.WriteLine($"   成绩 ×2 ：{string.Join(", ", grades2x)}");

            var brief = students.Select(s => $"{s.Name}({s.Grade})").ToList();
            Console.WriteLine($"   组合字符串：{string.Join(", ", brief)}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 3：排序 ----");

            var byGrade = students.OrderBy(s => s.Grade).Select(s => $"{s.Name}:{s.Grade}");
            Console.WriteLine($"   按成绩升序：{string.Join(", ", byGrade)}");

            var byGradeDesc = students.OrderByDescending(s => s.Grade).Select(s => $"{s.Name}:{s.Grade}");
            Console.WriteLine($"   按成绩降序：{string.Join(", ", byGradeDesc)}");

            var multi = students.OrderBy(s => s.City).ThenByDescending(s => s.Grade)
                                .Select(s => $"{s.City}-{s.Name}:{s.Grade}");
            Console.WriteLine($"   先城市后成绩：{string.Join(", ", multi)}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 4：聚合运算 ----");

            Console.WriteLine($"   Count()                = {students.Count()}");
            Console.WriteLine($"   Count(及格)            = {students.Count(s => s.Grade >= 60)}");
            Console.WriteLine($"   Sum(成绩)              = {students.Sum(s => s.Grade)}");
            Console.WriteLine($"   Average(成绩)          = {students.Average(s => s.Grade):F2}");
            Console.WriteLine($"   Max(成绩)              = {students.Max(s => s.Grade)}");
            Console.WriteLine($"   Min(成绩)              = {students.Min(s => s.Grade)}");
            Console.WriteLine($"   Any(有优秀的吗)        = {students.Any(s => s.Grade >= 90)}");
            Console.WriteLine($"   All(全及格了吗)        = {students.All(s => s.Grade >= 60)}");
            Console.WriteLine($"   Any(列表非空)          = {students.Any()}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 5：★ GroupBy 分组 ----");

            var byCity = students.GroupBy(s => s.City);
            foreach (var g in byCity)
            {
                Console.WriteLine($"   {g.Key}：{g.Count()} 人，平均 {g.Average(s => s.Grade):F1} 分，"
                                  + $"名单[{string.Join(",", g.Select(s => s.Name))}]");
            }
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 6：Distinct / Take / Skip ----");

            var cities = students.Select(s => s.City).Distinct().ToList();
            Console.WriteLine($"   Distinct 城市去重：{string.Join(", ", cities)}");

            var top2 = students.OrderByDescending(s => s.Grade).Take(2).Select(s => $"{s.Name}:{s.Grade}");
            Console.WriteLine($"   Take(2) 前两名   ：{string.Join(", ", top2)}");

            var rest = students.OrderByDescending(s => s.Grade).Skip(2).Select(s => $"{s.Name}:{s.Grade}");
            Console.WriteLine($"   Skip(2) 剩下的   ：{string.Join(", ", rest)}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 7：★ 链式组合的威力 ----");

            // 需求：成都学生中成绩 >= 70 的，按成绩降序，取前 2 名，只要名字
            var result = students
                .Where(s => s.City == "成都")
                .Where(s => s.Grade >= 70)
                .OrderByDescending(s => s.Grade)
                .Take(2)
                .Select(s => s.Name)
                .ToList();

            Console.WriteLine("   需求：成都学生中成绩 >= 70 的，按成绩降序，取前 2 名，只要名字");
            Console.WriteLine($"   LINQ 一行链式结果：{string.Join(", ", result)}");
            Console.WriteLine();

            // 对照：手写循环要多少行
            var manual = new List<string>();
            var temp = new List<Student>();
            foreach (var s in students)
                if (s.City == "成都" && s.Grade >= 70) temp.Add(s);
            temp.Sort((a, b) => b.Grade.CompareTo(a.Grade));
            for (int i = 0; i < temp.Count && i < 2; i++) manual.Add(temp[i].Name);
            Console.WriteLine($"   手写循环需要 7 行，结果一样：{string.Join(", ", manual)}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 8：★★ 延迟执行（本节最重要）----");

            var nums = new List<int> { 1, 2, 3, 4, 5 };

            Console.WriteLine("   var query = nums.Where(n => n > 2);   ← 此时什么都没算");
            var query = nums.Where(n => n > 2);

            Console.WriteLine($"   现在 nums 是：[{string.Join(", ", nums)}]");
            Console.WriteLine("   往 nums 里加一个 100...");
            nums.Add(100);

            Console.WriteLine($"   遍历 query → [{string.Join(", ", query)}]");
            Console.WriteLine("   ★ 注意！100 也被查出来了 —— 因为查询在【遍历的那一刻】才执行");
            Console.WriteLine();

            // 证明"每次遍历都会重新计算"
            int checkCount = 0;
            var counting = nums.Where(n =>
            {
                checkCount++;                     // ★ 每次检查都会 +1
                return n > 2;
            });

            Console.WriteLine("   var q = nums.Where(n => { checkCount++; return n > 2; });");
            int c1 = counting.Count();
            Console.WriteLine($"   第一次 Count() 之后，checkCount = {checkCount}（检查了 {c1} 个元素）");
            int c2 = counting.Count();
            Console.WriteLine($"   第二次 Count() 之后，checkCount = {checkCount}   ← 又算了一遍！");
            Console.WriteLine();

            // 解决办法：ToList() 立即执行并固定结果
            var fixedList = nums.Where(n => n > 2).ToList();
            nums.Add(999);
            Console.WriteLine($"   ✅ 用 ToList() 之后：fixedList 有 {fixedList.Count} 个元素");
            Console.WriteLine($"      加了 999 之后，fixedList 仍然是 {fixedList.Count} 个（不受影响）");
            Console.WriteLine();
            Console.WriteLine("   ★ 实用建议：养成习惯，查询完就 .ToList()，行为最直观");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 9：查询语法（对照）----");

            var methodSyntax = students
                .Where(s => s.Grade >= 60)
                .OrderByDescending(s => s.Grade)
                .Select(s => s.Name)
                .ToList();

            var querySyntax = (from s in students
                               where s.Grade >= 60
                               orderby s.Grade descending
                               select s.Name).ToList();

            Console.WriteLine($"   方法语法：{string.Join(", ", methodSyntax)}");
            Console.WriteLine($"   查询语法：{string.Join(", ", querySyntax)}");
            Console.WriteLine("   ★ 两者完全等价。实际项目里【方法语法】占绝大多数");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 10：LINQ 的性能代价（实测）----");

            const int N = 1_000_000;
            var bigData = new List<int>(N);
            for (int i = 0; i < N; i++) bigData.Add(i);

            // LINQ 版本
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            var sw = Stopwatch.StartNew();
            long sumLinq = bigData.Where(n => n % 2 == 0).Select(n => (long)n).Sum();
            sw.Stop();
            double linqMs = sw.Elapsed.TotalMilliseconds;

            // 手写循环版本
            sw.Restart();
            long sumLoop = 0;
            for (int i = 0; i < bigData.Count; i++)
            {
                if (bigData[i] % 2 == 0) sumLoop += bigData[i];
            }
            sw.Stop();
            double loopMs = sw.Elapsed.TotalMilliseconds;

            Console.WriteLine($"   处理 {N:N0} 个元素（筛选偶数并求和）：");
            Console.WriteLine($"      LINQ        : {linqMs,7:F2} ms");
            Console.WriteLine($"      手写循环    : {loopMs,7:F2} ms");
            if (loopMs > 0.01) Console.WriteLine($"      → 手写循环快约 {linqMs / loopMs:F1} 倍");
            Console.WriteLine($"      （校验和：{sumLinq} / {sumLoop}）");
            Console.WriteLine();
            Console.WriteLine("   ★ 但代码可读性：LINQ 一行 vs 循环 6 行");
            Console.WriteLine("   ★ 规则：业务逻辑用 LINQ，热路径用循环。测量证明是瓶颈才优化");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("🎉 语言部分（第 00-16 课）到此结束！");
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson16Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
