// ============================================================================
//  第 11 课 · 练习：值类型与引用类型（深入）
//
//  ★ 本课重点：
//     1. 值类型变量装【数据】，引用类型变量装【地址】
//     2. "值类型在栈上"是错的 —— 类字段里的 struct 在【堆】上
//     3. 对象至少 24 字节（16 字节头 + 填充）
// ============================================================================

using System.Runtime.CompilerServices;

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ 本课要测量的类型写在这里 ↓↓↓
    // ================================================================

    // ★ 为什么类型要写在这里，而不是写在 Main 里面？
    //   C# 的类 / 结构体【不能】定义在方法内部，只能放在 namespace 里、别的 class 的外面。
    //   （Python 里 class 可以写在任何地方，连函数内部都行 —— 这是两门语言的差别之一。）
    //
    // ★★ 下面这些字段是【故意】不赋值、也不读取的：本课要测的就是"什么字段都没装的对象"有多大。
    //     编译器因此会警告 CS0649（字段从未被赋值）/ CS0169（字段从未被使用）/
    //     CS0414（字段被赋了值却没人读）—— 它们都属于"没被用上的字段"这一族。
    //     用 pragma 把这些警告临时关掉：pragma 是【按行生效】的，只覆盖下面这一小段类型定义，
    //     不会影响 Main 里真正的练习代码。
#pragma warning disable CS0649, CS0169, CS0414

    // ──────────────── TODO 1：字段完全一样，只有 struct / class 的区别 ────────────────
    // ★ 这一对是"对照组"：字段一模一样，唯一的差别就是关键字。
    //   struct 的变量里装【数据】，class 的变量里装【地址】。
    //   连 new 的含义都不同：对 struct，new 只是"造一个值"；对 class，new 才真的在堆上分配。
    struct MyStruct
    {
        public int X;
        public int Y;
    }

    class MyClass
    {
        public int X;
        public int Y;
    }


    // ──────────────── TODO 2：验证"任何对象至少 24 字节"的 4 个 class ────────────────
    // ★ 字段个数 0 → 1 → 2 → 3，对应数据 0 → 4 → 8 → 12 字节。
    //   这里故意用【字段】而不是属性：属性会在背后生成隐藏字段和 get/set 方法，把测量搅浑。
    class Empty { }                                              // 0 字节数据
    class OneInt { public int A; }                               // 4 字节数据
    class TwoInt { public int A; public int B; }                 // 8 字节数据
    class ThreeInt { public int A; public int B; public int C; } // 12 字节数据


    // ──────────────── TODO 3：类字段里的 struct 住在【堆】上 ────────────────
    struct Point
    {
        public int X;
        public int Y;
    }

    class Shape
    {
        public int Id;         // 这个 int 在堆上
        public Point Center;   // 这个 struct 也在堆上（它是 Shape 对象的一部分）
    }

    // 对照组：字段和 Point 一模一样，只把 struct 换成 class。
    // ★ 有了它才能回答 TODO 3【要求】2 里的"如果 Point 是 class 的话是多少"。
    class PointAsClass
    {
        public int X;
        public int Y;
    }


    // ──────────────── TODO 4：64 字节的大 struct / 大 class ────────────────
    // ★ 8 个 long × 8 字节 = 64 字节，远远越过"建议 struct 小于 16 字节"那条线。
    struct BigStruct
    {
        public long A, B, C, D, E, F, G, H;
    }

    class BigClass
    {
        public long A, B, C, D, E, F, G, H;
    }

