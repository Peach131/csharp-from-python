using System.Diagnostics;

namespace Experiments;

/// <summary>
/// 第 1 周 · 实验 2：JIT 只编译一次
/// </summary>
/// <remarks>
/// 结论：
///   1. 方法的第一次调用包含 JIT 编译开销，所以明显更慢。
///   2. 之后的调用走已编译的机器码，极快。
///   3. 这意味着：用 Stopwatch 做基准测试时，必须【先预热】，
///      否则测到的是 JIT 时间而不是代码性能。
///   4. 对比 Python：CPython 每次执行都要解释字节码，没有"编译成机器码"这一步。
///      这是 C# 在计算密集场景通常快 10-50 倍的首要原因。
/// </remarks>
public static class E02_JitCompilation
{
    private static long Add(long a, long b) => a + b;

    public static void Run()
    {
        Console.WriteLine("【实验 A：首次调用包含 JIT 编译】");
        Console.WriteLine();

        // 第一次调用：包含 JIT 编译
        var sw = Stopwatch.StartNew();
        Add(1, 2);
        sw.Stop();
        var firstCallTicks = sw.ElapsedTicks;
        Console.WriteLine($"  第 1 次调用 : {firstCallTicks,10} ticks  ← 包含 JIT 编译");

        // 再调 100 万次
        sw.Restart();
        long sum = 0;
        for (int i = 0; i < 1_000_000; i++)
            sum += Add(i, 1);
        sw.Stop();
        var millionTicks = sw.ElapsedTicks;
        Console.WriteLine($"  100 万次调用: {millionTicks,10} ticks");
        Console.WriteLine($"  平均每次    : {(double)millionTicks / 1_000_000,10:F4} ticks");
        Console.WriteLine();

        var ratio = firstCallTicks / Math.Max(1.0, (double)millionTicks / 1_000_000);
        Console.WriteLine($"  → 首次调用约等于 {ratio:N0} 次稳态调用的耗时");
        Console.WriteLine("  → 差值主要就是 JIT 编译时间（把 IL 翻译成机器码）");
        Console.WriteLine();

        Console.WriteLine("【实验 B：预热对基准测试的影响】");
        Console.WriteLine();

        const int iterations = 5_000_000;

        // 不预热
        sw.Restart();
        long s1 = 0;
        for (int i = 0; i < iterations; i++) s1 += Add(i, 1);
        sw.Stop();
        var noWarmup = sw.Elapsed.TotalMilliseconds;

        // 预热后再测
        for (int i = 0; i < 100_000; i++) Add(i, 1);      // 预热
        sw.Restart();
        long s2 = 0;
        for (int i = 0; i < iterations; i++) s2 += Add(i, 1);
        sw.Stop();
        var withWarmup = sw.Elapsed.TotalMilliseconds;

        Console.WriteLine($"  不预热 : {noWarmup,8:F1} ms");
        Console.WriteLine($"  已预热 : {withWarmup,8:F1} ms");
        Console.WriteLine($"  差异   : {noWarmup - withWarmup,8:F1} ms  ← 这就是 JIT 的代价");
        Console.WriteLine($"  (校验和: {s1} / {s2}，用 GC.KeepAlive 防止被优化掉)");
        GC.KeepAlive(s1 + s2);
        Console.WriteLine();

        Console.WriteLine("【关键教训】");
        Console.WriteLine("  做性能对比时，必须让两边都【预热】。");
        Console.WriteLine("  正式基准测试请用 BenchmarkDotNet 而不是 Stopwatch —— 它会自动处理");
        Console.WriteLine("  预热、多次迭代、统计显著性、防止死代码消除。");
        Console.WriteLine("  这不是爱好问题：用 Stopwatch 得出的性能结论经常是错的。");
    }
}
