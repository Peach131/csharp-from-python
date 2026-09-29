namespace LessonPractice
{
    class Lesson01Practice
    {
        static void Main()
        {
            Console.WriteLine("===== 第 01 课练习：变量与类型 =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 声明 5 种不同类型的变量
            // ================================================================
            // 【要求】声明并用插值打印这 5 个变量：
            //     studentName  string   你的名字
            //     studentAge   int      你的年龄
            //     score        double   88.5
            //     grade        char     'B'      ← 注意单引号
            //     isPassed     bool     true
            // 【预期输出】
            //     姓名：qzl
            //     年龄：20
            //     分数：88.5
            //     等级：B
            //     是否及格：True
            // 【提示】声明格式：类型 名字 = 值;
            //         string studentName = "qzl";
            //         Console.WriteLine($"姓名：{studentName}");

            Console.WriteLine("--- TODO 1 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ double 和 decimal 的区别（本课重点）
            // ================================================================
            // 【要求】
            //   1. 用 double 计算 0.1 + 0.2，打印结果
            //   2. 用 decimal 计算 0.1 + 0.2，打印结果
            // 【预期输出】
            //     double  : 0.30000000000000004
            //     decimal : 0.3
            // 【提示】double d = 0.1 + 0.2;
            //         decimal m = 0.1m + 0.2m;      ← decimal 字面量末尾要加 m！
            // 【考察点】亲眼看到 double 算不准，才会记住"算钱用 decimal"

            Console.WriteLine("--- TODO 2 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ 模拟"用户输入 → 转换 → 计算"
            // ================================================================
            // 【要求】给定两个字符串（一个合法一个非法）：
            //     string inputA = "25";
            //     string inputB = "abc";
            //   用 int.TryParse 分别转换：
            //     - 成功的打印 "转换成功，结果是 25，加 10 等于 35"
            //     - 失败的打印 "转换失败"
            // 【预期输出】
            //     "25"  转换成功，结果是 25，加 10 等于 35
            //     "abc" 转换失败
            // 【提示】if (int.TryParse(inputA, out int a)) { ... } else { ... }
            // 【考察点】★ 两个输入都要测！只测合法的那个不算完成
            //          （把 int.Parse(inputB) 替换进去试试，看程序怎么崩）

            Console.WriteLine("--- TODO 3 ---");

            string inputA = "25";
            string inputB = "abc";

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ 插值格式化
            // ================================================================
            // 【要求】用插值格式化打印：
            //     double price = 12.5;      → 价格：￥12.50
            //     double rate = 0.1234;     → 利率：12.3%
            //     int population = 1234567; → 人口：1,234,567
            // 【提示】{price:F2}      保留 2 位小数
            //         {rate:P1}       百分比，1 位小数
            //         {population:N0} 千分位分隔，0 位小数

            Console.WriteLine("--- TODO 4 ---");

            double price = 12.5;
            double rate = 0.1234;
            int population = 1234567;

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ 字符串操作 + 类型转换配合
            // ================================================================
            // 【要求】给定 string record = "  张三,25,北京  ";
            //   1. 去掉首尾空格            → .Trim()
            //   2. 按逗号拆分成 3 部分      → .Split(',')
            //   3. 把第 2 部分转成 int      → int.TryParse
            //   4. 打印成一句话
            // 【预期输出】
            //     张三 今年 25 岁，来自 北京
            // 【提示】string trimmed = record.Trim();
            //         string[] parts = trimmed.Split(',');
            //         // parts[0]=姓名  parts[1]=年龄  parts[2]=城市
            // 【考察点】这是"解析一行数据"的标准套路，以后读文件、读 CSV 都靠它

            Console.WriteLine("--- TODO 5 ---");

            string record = "  张三,25,北京  ";

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 6 ｜ 综合：BMI 计算器（本课考核）
            // ================================================================
            // 【要求】
            //   1. 用 Console.ReadLine() 让用户输入身高（米）和体重（公斤）
            //   2. 用 double.TryParse 转换（非法输入要提示并退出）
            //   3. 计算 BMI = 体重 / (身高 × 身高)
            //   4. 用 F2 格式打印，保留 2 位小数
            // 【预期运行效果】
            //     请输入身高（米）：1.75
            //     请输入体重（公斤）：70
            //     你的 BMI 是：22.86
            // 【提示】Console.Write("请输入身高（米）：");
            //         string? h = Console.ReadLine();
            //         if (!double.TryParse(h, out double height))
            //         {
            //             Console.WriteLine("身高输入不合法");
            //             return;                    // return 表示结束 Main，程序退出
            //         }
            //         // 体重同理，然后算 BMI
            // 【考察点】
            //   · ★ 解析出的数字要存进【另一个变量】，不能拿原始字符串去算
            //   · 用 double 而不是 decimal，因为这是科学计算不是算钱

            Console.WriteLine("--- TODO 6 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}