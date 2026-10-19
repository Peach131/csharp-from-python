namespace LessonPractice
{
    class Lesson03Practice
    {
        static void Main()
        {
            Console.WriteLine("===== 第 03 课练习：循环与数组 =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ for 循环基础
            // ================================================================
            // 【要求】用 for 循环打印：
            //   1. 1 到 10（同一行，用空格分隔）
            //   2. 10 到 1（倒着数）
            //   3. 1 到 20 之间的所有偶数
            // 【预期输出】
            //     1 到 10：1 2 3 4 5 6 7 8 9 10
            //     10 到 1：10 9 8 7 6 5 4 3 2 1
            //     偶数：2 4 6 8 10 12 14 16 18 20
            // 【提示】Console.Write("1 到 10：");
            //         for (int i = 1; i <= 10; i++) { Console.Write($"{i} "); }
            //         Console.WriteLine();          // ← 循环结束后才换行
            //         倒着数：for (int i = 10; i >= 1; i--)
            //         偶数：for (int i = 2; i <= 20; i += 2)    ← 步长 2
            // 【考察点】注意换行要写在循环【外面】

            Console.WriteLine("--- TODO 1 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓
            Console.Write("1 到 10：");
            for (int i = 1; i <= 10; i++) { Console.Write($"{i} "); }
            Console.WriteLine(); 
            Console.Write("10 到 1（倒着数）：");
            for (int i=10;i>0;i--) {Console.Write($"{i} ");}
            Console.WriteLine(); 
            Console.Write("1 到 20 之间的所有偶数：");
            // ★ 起始值必须是 2：题目要的是"1 到 20 之间"，0 不在这个区间里
            for (int i = 2; i <= 20; i += 2) { Console.Write($"{i} "); }
            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ 累加、累乘
            // ================================================================
            // 【要求】
            //   1. 计算 1 + 2 + ... + 100
            //   2. 计算 10 的阶乘（1 × 2 × ... × 10）
            // 【预期输出】
            //     1 到 100 的和 = 5050
            //     10 的阶乘 = 3628800
            // 【提示】int sum = 0;                     // ★ 累加器初始化为 0
            //         for (int i = 1; i <= 100; i++) { sum += i; }
            //         int product = 1;                // ★ 累乘器初始化为 1（不是 0！）
            // 【考察点】★★ 累加器初值 0、累乘器初值 1 —— 这是最容易错的地方

            Console.WriteLine("--- TODO 2 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓
            int sum = 0;
            int product = 1;
            for (int i = 1; i <= 100; i++) { sum += i; }
            for (int i = 1; i <= 10; i++) { product *= i; }
            Console.WriteLine($"计算 1 + 2 + ... + 100: {sum}");
            Console.WriteLine($"计算 10 的阶乘（1 × 2 × ... × 10）: {product}");
            
            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ 遍历数组，求和 / 求最值 / 求平均
            // ================================================================
            // 【要求】给定成绩数组，算出总分、最高分、平均分（2 位小数）
            // 【预期输出】
            //     成绩：[88, 92, 76, 95, 81]
            //     总分：432
            //     最高分：95
            //     平均分：86.40
            // 【提示】int[] scores = { 88, 92, 76, 95, 81 };
            //         Console.WriteLine($"成绩：[{string.Join(", ", scores)}]");
            //         int max = scores[0];          // ★ 从第一个元素开始，不要写 0
            //         double avg = (double)total / scores.Length;
            // 【考察点】
            //   · 求最大值时 max 初值为什么不能用 0？（如果全是负数会怎样？）
            //   · 平均分为什么必须先转 double？

            Console.WriteLine("--- TODO 3 ---");

            int[] scores = { 88, 92, 76, 95, 81 };

            // ↓↓↓ 在这里写你的代码 ↓↓↓
            Console.WriteLine($"成绩：[{string.Join(", ", scores)}]");
            int total = 0;
            int max = scores[0];
            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i] > max)
                {
                    max = scores[i];
                }
            }
            for (int i = 0; i < scores.Length; i++)
            {
                total += scores[i];
            }
            double avg = (double)total / scores.Length;
            Console.WriteLine($"总分：{total}");
            Console.WriteLine($"最高分：{max}");
            Console.WriteLine($"平均分：{avg:F}");
            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ 条件 + 循环：统计
            // ================================================================
            // 【要求】遍历上面的 scores，统计：
            //   · 及格（>= 60）的人数
            //   · 优秀（>= 90）的人数
            //   · 不及格的具体分数（全及格时打印"（无）"）
            // 【预期输出】
            //     及格人数：5
            //     优秀人数：2
            //     不及格分数：（无）
            // 【提示】int passCount = 0;
            //         for (...) { if (scores[i] >= 60) passCount++; }
            //         // 不及格名单：用一个 bool 记录"有没有找到"
            //         bool any = false;
            //         Console.Write("不及格分数：");
            //         for (...) { if (scores[i] < 60) { Console.Write($"{scores[i]} "); any = true; } }
            //         if (!any) Console.Write("（无）");
            // 【考察点】★ 要打印【分数本身】，不是打印"不及格分数"这四个字
            // 【自测】把 scores 改成 { 88, 92, 55, 95, 81 } 再跑，应该出现"不及格分数：55"

            Console.WriteLine("--- TODO 4 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓
            // ── 第 1 步：及格人数 ────────────────────────────────
            // 计数器初值 0，每命中一个就 +1
            int passCount = 0;
            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i] >= 60)
                {
                    passCount++;
                }
            }

            // ── 第 2 步：优秀人数 ────────────────────────────────
            // 和上一步是同一个套路，只是条件从 >= 60 换成 >= 90
            // ★ 要点：一个统计任务配一个独立的计数器，不要试图用一个变量算两件事
            int excellentCount = 0;
            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i] >= 90)
                {
                    excellentCount++;
                }
            }

            // ── 第 3 步：不及格分数名单 ──────────────────────────
            // ★ 本 TODO 的核心：标志位 any 的读写位置
            //   循环【内】：找到不及格的 → 打印它 + 把 any 置为 true
            //   循环【外】：循环彻底结束后，才用 any 判断"到底找到过没有"
            bool any = false;
            Console.Write("不及格分数：");
            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i] < 60)
                {
                    Console.Write($"{scores[i]} ");
                    any = true;          // ← 写在 if 里面，只对不及格的元素执行
                }
            }
            if (!any)                    // ← ★ 写在 for 的【外面】
            {
                Console.Write("（无）");
            }
            Console.WriteLine();

            // ── 第 4 步：统一输出（标签与课文预期逐字对齐）────────
            Console.WriteLine($"及格人数：{passCount}");
            Console.WriteLine($"优秀人数：{excellentCount}");


            // ================================================================
            // TODO 5 ｜ 嵌套循环：打印图形
            // ================================================================
            // 【要求】用嵌套循环打印倒三角形（5 行）
            // 【预期输出】
            //     *****
            //     ****
            //     ***
            //     **
            //     *
            // 【提示】for (int i = 5; i >= 1; i--)        // 外层：行数从 5 减到 1
            //         {
            //             for (int j = 1; j <= i; j++)    // 内层：这行打印 i 个
            //             { Console.Write("*"); }
            //             Console.WriteLine();
            //         }
            // 【考察点】内层循环的次数由外层变量决定
            // 【进阶】再打印一个等腰三角形（前面补空格）：
            //     提示：外层 i 从 1 到 5，每行先打印 (5-i) 个空格，再打印 (2*i-1) 个星号

            Console.WriteLine("--- TODO 5 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓
            for (int i = 5; i >= 1; i--)        // 外层：行数从 5 减到 1
            {
                for (int j = 1; j <= i; j++)    // 内层：这行打印 i 个
                { Console.Write("*"); }
                Console.WriteLine();
            }
            
            Console.WriteLine();


            // ================================================================
            // TODO 6 ｜ 综合：学生成绩报告（本课考核）
            // ================================================================
            // 【要求】用两个【索引对应】的数组生成报告：
            //   1. 打印每个人的姓名和成绩，用 ★ 标出最高分
            //   2. 统计：总分、平均分、最高分、最低分、及格率
            // 【数据】
            //     string[] names  = { "张三", "李四", "王五", "赵六", "钱七" };
            //     int[]    grades = { 88, 92, 76, 95, 81 };
            // 【预期输出】
            //     ========== 成绩报告 ==========
            //     张三：88
            //     李四：92
            //     王五：76
            //     赵六：95  ★
            //     钱七：81
            //     ------------------------------
            //     总分：432
            //     平均分：86.40
            //     最高分：95
            //     最低分：76
            //     及格率：100.0%
            // 【提示】分三步：
            //     第 1 步：先找出最高分
            //     第 2 步：遍历打印，边打印边算总分/最低分/及格人数
            //         string mark = grades[i] == max ? "  ★" : "";     ← 三元运算符
            //     第 3 步：打印统计结果
            // 【考察点】
            //   · 两个数组用【相同索引】关联（names[i] 对应 grades[i]）
            //   · ★ 及格率公式：(double)及格人数 / 总人数 * 100
            //   · 注意输出格式：字段名后要用【中文冒号】，格式统一
            // 【自测】把 grades 改成 { 88, 92, 76, 95, 55 }，及格率应该是 80.0%

            Console.WriteLine("--- TODO 6 ---");

            string[] names  = { "张三", "李四", "王五", "赵六", "钱七" };
            int[] grades = { 88, 92, 76, 95, 81 };

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ── 第 1 步：先找出最高分 ────────────────────────────
            // ★ 为什么先找最高分？因为打印时要"边打印边判断谁该标 ★"，
            //   判断需要拿 maxGrade 来比，所以它必须在打印【之前】就算出来。
            int maxGrade = grades[0];
            for (int i = 1; i < grades.Length; i++)
            {
                if (grades[i] > maxGrade)
                {
                    maxGrade = grades[i];
                }
            }

            // ── 第 2 步：打印表头 + 逐行打印（顺便把 4 项统计一起算了）──
            // ★ 核心思路：一趟循环同时干 4 件事（打印、累加总分、跟踪最低分、数及格人数）
            //   这叫"一次遍历多个统计"。比写 4 个独立循环更高效，也更不容易写漏。
            Console.WriteLine("========== 成绩报告 ==========");

            int totalGrade = 0;                        // 累加器：初值 0
            int minGrade = grades[0];                  // ★ 最小值初值也取第一个元素，不能取 0
            int passNumber = 0;                        // 及格人数计数器

            for (int i = 0; i < grades.Length; i++)
            {
                totalGrade += grades[i];               // ① 累加总分

                if (grades[i] < minGrade)              // ② 跟踪最低分
                {
                    minGrade = grades[i];
                }

                if (grades[i] >= 60)                   // ③ 数及格人数
                {
                    passNumber++;
                }

                // ④ 三元运算符：这一行该不该跟 ★ ？
                //    等于最高分 → 拿到 "  ★"；否则 → 拿到空字符串 ""
                // ★ 注意 ★ 前面留两个空格，这样"95  ★"和"88"的分数位才对得齐
                string mark = grades[i] == maxGrade ? "  ★" : "";
                Console.WriteLine($"{names[i]}：{grades[i]}{mark}");
            }

            Console.WriteLine("------------------------------");

            // ── 第 3 步：打印统计结果 ────────────────────────────
            double averageGrade = (double)totalGrade / grades.Length;   // ★ 先转 double 再除

            // 及格率：(double)及格人数 / 总人数 * 100
            // ★ 为什么必须先转 double？
            //   如果写 passNumber / grades.Length，整数除法会先把 5/5 砍成 1，
            //   再乘 100 得 100 —— 这次【恰好】对；但换成自测数据 4/5 就会得到 0，
            //   而不是 80.0。先转 double 才在任何数据下都正确。
            double passRate = (double)passNumber / grades.Length * 100;

            Console.WriteLine($"总分：{totalGrade}");
            Console.WriteLine($"平均分：{averageGrade:F2}");
            Console.WriteLine($"最高分：{maxGrade}");
            Console.WriteLine($"最低分：{minGrade}");
            Console.WriteLine($"及格率：{passRate:F1}%");




            Console.WriteLine();


            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}