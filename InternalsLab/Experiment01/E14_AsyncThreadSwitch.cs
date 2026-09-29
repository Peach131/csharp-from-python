using System.Diagnostics;

namespace Experiments;

/// <summary>
/// 第 10 周 · 实验 14：await 之后线程会变吗  ★核心
/// </summary>
/// <remarks>
/// 结论：
///   1. await 不是「暂停线程」，而是把方法【拆成多段并返回】。
///   2. 遇到未完成的任务时，当前调用直接 return（返回一个未完成的 Task），
///      调用方线程被释放去做别的事。
///   3. 任务完成后，由线程池线程（或 SynchronizationContext）再次调用状态机的
///      MoveNext() 继续执行 —— 所以【await 之后可能在不同的线程上】。
///   4. 控制台程序没有 SynchronizationContext，所以会在【线程池线程】上恢复。
///   5. WPF/WinForms 有 SynchronizationContext，会把续体调度回 UI 线程。
///   6. 对比 Python asyncio：单线程事件循环，await 后永远是同一个线程。
///      所以「await 后线程会变」是 C# 独有的心智模型，Python 转过来必须重建。
/// </remarks>
public static class E14_AsyncThreadSwitch
{
    private static string ThreadInfo()
    {
        var t = Thread.CurrentThread;
        return $"线程 {t.ManagedThreadId,2} ({(t.IsThreadPoolThread ? "线程池" : "非线程池")})";
    }

    private static void Log(string label)
        => Console.WriteLine($"  [{DateTime.Now:HH:mm:ss.fff}] {label,-26} {ThreadInfo()}");

    private static async Task SimpleAsync()
    {
        Log("进入方法");
        await Task.Delay(100);
        Log("第一个 await 之后");
        await Task.Delay(100);
        Log("第二个 await 之后");
        await Task.Yield();                         // 强制让出，一定会换线程
        Log("Task.Yield 之后");
    }

    private static async Task<int> ComputeAsync(int id)
    {
        var startThread = Environment.CurrentManagedThreadId;
        await Task.Delay(200);
        return startThread * 1000 + Environment.CurrentManagedThreadId;
    }

    public static async Task RunAsync()
    {
        Console.WriteLine("【实验 A：await 前后的线程 ID】");
        Console.WriteLine();
        Log("Main 开始");

        await SimpleAsync();

        Log("回到 Main");
        Console.WriteLine();
        Console.WriteLine("  ★ 观察：await 之后的线程 ID 变了，而且变成了线程池线程。");
        Console.WriteLine("    这就是「状态机 + 线程池」模型：await 让出线程，续体在线程池上恢复。");
        Console.WriteLine();

        Console.WriteLine("【实验 B：SynchronizationContext 是什么】");
        Console.WriteLine();
        var ctx = SynchronizationContext.Current;
        Console.WriteLine($"  当前 SynchronizationContext: {(ctx is null ? "null（控制台程序就是这样）" : ctx.GetType().Name)}");
        Console.WriteLine();
        Console.WriteLine("  ★ 控制台程序没有 SynchronizationContext，所以 await 后在线程池线程恢复。");
        Console.WriteLine("  ★ WPF/WinForms 会安装一个，它把 await 的续体「投递」回 UI 线程，");
        Console.WriteLine("    所以你在 UI 代码里 await 之后能安全地更新控件。");
        Console.WriteLine();

        Console.WriteLine("【实验 C：并发执行 —— await 不阻塞调用方】");
        Console.WriteLine();

        var sw = Stopwatch.StartNew();

        // 顺序执行：200 + 200 + 200 = 600ms
        sw.Restart();
        var r1 = await ComputeAsync(1);
        var r2 = await ComputeAsync(2);
        var r3 = await ComputeAsync(3);
        sw.Stop();
        Console.WriteLine($"  顺序 await 三个 200ms 任务: {sw.ElapsedMilliseconds,5} ms  (结果 {r1}, {r2}, {r3})");

        // 并发执行：约 200ms
        sw.Restart();
        var tasks = new[] { ComputeAsync(1), ComputeAsync(2), ComputeAsync(3) };
        var results = await Task.WhenAll(tasks);
        sw.Stop();
        Console.WriteLine($"  Task.WhenAll 三个 200ms 任务: {sw.ElapsedMilliseconds,5} ms  (结果 {string.Join(", ", results)})");
        Console.WriteLine();
        Console.WriteLine("  ★ 关键：ComputeAsync(1) 被【调用】的瞬间就开始跑（返回一个未完成的 Task），");
        Console.WriteLine("    所以三个任务是同时在跑的。await 只是「等」，不是「启动」。");
        Console.WriteLine("  ★ 对照 Python：asyncio.gather 需要协程对象，而 Python 的协程在被 await 前");
        Console.WriteLine("    【不会】执行。C# 的 Task 从方法被调用就开始执行 —— 这个差异很容易搞错。");
        Console.WriteLine();

        Console.WriteLine("【实验 D：异步方法的同步部分仍在调用者线程上执行】");
        Console.WriteLine();
        Log("准备调用 MostlySyncAsync");
        await MostlySyncAsync();
        Console.WriteLine();

        Console.WriteLine("【实验 E：ConfigureAwait 的作用】");
        Console.WriteLine();
        await WithConfigureAwaitFalse();
        Console.WriteLine();
        Console.WriteLine("  ★ 在控制台程序里，ConfigureAwait(false) 看不出差别（因为没有");
        Console.WriteLine("    SynchronizationContext 需要回）。");
        Console.WriteLine("  ★ 但在 UI 程序里差别巨大：");
        Console.WriteLine("      - 库代码用 ConfigureAwait(false)：不需要回 UI 线程，更快、更安全");
        Console.WriteLine("      - UI 代码【不能】加：加了之后就无法更新控件了");
        Console.WriteLine();

        Console.WriteLine("【实验 F：async void vs async Task】");
        Console.WriteLine();
        Console.WriteLine("  演示 async Task 的异常可以被捕获：");
        try
        {
            await ThrowsAsync();
            Console.WriteLine("    （不会执行到这里）");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"    ✅ 捕获到: {ex.Message}");
        }
        Console.WriteLine();
        Console.WriteLine("  ❌ 如果是 async void，异常会直接抛到 SynchronizationContext 上，");
        Console.WriteLine("     调用方【无法】catch，通常导致进程崩溃。");
        Console.WriteLine("     所以：只有事件处理器可以用 async void，其他一律 async Task。");
        Console.WriteLine();

