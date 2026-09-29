// ============================================================================
//  第 01 课 · 示例代码
//  变量与类型 —— 每一行都有注释，请逐行读完再运行
//
//  运行方法：
//    Rider 右侧 Solution 面板 → 右键 Lesson01Demo → 设为启动项目 → Ctrl + F5
// ============================================================================

namespace Lesson01Demo
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("========== 第 01 课：变量与类型 ==========");
            Console.WriteLine();

            // ================================================================
            // 演示 1：变量的声明 —— 类型 名字 = 值;
            // ================================================================
            Console.WriteLine("---- 演示 1：声明变量 ----");

            // 完整写法：类型 变量名 = 初始值;
            int age = 20;
            string name = "qzl";
            bool isStudent = true;
            char grade = 'A';              // 注意：char 用【单引号】

            Console.WriteLine($"int    age       = {age}");
            Console.WriteLine($"string name      = {name}");
            Console.WriteLine($"bool   isStudent = {isStudent}");
            Console.WriteLine($"char   grade     = {grade}");
            Console.WriteLine();

            // ================================================================
            // 演示 2：各种数字类型的范围
            // ================================================================
            Console.WriteLine("---- 演示 2：数字类型能存多大 ----");

            // 每个数字类型都有一个 MaxValue（最大值）和 MinValue（最小值）
            // 这是 C# 内置的常量，直接用 类型名.MaxValue 就能拿到
            Console.WriteLine($"int   范围: {int.MinValue} ~ {int.MaxValue}");
            Console.WriteLine($"long  范围: {long.MinValue} ~ {long.MaxValue}");
            Console.WriteLine();

            // ★ 这是 Python 程序员必踩的坑：int 会【溢出】
            int maxInt = int.MaxValue;
            Console.WriteLine($"int 最大值      = {maxInt}");
            Console.WriteLine($"int 最大值 + 1  = {maxInt + 1}   ← 溢出变成了负数！");
            Console.WriteLine("  Python 的 int 是任意精度，不会溢出；C# 的 int 只有 32 位，会。");
            Console.WriteLine();

            // ================================================================
            // 演示 3：double vs decimal —— 算钱必须用 decimal
            // ================================================================
            Console.WriteLine("---- 演示 3：double 和 decimal 的区别 ----");

            // double：二进制浮点，字面量不加后缀
            double d = 0.1 + 0.2;
            Console.WriteLine($"double  0.1 + 0.2 = {d}");
            Console.WriteLine("  ↑ 不是 0.3！因为 0.1 在二进制里是无限循环小数，只能近似存");

            // decimal：十进制浮点，字面量要加 m 后缀
            decimal m = 0.1m + 0.2m;
            Console.WriteLine($"decimal 0.1 + 0.2 = {m}");
            Console.WriteLine("  ↑ 精确的 0.3，因为它是按十进制存的");
            Console.WriteLine();

            // 再举一个更直观的例子：算钱（这是真正会让你踩坑的场景）
            decimal unitPrice = 4.35m;

            // 场景：单价 4.35 元，卖 100 件，应该收多少钱？
            // 正确答案显然是 435.00 元
            double  totalByDouble  = 4.35  * 100;
            decimal totalByDecimal = unitPrice * 100;

            Console.WriteLine("场景：单价 4.35 元的商品，卖 100 件，应收多少钱？");
            Console.WriteLine($"  用 double  算 = {totalByDouble}");
            Console.WriteLine($"  用 decimal 算 = {totalByDecimal}");
            Console.WriteLine("  ↑ double 算出了 434.99999999999994，少了！这就是真实的金钱计算错误。");
            Console.WriteLine();

            // 再看一个：累加 10 次 0.1，答案应该是 1.0
            double sumD = 0;
            decimal sumM = 0;
            for (int i = 0; i < 10; i++)
            {
                sumD += 0.1;
                sumM += 0.1m;
            }
            Console.WriteLine("场景：把 0.1 累加 10 次，答案应该是 1.0");
            Console.WriteLine($"  用 double  累加 = {sumD}");
            Console.WriteLine($"  用 decimal 累加 = {sumM}");
            Console.WriteLine("  ↑ 又是 double 算错。误差会随着交易次数不断累积。");
            Console.WriteLine();

            Console.WriteLine("  ★ 规则：算钱【必须】用 decimal，这不是风格问题，是真会算错。");
            Console.WriteLine("    double 适合：科学计算、物理、图形（小幅误差无所谓，但快得多）");
            Console.WriteLine();

            // ================================================================
            // 演示 4：var —— 让编译器推断类型（不是动态类型！）
            // ================================================================
            Console.WriteLine("---- 演示 4：var 类型推断 ----");

            var v1 = 20;              // 编译器推断成 int
            var v2 = "hello";         // 推断成 string
            var v3 = 3.14;            // 推断成 double
            var v4 = 3.14m;           // 推断成 decimal（因为有 m）
            var v5 = 'X';             // 推断成 char
            var v6 = true;            // 推断成 bool

            // GetType().Name 能在运行时拿到变量的真实类型名
            Console.WriteLine($"var v1 = 20;      → 类型是 {v1.GetType().Name}");
            Console.WriteLine($"var v2 = \"hello\"; → 类型是 {v2.GetType().Name}");
            Console.WriteLine($"var v3 = 3.14;    → 类型是 {v3.GetType().Name}");
            Console.WriteLine($"var v4 = 3.14m;   → 类型是 {v4.GetType().Name}");
            Console.WriteLine($"var v5 = 'X';     → 类型是 {v5.GetType().Name}");
            Console.WriteLine($"var v6 = true;    → 类型是 {v6.GetType().Name}");
            Console.WriteLine();

            // ★★★ 关键：var 只是"少打字"，类型在编译时就已经锁死了
            //     下面这行如果取消注释，会【编译错误】：
            // v2 = 123;         // ❌ 无法将 int 隐式转换为 string
            Console.WriteLine("★ var 不是动态类型！v2 已经被推断为 string，不能再装数字。");
            Console.WriteLine("  它和 Python 的 x = 20; x = \"hello\" 完全是两回事。");
            Console.WriteLine();

            // ================================================================
            // 演示 5：类型转换
            // ================================================================
            Console.WriteLine("---- 演示 5：类型转换 ----");

            // ① 小范围 → 大范围：自动转换（隐式转换），安全
            int small = 100;
            long big = small;              // ✅ 自动
            double asDouble = small;       // ✅ 自动
            Console.WriteLine($"int 100 → long   = {big}");
            Console.WriteLine($"int 100 → double = {asDouble}");
            Console.WriteLine("  ↑ 小范围转大范围，编译器自动处理");

            // ② 大范围 → 小范围：必须显式写转换，可能丢数据
            double pi = 3.99;
            // int wrong = pi;             // ❌ 取消注释会编译错误
            int truncated = (int)pi;       // ✅ 必须写 (int)
            Console.WriteLine($"double 3.99 → (int) = {truncated}   ← 直接砍掉小数，不是四舍五入");
            int rounded = (int)Math.Round(pi);
            Console.WriteLine($"double 3.99 → Math.Round 再转 = {rounded}   ← 这才是四舍五入");
            Console.WriteLine();

            // ③ 字符串 → 数字：用 TryParse（安全）而不是 Parse（会崩）
            Console.WriteLine("  字符串转数字：");
            string goodInput = "123";
            string badInput = "abc";

            // Parse：失败会【抛异常】，程序崩溃
            // int p = int.Parse(badInput);    // ❌ 取消注释就会崩

            // TryParse：失败返回 false，不崩溃 —— 推荐写法
            if (int.TryParse(goodInput, out int r1))
                Console.WriteLine($"    int.TryParse(\"123\") 成功，结果是 {r1}");
            else
                Console.WriteLine("    转换失败");

            if (int.TryParse(badInput, out int r2))
                Console.WriteLine($"    int.TryParse(\"abc\") 成功，结果是 {r2}");
            else
                Console.WriteLine($"    int.TryParse(\"abc\") 失败！r2 的值是 {r2}（失败时 out 参数是默认值 0）");
            Console.WriteLine();

            // ================================================================
            // 演示 6：常量 const
            // ================================================================
            Console.WriteLine("---- 演示 6：常量 const ----");

            const double Pi = 3.14159;      // 常量：一旦赋值不能改
            // Pi = 3;                      // ❌ 取消注释会编译错误

            double radius = 5;
            double area = Pi * radius * radius;
            Console.WriteLine($"半径 {radius} 的圆面积 = {area:F4}");
            Console.WriteLine("  const 的命名惯例是 PascalCase（Pi 而不是 PI）；");
            Console.WriteLine("  Python 没有真常量，只能靠全大写命名约定。");
            Console.WriteLine();

            // ================================================================
            // 演示 7：插值格式化
            // ================================================================
            Console.WriteLine("---- 演示 7：插值格式化 $ 加上 {值:格式} ----");

            double price = 12.5;
            double rate = 0.1234;
            int population = 1234567;

            Console.WriteLine($"{price:F2}        ← F2 = 保留 2 位小数");
            Console.WriteLine($"{price:F5}        ← F5 = 保留 5 位小数");
            Console.WriteLine($"{rate:P1}         ← P1 = 百分比，1 位小数");
            Console.WriteLine($"{population:N0}      ← N0 = 千分位分隔，0 位小数");
            Console.WriteLine($"{42,10}|        ← ,10 = 右对齐占 10 格");
            Console.WriteLine($"{42,-10}|        ← ,-10 = 左对齐占 10 格");
            Console.WriteLine($"{3.14159:F3}       ← 会自动四舍五入");
            Console.WriteLine();

            // ================================================================
            // 演示 8：字符串的常用操作
            // ================================================================
            Console.WriteLine("---- 演示 8：字符串常用操作 ----");

            string s = "Hello World";
            Console.WriteLine($"s                  = \"{s}\"");
            Console.WriteLine($"s.Length           = {s.Length}        ← 长度（不是 len(s)）");
            Console.WriteLine($"s.ToUpper()        = {s.ToUpper()}");
            Console.WriteLine($"s.ToLower()        = {s.ToLower()}");
            Console.WriteLine($"s.Contains(\"World\") = {s.Contains("World")}");
            Console.WriteLine($"s.Replace(...)     = {s.Replace("World", "C#")}");
            Console.WriteLine($"s.Substring(0, 5)  = {s.Substring(0, 5)}   ← 从 0 开始取 5 个");
            Console.WriteLine($"s[0]               = {s[0]}             ← 取第 0 个字符");
            Console.WriteLine($"s.IndexOf(\" \")     = {s.IndexOf(' ')}        ← 空格在第几位");
            Console.WriteLine();

            // ================================================================
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson01Practice.cs");
            Console.WriteLine("⚠️ 记得先把 LessonPractice.csproj 里的 StartupObject");
            Console.WriteLine("   从 Lesson00Practice 改成 Lesson01Practice");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
