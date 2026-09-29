// ============================================================================
//  第 04 课 · 示例代码
//  方法与参数 —— 每一行都有注释，请逐行读完再运行
//
//  ★ 注意这个文件的结构：所有方法都写在 class Program 里面！
//    这是 C# 和 Python 最大的结构区别（Python 可以在文件里随便定义函数）
// ============================================================================

namespace Lesson04Demo
{
    class Program
    {
        // ====================================================================
        // 一、不带参数、不返回值的方法
        // ====================================================================

        // void 表示"不返回任何值"
        static void PrintSeparator()
        {
            Console.WriteLine("--------------------------------");
        }

        // 表达式体写法：方法体只有一条语句时可以简写
        // => 读作 "goes to"，意思就是"这个方法的返回值/行为是……"
        static void PrintTitle(string title) => Console.WriteLine($"===== {title} =====");


        // ====================================================================
        // 二、有参数、有返回值的方法
        // ====================================================================

        static int Add(int a, int b)
        {
            return a + b;                  // return 把结果交回给调用者
        }

        static double Average(int[] numbers)
        {
            int total = 0;
            for (int i = 0; i < numbers.Length; i++)
            {
                total += numbers[i];
            }
            return (double)total / numbers.Length;      // 注意转 double！
        }

        static bool IsEven(int n) => n % 2 == 0;         // 表达式体写法


        // ====================================================================
        // 三、参数传递演示用的方法
        // ====================================================================

        // 值类型参数：改的是【副本】，不影响外面
        static void TryChangeInt(int x)
        {
            x = 999;
        }

        // ref 参数：真引用，能改外部变量
        static void ChangeByRef(ref int x)
        {
            x = 999;
        }

        // 数组参数：传的是地址副本，但指向同一个数组
        // 所以改【数组元素】会影响外面
        static void FillWithZero(int[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = 0;
            }
        }

        // 字符串参数：虽然 string 是引用类型，但赋值只改副本的指向
        // 而且字符串不可变，所以外面完全不受影响
        static void TryChangeString(string s)
        {
            s = "方法内改过的值";
        }


        // ====================================================================
        // 四、out 参数：额外返回一个值
        // ====================================================================

        // 一个方法只能 return 一个值。
        // 但要同时告诉调用者"成功没"和"结果是多少"，就 return bool + out 结果
        static bool TryHalf(int input, out int result)
        {
            if (input % 2 != 0)
            {
                result = 0;                // ★ out 参数在返回前【必须】赋值
                return false;              // 失败
            }
            result = input / 2;
            return true;                   // 成功
        }


        // ====================================================================
        // 五、元组：返回多个值（比 out 更现代）
        // ====================================================================

        // 返回类型写成 (类型 名字, 类型 名字)
        static (int min, int max) GetMinMax(int[] numbers)
        {
            int min = numbers[0];
            int max = numbers[0];
            foreach (int n in numbers)
            {
                if (n < min) min = n;
                if (n > max) max = n;
            }
            return (min, max);             // 用圆括号打包返回
        }


        // ====================================================================
        // 六、方法重载：同名，不同参数
        // ====================================================================

        static int Add(int a, int b, int c) => a + b + c;             // 3 个 int
        static double Add(double a, double b) => a + b;               // 2 个 double
        static string Add(string a, string b) => a + b;               // 2 个 string
        // 注意：上面已经有一个 Add(int, int)，这里是重载而不是重复定义


        // ====================================================================
        // 七、可选参数和命名参数
        // ====================================================================

        // greeting 有默认值，所以调用时可以省略
        static string Greet(string name, string greeting = "你好")
        {
            return $"{greeting}，{name}！";
        }


