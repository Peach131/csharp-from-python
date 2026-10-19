// ============================================================================
//  第 22 课 · 练习：字符串的底层真相
//
//  ★ 前置：第 13 课（字符串怎么用）。本课是【加深 + 实验验证】，
//    不重复第 13 课讲过的入门内容，重点在底层机制。
//
//  ★ 本课重点：
//     1. 不可变性的技术根基：string 的索引器【只有 get，没有 set】
//        → s[0] = 'x' 是【编译错误】（CS0200），不是运行时异常
//     2. 驻留（Interning）：字面量共享同一个对象；Intern 池永不释放
//     3. UTF-16：char 占 2 字节；"𝄞".Length 是 2（Python 是 1）
//     4. 拼接灾难：循环 += 是 O(n²)，用 GC.GetAllocatedBytesForCurrentThread 实测
//
//  ★ 运行方式：打开 第22课.sln，把 Lesson22Practice 设为启动项目，Ctrl + F5
//
//  ⚠️ 本文件【当前可以正常编译运行】，会打印一堆 --- TODO n --- 和最后的提示。
//     你每完成一个 TODO，就在对应位置把代码补上。
//
//  ⚠️ 有一个地方【千万不要】照着直觉写：
//        s[0] = 'x';        // ❌ 编译错误 CS0200（本文件里也绝对不能写）
//     本课练习的 TODO 3 会让你在【注释】里描述这个错误，而不是执行它。
// ============================================================================

namespace LessonPractice
{
    class Lesson22Practice
    {
        // ================================================================
        // ↓↓↓ 本课的辅助方法写在这里 ↓↓↓
        // ================================================================
        //
        // ★ TODO 4 和 TODO 5 都要"测量一段代码分配了多少内存"，
        //   所以这里先给你一个可以直接用的测量函数。
        //
        // ★ 为什么把测量逻辑单独抽出来？
        //   因为"怎么测"和"测什么"应该分开：
        //     · 测量函数只关心 Stopwatch + GC 计数（这段逻辑永远不变）
        //     · 被测代码通过参数 action 传进来（每次都不同）
        //   这就是第 14 课讲的委托最典型的用法 —— 把"行为"当参数传。
        //
        // ★ 用法：
        //     var r = MeasureAlloc(() => { /* 你要测的代码 */ });
        //     Console.WriteLine($"{r.Ms:F1} ms，分配 {r.Mb:F2} MB");
        //
        // 【要求】先把下面这个方法【逐行读懂】，再开始做 TODO。
        //         读不懂就对照注释，尤其是 before / after 那一对。
        // ================================================================

        static (double Ms, double Mb) MeasureAlloc(Action action, int warmup = 1)
        {
            // ① 预热：先跑几遍。
            //   ★ 因为第一次执行的方法要被 JIT 编译成机器码（第 17 课），
            //     那部分开销不该算进被测代码里，否则你测到的主要是 JIT 的时间。
            for (int i = 0; i < warmup; i++)
            {
                action();
            }

            // ② 先把上一段代码留下的垃圾清掉，从"干净起点"开始计数。
            //   ⚠️ 生产代码永远不要调 GC.Collect()，只有测试和诊断可以。
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // ③ 记下"已经分配了多少字节"的累计值 —— 这是计时的起点
            long before = GC.GetAllocatedBytesForCurrentThread();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            action();

            // ④ 停表 + 再读一次累计值，两个数相减 = 这段代码分配了多少
            sw.Stop();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            // ⑤ 字节 → MB：★ 必须除以 1024.0（带小数点的 double），
            //    写成 1024 会变成整数除法，结果直接是 0
            return (sw.Elapsed.TotalMilliseconds, allocated / 1024.0 / 1024.0);
        }

        // ================================================================
        // ↓↓↓ 主流程 ↓↓↓
        // ================================================================

