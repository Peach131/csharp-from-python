// ============================================================================
//  第 20 课 · 示例：终结器与 IDisposable
//
//  ★ 本示例回答四个问题：
//      1. GC 到底不管什么？（非托管资源）
//      2. 终结器 ~ClassName() 和 Dispose() 有什么区别？
//      3. 为什么有终结器的对象要"多活一轮 GC"？（有实验、有数字）
//      4. 标准的 Dispose 模式，每一行是干什么的？
//
//  ★ 运行方式：
//      1. 打开 第20课.sln，把 Lesson20Demo 设为启动项目，Ctrl + F5
//      2. 或者命令行：dotnet run --project Lesson20Demo
//
//  ★ 运行完，强烈建议把文件末尾「自己动手改坏它」那 5 条挨个试一遍 ——
//     本课所有的"感觉"都是从改坏代码里来的。
//
//  ★ 重要：本程序里会出现【垃圾回收是故意的】的字样。
//     在真实项目里，除了测试和诊断，永远不要主动调用 GC.Collect()。
//     这里调它，纯粹是为了把"不确定的时机"变成"看得见的输出"。
// ============================================================================

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LessonPractice
{
    // ========================================================================
    //  一、演示用的几个类（都写在 Lesson20Demo 外面）
    // ========================================================================

    // ────────────────────────────────────────────────────────────────────────
    //  1) DemoOrder：演示【终结器 vs Dispose】的调用顺序与调用者
    // ────────────────────────────────────────────────────────────────────────
    //  ★ 这个类【故意】写成一个坏设计：它只持有托管资源，却加了终结器。
    //    目的就是让你看到"终结器什么时候跑"以及"它让你付出什么代价"。
    //  ★ 终结器里只做两件事：计数器 +1、打印一行。
    //    真实代码里终结器要尽可能短，绝不做复杂操作（原因见课文）。
    class DemoOrder
    {
        private readonly string _name;
        private bool _disposed;

        // static：所有实例共用一份，用来统计"一共走了几次终结器"
        public static int FinalizerRuns;

        public DemoOrder(string name)
        {
            _name = name;
            Console.WriteLine($"    构造 : {_name}   （主线程 id = {Environment.CurrentManagedThreadId}）");
        }

        // ★ 终结器：类名前面加一个 ~，没有参数、没有返回值、不能手动调用。
        ~DemoOrder()
        {
            FinalizerRuns++;
            Console.WriteLine($"    ★终结器 : {_name}   （终结器线程 id = {Environment.CurrentManagedThreadId}）");
        }

        // ★ 提供 Close 是很常见的做法：语义清楚（"关掉它"），实现就是 Dispose
        public void Close()
        {
            Console.WriteLine($"    Close : {_name}");
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Console.WriteLine($"    Dispose : {_name}");

            // ★ 主动释放过了，就告诉 GC：这个对象不要再走终结器了。
            //   少了这一行，程序结束时你会看到同一个对象 Dispose 和 ★终结器 都出现。
            GC.SuppressFinalize(this);
        }
    }


    // ────────────────────────────────────────────────────────────────────────
    //  3) NativeBuffer：★ 标准 Dispose 模式的完整样板
    // ────────────────────────────────────────────────────────────────────────
    //  ★ 这是本课最该背下来的代码。每一行的作用见正文第五节。
    //  ★ 它【直接】持有非托管内存（Marshal.AllocHGlobal），所以它是本课
    //    唯一"真的有资格写终结器"的类。
    class NativeBuffer : IDisposable
    {
        // ★ _disposed：防止重复释放。
        //   Dispose 可能被调用多次（你调一次 + using 再调一次），
        //   而 FreeHGlobal 同一个指针调两次是【未定义行为】，可能直接把进程搞崩。
        private bool _disposed;

        // ★ IntPtr：一块非托管内存的地址（就是个整数，不是 C# 对象）。
        //   它的生命周期【完全】由你负责 —— GC 看不见它，也不会替你释放。
        private IntPtr _native;

        // ★ 这个类还持有托管对象，用来演示 Dispose(bool disposing) 的分支。
        private readonly MemoryStream _stream = new();

        public NativeBuffer(int size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            _native = Marshal.AllocHGlobal(size);
            Size = size;
        }

        public int Size { get; }

        public bool IsDisposed => _disposed;

        // ★ 终结器：最后一道防线。写法固定是 => Dispose(false)
        //   注意 false 的含义：我是终结器线程，【不能碰托管对象】。
        ~NativeBuffer() => Dispose(false);

        // ★ 往非托管内存里写数据（用 Marshal.Copy，不需要 unsafe / 指针）
        public void WriteBytes(byte[] data, int offset)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentOutOfRangeException.ThrowIfNegative(offset);
            if (data.Length > Size - offset) throw new ArgumentException("数据超出缓冲区大小", nameof(data));
            Marshal.Copy(data, 0, IntPtr.Add(_native, offset), data.Length);
        }

        // ★ 从非托管内存读回来，用于验证"数据真的在那里"
        public byte[] ReadBytes(int count)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var result = new byte[count];
            Marshal.Copy(_native, result, 0, count);
            return result;
        }

        // ★ 用户主动释放的入口。两行，顺序不能反。
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);   // ★ 已经释放了，别再排队等终结器
        }

        // ★ 真正的释放逻辑，两个入口共用：
        //     disposing == true  → 用户主动调用（Dispose / using）：托管、非托管都能放
        //     disposing == false → 终结器线程调用：★只能放非托管资源
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;       // ① 已经释放过，直接返回

            if (disposing)
            {
                _stream.Dispose();       // ② 托管对象：只有用户主动释放时才碰
            }

            if (_native != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_native);   // ③ 非托管资源：两个入口都必须放
                _native = IntPtr.Zero;          // ④ 置零：防止二次 Free 用同一个地址
            }

            _disposed = true;            // ⑤ 打上标记
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    //  4) NativeBufferChild：派生类怎么扩展 Dispose 模式
    // ────────────────────────────────────────────────────────────────────────
    //  ★ 派生类【不要】再写终结器（基类那个已经够用了）。
    //    它只要重写 Dispose(bool)，放掉自己新增的东西，然后调 base。
    class NativeBufferChild : NativeBuffer
    {
        // 演示"派生类自己加的托管资源"
        private readonly MemoryStream _extra = new();

        public NativeBufferChild(int size) : base(size) { }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _extra.Dispose();        // 先放自己的
            }
            base.Dispose(disposing);     // ★ 再放基类的 —— 顺序和"构造"是反的
        }
    }


    // ────────────────────────────────────────────────────────────────────────
    //  5) DemoFileWriter：把"忘 Dispose 的后果"演出来的小工具
    // ────────────────────────────────────────────────────────────────────────
    //  ★ 它包装一个 FileStream，用 WriteLine 往文件里写。
    //    FileStream 自带 4KB 缓冲区：你不 Dispose，数据就一直躺在内存里没落盘。
    class DemoFileWriter : IDisposable
    {
        private FileStream? _file;
        private StreamWriter? _writer;
        private bool _disposed;

        public static int FinalizerRuns;

        public DemoFileWriter(string path)
        {
            // FileShare.None：独占打开。别的 FileStream 再想打开同一个文件会被拒绝
            //   （这就是第 4 个实验里"句柄泄漏被抓住"的原理）
            _file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            _writer = new StreamWriter(_file);
        }

        public void WriteLine(string line) => _writer?.WriteLine(line);

        // ★ 把缓冲的数据刷到文件，但不关闭句柄 —— 用来对照"刷了"和"没刷"的区别
        public void Flush() => _writer?.Flush();

        // ★ 这个终结器是【教学用】的：真实的包装类不需要它。
        //   它只做两件【绝对安全】的事：计数器 +1、调 Dispose(false)。
        //   ★ 注意：终结器里没有 Console.WriteLine！
        //     终结器运行在专用线程上，时机不确定，打印会插在输出的任意位置，
        //     而且终结器里做任何可能抛异常的事，都会让进程直接崩掉。
        //   它也不去 Flush —— 因为 _writer 是托管对象，终结器线程上不该碰它。
        ~DemoFileWriter()
        {
            FinalizerRuns++;
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                // ★ 顺序很重要：先关 Writer（它会把缓冲区刷到文件），再关 FileStream。
                //   反过来先关掉 FileStream，Writer 就会抛异常。
                // ★ 用 ?. ：_writer / _file 可能是 null（构造到一半就出错了），
                //   而且终结器线程上不能假设字段还有效 —— 这正是 Nullable 警告在提醒我们的。
                _writer?.Dispose();
                _file?.Dispose();
                _writer = null;
                _file = null;
            }
            // ★ disposing == false 时什么都不做：数据没了，只能认。
            //   这就是"终结器只能兜住非托管资源，兜不住托管缓冲区"的意思。
        }
    }


    // ========================================================================
    //  二、程序本体
    // ========================================================================
    class Lesson20Demo
    {
        // 小工具：打印带缩进的一段文字，让输出有层次
        static void P(string text) => Console.WriteLine("  " + text);

        static void Section(string title)
        {
            Console.WriteLine();
            Console.WriteLine(new string('-', 72));
            Console.WriteLine(title);
            Console.WriteLine(new string('-', 72));
        }

        static void Main()
        {
            // 这个临时目录用来放本示例产生的文件
            string workDir = Path.Combine(Path.GetTempPath(), "Lesson20Demo");
            Directory.CreateDirectory(workDir);

            Console.WriteLine("===== 第 20 课：终结器与 IDisposable =====");
            Console.WriteLine($"  运行时版本 : {Environment.Version}");
            Console.WriteLine($"  主线程 id  : {Environment.CurrentManagedThreadId}");
            Console.WriteLine($"  临时目录   : {workDir}");

            // ────────────────────────────────────────────────────────────────
            Section("实验 1 ｜ 边界：GC 管「托管内存」，不管「非托管资源」");
            // ────────────────────────────────────────────────────────────────
            // ★ 先看 GC 擅长的领域：托管内存。
            P("① GC 会替你回收【托管内存】，这一点它做得非常好：");

            long before = GC.GetTotalMemory(forceFullCollection: true);
            var keeper = new List<byte[]>();
            for (int i = 0; i < 2000; i++)
            {
                keeper.Add(new byte[1024]);     // 2000 × 1KB ≈ 2MB
            }
            long after = GC.GetTotalMemory(forceFullCollection: true);
            long perObject = (after - before) / keeper.Count;

            P($"   分配前 : {before,12:N0} 字节");
            P($"   分配后 : {after,12:N0} 字节");
            P($"   差值   : {after - before,12:N0} 字节（每个 byte[1024] 约 {perObject} 字节）");
            P("   ★ 注意：这些内存【不需要】你写任何释放代码，GC 自己会收。");
            P("");

            // ★ 再看 GC 完全不管的领域：非托管资源
            P("② GC 完全不管的几类东西（它们都不是「托管堆上的对象」）：");
            P("   · 文件句柄      FileStream / File / Directory 背后的 OS 句柄");
            P("   · 网络连接      Socket / TcpClient / HttpClient 的底层连接");
            P("   · 数据库连接    SqlConnection 这类（连接池会因为你忘了放而耗尽）");
            P("   · 原生内存      Marshal.AllocHGlobal、C 库 malloc 出来的内存");
            P("   · GDI/窗口句柄  画刷、字体、位图");
            P("");
            P("   ★ 这些东西共有一个特点：GC 只看得到「托管堆里那个包装对象」，");
            P("     看不到它手里握着的那份【外部资源】。");
            P("     包装对象被回收 ≠ 外部资源被归还。");
            P("");
            P("   验证一下：FileStream 不用 Dispose（也没走到终结器），");
            P("   文件句柄就一直开着，别人打不开这个文件。");

            string lockPath = Path.Combine(workDir, "locked.txt");
            var first = new FileStream(lockPath, FileMode.Create, FileAccess.Write, FileShare.None);
            first.WriteByte(65);
            P($"   已用独占方式打开 : {Path.GetFileName(lockPath)}");

            try
            {
                // 第二个 FileStream 想打开同一个文件 → 被操作系统拒绝
                using var second = new FileStream(lockPath, FileMode.Open, FileAccess.Read, FileShare.None);
                P($"   第二个 FileStream 竟然打开了？（不应该发生）长度 = {second.Length}");
            }
            catch (IOException ex)
            {
                P("   再想打开同一个文件 → 失败：");
                P($"     {ex.GetType().Name}: {ex.Message.Split('\n')[0]}");
            }

            first.Dispose();
            P("   第一个 FileStream 调 Dispose 之后 → 句柄归还，锁没了");
            using (var again = new FileStream(lockPath, FileMode.Open, FileAccess.Read))
            {
                P($"   这次能打开了，读出 {again.Length} 字节");
            }
            P("");
            P("② GC 会收【托管内存】，但对【非托管资源】一点都不着急：");
            P("   下面分配 20 MB 托管内存，然后在还没自动 GC 的时候，");
            P("   直接问运行时「你回收过几次第 0 代」：");

            int gen0Before = GC.CollectionCount(0);
            var garbage = new List<byte[]>();
            for (int i = 0; i < 20; i++)
            {
                garbage.Add(new byte[1024 * 1024]);    // 每次 1MB
            }
            long managedNow = GC.GetTotalMemory(forceFullCollection: false);
            int gen0After = GC.CollectionCount(0);
            P($"   已分配托管内存 : {managedNow,12:N0} 字节（这 20MB 还好好躺在托管堆上）");
            P($"   第 0 代回收次数 : {gen0Before} → {gen0After}（GC 觉得还没必要动）");
            P("   ★ 内存它管，而且管得很好 —— 只是【资源】不归它管。");

            // ────────────────────────────────────────────────────────────────
            Section("实验 2 ｜ 终结器 vs Dispose：谁调用、什么时候、什么代价");
            // ────────────────────────────────────────────────────────────────
            P("先看两条【确定的】路径 —— 你调用，你说了算：");

            var a = new DemoOrder("a");
            a.Dispose();        // 你主动释放

            var b = new DemoOrder("b");
            b.Close();          // Close 语义清楚，内部就是 Dispose

            P("");
            P("再看两条【不确定的】路径 —— 交给 GC：");

            var c = new DemoOrder("c");
            c.Dispose();        // ★ 加了 GC.SuppressFinalize，所以 c 不会走终结器
            var d = new DemoOrder("d");   // ★ 不 Dispose，直接丢给 GC

            c = null;
            d = null;

            P("");
            P("   现在 c、d 都不再被引用。强制垃圾回收（★垃圾回收是故意的★）：");
            GC.Collect();
            GC.WaitForPendingFinalizers();   // 等终结器队列排空
            GC.Collect();

            P($"   DemoOrder 终结器一共跑了 {DemoOrder.FinalizerRuns} 次");
            P("   ★ 只有 d（没 Dispose 的那个）走了终结器。");
            P("   ★ 而且它的时机是「刚才那一刻」，不是「d = null 的那一刻」。");
            P("     到底哪一刻？没人能事先告诉你 —— 这正是终结器不能当主力用的原因。");
            P("");
            P("   ┌──────────────────┬────────────────────────┬──────────────────────────┐");
            P("   │                  │ 终结器 ~ClassName()    │ IDisposable.Dispose()    │");
            P("   ├──────────────────┼────────────────────────┼──────────────────────────┤");
            P("   │ 谁调用           │ GC（运行时）           │ 你（或 using 自动调）    │");
            P("   │ 触发时机         │ 回收时，不确定         │ 你写的那一行，确定       │");
            P("   │ 执行线程         │ 专用终结器线程         │ 调用它的那个线程         │");
            P("   │ 能否碰托管对象   │ ★不能                  │ 可以                     │");
            P("   │ 开销             │ 让对象多活一轮 GC      │ 基本没有                 │");
            P("   │ 能否被保证执行   │ 不保证（进程被杀就没了）│ 只要 using 就一定执行   │");
            P("   │ 定位             │ 最后防线               │ ★主要手段                │");
            P("   └──────────────────┴────────────────────────┴──────────────────────────┘");

            // ────────────────────────────────────────────────────────────────
            Section("实验 3 ｜ 终结器的代价：对象要多活一轮 GC");
            // ────────────────────────────────────────────────────────────────
            P("观察方式：直接问运行时「这个对象现在在第几代」（GC.GetGeneration）。");
            P("");

            // ① 没有终结器的普通对象
            var plain = new object();
            int genPlainBefore = GC.GetGeneration(plain);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            int genPlainAfter = GC.GetGeneration(plain);
            P($"   ① 普通对象      ：回收前 第 {genPlainBefore} 代 → GC 一次后 第 {genPlainAfter} 代");
            GC.KeepAlive(plain);   // ★ 让「这个对象一直是活的」，否则它可能在你读之前就被回收了

            // ② 有终结器的对象
            var buffer = new NativeBuffer(64);
            int genBufferBefore = GC.GetGeneration(buffer);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            int genBufferAfter = GC.GetGeneration(buffer);
            P($"   ② 有终结器的对象：回收前 第 {genBufferBefore} 代 → GC 一次后 第 {genBufferAfter} 代");
            GC.KeepAlive(buffer);
            P("");
            P("   ★ 差异就是答案：普通对象一次 GC 就能回收（它一直活着是因为我们 KeepAlive）；");
            P("     而有终结器的对象，只要它还没走完终结器，就【不能】被回收 ——");
            P("     运行时只能把它挪到更老的一代去，等下一轮再说。");
            P("");

            // ③ 用一个 WeakReference 把"两轮 GC"钉死：这是不依赖 JIT 行为的硬证据
            P("   下面用一个 WeakReference 把「两轮 GC」钉死（WeakReference = 弱引用，");
            P("   它不阻止对象被回收；对象被回收后 IsAlive 会变成 false）：");

            var probe = new NativeBuffer(32);
            var weak = new WeakReference(probe);
            probe = null;                     // ★ 去掉最后一个强引用
            P($"   强引用已断开，WeakReference.IsAlive = {weak.IsAlive}");

            GC.Collect();
            GC.WaitForPendingFinalizers();
            P($"   第 1 次 GC 之后：IsAlive = {weak.IsAlive}   ← ★还是活的！");
            P("     终结器已经跑完了，但对象的内存在这一轮【不会】被归还。");

            GC.Collect();
            GC.WaitForPendingFinalizers();
            P($"   第 2 次 GC 之后：IsAlive = {weak.IsAlive}   ← ★现在才真的回收");
            P("");
            P("   ★ 结论：有终结器的对象最多要两轮 GC 才能真正回收。");
            P("     这就是「终结器很贵」的全部含义，也是应当优先 Dispose 的理由。");
            P("");
            P("   那「主动 Dispose」省掉了什么？看数字（同一台机器，同一时刻）：");
            P("   ⚠️ 这不是严谨的基准测试，只看数量级，不要背具体毫秒数。");

            const int N = 300_000;

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < N; i++)
            {
                var temp = new NativeBuffer(64);
                temp.Dispose();          // ★ 主动释放 → 不进终结器队列
            }
            sw.Stop();
            double withDispose = sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            for (int i = 0; i < N; i++)
            {
                _ = new NativeBuffer(64);   // ★ _ = 表示"我故意不要这个引用"
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            sw.Stop();
            double withoutDispose = sw.Elapsed.TotalMilliseconds;

            P($"   {N,8:N0} 次 创建+Dispose        : {withDispose,10:F1} ms");
            P($"   {N,8:N0} 次 创建+丢弃（走终结器）: {withoutDispose,10:F1} ms");
            P($"   倍数 : {withoutDispose / withDispose:F2} 倍");
            P("   ★ 这个数字每台机器都不同，不用记具体值，记结论：终结器是【额外开销】。");
            P("     所以标准做法是：Dispose 当主力，终结器只当兜底。");

            // ────────────────────────────────────────────────────────────────
            Section("实验 4 ｜ 标准 Dispose 模式：每一行的作用");
            // ────────────────────────────────────────────────────────────────
            P("① 正常路径：用户主动 Dispose");
            var nb = new NativeBuffer(256);
            var payload = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            nb.WriteBytes(payload, 0);
            nb.WriteBytes(payload, 128);
            byte[] head = nb.ReadBytes(8);
            P($"   写入 8 字节到偏移 0 和 128，读回前 8 字节 = {string.Join(",", head)}");
            P($"   释放前 IsDisposed = {nb.IsDisposed}");
            nb.Dispose();
            P($"   释放后 IsDisposed = {nb.IsDisposed}");
            P("   再调一次 Dispose（幂等，不会崩）：");
            nb.Dispose();
            P($"   IsDisposed 仍然是 = {nb.IsDisposed}   ← _disposed 标志位挡住了第二次释放");
            P("   释放后再调用方法 → 抛 ObjectDisposedException：");
            try
            {
                nb.ReadBytes(1);
            }
            catch (ObjectDisposedException ex)
            {
                P($"     {ex.GetType().Name}: {ex.Message}");
            }
            P("");
            P("   ★ 三行小结：");
            P("     · _disposed 标志   → 幂等：Dispose 调多少次都安全（using 会再调一次）");
            P("     · Dispose(bool)    → 一个实现，两个入口（用户 / 终结器）");
            P("     · SuppressFinalize → 我处理过了，别再让 GC 花时间走终结器");
            P("");

            P("② 兜底路径：用户忘了 Dispose，终结器收拾残局");
            var forgotten = new NativeBuffer(64);
            P($"   创建了一个没人管的 NativeBuffer（{forgotten.Size} 字节非托管内存）");
            forgotten = null;   // ← 故意不 Dispose
            P("   ★垃圾回收是故意的★");
            GC.Collect();
            GC.WaitForPendingFinalizers();   // 终结器里会调 Dispose(false)，把那块内存放掉
            GC.Collect();
            P("   终结器已经替我们 FreeHGlobal 了 —— 程序不会泄漏，但时机由不得你。");
            P("");

            P("③ 派生类扩展：重写 Dispose(bool)，放完自己再调 base");
            var child = new NativeBufferChild(32);
            P($"   NativeBufferChild 创建（{child.Size} 字节非托管 + 自己的 MemoryStream）");
            child.Dispose();
            P("   Dispose 完成：派生类的 _extra 先放，再放基类的 _native");
            P($"   IsDisposed = {child.IsDisposed}");
            P("");
            P("   ★ 派生类【不需要】再写终结器 —— 基类那个已经会调 Dispose(false)，");
            P("     而 Dispose(false) 会一路调到派生类的重写版本。");

            // ────────────────────────────────────────────────────────────────
            Section("实验 5 ｜ 一行代码不写，也能保证释放：using");
            // ────────────────────────────────────────────────────────────────
            P("① using 的本质：它就是 try / finally 的语法糖。手写版长这样：");
            P("");
            P("       var f = new DemoFileWriter(path);");
            P("       try { ...用 f... }");
            P("       finally { f.Dispose(); }   ← 编译器帮你写的就是这一行");
            P("");

            string usingPath = Path.Combine(workDir, "using.txt");

            P("② 写法一：using (var f = ...) { ... } —— 作用域形式，右花括号就是释放点");
            using (var w1 = new DemoFileWriter(usingPath))
            {
                w1.WriteLine("第一行");
                w1.WriteLine("第二行");
                P("   还在 using 块里：数据可能还在缓冲区，没落盘");
            }
            P($"   出了 using 块（右花括号）→ 立刻 Dispose，文件长度 = {new FileInfo(usingPath).Length} 字节");
            P("");

            P("③ 写法二：using var f = ...; —— 声明式（C# 8+，推荐），");
            P("   释放点 = 这个变量所在【作用域】结束时，而不是某一行代码");

            string declPath = Path.Combine(workDir, "decl.txt");
            {
                // ★ 外面这对花括号是"作用域"，用它来划定 using var 的释放点
                using var w2 = new DemoFileWriter(declPath);
                w2.WriteLine("声明式 using");
                P($"   作用域内：文件长度 = {new FileInfo(declPath).Length} 字节（数据还没落盘）");
            }
            P($"   出了作用域 → 立刻 Dispose，文件长度 = {new FileInfo(declPath).Length} 字节");
            P("");

            P("④ 多个资源：释放顺序和创建顺序【相反】（像叠盘子）");
            string multiPath = Path.Combine(workDir, "multi.txt");
            using (var outer = new DemoFileWriter(multiPath))
            {
                outer.WriteLine("outer");
                // ★ 写在里面的先释放；C# 保证这个顺序，Python 的 with 也一样
                P("   （内层资源先释放，外层后释放）");
            }
            P("");
            P("⑤ 出异常也照样释放：");
            try
            {
                using var boom = new DemoFileWriter(Path.Combine(workDir, "boom.txt"));
                boom.WriteLine("这行写进去了");
                throw new InvalidOperationException("假装业务代码炸了");
            }
            catch (InvalidOperationException ex)
            {
                P($"   捕获到异常：{ex.Message}");
                P("   ★ 但文件已经正常关闭并落盘 —— using 在异常路径上同样生效。");
                P("     这就是「确定性释放」的价值：不管怎么退出，资源都还回去。");
            }

            // ────────────────────────────────────────────────────────────────
            Section("实验 6 ｜ 忘记 Dispose 的后果");
            // ────────────────────────────────────────────────────────────────
            string leakPath = Path.Combine(workDir, "leak.txt");
            P("① 被锁住：不释放的资源，别人拿不到");
            var kept = new DemoFileWriter(leakPath);
            kept.WriteLine("这行数据在缓冲区里");
            kept.Flush();
            // ★ 上面这行 Flush 救了【数据】，但救不了【句柄】：
            //   Flush 只是把缓冲区写到操作系统，文件句柄还牢牢握在我们手里。
            //   "数据写没写下去" 和 "句柄还没还" 是两件事 —— 下面被抓的是后者。
            try
            {
                using var other = new FileStream(leakPath, FileMode.Open, FileAccess.Read, FileShare.None);
                P($"   另一个 FileStream 打开了？长度 = {other.Length}（不应该发生）");
            }
            catch (IOException ex)
            {
                P($"   → 失败：{ex.GetType().Name}: {ex.Message.Split('\n')[0]}");
            }

            P("");
            P("② 数据丢：缓冲区里的东西跟着对象一起没了");
            P("   FileStream / StreamWriter 自带缓冲区（默认 1KB~4KB）：");
            P("   你不 Dispose，数据就一直躺在【内存】里，没写到文件里。");
            P("");
            P("   对照组：还是同一个类，写同样一行，但这次 Dispose（Flush 也是够的）：");
            string flushPath = Path.Combine(workDir, "flush.txt");
            using (var ok = new DemoFileWriter(flushPath))
            {
                ok.WriteLine("这行数据在缓冲区里");
            }
            P($"     用 using 的  : 文件长度 = {new FileInfo(flushPath).Length} 字节");
            P("");
            P("   实验组：不 Dispose，直接丢给 GC：");
            // ★ 注意：这个 writer 从头到尾【没有】Flush 过 ——
            //   上面 ① 里那个 kept 我们都 Flush 过了（所以它的数据是安全的）。
            //   差别就在这一行：有没有把缓冲区交出去。
            string lostPath = Path.Combine(workDir, "lost.txt");
            var sloppy = new DemoFileWriter(lostPath);
            sloppy.WriteLine("这行数据在缓冲区里，还没落盘");

            var weakLeak = new WeakReference(sloppy);
            sloppy = null!;      // ★ null! = "我知道编译器在担心 null，这里我有意为之"
            long lostSizeBefore = new FileInfo(lostPath).Length;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            long lostSizeAfter = new FileInfo(lostPath).Length;
            P($"     不 Dispose 的: 文件长度 = {lostSizeAfter} 字节");
            P($"     （GC 之前 {lostSizeBefore} 字节，GC 之后 {lostSizeAfter} 字节）");
            P($"     泄漏对象还在堆上吗：{weakLeak.IsAlive}");
            P("   ★ 注意 GC 之前就是 0 —— 数据从来没离开过内存。");
            P("     GC 之后还是 0 —— FileStream 的终结器只关句柄，不会替你 Flush，救不回来。");
            P("");
            P("③ 更狠的后果（本示例没演示，但你要知道）：");
            P("   · 文件句柄 / Socket 泄漏到上限 → 抛「Too many open files」，程序只能重启");
            P("   · 数据库连接池被占满 → 整个服务卡死，而且【GC 不会及时救你】");
            P("   · 非托管内存（AllocHGlobal）泄漏 → 任务管理器里进程内存一直涨，托管堆却很小");
            P("");
            P("④ 顺便看一个数字：本程序创建了 8 个 DemoFileWriter，");
            P($"   其中 {DemoFileWriter.FinalizerRuns} 个是被 GC 的终结器收走的，不是被你 Dispose 的。");
            P("   ★ 每个这样的对象都意味着：至少多活一轮 GC（而且它缓冲的数据很可能已经丢了）。");
            P("     上面那 6 个用 using 的，贡献了 0 个。");

            // ────────────────────────────────────────────────────────────────
            Section("实验 7 ｜ Python 对照表");
            // ────────────────────────────────────────────────────────────────
            P("   ┌──────────────────────────┬──────────────────────────────────────┐");
            P("   │ Python                   │ C#                                   │");
            P("   ├──────────────────────────┼──────────────────────────────────────┤");
            P("   │ with open(...) as f:     │ using var f = ...;                   │");
            P("   │ __enter__                │ （没有对应物，using 只需要 IDisposable）│");
            P("   │ __exit__                 │ Dispose()                            │");
            P("   │ __del__                  │ ~ClassName() 终结器                  │");
            P("   │ try/finally 手动清理     │ 一样，但 C# 用 using 更省事           │");
            P("   │ 引用计数（CPython 立刻）  │ GC（分代、非确定）                    │");
            P("   │ 两阶段回收（gc 模块）     │ 无（C# 没有循环引用回收阶段）          │");
            P("   │ await 异步上下文管理器    │ IAsyncDisposable + await using        │");
            P("   └──────────────────────────┴──────────────────────────────────────┘");
            P("");
            P("   ★ 最容易踩的 Python 习惯：");
            P("     CPython 里 f = open(...) 然后忘了 close，函数一返回文件往往就自动关了");
            P("     （引用计数归零 → __del__ → 关文件）。");
            P("     C# 里【没有】这个保证：GC 什么时候来完全不知道，可能几秒后，可能程序结束才来。");
            P("     所以 C# 的规矩很硬：凡是 IDisposable，一律 using。");

            // ────────────────────────────────────────────────────────────────
            Section("实验 8 ｜ 自己动手改坏它（本课作业）");
            // ────────────────────────────────────────────────────────────────
            P("打开 Lesson20Demo\\Program.cs，按下面 5 条各改一次，每次改完重新运行：");
            P("");
            P("  ① 把 DemoOrder.Dispose() 里的 GC.SuppressFinalize(this); 注释掉");
            P("     预期：c 会同时出现「Dispose : c」和「★终结器 : c」—— 同一个对象被释放两次。");
            P("          现在 c 的 Dispose 里没干什么坏事，所以只是多打一行；");
            P("          换成 NativeBuffer 那种 FreeHGlobal，就是重复释放 → 未定义行为 → 崩溃。");
            P("");
            P("  ② 把 NativeBuffer 的终结器 ~NativeBuffer() => Dispose(false); 删掉，");
            P("     并把 Dispose 里的 GC.SuppressFinalize 也删掉，然后跑「实验 3」的内存循环");
            P("     预期：没有兜底了。忘记 Dispose → AllocHGlobal 的内存永久泄漏，");
            P("          任务管理器里能看到内存不回落。");
            P("");
            P("  ③ 把 Dispose(bool disposing) 里 _stream.Dispose(); 外面的 if (disposing) 拿掉");
            P("     （也就是让终结器线程也去碰托管对象）");
            P("     预期：行为变得不可预测。运气好没事，运气差抛 NullReferenceException /");
            P("          ObjectDisposedException，而且是【终结器线程】上抛 —— 直接杀进程，无法 catch。");
            P("");
            P("  ④ 在 NativeBuffer 的终结器里加一句 Console.WriteLine(\"终结器在跑\");");
            P("     预期：输出会【插在别的地方】，而不是你写的那一行 ——");
            P("          这就是「时机不确定」最直观的样子。");
            P("");
            P("  ⑤ 把实验 5 里的 using (var w1 = new DemoFileWriter(usingPath)) 改成");
            P("     直接 new DemoFileWriter(usingPath) 不写 using");
            P("     预期：文件长度变成 0（数据全在缓冲区里跟着对象一起丢）。");
            P("          改回来，想想你刚才省掉的那几个字符值多少钱。");

            // ────────────────────────────────────────────────────────────────
            Section("本课一句话总结");
            // ────────────────────────────────────────────────────────────────
            P("  GC 负责【内存】，不负责【资源】。");
            P("  资源靠 Dispose，Dispose 靠 using，终结器只是忘了写 using 时的保险。");
            P("  每写一个持有资源的类，问自己两句：");
            P("     ① 我【直接】持有非托管资源吗？是 → 才需要终结器。");
            P("     ② 用了这个类的每一处，都包在 using 里了吗？");

            Console.WriteLine();
            Console.WriteLine("===== 示例结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
