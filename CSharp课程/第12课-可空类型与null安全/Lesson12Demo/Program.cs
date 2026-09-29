// ============================================================================
//  第 12 课 · 示例代码
//  可空类型与 null 安全
//  ★ 回答第 01 课埋的伏笔：为什么 SPoint? 没有 HasValue
// ============================================================================

namespace Lesson12Demo
{
    // 演示 required
    class UserProfile
    {
        public required string UserName { get; set; }      // ★ required：创建时必须赋值
        public string? NickName { get; set; }              // 可选
        public string? Email { get; set; }                 // 可选
        public List<string>? Tags { get; set; }            // 可选
    }

    // 演示 CS8618 警告（故意不初始化）
    class BadStudent
    {
        public string Name { get; set; } = null!;          // ! 让编译器闭嘴（但这是我们手动保证的）
        public int Age { get; set; }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("========== 第 12 课：可空类型与 null 安全 ==========\n");


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 1：★ 值类型的 ? 是 Nullable<T> ----");

            int? a = null;
            int? b = 42;
            int bValue = b.Value;                       // ★ 先取出来再拼字符串
                                                        //   否则 $"...{b.Value}" 会触发 CS8629
                                                        //   （编译器认为"可为 null 的值类型可能为 null"）

            Console.WriteLine($"   int? a = null;");
            Console.WriteLine($"      a.HasValue = {a.HasValue}      ← 有 HasValue 属性");
            Console.WriteLine($"   int? b = 42;");
            Console.WriteLine($"      b.HasValue = {b.HasValue}");
            Console.WriteLine($"      b.Value    = {bValue}      ← 有 Value 属性");
            Console.WriteLine();

