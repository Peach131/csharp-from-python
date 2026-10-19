namespace LessonPractice
{
    // ============================================================================
    //  第 18 课 · 示例：栈与堆、对象的内存布局
    //
    //  ★ 本课的目标：把「变量住在哪、对象有多大」从"背结论"变成"自己测出来"。
    //     第 11 课已经讲过主体结论，这一课把它们【全部实测一遍】。
    //
    //  ★ 运行方式：
    //      1. 打开 第18课.sln，把 Lesson18Demo 设为启动项目，Ctrl + F5
    //      2. 或者命令行：dotnet run --project Lesson18Demo
    //
    //  ★ 配套实验（InternalsLab，老师早期建的底层实验台）：
    //      · 菜单 4：E04_ObjectSize.cs        ← 本文件第二、四段的出处
    //      · 菜单 5：E05_StackAndRecursion.cs ← 本文件第五、六段的出处
    //     位置：D:\Zlprogect\Projectset\learncsrop\csharp-from-python\InternalsLab\
    //
    //  ★ 注意：本项目的 .csproj 没有开 AllowUnsafeBlocks，
    //     所以全程只允许 sizeof(int) / sizeof(long) 这种"安全的内置类型 sizeof"；
    //     sizeof(自定义类型) 需要 unsafe 上下文，本课不碰（第 20 课会讲 Unsafe.SizeOf）。
    // ============================================================================
#pragma warning disable CS0649, CS0169, CS0414
    // ↑ 下面这些类型的字段是【故意】不赋值、也不读取的：本课要测的就是"空对象"有多大。
    //   编译器会因此警告 CS0649（从未赋值）/ CS0169（从未使用）/ CS0414（赋了值没人读）——
    //   它们正是这里想证明的东西，所以用 pragma 临时关掉（按行生效，不影响后面的代码）。

    // ────────────────────────────────────────────────────────────────────────
    //  被测量用的类型：字段个数 0 → 1 → 2 → 3，以及各种字段组合
    // ────────────────────────────────────────────────────────────────────────
    // ★ 为什么用【字段】而不是属性？
    //   属性会在背后生成一个隐藏字段 + get/set 两个方法，测量结果就被搅浑了。
    //   要测"字段占多少"，就得老老实实写字段。

    class Empty { }                                                  // 0 字节数据
    class OneInt { public int A; }                                   // 4 字节数据
    class TwoInt { public int A; public int B; }                     // 8 字节数据
    class ThreeInt { public int A; public int B; public int C; }     // 12 字节数据
    class OneLong { public long A; }                                 // 8 字节数据（一个 8 字节字段）
    class TwoLong { public long A; public long B; }                  // 16 字节数据
    class ThreeLong { public long A; public long B; public long C; } // 24 字节数据

    // 对照用的 struct：注意它们【不是对象】，没有 16 字节头
    struct Point { public int X; public int Y; }                     // 8 字节数据

    // ★ 下面这一对是本课最重要的对照实验：字段完全一样，只有 struct / class 的区别
    class ClassHolder { public Point P; }                            // P 内嵌在对象里
    class ClassHolderRef { public PointAsClass P = new PointAsClass(); }  // P 是"另一个堆对象"

    class PointAsClass { public int X; public int Y; }

    // 用来演示"值类型装在哪里"的类型
    class Player
    {
        public int Hp;                  // 这个 int 在【堆】上（它是 Player 对象的一部分）
        public Point Position;          // 这个 struct 也在【堆】上（内嵌在 Player 对象里）
        public string Name = "";        // 这个"地址"在堆上，它指向的字符串也在堆上
    }

    // 用来演示"引用字段要额外算目标对象"的类型
    class WithName
    {
        public string Name = "";        // 字段本身只占 8 字节（一个地址），字符串本体另算
    }
#pragma warning restore CS0649, CS0169, CS0414

    class Lesson18Demo
    {
        // ================================================================
        //  辅助方法
        // ================================================================

        /// <summary>
        /// 测量一个"没有任何字段的引用类型对象"在堆上占多少字节。
        /// ★ 做法来自 InternalsLab 的 E04_ObjectSize.cs：
        ///   先强制 GC 得到干净基线 → 分配 10 万个对象 → 看堆长大了多少 → 平均。
        /// </summary>
        /// <remarks>
        /// ★ 为什么把测量方法标成 NoInlining？
        ///   NoInlining = 禁止 JIT 把这个方法内联进调用方。
        ///   测量内存的方法一旦被内联，它的局部变量就和调用方的混在一起，
        ///   量的东西就不干净了（第 17 课量 JIT 时间时也是同一个理由）。
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static long MeasureClass<T>(int count = 100_000) where T : new()
        {
            // ① 先回收两轮，把前面几段产生的垃圾清干净，before 才是干净的起点。
            //   ★ 中间那句 WaitForPendingFinalizers 是等"终结器队列"跑完 ——
            //     有些对象要等终结器执行完才真正被回收，不等它，垃圾就会被算进结果里。
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // ② GetTotalMemory(true)：先做一次完整回收，再返回"当前托管堆里活着的字节数"
            long before = GC.GetTotalMemory(true);

            // ③ 先造数组：一来被测量的对象得有个地方挂住（否则会被 GC 当垃圾收走），
            //    二来数组自身的开销待会儿要减掉
            T[] arr = new T[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = new T();       // ← 真正被测量的分配就发生在这一行
            }

            long after = GC.GetTotalMemory(true);

            // ④ ★ KeepAlive 写在测量【之后】才有用：它告诉 JIT"arr 到这里还要用"，
            //    JIT 的存活分析是从后往前看的 → arr 在两次读数之间一直是活的
            //    → 它挂着的 10 万个对象一个都不会被回收。
            GC.KeepAlive(arr);

            // ⑤ 减掉引用数组自身的开销：
            //      对象头 8 + MethodTable 指针 8 + 长度 4 + 填充 4 = 24 字节
            //      每个元素是一个 8 字节的引用槽
            //    ★ 这里的顺序很重要：先转 double 再除。
            //      如果写成 (after - before - ...) / count（整数除法），
            //      24 会被截断成 23，看起来像"对象只占 23 字节"（第 11 课特意提醒过这个坑）。
            double perObject = (after - before - (24 + count * 8L)) / (double)count;
            return (long)Math.Round(perObject);
        }

        /// <summary>
        /// 测量一个"指定大小的 struct 数组"里每个元素占多少字节。
        /// ★ 要点：数组元素是【内嵌】在数组对象内部的，所以减掉数组开销之后，
        ///   剩下的就是"数据本身"的大小 —— 里面【不含】任何 16 字节对象头。
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static double MeasureStruct<T>(int count = 200_000) where T : new()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long before = GC.GetTotalMemory(true);
            T[] arr = new T[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = new T();       // struct 的 new 不上堆：只是把这块内存清零
            }
            long after = GC.GetTotalMemory(true);
            GC.KeepAlive(arr);

            // ★ 元素是值类型，数组内部直接排着 count 份数据，没有"每个元素 8 字节引用槽"这回事
            return (after - before - 24) / (double)count;
        }

        /// <summary>把一个类名和它的实测大小对齐打印出来。</summary>
        static void Print(string name, string fields, string layout, long actual)
        {
            Console.WriteLine($"  {name,-16}{fields,-12}{layout,-14}{actual,4} 字节");
        }

        /// <summary>
        /// 递归数深度：每调用一次就 +1，返回"一共能压多少层"。
        /// ★ 有安全阀：到 stopAt 层就主动停下并返回，所以这个函数【不会】把进程搞崩。
        /// </summary>
        /// <remarks>
        /// ★ NoInlining：如果 JIT 把它内联进调用方，那就不是"每层一个栈帧"了，测出来的深度毫无意义
        ///   （第 17 课量 JIT 时用的是同一个招数）。
        /// </remarks>
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static int CountStackFrames(int depth, int spillWords = 4, int stopAt = 20_000)
        {
            // ★ 局部变量也是栈帧的一部分：下面这个数组【就长在栈帧里】，
            //   把它调大（spillWords 从 4 改成 64）就会让【每帧变大 → 能压的层数变少】。
            //   这正是第九段【实验 5】要自己动手改的地方。
            //   （★ 这里必须用 stackalloc 而不是 new long[...]：new 出来的是【堆】上的数组，
            //     每帧只有 8 字节的引用落在栈上，那样就演示不出"帧变大"了。）
            Span<long> spill = stackalloc long[spillWords];
            spill[0] = depth;

            // 用一下数组，避免"变量被赋值却从未使用"的警告，同时挡住 JIT 把这块空间优化掉
            if (spill[0] < 0) return -1;

            if (depth >= stopAt) return depth;
            return CountStackFrames(depth + 1, spillWords, stopAt);
        }

        /// <summary>
        /// 和 CountStackFrames 是同一个函数，唯一的区别：调用【之后】还要加 1。
        /// ★ 加了这一句，调用就不再处于"尾位置"，JIT 无法把它变成 jmp → 栈帧必须真的压起来。
        ///   所以这个版本能压的层数【少得多】—— 这就是"尾调用优化"最直观的演示。
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static int CountStackFramesNoTail(int depth, int stopAt = 20_000)
        {
            if (depth >= stopAt) return depth;
            // ★ 故意在调用之后再加 1：有"活要干"，就没法尾调用
            return CountStackFramesNoTail(depth + 1, stopAt) + 1;
        }

        /// <summary>尾位置上的递归：用来演示 .NET 在 x64 上确实会做尾调用优化。</summary>
        private static int TailCountDown(int depth)
        {
            if (depth == 0) return 0;
            // ★ 这是尾位置上的调用（调用结果直接 return 出去，后面没有别的活要干）。
            //   x64 上的 JIT 会把它编译成 jmp 而不是 call，于是【栈帧根本不增长】。
            //   所以 C# 不是"完全不做尾调用优化"，准确的说法是：
            //     · 尾【调用】优化：x64 上条件满足时确实会做（下面那段就是证明）
            //     · 尾【递归】优化：.NET 【不保证】，不要拿它当"深递归不会崩"的理由
            return TailCountDown(depth - 1);
        }

        static void Main()
        {
            Console.WriteLine("===== 第 18 课：栈与堆、对象的内存布局 =====");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第一段：先确认"一个地址有多大"
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 一、先量一把尺子：指针（引用）多大 ----------");
            Console.WriteLine();

            // ★ 栈/堆的一切尺寸都建立在"一个地址多少字节"上，所以先量这个
            int ptrSize = sizeof(IntPtr);       // IntPtr 就是"一个指针/一个引用"的大小
            Console.WriteLine($"  64 位进程      : {Environment.Is64BitProcess}");
            Console.WriteLine($"  sizeof(IntPtr) : {ptrSize} 字节   ← 这就是一个引用占的字节数");
            Console.WriteLine($"  可寻址空间     : {Math.Pow(2, ptrSize * 8):E2} 字节（理论上限）");
            Console.WriteLine();
            Console.WriteLine($"  sizeof(int)    : {sizeof(int)}      字节");
            Console.WriteLine($"  sizeof(long)   : {sizeof(long)}      字节");
            Console.WriteLine($"  sizeof(double) : {sizeof(double)}      字节");
            Console.WriteLine($"  sizeof(bool)   : {sizeof(bool)}      字节   ← 注意 C# 的 bool 占 1 字节（Python 的 bool 是 int 的子类）");
            Console.WriteLine($"  sizeof(char)   : {sizeof(char)}      字节   ← UTF-16，所以一个中文汉字占 2 字节");
            Console.WriteLine();
            Console.WriteLine("  ★ Python 对照：CPython 的 int 是【对象】，一个不大的整数也要 28 字节上下");
            Console.WriteLine("     （PyLongObject：引用计数 8 + 类型指针 8 + 数字位数 8 + 若干个 30 位数字块），");
            Console.WriteLine("     C# 的 int 就是 4 个字节的裸数据。这就是\"数值计算快很多\"的第一个原因。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第二段：栈 vs 堆 对照表
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 二、栈 vs 堆 ----------");
            Console.WriteLine();
            Console.WriteLine("  ┌──────────┬────────────────────────┬────────────────────────────┐");
            Console.WriteLine("  │          │ 栈 Stack               │ 托管堆 Managed Heap        │");
            Console.WriteLine("  ├──────────┼────────────────────────┼────────────────────────────┤");
            Console.WriteLine("  │ 存什么   │ 局部变量、参数、返回地址 │ 对象本体                   │");
            Console.WriteLine("  │ 谁管理   │ 编译器 + CPU 栈指针      │ GC                         │");
            Console.WriteLine("  │ 速度     │ 极快（移动栈指针）       │ 快（移动分配指针）         │");
            Console.WriteLine("  │ 容量     │ 小（默认 1 MB / 线程）   │ 大（受物理内存限制）       │");
            Console.WriteLine("  │ 碎片     │ 不会                     │ 会（所以 GC 要压缩）       │");
            Console.WriteLine("  │ 释放     │ 方法返回时自动弹栈       │ 不确定，等 GC              │");
            Console.WriteLine("  └──────────┴────────────────────────┴────────────────────────────┘");
            Console.WriteLine();
            Console.WriteLine("  ★ 为什么栈\"极快\"？因为分配就是从栈指针【减去】一个数，释放就是【加】回去。");
            Console.WriteLine("     没有查找空闲块、没有链表、没有锁 —— 就是一条 CPU 指令。");
            Console.WriteLine("  ★ 为什么栈\"容量小\"？因为它是【连续预留】的一块地址空间，");
            Console.WriteLine("     预留 1MB 很便宜，预留 1GB 就等于每个线程都要吃掉 1GB。");
            Console.WriteLine();

            // 用 Stopwatch 把"栈分配便宜"变成一个可见的数字
            // （★ 这不是严谨的基准测试，只看量级；第 17 课讲过为什么不预热的数据不可信）
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long stackSink = 0;
            for (int i = 0; i < 10_000_000; i++)
            {
                Span<int> local = stackalloc int[4];   // ← 栈上分配 16 字节（Span 是第 20 课的内容，这里只看现象）
                local[0] = i;
                stackSink += local[0];
            }
            sw.Stop();
            double stackAllocMs = sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            long heapSink = 0;
            for (int i = 0; i < 1_000_000; i++)
            {
                int[] boxed = new int[4];     // ← 堆上分配：要付 24 字节数组头 + 16 字节数据
                boxed[0] = i;
                heapSink += boxed[0];
            }
            sw.Stop();
            double heapAllocMs = sw.Elapsed.TotalMilliseconds;

            Console.WriteLine($"  栈上分配 1000 万次 : {stackAllocMs,8:F1} ms   （每次 {stackAllocMs / 10_000_000 * 1000,8:F3} 微秒）");
            Console.WriteLine($"  堆上分配  100 万次 : {heapAllocMs,8:F1} ms   （每次 {heapAllocMs / 1_000_000 * 1000,8:F3} 微秒）");
            Console.WriteLine();
            Console.WriteLine("  ★ 别急着下结论：这两行【不可直接比较】—— 次数不同、还要算上 GC 的账。");
            Console.WriteLine("     真正能带走的一句是：栈分配没有\"分配\"这个动作，它只是移动指针；");
            Console.WriteLine("     堆分配虽然也只是移动指针，但事后要由 GC 来收（第 19 课的主角）。");
            Console.WriteLine($"  （防止编译器优化：校验和 {stackSink + heapSink != 0}）");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第三段：x64 上的对象布局
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 三、x64 上的对象布局 ----------");
            Console.WriteLine();
            Console.WriteLine("        托管堆上");
            Console.WriteLine("  ┌──────────────────────────────┐");
            Console.WriteLine("  │  Object Header  (8 字节)     │ ← 同步块索引：lock、缓存 GetHashCode 用它");
            Console.WriteLine("  ├──────────────────────────────┤");
            Console.WriteLine("  │  MethodTable 指针 (8 字节)   │ ← 指向类型描述表，GetType() 读的就是它");
            Console.WriteLine("  ├──────────────────────────────┤");
            Console.WriteLine("  │  字段 1                      │");
            Console.WriteLine("  │  字段 2                      │ ← 按 8 字节对齐排列");
            Console.WriteLine("  └──────────────────────────────┘");
            Console.WriteLine("         ↑");
            Console.WriteLine("     变量里存的是这个地址（8 字节）");
            Console.WriteLine();
            Console.WriteLine("  ★ 那 16 字节的头部是什么？");
            Console.WriteLine("     · Object Header（也叫同步块索引）：平时是 0，一旦你 lock 它、");
            Console.WriteLine("       或第一次调用 GetHashCode，运行时就在\"同步块表\"里占一格，把索引写在这里。");
            Console.WriteLine("       → 所以 lock 一个对象，会改动这个对象自己的头部。");
            Console.WriteLine("     · MethodTable 指针：GetType()、虚方法分派（第 07 课的多态）、");
            Console.WriteLine("       类型转换检查，都靠它。运行时会把这个指针的低位当标志位用（这就是为什么");
            Console.WriteLine("       对齐是 8 字节而不是随便什么数）。");
            Console.WriteLine();
            Console.WriteLine("  ★ 大小规律：");
            Console.WriteLine("       对象大小 = 16（头）+ 字段总大小，然后【向上对齐到 8 的倍数】");
            Console.WriteLine("       并且最小是 24 字节");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第四段：实测 16 + 字段 → 对齐到 8
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 四、实测：16 + 字段，向上对齐到 8 ----------");
            Console.WriteLine();
            Console.WriteLine("  （每类分配 10 万个，算平均；数字可能有 ±0 的波动，看规律）");
            Console.WriteLine();
            Console.WriteLine("  类型            字段数据    推导          实测");
            Console.WriteLine("  ────────────────────────────────────────────────────────────");

            Print("Empty", "—", "16+0=16→24", MeasureClass<Empty>());
            Print("OneInt", "4", "16+4=20→24", MeasureClass<OneInt>());
            Print("TwoInt", "8", "16+8=24→24", MeasureClass<TwoInt>());
            Print("ThreeInt", "12", "16+12=28→32", MeasureClass<ThreeInt>());
            Print("OneLong", "8", "16+8=24→24", MeasureClass<OneLong>());
            Print("TwoLong", "16", "16+16=32→32", MeasureClass<TwoLong>());
            Print("ThreeLong", "24", "16+24=40→40", MeasureClass<ThreeLong>());

            Console.WriteLine();
            Console.WriteLine("  ★ 五个必须记住的点：");
            Console.WriteLine("     ① Empty 是 24 字节，不是 0 —— 空对象也有对象头和 MethodTable 指针。");
            Console.WriteLine("     ② OneInt 和 Empty 一样大：那 4 字节的 int 塞进了对齐填充里，等于免费。");
            Console.WriteLine("     ③ ★ TwoInt 是 24，不是 32。16 + 8 = 24 本身就是 8 的倍数，不需要再补。");
            Console.WriteLine("        这个数字我见过太多次被写错 —— 记住只有\"零头\"才需要向上补。");
            Console.WriteLine("     ④ ThreeInt 是 32：16 + 12 = 28，零头 4 字节跨过了 24 这条线，要多花 8 字节。");
            Console.WriteLine("        那个多花的 8 字节里，只有 4 字节是数据，另外 4 字节是纯填充。");
            Console.WriteLine("     ⑤ OneLong 也是 24：一个 long 是 8 字节，和两个 int 一样占满填充位。");
            Console.WriteLine("        所以\"把 int 换成 long\"在【单字段】的情况下不花任何额外内存 ——");
            Console.WriteLine("        这正是本课【实验 1】要你亲手验证的东西。");
            Console.WriteLine();

            // struct 的对照：同样两份数据，struct 版本没有 16 字节头
            Console.WriteLine("  对照：同样的数据如果是 struct（数组元素内嵌，没有对象头）");
            Console.WriteLine($"    Point（2×int，8 字节数据） 实测每个元素 {MeasureStruct<Point>():F1} 字节");
            Console.WriteLine("    ★ 8 字节 vs class 版本的 24 字节 —— 这就是第 08 课说的\"struct 省内存\"");
            Console.WriteLine("      到底省在哪：省掉了【每个对象 16 字节的头】。");
            Console.WriteLine("    ★ 但前提是它别被装箱！一装箱，24 字节立刻又回来了。");
            Console.WriteLine();

            // 装箱的账：拿 E04 的第二个实验对照
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long boxBefore = GC.GetTotalMemory(true);
            object[] boxes = new object[100_000];
            for (int i = 0; i < 100_000; i++)
            {
                boxes[i] = 42;              // ← 装箱：在堆上造一个盒子，把 42 复制进去
            }
            long boxAfter = GC.GetTotalMemory(true);
            GC.KeepAlive(boxes);
            long perBox = (boxAfter - boxBefore - (24 + 100_000 * 8L)) / 100_000;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long rawBefore = GC.GetTotalMemory(true);
            int[] raws = new int[100_000];
            for (int i = 0; i < 100_000; i++)
            {
                raws[i] = 42;               // ← 不装箱
            }
            long rawAfter = GC.GetTotalMemory(true);
            GC.KeepAlive(raws);
            double perRaw = (rawAfter - rawBefore - 24) / 100_000.0;

            Console.WriteLine("  装箱的账：");
            Console.WriteLine($"    装箱一个 int    : {perBox} 字节   （值本身只有 4 字节）");
            Console.WriteLine($"    不装箱的 int    : {perRaw:F1} 字节   ← 只有数据本身");
            Console.WriteLine($"    → 放大 {perBox / 4.0:F1} 倍。这就是为什么 Unity 里怕装箱怕得要命。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第五段：引用字段的账 —— 字符串本体是"另一个对象"
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 五、引用字段：字段只有 8 字节，本体另算 ----------");
            Console.WriteLine();

            long holderSize = MeasureClass<WithName>();
            Console.WriteLine($"  class WithName {{ public string Name; }}");
            Console.WriteLine($"    对象本身             : {holderSize} 字节  （16 头 + 8 的引用）");
            Console.WriteLine("    但它指向的字符串本体  : 另算！是堆上【另一个对象】");
            Console.WriteLine();
            Console.WriteLine("  ★ 这就是\"对象大小\"容易被低估的地方：");
            Console.WriteLine("     MeasureClass 量的是\"这个对象自己占多少\"，不包含它引用的东西。");
            Console.WriteLine("     一个 Person 对象有 5 个字符串字段，它自己只有 16 + 40 = 56 字节，");
            Console.WriteLine("     但它背后可能挂着一两百字节的字符串 —— 那些是【另外很多个对象】。");
            Console.WriteLine();

            // 用"字符数 → 字符串对象大小"实测一次
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long strBefore = GC.GetTotalMemory(true);
            string[] strs = new string[50_000];
            for (int i = 0; i < 50_000; i++)
            {
                // 每个字符串 10 个字符，而且每次都 new 一个【不同的】对象，避免被驻留池合并
                strs[i] = new string((char)('a' + i % 26), 10);
            }
            long strAfter = GC.GetTotalMemory(true);
            GC.KeepAlive(strs);
            double perStr = (strAfter - strBefore - (24 + 50_000 * 8L)) / 50_000.0;

            Console.WriteLine($"  一个 10 个字符的字符串实际占 {perStr:F1} 字节。");
            Console.WriteLine("  推导：16（头）+ 4（长度）+ 10×2（UTF-16 每字符 2 字节）+ 填充 = 40");
            Console.WriteLine("  ★ 所以字符串的\"隐形开销\"很大：10 个字的数据，花了 20 字节存，40 字节总共。");
            Console.WriteLine();

            // 引用类型字段 vs 值类型字段：字段一样，代价完全不同
            Console.WriteLine("  对照实验（这一对是本课最重要的对照）：");
            Console.WriteLine($"    class ClassHolder    {{ Point P; }}           → {MeasureClass<ClassHolder>()} 字节（P 内嵌）");
            Console.WriteLine($"    class ClassHolderRef {{ PointAsClass P; }}    → {MeasureClass<ClassHolderRef>()} 字节（P 是引用）");
            Console.WriteLine("        ↑ 两个都是 24 字节（16 头 + 8 字节数据）。要注意的是第 4 段那个方法");
            Console.WriteLine("          只量 ClassHolderRef 这个对象【自己】：里面那个 8 字节是引用；");
            Console.WriteLine("          它指向的每个 PointAsClass 对象，还要【各自】再花 24 字节。");
            Console.WriteLine("    ★ 把 struct 塞进 class 的字段里，省掉的是\"每个对象一份的头\"；");
            Console.WriteLine("      换成 class 字段，你就为每个 Point 多买了一次 16 字节的头。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第六段：「值类型在栈上」为什么是错的
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 六、\"值类型在栈上\"为什么是错的 ----------");
            Console.WriteLine();
            Console.WriteLine("  先记住一句话：");
            Console.WriteLine("     栈 / 堆 的区别【不属于类型】，属于【位置】。");
            Console.WriteLine();
            Console.WriteLine("  唯一的判断标准（问自己一句话）：");
            Console.WriteLine("     「这个值是不是某个堆对象的一部分？」");
            Console.WriteLine("         是   → 堆");
            Console.WriteLine("         不是 → 它自己就是一个独立的局部变量 → 栈");
            Console.WriteLine();

            Player player = new Player { Hp = 100, Name = "小明" };
            player.Position.X = 3;      // ← 点号能一路穿进对象里去改
            int localInt = 7;           // ← 独立的局部变量
            Point localPoint = new Point { X = 1, Y = 2 };   // ← 独立的局部变量（new 也没上堆）

            Console.WriteLine("  场景代码：");
            Console.WriteLine("     int localInt = 7;                       // 独立的局部变量");
            Console.WriteLine("     Point localPoint = new Point();         // 独立的局部变量（new 没上堆！）");
            Console.WriteLine("     Player player = new Player();");
            Console.WriteLine("     player.Hp / player.Position / player.Name");
            Console.WriteLine();
            Console.WriteLine("  逐个判定：");
            Console.WriteLine("     localInt              → 栈    （它就是变量本身，里面直接装着 7）");
            Console.WriteLine("     localPoint            → 栈    （8 个字节整个躺在栈帧里；struct 的 new 只是清零）");
            Console.WriteLine("     player 这个【变量】    → 栈    （变量本身在栈上，里面装的是 8 字节的地址）");
            Console.WriteLine("     player 指向的对象      → 堆    （new 出来的本体）");
            Console.WriteLine("     player.Hp             → 堆    ★ 值类型！但它是对象的字段，跟着对象走");
            Console.WriteLine("     player.Position       → 堆    ★ 值类型！内嵌在 Player 对象里，也是对象的一部分");
            Console.WriteLine("     player.Position.X     → 堆    （内嵌的内嵌，还是对象的一部分）");
            Console.WriteLine("     player.Name 这个地址   → 堆    （字段的位置跟着对象走）");
            Console.WriteLine("     player.Name 指向的串   → 堆    （string 是引用类型，本体在堆上）");
            Console.WriteLine();

            // 用一个"能编译通过"的事实来佐证内嵌
            Console.WriteLine("  一个不用测量也看得懂的佐证：");
            Console.WriteLine("     player.Position.X = 3;   // ← 这行能编译通过");
            Console.WriteLine("     ★ 说明 Position 的 8 个字节就长在 Player 对象【内部】，中间没有第二次寻址。");
            Console.WriteLine("     ★ 反例：如果 Position 是【属性】而不是字段，这行会直接编译报错 CS1612 ——");
            Console.WriteLine("       因为读属性拿到的只是一份副本，改副本没有意义（第 11 课讲过这个坑）。");
            Console.WriteLine();

            Console.WriteLine("  另外三种\"值类型却在堆上\"的情况：");
            Console.WriteLine("     ① 数组元素：int[] arr = new int[10];  → 那 10 个 int 都在堆上（内嵌在数组里）");
            Console.WriteLine("     ② 装箱：object o = 42;               → 42 的一份副本被复制到堆上的盒子里");
            Console.WriteLine("     ③ 静态字段：static int Count;         → 在\"静态字段区\"，既不在栈也不在普通堆上，");
            Console.WriteLine("        它的地址写在类型的 MethodTable 里（所以它活得和类型一样久）");
            Console.WriteLine();

            int[] embeddedInts = new int[10];        // ← 这 10 个 int 全在堆上
            embeddedInts[0] = localInt + localPoint.X + player.Hp;   // 用一下，避免"变量未使用"警告
            Console.WriteLine($"  （佐证：int[10] 的 10 个元素都在堆上，占用 24 + 40 = 64 字节；");
            Console.WriteLine($"    而 int[] 变量自己只有 8 字节，在栈上。当前 arr[0] = {embeddedInts[0]}）");
            Console.WriteLine();

            Console.WriteLine("  ★ Python 对照：Python 里【根本没有\"值类型住在栈上\"这回事】。");
            Console.WriteLine("     所有 Python 对象都在堆上，变量只是一个\"名字 → 对象\"的绑定（PyObject*）。");
            Console.WriteLine("     连 a = 1 那个 1 都是堆上的一个 PyLongObject。");
            Console.WriteLine("     所以 Python 没有\"结构体省内存\"这一说（除非用 numpy / array 模块）。");
            Console.WriteLine("     C# 的 struct 是一块【真的可以内嵌、可以放在栈上】的裸内存。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第七段：栈帧与递归
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 七、栈帧与递归 ----------");
            Console.WriteLine();
            Console.WriteLine("  每次方法调用都会在栈上压入一个【栈帧】，里面装着：");
            Console.WriteLine("     · 参数");
            Console.WriteLine("     · 返回地址（回到调用方哪一行继续执行）");
            Console.WriteLine("     · 局部变量");
            Console.WriteLine("     · 保存的寄存器（被调用方要用到的那些）");
            Console.WriteLine("     · 有时还有一点安全余量（Probe 页，见第八段）");
            Console.WriteLine();
            Console.WriteLine("  所以：栈帧越大 → 同样的栈能撑的层数越少。");
            Console.WriteLine();

            sw.Restart();
            int frames4 = CountStackFrames(0, 4);
            sw.Stop();
            double smallFrameMs = sw.Elapsed.TotalMilliseconds;

            // ★ 同一个函数，只是把每帧的 stackalloc 数组从 4 个 long 加到 64 个 long（多 480 字节/帧）
            int frames64 = CountStackFrames(0, 64);
            int framesNoTail = CountStackFramesNoTail(0);
            int tailDepth = TailCountDown(10_000_000);

            Console.WriteLine($"  实测（每帧 1 个 stackalloc 数组，4 个 long）  : 压了 {frames4:N0} 层，用时 {smallFrameMs:F1} ms");
            Console.WriteLine($"  同函数，每帧改成 64 个 long（+480 字节）    : 压了 {frames64:N0} 层");
            Console.WriteLine("     ★ 每帧的局部变量多了一点，能压的层数就【明显变少】——");
            Console.WriteLine("       所以\"栈够不够用\"从来不只看递归深度，还要看每层的帧有多大。");
            Console.WriteLine();
            Console.WriteLine($"  对照（调用后还要 +1，无法尾调用）      : 压了 {framesNoTail:N0} 层");
            Console.WriteLine($"  对照（尾位置调用，TailCountDown）      : 跑到 {tailDepth:N0} 也没崩");
            Console.WriteLine();
            Console.WriteLine("  ★ 为什么最后那个 1000 万层的没崩？");
            Console.WriteLine("     因为 return TailCountDown(depth - 1); 是【尾位置】的调用，");
            Console.WriteLine("     x64 上 JIT 把它编译成了跳转（jmp）而不是调用（call），栈帧根本不增长。");
            Console.WriteLine("  ★ 所以准确的说法是：");
            Console.WriteLine("     · 尾【调用】优化：x64 上满足条件时 .NET 会做（上面就是证据）");
            Console.WriteLine("     · 尾【递归】优化：.NET 【不保证】。换个目标平台、换个 JIT、加一个局部变量，");
            Console.WriteLine("       这个优化就可能消失。永远不要靠它把深递归变成循环。");
            Console.WriteLine("     （F# 会保证尾递归优化，C# 不会 —— 原因之一是保留完整栈帧才能给你准确的异常调用栈）");
            Console.WriteLine("  ★ 实用结论：处理\"可能有几万层深\"的数据结构（超深链表、超深目录树）：");
            Console.WriteLine("     老老实实改成【迭代 + 自己的栈（Stack<T>）】，不要赌 JIT。");
            Console.WriteLine();

            // 异常堆栈 = 栈帧的快照
            Console.WriteLine("  异常堆栈就是栈帧的快照：");
            try
            {
                Level1();
            }
            catch (Exception ex)
            {
                Console.WriteLine("    捕获到异常，StackTrace 记录了完整的调用链（每行 = 一层栈帧）：");
                Console.WriteLine();
                // ★ StackTrace 是可空的（NetStandard 的签名如此），所以这里要 ?. 或者判空 —— 第 12 课的内容
                string trace = ex.StackTrace ?? "（没有堆栈信息）";
                foreach (string line in trace.Split('\n'))
                {
                    string trimmed = line.Trim();
                    if (trimmed.Length > 0)
                    {
                        Console.WriteLine($"      {trimmed}");
                    }
                }
                Console.WriteLine();
                Console.WriteLine("    ★ 所以\"抛异常\"很贵：它要把整条调用链抓下来。");
                Console.WriteLine("      不要用异常做流程控制（能用返回值判断的，就别抛）。");
            }
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第八段：StackOverflow 为什么 catch 不到
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 八、为什么栈溢出 catch 不到 ----------");
            Console.WriteLine();
            Console.WriteLine("  很多人第一反应是这么写：");
            Console.WriteLine();
            Console.WriteLine("      try { Boom(0); }");
            Console.WriteLine("      catch (Exception e) { Console.WriteLine(\"抓到了\"); }   // ← 永远不会执行");
            Console.WriteLine();
            Console.WriteLine("  它抓不到的，原因是：");
            Console.WriteLine();
            Console.WriteLine("     ① catch 块本身【需要栈】才能运行（要压栈帧、要调 Console.WriteLine）");
            Console.WriteLine("     ② 栈已经满了 —— 没有空间给 catch 块用");
            Console.WriteLine("     ③ 运行时的选择是：不冒险，直接终止进程");
            Console.WriteLine();
            Console.WriteLine("  所以 StackOverflowException 有几个\"反常识\"的特点：");
            Console.WriteLine();
            Console.WriteLine("     · try / catch 【抓不到】它（catch(Exception) 也抓不到）");
            Console.WriteLine("     · finally 【不会】执行");
            Console.WriteLine("     · 进程【立即死亡】，退出码非 0，没有任何\"优雅收尾\"的机会");
            Console.WriteLine("     · 在 .NET Core / .NET 5+ 里，你【无法】从托管代码捕获它");
            Console.WriteLine("       （.NET Framework 时代还能在某些 CLR 宿主里拦一下，现在是彻底不给机会）");
            Console.WriteLine("     · 它是 .NET 里少数【无法用异常机制处理】的错误之一");
            Console.WriteLine();
            Console.WriteLine("  ★ 为什么\"没有空间\"？顺便看一眼 CLR 的保险丝：");
            Console.WriteLine("     每次方法进入，JIT 生成的代码会做一次【栈探测】（probe）：");
            Console.WriteLine("     往栈顶下面的一页内存写一个字节。这一页就是\"守卫页\"（guard page）。");
            Console.WriteLine("     指针一旦走进守卫页，操作系统就抛一个【结构化异常】，");
            Console.WriteLine("     CLR 看到\"这是栈溢出\"就直接杀进程 —— 它不会去抛一个托管异常。");
            Console.WriteLine("     探测的作用是保证\"越界之前就被发现\"，而不是等踩到别的线程的栈。");
            Console.WriteLine();
            Console.WriteLine("  会踩到这个坑的真实场景：");
            Console.WriteLine("     · 树/链表的递归遍历，数据深到几十万层");
            Console.WriteLine("     · 递归解析用户输入（比如没有长度上限的嵌套 JSON）← 安全漏洞常见来源");
            Console.WriteLine("     · 属性 getter 里不小心调了自己（无限递归，但看起来像死循环）");
            Console.WriteLine("     · 终结器里抛异常、或终结器互相等待");
            Console.WriteLine();
            Console.WriteLine("  怎么防：");
            Console.WriteLine("     ① 深结构改成【迭代】+ 显式 Stack<T>（把\"隐式的栈\"搬到\"显式的堆\"上）");
            Console.WriteLine("     ② 一定要递归时，加上深度上限参数，超了就抛普通异常或返回错误");
            Console.WriteLine("     ③ 需要真的大栈时，可以新建线程并指定栈大小（下面的注释里有写法）");
            Console.WriteLine("     ④ 也可以给整个进程改默认栈大小：可执行文件的 PE 头里有一个\"保留栈\"字段，");
            Console.WriteLine("        改它需要链接器选项或运行时配置（.NET 没有简单的 csproj 开关，");
            Console.WriteLine("        所以实操上通常还是用 ③ 新建一个大栈线程）；");
            Console.WriteLine("        ③④ 对【主线程】都无效 —— 主线程的栈在进程启动时就定好了。");
            Console.WriteLine();
            Console.WriteLine("  ③ 的写法（本项目没执行，只是留个样子给你自己试）：");
            Console.WriteLine();
            Console.WriteLine("      // 默认 1MB 不够？给这个线程 16MB 栈");
            Console.WriteLine("      var t = new Thread(() => { /* 深递归放这里 */ }, 16 * 1024 * 1024);");
            Console.WriteLine("      t.Start();");
            Console.WriteLine("      t.Join();");
            Console.WriteLine("      // ★ 注意：栈大小只能在【创建线程时】指定，创建完就改不了。");
            Console.WriteLine();
            Console.WriteLine("  ★ 想亲眼看到进程崩溃吗？InternalsLab 的菜单 5（E05_StackAndRecursion.cs）");
            Console.WriteLine("     里有一个\"输入 YES 就真的崩\"的开关，那里的注释和这里一模一样 ——");
            Console.WriteLine("     建议你至少试一次：亲眼看到\"进程忽然没了、退出码不是 0\"的记忆，比读十遍都深。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第九段：自己动手改坏它
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 九、自己动手改坏它（本课最重要的部分）----------");
            Console.WriteLine();
            Console.WriteLine("  下面每条都在\"预期会看到什么\"里给了答案。改一处、重跑一次，比读十行强。");
            Console.WriteLine();
            Console.WriteLine("  【实验 1】把 OneInt 的 int 换成 long");
            Console.WriteLine("     改：class OneInt { public long A; }");
            Console.WriteLine("     想：16 + 8 = 24 → 还是 24");
            Console.WriteLine("     看：数字【没变】。为什么？因为 8 字节刚好把填充位填满，一个字节都没多。");
            Console.WriteLine("     再改：在 OneInt 里再加一个 int 字段（共 12 字节），这次会变成 32。");
            Console.WriteLine();
            Console.WriteLine("  【实验 2】把 MeasureClass 里的 GC.Collect() 注释掉");
            Console.WriteLine("     看：数字开始乱跳，24 可能变成 23、56 甚至变成 200 多。");
            Console.WriteLine("     为什么？因为 before 的基线里混着前面几段留下的垃圾，");
            Console.WriteLine("     读数就不再是\"干净起点 → 只多了新对象\"这个差值了。");
            Console.WriteLine("     ★ 教训：测内存【必须先强制回收】把基线弄干净，否则你测的是噪声。");
            Console.WriteLine();
            Console.WriteLine("  【实验 3】把 count 从 100_000 改成 10");
            Console.WriteLine("     看：数字变得很难看（可能是 24，也可能是 16 或 32 甚至 0）。");
            Console.WriteLine("     为什么？① 样本太少，一点噪声就影响平均；");
            Console.WriteLine("     ② 数组自身 24 字节的开销摊到 10 个元素上，误差被放大了 1 万倍。");
            Console.WriteLine("     ★ 教训：测平均值时，样本要足够大，固定开销才能被摊薄。");
            Console.WriteLine();
            Console.WriteLine("  【实验 4】把求平均那行改成整数除法");
            Console.WriteLine("     改：(after - before - (24 + count * 8L)) / count   ← 去掉 (double)");
            Console.WriteLine("     看：24 会变成 23，56 会变成 55 —— 全部少 1。");
            Console.WriteLine("     为什么？整数除法是【截断】不是四舍五入（第 02 课的坑在这里又出现一次）。");
            Console.WriteLine("     ★ 教训：凡是要\"平均\"，先把被除数转成 double。");
            Console.WriteLine();
            Console.WriteLine("  【实验 5】把 CountStackFrames 的 spillWords 从 4 改成 64");
            Console.WriteLine("     改：int frames4 = CountStackFrames(0, 4);   →  CountStackFrames(0, 64)");
            Console.WriteLine("     看：能递归的层数【明显变少】。");
            Console.WriteLine("     为什么？每帧多了 60 个 long（480 字节）—— 局部变量就长在栈帧里，帧越大层数越少。");
            Console.WriteLine("     ★ 教训：\"栈溢出\"不只看递归深度，还看每层的帧有多大。");
            Console.WriteLine();
            Console.WriteLine("  【实验 6】把 MeasureClass<ClassHolder>() 换成 MeasureStruct<Point>()");
            Console.WriteLine("     看：24 → 8。少了整整 16 字节，这就是\"每个对象一份的 16 字节头\"。");
            Console.WriteLine("     为什么？数组元素是内嵌的，不长成独立对象，也就没有对象头。");
            Console.WriteLine("     ★ 教训：struct 省内存的前提是\"别把它变成独立对象\"。");
            Console.WriteLine();
            Console.WriteLine("  【实验 7】把变量 live 起来：加一句 GC.KeepAlive(arr)，位置放在读数【之前】试试");
            Console.WriteLine("     看：数字开始随机波动，甚至偶尔偏小。");
            Console.WriteLine("     为什么？KeepAlive 只影响\"到那一行为止还活着\"，放在中间会让 JIT");
            Console.WriteLine("     认为后面 arr 已经死了 —— 那 10 万个对象可能在任何一次 GC 里被收走。");
            Console.WriteLine("     ★ 教训：KeepAlive 必须写在测量的【最后】，这是 E04 里的原话。");
            Console.WriteLine();

            Console.WriteLine("===== 示例结束 =====");
            Console.WriteLine();
            Console.WriteLine("  下一步：去做 Lesson18Practice 的 5 个 TODO。");
            Console.WriteLine("  然后去 InternalsLab 跑菜单 4 和菜单 5，那里面还有 struct 尺寸和崩溃演示。");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }

        // ================================================================
        //  给"异常堆栈"那一段用的三个调用层
        // ================================================================
        static void Level1() => Level2();
        static void Level2() => Level3();
        static void Level3() => ThrowHere();

        static void ThrowHere()
        {
            throw new InvalidOperationException("这是 Level3 抛出的异常");
        }
    }
}
