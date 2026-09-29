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




    class Lesson12Practice
    {
        // ================================================================
        // ↓↓↓ TODO 5 用的方法写在这里 ↓↓↓
        // ================================================================




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




            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
