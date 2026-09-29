namespace Experiments;

/// <summary>
/// 第 2 周 · 实验 3：值语义 vs 引用语义  ★核心
/// </summary>
/// <remarks>
/// 结论：
///   1. struct（值类型）：赋值 = 复制全部数据。改副本不影响原对象。
///   2. class（引用类型）：赋值 = 复制"地址"。两个变量指向同一个对象。
///   3. "值类型在栈上、引用类型在堆上"这句话是【不完整】的：
///      局部变量的 struct 在栈上，但类字段里的 struct 在堆上（作为对象的一部分）。
///   4. Python 里所有东西都是引用语义，所以你会在第 1 条上反复踩坑。
/// </remarks>
public static class E03_ValueVsReference
{
    // ========================================================================
    // ↓↓↓ 【打破它】练习就是改这一行 ↓↓↓
    //
    //   把下面的 struct 改成 class，然后再跑一次实验 3，观察【实验 A】的变化。
    //
    //   ⚠️ 注意：改完之后你会遇到【一个编译错误】和【一个警告】，那都是正常的，
    //      而且它们本身就是最好的学习材料：
    //        · 错误：实验 D 里 SPoint? 的写法不再有效（class 没有 HasValue）
    //        · 警告 CS8618：实验 B 的 StructHolder.Point 字段可能为 null
    //      看不懂就把光标放上去按 Alt+Enter，让 Rider 解释。
    //      【搞清楚为什么会报，比跑通更有价值。】
    //
    //   ⚠️ 做完记得改回 struct（Ctrl+Z）。
    // ========================================================================
    private struct SPoint { public int X; public int Y; }

    private class CPoint
    {
        public int X;
        public int Y;
        public override string ToString() => $"({X}, {Y})";
    }

    // struct 作为类的字段 —— 注意它在堆上！
    private class StructHolder
    {
        public SPoint Point;          // ← 这个 struct 住在堆上，是 StructHolder 的一部分
#pragma warning disable CS0649 // 这个字段只为演示布局存在，不需要赋值
        public int Number;            // ← 这个 int 也在堆上
#pragma warning restore CS0649
    }

