namespace LessonPractice
{
    class Lesson02Practice
    {
        static void Main()
        {
            Console.WriteLine("===== 第 02 课练习：运算符与流程控制 =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 基础算术运算
            // ================================================================
            // 【要求】已给 a=17, b=5，用插值打印它们的和差积商余
            // 【预期输出】
            //     17 + 5 = 22
            //     17 - 5 = 12
            //     17 * 5 = 85
            //     17 / 5 = 3      ← 注意是 3 不是 3.4
            //     17 % 5 = 2
            // 【提示】Console.WriteLine($"{a} + {b} = {a + b}");  ...剩下 4 行你写
            // 【考察点】17/5 结果是 3 —— 这就是整数除法

            Console.WriteLine("--- TODO 1 ---");

            int a = 17;
            int b = 5;

            // ↓↓↓ 在这里写你的代码 ↓↓↓
            Console.WriteLine($"{a} + {b} = {a + b}");
            Console.WriteLine($"{a} - {b} = {a - b}");
            Console.WriteLine($"{a} * {b} = {a * b}");
            Console.WriteLine($"{a} / {b} = {a / b}");
            Console.WriteLine($"{a} % {b} = {a % b}");
            Console.WriteLine();
            
            // ================================================================
            // TODO 2 ｜ 整数除法的陷阱（本课重点）
            // ================================================================
            // 【要求】
            //   1. 直接打印 7 / 2
            //   2. 用 (double) 强制转换让它得到小数
            //   3. 算平均分：totalScore = 266，count = 3
            //      先写错误版本（直接除），再写正确版本（先转 double）
            // 【预期输出】
            //     7 / 2 直接除        = 3
            //     7 / 2 转换后        = 3.5
            //     平均分（错误写法）  = 88
            //     平均分（正确写法）  = 88.66666666666667
            // 【提示】Console.WriteLine($"7 / 2 直接除        = {7 / 2}");
            //         Console.WriteLine($"7 / 2 转换后        = {(double)7 / 2}");
            // 【考察点】266/3 = 88.666，但整数除法会砍成 88，这是很隐蔽的 bug

            Console.WriteLine("--- TODO 2 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓
            Console.WriteLine($"7 / 2 直接除: {7 / 2}");
            Console.WriteLine($"7 / 2 转换后: {(double)7 / 2}");
            Console.WriteLine($"平均分：totalScore = 266，count = 3直接除: {266 / 3}");
            Console.WriteLine($"平均分：totalScore = 266，count = 3转换后double: {(double)266 / 3}");
            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ 取余 % 的应用
            // ================================================================
            // 【要求】已给 number = 42，完成两件事：
            //   1. 判断奇偶 → 打印 "42 是偶数" 或 "42 是奇数"
            //   2. 判断能否被 3 整除 → 打印 "42 能被 3 整除" 或 "42 不能被 3 整除"
            // 【预期输出】
            //     42 是偶数
            //     42 能被 3 整除
            // 【提示】if (number % 2 == 0) { ... } else { ... }
            //         // 第二项同理，用 number % 3 == 0
            // 【考察点】★ 两个判断都要做，不要只做第一个

            Console.WriteLine("--- TODO 3 ---");

            int number = 42;

            // ↓↓↓ 在这里写你的代码 ↓↓↓
            if (number % 2 == 0){Console.WriteLine($"{number} 是偶数");}
            else {Console.WriteLine($"{number} 是奇数");}
            
            if (number % 3 == 0){Console.WriteLine($"{number} 能被 3 整除");}
            else {Console.WriteLine($"{number} 不能被 3 整除");}
            
            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ 比较运算符和逻辑运算符
            // ================================================================
            // 【要求】判断 year = 2024 是不是闰年
            //   闰年规则：能被 4 整除，且（不能被 100 整除，或者能被 400 整除）
            // 【预期输出】
            //     2024 是闰年
            // 【提示】bool isLeap = (year % 4 == 0) && (year % 100 != 0 || year % 400 == 0);
            // 【考察点】&& 的优先级比 || 高，所以【括号是必须的】
            // 【加分】把 year 改成 1900（不是闰年）、2000（是闰年）各测一次

            Console.WriteLine("--- TODO 4 ---");

            int year = 2024;
            // ↓↓↓ 在这里写你的代码 ↓↓↓
            bool isLeap = (year % 4 == 0) && (year % 100 != 0 || year % 400 == 0);
            if (isLeap){Console.WriteLine($"{year} 是闰年");}
            else{Console.WriteLine($"{year} 不是闰年");}
            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ if / else if / else 多分支
            // ================================================================
            // 【要求】根据 bmi = 26.5 输出健康等级：
            //     < 18.5        → "偏瘦"
            //     18.5 ~ 24     → "正常"
            //     24 ~ 28       → "超重"
            //     >= 28         → "肥胖"
            // 【预期输出】
            //     BMI 26.5 → 超重
            // 【提示】if (bmi < 18.5) { level = "偏瘦"; }
            //         else if (bmi < 24) { level = "正常"; }   ← 不用写 >= 18.5，上面已排除
            // 【考察点】else if 从上到下判断，命中一个就不再往下

            Console.WriteLine("--- TODO 5 ---");

            double bmi = 26.5;
            // ↓↓↓ 在这里写你的代码 ↓↓↓
            string level = bmi switch
            {
                >= 28 => "肥胖",
                >= 24 => "超重",
                >= 18.5 => "正常",
                _    => "偏瘦"          
            };
            Console.WriteLine(level);     
            Console.WriteLine();


            // ================================================================
            // TODO 6 ｜ 综合：个税计算器（本课考核）
            // ================================================================
            // 【要求】
            //   1. 用 Console.ReadLine() 读月收入
            //   2. 用 double.TryParse 转换，非法输入要提示并【退出】
            //   3. 按规则算税：
            //        <= 5000            → 0
            //        5000 ~ 10000       → (收入-5000) * 0.03
            //        10000 ~ 20000      → (收入-5000) * 0.10
            //        > 20000            → (收入-5000) * 0.20
            //   4. 打印应纳税额和税后收入，都保留 2 位小数
            // 【预期运行效果】（输入 15000）
            //     请输入你的月收入（元）：15000
            //     应纳税额：1000.00 元
            //     税后收入：14000.00 元
            // 【提示】先把税算进一个变量，再统一打印：
            //         double tax;
            //         if (income <= 5000)       { tax = 0; }
            //         else if (income <= 10000) { tax = (income - 5000) * 0.03; }
            //         ... 剩下你写
            //         Console.WriteLine($"应纳税额：{tax:F2} 元");
            //         Console.WriteLine($"税后收入：{income - tax:F2} 元");
            // 【考察点】
            //   · ★ TryParse 失败后必须 return，否则程序会拿 0 继续算出错误结果
            //   · 税额要先算进变量再打印，不要打印时算一遍、算总收入时又算一遍
            //     （两处精度不同会导致"显示的税"和"实际扣的税"对不上）

            Console.WriteLine("--- TODO 6 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓
            double tax = 0;
            Console.Write($"请输入你的月收入（元）:");
            string incomes = Console.ReadLine();
            if (!double.TryParse(incomes, out double income))
            {
                Console.WriteLine($"非法输入,将【退出】");
                return;
            }
            if (income <= 5000)       { tax = 0; }
            else if (income <= 10000) { tax = (income - 5000) * 0.03; }
            else if (income <= 20000 ) { tax = (income - 5000) * 0.10;}
            else if (income > 20000 ) { tax = (income - 5000) * 0.20;}
            Console.WriteLine($"应纳税额：{tax:F2} 元");
            Console.WriteLine($"税后收入：{income - tax:F2} 元");
            Console.WriteLine();


            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}