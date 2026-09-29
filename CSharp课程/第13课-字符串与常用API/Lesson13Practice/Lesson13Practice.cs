// ============================================================================
//  第 13 课 · 练习：字符串与常用 API
//
//  ★ 本课重点：
//     1. 字符串不可变，循环拼接必须用 StringBuilder
//     2. Split / Join / Substring / Replace 的用法
//     3. 忽略大小写比较用 StringComparison.OrdinalIgnoreCase
// ============================================================================

using System.Text;

namespace LessonPractice
{
    class Lesson13Practice
    {
        static void Main()
        {
            Console.WriteLine("===== 第 13 课练习：字符串与常用 API =====\n");

            // ================================================================
            // TODO 1 ｜ 验证字符串不可变 + 常用操作
            // ================================================================
            // 【要求】给定 string s = "  Hello World  ";
            //   1. 打印原始字符串和它的长度
            //   2. 调用 s.ToUpper()，打印返回值，再打印 s 本身（验证 s 没变）
            //   3. 把 ToUpper 的结果赋回 s，再打印
            //   4. 依次演示：Trim()、Contains、IndexOf、Replace、Substring
            //
            // 【预期输出】
            //     原始：'  Hello World  '，长度 15
            //     s.ToUpper() 返回：'  HELLO WORLD  '
            //     s 本身还是：'  Hello World  '   ← 没变！
            //     s = s.Trim().ToUpper() 之后：'HELLO WORLD'
            //     ─────────────
            //     Trim()            : 'Hello World'
            //     Contains("World") : True
            //     IndexOf("World")  : 6
            //     Replace(World→C#) : 'Hello C#'
            //     Substring(0, 5)   : 'Hello'
            //
            // 【提示】
            //     string s = "  Hello World  ";
            //     Console.WriteLine($"原始：'{s}'，长度 {s.Length}");
            //     Console.WriteLine($"s.ToUpper() 返回：'{s.ToUpper()}'");
            //     Console.WriteLine($"s 本身还是：'{s}'   ← 没变！");
            //     s = s.Trim().ToUpper();
            //
            //     然后重新定义一个 t = "Hello World" 来演示后面的操作
            //
            // 【考察点】不可变性：ToUpper() 返回新字符串，原字符串不变

            Console.WriteLine("--- TODO 1 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ ★ 性能对照：+= 和 StringBuilder
            // ================================================================
            // 【要求】
            //   1. 用两种方式拼接 1 到 5000 的数字
            //   2. 都用 Stopwatch 计时，并打印耗时
            //   3. 用 GC.GetAllocatedBytesForCurrentThread() 测量各自分配了多少内存
            //   4. 得出结论（写一句注释）
            //
            // 【预期输出】（数字会不同）
            //     拼接 5000 次：
            //       s += i        : XXX ms，分配 XX.XX MB
            //       StringBuilder : X ms，分配 X.XX MB
            //       → 慢了约 XX 倍
            //     两者结果长度一致：XXXXX / XXXXX
            //
            // 【提示】
            //     var sw = System.Diagnostics.Stopwatch.StartNew();
            //     long before = GC.GetAllocatedBytesForCurrentThread();
            //
            //     string bad = "";
            //     for (int i = 1; i <= 5000; i++) bad += i;
            //
            //     long allocBad = GC.GetAllocatedBytesForCurrentThread() - before;
            //     sw.Stop();
            //     double badMs = sw.Elapsed.TotalMilliseconds;
            //
            //     // StringBuilder 同理
            //     var sb = new StringBuilder();
            //     for (int i = 1; i <= 5000; i++) sb.Append(i);
            //     string good = sb.ToString();
            //
            //     Console.WriteLine($"s += i        : {badMs:F1} ms，分配 {allocBad / 1024.0 / 1024:F2} MB");
            //
            // 【考察点】
            //   · += 是 O(n²)，StringBuilder 是 O(n)
            //   · 用真实数字说话，不要凭感觉

            Console.WriteLine("--- TODO 2 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ 解析一行数据（实战）
            // ================================================================
            // 【要求】给定一行 CSV 格式的数据：
            //     string line = "  1001, 张三 , 88.5 , 重庆 , 备注:优秀  ";
            //   要求：
            //     1. 去掉首尾空格
            //     2. 按逗号拆分
            //     3. 每个字段再 Trim 一次（因为逗号后面有空格）
            //     4. 解析出：学号（int）、姓名（string）、成绩（double）、城市、备注
            //     5. 打印成规范格式
            //
            // 【预期输出】
            //     ===== 解析结果 =====
            //     学号：1001
            //     姓名：张三
            //     成绩：88.50
            //     城市：重庆
            //     备注：优秀
            //
            // 【提示】
            //     string line = "  1001, 张三 , 88.5 , 重庆 , 备注:优秀  ";
            //     string[] parts = line.Trim().Split(',');
            //
            //     // ★ 每个字段都要再 Trim（否则 " 张三 " 会带空格）
            //     int id = int.Parse(parts[0].Trim());
            //     string name = parts[1].Trim();
            //     double score = double.Parse(parts[2].Trim());
            //     string city = parts[3].Trim();
            //     string note = parts[4].Trim().Replace("备注:", "");
            //
            //   ★ 更健壮的写法：用 int.TryParse / double.TryParse 而不是 Parse
            //
            // 【考察点】
            //   · Split 不会自动去掉字段两侧的空格，必须手动 Trim
            //   · Trim().Replace() 链式调用的写法

            Console.WriteLine("--- TODO 3 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ 格式化输出报表
            // ================================================================
            // 【要求】用格式化输出一个对齐的商品报表：
            //     string[] names = { "苹果", "香蕉", "进口车厘子" };
            //     decimal[] prices = { 5.5m, 3.2m, 128.888m };
            //     int[] counts = { 10, 25, 3 };
            //
            //   要求表头对齐，每行显示：名称（左对齐 12 格）、单价（右对齐 10 格，2 位小数）、
            //   数量（右对齐 6 格）、小计（右对齐 12 格，2 位小数 + 千分位）
            //
            // 【预期输出】
            //     名称          单价      数量        小计
            //     ------------------------------------------
            //     苹果          5.50        10       55.00
            //     香蕉          3.20        25       80.00
            //     进口车厘子    128.89         3      386.66
            //     ------------------------------------------
            //     合计                              521.66
            //
            // 【提示】
            //     Console.WriteLine($"{"名称",-12}{"单价",10}{"数量",6}{"小计",12}");
            //     Console.WriteLine(new string('-', 42));
            //
            //     decimal total = 0;
            //     for (int i = 0; i < names.Length; i++)
            //     {
            //         decimal subtotal = prices[i] * counts[i];
            //         total += subtotal;
            //         Console.WriteLine($"{names[i],-12}{prices[i],10:F2}{counts[i],6}{subtotal,12:N2}");
            //     }
            //
            //   ⚠️ 注意：C# 里中文字符的"宽度"是按 1 算的（不是 2），
            //      所以中文和英文混排时对齐会略有偏差。这里不必强求完美。
            //
            // 【考察点】
            //   · `,对齐宽度` 和 `:格式符` 可以组合使用
            //   · 金额用 decimal（第 01 课的规则）

            Console.WriteLine("--- TODO 4 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ 综合：文本统计器（本课考核）
            // ================================================================
            // 【要求】写一个方法 AnalyzeText(string text)，统计并打印：
            //     1. 总字符数（含空格）
            //     2. 去掉空格后的字符数
            //     3. 单词数（按空格拆分，忽略空字符串）
            //     4. 每个单词出现的次数（用 Dictionary，第 09 课学过）
            //     5. 最长的单词
            //
            // 【文本】（照抄）
            //     string text = "the quick brown fox jumps over the lazy dog the fox";
            //
            // 【预期输出】
            //     ========== 文本分析 ==========
            //     原文：the quick brown fox jumps over the lazy dog the fox
            //     总字符数（含空格）：51
            //     去掉空格后字符数：42
            //     单词数：11
            //     ---------- 词频 ----------
            //     the   3 次
            //     quick 1 次
            //     ... (按你自己的遍历顺序)
            //     ----------
            //     最长单词：quick（5 个字母）
            //
            // 【提示】
            //     static void AnalyzeText(string text)
            //     {
            //         Console.WriteLine($"总字符数（含空格）：{text.Length}");
            //         Console.WriteLine($"去掉空格后字符数：{text.Replace(" ", "").Length}");
            //
            //         string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            //         Console.WriteLine($"单词数：{words.Length}");
            //
            //         var counts = new Dictionary<string, int>();
            //         string longest = "";
            //         foreach (string w in words)
            //         {
            //             // 统计词频
            //             if (counts.TryGetValue(w, out int c)) counts[w] = c + 1;
            //             else counts[w] = 1;
            //
            //             // 找最长
            //             if (w.Length > longest.Length) longest = w;
            //         }
            //         foreach (var kv in counts) Console.WriteLine($"{kv.Key,-6}{kv.Value} 次");
            //         Console.WriteLine($"最长单词：{longest}（{longest.Length} 个字母）");
            //     }
            //
            //   ★ StringSplitOptions.RemoveEmptyEntries 会自动去掉空字符串
            //     （如果文本里有连续空格，"a  b".Split(' ') 会产生空字符串）
            //
            // 【考察点】
            //   · 组合运用：字符串方法 + Dictionary + 循环 + 格式化
            //   · 一次遍历同时做多件事（统计词频 + 找最长）

            Console.WriteLine("--- TODO 5 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
