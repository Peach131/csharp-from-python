namespace LessonPractice
{
    class Lesson04Practice
    {
        // ================================================================
        // ↓↓↓ 本课的类定义写在这里（在 Lesson04Practice 类的【外面】）↓↓↓
        // ================================================================




        // ================================================================
        // ↓↓↓ TODO 1~5 的方法写在这一带（在 class 里面，Main 的【上面】）↓↓↓
        // ================================================================




        static void Main()
        {
            Console.WriteLine("===== 第 04 课练习：方法与参数 =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 定义并调用两个方法
            // ================================================================
            // 【要求】在上面定义两个方法：
            //   ① PrintSeparator()          —— 无参数无返回值，打印一行横线
            //   ② PrintTitle(string title)  —— 打印标题
            //   然后调用它们，输出：
            //     ========== 我的第一个方法 ==========
            //     ------------------------------
            //     方法调用成功！
            //     ------------------------------
            // 【语法提示】
            //     static void PrintSeparator()
            //     { Console.WriteLine("------------------------------"); }
            //
            //     static void PrintTitle(string title)
            //     { Console.WriteLine($"========== {title} =========="); }
            // 【考察点】
            //   · 方法必须写在 class 里面，不能写在 Main 里面
            //   · static void 表示"不需要对象、不返回值"

            Console.WriteLine("--- TODO 1 ---");

            // ↓↓↓ 在这里调用你定义的方法 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ 带参数、有返回值的方法
            // ================================================================
            // 【要求】定义两个方法：
            //   ① static double CircleArea(double radius)  返回圆面积
            //   ② static int FindMax(int[] numbers)        返回数组最大值
            //   然后调用：
            //     半径 5 的圆面积：78.54
            //     数组 [3, 7, 2, 9, 4] 的最大值：9
            // 【提示】
            //     static double CircleArea(double radius)
            //     { return 3.14159 * radius * radius; }
            //
            //     static int FindMax(int[] numbers)
            //     {
            //         int max = numbers[0];       // ★ 从第一个元素开始
            //         for (int i = 1; i < numbers.Length; i++)
            //         { if (numbers[i] > max) max = numbers[i]; }
            //         return max;
            //     }
            // 【考察点】返回类型要写对；max 初值为什么不用 0

            Console.WriteLine("--- TODO 2 ---");

            // ↓↓↓ 在这里调用你定义的方法 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ 用元组返回多个值
            // ================================================================
            // 【要求】定义方法：
            //     static (int min, int max) GetMinMax(int[] numbers)
            //   一次返回最小值和最大值
            //   处理 { 15, 3, 27, 9, 42, 8 }，输出：最小值：3，最大值：42
            // 【提示】
            //     static (int min, int max) GetMinMax(int[] numbers)
            //     {
            //         int min = numbers[0];
            //         int max = numbers[0];
            //         foreach (int n in numbers)
            //         {
            //             if (n < min) min = n;
            //             if (n > max) max = n;
            //         }
            //         return (min, max);          // 用圆括号打包
            //     }
            //   调用（挑一种）：
            //     var result = GetMinMax(data);
            //     Console.WriteLine($"最小值：{result.min}，最大值：{result.max}");
            //     // 或直接拆包：var (lo, hi) = GetMinMax(data);
            // 【考察点】元组的返回类型写法，和拆包的写法

            Console.WriteLine("--- TODO 3 ---");

            // ↓↓↓ 在这里调用你定义的方法 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ ★ 重构：把长代码拆成方法（本课核心！）
            // ================================================================
            // 【背景】下面这段代码全部堆在 Main 里，又长又乱。
            //        把它拆成 4 个方法 —— 这是你第一次做【重构】。
            //
            // 【要求】拆成：
            //   ① static int[] ReadScores()            读入 5 个成绩，返回数组
            //   ② static int Sum(int[] numbers)        求和
            //   ③ static double Average(int[] nums)     求平均（注意转 double）
            //   ④ static void PrintReport(int[] scores) 打印报告（内部调用 ②③）
            //   最后 Main 里只留 2-3 行
            //
            // 【原始代码 —— 先读一遍理解它在干什么】
            //     Console.WriteLine("请输入 5 个成绩：");
            //     int[] scores = new int[5];
            //     for (int i = 0; i < 5; i++)
            //     {
            //         Console.Write($"第 {i + 1} 个：");
            //         string? input = Console.ReadLine();
            //         if (!int.TryParse(input, out scores[i]))
            //         {
            //             Console.WriteLine("输入不合法，按 0 处理");
            //             scores[i] = 0;
            //         }
            //     }
            //     int total = 0;
            //     for (int i = 0; i < scores.Length; i++) total += scores[i];
            //     double avg = (double)total / scores.Length;
            //     int max = scores[0];
            //     for (int i = 1; i < scores.Length; i++)
            //         if (scores[i] > max) max = scores[i];
            //     Console.WriteLine("========== 成绩报告 ==========");
            //     for (int i = 0; i < scores.Length; i++)
            //         Console.WriteLine($"  第 {i + 1} 个：{scores[i]}");
            //     Console.WriteLine($"总分：{total}");
            //     Console.WriteLine($"平均分：{avg:F2}");
            //     Console.WriteLine($"最高分：{max}");
            //     Console.WriteLine("==============================");
            //
            // 【提示 —— 一个方法一个方法地写，别一次写完】
            //     第 1 步：先写 ReadScores，其他不动，运行确认能读入
            //     第 2 步：写 Sum 和 Average，在 Main 里验证数字对
            //     第 3 步：写 PrintReport
            //     第 4 步：把 Main 里原来的长代码删掉
            //   ★ 注意 scores[i] 的赋值要写在 if/else 两边分别处理，
            //     不要写在 if 外面 —— 否则失败分支的赋值会被覆盖（虽然值恰好都是 0）
            // 【考察点】
            //   · 方法之间可以互相调用（PrintReport 内部调 Sum 和 Average）
            //   · 一个方法只做一件事

            Console.WriteLine("--- TODO 4 ---");

            // ↓↓↓ 这里最终只应该有 2-3 行 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ 方法重载
            // ================================================================
            // 【要求】定义三个同名方法 Area：
            //   ① static double Area(double side)              正方形面积
            //   ② static double Area(double width, double height) 长方形面积
            //   ③ static double Area(double radius, bool isCircle) 圆面积（isCircle 为 true 时）
            //   调用并输出：
            //     正方形（边长 4）：16.00
            //     长方形（4 × 6）：24.00
            //     圆（半径 4）：50.27
            // 【提示】Console.WriteLine($"正方形（边长 4）：{Area(4.0):F2}");
            //         Console.WriteLine($"长方形（4 × 6）：{Area(4.0, 6.0):F2}");
            //         Console.WriteLine($"圆（半径 4）：{Area(4.0, true):F2}");
            // 【考察点】重载只看参数（个数、类型），不看返回类型
            //          Area(4.0, true) 和 Area(4.0, 6.0) 参数类型不同，所以是合法重载

            Console.WriteLine("--- TODO 5 ---");

            // ↓↓↓ 在这里调用你定义的方法 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 6 ｜ 综合：用方法重写个税计算器（本课考核）
            // ================================================================
            // 【要求】把第 02 课的个税计算器改造成"方法版本"：
            //   ① static double ReadIncome()
            //        读入月收入，非法就返回 -1（用 -1 表示"输入失败"）
            //   ② static double CalculateTax(double income)
            //        根据收入算税额：
            //           <= 5000         → 0
            //           5000 ~ 10000    → (income - 5000) * 0.03
            //           10000 ~ 20000   → (income - 5000) * 0.10
            //           > 20000         → (income - 5000) * 0.20
            //   ③ static void PrintResult(double income, double tax)
            //        打印结果
            //   Main 里只留 4-5 行：
            //       double income = ReadIncome();
            //       if (income < 0) return;
            //       double tax = CalculateTax(income);
            //       PrintResult(income, tax);
            // 【预期运行效果】（输入 15000）
            //     ========== 个税计算结果 ==========
            //     税前收入：15000.00 元
            //     应纳税额：1000.00 元
            //     税后收入：14000.00 元
            //     ==================================
            // 【提示】ReadIncome 里：
            //     Console.Write("请输入你的月收入（元）：");
            //     string? input = Console.ReadLine();
            //     if (!double.TryParse(input, out double income))
            //     {
            //         Console.WriteLine($"输入不合法：{input}");
            //         return -1;                  // ← 用 -1 表示失败
            //     }
            //     return income;
            // 【考察点】★ 重要的思维转变：
            //   方法不能"退出整个程序"，只能"返回一个值"。
            //   所以 ReadIncome 用 -1 当失败标记，让 Main 去判断。

            Console.WriteLine("--- TODO 6 ---");

            // ↓↓↓ 这里最终只应该有 4-5 行 ↓↓↓




            Console.WriteLine();


            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}