#pragma warning restore CS0649, CS0169, CS0414




    class Lesson11Practice
    {
        // ================================================================
        // ↓↓↓ 测量对象大小的方法 ↓↓↓
        // ================================================================

        // ★ 为什么写成泛型方法 MeasureClass<T>？
        //   本课要测的类型有 6 个（Empty / OneInt / TwoInt / ThreeInt / Shape / PointAsClass），
        //   要是每种类型写一个方法，就是 6 份几乎一样的代码。
        //   泛型把"测哪个类型"变成参数，方法体只写一遍，换类型时一行都不用改。
        // ★ where T : new() 是【泛型约束】：保证 T 有一个无参构造函数，
        //   方法体里才允许写 new T()（否则编译器不知道该怎么凭空造一个 T 出来）。
        //
        // ★ [MethodImpl(MethodImplOptions.NoInlining)] = 禁止 JIT 把这个方法内联到调用方。
        //   测量内存/时间的方法一旦被内联，它的代码就和调用方的代码糊在一起，
        //   量的东西就不干净了。（MethodImpl 来自文件开头的 using System.Runtime.CompilerServices;）
        [MethodImpl(MethodImplOptions.NoInlining)]
        static long MeasureClass<T>(int count = 50000) where T : new()
        {
            // ① 先强制回收两轮，把前面练习产生的垃圾清干净，
            //    这样 before 才是一个干净的起点。
            //    ★ 中间那次 WaitForPendingFinalizers 是等"终结器队列"跑完 ——
            //      有些对象要等终结器执行完才真正被回收，不等它，垃圾就会被算进结果里。
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // ② before / after 读到的都是"当前托管堆里活着的字节数"
            //    GetTotalMemory(true) 的参数 true = 先做一次完整回收再读数
            long before = GC.GetTotalMemory(true);

            // ③ 先造一个数组：一来被测量的对象得有地方"挂住"，不然会被 GC 回收；
            //    二来数组自身的开销待会儿要减掉
            T[] arr = new T[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = new T();   // ← 这一步才是真正被测量的分配（引用类型才会真的上堆）
            }

            long after = GC.GetTotalMemory(true);

            // ④ ★ KeepAlive 写在测量【之后】，是为了让 arr 在那次 GC 时还活着。
            //    JIT 的存活分析是"从后往前"看的：后面还要用 arr，
            //    说明整个测量区间里 arr 都是活的 → 它挂着的 count 个对象一个都不会被回收。
            GC.KeepAlive(arr);

            // ⑤ 减掉数组自身的开销：24 字节头（对象头 8 + MethodTable 指针 8 + 长度 4 + 填充 4）
            //    + 每个元素 8 字节的引用槽（x64 上一个地址就是 8 字节）
            //    ★ 最后这一步用 double 除、再四舍五入：如果直接写 / count（整数除法），
            //      24 会被截断成 23，看起来像"对象只占 23 字节"（课文里特意提醒过这个坑）
            double perObject = (after - before - (24 + count * 8L)) / (double)count;
            return (long)Math.Round(perObject);
        }




        static void Main()
        {
            Console.WriteLine("===== 第 11 课练习：值类型与引用类型（深入）=====\n");

            // ================================================================
            // TODO 1 ｜ 验证"变量里装了什么"
            // ================================================================
            // 【要求】
            //   在上面定义两个类型（字段完全一样，只有 struct/class 的区别）：
            //     struct MyStruct { public int X; public int Y; }
            //     class  MyClass  { public int X; public int Y; }
            //
            //   然后：
            //     1. 各创建一个实例，用 ReferenceEquals 验证地址
            //     2. struct 版本：装箱两次，验证是【两个不同的盒子】
            //     3. class 版本：同一个对象赋给两个变量，验证是【同一个地址】
            //
            // 【预期输出】
            //     === struct 装箱验证 ===
            //     装箱两次是同一个对象吗？False   ← 每次装箱都是新盒子
            //     === class 地址验证 ===
            //     两个变量指向同一个对象吗？True
            //
            // 【提示】
            //     var s = new MyStruct { X = 1, Y = 2 };
            //     object box1 = s;
            //     object box2 = s;
            //     Console.WriteLine($"装箱两次是同一个对象吗？{ReferenceEquals(box1, box2)}");
            //
            //     var c1 = new MyClass { X = 1, Y = 2 };
            //     var c2 = c1;
            //     Console.WriteLine($"两个变量指向同一个对象吗？{ReferenceEquals(c1, c2)}");
            //
            // 【考察点】
            //   · 装箱 = 在堆上新建一个盒子，把值复制进去
            //   · 所以同一个 struct 装箱两次得到两个不同对象

            Console.WriteLine("--- TODO 1 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① struct 版本：变量 s 里装的就是【数据本身】（X、Y 一共 8 个字节）
            //   ★ 这里的 new 用不着堆：对 struct 来说，new 只是"造一个值并把字段清零"，
            //     所以 new MyStruct{...} 自始至终没碰过堆。（"new 就等于上堆"是错的。）
            var s = new MyStruct { X = 1, Y = 2 };

            // ② 装箱：把 s 这 8 个字节【复制】到堆上一个新造的盒子里
            //   ★ object box1 = s; 这行代码编译器会自动插一条 box 指令，
            //     装箱不是什么魔法，它就是"在堆上新建对象 + 复制值"。
            //   ★ 因为复制的是【数据】，所以第 01 课那个实验才有那个结果：
            //     改 s2.X 不会影响 s1.X —— 值类型变量之间/盒子之间，谁也不认识谁。
            object box1 = s;
            object box2 = s;    // 再装一次 → 又造一个新盒子（并不会复用 box1）

            Console.WriteLine("=== struct 装箱验证 ===");
            // ③ ReferenceEquals 比的是变量里那 8 个字节的【地址】，不是内容。
            //   两个盒子是两个不同的堆对象 → 地址不同 → False。
            Console.WriteLine($"装箱两次是同一个对象吗？{ReferenceEquals(box1, box2)}   ← 每次装箱都是新盒子");

            // ④ class 版本：变量 c1 里装的是【地址】，对象本体在堆上
            var c1 = new MyClass { X = 1, Y = 2 };
            //   c2 = c1 只复制了 8 个字节的地址，堆上【还是同一个对象】。
            //   ★ 对比上面 struct 的装箱：那边是"整份数据抄一遍"，这边只是"多了一根指针"。
            //   ★ 这就是"引用共享"：现在写 c2.X = 99，c1.X 也会跟着变成 99，
            //     因为两个变量名指向的是同一个对象 —— 第 01 课那个实验的底层原因就在这里。
            var c2 = c1;

            Console.WriteLine("=== class 地址验证 ===");
            // ⑤ 两个变量装着同一串地址 → ReferenceEquals 返回 True
            Console.WriteLine($"两个变量指向同一个对象吗？{ReferenceEquals(c1, c2)}");




            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ 测量对象大小（验证 24 字节）
            // ================================================================
            // 【要求】
            //   1. 写一个泛型方法 MeasureClass<T>() 测量每个对象占多少字节：
            //        static long MeasureClass<T>(int count = 50000) where T : new()
            //        {
            //            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            //            var before = GC.GetTotalMemory(true);
            //            var arr = new T[count];
            //            for (int i = 0; i < count; i++) arr[i] = new T();
            //            GC.KeepAlive(arr);
            //            var after = GC.GetTotalMemory(true);
            //            // 减去引用数组自身开销：24 字节头 + 8 字节/元素
            //            return (after - before - (24 + count * 8L)) / count;
            //        }
            //   2. 定义 4 个 class 并测量：Empty（无字段）、OneInt、TwoInt、ThreeInt
            //   3. 用 sizeof 对比"数据本身"的大小
            //
            // 【预期输出】（数字可能略有波动，看量级和规律）
            //     Empty（无字段）    → 24 字节
            //     OneInt（4 字节）   → 24 字节   ← 和 Empty 一样大！
            //     TwoInt（8 字节）   → 24 字节
            //     ThreeInt（12 字节）→ 32 字节
            //     sizeof(OneInt 的字段) = 4 字节   ← 数据本身只有 4 字节
            //
            //   ⚠️ 【本处已于 2026-10-06 订正】原来这里把 TwoInt 写成 32 字节，是笔误。
            //      按本课的对齐规则算一遍就知道应该是 24：
            //         TwoInt : 16（对象头 + MethodTable 指针）+ 8（两个 int）= 24
            //                  24 已经是 8 的倍数，【不需要】再补填充 → 24 字节
            //         ThreeInt: 16 + 12 = 28 → 向上对齐到 32   ← 这个才是 32
            //      ★ 所以 TwoInt 和 OneInt 一样是 24，只有 ThreeInt 跨过 24 那条线变成 32。
            //      课文 第11课.md 的实测表、以及 Lesson11Demo\Program.cs 里都写的是 24，
            //      三处里只有这里写错了，已统一。
            //
            // 【提示】
            //     class Empty { }
            //     class OneInt { public int A; }
            //     class TwoInt { public int A; public int B; }
            //     class ThreeInt { public int A; public int B; public int C; }
            //
            //     Console.WriteLine($"Empty（无字段）    → {MeasureClass<Empty>()} 字节");
            //     Console.WriteLine($"OneInt（4 字节）   → {MeasureClass<OneInt>()} 字节   ← 和 Empty 一样大！");
            //
            //   ⚠️ 如果编译报 CS0649 警告（字段从未赋值），可以加 #pragma warning disable CS0649
            //
            // 【考察点】
            //   · 每个对象至少 24 字节 = 8（对象头）+ 8（MethodTable 指针）+ 8（填充）
            //   · OneInt 和 Empty 一样大，因为 4 字节的 int 塞进了填充位
            //   · struct 没有这 16 字节头，所以 struct 更省内存（前提是别装箱）

            Console.WriteLine("--- TODO 2 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 测 4 个 class：字段从 0 个到 3 个
            //   ★ 规律不是"字段之和"，而是：16 字节头 + 字段，再【向上对齐到 8 字节】
            Console.WriteLine($"Empty（无字段）    → {MeasureClass<Empty>()} 字节");
            //   Empty   : 16 + 0  = 16 → .NET 里最小的对象是 24 字节，所以补到 24
            Console.WriteLine($"OneInt（4 字节）   → {MeasureClass<OneInt>()} 字节   ← 和 Empty 一样大！");
            //   OneInt  : 16 + 4  = 20 → 对齐到 24 ← 那个 int 塞进了填充位里，等于"免费"
            Console.WriteLine($"TwoInt（8 字节）   → {MeasureClass<TwoInt>()} 字节");
            //   TwoInt  : 16 + 8  = 24 → 正好 24，不用再补
            //   ★★ 这一行曾经是课件里的一个错例：原来的【预期输出】把它写成了 32 字节。
            //      已在 2026-10-06 订正为 24（见上面 TODO 2 预期输出处的说明）。
            //      ★ 留个记录给以后的你：如果你哪天看到某个数字"和公式对不上"，
            //        正确的动作是【按规则自己算一遍】，而不是先怀疑代码或直接抄数字 ——
            //        这条纪律比这个 24 本身值钱得多。
            Console.WriteLine($"ThreeInt（12 字节）→ {MeasureClass<ThreeInt>()} 字节");
            //   ThreeInt: 16 + 12 = 28 → 向上对齐到 32 ← 零头跨过了 24 这条线，得多花 8 字节

            // ② 再看"数据本身"有多大：sizeof 只算字段，不含对象头
            //   ★ sizeof(int) 是编译期常量，在【安全代码】里就能用；
            //     但 sizeof(OneInt) 那种"算某个类型多大"的写法必须进 unsafe 上下文，
            //     而本项目的 .csproj 没开 <AllowUnsafeBlocks>，所以这里不能那么写。
            Console.WriteLine($"sizeof(OneInt 的字段) = {sizeof(int)} 字节   ← 数据本身只有 4 字节");




            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ 验证"类字段里的 struct 在堆上"
            // ================================================================
            // 【要求】
            //   1. 定义：
            //        struct Point { public int X; public int Y; }
            //        class Shape
            //        {
            //            public int Id;          // 这个 int 在堆上
            //            public Point Center;    // 这个 struct 也在堆上！
            //        }
            //   2. 创建 Shape 对象，用测量方法验证：
            //        · 单独测量 Point（如果它是 class 的话是多少）
            //        · 测量 Shape 的大小，看 Point 是"内嵌"还是"单独一个对象"
            //   3. 用文字总结：怎么判断一个值类型住在栈还是堆
            //
            // 【预期输出】（数字会有波动）
            //     Shape 的大小约 XX 字节
            //     分析：Shape 里有 int(4) + Point(8)，加上 16 字节头 = 28 → 对齐到 32
            //     如果 Point 是单独的堆对象，Shape 会更大（因为要存地址）
            //     ★ 结论：类字段里的 struct 是【内嵌】在对象里的，不占额外对象头
            //
            // 【提示】
            //     struct Point { public int X; public int Y; }
            //     class Shape { public int Id; public Point Center; }
            //
            //     Console.WriteLine($"Shape 的大小 = {MeasureClass<Shape>()} 字节");
            //
            //   参考：
            //     struct 内嵌 → Shape = 16(头) + 4(Id) + 8(Point) = 28 → 对齐 32
            //     若 Point 是 class → Shape = 16(头) + 4(Id) + 4(填充) + 8(地址) = 32，
            //                        再加每个 Point 对象自身的 24 字节
            //
            // 【考察点】
            //   · 判断方法：问"这个值是不是某个堆对象的一部分？"
            //   · struct 内嵌在 class 里时，省掉了自己的对象头 —— 这是它真正的价值

            Console.WriteLine("--- TODO 3 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 先做一个"不用测量也看得懂"的验证：点号能一路穿进对象里，改内嵌的那个 struct
            Shape shape = new Shape { Id = 1 };
            shape.Center.X = 3;
            //   ★ shape.Center.X = 3 能编译通过，说明 Center 的 8 个字节就长在 Shape 对象内部，
            //     它不是一个"另外的对象"（否则中间必须多一次寻址）。
            //   ★ 反例：如果 Center 是【属性】而不是字段，这一行会直接编译报错（CS1612），
            //     因为"读属性"拿到的只是一份副本，改副本没有意义 —— 又一次印证"值类型 = 复制"。

            // ② 真正测一下：Shape 里 int(4) + Point(8)，加上 16 字节头 = 28 → 对齐到 32
            //   ★ 如果 Point 是"单独一个堆对象"，Shape 里就得存一个 8 字节的地址，
            //     而且每个 Point 还要自己付 24 字节的对象头。
            //     这些字节在测量结果里【没有出现】，所以可以断定 Point 是被【内嵌】进去的。
            Console.WriteLine($"Shape 的大小约 {MeasureClass<Shape>()} 字节");
            Console.WriteLine("分析：Shape 里有 int(4) + Point(8)，加上 16 字节头 = 28 → 对齐到 32");
            Console.WriteLine("如果 Point 是单独的堆对象，Shape 会更大（因为要存地址）");
            Console.WriteLine("★ 结论：类字段里的 struct 是【内嵌】在对象里的，不占额外对象头");

            // ③ 对照组：【要求】2 还让"单独测量 Point"。
            //   把 Point 的两个 int 原样搬进一个 class 再测，就得到"如果 Point 是 class"的代价：
            //   数据还是那 8 字节，但因为要当独立对象，每个都得先付 16 字节的头 → 24 字节。
            //   ★ 下面这行是额外对照，不在上面的【预期输出】清单里。
            Console.WriteLine($"（额外对照）Point 若改成 class：每个 {MeasureClass<PointAsClass>()} 字节");

            // ④ 文字总结：怎么判断一个值类型住在栈还是堆？
            //   只问一句话："这个值是不是某个堆对象的一部分？"
            //     是（类字段、数组元素、装箱后的盒子）→ 堆
            //     不是（它自己就是一个独立的局部变量）→ 栈
            //   ★ 所以"值类型在栈上"这句话是错的：位置由【它在哪】决定，不由【它是什么类型】决定。
            //     TODO 5 会把这条规则用到 11 个场景上。




            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ 复制成本对照
            // ================================================================
            // 【要求】
            //   1. 定义一个大 struct（字段加起来约 64 字节）和大 class（字段一样）
            //   2. 用 Stopwatch 测量：赋值 100 万次，各花多长时间
            //   3. 得出结论
            //
            // 【预期输出】（数字会不同，看倍数关系）
            //     大 struct 赋值 100 万次：XXX ms
            //     大 class  赋值 100 万次：XXX ms
            //     → struct 慢了约 X 倍（因为要复制 64 字节而不是 8 字节）
            //
            // 【提示】
            //     struct BigStruct
            //     {
            //         public long A, B, C, D, E, F, G, H;    // 8 × 8 = 64 字节
            //     }
            //
            //     class BigClass
            //     {
            //         public long A, B, C, D, E, F, G, H;
            //     }
            //
            //     var sw = System.Diagnostics.Stopwatch.StartNew();
            //     var src = new BigStruct();
            //     for (int i = 0; i < 1_000_000; i++) { var copy = src; }
            //     sw.Stop();
            //
            //   ⚠️ 编译器可能把没用的赋值优化掉。可以用 GC.KeepAlive(copy) 防止。
            //
            // 【考察点】
            //   · struct 复制的成本 = 字段总大小
            //   · 这就是微软建议"struct 小于 16 字节"的原因
            //   · 大型 struct 不但不省内存，反而更慢

            Console.WriteLine("--- TODO 4 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            BigStruct srcStruct = new BigStruct();
            BigClass srcClass = new BigClass();
            long sink = 0;   // ★ 累加器：把复制出来的值"用掉"，JIT 才不敢把复制删掉

            // ① 热身：分层 JIT 下，方法头几次跑的是"编译快、跑得慢"的 tier-0 版本。
            //    不热身的话，先测的那一段（struct）会白多算这笔开销，倍数就不准了。
            for (int i = 0; i < 100_000; i++)
            {
                var warm = srcStruct;
                sink += warm.A + warm.B + warm.C + warm.D + warm.E + warm.F + warm.G + warm.H;
            }
            for (int i = 0; i < 100_000; i++)
            {
                var warm = srcClass;
                sink += warm.A + warm.B + warm.C + warm.D + warm.E + warm.F + warm.G + warm.H;
            }

            // ② struct 版本：每轮 var copy = srcStruct 都要把 8 个 long 一共 64 字节【整块复制】一遍
            var swStruct = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 1_000_000; i++)
            {
                var copy = srcStruct;   // ← 复制 64 字节
                // ★ 下面这一行有两个作用：
                //   1) 让复制的结果真被用到，JIT 就不会把"没人用的赋值"整段删掉；
                //   2) 顺便避开课文提示里的 GC.KeepAlive(copy)：KeepAlive 的参数类型是 object，
                //      传一个 struct 进去会触发【装箱】—— 100 万次装箱 = 100 万个堆上的盒子，
                //      那样测到的是"装箱成本"，不是"复制成本"，反而把结论带偏了。
                sink += copy.A + copy.B + copy.C + copy.D + copy.E + copy.F + copy.G + copy.H;
            }
            swStruct.Stop();

            // ③ class 版本：copy 只是 8 字节的地址复制，堆上那个对象一个字节都没动
            var swClass = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 1_000_000; i++)
            {
                var copy = srcClass;    // ← 只复制 8 字节地址（同一个对象，只是多了个名字）
                sink += copy.A + copy.B + copy.C + copy.D + copy.E + copy.F + copy.G + copy.H;
            }
            swClass.Stop();

            // ④ 报数：F1 = 保留一位小数；TotalMilliseconds 是 double，比 ElapsedMilliseconds 精细
            double structMs = swStruct.Elapsed.TotalMilliseconds;
            double classMs = swClass.Elapsed.TotalMilliseconds;
            Console.WriteLine($"大 struct 赋值 100 万次：{structMs:F1} ms");
            Console.WriteLine($"大 class  赋值 100 万次：{classMs:F1} ms");

            // ★ Math.Max(classMs, 0.001) 只为防除零：classMs 理论上可能被测成 0.0
            double ratio = structMs / Math.Max(classMs, 0.001);
            Console.WriteLine($"→ struct 慢了约 {ratio:F1} 倍（因为要复制 64 字节而不是 8 字节）");
            // ★ 万一两边测出来几乎一样快，别急着怀疑结论 —— 那是 JIT 把这段复制优化掉了。
            //   写法稍微改一点（比如把复制结果传给一个不内联的方法）结果就会变。
            //   基准测试本身是门手艺：这就是为什么"跑得快不快"永远要实测，不能靠推理。

            // ⑤ 收尾时再用一次 sink：如果累加结果"没人看"，
            //    JIT 可能连累加带复制一起删掉，上面测的就成了空循环。
            //   （KeepAlive 的参数是 object，long 会装箱一次 —— 只装这 1 次，无所谓；
            //     但千万别把它放进循环里。）
            GC.KeepAlive(sink);




            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ 综合：判断每个值住在哪（本课考核）
            // ================================================================
            // 【要求】不用写代码运行，用【注释】回答下面 8 个问题，
            //        每个都写出"在栈上"还是"在堆上"，以及一句话理由。
            //
            //   场景代码：
            //     struct Point { public int X, Y; }
            //     class Player
            //     {
            //         public int Hp;
            //         public Point Position;
            //         public string Name = "";
            //     }
            //
            //     void Test()
            //     {
            //         int a = 10;                        // ① a 这个 int
            //         Point p = new Point();             // ② p 这个 struct
            //         var player = new Player();         // ③ player 这个变量
            //         // ④ player 指向的那个 Player 对象
            //         // ⑤ player.Hp 这个 int
            //         // ⑥ player.Position 这个 struct
            //         // ⑦ player.Name 里存的地址
            //         // ⑧ player.Name 指向的那个字符串
            //         Point[] arr = new Point[5];        // ⑨ 数组本身
            //         // ⑩ 数组里的 5 个 Point
            //         object boxed = p;                  // ⑪ 装箱后的 p
            //     }
            //
            // 【预期答案格式】（写在你自己的代码注释里）
            //     ① a       → 栈   （独立的局部变量）
            //     ② p       → 栈   （独立的局部变量）
            //     ③ player  → 栈   （变量本身是局部变量，装的是地址）
            //     ④ Player对象 → 堆
            //     ... 剩下你自己写
            //
            // 【考察点】
            //   ★ 唯一的判断标准：问"这个值是不是某个堆对象的一部分？"
            //     是 → 堆；不是（自己就是变量）→ 栈
            //   ★ 记住：变量本身的位置 和 变量指向的对象的位置 是两回事

            Console.WriteLine("--- TODO 5 ---");
            // ↓↓↓ 用注释写出你的答案 ↓↓↓

            // 判断口诀（课文原话）：问自己「这个值是不是某个堆对象的一部分？」
            //   是 → 堆；不是（它本身就是个独立的局部变量）→ 栈
            // ★ 注意题目列了 11 个小标号（①~⑪），比【要求】里写的"8 个"多，下面全部答一遍。
            //
            // ① a 这个 int                 → 栈   （它是独立的局部变量，变量里直接装着 10 这 4 个字节）
            // ② p 这个 struct              → 栈   （整个 8 字节都躺在栈帧里）
            //      ★ 这里最容易错：new Point() 的 new【不会】在堆上分配任何东西，
            //        对 struct 而言 new 只是"把 8 个字节清零"。new ≠ 上堆。
            // ③ player 这个变量            → 栈   （局部变量本身在栈上，里面装的是 8 字节的地址）
            // ④ player 指向的那个 Player 对象 → 堆（new 出来的对象本体住在托管堆）
            // ⑤ player.Hp 这个 int         → 堆   （它是 Player 对象的字段，是那个堆对象的一部分）
            // ⑥ player.Position 这个 struct → 堆  （它【内嵌】在 Player 对象里，同样是对象的一部分）
            //      ★ "值类型在栈上"之所以是错的，⑥ 就是反例：
            //        位置不取决于"是不是值类型"，只取决于"它在不在某个堆对象里"。
            // ⑦ player.Name 里存的地址      → 堆   （Name 是对象的字段，字段的位置跟着对象走）
            // ⑧ player.Name 指向的那个字符串 → 堆  （string 是引用类型，本体在堆上；
            //      连 "" 这种字面量也放在堆上的"字符串驻留池"里，不在栈上）
            // ⑨ Point[] arr 这个数组本身    → 堆   （数组是引用类型，本体在堆上；只有 arr 变量在栈）
            // ⑩ 数组里的 5 个 Point        → 堆   （数组元素【内嵌】在数组对象内部，是数组的一部分）
            //      ★ 好处：5 个 Point 一共只花 40 字节，没有各自的 16 字节对象头
            // ⑪ boxed 装箱后的那个 p        → 堆   （装箱 = 在堆上新造一个盒子，把 p 的 8 字节复制进去；
            //      boxed 变量本身在栈，它装着盒子的地址；这个盒子占 16(头) + 8 = 24 字节）
            //
            // ★ 一句话总结：栈 / 堆的区别【不属于类型】，属于【位置】。
            //   同一个 int：写成局部变量在栈上，写成类字段就在堆上；
            //   同一个 Point：当局部变量在栈上，当类字段/数组元素就在堆上。
            //   变量本身在哪 和 变量指向的对象在哪，是两件独立的事。




            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