        Console.WriteLine("【实验 G：绝不要用 .Result / .Wait()】");
        Console.WriteLine();
        Console.WriteLine("  这段代码在控制台里能跑，但在 UI 程序里会【死锁】：");
        Console.WriteLine("      var data = FetchAsync().Result;   // ❌");
        Console.WriteLine("  原因：");
        Console.WriteLine("    1. UI 线程调用 .Result，同步阻塞自己");
        Console.WriteLine("    2. FetchAsync 内部 await 完成后，SynchronizationContext 想把续体");
        Console.WriteLine("       投递回 UI 线程");
        Console.WriteLine("    3. 但 UI 线程正卡在 .Result 上，永远不会去执行那个续体");
        Console.WriteLine("    4. 互相等待 → 死锁");
        Console.WriteLine();

        Console.WriteLine("【动手任务】");
        Console.WriteLine("  1. 把整个实验跑 3 遍，观察线程 ID 每次都不同（线程池是复用的）");
        Console.WriteLine("  2. 写一个 WPF 小程序，在按钮点击里 await 一个 2 秒的 Task，");
        Console.WriteLine("     然后直接改 textBlock.Text —— 能改，因为 SynchronizationContext");
        Console.WriteLine("  3. 在同一个 WPF 程序里把它改成 .Result，看程序卡死（亲自体验一次）");
        Console.WriteLine("  4. Unity：Unity 的协程（IEnumerator + yield）和 async/await 是【两套东西】");
        Console.WriteLine("     —— 去查 UniTask 这个库，它是 Unity 里 async/await 的标准方案");
    }

    /// <summary>演示：方法在第一个 await 之前的代码，仍在调用者线程上同步执行。</summary>
    private static async Task MostlySyncAsync()
    {
        Log("同步部分（await 之前）");     // 与调用者是同一线程
        await Task.Delay(50);
        Log("异步部分（await 之后）");     // 线程可能变了
    }

    private static async Task WithConfigureAwaitFalse()
    {
        Log("await 前");
        await Task.Delay(50).ConfigureAwait(false);
        Log("ConfigureAwait(false) 之后");
    }

    private static async Task ThrowsAsync()
    {
        await Task.Delay(50);
        throw new InvalidOperationException("异步方法里抛出的异常");
    }
}
