namespace Experiments;

/// <summary>
/// 第 6 周 · 实验 9：分代、晋升与大对象堆
/// </summary>
/// <remarks>
/// 结论：
///   1. 分代假设：「绝大多数对象活不过第一次 GC」。所以新对象（Gen0）检查频繁，
///      老对象（Gen2）检查稀疏 —— 这样每次 GC 只需扫描一小部分堆。
///   2. 经过一次 GC 仍存活的对象会「晋升」到下一代。Gen2 是终点。
///   3. 小于 85000 字节的对象走普通堆；大于等于则直接进大对象堆（LOH，属 Gen2）。
///   4. LOH 默认【不压缩】，反复分配/释放不同大小的数组会造成碎片。
///   5. 用 ArrayPool<T> 复用大数组是标准解法。
/// </remarks>
public static class E09_GenerationsAndGc
{
    private const int LohThreshold = 85_000;

    public static void Run()
    {
        Console.WriteLine("【实验 A：观察对象晋升】");
        Console.WriteLine();

        var obj = new object();
        Console.WriteLine($"  new object()          → Gen{GC.GetGeneration(obj)}");

        for (int i = 1; i <= 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.WriteLine($"  第 {i} 次 GC.Collect() 后 → Gen{GC.GetGeneration(obj)}");
        }
        Console.WriteLine();
        Console.WriteLine("  ★ 晋升是单向的，到 Gen2 就封顶。");
        Console.WriteLine("  ★ 一个对象在 Gen2 意味着它每次 Gen2 GC 都要被扫描一遍。");
        Console.WriteLine("    所以「长期存活的大集合」是内存分析里的重点怀疑对象。");
        Console.WriteLine();

        Console.WriteLine("【实验 B：观察各代 GC 的触发频率】");
        Console.WriteLine();

        ResetCounters();

        Console.WriteLine("  分配 100 万个短命对象（模拟正常业务）：");
        for (int i = 0; i < 1_000_000; i++)
        {
            var temp = new byte[64];              // 立刻变成垃圾
            if (i % 250_000 == 0)
                Report($"    i={i,9:N0}");
        }
        Report("    结束       ");
        Console.WriteLine();
        Console.WriteLine("  ★ 观察到：Gen0 回收了几十次，Gen1 寥寥几次，Gen2 通常 0 次。");
        Console.WriteLine("    这就是分代的价值：绝大多数回收只扫描 Gen0，非常快。");
        Console.WriteLine();

        Console.WriteLine("【实验 C：大对象堆（LOH）】");
        Console.WriteLine();

        ResetCounters();

        Console.WriteLine($"  LOH 阈值：{LohThreshold:N0} 字节");
        Console.WriteLine();

        var small = new byte[LohThreshold - 1];
        var large = new byte[LohThreshold + 1];

        GC.Collect();
        GC.WaitForPendingFinalizers();

        Console.WriteLine($"  new byte[{LohThreshold - 1:N0}]  → Gen{GC.GetGeneration(small)}   （普通堆）");
        Console.WriteLine($"  new byte[{LohThreshold + 1:N0}]  → Gen{GC.GetGeneration(large)}   （LOH，直接是 Gen2）");
        Console.WriteLine();

        Report("  分配大对象前");
        Console.WriteLine("  现在分配 50 个 1MB 的数组...");
        var list = new List<byte[]>();
        for (int i = 0; i < 50; i++)
            list.Add(new byte[1024 * 1024]);
        Report("  分配后      ");
        Console.WriteLine();

        var info = GC.GetGCMemoryInfo();
        Console.WriteLine($"  堆总大小     : {info.HeapSizeBytes / 1024.0 / 1024,8:N1} MB");
        Console.WriteLine($"  碎片化字节数 : {info.FragmentedBytes / 1024.0 / 1024,8:N2} MB");
        Console.WriteLine($"  已提交内存   : {info.TotalCommittedBytes / 1024.0 / 1024,8:N1} MB");
        Console.WriteLine($"  已分配的对象 : {info.HeapSizeBytes:N0} 字节");
        Console.WriteLine();

        Console.WriteLine("  ★ 注意：即使你在托管代码里，进程占用的物理内存（任务管理器）");
        Console.WriteLine("    只增不减 —— 因为 GC 回收了对象，但【很少把内存还给操作系统】，");
        Console.WriteLine("    它留着自己复用。这是正常的，不是内存泄漏。");
        Console.WriteLine();

        Console.WriteLine("【实验 D：ArrayPool 复用大数组（LOH 的标准解法）】");
        Console.WriteLine();

        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        ResetCounters();

        const int iterations = 2_000;
        const int bufferSize = 200_000;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        long allocated;
        var allocBefore = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < iterations; i++)
        {
            var buf = new byte[bufferSize];       // ❌ 每次都在 LOH 上分配
            buf[0] = 1;
            GC.KeepAlive(buf);
        }
        allocated = GC.GetAllocatedBytesForCurrentThread() - allocBefore;
        sw.Stop();
        var naiveMs = sw.Elapsed.TotalMilliseconds;
        var naiveAlloc = allocated;
        var naiveGc = GC.CollectionCount(2);

        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        ResetCounters();

