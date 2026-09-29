using System.Collections;
using System.Diagnostics;

namespace Experiments;

/// <summary>
/// 第 9 周 · 实验 13：泛型的运行时特化 —— 装箱的真实代价
/// </summary>
/// <remarks>
/// 结论：
///   1. C# 的泛型是「具现化（reified）」的：运行时完整保留类型信息。
///   2. 值类型实参会被【特化】：List&lt;int&gt; 和 List&lt;double&gt; 有两份不同的原生代码，
///      int 直接内联在数组里 → 零装箱。
///   3. 引用类型实参【共享】JIT 代码（因为引用都是 8 字节指针）。
///   4. ArrayList / Hashtable 等非泛型集合把元素存成 object → 每次读写都装箱/拆箱。
///   5. 对比：Java 的类型擦除会丢失类型信息；Python 无泛型，纯动态。
/// </remarks>
public static class E13_GenericsVsArrayList
{
    private static long Allocated() => GC.GetAllocatedBytesForCurrentThread();

    private static (double Ms, double Mb, int Gen0) Measure(Action action, int warmup = 1)
    {
        for (int i = 0; i < warmup; i++) action();

        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        var gen0Before = GC.CollectionCount(0);
        var before = Allocated();
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        return (sw.Elapsed.TotalMilliseconds, (Allocated() - before) / 1024.0 / 1024.0,
                GC.CollectionCount(0) - gen0Before);
    }

    private static void Report(string label, (double Ms, double Mb, int Gen0) r)
    {
        Console.WriteLine($"  {label,-30} {r.Ms,9:F1} ms {r.Mb,10:F2} MB {r.Gen0,7} 次");
    }

