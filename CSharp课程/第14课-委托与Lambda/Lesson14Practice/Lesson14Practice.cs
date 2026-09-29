// ============================================================================
//  第 14 课 · 练习：委托与 Lambda
//
//  ★ 本课重点：
//     1. 委托 = 能装方法的变量；Func<T,TResult> 最后一格是返回类型
//     2. Lambda：x => x * 2
//     3. 闭包会堆分配；for 循环捕获变量要用局部副本
// ============================================================================

namespace LessonPractice
{
    class Lesson14Practice
    {
        // ================================================================
        // ↓↓↓ TODO 2、5 用的方法写在这里 ↓↓↓
        // ================================================================




        static void Main()
        {
            Console.WriteLine("===== 第 14 课练习：委托与 Lambda =====\n");

            // ================================================================
            // TODO 1 ｜ 定义委托变量并调用
            // ================================================================
            // 【要求】
            //   1. 声明一个 Func<int, int> 叫 square，功能是求平方
            //   2. 声明一个 Func<int, int, int> 叫 maxOf，功能是返回较大值
            //   3. 声明一个 Action<string> 叫 print，功能是打印 "[信息] " + 内容
            //   4. 声明一个 Predicate<int> 叫 isAdult，判断是否 >= 18
            //   5. 分别调用它们并打印结果
            //
            // 【预期输出】
            //     square(7)      = 49
            //     maxOf(3, 9)    = 9
            //     [信息] 你好
            //     isAdult(20)    = True
            //     isAdult(15)    = False
            //
            // 【提示】
            //     Func<int, int> square = x => x * x;
            //     Func<int, int, int> maxOf = (a, b) => a > b ? a : b;
            //     Action<string> print = msg => Console.WriteLine($"[信息] {msg}");
            //     Predicate<int> isAdult = age => age >= 18;
            //
            //     Console.WriteLine($"square(7)      = {square(7)}");
            //     print("你好");
            //
            // 【考察点】四种内置委托的写法和区别

            Console.WriteLine("--- TODO 1 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ ★ 用委托做通用筛选（本课核心）
            // ================================================================
            // 【要求】
            //   1. 写一个通用方法：
            //        static List<int> Filter(List<int> nums, Func<int, bool> condition)
            //      把满足 condition 的元素放进新 List 返回
            //   2. 写一个通用方法：
            //        static List<int> Transform(List<int> nums, Func<int, int> mapper)
            //      把每个元素用 mapper 转换后放进新 List 返回
            //   3. 用同一个 Filter 处理 4 种不同条件
            //   4. 用 Transform 做 3 种不同转换
            //
            // 【数据】var nums = new List<int> { 5, 12, 8, -3, 20, 7, -10, 0 };
            //
            // 【预期输出】
            //     数据：[5, 12, 8, -3, 20, 7, -10, 0]
            //     ---------- 筛选 ----------
            //     偶数      ：[12, 8, 20, -10, 0]
            //     大于 10   ：[12, 20]
            //     正数      ：[5, 12, 8, 20, 7]
            //     绝对值小于 10：[5, 8, -3, 7, 0]
            //     ---------- 转换 ----------
            //     平方      ：[25, 144, 64, 9, 400, 49, 100, 0]
            //     取绝对值  ：[5, 12, 8, 3, 20, 7, 10, 0]
            //     加 100    ：[105, 112, 108, 97, 120, 107, 90, 100]
            //
            // 【提示】
            //     static List<int> Filter(List<int> nums, Func<int, bool> condition)
            //     {
            //         var result = new List<int>();
            //         foreach (int n in nums)
            //         {
            //             if (condition(n)) result.Add(n);
            //         }
            //         return result;
            //     }
            //
            //     static List<int> Transform(List<int> nums, Func<int, int> mapper)
            //     {
            //         var result = new List<int>();
            //         foreach (int n in nums)
            //         {
            //             result.Add(mapper(n));          // ★ 调用传进来的转换方法
            //         }
            //         return result;
            //     }
            //
            //     // 调用
            //     Console.WriteLine($"偶数      ：[{string.Join(", ", Filter(nums, n => n % 2 == 0))}]");
            //     Console.WriteLine($"平方      ：[{string.Join(", ", Transform(nums, n => n * n))}]");
            //     Console.WriteLine($"取绝对值  ：[{string.Join(", ", Transform(nums, n => Math.Abs(n)))}]");
            //
            // 【考察点】
            //   · 这就是"把行为当参数传"的威力：Filter 只写一次，条件随便换
            //   · 你其实在实现 LINQ 的 Where 和 Select（第 16 课会看到官方的版本）

            Console.WriteLine("--- TODO 2 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ 观察闭包的分配
            // ================================================================
            // 【要求】
            //   1. 用 GC.GetAllocatedBytesForCurrentThread() 测量：
            //        · 创建 1000 次"捕获外部变量"的 Lambda，分配了多少字节
            //        · 创建 1000 次"不捕获任何变量"的 Lambda，分配了多少字节
            //   2. 打印对比数字，并写一句注释结论
            //
            // 【预期输出】（数字会不同）
            //     闭包（捕获变量）  ：分配 XXXXX 字节
            //     不捕获           ：分配     0 字节
            //     → 闭包每次创建都要堆分配
            //
            // 【提示】
            //     long Alloc() => GC.GetAllocatedBytesForCurrentThread();
            //
            //     int threshold = 10;
            //
            //     GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            //     long before = Alloc();
            //     for (int i = 0; i < 1000; i++)
            //     {
            //         Func<int, bool> cap = n => n > threshold;    // ★ 捕获 threshold
            //         GC.KeepAlive(cap);                            // 防止被优化掉
            //     }
            //     long allocCap = Alloc() - before;
            //
            //     // 不捕获的版本同理（n => n > 10）
            //     Console.WriteLine($"闭包（捕获变量）  ：分配 {allocCap} 字节");
            //
            // 【考察点】
            //   · 闭包 = 编译器生成一个类装被捕获的变量
            //   · 所以"捕获变量"和"不捕获"在性能上差别很大
            //   · Unity 热路径（Update）里绝对不要创建闭包

            Console.WriteLine("--- TODO 3 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ 循环变量捕获陷阱
            // ================================================================
            // 【要求】
            //   1. 用 for 循环创建 3 个 Action，每个打印自己的序号
            //      （先写【错误】版本，观察输出是 3 3 3）
            //   2. 再用"局部副本"的写法修复，输出 0 1 2
            //   3. 用 foreach 也试一遍，观察它是安全的
            //
            // 【预期输出】
            //     ❌ for 直接捕获 ：3 3 3
            //     ✅ for + 局部副本：0 1 2
            //     ✅ foreach      ：0 1 2
            //
            // 【提示】
            //     var actions = new List<Action>();
            //
            //     // ❌ 错误版本
            //     for (int i = 0; i < 3; i++)
            //         actions.Add(() => Console.Write($"{i} "));
            //     Console.Write("❌ for 直接捕获 ：");
            //     foreach (var a in actions) a();
            //     Console.WriteLine();
            //
            //     // ✅ 修复版本
            //     actions.Clear();
            //     for (int i = 0; i < 3; i++)
            //     {
            //         int copy = i;                       // ★ 关键
            //         actions.Add(() => Console.Write($"{copy} "));
            //     }
            //
            //     // ✅ foreach 版本
            //     actions.Clear();
            //     foreach (var i in Enumerable.Range(0, 3))
            //         actions.Add(() => Console.Write($"{i} "));
            //
            // 【考察点】
            //   · for 的循环变量只有一个，所有 Lambda 捕获的是同一个
            //   · Lambda 捕获的是"变量"而不是"值的快照"
            //   · 修复方法：在循环体内创建局部副本

            Console.WriteLine("--- TODO 4 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ 综合：可配置的数据处理器（本课考核）
            // ================================================================
            // 【要求】写一个方法：
            //     static void ProcessData(List<int> data,
            //                             Func<int, bool> filter,
            //                             Func<int, int> transform,
            //                             Action<int> output)
            //   处理流程：
            //     1. 用 filter 筛选出需要的元素
            //     2. 用 transform 转换每个元素
            //     3. 用 output 输出每个结果
            //
            //   然后调用它两次，传不同的 Lambda：
            //     场景 1：筛选正偶数，变换成平方，打印 "结果：X"
            //     场景 2：筛选负数，变换成绝对值，打印 "[负数转正] X"
            //
            // 【数据】var data = new List<int> { 5, 12, -8, -3, 20, 7, -10, 4 };
            //
            // 【预期输出】
            //     数据：[5, 12, -8, -3, 20, 7, -10, 4]
            //
            //     ===== 场景 1：正偶数 → 平方 =====
            //     结果：144
            //     结果：400
            //     结果：16
            //
            //     ===== 场景 2：负数 → 绝对值 =====
            //     [负数转正] 8
            //     [负数转正] 3
            //     [负数转正] 10
            //
            // 【提示】
            //     static void ProcessData(List<int> data,
            //                             Func<int, bool> filter,
            //                             Func<int, int> transform,
            //                             Action<int> output)
            //     {
            //         foreach (int n in data)
            //         {
            //             if (!filter(n)) continue;      // 不满足条件就跳过
            //             int result = transform(n);     // 转换
            //             output(result);                // 输出
            //         }
            //     }
            //
            //     // 场景 1
            //     Console.WriteLine("===== 场景 1：正偶数 → 平方 =====");
            //     ProcessData(data,
            //         n => n > 0 && n % 2 == 0,          // 筛选
            //         n => n * n,                        // 转换
            //         r => Console.WriteLine($"结果：{r}"));   // 输出
            //
            //     // 场景 2
            //     Console.WriteLine();
            //     Console.WriteLine("===== 场景 2：负数 → 绝对值 =====");
            //     ProcessData(data,
            //         n => n < 0,
            //         n => Math.Abs(n),
            //         r => Console.WriteLine($"[负数转正] {r}"));
            //
            // 【考察点】
            //   · 三个委托分别负责"筛选 / 转换 / 输出"，职责清晰
            //   · 业务逻辑（ProcessData）和具体行为（Lambda）解耦了
            //   · ★ 这就是真实项目里"可配置处理管道"的雏形
            //   · 同一个 ProcessData，传不同 Lambda 就是不同功能

            Console.WriteLine("--- TODO 5 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