        var pool = System.Buffers.ArrayPool<byte>.Shared;
        allocBefore = GC.GetAllocatedBytesForCurrentThread();
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var buf = pool.Rent(bufferSize);      // ✅ 从池里借
            try
            {
                buf[0] = 1;
                GC.KeepAlive(buf);
            }
            finally
            {
                pool.Return(buf);                 // 归还，下次复用
            }
        }
        allocated = GC.GetAllocatedBytesForCurrentThread() - allocBefore;
        sw.Stop();
        var pooledMs = sw.Elapsed.TotalMilliseconds;
        var pooledAlloc = allocated;
        var pooledGc = GC.CollectionCount(2);

        Console.WriteLine($"  {"方式",-22} {"耗时",10} {"分配",14} {"Gen2 GC",9}");
        Console.WriteLine("  " + new string('─', 58));
        Console.WriteLine($"  {"new byte[]（朴素）",-22} {naiveMs,8:F1} ms {naiveAlloc / 1024.0 / 1024,11:N1} MB {naiveGc,9}");
        Console.WriteLine($"  {"ArrayPool.Rent（池化）",-22} {pooledMs,8:F1} ms {pooledAlloc / 1024.0 / 1024,11:N1} MB {pooledGc,9}");
        Console.WriteLine();
        Console.WriteLine($"  → 分配量降低到 1/{Math.Max(1, naiveAlloc / Math.Max(1, pooledAlloc))} 左右");
        Console.WriteLine("  ★ 池化的代价：借来的数组【内容不保证清零】，长度可能大于请求值，");
        Console.WriteLine("    所以要用 Rent 返回的实际长度，而不是你请求的长度。");
        Console.WriteLine();

        Console.WriteLine("【实验 E：ArrayPool 的坑（借来的数组是脏的）】");
        Console.WriteLine();
        var rented = pool.Rent(100);
        Console.WriteLine($"  请求 100 字节，实际拿到 {rented.Length} 字节（池会给你更大的）");
        Console.WriteLine($"  前 10 个字节的内容: {string.Join(",", rented.Take(10))}   ← 可能是残留数据！");
        Console.WriteLine("  ★ 需要干净的数组时：pool.Rent(size, clearArray: true) 或 Array.Clear");
        pool.Return(rented);
        Console.WriteLine();

        Console.WriteLine("【动手任务】");
        Console.WriteLine("  1. 在 Rider 的 Memory 视图里，跑实验 C 然后抓两次堆快照对比");
        Console.WriteLine("  2. 思考：Unity 里 GameManager 长期持有一个 List<Enemy>，");
        Console.WriteLine("     敌人死了只 Remove 不 Clear，会发生什么？（提示：晋升到 Gen2 + 碎片）");
        Console.WriteLine("  3. 用 dotnet-counters 监控一个控制台程序的 GC 频率（需要装工具）");
    }

    private static void ResetCounters() { /* GC 计数无法重置，用相对量比较 */ }

    private static void Report(string label)
    {
        var mem = GC.GetTotalMemory(false);
        Console.WriteLine($"  {label}: Gen0={GC.CollectionCount(0),4}, Gen1={GC.CollectionCount(1),3}, " +
                          $"Gen2={GC.CollectionCount(2),2}, 堆={mem / 1024.0 / 1024,7:N1} MB");
    }
}
