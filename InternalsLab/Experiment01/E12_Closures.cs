namespace Experiments;

/// <summary>
/// 第 8 周 · 实验 12：闭包会分配对象 + 循环变量捕获的坑
/// </summary>
/// <remarks>
/// 结论：
///   1. 委托是堆上的对象（含 Target + Method 指针），不是一个裸函数指针。
///   2. 捕获了外部变量的 Lambda，编译器会生成一个 display class 来装那些变量 →
///      【必然产生堆分配】。
///   3. 不捕获任何变量的 Lambda，C# 9+ 会缓存成静态单例 → 零分配。
///   4. 捕获的是【变量本身】而不是值的快照。
///   5. foreach 的循环变量每次迭代是新变量（C# 5+ 已修复），
///      但 for 的循环变量只有一个 → 全部 Lambda 捕获同一个变量。
///   6. Unity：在 Update() 里写 list.ForEach(x => ...) 或 list.Where(x => ...)
///      会每帧分配闭包 + 委托 → GC 卡顿。
/// </remarks>
public static class E12_Closures
{
    private static long Allocated() => GC.GetAllocatedBytesForCurrentThread();

    private static double MeasureAllocPerCall(Action<int> action, int iterations = 10_000)
    {
        for (int i = 0; i < 100; i++) action(i);              // 预热

        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        var before = Allocated();
        for (int i = 0; i < iterations; i++) action(i);
        return (Allocated() - before) / (double)iterations;
    }

    private static int _multiplier = 3;
    private static readonly Func<int, int> StaticLambda = x => x * 2;   // 无捕获，可复用

