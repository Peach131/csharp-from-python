// ============================================================================
//  第 02 课 · 示例代码
//  运算符与流程控制 —— 每一行都有注释，请逐行读完再运行
//
//  运行方法：
//    Rider 右侧 Solution 面板 → 右键 Lesson02Demo → 设为启动项目 → Ctrl + F5
// ============================================================================

namespace Lesson02Demo
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("========== 第 02 课：运算符与流程控制 ==========");
            Console.WriteLine();

            // ================================================================
            // 演示 1：算术运算符 —— 注意整数除法的陷阱
            // ================================================================
            Console.WriteLine("---- 演示 1：算术运算符 ----");

            int a = 10;
            int b = 3;

            Console.WriteLine($"a = {a}, b = {b}");
            Console.WriteLine($"a + b = {a + b}     加法");
            Console.WriteLine($"a - b = {a - b}      减法");
            Console.WriteLine($"a * b = {a * b}     乘法");
            Console.WriteLine($"a / b = {a / b}      除法 ← 注意！不是 3.333");
            Console.WriteLine($"a % b = {a % b}      取余（余数）");
            Console.WriteLine();

            // ★★★ 陷阱 1：两个 int 相除，结果还是 int，小数被【砍掉】
            Console.WriteLine("  ★ 陷阱：整数除法");
            Console.WriteLine($"     7 / 2     = {7 / 2}       ← 两个 int，结果是 int");
            Console.WriteLine($"     7.0 / 2   = {7.0 / 2}     ← 有 double 参与，正常除法");
            Console.WriteLine($"     7 / 2.0   = {7 / 2.0}     ← 同上");
            Console.WriteLine($"     7m / 2    = {7m / 2}      ← decimal 也行");
            Console.WriteLine("     Python 里 7 / 2 = 3.5，C# 里 7 / 2 = 3 —— 规则正好相反！");
            Console.WriteLine();

            // 想得到 3.5 怎么办？显式转换其中一个
            int total = 7, count = 2;
            Console.WriteLine($"     想让 7/2 得到 3.5：(double)7 / 2 = {(double)total / count}");
            Console.WriteLine();

            // ★★★ 陷阱 2：负数除法 / 取余，和 Python 规则不同
            Console.WriteLine("  ★ 陷阱：负数除法和取余");
            Console.WriteLine($"     -7 / 2  →  C#: {-7 / 2}      Python: -4");
            Console.WriteLine($"     -7 % 3  →  C#: {-7 % 3}      Python: 2");
            Console.WriteLine("     C# 规则：除法【向零取整】，余数【符号跟左操作数】");
            Console.WriteLine("     Python 规则：除法【向下取整】，余数符号跟除数");
            Console.WriteLine();

            // ★★★ 陷阱 3：整数溢出
            Console.WriteLine("  ★ 陷阱：整数溢出");
            int maxInt = int.MaxValue;
            Console.WriteLine($"     int.MaxValue     = {maxInt}");
            Console.WriteLine($"     int.MaxValue + 1 = {maxInt + 1}   ← 溢出成负数，默认不报错");
            try
            {
                int crash = checked(maxInt + 1);      // checked 会检查溢出
                Console.WriteLine($"     checked: {crash}");
            }
            catch (OverflowException)
            {
                Console.WriteLine("     用 checked 包起来就会抛 OverflowException ✅");
            }
            Console.WriteLine("     Python 的 int 无限精度，永远不溢出；C# 的 int 只有 32 位。");
            Console.WriteLine();


            // ================================================================
            // 演示 2：取余的实用场景
            // ================================================================
            Console.WriteLine("---- 演示 2：取余 % 能干什么 ----");

            int n = 7;
            Console.WriteLine($"判断 {n} 是奇数还是偶数：");
            if (n % 2 == 0)
            {
                Console.WriteLine("  是偶数");
            }
            else
            {
                Console.WriteLine("  是奇数（因为 n % 2 == 1）");
            }
            Console.WriteLine();

            Console.WriteLine("找出 1~15 里所有 3 的倍数：");
            Console.Write("  ");
            for (int i = 1; i <= 15; i++)
            {
                if (i % 3 == 0)
                {
                    Console.Write($"{i} ");
                }
            }
            Console.WriteLine();
            Console.WriteLine();


            // ================================================================
            // 演示 3：赋值运算符和自增自减
            // ================================================================
            Console.WriteLine("---- 演示 3：赋值运算符 ----");

            int x = 10;
            Console.WriteLine($"初始 x = {x}");
            x += 5;  Console.WriteLine($"x += 5  →  {x}     （等价 x = x + 5）");
            x -= 3;  Console.WriteLine($"x -= 3  →  {x}     （等价 x = x - 3）");
            x *= 2;  Console.WriteLine($"x *= 2  →  {x}     （等价 x = x * 2）");
            x /= 4;  Console.WriteLine($"x /= 4  →  {x}     （等价 x = x / 4）");
            x %= 4;  Console.WriteLine($"x %= 4  →  {x}     （等价 x = x % 4）");
            Console.WriteLine();

            // 前置 ++ 和后置 ++ 的区别
            int p = 5;
            int q = p++;                    // 后置：先把 p 的旧值给 q，然后 p 自己加 1
            Console.WriteLine($"int q = p++;  →  p = {p}, q = {q}     （q 拿到旧值 5，p 变成 6）");

            int r = 5;
            int s = ++r;                    // 前置：r 先加 1，再把新值给 s
            Console.WriteLine($"int s = ++r;  →  r = {r}, s = {s}     （r 先变 6，s 拿到 6）");
            Console.WriteLine("  ★ 建议：把 ++ 单独写一行，不要嵌在表达式里，避免混淆");
            Console.WriteLine();


            // ================================================================
            // 演示 4：比较运算符 —— 结果是 bool
            // ================================================================
            Console.WriteLine("---- 演示 4：比较运算符 ----");

            int m = 10, k = 20;
            Console.WriteLine($"{m} == {k}  →  {m == k}     等于（注意是两个等号！）");
            Console.WriteLine($"{m} != {k}  →  {m != k}     不等于");
            Console.WriteLine($"{m} >  {k}  →  {m > k}     大于");
            Console.WriteLine($"{m} <  {k}  →  {m < k}     小于");
            Console.WriteLine($"{m} >= 10  →  {m >= 10}     大于等于");
            Console.WriteLine($"{m} <= 10  →  {m <= 10}     小于等于");
            Console.WriteLine();

            // 字符串也能用 == 比较（C# 的 string 重载了 ==，按内容比）
            string s1 = "hello";
            string s2 = "hello";
            Console.WriteLine($"\"hello\" == \"hello\"  →  {s1 == s2}    字符串按内容比较");
            Console.WriteLine();


            // ================================================================
            // 演示 5：逻辑运算符和短路求值
            // ================================================================
            Console.WriteLine("---- 演示 5：逻辑运算符 ----");

            int age = 20;
            bool hasTicket = true;

            Console.WriteLine($"age = {age}, hasTicket = {hasTicket}");
            Console.WriteLine($"age >= 18 && hasTicket  →  {age >= 18 && hasTicket}     两个都满足才 true");
            Console.WriteLine($"age < 18  || hasTicket  →  {age < 18 || hasTicket}     满足一个就 true");
            Console.WriteLine($"!hasTicket              →  {!hasTicket}    取反");
            Console.WriteLine();

            // ★ 短路求值：左边定了结果，右边就不执行
            Console.WriteLine("  ★ 短路求值（很重要，能防崩溃）");
            string? nullName = null;

            // 下面的判断顺序是关键：先检查 != null，再访问 .Length
            // 如果反过来写 name.Length > 0 && name != null，就会崩溃
            if (nullName != null && nullName.Length > 0)
            {
                Console.WriteLine("   （不会走到这里）");
            }
            else
            {
                Console.WriteLine("   nullName != null 是 false → && 右边【没有执行】→ 没崩溃 ✅");
            }
            Console.WriteLine();


            // ================================================================
            // 演示 6：if / else if / else
            // ================================================================
            Console.WriteLine("---- 演示 6：if / else if / else ----");

            // 逐个测试不同的分数
            foreach (int score in new[] { 95, 85, 72, 45 })
            {
                string level;

                if (score >= 90)
                {
                    level = "优秀";
                }
                else if (score >= 80)          // ← C# 是 "else if"，不是 Python 的 "elif"
                {
                    level = "良好";
                }
                else if (score >= 60)
                {
                    level = "及格";
                }
                else
                {
                    level = "不及格";
                }

                Console.WriteLine($"   分数 {score,3} → {level}");
            }
            Console.WriteLine();


            // ================================================================
            // 演示 7：三元运算符（上面那段 if/else 的简写）
            // ================================================================
            Console.WriteLine("---- 演示 7：三元运算符 ? : ----");

            int checkAge = 20;

            // C# 写法：  条件 ? 真的值 : 假的值
            string typeCSharp = checkAge >= 18 ? "成年" : "未成年";
            Console.WriteLine($"   C#   ：checkAge >= 18 ? \"成年\" : \"未成年\"  →  {typeCSharp}");
            Console.WriteLine("   Python：\"成年\" if checkAge >= 18 else \"未成年\"");
            Console.WriteLine("   ★ 顺序【完全相反】！C# 条件在前，Python 条件在中。");
            Console.WriteLine();

            // 嵌套三元（不推荐，可读性差）
            int grade = 85;
            string lvl = grade >= 90 ? "优秀" : grade >= 60 ? "及格" : "不及格";
            Console.WriteLine($"   嵌套三元：{grade} → {lvl}（能写，但难读，建议改用 switch 表达式）");
            Console.WriteLine();


            // ================================================================
            // 演示 8：switch 语句
            // ================================================================
            Console.WriteLine("---- 演示 8：switch 语句 ----");

            foreach (int day in new[] { 1, 3, 6, 9 })
            {
                Console.Write($"   星期 {day} → ");
                switch (day)
                {
                    case 1:
                        Console.WriteLine("星期一");
                        break;                 // ★ break 不能忘，否则编译错误

                    case 2:
                        Console.WriteLine("星期二");
                        break;

                    case 3:
                        Console.WriteLine("星期三");
                        break;

                    case 6:
                    case 7:                    // ← 空的 case 会"落到"下面，表示 6 和 7 走同一段
                        Console.WriteLine("周末！");
                        break;

                    default:                   // ← 相当于 else：都不匹配时执行
                        Console.WriteLine("其他日子");
                        break;
                }
            }
            Console.WriteLine();


            // ================================================================
            // 演示 9：switch 表达式（C# 8+ 的现代写法，推荐）
            // ================================================================
            Console.WriteLine("---- 演示 9：switch 表达式（更好用）----");

            foreach (int sc in new[] { 95, 85, 72, 45 })
            {
                // 语法：变量 switch { 匹配条件 => 结果, ... , _ => 兜底 }
                string level = sc switch
                {
                    >= 90 => "优秀",
                    >= 80 => "良好",
                    >= 60 => "及格",
                    _     => "不及格"          // _ 是"其他所有情况"
                };
                Console.WriteLine($"   分数 {sc,3} → {level}");
            }
            Console.WriteLine();

            // 对比：switch 表达式不需要 break，而且能直接赋值给变量
            int number = 7;
            string desc = number switch
            {
                0            => "零",
                < 0          => "负数",
                > 0 and < 10 => "个位数正数",     // and 可以组合条件
                _            => "其他"
            };
            Console.WriteLine($"   {number} 的描述：{desc}");
            Console.WriteLine();


            // ================================================================
            // 演示 10：Math 类
            // ================================================================
            Console.WriteLine("---- 演示 10：Math 类常用方法 ----");

            Console.WriteLine($"   Math.Abs(-5)          = {Math.Abs(-5)}        绝对值");
            Console.WriteLine($"   Math.Max(3, 7)        = {Math.Max(3, 7)}        较大值");
            Console.WriteLine($"   Math.Min(3, 7)        = {Math.Min(3, 7)}        较小值");
            Console.WriteLine($"   Math.Round(3.567, 2)  = {Math.Round(3.567, 2)}     四舍五入到 2 位");
            Console.WriteLine($"   Math.Floor(3.9)       = {Math.Floor(3.9)}        向下取整");
            Console.WriteLine($"   Math.Ceiling(3.1)     = {Math.Ceiling(3.1)}        向上取整");
            Console.WriteLine($"   Math.Pow(2, 10)       = {Math.Pow(2, 10)}      2 的 10 次方");
            Console.WriteLine($"   Math.Sqrt(16)         = {Math.Sqrt(16)}         平方根");
            Console.WriteLine($"   Math.PI               = {Math.PI}   ← 常量，不加括号");
            Console.WriteLine();

            // 实用例子：判断一个数是不是整数
            double value = 4.0;
            bool isWhole = value == Math.Floor(value);
            Console.WriteLine($"   {value} 是整数吗？{isWhole}");
            Console.WriteLine();


            // ================================================================
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson02Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
