namespace LessonPractice
{
    // ============================================================================
    //  第 19 课 · 示例：垃圾回收（GC）
    //
    //  ★ 本示例的目的不是"算出什么东西"，而是让你【亲眼看到】GC 在工作：
    //      · 对象什么时候变成垃圾（不可达）
    //      · 垃圾什么时候真的没（GC 跑的时候 —— 不是"立刻"）
    //      · 对象怎么从 Gen0 晋升到 Gen2
    //      · 大对象堆（LOH）的门槛
    //      · 怎么用 WeakReference【证明】对象真的被回收了
    //
    //  ★ 运行方式：
    //      1. 打开 第19课.sln，把 Lesson19Demo 设为启动项目，Ctrl + F5
    //      2. 或者命令行：dotnet run --project Lesson19Demo
    //
    //  ★ 运行前请先读课文《第19课.md》的第五、六、七节，
    //    否则你会看不懂为什么有的地方"必须"调用 GC.Collect()。
    //
    //  ⚠️ 本示例里出现的 GC.Collect() 全部是为了【演示】。
    //     生产代码永远不要调用它 —— 理由见示例最后一段。
    // ============================================================================
    class Lesson19Demo
    {
        // ========================================================================
        //  本课用到的辅助成员
        // ========================================================================

        /// <summary>
        /// 一个"大的"托管对象：里面挂着一个 100,000 字节的数组。
        /// ★ 100,000 大于 85,000，所以这个数组会进【大对象堆（LOH）】——
        ///   这样它在 GC 前后的变化更容易观察。
        /// </summary>
        private class BigObject
        {
            // ★ 字段给了初值，就不会触发 CS8618（不可空字段未初始化）警告
            public byte[] Payload = new byte[100_000];
            public string Tag = "";

            public BigObject(string tag) => Tag = tag;
        }

        /// <summary>
        /// 循环引用演示用的节点：A 指向 B，B 指向 A，形成环。
        /// </summary>
        private class Node
        {
            public string Name = "";
            public Node? Next;              // ★ 可空字段必须显式写成 Node?
        }

        /// <summary>
        /// 把弱引用翻译成一句看得懂的中文。
        /// ★ 非泛型 WeakReference 的写法：先看 IsAlive，再看 Target。
        ///   （泛型版 WeakReference&lt;T&gt; 是 TryGetTarget(out T target)，见课文第九节）
        /// </summary>
        private static string Status(WeakReference weak)
        {
            if (!weak.IsAlive)
            {
                return "已回收（IsAlive = False, Target = null）";
            }
            return $"还活着（Target = {weak.Target}）";
        }

        /// <summary>
        /// 在方法【内部】创建对象，只把弱引用交出去，强引用随方法返回一起消失。
        /// ★ 关键是：方法返回后，调用方【没有任何强引用】指向这个对象。
        ///   它"不可达"了 —— 但注意，不可达不等于已经被回收。
        /// </summary>
        private static WeakReference CreateAndDrop()
        {
            var obj = new BigObject("临时大对象");
            Console.WriteLine($"  在方法内创建：Tag = {obj.Tag}，载荷 {obj.Payload.Length:N0} 字节");
            return new WeakReference(obj);
        }

        /// <summary>
        /// 打印当前堆的四个关键数字。
        /// ★ forceFullCollection: true 表示"先跑一次完整的 GC，再报数"——
        ///   这样报出来的是【存活对象】的大小，也就是"真实占用"。
        ///   false 则是"现在堆里有多少"，【包含还没被收走的垃圾】。
        /// </summary>
        private static void ShowHeap(string label)
        {
            var info = GC.GetGCMemoryInfo();
            Console.WriteLine($"  [{label}]");
            Console.WriteLine($"    存活堆大小  : {GC.GetTotalMemory(true) / 1024.0 / 1024,7:N2} MB   ← 强制 GC 之后还在的对象");
            Console.WriteLine($"    当前堆大小  : {info.HeapSizeBytes / 1024.0 / 1024,7:N2} MB   ← 含尚未回收的垃圾");
            Console.WriteLine($"    碎片化字节  : {info.FragmentedBytes / 1024.0 / 1024,7:N2} MB");
            Console.WriteLine($"    已提交内存  : {info.TotalCommittedBytes / 1024.0 / 1024,7:N2} MB   ← 向操作系统要来的");
            Console.WriteLine($"    GC 次数     : Gen0={GC.CollectionCount(0)}, Gen1={GC.CollectionCount(1)}, Gen2={GC.CollectionCount(2)}");
        }

