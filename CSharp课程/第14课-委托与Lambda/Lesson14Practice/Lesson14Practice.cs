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

        // ────────────────────────────────────────────────────────────
        // TODO 2 ｜ 通用筛选：条件由【参数】传进来
        // ────────────────────────────────────────────────────────────
        // ★ 对比：如果不用委托，就要为"偶数""大于 10""正数"各写一个方法，
        //   而它们的方法体一模一样，只有 if 的条件不同。
        //   把条件变成 Func<int, bool> 参数之后，Filter 只写一次、永远不用改。
        // ★ 为什么返回类型是 List<int> 而不是 void？
        //   因为调用者要拿结果去打印/继续处理，所以必须把新集合 return 出去。
        // ★ 注意这里是【新建】一个 List，没有修改传进来的 nums ——
        //   调用者的数据保持原样，这种"不改参数"的写法更安全。
        static List<int> Filter(List<int> nums, Func<int, bool> condition)
        {
            var result = new List<int>();

            foreach (int n in nums)
            {
                // ★ 这一行就是本课的魔法：condition 是个【变量】，
                //   但它后面能加括号调用 —— 因为委托变量装的就是一个方法。
                //   condition(n) 返回 true 表示"这个元素要"，false 表示"跳过"。
                if (condition(n))
                {
                    result.Add(n);
                }
            }

            return result;
        }

        // ────────────────────────────────────────────────────────────
        // TODO 2 ｜ 通用转换：把每个元素"变成"另一个值
        // ────────────────────────────────────────────────────────────
        // ★ 和 Filter 的区别只在返回类型上：
        //   Filter 的条件函数返回 bool（只回答"要不要"）
        //   Transform 的映射函数返回 int（给出"新值是什么"）
        //   Func<int, int> 的前一个 int 是输入类型，后一个 int 是输出类型。
        static List<int> Transform(List<int> nums, Func<int, int> mapper)
        {
            var result = new List<int>();

            foreach (int n in nums)
            {
                result.Add(mapper(n));          // ★ 调用传进来的转换方法

                // ★ 这里不需要再写 n * n 或者 Math.Abs(n) ——
                //   具体怎么变由调用者决定，Transform 只负责"遍历 + 收集"。
            }

            return result;
        }

        // ────────────────────────────────────────────────────────────
        // TODO 5 ｜ 综合：可配置的数据处理器（本课考核）
        // ────────────────────────────────────────────────────────────
        // ★ 三个委托参数各管一件事，这就是"职责分离"：
        //     filter    → 决定"哪些元素留下"（筛选）
        //     transform → 决定"元素变成什么"（转换）
        //     output    → 决定"结果怎么显示"（输出）
        //   ProcessData 本身一个业务字眼都没有，所以它能被无限复用。
        // ★ 和上面 Filter + Transform 的区别：
        //   那两个方法都把结果装进 List 返回，需要额外的内存；
        //   而 ProcessData 是"边算边吐"，元素处理完立刻交给 output，不存中间结果。
        static void ProcessData(List<int> data,
                                Func<int, bool> filter,
                                Func<int, int> transform,
                                Action<int> output)
        {
            foreach (int n in data)
            {
                if (!filter(n)) continue;      // 不满足条件就跳过

                int result = transform(n);     // 转换

                output(result);                // 输出
            }

            // ★ 返回类型 void：因为"输出"这件事已经由 output 委托做完了，
            //   ProcessData 不需要把任何东西还给调用者。
        }

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

            // ★ ① Func<int, int>：两个 int 的含义是"接收一个 int、返回一个 int"，
            //   最后一格永远是返回类型，所以 Func<int, int> 是"一个进一个出"。
            //   给变量起名 square（平方），是因为它装的是求平方这个方法。
            Func<int, int> square = x => x * x;

            // ★ ② Func<int, int, int>：三格 = 两个参数 + 一个返回类型。
            //   (a, b) 是参数列表；右侧用三元运算符（第 02 课学的）挑出较大值。
            //   可以读成"a > b 就返回 a，否则返回 b"。
            Func<int, int, int> maxOf = (a, b) => a > b ? a : b;

            // ★ ③ Action<string>：只有一格，那是【参数类型】，不是返回类型。
            //   Action 家族永远"有去无回"（可以带参数，但没有返回值），
            //   所以它的 Lambda 体里不需要（也不能）写 return。
            Action<string> print = msg => Console.WriteLine($"[信息] {msg}");

            // ★ ④ Predicate<int>：专门表示"判断"的委托，等价于 Func<int, bool>。
            //   它只有一格（参数类型），返回类型被固定写死成 bool，
            //   所以 Lambda 体必须是一个能算出 true/false 的表达式。
            //   （注意：Predicate<int> 和 Func<int, bool> 是两种不同的委托类型，
            //     不能互相赋值，这里是独立的一份。）
            Predicate<int> isAdult = age => age >= 18;

            // ★ 调用委托变量时就当普通方法用：变量名 + (参数)。
            //   编译器在背后翻译成 square.Invoke(7)，写起来更短。
            //
            // ★★ = 号前面那一串空格不是随便敲的：三行都是"文字 + 对齐空格 + = 值"，
            //    数一下就知道每行的文字部分都占 12 个字符宽，所以三个 = 号能上下对齐，
            //    打印出来正好和上面【预期输出】里的空格数一致（改空格数输出就变了）。
            Console.WriteLine($"square(7)      = {square(7)}");
            Console.WriteLine($"maxOf(3, 9)    = {maxOf(3, 9)}");

            // ★ print 是 Action（无返回值），所以它【不能】放在 $"..." 的 {} 里取值，
            //   只能单独当一条语句调用 —— 它会自己把内容打印出来。
            print("你好");

            // ★ {isAdult(20)} 里的 bool 会被插值成 "True" / "False"（首字母大写），
            //   这是 C# 的 bool.ToString() 规则，和 Python 的 True/False 恰好一致。
            Console.WriteLine($"isAdult(20)    = {isAdult(20)}");
            Console.WriteLine($"isAdult(15)    = {isAdult(15)}");

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

            // ★ 数据放在这里只声明一次，下面 7 个调用共用它。
            //   因为 Filter / Transform 都【不修改】传入的集合，所以重复传是安全的。
            var nums = new List<int> { 5, 12, 8, -3, 20, 7, -10, 0 };

            // ★ string.Join(", ", 集合) 的作用：把集合元素用 ", " 连成一个字符串。
            //   外面再补一对 [ ]，就能打印成 Python 列表的形状 [5, 12, ...]。
            //   （它不是 LINQ，只是 string 类的一个静态方法，第 13 课学过。）
            Console.WriteLine($"数据：[{string.Join(", ", nums)}]");

            Console.WriteLine("---------- 筛选 ----------");

            // ★ 下面 4 行是同一个 Filter 的 4 次调用，变的只是第二个参数。
            //   每个 n => ... 都是"当场写、当场用、没有名字"的一次性方法。
            //   参数类型 int 和返回类型 bool 都能从 Func<int, bool> 推断出来，
            //   所以我们只写参数名 n 就行，不用写 (int n) => ...。
            Console.WriteLine($"偶数      ：[{string.Join(", ", Filter(nums, n => n % 2 == 0))}]");
            //   n % 2 == 0 用的是第 02 课的取余：负奇数取余为 -1，不等于 0，
            //   所以这个条件对 -10 也成立 → -10 被算作偶数，正确。

            Console.WriteLine($"大于 10   ：[{string.Join(", ", Filter(nums, n => n > 10))}]");

            Console.WriteLine($"正数      ：[{string.Join(", ", Filter(nums, n => n > 0))}]");
            //   注意 0 不是正数，所以结果里没有 0。

            Console.WriteLine($"绝对值小于 10：[{string.Join(", ", Filter(nums, n => Math.Abs(n) < 10))}]");
            //   这个条件同时吃掉了"小的正数"和"绝对值小的负数"，
            //   -3 能留下、-10 被过滤掉 —— 一行 Lambda 就表达完了。

            Console.WriteLine("---------- 转换 ----------");

            // ★ 传进去的函数决定"怎么变"，Transform 本身完全不知道业务：
            //   传 n => n * n 就是平方，传 n => Math.Abs(n) 就是取绝对值。
            //   ★ 这就是本课的核心收获：行为（Lambda）和数据（nums）分开了。
            Console.WriteLine($"平方      ：[{string.Join(", ", Transform(nums, n => n * n))}]");
            Console.WriteLine($"取绝对值  ：[{string.Join(", ", Transform(nums, n => Math.Abs(n)))}]");
            Console.WriteLine($"加 100    ：[{string.Join(", ", Transform(nums, n => n + 100))}]");

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

            // ★ 局部函数（第 04 课学过）：只是给这行长表达式起个短名字，
            //   让下面的代码读起来是 "before = Alloc()" 而不是一长串。
            //   GC.GetAllocatedBytesForCurrentThread() 返回的是
            //   "当前线程从出生到现在累计分配的总字节数"，所以要用【差值】来测。
            long Alloc() => GC.GetAllocatedBytesForCurrentThread();

            // ★ 被捕获的变量：Lambda 里用到它，编译器就必须把它搬到堆上的
            //   "显示类"（display class）里，否则 Lambda 执行时早就找不到它了。
            int threshold = 10;

            // ★ 连做三次 GC 是为了把"垃圾回收本身造成的噪声"降到最低：
            //   GC.Collect() 回收；WaitForPendingFinalizers() 等终结器跑完；
            //   再 Collect() 一次收掉终结器产生的垃圾。这样读数才干净。
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            long before = Alloc();
            for (int i = 0; i < 1000; i++)
            {
                Func<int, bool> cap = n => n > threshold;    // ★ 捕获 threshold
                GC.KeepAlive(cap);                            // 防止被优化掉
            }
            long allocCaptured = Alloc() - before;

            // ★ 不捕获的版本：Lambda 体里只有常量 10，没碰任何外部变量。
            //   C# 9 之后编译器会把这种 Lambda 缓存成一个【静态单例】，
            //   所以循环 1000 次理论上一次堆分配都没有。
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            before = Alloc();
            for (int i = 0; i < 1000; i++)
            {
                Func<int, bool> plain = n => n > 10;          // ★ 没有任何捕获
                GC.KeepAlive(plain);                           // 同样防止被优化掉
            }
            long allocNoCapture = Alloc() - before;

            // ★ 为什么必须写 GC.KeepAlive？
            //   如果 Lambda 变量创建后没人用，JIT 可能认为"这行没意义"而整段消除，
            //   于是分配字节数变成 0 —— 测出来的就不是真相了。
            //   KeepAlive(x) 是空方法，唯一的语义就是"告诉编译器：x 还活着"。
            //
            // ★★ 数字会因【机器、Debug/Release、.NET 版本】而不同，别抄具体值：
            //    Release（开优化）下不捕获那行正好是 0；
            //    Debug 下委托对象本身也会分配，可能出现几百字节的小数字。
            //    这里用 ,6 右对齐，是为了让两行的数字宽度一致、便于对比。
            Console.WriteLine($"闭包（捕获变量）  ：分配 {allocCaptured,6} 字节");
            Console.WriteLine($"不捕获           ：分配 {allocNoCapture,6} 字节");
            Console.WriteLine("→ 闭包每次创建都要堆分配");

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

            // ★ List<Action>：集合里装的不是数字，而是"待会儿要执行的动作"。
            //   这行执行完，什么事都还没发生 —— 我们只是把 3 个动作排队存起来。
            var actions = new List<Action>();

            // ★ ❌ 错误版本：for 的 i 从头到尾【只有一个变量】，
            //   循环体里那句 () => Console.Write(i) 捕获的是"变量 i 本身"，
            //   不是"i 当时的那个值"（捕获的是盒子，不是盒子里的快照）。
            //   所以等下面真正执行这 3 个 Action 时，i 早就变成 3 了 → 打印 3 3 3。
            for (int i = 0; i < 3; i++)
                actions.Add(() => Console.Write(i));

            Console.Write("❌ for 直接捕获 ：");
            // ★ 执行动作：a 是 Action，a() 就是"调用这个动作"。
            foreach (var a in actions) a();
            Console.WriteLine();

            // ★ ✅ 修复版本：每次迭代都在循环体里新建一个局部变量 copy，
            //   于是每一轮都得到【一个新的盒子】，3 个 Lambda 分别捕获 3 个不同的 copy。
            //   C# 里用 { } 包出作用域，copy 的生命周期就只有这一轮 ——
            //   编译器会为每一位"被捕获的变量"生成独立的存储。
            actions.Clear();
            for (int i = 0; i < 3; i++)
            {
                int copy = i;                       // ★ 关键
                actions.Add(() => Console.Write(copy));
            }

            Console.Write("✅ for + 局部副本：");
            foreach (var a in actions) a();
            Console.WriteLine();

            // ★ ✅ foreach 版本：C# 5 之后规定了 foreach 的迭代变量
            //   【每次迭代都是一个全新的变量】，所以直接捕获就天然安全，
            //   不需要再写局部副本。
            //   （Enumerable.Range(0, 3) 就是"从 0 开始数 3 个"→ 0,1,2，
            //     这里只是不想手写数组 new[] { 0, 1, 2 } 而已。）
            actions.Clear();
            foreach (var i in Enumerable.Range(0, 3))
                actions.Add(() => Console.Write(i));

            Console.Write("✅ foreach      ：");
            foreach (var a in actions) a();
            Console.WriteLine();

            // ★ 一句话记住：Lambda 捕获的是【变量】，不是【值的快照】。
            //   Python 里也有同样的坑：[lambda: i for i in range(3)] 会全部返回 2。
            //   最稳的习惯：for 循环里要捕获循环变量，就先 int copy = i;

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

            // ★ 本场景的数据（第 2 个场景要用它，所以先打印出来对照）
            var data = new List<int> { 5, 12, -8, -3, 20, 7, -10, 4 };
            Console.WriteLine($"数据：[{string.Join(", ", data)}]");
            Console.WriteLine();

            // ★ 场景 1：三个 Lambda 分别占住三个槽位。
            //   ① n => n > 0 && n % 2 == 0 —— 用 && 把两个条件串起来：
            //      先要正数，再要偶数。数据里 12、20、4 满足。
            //   ② n => n * n             —— 把通过筛选的值平方。
            //   ③ r => Console.WriteLine($"结果：{r}")
            //      —— 注意它的参数名是 r（结果 result），
            //         因为它拿到的是 transform 【处理之后】的值，不是原始值。
            //         Action<int> 没有返回值，所以 Lambda 体就是一句打印。
            Console.WriteLine("===== 场景 1：正偶数 → 平方 =====");
            ProcessData(data,
                n => n > 0 && n % 2 == 0,          // 筛选
                n => n * n,                        // 转换
                r => Console.WriteLine($"结果：{r}"));   // 输出

            // ★ 场景 2：调用的是【同一个】ProcessData，只换了三个 Lambda。
            //   筛选换成"只留负数"，转换换成取绝对值，输出格式也换了 ——
            //   ProcessData 一行都没动，这就是"可配置处理管道"。
            Console.WriteLine();
            Console.WriteLine("===== 场景 2：负数 → 绝对值 =====");
            ProcessData(data,
                n => n < 0,
                n => Math.Abs(n),
                r => Console.WriteLine($"[负数转正] {r}"));

            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
