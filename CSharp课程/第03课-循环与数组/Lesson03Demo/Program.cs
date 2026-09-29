// ============================================================================
//  第 03 课 · 示例代码
//  循环与数组 —— 每一行都有注释，请逐行读完再运行
// ============================================================================

namespace Lesson03Demo
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("========== 第 03 课：循环与数组 ==========");
            Console.WriteLine();

            // ================================================================
            // 演示 1：for 循环的三段结构
            // ================================================================
            Console.WriteLine("---- 演示 1：for 循环 ----");

            // for (初始化 ; 循环条件 ; 每次循环后执行)
            //       ①            ②            ③
            Console.Write("   for (int i = 0; i < 5; i++) 输出：");
            for (int i = 0; i < 5; i++)
            {
                Console.Write($"{i} ");
            }
            Console.WriteLine();
            Console.WriteLine("   注意：i 是 0,1,2,3,4 —— 到 5 就停了，所以【没有 5】");
            Console.WriteLine();

            // 倒着数
            Console.Write("   倒着数 for (int i = 5; i > 0; i--)：");
            for (int i = 5; i > 0; i--)
            {
                Console.Write($"{i} ");
            }
            Console.WriteLine();
            Console.WriteLine();

            // 步长不是 1
            Console.Write("   步长 2：for (int i = 1; i <= 9; i += 2)：");
            for (int i = 1; i <= 9; i += 2)
            {
                Console.Write($"{i} ");
            }
            Console.WriteLine();
            Console.WriteLine("   Python 对照：range(1, 10, 2)");
            Console.WriteLine();


            // ================================================================
            // 演示 2：累加求和（循环最典型的用途）
            // ================================================================
            Console.WriteLine("---- 演示 2：累加求和 ----");

            // 计算 1 + 2 + 3 + ... + 100
            int sum = 0;                      // ★ 累加器必须先初始化为 0
            for (int i = 1; i <= 100; i++)
            {
                sum += i;                     // 等价 sum = sum + i
            }
            Console.WriteLine($"   1 + 2 + ... + 100 = {sum}");
            Console.WriteLine("   ★ 关键：累加器 sum 必须在循环【外面】初始化，否则每次都被清零");
            Console.WriteLine();

            // 求平均值（注意整数除法的坑）
            int[] scores = { 88, 92, 76, 95, 81 };
            int total = 0;
            for (int i = 0; i < scores.Length; i++)
            {
                total += scores[i];
            }
            Console.WriteLine($"   总分 = {total}");
            Console.WriteLine($"   平均分（错误写法） = {total / scores.Length}      ← 整数除法，小数被砍");
            Console.WriteLine($"   平均分（正确写法） = {(double)total / scores.Length:F2}   ← 先转 double");
            Console.WriteLine();


            // ================================================================
            // 演示 3：while 循环
            // ================================================================
            Console.WriteLine("---- 演示 3：while 循环 ----");

            int w = 0;
            Console.Write("   while (w < 5)：");
            while (w < 5)
            {
                Console.Write($"{w} ");
                w++;                          // ★ 必须改变条件，否则死循环
            }
            Console.WriteLine();
            Console.WriteLine();

            // while 的典型用途：条件满足就一直做
            Console.WriteLine("   模拟：从 1000 开始每次减半，直到小于 100");
            int value = 1000;
            int steps = 0;
            while (value >= 100)
            {
                Console.Write($"   {value}");
                value /= 2;
                steps++;
            }
            Console.WriteLine($" → 最终 {value}（用了 {steps} 步）");
            Console.WriteLine();


            // ================================================================
            // 演示 4：do-while（至少执行一次）
            // ================================================================
            Console.WriteLine("---- 演示 4：while vs do-while ----");

            int a = 10;
            Console.Write("   while (10 < 5) 的结果：");
            while (a < 5)
            {
                Console.Write("执行了 ");
                a++;
            }
            Console.WriteLine("（一次都没执行，因为条件一开始就是 false）");

            int b = 10;
            Console.Write("   do-while (10 < 5) 的结果：");
            do
            {
                Console.Write("执行了 ");
                b++;
            } while (b < 5);                  // ★ 注意结尾的分号！
            Console.WriteLine("（执行了 1 次，因为它先执行后判断）");
            Console.WriteLine();

            // 真实用途：至少问一次
            Console.WriteLine("   do-while 的真实用途：菜单（至少显示一次）");
            int menuCount = 0;
            string? choice;
            do
            {
                menuCount++;
                Console.WriteLine($"     [第 {menuCount} 次显示菜单]");
                choice = "n";                 // 模拟用户输入 n（演示用，所以直接赋值）
            } while (choice == "y");
            Console.WriteLine("     用户选择了 n，循环结束");
            Console.WriteLine();


            // ================================================================
            // 演示 5：foreach（遍历最简洁）
            // ================================================================
            Console.WriteLine("---- 演示 5：foreach ----");

            int[] nums = { 10, 20, 30, 40 };

            Console.Write("   foreach (int n in nums)：");
            foreach (int n in nums)
            {
                Console.Write($"{n} ");
            }
            Console.WriteLine();
            Console.WriteLine("   和 Python 的 for n in nums 几乎一样！");
            Console.WriteLine();

            // ★ foreach 里【不能】修改元素（取消注释会编译错误）
            // foreach (int n in nums)
            // {
            //     n = 99;            // ❌ 无法为"n"赋值，因为它是 foreach 迭代变量
            // }

            // 想修改必须用 for + 索引
            Console.Write("   用 for 把每个元素乘 10：");
            for (int i = 0; i < nums.Length; i++)
            {
                nums[i] *= 10;                // ✅ 通过索引修改
            }
            Console.WriteLine(string.Join(", ", nums));
            Console.WriteLine();


            // ================================================================
            // 演示 6：break 和 continue
            // ================================================================
            Console.WriteLine("---- 演示 6：break vs continue ----");

            Console.Write("   break：遇到 3 就整个停止   → ");
            for (int i = 1; i <= 6; i++)
            {
                if (i == 3) break;
                Console.Write($"{i} ");
            }
            Console.WriteLine();

            Console.Write("   continue：跳过 3，继续后面 → ");
            for (int i = 1; i <= 6; i++)
            {
                if (i == 3) continue;
                Console.Write($"{i} ");
            }
            Console.WriteLine();
            Console.WriteLine();

            // break 的实用场景：找到就停，不白跑
            int[] data = { 5, 12, 8, 99, 3 };
            int target = 8;
            int foundIndex = -1;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == target)
                {
                    foundIndex = i;
                    break;
                }
            }
            Console.WriteLine($"   在 [5,12,8,99,3] 里找 {target}：索引 {foundIndex}");
            Console.WriteLine();


            // ================================================================
            // 演示 7：数组的声明方式和 Length
            // ================================================================
            Console.WriteLine("---- 演示 7：数组 ----");

            int[] arr1 = { 1, 2, 3 };                  // 直接给初值（最常用）
            int[] arr2 = new int[5];                   // 指定长度，自动填 0
            string[] arr3 = new string[3];             // 字符串数组，自动填 null

            Console.WriteLine($"   int[] arr1 = {{ 1, 2, 3 }};        arr1.Length = {arr1.Length}");
            Console.WriteLine($"   int[] arr2 = new int[5];          arr2.Length = {arr2.Length}");
            Console.WriteLine($"   arr2 的内容：[{string.Join(", ", arr2)}]  ← 默认全是 0");
            Console.WriteLine($"   string[3] 的元素默认是 null（不是空字符串）");
            Console.WriteLine();

            // ★ Length 还是 Count？
            var list = new List<int> { 1, 2, 3 };
            Console.WriteLine($"   数组：arr1.Length = {arr1.Length}      ← 数组用 Length");
            Console.WriteLine($"   字符串：\"hello\".Length = {"hello".Length}      ← 字符串也用 Length");
            Console.WriteLine($"   List：list.Count = {list.Count}          ← List 用 Count");
            Console.WriteLine();

            // 索引和 ^ 运算符
            int[] arr4 = { 100, 200, 300, 400 };
            Console.WriteLine($"   arr4 = [{string.Join(", ", arr4)}]");
            Console.WriteLine($"   arr4[0]        = {arr4[0]}       第一个");
            Console.WriteLine($"   arr4[^1]       = {arr4[^1]}      最后一个（C# 8+ 语法，等价 arr4[Length-1]）");
            Console.WriteLine($"   arr4[^2]       = {arr4[^2]}      倒数第二个");
            Console.WriteLine();

            // ★ 数组越界会崩溃
            Console.WriteLine("   数组越界的后果：");
            try
            {
                int bad = arr4[10];               // 只有 4 个元素，访问第 10 个
                Console.WriteLine($"    {bad}");
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine($"    抛异常：{ex.GetType().Name}");
                Console.WriteLine($"    消息：{ex.Message}");
            }
            Console.WriteLine();


            // ================================================================
            // 演示 8：数组常用操作
            // ================================================================
            Console.WriteLine("---- 演示 8：数组常用操作 ----");

            int[] sortable = { 5, 2, 8, 1, 9, 3 };
            Console.WriteLine($"   原始      ：[{string.Join(", ", sortable)}]");

            Array.Sort(sortable);
            Console.WriteLine($"   Sort 之后 ：[{string.Join(", ", sortable)}]   ← 原地排序（改变了原数组）");

            Array.Reverse(sortable);
            Console.WriteLine($"   Reverse 后：[{string.Join(", ", sortable)}]");

            int idx = Array.IndexOf(sortable, 5);
            Console.WriteLine($"   IndexOf(5)  = {idx}");
            Console.WriteLine($"   IndexOf(999) = {Array.IndexOf(sortable, 999)}   ← 找不到返回 -1");
            Console.WriteLine();


            // ================================================================
            // 演示 9：嵌套循环
            // ================================================================
            Console.WriteLine("---- 演示 9：嵌套循环 ----");

            Console.WriteLine("   打印三角形：");
            for (int i = 1; i <= 4; i++)          // 外层控制行
            {
                Console.Write("     ");
                for (int j = 1; j <= i; j++)      // 内层控制这一行打印几个
                {
                    Console.Write("*");
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            Console.WriteLine("   九九乘法表（只打印一半）：");
            for (int i = 1; i <= 9; i++)
            {
                Console.Write("     ");
                for (int j = 1; j <= i; j++)
                {
                    Console.Write($"{j}×{i}={i * j,2}  ");   // ,2 表示占 2 格，用于对齐
                }
                Console.WriteLine();
            }
            Console.WriteLine();


            // ================================================================
            // 演示 10：二维数组
            // ================================================================
            Console.WriteLine("---- 演示 10：二维数组 ----");

            // int[,] 方括号里是【逗号】，不是两个方括号
            int[,] grid = new int[2, 3]
            {
                { 1, 2, 3 },        // 第 0 行
                { 4, 5, 6 }         // 第 1 行
            };

            Console.WriteLine($"   int[,] grid = new int[2, 3]  （2 行 3 列）");
            Console.WriteLine($"   grid.GetLength(0) = {grid.GetLength(0)}   ← 行数");
            Console.WriteLine($"   grid.GetLength(1) = {grid.GetLength(1)}   ← 列数");
            Console.WriteLine($"   grid.Length       = {grid.Length}   ← 总元素数（注意不是行数！）");
            Console.WriteLine($"   grid[1, 2]        = {grid[1, 2]}   ← 第1行第2列");
            Console.WriteLine();
            Console.WriteLine("   遍历：");
            for (int row = 0; row < grid.GetLength(0); row++)
            {
                Console.Write("     ");
                for (int col = 0; col < grid.GetLength(1); col++)
                {
                    Console.Write($"{grid[row, col]}  ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();


            // ================================================================
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson03Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
