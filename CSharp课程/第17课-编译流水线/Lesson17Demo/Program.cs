namespace LessonPractice
{
    // ============================================================================
    //  第 17 课 · 示例：编译流水线（源码 → IL → JIT → 机器码）
    //
    //  ★ 本示例的目的不是"算出什么东西"，而是让你【亲眼看到】编译产物的存在。
    //
    //  ★ 运行方式：
    //      1. 打开 第17课.sln，把 Lesson17Demo 设为启动项目，Ctrl + F5
    //      2. 或者命令行：dotnet run --project Lesson17Demo
    //
    //  ★ 运行完，一定要去 bin\Debug\net9.0\ 目录里看看生成了哪些文件
    //     （示例的最后一段会告诉你每个文件是什么）
    // ============================================================================
    class Lesson17Demo
    {
        /// <summary>
        /// 一个普通的方法，用来演示"JIT 只编译一次"。
        /// ★ 为什么把它单独拿出来、还要标 NoInlining？
        ///   如果它被内联进调用方，就不存在"这个方法被 JIT 编译"这件事了 ——
        ///   测量也就失去意义。NoInlining 强制它保持独立。
        /// </summary>
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static long JitTarget(int x)
        {
            long sum = 0;
            for (int i = 0; i < 50; i++)
            {
                sum += (long)x * i;
            }
            return sum;
        }

        static void Main()
        {
            Console.WriteLine("===== 第 17 课：编译流水线 =====");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第一段：我运行在什么之上？
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 一、运行环境 ----------");

            // ★ .NETVersion 是【运行时】版本，不是编译时用的 SDK 版本。
            //   两者可以不同：用 SDK 10 编译出 net9.0 的程序，就跑在 9.x 运行时上。
            Console.WriteLine($"  运行时版本      : {Environment.Version}");

            // ★ 空格填充的写法：{0,-8} 表示"第 0 个参数，占 8 格，左对齐"
            //   正数 8 是右对齐，负数 -8 是左对齐。这和第 13 课表格对齐是同一个语法。
            Console.WriteLine($"  64 位进程       : {Environment.Is64BitProcess,-8}");

            // ★ 这行打印的是【编译时】就写进程序集的值 —— 它来自 .csproj 的 TargetFramework
            Console.WriteLine($"  程序集位置      : {Environment.ProcessPath}");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第二段：JIT 只编译一次
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 二、JIT：首次调用 vs 稳态调用 ----------");

            // ① 第一次调用：这一次包含了"把 IL 翻译成机器码"的开销
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long r1 = JitTarget(1);
            sw.Stop();
            double firstMs = sw.Elapsed.TotalMilliseconds;

            // ② 再调用 100 万次：方法已经是机器码了，直接执行，不再编译
            sw.Restart();
            long r2 = 0;
            for (int i = 0; i < 1_000_000; i++)
            {
                r2 += JitTarget(i % 100);
            }
            sw.Stop();
            double loopMs = sw.Elapsed.TotalMilliseconds;

            Console.WriteLine($"  第 1 次调用      : {firstMs,10:F4} ms   ← 含 JIT 编译开销");
            Console.WriteLine($"  之后 100 万次    : {loopMs,10:F2} ms   ← 已经是机器码");
            Console.WriteLine($"  平均每次         : {loopMs / 1_000_000,10:F6} ms");
            Console.WriteLine();
            Console.WriteLine("  ★ 结论：第一次调用比后面【平均每次】慢成百上千倍。");
            Console.WriteLine("     这就是为什么做性能测试前必须【预热】——");
            Console.WriteLine("     不预热的话，你测到的主要是 JIT 的时间，不是代码的时间。");
            Console.WriteLine();

            // ★ 用一下 r1 / r2，否则编译器会警告"变量已赋值但从未使用"（CS0219）
            Console.WriteLine($"  （防止编译器优化：结果校验 {r1 + r2 != 0}）");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第三段：bin 和 obj 里到底有什么
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 三、编译产物清单 ----------");

            // ★ 从"当前程序集的位置"往回推 bin 目录，这样不管你在哪台机器上跑都对
            string exePath = Environment.ProcessPath ?? "";
            string? binDir = Path.GetDirectoryName(exePath);

            if (binDir is not null && Directory.Exists(binDir))
            {
                Console.WriteLine($"  bin 目录：{binDir}");
                Console.WriteLine();

                // 按文件名排序，输出每个文件的大小
                string[] files = Directory.GetFiles(binDir);
                Array.Sort(files);

                foreach (string file in files)
                {
                    var info = new FileInfo(file);
                    // ★ {info.Length,10:N0} = 占 10 格右对齐 + 千分位分隔
                    Console.WriteLine($"    {info.Name,-32}{info.Length,10:N0} 字节");
                }
            }
            else
            {
                Console.WriteLine("  （没找到 bin 目录）");
            }

            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第四段：每个文件是干什么的
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 四、这些文件分别是什么 ----------");
            Console.WriteLine();
            Console.WriteLine("  Lesson17Demo.dll          ★ 主角。里面装着 IL + 元数据");
            Console.WriteLine("                              IL  = 你的代码编译成的中间语言");
            Console.WriteLine("                              元数据 = 类型信息（有哪些类、哪些方法、参数是什么类型）");
            Console.WriteLine();
            Console.WriteLine("  Lesson17Demo.exe          Windows 上的启动器（很小）");
            Console.WriteLine("                             它只负责「找到 dotnet 运行时、把 dll 交给它」，不含你的代码");
            Console.WriteLine();
            Console.WriteLine("  Lesson17Demo.pdb          调试符号：IL 的行号 ↔ 你的源码行号 的对照表");
            Console.WriteLine("                             删了它程序照样跑，但断点会失准");
            Console.WriteLine();
            Console.WriteLine("  Lesson17Demo.deps.json    依赖清单：这个程序需要哪些包、什么版本");
            Console.WriteLine("  Lesson17Demo.runtimeconfig.json  运行时配置：需要哪个版本的 .NET");
            Console.WriteLine();
            Console.WriteLine("  ★ 一个反直觉的事实：.exe 才是那个"没用"的文件 ——");
            Console.WriteLine("     你所有的代码都在 .dll 里。Linux/macOS 上根本没有 .exe，只有 .dll。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 第五段：obj 目录
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 五、obj 目录里有什么 ----------");

            if (binDir is not null)
            {
                // bin\Debug\net9.0 → 往上两级就是项目目录，再进 obj\Debug\net9.0
                string? configDir = Path.GetDirectoryName(binDir);        // bin\Debug
                string? binRoot = Path.GetDirectoryName(configDir);       // bin
                string? projectDir = Path.GetDirectoryName(binRoot);      // 项目根目录

                if (projectDir is not null)
                {
                    string objDir = Path.Combine(projectDir, "obj", "Debug", "net9.0");
                    if (Directory.Exists(objDir))
                    {
                        Console.WriteLine($"  obj 目录：{objDir}");
                        Console.WriteLine();
                        foreach (string file in Directory.GetFiles(objDir))
                        {
                            var info = new FileInfo(file);
                            Console.WriteLine($"    {info.Name,-52}{info.Length,8:N0} 字节");
                        }
                        Console.WriteLine();
                        Console.WriteLine("  ★ 注意那个 Lesson17Demo.AssemblyInfo.cs ——");
                        Console.WriteLine("     你从没写过这个文件，是编译过程自动生成的。");
                        Console.WriteLine("     它里面记录着版本号、程序集名等信息。");
                    }
                    else
                    {
                        Console.WriteLine($"  （没找到 {objDir}）");
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("---------- 六、下一步：看 IL ----------");
            Console.WriteLine();
            Console.WriteLine("  光看文件列表还不够，你要【看到 IL 本身】。三种办法：");
            Console.WriteLine();
            Console.WriteLine("  ① Rider 自带：把光标放在 JitTarget 方法名上，");
            Console.WriteLine("     右键 → 找 \"IL Viewer\" 相关菜单项（不同版本位置略有不同）");
            Console.WriteLine();
            Console.WriteLine("  ② 装 ILSpy（免费开源，推荐）：");
            Console.WriteLine("     下载后直接把这个 dll 拖进去：");
            // ★ 把 .exe 路径换成 .dll 路径，方便你直接拖进 ILSpy
            //   ⚠️ 注意写法：这里是【普通字符串】而不是插值字符串，
            //   所以 Replace 里的双引号可以直接写。
            //   但如果你把它塞进 $"..." 里（像这样：$"{(x.Replace(".exe", ".dll"))}"），
            //   里面的 " 会把插值字符串提前结束掉 → 编译错误。
            //   规则：想在内层用双引号，就必须写 \" 转义。
            string? dllPath = exePath?.Replace(".exe", ".dll");
            Console.WriteLine($"       {dllPath}");
            Console.WriteLine();
            Console.WriteLine("  ③ 命令行（需要 SDK）：");
            Console.WriteLine("       ildasm Lesson17Demo.dll");
            Console.WriteLine();
            Console.WriteLine("  ★ 打开后重点看两处：");
            Console.WriteLine("     · 顶层语句被编译器包装成了 <Main>$ 方法");
            Console.WriteLine("     · JitTarget 里那个 for 循环 → 变成了 ldloc / add / blt 这样的指令");
            Console.WriteLine();

            Console.WriteLine("===== 示例结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
