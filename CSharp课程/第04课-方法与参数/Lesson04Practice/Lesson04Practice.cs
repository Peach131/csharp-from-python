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

        // ────────────────────────────────────────────────────────────────
        // TODO 1 ｜ 无参数无返回值 + 有参数无返回值
        // ────────────────────────────────────────────────────────────────
        // `static`：不需要创建对象就能调用（Main 也是 static，所以只能直接调 static 方法）
        // `void`  ：不返回任何值，所以方法体里不需要（也不能）写 return 值
        static void PrintSeparator()
        {
            Console.WriteLine("------------------------------");
        }

        static void PrintTitle(string title)
        {
            // title 是【形参】：调用时传进来的"实参"会被复制给它
            Console.WriteLine($"========== {title} ==========");
        }


        // ────────────────────────────────────────────────────────────────
        // TODO 2 ｜ 带参数 + 有返回值
        // ────────────────────────────────────────────────────────────────
        static double CircleArea(double radius)
        {
            // 返回类型写 double，所以 return 出去的必须是 double
            // （radius * radius 是 double，3.14159 * ... 也是 double，类型天然匹配）
            return 3.14159 * radius * radius;
        }

        static int FindMax(int[] numbers)
        {
            // ★ 初值取 numbers[0]，不能取 0。
            //   如果数组全是负数（比如 {-5, -3, -8}），用 0 当初值会返回 0 ——
            //   一个根本不在数组里的"最大值"。
            int max = numbers[0];

            // 从索引 1 开始：索引 0 已经被当作初值了，再比一次是浪费
            for (int i = 1; i < numbers.Length; i++)
            {
                if (numbers[i] > max)
                {
                    max = numbers[i];
                }
            }

            return max;
        }


        // ────────────────────────────────────────────────────────────────
        // TODO 3 ｜ 用元组返回多个值
        // ────────────────────────────────────────────────────────────────
        // 返回类型写成 (int min, int max) —— 这是【具名元组】，两个值各带名字
        // （C# 方法只能 return 一个东西，元组是"把两个值打包成一个"的官方做法）
        static (int min, int max) GetMinMax(int[] numbers)
        {
            int min = numbers[0];
            int max = numbers[0];

            // 这里用 foreach 更简洁：不需要索引，只需要每个元素的值
            foreach (int n in numbers)
            {
                if (n < min) { min = n; }
                if (n > max) { max = n; }
            }

            // 用圆括号打包返回。顺序必须和声明的 (int min, int max) 一致
            return (min, max);
        }


        // ────────────────────────────────────────────────────────────────
        // TODO 4 ｜ 重构：一个方法只做一件事
        // ────────────────────────────────────────────────────────────────

        // ① 读入 5 个成绩，返回数组
        static int[] ReadScores()
        {
            Console.WriteLine("请输入 5 个成绩：");

            int[] scores = new int[5];              // 数组长度固定为 5

            for (int i = 0; i < scores.Length; i++)
            {
                Console.Write($"第 {i + 1} 个：");
                string? input = Console.ReadLine();

                // ★ 关键：TryParse 直接往 scores[i] 里写，失败才补 0
                if (!int.TryParse(input, out scores[i]))
                {
                    Console.WriteLine("输入不合法，按 0 处理");
                    scores[i] = 0;
                }
            }

            return scores;                          // 把整个数组交回去
        }

        // ② 求和
        static int Sum(int[] numbers)
        {
            int total = 0;                          // 累加器初值 0
            for (int i = 0; i < numbers.Length; i++)
            {
                total += numbers[i];
            }
            return total;
        }

        // ③ 求平均（★ 内部调用 Sum —— 方法可以调用别的方法）
        static double Average(int[] nums)
        {
            // 为什么不在这里重新写一遍求和循环？
            // 因为"求和"这件事已经由 Sum 负责了。重复写会导致以后改逻辑要改两处。
            //
            // ★★ 类型转换的两个要点：
            //   1. Sum(nums) 返回 int，要【先转 double 再除】，否则整数除法会砍掉小数
            //   2. 所以强制转换必须加在 Sum(nums) 这个【整体】前面，写成
            //      (double)Sum(nums) / nums.Length
            //      如果写成 (double)(Sum(nums) / nums.Length) 就是先砍再转，白转
            return (double)Sum(nums) / nums.Length;
        }

        // ④ 打印报告（★ 内部调用 ② 和 ③）
        static void PrintReport(int[] scores)
        {
            // 只有 void 方法可以"不 return"；想提前结束可以用裸 return;
            int total = Sum(scores);                    // 调用 ②
            double avg = Average(scores);               // 调用 ③
            int max = FindMax(scores);                  // 复用 TODO 2 写好的方法

            Console.WriteLine("========== 成绩报告 ==========");

            for (int i = 0; i < scores.Length; i++)
            {
                // 字符串插值里参数宽度用【逗号】：{i + 1,2} 表示占 2 个字符、右对齐
                // （如果是负数表示左对齐，如 {i + 1,-2}）
                Console.WriteLine($"  第 {i + 1,2} 个：{scores[i]}");
            }

            Console.WriteLine($"总分：{total}");
            Console.WriteLine($"平均分：{avg:F2}");
            Console.WriteLine($"最高分：{max}");
            Console.WriteLine("==============================");
        }


        // ────────────────────────────────────────────────────────────────
        // TODO 5 ｜ 方法重载：同名，参数不同
        // ────────────────────────────────────────────────────────────────
        // ★ 重载的判定只看【参数列表】（个数 / 类型 / 顺序），
        //   和返回类型无关。下面三个都叫 Area，但参数各不相同，合法。
        static double Area(double side)                         // 1 个参数 → 正方形
        {
            return side * side;
        }

        static double Area(double width, double height)         // 2 个 double → 长方形
        {
            return width * height;
        }

        // ★ 注意：这个和上面那个都是"2 个参数"，但第二个是 bool 不是 double，
        //   所以类型不同 → 仍然是合法重载。调用时靠实参类型决定用哪个。
        static double Area(double radius, bool isCircle)        // (double, bool) → 圆
        {
            if (isCircle)
            {
                return 3.14159 * radius * radius;
            }
            // 非圆的情况下返回 0（本练习用不到这个分支，但方法必须有返回值）
            return 0;
        }


        // ────────────────────────────────────────────────────────────────
        // TODO 6 ｜ 方法化改造：个税计算器
        // ────────────────────────────────────────────────────────────────

        // ① 读入月收入。★ 失败时返回 -1 作为"约定好的失败标记"
        static double ReadIncome()
        {
            Console.Write("请输入你的月收入（元）：");
            string? input = Console.ReadLine();

            if (!double.TryParse(input, out double income))
            {
                Console.WriteLine($"输入不合法：{input}");
                // ★★ 本课最重要的思维转变：
                //   方法【不能】退出整个程序（return 只能结束自己这一层）。
                //   所以这里不能像第 02 课那样直接 return; 了事 ——
                //   必须返回一个"调用方看得懂"的值，让 Main 去决定要不要退出。
                return -1;
            }

            return income;
        }

        // ② 算税额
        static double CalculateTax(double income)
        {
            // ★ 收入为负数（ReadIncome 的失败标记）时直接返回 0，
            //   让本方法在"任何输入"下都安全 —— 这叫防御性写法
            if (income < 0)
            {
                return 0;
            }

            if (income <= 5000)
            {
                return 0;
            }
            else if (income <= 10000)
            {
                return (income - 5000) * 0.03;
            }
            else if (income <= 20000)
            {
                return (income - 5000) * 0.10;
            }
            else
            {
                // ★ 最后一个分支用 else 而不是 else if (income > 20000)：
                //   前面的分支已经把 <= 20000 全排除了，走到这里必然 > 20000
                return (income - 5000) * 0.20;
            }
        }

        // ③ 打印结果（参数是"算好的数据"，本方法只负责显示）
        static void PrintResult(double income, double tax)
        {
            Console.WriteLine("========== 个税计算结果 ==========");
            Console.WriteLine($"税前收入：{income:F2} 元");
            Console.WriteLine($"应纳税额：{tax:F2} 元");
            Console.WriteLine($"税后收入：{income - tax:F2} 元");
            Console.WriteLine("==================================");
        }




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
            PrintTitle("我的第一个方法");
            PrintSeparator();
            Console.WriteLine("方法调用成功！");
            PrintSeparator();

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

            // 有返回值的方法，调用结果一般要【接住】再使用
            double area = CircleArea(5);
            Console.WriteLine($"半径 5 的圆面积：{area:F2}");

            // 也可以直接嵌在插值里调用：{CircleArea(5):F2}
            // ★ 注意 5 是 int，CircleArea 要 double —— C# 会自动把 int 隐式转成 double（安全方向）
            int[] data = { 3, 7, 2, 9, 4 };
            int maxValue = FindMax(data);
            Console.WriteLine($"数组 [3, 7, 2, 9, 4] 的最大值：{maxValue}");

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

            int[] data2 = { 15, 3, 27, 9, 42, 8 };

            // 写法一：接住整个元组，再用 .名字 取字段
            var result = GetMinMax(data2);
            Console.WriteLine($"最小值：{result.min}，最大值：{result.max}");

            // 写法二（推荐）：直接拆包成两个变量，之后用起来和普通变量一样
            var (lo, hi) = GetMinMax(data2);
            Console.WriteLine($"拆包写法：最小值 {lo}，最大值 {hi}");

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

            // ★ 重构的成果：原来 20 多行的逻辑，现在两行说清楚"读入 → 打印报告"
            //   细节全在 ReadScores 和 PrintReport 里面，Main 只负责"流程"
            int[] scores = ReadScores();
            PrintReport(scores);

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

            // ★ 三个调用长得几乎一样，区别只在【实参的个数和类型】——
            //   编译器就是靠这个来决定调用哪一个 Area 的，这叫"重载解析"
            Console.WriteLine($"正方形（边长 4）：{Area(4.0):F2}");        // → 1 个 double
            Console.WriteLine($"长方形（4 × 6）：{Area(4.0, 6.0):F2}");    // → 2 个 double
            Console.WriteLine($"圆（半径 4）：{Area(4.0, true):F2}");      // → (double, bool)

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

            // ★ 这 4 行就是本课的终点：Main 不再关心"怎么算税"，只关心"流程"
            double income = ReadIncome();
            if (income < 0)
            {
                return;                 // ★ 这里 return 是合法的：Main 是 void，直接结束程序
            }
            double tax = CalculateTax(income);
            PrintResult(income, tax);

            Console.WriteLine();


            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}