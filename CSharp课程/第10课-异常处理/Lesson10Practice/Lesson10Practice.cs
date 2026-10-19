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

    // ★ 为什么异常类能直接写在这里？
    //   class 属于"类型"，类型只能住在 namespace 里（不能嵌在另一个 class 里面），
    //   所以自定义异常和 Lesson10Practice 类是【平级】的兄弟。
    //   又因为同一个 namespace 内的类型不需要 using，Main 里可以直接 new 它。

    // ★ 继承 Exception（第 07 课的语法）：继承表示"是一种"，
    //   OutOfStockException 是一种 Exception，所以它才能被 throw、被 catch。
    //   惯例：类名以 Exception 结尾（和 Python 里以 Error 结尾是一个道理）。
    class OutOfStockException : Exception
    {
        // ★ 这两个是【自动属性】，用来携带"业务数据"（商品名、需求数量）。
        //   ProductName 写 { get; } 只读，值在下面的构造函数里一次性写入；
        //   int 是值类型，默认就是 0，永远不会是 null。
        //   因为构造函数一定会给它们赋值，所以不会有 CS8618 警告，也不需要写 = ""。
        public string ProductName { get; }
        public int Requested { get; }

        // ★★ 关键在 : base(...) ——
        //   base 指的是父类 Exception，: base("消息") 的意思是
        //   "先调用父类的构造函数，把这条消息交给它"，
        //   父类会把消息存进自己的 Message 属性里，
        //   于是调用方 catch 到之后 ex.Message 就能读出来。
        //   ❌ 如果只写普通构造函数、不写 : base(...)，
        //      消息就永远存不到 Message 上，ex.Message 会是空字符串。
        public OutOfStockException(string name, int requested)
            : base($"商品【{name}】库存不足，需要 {requested} 件")
        {
            // : base(...) 先执行，然后才执行这里的语句。
            // ★ 左边是属性、右边是参数，名字不同，编译器分得清，不用写 this。
            ProductName = name;
            Requested = requested;
        }
    }


    class Lesson10Practice
    {
        // ================================================================
        // ↓↓↓ TODO 2、3、5 用的方法写在这里 ↓↓↓
        // ================================================================

        // ────────────────────────────────────────────────────────────
        // TODO 2 用的两个方法
        // ────────────────────────────────────────────────────────────

        // ★ 这里为什么写 static？
        //   因为 Main 也是 static —— static 方法不能直接调用非 static 成员
        //   （第 06 课讲过）。而且这个方法不需要访问任何对象的状态，
        //   参数进来、结果出去，写成 static 最合适。
        static int SafeDivide(int a, int b)
        {
            // ★ throw 是"抛出"：一旦执行，当前方法【立刻结束】，
            //   后面的代码一行都不会执行，控制权交给调用方的 catch。
            //   注意 throw 是一条语句，所以 if 不写花括号也能编译
            //   （但花括号更清楚，建议养成习惯写全）。
            if (b == 0)
            {
                throw new DivideByZeroException("除数不能为 0");
            }

            // ★ 对比 Python：C# 的整数除法同样是"向零截断"，
            //   100 / 5 = 20（不是 20.0），因为两边都是 int。
            return a / b;
        }

        // ★ 这个方法专门演示"finally 和 return 的顺序"。
        static string TestFinally()
        {
            try
            {
                // ★★ 请特别注意这一行和 finally 的配合顺序：
                //   return 并不是"立刻走人"——它是【两步】：
                //     第 1 步：把要返回的值算出来、暂存起来
                //     第 2 步：执行 finally，然后才真正离开方法
                //   所以控制台会先看到 finally 那一行，再看到调用处打印返回值。
                return "try 的返回值";
            }
            finally
            {
                // ★ 这里【故意不写 catch】：正好演示 finally 的另一半威力 ——
                //   就算 try 里的异常没人接住（程序要崩了），finally 也照样执行。
                //   所以清理代码放 finally 最保险：不管成功、失败还是 return 都会跑到。
                Console.WriteLine("  （finally 执行了）");
            }
        }

        // ────────────────────────────────────────────────────────────
        // TODO 3 用的方法
        // ────────────────────────────────────────────────────────────
        static void CheckAge(int age)
        {
            // ★ 为什么两次都 throw ArgumentException？
            //   因为对调用方来说"年龄不合法"是同一类错误，
            //   具体是"负数"还是"太大"，靠 Message 里的关键字来区分，
            //   然后让 Main 里的 when 筛选器去分流（下面马上看到）。
            if (age < 0)
            {
                throw new ArgumentException("年龄不能为负数");
            }

            if (age > 150)
            {
                throw new ArgumentException("年龄不合理");
            }

            // ★ 能走到这一行，说明上面两个 if 都没触发（异常会把控制流带走），
            //   所以这里不需要写 else —— 这是异常的一个好用之处：
            //   正常路径的代码可以一直平铺往下写，不用层层缩进。
            Console.WriteLine($"年龄 {age} 合法");
        }

        // ────────────────────────────────────────────────────────────
        // TODO 4 用的方法
        // ────────────────────────────────────────────────────────────
        static void Buy(string name, int stock, int quantity)
        {
            // ★ 库存不足是"业务规则被违反"，不是系统故障，
            //   所以这里用【自定义异常】：调用方能精确地 catch 到它，
            //   还能从异常对象上读回 ProductName / Requested 这两个业务数据。
            if (quantity > stock)
            {
                throw new OutOfStockException(name, quantity);
            }

            // ★ 只在成功路径打印 —— 失败时由调用方（Main）决定怎么显示，
            //   工具方法不替调用方做决定（TODO 5 的考察点也是这个道理）。
            Console.WriteLine("购买成功");
        }

        // ────────────────────────────────────────────────────────────
        // TODO 5 用的方法：★ 本课核心 —— 工具方法该抛异常，不该自己打印
        // ────────────────────────────────────────────────────────────
        static double ParsePositive(string input)
        {
            // ★★ 这是本课"不要用异常做流程控制"的正面用法：
            //   判断"能不能转成数字"是可预期的正常分支，用 TryParse 就够了，
            //   完全不需要 try-catch 去接 FormatException。
            //   TryParse 返回 bool 表示成功与否，转换结果通过 out 参数带出来
            //   （out 表示"这个方法会往里面写值"，第 04 课学过）。
            //   注意这里把 ! 取反了：转换【失败】才是要处理的分支。
            if (!double.TryParse(input, out double value))
            {
                // ★ 转换失败才抛异常 —— 这是"契约被违反"：
                //   方法要求"给我一个数字字符串"，调用方给了 "abc"。
                // ★ 注意消息里用了 {input} 把原始输入带出来，
                //   否则调用方只知道"不是有效数字"，不知道到底输入了什么。
                throw new ArgumentException($"输入不是有效数字：{input}");
            }

            if (value <= 0)
            {
                // ★ 为什么用 ArgumentOutOfRangeException 而不是 ArgumentException？
                //   它是 ArgumentException 的【子类】（第 07 课的继承），
                //   语义更精确："参数的值超出了允许范围"。
                //   两全其美：既能被 catch (ArgumentOutOfRangeException) 精确接住，
                //   也能被 catch (ArgumentException) 兜住（父类能接子类的异常）。
                // ★ nameof(input) 会变成字符串 "input"，好处是：
                //   以后你把这个参数改名，编译器会自动同步，
                //   而写死 "input" 字符串则会悄悄失效（第 12 课会讲到 nameof 的更多用法）。
                // ★ .NET 9 下 ex.Message 就是你传的这句消息本身（"必须大于 0"），
                //   参数名单独存在 ex.ParamName 里，不会拼进消息里。
                throw new ArgumentOutOfRangeException(nameof(input), "必须大于 0");
            }

            // ★ 走到这里说明输入合法，把数字交回给调用方。
            // ★★ 对比一种常见的新手写法：出错时 Console.WriteLine("错误") 然后 return 0。
            //    那样调用方拿到 0，根本分不清"用户真的输入了 0"还是"出错了"，
            //    bug 就这么被掩盖掉了。抛异常才是正确做法 —— 让调用方自己决定
            //   是打印提示、还是重试、还是往上抛。
            return value;
        }


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

            // ① 把 4 种输入放进数组，用同一段逻辑反复处理
            //   ★ 为什么不用真的 Console.ReadLine()？
            //     因为练习要能"一键跑完"、每次都得到同样的结果。
            //     用代码模拟输入，是测试异常处理最省事的办法。
            string[] inputs = { "10", "abc", "0", "999999999999" };

            foreach (string input in inputs)
            {
                // ★ try 里面放"可能出事"的代码。注意 int.Parse 和除法【两件事】
                //   都放在同一个 try 里 —— 因为它们都可能抛异常，
                //   而无论哪一步抛，后面的 Console.WriteLine 都不会执行。
                try
                {
                    int a = int.Parse(input);

                    // ★ 插值字符串里想显示双引号，要写转义 \"；
                    //   想显示花括号才需要写 {{ }}（本行不需要）。
                    // ★ 这一行能被打印出"结果"，前提是上面没抛异常。
                    Console.WriteLine($"输入 \"{input}\" → 结果 {100 / a}");
                }
                // ★★ catch 的顺序＝从具体到宽泛。
                //   这三个异常【互相之间没有继承关系】，所以谁先谁后都能正常工作；
                //   但如果末尾再加一个 catch (Exception) 兜底，它就必须排在最后 ——
                //   否则它会把前面三个全挡住，编译器直接报错（这就是"从具体到宽泛"）。
                //
                // ★ int.Parse("abc") 抛 FormatException：
                //   字符串的格式不是合法整数（Python 里对应 ValueError）。
                catch (FormatException)
                {
                    // ★ 这里没写 ex 变量名，因为我们不需要异常对象里的任何信息
                    //   —— 场景本身已经把要说什么讲清楚了。
                    //   ⚠️ 但这绝【不是】空 catch：空 catch 是连处理都不做，
                    //      那才是本课反复强调的最危险写法。
                    Console.WriteLine($"输入 \"{input}\" → 格式错误，不是有效数字");
                }
                // ★ 除数为 0 时抛 DivideByZeroException。
                //   Python 里对应 ZeroDivisionError —— 整数除以 0 必然报错，
                //   而浮点数除以 0 得到的是 Infinity，两者行为不同。
                catch (DivideByZeroException)
                {
                    Console.WriteLine($"输入 \"{input}\" → 除数不能为零");
                }
                // ★ "999999999999" 能解析成数字、但装不进 int（超过 int.MaxValue
                //   2147483647），所以它抛的不是 FormatException 而是 OverflowException。
                //   ★ 这也说明"每个 catch 各管一种类型"：溢出不归格式那个 catch 管，
                //     所以必须单独写一个 catch，否则这个输入就没法被友好地处理。
                catch (OverflowException)
                {
                    Console.WriteLine($"输入 \"{input}\" → 数字超出了 int 的范围");
                }
            }

            // ★ 观察点：输入 "abc" 时，try 里 int.Parse 之后的代码【全部被跳过】，
            //   直接跳到 FormatException 的 catch —— 这就是"异常打断控制流"。
            //   然后 foreach 继续下一轮，异常被处理掉了，程序不会崩。

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

            // ① 正常情况：b 不为 0，SafeDivide 走 return 分支
            //   ★ 100 和 5 都是 int，方法参数也是 int → 结果是 20，不写 .0
            Console.WriteLine($"100 / 5 = {SafeDivide(100, 5)}");

            // ② 出错情况：b == 0 → 方法内部 throw → 这里 catch 接住
            //   ★★ 注意 try 里【只放了一行】：
            //     因为 SafeDivide 一抛异常，这一行就中断了，
            //     它后面的代码属于"异常发生时不该执行"的部分。
            //     如果换成用返回值表示错误，就得写 if 判断返回值，
            //     还可能忘了判断 —— 异常的强制中断反而更安全。
            try
            {
                int result = SafeDivide(100, 0);
                Console.WriteLine($"100 / 0 = {result}");
            }
            catch (DivideByZeroException ex)
            {
                // ★ ex.Message 就是刚才 throw 时传进去的 "除数不能为 0"
                //   —— 异常对象把"为什么出错"一路带到了处理现场。
                Console.WriteLine($"100 / 0 → 捕获到：{ex.Message}");
            }

            // ③ 验证 finally 在 return 之前执行
            Console.WriteLine("调用 TestFinally：");

            // ★★ 这一行是本节的重点，请记住执行顺序：
            //   TestFinally() 内部先算出 "try 的返回值" 暂存起来
            //   → 执行 finally、打印 "  （finally 执行了）"
            //   → 方法真正返回，外层才拿到值、打印下面这一行
            //   所以屏幕上 finally 那句一定出现在"返回值："之前。
            //   ★ 实际用途：不管方法从哪条路返回，清理代码（关文件、断开连接）
            //     都一定会被执行到。
            string value = TestFinally();
            Console.WriteLine($"返回值：{value}");

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

            // ★ 三个待测年龄放进数组，用同一段 try-catch 反复测
            int[] ages = { -5, 200, 25 };

            foreach (int age in ages)
            {
                // ★ 先打印"在测哪个值"。
                //   这一行写在 try【外面】，因为它和"会不会抛异常"无关，
                //   放里面的话抛异常时就打不出来了。
                Console.WriteLine($"CheckAge({age})：");

                try
                {
                    CheckAge(age);
                }
                // ★★ 本节的考点：两个 catch【类型完全相同】，都是 ArgumentException，
                //   区分它们的唯一手段就是 when 后面的条件。
                //   when 不成立时，这个 catch 直接【跳过】，异常继续往下找下一个 catch
                //   —— 关键在于：跳过的 catch 里如果抛了新异常，绝不会被后面接住。
                //
                // ★ 为什么这里必须写 ex 变量名（而 TODO 1 没写）？
                //   因为 when 的条件要用到 ex.Message。只在 when 里用到也算"用到了"，
                //   所以不会报 CS0168"变量已声明但未使用"。
                catch (ArgumentException ex) when (ex.Message.Contains("负数"))
                {
                    // ★ 进了这个分支，说明消息里含"负数"，即 age < 0。
                    Console.WriteLine("  【负数】年龄不能小于 0");
                }
                // ★ 第二个筛选器：消息里含"不合理"。两个 when 互斥，
                //   所以同一个异常只会走其中一个分支，不会重复打印。
                catch (ArgumentException ex) when (ex.Message.Contains("不合理"))
                {
                    Console.WriteLine("  【不合理】请检查年龄");
                }

                // ★ age = 25 时 CheckAge 没抛异常、自己打印了"年龄 25 合法"，
                //   两个 catch 都不会执行 —— 异常处理的"正常路径"就是不进 catch。
            }

            // ★ 对比 without-when 的老写法：只能 catch 到一个分支，
            //   然后在里面再写 if (ex.Message.Contains(...)) 手工分流，
            //   一旦忘了写 if，"负数"和"不合理"就会打印成同一句，信息就丢了。
            //   when 把"分流条件"提到了 catch 上，意图更清楚。

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

            // ① 库存充足：10 >= 3 → Buy 走正常路径，打印"购买成功"
            try
            {
                // ★ 用 Console.Write（不换行）+ Buy 里的 WriteLine，
                //   拼起来正好是一行："买 iPhone（……）：购买成功"
                Console.Write("买 iPhone（库存 10，要 3 件）：");
                Buy("iPhone", 10, 3);
            }
            // ★★ 这里 catch 的是【自定义】异常类型。
            //   好处是精确：只有"库存不足"才会进这个 catch，
            //   别的异常（比如以后 Buy 里又加了别的校验）不会被误当成库存问题。
            //   对比第 07 课的"多态"：catch 一个具体类型，
            //   就是让编译器按类型帮我们把处理逻辑分派到正确的分支上。
            catch (OutOfStockException ex)
            {
                // 正常情况下走不到这里，写出来是为了万一逻辑被改坏时能立刻发现
                Console.WriteLine($"意外：{ex.Message}");
            }

            // ② 库存不足：2 < 5 → Buy 抛 OutOfStockException
            try
            {
                Console.Write("买 iPad（库存 2，要 5 件）：\n");
                Buy("iPad", 2, 5);

                // ★ 注意 Buy 后面【没有】再写 Console.WriteLine：
                //   因为 Buy 一旦抛异常，这里就中断了，写了也是死代码。
                //   体会"异常让代码中断"这件事。
            }
            catch (OutOfStockException ex)
            {
                // ★ 第一行刻意没有前导空格，好和上面那句商店信息对齐成两行
                Console.WriteLine("  → 捕获到 OutOfStockException");

                // ★ ex.Message 来自自定义异常构造函数里的 : base(...)，
                //   所以消息是构造时就拼好的："商品【iPad】库存不足，需要 5 件"。
                Console.WriteLine($"    消息：{ex.Message}");

                // ★★ 这才是自定义异常真正的价值：除了消息，
                //   还能把【结构化】的业务数据带出来给调用方用
                //   （比如拿 ProductName 去查库存、拿 Requested 去重下单）。
                //   如果只用内置 Exception，这些数据就丢了，只能去解析字符串。
                Console.WriteLine($"    额外数据：商品={ex.ProductName}，需要数量={ex.Requested}");
            }

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

            // ★ 用数组假装"用户依次输入了这三个值"。
            //   真正的 ReadPositiveNumber(prompt) 就是把 input 换成 Console.ReadLine()，
            //   其余逻辑一模一样 —— 这样拆开是为了能一键自动测试。
            string[] testInputs = { "25.5", "abc", "-5" };

            foreach (string input in testInputs)
            {
                try
                {
                    double v = ParsePositive(input);

                    // ★ {v:F2} 的 F2 = 定点格式、保留 2 位小数 → 25.50
                    //   注意是"格式化显示"，v 本身仍然是 25.5，没有变。
                    Console.WriteLine($"输入 \"{input}\"  → 解析成功：{v:F2}");
                }
                // ★★ 这一个 catch 同时接住了两种异常：
                //   · ParsePositive 里抛的 ArgumentException（"abc" 那种）
                //   · 以及它的【子类】ArgumentOutOfRangeException（-5 那种）
                //   —— 这就是第 07 课继承的实用价值：父类类型的 catch
                //   能接住所有子类异常（反过来不行）。
                //   所以这里根本不用写两个 catch 分支，代码更短。
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"输入 \"{input}\"  → 错误：{ex.Message}");
                }
            }

            // ★★ 本节最重要的思维转变（也是本课的"考核点"）：
            //   ParsePositive 是【工具方法】，它只负责"判断输入合不合法"，
            //   不负责"告诉用户怎么办"。
            //   · 抛异常 → 调用方自己决定是打印、重试还是往上抛（现在这样）
            //   · 打印错误再 return 0 → 调用方只看得到 0，压根不知道出过错，
            //     于是"错误"被静默地当成了合法数字 0 继续算下去。
            //   工具方法只管规则，输出方式留给调用方 —— 这就是分工。

            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
