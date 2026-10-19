// ============================================================================
//  第 19 课 · 练习：垃圾回收（GC）
//
//  ★ 本课重点：
//     1. 用 GC.GetAllocatedBytesForCurrentThread() 量化"我产生了多少垃圾"
//     2. 分代与晋升：Gen0 → Gen1 → Gen2（单向，封顶在 Gen2）
//     3. ★ 用 WeakReference【证明】对象真的被回收了（最有力的手段）
//     4. 大对象堆 LOH 的 85,000 字节门槛（字节数，不是元素个数）
//     5. 综合：一个"内存快照对比"小工具
//
//  ★ 运行方式：
//      1. 打开 第19课.sln，把 Lesson19Practice 设为启动项目，Ctrl + F5
//      2. 或者命令行：dotnet run --project Lesson19Practice
//
//  ⚠️ 本文件里你会大量使用 GC.Collect() —— 练习和诊断是它【唯一】的合法场合。
//     真实项目里永远不要写它（理由见课文第十一节）。
//
//  ★ 每做完一个 TODO 就 Ctrl + F5 跑一次，把输出和 TODO 里的【预期输出】对照。
//    对不上不要改预期，改代码 —— 预期是给你当"标准答案"用的。
// ============================================================================

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ 本课的类/字段写在这里（如果需要）↓↓↓
    // ================================================================

    /// <summary>
    /// 一个"大的"托管对象：挂着一个 100,000 字节的数组。
    /// ★ 100,000 大于 85,000 → 这个数组进【大对象堆（LOH）】，方便观察。
    /// ★ 字段都给了初值，这样在 Nullable enable 下不会报 CS8618。
    /// </summary>
    class Payload
    {
        public byte[] Data = new byte[100_000];
        public string Tag = "";

        public Payload(string tag) => Tag = tag;
    }

    /// <summary>
    /// TODO 5 要用的"内存快照"：把某一刻 GC 的几个关键数字一起记下来。
    /// ★ 字段先全部给上初值 —— 这样 new GcSnapshot() 就能直接用。
    /// ★ LOH 不是"第 3 代"，它在 GenerationInfo 里排在 MaxGeneration 的【后面一位】，
    ///   所以这里用常量 LohIndex 表示它的下标。
    /// </summary>
    class GcSnapshot
    {
        public const int LohIndex = 3;          // GC.MaxGeneration(=2) + 1

        public string Label = "";               // 这条快照的说明（比如 "第 1 轮分配后"）
        public long HeapSizeBytes;              // info.HeapSizeBytes
        public long CommittedBytes;             // info.TotalCommittedBytes
        public long FragmentedBytes;            // info.FragmentedBytes
        public long TotalHeapBytes;             // GC.GetTotalMemory(false)
        public long AllocatedBytes;             // GC.GetAllocatedBytesForCurrentThread()
        public int Gen0Count;                   // GC.CollectionCount(0)
        public int Gen1Count;                   // GC.CollectionCount(1)
        public int Gen2Count;                   // GC.CollectionCount(2)
        public long LohSizeBytes;               // info.GenerationInfo[LohIndex].SizeAfterBytes
        public DateTime TakenAt = DateTime.Now; // 什么时候抓的
    }

    class Lesson19Practice
    {
        // ================================================================
        // ↓↓↓ 本课的辅助方法写在这里 ↓↓↓
        // ================================================================

        /// <summary>
        /// 让 GC 跑一次【完整】回收，并把等待终结器的对象也处理掉。
        /// ★ 为什么要连写两次 Collect()？
        ///   第一次把普通垃圾收走；但如果某些对象有终结器，它们会被挪进"待终结队列"
        ///   （那一刻它们还活着），所以需要再收一次才能真正清干净。
        ///   WaitForPendingFinalizers() 就是"等终结队列跑完"（第 20 课细讲）。
        /// ★ 只有测试/诊断代码才需要这么写。
        /// </summary>
        static void ForceFullGc()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// 抓一张内存快照（TODO 5 用）。
        /// </summary>
        static GcSnapshot Capture(string label)
        {
            var info = GC.GetGCMemoryInfo();
            return new GcSnapshot
            {
                Label = label,
                HeapSizeBytes = info.HeapSizeBytes,
                CommittedBytes = info.TotalCommittedBytes,
                FragmentedBytes = info.FragmentedBytes,
                TotalHeapBytes = GC.GetTotalMemory(false),
                AllocatedBytes = GC.GetAllocatedBytesForCurrentThread(),
                Gen0Count = GC.CollectionCount(0),
                Gen1Count = GC.CollectionCount(1),
                Gen2Count = GC.CollectionCount(2),
                LohSizeBytes = info.GenerationInfo[GcSnapshot.LohIndex].SizeAfterBytes,
                TakenAt = DateTime.Now
            };
        }

        /// <summary>
        /// 把两张快照并排打印出来，并给出差值（TODO 5 用）。
        /// </summary>
        static void PrintComparison(GcSnapshot before, GcSnapshot after)
        {
            Console.WriteLine($"  {"指标",-22}{"前",16}{"后",16}{"变化",16}");
            Console.WriteLine("  " + new string('─', 70));
            Console.WriteLine($"  {"堆大小 HeapSize",-22}{before.HeapSizeBytes,16:N0}{after.HeapSizeBytes,16:N0}{after.HeapSizeBytes - before.HeapSizeBytes,16:N0}");
            Console.WriteLine($"  {"已提交 Committed",-22}{before.CommittedBytes,16:N0}{after.CommittedBytes,16:N0}{after.CommittedBytes - before.CommittedBytes,16:N0}");
            Console.WriteLine($"  {"碎片 Fragmented",-22}{before.FragmentedBytes,16:N0}{after.FragmentedBytes,16:N0}{after.FragmentedBytes - before.FragmentedBytes,16:N0}");
            Console.WriteLine($"  {"堆内总计 GetTotalMemory",-22}{before.TotalHeapBytes,16:N0}{after.TotalHeapBytes,16:N0}{after.TotalHeapBytes - before.TotalHeapBytes,16:N0}");
            Console.WriteLine($"  {"累计分配 Allocated",-22}{before.AllocatedBytes,16:N0}{after.AllocatedBytes,16:N0}{after.AllocatedBytes - before.AllocatedBytes,16:N0}");
            Console.WriteLine($"  {"LOH 大小",-22}{before.LohSizeBytes,16:N0}{after.LohSizeBytes,16:N0}{after.LohSizeBytes - before.LohSizeBytes,16:N0}");
            Console.WriteLine($"  {"Gen0 次数",-22}{before.Gen0Count,16}{after.Gen0Count,16}{after.Gen0Count - before.Gen0Count,16}");
            Console.WriteLine($"  {"Gen1 次数",-22}{before.Gen1Count,16}{after.Gen1Count,16}{after.Gen1Count - before.Gen1Count,16}");
            Console.WriteLine($"  {"Gen2 次数",-22}{before.Gen2Count,16}{after.Gen2Count,16}{after.Gen2Count - before.Gen2Count,16}");
            Console.WriteLine($"  {"采样时刻",-22}{before.TakenAt:HH:mm:ss.fff,16}{after.TakenAt:HH:mm:ss.fff,16}");
        }

        // ================================================================
        // ↓↓↓ TODO 5 要用到的"长命引用"（故意做成静态字段，模拟内存泄漏）↓↓↓
        // ================================================================
        // 【为什么放静态字段】静态字段是 GC 的根 → 里面的对象永远不会被回收。
        //   这正是"静态集合只 Add 不 Remove = 内存泄漏"的原理。
        static readonly List<Payload> LeakedPayloads = new List<Payload>();

        static void Main()
        {
            Console.WriteLine("===== 第 19 课练习：垃圾回收 =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 测分配量：我到底制造了多少垃圾？
            // ================================================================
            // 【要求】
            //   1. 在循环【之前】调 GC.GetAllocatedBytesForCurrentThread()，记下基准值
            //   2. 循环 200,000 次，每次 new 一个 byte[24]，并用 GC.KeepAlive 保住它
            //   3. 在循环【之后】再取一次，算出这段循环一共分配了多少字节
            //   4. 打印三项：总分配字节数、平均每个对象多少字节、本次 Gen0 回收次数
            //      （Gen0 次数用 GC.CollectionCount(0)，记得也在循环前后各取一次做差）
            //
            // 【预期输出】(数字会随机器/运行时略有差异，量级必须对)
            //     循环 200,000 次 new byte[24]
            //     新分配字节 : 4,800,000 字节（约 4.58 MB）
            //     平均每个   : 24.0 字节
            //     期间 Gen0  : 0 次
            //
            // 【提示】
            //     long before = GC.GetAllocatedBytesForCurrentThread();
            //     for (int i = 0; i < 200_000; i++)
            //     {
            //         var bytes = new byte[24];
            //         GC.KeepAlive(bytes);        // 没有这一行，JIT 可能干脆不给你分配
            //     }
            //     long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            //     // ★ 先转 double 再除，否则整数除法会把小数截掉（第 02 课的坑）
            //     double perObject = allocated / (double)200_000;
            //
            // 【考察点】
            //   · GetTotalMemory 量的是"堆里现在有多少"，GetAllocatedBytesForCurrentThread
            //     量的是"累计分配了多少"—— 前者会降，后者只增。别混。
            //   · GC.KeepAlive 是让测量成立的工具，不是"防止 GC"的魔法
            //   · 整数除法 vs 浮点除法

            Console.WriteLine("--- TODO 1 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 2 ｜ 观察晋升：Gen0 → Gen1 → Gen2
            // ================================================================
            // 【要求】
            //   1. 先打印 GC.MaxGeneration（本运行时的最大代数）
            //   2. new 一个 object，立刻打印它在第几代
            //   3. 连续做 3 轮"强制 GC"（直接调用上面写好的辅助方法 ForceFullGc()），
            //      每轮结束后打印这个对象在第几代
            //
            // 【预期输出】
            //     最大代数 : Gen2
            //     刚 new   : Gen0
            //     第 1 次 GC → Gen1
            //     第 2 次 GC → Gen2
            //     第 3 次 GC → Gen2   ← 封顶了，不会变成 Gen3
            //
            // 【提示】
            //     Console.WriteLine($"最大代数 : Gen{GC.MaxGeneration}");
            //     var obj = new object();
            //     Console.WriteLine($"刚 new   : Gen{GC.GetGeneration(obj)}");
            //     for (int round = 1; round <= 3; round++)
            //     {
            //         ForceFullGc();
            //         Console.WriteLine($"第 {round} 次 GC → Gen{GC.GetGeneration(obj)}");
            //     }
            //
            // 【考察点】
            //   · 晋升是【单向】的，且 Gen2 是终点
            //   · 一个对象在 Gen2 意味着：每次 Gen2 回收都要扫描它 → 长期存活的大集合是重点怀疑对象
            //
            // 【★ 进阶思考（做完写一行注释就行）】
            //   如果循环里临时 new 一个 local object 做同样的事，结果会一样吗？为什么？

            Console.WriteLine("--- TODO 2 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 3 ｜ WeakReference：证明对象真的被回收了
            // ================================================================
            // 【要求】
            //   1. 在 Main 里 new 一个 Payload（Tag 用 "待回收"）
            //   2. 顺手 new 一个 WeakReference 指向它
            //   3. 打印第一次状态：weak.IsAlive（此时是 True，因为有强引用）
            //   4. 把强引用变量设为 null（写法：p = null!; 叹号是告诉编译器"我知道这里可能为空"）
            //   5. 调用 ForceFullGc()
            //   6. 再打印 weak.IsAlive 和 weak.Target
            //      · Target 为 null 时，输出"null（对象已被回收）"
            //
            // 【预期输出】
            //     有强引用时   : IsAlive = True
            //     断开引用并 GC: IsAlive = False, Target = null（对象已被回收）
            //
            // 【提示】
            //     var p = new Payload("待回收");
            //     var weak = new WeakReference(p);
            //     Console.WriteLine($"有强引用时   : IsAlive = {weak.IsAlive}");
            //     p = null!;
            //     ForceFullGc();
            //     string target = weak.Target is null ? "null（对象已被回收）" : "仍然存在";
            //     Console.WriteLine($"断开引用并 GC: IsAlive = {weak.IsAlive}, Target = {target}");
            //
            //     // ★ 泛型版写法也认一下（两种都对）：
            //     //   var weak2 = new WeakReference<Payload>(p);
            //     //   if (weak2.TryGetTarget(out Payload? got)) { ... } else { /* 已回收 */ }
            //
            // 【考察点】
            //   · WeakReference = 不阻止回收的引用；它不是根
            //   · 这是验证 GC 行为【最可靠】的手段 —— 比任何文档都可信
            //   · "不可达"（没根指向）和"已回收"（GC 真的跑过了）是两件事

            Console.WriteLine("--- TODO 3 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 4 ｜ LOH 门槛：85,000 字节到底怎么算
            // ================================================================
            // 【要求】
            //   1. 打印门槛值 85,000
            //   2. 分配四个数组，各自打印"字节数"和"所在代"：
            //        new byte[84_999]、new byte[85_000]、new byte[85_001]
            //        （第四个自己设计：用 int 数组，让它刚好也进 LOH）
            //   3. 先 ForceFullGc() 再打印代数，结果才稳定
            //   4. 输出后用一句话说明：为什么 byte[84_999] 和 byte[85_001] 差 2 个字节，
            //      代数却完全不一样
            //
            // 【预期输出】
            //     LOH 门槛 : 85,000 字节
            //     new byte[84,999]  → 84,999 字节  → Gen0   普通堆
            //     new byte[85,000]  → 85,000 字节  → Gen2   ★ LOH
            //     new byte[85,001]  → 85,001 字节  → Gen2   ★ LOH
            //     new int[21,251]   → 85,004 字节  → Gen2   ★ LOH（门槛看字节，不是元素个数）
            //
            // 【提示】
            //     const int LohThreshold = 85_000;
            //     var a = new byte[LohThreshold - 1];
            //     var b = new byte[LohThreshold];
            //     var c = new byte[LohThreshold + 1];
            //     var d = new int[(LohThreshold / 4) + 1];   // 21,251 个 int = 85,004 字节
            //     ForceFullGc();
            //     Console.WriteLine($"new byte[{a.Length:N0}] → {a.Length:N0} 字节 → Gen{GC.GetGeneration(a)}");
            //     // ★ 数组的"字节数"要自己算：元素个数 × 每个元素的字节数（sizeof(int) 是 4）
            //     // ★ 边界要以实测为准：数组还有对象头/对齐开销，各运行时版本的判定略有差异。
            //     //   想要严格证明而不是靠默认行为，可以换用配置开关：
            //     //     AppContext.SetSwitch("System.GC.LargeObjectHeapThreshold", 100_000);
            //
            // 【考察点】
            //   · 门槛是【字节总数】，不是元素个数
            //   · LOH 里的对象"一出生就是 Gen2"
            //   · LOH 默认【不压缩】→ 反复分配/释放不同大小的数组会产生碎片
            //     标准解法是 ArrayPool<T>（InternalsLab 实验 9 的 D 组）

            Console.WriteLine("--- TODO 4 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 5 ｜ ★ 综合（本课考核）：内存快照对比小工具
            // ================================================================
            // 【要求】写一个"压力测试 + 两次快照对比"的小工具，用上面给好的
            //        Capture(label) / PrintComparison(before, after) 两个方法。
            //
            //   1. 先 ForceFullGc()，然后 Capture("基准") 抓基准快照
            //   2. 造垃圾：循环 100,000 次，每次 new 一个 byte[32]，随手用一下
            //   3. 抓第二张快照 Capture("造完垃圾")，打印两张快照的对比
            //      → 重点看"堆大小"涨了多少、"累计分配"涨了多少
            //   4. 再 ForceFullGc()，抓第三张快照 Capture("回收后")，再对比一次
            //      → 重点看"堆大小"是不是掉回去了
            //   5. ★ 制造泄漏并证明它：调用 5 轮
            //         LeakedPayloads.Add(new Payload($"泄漏 #{i}"));  ForceFullGc();
            //      每轮之后打印 LeakedPayloads.Count、GC.GetTotalMemory(false) 和 LOH 大小
            //      （LOH 大小 = GC.GetGCMemoryInfo().GenerationInfo[GcSnapshot.LohIndex].SizeAfterBytes）
            //      → 观察：GC 跑了，堆却不降。因为静态字段是根，这些对象永远活着。
            //   6. 最后收尾：LeakedPayloads.Clear(); ForceFullGc(); 再抓一张快照，
            //      证明"把根断开之后，内存才真的还回来"
            //
            // 【预期输出】(节选，数字随机器变化)
            //     --- TODO 5 ---
            //     ========== 快照对比 1：造垃圾前 vs 造垃圾后 ==========
            //       指标                              前               后              变化
            //       ──────────────────────────────────────────────────────────────
            //       堆大小 HeapSize           12,345,678       15,678,901       3,333,223
            //       累计分配 Allocated         2,345,678        5,545,678       3,200,000
            //     ========== 快照对比 2：造垃圾后 vs 回收后 ==========
            //       堆大小 HeapSize           15,678,901       12,400,000      -3,278,901   ← 垃圾被收走了
            //     ========== 泄漏演示 ==========
            //     第 1 轮：Count=1  堆=13.3 MB  LOH=0.19 MB
            //     第 2 轮：Count=2  堆=13.5 MB  LOH=0.29 MB     ← GC 跑了，但堆只涨不降
            //     ...
            //     清空静态列表后：Count=0  堆=12.4 MB  LOH=0.00 MB  ← 终于还回来了
            //
            // 【提示】
            //     看 TODO 5 上方那两个辅助方法（Capture / PrintComparison）的实现，
            //     它们已经替你把 GCMemoryInfo 的字段取好了，你只要调用即可：
            //         var before = Capture("基准");
            //         var after = Capture("造完垃圾");
            //         PrintComparison(before, after);
            //     打印标题用：
            //         Console.WriteLine("========== 快照对比 1：造垃圾前 vs 造垃圾后 ==========");
            //     泄漏循环里注意：Payload 的构造参数是 Tag（字符串），别忘了把它用起来，
            //     否则编译器会警告"变量已赋值但从未使用"。
            //
            // 【考察点】
            //   · 把 4 个测量 API 串成一个能用的工具（GetTotalMemory / GetAllocatedBytes /
            //     CollectionCount / GetGCMemoryInfo）
            //   · ★ 理解"GC 跑了但内存不降"= 还有根引用着它 = 泄漏
            //   · 学会用"两次快照做差"而不是"看绝对值"，因为绝对值里有噪声

            Console.WriteLine("--- TODO 5 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
