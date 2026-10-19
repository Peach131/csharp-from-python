namespace LessonPractice
{
    // ============================================================================
    //  第 22 课 · 示例：字符串的底层真相
    //
    //  ★ 前置：第 13 课（字符串怎么用）。本课回答的是"为什么"和"代价是多少"。
    //  ★ 对应实验：InternalsLab 菜单 10（字符串驻留）、菜单 11（拼接 vs StringBuilder）
    //
    //  ★ 运行方式：
    //      1. 打开 第22课.sln，把 Lesson22Demo 设为启动项目，Ctrl + F5
    //      2. 或者命令行：dotnet run --project Lesson22Demo
    //
    //  ★ 本示例会打印【实测数据】（耗时 + 分配字节数）。
    //    数字和你的机器有关，不要背具体数值，要看【数量级和倍率】。
    //
    //  ★ 全篇的重点只有一句话：
    //      字符串看起来像语法糖，其实每一条"顺手写出来"的写法背后
    //      都对应着【堆上的一次分配 + 一次内存复制】。
    // ============================================================================
    class Lesson22Demo
    {
        // ════════════════════════════════════════════════════════════════════
        //  测量工具
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// 读当前线程【从进程启动到现在】累计分配过的字节数。
        /// ★ 两次相减 = 这段时间里这个线程分配了多少托管内存。
        /// ★ 它只统计"分配"，不管有没有被 GC 回收 —— 这正是我们要的数字：
        ///   拼接灾难的危害不在于"内存没释放"，而在于"分配得太频繁，把 GC 逼疯"。
        /// </summary>
        static long Allocated() => GC.GetAllocatedBytesForCurrentThread();

        /// <summary>
        /// 测一段代码的耗时和分配量。
        /// ★ 为什么先 warmup？因为第一次执行的方法要被 JIT 编译（第 17 课），
        ///   那部分开销不该算进被测代码里。
        /// ★ 为什么先 GC.Collect()？为了从"干净的起点"开始计数，
        ///   免得把上一段代码残留的垃圾算到这一段头上。
        ///   （生产代码永远不要调 GC.Collect，只有测试和诊断可以。）
        /// </summary>
        static (double Ms, double Mb) Measure(Action action, int warmup = 1)
        {
            for (int i = 0; i < warmup; i++)
            {
                action();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long before = Allocated();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            action();

            sw.Stop();
            long used = Allocated() - before;

            return (sw.Elapsed.TotalMilliseconds, used / 1024.0 / 1024.0);
        }

        /// <summary>打印一行"方式 / 耗时 / 分配"的对照数据。</summary>
        static void Report(string label, (double Ms, double Mb) r)
        {
            // {label,-32} 左对齐 32 格；{r.Ms,9:F1} 右对齐 9 格保留 1 位小数
            Console.WriteLine($"  {label,-32}{r.Ms,9:F1} ms{r.Mb,10:F2} MB");
        }

        /// <summary>打印一条分隔线。</summary>
        static void Rule(char c = '─', int width = 68)
        {
            Console.WriteLine(new string(c, width));
        }

        /// <summary>
        /// 判断一个 char 是不是"代理对的前半个"（high surrogate）。
        /// ★ 0xD800~0xDBFF 这段区间专门用来表示"这个 char 和后一个 char 拼起来才是一个字"。
        /// </summary>
        static bool IsHighSurrogate(char c) => c >= '\uD800' && c <= '\uDBFF';

        /// <summary>判断一个 char 是不是"代理对的后半个"（low surrogate）。</summary>
        static bool IsLowSurrogate(char c) => c >= '\uDC00' && c <= '\uDFFF';

        // ════════════════════════════════════════════════════════════════════
        //  主流程
        // ════════════════════════════════════════════════════════════════════
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("===== 第 22 课：字符串的底层真相 =====");
            Console.WriteLine();
            Console.WriteLine("本课不教\"怎么用字符串\"（那是第 13 课），本课回答三个问题：");
            Console.WriteLine("  ① 为什么它改不了？（编译器层面，不是运行时）");
            Console.WriteLine("  ② 为什么两个一样的字符串可能是同一个对象？（驻留池）");
            Console.WriteLine("  ③ 为什么改字符串这么贵？（分配 + 复制）");
            Console.WriteLine();

            Section1IndexerIsReadOnly();
            Section2Interning();
            Section3InternDanger();
            Section4Utf16();
            Section5ConcatDisaster();
            Section6ModernTools();
            Section7BreakIt();

            Console.WriteLine("===== 示例结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }

        // ════════════════════════════════════════════════════════════════════
        //  一、不可变性的技术根基：索引器只有 get，没有 set
        // ════════════════════════════════════════════════════════════════════
        static void Section1IndexerIsReadOnly()
        {
            Console.WriteLine("---------- 一、不可变性的技术根基：索引器没有 set ----------");
            Console.WriteLine();

            string s = "Hello";
            Console.WriteLine($"  s = \"{s}\"");

            // ★ s[0] 读得到：索引器有 get 访问器
            Console.WriteLine($"  s[0]  = '{s[0]}'    ← 读得到，因为索引器有 get");
            Console.WriteLine();

            // ★★ 下面这一行是本课的第一句话，但它是一句【注释】，不是代码。
            //    因为如果写成真代码：
            //
            //        s[0] = 'J';        // ❌ 编译错误 CS0200
            //
            //    编译器会报：
            //        CS0200: 无法为属性或索引器 "string.this[int]" 赋值 -- 它是只读的
            //
            //    ★ 注意措辞："它是只读的" —— 这是【编译器】在拒绝你，
            //      不是运行时的异常。你的程序根本编译不出 dll 来。
            Console.WriteLine("  s[0] = 'J';   ← 这一行【编译不过】（CS0200），所以本示例里没写");
            Console.WriteLine();
            Console.WriteLine("  C# 编译器给的原文是：");
            Console.WriteLine("    CS0200: 无法为属性或索引器 \"string.this[int]\" 赋值 -- 它是只读的");
            Console.WriteLine();

            Rule();
            Console.WriteLine("  ★ 和 Python 对照（这是本课第一个重要差异）：");
            Console.WriteLine();
            Console.WriteLine("     Python:                              C#:");
            Console.WriteLine("       s = \"Hello\"                          string s = \"Hello\";");
            Console.WriteLine("       s[0] = 'J'                           s[0] = 'J';");
            Console.WriteLine("       ─────────────                        ─────────────");
            Console.WriteLine("       TypeError:                           CS0200（编译错误）");
            Console.WriteLine("       'str' object does not               程序根本【生成不出】");
            Console.WriteLine("       support item assignment              可执行文件");
            Console.WriteLine();
            Console.WriteLine("     两者都在说\"字符串不可改\"，但发现问题的时机不同：");
            Console.WriteLine("       Python → 代码【跑到这一行】才炸（运行时）");
            Console.WriteLine("       C#     → 敲完回车，IDE 当场标红（编译时）");
            Console.WriteLine();
            Console.WriteLine("  ★ 为什么 C# 能更早发现？因为 C# 是静态类型的：");
            Console.WriteLine("    索引器只是一个名叫 this[int] 的【属性】。");
            Console.WriteLine("    属性可以有 get、有 set、或者只有 get。");
            Console.WriteLine("    string 的索引器【只声明了 get】，于是\"赋值\"这件事");
            Console.WriteLine("    在类型系统里根本不存在 —— 编译器的类型检查一过就拦下了。");
            Console.WriteLine();
            Console.WriteLine("    而 Python 的 s[0] = x 最终会调用 __setitem__，");
            Console.WriteLine("    str 类没实现 __setitem__，所以只能在运行时抛 TypeError。");
            Rule();
            Console.WriteLine();

            // ── 那"想改"怎么办？造一个新的 ──────────────────────────────────
            string original = "Hello";
            string replaced = original.Replace('H', 'J');

            Console.WriteLine($"  original            = \"{original}\"");
            Console.WriteLine($"  original.Replace    = \"{replaced}\"");
            Console.WriteLine($"  原对象变了吗？        没有 —— 还是 \"{original}\"");
            Console.WriteLine($"  ReferenceEquals     = {ReferenceEquals(original, replaced)}   ← 不是同一个对象");
            Console.WriteLine();

            // ── 用拼接"改一个字符" ────────────────────────────────────────
            // ★ 这一行最能说明问题：你在源码里只写了 2 个片段，
            //   但运行时会【先分配一个长度为 5 的新字符串】，把两段内容复制进去。
            //   原来那个 "Hello" 一个字节都没动。
            string patched = s[0] + "ello";
            Console.WriteLine($"  s[0] + \"ello\"        = \"{patched}\"");
            Console.WriteLine($"  ReferenceEquals(s, patched) = {ReferenceEquals(s, patched)}");
            Console.WriteLine();

            // ★ char + string：char 会先被转成"只含一个字符的字符串"，然后两个字符串拼接。
            //   所以这一行最少产生 2 个临时字符串对象（转换 1 个 + 拼接 1 个）。
            Console.WriteLine("  ★ s[0] 是 char，char + string 时 char 会被转成字符串，");
            Console.WriteLine("    这一步本身就分配了一个 1 字符的临时字符串，然后才拼接。");
            Console.WriteLine("    \"改一个字符\" = 至少 2 次堆分配。");
            Console.WriteLine();

            // ── 想真的原地改：只有一种合法途径，而且要一开始就用对类型 ─────
            Console.WriteLine("  ★ 那如果我就是想\"原地改一个字符\"呢？");
            Console.WriteLine("    唯一合法的途径是【一开始就别用 string】：");
            Console.WriteLine();
            Console.WriteLine("        var chars = new char[] { 'H', 'e', 'l', 'l', 'o' };");
            Console.WriteLine("        chars[0] = 'J';        // ✅ 数组的索引器【有】 set");
            Console.WriteLine("        string fromArray = new string(chars);   // 最后再冻结成字符串");
            Console.WriteLine();
            Console.WriteLine("    数组的索引器是\"可读可写\"的（它返回的是一个存储位置），");
            Console.WriteLine("    而 string 的索引器只返回【值】—— 两者在类型系统里完全不同。");
            Console.WriteLine();
            Console.WriteLine("    这也是 StringBuilder 的原理（第 13 课用过它）：");
            Console.WriteLine("    StringBuilder 内部就是一个可变的 char 数组。");
            Console.WriteLine();

            // 顺手验证一下"数组可写、字符串不可写"
            char[] chars = { 'H', 'e', 'l', 'l', 'o' };
            chars[0] = 'J';
            string fromArray = new string(chars);
            Console.WriteLine($"  实测：char[] 改完 → new string(chars) = \"{fromArray}\"");
            Console.WriteLine($"        ReferenceEquals(s, fromArray)   = {ReferenceEquals(s, fromArray)}");
            Console.WriteLine($"        （用了 {chars.Length} 个元素的 char[]，s 依然是 \"{s}\"）");
            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════════
        //  二、驻留（Interning）：为什么两个 "abc" 是同一个对象
        // ════════════════════════════════════════════════════════════════════
        static void Section2Interning()
        {
            Console.WriteLine("---------- 二、驻留：字面量共享同一个对象 ----------");
            Console.WriteLine();

            // ── 实验 A：字面量 ──────────────────────────────────────────────
            string a = "abc";
            string b = "abc";

            Console.WriteLine("  string a = \"abc\";");
            Console.WriteLine("  string b = \"abc\";");
            Console.WriteLine();
            Console.WriteLine($"  a == b                : {a == b}      ← 比【内容】（string 重载了 ==）");
            Console.WriteLine($"  ReferenceEquals(a, b) : {ReferenceEquals(a, b)}      ← 比【身份】★ 竟然是 True！");
            Console.WriteLine();
            Console.WriteLine("  ★ 同一个字面量在源码里出现两次，编译后只保留【一份】数据，");
            Console.WriteLine("    两个变量指向堆上的同一个对象。这就是\"驻留（interning）\"。");
            Console.WriteLine("    为什么可以这么做？因为字符串不可变 —— 共享是绝对安全的。");
            Console.WriteLine();

            // ── 实验 B：编译期常量折叠 ─────────────────────────────────────
            string c = "ab" + "c";
            Console.WriteLine("  string c = \"ab\" + \"c\";      ← 两边都是字面量");
            Console.WriteLine($"  ReferenceEquals(a, c) : {ReferenceEquals(a, c)}      ← 还是同一个对象");
            Console.WriteLine();
            Console.WriteLine("  ★ 编译器在【编译期】就把 \"ab\" + \"c\" 算成了 \"abc\"，");
            Console.WriteLine("    所以 c 也是一个字面量，照样进池。这叫\"常量折叠\"。");
            Console.WriteLine("    ★ 变量参与就不行了 —— 见实验 C。");
            Console.WriteLine();

            // ── 实验 C：运行时才拼出来的字符串不进池 ───────────────────────
            string left = "ab";
            string d = left + "c";                 // 运行时拼接
            Console.WriteLine("  string left = \"ab\";");
            Console.WriteLine("  string d = left + \"c\";       ← 有变量参与，只能运行时算");
            Console.WriteLine($"  d == a                : {d == a}      ← 内容一样");
            Console.WriteLine($"  ReferenceEquals(a, d) : {ReferenceEquals(a, d)}     ← ★ 不同对象！");
            Console.WriteLine();

            string e = string.Concat("abc");
            Console.WriteLine("  string e = string.Concat(\"abc\");");
            Console.WriteLine($"  ReferenceEquals(a, e) : {ReferenceEquals(a, e)}     ← 也不同");
            Console.WriteLine($"  但内容仍然相等：a == e : {a == e}");
            Console.WriteLine();

            string f = new string(new[] { 'a', 'b', 'c' });
            Console.WriteLine("  string f = new string(new[] { 'a', 'b', 'c' });");
            Console.WriteLine($"  ReferenceEquals(a, f) : {ReferenceEquals(a, f)}     ← 也不同");
            Console.WriteLine();

            // ── 实验 D：空字符串的驻留 ────────────────────────────────────
            string empty1 = "";
            string empty2 = string.Empty;
            Console.WriteLine("  string empty1 = \"\";");
            Console.WriteLine("  string empty2 = string.Empty;");
            Console.WriteLine($"  ReferenceEquals(empty1, empty2) : {ReferenceEquals(empty1, empty2)}");
            Console.WriteLine("  ★ 空字符串也是一个特例：整个进程只有一份 \"\"。");
            Console.WriteLine("    所以判断空串时用 s.Length == 0 或 s == \"\" 都一样，不会有分配。");
            Console.WriteLine();

            Rule();
            Console.WriteLine("  ★ Python 对照：Python 也有\"驻留\"，但它是【可选 + 有限】的");
            Console.WriteLine();
            Console.WriteLine("     Python:                                  C#:");
            Console.WriteLine("       a = \"abc\"                                string a = \"abc\";");
            Console.WriteLine("       b = \"abc\"                                string b = \"abc\";");
            Console.WriteLine("       a is b  →  True                          ReferenceEquals → True");
            Console.WriteLine("       （CPython 对\"看起来像标识符\"的            （★ 所有字面量都驻留，");
            Console.WriteLine("        字面量做了驻留，属于实现细节）            这是规范保证的行为）");
            Console.WriteLine();
            Console.WriteLine("       import sys                               using System;");
            Console.WriteLine("       c = sys.intern(some_string)              string c = string.Intern(some);");
            Console.WriteLine("                                                // 返回池里那一份");
            Console.WriteLine();
            Console.WriteLine("     ★ 三个关键差异：");
            Console.WriteLine("       1. C# 的驻留是【强制】的：编译期所有字面量都在池里，一个不漏。");
            Console.WriteLine("          Python 只对\"看起来像标识符\"的常量做驻留，");
            Console.WriteLine("          而且那是 CPython 的实现细节、不是语言规范（PyPy 就不一样）。");
            Console.WriteLine("       2. C# 的驻留池【永不释放】，这是内存泄漏风险（见第三节）。");
            Console.WriteLine("          Python 的 sys.intern 官方文档特意声明");
            Console.WriteLine("          \"interned strings are not immortal\"，");
            Console.WriteLine("          但只要还有引用就留着 —— 实践中同样不要拿用户输入去 intern。");
            Console.WriteLine("       3. Python 里 == 比内容、is 比身份；");
            Console.WriteLine("          C# 里 == 也被 string 重载成比内容，比身份要用 ReferenceEquals。");
            Console.WriteLine("          ★ 这和 Python 的 is 不同：C# 的 == 对字符串【默认是值语义】。");
            Rule();
            Console.WriteLine();

            // 把上面用到的变量都"用一下"，避免编译器抱怨未使用
            Console.WriteLine($"  （本节的字符串：{a}{b}{c}{d}{e}{f}{empty1}{empty2}，用于防止被优化掉）");
            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════════
        //  三、手动驻留 string.Intern() 与它的危险
        // ════════════════════════════════════════════════════════════════════
        static void Section3InternDanger()
        {
            Console.WriteLine("---------- 三、string.Intern()：手动入池，以及它的危险 ----------");
            Console.WriteLine();

            string literal = "hello";

            // 用 char[] 造一个"内容一模一样但对象不同"的字符串
            string built = new string(new[] { 'h', 'e', 'l', 'l', 'o' });
            Console.WriteLine("  string literal = \"hello\";                      ← 字面量，已在池里");
            Console.WriteLine("  string built   = new string(new[]{'h','e',...});  ← 运行时造的，不在池里");
            Console.WriteLine();
            Console.WriteLine($"  built == literal                : {built == literal}      ← 内容相同");
            Console.WriteLine($"  ReferenceEquals(literal, built) : {ReferenceEquals(literal, built)}     ← 不是同一个对象");
            Console.WriteLine();

            // ★ string.Intern 不改变 built 本身，它【返回】池里那一份
            string interned = string.Intern(built);
            Console.WriteLine("  string interned = string.Intern(built);");
            Console.WriteLine($"  ReferenceEquals(literal, interned) : {ReferenceEquals(literal, interned)}      ← 变成同一个了！");
            Console.WriteLine($"  ReferenceEquals(built,  interned)  : {ReferenceEquals(built, interned)}     ← built 自己没变");
            Console.WriteLine();
            Console.WriteLine("  ★ 关键：Intern 的返回值才是在池里的对象。");
            Console.WriteLine("    想让 built 也用池里那份，必须【接住返回值】：");
            Console.WriteLine("        built = string.Intern(built);    ← 少了这个赋值就等于没写");
            Console.WriteLine();

            // ── IsInterned：只查不插 ─────────────────────────────────────
            string notInPool = new string(new[] { 'z', 'z', 'z' });
            string? found = string.IsInterned(notInPool);
            Console.WriteLine("  string.IsInterned(x) 只【查】不【插】：");
            Console.WriteLine($"    对 \"{notInPool}\" 的结果 = {(found is null ? "null（池里没有）" : found)}");
            Console.WriteLine($"    对 \"hello\" 的结果     = {string.IsInterned("hello") ?? "null"}");
            Console.WriteLine();
            Console.WriteLine("  ★ IsInterned 返回 null 表示\"池里没有\"，它不会顺手把字符串塞进去。");
            Console.WriteLine("    这是安全地\"查询驻留状态\"的唯一方式。");
            Console.WriteLine();

            Rule('!');
            Console.WriteLine("  ⚠️⚠️ 警告：驻留池里的字符串【直到进程结束都不会被回收】");
            Rule('!');
            Console.WriteLine();
            Console.WriteLine("  驻留池不是普通堆对象。GC 扫描根的时候会把它当成【根】的一部分，");
            Console.WriteLine("  所以池里的字符串永远是\"可达\"的 —— GC 永远不敢回收它。");
            Console.WriteLine("  换句话说：");
            Console.WriteLine();
            Console.WriteLine("      string.Intern(x)  ==  一次【永久】的内存承诺");
            Console.WriteLine();
            Console.WriteLine("  ❌ 绝对不要这样写：");
            Console.WriteLine();
            Console.WriteLine("      // 从网络/文件/输入框拿到的字符串");
            Console.WriteLine("      string s = string.Intern(userInput);");
            Console.WriteLine();
            Console.WriteLine("  为什么这是【拒绝服务攻击】的入口：");
            Console.WriteLine();
            Console.WriteLine("      用户可以给你发 100 万个【各不相同】的字符串。");
            Console.WriteLine("      每个都进池 → 池里永久留着 100 万个字符串，");
            Console.WriteLine("      每个平均 50 字符 = 100 字节 + 对象头，约 130 字节");
            Console.WriteLine("      → 约 130 MB 内存【这辈子都不会还给你】。");
            Console.WriteLine("      再发 1000 万个？进程直接被 OOM 杀掉。");
            Console.WriteLine();
            Console.WriteLine("      ★ 而且注意：这 100 万个字符串里【没有一个】是重复的，");
            Console.WriteLine("        也就是说驻留本来想省的\"去重\"收益是 0 ——");
            Console.WriteLine("        你付出了永久的代价，换来了零收益。");
            Console.WriteLine();
            Console.WriteLine("  ✅ 正确做法：用自己的 Dictionary 做缓存");
            Console.WriteLine();
            Console.WriteLine("      var cache = new Dictionary<string, string>();");
            Console.WriteLine("      if (!cache.TryGetValue(raw, out string? v))");
            Console.WriteLine("      {");
            Console.WriteLine("          v = raw;");
            Console.WriteLine("          cache[raw] = v;");
            Console.WriteLine("      }");
            Console.WriteLine();
            Console.WriteLine("      ★ 区别在哪？Dictionary 是【普通托管对象】：");
            Console.WriteLine("        它随 GC 一起被回收，键值都不再被引用后内存就还回去了。");
            Console.WriteLine("        而且你还能加【上限】：超过 10000 条就清空重来，");
            Console.WriteLine("        这样对手发再多也撑不爆你。");
            Console.WriteLine();

            Console.WriteLine("  ★ 那什么时候【才】该用 string.Intern()？");
            Console.WriteLine("    只有一种情形：字符串来自【有限且已知】的集合，而且重复率极高。");
            Console.WriteLine("    比如：从配置里读出来的几十个字段名、枚举名、状态码。");
            Console.WriteLine();
            Console.WriteLine("    ★ 而且连这个场景都基本不需要手动 Intern 了：");
            Console.WriteLine("      编译器会自动驻留字面量；");
            Console.WriteLine("      Dictionary 的默认比较器本来就会比较内容（不看对象身份）；");
            Console.WriteLine("      真要省内存，用的是 ArrayPool / Span，而不是驻留池。");
            Console.WriteLine("    → 【结论：日常代码里你几乎永远不该调用 string.Intern】");
            Console.WriteLine();

            Console.WriteLine($"  （本节验证：{interned}{notInPool}{found} —— 防止被优化掉）");
            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════════
        //  四、UTF-16：为什么 "𝄞".Length 是 2
        //
        //  ★ 注意上面那对引号里的字符：它就是高音谱号（U+1D11E），
        //    本文件的编码是 UTF-8，它在源码里占 4 个字节。
        //    但它在【内存里】要占 2 个 UTF-16 码元（4 字节）——
        //    因为 U+1D11E > 0xFFFF，超出了 BMP，只能拆成一对代理对：
        //        0xD834（高代理） + 0xDD1E（低代理）
        //    → 所以 Length 是 2，不是 1。
        //
        //  ★ 等价的转义写法是 "\uD834\uDD1E"，两种写法编译后完全一样。
        //    下面的 samples 数组用的是【转义写法】，
        //    clef 变量用的是【字符本身】—— 两种都演示一遍。
        // ════════════════════════════════════════════════════════════════════
        static void Section4Utf16()
        {
            Console.WriteLine("---------- 四、UTF-16：char 是 2 字节，不是\"一个字符\" ----------");
            Console.WriteLine();

            Console.WriteLine("  C# 的 string 内部就是一片连续的 UTF-16 码元（code unit）：");
            Console.WriteLine();
            Console.WriteLine("      char = 1 个 UTF-16 码元 = 【2 字节】（不是 1 字节！）");
            Console.WriteLine("      string = char[] + 长度 + 对象头");
            Console.WriteLine();
            Console.WriteLine("  ★ 但\"码元\"不等于\"字符\"。UTF-16 是【变长】的：");
            Console.WriteLine("    · 基本多文种平面（BMP）内的字符 → 1 个码元");
            Console.WriteLine("    · 之外的字符（emoji、音乐符号、生僻汉字）→ 2 个码元");
            Console.WriteLine("      这 2 个码元合起来叫一个【代理对（surrogate pair）】");
            Console.WriteLine();
            Rule();
            Console.WriteLine("  实测：");
            Console.WriteLine();

            string[] samples = { "a", "中", "\uD834\uDD1E", "\U0001F468\u200D\U0001F469\u200D\U0001F467" };
            string[] labels = { "ASCII 字母", "汉字（BMP 内）", "高音谱号 U+1D11E", "一家三口 emoji" };

            Console.WriteLine($"  {"样本",-6}{"说明",-22}{"Length",-9}{"码点",-8}{"UTF-16字节",-12}{"UTF-8字节"}");
            Console.WriteLine("  " + new string('─', 70));

            for (int i = 0; i < samples.Length; i++)
            {
                string sample = samples[i];

                // ★ Length 数的是【码元】个数
                int codeUnits = sample.Length;

                // ★ 码点数：EnumerateRunes() 会把代理对正确合并成一个 Rune
                int codePoints = sample.EnumerateRunes().Count();

                int utf16Bytes = System.Text.Encoding.Unicode.GetByteCount(sample);
                int utf8Bytes = System.Text.Encoding.UTF8.GetByteCount(sample);

                Console.WriteLine($"  \"{sample}\"  {labels[i],-20}{codeUnits,-9}{codePoints,-8}{utf16Bytes,-12}{utf8Bytes}");
            }

            Console.WriteLine();
            Console.WriteLine("  ★ 看这张表的三列：");
            Console.WriteLine("      \"a\"    → Length 1，码点 1   （一致）");
            Console.WriteLine("      \"中\"   → Length 1，码点 1   （一致，汉字在 BMP 内）");
            Console.WriteLine("      \"𝄞\"   → Length 2，码点 1   ★ 分道扬镳！");
            Console.WriteLine("      emoji  → Length 8，码点 5   ★ 更夸张（3 个代理对 + 2 个零宽连接符）");
            Console.WriteLine();
            Rule();
            Console.WriteLine("  ★★ 本课最容易让 Python 程序员翻车的一行：");
            Console.WriteLine();
            Console.WriteLine("        Console.WriteLine(\"𝄞\".Length);      // 输出 2");
            Console.WriteLine("        print(len(\"𝄞\"))                      # 输出 1");
            Console.WriteLine();
            Console.WriteLine("     同一个字符，C# 说 2，Python 说 1。为什么？");
            Console.WriteLine();
            Console.WriteLine("        C#       的 Length = 【UTF-16 码元】的个数");
            Console.WriteLine("        Python 3 的 len()  = 【Unicode 码点】的个数");
            Console.WriteLine();
            Console.WriteLine("     一个字（码点）可能占 2 个码元 → 2 个 char → Length = 2。");
            Console.WriteLine("     这不是 bug，是两种语言【数东西的方式不同】。");
            Console.WriteLine();

            // 用真正的高音谱号实测一遍，让读者看到数字
            // ★ 这里的 "𝄞" 是【源码里的真字符本身】（UTF-8 文件里 4 字节），
            //   等价于上面的 "\uD834\uDD1E"。两种写法编译出来一模一样。
            string clef = "𝄞";
            Console.WriteLine("  实测：string clef = \"𝄞\";     // 这就是高音谱号本身");
            Console.WriteLine($"        等价转义写法：\"\\uD834\\uDD1E\"（结果相同：{clef == "\uD834\uDD1E"}）");
            Console.WriteLine($"        clef.Length                   = {clef.Length}          ← 2 个 char（2 个码元）");
            Console.WriteLine($"        clef.EnumerateRunes().Count() = {clef.EnumerateRunes().Count()}          ← 1 个码点（正确的\"字数\"）");
            Console.WriteLine($"        (int)clef[0]                  = 0x{(int)clef[0]:X4}     ← 高代理（high surrogate）");
            Console.WriteLine($"        (int)clef[1]                  = 0x{(int)clef[1]:X4}     ← 低代理（low surrogate）");
            Console.WriteLine($"        IsHighSurrogate(clef[0])      = {IsHighSurrogate(clef[0])}");
            Console.WriteLine($"        IsLowSurrogate(clef[1])       = {IsLowSurrogate(clef[1])}");
            Console.WriteLine();
            Console.WriteLine("  ★ 如果你在自己机器上跑出来 (int)clef[0] 不是 0xD834，");
            Console.WriteLine("    那说明源文件的编码被编辑器改坏了（应该存成 UTF-8）。");
            Console.WriteLine("    这也是为什么关键字符建议写成 \\uXXXX 转义 —— 转义不受编码影响。");
            Console.WriteLine();
            Console.WriteLine("  ★ 看 clef[0] 和 clef[1]：它们各自【不是任何字符】，");
            Console.WriteLine("    只有拼在一起才是那个高音谱号。所以下面这些\"看起来很无辜\"的代码全是错的：");
            Console.WriteLine();
            Console.WriteLine("        clef[0]                    // ❌ 拿到半个代理对");
            Console.WriteLine("        clef.Substring(0, 1)       // ❌ 同上");
            Console.WriteLine("        clef.Substring(1)          // ❌ 拿到后半个");
            Console.WriteLine("        clef.Remove(0, 1)          // ❌ 拆散了代理对");
            Console.WriteLine("        clef.Length == 1           // ❌ 这个判断永远不成立");
            Console.WriteLine("        for (int i = 0; i < clef.Length; i++) Console.Write(clef[i]);   // ⚠️ 逐半输出");
            Console.WriteLine();
            Console.WriteLine("  ✅ 正确的写法（.NET Core 3.0+ 的 Rune）：");
            Console.WriteLine();
            Console.WriteLine("        foreach (Rune r in clef.EnumerateRunes())");
            Console.WriteLine("        {");
            Console.WriteLine("            Console.WriteLine(r);           // 一次拿到一个【完整字符】");
            Console.WriteLine("        }");
            Console.WriteLine("        int 正确字数 = clef.EnumerateRunes().Count();");
            Console.WriteLine();
            Console.WriteLine("      ★ Rune 类型在 System.Text 命名空间里。");
            Console.WriteLine("        它表示\"一个 Unicode 码点\"，能正确装下 BMP 外的字符。");
            Console.WriteLine();

            Rule();
            Console.WriteLine("  ★ \"中文\".Length 为什么是 2？（这个反倒和 Python 一样）");
            Console.WriteLine();
            Console.WriteLine("     汉字\"中\" = U+4E2D，落在基本多文种平面（BMP）内，");
            Console.WriteLine("     一个码元就装得下 → 1 个 char → 一个汉字算 1。");
            Console.WriteLine();
            Console.WriteLine("     C#:     \"中文\".Length        → 2");
            Console.WriteLine("     Python: len(\"中文\")           → 2");
            Console.WriteLine("     ★ 一样！所以\"C# 的中文长度不对\"是错觉 ——");
            Console.WriteLine("       出问题的只有【BMP 之外】的字符（emoji、高音谱号这类）。");
            Console.WriteLine();

            // 实测中文
            string chinese = "中文";
            Console.WriteLine($"  实测：\"中文\".Length = {chinese.Length}   UTF-16 字节 = {System.Text.Encoding.Unicode.GetByteCount(chinese)}（每个 char 2 字节）");
            Console.WriteLine();

            Rule();
            Console.WriteLine("  ★ Python 对照：为什么 Python 3 不这样？");
            Console.WriteLine();
            Console.WriteLine("     Python 3 的 str 是【码点序列】，内部有 3 种表示（PEP 393）：");
            Console.WriteLine("       · 全是 Latin-1      → 每字符 1 字节");
            Console.WriteLine("       · 含 BMP 内字符      → 每字符 2 字节");
            Console.WriteLine("       · 含 BMP 外字符      → 每字符 4 字节（UTF-32）");
            Console.WriteLine("     所以 len() 永远等于码点数 —— 简单、直觉，代价是内存。");
            Console.WriteLine();
            Console.WriteLine("     C# / Java / JavaScript / Windows API 选了 UTF-16：");
            Console.WriteLine("       · 定长 2 字节 → 索引 s[i] 是 O(1) 的纯地址计算（快）");
            Console.WriteLine("       · 英文文本省内存（比 UTF-32 省一半）");
            Console.WriteLine("       · 代价：s.Length 不是\"字符数\"，索引会切到代理对中间");
            Console.WriteLine();
            Console.WriteLine("     ★ JavaScript 一模一样：\"𝄞\".length 也是 2。");
            Console.WriteLine("       这不是 C# 的怪癖，是整个 UTF-16 家族的通病。");
            Console.WriteLine();

            Console.WriteLine("  📌 什么时候必须小心？");
            Console.WriteLine("     · 用户昵称/聊天内容里有 emoji");
            Console.WriteLine("     · 输入框的\"最多 10 个字\"限制");
            Console.WriteLine("     · 按长度截断文本做摘要");
            Console.WriteLine("     · 反转字符串、取首字母、做头像文字");
            Console.WriteLine();
            Console.WriteLine("     ★ 正确计数：text.EnumerateRunes().Count()");
            Console.WriteLine("     ★ 正确按\"人眼看到的字\"计数（连家庭 emoji 都算 1 个）：");
            Console.WriteLine("        new System.Globalization.StringInfo(text).LengthInTextElements");
            Console.WriteLine();

            var si = new System.Globalization.StringInfo(samples[3]);
            Console.WriteLine($"  实测那串家庭 emoji：Length={samples[3].Length}，码点={samples[3].EnumerateRunes().Count()}，"
                            + $"人眼看到的字={si.LengthInTextElements}");
            Console.WriteLine("     ★ 三个数字都不一样 —— 这就是\"长度\"这个词在文本处理里的歧义。");
            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════════
        //  五、拼接灾难：+= 是 O(n²)
        // ════════════════════════════════════════════════════════════════════
        static void Section5ConcatDisaster()
        {
            Console.WriteLine("---------- 五、拼接灾难：循环里 += 是 O(n²) ----------");
            Console.WriteLine();

            Console.WriteLine("  先看 n 次 += 到底发生了什么：");
            Console.WriteLine();
            Console.WriteLine("      string s = \"\";");
            Console.WriteLine("      for (int i = 0; i < n; i++) s += i;");
            Console.WriteLine();
            Console.WriteLine("  第 i 次循环内部其实是：");
            Console.WriteLine("      ① 分配一块能装下【旧内容 + 新内容】的新内存");
            Console.WriteLine("      ② 把旧内容【全部复制】过去          ← 这一步是灾难的根源");
            Console.WriteLine("      ③ 把新内容追加进去");
            Console.WriteLine("      ④ 旧字符串变成垃圾，等 GC 来收");
            Console.WriteLine();
            Console.WriteLine("  → 复制的字符总数 = 1 + 2 + 3 + ... + n ≈ n²/2");
            Console.WriteLine("  → 复杂度 O(n²)。n 翻倍，总工作量【翻 4 倍】。");
            Console.WriteLine();

            // ★ 先算好"正确答案"，后面每种写法都拿它当锚点
            const int Times = 20_000;                        // 大纲里用的次数
            string[] parts = new string[Times];
            for (int i = 0; i < Times; i++)
            {
                parts[i] = i.ToString();
            }

            int expectedLength = 0;
            foreach (string p in parts)
            {
                expectedLength += p.Length;
            }
            Console.WriteLine($"  本次基准：拼接 {Times:N0} 次，最终字符串应该有 {expectedLength:N0} 个字符。");
            Console.WriteLine();

            Rule();
            Console.WriteLine($"  {"方式",-34}{"耗时",9}   {"分配",10}");
            Rule();

            string result = "";

            // ① 反面教材：循环里 +=
            Report("s += i（灾难）", Measure(() =>
            {
                string s = "";
                for (int i = 0; i < Times; i++)
                {
                    s += i;                   // ★ 每一次都在堆上造一个新字符串
                }
                result = s;
            }));

            // ② 正面教材：StringBuilder
            Report("StringBuilder（正确）", Measure(() =>
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < Times; i++)
                {
                    sb.Append(i);
                }
                result = sb.ToString();
            }));

            // ③ 更好：预先估好容量，连扩容复制都省掉
            Report("StringBuilder(预分配容量)", Measure(() =>
            {
                var sb = new System.Text.StringBuilder(expectedLength);
                for (int i = 0; i < Times; i++)
                {
                    sb.Append(i);
                }
                result = sb.ToString();
            }));

            // ④ 已知所有片段：string.Concat 一次分配到位
            Report("string.Concat(数组)", Measure(() =>
            {
                result = string.Concat(parts);
            }));

            Rule();
            Console.WriteLine();
            Console.WriteLine($"  （四种写法的结果长度必须一致，否则说明写错了：{result.Length:N0} / {expectedLength:N0}）");
            Console.WriteLine($"  长度是否一致：{result.Length == expectedLength}");
            Console.WriteLine();

            // ── 为什么 += 是 O(n²)：用数字说话 ──────────────────────────
            Console.WriteLine("  ★ 怎么证明它是 O(n²)？看\"平均每次分配了多少字节\"这一列：");
            Console.WriteLine();
            Console.WriteLine($"  {"拼接次数",-12}{"耗时",11}{"分配",13}{"分配/次",12}");
            Console.WriteLine("  " + new string('─', 52));

            foreach (int n in new[] { 2_000, 4_000, 8_000, 16_000 })
            {
                (double ms, double mb) = Measure(() =>
                {
                    string s = "";
                    for (int i = 0; i < n; i++)
                    {
                        s += "x";
                    }
                    GC.KeepAlive(s);
                });

                // 分配/次 = 总分配字节数 ÷ 次数（★ 一定要除以浮点数，否则整数除法）
                double bytesPerIter = mb * 1024 * 1024 / n;
                Console.WriteLine($"  {n,-12:N0}{ms,9:F1} ms{mb,11:F2} MB{bytesPerIter,10:F0} B");
            }

            Console.WriteLine();
            Console.WriteLine("  ★ 看最后那一列：次数翻倍，\"平均每次分配\"也在翻倍。");
            Console.WriteLine("    这就是 O(n²) 的铁证 —— 如果是 O(n)，这一列应该【保持常数】。");
            Console.WriteLine();

            // ── 理论值对照 ────────────────────────────────────────────────
            Console.WriteLine("  ★ 顺便算一下理论值，你可以拿它对实测数字：");
            Console.WriteLine();
            Console.WriteLine("      n=20000 时，复制的字符数 ≈ 20000² / 2 = 2 亿个字符");
            Console.WriteLine("      每个 char 2 字节（UTF-16）→ 约 4 亿字节 ≈ 400 MB 的【复制量】");
            Console.WriteLine("      而每个中间结果都要【存活到下一次】才变成垃圾，");
            Console.WriteLine("      所以累计【分配量】还要把这些中间结果全加起来：");
            Console.WriteLine("      Σ(第 i 次新建的字符数 × 2 字节) ≈ 数亿字节 ≈ 数百 MB");
            Console.WriteLine();
            Console.WriteLine("     实测通常更大（1000 MB 以上），因为：");
            Console.WriteLine("       · 数字 0~19999 平均 4.x 个字符，比\"1 个字符\"的估算多得多");
            Console.WriteLine("       · 字符串对象头 + 内存对齐本身也占字节");
            Console.WriteLine();

            // ── 大纲里的基准数字 ─────────────────────────────────────────
            Rule();
            Console.WriteLine("  📌 大纲记录的第 13 课实测基准（你的机器上数字会不同，看数量级）：");
            Console.WriteLine();
            Console.WriteLine("      拼接 20,000 次：");
            Console.WriteLine("        s += i        :  77.5 ms,  分配 1581.07 MB");
            Console.WriteLine("        StringBuilder :   0.8 ms,  分配     0.35 MB");
            Console.WriteLine("        → 慢了约 91 倍，多分配约 4454 倍内存");
            Console.WriteLine();
            Console.WriteLine("  ★ 拿上面那段实测和这段基准对比。差值大不是因为\"大纲的数字不准\"，");
            Console.WriteLine("    而是因为：");
            Console.WriteLine("      · += 的耗时刻受 GC 何时插入的影响（波动极大，几十到几百 ms 都正常）");
            Console.WriteLine("      · 分配量则非常稳定（它只跟 n 有关）→ 【分配量才是可靠指标】");
            Console.WriteLine();
            Console.WriteLine("    ★ 这也是本课的方法论：");
            Console.WriteLine("      测时间容易被 JIT / GC / 后台任务干扰；");
            Console.WriteLine("      测【分配字节数】几乎不受干扰，是判断\"这段代码贵不贵\"的首选。");
            Rule();
            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════════
        //  六、现代工具箱：什么场合用什么
        // ════════════════════════════════════════════════════════════════════
        static void Section6ModernTools()
        {
            Console.WriteLine("---------- 六、现代工具箱：别只知道 StringBuilder ----------");
            Console.WriteLine();

            Console.WriteLine($"  {"场景",-26}{"首选写法",-30}为什么");
            Console.WriteLine("  " + new string('─', 92));
            Console.WriteLine($"  {"少量拼接（2~5 段）",-26}{"a + b 或 $\"...\"",-30}可读性优先，开销可忽略");
            Console.WriteLine($"  {"循环里累加",-26}{"StringBuilder",-30}内部可变缓冲区，O(n)");
            Console.WriteLine($"  {"已知最终长度",-26}{"StringBuilder(capacity)",-30}连扩容复制都省掉");
            Console.WriteLine($"  {"已知所有片段",-26}{"string.Concat / Join",-30}一次分配到位，零中间对象");
            Console.WriteLine($"  {"已知长度 + 只填一次",-26}{"string.Create",-30}直接在最终对象里写，零额外分配");
            Console.WriteLine($"  {"要做搜索/切片",-26}{"ReadOnlySpan<char>",-30}不分配新字符串（延伸阅读）");
            Console.WriteLine();

            // ── string.Create：唯一能在"最终对象"里直接写字的方式 ──────
            Console.WriteLine("  string.Create 是所有写法里最省的 —— 它先分配好【最终长度】的字符串，");
            Console.WriteLine("  然后把一片可写的 Span<char> 交给你，让你直接在里面写：");
            Console.WriteLine();

            // ★ 这里的 state 参数演示了 string.Create 的第二个用途：
            //   它是"传给 lambda 的状态值"，避免 lambda 捕获外部变量（一捕获就生成闭包类，第 23 课）。
            //   本示例的 lambda 里没有真正需要 state，所以我们把它当"起始字符"用上，
            //   既吃掉了这个参数（编译器不报"未使用"），又演示了它的语义。
            char startChar = 'a';
            string createSample = string.Create(10, startChar, (span, state) =>
            {
                for (int i = 0; i < span.Length; i++)
                {
                    // (char)(state + i)：state 是 'a'，所以结果是 a b c d e f g h i j
                    // ⚠️ 早先的版本这里写的是 'x' + i —— 到 i=3 就越过了 'z'，
                    //    变成 { | } ~ 这些符号，i 再大就跑进 0x80 以上的【控制字符】，
                    //    终端会显示成乱码或问号。改成 'a' 起步，10 个字母刚好落满 a~j，输出可读。
                    span[i] = (char)(state + i);
                }
            });
            Console.WriteLine($"      string.Create(10, 'a', (span, state) => ...)  = \"{createSample}\"");
            Console.WriteLine();
            Console.WriteLine("      ★ span 就是最终字符串的内部存储，改它就是改结果本身，");
            Console.WriteLine("        中间【一次临时字符串都没有】。");
            Console.WriteLine("      ★ 注意上面的 lambda 没有捕获外部局部变量（只用了参数 span/state）——");
            Console.WriteLine("        一旦捕获就会生成闭包类（第 23 课），反而多一次分配。");
            Console.WriteLine($"      ★ 本示例的 createSample 长度 = {createSample.Length}");
            Console.WriteLine();

            // ── 插值字符串的现代优化 ─────────────────────────────────────
            Console.WriteLine("  插值字符串 $\"...\" 在 .NET 6+ 已经被编译器优化了：");
            Console.WriteLine();
            Console.WriteLine("      int id = 42; double score = 3.14159;");
            Console.WriteLine("      string s1 = $\"{id},{score}\";                      // ✅ 走 Handler");
            Console.WriteLine("      string s2 = string.Format(\"{0},{1}\", id, score);   // ❌ 慢，还可能装箱");
            Console.WriteLine();

            int demoId = 42;
            double demoScore = 3.14159;

            (double msInterp, double mbInterp) = Measure(() =>
            {
                for (int i = 0; i < 100_000; i++)
                {
                    GC.KeepAlive($"{demoId},{demoScore}");
                }
            }, warmup: 100);

            (double msFormat, double mbFormat) = Measure(() =>
            {
                for (int i = 0; i < 100_000; i++)
                {
                    GC.KeepAlive(string.Format("{0},{1}", demoId, demoScore));
                }
            }, warmup: 100);

            Console.WriteLine($"  {"方式（各 10 万次）",-34}{"耗时",9}   {"分配",10}");
            Console.WriteLine("  " + new string('─', 58));
            Report("$\"{id},{score}\"（插值）", (msInterp, mbInterp));
            Report("string.Format(...)", (msFormat, mbFormat));
            Console.WriteLine();
            Console.WriteLine("  ★ 两者的分配量差别最能说明问题：插值直接在缓冲区里写，");
            Console.WriteLine("    string.Format 要组装 object[]、走格式化流程。");
            Console.WriteLine("    → 记住结论：用 $\"...\"，淘汰 string.Format。");
            Console.WriteLine();

            // ── StringBuilder 的正确用法细节 ─────────────────────────────
            Console.WriteLine("  StringBuilder 的三个细节（很多人写错）：");
            Console.WriteLine();
            Console.WriteLine("   ① Append(int) 有专门重载，【不装箱】；Append(object) 才装箱。");
            Console.WriteLine("      编译器会自动选最匹配的那个重载，你只要别把值先转成 object。");
            Console.WriteLine();
            Console.WriteLine("   ② 能估出长度就传 capacity：（第 13 课的实测里，这能再省一大截）");
            Console.WriteLine("        var sb = new StringBuilder(expectedLength);");
            Console.WriteLine();
            Console.WriteLine("   ③ 最后只调一次 ToString()。不要写在循环里 ——");
            Console.WriteLine("      那等于每轮都把缓冲区【整个复制】成一个新字符串，白费 StringBuilder。");
            Console.WriteLine();

            Console.WriteLine($"  （本节验证：{createSample} / {demoId} / {demoScore}）");
            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════════
        //  七、自己动手改坏它
        // ════════════════════════════════════════════════════════════════════
        static void Section7BreakIt()
        {
            Console.WriteLine("---------- 七、自己动手改坏它（学习效果最好的一步） ----------");
            Console.WriteLine();

            Console.WriteLine("  下面每一条都是\"改一行 → 看输出有什么变化 → 想清楚为什么\"。");
            Console.WriteLine("  建议在 Lesson22Practice 里做，改坏了不影响示例。");
            Console.WriteLine();

            Console.WriteLine("  ① 把拼接次数从 20,000 改成 40,000");
            Console.WriteLine("     → 期望：耗时约 ×4，【分配量约 ×4】（不是 ×2）。");
            Console.WriteLine("       亲眼看到 ×4，你就亲手验证了 O(n²)。");
            Console.WriteLine();

            Console.WriteLine("  ② 把 string.Concat(parts) 换成手写的 for + +=");
            Console.WriteLine("     → 期望：结果字符串一模一样，但分配量暴涨。");
            Console.WriteLine("       → 结论：\"结果对\"和\"过程贵\"是两件互不相干的事。");
            Console.WriteLine();

            Console.WriteLine("  ③ 把 string.Intern(built) 的返回值【扔掉不接】：");
            Console.WriteLine("        string.Intern(built);       // 没有赋值！");
            Console.WriteLine("        ReferenceEquals(literal, built)   // 还是 False");
            Console.WriteLine("     → 期望：什么也没变。Intern 只是\"保证池里有\"，");
            Console.WriteLine("       不会改写你手上那个变量。");
            Console.WriteLine();

            Console.WriteLine("  ④ 写一个循环，Intern 一万个互不相同的字符串：");
            Console.WriteLine("        for (int i = 0; i < 10_000; i++)");
            Console.WriteLine("            string.Intern(\"unique_\" + i);");
            Console.WriteLine("     → 用 GC.GetTotalMemory(true) 前后对比：");
            Console.WriteLine("       ★ 强制 GC 之后，这些字符串【一个都不会少】。");
            Console.WriteLine("       这就是\"池里永不释放\"的直接证据。");
            Console.WriteLine("       ⚠️ 千万别写成 1_000_000，那真的会把内存吃掉。");
            Console.WriteLine();

            Console.WriteLine("  ⑤ 把高音谱号换成 \"中文\" 和 \"abc\"，各跑一次 Length");
            Console.WriteLine("     → 期望：1 / 2 / 3（码元）；");
            Console.WriteLine("       再用 EnumerateRunes().Count() → 1 / 2 / 3（码点）。");
            Console.WriteLine("       ★ 挑一个 emoji 试试，两个数字马上分道扬镳。");
            Console.WriteLine();

            Console.WriteLine("  ⑥ 把 s[0] = 'J'; 写进去，然后【别改回来，直接编译】");
            Console.WriteLine("     → 期望：CS0200 编译错误。");
            Console.WriteLine("       ★ 重点看错误列表的措辞 —— 它说的是\"只读\"，");
            Console.WriteLine("         而且【程序根本没运行】。这就是编译期/运行时的分界。");
            Console.WriteLine("       （所以本示例文件里这一行只能出现在注释里。）");
            Console.WriteLine();

            Console.WriteLine("  ⑦ 把 for 循环里的 sb.Append(i) 改成 sb.Append(i.ToString() + \" \")");
            Console.WriteLine("     → 期望：StringBuilder 的优势大幅缩水 ——");
            Console.WriteLine("       因为你每轮又造了一个临时字符串，把省下的又花了出去。");
            Console.WriteLine("       → 结论：StringBuilder 只解决\"累加\"，不解决\"每轮的临时对象\"。");
            Console.WriteLine();

            Console.WriteLine("  ⑧ （进阶）用 ILSpy 打开 Lesson22Demo.dll，看：");
            Console.WriteLine("     · 一个 for + += 的方法里有多少个 newobj / call string::Concat");
            Console.WriteLine("     · $\"...\" 变成了 new DefaultInterpolatedStringHandler");
            Console.WriteLine("     · Section2 里的 \"abc\" 用的是 ldstr 指令");
            Console.WriteLine("       ★ ldstr 就是\"从驻留池取/放入\" —— 驻留的机器码级别证据。");
            Console.WriteLine();
        }
    }
}