    public static void Run()
    {
        Console.WriteLine("【实验 A：闭包的分配成本】");
        Console.WriteLine();
        Console.WriteLine($"  {"写法",-46} {"分配/次",12}");
        Console.WriteLine("  " + new string('─', 60));

        // 1. 捕获局部变量 → 每个 Lambda 都要分配 display class
        var capturedAlloc = MeasureAllocPerCall(_ =>
        {
            int local = 10;
            Func<int, int> f = x => x * local;                // 捕获 local
            GC.KeepAlive(f(1));
        });
        Console.WriteLine($"  {"捕获局部变量的 Lambda",-46} {capturedAlloc,9:F0} B");

        // 2. 捕获静态字段 → 不需要 display class，但委托对象仍需分配
        var staticFieldAlloc = MeasureAllocPerCall(_ =>
        {
            Func<int, int> f = x => x * _multiplier;          // 捕获静态字段
            GC.KeepAlive(f(1));
        });
        Console.WriteLine($"  {"捕获静态字段的 Lambda",-46} {staticFieldAlloc,9:F0} B");

        // 3. 不捕获任何变量 → C# 9+ 静态缓存
        var noCaptureAlloc = MeasureAllocPerCall(_ =>
        {
            Func<int, int> f = x => x * 2;                    // 无捕获
            GC.KeepAlive(f(1));
        });
        Console.WriteLine($"  {"不捕获任何变量的 Lambda（C# 9+ 缓存）",-46} {noCaptureAlloc,9:F0} B");

        // 4. 复用已缓存的委托
        var reusedAlloc = MeasureAllocPerCall(_ =>
        {
            GC.KeepAlive(StaticLambda(1));
        });
        Console.WriteLine($"  {"复用静态字段里的委托",-46} {reusedAlloc,9:F0} B");

        Console.WriteLine();
        Console.WriteLine("  ★ 结论：捕获变量越少，分配越少。热路径上把 Lambda 提出来存成");
        Console.WriteLine("    静态字段或实例字段，是消除分配的标准手段。");
        Console.WriteLine();

        Console.WriteLine("【实验 B：循环变量捕获的经典陷阱】");
        Console.WriteLine();

        var actions = new List<Action>();

        // foreach：每次迭代是【新变量】→ 安全
        foreach (var i in Enumerable.Range(0, 3))
            actions.Add(() => Console.Write($"{i} "));
        Console.Write("  foreach 捕获：");
        foreach (var a in actions) a();
        Console.WriteLine("   ← 0 1 2 ✅ 正确");

        // for：循环变量只有【一个】→ 全部捕获同一个
        actions.Clear();
        for (int i = 0; i < 3; i++)
            actions.Add(() => Console.Write($"{i} "));
        Console.Write("  for 捕获    ：");
        foreach (var a in actions) a();
        Console.WriteLine("   ← 3 3 3 ❌ 全变成 3 了！");

        // 修复：在循环体内创建局部副本
        actions.Clear();
        for (int i = 0; i < 3; i++)
        {
            int copy = i;                                     // ✅ 每次迭代一个新变量
            actions.Add(() => Console.Write($"{copy} "));
        }
        Console.Write("  for + 副本  ：");
        foreach (var a in actions) a();
        Console.WriteLine("   ← 0 1 2 ✅ 修复成功");
        Console.WriteLine();
        Console.WriteLine("  ★ 原因：for 的 i 是【同一个变量】，三个 Lambda 的 display class");
        Console.WriteLine("    捕获的是同一个字段。循环结束时 i==3，所以全部输出 3。");
        Console.WriteLine("  ★ Python 也有同样的坑：");
        Console.WriteLine("        funcs = [lambda: i for i in range(3)]  # 全部返回 2");
        Console.WriteLine("    但 Python 连 for/foreach 都不区分，所有循环都是同一个变量。");
        Console.WriteLine();

        Console.WriteLine("【实验 C：Python 里的同一个坑（对照）】");
        Console.WriteLine();
        Console.WriteLine("  Python 代码：");
        Console.WriteLine("      fs = []");
        Console.WriteLine("      for i in range(3):");
        Console.WriteLine("          fs.append(lambda: i)");
        Console.WriteLine("      print([f() for f in fs])   # [2, 2, 2]");
        Console.WriteLine();
        Console.WriteLine("  修复方式也相同 —— 用默认参数绑定当前值：");
        Console.WriteLine("      fs.append(lambda i=i: i)   # [0, 1, 2]");
        Console.WriteLine("  C# 里的等价修复就是 int copy = i;");
        Console.WriteLine();

        Console.WriteLine("【实验 D：多播委托是不可变的】");
        Console.WriteLine();

        Action a1 = () => Console.Write("A");
        var original = a1;
        a1 += () => Console.Write("B");
        a1 += () => Console.Write("C");

        Console.Write("  多播委托调用结果：");
        a1();
        Console.WriteLine();

        Console.WriteLine($"  ReferenceEquals(a1, original) = {ReferenceEquals(a1, original)}");
        Console.Write("  original 仍然只包含：");
        original();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("  ★ += 会创建一个【新的】委托对象（内部是委托数组），原委托不变。");
        Console.WriteLine("    所以每次 += 都是 O(n) 的复制操作。事件订阅极频繁时要注意。");
        Console.WriteLine("  ★ Python 的 += 对列表是【原地修改】，语义完全不同，别混淆。");
        Console.WriteLine();

        Console.WriteLine("【实验 E：EventHandler 的签名约定】");
        Console.WriteLine();
        var publisher = new Publisher();
        publisher.Message += (sender, args) => Console.WriteLine($"  订阅者1 收到: {args.Message}");
        publisher.Message += (sender, args) => Console.WriteLine($"  订阅者2 收到: {args.Message}");
        publisher.Publish("hello");
        Console.WriteLine();
        Console.WriteLine("  ★ 事件 = 受限的委托字段：外部只能 += / -=，不能 = 覆盖，也不能直接调用。");
        Console.WriteLine("    这是封装：只有发布者能触发事件，订阅者只能监听。");
        Console.WriteLine("  ★ 标准签名是 (object? sender, EventArgs e)，用 EventHandler 或");
        Console.WriteLine("    EventHandler<TEventArgs>。自己定义事件时请遵循这个约定。");
        Console.WriteLine();

        Console.WriteLine("【动手任务】");
        Console.WriteLine("  1. 打开 ILSpy，看捕获变量那个 Lambda 生成的 <>c__DisplayClass 类");
        Console.WriteLine("  2. 用 Rider 的 Memory 视图，跑实验 A 的各段，看哪个生成了最多对象");
        Console.WriteLine("  3. Unity 实战：把下面这段改造成零分配版本");
        Console.WriteLine("        void Update() {");
        Console.WriteLine("            enemies.ForEach(e => e.Tick(Time.deltaTime));");
        Console.WriteLine("            var alive = enemies.Where(e => e.Hp > 0).ToList();");
        Console.WriteLine("        }");
    }

    private sealed class Publisher
    {
        public event EventHandler<MessageEventArgs>? Message;

        public void Publish(string text) => Message?.Invoke(this, new MessageEventArgs(text));
    }

    private sealed class MessageEventArgs : EventArgs
    {
        public string Message { get; }
        public MessageEventArgs(string message) => Message = message;
    }
}
