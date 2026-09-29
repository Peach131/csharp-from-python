namespace Experiments;

/// <summary>
/// 第 2 周 · 实验 4：对象在堆上到底占多少字节
/// </summary>
/// <remarks>
/// 结论（x64 平台）：
///   1. 每个堆对象有 16 字节固定开销：
///        - Object Header (8 字节)：同步块索引，用于 lock、缓存 GetHashCode
///        - MethodTable 指针 (8 字节)：指向类型描述表，GetType() 就是读它
///   2. 所以 new object() 占 24 字节（16 头 + 至少 8 字节填充），不是 0 字节！
///   3. 对象总大小会按 8 字节对齐。
///   4. 这个"隐形开销"在 Unity 里非常致命：一个只有 1 个 int 的类，
///      实际占 24 字节，是数据本身的 6 倍。
/// </remarks>
public static class E04_ObjectSize
{
    // 下面这些类/结构只为「字段布局」而存在，字段永远不被读写（我们只测大小）。
    // CS0649（字段从未赋值）正是它们要证明的东西，所以这里显式关掉该警告。
#pragma warning disable CS0649
    private class Empty { }
    private class OneInt { public int A; }
    private class TwoInt { public int A; public int B; }
    private class ThreeInt { public int A; public int B; public int C; }
    private class OneLong { public long A; }
    private class TwoLong { public long A; public long B; }

    private struct SOneInt { public int A; }
    private struct STwoInt { public int A; public int B; }
    private struct SThreeInt { public int A; public int B; public int C; }
#pragma warning restore CS0649

    /// <summary>
    /// 用「分配前 / 分配后的存活堆大小差」测量每个对象的实际占用。
    /// </summary>
    private static long MeasureClass<T>(int count = 100_000) where T : new()
    {
        // 先清理，保证基线干净
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = GC.GetTotalMemory(true);
        var arr = new T[count];                 // 引用数组：8 字节/元素
        for (int i = 0; i < count; i++)
            arr[i] = new T();
        GC.KeepAlive(arr);
        var after = GC.GetTotalMemory(true);

        // 减去数组本身的 24 字节头 + 8 字节/元素
        var arrayOverhead = 24 + count * 8L;
        return (after - before - arrayOverhead) / count;
    }

    public static void Run()
    {
        Console.WriteLine("【实验 A：对象大小实测（每类分配 10 万个，取平均）】");
        Console.WriteLine();
        Console.WriteLine("  类型                实例字段            实测大小   说明");
        Console.WriteLine("  ─────────────────────────────────────────────────────────────");

        Print("Empty (无字段)", "—", MeasureClass<Empty>());
        Print("OneInt (1×int)", "4 字节", MeasureClass<OneInt>());
        Print("TwoInt (2×int)", "8 字节", MeasureClass<TwoInt>());
        Print("ThreeInt (3×int)", "12 字节", MeasureClass<ThreeInt>());
        Print("OneLong (1×long)", "8 字节", MeasureClass<OneLong>());
        Print("TwoLong (2×long)", "16 字节", MeasureClass<TwoLong>());

        Console.WriteLine();
        Console.WriteLine("  ★ 关键发现：Empty 和 OneInt 一样大！");
        Console.WriteLine("    因为 16 字节对象头之后还有 8 字节的对齐填充，");
        Console.WriteLine("    那 4 字节的 int 刚好塞进填充位里，不增加大小。");
        Console.WriteLine("    这就是「内存对齐」的实际影响。");
        Console.WriteLine();

        Console.WriteLine("【实验 B：可空类型的装箱开销】");
        Console.WriteLine();
        long before, after;

        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        before = GC.GetTotalMemory(true);
        var boxed = new object[100_000];
        for (int i = 0; i < 100_000; i++)
            boxed[i] = 42;                      // 装箱 int
        GC.KeepAlive(boxed);
        after = GC.GetTotalMemory(true);
        var perBox = (after - before - (24 + 100_000 * 8L)) / 100_000;
        Console.WriteLine($"  装箱一个 int    : {perBox} 字节  (值本身只占 4 字节！)");
        Console.WriteLine();

        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        before = GC.GetTotalMemory(true);
        var noBox = new int[100_000];
        for (int i = 0; i < 100_000; i++)
            noBox[i] = 42;                      // 不装箱
        GC.KeepAlive(noBox);
        after = GC.GetTotalMemory(true);
        var perInt = (after - before - 24) / 100_000.0;
        Console.WriteLine($"  不装箱的 int    : {perInt:F1} 字节  ← 只有数据本身");
        Console.WriteLine();
        Console.WriteLine($"  → 装箱把一个 4 字节的值放大成了 {perBox / 4.0:F1} 倍");
        Console.WriteLine();

        Console.WriteLine("【实验 C：用 sizeof / Unsafe.SizeOf 看数据本身】");
        Console.WriteLine();
        unsafe
        {
            Console.WriteLine($"  sizeof(int)        = {sizeof(int)}");
            Console.WriteLine($"  sizeof(long)       = {sizeof(long)}");
            Console.WriteLine($"  sizeof(double)     = {sizeof(double)}");
            Console.WriteLine($"  sizeof(bool)       = {sizeof(bool)}");
            Console.WriteLine($"  sizeof(char)       = {sizeof(char)}   ← UTF-16，所以中文占 2 字节");
            Console.WriteLine($"  sizeof(SOneInt)    = {sizeof(SOneInt)}");
            Console.WriteLine($"  sizeof(STwoInt)    = {sizeof(STwoInt)}");
            Console.WriteLine($"  sizeof(SThreeInt)  = {sizeof(SThreeInt)}");
            Console.WriteLine($"  sizeof(IntPtr)     = {sizeof(IntPtr)}   ← 引用（指针）的大小");
        }
        Console.WriteLine();
        Console.WriteLine("  注意：struct 的大小【不含】16 字节对象头（它不是对象）。");
        Console.WriteLine("        但 struct 一旦装箱，就要额外付出 24 字节的代价。");
        Console.WriteLine();

        Console.WriteLine("【动手任务】");
        Console.WriteLine("  1. 把 MeasureClass<Empty>() 的大小和 sizeof(SOneInt) 对比，理解");
        Console.WriteLine("     「对象」和「数据」的区别");
        Console.WriteLine("  2. 递归分配 100 万个 new object()，用任务管理器观察进程内存");
        Console.WriteLine("  3. 去 Unity 里想一想：如果一个 MonoBehaviour 只有 3 个 int，");
        Console.WriteLine("     它在托管堆上占多少？（提示：MonoBehaviour 继承链很长，远不止 24 字节）");
    }

    private static void Print(string name, string fields, long actual)
    {
        Console.WriteLine($"  {name,-20} {fields,-16} {actual,6} 字节");
    }
}
