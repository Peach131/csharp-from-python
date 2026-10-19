// ============================================================================
//  第 17 课 · 练习：编译流水线
//
//  ★ 本课重点：
//     1. 源码 → IL → JIT → 机器码，四步各发生在什么时候
//     2. bin\ 和 obj\ 里每个文件的用途
//     3. JIT 只编译一次 —— 这是"性能测试必须先预热"的根本原因
//
//  ★ 怎么做这一课的练习：
//     前两个 TODO 在代码里做，后三个 TODO 需要你【离开 IDE 去看文件夹】。
//     这不是偷懒 —— "会用文件系统看编译产物"本身就是本课的技能。
// ============================================================================

namespace LessonPractice
{
    class Lesson17Practice
    {
        // ================================================================
        // ↓↓↓ 本课的辅助方法写在这里（在 Main 的【上面】）↓↓↓
        // ================================================================



        static void Main()
        {
            Console.WriteLine("===== 第 17 课练习：编译流水线 =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 打印"我运行在什么之上"
            // ================================================================
            // 【要求】打印三样东西：
            //     1. 运行时版本（用 Environment.Version）
            //     2. 当前进程是不是 64 位（用 Environment.Is64BitProcess）
            //     3. 当前可执行文件（.exe）的完整路径（用 Environment.ProcessPath）
            //
            // 【预期输出】（具体数字会不同）
            //     运行时版本：9.0.x
            //     64 位进程：True
            //     可执行文件：C:\...\bin\Debug\net9.0\Lesson17Practice.exe
            //
            // 【提示】
            //     Console.WriteLine($"运行时版本：{Environment.Version}");
            //     注意 Environment.ProcessPath 的类型是 string?（可能为 null）——
            //     直接插值打印不会报错，但如果你要调用它的方法就要先判空。
            //
            // 【考察点】
            //   · Environment.Version 是【运行时】版本，不是你编译时用的 SDK 版本
            //   · 用 SDK 10 编译出 net9.0 的程序，这里就会显示 9.x（因为跑在 9.x 运行时上）

            Console.WriteLine("--- TODO 1 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ 亲手证明"JIT 只编译一次"
            // ================================================================
            // 【要求】
            //   1. 在上面（Main 外面）写一个方法 `static long SlowMethod(int x)`：
            //        里面写一个循环做点计算，比如把 x*i 累加 50 次，返回结果。
            //        ★ 给这个方法加 NoInlining 特性，防止它被内联掉：
            //          [System.Runtime.CompilerServices.MethodImpl(
            //              System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
            //   2. 用 Stopwatch 测【第一次】调用的耗时
            //   3. 再用 Stopwatch 测【接下来 100 万次】调用的总耗时，算出平均每次
            //   4. 打印三个数字：第一次、100 万次总计、平均每次
            //
            // 【预期输出】（数字会不同，看【量级差异】）
            //     第 1 次调用   ：    0.1234 ms   ← 含 JIT 编译开销
            //     之后 100 万次 ：     250.00 ms
            //     平均每次      ：    0.000250 ms
            //
            // 【提示】
            //     var sw = System.Diagnostics.Stopwatch.StartNew();
            //     long r1 = SlowMethod(1);
            //     sw.Stop();
            //     double first = sw.Elapsed.TotalMilliseconds;
            //
            //     sw.Restart();                       // ★ Restart 是"归零再开始"
            //     long r2 = 0;
            //     for (int i = 0; i < 1_000_000; i++) { r2 += SlowMethod(i % 100); }
            //     sw.Stop();
            //
            //     ★★ 记得用一下 r1 和 r2！否则编译器会警告 CS0219（变量已赋值但从未使用）
            //
            // 【考察点】
            //   · 第一次调用比"平均每次"慢成百上千倍 —— 差的就是 JIT 的时间
            //   · 这就是为什么性能测试必须先"预热"，否则测到的主要是 JIT

            Console.WriteLine("--- TODO 2 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ 找出 bin 目录里到底有哪些文件
            // ================================================================
            // 【要求】
            //   1. 用代码列出 bin 目录下所有文件名 + 文件大小
            //      （从 Environment.ProcessPath 往回推出目录路径）
            //   2. 然后【关掉程序】，自己去文件管理器里打开这个目录，
            //      确认代码列出来的和你在资源管理器里看到的一致
            //
            // 【预期输出】（至少这几个）
            //     Lesson17Practice.dll
            //     Lesson17Practice.exe
            //     Lesson17Practice.pdb
            //     Lesson17Practice.deps.json
            //     Lesson17Practice.runtimeconfig.json
            //
            // 【提示】
            //     string? exePath = Environment.ProcessPath;
            //     string? dir = Path.GetDirectoryName(exePath);
            //     if (dir is not null && Directory.Exists(dir))
            //     {
            //         string[] files = Directory.GetFiles(dir);
            //         Array.Sort(files);                       // ★ 排序，输出才稳定
            //         foreach (string f in files)
            //         {
            //             var info = new FileInfo(f);
            //             Console.WriteLine($"  {info.Name,-34}{info.Length,10:N0} 字节");
            //         }
            //     }
            //     // ★ {info.Name,-34} 是"占 34 格左对齐"，{info.Length,10:N0} 是"占 10 格右对齐 + 千分位"
            //
            // 【考察点】
            //   · 哪个文件最大？（是 .dll —— 你的代码全在里面）
            //   · .exe 有多大？它为什么这么小？

            Console.WriteLine("--- TODO 3 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ 看懂"顶层语句被包装成了什么"
            // ================================================================
            // 【要求】这一题【不用写代码】，用文字回答三个问题，答案写成注释：
            //
            //   问题 1：你在这个文件里写的是"顶层语句"（没有 class 包着 Main 的写法），
            //          但编译出来的 dll 里一定有一个 Main 方法。
            //          编译器把文件顶部那行注释里的内容包装进了哪个类？
            //          （提示：去看 dll 反编译结果里的 <Main>$ 方法）
            //
            //   问题 2：ILSpy 里打开 Lesson17Practice.dll，
            //          找到 TODO 2 写的那个 SlowMethod。
            //          它里面那个 for 循环，在 IL 里变成了哪几条指令？
            //          （最少认出：加载变量、加法、比较、条件跳转）
            //
            //   问题 3：为什么 JIT 是"按方法"编译的，而不是"一次性把整个 dll 都编译成机器码"？
            //          这样设计有什么好处？（提示：想想一个有几万方法的程序，
            //          如果启动时全部编译，启动要多慢？）
            //
            // 【怎么打开 ILSpy】
            //   方式一：Rider 自带 IL Viewer（光标放在方法名上右键找菜单）
            //   方式二：下载 ILSpy（免费），把 bin\Debug\net9.0\Lesson17Practice.dll 拖进去
            //
            // 【考察点】
            //   · 顶层语句不是"没有类"，只是编译器帮你写了那个类
            //   · IL 是【静态类型】的字节码，和 Python 的字节码不是一回事
            //   · 按需编译 = 启动快；代价 = 第一次调用慢（就是 TODO 2 测到的现象）

            Console.WriteLine("--- TODO 4 ---");

            // ↓↓↓ 把你的答案写成注释 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ ★ 综合：编译产物自检工具（本课考核）
            // ================================================================
            // 【要求】写一个方法 `static void CheckBuildOutput(string binDir)`，
            //        检查 bin 目录里该有的文件是不是都在，并打印一份报告。
            //
            //   要检查的文件（以及"缺了会有什么后果"）：
            //     .dll                    → 缺了程序根本跑不起来
            //     .exe（Windows 上）       → 缺了双击启动不了（命令行还能 dotnet xxx.dll）
            //     .pdb                    → 缺了程序照跑，但断点会失准（没有行号对照）
            //     .deps.json              → 缺了运行时不知道要加载哪些包
            //     .runtimeconfig.json     → 缺了运行时不知道该用哪个版本的 .NET
            //
            //   报告格式：
            //     ========== 编译产物自检 ==========
            //     目录：C:\...\bin\Debug\net9.0
            //     [OK]   Lesson17Practice.dll
            //     [OK]   Lesson17Practice.pdb
            //     [缺失] Lesson17Practice.deps.json     ← 运行时无法确定依赖
            //     ----------------------------------
            //     检查 5 项，缺失 1 项
            //
            // 【提示】分三步写：
            //     第 1 步：定义"要检查什么"——用数组存
            //         string[] required = { ".dll", ".pdb", ".deps.json", ".runtimeconfig.json" };
            //         （注意 .exe 在 Linux/macOS 上没有，所以单独处理或干脆不检查它）
            //     第 2 步：从 binDir 里查出程序集的名字（不带扩展名）
            //         ★ 怎么拿到？想想 bin 目录里的文件名长什么样 —— 它们都以程序集名开头。
            //           可以找第一个 .dll 文件，取它的文件名去掉扩展名。
            //     第 3 步：循环检查每个"程序集名 + 扩展名"是否存在
            //         用 File.Exists(Path.Combine(binDir, name))
            //         在报告里就打印 [OK] / [缺失]，缺失的还要打印后果说明
            //
            // 【考察点】
            //   · 把"知道每个文件干什么"变成"能自动检查它们"
            //   · 这是很实用的技能：以后 CI/CD 里就是靠这类检查拦问题的
            //   · 练 Path.Combine / File.Exists / Directory.GetFiles 的组合使用

            Console.WriteLine("--- TODO 5 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓
            // 记得在 Main 外面定义 CheckBuildOutput 方法！




            Console.WriteLine();


            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
