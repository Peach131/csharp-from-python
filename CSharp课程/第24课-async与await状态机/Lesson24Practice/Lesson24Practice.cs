// ============================================================================
//  第 24 课 · 练习：async / await 状态机
//
//  ★ 本课唯一要证明的一句话：
//
//        await 不是「暂停线程」，而是「把方法拆成多段并返回」。
//
//  ★ 和前面几课最大的不同：
//      这一课的 Main 是  static async Task Main()
//      —— 因为在 Main 里也要 await。这是现代 C# 支持的标准写法：
//         编译器会在背后生成一个同步的 Main，由它调用 GetAwaiter().GetResult()。
//
//  ★ 做题顺序建议：先把 Lesson24Demo 跑一遍，再回来做这里。
//    每做完一个 TODO 就 Ctrl + F5 跑一次，别攒着一起跑。
//
//  ★ 关于下面的"辅助方法"区：
//      里面【已经写好】的几个方法是本课的脚手架（打印线程、假 IO、状态机样本、
//      模拟 UI 上下文）。它们写全是为了让这个文件一打开就能 0 警告编译通过。
//      ★ 真正要你写的是 Main 里 5 个 TODO 的填空区 —— 那才是本课的考核内容。
//      只有一个例外：TODO 3 要你自己【新增】一个 async 方法（原因见 TODO 3 的说明）。
//
//  ★ 三条本课铁律，写代码时随时对照：
//      1. 不用 .Result / .Wait()；2. 不写 async void（除事件处理器）；
//      3. 库代码加 .ConfigureAwait(false)，UI 代码不加。
// ============================================================================

using System.Reflection;

namespace LessonPractice
{
    class Lesson24Practice
    {
        // ================================================================
        // ↓↓↓ 本课的辅助方法写在这里（async 方法）↓↓↓
        // ================================================================

        /// <summary>
        /// 【TODO 1 会用】打印当前线程：标签 + 线程 ID + 是不是线程池线程。
        /// </summary>
        static void ShowThread(string label)
        {
            int tid = Environment.CurrentManagedThreadId;
            bool pooled = Thread.CurrentThread.IsThreadPoolThread;
            Console.WriteLine($"{label,-22}：线程 {tid,2}（{(pooled ? "线程池" : "非线程池")}）");
        }

        /// <summary>
        /// 【TODO 2 会用】模拟一次耗时 ms 毫秒的 IO，返回 id * 10。
        /// ★ 注意这里用的是 await Task.Delay，不是 Thread.Sleep ——
        ///   前者让出线程，后者把线程按住（本课反复强调的区别）。
        /// </summary>
        static async Task<int> FakeIoAsync(int id, int ms)
        {
            await Task.Delay(ms);
            return id * 10;
        }

        /// <summary>
        /// 【TODO 3 的样本】一个普通的 async 方法：它存在的意义不是"干活"，
        /// 而是让编译器为它生成一个状态机类型，好让你用反射挖出来。
        /// </summary>
        static async Task SomeAsyncMethod()
        {
            await Task.Delay(10);
        }

        /// <summary>
        /// 【TODO 4 会用】一个 async void 方法，它会在 await 之后抛出异常。
        /// ⚠️⚠️ 绝对不要在控制台程序的普通流程里调用它：异常会甩到线程池上，
        ///     整个进程直接崩溃。它只能在 TODO 4 那种"有 SynchronizationContext"
        ///     的环境里被安全观察。
        /// </summary>
        static async void AsyncVoidBomb()
        {
            // ★ 顺便观察一个细节：这里用 await Task.Run(...) 而不是 await Task.Delay(...)。
            //   Task.Run 的续体【不捕获】SynchronizationContext，
            //   所以后面那个 throw 是在线程池线程上执行的 ——
            //   可异常照样会回到「当前没有上下文 → 线程池」那条崩溃路径上。
            //   而 await Task.Delay 的续体会回到上下文，异常就 post 给上下文。
            //   两条路的共同点是：【都没有 Task 装它，调用方永远接不住】。
            await Task.Run(() => Thread.Sleep(20));

            throw new InvalidOperationException("async void 内部炸了，而调用方根本接不住");
        }

