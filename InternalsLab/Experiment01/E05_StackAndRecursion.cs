namespace Experiments;

/// <summary>
/// 第 2 周 · 实验 5：栈帧与递归
/// </summary>
/// <remarks>
/// 结论：
///   1. 每次方法调用都会在栈上压入一个「栈帧」（含参数、返回地址、局部变量）。
///   2. 栈是有限的（Windows 默认 1MB / 线程），递归必须收敛。
///   3. StackOverflowException 【无法 catch】—— 因为栈溢出时已经没有空间运行 catch 块了。
///      它会导致进程【立即崩溃】，finally 也不会执行。这是 .NET 里少数无法用异常机制
///      处理的错误之一。
///   4. 所以：递归处理深层次数据结构（如超深链表、超深目录树）时要改成迭代。
/// </remarks>
public static class E05_StackAndRecursion
{
    /// <summary>用无副作用的方式估算栈深度上限，不会崩溃。</summary>
    private static int MeasureDepth(int depth)
    {
        if (depth > 200_000) return depth;                  // 安全阀
        return MeasureDepth(depth + 1);
    }

    /// <summary>尾递归的迭代版本（C# 不会自动做尾调用优化，必须手写）。</summary>
    private static long FactorialIterative(int n)
    {
        long result = 1;
        for (int i = 2; i <= n; i++) result *= i;
        return result;
    }

    private static long FactorialRecursive(int n)
        => n <= 1 ? 1 : n * FactorialRecursive(n - 1);

    private static void A() => B();
    private static void B() => C();
    private static void C() => ThrowHere();

    private static void ThrowHere()
    {
        throw new InvalidOperationException("这是 C() 抛出的异常");
    }

    public static void Run()
    {
        Console.WriteLine("【实验 A：安全的递归深度测量】");
        Console.WriteLine();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var depth = MeasureDepth(0);
        sw.Stop();
        Console.WriteLine($"  递归到深度 {depth:N0} 用时 {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"  每个栈帧大约几百字节，所以 {depth:N0} 层约占 "
                          + $"{depth * 256L / 1024 / 1024} MB 栈空间（粗估）");
        Console.WriteLine("  ★ 但这不是真正的上限！JIT 会把 MeasureDepth 优化成尾调用，");
        Console.WriteLine("    所以它可能根本没消耗栈。下面用真实场景看上限。");
        Console.WriteLine();

        Console.WriteLine("【实验 B：递归 vs 迭代 的性能】");
        Console.WriteLine();

        const int n = 20;

        sw.Restart();
        var r1 = FactorialRecursive(n);
        sw.Stop();
        var recursiveMs = sw.Elapsed.TotalMilliseconds;

        sw.Restart();
        var r2 = FactorialIterative(n);
        sw.Stop();
        var iterativeMs = sw.Elapsed.TotalMilliseconds;

        Console.WriteLine($"  递归 20! = {r1}  耗时 {recursiveMs:F4} ms");
        Console.WriteLine($"  迭代 20! = {r2}  耗时 {iterativeMs:F4} ms");
        Console.WriteLine("  20! = 2,432,902,008,176,640,000 —— 注意这已经接近 long 的极限");
        Console.WriteLine("  (long 最大约 9.22 × 10^18，所以 21! 会溢出成负数！自己试试)");
        Console.WriteLine();

        Console.WriteLine("【实验 C：异常堆栈 = 栈帧的快照】");
        Console.WriteLine();
        try
        {
            A();
        }
        catch (Exception ex)
        {
            Console.WriteLine("  捕获到异常，StackTrace 记录了完整的调用链：");
            Console.WriteLine();
            foreach (var line in ex.StackTrace!.Split('\n'))
                Console.WriteLine($"    {line.Trim()}");
            Console.WriteLine();
            Console.WriteLine("  ★ 每一个箭头就是一层栈帧。异常机制就是靠保存栈帧信息实现的。");
            Console.WriteLine("  ★ 所以：异常很贵（要捕获堆栈），不要用它做流程控制。");
        }
        Console.WriteLine();

        Console.WriteLine("【实验 D：StackOverflow 演示（危险，默认不执行）】");
        Console.WriteLine();
        Console.WriteLine("  下面这段代码会让进程【立即崩溃】，try/catch 完全无效：");
        Console.WriteLine();
        Console.WriteLine("      static void Boom(int d) { Console.WriteLine(d); Boom(d + 1); }");
        Console.WriteLine("      try { Boom(0); }");
        Console.WriteLine("      catch (Exception e) { Console.WriteLine(\"抓到了\"); }  // ← 永远不会执行");
        Console.WriteLine();
        Console.WriteLine("  结果是一个没有任何输出的进程终止（退出码非 0），");
        Console.WriteLine("  连 finally 块都不会执行。");
        Console.WriteLine();
        Console.Write("  要亲自试一次吗？（会杀掉本程序）输入 YES 执行，其他跳过 > ");
        var confirm = Console.ReadLine();

        if (string.Equals(confirm?.Trim(), "YES", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine();
            Console.WriteLine("  3 秒后崩溃...（注意观察最后的输出，以及进程是怎么死的）");
            Thread.Sleep(3000);
            Boom(0);
            Console.WriteLine("  ← 这行永远不会打印");
        }
        else
        {
            Console.WriteLine("  已跳过。（建议至少试一次，亲眼看到进程崩溃的记忆比读十遍更深刻）");
        }
        Console.WriteLine();

        Console.WriteLine("【动手任务】");
        Console.WriteLine("  1. 在 Rider 里对 ThrowHere() 打断点，跑到断点后看 Debug 窗口的");
        Console.WriteLine("     Frames 面板 —— 那就是栈帧的可视化，点每一帧能看到当时的局部变量");
        Console.WriteLine("  2. 把 MeasureDepth 改成一个【真的会崩溃】的版本，观察退出码");
        Console.WriteLine("  3. 思考：为什么 C# 不做尾调用优化（而 F# 做）？");
        Console.WriteLine("     提示：这会破坏异常堆栈的完整性，让调试变难");
    }

    /// <summary>故意无限递归。仅用于演示 StackOverflow，调用后进程会崩溃。</summary>
    private static void Boom(int depth)
    {
        // 加一个局部变量，避免被 JIT 优化成尾调用
        Span<byte> padding = stackalloc byte[256];
        padding[0] = (byte)(depth & 0xFF);
        if (depth % 5000 == 0)
            Console.WriteLine($"    深度 {depth:N0}，padding[0]={padding[0]}");

        Boom(depth + 1);
    }
}