    public static void Run()
    {
        const int N = 5_000_000;

        Console.WriteLine($"【实验 A：ArrayList vs List<int>（{N:N0} 次写入）】");
        Console.WriteLine();
        Console.WriteLine($"  {"集合类型",-30} {"耗时",9} {"分配",10} {"Gen0 GC",8}");
        Console.WriteLine("  " + new string('─', 62));

        Report("ArrayList（非泛型，装箱）", Measure(() =>
        {
            var list = new ArrayList();
            for (int i = 0; i < N; i++) list.Add(i);
            GC.KeepAlive(list);
        }));

        Report("List<int>（泛型，零装箱）", Measure(() =>
        {
            var list = new List<int>(N);
            for (int i = 0; i < N; i++) list.Add(i);
            GC.KeepAlive(list);
        }));

        Console.WriteLine();
        Console.WriteLine($"  ★ ArrayList 每次 Add 都要装箱一个 int（24 字节），");
        Console.WriteLine($"    所以它分配了几百 MB、触发了多次 GC。");
        Console.WriteLine();

        Console.WriteLine("【实验 B：读取时的拆箱成本】");
        Console.WriteLine();
        Console.WriteLine($"  {"操作",-30} {"耗时",9} {"分配",10} {"Gen0 GC",8}");
        Console.WriteLine("  " + new string('─', 62));

        // 先准备好数据（不计入测量）
        var arrayList = new ArrayList();
        var genericList = new List<int>(N);
        for (int i = 0; i < N; i++) { arrayList.Add(i); genericList.Add(i); }

        Report("ArrayList 求和（拆箱）", Measure(() =>
        {
            long sum = 0;
            for (int i = 0; i < arrayList.Count; i++)
                sum += (int)arrayList[i]!;              // 拆箱
            GC.KeepAlive(sum);
        }));

        Report("List<int> 求和", Measure(() =>
        {
            long sum = 0;
            for (int i = 0; i < genericList.Count; i++)
                sum += genericList[i];                  // 无拆箱
            GC.KeepAlive(sum);
        }));

        Console.WriteLine();

        Console.WriteLine("【实验 C：泛型的类型身份】");
        Console.WriteLine();
        Console.WriteLine($"  typeof(List<int>)    == typeof(List<double>)   : {typeof(List<int>) == typeof(List<double>)}");
        Console.WriteLine($"  typeof(List<string>) == typeof(List<object>)   : {typeof(List<string>) == typeof(List<object>)}");
        Console.WriteLine($"  typeof(List<int>).IsGenericType                 : {typeof(List<int>).IsGenericType}");
        Console.WriteLine($"  typeof(List<int>).GetGenericTypeDefinition()    : {typeof(List<int>).GetGenericTypeDefinition().Name}");
        Console.WriteLine();
        Console.WriteLine("  ★ List<int> 和 List<double> 是【两个不同的运行时类型】，");
        Console.WriteLine("    各有各的原生代码（值类型特化）。");
        Console.WriteLine("  ★ List<string> 和 List<object> 也是不同类型，但 JIT 内部");
        Console.WriteLine("    【共享】同一份代码 —— 因为引用的大小都是 8 字节。");
        Console.WriteLine();

        Console.WriteLine("【实验 D：泛型类型的静态字段是独立的】");
        Console.WriteLine();

        Counter<int>.Count = 1;
        Counter<string>.Count = 2;
        Counter<double>.Count = 3;

        Console.WriteLine($"  Counter<int>.Count    = {Counter<int>.Count}");
        Console.WriteLine($"  Counter<string>.Count = {Counter<string>.Count}");
        Console.WriteLine($"  Counter<double>.Count = {Counter<double>.Count}");
        Console.WriteLine();
        Console.WriteLine("  ★ 每个「具体泛型类型」在运行时都是一个独立类型，有自己的静态字段。");
        Console.WriteLine("  ★ 这是实现「按类型缓存」的常用技巧，例如：");
        Console.WriteLine("        class Serializer<T> { static readonly JsonSerializerOptions Opts = ...; }");
        Console.WriteLine("    每种 T 自动获得独立缓存，无需 Dictionary。");
        Console.WriteLine();

        Console.WriteLine("【实验 E：泛型约束决定性能】");
        Console.WriteLine();

        Report("Max1<T>（无约束，装箱）", Measure(() =>
        {
            long sum = 0;
            for (int i = 0; i < 1_000_000; i++) sum += Max1(i, i + 1);
            GC.KeepAlive(sum);
        }, warmup: 10));

        Report("Max2<T>（有约束，无装箱）", Measure(() =>
        {
            long sum = 0;
            for (int i = 0; i < 1_000_000; i++) sum += Max2(i, i + 1);
            GC.KeepAlive(sum);
        }, warmup: 10));

        Console.WriteLine();
        Console.WriteLine("  ★ 无约束时 T 被当成 object 处理，调用接口方法要装箱；");
        Console.WriteLine("    有 IComparable<T> 约束时编译器直接调用，零装箱。");
        Console.WriteLine("  ★ 所以「加约束」不只是为了类型安全，也是为了性能。");
        Console.WriteLine();

        Console.WriteLine("【动手任务】");
        Console.WriteLine("  1. 打开 ILSpy，对比 ArrayList.Add 和 List<int>.Add 的 IL，");
        Console.WriteLine("     找到 ArrayList 里的 `box` 指令");
        Console.WriteLine("  2. 把一个旧项目里的 ArrayList / Hashtable / object[] 全换成泛型版本，");
        Console.WriteLine("     用这个实验的方法量化改进");
        Console.WriteLine("  3. 思考：为什么 List<ValueType>（100 字节的 struct）反而可能比");
        Console.WriteLine("     List<SomeClass>（引用）慢？（提示：复制开销 vs 堆分配开销）");
    }

    /// <summary>无约束：T 被当作 object，比较需要装箱。</summary>
    private static T Max1<T>(T a, T b)
    {
        return Comparer<T>.Default.Compare(a, b) >= 0 ? a : b;   // Comparer<T> 内部对值类型会装箱
    }

    /// <summary>有约束：编译器知道 T 实现了 IComparable&lt;T&gt;，可直接调用。</summary>
    private static T Max2<T>(T a, T b) where T : IComparable<T>
    {
        return a.CompareTo(b) >= 0 ? a : b;                       // 无装箱
    }

    private static class Counter<T>
    {
        public static int Count;
    }
}
