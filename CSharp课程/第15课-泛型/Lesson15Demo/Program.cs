// ============================================================================
//  第 15 课 · 示例代码
//  泛型
// ============================================================================

using System.Collections;
using System.Diagnostics;

namespace Lesson15Demo
{
    // ========================================================================
    // 一、自己写的泛型类
    // ========================================================================
    class Box<T>
    {
        private T _value;

        public Box(T value) => _value = value;

        public T GetValue() => _value;
        public void SetValue(T value) => _value = value;

        public override string ToString() => $"Box({_value})";
    }

    // 两个类型参数
    class Pair<TKey, TValue>
    {
        public TKey Key { get; }
        public TValue Value { get; }

        public Pair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString() => $"[{Key} = {Value}]";
    }

    // 泛型静态字段（用来演示"每种 T 独立"）
    class Counter<T>
    {
        public static int Count;
    }

    record Student(string Name, int Grade);

    // ★ 专门用来演示 new() 约束的类
    //   注意：record Student(...) 是【位置记录】，它只有一个带参构造函数，
    //   不满足 new() 约束（没有公开无参构造函数）。
    //   这个类有隐式无参构造函数，所以可以。
    class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }

        public override string ToString() => $"Person(未命名, {Age})";
    }

    class Program
    {
        // ====================================================================
        // 二、泛型方法
        // ====================================================================

        // 没有约束 → 不能调用 a.CompareTo
        // static T MaxBad<T>(T a, T b) => a.CompareTo(b) > 0 ? a : b;   // ❌ 编译错误

        // 加了 IComparable<T> 约束 → 可以比较
        static T Max<T>(T a, T b) where T : IComparable<T>
        {
            return a.CompareTo(b) > 0 ? a : b;
        }

        // ref 参数 + 泛型（第 04 课 + 本课）
        static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        // new() 约束 → 能创建实例
        static T CreateDefault<T>() where T : new() => new T();

        // struct 约束 → 能拿零值
        static T GetZero<T>() where T : struct => default;

        // 引用类型约束 → 返回 default（对引用类型就是 null）
        static T? GetDefaultRef<T>() where T : class => default;

        static void Main()
        {
            Console.WriteLine("========== 第 15 课：泛型 ==========\n");


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 1：自己写的泛型类 ----");

            var intBox = new Box<int>(42);
            var strBox = new Box<string>("hello");
            var stuBox = new Box<Student>(new Student("张三", 88));

            Console.WriteLine($"   new Box<int>(42)                → {intBox}");
            Console.WriteLine($"   new Box<string>(\"hello\")        → {strBox}");
            Console.WriteLine($"   new Box<Student>(...)           → {stuBox}");
            Console.WriteLine();
            Console.WriteLine($"   intBox.GetValue() 的类型是 int，可以直接算数：{intBox.GetValue() * 2}");
            Console.WriteLine("   ★ 同一个 Box<T> 类，T 填什么就装什么，而且类型安全");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 2：多个类型参数 ----");

            var p1 = new Pair<string, int>("年龄", 25);
            var p2 = new Pair<int, string>(1001, "张三");

            Console.WriteLine($"   new Pair<string, int>(\"年龄\", 25)  → {p1}");
            Console.WriteLine($"   new Pair<int, string>(1001, \"张三\") → {p2}");
            Console.WriteLine("   ★ 这就是 Dictionary<TKey, TValue> 的原理");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 3：泛型方法（类型自动推断）----");

            Console.WriteLine($"   Max(3, 7)         = {Max(3, 7)}          ← T 推断为 int");
            Console.WriteLine($"   Max(3.14, 2.71)   = {Max(3.14, 2.71)}     ← T 推断为 double");
            Console.WriteLine($"   Max(\"abc\", \"xyz\") = {Max("abc", "xyz")}      ← T 推断为 string");
            Console.WriteLine($"   Max<int>(3, 7)    = {Max<int>(3, 7)}          ← 也可以显式指定");
            Console.WriteLine();

            int x = 1, y = 2;
            Swap(ref x, ref y);
            Console.WriteLine($"   Swap 之后：x = {x}, y = {y}");
            Console.WriteLine("   ★ 泛型方法不用为每种类型重写，编译器自动推断");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 4：★ 泛型约束 ----");

            Console.WriteLine($"   new() 约束：CreateDefault<Person>() = {CreateDefault<Person>()}");
            Console.WriteLine($"      （Person 有隐式无参构造函数，所以满足 new() 约束）");
            Console.WriteLine($"   ⚠️ 注意：record Student(string,int) 是位置记录，只有带参构造函数，");
            Console.WriteLine($"      不满足 new() 约束 —— 这是个真实的踩坑点");
            Console.WriteLine($"   struct 约束：GetZero<int>() = {GetZero<int>()}");
            Console.WriteLine($"                 GetZero<bool>() = {GetZero<bool>()}");
            Console.WriteLine();
            Console.WriteLine("   常用约束：");
            Console.WriteLine("      where T : class            T 必须是引用类型");
            Console.WriteLine("      where T : struct           T 必须是值类型");
            Console.WriteLine("      where T : new()            T 必须有公开无参构造函数");
            Console.WriteLine("      where T : IComparable<T>   T 必须实现某接口");
            Console.WriteLine("      where T : BaseClass        T 必须继承某个类");
            Console.WriteLine();
            Console.WriteLine("   ★ 没有约束时，编译器不知道 T 有哪些成员，什么都调不了");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 5：★ 泛型 vs object（装箱对照，实测）----");

            const int N = 2_000_000;

            // ❌ 非泛型集合：每次 Add 都装箱
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            long before = GC.GetAllocatedBytesForCurrentThread();
            var sw = Stopwatch.StartNew();
            var oldList = new ArrayList();
            for (int i = 0; i < N; i++) oldList.Add(i);
            sw.Stop();
            long allocOld = GC.GetAllocatedBytesForCurrentThread() - before;
            double oldMs = sw.Elapsed.TotalMilliseconds;

            // ✅ 泛型集合：零装箱
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            before = GC.GetAllocatedBytesForCurrentThread();
            sw.Restart();
            var newList = new List<int>(N);
            for (int i = 0; i < N; i++) newList.Add(i);
            sw.Stop();
            long allocNew = GC.GetAllocatedBytesForCurrentThread() - before;
            double newMs = sw.Elapsed.TotalMilliseconds;

            Console.WriteLine($"   往集合里 Add {N:N0} 个 int：");
            Console.WriteLine($"      ArrayList（object）  : {oldMs,8:F1} ms, 分配 {allocOld / 1024.0 / 1024,8:F1} MB");
            Console.WriteLine($"      List<int>（泛型）    : {newMs,8:F1} ms, 分配 {allocNew / 1024.0 / 1024,8:F1} MB");
            if (newMs > 0.01 && newMs > 0)
            {
                Console.WriteLine($"      → 泛型快了约 {oldMs / newMs:F1} 倍，少分配 {allocOld / Math.Max(1, allocNew):F0} 倍内存");
            }
            Console.WriteLine();
            Console.WriteLine("   原因：object 每次 Add 都要【装箱】（堆上分配一个 24 字节的盒子）");
            Console.WriteLine("   ★ 这是泛型最大的价值 —— 性能收益大于代码复用收益");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 6：★ 泛型的运行时特化 ----");

            Console.WriteLine($"   typeof(List<int>)    == typeof(List<double>) : {typeof(List<int>) == typeof(List<double>)}");
            Console.WriteLine($"   typeof(List<string>) == typeof(List<object>) : {typeof(List<string>) == typeof(List<object>)}");
            Console.WriteLine();
            Console.WriteLine("   两个关键机制：");
            Console.WriteLine("      ① 值类型【特化】：List<int> 和 List<double> 是两份不同的原生代码，");
            Console.WriteLine("         int 直接内联在数组里 → 零装箱");
            Console.WriteLine("      ② 引用类型【共享】：List<string> 和 List<object> 类型身份不同，");
            Console.WriteLine("         但 JIT 内部共享同一份代码（因为引用都是 8 字节地址）");
            Console.WriteLine();
            Console.WriteLine("   对比其他语言：");
            Console.WriteLine("      Java   类型擦除（编译后都是 Object）→ 值类型要装箱");
            Console.WriteLine("      Python 没有泛型（动态类型）→ 运行时才知道");
            Console.WriteLine("      C#     具现化（运行时保留类型信息）→ 值类型特化，零装箱");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 7：★ 泛型静态字段是独立的 ----");

            Counter<int>.Count = 1;
            Counter<string>.Count = 2;
            Counter<double>.Count = 3;

            Console.WriteLine($"   Counter<int>.Count    = {Counter<int>.Count}");
            Console.WriteLine($"   Counter<string>.Count = {Counter<string>.Count}");
            Console.WriteLine($"   Counter<double>.Count = {Counter<double>.Count}");
            Console.WriteLine();
            Console.WriteLine("   ★ 每个具体泛型类型（Counter<int>、Counter<string>）");
            Console.WriteLine("     在运行时都是【独立的类型】，各有各的静态字段");
            Console.WriteLine();
            Console.WriteLine("   实用技巧：可以用这个做按类型自动缓存，不需要 Dictionary：");
            Console.WriteLine("        class Serializer<T> { static readonly Options Opts = new(); }");
            Console.WriteLine("     每种 T 自动获得独立缓存");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 8：default(T) 的含义 ----");

            Console.WriteLine($"   default(int)          = {default(int)}");
            Console.WriteLine($"   default(bool)         = {default(bool)}");
            Console.WriteLine($"   default(double)       = {default(double)}");

            // ★ 这里用泛型方法（不是直接 default(Student)），避免"表达式始终与常量匹配"的警告
            Student? defaultStu = GetDefaultRef<Student>();
            Console.WriteLine($"   default(Student)      = {(defaultStu is null ? "null（引用类型默认是 null）" : defaultStu.ToString())}");
            Console.WriteLine();
            Console.WriteLine("   ★ 回忆第 01 课你踩的坑：把 SPoint 从 struct 改成 class 之后，");
            Console.WriteLine("     default(SPoint) 从 (0,0) 变成了 null —— 这就是 SPoint? 失去 HasValue 的原因");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson15Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
