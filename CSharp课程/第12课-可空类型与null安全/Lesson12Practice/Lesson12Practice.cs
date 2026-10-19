// ============================================================================
//  第 12 课 · 练习：可空类型与 null 安全
//
//  ★ 本课重点：
//     1. int? 是 Nullable<int>（有 HasValue）；string? 只是编译期标记（没有 HasValue）
//     2. ?. 为 null 返回 null；?? 为 null 用默认值；??= 为 null 才赋值
//     3. required 强制初始化，把 null 隐患提前到编译期
// ============================================================================

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ TODO 4 用的类写在这里 ↓↓↓
    // ================================================================

    // ────────────────────────────────────────────────────────────────
    // TODO 4 ｜ Product：required（必填）+ string? / List<string>?（可选）
    // ────────────────────────────────────────────────────────────────
    // ★ 这个类定义本身就是本课的"活教材"：四个属性用了三种不同的"非空保证"方式，
    //   一眼就能看出 required、值类型、可空引用类型三者的分工。
    class Product
    {
        // ★ required = "创建对象时必须给这个属性赋值"，漏写是【编译错误】（CS9035），不是警告。
        //   它把"忘了初始化"从运行时崩溃提前到了编译期 —— 这正是本课说的"用编译器的力量抓 bug"。
        //   有了 required，编译器也不会再报 CS8618（"不可为 null 的属性在退出构造函数时必须含非 null 值"）。
        public required string Name { get; set; }

        // ★ decimal 是【值类型】，值类型天生不能为 null（默认值 0），
        //   所以它既不需要 required，也不存在"忘了填 → null"的隐患。
        //   （如果业务上真的允许"还没定价"，才写成 decimal? → Nullable<decimal>，那时读它就要判空了。）
        public decimal Price { get; set; }

        // ★ string? 是【可空引用类型】：明确声明"这个字段可以不填"。
        //   不填时它的值是 null，所以读取它的时候要配合 ?? 给一个显示用的默认值。
        public string? Description { get; set; }

        // ★ List<string>? 同理：整个列表本身可以是 null。
        //   ⚠️ 两个 ? 的位置含义完全不同，别混：
        //        List<string>?  → 列表本身可能是 null（这里是这种）
        //        List<string?>  → 列表一定是对象，但里面的元素可能是 null
        //   用 ??= 延迟初始化它，就能省掉"用之前先判空"的 if。
        public List<string>? Tags { get; set; }
    }




    class Lesson12Practice
    {
        // ================================================================
        // ↓↓↓ TODO 5 用的方法写在这里 ↓↓↓
        // ================================================================

        // ────────────────────────────────────────────────────────────
        // TODO 5 ｜ 健壮的配置读取：把"键不存在"和"值是 null"分开处理
        // ────────────────────────────────────────────────────────────
        // ★ 参数类型里的两处 ? 含义不同，值得逐字读一遍：
        //     Dictionary<string, string?>     → 键是非空 string，【值】可能是 null
        //     string key / string defaultValue → 普通参数，调用方保证传进来的是非 null
        //   static 是因为它不依赖任何对象状态，跟着类走就行（第 06 课）。
        static string GetConfig(Dictionary<string, string?> config,
                                string key, string defaultValue)
        {
            // ★ TryGetValue 一次调用回答两个问题：
            //     "这个 key 在不在？" → 返回值 bool
            //     "在的话值是什么？"   → out 参数 value
            //   比先 ContainsKey 再 config[key] 少查一遍表，而且不会抛 KeyNotFoundException（第 09 课）。
            //   ★ out 参数写成 string? 而不是 string：因为这份配置里的值【真的可能是 null】；
            //     写成 string 会得到 CS8600 警告（把可能为 null 的值赋给"非空"变量）。
            if (!config.TryGetValue(key, out string? value))
            {
                // 【第 1 层】键根本不存在 → 连值都没有 → 用 defaultValue
                return defaultValue;
            }

            // 【第 2 层】键存在，但值本身还可能为 null：
            //   · 值是 null   → 用 defaultValue（"存在但没填"）
            //   · 值不是 null → 直接用那个值
            //   ★ 一个 ?? 就把这两种情况收拢在一行里 —— 这就是本课的考核点：
            //     初学者最容易把"键不存在"和"键存在但值为 null"当成同一件事。
            return value ?? defaultValue;
        }




        static void Main()
        {
            Console.WriteLine("===== 第 12 课练习：可空类型与 null 安全 =====\n");

            // ================================================================
            // TODO 1 ｜ 值类型的可空版本
            // ================================================================
            // 【要求】
            //   1. 声明 int? a = null; 和 int? b = 42;
            //   2. 打印它们的 HasValue 和（如果有值）Value
            //   3. 对 a 调用 .Value，用 try-catch 捕获异常并打印异常类型
            //   4. 用 is null 判断 a 和 b，对比 HasValue 的写法
            //
            // 【预期输出】
            //     a.HasValue = False
            //     b.HasValue = True，b.Value = 42
            //     对 a.Value 取值 → InvalidOperationException
            //     用 is null 判断：a 是 null？True；b 是 null？False
            //
            // 【提示】
            //     int? a = null;
            //     int? b = 42;
            //     Console.WriteLine($"a.HasValue = {a.HasValue}");
            //     Console.WriteLine($"b.HasValue = {b.HasValue}，b.Value = {b.Value}");
            //
            //     try { int x = a.Value; }
            //     catch (InvalidOperationException ex)
            //     { Console.WriteLine($"对 a.Value 取值 → {ex.GetType().Name}"); }
            //
            // 【考察点】值类型的 ? 是 Nullable<T>，有 HasValue / Value

            Console.WriteLine("--- TODO 1 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① int? 就是 Nullable<int>：一个真正的 struct，里面装了"一个 bool + 一个 int"。
            //   ★ 正因为它是"包了一层"的包装类型，才会有 HasValue / Value 这两个成员 ——
            //     这是值类型独有的待遇，引用类型的 ? 没有（TODO 3 会亲眼验证）。
            int? a = null;
            int? b = 42;

            // ② HasValue 问的是"盒子里到底有没有值"。a 是 null → False
            Console.WriteLine($"a.HasValue = {a.HasValue}");

            // ③ b 有值：HasValue 是 True，取出来就是 42。
            //   ★ 这里先把 b.Value 取出来放进一个 int 变量，再拼进插值字符串 ——
            //     如果直接写 $"...{b.Value}"，编译器会怀疑"万一 b 是 null 呢"，可能报 CS8629。
            //     先取成非空的 int，编译器就彻底放心了。
            int bValue = b.Value;
            Console.WriteLine($"b.HasValue = {b.HasValue}，b.Value = {bValue}");

            // ④ 对 null 的 Nullable<T> 取 .Value 会抛 InvalidOperationException
            //   （注意不是 NullReferenceException —— 因为 a 这个结构体本身是存在的，只是里面没装值）。
            //   ★ 亲手让异常抛一次，比背结论记得牢。
            try
            {
#pragma warning disable CS8629
                // ★ 这一行编译器会警告 CS8629"可为 null 的值类型可能为 null" ——
                //   它恰好证明编译器能提前发现这个坑。我们现在就是要故意踩它，所以临时关掉警告。
                int x = a.Value;
#pragma warning restore CS8629

                // ★ 上一行已经抛异常了，这一行【永远执行不到】；
                //   留着它只是为了"用一下" x，避免出现"变量赋值后从未使用"的提示。
                Console.WriteLine($"不该看到这行：{x}");
            }
            catch (InvalidOperationException ex)
            {
                // ex.GetType().Name 拿到的是类型名（不带命名空间）→ InvalidOperationException
                Console.WriteLine($"对 a.Value 取值 → {ex.GetType().Name}");
            }

            // ⑤ 对比两种判空写法：
            //     a.HasValue → 只有 Nullable<T>（值类型）有
            //     a is null  → 值类型、引用类型【通用】（引用类型只能用它）
            //   ★ 所以养成写 is null / is not null 的习惯：将来把 int? 换成 string? 时不用改代码。
            Console.WriteLine($"用 is null 判断：a 是 null？{a is null}；b 是 null？{b is null}");




            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ 引用类型的 ? 与三个运算符
            // ================================================================
            // 【要求】给定两个字符串（一个为 null，一个有值）：
            //     string? s1 = null;
            //     string? s2 = "  hello  ";
            //
            //   对每个都演示：
            //     ① ?. 安全访问长度
            //     ② ?. 链式调用（ToUpper()?.Trim()）
            //     ③ ?? 提供默认值
            //     ④ ??= 赋值（只对 null 的那个生效）
            //
            // 【预期输出】
            //     s1?.Length          = null（不崩溃）
            //     s2?.Length          = 9
            //     s1?.ToUpper()?.Trim() = null
            //     s2?.ToUpper()?.Trim() = HELLO
            //     s1 ?? "默认值"       = 默认值
            //     s1 ??= "赋值后的值" 之后，s1 = 赋值后的值
            //     s1 ??= "再赋一次" 之后，s1 = 赋值后的值   ← 不是 null 所以没变
            //
            // 【提示】
            //     Console.WriteLine($"s1?.Length = {(s1?.Length is null ? "null（不崩溃）" : s1.Length.ToString())}");
            //     s1 ??= "赋值后的值";
            //     Console.WriteLine($"s1 = {s1}");
            //
            // 【考察点】
            //   · string? 没有 HasValue，要用 is null 判断
            //   · ??= 只在当前值是 null 时才赋值

            Console.WriteLine("--- TODO 2 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 两个变量都声明成 string?（可空引用类型）：
            //   ★ 它【不是】新类型，运行时就是 string，只是给编译器打了个标记"它可能为 null"。
            //     所以判空只能用 is null / ?. / ??，不能用 HasValue（string? 根本没有这个属性）。
            //   （如果写成 string s1 = null; 本课开启的 NRT 会给出 CS8600 警告。）
            string? s1 = null;
            string? s2 = "  hello  ";

            // ② ?. 空条件访问：左边是 null 时【整个表达式短路成 null】，压根不会去碰 .Length，所以不崩。
            //   ★ 返回类型也自动变成 int?（"可能是 null"），所以打印前用三元把 null 显示成文字，
            //     否则直接打印 null 只会看到一片空白，看不出效果。
            //   ★ 三元里走的是 s1.Length（不是 s1?.Length）：因为"s1?.Length is null"为假
            //     已经让编译器推出 s1 非 null 了 —— 这就是 null 状态分析跟着条件推理。
            Console.WriteLine($"s1?.Length          = {(s1?.Length is null ? "null（不崩溃）" : s1.Length.ToString())}");
            Console.WriteLine($"s2?.Length          = {s2?.Length}");

            // ③ ?. 链式调用：s1?.ToUpper()?.Trim()
            //   ★ 链上【任何一环】是 null，整条链就返回 null，后面的调用统统跳过 ——
            //     "一处为 null，全程不崩"，这就是链式 ?. 的价值（Python 里得写成两层条件表达式）。
            string? upper1 = s1?.ToUpper()?.Trim();
            string? upper2 = s2?.ToUpper()?.Trim();
            Console.WriteLine($"s1?.ToUpper()?.Trim() = {(upper1 is null ? "null" : upper1)}");
            Console.WriteLine($"s2?.ToUpper()?.Trim() = {upper2}");

            // ④ ?? 空合并：左边是 null 就用右边的默认值。
            //   ★ 它只"读出一个值"，【不会】修改 s1 —— 这一点正好和下面的 ??= 相反。
            Console.WriteLine($"s1 ?? \"默认值\"       = {s1 ?? "默认值"}");

            // ⑤ ??= 空合并赋值：只在左边【当前是 null】时才写进去，
            //   等价于 if (s1 is null) { s1 = "赋值后的值"; }，但 s1 只会被求值一次。
            //   这是"延迟初始化"的常用写法：第一次真正要用的时候才创建。
            s1 ??= "赋值后的值";
            Console.WriteLine($"s1 ??= \"赋值后的值\" 之后，s1 = {s1}");

            // ⑥ 再赋值一次：此时 s1 已经不是 null，所以 ??= 什么都不做（值没变）。
            //   ★ 一句话记住：?? 是"读的时候兜底"，??= 是"只在为 null 时写"。
            s1 ??= "再赋一次";
            Console.WriteLine($"s1 ??= \"再赋一次\" 之后，s1 = {s1}   ← 不是 null 所以没变");




            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ 对照实验：int? 和 string? 的区别
            // ================================================================
            // 【要求】写一段代码，用【注释】回答：
            //   1. int? 能不能调用 .HasValue？为什么？
            //   2. string? 能不能调用 .HasValue？为什么？
            //   3. 试着写一句 string? s = null; Console.WriteLine(s.HasValue);
            //      看编译器报什么错，把错误信息记在注释里
            //
            // 【预期输出】
            //     int?  a = null;  a.HasValue = False        ← 能调用
            //     string? s = null; is null = True           ← 只能用 is null
            //     结论：值类型的 ? 包了一层 Nullable<T>，引用类型的 ? 只是标记
            //
            // 【提示】
            //     把下面这行取消注释，看报错：
            //     // string? s = null; Console.WriteLine(s.HasValue);
            //
            //     错误大概是：CS1061: "string"未包含"HasValue"的定义
            //
            // 【考察点】这就是你第 1 周踩过的坑（疑问与踩坑记录 #03）

            Console.WriteLine("--- TODO 3 ---");
            // ↓↓↓ 在这里写你的代码 + 注释 ↓↓↓

            // 【问题 1】int? 能调用 .HasValue 吗？—— 能。
            //   因为 int? 是 Nullable<int>（一个 struct），而 Nullable<T> 上就定义了
            //   HasValue / Value 这两个成员，所以编译器和运行时都认得它们。
            int? valueType = null;                    // 对应预期输出里的 a
            Console.WriteLine($"int?  a = null;  a.HasValue = {valueType.HasValue}        ← 能调用");

            // 【问题 2】string? 能调用 .HasValue 吗？—— 不能。
            //   因为 string? 在运行时【就是 string】，? 只是编译期的可空标记，没有包任何东西，
            //   自然也就没有 HasValue。引用类型判空只能用 is null / is not null。
            string? refType = null;                   // 对应预期输出里的 s
            Console.WriteLine($"string? s = null; is null = {refType is null}           ← 只能用 is null");

            // 【问题 3】把下面这行取消注释，编译器的报错是：
            //   CS1061: "string"未包含"HasValue"的定义，并且找不到可接受第一个"string"类型参数的
            //           可访问扩展方法"HasValue"（是否缺少 using 指令或程序集引用?）
            //   ★ 注意报错里写的是 "string" 而不是 "string?" ——
            //     这恰好证明了：string? 和 string 是【同一个类型】，? 只影响编译期的检查。
            // string? s = null; Console.WriteLine(s.HasValue);      // ❌ 编译错误：CS1061

            // 【顺带认识第 4 种消除警告的写法】! （null-forgiving，"原谅操作符"）
            //   它只是让编译器闭嘴，运行时【什么检查都不做】：
            //       string s2 = refType!;              // 编译通过，没有警告
            //       Console.WriteLine(s2.Length);      // 但 refType 真的是 null → NullReferenceException
            //   ★ 所以它是反模式：等于把编译器的保护关掉，bug 只是从"编译期提醒"推迟到"运行时崩溃"。
            //     本课要求认识它、看懂它，但不鼓励在练习里用它。

            // 【结论】
            //   值类型的 ?  = 包了一层 Nullable<T>（多存了一个 bool）→ 有 HasValue / Value
            //   引用类型的 ? = 只是"打个标记"（运行时零开销）      → 只有 is null 能用
            //   ★ 回想第 01 课：把 SPoint 从 struct 改成 class 之后，SPoint? 突然没有 HasValue 了，
            //     原因就在这里 —— ? 的含义是跟着类型走的，不是"."后面想点什么就有什么。
            Console.WriteLine("结论：值类型的 ? 包了一层 Nullable<T>，引用类型的 ? 只是标记");




            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ required 和可选字段
            // ================================================================
            // 【要求】
            //   1. 在上面定义：
            //        class Product
            //        {
            //            public required string Name { get; set; }      // 必填
            //            public decimal Price { get; set; }              // 必填（值类型不用 required）
            //            public string? Description { get; set; }        // 可选
            //            public List<string>? Tags { get; set; }         // 可选
            //        }
            //   2. 创建两个 Product：
            //        · 一个填全（含描述和标签）
            //        · 一个只填必填项
            //   3. 用 ?. ?? ??= 安全打印所有字段
            //
            // 【预期输出】
            //     ===== 商品 1 =====
            //     名称：iPhone
            //     价格：6999.00
            //     描述：最新款手机
            //     标签数：2
            //     ===== 商品 2 =====
            //     名称：iPad
            //     价格：3999.00
            //     描述：（暂无描述）
            //     标签数：0          ← 用 ??= 延迟初始化成空列表
            //
            // 【提示】
            //     var p1 = new Product
            //     {
            //         Name = "iPhone",
            //         Price = 6999,
            //         Description = "最新款手机",
            //         Tags = new List<string> { "电子产品", "手机" }
            //     };
            //
            //     var p2 = new Product { Name = "iPad", Price = 3999 };
            //
            //     // 打印时：
            //     Console.WriteLine($"描述：{p.Description ?? "（暂无描述）"}");
            //     p.Tags ??= new List<string>();          // ★ 延迟初始化
            //     Console.WriteLine($"标签数：{p.Tags.Count}");
            //
            //   ⚠️ 试着写 var bad = new Product(); 看编译错误（required 的作用）
            //
            // 【考察点】
            //   · required 让"忘填必填项"变成编译错误
            //   · string? 声明可选字段，用 ?? 给显示用的默认值
            //   · List? 用 ??= 延迟初始化，避免"用之前还要判空"

            Console.WriteLine("--- TODO 4 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 填全的那个：required 的 Name 必须写；Price 是值类型（不写就是 0）；
            //   两个"可空"字段这次都给了值。
            //   ★ 对象初始化器里的 Name = "iPhone" 就是"创建时赋值"，正好满足 required 的要求。
            var p1 = new Product
            {
                Name = "iPhone",
                Price = 6999,                                  // int 常量 → 隐式转成 decimal，不用写 6999m
                Description = "最新款手机",
                Tags = new List<string> { "电子产品", "手机" }   // 2 个标签
            };

            // ② 只填必填项的那个：Description / Tags 都没写 → 它们的值就是 null。
            //   ★ 如果照着上面【要求】里那句提示去写 var bad = new Product();，编译器会报
            //     CS9035: 必须设置必需成员 "Product.Name"。—— required 的价值就在这里：
            //     忘填必填项根本通不过编译，而不是拖到运行时才 NullReferenceException。
            var p2 = new Product { Name = "iPad", Price = 3999 };

            // ③ 打印商品 1：四个字段分别演示"直接用 / 格式化 / ?? / ??="
            Console.WriteLine("===== 商品 1 =====");
            Console.WriteLine($"名称：{p1.Name}");                     // required 保证非 null → 直接用，不需要 ?
            Console.WriteLine($"价格：{p1.Price:F2}");                // F2 = 保留 2 位小数 → 6999.00
            Console.WriteLine($"描述：{p1.Description ?? "（暂无描述）"}");   // 有值 → 用左边的值
            p1.Tags ??= new List<string>();                           // 已经有标签，所以这行【什么也不做】
            Console.WriteLine($"标签数：{p1.Tags.Count}");            // ??= 之后编译器确定非 null，能放心 .Count

            // ④ 打印商品 2：字段全空，正好检验三种运算符的组合用法
            Console.WriteLine("===== 商品 2 =====");
            Console.WriteLine($"名称：{p2.Name}");
            Console.WriteLine($"价格：{p2.Price:F2}");
            Console.WriteLine($"描述：{p2.Description ?? "（暂无描述）"}");   // null → 用右边的默认文案
            p2.Tags ??= new List<string>();                           // ★ 延迟初始化：第一次要用它时才创建空列表
            Console.WriteLine($"标签数：{p2.Tags.Count}          ← 用 ??= 延迟初始化成空列表");

            // ★ 小结：这一段打印里【没有一句】if (x != null)，但一个空引用异常都不会有 ——
            //   required 把"必填"交给编译器保证，?? 给显示兜一个默认值，
            //   ??= 顺手把可能为 null 的集合延迟初始化。这就是"用类型系统消灭判空嵌套"。




            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ ★ 综合：健壮的配置读取（本课考核）
            // ================================================================
            // 【背景】从"可能缺失的配置"里安全地取值，是真实项目的常见需求。
            //        下面用一个 Dictionary 模拟配置。
            //
            // 【要求】写一个方法：
            //     static string GetConfig(Dictionary<string, string?> config,
            //                             string key, string defaultValue)
            //   行为：
            //     1. 如果 key 不存在 → 返回 defaultValue
            //     2. 如果 key 存在但值是 null → 返回 defaultValue
            //     3. 如果 key 存在且值非 null → 返回那个值
            //
            //   然后用这个配置测试：
            //     var config = new Dictionary<string, string?>
            //     {
            //         ["appName"] = "我的应用",
            //         ["version"] = null,              // 存在但值为 null
            //         // "author" 这个 key 完全不存在
            //     };
            //
            //   测试 3 种情况并打印
            //
            // 【预期输出】
            //     appName  → 我的应用
            //     version  → （未配置）
            //     author   → （未配置）
            //
            // 【提示】
            //     static string GetConfig(Dictionary<string, string?> config,
            //                             string key, string defaultValue)
            //     {
            //         // TryGetValue 拿到值（可能是 null），拿不到就用 defaultValue
            //         if (!config.TryGetValue(key, out string? value))
            //         {
            //             return defaultValue;
            //         }
            //         // value 可能是 null，用 ?? 兜底
            //         return value ?? defaultValue;
            //     }
            //
            //     // 调用
            //     Console.WriteLine($"appName  → {GetConfig(config, "appName", "（未配置）")}");
            //     Console.WriteLine($"version  → {GetConfig(config, "version", "（未配置）")}");
            //     Console.WriteLine($"author   → {GetConfig(config, "author", "（未配置）")}");
            //
            // 【考察点】
            //   · TryGetValue 处理"键不存在"（第 09 课）
            //   · ?? 处理"值是 null"（本课）
            //   · ★ 两层可能性的区别：键不存在 vs 键存在但值为 null
            //     —— 很多初学者会把这两种混为一谈
            //   · Dictionary<string, string?> 里的两个 ? 位置含义不同：
            //       第一个（string）是值类型参数
            //       第二个（string?）表示"值可以是 null"

            Console.WriteLine("--- TODO 5 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 用集合初始化器造一份"可能缺项的配置"（第 09 课的 Dictionary）
            //   ★ 泛型参数写的是 string? —— 它明确告诉编译器"这份配置里的【值】允许是 null"。
            //     如果写成 Dictionary<string, string>，那 ["version"] = null 会得到 CS8625 警告，
            //     下面的 value ?? defaultValue 在编译器眼里也会变得"多余"。
            var config = new Dictionary<string, string?>
            {
                ["appName"] = "我的应用",
                ["version"] = null,              // 情况 2：键存在，但值是 null
                // "author" 这个 key 完全不存在（情况 1：连键都没有）
            };

            // ② 三种情况各测一次，看 GetConfig 是不是都兜得住
            //   ★ 两种"拿不到值"的成因完全不同，但对外都该给出默认值：
            //       情况 1：键不存在   → TryGetValue 返回 false → 走第一个 return
            //       情况 2：值是 null  → TryGetValue 返回 true，但 value 是 null → 走 ??
            //     这正是本课的考核点：初学者常把这两件事当成同一件事。
            Console.WriteLine($"appName  → {GetConfig(config, "appName", "（未配置）")}");
            Console.WriteLine($"version  → {GetConfig(config, "version", "（未配置）")}");
            Console.WriteLine($"author   → {GetConfig(config, "author", "（未配置）")}");

            // ★ 小结：GetConfig 内部只有"一次 TryGetValue + 一个 ??"，
            //   却把三种情况全兜住了：键不存在 / 值是 null / 值正常。
            //   写 null 安全代码的思路就是这样 —— 不指望调用方自觉，
            //   而是先让类型把"可能没有值"这件事写清楚，再用运算符一次性收干净。




            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
