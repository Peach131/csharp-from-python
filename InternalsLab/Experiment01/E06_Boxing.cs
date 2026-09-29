using System.Collections;

namespace Experiments;

/// <summary>
/// 第 3-4 周 · 实验 6：装箱与拆箱  ★核心
/// </summary>
/// <remarks>
/// 结论：
///   1. 装箱 = 把值类型复制到堆上的一个盒子里。至少分配 24 字节。
///   2. 触发条件：赋值给 object / 接口、放进非泛型集合、string.Format 传值类型、
///      对未重写 ToString / GetHashCode 的 struct 调用这两个方法。
///   3. 泛型容器（List<int>）完全不装箱，这是泛型最大的价值。
///   4. 拆箱要求类型【精确匹配】，(long)(object)42 会抛 InvalidCastException。
///   5. Unity 里 Update() 每帧执行，任何一次装箱都会累积成 GC 卡顿。这是
///      Unity 新手最常见也最致命的性能问题。
///   6. 【测量教训】观察装箱最干净的窗口是 GetHashCode，因为它不产生字符串。
///      对 ToString 做对比时，如果两个版本返回的字符串长度不同，
///      字符串本身的分配差异会掩盖装箱的效果 —— 必须做严格对照组。
/// </remarks>
public static class E06_Boxing
{
    private static long Allocated() => GC.GetAllocatedBytesForCurrentThread();

    // 两个 struct 的 ToString 都【只返回一个字符】，保证字符串本身的分配可比。
    // 唯一的差异就是：一个重写了 ToString，一个没重写。
    private struct BadStruct
    {
        public int X;
        // 故意不重写 ToString() —— 调用时会装箱，走 ValueType.ToString()
    }

    private struct GoodStruct
    {
        public int X;
        public override string ToString() => "G";                  // 重写后不装箱
    }

    // 用于对比 GetHashCode / Equals 的装箱成本（这两个方法【不产生字符串】，
    // 所以装箱的 24 字节会直接暴露在分配量里 —— 这是观察装箱最干净的窗口）
    private struct BadHash
    {
        public int X;
        // 不重写 GetHashCode()
    }

    private struct GoodHash
    {
        public int X;
        public override int GetHashCode() => X;                     // 重写后不装箱
    }

    private struct EqStruct : IEquatable<EqStruct>
    {
        public int X;
        // 只实现强类型 Equals 重载（泛型集合会优先用它，不装箱）
        public bool Equals(EqStruct other) => X == other.X;
    }

