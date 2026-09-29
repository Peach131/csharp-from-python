// ============================================================================
//  第 11 课 · 示例代码
//  值类型与引用类型（深入）—— 从内存层面看清"变量里装了什么"
// ============================================================================

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lesson11Demo
{
    // 用于测量对象大小的类
    //
    // ⚠️ 这些类的字段【故意】从不赋值 —— 我们只关心"占多少内存"，
    //    不关心字段的值。编译器会为此发出 CS0649 警告，所以这里显式关掉。
#pragma warning disable CS0649
    class Empty { }
    class OneInt { public int A; }
    class TwoInt { public int A; public int B; }
    class ThreeInt { public int A; public int B; public int C; }

    struct SOneInt { public int A; }
    struct STwoInt { public int A; public int B; }
#pragma warning restore CS0649

    // 值类型嵌套在类里
    class Holder
    {
        public int Number;              // 这个 int 在【堆】上
        public SOneInt Point;           // 这个 struct 也在【堆】上
    }

    class Program
    {
        /// <summary>用「分配前/后的存活堆差」测量每个对象的实际占用</summary>
        /// <remarks>
        /// ⚠️ 注意这里用 double 除再四舍五入，【不要】用整除。
        ///    整除会把 24 截断成 23，让你以为对象只占 23 字节。
        ///    这是本示例第一版真实踩过的坑。
        /// </remarks>
        static long MeasureClass<T>(int count = 50_000) where T : new()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var before = GC.GetTotalMemory(true);
            var arr = new T[count];
            for (int i = 0; i < count; i++) arr[i] = new T();
            GC.KeepAlive(arr);
            var after = GC.GetTotalMemory(true);

            // 减去引用数组自身的开销：24 字节头 + 8 字节/元素
            long totalObjects = after - before - (24 + count * 8L);

            // ★ 用 double 除，再四舍五入到最近的 8 字节倍数（.NET 对象按 8 字节对齐）
            double perObject = (double)totalObjects / count;
            return (long)(Math.Round(perObject / 8.0) * 8);
        }

        static void Main()
        {
            Console.WriteLine("========== 第 11 课：值类型与引用类型（深入）==========\n");


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 1：变量里到底装了什么 ----");

            int valueVar = 42;
            var refVar = new OneInt { A = 42 };

            Console.WriteLine($"   值类型变量：int valueVar = 42;");
            Console.WriteLine($"      → 变量里【直接装着 42】这 4 个字节");
            Console.WriteLine($"      → 读出 valueVar = {valueVar}（读的就是变量自己那 4 个字节）");
            Console.WriteLine();
            Console.WriteLine($"   引用类型变量：var refVar = new OneInt();");
            Console.WriteLine($"      → 变量里装着【一个地址】，地址指向堆上的对象");
            Console.WriteLine($"      → 那个对象里有：对象头(8) + MethodTable指针(8) + 字段(4) + 填充");
            Console.WriteLine();
            Console.WriteLine($"   证明：ReferenceEquals 比的就是【地址】");
            var a = refVar;
            var b = refVar;
            var c = new OneInt { A = 42 };
            Console.WriteLine($"      ReferenceEquals(refVar, a) = {ReferenceEquals(refVar, a)}   ← 同一个地址");
            Console.WriteLine($"      ReferenceEquals(refVar, c) = {ReferenceEquals(refVar, c)}  ← 不同地址（虽然内容一样）");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 2：★ 对象至少占 24 字节（实测）----");

            Console.WriteLine($"   {"类型",-24}{"字段",-14}{"实测大小"}");
            Console.WriteLine("   " + new string('-', 52));
            Console.WriteLine($"   {"Empty（无字段）",-22}{"—",-14}{MeasureClass<Empty>(),4} 字节");
            Console.WriteLine($"   {"OneInt（1×int）",-22}{"4 字节",-14}{MeasureClass<OneInt>(),4} 字节");
            Console.WriteLine($"   {"TwoInt（2×int）",-22}{"8 字节",-14}{MeasureClass<TwoInt>(),4} 字节");
            Console.WriteLine($"   {"ThreeInt（3×int）",-22}{"12 字节",-14}{MeasureClass<ThreeInt>(),4} 字节");
            Console.WriteLine();
            Console.WriteLine("   ★ 规律：16（对象头 + MethodTable 指针）+ 字段，然后【向上对齐到 8 字节】");
            Console.WriteLine("     Empty    : 16 + 0  = 16 → 对齐到 24（最小对象是 24）");
            Console.WriteLine("     OneInt   : 16 + 4  = 20 → 对齐到 24   ← 和 Empty 一样大！");
            Console.WriteLine("     TwoInt   : 16 + 8  = 24 → 24");
            Console.WriteLine("     ThreeInt : 16 + 12 = 28 → 对齐到 32");
            Console.WriteLine();
            Console.WriteLine("   ★ 所以 OneInt 的那个 int 是\"免费\"的 —— 它塞进了对齐填充里。");
            Console.WriteLine("     这也说明：对象大小不是\"字段之和\"，而是按 8 字节对齐后的结果。");
            Console.WriteLine();

            unsafe
            {
                Console.WriteLine("   对比：直接用 sizeof 看【数据本身】的大小（不含对象头）");
                Console.WriteLine($"      sizeof(SOneInt)  = {sizeof(SOneInt)}   ← struct 没有对象头！");
                Console.WriteLine($"      sizeof(STwoInt)  = {sizeof(STwoInt)}");
                Console.WriteLine($"      sizeof(int)      = {sizeof(int)}");
                Console.WriteLine($"      sizeof(IntPtr)   = {sizeof(IntPtr)}   ← 引用（地址）的大小");
            }
            Console.WriteLine("   ★ 这就是 struct 的第一个好处：省掉 16 字节的对象头");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 3：★ struct 嵌套在 class 里，它在哪？ ----");

            var holder = new Holder { Number = 5, Point = new SOneInt { A = 7 } };
            Console.WriteLine($"   class Holder {{ public int Number; public SOneInt Point; }}");
            Console.WriteLine($"   var holder = new Holder();");
            Console.WriteLine();
            Console.WriteLine("   holder 这个【变量】里装的是地址（栈上）");
            Console.WriteLine("   它指向的【对象】在堆上，对象里【内嵌】着：");
            Console.WriteLine("      · Number 这个 int");
            Console.WriteLine("      · Point 这个 struct");
            Console.WriteLine();
            Console.WriteLine("   ★ 所以：\"值类型在栈上\"是错的！");
            Console.WriteLine("     类的字段里的 struct 和 int，都住在【堆】上（对象的一部分）");
            Console.WriteLine();
            Console.WriteLine("   判断方法：问\"这个值是不是某个堆对象的一部分？\"");
            Console.WriteLine("      是 → 在堆上；不是（独立的局部变量）→ 在栈上");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 4：★ 装箱会复制到堆上 ----");

            SOneInt s = new SOneInt { A = 1 };

            long allocBefore = GC.GetAllocatedBytesForCurrentThread();
            object boxed1 = s;
            object boxed2 = s;
            long allocAfter = GC.GetAllocatedBytesForCurrentThread();

            Console.WriteLine($"   把同一个 struct 装箱两次：");
            Console.WriteLine($"      object boxed1 = s;");
            Console.WriteLine($"      object boxed2 = s;");
            Console.WriteLine($"      ReferenceEquals(boxed1, boxed2) = {ReferenceEquals(boxed1, boxed2)}   ← 两次【独立】装箱");
            Console.WriteLine($"      这两次装箱分配了约 {allocAfter - allocBefore} 字节（每个盒子 24 字节）");
            Console.WriteLine();
            Console.WriteLine("   ★ 装箱 = 把值复制到堆上的一个盒子里");
            Console.WriteLine("   ★ Unity 里这是性能杀手：Update() 每帧装箱 → GC 卡顿");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 5：★ 复制成本对照 ----");

            var sp1 = new STwoInt { A = 1, B = 2 };
            var sp2 = sp1;                       // 复制 8 字节的数据
            sp2.A = 99;
            Console.WriteLine($"   struct：sp1 = ({sp1.A},{sp1.B}), sp2 = ({sp2.A},{sp2.B})");
            Console.WriteLine($"      → 赋值复制了【8 字节的数据】");

            var cp1 = new TwoInt { A = 1, B = 2 };
            var cp2 = cp1;                       // 复制 8 字节的地址
            cp2.A = 99;
            Console.WriteLine($"   class ：cp1 = ({cp1.A},{cp1.B}), cp2 = ({cp2.A},{cp2.B})");
            Console.WriteLine($"      → 赋值复制了【8 字节的地址】，两个变量指向同一个对象");
            Console.WriteLine();
            Console.WriteLine("   ★ 公式：struct 赋值成本 = 字段总大小；class 赋值成本 = 8 字节（地址）");
            Console.WriteLine("   ★ 所以 struct 别太大（微软建议 < 16 字节），否则复制开销超过省下的对象头");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 6：数据住在哪（决策表）----");

            Console.WriteLine("   ┌────────────────────────────────┬──────────────┐");
            Console.WriteLine("   │ 情况                            │ 住在哪        │");
            Console.WriteLine("   ├────────────────────────────────┼──────────────┤");
            Console.WriteLine("   │ 局部变量 int x                  │ 栈            │");
            Console.WriteLine("   │ 局部变量 struct p               │ 栈            │");
            Console.WriteLine("   │ class 的字段（int / struct）     │ 堆（对象内嵌） │");
            Console.WriteLine("   │ 数组的元素                      │ 堆（数组内嵌） │");
            Console.WriteLine("   │ 装箱后的值类型                   │ 堆            │");
            Console.WriteLine("   │ 引用类型对象本体                 │ 堆            │");
            Console.WriteLine("   │ 引用类型变量的【地址】            │ 看变量在哪     │");
            Console.WriteLine("   └────────────────────────────────┴──────────────┘");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 7：为什么数组遍历比链表快 ----");

            const int N = 2_000_000;

            int[] arr = new int[N];
            for (int i = 0; i < N; i++) arr[i] = i;

            var linked = new LinkedList<int>();
            for (int i = 0; i < N; i++) linked.AddLast(i);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            long sum1 = 0;
            for (int i = 0; i < arr.Length; i++) sum1 += arr[i];
            sw.Stop();
            double arrayMs = sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            long sum2 = 0;
            foreach (int v in linked) sum2 += v;
            sw.Stop();
            double listMs = sw.Elapsed.TotalMilliseconds;

            Console.WriteLine($"   遍历 {N:N0} 个元素求和：");
            Console.WriteLine($"      int[]            = {arrayMs,8:F1} ms");
            Console.WriteLine($"      LinkedList<int>  = {listMs,8:F1} ms");
            if (arrayMs > 0.01) Console.WriteLine($"      → 数组快了约 {listMs / arrayMs:F1} 倍");
            Console.WriteLine($"      （校验和 {sum1} / {sum2} 相同，说明结果一致）");
            Console.WriteLine();
            Console.WriteLine("   原因：数组在内存里是【连续】的，CPU 一次缓存一整块；");
            Console.WriteLine("        链表的节点散落在各处，每次跳转都可能\"缓存未命中\"。");
            Console.WriteLine("   ★ 所以即使复杂度都是 O(n)，实际速度可以差好几倍。");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson11Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