        /// <summary>
        /// 【TODO 4/5 会用】一个"模拟 UI 线程"的 SynchronizationContext。
        /// ★ 骨架已经给全了：Post 排队、RunLoop 在同一个线程上一条条执行、接住异常。
        ///   它模拟的就是 WPF 的 DispatcherSynchronizationContext：
        ///   「续体不许在别的线程上跑，必须回到我这条线程」。
        /// </summary>
        sealed class FakeUiContext : SynchronizationContext
        {
            // 线程安全 + 取空就阻塞等待的队列，正好是"消息泵"需要的东西
            readonly System.Collections.Concurrent.BlockingCollection<(SendOrPostCallback Callback, object? State)> queue = new();

            // ★ 只是为了你在调试时能看到"这条队列处理过多少活儿"
            int handled;

            /// <summary>把活儿投递到"UI 线程"的队列里（立刻返回，不等待）。</summary>
            public override void Post(SendOrPostCallback d, object? state)
            {
                queue.Add((d, state));
            }

            /// <summary>在"UI 线程"上同步执行（本练习用不到，留个直通实现即可）。</summary>
            public override void Send(SendOrPostCallback d, object? state)
            {
                d(state);
            }

            /// <summary>让 RunLoop 结束（清理用）。</summary>
            public void PostQuit()
            {
                queue.CompleteAdding();
            }

            /// <summary>
            /// "UI 消息泵"：在安装了这个上下文的那个线程上，一条条取出活儿来执行。
            /// </summary>
            /// <param name="onUnhandled">
            /// 有活儿抛出异常时调用它 —— 这就是 WPF 里 Dispatcher.UnhandledException 的位置。
            /// 真实框架在这里会让进程崩溃；我们把它记下来，好让练习继续跑完。
            /// </param>
            public void RunLoop(Action<Exception> onUnhandled)
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
                            // ★ 这一行就是"async void 的异常最后落在哪"的答案：
                            //   它落在当前 SynchronizationContext 上，而不是调用点。
                            onUnhandled(ex);
                        }