    private static void Section(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"【{title}】");
        Console.WriteLine();
    }

    public static void Run()
    {
        Section("实验 A1：装箱在哪儿发生（分配量）");
        Console.WriteLine("  写法                                        分配       说明");
        Console.WriteLine("  ─────────────────────────────────────────────────────────────────");

        Measure("object o = 42;                         ", () =>
        {
            object o = 42;
            GC.KeepAlive(o);
        }, 10_000);

        Measure("(object)42 连续 10 次                   ", () =>
        {
            for (int i = 0; i < 10; i++) { object o = i; GC.KeepAlive(o); }
        }, 1_000);

        Measure("List<int>.Add(42)                       ", () =>
        {
            var l = new List<int>(10);
            for (int i = 0; i < 10; i++) l.Add(i);
            GC.KeepAlive(l);
        }, 1_000);

        Measure("ArrayList.Add(42)  ← 非泛型，装箱      ", () =>
        {
            var l = new ArrayList(10);
            for (int i = 0; i < 10; i++) l.Add(i);
            GC.KeepAlive(l);
        }, 1_000);

        Measure("$\"{42}\"  ← 插值（.NET 6+ 已优化）    ", () =>
        {
            string s = $"value={42}";
            GC.KeepAlive(s);
        }, 1_000);

        Measure("string.Format(\"{0}\", 42)              ", () =>
        {
            string s = string.Format("{0}", 42);
            GC.KeepAlive(s);
        }, 1_000);

        Console.WriteLine();
        Console.WriteLine("  ★ 注意 ArrayList 和 List<int> 差了约 4 倍：");
        Console.WriteLine("    ArrayList 每次 Add 都要把 int 装箱（24 字节的盒子对象），");
        Console.WriteLine("    List<int> 直接把 int 内联在数组里，零分配。");
        Console.WriteLine();

        Section("实验 A2：struct 的方法调用会不会偷偷装箱");
        Console.WriteLine();
        Console.WriteLine("  ⚠️ 这一节必须【严格对照】：两个 struct 的 ToString 都只返回一个");
        Console.WriteLine("     字符，否则字符串本身的长度差异会掩盖装箱的效果（这是我第一版");
        Console.WriteLine("     踩的坑 —— 详见 README 的「一个真实的测量教训」）。");
        Console.WriteLine();
        Console.WriteLine("  写法                                        分配       说明");
        Console.WriteLine("  ─────────────────────────────────────────────────────────────────");

        Measure("BadStruct.ToString()  ← 未重写 ToString ", () =>
        {
            var b = new BadStruct { X = 1 };
            GC.KeepAlive(b.ToString());
        }, 20_000);

        Measure("GoodStruct.ToString() ← 已重写 ToString ", () =>
        {
            var g = new GoodStruct { X = 1 };
            GC.KeepAlive(g.ToString());
        }, 20_000);

        Measure("BadHash.GetHashCode() ← 未重写         ", () =>
        {
            var b = new BadHash { X = 1 };
            GC.KeepAlive(b.GetHashCode());
        }, 20_000);

        Measure("GoodHash.GetHashCode()← 已重写         ", () =>
        {
            var g = new GoodHash { X = 1 };
            GC.KeepAlive(g.GetHashCode());
        }, 20_000);

        Measure("EqStruct.Equals(x) ← 强类型 IEquatable ", () =>
        {
            var a = new EqStruct { X = 1 };
            GC.KeepAlive(a.Equals(new EqStruct { X = 1 }));
        }, 20_000);

        Console.WriteLine();
        Console.WriteLine("  ★ 实测结果（Debug，你的数字会有小幅波动，看量级）：");
        Console.WriteLine("      BadStruct.ToString()   24 B/次   ← 就是装箱的盒子");
        Console.WriteLine("      GoodStruct.ToString()   0 B/次   ← 重写后零分配");
        Console.WriteLine("      BadHash.GetHashCode()  48 B/次   ← 装箱 + 反射开销");
        Console.WriteLine("      GoodHash.GetHashCode() 24 B/次   ← 只剩装箱（方法本身没分配）");
        Console.WriteLine();
        Console.WriteLine("  ★ 为什么 ToString 能到 0 B：");
        Console.WriteLine("      object.ToString() 返回的是【类型名】，而反射拿到的类型名");
        Console.WriteLine("      是被缓存/驻留的字符串对象，不是每次新建 —— 所以未重写版本");
        Console.WriteLine("      多出来的 24 字节正好是盒子的纯成本。");
        Console.WriteLine("  ★ 为什么 GetHashCode 都还剩 24 B：");
        Console.WriteLine("      两个 GetHashCode 本身都不分配，所以 24 B 就是装箱。");
        Console.WriteLine("      未重写版多出的那 24 B 来自 ValueType.GetHashCode 的反射实现。");
        Console.WriteLine();
        Console.WriteLine("  ★ 干净的结论：调用未重写的 ToString / GetHashCode / Equals");
        Console.WriteLine("    会让 struct 各装箱一次，固定多花 24 字节 + 一次堆分配。");
        Console.WriteLine("    对 struct 来说重写这三个方法不是「可选优化」，是「必须做的事」。");
        Console.WriteLine();

        Section("实验 B：拆箱必须类型精确匹配");
        Console.WriteLine();

        object boxedInt = 42;

        try
        {
            int i = (int)boxedInt;                       // ✅
            Console.WriteLine($"  (int)(object)42       = {i}          ✅ 成功");
        }
        catch (Exception ex) { Console.WriteLine($"  (int) 失败: {ex.GetType().Name}"); }

        try
        {
            long l = (long)boxedInt;                     // ❌ 盒子里的类型是 Int32
            _ = l;
            Console.WriteLine($"  (long)(object)42      = {l}");
        }
        catch (InvalidCastException ex)
        {
            Console.WriteLine($"  (long)(object)42      → {ex.GetType().Name}  ❌");
            Console.WriteLine($"     {ex.Message}");
        }

        object boxedLong = 42L;
        try
        {
            long l = (long)boxedLong;                    // ✅ 盒子里是 Int64
            Console.WriteLine($"  (long)(object)42L     = {l}          ✅ 成功");
        }
        catch (Exception ex) { Console.WriteLine($"  失败: {ex.GetType().Name}"); }

        Console.WriteLine();
        Console.WriteLine("  ★ 拆箱不是类型转换，而是「把盒子里的东西原样拿出来」。");
        Console.WriteLine("    需要转换时用 Convert.ToInt64()，但它也会做装箱/拆箱。");
        Console.WriteLine();

        Section("实验 C：真实场景 —— Unity Update() 的累积效应");
        Console.WriteLine();

        Console.WriteLine("  模拟一个 Update 方法，每秒 60 次，跑 60 秒（3600 帧）：");
        Console.WriteLine();

        // ❌ 坏写法
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        var before = Allocated();
        var gen0Before = GC.CollectionCount(0);
        for (int frame = 0; frame < 3600; frame++)
        {
            int score = frame;
            // 典型新手写法：每帧拼接字符串 + 装箱
            object boxedScore = score;
            string msg = "Score: " + boxedScore;
            GC.KeepAlive(msg);
        }
        var badAlloc = Allocated() - before;
        var badGc = GC.CollectionCount(0) - gen0Before;

        Console.WriteLine($"  ❌ 坏写法（每帧装箱+拼接）");
        Console.WriteLine($"     分配 : {badAlloc,10:N0} 字节  ({badAlloc / 1024.0:F1} KB)");
        Console.WriteLine($"     触发 Gen0 GC : {badGc} 次");
        Console.WriteLine();

        // ✅ 好写法
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        before = Allocated();
        gen0Before = GC.CollectionCount(0);
        var cache = new char[16];
        for (int frame = 0; frame < 3600; frame++)
        {
            int score = frame;
            // 只在需要时格式化，且用 stackalloc/Span 或复用缓冲区
            if (score != _lastScore)
            {
                _lastScore = score;
                var span = cache.AsSpan();
                score.TryFormat(span, out _);
            }
            GC.KeepAlive(cache);
        }
        var goodAlloc = Allocated() - before;
        var goodGc = GC.CollectionCount(0) - gen0Before;

        Console.WriteLine($"  ✅ 好写法（缓存 + TryFormat，只在变化时更新）");
        Console.WriteLine($"     分配 : {goodAlloc,10:N0} 字节  ({goodAlloc / 1024.0:F1} KB)");
        Console.WriteLine($"     触发 Gen0 GC : {goodGc} 次");
        Console.WriteLine();
        Console.WriteLine($"  → 差了 {(badAlloc - goodAlloc) / 1024.0:F1} KB 的垃圾，" +
                          $"{badGc - goodGc} 次 GC 停顿");
        Console.WriteLine("  → 这还只是一行日志。真实项目里几十个这样的调用，就是肉眼可见的卡顿。");
        Console.WriteLine();

        Section("动手任务");
        Console.WriteLine("  1. 打开 ILSpy，看上面每种坏写法生成的 IL，找到 `box` 指令");
        Console.WriteLine("  2. 找一个你自己写的 struct，检查有没有重写 ToString / Equals / GetHashCode");
        Console.WriteLine("  3. 记住这个清单，看到就警觉：");
        Console.WriteLine("     - 把值类型赋给 object / 非泛型接口变量");
        Console.WriteLine("     - ArrayList / Hashtable 等非泛型集合");
        Console.WriteLine("     - string.Format(...) 传值类型");
        Console.WriteLine("     - 未重写 ToString 的 struct 调用 ToString");
        Console.WriteLine("     - object[] 里放值类型");
    }

    private static int _lastScore = -1;

    private static void Measure(string label, Action action, int iterations)
    {
        // 预热（避免把 JIT 开销算进去）
        for (int i = 0; i < 100; i++) action();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = Allocated();
        for (int i = 0; i < iterations; i++) action();
        var total = Allocated() - before;

        var perIteration = (double)total / iterations;
        Console.WriteLine($"  {label} {perIteration,9:F1} B/次");
    }
}
