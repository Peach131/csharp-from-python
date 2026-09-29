// ============================================================================
//  第 10 课 · 练习：异常处理
//
//  ★ 本课重点：
//     1. try 里出异常后，剩余代码立刻中断
//     2. catch 按【从具体到宽泛】排列
//     3. throw; 保留堆栈，throw ex; 会丢失
//     4. ★ 永远不要"空 catch"（吞异常）
//     5. 可预期的分支用 TryXxx，不用异常
// ============================================================================

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ TODO 4 ｜ 自定义异常写在这里
    // ================================================================




    class Lesson10Practice
    {
        // ================================================================
        // ↓↓↓ TODO 2、3、5 用的方法写在这里 ↓↓↓
        // ================================================================




        static void Main()
        {
            Console.WriteLine("===== 第 10 课练习：异常处理 =====\n");

            // ================================================================
            // TODO 1 ｜ 基础 try-catch
            // ================================================================
            // 【要求】用代码模拟 4 种不同的输入，分别处理：
            //     ① "10"            正常 → 打印结果
            //     ② "abc"           格式错误 → 捕获 FormatException
            //     ③ "0" 作为除数    除零 → 捕获 DivideByZeroException
            //     ④ "999999999999"  溢出 → 捕获 OverflowException
            //
            //   对每个输入做：int a = int.Parse(input); 然后算 100 / a
            //
            // 【预期输出】
            //     输入 "10"           → 结果 10
            //     输入 "abc"          → 格式错误，不是有效数字
            //     输入 "0"            → 除数不能为零
            //     输入 "999999999999" → 数字超出了 int 的范围
            //
            // 【语法提示】
            //     string[] inputs = { "10", "abc", "0", "999999999999" };
            //     foreach (string input in inputs)
            //     {
            //         try
            //         {
            //             int a = int.Parse(input);
            //             Console.WriteLine($"输入 \"{input}\" → 结果 {100 / a}");
            //         }
            //         catch (FormatException)
            //         {
            //             Console.WriteLine($"输入 \"{input}\" → 格式错误，不是有效数字");
            //         }
            //         catch (DivideByZeroException)
            //         { ... }
            //         catch (OverflowException)
            //         { ... }
            //     }
            //
            // 【考察点】
            //   · 一个 try 可以配多个 catch
            //   · 注意 "999999999999" 会抛 OverflowException（超出 int.MaxValue）

            Console.WriteLine("--- TODO 1 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ finally 和 return 的关系
            // ================================================================
            // 【要求】
            //   1. 写一个方法 int SafeDivide(int a, int b)：
            //        · 如果 b == 0，用 throw 抛出 DivideByZeroException
            //        · 否则返回 a / b
            //   2. 写一个方法 string TestFinally()：
            //        · try 里 return "try 的返回值"
            //        · finally 里打印 "（finally 执行了）"
            //   3. 调用它们，验证 finally 在 return 之前也执行
            //
            // 【预期输出】
            //     100 / 5 = 20
            //     100 / 0 → 捕获到：除数不能为 0
            //     调用 TestFinally：
            //       （finally 执行了）
            //     返回值：try 的返回值
            //
            // 【语法提示】
            //     static int SafeDivide(int a, int b)
            //     {
            //         if (b == 0) throw new DivideByZeroException("除数不能为 0");
            //         return a / b;
            //     }
            //
            //     static string TestFinally()
            //     {
            //         try
            //         {
            //             return "try 的返回值";
            //         }
            //         finally
            //         {
            //             Console.WriteLine("  （finally 执行了）");
            //         }
            //     }
            //
            // 【考察点】finally 在 return 之前执行 —— 所以它适合做清理工作

            Console.WriteLine("--- TODO 2 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ 异常筛选器 when
            // ================================================================
            // 【要求】写一个方法 void CheckAge(int age)：
            //     age < 0    → throw new ArgumentException("年龄不能为负数")
            //     age > 150  → throw new ArgumentException("年龄不合理")
            //     否则打印 "年龄 25 合法"
            //
            //   然后用 when 分别捕获这两种情况（它们都是 ArgumentException）：
            //     when (ex.Message.Contains("负数")) → 打印"【负数】年龄不能小于 0"
            //     when (ex.Message.Contains("不合理")) → 打印"【不合理】请检查年龄"
            //
            //   测试 -5、200、25 三个值
            //
            // 【预期输出】
            //     CheckAge(-5)：
            //       【负数】年龄不能小于 0
            //     CheckAge(200)：
            //       【不合理】请检查年龄
            //     CheckAge(25)：
            //       年龄 25 合法
            //
            // 【提示】
            //     static void CheckAge(int age)
            //     {
            //         if (age < 0) throw new ArgumentException("年龄不能为负数");
            //         if (age > 150) throw new ArgumentException("年龄不合理");
            //         Console.WriteLine($"年龄 {age} 合法");
            //     }
            //
            //     try { CheckAge(-5); }
            //     catch (ArgumentException ex) when (ex.Message.Contains("负数"))
            //     { Console.WriteLine("  【负数】年龄不能小于 0"); }
            //     catch (ArgumentException ex) when (ex.Message.Contains("不合理"))
            //     { Console.WriteLine("  【不合理】请检查年龄"); }
            //
            // 【考察点】when 让同一种异常按条件走不同分支

            Console.WriteLine("--- TODO 3 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ 自定义异常
            // ================================================================
            // 【要求】
            //   1. 在上面（class 外面）定义自定义异常：
            //        class OutOfStockException : Exception
            //        {
            //            public string ProductName { get; }
            //            public int Requested { get; }
            //            public OutOfStockException(string name, int requested)
            //                : base($"商品【{name}】库存不足，需要 {requested} 件")
            //            { ProductName = name; Requested = requested; }
            //        }
            //   2. 写一个方法 void Buy(string name, int stock, int quantity)：
            //        如果 quantity > stock → throw new OutOfStockException(name, quantity)
            //        否则打印 "购买成功"
            //   3. 测试库存充足和不足两种情况
            //
            // 【预期输出】
            //     买 iPhone（库存 10，要 3 件）：购买成功
            //     买 iPad（库存 2，要 5 件）：
            //       → 捕获到 OutOfStockException
            //         消息：商品【iPad】库存不足，需要 5 件
            //         额外数据：商品=iPad，需要数量=5
            //
            // 【提示】
            //     class OutOfStockException : Exception
            //     {
            //         public string ProductName { get; }
            //         public int Requested { get; }
            //
            //         public OutOfStockException(string name, int requested)
            //             : base($"商品【{name}】库存不足，需要 {requested} 件")
            //         {
            //             ProductName = name;
            //             Requested = requested;
            //         }
            //     }
            //
            //   ★ 注意 : base("消息") —— 把消息交给父类 Exception 的构造函数
            //
            // 【考察点】
            //   · 自定义异常继承 Exception
            //   · 用 : base(msg) 传消息给父类
            //   · 可以加自己的属性携带业务数据

            Console.WriteLine("--- TODO 4 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ ★ 综合：健壮的输入处理（本课考核）
            // ================================================================
            // 【背景】真实的输入处理要能扛住任何乱输入，而且不能掩盖 bug。
            //
            // 【要求】写一个方法：
            //     static double ReadPositiveNumber(string prompt)
            //   行为：
            //     1. 打印 prompt（提示文字）
            //     2. 读一行输入
            //     3. 用 double.TryParse 转换
            //        · 失败 → 抛 ArgumentException("输入不是有效数字：{input}")
            //     4. 如果数字 <= 0 → 抛 ArgumentOutOfRangeException("必须大于 0")
            //     5. 合法就返回这个数字
            //
            //   然后在 Main 里用 try-catch 调用它，模拟 3 种输入
            //   （因为不能真的交互，用参数模拟输入值）
            //
            //   改成这样的签名方便测试：
            //     static double ParsePositive(string input)
            //
            // 【预期输出】
            //     输入 "25.5"  → 解析成功：25.50
            //     输入 "abc"   → 错误：输入不是有效数字：abc
            //     输入 "-5"    → 错误：必须大于 0
            //
            // 【提示】
            //     static double ParsePositive(string input)
            //     {
            //         if (!double.TryParse(input, out double value))
            //         {
            //             throw new ArgumentException($"输入不是有效数字：{input}");
            //         }
            //         if (value <= 0)
            //         {
            //             throw new ArgumentOutOfRangeException(nameof(input), "必须大于 0");
            //         }
            //         return value;
            //     }
            //
            //     // 调用
            //     foreach (string s in new[] { "25.5", "abc", "-5" })
            //     {
            //         try
            //         {
            //             double v = ParsePositive(s);
            //             Console.WriteLine($"输入 \"{s}\"  → 解析成功：{v:F2}");
            //         }
            //         catch (ArgumentException ex)
            //         {
            //             Console.WriteLine($"输入 \"{s}\"  → 错误：{ex.Message}");
            //         }
            //     }
            //
            // 【考察点】
            //   · ★ 一个重要的思维转变：这个方法是"工具方法"，
            //     它【不应该】自己打印错误然后返回 0 —— 那样调用方不知道出错了。
            //     正确做法：抛异常，让调用方决定怎么处理。
            //   · catch (ArgumentException) 能同时捕获 ArgumentException 和
            //     它的子类 ArgumentOutOfRangeException（第 07 课的继承！）
            //   · nameof(input) 会变成字符串 "input" —— 这是 C# 的常用技巧

            Console.WriteLine("--- TODO 5 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
