using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LessonPractice
{
    // ============================================================================
    //  第 24 课 · 示例：async / await 状态机
    //
    //  ★ 本示例唯一要证明的一句话：
    //
    //        await 不是「暂停线程」，而是「把方法拆成多段并返回」。
    //
    //    拆开来说就是四步：
    //      1. 遇到未完成的 Task → 方法【直接 return】（返回一个未完成的 Task）
    //      2. 调用方线程被释放，去干别的事（不是"卡住"，是"走了"）
    //      3. 任务完成后，由线程池线程（或 SynchronizationContext）
    //         再次调用状态机的 MoveNext() 继续执行
    //      4. 所以 → 【await 之后可能在不同的线程上】！
    //
    //  ★ 第 4 条是 Python 转过来的人最容易搞错的：asyncio 里 await 前后
    //    永远是同一个线程，C# 里不是。本示例用线程 ID 把它直接打印出来。
    //
    //  ★ 运行方式：
    //      1. 打开 第24课.sln，把 Lesson24Demo 设为启动项目，Ctrl + F5
    //      2. 或者命令行：dotnet run --project Lesson24Demo
    //     ⚠️ 全程大约跑 3~4 秒（里面全是 Task.Delay，没有空转）。
    //
    //  ★ 配套实验：InternalsLab 菜单 14（E14_AsyncThreadSwitch.cs）
    //      —— 本示例是它的"逐条拆解 + 加料版"，建议两个都跑一遍。
    //
    //  ★ 本示例刻意【不】调用 .Result / .Wait()：
    //     那会真的把程序挂死，我们在"七、死锁"里用一个【安全的沙盒】
    //     把死锁复现出来（1 秒后自动超时退出），而不是把示例跑挂。
    // ============================================================================
    class Lesson24Demo
    {
        // 一次"假 IO"的耗时。故意做得短一点，让整个示例 3~4 秒能跑完。
        // ★ 注意：这里能用常量，但并发次数不能写成"猜的数字"，
        //   后面每一段的耗时都会和它做对比，你得能自己算出来。
        const int FakeIoMs = 150;

        // ────────────────────────────────────────────────────────────────
        //  小工具：线程信息
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// 打印一行带时间戳、带线程 ID 的日志。
        /// ★ 为什么要带时间戳：delay 到底等了多久、续体是几点几分回来的，
        ///   光看线程 ID 看不出来，两个一起看才能对上号。
        /// </summary>
        static void Log(string label)
        {
            int tid = Environment.CurrentManagedThreadId;
            bool pooled = Thread.CurrentThread.IsThreadPoolThread;
            string kind = pooled ? "线程池" : "非线程池";
            string stamp = DateTime.Now.ToString("HH:mm:ss.fff");

            // {label,-28} = 占 28 格左对齐，这样后面那列能对齐，一眼看出线程变没变
            Console.WriteLine($"    [{stamp}] {label,-28} 线程 {tid,2} （{kind}）");
        }

        /// <summary>只取线程 ID，不打印 —— 用来做"变了没变"的对比。</summary>
        static int Tid() => Environment.CurrentManagedThreadId;

        // ────────────────────────────────────────────────────────────────
        //  顶部：演示用的 async 方法
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// 观察点最集中的一个方法：进入、第一个 await 之后、第二个 await 之后、Yield 之后。
        /// </summary>
        static async Task ThreadHopAsync()
        {
            // ① await 之前：这段仍然是【同步】执行的，跑在调用者线程上。
            Log("进入方法（await 之前）");

            // ② Task.Delay 的续体：注意它【不保证】换线程（见下面的注释）
            await Task.Delay(80);
            Log("第 1 个 await 之后");

            await Task.Delay(80);
            Log("第 2 个 await 之后");

            // ③ Task.Yield()：唯一"保证"会换线程的等待 ——
            //    它让自己排队回线程池，所以几乎一定换一个线程继续跑。
            await Task.Yield();
            Log("Task.Yield 之后");

            // ★ 这里想让你做一个反直觉的观察：
            //   Task.Delay 之后的线程 ID【经常和 await 之前一样】。
            //   为什么？因为 .NET 的 Timer 会优先把续体塞回"刚空出来的那个线程"，
            //   这是性能优化，不是语言保证。
            //   → 所以「await 后线程可能变」的正确理解是【可能变】，
            //     而不是「一定变」或「一定不变」。
            //   → 真正不允许依赖的是：await 之后不能假设自己在哪个线程上。
        }

        /// <summary>
        /// 带 AsyncLocal 的版本：证明"线程会换，但逻辑上下文不会丢"。
        /// </summary>
        static async Task AsyncLocalAsync()
        {
            // AsyncLocal<T> ≈ Python 的 contextvars.ContextVar。
            // ★ 关键：它的值跟着【异步控制流】流动，不跟着线程跑。
            //   所以线程 ID 会变，requestId 不会丢。
            var requestId = new AsyncLocal<string> { Value = "REQ-42" };

            Console.WriteLine($"    await 之前：requestId = {requestId.Value}，线程 {Tid()}");
            await Task.Yield();

            // 这一行是第一行代码想让你亲眼看的东西：
            //   线程号变了（线程池 8 → 线程池 11 之类），
            //   但 requestId 还是 "REQ-42"。
            //   如果改成普通的 [ThreadStatic] 变量，这里就已经是 null 了。
            Console.WriteLine($"    await 之后：requestId = {requestId.Value}，线程 {Tid()}");
            Console.WriteLine();
            Console.WriteLine("    ★ 结论：await 换的是【线程】，不是【逻辑上下文】。");
            Console.WriteLine("      线程是物理资源（会被复用、会变），ExecutionContext 是逻辑资源（跟着流程走）。");
            Console.WriteLine("      后面讲的 SynchronizationContext 就是靠这套机制传播的。");
        }

        /// <summary>观察点：async 方法在第一个 await 之前，完全跑在调用者线程上。</summary>
        static async Task MostlySyncAsync()
        {
            Log("同步部分（await 之前）");
            await Task.Delay(50);
            Log("异步部分（await 之后）");
        }

        /// <summary>一行"假 IO"：等待一会儿，然后交回一个结果。</summary>
        static async Task<int> SimulateIoAsync(int id, int ms)
        {
            await Task.Delay(ms);
            return id * 10;
        }

        /// <summary>在锁里 await —— 反例，用来印证"await 会让出线程"。</summary>
        static readonly object Gate = new object();

        static async Task LockInsideAsync(int index)
        {
            // ⚠️ 教学反例，不要模仿：这里的 lock 里【没有】await，是为了能编译。
            //    ★★ 请亲手试一次：把下面注释掉的那行 await 解开 ——
            //       编译器会直接报错：error CS1996「无法在 lock 语句的正文中等待」。
            //       这门语言【故意】不让你在 lock 里 await。
            //       为什么？因为 lock 是按【线程】记账的：
            //         · await 会让出线程，锁却还被"这个线程"持有 —— 语义立刻崩坏；
            //         · 别的线程可能拿到同一把锁，于是"互斥"名存实亡。
            //   结论：异步代码要用 SemaphoreSlim.WaitAsync()（不占线程的锁）。
            lock (Gate)
            {
                Console.WriteLine($"      任务 {index}：拿到锁（线程 {Tid()}）");
                // await Task.Delay(50);   // ← 解开这一行，你会拿到 error CS1996
            }
            await Task.Delay(20);
            Console.WriteLine($"      任务 {index}：做完（线程 {Tid()}）");
        }

        /// <summary>把调用栈打出来：证明 await "返回"过，而不是"停在那里"。</summary>
        static async Task ShowStackAsync()
        {
            Console.WriteLine("    进入 ShowStackAsync，此刻的调用栈：");
            PrintFrames();

            await Task.Delay(50);

            Console.WriteLine();
            Console.WriteLine("    await 恢复之后的调用栈：");
            PrintFrames();
            Console.WriteLine();
            Console.WriteLine("    ★ 看到没？ShowStackAsync 上面那几层（RunDemo5Async 等）全都不见了，");
            Console.WriteLine("      只剩下「线程池 → MoveNext」两三层。");
            Console.WriteLine("      原因：当初 await 的时候这个方法的栈帧已经【弹出并销毁】了，");
            Console.WriteLine("      现在这一帧是【新建】的，恢复地址来自状态机对象（堆上），不是栈。");
        }

        static void PrintFrames()
        {
            // ★ StackTrace 默认会省掉自己的第一帧（SkipFrames = 1），所以这里不用减
            var trace = new StackTrace();
            // GetFrames() 的 Nullable 标注是 StackFrame[]?，所以要处理可能为 null
            StackFrame[]? frames = trace.GetFrames();
            if (frames is null)
            {
                Console.WriteLine("      （拿不到调用栈）");
                return;
            }

            foreach (StackFrame frame in frames)
            {
                MethodBase? method = frame.GetMethod();
                if (method is null)
                {
                    continue;
                }
                string typeName = method.DeclaringType?.Name ?? "?";
                Console.WriteLine($"      {typeName}.{method.Name}");
            }
        }

        /// <summary>会抛异常的 async Task。</summary>
        static async Task ThrowsAsync()
        {
            await Task.Delay(20);
            throw new InvalidOperationException("异步方法内部抛出的异常");
        }

        // ────────────────────────────────────────────────────────────────
        //  顶部：状态机反射工具
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// 把一个"async 方法的状态机类型"摊开打印。
        /// ★ 这是本课最"底层"的一段：你能亲眼看到编译器生成的类型名和字段。
        /// </summary>
        static void DumpStateMachine(Type smType)
        {
            string fullName = smType.FullName ?? smType.Name;
            Console.WriteLine($"  完整名字：{fullName}");
            Console.WriteLine($"  是值类型（struct）：{smType.IsValueType,-6}  ← true = 状态机先活在栈上，只有等不下去时才装箱到堆（省 GC）");
            Console.WriteLine($"  嵌套在：{smType.DeclaringType?.Name ?? "?"}");
            Console.WriteLine();
            Console.WriteLine("  它实现了这些接口：");

            Type[] interfaces = smType.GetInterfaces();
            if (interfaces.Length == 0)
            {
                Console.WriteLine("    （没有公开接口）");
            }
            foreach (Type itf in interfaces)
            {
                Console.WriteLine($"    · {itf.Name}");
            }

            Console.WriteLine();
            Console.WriteLine("  【字段】—— 这就是 await 期间「活下来」的全部家当：");

            // BindingFlags 要写全，否则 private 字段一个都看不到
            FieldInfo[] fields = smType.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo f in fields)
            {
                Console.WriteLine($"    {f.FieldType.Name,-34} {f.Name}");
            }
            Console.WriteLine();
            Console.WriteLine("  【方法】：");
            foreach (MethodInfo m in smType.GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                Console.WriteLine($"    {m.Name}");
            }
        }

        // ================================================================
        //  主流程
        // ================================================================

        // ★ 注意这个签名：static async Task Main()
        //   async 的 Main 是 C# 7.3 之后支持的，编译器会替你生成
        //   「同步的 Main → 调 GetAwaiter().GetResult()」那层包装。
        //   教学上这是最干净的写法：本课讲的 await 在 Main 里也成立。
        static async Task Main()
        {
            Console.WriteLine("===== 第 24 课：async / await 状态机 =====");
            Console.WriteLine();
            Console.WriteLine("  一句话：await 不是「暂停线程」，而是「把方法拆成多段并返回」。");
            Console.WriteLine($"  本示例全程约 3~4 秒。进程可用线程数上限：{ThreadPool.MaxThreads}（本机 CPU 核数 {Environment.ProcessorCount}）");
            Console.WriteLine();

            await RunDemo1ThreadSwitchAsync();
            await RunDemo2StateMachineAsync();
            await RunDemo3ConcurrencyAsync();
            await RunDemo4BlockingAsync();
            await RunDemo5StackAsync();
            await RunDemo6SyncContextAsync();
            await RunDemo7DeadlockAsync();
            RunDemo8Summary();

            Console.WriteLine("===== 示例结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }

        // ────────────────────────────────────────────────────────────────
        //  一、await 前后线程 ID 的对比
        // ────────────────────────────────────────────────────────────────
        static async Task RunDemo1ThreadSwitchAsync()
        {
            Console.WriteLine("---------- 一、await 前后，线程 ID 到底变不变 ----------");
            Console.WriteLine();

            Log("Main 开始");
            int before = Tid();

            await ThreadHopAsync();

            int after = Tid();
            Log("回到 Main（Main 的 await 之后）");
            Console.WriteLine();

            Console.WriteLine("  【怎么读这个输出】");
            Console.WriteLine("    · await 之前那行：线程号和 Main 一样（同步部分在调用者线程上跑）");
            Console.WriteLine("    · 之后几行：线程号往往变了，而且标着「线程池」");
            Console.WriteLine("    · Task.Yield 之后【几乎一定】变 —— 它是强制让出的标准手段");
            Console.WriteLine();
            Console.WriteLine($"  Main 自己的线程 {before} → 回到 Main 时是 {after}，相同：{before == after}");
            Console.WriteLine("  ★ 但请注意：Main 里这个「相同」是巧合（见下面第二段）。");
            Console.WriteLine("    绝对不要写依赖它的代码。");
            Console.WriteLine();

            // 第二段：证明"相同是巧合"——把一个只有 yield 的方法连调几次
            Console.WriteLine("  再做个更硬的实验：连着 5 次 await Task.Yield()，看线程怎么跳");
            var hops = new StringBuilder();
            for (int i = 0; i < 5; i++)
            {
                await Task.Yield();
                hops.Append(Tid()).Append(i < 4 ? " → " : "");
            }
            Console.WriteLine($"    线程轨迹：{hops}");
            Console.WriteLine("    ★ 线程池会把你扔到【任意一个空闲线程】上，轨迹每次运行都不一样。");
            Console.WriteLine();

            // 第三段：AsyncLocal vs 线程
            Console.WriteLine("  第三段：线程变了，逻辑上下文会丢吗？（AsyncLocal）");
            await AsyncLocalAsync();
            Console.WriteLine();
            Console.WriteLine("  Python 对照：");
            Console.WriteLine("    Python 的 contextvars.ContextVar 也是跟着任务走的。");
            Console.WriteLine("    但 Python 里根本不会出现「线程变了」这回事 —— 因为只有一个线程。");
            Console.WriteLine("    所以这个知识点在 Python 里是「体会不到」的，在 C# 里是「必须懂」的。");
            Console.WriteLine();
        }

        // ────────────────────────────────────────────────────────────────
        //  二、async 方法被编译成了什么
        // ────────────────────────────────────────────────────────────────
        static async Task RunDemo2StateMachineAsync()
        {
            Console.WriteLine("---------- 二、async 方法 = 编译器生成的状态机 ----------");
            Console.WriteLine();
            Console.WriteLine("  你写的：");
            Console.WriteLine("      static async Task ThreadHopAsync() { ... await Task.Delay(80); ... }");
            Console.WriteLine();
            Console.WriteLine("  编译器实际生成（大致形状，去 ILSpy 里能看到真货）：");
            Console.WriteLine();
            Console.WriteLine("      [CompilerGenerated]");
            Console.WriteLine("      private struct <ThreadHopAsync>d__8 : IAsyncStateMachine");
            Console.WriteLine("      {");
            Console.WriteLine("          public int <>1__state;                      // ★ 执行到第几段了");
            Console.WriteLine("          public AsyncTaskMethodBuilder <>t__builder;  // 那个返回给调用方的 Task");
            Console.WriteLine("          public TaskAwaiter <>u__1;                   // 当前这次 await 的 awaiter");
            Console.WriteLine("          public int <localVar>5__2;                   // 你的局部变量（被提升成字段）");
            Console.WriteLine();
            Console.WriteLine("          void IAsyncStateMachine.MoveNext() { ... }   // ★ 分段执行的核心");
            Console.WriteLine("          void IAsyncStateMachine.SetStateMachine(...) { }");
            Console.WriteLine("      }");
            Console.WriteLine();

            // 用反射把真货摊开 —— 上面那段注释是我手写的"形状"，
            // 下面这段是【运行时真实类型】，不会有任何美化。
            Console.WriteLine("  下面这段不是注释，是真实反射结果：");
            Console.WriteLine();

            // ★ 这里【不能】把类型名写死成 "<ThreadHopAsync>d__8"：
            //   名字里那个数字是编译器给的第 N 个状态机，你改一行代码它就可能变。
            //   所以按【命名规律】去反射查找 —— 这本身也顺便教了你状态机的命名规则。
            Type? smType = null;
            foreach (Type nested in typeof(Lesson24Demo).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (nested.Name.StartsWith("<ThreadHopAsync>", StringComparison.Ordinal))
                {
                    smType = nested;
                    break;
                }
            }
            if (smType is null)
            {
                Console.WriteLine("  （没找到状态机类型 —— 名字规律变了？去 ILSpy 里手动找 <ThreadHopAsync>d__N）");
                Console.WriteLine();
            }
            else
            {
                DumpStateMachine(smType);
            }

            Console.WriteLine("  ★ 三个一定要记住的点：");
            Console.WriteLine("    1. <>1__state 就是「第几段」。MoveNext() 一进来先 switch(state)，跳到该继续的位置。");
            Console.WriteLine("       所以 await 恢复时【不是重新执行方法】，而是直接跳进后半段。");
            Console.WriteLine("    2. 你的局部变量活着，并不是因为线程记着它 —— 而是因为它被搬进了状态机对象的字段。");
            Console.WriteLine("       【栈在 await 时就没了，数据在堆上】。这是理解 async 的钥匙。");
            Console.WriteLine("    3. 状态机是 struct（值类型），第一次 await 未完成时才被装箱到堆上。");
            Console.WriteLine("       → 如果一个 async 方法全程不 await（或 await 的东西已完成），它可以完全不分配！");
            Console.WriteLine();

            // 亲手验证第 2 条：await 前后局部变量的值
            Console.WriteLine("  亲手验证第 2 条（局部变量在 await 之后还活着）：");
            await VerifyLocalSurvivesAsync();
            Console.WriteLine();

            Console.WriteLine("  ★ 顺便解释一个你迟早会遇到的怪名字：");
            Console.WriteLine("     堆栈里 / 日志里出现 <FooAsync>d__12.MoveNext()，");
            Console.WriteLine("     这不是 bug，就是编译器生成的状态机在跑。看到它就想到本课。");
            Console.WriteLine();
            Console.WriteLine("  ★ async 的 Main 也是同一套机制：编译器生成了一个");
            Console.WriteLine("     <Main>$ 状态机，外面再套一个同步 Main 负责 GetAwaiter().GetResult()。");
            Console.WriteLine();
        }

        static async Task VerifyLocalSurvivesAsync()
        {
            // ① 一个普通局部变量，await 之前赋值
            int secret = 12345;
            // ② 一个引用类型局部变量
            var box = new StringBuilder("await 之前");
            // ③ 一个在 await 之后才用到的变量
            string later = "";

            Console.WriteLine($"    await 之前：secret = {secret}，box = '{box}'，线程 {Tid()}");

            await Task.Delay(30);

            // await 之后接着用这三个变量 —— 如果它们真"死在栈上"了，这里就该是垃圾值。
            // 实际上值完好，因为它们已经变成了状态机对象的字段。
            later = "await 之后";
            Console.WriteLine($"    await 之后：secret = {secret}，box = '{box}'，later = '{later}'，线程 {Tid()}");
        }

        // ────────────────────────────────────────────────────────────────
        //  三、顺序 await vs Task.WhenAll
        // ────────────────────────────────────────────────────────────────
        static async Task RunDemo3ConcurrencyAsync()
        {
            Console.WriteLine("---------- 三、顺序 await vs Task.WhenAll（实测耗时）----------");
            Console.WriteLine();
            Console.WriteLine($"  三个任务，每个都是 await Task.Delay({FakeIoMs})。");
            Console.WriteLine();

            // ── 顺序：一个等完再起下一个 ──
            var sw = Stopwatch.StartNew();

            int a1 = await SimulateIoAsync(1, FakeIoMs);
            int a2 = await SimulateIoAsync(2, FakeIoMs);
            int a3 = await SimulateIoAsync(3, FakeIoMs);

            sw.Stop();
            long sequentialMs = sw.ElapsedMilliseconds;
            Console.WriteLine($"  ① 顺序 await 三次：{sequentialMs,4} ms   结果 {a1}, {a2}, {a3}");

            // ── 并发：先把三个任务全"启动"，再一起等 ──
            sw.Restart();

            // ★★ 这一行是本节的灵魂，请盯着它看 10 秒：
            //     Task t1 = SimulateIoAsync(1, ...) —— 【调用】这个 async 方法，
            //     方法体会立刻开始执行，跑到第一个 await（Task.Delay）才返回。
            //     返回的是一个【已经在跑的】未完成任务。
            //
            //     Python 里不是这样：
            //         coro = fetch(1)          # 什么都没发生！只是造了个协程对象
            //         await coro               # 到这一行才开始跑
            //     所以 Python 必须 asyncio.gather(fetch(1), fetch(2))，
            //     C# 里光靠「先调用、后 await」就已经并发了。
            Task<int> t1 = SimulateIoAsync(1, FakeIoMs);
            Task<int> t2 = SimulateIoAsync(2, FakeIoMs);
            Task<int> t3 = SimulateIoAsync(3, FakeIoMs);

            int[] results = await Task.WhenAll(t1, t2, t3);

            sw.Stop();
            long concurrentMs = sw.ElapsedMilliseconds;
            Console.WriteLine($"  ② Task.WhenAll 三个：{concurrentMs,4} ms   结果 {string.Join(", ", results)}");
            Console.WriteLine();

            // ★ 用整毫秒做除法会得到 0，所以用 TotalMilliseconds 算倍数
            double speedup = concurrentMs > 0 ? (double)sequentialMs / concurrentMs : 0;
            Console.WriteLine($"  理论值：顺序 = 3 × {FakeIoMs} = {3 * FakeIoMs} ms；并发 ≈ {FakeIoMs} ms");
            Console.WriteLine($"  实测倍数：{speedup:F2} 倍");
            Console.WriteLine();
            Console.WriteLine("  ★ 结论：await 是「等」，不是「启动」。想让 N 件事同时进行，");
            Console.WriteLine("     就必须在 await 之前把 N 个 Task 全创建出来。");
            Console.WriteLine("     写成 await A(); await B(); 是【串行】，和写同步代码一样慢。");
            Console.WriteLine();

            // ── 反例：忘了"先创建" ──
            Console.WriteLine("  最容易犯的错，长这样：");
            Console.WriteLine("      var results = await Task.WhenAll(");
            Console.WriteLine("          FetchAsync(1),   // ← 这样写【是对的】");
            Console.WriteLine("          FetchAsync(2));  //    参数会在 WhenAll 调用前全部求值");
            Console.WriteLine();
            Console.WriteLine("  但下面这种就退化成串行了：");
            Console.WriteLine("      var t1 = FetchAsync(1); var r1 = await t1;   // ❌ 等完了才起第二个");
            Console.WriteLine("      var t2 = FetchAsync(2); var r2 = await t2;");
            Console.WriteLine();

            // ── 一条一条处理、谁先完谁先来 ──
            Console.WriteLine("  进阶：谁先完成谁先处理（不用等最慢的那个）—— Task.WhenAny");
            var tasks = new List<Task<int>>
            {
                SimulateIoAsync(1, 120),
                SimulateIoAsync(2, 40),      // 这个最快
                SimulateIoAsync(3, 80)
            };
            sw.Restart();
            while (tasks.Count > 0)
            {
                Task<int> finished = await Task.WhenAny(tasks);
                tasks.Remove(finished);
                Console.WriteLine($"      完成：id {finished.Result / 10}，耗时 {sw.ElapsedMilliseconds,4} ms");
            }
            sw.Stop();
            Console.WriteLine("    ★ Python 对照：Task.WhenAny ≈ asyncio.wait(..., return_when=FIRST_COMPLETED)");
            Console.WriteLine("      （注意 finished.Result 在这里是安全的：WhenAny 已经保证它完成了）");
            Console.WriteLine();
        }

        // ────────────────────────────────────────────────────────────────
        //  四、阻塞调用：一个线程被占住，世界并不会停
        // ────────────────────────────────────────────────────────────────
        static async Task RunDemo4BlockingAsync()
        {
            Console.WriteLine("---------- 四、阻塞调用 vs 异步等待 ----------");
            Console.WriteLine();
            Console.WriteLine("  同样是「等 300 毫秒」，两种写法对线程的占用完全不同。");
            Console.WriteLine();

            // ① 阻塞：Thread.Sleep 把当前线程按在 CPU 调度队列里睡 300ms
            var sw = Stopwatch.StartNew();
            int before = Tid();
            Thread.Sleep(300);
            int after = Tid();
            sw.Stop();
            Console.WriteLine($"  ① Thread.Sleep(300)   线程 {before} → {after}，耗时 {sw.ElapsedMilliseconds} ms（线程换了没：{before != after}）");

            // ② 异步：Task.Delay 只是"定个闹钟"，线程立刻被还回去
            sw.Restart();
            before = Tid();
            await Task.Delay(300);
            after = Tid();
            sw.Stop();
            Console.WriteLine($"  ② await Task.Delay(300) 线程 {before} → {after}，耗时 {sw.ElapsedMilliseconds} ms（线程换了没：{before != after}）");
            Console.WriteLine();
            Console.WriteLine("  ★ 两边耗时几乎一样，区别在【这 300ms 里那个线程在干什么】：");
            Console.WriteLine("     · Thread.Sleep：线程被占着，什么都干不了（浪费一个线程池线程）");
            Console.WriteLine("     · Task.Delay  ：线程被还回池子，可以去服务别的请求");
            Console.WriteLine();

            Console.WriteLine("  为什么这件事在 C# 里是「性能问题」，在 Python 里是「生死问题」？");
            Console.WriteLine("     · C#  ：线程池有几十上百个线程，睡死几个还能撑住");
            Console.WriteLine("     · Python：asyncio 只有一个线程在跑事件循环，");
            Console.WriteLine("              你在协程里 time.sleep(300) 就是把整个事件循环按住 300ms，");
            Console.WriteLine("              【所有】并发任务一起卡住。这就是 C# 的坑只坑自己，Python 的坑坑全体。");
            Console.WriteLine();

            // ③ 一个更隐蔽的坑：Task.Run 里套同步阻塞
            Console.WriteLine("  隐蔽坑：Task.Run(() => Thread.Sleep(...)) 看起来异步，其实是找了个线程去睡");
            sw.Restart();
            await Task.Run(() => Thread.Sleep(200));
            sw.Stop();
            Console.WriteLine($"      耗时 {sw.ElapsedMilliseconds} ms —— 能跑，但白占一个线程池线程");
            Console.WriteLine("      ✅ 正解：能用真正的异步 API 就用（File.ReadAllTextAsync、HttpClient.GetStringAsync…），");
            Console.WriteLine("         实在只有同步 API 才用 Task.Run 把它扔到线程池。");
            Console.WriteLine();
            Console.WriteLine("  ★ 判断标准（背下来）：");
            Console.WriteLine("     这个操作在等的时候，需不需要一个线程站着？");
            Console.WriteLine("     不需要 → 用 async（Delay / ReadAsync / WriteAsync）");
            Console.WriteLine("     需要   → 它是 CPU 计算，用 Task.Run 交给线程池，或者干脆同步跑");
            Console.WriteLine();
        }

        // ────────────────────────────────────────────────────────────────
        //  五、await 之后，调用栈去哪儿了
        // ────────────────────────────────────────────────────────────────
        static async Task RunDemo5StackAsync()
        {
            Console.WriteLine("---------- 五、await 之后，调用栈去哪儿了 ----------");
            Console.WriteLine();
            Console.WriteLine("  「方法被拆成多段并返回」这句话，看调用栈最直观。");
            Console.WriteLine();

            await ShowStackAsync();

            Console.WriteLine("  ★ 这就是「返回」的字面意思：await 那一刻，方法真的 return 了，");
            Console.WriteLine("     调用链一路弹回去，把线程还给了别人。");
            Console.WriteLine("     它不是「暂停」（暂停意味着栈还留着、线程还占着），它是「撤离」。");
            Console.WriteLine();

            // 反面教材：锁里 await
            Console.WriteLine("  反面教材：在 lock 里 await");
            var sw = Stopwatch.StartNew();
            await Task.WhenAll(LockInsideAsync(1), LockInsideAsync(2), LockInsideAsync(3));
            sw.Stop();
            Console.WriteLine($"    （三个任务总耗时 {sw.ElapsedMilliseconds} ms）");
            Console.WriteLine("    ★ 注意上面「拿到锁」那三行：它们是【先后】出现的 —— 因为 lock 是互斥的；");
            Console.WriteLine("      而真正的 await 在 lock 外面，所以三个任务又并发跑起来了。");
            Console.WriteLine("      现在去 Lesson24Demo.cs 里把 LockInsideAsync 中那行 await Task.Delay(50);");
            Console.WriteLine("      的注释解开（放进 lock 里面），再跑一遍：");
            Console.WriteLine("        · 你会直接拿到编译错误 CS1996「无法在 lock 语句的正文中等待」——");
            Console.WriteLine("          语言在设计上就不允许这件事，因为 lock 按线程记账，而 await 会换线程。");
            Console.WriteLine("        · 想真的「锁住一段时间」，只能用不会换线程的阻塞等待（那是反模式），");
            Console.WriteLine("          或者换成 SemaphoreSlim.WaitAsync()（见下面）。");
            Console.WriteLine("    ★ 更危险的变体：如果你的锁保护的是「只有某个线程能碰的资源」（比如控件），");
            Console.WriteLine("      在临界区里 await 再回来，就可能在别的线程上碰它 —— 直接抛异常。");
            Console.WriteLine("      ✅ 异步代码的并发控制：SemaphoreSlim.WaitAsync()（不占线程的锁）。");
            Console.WriteLine();
        }

        // ────────────────────────────────────────────────────────────────
        //  六、SynchronizationContext：await 之后我在哪个线程
        // ────────────────────────────────────────────────────────────────
        static async Task RunDemo6SyncContextAsync()
        {
            Console.WriteLine("---------- 六、SynchronizationContext：谁决定我在哪个线程恢复 ----------");
            Console.WriteLine();

            SynchronizationContext? ctx = SynchronizationContext.Current;
            // ★ 这里会打印 null，而且是【故意的】：
            //   .NET Core 之后，控制台/服务端默认不再安装 SynchronizationContext。
            //   → 于是 await 的续体一律扔回线程池（哪个线程有空就用哪个）。
            //   → 这就是为什么控制台里"线程 ID 天天变"。
            Console.WriteLine($"  当前 SynchronizationContext：{(ctx is null ? "null" : ctx.GetType().Name)}");
            Console.WriteLine();
            Console.WriteLine("  同一个 await，在三种环境里的落点完全不同：");
            Console.WriteLine();
            Console.WriteLine("  ┌────────────────┬──────────────────────────────────────────────┐");
            Console.WriteLine("  │ 环境           │ await 续体在哪里恢复                          │");
            Console.WriteLine("  ├────────────────┼──────────────────────────────────────────────┤");
            Console.WriteLine("  │ 控制台 / 服务端│ 线程池的任意线程（没有上下文，随便挑一个）    │");
            Console.WriteLine("  │ WPF / WinForms │ 被投递回 UI 线程（因为有 SynchronizationContext）│");
            Console.WriteLine("  │ ASP.NET Core   │ 线程池（每个请求没有专属线程）                │");
            Console.WriteLine("  └────────────────┴──────────────────────────────────────────────┘");
            Console.WriteLine();
            Console.WriteLine("  ★ 所以「await 之后能不能直接改 UI」的答案是：");
            Console.WriteLine("     能 —— 但靠的不是语言，是 SynchronizationContext 把续体搬回了 UI 线程。");
            Console.WriteLine("     控制台里没有它，你就得自己管线程（或者干脆用锁/Channel 传递结果）。");
            Console.WriteLine();

            // 观察点：await 之前的同步代码一定在调用者线程上
            Log("准备调用 MostlySyncAsync");
            await MostlySyncAsync();
            Console.WriteLine("    ★ 注意最上面那行「同步部分」：它和调用者是同一个线程 —— ");
            Console.WriteLine("      async 方法在遇到第一个【未完成】的 await 之前，是完全同步执行的。");
            Console.WriteLine();

            // ConfigureAwait 演示
            Console.WriteLine("  ConfigureAwait(false) 在控制台里看不出差别（本来就没有上下文要回）：");
            Log("await 前");
            await Task.Delay(50).ConfigureAwait(false);
            Log("ConfigureAwait(false) 之后");
            Console.WriteLine();
            Console.WriteLine("  但在 WPF 里差别巨大：");
            Console.WriteLine("      await Task.Delay(50);                       // 回到 UI 线程");
            Console.WriteLine("      await Task.Delay(50).ConfigureAwait(false);  // ❌ 可能落在别的线程");
            Console.WriteLine("                                                    //    下一行改控件就抛异常");
            Console.WriteLine();
            Console.WriteLine("  ★ 铁律 3 的完整版：");
            Console.WriteLine("      · 写【库】（不知道谁来调）→ 每个 await 都加 .ConfigureAwait(false)");
            Console.WriteLine("        好处：不需要回 UI 线程 → 少一次调度、少一次死锁风险、更快");
            Console.WriteLine("      · 写【UI/事件处理器】（要碰控件）→ 不加，让它回到 UI 线程");
            Console.WriteLine();
        }

        // ────────────────────────────────────────────────────────────────
        //  七、.Result / .Wait() 的死锁：成因 + 安全复现
        // ────────────────────────────────────────────────────────────────
        static async Task RunDemo7DeadlockAsync()
        {
            Console.WriteLine("---------- 七、.Result 死锁：亲手把它复现出来（但不会挂住）----------");
            Console.WriteLine();
            Console.WriteLine("  先看「为什么会死锁」的四步：");
            Console.WriteLine();
            Console.WriteLine("    UI 线程                          异步方法（FetchAsync）");
            Console.WriteLine("    ────────                         ──────────────────────");
            Console.WriteLine("    ① 调 FetchAsync() ─────────────→ 开始跑，遇到 await，");
            Console.WriteLine("                                      把续体登记到 UI 线程的");
            Console.WriteLine("                                      SynchronizationContext 上");
            Console.WriteLine("    ② 拿到未完成的 Task             ←── return（人走了）");
            Console.WriteLine("    ③ 调 .Result");
            Console.WriteLine("       → 阻塞 UI 线程 ⛔");
            Console.WriteLine("                                      Task 完成，上下文说：");
            Console.WriteLine("                                      把续体投递回 UI 线程");
            Console.WriteLine("    ④ 等续体执行                     ⑤ 续体排在 UI 线程的队列里");
            Console.WriteLine("       ↑___________死锁___________↓     等 UI 线程空出来");
            Console.WriteLine();
            Console.WriteLine("  一句话成因：");
            Console.WriteLine("     .Result 阻塞的那个线程，正是续体【唯一被允许运行】的线程。");
            Console.WriteLine("     我等你，你等我 → 谁都不动。");
            Console.WriteLine();
            Console.WriteLine("  ★ 关键认知：这不是「UI 线程天生特殊」，而是「有 SynchronizationContext 时才会」。");
            Console.WriteLine("     控制台程序没有上下文 → 续体去线程池 → .Result 其实能跑完（但依然是坏习惯）。");
            Console.WriteLine("     所以要复现死锁，我们必须【自己造一个上下文】。");
            Console.WriteLine();

            // ── 沙盒 1：有上下文 + .Result → 死锁 ──
            Console.WriteLine("  ── 复现 ①：有 SynchronizationContext，库代码没加 ConfigureAwait(false) ──");
            Console.WriteLine("     （这是个【模拟 UI 线程】的沙盒：后台一个线程跑循环，只认自己的队列。");
            Console.WriteLine("       1 秒等不到结果就判为死锁并退出，不会把示例挂住。）");
            Console.WriteLine();

            bool deadlocked = !CallWithResultUnderUiContext(syncContext: null, continueOnCaptured: true);
            Console.WriteLine($"     结果：{(deadlocked ? "❌ 死锁（续体永远排不到，1 秒超时）" : "意外地跑完了？")}");
            Console.WriteLine();

            // ── 沙盒 2：有上下文 + ConfigureAwait(false) → 活过来了 ──
            Console.WriteLine("  ── 复现 ②：同样的 .Result，但库代码加了 ConfigureAwait(false) ──");
            Console.WriteLine();

            bool alive = CallWithResultUnderUiContext(syncContext: null, continueOnCaptured: false);
            Console.WriteLine($"     结果：{(alive ? "✅ 跑完了！（续体不要求回 UI 线程，直接在线程池上跑）" : "❌ 居然死锁了")}");
            Console.WriteLine();
            Console.WriteLine("  ★ 这个对比是本课最有价值的一段：");
            Console.WriteLine("     · 库代码加 ConfigureAwait(false) 的理由，不只是「更快」，");
            Console.WriteLine("       更是【别把调用方拖进死锁】。");
            Console.WriteLine("     · 但反过来说：调用方永远不该写 .Result。");
            Console.WriteLine("       库作者做对了是运气，调用方写对了才是本分。");
            Console.WriteLine();

            // ── 正确姿势 ──
            Console.WriteLine("  ── 正确姿势（三选一，永远有得选）──");
            Console.WriteLine("     ✅ 一路 async 到底：static async Task Main()  // 本示例就是");
            Console.WriteLine("     ✅ 真的在同步方法里：task.GetAwaiter().GetResult()");
            Console.WriteLine("        至少它【不包装异常】（.Result 会把异常包成 AggregateException）");
            Console.WriteLine("     ✅ 或者 Task.Run(...).GetAwaiter().GetResult()  // 换个线程去等，不会和 UI 线程撞上");
            Console.WriteLine("     ❌ var data = FetchAsync().Result;   // 永远不要");
            Console.WriteLine("     ❌ FetchAsync().Wait();              // 同上，而且异常被包成 AggregateException");
            Console.WriteLine();
            Console.WriteLine("  ★ 本示例【故意】没有在 Main 里真的调 .Result ——");
            Console.WriteLine("     那个死锁在控制台里都可能把你卡到重启 IDE。");
            Console.WriteLine("     想体验真死锁：去写个 WPF 小程序，在按钮事件里写 .Result，见第八节。");
            Console.WriteLine();
        }

        /// <summary>
        /// 死锁沙盒。返回 true = 在超时前拿到了结果；false = 卡住了（死锁）。
        /// </summary>
        /// <param name="syncContext">要安装的上下文；传 null 表示内部自造一个「模拟 UI 线程」的上下文。</param>
        /// <param name="continueOnCaptured">true = 库代码写的是 await（会回上下文）；false = 写了 ConfigureAwait(false)。</param>
        static bool CallWithResultUnderUiContext(SynchronizationContext? syncContext, bool continueOnCaptured)
        {
            // 没传就自己造一个：单线程 + 只跑自己的队列，行为和 UI 线程一样
            var context = syncContext ?? new SingleThreadContext();
            var done = new ManualResetEventSlim(false);
            // ★ StrongBox<int> = 一个"能放进闭包的盒子"。
            //   普通 int 捕进 lambda 之后，外面读不到它被改成什么；
            //   用一个装在堆上的盒子，才能跨线程读到模拟 UI 线程的 ID。
            var uiThreadId = new StrongBox<int>(-1);

            // 起一个后台线程专门跑「上下文循环」——它就是那个被 .Result 阻塞的 UI 线程
            var runner = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(context);
                uiThreadId.Value = Environment.CurrentManagedThreadId;
                Console.WriteLine($"     模拟 UI 线程 = {uiThreadId.Value}（它接下来会被 .Result 阻塞住）");

                if (context is SingleThreadContext stc)
                {
                    stc.RunLoop(() => done.IsSet);
                }
            })
            {
                IsBackground = true,   // ★ 后台线程：即使它卡死，进程也能退出。这是沙盒的安全网
                Name = "模拟UI线程"
            };
            runner.Start();

            // 等模拟 UI 线程把上下文装好（最多等 500ms）
            for (int i = 0; i < 50 && uiThreadId.Value < 0; i++)
            {
                Thread.Sleep(10);
            }

            Task<int> call = MakeCallAsync(context, continueOnCaptured, uiThreadId.Value);
            Console.WriteLine($"     调用方线程 = {Tid()}：开始 .Result（同步阻塞自己）");

            // ★ 这里就是「.Result」—— 只不过换成了带超时的 Wait(TimeSpan)，
            //   免得演示真的把进程挂死。除了多一个超时，语义和 .Result 完全一样：
            //   【同步阻塞当前线程，等这个 Task 完成】。
            bool completed = call.Wait(TimeSpan.FromSeconds(1));

            if (completed)
            {
                Console.WriteLine($"     .Result 拿到结果：{call.Result}（线程 {Tid()}）");
            }
            else
            {
                Console.WriteLine($"     ⛔ 1 秒内没等到 —— 死锁确认。call.Status = {call.Status}");
            }

            // 收尾：让模拟 UI 线程的循环退出，不然它会一直在那转
            done.Set();
            if (context is SingleThreadContext stc2)
            {
                stc2.PostQuit();
            }

            // 等模拟 UI 线程退出（也给它一个上限，免得清理动作本身把示例挂住）
            runner.Join(TimeSpan.FromMilliseconds(500));

            return completed;
        }

        /// <summary>
        /// 模拟一段"库代码"。
        /// continueOnCaptured = true  → 相当于 await FooAsync();（会回上下文）
        /// continueOnCaptured = false → 相当于 await FooAsync().ConfigureAwait(false);
        /// </summary>
        static async Task<int> MakeCallAsync(SynchronizationContext context, bool continueOnCaptured, int uiThreadId)
        {
            // ★★ 本方法的全部看点在这一行：
            //     ConfigureAwait(false) 的作用【不是】"换个线程"，而是
            //     "别把续体 post 回我所在的 SynchronizationContext"。
            //     不 post 回上下文 → 上下文那个线程阻塞不阻塞就跟它没关系了 → 不会死锁。
            if (continueOnCaptured)
            {
                await Task.Yield();                                  // 默认行为：回上下文
            }
            else
            {
                await Task.Yield().ConfigureAwait(false);            // 不回上下文：扔给线程池
            }

            int resumeThread = Environment.CurrentManagedThreadId;
            bool backOnUi = resumeThread == uiThreadId;

            Console.WriteLine($"       续体恢复于线程 {resumeThread}（是不是那个 UI 线程：{backOnUi}）");
            Console.WriteLine($"       此处的 SynchronizationContext 是：{SynchronizationContext.Current?.GetType().Name ?? "null"}");

            return context.GetHashCode();
        }

        // ────────────────────────────────────────────────────────────────
        //  八、四条铁律 + 自己动手改坏它
        // ────────────────────────────────────────────────────────────────
        static void RunDemo8Summary()
        {
            Console.WriteLine("---------- 八、四条铁律 ----------");
            Console.WriteLine();
            Console.WriteLine("  ① 绝不用 .Result / .Wait()（UI 程序会死锁）");
            Console.WriteLine("       → 用 await；实在不行用 GetAwaiter().GetResult()");
            Console.WriteLine();
            Console.WriteLine("  ② 绝不写 async void（除事件处理器）");
            Console.WriteLine("       → async void 没有 Task 装异常，异常直接甩给 SynchronizationContext");
            Console.WriteLine("       → try/catch 包住调用点是【抓不到】的。为什么？下一段亲手验证。");
            Console.WriteLine();
            Console.WriteLine("  ③ 库代码用 .ConfigureAwait(false)，UI 代码不加");
            Console.WriteLine("       → 库：不需要回 UI 线程，少一次调度，也不会把调用方锁死");
            Console.WriteLine("       → UI：加了就回不到 UI 线程，改控件直接抛 InvalidOperationException");
            Console.WriteLine();
            Console.WriteLine("  ④ await 之后要更新 UI，靠的是 SynchronizationContext");
            Console.WriteLine("       → 它不是语言特性，是「谁来跑我的续体」的协议");
            Console.WriteLine("       → 控制台里它是 null，所以控制台的学习经验和 WPF 不能直接互搬");
            Console.WriteLine();

            Console.WriteLine("---------- 九、async void 的异常到底跑哪去了 ----------");
            Console.WriteLine();
            Console.WriteLine("  下面用一个【模拟 UI 上下文】把 async void 的异常抓个正着 ——");
            Console.WriteLine("  这样它就不会把示例进程打死，我们还能看清它去了哪里：");
            Console.WriteLine();

            var context = new SingleThreadContext();
            var gotError = false;
            Exception? captured = null;
            Exception? postedToContext = null;

            var runner = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(context);
                if (context is SingleThreadContext stc)
                {
                    // ★ 参数就是「框架里 UnhandledException 的位置」：
                    //   收到什么异常，就等于真实 UI 框架里会崩掉的那个异常。
                    stc.RunLoop(
                        () => gotError,
                        ex =>
                        {
                            postedToContext = ex;
                            gotError = true;
                        });
                }
            })
            {
                IsBackground = true,
                Name = "模拟UI线程-asyncvoid"
            };
            runner.Start();
            Thread.Sleep(100);   // 等上下文装好

            // ⚠️ 把异步方法投到这个上下文上跑。它内部会 await → 然后（在 await 之后）抛出异常。
            //    关键在于：async void 的异常【不会】从这行调用里冒出来，
            //    而是被 post 到当前 SynchronizationContext 上。
            context.Post(_ =>
            {
                try
                {
                    FireAndForgetOnContext(context);
                    // ★ 注意：这一行会打印 —— 也就是 try 块「正常结束了」。
                    //   它返回的时候方法体只跑到第一个 await，异常还没发生；
                    //   等异常真的发生时，调用点早就走人了 —— 没有任何人能接住它。
                    Console.WriteLine("      try 块正常结束了 —— 调用点完全不知道后面会出事");
                }
                catch (Exception ex)
                {
                    // ★ 这一行【永远不会】执行。这就是 async void 最要命的地方：
                    //   你以为 try/catch 防住了，其实异常根本不从这儿过。
                    captured = ex;
                    Console.WriteLine($"      不该出现的一行：catch 抓到了 {ex.GetType().Name}");
                }
            }, null);

            // 等异常被上下文捕获
            for (int i = 0; i < 50 && !gotError; i++)
            {
                Thread.Sleep(10);
            }

            if (context is SingleThreadContext stc2)
            {
                stc2.PostQuit();
            }
            runner.Join(TimeSpan.FromMilliseconds(500));

            Console.WriteLine();
            Console.WriteLine($"  try/catch 抓到了异常吗：{captured is not null}   ← false 才是正常的（说明抓不到）");
            Console.WriteLine($"  异常被 SynchronizationContext 接走了吗：{gotError}");
            if (postedToContext is not null)
            {
                Console.WriteLine($"  上下文接到的异常：{postedToContext.GetType().Name}：{postedToContext.Message}");
            }
            Console.WriteLine();
            Console.WriteLine("  ★ 结论：async void 抛异常时，调用点的 try/catch 是【无效的】。");
            Console.WriteLine("     · 异常不走「方法返回」这条路，所以调用点根本看不到它");
            Console.WriteLine("     · 它走的是「post 给当前 SynchronizationContext」这条路");
            Console.WriteLine("     · 在 WPF/WinForms 里：异常被扔到 Dispatcher 上 → 进程崩溃");
            Console.WriteLine("     · 在控制台里：异常被扔到线程池 → 进程崩溃");
            Console.WriteLine("     · 唯一的补救是全局兜底（AppDomain.UnhandledException），");
            Console.WriteLine("       但那时进程已经在死了，你什么都救不回来。");
            Console.WriteLine("  ★ 所以：只有【事件处理器】可以用 async void（框架要求签名必须是 void），");
            Console.WriteLine("     而且事件处理器内部必须自己 try/catch 到最外层。其他一律 async Task。");
            Console.WriteLine();
            Console.WriteLine("  ★ 顺便对比一下 async Task 的异常路径（这才是你该用的）：");
            try
            {
                await ThrowsAsync();
                Console.WriteLine("      （不会执行到这里）");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"      ✅ await 一个 Task，异常规规矩矩地抛在 await 那一行：{ex.Message}");
            }
            Console.WriteLine("      → 同一个异常，换个签名就从「进程崩溃」变成「一行 catch」。");
            Console.WriteLine("        这就是铁律 ② 的全部理由。");
            Console.WriteLine();

            Console.WriteLine("---------- 十、自己动手，把它改坏 ----------");
            Console.WriteLine();
            Console.WriteLine("  学底层机制最好的办法是「改坏它，看它怎么坏」。按顺序做这 6 个实验：");
            Console.WriteLine();
            Console.WriteLine("  ① 把 ThreadHopAsync 里的 await Task.Delay(80) 改成 Thread.Sleep(80)");
            Console.WriteLine("     → 观察线程 ID：不再跳了（因为压根没让出线程）。");
            Console.WriteLine("       这是本课最值钱的一次实验：你亲眼看到「不 await = 不换线程」。");
            Console.WriteLine("       ⚠️ 编译器【连警告都不会给】—— 因为方法里还有别的 await。");
            Console.WriteLine("          这也是它危险的地方：一个 Thread.Sleep 就能悄悄毁掉整个异步链。");
            Console.WriteLine();
            Console.WriteLine("  ② 把三、里 Task.WhenAll 那段拆成 3 个顺序 await");
            Console.WriteLine("     → 耗时从 ~150ms 变成 ~450ms。多跑几遍看数字是否稳定。");
            Console.WriteLine();
            Console.WriteLine("  ③ 把 ThreadHopAsync 里两个 await 全删掉");
            Console.WriteLine("     → 编译器给 warning CS1998「此异步方法缺少 await 运算符」；");
            Console.WriteLine("       同时返回值类型变成 Task，接口没变，但方法已经纯同步了。");
            Console.WriteLine("       记住这条警告：它通常意味着你写了「假异步」。");
            Console.WriteLine("       （顺带一提：把 ThreadHopAsync 改成返回 Task 且不 async，");
            Console.WriteLine("         再用 Task.CompletedTask 返回，性能更好也更诚实。）");
            Console.WriteLine();
            Console.WriteLine("  ④ 在 VerifyLocalSurvivesAsync 里用 ref 参数去接 secret");
            Console.WriteLine("     → 编译不过（不能在 async 方法里对局部变量取 ref：CS1628 一类）。");
            Console.WriteLine("       这正是「状态机字段搬走了局部变量」这条规则的副产品：");
            Console.WriteLine("       跨 await 活着的东西必须能放进对象字段，栈上的引用不行。");
            Console.WriteLine();
            Console.WriteLine("  ⑤ 在 MakeCallAsync 里把 ConfigureAwait(false) 拿掉（变成 true）");
            Console.WriteLine("     → 复现 ② 立刻变回死锁。这条最能说明 ConfigureAwait 的真实价值。");
            Console.WriteLine();
            Console.WriteLine("  ⑥ 打开 ILSpy，找状态机类型 <ThreadHopAsync>d__N（N 是编译器给的编号）");
            Console.WriteLine("     → 亲眼看到 <>1__state 和 MoveNext()（第 17 课学的 IL 在这里派上用场）。");
            Console.WriteLine();
            Console.WriteLine("  ⑦ （选做，需要 WPF）新建一个 WPF 程序，按钮里写：");
            Console.WriteLine("        var s = FetchAsync().Result;   // 死锁，界面直接卡住");
            Console.WriteLine("        // 改成 await FetchAsync(); 立刻正常");
            Console.WriteLine("     亲眼看一次真死锁，比读十遍原理都管用。");
            Console.WriteLine();
        }

        /// <summary>
        /// 一个"忘了写 try/catch 的 async void"—— 异常会直接 post 给当前上下文。
        /// ★ 这是教学脚手架，不是实际写法。
        /// </summary>
        static async void FireAndForgetOnContext(SynchronizationContext context)
        {
            // ConfigureAwait(true) 就是默认行为：续体回到我所在的上下文。
            // （这里的参数只是为了在代码里把这件事写明，写成 await Task.Delay(20) 效果一样。）
            await Task.Delay(20).ConfigureAwait(true);

            // ★★ 这一行是本节的主角。
            //    它不是"返回给调用方"的异常 —— 因为没有 Task 可以装它。
            //    它是被「post 到 SynchronizationContext」的异常。
            //    换成 WPF，这一行就等于界面直接崩掉。
            //    ★ 对照实验：把上面那行 await 改成 Thread.Sleep(20)（同步阻塞），
            //      异常就会当场从这一行直接抛给调用方 —— 但那已经不是 async 了。
            Console.WriteLine("      （async void 内部：现在抛一个异常，看它往哪跑）");
            throw new InvalidOperationException("async void 里抛出的异常，调用方接不住");
        }

        // ================================================================
        //  模拟 UI 线程的 SynchronizationContext
        //  （不是 UI 框架的一部分，是手动实现的最小可运行版本）
        // ================================================================
        sealed class SingleThreadContext : SynchronizationContext
        {
            readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> queue = new();

            /// <summary>把活儿扔进队列就返回（UI 消息泵就是这个语义）。</summary>
            public override void Post(SendOrPostCallback d, object? state)
            {
                queue.Add((d, state));
            }

            /// <summary>
            /// 在"UI 线程"上同步执行。注意：如果这个线程正在被 .Result 阻塞着，
            /// 它就永远执行不到这里 —— 那一整张图讲的就是这件事。
            /// </summary>
            public override void Send(SendOrPostCallback d, object? state)
            {
                d(state);
            }

            /// <summary>让循环退出（清理用）。</summary>
            public void PostQuit()
            {
                queue.CompleteAdding();
            }

            /// <summary>
            /// UI 消息泵的最小实现：一条条取出来执行。
            /// </summary>
            /// <param name="shouldStop">返回 true 表示可以收工了。</param>
            /// <param name="onUnhandled">
            /// 有人往这个上下文上扔了没接住的异常时调用 ——
            /// 这就是 Dispatcher.UnhandledException / Application.ThreadException 的位置。
            /// 真实 UI 框架在这里会让进程崩溃；我们记下来，好让示例跑完。
            /// </param>
            public void RunLoop(Func<bool> shouldStop, Action<Exception>? onUnhandled = null)
            {
                try
                {
                    foreach ((SendOrPostCallback callback, object? state) in queue.GetConsumingEnumerable())
                    {
                        try
                        {
                            callback(state);
                        }
                        catch (Exception ex)
                        {
                            onUnhandled?.Invoke(ex);
                            Console.WriteLine("      ⚠️ 上下文接住了一个没人处理的异常（真实 UI 框架在这里会让进程崩溃）");
                        }

                        if (shouldStop())
                        {
                            return;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // CompleteAdding 之后 GetConsumingEnumerable 会正常结束，这里兜一下
                }
            }
        }
    }
}