            try
            {
                // ★ 注意：这一行会触发警告 CS8629"可为 null 的值类型可为 null"。
                //   编译器在提醒你：a 是 null，取 .Value 会抛异常 —— 这正是我们要演示的！
                //   所以这里临时关掉警告。也顺便展示了"编译器能提前发现这个坑"。
#pragma warning disable CS8629
                int bad = a.Value;                      // a 是 null，会抛异常
#pragma warning restore CS8629
                Console.WriteLine($"   {bad}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"   对 null 取 .Value → {ex.GetType().Name}");
                Console.WriteLine($"      消息：{ex.Message}");
            }
            Console.WriteLine();

            Console.WriteLine("   ★ 编译器会对 a.Value 发出警告 CS8629，提醒你\"它可能是 null\"");
            Console.WriteLine("     两种消除警告的正确写法：");
            Console.WriteLine("        ① if (a.HasValue) { 用 a.Value }        ← 先判断");
            Console.WriteLine("        ② int v = a.GetValueOrDefault();        ← null 时给 0");
            Console.WriteLine($"     实测 ② ：a.GetValueOrDefault() = {a.GetValueOrDefault()}（a 是 null，返回默认值 0）");
            Console.WriteLine();

            Console.WriteLine("   ★ int? 其实是 Nullable<int> —— 一个真正的 struct：");
            Console.WriteLine("        struct Nullable<T> { bool hasValue; T value; }");
            Console.WriteLine("     所以它里面装了两样东西：一个 bool + 一个 int");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 2：★ 引用类型的 ? 只是标记（关键区别）----");

            string? s = null;
            string normal = "abc";
            int normalLength = normal.Length;           // 用一下，避免"变量未使用"警告

            Console.WriteLine($"   string? s = null;");
            Console.WriteLine($"      s is null = {s is null}       ← 用 is null 判断");
            Console.WriteLine($"   string normal = \"abc\";");
            Console.WriteLine($"      normal.Length = {normalLength}              ← 非空类型可以直接用，没有警告");
            Console.WriteLine($"   ★ 注意：string? 【没有】 HasValue 属性！");
            Console.WriteLine($"      （第 01 课你把 SPoint 从 struct 改成 class 后报的错，就是这个原因）");
            Console.WriteLine();
            Console.WriteLine("   ┌────────────────┬──────────────────────┬──────────────┐");
            Console.WriteLine("   │ 写法            │ 底层是什么            │ 有 HasValue？ │");
            Console.WriteLine("   ├────────────────┼──────────────────────┼──────────────┤");
            Console.WriteLine("   │ int?           │ Nullable<int>（struct）│ ✅ 有         │");
            Console.WriteLine("   │ string?        │ 就是 string           │ ❌ 没有       │");
            Console.WriteLine("   └────────────────┴──────────────────────┴──────────────┘");
            Console.WriteLine();
            Console.WriteLine("   ★ 口诀：值类型的 ? 是「包一层」，引用类型的 ? 是「打个标记」");
            Console.WriteLine("   ★ 引用类型的 ? 在运行时是【零开销】的");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 3：?. 空条件运算符 ----");

            string? name1 = null;
            string? name2 = "hello";

            Console.WriteLine($"   name1 = null，name1?.Length = {(name1?.Length is null ? "null（不崩溃）" : name1.Length.ToString())}");
            Console.WriteLine($"   name2 = \"hello\"，name2?.Length = {name2?.Length}");
            Console.WriteLine();

            // 链式调用
            string? result = name1?.ToUpper()?.Trim();
            Console.WriteLine($"   name1?.ToUpper()?.Trim() = {(result is null ? "null（链上任何一环是 null，整条链返回 null）" : result)}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 4：?? 空合并运算符 ----");

            string? input1 = null;
            string? input2 = "有值";

            Console.WriteLine($"   input1 ?? \"（未填写）\"  = {input1 ?? "（未填写）"}");
            Console.WriteLine($"   input2 ?? \"（未填写）\"  = {input2 ?? "（未填写）"}");
            Console.WriteLine();

            // 链式：取第一个非 null
            string? first = null;
            string? second = null;
            string? third = "第三个";
            Console.WriteLine($"   first ?? second ?? third ?? \"兜底\" = {first ?? second ?? third ?? "兜底"}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 5：??= 空合并赋值（延迟初始化）----");

            string? cache = null;
            Console.WriteLine($"   初始：cache = {(cache is null ? "null" : cache)}");

            cache ??= "第一次赋值";
            Console.WriteLine($"   cache ??= \"第一次赋值\" 之后：{cache}");

            cache ??= "第二次赋值";
            Console.WriteLine($"   cache ??= \"第二次赋值\" 之后：{cache}   ← 不是 null，所以没被覆盖");
            Console.WriteLine();

            // 典型用途：延迟创建集合
            List<string>? tags = null;
            tags ??= new List<string>();
            tags.Add("C#");
            Console.WriteLine($"   延迟初始化集合：tags.Count = {tags.Count}");
            Console.WriteLine("   ★ 这个模式在真实项目里非常常用（缓存、懒加载）");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 6：required（强制初始化）----");

            // 下面这行如果取消注释，会【编译错误】：
            // var bad = new UserProfile();          // ❌ 必须初始化 UserName

            var user = new UserProfile
            {
                UserName = "qzl",                   // ★ required，必须写
                // NickName 没写 → 是 null（因为声明成 string?）
            };

            Console.WriteLine($"   new UserProfile {{ UserName = \"qzl\" }} 创建成功");
            Console.WriteLine($"      UserName = {user.UserName}      ← required 保证非 null");
            Console.WriteLine($"      NickName = {(user.NickName is null ? "null（可选字段，没填）" : user.NickName)}");
            Console.WriteLine();

            Console.WriteLine("   用 ?. 和 ?? 安全处理可选字段：");
            Console.WriteLine($"      昵称显示为：{user.NickName ?? "（未设置）"}");
            Console.WriteLine($"      邮箱长度：{user.Email?.Length ?? 0}");
            user.Tags ??= new List<string>();
            Console.WriteLine($"      标签数：{user.Tags.Count}");
            Console.WriteLine();

            Console.WriteLine("   ★ 这段代码里没有一个 if (x != null) 嵌套，但完全不会崩");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 7：CS8618 警告（编译器预言 bug）----");

            var bs = new BadStudent { Age = 20 };
            Console.WriteLine($"   class BadStudent {{ public string Name {{ get; set; }} }}");
            Console.WriteLine($"   编译器会警告：CS8618 —— \"退出构造函数时 Name 可能是 null\"");
            Console.WriteLine($"   ★ 这个警告是真 bug 的预言，不是噪音");
            Console.WriteLine();
            Console.WriteLine("   三种修法：");
            Console.WriteLine("      ① public string Name { get; set; } = \"\";            ← 给默认值");
            Console.WriteLine("      ② 在构造函数里赋值");
            Console.WriteLine("      ③ public required string Name { get; set; }        ← 推荐");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 8：null 状态分析（编译器会跟着 if 推理）----");

            string? maybeNull = Console.ReadLine();       // 可能返回 null

            // 这里编译器知道 maybeNull 可能是 null
            Console.WriteLine($"   input?.Length = {(maybeNull?.Length is null ? "null" : maybeNull.Length.ToString())}");

            if (maybeNull is not null)
            {
                // ★ 在这个分支里，编译器【知道】它不是 null，所以不会警告
                Console.WriteLine($"   进入 if 分支，确定非 null：长度 = {maybeNull.Length}");
            }
            else
            {
                Console.WriteLine("   else 分支：输入是 null（直接回车或输入被重定向）");
            }
            Console.WriteLine();

            Console.WriteLine("   四种消除 null 警告的方式：");
            Console.WriteLine("      ① if (x is not null) { x.Length }       ← 推荐");
            Console.WriteLine("      ② x?.Length                             ← 推荐");
            Console.WriteLine("      ③ x ?? \"默认\"                           ← 推荐");
            Console.WriteLine("      ④ x!.Length                             ← 危险！只是让编译器闭嘴");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson12Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
