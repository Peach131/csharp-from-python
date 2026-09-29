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




    class Lesson11Practice
    {
        // ================================================================
        // ↓↓↓ 测量对象大小的方法 ↓↓↓
        // ================================================================




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
            //     TwoInt（8 字节）   → 32 字节
            //     ThreeInt（12 字节）→ 32 字节
            //     sizeof(OneInt 的字段) = 4 字节   ← 数据本身只有 4 字节
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




            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
