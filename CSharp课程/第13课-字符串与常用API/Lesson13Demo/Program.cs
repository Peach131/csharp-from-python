// ============================================================================
//  第 13 课 · 示例代码
//  字符串与常用 API
// ============================================================================

using System.Diagnostics;
using System.Text;

namespace Lesson13Demo
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("========== 第 13 课：字符串与常用 API ==========\n");


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 1：★ 字符串不可变 ----");

            string s = "hello";
            Console.WriteLine($"   string s = \"hello\";");
            Console.WriteLine($"   s.ToUpper() 的返回值 = \"{s.ToUpper()}\"");
            Console.WriteLine($"   s 本身              = \"{s}\"   ← 没变！");
            Console.WriteLine();
            s = s.ToUpper();
            Console.WriteLine($"   s = s.ToUpper() 之后，s = \"{s}\"   ← 必须重新赋值");
            Console.WriteLine();
            Console.WriteLine("   ★ 任何\"修改\"字符串的操作，都是在堆上创建一个【新字符串】");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 2：★★ 循环拼接的性能灾难（实测）----");

            const int N = 20_000;

            // ❌ 用 += 拼接
            var sw = Stopwatch.StartNew();
            long allocBefore = GC.GetAllocatedBytesForCurrentThread();
            string bad = "";
            for (int i = 0; i < N; i++)
            {
                bad += i;
            }
            long allocBad = GC.GetAllocatedBytesForCurrentThread() - allocBefore;
            sw.Stop();
            double badMs = sw.Elapsed.TotalMilliseconds;

            // ✅ 用 StringBuilder
            sw.Restart();
            allocBefore = GC.GetAllocatedBytesForCurrentThread();
            var sb = new StringBuilder();
            for (int i = 0; i < N; i++)
            {
                sb.Append(i);
            }
            string good = sb.ToString();
            long allocGood = GC.GetAllocatedBytesForCurrentThread() - allocBefore;
            sw.Stop();
            double goodMs = sw.Elapsed.TotalMilliseconds;

            Console.WriteLine($"   拼接 {N:N0} 次：");
            Console.WriteLine($"      s += i            : {badMs,9:F1} ms, 分配 {allocBad / 1024.0 / 1024,8:F2} MB");
            Console.WriteLine($"      StringBuilder     : {goodMs,9:F1} ms, 分配 {allocGood / 1024.0 / 1024,8:F2} MB");
            if (goodMs > 0.01)
            {
                Console.WriteLine($"      → 字符串拼接慢了约 {badMs / goodMs:F0} 倍，多分配 {allocBad / Math.Max(1, allocGood):F0} 倍内存");
            }
            Console.WriteLine($"      （两者结果一致：长度都是 {bad.Length} / {good.Length}）");
            Console.WriteLine();
            Console.WriteLine("   原因：每次 += 都要「分配更大的新内存 + 复制全部旧内容」");
            Console.WriteLine("        n 次拼接 = 复制 1+2+...+n ≈ n²/2 个字符 → O(n²)");
            Console.WriteLine("   ★ 规则：循环里拼接 / 拼接很多次 → 必须用 StringBuilder");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 3：插值格式化 ----");

            double price = 1234.5678;
            int count = 1234567;
            double rate = 0.12345;
            var now = DateTime.Now;

            Console.WriteLine($"   {{price:F2}}  → {price:F2}      固定 2 位小数");
            Console.WriteLine($"   {{price:N2}}  → {price:N2}     千分位 + 2 位小数");
            Console.WriteLine($"   {{price:C}}   → {price:C}    货币格式");
            Console.WriteLine($"   {{count:N0}}  → {count:N0}    千分位无小数");
            Console.WriteLine($"   {{rate:P1}}   → {rate:P1}       百分比 1 位");
            Console.WriteLine($"   {{rate:P2}}   → {rate:P2}      百分比 2 位");
            Console.WriteLine();
            Console.WriteLine($"   对齐：{{42,10}}  → [{42,10}]");
            Console.WriteLine($"   对齐：{{42,-10}} → [{42,-10}]");
            Console.WriteLine($"   组合：{{price,12:F2}} → [{price,12:F2}]");
            Console.WriteLine("   （语法：花括号里用逗号指定宽度，正数右对齐、负数左对齐）");
            Console.WriteLine();
            Console.WriteLine($"   日期：{{now:yyyy-MM-dd}}          → {now:yyyy-MM-dd}");
            Console.WriteLine($"   日期：{{now:yyyy-MM-dd HH:mm:ss}} → {now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"   日期：{{now:HH:mm}}                → {now:HH:mm}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 4：字符串常用操作 ----");

            string t = "  Hello World  ";
            Console.WriteLine($"   原始            : \"{t}\"（长度 {t.Length}）");
            Console.WriteLine($"   Trim()          : \"{t.Trim()}\"");
            Console.WriteLine($"   Trim().Length   : {t.Trim().Length}");
            Console.WriteLine($"   ToUpper()       : {t.Trim().ToUpper()}");
            Console.WriteLine($"   Contains(\"World\"): {t.Contains("World")}");
            Console.WriteLine($"   IndexOf(\"World\") : {t.IndexOf("World")}");
            Console.WriteLine($"   Replace(World→C#): {t.Trim().Replace("World", "C#")}");
            Console.WriteLine($"   Substring(2, 5) : \"{t.Trim().Substring(2, 5)}\"");
            Console.WriteLine($"   t[0]            : '{t[0]}'");
            Console.WriteLine($"   t[1..4]         : \"{t[1..4]}\"   ← 范围切片");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 5：拆分和连接 ----");

            string csv = "张三,25,重庆";
            string[] parts = csv.Split(',');
            Console.WriteLine($"   \"{csv}\".Split(',') → [{string.Join(" | ", parts)}]");
            Console.WriteLine($"      长度 = {parts.Length}");
            for (int i = 0; i < parts.Length; i++)
            {
                Console.WriteLine($"      parts[{i}] = {parts[i]}");
            }
            Console.WriteLine();
            Console.WriteLine($"   string.Join(\" + \", parts) → {string.Join(" + ", parts)}");
            Console.WriteLine("   ★ 注意参数顺序：C# 是 Join(分隔符, 数组)，Python 是 分隔符.join(数组)");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 6：★ 字符串比较的坑 ----");

            string a1 = "hello";
            string b1 = string.Concat("hel", "lo");     // 运行时拼接
            string c1 = "HELLO";

            Console.WriteLine($"   a1 = \"hello\"，b1 = string.Concat(\"hel\",\"lo\")");
            Console.WriteLine($"      a1 == b1               = {a1 == b1}        ← 按【内容】比较");
            Console.WriteLine($"      ReferenceEquals(a1,b1) = {ReferenceEquals(a1, b1)}       ← 按【地址】比较，不同对象");
            Console.WriteLine();
            Console.WriteLine($"   忽略大小写比较 \"hello\" 和 \"HELLO\"：");
            Console.WriteLine($"      a1 == c1                                = {a1 == c1}");
            Console.WriteLine($"      a1.Equals(c1, OrdinalIgnoreCase)        = {a1.Equals(c1, StringComparison.OrdinalIgnoreCase)}   ← 推荐写法");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 7：判断空字符串 ----");

            string? empty1 = "";
            string? empty2 = null;
            string? spaces = "   ";
            string? normal = "abc";

            Console.WriteLine($"   {"输入",-10}{"IsNullOrEmpty",-18}{"IsNullOrWhiteSpace"}");
            Console.WriteLine("   " + new string('-', 52));
            foreach (var item in new[] { empty1, empty2, spaces, normal })
            {
                string label = item is null ? "null" : $"\"{item}\"";
                Console.WriteLine($"   {label,-10}{string.IsNullOrEmpty(item),-18}{string.IsNullOrWhiteSpace(item)}");
            }
            Console.WriteLine("   ★ IsNullOrWhiteSpace 更实用：它能识别\"全是空格\"（用户按空格键的情况）");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 8：DateTime 和 TimeSpan ----");

            var now2 = DateTime.Now;
            Console.WriteLine($"   当前时间     : {now2:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"   明天这时候   : {now2.AddDays(1):yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"   一月前       : {now2.AddMonths(-1):yyyy-MM-dd}");
            Console.WriteLine($"   星期几       : {now2:dddd}");
            Console.WriteLine();

            var ts = new TimeSpan(2, 30, 15);         // 2 时 30 分 15 秒
            Console.WriteLine($"   TimeSpan(2,30,15)  = {ts}");
            Console.WriteLine($"      ts.Hours         = {ts.Hours}        ← 小时的【部分】");
            Console.WriteLine($"      ts.TotalHours    = {ts.TotalHours}     ← 【总】小时数");
            Console.WriteLine();

            var ts2 = TimeSpan.FromDays(2) + TimeSpan.FromHours(5);
            Console.WriteLine($"   2 天 + 5 小时：");
            Console.WriteLine($"      ts2.Hours        = {ts2.Hours}        ← 只是小时部分");
            Console.WriteLine($"      ts2.TotalHours   = {ts2.TotalHours}       ← 总小时数（= 53）");
            Console.WriteLine("   ⚠️ 这两个非常容易搞混，用 Total* 才是总数");
            Console.WriteLine();

            // 计算时间差
            var start = DateTime.Now;
            var end = start.AddHours(3).AddMinutes(45);
            var gap = end - start;
            Console.WriteLine($"   时间差计算：{gap.TotalHours} 小时 = {gap.TotalMinutes} 分钟");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 9：Math 和 Convert ----");

            Console.WriteLine($"   Math.Abs(-5)        = {Math.Abs(-5)}");
            Console.WriteLine($"   Math.Round(3.567,2) = {Math.Round(3.567, 2)}");
            Console.WriteLine($"   Math.Floor(3.9)     = {Math.Floor(3.9)}");
            Console.WriteLine($"   Math.Ceiling(3.1)   = {Math.Ceiling(3.1)}");
            Console.WriteLine($"   Math.Pow(2,10)      = {Math.Pow(2, 10)}");
            Console.WriteLine($"   Math.Sqrt(16)       = {Math.Sqrt(16)}");
            Console.WriteLine($"   Math.PI             = {Math.PI:F6}   ← 常量，不加括号");
            Console.WriteLine();

            Console.WriteLine($"   Convert.ToInt32(\"123\")  = {Convert.ToInt32("123")}");
            Console.WriteLine($"   Convert.ToDouble(\"3.14\") = {Convert.ToDouble("3.14")}");
            Console.WriteLine($"   Convert.ToInt32(null)     = {Convert.ToInt32((string?)null)}   ← null 变成 0，不抛异常");
            // int.Parse(null) 会抛 ArgumentNullException
            Console.WriteLine("   （对比：int.Parse(null) 会抛 ArgumentNullException）");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson13Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
