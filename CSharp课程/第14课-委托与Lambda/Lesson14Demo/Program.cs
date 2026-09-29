// ============================================================================
//  第 14 课 · 示例代码
//  委托与 Lambda
// ============================================================================

namespace Lesson14Demo
{
    class Button
    {
        public event Action? Clicked;               // 事件：外面只能 += / -=

        public void Click()
        {
            Clicked?.Invoke();                      // ?. 防止没人订阅时崩溃
        }
    }

    class Program
    {
        // ====================================================================
        // 一、不用委托：为每种条件写一个方法（笨办法）
        // ====================================================================
        static List<int> FilterEven(List<int> nums)
        {
            var result = new List<int>();
            foreach (int n in nums) if (n % 2 == 0) result.Add(n);
            return result;
        }

        // ====================================================================
        // 二、用委托：一个方法处理所有条件（好办法）
        // ====================================================================
        static List<int> Filter(List<int> nums, Func<int, bool> condition)
        {
            var result = new List<int>();
            foreach (int n in nums)
            {
                if (condition(n))                       // ★ 调用传进来的方法
                {
                    result.Add(n);
                }
            }
            return result;
        }

        // 命名方法（可以赋给 Func 变量）
        static bool IsPositive(int n) => n > 0;

        static void Main()
        {
            Console.WriteLine("========== 第 14 课：委托与 Lambda ==========\n");


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 1：委托是什么 ----");

            Func<int, bool> isEven = n => n % 2 == 0;    // 委托变量"装着"一个方法
            Console.WriteLine($"   Func<int, bool> isEven = n => n % 2 == 0;");
            Console.WriteLine($"      isEven(4) = {isEven(4)}");
            Console.WriteLine($"      isEven(7) = {isEven(7)}");
            Console.WriteLine();
            Console.WriteLine("   ★ 委托 = 一个\"能装方法\"的变量（普通变量装数据，它装方法）");
            Console.WriteLine("   ★ Func<int, bool>：接收 int，返回 bool（最后一格是返回类型）");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 2：★ 委托的威力（条件可替换）----");

            var nums = new List<int> { 5, 12, 8, -3, 20, 7, -10, 0 };

            Console.WriteLine($"   数据：[{string.Join(", ", nums)}]");
            Console.WriteLine();
            Console.WriteLine($"   用笨办法 FilterEven   → [{string.Join(", ", FilterEven(nums))}]");
            Console.WriteLine();
            Console.WriteLine("   用委托 Filter（同一个方法，不同条件）：");
            Console.WriteLine($"      偶数      n => n % 2 == 0    → [{string.Join(", ", Filter(nums, n => n % 2 == 0))}]");
            Console.WriteLine($"      大于 10   n => n > 10        → [{string.Join(", ", Filter(nums, n => n > 10))}]");
            Console.WriteLine($"      正数      IsPositive（命名方法）→ [{string.Join(", ", Filter(nums, IsPositive))}]");
            Console.WriteLine($"      个位数    n => n > 0 && n < 10 → [{string.Join(", ", Filter(nums, n => n > 0 && n < 10))}]");
            Console.WriteLine();
            Console.WriteLine("   ★ 加新条件只需写一个表达式，Filter 一行都不用改");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 3：三种内置委托 ----");

            // Action：有参数、无返回值
            Action<string> log = msg => Console.WriteLine($"      [日志] {msg}");
            log("这行是用 Action 打印的");

            // Func：有返回值
            Func<int, int, int> add = (a, b) => a + b;
            Console.WriteLine($"      Func<int,int,int> add = (a,b) => a+b;  add(3,5) = {add(3, 5)}");

            // Predicate：返回 bool（判断用）
            Predicate<int> isBig = n => n > 100;
            Console.WriteLine($"      Predicate<int> isBig = n => n > 100;  isBig(150) = {isBig(150)}");
            Console.WriteLine();

            Console.WriteLine("   对照表：");
            Console.WriteLine("      Action            → 无参数、无返回值");
            Console.WriteLine("      Action<T>         → 有参数、无返回值");
            Console.WriteLine("      Func<T, TResult>  → 有参数、有返回值（最后一格是返回类型）");
            Console.WriteLine("      Predicate<T>      → 一个参数、返回 bool");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 4：Lambda 的四种写法 ----");

            Func<int, int> square = x => x * x;                          // 单参数单表达式
            Console.WriteLine($"      ① x => x * x            square(6) = {square(6)}");

            Func<int, int, int> multiply = (a, b) => a * b;              // 多参数
            Console.WriteLine($"      ② (a,b) => a * b        multiply(4,7) = {multiply(4, 7)}");

            Func<int, string> describe = n =>                            // 多语句要 { } + return
            {
                if (n > 0) return "正数";
                if (n < 0) return "负数";
                return "零";
            };
            Console.WriteLine($"      ③ 多语句 Lambda          describe(-5) = {describe(-5)}");

            Action hello = () => Console.WriteLine("      ④ 无参数 Lambda 执行了");
            hello();
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 5：★ 闭包会堆分配 ----");

            long Alloc() => GC.GetAllocatedBytesForCurrentThread();

            int threshold = 10;

            // 捕获外部变量 → 每次创建都要分配一个"装变量的类"
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            long before = Alloc();
            for (int i = 0; i < 1000; i++)
            {
                Func<int, bool> captured = n => n > threshold;       // ★ 捕获了 threshold
                GC.KeepAlive(captured);
            }
            long allocCaptured = Alloc() - before;

            // 不捕获任何变量 → C# 9+ 缓存成静态单例
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            before = Alloc();
            for (int i = 0; i < 1000; i++)
            {
                Func<int, bool> noCapture = n => n > 10;             // 没有捕获
                GC.KeepAlive(noCapture);
            }
            long allocNoCapture = Alloc() - before;

            Console.WriteLine($"   创建 1000 次 Lambda：");
            Console.WriteLine($"      捕获外部变量（闭包）  : 分配 {allocCaptured,7} 字节");
            Console.WriteLine($"      不捕获任何变量        : 分配 {allocNoCapture,7} 字节   ← 几乎为 0");
            Console.WriteLine();
            Console.WriteLine("   ★ 闭包 = 编译器生成一个类来装被捕获的变量 → 每次创建都堆分配");
            Console.WriteLine("   ★ Unity 里这条是致命的：Update() 每帧创建闭包 = 每帧产生垃圾");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 6：★ 循环变量捕获的经典陷阱 ----");

            var actions = new List<Action>();

            // foreach：每次迭代是新变量 → 安全
            foreach (var i in Enumerable.Range(0, 3))
                actions.Add(() => Console.Write($"{i} "));
            Console.Write("   foreach 捕获：");
            foreach (var a in actions) a();
            Console.WriteLine("  ← 0 1 2 ✅");

            // for：循环变量只有一个 → 全部捕获同一个
            actions.Clear();
            for (int i = 0; i < 3; i++)
                actions.Add(() => Console.Write($"{i} "));
            Console.Write("   for 捕获    ：");
            foreach (var a in actions) a();
            Console.WriteLine("  ← 3 3 3 ❌ 全变成 3 了");

            // 修复：循环体内创建局部副本
            actions.Clear();
            for (int i = 0; i < 3; i++)
            {
                int copy = i;                                  // ★ 每次迭代一个新变量
                actions.Add(() => Console.Write($"{copy} "));
            }
            Console.Write("   for + 副本 ：");
            foreach (var a in actions) a();
            Console.WriteLine("  ← 0 1 2 ✅ 修复成功");
            Console.WriteLine();
            Console.WriteLine("   原因：for 的 i 是【同一个变量】，三个 Lambda 捕获的是同一个");
            Console.WriteLine("        Python 里也有同样的坑：[lambda: i for i in range(3)] 全部返回 2");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 7：event 事件 ----");

            var btn = new Button();
            btn.Clicked += () => Console.WriteLine("      处理点击 1");
            btn.Clicked += () => Console.WriteLine("      处理点击 2");

            Console.WriteLine("   订阅了两个处理器，现在触发 Click()：");
            btn.Click();

            // btn.Clicked = null;          // ❌ 编译错误：事件外面不能直接赋值
            Console.WriteLine();
            Console.WriteLine("   ★ event 让外面只能 += / -=，不能覆盖或直接触发 —— 这是封装");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson14Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