        // ====================================================================
        //  程序入口
        // ====================================================================
        static void Main()
        {
            PrintTitle("第 04 课：方法与参数");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 1：定义和调用方法 ----");

            PrintSeparator();                          // 调用不带参数的方法
            int sum = Add(3, 5);                       // 调用有返回值的方法
            Console.WriteLine($"Add(3, 5) = {sum}");

            // 有返回值的方法可以直接用在表达式里
            Console.WriteLine($"Add(10, 20) + 5 = {Add(10, 20) + 5}");

            PrintSeparator();
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 2：方法的好处（不用重复代码）----");

            // 如果没有 Average 方法，每个班都要写一遍循环
            int[] classA = { 88, 92, 76 };
            int[] classB = { 70, 85, 90, 66 };
            int[] classC = { 100, 95 };

            Console.WriteLine($"   A 班平均分：{Average(classA):F2}");
            Console.WriteLine($"   B 班平均分：{Average(classB):F2}");
            Console.WriteLine($"   C 班平均分：{Average(classC):F2}");
            Console.WriteLine("   ★ 同一个方法，处理三个长度不同的数组 —— 这就是方法的价值");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 3：★ 值类型参数是副本 ----");

            int n = 5;
            Console.WriteLine($"   调用前：n = {n}");
            TryChangeInt(n);
            Console.WriteLine($"   调用后：n = {n}       ← 没变！方法里改的是副本");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 4：★ ref 才是真引用 ----");

            int m = 5;
            Console.WriteLine($"   调用前：m = {m}");
            ChangeByRef(ref m);                        // ★ 调用时也要写 ref
            Console.WriteLine($"   调用后：m = {m}       ← 变了！ref 让方法能改外部变量");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 5：★ 数组参数（改内容会影响外面）----");

            int[] nums = { 1, 2, 3 };
            Console.WriteLine($"   调用前：nums = [{string.Join(", ", nums)}]");
            FillWithZero(nums);
            Console.WriteLine($"   调用后：nums = [{string.Join(", ", nums)}]       ← 变了！");
            Console.WriteLine("   原因：arr 和 nums 是两份【地址副本】，但指向同一个数组。");
            Console.WriteLine("        改 arr[i] 就是改堆上那个数组的内容。");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 6：★ string 参数（反直觉！）----");

            string text = "原始值";
            Console.WriteLine($"   调用前：text = {text}");
            TryChangeString(text);
            Console.WriteLine($"   调用后：text = {text}       ← 没变！");
            Console.WriteLine("   为什么？两个原因叠加：");
            Console.WriteLine("     ① 字符串【不可变】，s = \"...\" 不是改内容，而是让 s 指向新对象");
            Console.WriteLine("     ② 那只改了【副本的指向】，外面的 text 还指着原来那个");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 7：out 参数 ----");

            if (TryHalf(10, out int half))
                Console.WriteLine($"   TryHalf(10) 成功，一半是 {half}");
            else
                Console.WriteLine("   TryHalf(10) 失败");

            if (TryHalf(7, out int half2))
                Console.WriteLine($"   TryHalf(7) 成功，一半是 {half2}");
            else
                Console.WriteLine($"   TryHalf(7) 失败（奇数），out 参数的值是 {half2}");
            Console.WriteLine("   ★ 这就是 double.TryParse 的原理：return 成功与否，out 带出结果");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 8：元组返回多个值 ----");

            int[] data = { 5, 2, 9, 1, 7 };
            Console.WriteLine($"   数据：[{string.Join(", ", data)}]");

            // 方式 1：拿到整个元组，用 .名字 访问
            var result = GetMinMax(data);
            Console.WriteLine($"   方式1：最小 {result.min}，最大 {result.max}");

            // 方式 2：直接拆包成两个变量
            var (lo, hi) = GetMinMax(data);
            Console.WriteLine($"   方式2：最小 {lo}，最大 {hi}");

            // 方式 3：不要的用 _ 丢弃
            var (onlyMin, _) = GetMinMax(data);
            Console.WriteLine($"   方式3：只要最小 {onlyMin}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 9：方法重载（同名不同参数）----");

            Console.WriteLine($"   Add(1, 2)         = {Add(1, 2)}          ← 2 个 int");
            Console.WriteLine($"   Add(1.5, 2.5)     = {Add(1.5, 2.5)}        ← 2 个 double");
            Console.WriteLine($"   Add(\"a\", \"b\")     = {Add("a", "b")}          ← 2 个 string");
            Console.WriteLine($"   Add(1, 2, 3)      = {Add(1, 2, 3)}          ← 3 个 int");
            Console.WriteLine("   ★ 四个方法同名，编译器根据【参数】自动选对的那个");
            Console.WriteLine("   ★ Console.WriteLine 本身就有 18 个重载");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 10：可选参数和命名参数 ----");

            Console.WriteLine($"   Greet(\"小明\")                 = {Greet("小明")}");
            Console.WriteLine($"   Greet(\"小明\", \"早上好\")       = {Greet("小明", "早上好")}");
            Console.WriteLine();

            // 命名参数：写参数名，顺序可以打乱，代码更自解释
            Console.WriteLine($"   命名参数：Greet(greeting: \"晚安\", name: \"小红\")");
            Console.WriteLine($"             = {Greet(greeting: "晚安", name: "小红")}");
            Console.WriteLine("   ★ 参数多的时候特别有用，一眼看出每个值是什么意思");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 11：作用域 ----");

            int outer = 10;
            Console.WriteLine($"   外层变量 outer = {outer}");

            if (outer > 5)
            {
                int inner = 20;
                Console.WriteLine($"   if 里面能看见 outer = {outer}   ← 内层能看外层");
                Console.WriteLine($"   if 里面能看见 inner = {inner}");
            }

            // Console.WriteLine(inner);   // ❌ 编译错误：inner 出了 { } 就消失了
            Console.WriteLine("   if 外面的 inner 已经消失（取消注释那行会编译错误）");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson04Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