                        handled++;
                        // ★ 想让 async void 的异常真的把进程带走？把上面那个 catch 删掉。
                        //   那时异常会从 RunLoop 逃出去，撞在这条线程的顶层 → 进程崩溃。
                    }
                }
                catch (InvalidOperationException)
                {
                    // PostQuit() 之后 GetConsumingEnumerable 会正常结束，这里兜一下
                }
            }
        }

        // ================================================================
        //  主流程
        // ================================================================
        static async Task Main()
        {
            Console.WriteLine("===== 第 24 课练习：async / await 状态机 =====");
            Console.WriteLine();

            // ★ 小提示：Main 自己也在这条 async 链上，
            //   所以你现在就已经在"状态机"里了 —— Main 的线程号后面会用到。
            ShowThread("Main 开始");

            // ================================================================
            // TODO 1 ｜ 观察 await 前后的线程 ID
            // ================================================================
            // 【要求】
            //   1. 在下面这块里写一个【局部 async 方法】：
            //        async Task ObserveAsync()
            //        {
            //            ShowThread("await 之前");
            //            await Task.Delay(50);
            //            ShowThread("Task.Delay 之后");
            //            await Task.Yield();
            //            ShowThread("Task.Yield 之后");
            //        }
            //      （局部方法就是写在 Main 里面的方法；加上 async 就是局部 async 方法）
            //   2. await 它，并在调用前后各 ShowThread 一次 Main 自己的线程
            //   3. 把那三次"await 前/后"的线程 ID 和 Main 的线程 ID 做个对比并打印结论
            //   4. 回答（写成注释）：Task.Delay 之后线程变了吗？Task.Yield 之后呢？
            //      为什么"变"和"没变"都算正常？
            //
            // 【预期输出】（线程号每次运行都不一样，看"变没变"而不是看具体数字）
            //     --- TODO 1 ---
            //     Main 开始            ：线程  1（非线程池）
            //     await 之前           ：线程  1（非线程池）    ← 和 Main 同一个线程
            //     Task.Delay 之后      ：线程  1（非线程池）    ← 经常【没变】，这是允许的
            //     Task.Yield 之后      ：线程  7（线程池）      ← 几乎一定【变了】
            //     Main 的后续代码      ：线程  7（线程池）
            //     ★ 结论：await 之后线程【可能】变，任何依赖"我还在这条线程上"的代码都是错的
            //
            // 【提示】
            //     局部 async 方法的写法：
            //         async Task ObserveAsync()
            //         {
            //             ...
            //         }
            //     然后 await ObserveAsync();
            //     ⚠️ 插值字符串里要显示双引号必须写 \" ，不要写成裸的 "
            //
            // 【考察点】await 不是"暂停线程"；Task.Delay 不保证换线程，Task.Yield 几乎一定换

            Console.WriteLine("--- TODO 1 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 2 ｜ 顺序 await vs Task.WhenAll 实测耗时
            // ================================================================
            // 【要求】
            //   1. 用 Stopwatch 测【顺序】执行三个 FakeIoAsync(id, 150) 的耗时
            //   2. 用 Stopwatch 测【并发】执行同样三个的耗时：
            //      先把三个 Task 都创建出来，再 await Task.WhenAll
            //   3. 打印两次耗时、结果、以及倍数
            //   4. 关键理解（写成注释）：为什么"先把 Task 创建出来"才能并发？
            //      对照 Python：coro = fetch(1) 这一行会让协程开始跑吗？
            //
            // 【预期输出】
            //     --- TODO 2 ---
            //     FakeIoAsync 结果验证：10（顺序也要能拿到值）
            //     顺序 await 三个 150ms ：约 450 ms   结果 10, 20, 30
            //     Task.WhenAll 三个    ：约 150 ms   结果 10, 20, 30
            //     ★ 并发快了约 3 倍
            //
            // 【提示】
            //     var sw = System.Diagnostics.Stopwatch.StartNew();
            //     int a1 = await FakeIoAsync(1, 150);   // 顺序：等完一个再起下一个
            //     ...
            //     sw.Restart();
            //     Task<int> t1 = FakeIoAsync(1, 150);   // ★ 这一行调用下去就已经开始跑了
            //     Task<int> t2 = FakeIoAsync(2, 150);
            //     Task<int> t3 = FakeIoAsync(3, 150);
            //     int[] r = await Task.WhenAll(t1, t2, t3);
            //     打印耗时用 sw.ElapsedMilliseconds（整数毫秒，算倍数时记得转 double）
            //
            // 【考察点】await 是"等"，不是"启动"；C# 的 Task 一被调用就开始执行

            Console.WriteLine("--- TODO 2 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // 先确认辅助方法能用（顺便让编译器知道 FakeIoAsync 有人调用）
            int warmup = await FakeIoAsync(1, 1);
            Console.WriteLine($"    FakeIoAsync 试跑：{warmup}（应为 10）");




            Console.WriteLine();

            // ================================================================
            // TODO 3 ｜ 把编译器生成的状态机挖出来看看
            // ================================================================
            // 【要求】
            //   1. 先在下面这块里【新增】一个你自己的 async 方法，名字叫 StudentAsync：
            //        static async Task StudentAsync()
            //        {
            //            int local = 42;                 // ★ 故意留一个局部变量，等下要用
            //            ShowThread("StudentAsync 同步部分");
            //            await Task.Delay(20);
            //            Console.WriteLine($"    await 之后 local 还活着：{local}");
            //        }
            //      ⚠️ 注意：写成 static 的话它会成为 Main 的【局部函数】，
            //         反射时类型名会变成 <<Main>$>g__StudentAsync|0_N>d 这种；
            //         【字段还是一样的】，不影响本 TODO 的结论。
            //         想看到课文里那种干净的 <StudentAsync>d__N，
            //         就把它写成 Main【外面】的一个普通 static 方法。两种都行。
            //   2. 用反射找出编译器为 StudentAsync 生成的状态机类型：
            //        ⚠️ 写成局部函数时，状态机是嵌在 <Main>$ 里面的，
            //           所以要遍历 typeof(Lesson24Practice) 的嵌套类型【的嵌套类型】；
            //           写成 Main 外面的方法时，直接遍历 typeof(Lesson24Practice) 就行。
            //        遍历 GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)，
            //        找名字里包含 "StudentAsync" 的那个类型
            //      ⚠️ 不要写死编号（像 d__4 这种），编译器改一行代码就可能重排
            //   3. 打印：完整类型名、IsValueType、实现的接口、所有字段（名字 + 类型）
            //      （⭐ 如果反射出来是空列表，说明你拿到的不是状态机类型 ——
            //        检查一下是不是遍历深度不够，或者名字没筛对）
            //   4. 在字段里指出：<>1__state（第几段）和 <>t__builder（返回给调用方的 Task）
            //   5. 回答（写成注释）：local 这个局部变量为什么会在字段列表里出现？
            //   6. 顺手也把 SomeAsyncMethod 的状态机打出来对比一下（可选）
            //      —— SomeAsyncMethod 是 Main 外面的方法，它的状态机最好找
            //
            // 【预期输出】
            //     --- TODO 3 ---
            //     状态机类型：LessonPractice.Lesson24Practice+<StudentAsync>d__N   （N 不定）
            //     是值类型（struct）：True
            //     接口：IAsyncStateMachine
            //     字段：
            //       <>1__state          Int32
            //       <>t__builder        AsyncTaskMethodBuilder
            //       <local>5__2         Int32          ← ★ 你的局部变量搬进来了！
            //       <>u__1              TaskAwaiter
            //     ★ 结论：async 方法 = 状态机 struct + 一层同步壳；局部变量靠字段活过 await
            //
            // 【提示】
            //     · 文件顶部的 using System.Reflection; 已经加好了
            //     · 字段：t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            //     · 筛名字用 t.Name.Contains("StudentAsync", StringComparison.Ordinal) 最省事
            //       （状态机名字可能是 <StudentAsync>d__N，也可能是
            //         <<Main>$>g__StudentAsync|0_N>d —— 用 Contains 两种都能命中）
            //     · 找不到就打印一句「没找到」，别让程序崩（说明名字规律变了）
            //     · ★ 最省事的做法：先把所有嵌套类型的名字都打印一遍
            //            foreach (Type t in typeof(Lesson24Practice).GetNestedTypes(
            //                         BindingFlags.NonPublic | BindingFlags.Public))
            //                Console.WriteLine(t.FullName);
            //       你会看到真实的 <StudentAsync>d__N，然后就知道该怎么筛了。
            //       （这不是作弊，是让编译器直接告诉你答案。）
            //
            // 【考察点】async 方法被编译成状态机；局部变量被提升为字段才能跨 await 存活

            Console.WriteLine("--- TODO 3 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // 让编译器知道脚手架方法有人用（也顺便演示：await 一个"什么都不做"的方法）
            await SomeAsyncMethod();




            Console.WriteLine();

            // ================================================================
            // TODO 4 ｜ 证明 async void 的异常抓不到
            // ================================================================
            // 【要求】
            //   1. 造一个 FakeUiContext（辅助方法区已经写好了）
            //   2. 起一个后台线程：
            //        · SynchronizationContext.SetSynchronizationContext(context)
            //        · 然后调 context.RunLoop(ex => { 记下这个异常 })
            //        · 记得 IsBackground = true（万一没收干净，进程也能退出）
            //      —— 这条线程就是你的"模拟 UI 线程"
            //   3. 用 context.Post(...) 往它上面投递一段代码，里面【用 try/catch 包住
            //      AsyncVoidBomb() 的调用】，catch 里打印"我抓到了"（这一行不该出现）
            //   4. 等 300ms 左右（异常是 20ms 之后才发生的），再打印两个判断：
            //        · try/catch 抓到了吗？（应该是 False）
            //        · 上下文接住了吗？（应该是 True）
            //   5. 再写一段对照：await 一个会抛异常的 async Task，看 try/catch 能不能抓到
            //      （这条 async Task 你可以自己新写一个，也可以用 Task.FromException）
            //   6. 收尾：context.PostQuit() + 后台线程 Join(500)
            //   7. 回答（写成注释）：如果把 RunLoop 里那个 catch 删掉会怎样？
            //      为什么"事件处理器可以用 async void"但其他场景绝对不行？
            //
            // 【预期输出】
            //     --- TODO 4 ---
            //     调用点：try 块正常结束了 —— 完全不知道后面会出事
            //     （约 20ms 后）⚠️ 上下文接住了一个没人处理的异常：InvalidOperationException
            //     try/catch 抓到了吗：False    ← ★ 重点：抓不到
            //     上下文接住了吗    ：True
            //     ── 对照：async Task ──
            //     ✅ await 之后异常规规矩矩地抛在 await 那一行：xxx
            //     ★ 结论：async void 的异常不走"方法返回"，走 SynchronizationContext
            //
            // 【提示】
            //     var context = new FakeUiContext();
            //     Exception? posted = null;
            //     var runner = new Thread(() =>
            //     {
            //         SynchronizationContext.SetSynchronizationContext(context);
            //         context.RunLoop(ex => posted = ex);
            //     });
            //     runner.IsBackground = true;
            //     runner.Start();
            //     Thread.Sleep(100);                    // 等上下文装好
            //     context.Post(_ =>
            //     {
            //         try { AsyncVoidBomb(); } catch (Exception ex) { Console.WriteLine("我抓到了"); }
            //     }, null);
            //     Thread.Sleep(300);                    // 等异常发生
            //     Console.WriteLine($"抓到了吗：{posted is not null}");
            //     context.PostQuit(); runner.Join(500);
            //     ⚠️ 别忘了 AsyncVoidBomb 自己已经写好了：它内部 await Task.Run(...) 之后 throw
            //     ⚠️ 想亲手看一次"进程崩溃"，就在下面这块最后加一行 AsyncVoidBomb();
            //        然后 Ctrl+F5 —— 程序会带着未处理异常死掉。看完记得删掉这一行。
            //
            // 【考察点】async void 的异常无法被调用点捕获；它只能被上下文/全局兜底看到

            Console.WriteLine("--- TODO 4 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // 让编译器知道 AsyncVoidBomb 有人"引用"（这里故意不调用它！）
            // ⚠️ 下面这行是【永远不会执行】的守卫，只为了让编译器别报"从未使用"。
            //    真正的调用要你自己在 TODO 4 的上下文里写。
            if (DateTime.Now.Year < 2000)
            {
                AsyncVoidBomb();
            }




            Console.WriteLine();

            // ================================================================
            // TODO 5 ｜ ★ 综合（本课考核）：并发 + 异常 + ConfigureAwait(false)
            // ================================================================
            // 【场景】
            //   你写了一个"库方法"要给别人（包括 UI 程序）调用，它内部会 await。
            //   现在要证明两件事：
            //     ① 并发比顺序快得多          —— 这是 async 的收益
            //     ② 库代码不加 ConfigureAwait(false) 会把调用方拖进死锁
            //                                    —— 这是 async 的代价
            //
            // 【要求】
            //   1. 并发部分：
            //        · 写一个局部 async 方法 Task<int> JobAsync(int id, int ms)：
            //            await Task.Delay(ms) 之后返回 id * 10
            //        · 6 个任务，耗时分别用 100 / 60 / 120 / 40 / 80 / 140 ms
            //        · 用 Stopwatch 测【顺序】跑 6 个的耗时
            //        · 用 Stopwatch 测【Task.WhenAll 并发】跑 6 个的耗时
            //        · 打印两者耗时和倍数（理论：顺序 = 540ms，并发 ≈ 最慢那个 = 140ms）
            //   2. 异常部分：
            //        · 让其中一个任务在 await 之后抛异常（例如 id == 3 时 throw）
            //        · 用 try/catch 包住 await Task.WhenAll(...)，打印抓到的异常类型和消息
            //        · 判断它是不是 AggregateException；如果是，再遍历 InnerExceptions
            //          —— 顺便回答：为什么 WhenAll 要用聚合异常？
            //   3. ConfigureAwait 部分（★ 本课考核的核心）：
            //        · 写一个局部 async 方法：
            //            async Task<int> LibraryCallAsync(bool useConfigureAwaitFalse)
            //          里面 await Task.Yield()（加了参数时用 .ConfigureAwait(false)），
            //          然后返回 Environment.CurrentManagedThreadId
            //        · 造一个 FakeUiContext，起后台线程安装并 RunLoop（做法同 TODO 4）
            //        · 在这个上下文上【同步阻塞地】等结果（用带超时的 Wait）：
            //            参数 false（不加 ConfigureAwait）→ 应该 1 秒超时（死锁）
            //            参数 true （加了 ConfigureAwait）→ 应该立刻拿到结果
            //        · 打印两次结果，并用注释解释差别从哪来
            //        · ⚠️ 一定要用带超时的 Wait(TimeSpan)，不要用 .Result，
            //             否则你会真的把练习挂死（然后以为是自己写错了）
            //   4. 最后打印一段你自己的总结（3~5 行），说清三件事：
            //        · await 到底对线程做了什么
            //        · 为什么库代码要加 ConfigureAwait(false)
            //        · 为什么永远不要写 .Result / .Wait()
            //
            // 【预期输出】（数字每次不同；顺序/并发那两行取决于你写的 JobAsync）
            //     --- TODO 5 ---
            //     顺序 6 个任务 ：约 540 ms
            //     并发 6 个任务 ：约 140 ms（≈最慢的那个）
            //     ★ 并发快约 3.9 倍
            //     并发中有一个失败：捕获到 AggregateException
            //       └ InvalidOperationException：任务 3 炸了
            //     ── ConfigureAwait 对比 ──
            //     不加 ConfigureAwait(false)：❌ 1 秒超时 —— 死锁确认（Status=WaitingForActivation）
            //     加了 ConfigureAwait(false)：✅ 拿到结果，续体线程 9（线程池）
            //     ★ 总结：
            //       · await 让方法在等待点 return，任务完成后由线程池/上下文再调 MoveNext 继续
            //       · 库代码加 ConfigureAwait(false)，续体就不必抢那条被阻塞的线程 → 不会死锁
            //       · .Result 阻塞的正是续体唯一能跑的线程，所以永远别写
            //
            // 【提示】
            //     · 同步阻塞等结果：bool ok = task.Wait(TimeSpan.FromSeconds(1));
            //       （带超时的 Wait，比 .Result 安全：超时了还能继续往下跑）
            //     · 超时后一定要清理：context.PostQuit() + runner.Join(500)
            //     · WhenAll 的异常：
            //         try { await Task.WhenAll(ts); }
            //         catch (Exception ex) { Console.WriteLine(ex.GetType().Name); }
            //     · 让 6 个任务里恰好一个失败：
            //         async Task<int> JobAsync(int id, int ms)
            //         {
            //             await Task.Delay(ms);
            //             if (id == 3) throw new InvalidOperationException("任务 3 炸了");
            //             return id * 10;
            //         }
            //       ⚠️ 注意：id==3 的那个 Task 会变成 Faulted，await WhenAll 会抛异常，
            //          但其它任务其实都跑完了 —— 这正是 WhenAll 的语义（等全部结束）
            //     · 死锁那一段要保证"库方法在阻塞之前就已经登记好续体"：
            //         Task<int> call = LibraryCallAsync(false);
            //         bool ok = call.Wait(TimeSpan.FromSeconds(1));
            //
            // 【考察点】并发收益 / 异常聚合 / ConfigureAwait(false) 的真实作用（防死锁）

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
