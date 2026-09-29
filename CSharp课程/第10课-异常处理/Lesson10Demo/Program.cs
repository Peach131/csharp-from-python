// ============================================================================
//  第 10 课 · 示例代码
//  异常处理 —— try / catch / finally、throw、自定义异常
// ============================================================================

namespace Lesson10Demo
{
    // ========================================================================
    // 自定义异常：携带业务信息
    // ========================================================================
    class InsufficientBalanceException : Exception
    {
        public decimal Balance { get; }
        public decimal Required { get; }

        public InsufficientBalanceException(decimal balance, decimal required)
            : base($"余额不足：需要 {required:F2}，实际只有 {balance:F2}")
        {
            Balance = balance;
            Required = required;
        }
    }


    class Program
    {
        static int Divide(int a, int b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException("除数不能为 0");
            }
            return a / b;
        }

        static void Withdraw(decimal balance, decimal amount)
        {
            if (amount > balance)
            {
                throw new InsufficientBalanceException(balance, amount);
            }
            Console.WriteLine($"      取款成功，剩余 {balance - amount:F2}");
        }

        // 用异常做流程控制（不推荐）
        static bool IsNumberByException(string s)
        {
            try
            {
                int.Parse(s);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


        static void Main()
        {
            Console.WriteLine("========== 第 10 课：异常处理 ==========\n");


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 1：不处理异常会怎样 ----");

            Console.WriteLine("   直接 int.Parse(\"abc\")：");
            try
            {
                int n = int.Parse("abc");          // 会抛 FormatException
                Console.WriteLine($"   {n}");
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"   → 抛出了 {ex.GetType().Name}");
                Console.WriteLine($"   → 消息：{ex.Message}");
            }
            Console.WriteLine("   ★ 如果不用 try-catch，程序会直接崩溃退出");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 2：★ try 里出异常后，后面的代码不执行 ----");

            try
            {
                Console.WriteLine("   A");
                int x = int.Parse("abc");          // ← 在这里抛异常
                Console.WriteLine("   B（永远不会打印）");
            }
            catch (FormatException)
            {
                Console.WriteLine("   C（直接从 A 跳到这里）");
            }
            Console.WriteLine("   ★ try 块里一旦出异常，剩余代码【立刻中断】，跳到 catch");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 3：多种异常分别处理 ----");

            // 用代码模拟三种不同的输入
            string[] testInputs = { "10", "abc", "0", "999999999999" };

            foreach (string input in testInputs)
            {
                Console.Write($"   输入 \"{input}\" 除以 10 → ");
                try
                {
                    int a = int.Parse(input);
                    Console.WriteLine($"{10 / a}");
                }
                catch (FormatException)
                {
                    Console.WriteLine("格式错误（不是数字）");
                }
                catch (DivideByZeroException)
                {
                    Console.WriteLine("除数不能为 0");
                }
                catch (OverflowException)
                {
                    Console.WriteLine("数字超出 int 范围");
                }
            }
            Console.WriteLine("   ★ catch 要按【从具体到宽泛】的顺序排列");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 4：finally 一定会执行 ----");

            Console.WriteLine("   情况 A：try 正常结束");
            try
            {
                Console.WriteLine("      try");
            }
            finally
            {
                Console.WriteLine("      finally ← 执行了");
            }

            Console.WriteLine("   情况 B：try 里抛异常");
            try
            {
                Console.WriteLine("      try → 要抛异常了");
                throw new Exception("测试异常");
            }
            catch (Exception)
            {
                Console.WriteLine("      catch");
            }
            finally
            {
                Console.WriteLine("      finally ← 也执行了");
            }

            Console.WriteLine("   情况 C：try 里 return");
            Console.WriteLine($"      调用有 return 的方法：{MethodWithReturn()}");
            Console.WriteLine("   ★ finally 用途：清理资源（关文件、断连接）");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 5：throw 抛出异常 ----");

            try
            {
                Console.WriteLine($"   10 / 2 = {Divide(10, 2)}");
                Console.WriteLine($"   10 / 0 → 即将抛异常");
                Console.WriteLine($"   10 / 0 = {Divide(10, 0)}");
            }
            catch (DivideByZeroException ex)
            {
                Console.WriteLine($"   捕获到：{ex.Message}");
            }
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 6：★ throw; vs throw ex; ----");

            try
            {
                try
                {
                    throw new InvalidOperationException("原始错误");
                }
                catch (Exception)                   // ★ 不写变量名：因为只重新抛出，不需要 ex 的内容
                {
                    Console.WriteLine("   内层捕获，准备重新抛出");
                    throw;                          // ✅ 保留完整堆栈
                }
            }
            catch (Exception ex)
            {
                // 取堆栈的第一行 —— 就是错误真正发生的位置
                string? firstFrame = ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim();
                Console.WriteLine($"   外层捕获到：{ex.Message}");
                Console.WriteLine($"   堆栈第一行：{firstFrame}");
                Console.WriteLine("   ★ throw; 保留了原始位置。换成 throw ex; 这里会指向重新抛出的那一行");
            }
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 7：异常筛选器 when ----");

            string[] errorCodes = { "库存不足", "余额不足", "其他错误" };
            foreach (string code in errorCodes)
            {
                try
                {
                    throw new InvalidOperationException(code);
                }
                // ★ 两种写法对比：
                //   catch (T ex)  → 后面要用到 ex（打印消息、读属性）
                //   catch (T)     → 不需要 ex 的具体内容时，不写变量名（避免未使用警告）
                //
                // ⚠️ 注意：只在 when 里用到 ex，编译器仍会报 CS0168"变量未使用"，
                //    所以这种情况下干脆不写变量名。
                catch (InvalidOperationException) when (code == "库存不足")
                {
                    Console.WriteLine($"   \"{code}\" → 走【库存】分支：稍后重试");
                }
                catch (InvalidOperationException) when (code == "余额不足")
                {
                    Console.WriteLine($"   \"{code}\" → 走【余额】分支：请充值");
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine($"   \"{code}\" → 走【兜底】分支");
                }
            }
            Console.WriteLine("   ★ when 让同一个异常类型按条件分流");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 8：★ 自定义异常 ----");

            try
            {
                Console.Write("   余额 100，取 500：");
                Withdraw(100, 500);
            }
            catch (InsufficientBalanceException ex)
            {
                Console.WriteLine($"   → 捕获到自定义异常");
                Console.WriteLine($"     消息：{ex.Message}");
                Console.WriteLine($"     额外数据：余额={ex.Balance}，需要={ex.Required}");
            }
            Console.WriteLine("   ★ 自定义异常能给调用方【精确捕获】+ 携带【业务数据】");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 9：★ 不要用异常做流程控制 ----");

            string test = "123";

            Console.WriteLine($"   判断 \"{test}\" 是不是数字：");
            Console.WriteLine($"      用 try-catch  ：{IsNumberByException(test)}   ← 有性能代价");
            Console.WriteLine($"      用 TryParse   ：{int.TryParse(test, out _)}   ← 推荐，无异常开销");
            Console.WriteLine();

            string test2 = "abc";
            Console.WriteLine($"   判断 \"{test2}\" 是不是数字：");
            Console.WriteLine($"      用 try-catch  ：{IsNumberByException(test2)}");
            Console.WriteLine($"      用 TryParse   ：{int.TryParse(test2, out _)}");
            Console.WriteLine("   ★ 用户输入错误是【正常情况】，不该用异常；磁盘满了才是【异常情况】");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 10：防止 NullReferenceException ----");

            string? name = null;

            Console.WriteLine("   对 null 调用成员会崩溃，4 种防御写法：");

            // ① 判空
            if (name is not null)
            {
                Console.WriteLine($"      ① is not null 判断：{name.Length}");
            }
            else
            {
                Console.WriteLine("      ① is not null 判断：name 是 null，跳过");
            }

            // ② ?. 空条件运算符
            int? len = name?.Length;
            Console.WriteLine($"      ② name?.Length = {(len is null ? "null（不崩溃）" : len.ToString())}");

            // ③ ?? 空合并
            string display = name ?? "（未填写）";
            Console.WriteLine($"      ③ name ?? \"（未填写）\" = {display}");

            // ④ ??= 空合并赋值
            name ??= "默认名字";
            Console.WriteLine($"      ④ name ??= \"默认名字\" 之后 = {name}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson10Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }

        // 演示 finally 在 return 时也会执行
        static string MethodWithReturn()
        {
            try
            {
                return "try 的返回值";
            }
            finally
            {
                Console.WriteLine("      （finally 在 return 之前也执行了）");
            }
        }
    }
}
