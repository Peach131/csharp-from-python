// ============================================================================
//  第 20 课 · 练习：终结器与 IDisposable
//
//  ★ 本课重点：
//     1. GC 只管托管内存，非托管资源（文件句柄、连接、原生内存）必须自己还
//     2. 终结器 ~ClassName() 是"最后防线"，时机不确定，还会让对象多活一轮 GC
//     3. 标准 Dispose 模式：_disposed 标志 + Dispose(bool disposing) + SuppressFinalize
//     4. using 的两种写法 —— 它就是 Python 的 with
//     5. 忘记 Dispose 的后果：句柄泄漏、数据丢失、连接池被占满
//
//  ★ 做题方式：
//     每个 TODO 下面留了空行，直接把代码写在「↓↓↓ 在这里写你的代码 ↓↓↓」后面。
//     本课要用的类，写在 Lesson20Practice 类的【外面】（文件里已经留好位置）。
//     参考答案：D:\Zlprogect\Projectset\learncsrop\csharp-from-python\
//               学习档案\参考答案\第20课-参考答案.md
//     （★ 先自己写，卡住 15 分钟以上再看答案）
//
//  ★ 运行：打开 第20课.sln → 启动项目选 Lesson20Practice → Ctrl + F5
// ============================================================================

using System.Runtime.InteropServices;   // ★ Marshal.AllocHGlobal / FreeHGlobal 在这里
                                        //   （它们【不需要】unsafe / 指针，是安全的 API）

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ 本课要实现的类写在这里（在 Lesson20Practice 类的【外面】）↓↓↓
    // ================================================================

    // ────────────────────────────────────────────────────────────────
    // TODO 1 ｜ TraceResource：最简单的 IDisposable
    // ────────────────────────────────────────────────────────────────
    // 【要求】
    //   写一个 TraceResource 类，实现 IDisposable：
    //     · 构造函数接收一个 string name，并打印：  打开 <name>
    //     · Dispose() 打印：                        关闭 <name>
    //     · Dispose() 要幂等（重复调用只打印一次"关闭"）
    //   然后在 Main 里用它验证以下两件事，并各打印一句话说明结果：
    //     A. using (var r = new TraceResource("A")) { ... } 里的方法调用，
    //        发生在"打开"和"关闭"之间
    //     B. 出了 using 块之后，Dispose 已经执行过了（用一个 bool 标志记录）
    //
    // 【预期输出】（格式不必完全一样，顺序必须对）
    //     打开 A
    //     正在使用 A
    //     关闭 A
    //     A 已经关闭了吗：True
    //
    // 【提示】
    //     class TraceResource : IDisposable
    //     {
    //         private readonly string _name;
    //         private bool _disposed;
    //         public TraceResource(string name) { _name = name; Console.WriteLine($"打开 {_name}"); }
    //         public bool IsClosed => _disposed;
    //         public void Dispose()
    //         {
    //             if (_disposed) return;      // ← 这一行就是"幂等"
    //             _disposed = true;
    //             Console.WriteLine($"关闭 {_name}");
    //         }
    //     }
    //     ★ 检测 B 的时候注意：using 块里声明的变量，出了块就访问不到了（作用域规则）。
    //       所以要在块【里面】把引用存到一个块外面声明的变量里：
    //         TraceResource? keep = null;
    //         using (var r = new TraceResource("A")) { keep = r; }
    //         Console.WriteLine($"A 已经关闭了吗：{keep.IsClosed}");
    //       （用完之后 keep 可能为 null，所以用 keep?.IsClosed 更稳妥）
    //
    // 【考察点】
    //   · IDisposable 只有一个方法：void Dispose()
    //   · using 是一个"作用域"，右花括号 = 释放点
    //   · 幂等：Dispose 被调用多次也必须安全（using 会再调一次）



    // ────────────────────────────────────────────────────────────────
    // TODO 2 ｜ FinalizableThing：观察终结器什么时候执行
    // ────────────────────────────────────────────────────────────────
    // 【要求】
    //   写一个带终结器的 FinalizableThing 类：
    //     · 构造函数打印：  构造 <id>
    //     · 终结器打印：    ★终结器 <id>
    //     · 有一个 static int 计数器，终结器每跑一次 +1
    //   然后在 Main 里：
    //     ① 在方法里创建 2 个对象（id = 1、2），不 Dispose，直接离开引用
    //     ② 打印"强引用已断开，现在谁也不知道终结器啥时候跑"
    //     ③ 什么都不做，先打印 3 行普通输出（让"不确定"这个感觉出来）
    //     ④ 再强制回收：GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    //     ⑤ 打印终结器一共跑了几次
    //
    // 【预期输出】（前 2 行在前，最后 3 行在后；中间 3 行普通输出插在中间）
    //     构造 1
    //     构造 2
    //     强引用已断开，现在谁也不知道终结器啥时候跑
    //     （中间三行任意内容）
    //     ★终结器 2
    //     ★终结器 1
    //     终结器一共跑了 2 次
    //   ⚠️ 注意：两个终结器的先后顺序【不保证】，不要依赖它。
    //
    // 【提示】
    //     class FinalizableThing
    //     {
    //         private readonly int _id;
    //         public static int FinalizerRuns;
    //         public FinalizableThing(int id) { _id = id; Console.WriteLine($"构造 {_id}"); }
    //         ~FinalizableThing()
    //         {
    //             FinalizerRuns++;
    //             Console.WriteLine($"★终结器 {_id}");
    //         }
    //     }
    //     ★ 要让"创建后离开作用域"这件事真的发生，把创建动作放进一个单独的方法：
    //         static void CreateAndForget()
    //         {
    //             var x = new FinalizableThing(1);
    //             var y = new FinalizableThing(2);
    //         }   // ← 方法返回，x/y 都没人引用了
    //
    // 【考察点】
    //   · 终结器不能手动调用、不能有参数和返回值，只能等 GC
    //   · 时机不确定：可能几毫秒，也可能程序结束才跑
    //   · 顺序也不确定：不要写依赖终结顺序的代码

    // TODO 2 用的 FinalizableThing 类写在这里：



    // ────────────────────────────────────────────────────────────────
    // TODO 3 ｜ NativeBlock：完整实现标准 Dispose 模式
    // ────────────────────────────────────────────────────────────────
    // 【要求】
    //   照着"标准 Dispose 模式"实现 NativeBlock，它【直接】持有一块非托管内存：
    //     字段：bool _disposed、IntPtr _native、int _size
    //     构造函数：Marshal.AllocHGlobal(size)
    //     终结器：~NativeBlock() => Dispose(false);
    //     Dispose()：Dispose(true) + GC.SuppressFinalize(this)
    //     Dispose(bool disposing)：
    //         if (_disposed) return;
    //         if (disposing) { /* 这里放"托管"资源的释放；本类暂时没有，写注释说明 */ }
    //         if (_native != IntPtr.Zero) { Marshal.FreeHGlobal(_native); _native = IntPtr.Zero; }
    //         _disposed = true;
    //     另外提供：bool IsDisposed、int Size、以及一个 static int 终结器计数器
    //   然后在 Main 里做两组对照实验：
    //     ① 主动 Dispose：打印 Dispose 前后的 IsDisposed，并且
    //        在 GC.Collect + WaitForPendingFinalizers 之后，打印终结器计数（应当是 0）
    //     ② 故意不 Dispose：把引用置 null，强制回收，再打印终结器计数（应当是 1）
    //
    // 【预期输出】（节选）
    //     --- 主动 Dispose ---
    //     释放前 IsDisposed = False
    //     释放后 IsDisposed = True
    //     走终结器的次数 = 0     ← SuppressFinalize 生效
    //     --- 忘了 Dispose ---
    //     走终结器的次数 = 1     ← 兜底生效，但时机由 GC 说了算
    //
    // 【提示】
    //     · IntPtr.Zero 就是"空指针"，判断它比判断 null 更合适
    //     · ★ _disposed 标志必须在最前面判断：重复 FreeHGlobal 是未定义行为
    //     · 计数用 static 字段，别用实例字段（实例字段你已经拿不到那个对象了）
    //     · 构造里的 size 记得同时存进 _size，否则 Size 属性没东西可返回
    //     · AllocHGlobal 失败会抛 OutOfMemoryException，本练习不用特意处理
    //
    // 【考察点】
    //   · 一个实现、两个入口：Dispose(bool)
    //   · GC.SuppressFinalize 把对象从终结队列里摘掉
    //   · 只有【直接】持有非托管资源的类才需要终结器

    // TODO 3 用的 NativeBlock 类写在这里：



    // ────────────────────────────────────────────────────────────────
    // TODO 4 ｜ 演示"忘记 Dispose"的后果
    // ────────────────────────────────────────────────────────────────
    // 【要求】用 FileStream / StreamWriter 做三件事，每件事都要打印证据：
    //     ① 用 using 写一个文件，写出后立刻打印文件长度（应当 > 0）
    //     ② 不用 using 打开同一个文件的"独占锁"，然后开第二个 FileStream 去读它，
    //        捕获 IOException 并打印异常类型（证明句柄没还，别人拿不到）
    //     ③ 不用 using 写一个新文件（内容留在缓冲区里），把这个对象置 null、
    //        强制 GC，然后打印：GC 前后文件长度、以及"数据是不是丢了"的结论
    //
    // 【预期输出】（节选，长度数字以实际为准；Windows 上换行是 \r\n，每行多 1 字节）
    //     --- ① using 正常写 ---
    //     文件长度 = 7 字节
    //     --- ② 不释放 → 别人打不开 ---
    //     第二个 FileStream 失败：IOException
    //     --- ③ 不 Dispose → 数据丢 ---
    //     GC 前文件长度 = 0，GC 后 = 0
    //     结论：数据丢了 —— FileStream 的终结器只关句柄，不会替你 Flush
    //
    // 【提示】
    //     · 临时目录：string dir = Path.Combine(Path.GetTempPath(), "Lesson20Practice");
    //       Directory.CreateDirectory(dir);
    //     · 独占打开：new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)
    //     · StreamWriter 默认带缓冲区，不 Flush / 不 Dispose，数据就不落盘
    //     · ② 里第二个 FileStream 记得包在 using 里（否则它自己也泄漏一个句柄）
    //     · ③ 里"强制 GC"三连：GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    //     · 想验证对象真的被收走了，可以用 WeakReference（弱引用）
    //
    // 【考察点】
    //   · 非托管资源（句柄）不会因为"对象被回收"而及时归还
    //   · 托管缓冲区里的数据，终结器救不回来
    //   · using 是唯一可靠的"确定性释放"手段

    // TODO 4 不需要新类；如果你愿意，也可以写个包装类再演示一遍



    // ────────────────────────────────────────────────────────────────
    // TODO 5 ｜ ★ 综合（本课考核）：一个连接池雏形
    // ────────────────────────────────────────────────────────────────
    // 【要求】写 FakeConnection + FakeConnectionPool 两个类，把本课所有知识点串起来：
    //
    //   FakeConnection : IDisposable
    //     · 字段：string _name（= ""）、bool _disposed、static int ActiveCount
    //     · 构造：_name 赋值，ActiveCount++，打印  + 打开连接 <name>（当前活跃 N）
    //     · Dispose()：
    //         if (_disposed) return;
    //         _disposed = true;
    //         ActiveCount--;
    //         Console.WriteLine($"  - 归还连接 {_name}（当前活跃 {ActiveCount}）");
    //         GC.SuppressFinalize(this);
    //     · 终结器：打印 ★警告：连接 <name> 没有被归还！并把 ActiveCount-- 兜底
    //       （★ 真实代码里绝不能在终结器里碰托管对象；这里只是为了让你看到兜底效果）
    //     · bool IsDisposed => _disposed;
    //
    //   FakeConnectionPool : IDisposable（★ 这是"资源管理器"那一层）
    //     · 构造函数接收 int max
    //     · Acquire(string name)：
    //         ObjectDisposedException.ThrowIf(_disposed, this);   // 池子已关就不能再借
    //         如果 FakeConnection.ActiveCount >= _max → throw new InvalidOperationException("连接池已满")
    //         否则 return new FakeConnection(name);
    //     · Dispose()：打印"连接池已关闭"，并把自己标记为已释放
    //
    //   然后在 Main 里：
    //     ① 用 using 包住连接池，池子里顺序 Acquire 两个连接，各自也包 using
    //        → 观察"归还"顺序和"连接池已关闭"出现的时机
    //     ② 写一个 static void LeakOneConnection(FakeConnectionPool pool) 方法：
    //        在里面 Acquire 一个连接【不释放】，方法结束后强制 GC
    //        → 观察终结器的 ★警告 和 ActiveCount 的变化
    //     ③ 演示"池子满了"：把 max 设成 2，第三个 Acquire 抛异常，捕获并打印
    //
    // 【预期输出】（节选）
    //     --- ① 规范用法：using 套 using ---
    //       + 打开连接 conn-1（当前活跃 1）
    //       + 打开连接 conn-2（当前活跃 2）
    //       - 归还连接 conn-2（当前活跃 1）
    //       - 归还连接 conn-1（当前活跃 0）
    //       连接池已关闭
    //     --- ② 忘了归还 ---
    //       + 打开连接 leaky（当前活跃 1）
    //       ★警告：连接 leaky 没有被归还！（当前活跃 0）
    //     --- ③ 池子满了 ---
    //       + 打开连接 a（当前活跃 1）
    //       + 打开连接 b（当前活跃 2）
    //       第三个失败：连接池已满
    //
    // 【提示】
    //     · 释放顺序是"后进先出"：conn-2 先归还。这和第 20 课讲的 using 嵌套顺序一致
    //     · 判断上限用静态计数 ActiveCount，不要用 List 之类的容器（本练习不需要）
    //     · 终结器里的兜底"ActiveCount--"要防止重复扣：
    //         如果对象已经 Dispose 过，终结器不会再跑（SuppressFinalize 摘掉了），
    //         所以不用额外判断；但你要能说清"为什么这里不用判断"
    //     · 抛异常用 throw new InvalidOperationException("连接池已满");
    //     · ② 里方法返回后要 GC.Collect(); GC.WaitForPendingFinalizers(); 才看得到警告
    //
    // 【考察点】
    //   · 资源所有者的层级：池管连接，连接管底层资源 —— 每层都要能释放
    //   · 主动路径（using）和兜底路径（终结器）如何配合
    //   · 为什么"资源池"最怕忘记归还（池子会枯竭，而不是内存泄漏）
    //   · 完整标准模式：_disposed + Dispose(bool) + SuppressFinalize + 终结器

    // TODO 5 用的 FakeConnection / FakeConnectionPool 类写在这里：



    class Lesson20Practice
    {
        static void Main()
        {
            Console.WriteLine("===== 第 20 课练习：终结器与 IDisposable =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 最简单的 IDisposable：观察 using 的调用顺序
            // ================================================================
            // 【要求】实现 TraceResource 类（写在类外面），并在下面验证：
            //   A. using 块里"正在使用"夹在"打开"和"关闭"之间
            //   B. 出了 using 块，Dispose 已经执行过
            // 【预期输出】
            //     打开 A
            //     正在使用 A
            //     关闭 A
            //     A 已经关闭了吗：True
            // 【提示】细节见文件顶部 TODO 1 的注释
            // 【考察点】IDisposable / using 的作用域 / Dispose 幂等

            Console.WriteLine("--- TODO 1 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 2 ｜ 带终结器的类：观察它【何时】执行
            // ================================================================
            // 【要求】实现 FinalizableThing（写在类外面），然后：
            //   创建 2 个对象后让它们失去引用 → 打印"不知道啥时候跑"→
            //   打印 3 行普通输出 → 强制 GC → 打印终结器跑了多少次
            // 【预期输出】
            //     构造 1 / 构造 2 / 强引用已断开…… /（3 行普通输出）/
            //     ★终结器 2 / ★终结器 1 / 终结器一共跑了 2 次
            // 【提示】创建一个 static void CreateAndForget() 方法在里面 new 两个对象
            // 【考察点】终结器只能等 GC；时机和顺序都不确定

            Console.WriteLine("--- TODO 2 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 3 ｜ NativeBlock：完整实现标准 Dispose 模式
            // ================================================================
            // 【要求】用 Marshal.AllocHGlobal 实现 NativeBlock（写在类外面），
            //   然后对照两种路径：主动 Dispose（终结器不跑）vs 忘了 Dispose（终结器兜底）
            // 【预期输出】
            //     --- 主动 Dispose --- / 释放前 IsDisposed = False / 释放后 = True /
            //     走终结器的次数 = 0 / --- 忘了 Dispose --- / 走终结器的次数 = 1
            // 【提示】Dispose(bool)、_disposed、SuppressFinalize 三件套一个都不能少
            // 【考察点】标准模式每一行的作用

            Console.WriteLine("--- TODO 3 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 4 ｜ 忘记 Dispose 的后果（句柄泄漏 + 数据丢失）
            // ================================================================
            // 【要求】用 FileStream / StreamWriter 演示三件事：
            //   ① using 写出 → 长度 > 0
            //   ② 不释放 → 第二个 FileStream 打不开（IOException）
            //   ③ 不 Dispose → 缓冲区数据丢了（GC 前后文件长度都是 0）
            // 【预期输出】
            //     文件长度 = 7 字节 / 第二个 FileStream 失败：IOException /
            //     GC 前 0，GC 后 0 / 结论：数据丢了
            // 【提示】临时目录用 Path.Combine(Path.GetTempPath(), "Lesson20Practice")
            // 【考察点】GC 回收对象 ≠ 归还资源

            Console.WriteLine("--- TODO 4 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 5 ｜ ★ 综合（本课考核）：连接池雏形
            // ================================================================
            // 【要求】实现 FakeConnection + FakeConnectionPool（写在类外面），然后：
            //   ① using 套 using：看归还顺序 + "连接池已关闭"的时机
            //   ② LeakOneConnection(pool)：借了不还 → 看终结器警告和活跃数
            //   ③ max = 2，第三个 Acquire 抛 InvalidOperationException
            // 【预期输出】
            //     --- ① 规范用法 --- / + 打开连接 conn-1（当前活跃 1）/ …… /
            //     - 归还连接 conn-2 / - 归还连接 conn-1 / 连接池已关闭
            //     --- ② 忘了归还 --- / ★警告：连接 leaky 没有被归还！（当前活跃 0）
            //     --- ③ 池子满了 --- / 第三个失败：连接池已满
            // 【提示】细节和字段清单见文件顶部 TODO 5 的注释
            // 【考察点】资源所有权分层、主动释放与兜底释放的配合、池枯竭

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