        static void Main()
        {
            Console.WriteLine("===== 第 19 课：垃圾回收（GC）=====");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第一段：先建立"分配 / 垃圾 / 回收"三个直觉
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 一、分配、垃圾、回收：三个数字 ----------");
            Console.WriteLine();

            // ★ GC.GetAllocatedBytesForCurrentThread() = 本线程【从进程启动到现在】累计分配了多少字节。
            //   它是一个单调递增的计数器：只统计"分配"，不减去"回收"。
            //   想知道"这段代码产生了多少垃圾"，就用前后差值 —— 这是本课最有用的测量手段。
            long allocStart = GC.GetAllocatedBytesForCurrentThread();
            long memStart = GC.GetTotalMemory(false);
            Console.WriteLine($"  基准：本线程已累计分配 {allocStart,12:N0} 字节，当前堆 {memStart / 1024.0,10:N1} KB");
            Console.WriteLine();

            // 造 200,000 个短命字符串，全部只存在一瞬间
            const int junkCount = 200_000;
            long junkChars = 0;
            for (int i = 0; i < junkCount; i++)
            {
                // ★ 这个字符串下一轮循环就没人引用了 → 立刻变成垃圾
                string junk = $"第 {i} 号垃圾对象，这个字符串很长很长很长很长很长很长很长很长";
                junkChars += junk.Length;
            }

            long allocated = GC.GetAllocatedBytesForCurrentThread() - allocStart;

            Console.WriteLine($"  循环 {junkCount:N0} 次，每次造一个字符串（共 {junkChars:N0} 个字符）");
            Console.WriteLine($"  → 本线程新增分配 : {allocated,12:N0} 字节  （约 {allocated / 1024.0 / 1024,6:N2} MB）");
            Console.WriteLine($"  → 平均每个字符串 : {allocated / (double)junkCount,12:N1} 字节");
            Console.WriteLine();

            // ★ 注意这里用的是 false（不强制回收）：
            //   这些字符串已经不可达了，但它们占的地方还在 —— 这就是"垃圾"。
            long memAfterJunk = GC.GetTotalMemory(false);
            Console.WriteLine($"  此时堆大小：{memAfterJunk / 1024.0,10:N1} KB（比基准多了 {(memAfterJunk - memStart) / 1024.0:N1} KB）");
            Console.WriteLine("  ★ 这些多出来的内存里，绝大部分是【已经没人引用的垃圾】。");
            Console.WriteLine("    它们还占着地方，只是因为 GC 还没被触发。");
            Console.WriteLine();

            // 现在强制回收一次，看堆缩回去
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memAfterCollect = GC.GetTotalMemory(false);
            Console.WriteLine($"  强制 GC 之后堆大小：{memAfterCollect / 1024.0,10:N1} KB");
            Console.WriteLine($"  → 回收掉了约 {(memAfterJunk - memAfterCollect) / 1024.0:N1} KB");
            Console.WriteLine();
            Console.WriteLine("  ★ 三句话总结这一段：");
            Console.WriteLine("     ① 分配很便宜（只是移动一个指针）");
            Console.WriteLine("     ② 不可达不等于已回收（垃圾要等 GC 跑）");
            Console.WriteLine("     ③ 想知道\"我产生了多少垃圾\"，用 GetAllocatedBytesForCurrentThread 求差");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第二段：可达性分析 —— 谁才是 GC 的"根"
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 二、可达性分析：从根出发找活人 ----------");
            Console.WriteLine();

            WeakReference weak = CreateAndDrop();
            Console.WriteLine($"  方法已返回，此时对象：{Status(weak)}");
            Console.WriteLine();
            Console.WriteLine("  ★ 关键：它已经【不可达】了（没有任何根指向它），");
            Console.WriteLine("    但 IsAlive 很可能还是 True —— 因为 GC 还没跑。");
            Console.WriteLine("    这就是\"不可达\"和\"已回收\"的区别，也是 C# 内存曲线");
            Console.WriteLine("    看起来像\"锯齿\"而不是\"平的\"的原因。");
            Console.WriteLine();
            Console.WriteLine("  现在调用 GC.Collect() ...");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Console.WriteLine($"  → 此时对象：{Status(weak)}");
            Console.WriteLine();

            // ★ GC.KeepAlive 演示：强引用存在时，GC 拿它没办法
            var strong = new BigObject("被强引用按住");
            var weakStrong = new WeakReference(strong);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine($"  有强引用时：{Status(weakStrong)}");
            Console.WriteLine("  ★ 因为 strong 这个【局部变量】就是 GC 的根之一。");

            // ★ GC.KeepAlive(obj) 的作用：告诉 JIT"到这里为止，obj 都算活着的"。
            //   没有它，JIT 可能在变量"最后一次被使用"之后就认为它死了 ——
            //   于是下面这次 GC 可能在我们还没检查之前就把对象收走（Debug 下通常不会，
            //   但 Release 下、方法被内联时就会）。加 KeepAlive 是为了让结论稳定可复现。
            GC.KeepAlive(strong);

            strong = null!;                      // ★ 对可空引用类型来说，null! 的意思是"我知道这里可能为空，别警告我"
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Console.WriteLine($"  断开强引用后：{Status(weakStrong)}");
            Console.WriteLine();
            Console.WriteLine("  ★ 这就是\"根\"的威力：同一个对象，有根指着它 → 活着；根断了 → 变成垃圾。");
            Console.WriteLine("    根 = 栈上的局部变量 + 静态字段 + CPU 寄存器 + GC Handle（第十节汇总）。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第三段：标记 → 清除 → 压缩
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 三、标记 → 清除 → 压缩 ----------");
            Console.WriteLine();

            ShowHeap("回收之前");

            // ★ 把大对象放进一个数组：数组本身是根，但数组里每个格子都可以是 null
            var garbageBag = new BigObject?[30];
            for (int i = 0; i < garbageBag.Length; i++)
            {
                garbageBag[i] = new BigObject($"第 {i} 号");
            }
            long memWithBag = GC.GetTotalMemory(false);
            Console.WriteLine($"  刚分配 30 个 100KB 的大对象，堆 = {memWithBag / 1024.0 / 1024:N2} MB");
            Console.WriteLine();

            Console.WriteLine("  ① 标记阶段：从根出发遍历，把能走到的对象打上\"活着\"标记");
            Console.WriteLine("  ② 清除阶段：没有标记的对象，它们占的内存被【记为空闲】");
            Console.WriteLine("  ③ 压缩阶段：把活着的对象往一头挪，把空隙挤掉（移动 = 改引用地址）");
            Console.WriteLine();

            for (int i = 0; i < garbageBag.Length; i++)
            {
                garbageBag[i] = null;            // ★ 断开引用 → 它们全部变成垃圾
            }
            Console.WriteLine("  已把 30 个格子全部设为 null（对象变成垃圾，但还占着地方）");
            Console.WriteLine($"  当前堆 = {GC.GetTotalMemory(false) / 1024.0 / 1024:N2} MB  ← 数字没变，垃圾还在");
            Console.WriteLine();

            Console.WriteLine("  执行 GC.Collect()（强制一次完整回收）...");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Console.WriteLine($"  回收后堆 = {GC.GetTotalMemory(false) / 1024.0 / 1024:N2} MB  ← 垃圾被清掉、活对象被压缩");
            Console.WriteLine();

            ShowHeap("回收之后");
            Console.WriteLine();
            Console.WriteLine("  ★ 注意 FragmentedBytes（碎片化字节）：压缩会把碎片挤掉。");
            Console.WriteLine("    但大对象堆（LOH）默认【不压缩】—— 它的碎片会留下来，后面第五段会说。");
            Console.WriteLine();
            Console.WriteLine("  ★ 为什么\"压缩\"值得做？因为下一段你就知道：分配只需要移动一个指针，");
            Console.WriteLine("    前提是空闲空间必须连成【一大片】。碎片会让分配变慢。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第四段：分代 —— Gen0 / Gen1 / Gen2 与"晋升"
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 四、分代：Gen0 → Gen1 → Gen2 ----------");
            Console.WriteLine();

            Console.WriteLine($"  本运行时支持的最大代数：Gen{GC.MaxGeneration}");
            Console.WriteLine("  （0 = 最新，回收最频繁；2 = 最老，回收最稀疏）");
            Console.WriteLine();

            var genObj = new object();
            Console.WriteLine($"  刚 new 出来的对象在 Gen{GC.GetGeneration(genObj)}");
            for (int round = 1; round <= 3; round++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Console.WriteLine($"  第 {round} 次 GC 之后 → Gen{GC.GetGeneration(genObj)}");
            }
            Console.WriteLine("  ★ 晋升是【单向】的：Gen0 → Gen1 → Gen2，到 Gen2 就封顶，永不回头。");
            Console.WriteLine("    所以 Gen2 里放的都是\"经历过至少两次大扫除还活着\"的老油条。");
            Console.WriteLine();
            Console.WriteLine("  ★ 分代假设：\"绝大多数对象活不过第一次 GC\"。");
            Console.WriteLine("    既然新对象大多是垃圾，那就【只扫 Gen0】—— 一次回收只碰一小块内存，极快。");
            Console.WriteLine("    这就是为什么你的程序分配了几百万个对象，Gen0 回收了几十次，");
            Console.WriteLine("    而你几乎感觉不到卡顿。");
            Console.WriteLine();

            Console.WriteLine("  各代堆的实测大小：");
            var gi = GC.GetGCMemoryInfo();
            for (int g = 0; g <= GC.MaxGeneration; g++)
            {
                // ★ {0,12:N0} = 右对齐占 12 格 + 千分位分隔（第 13 课表格对齐的写法）
                Console.WriteLine($"    Gen{g} : {gi.GenerationInfo[g].SizeAfterBytes,12:N0} 字节");
            }
            Console.WriteLine($"    LOH  : {gi.GenerationInfo[GC.MaxGeneration + 1].SizeAfterBytes,12:N0} 字节");
            Console.WriteLine("  ★ 注意 GenerationInfo[MAX+1] 才是大对象堆 —— 它不算\"第 3 代\"，");
            Console.WriteLine("    但它在 Gen2 回收时被【顺便】一起处理（因为它也算\"老\"）。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第五段：大对象堆（LOH）与 85,000 字节门槛
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 五、大对象堆（LOH）与 85,000 字节门槛 ----------");
            Console.WriteLine();

            const int LohThreshold = 85_000;
            Console.WriteLine($"  门槛：{LohThreshold:N0} 字节（即 83.0 KB 左右）");
            Console.WriteLine();

            // ★ 注意：这里的 85000 是【字节总数】，不是元素个数。
            //   new byte[85000] 正好 85,000 字节 → 踩在门槛上（>= 就进 LOH）。
            var small = new byte[LohThreshold - 1];      // 84,999 字节
            var large = new byte[LohThreshold + 1];      // 85,001 字节
            var smallInts = new int[10_000];             // 40,000 字节
            var largeInts = new int[100_000];            // 400,000 字节

            GC.Collect();
            GC.WaitForPendingFinalizers();

            // ★ 先把"代数"算成字符串：插值表达式里再套字符串字面量（比如 {"Gen" + ...}）
            //   虽然合法，但引号一多就很难读 —— 拆成变量是最省心的写法。
            string genSmall = "Gen" + GC.GetGeneration(small);
            string genLarge = "Gen" + GC.GetGeneration(large);
            string genSmallInts = "Gen" + GC.GetGeneration(smallInts);
            string genLargeInts = "Gen" + GC.GetGeneration(largeInts);

            // ★ {0,-24} = 占 24 格【左】对齐；{1,14} = 占 14 格【右】对齐（第 13 课的表格对齐写法）
            Console.WriteLine($"  {"对象",-24}{"字节数",14}  {"所在代",-8}  结论");
            Console.WriteLine("  " + new string('─', 72));
            Console.WriteLine($"  {"new byte[84,999]",-24}{small.Length,14:N0}  {genSmall,-8}  普通堆");
            Console.WriteLine($"  {"new byte[85,001]",-24}{large.Length,14:N0}  {genLarge,-8}  ★ 直接进 LOH");
            Console.WriteLine($"  {"new int[10,000]",-24}{smallInts.Length * 4,14:N0}  {genSmallInts,-8}  普通堆");
            Console.WriteLine($"  {"new int[100,000]",-24}{largeInts.Length * 4,14:N0}  {genLargeInts,-8}  ★ 直接进 LOH");
            Console.WriteLine();
            Console.WriteLine("  ★ 两个容易记错的点：");
            Console.WriteLine("     ① 门槛看的是【字节总数】，不是元素个数。int 要 21,251 个才够 85,004 字节。");
            Console.WriteLine("     ② 大对象\"一出生就是 Gen2\"，因为它直接进 LOH，而 LOH 由 Gen2 回收顺带处理。");
            Console.WriteLine();
            Console.WriteLine("  ★ LOH 的两个大问题：");
            Console.WriteLine("     ① 默认【不压缩】→ 反复分配/释放不同大小的数组 → 碎片");
            Console.WriteLine("     ② 回收代价高（一大块内存要整体处理），而且它总跟着 Gen2 一起被处理");
            Console.WriteLine("     解法：需要反复用的大数组，用 ArrayPool<T> 复用（见 InternalsLab 实验 9 的实验 D）");
            Console.WriteLine();

            // ★ GC.GetGCMemoryInfo() 给的碎片数字是【整堆】的，不是单独 LOH 的。
            var frag = GC.GetGCMemoryInfo();
            double fragPercent = frag.HeapSizeBytes > 0
                ? frag.FragmentedBytes * 100.0 / frag.HeapSizeBytes
                : 0;
            Console.WriteLine($"  当前整堆碎片化字节：{frag.FragmentedBytes:N0} 字节（占堆的 {fragPercent:F1}%）");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第六段：为什么托管堆分配这么快
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 六、为什么托管堆分配这么快？----------");
            Console.WriteLine();

            const int batch = 200_000;
            long beforeLoop = GC.GetAllocatedBytesForCurrentThread();
            long t0 = Environment.TickCount64;
            for (int i = 0; i < batch; i++)
            {
                var bytes = new byte[24];        // 24 字节的小对象
                GC.KeepAlive(bytes);             // ★ 防止 JIT 觉得"反正没人用"而干脆不分配
            }
            long elapsedMs = Environment.TickCount64 - t0;
            long loopAllocated = GC.GetAllocatedBytesForCurrentThread() - beforeLoop;

            Console.WriteLine($"  new byte[24] 连续分配 {batch:N0} 个：");
            Console.WriteLine($"    总耗时      : {elapsedMs,10} ms");
            Console.WriteLine($"    新分配字节  : {loopAllocated,10:N0} 字节");
            Console.WriteLine($"    平均每个    : {loopAllocated / (double)batch,10:N1} 字节  ← 24 字节数据 + 对象头 + 对齐");
            Console.WriteLine($"    GC 次数     : Gen0={GC.CollectionCount(0)}（分配这么多，才触发了几次 Gen0）");
            Console.WriteLine();
            Console.WriteLine("  ★ 托管堆的分配 = 移动一个\"分配指针\"（bump pointer allocation）：");
            Console.WriteLine("       next = next + size;   return next;");
            Console.WriteLine("    这在机器码层面大约就是【一条加法指令】—— 比 malloc 快得多。");
            Console.WriteLine("    代价是：空闲空间必须【连续】，所以回收时要做\"压缩\"把空隙挤掉。");
            Console.WriteLine();
            Console.WriteLine("  ★ 对比一下直觉：");
            Console.WriteLine("     C/C++ 的 malloc：在空闲链表里找一块够大的 → 可能找不到 → 系统调用");
            Console.WriteLine("     C# 的托管堆    ：指针往后挪一格 → 一定成功 → 不够了才触发 GC");
            Console.WriteLine("     这就是\"C# 分配对象几乎不要钱\"的真正原因。");
            Console.WriteLine();
            Console.WriteLine("  ⚠️ 但注意这个恒等式：分配快 不等于 回收免费。");
            Console.WriteLine("     你分配得越多，GC 来得越勤。真正的性能原则是【少产生垃圾】，不是【不怕分配】。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第七段：循环引用 —— C# 和 Python 最大的分歧
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 七、循环引用会被回收吗？（对比 Python）----------");
            Console.WriteLine();

            Node nodeA = new Node { Name = "A" };
            Node nodeB = new Node { Name = "B" };
            nodeA.Next = nodeB;
            nodeB.Next = nodeA;                  // 互相引用 → 形成环

            var weakA = new WeakReference(nodeA);
            var weakB = new WeakReference(nodeB);
            Console.WriteLine("  建好环：A.Next = B，B.Next = A。");
            Console.WriteLine();

            nodeA = null!;
            nodeB = null!;                       // ★ 断开外部引用：现在环里谁也出不来，外面也进不去
            Console.WriteLine("  断开外部引用（A、B 两个局部变量置空）——");
            Console.WriteLine("  在 Python 里，此刻两个对象的引用计数都是 1，它们【互相保命】，谁都死不掉。");
            Console.WriteLine();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Console.WriteLine("  C# 强制 GC 之后：");
            Console.WriteLine($"    A 的状态：{Status(weakA)}");
            Console.WriteLine($"    B 的状态：{Status(weakB)}");
            Console.WriteLine();
            Console.WriteLine("  ★ C# 直接把两个都收了！因为追踪式 GC 从【根】出发，");
            Console.WriteLine("    根本走不进这个环 —— 环里的对象既不是根，也不被根指向。");
            Console.WriteLine("    \"环内部互相引用\"在追踪式 GC 眼里毫无意义。");
            Console.WriteLine();
            Console.WriteLine("  ★ Python 就不一样了（引用计数 + 循环检测器）：");
            Console.WriteLine("      · 环里每个对象的引用计数都 >= 1 → 引用计数回收永远轮不到它们");
            Console.WriteLine("      · 必须靠 gc 模块的\"循环检测器\"定期扫描才能发现并回收");
            Console.WriteLine("      · 所以 Python 里 __del__ 在循环引用中可能【永远不被调用】");
            Console.WriteLine();
            Console.WriteLine("  ★ 这就是两种方案的取舍：");
            Console.WriteLine("     Python：不可达通常【立刻】释放（引用计数归零的瞬间）→ 内存曲线平滑、时机可预测");
            Console.WriteLine("             代价：每次赋值都要改计数（多线程要加锁）、处理不了环、有额外内存开销");
            Console.WriteLine("     C#    ：释放时机【不确定】（等 GC）→ 内存曲线呈锯齿、可能有停顿");
            Console.WriteLine("             代价换来了：分配极快、天然处理环、没有计数开销、能压缩内存");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第八段：关键 API 速览（附带真实数字）
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 八、五个关键 API 的实测数字 ----------");
            Console.WriteLine();

            var memInfo = GC.GetGCMemoryInfo();
            Console.WriteLine("  ① GC.GetAllocatedBytesForCurrentThread()  本线程累计【分配】了多少字节");
            Console.WriteLine($"       → {GC.GetAllocatedBytesForCurrentThread():N0} 字节（进程启动到现在的累计值）");
            Console.WriteLine("       用途：测\"这段代码产生多少垃圾\"。只看当前线程，多线程要各线程分别测。");
            Console.WriteLine();
            Console.WriteLine("  ② GC.GetTotalMemory(forceFullCollection)  当前【堆】有多大");
            Console.WriteLine($"       → false（先不回收，直接报）：{GC.GetTotalMemory(false) / 1024.0 / 1024,8:N2} MB");
            Console.WriteLine($"       → true （先强制一次完整 GC，再报）：{GC.GetTotalMemory(true) / 1024.0 / 1024,8:N2} MB");
            Console.WriteLine("       注意：true 会【阻塞】当前线程直到 GC 完成 —— 测试代码里用它，");
            Console.WriteLine("             热路径里千万不要用，它比 GC.Collect() 还阴险，因为它藏在一个看起来无害的调用里。");
            Console.WriteLine();
            Console.WriteLine("  ③ GC.CollectionCount(generation)  第 N 代被回收了多少次");
            Console.WriteLine($"       → Gen0={GC.CollectionCount(0)}, Gen1={GC.CollectionCount(1)}, Gen2={GC.CollectionCount(2)}");
            Console.WriteLine("       用途：判断\"我的代码是不是在疯狂制造垃圾\"。Gen0 增长极快 = 分配太猛。");
            Console.WriteLine();
            Console.WriteLine("  ④ GC.GetGCMemoryInfo()  堆的详细体检报告");
            Console.WriteLine($"       → HeapSizeBytes        : {memInfo.HeapSizeBytes,14:N0} 字节");
            Console.WriteLine($"       → TotalCommittedBytes  : {memInfo.TotalCommittedBytes,14:N0} 字节   （向 OS 要来的）");
            Console.WriteLine($"       → FragmentedBytes      : {memInfo.FragmentedBytes,14:N0} 字节");
            Console.WriteLine($"       → PauseDurations 条数  : {memInfo.PauseDurations.Length}");
            Console.WriteLine("       用途：排查内存问题。PauseDurations 记录各次 GC 造成的停顿（GC 停顿就是这么量的）。");
            Console.WriteLine();
            Console.WriteLine("  ⑤ GC.KeepAlive(obj)  把对象的\"死亡时间\"推迟到这一行");
            Console.WriteLine("       用途：让\"对象还活着\"这件事在测量点上确定成立，避免 JIT 提前判定它死了。");
            Console.WriteLine("       本示例第二段就用它保证了\"有强引用 → 一定不会被回收\"这个结论稳定可复现。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第九段：⚠️ 关于 GC.Collect() 的警告
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 九、⚠️ GC.Collect() 只在测试和诊断里用 ----------");
            Console.WriteLine();
            Console.WriteLine("  你在这个示例里看到的每一次 GC.Collect()，都是为了\"让现象立刻发生\"，");
            Console.WriteLine("  因为默认情况下 GC 什么时候跑是【运行时决定的】，你不该也不能依赖它。");
            Console.WriteLine();
            Console.WriteLine("  为什么生产代码永远不要调用它：");
            Console.WriteLine("    ① 它强制一次【完整】回收（Gen2 + LOH），代价比一次普通 Gen0 回收高几十倍");
            Console.WriteLine("    ② 它打断 GC 自己的节奏：本来 GC 会挑\"合适的时机\"（比如 Gen0 满了）");
            Console.WriteLine("    ③ 它造成【停顿】（stop-the-world），在服务端就是请求延迟尖刺（长尾延迟）");
            Console.WriteLine("    ④ 它几乎从不是解药：如果你在为内存发愁，真问题是\"谁还引用着这些对象\"");
            Console.WriteLine("    ⑤ GC 是自适应的：它观察你的分配速率来调整策略，你手动插一脚反而让它判断失准");
            Console.WriteLine();
            Console.WriteLine("  ★ 那什么时候【可以】调用？");
            Console.WriteLine("     · 单元测试：要断言\"对象确实被回收了\"（配合 WeakReference）");
            Console.WriteLine("     · 诊断工具 / 内存快照前：先 GC 两次把垃圾清干净，快照才是\"存活对象\"");
            Console.WriteLine("     · 以上就是全部例外，其他场合一律交给运行时。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第十段：GC 的根清单 + 本课小结
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 十、GC 的根有哪些 ----------");
            Console.WriteLine();
            Console.WriteLine("  只要对象能从【任意一个根】走到，它就活着：");
            Console.WriteLine();
            Console.WriteLine("    根                                 本示例里的例子");
            Console.WriteLine("    ─────────────────────────────────  ──────────────────────────────");
            Console.WriteLine("    · 栈上的局部变量 / 方法参数         strong 变量（第二段）");
            Console.WriteLine("    · CPU 寄存器里暂存的引用            JIT 把引用放进寄存器时（你看不见）");
            Console.WriteLine("    · 静态字段                         static List<Enemy> _allEnemies");
            Console.WriteLine("    · GC Handle（GCHandle / 固定指针）  与非托管代码交互时（第 22 课）");
            Console.WriteLine("    · 终结队列里的对象（第 20 课）       有待跑终结器的对象");
            Console.WriteLine();
            Console.WriteLine("  ★ 最实用的一条推论：");
            Console.WriteLine("    \"静态字段是根\" 意味着 \"静态字段会造成内存泄漏\"。");
            Console.WriteLine("    因为只要类还活着，静态字段指向的对象就永远不会被回收。");
            Console.WriteLine("    静态集合只 Add 不 Remove，就是永久的泄漏 —— Unity 里切场景最常见的坑。");
            Console.WriteLine();
            Console.WriteLine("  ★ 想亲手验证？打开 InternalsLab（解决方案 InternalsLab.sln）：");
            Console.WriteLine("     菜单 8 = WeakReference（E08_WeakReference.cs）：强引用 / 静态字段 / 循环引用四组对照");
            Console.WriteLine("     菜单 9 = 分代与晋升（E09_GenerationsAndGc.cs）：晋升、LOH、ArrayPool 四组实验");
            Console.WriteLine("    本示例是它们的\"精编版 + 中文逐行讲解\"，两者对照看效果最好。");
            Console.WriteLine();

            // ★ 收尾：把弱引用变量也断开，避免你误以为它们"持有"对象
            weak = null!;
            weakStrong = null!;
            weakA = null!;
            weakB = null!;
            Console.WriteLine("  （示例收尾：弱引用变量本身不持有对象，断开它们只是为了让这个意图更清楚）");
            Console.WriteLine();

            Console.WriteLine("===== 示例结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