        static void Main()
        {
            Console.WriteLine("===== 第 22 课练习：字符串的底层真相 =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 验证驻留：字面量为什么是同一个对象
            // ================================================================
            // 【要求】
            //   1. 定义 string a = "abc"; string b = "abc";
            //      分别打印 a == b 和 ReferenceEquals(a, b)，并对照说明
            //      "== 比内容，ReferenceEquals 比身份"
            //   2. 定义 string c = "ab" + "c"; 打印 ReferenceEquals(a, c)
            //      ★ 并在注释里说清楚：为什么 c 还是同一个对象？
            //   3. 定义 string left = "ab"; string d = left + "c";
            //      打印 d == a 和 ReferenceEquals(a, d)
            //      ★ 并在注释里说清：a 和 d 内容一样，为什么身份不一样？
            //   4. string e = string.Concat("abc"); 再验一次
            //   5. 用 string.IsInterned(d) 查一下 d 在不在池里（不在会返回 null）
            //      ★ 注意 IsInterned 的返回类型是 string?，用之前要判 null
            //
            // 【预期输出】（顺序可不同，True/False 必须对）
            //     a == b                : True
            //     ReferenceEquals(a, b) : True    ← 字面量共享同一个对象
            //     ReferenceEquals(a, c) : True    ← 编译期常量折叠，还是字面量
            //     d == a                : True    ← 内容相同
            //     ReferenceEquals(a, d) : False   ← 运行时拼的，不进池
            //     ReferenceEquals(a, e) : False
            //     IsInterned(d)         : null（不在池里）
            //
            // 【提示】
            //     string a = "abc";
            //     Console.WriteLine($"  a == b                : {a == b}");
            //     Console.WriteLine($"  ReferenceEquals(a, b) : {ReferenceEquals(a, b)}");
            //
            //     // ★ 判 null 的写法（string? 是可空引用类型，Nullable enable 下必须处理）
            //     string? inPool = string.IsInterned(d);
            //     Console.WriteLine($"  IsInterned(d) : {(inPool is null ? "null（不在池里）" : inPool)}");
            //
            //     // ★ 想让 d 也进池，需要【接住返回值】：
            //     //   d = string.Intern(d);
            //     //   少了这个赋值，string.Intern(d) 就等于没写
            //
            // 【考察点】
            //   · == 和 ReferenceEquals 的区别（第 13 课埋的坑，这里挖到底）
            //   · 编译期常量折叠：字面量 + 字面量 = 还是字面量
            //   · 运行时构造的字符串【不会】自动进池

            Console.WriteLine("--- TODO 1 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 2 ｜ 绕过驻留：三种运行时构造字符串的写法
            // ================================================================
            // 【要求】
            //   目标是造出三个"内容都是 abc"，但【都不是池里那一份】的字符串。
            //   用下面三种写法各造一个，每一个都打印 ReferenceEquals(a, 结果)：
            //     ① 拼接：        string left = "ab"; string s1 = left + "c";
            //     ② string.Concat：string s2 = string.Concat("abc");
            //     ③ new string：  string s3 = new string(new[] { 'a', 'b', 'c' });
            //   然后：
            //     ④ 对 s3 调用 string.Intern，把【返回值】接住，再打印一次
            //        ReferenceEquals(a, 返回值) —— 看它变成 True
            //     ⑤ 同时打印 ReferenceEquals(s3, 返回值)，证明 s3 自己没变
            //   最后写一句注释总结：什么情况下字符串会自动进池，什么情况下不会。
            //
            // 【预期输出】
            //     s1 = left + "c"                        : False
            //     s2 = string.Concat("abc")              : False
            //     s3 = new string(new[]{'a','b','c'})    : False
            //     interned = string.Intern(s3)           : True    ← 变成池里那一份了
            //     ReferenceEquals(s3, interned)          : False   ← s3 自己没变
            //     三者内容都等于 a：True / True / True
            //
            // 【提示】
            //     // ★ string.Intern 返回的是【池里那一份】，不是改写你传进去的变量
            //     string interned = string.Intern(s3);
            //
            //     // ★ 想验证"内容都一样"，用 == 而不是 ReferenceEquals
            //     Console.WriteLine($"  三者内容都等于 a：{s1 == a} / {s2 == a} / {s3 == a}");
            //
            //     // ★ new string(char[]) 是最"干净"的运行时构造方式：
            //     //   没有任何字面量参与，编译器完全不知道内容是什么
            //
            // 【考察点】
            //   · 驻留只发生在【字面量】和【显式 Intern】两种情况下
            //   · Intern 的返回值必须接住，否则等于没调用
            //   · 内容相等（==）和对象相同（ReferenceEquals）是两件事

            Console.WriteLine("--- TODO 2 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 3 ｜ ★ 本课最容易翻车的地方：Length 不等于"字数"
            // ================================================================
            // 【要求】
            //   对下面这几个样本逐个统计，做成一张表（每个样本一行）：
            //       "a"、"中"、"中文"、"abc"、"𝄞"、"👨‍👩‍👧"
            //   每行打印四个数字：
            //     ① s.Length                      → 【UTF-16 码元】个数
            //     ② s.EnumerateRunes().Count()    → 【Unicode 码点】个数
            //     ③ Encoding.Unicode.GetByteCount(s)  → 内存里占多少字节
            //     ④ Encoding.UTF8.GetByteCount(s)     → 存成 UTF-8 文件占多少字节
            //
            //   ★ 高音谱号 "𝄞" 是 U+1D11E，在 BMP 之外，必须占 2 个 char。
            //     它有两种等价写法，随便用哪种（编译结果完全一样）：
            //         ① 转义：  string clef = "\uD834\uDD1E";     // 高代理 + 低代理
            //         ② 真字符：string clef = "𝄞";                // 直接写，靠文件编码（UTF-8）
            //     ⚠️ 千万【不要】写成 "\UD834DD1E" —— \U 后面要跟【8 位】十六进制，
            //        而且不能这样把两个码元拼在一个 \U 里。
            //     ⚠️ 用真字符时，如果跑出来 (int)clef[0] 不是 0xD834，
            //        说明你的编辑器把文件编码存坏了（要存 UTF-8）。
            //
            //   然后【额外】做两件事：
            //     ⑤ 对 "𝄞" 打印 s[0] 和 s[1] 的数值：
            //          Console.WriteLine($"  s[0]=0x{(int)clef[0]:X4}  s[1]=0x{(int)clef[1]:X4}");
            //        → 你会看到 0xD834 和 0xDD1E，它们各自【不是任何字符】
            //     ⑥ 用注释回答（不写代码）：为什么 "中文".Length 是 2 而 "𝄞".Length 是 2，
            //        但 Python 里 len("中文") 是 2、len("𝄞") 却是 1？
            //
            // 【预期输出】
            //     样本     Length   码点   UTF-16字节   UTF-8字节
            //     "a"           1      1            2           1
            //     "中"          1      1            2           3
            //     "中文"        2      2            4           6
            //     "abc"         3      3            6           3
            //     "𝄞"          2      1            4           4     ← ★ Length 是 2！
            //     "👨‍👩‍👧"       8      5           16          18
            //     ────────────
            //     "𝄞"[0] = 0xD834  "𝄞"[1] = 0xDD1E
            //
            // 【提示】
            //     样本放进数组用 foreach 遍历，别复制六遍代码：
            //         string[] samples = { "a", "中", "中文", "abc", "\uD834\uDD1E", "\U0001F468\u200D\U0001F469\u200D\U0001F467" };
            //         foreach (string s in samples) { ... }
            //
            //     ★ EnumerateRunes() 的返回类型是 IEnumerable<Rune>，
            //       想写 foreach (Rune r in ...) 才需要在文件顶部加 using System.Text;
            //       （本练习只调 .Count()，不用加 using，也不用写类型名）
            //     ★ 表头对齐用 {值,-10} 这种宽度写法（第 13 课讲过）
            //     ★ 为什么 emoji 的码点是 5 而不是 1？
            //       因为"👨‍👩‍👧"其实是 3 个 emoji 用 2 个零宽连接符（U+200D）粘起来的，
            //       人眼看是 1 个字，Unicode 层面是 5 个码点、8 个码元。
            //       → 想知道"人眼看到几个字"，用：
            //         new System.Globalization.StringInfo(s).LengthInTextElements
            //
            // 【考察点】
            //   · Length 数的是码元，不是码点，更不是"人眼看到的字数"
            //   · 代理对（surrogate pair）：BMP 之外的字符要占 2 个 char
            //   · s[0] 会拿到半个字符 —— 这是所有"按长度截断文本"bug 的根源

            Console.WriteLine("--- TODO 3 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 4 ｜ 实测拼接灾难：+= vs StringBuilder
            // ================================================================
            // 【要求】
            //   用上面给你的 MeasureAlloc 测这四种写法（都拼接 20000 次）：
            //     ① s += i              循环里 string +=
            //     ② StringBuilder       默认容量
            //     ③ StringBuilder(容量) 预先估好最终长度
            //     ④ string.Concat(数组) 先造好 string[] 再一次拼完
            //   打印成一张表：方式 / 耗时 / 分配 MB
            //   最后：
            //     ⑤ 打印 ④ 的结果长度，并和"理论长度"对比（证明结果都是对的）
            //     ⑥ 算一下 ① 比 ② 慢了多少倍、多分配了多少倍
            //     ⑦ 把每个测量结果先存进变量（r1 / r2 ...），再拼进输出字符串
            //        ★ 因为插值字符串里【不能】用三引号，也不能跨行；
            //          而且要在输出里显示一对花括号，必须写成 {{ }}
            //
            // 【预期输出】（数字随机器不同，看数量级和倍率）
            //     方式                          耗时        分配
            //     ────────────────────────────────────────────────
            //     s += i（灾难）             XXX.X ms    XXXX.XX MB
            //     StringBuilder                  X.X ms        X.XX MB
            //     StringBuilder(预分配容量)      X.X ms        X.XX MB
            //     string.Concat(数组)            X.X ms        X.XX MB
            //     ────────────────────────────────────────────────
            //     结果长度：XXXXX（四种写法必须一致）
            //     → 慢了约 XXX 倍，多分配约 XXXX 倍内存
            //
            // 【提示】
            //     const int Times = 20_000;
            //
            //     // ① 先造好"理论答案"：把 0..19999 转成字符串，长度加起来
            //     string[] parts = new string[Times];
            //     int expectedLength = 0;
            //     for (int i = 0; i < Times; i++)
            //     {
            //         parts[i] = i.ToString();
            //         expectedLength += parts[i].Length;
            //     }
            //
            //     // ② 测第一种。★ 结果要存到外面，否则编译器可能把整个循环优化掉
            //     string result = "";
            //     var r1 = MeasureAlloc(() =>
            //     {
            //         string s = "";
            //         for (int i = 0; i < Times; i++) s += i;
            //         result = s;
            //     });
            //     Console.WriteLine($"  {"s += i（灾难）",-28}{r1.Ms,9:F1} ms{r1.Mb,11:F2} MB");
            //
            //     // ③ 倍率要防零除（goodMs 可能小到 0）：
            //     double ratio = r2.Ms > 0 ? r1.Ms / r2.Ms : 1;
            //
            //     // ④ 字符串里要显示一个花括号，必须写两遍：
            //     Console.WriteLine($"  分配量倍率：{r1.Mb / r2.Mb:F0} 倍  {{按 GB 算是 ...}}");
            //
            //     // ★ 需要 using System.Text; 才能直接写 StringBuilder
            //     //   （不想加 using 就写全名 System.Text.StringBuilder）
            //
            // 【考察点】
            //   · += 是 O(n²)：每次新建 + 复制全部已有内容
            //   · 分配量比耗时更可靠（耗时受 GC 干扰，波动大）
            //   · 预分配容量的收益：连 StringBuilder 内部的扩容复制都省了

            Console.WriteLine("--- TODO 4 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 5 ｜ ★ 综合（本课考核）：字符串内存分析器
            // ================================================================
            // 【要求】写一个方法 string AnalyzeStrings(string[] samples)，
            //        它【返回】一份统计报告字符串（调用方负责打印）。
            //        ★ 为什么不直接在方法里 Console.WriteLine？
            //          把"算"和"打印"分开，这个方法才能被复用（写文件 / 做断言）。
            //          这是第 12 课讲过的原则。
            //        然后在 Main 里造一批样本、调用它、打印报告。
            //
            //   统计内容：
            //     ① 总样本数
            //     ② 驻留命中数：对每个样本调用 string.Intern(s)，
            //        如果返回值【和 s 本身是同一个对象】（ReferenceEquals 为 True），
            //        说明 s 早就在池里 → 算一次"命中"
            //     ③ 驻留命中率（百分比，保留 1 位小数）
            //     ④ 为了"验证驻留真的发生了"，统计这一轮你【新建】了多少个字符串：
            //        用 MeasureAlloc 包住整个循环，
            //        记录"分配字节数 / 每个字符串平均多少字节"
            //     ⑤ 用 Dictionary<string, int> 统计样本里的重复内容
            //        （第 09 课的字典，正好用上）
            //     ⑥ Unicode 健康检查：
            //        · 有多少个样本是"纯 BMP"（Length == 码点数）
            //        · 有多少个样本含代理对（Length != 码点数）
            //
            //   ★ 样本里必须同时包含这四类，否则统计没意义：
            //       · 字面量（会在池里）      ："abc"、"hello"
            //       · 运行时构造的（不在池里）：new string(new[]{'a','b','c'})
            //       · 重复出现的（考验字典统计）：同一个内容出现 2~3 次
            //       · 含代理对的（考验 UTF-16） ："\uD834\uDD1E"、"\U0001F600"
            //
            //   报告格式（自己排版，至少要有这些行）：
            //     ========== 字符串内存分析器 ==========
            //     样本总数        : 10
            //     驻留命中        : 4  （40.0%）
            //     未命中          : 6
            //     含代理对的样本  : 2
            //     这一轮分配      : X.XX MB（平均每个样本 XXX 字节）
            //     ──────────── 内容重复情况 ────────────
            //     "abc"      × 2
            //     "hello"    × 1
            //     ...
            //     ──────────────────────────────────────
            //     结论：<一句你自己的话，说明什么字符串会进池>
            //     ======================================
            //
            // 【提示】
            //     static string AnalyzeStrings(string[] samples)
            //     {
            //         var report = new System.Text.StringBuilder();
            //         report.AppendLine("========== 字符串内存分析器 ==========");
            //
            //         int interned = 0;
            //         int surrogate = 0;
            //         var counts = new Dictionary<string, int>();
            //
            //         // ★ 想测"这一轮分配了多少"，就得把整个循环包进 MeasureAlloc
            //         // ★★ 必须写 warmup: 0 —— 这一轮统计【有副作用】：
            //         //     第一遍跑完，所有"不在池里"的样本都被 Intern 进池了；
            //         //     再跑一遍，interned 的计数会直接翻倍（4 → 8）。
            //         var r = MeasureAlloc(() =>
            //         {
            //             foreach (string s in samples)
            //             {
            //                 // Intern 返回池里那一份；和 s 是同一个对象 → s 本来就在池里
            //                 if (ReferenceEquals(s, string.Intern(s))) interned++;
            //
            //                 // 码元数 != 码点数 → 含代理对
            //                 if (s.Length != s.EnumerateRunes().Count()) surrogate++;
            //
            //                 // 字典计数：TryGetValue 一次查找同时拿到"在不在"和"是多少"
            //                 if (counts.TryGetValue(s, out int c)) counts[s] = c + 1;
            //                 else counts[s] = 1;
            //             }
            //         }, warmup: 0);
            //
            //         // ★ 命中率要防零除
            //         double rate = samples.Length > 0 ? interned * 100.0 / samples.Length : 0;
            //         report.AppendLine($"驻留命中        : {interned}  （{rate:F1}%）");
            //
            //         // ★ 平均每个样本多少字节：/ 1024.0 别写成 / 1024
            //         report.AppendLine($"这一轮分配      : {r.Mb:F2} MB");
            //
            //         return report.ToString();
            //     }
            //
            //     // Main 里：
            //     string[] samples = { "abc", "hello", "abc", new string(new[]{'a','b','c'}), ... };
            //     Console.WriteLine(AnalyzeStrings(samples));
            //
            //   ⚠️ 为什么要"把循环包进 MeasureAlloc"？
            //     因为 string.Intern() 本身【也可能分配】（池里没有时就插入，
            //     内部要查哈希表）。所以你测到的分配量不全是"样本"的 ——
            //     这正好是让你自己想清楚的一件事：写进注释里。
            //
            // 【考察点】
            //   · 一次遍历同时做多件事（驻留判定 + 代理对判定 + 词频统计）
            //   · ReferenceEquals 的实战用法
            //   · 用实测数据支撑结论，而不是背概念

            Console.WriteLine("--- TODO 5 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓
            // ★ TODO 5 的辅助方法 AnalyzeStrings 请写在 Main 的【下面】
            //   （和上面 MeasureAlloc 并排），别写在 Main 里面。




            Console.WriteLine();

            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
