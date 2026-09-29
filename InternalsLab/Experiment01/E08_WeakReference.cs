namespace Experiments;

/// <summary>
/// 第 5-6 周 · 实验 8：WeakReference —— 证明 GC 真的回收了对象  ★核心
/// </summary>
/// <remarks>
/// 结论：
///   1. WeakReference 是「不阻止回收」的引用。GC 从根出发做可达性分析，
///      弱引用不算根，所以对象照样被回收。
///   2. 这是验证 GC 行为的最可靠手段 —— 比任何文档都可信。
///   3. 强引用存在时对象不会被回收，这是你验证「根」概念的方式。
///   4. 对比 Python：Python 用引用计数 + 循环检测，对象不可达时通常【立即】释放；
///      C# 用追踪式 GC，释放时机【不确定】。
/// </remarks>
public static class E08_WeakReference
{
    private class BigObject
    {
        public byte[] Payload = new byte[100_000];       // 约 100KB，会进大对象堆
        public string Tag;

        public BigObject(string tag) => Tag = tag;
    }

    private static WeakReference CreateAndDrop()
    {
        var obj = new BigObject("临时对象");
        Console.WriteLine($"  在方法内创建: {obj.Tag}, 载荷 {obj.Payload.Length:N0} 字节");
        // obj 是局部变量 —— 它是 GC 的【根】
        return new WeakReference(obj);
    }

    private static WeakReference _staticRef = null!;

    private static void HoldInStaticField()
    {
        var obj = new BigObject("被静态字段引用");
        _staticRef = new WeakReference(obj);
        StaticRoot = obj;                     // 存进静态字段 → 静态字段是根 → 不会被回收
        Console.WriteLine("  已把对象存进静态字段（静态字段是 GC 的根）");
    }

    private static BigObject? StaticRoot;

    public static void Run()
    {
        Console.WriteLine("【实验 A：对象不可达后会被回收】");
        Console.WriteLine();

        var weak = CreateAndDrop();

        Console.WriteLine($"  方法已返回，此时对象还活着吗？ {weak.IsAlive}");
        Console.WriteLine("  （可能还是 True —— GC 还没跑，对象只是「不可达」而非「已回收」）");
        Console.WriteLine();

        ShowHeap("回收前");

        Console.WriteLine("  调用 GC.Collect() ...");
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Console.WriteLine($"  → 对象还活着吗？ {weak.IsAlive}");
        Console.WriteLine($"  → 弱引用的 Target: {(weak.Target is null ? "null（已被回收）" : "仍然存在")}");
        Console.WriteLine();
        ShowHeap("回收后");
        Console.WriteLine();
        Console.WriteLine("  ★ 这就是「可达性分析」：GC 从根（栈变量、静态字段、寄存器、GC Handle）");
        Console.WriteLine("    出发遍历，遍历不到的对象就是垃圾。");
        Console.WriteLine();

        Console.WriteLine("【实验 B：强引用会阻止回收（这就是「根」）】");
        Console.WriteLine();

        var strong = new BigObject("强引用");
        var weak2 = new WeakReference(strong);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        Console.WriteLine($"  有强引用时: IsAlive = {weak2.IsAlive}   ← 不会被回收");

        strong = null!;                        // 断开强引用
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Console.WriteLine($"  断开强引用后: IsAlive = {weak2.IsAlive}   ← 被回收了");
        Console.WriteLine();

        Console.WriteLine("【实验 C：静态字段是根，不会被回收】");
        Console.WriteLine();

        HoldInStaticField();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        Console.WriteLine($"  GC 之后 IsAlive = {_staticRef.IsAlive}   ← 仍然活着！");
        Console.WriteLine("  原因：静态字段是 GC 的根，只要类还被加载，它引用的对象就不会被回收。");
        Console.WriteLine("  ★ 这也是「静态字段导致内存泄漏」的原理 —— 静态集合只增不减，");
        Console.WriteLine("    就是永久泄漏。Unity 里切场景时忘记清空静态缓存，就是这个后果。");
        StaticRoot = null;                     // 清理
        Console.WriteLine();

        Console.WriteLine("【实验 D：循环引用会被回收吗？（对比 Python）】");
        Console.WriteLine();

        var nodeA = new Node { Name = "A" };
        var nodeB = new Node { Name = "B" };
        nodeA.Next = nodeB;
        nodeB.Next = nodeA;                    // 互相引用，形成环

        var weakA = new WeakReference(nodeA);
        var weakB = new WeakReference(nodeB);

        nodeA = null!;                         // 断开外部引用
        nodeB = null!;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Console.WriteLine($"  A 还活着吗？ {weakA.IsAlive}");
        Console.WriteLine($"  B 还活着吗？ {weakB.IsAlive}");
        Console.WriteLine();
        Console.WriteLine("  ★ 都被回收了！追踪式 GC 从根出发找不到环里的任何对象，");
        Console.WriteLine("    所以环自然被回收 —— 【不需要】额外的循环检测器。");
        Console.WriteLine("  ★ 对比 Python：引用计数会让环里每个对象计数都 ≥ 1，");
        Console.WriteLine("    必须靠 gc 模块的循环检测器才能回收。所以 Python 的 __del__");
        Console.WriteLine("    在循环引用里可能永远不被调用。");
        Console.WriteLine();

        Console.WriteLine("【动手任务】");
        Console.WriteLine("  1. 在 GC.Collect() 那行打断点，用 Rider 的 Memory 视图看堆快照");
        Console.WriteLine("  2. 把 BigObject 的字节数组从 100_000 改成 10_000_000，");
        Console.WriteLine("     观察内存增长和回收后是否归还给操作系统（看任务管理器）");
        Console.WriteLine("  3. 思考：为什么 Unity 里「切场景后内存没降下来」很常见？");
        Console.WriteLine("     提示：静态字段 + 事件订阅（事件持有委托，委托持有目标对象）");
    }

    private class Node
    {
        public string Name = "";
        public Node? Next;
    }

    private static void ShowHeap(string phase)
    {
        var info = GC.GetGCMemoryInfo();
        Console.WriteLine($"  [{phase}] 堆大小 {GC.GetTotalMemory(false) / 1024.0,10:N1} KB, " +
                          $"已提交 {info.TotalCommittedBytes / 1024.0 / 1024,6:N1} MB, " +
                          $"Gen0 GC 次数 {GC.CollectionCount(0)}");
    }
}