    public static void Run()
    {
        Console.WriteLine("【实验 A：赋值语义】");
        Console.WriteLine();

        var s1 = new SPoint { X = 1, Y = 1 };
        var s2 = s1;                    // 复制整个 struct（16 字节全复制）
        s2.X = 99;
        Console.WriteLine($"  struct: s1=({s1.X},{s1.Y})  s2=({s2.X},{s2.Y})   ← s1 没被影响");
        Console.WriteLine("          赋值时复制了数据本身（值语义）");
        Console.WriteLine();

        var c1 = new CPoint { X = 1, Y = 1 };
        var c2 = c1;                    // 只复制引用（8 字节指针）
        c2.X = 99;
        Console.WriteLine($"  class : c1={c1}  c2={c2}           ← c1 也被改了");
        Console.WriteLine("          赋值时只复制了地址，两个变量指向同一个对象（引用语义）");
        Console.WriteLine();

        Console.WriteLine("  用 ReferenceEquals 验证：");
        Console.WriteLine($"    ReferenceEquals(c1, c2) = {ReferenceEquals(c1, c2)}   ← 同一个对象");
        object boxedS1 = s1;
        object boxedS2 = s1;
        Console.WriteLine($"    ReferenceEquals(s1, s2) = {ReferenceEquals(boxedS1, boxedS2)}  ← 装箱后是不同对象");
        Console.WriteLine("      （直接写 ReferenceEquals(s1, s2) 会被编译器警告：");
        Console.WriteLine("        值类型传进去会各自装箱一次，比较的必然是不同对象 —— 见 CA2013）");
        Console.WriteLine();

        Console.WriteLine("【实验 B：struct 作为类的字段（在堆上！）】");
        Console.WriteLine();

        var holder = new StructHolder { Point = new SPoint { X = 5, Y = 5 } };
        var copyOfHolder = holder;      // 复制引用，两个变量指向同一个 holder 对象
        copyOfHolder.Point.X = 100;     // 通过一个变量改，另一个也变
        Console.WriteLine($"  holder.Point.X = {holder.Point.X}   ← 变成了 100");
        Console.WriteLine("  原因：holder 和 copyOfHolder 指向同一个堆对象，");
        Console.WriteLine("        那个堆对象里【内嵌】着一个 struct。");
        Console.WriteLine();

        // ------------------------------------------------------------------
        // ★ 默认值差异：struct / class 最容易被忽略的一条区别
        //
        //   default(T) 对两种类型的含义完全不同：
        //     T 是 struct → 各字段为 0 / false（一个真实存在的值，永远不为 null）
        //     T 是 class  → null（不初始化就不能碰，碰了抛 NullReferenceException）
        //
        //   ⚠️ 这里【故意】不写 "如果 x 是 null" 这种判断，因为 `值类型 is null`
        //      本身就是编译错误 CS0037（值类型不可能为 null）。
        //      这个"编译器不让你写"的事实，比任何文字说明都有说服力。
        // ------------------------------------------------------------------
        var holder2 = new StructHolder();
        SPoint defaultPoint = default;                 // struct → (0,0)    class → null
        Console.WriteLine("  default(SPoint) 得到的变量 —— 它的内容取决于 SPoint 是什么：");
        Console.WriteLine("    · SPoint 是 struct → 内容是 (0,0)，你可以安全地读它的成员");
        Console.WriteLine("    · SPoint 是 class  → 内容是 null，读它的成员会【崩溃】");
        Console.WriteLine();
        Console.WriteLine("  ★ 这就是 C# 引入「可空引用类型」（string? / MyClass?）的原因：");
        Console.WriteLine("      值类型不存在 null，编译器不用操心；");
        Console.WriteLine("      引用类型默认为 null，所以编译器会【警告】你可能忘了初始化（CS8618）。");
        Console.WriteLine("      你改类型时看到的那个警告，正是在预言这个隐患。");
        Console.WriteLine();
        GC.KeepAlive(defaultPoint);

        Console.WriteLine("【实验 C：数组索引器 vs List 索引器】");
        Console.WriteLine();

        // ------------------------------------------------------------------
        // 用 int 演示索引器的区别 —— int 永远是值类型，所以这里不受你切换
        // SPoint 类型的影响，两种配置下都能编译运行。
        // ------------------------------------------------------------------
        var intArray = new int[1];
        intArray[0] = 42;               // ✅ 数组索引器返回「可寻址的引用」

        var intList = new List<int> { 1 };
        intList[0] = 42;                // ✅ List<int> 的索引器也能赋值...

        Console.WriteLine($"  int[]      : intArray[0] = {intArray[0]}");
        Console.WriteLine($"  List<int>  : intList[0]  = {intList[0]}");
        Console.WriteLine();

        // ...那为什么 List<SPoint> 就不能写 list[0].X = 42 呢？
        var list = new List<SPoint> { new SPoint { X = 1 } };
        // list[0].X = 42;              // ❌ SPoint 是 struct 时：编译错误 CS1612！
        //
        //   ★ 真正的原因不是「List 的索引器不能赋值」，而是：
        //       List<int>[0] = 42  是【给索引器赋值】（调用 set 访问器）→ 合法
        //       list[0].X   = 42   是【读取索引器返回的值，再改它的字段】
        //                           → 当 T 是 struct 时，读到的是副本，改副本没意义，编译器禁止
        //                           → 当 T 是 class  时，读到的是引用，改的是同一个对象，合法
        //
        //   你现在把 SPoint 改成了 class，所以这一行其实【可以】取消注释并编译通过 ——
        //   自己试一下，这是最直观的验证。

        var temp = list[0];             // 取出 → 改 → 放回（struct 时的标准做法）
        temp.X = 42;
        list[0] = temp;
        Console.WriteLine($"  List<SPoint>: list[0].X = {list[0].X}");
        Console.WriteLine("  ★ SPoint 是 struct 时：必须「取出-改-放回」，否则编译不过");
        Console.WriteLine("  ★ SPoint 是 class 时 ：可以直接 list[0].X = 42（但改的是共享对象！）");
        Console.WriteLine("    同一行代码能不能编译，取决于 T 是值类型还是引用类型。");
        Console.WriteLine();
        temp.X = 42;
        list[0] = temp;
        Console.WriteLine($"  List<T>  : list[0].X = {list[0].X}   ← struct 时必须取出-改-放回");
        Console.WriteLine("  这是 BCL 的历史不一致：数组索引器返回「可寻址的引用」，");
        Console.WriteLine("  List 索引器返回「值」（class 时这个值是个地址，所以又变得能改了）。");
        Console.WriteLine();

        Console.WriteLine("【实验 D：只有值类型才有 Nullable<T>，引用类型本来就能是 null】");
        Console.WriteLine();

        // ⚠️ 这个实验【故意】不依赖 SPoint 是 struct 还是 class，
        //    因为你在「打破它」练习里会把它改成 class —— 那样 SPoint? 的含义就变了。
        //    这本身就是一个重要发现，见下面的说明。
        int? nullableInt = null;                   // int 是值类型 → 类型是 Nullable<int>
        Console.WriteLine($"  int?  x = null;  x.HasValue = {nullableInt.HasValue}   ← 有 HasValue 属性");
        nullableInt = 42;
        Console.WriteLine($"  int?  x = 42;    x.HasValue = {nullableInt.HasValue}, x.Value = {nullableInt.Value}");

        string? nullableString = null;             // string 是引用类型 → 类型就是 string
        Console.WriteLine($"  string? s = null;  s is null = {nullableString is null}   ← 没有 HasValue！");
        nullableString = "hi";
        Console.WriteLine($"  string? s = \"hi\";  s.Length = {nullableString.Length}");

        Console.WriteLine();
        Console.WriteLine("  ★ 关键认知：`T?` 这个写法对【值类型】和【引用类型】含义完全不同！");
        Console.WriteLine("      - 值是【值类型】(int, double, struct...)  → T? = Nullable<T>，是真 struct，有 HasValue/Value");
        Console.WriteLine("      - 值是【引用类型】(string, class...)      → T? = 只是『可能为 null』的编译期标记，");
        Console.WriteLine("                                                   运行时零开销，【没有】 HasValue");
        Console.WriteLine();
        Console.WriteLine("  ★ 所以：把 SPoint 从 struct 改成 class 之后，`SPoint?` 就没有 HasValue 了。");
        Console.WriteLine("    你改完之后遇到的报错就是这个原因 —— 不是拼写错误，是【语义变了】。");
        Console.WriteLine("    编译器能在你打字的时候就把这种问题抓出来，这正是 C# 相对 Python 的核心优势。");
        Console.WriteLine();

        Console.WriteLine("【动手任务】");
        Console.WriteLine("  1. 给 SPoint 加一个方法 void Move(int dx) { X += dx; }");
        Console.WriteLine("     然后试试 list[0].Move(1); —— 会编译错误，想想为什么");
        Console.WriteLine("  2. 把 SPoint 改成 100 个字段的大 struct，用 Stopwatch");
        Console.WriteLine("     测量 100 万次赋值 vs CPoint 的 100 万次赋值，看差距");
        Console.WriteLine("  3. 记住公式：struct 赋值成本 = 字段总大小；class 赋值成本 = 8 字节");
    }
}
