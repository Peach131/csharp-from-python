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

            // ★★ 本课最核心的一条：string 是【不可变】的。
            //   下面每一行"看起来像修改"的调用（ToUpper/Trim/Replace...），
            //   实际都是在堆上【新建一个字符串】并把新对象返回给你 ——
            //   原来那个字符串对象一个字节都没动。
            string s = "  Hello World  ";

            // ① 先看原样：长度 15 是把首尾那 4 个空格也算进去的（空格也是字符）
            Console.WriteLine($"原始：'{s}'，长度 {s.Length}");

            // ② 调 ToUpper() 只打印【返回值】，故意不赋值给 s
            Console.WriteLine($"s.ToUpper() 返回：'{s.ToUpper()}'");
            //   紧跟着再打印 s 本身 —— 还是小写，因为 s 这个变量仍指向【旧对象】
            Console.WriteLine($"s 本身还是：'{s}'   ← 没变！");

            // ③ 想让 s "变大写"只有一个办法：把返回的新字符串【赋回】给 s
            //   ★ Trim() 先剥掉首尾空格得到新串，ToUpper() 再在这个新串上又造一个，
            //     链式调用一共产生 2 个中间字符串，最后只有最外层的结果被 s 接住。
            s = s.Trim().ToUpper();
            Console.WriteLine($"s = s.Trim().ToUpper() 之后：'{s}'");

            Console.WriteLine("─────────────");

            // ④ 为了演示后面的方法，重新定义一个【没有首尾空格】的 t
            string t = "Hello World";

            // Trim()：两边没空格时返回的就是原内容（StringComparison 无关，纯剥空白）
            Console.WriteLine($"Trim()            : '{t.Trim()}'");

            // Contains()：返回 bool，配合 $"{}" 输出的是 True/False（C# 的 bool 首字母大写）
            Console.WriteLine($"Contains(\"World\") : {t.Contains("World")}");

            // IndexOf()：返回【从 0 开始】的下标，H=0 e=1 l=2 l=3 o=4 空格=5 W=6
            //   找不到时返回 -1（不是抛异常，Python 的 find 也是这样）
            Console.WriteLine($"IndexOf(\"World\")  : {t.IndexOf("World")}");

            // Replace()：同样是"返回新字符串"，t 自己没被改动
            Console.WriteLine($"Replace(World→C#) : '{t.Replace("World", "C#")}'");

            // Substring(起始下标, 取几个字符)：第二个参数是【长度】不是结束下标
            //   ★ 这是最容易和 Python 切片 s[0:5] 搞混的地方
            Console.WriteLine($"Substring(0, 5)   : '{t.Substring(0, 5)}'");

            // ⑤ 补三个"访问单个字符"的写法（第 13 课速查表里的 s[i] / s[^1] / s[1..4]）
            //   s[i]   → char（单引号），不是 string
            //   s[^1]  → 索引运算符 ^ 表示"从末尾倒数"，^1 就是最后一个字符
            //   s[1..4]→ 范围切片，左闭右开（取下标 1、2、3），结果仍是 string
            Console.WriteLine($"t[0]              : '{t[0]}'");
            Console.WriteLine($"t[^1]             : '{t[^1]}'");
            Console.WriteLine($"t[1..5]           : '{t[1..5]}'");

            // ⑥ 大小写不敏感比较：Equals 的第二个参数指定比较规则
            //   ★ 比 t.ToLower() == "hello world" 更好：不用多造一个临时字符串，
            //     而且不受当前系统语言/区域设置影响（Ordinal 是纯粹的逐字符比较）。
            Console.WriteLine($"Equals(忽略大小写) : {t.Equals("HELLO WORLD", StringComparison.OrdinalIgnoreCase)}");

            // ⑦ Split 与 Join 是一对逆操作：先拆成 string[]，再用另一个分隔符拼回去
            //   ★ 注意参数顺序和 Python 相反："-".join(arr)  →  string.Join("-", arr)
            string[] parts = t.Split(' ');
            Console.WriteLine($"string.Join(\"_\") : '{string.Join("_", parts)}'");

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

            const int Times = 5000;
            Console.WriteLine($"拼接 {Times} 次：");

            // ---------- ① 反面教材：循环里 string += ----------
            // ★ Stopwatch 在 System.Diagnostics 里，本文件没有 using 它，
            //   所以写成全名；var 让编译器自己推断类型，省得记类名。
            var swBad = System.Diagnostics.Stopwatch.StartNew();

            // ★★ 这一行才是计时的起点：先记下【当前线程已经分配过多少字节】的累计值，
            //   循环跑完再取一次，两次相减就是"这次拼接到底分配了多少内存"。
            //   （不能用 Process 的内存，那样会把别的对象和 GC 都算进去）
            long beforeBad = GC.GetAllocatedBytesForCurrentThread();

            // ★ 循环拼接 = O(n²)：第 i 次 += 要新建一块能装下
            //   "旧内容 + i" 的内存，再把旧内容【整个复制】过去。
            //   5000 次加起来 ≈ 复制了几千万个字符，中间产物全是垃圾等 GC 回收。
            string bad = "";
            for (int i = 1; i <= Times; i++)
            {
                bad += i;
            }

            long allocBad = GC.GetAllocatedBytesForCurrentThread() - beforeBad;
            swBad.Stop();
            double badMs = swBad.Elapsed.TotalMilliseconds;

            // ---------- ② 正面教材：StringBuilder ----------
            // ★ 必须【重新 new 一个 Stopwatch 并 StartNew()】。
            //   如果偷懒复用上面那个 sw，读到的就是"两段代码加起来"的总时间，
            //   两种写法就比不出差别了。
            var swGood = System.Diagnostics.Stopwatch.StartNew();
            long beforeGood = GC.GetAllocatedBytesForCurrentThread();

            // ★ StringBuilder 内部维护一个【可变的 char 缓冲区】，
            //   Append 是往缓冲区里塞字符；缓冲区不够时才扩容（一般是翻倍），
            //   所以 5000 次追加只会发生十几次复制 → O(n)。
            //   不是"完全不分配"，而是"分配得少得多"，这正是它快的原因。
            var sb = new StringBuilder();
            for (int i = 1; i <= Times; i++)
            {
                sb.Append(i);
            }

            // ★ 只有最后这一下才真正生成不可变的 string；循环里一个临时字符串都没造
            string good = sb.ToString();

            long allocGood = GC.GetAllocatedBytesForCurrentThread() - beforeGood;
            swGood.Stop();
            double goodMs = swGood.Elapsed.TotalMilliseconds;

            // ---------- ③ 打印对照结果 ----------
            // / 1024.0 / 1024 得到 MB：★ 一定要除以 1024.0（带小数点的 double），
            // 写成 1024 会变成整数除法，结果直接是 0
            Console.WriteLine($"  s += i        : {badMs:F1} ms，分配 {allocBad / 1024.0 / 1024:F2} MB");
            Console.WriteLine($"  StringBuilder : {goodMs:F1} ms，分配 {allocGood / 1024.0 / 1024:F2} MB");

            // 倍率用下面这种写法是为了避开"0 不能做除数"：
            // 万一 goodMs 小到 0（计时精度不够），就退化成 1 倍，而不是抛异常
            double ratio = goodMs > 0 ? badMs / goodMs : 1;
            Console.WriteLine($"  → 慢了约 {ratio:F0} 倍");

            // ★ 两者长度必须相等（都是把 1..5000 挨个拼起来）：
            //   这是一句"正确性自检"—— 快的写法如果结果不对，那快也没用
            Console.WriteLine($"两者结果长度一致：{bad.Length} / {good.Length}");

            // 【结论】循环里拼接字符串一定要用 StringBuilder：
            //   += 每次都新建整串，分配量和耗时都随次数平方级增长；
            //   StringBuilder 只在内部缓冲区扩容时复制，耗时和分配都接近线性。
            //   ★ 但少量拼接（2~5 个）用 + 或 $"" 就好，为几段文字引入
            //     StringBuilder 反而更啰嗦，没有性能收益。

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

            // 注意这一行本身的"脏"：两头有空格、逗号后面也有空格
            string line = "  1001, 张三 , 88.5 , 重庆 , 备注:优秀  ";

            // ① 先 Trim() 去掉整行首尾的空格 —— 否则第一个字段会变成 "  1001"，
            //   最后一段会变成 "备注:优秀  "，多出来的空格会一路带到结果里
            string trimmed = line.Trim();

            // ② Split(',') 只按逗号切，它【不会】顺手帮你去掉字段两侧的空格。
            //   切完是 5 段：{"1001", " 张三 ", " 88.5 ", " 重庆 ", " 备注:优秀"}
            string[] parts = trimmed.Split(',');

            // ③ 所以每个字段都要再 Trim 一次。
            //   ★ 这里先把 5 个字段各自 Trim 好放进新数组，而不是在用的时候
            //     parts[1].Trim() 到处写 —— 一是少敲几次 Trim 不容易漏，
            //     二是后面想改用其他字段顺序时只改这一处。
            string[] fields =
            {
                parts[0].Trim(),
                parts[1].Trim(),
                parts[2].Trim(),
                parts[3].Trim(),
                parts[4].Trim(),
            };

            // ④ 字符串 → 数字：用 TryParse 而不是 Parse。
            //   TryParse 解析失败返回 false、结果给 0，【不会抛异常】；
            //   以后从文件/用户输入读进来的数据不一定合法，这个习惯更健壮。
            //   ★ out 参数写在括号里（C# 7+ 的"out 变量"写法），不用提前声明变量。
            int.TryParse(fields[0], out int id);
            double.TryParse(fields[2], out double score);

            // ⑤ 备注字段里带着前缀 "备注:"，用 Replace 把它抹掉。
            //   ★ Replace 返回的是【新字符串】，所以必须赋给一个变量才能用
            string note = fields[4].Replace("备注:", "");

            // ⑥ 打印：成绩用 F2 保留两位小数 → 88.5 显示成 88.50
            Console.WriteLine("===== 解析结果 =====");
            Console.WriteLine($"学号：{id}");
            Console.WriteLine($"姓名：{fields[1]}");
            Console.WriteLine($"成绩：{score:F2}");
            Console.WriteLine($"城市：{fields[3]}");
            Console.WriteLine($"备注：{note}");

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

            string[] names = { "苹果", "香蕉", "进口车厘子" };

            // ★ 金额用 decimal 而不是 double（第 01 课的规则）：
            //   double 是二进制浮点，0.1 这种数存不精确，算钱会攒出误差；
            //   decimal 是十进制浮点，专门为"钱的精度"设计。
            //   字面量后面的 m 是 decimal 后缀，写成 5.5 会被当成 double 而编译报错。
            decimal[] prices = { 5.5m, 3.2m, 128.888m };
            int[] counts = { 10, 25, 3 };

            // 表头：,对齐宽度 写在【值】的右边（负数=左对齐，正数=右对齐）
            //   $"{"名称",-12}"：把 "名称" 放进 12 格，左对齐（右边补空格）
            //   $"{"单价",10}" ：放进 10 格，右对齐（左边补空格）
            // ★ 关于花括号：这里的两层 {} 分别是"插值的洞"和"字符串字面量"，
            //   如果哪天要在插值字符串里【显示】一对花括号本身，
            //   必须写成 {{ 和 }} 来转义，否则编译器会以为插值的洞没关。
            Console.WriteLine($"{"名称",-12}{"单价",10}{"数量",6}{"小计",12}");

            // 42 = 后面几个列宽相加（12 并不是 42 的来源，这里只是画一条够长的分隔线）
            // ★ new string('-', 42) 是"把字符 - 重复 42 次"，比手敲 42 个减号可靠
            Console.WriteLine(new string('-', 42));

            decimal total = 0;

            for (int i = 0; i < names.Length; i++)
            {
                // ★ 先乘再累加：decimal * int 会自动转成 decimal，不会丢精度
                decimal subtotal = prices[i] * counts[i];
                total += subtotal;

                // ★ 每个占位符里都是"逗号宽度 + 冒号格式符"的组合：
                //     {names[i],-12}    名称左对齐占 12 格
                //     {prices[i],10:F2} 单价右对齐占 10 格、固定 2 位小数
                //     {counts[i],6}     数量右对齐占 6 格
                //     {subtotal,12:N2}  小计右对齐占 12 格、2 位小数 + 千分位
                //   F2 和 N2 的区别就在千分位：F2 → 1234.57，N2 → 1,234.57
                Console.WriteLine($"{names[i],-12}{prices[i],10:F2}{counts[i],6}{subtotal,12:N2}");
            }

            // ★★ 分隔线宽度必须等于表头各列宽度之和，否则会多出/少掉一截：
            //     名称 12 + 单价 10 + 数量 6 + 小计 12 = 【40】格
            //   写成 42 的话，横线会比表头两端各多伸出一个字符，一眼就能看出不齐。
            Console.WriteLine(new string('-', 40));

            // 合计行：前面 3 列没有内容，但列宽还得占住，
            // 否则最后一个数会挤到最左边，和上面的"小计"列对不齐。
            //
            // ★ -27 是怎么算出来的（这一步必须自己数，不能猜）：
            //   目标：让 521.66 的右边缘落在第 40 格，和上面 55.00 的右边缘对齐。
            //
            //     数据行：{subtotal,12:N2} → "       55.00"
            //             55.00 共 5 字符，在 12 格里右对齐 → 前面补 7 空格
            //             它的右边缘 = 12+10+6+12 = 第 40 格 ✅
            //
            //     合计行：521.66 共 6 字符（比 55.00 多 1 位整数）
            //             要让右边缘同样落在第 40 格 → 整行宽度必须是 40 格
            //             前半段 = 40 - 6 = 34 格？
            //   但前半段是 {"合计",-27}（27 格）+ {total,12:N2} 里的补白
            //     12 - 6 = 6 格补白
            //     → 27 + 6 = 33 ... 合计 2 字 + 25 空格 + 6 空格 + 6 字符 = 40 ✅
            //
            //   ★ 关键：N2 对 521.66 不产生千分位分隔符（不足 4 位整数），
            //     所以就是 6 个字符。若合计超过 999，会变成 "1,234.56"（8 字符），
            //     那时右对齐仍然成立（因为宽度是算好了的），不用改代码。
            Console.WriteLine($"{"合计",-27}{total,12:N2}");

            // ⚠️ 已知的不完美：C# 只按"字符个数"算宽度（中文字符也算 1 格），
            //   而终端里一个汉字要占 2 格，所以中文名那列的视觉对齐会略偏。
            //   要真正对齐得自己按"显示宽度"补空格，本课不要求。

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

            // 要分析的文本，照抄题目给的那一句
            string text = "the quick brown fox jumps over the lazy dog the fox";

            // 把统计逻辑封进 AnalyzeText（方法体写在 Main 的下面），
            // Main 里只留"调用"这一行 —— 这样换一段文本再调用一次就能复用。
            AnalyzeText(text);

            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }

        // ================================================================
        // TODO 5 的辅助方法：文本统计器
        // ================================================================
        // ★ 参数写成 string text 就够了，不用传 length：
        //   字符串自己知道有多长（text.Length），把相关数据都交给它自己保管。
        static void AnalyzeText(string text)
        {
            Console.WriteLine("========== 文本分析 ==========");

            // ① length 把【所有】字符都算上，包括空格和标点
            Console.WriteLine($"原文：{text}");
            Console.WriteLine($"总字符数（含空格）：{text.Length}");

            // ② 只统计"非空白字符"的个数。
            //   ★ 不要写 text.Replace(" ", "")：那只去掉半角空格，
            //     制表符 \t、换行 \n 会漏掉。char.IsWhiteSpace 才是完整判断。
            int noSpaceCount = 0;
            foreach (char c in text)
            {
                if (!char.IsWhiteSpace(c))
                {
                    noSpaceCount++;
                }
            }
            Console.WriteLine($"去掉空格后字符数：{noSpaceCount}");

            // ③ 按空格拆成单词。
            //   ★ StringSplitOptions.RemoveEmptyEntries 会把切出来的空串扔掉 ——
            //     文本里只要有连续两个空格，Split(' ') 就会多出一个 "" 元素，
            //     单词数会莫名其妙偏大，加上这个选项才安全。
            string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine($"单词数：{words.Length}");

            // ④ 一次循环同时干两件事：数词频 + 找最长单词（省一趟遍历）
            var counts = new Dictionary<string, int>();
            string longest = "";
            foreach (string w in words)
            {
                // 词频：字典里已有就 +1，没有就先记成 1。
                // ★ TryGetValue 一次查找就能同时拿到"在不在"和"值是多少"，
                //   比先 ContainsKey 再取 counts[w] 少查一遍（字典的查找是哈希，
                //   虽然快，但能省一次就省一次）。
                if (counts.TryGetValue(w, out int c))
                {
                    counts[w] = c + 1;
                }
                else
                {
                    counts[w] = 1;
                }

                // 最长单词：★ 用 > 而不是 >=，
                //   长度相同就保留【先出现】的那个，结果才是确定的；
                //   写 >= 会让"最后一个最长单词"胜出，取决于遍历顺序。
                if (w.Length > longest.Length)
                {
                    longest = w;
                }
            }

            // ⑤ 打印词频表。
            //   ★ Dictionary 的遍历顺序【没有保证】，只是实践中往往等于插入顺序；
            //     想让输出彻底固定，可以换成 SortedDictionary<string, int>，
            //     或者遍历后自己 OrderBy。这里题目的预期输出就是"你自己的顺序"。
            Console.WriteLine("---------- 词频 ----------");
            foreach (KeyValuePair<string, int> kv in counts)
            {
                // {kv.Key,-6}：单词左对齐占 6 格，保证后面的数字能对齐
                Console.WriteLine($"{kv.Key,-6}{kv.Value} 次");
            }
            Console.WriteLine("----------");

            // ⑥ longest.Length 就是字母个数，不用再遍历一遍去数
            Console.WriteLine($"最长单词：{longest}（{longest.Length} 个字母）");
        }
    }
}
