// ============================================================================
//  C# 底层实验台 —— 入口
//  用法：在 Rider / VS2022 里按 Ctrl+F5 运行，输入编号执行对应实验
//
//  ⚠️ 不要把 Console.ReadLine() 换成别的东西，也不要删掉这个循环。
//  ⚠️ 每个实验文件顶部都写了「结论」注释，先自己跑一遍，再去看结论。
// ============================================================================

using Experiments;

// ---------------------------------------------------------------------------
// 单实例保护
//
// 【为什么需要这个】这不是多余的代码，是一个真实的教训：
// 如果同时有两个本程序实例在运行，第一个实例会一直锁住 Experiments.dll，
// 导致第二个实例的编译【失败】，报错是：
//     MSB3027: 无法将"obj\...\Experiments.dll"复制到"bin\...\Experiments.dll"
//     文件被".NET Host (12345)"锁定。超出了重试计数 10。失败。
// 而失败原因（另一个自己还在跑）从报错信息里完全看不出来。
//
// 所以：启动时先检查有没有同名实例在跑，有就友好退出。
// ---------------------------------------------------------------------------
var lockPath = Path.Combine(AppContext.BaseDirectory, "lab.running");
FileStream? lockFile = null;

try
{
    lockFile = new FileStream(lockPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
}
catch (IOException)
{
    Console.WriteLine("================================================================");
    Console.WriteLine("  检测到实验台已经在运行了！");
    Console.WriteLine("================================================================");
    Console.WriteLine();
    Console.WriteLine($"  （锁文件：{lockPath}）");
    Console.WriteLine();
    Console.WriteLine("  本程序被设计成【同一时间只能运行一个实例】，原因是：");
    Console.WriteLine("  运行中的实例会锁住 Experiments.dll，第二个实例");
    Console.WriteLine("  编译时会失败，报 “文件被 .NET Host 锁定”。");
    Console.WriteLine();
    Console.WriteLine("  → 如果你想再运行一次：先关掉原来那个实验台窗口。");
    Console.WriteLine("  → 如果找不到那个窗口（比如它被藏在别的桌面）：");
    Console.WriteLine("     打开任务管理器，结束所有 “dotnet” 或 “Experiments” 进程。");
    Console.WriteLine();
    Console.WriteLine("  按回车退出...");

    // 注意：这里【不能】假设一定有键盘输入。
    // 如果输入被重定向（脚本/自动化场景），ReadLine 会立刻返回 null，
    // 那也应该直接退出，而不是卡在这里等一个永远不会来的回车。
    _ = Console.ReadLine();
    return;
}

try
{
    await RunLab();
}
finally
{
    // 无论怎么退出（包括 Ctrl+C），都要释放锁
    lockFile?.Dispose();
    try { File.Delete(lockPath); } catch { /* 删不掉就算了 */ }
}

// ===========================================================================

static async Task RunLab()
{
    while (true)
    {
        DrawMenu();
        Console.Write("请输入编号 > ");

        var input = Console.ReadLine();

        // -------------------------------------------------------------------
        // 【为什么检查 null】Console.ReadLine() 在以下情况返回 null：
        //   - 输入被重定向、且已经读到结尾（EOF）
        //   - 控制台窗口被关闭
        //   - 在 Rider 的“运行”输出面板里点了停止输入
        // 如果不检查，循环会疯狂空转（每次立刻再读一次 null），
        // 程序永远不退出，还一直占着 Experiments.dll —— 上一个版本就是这么
        // 留下孤儿进程、把编译搞崩的。
        // -------------------------------------------------------------------
        if (input is null)
        {
            Console.WriteLine();
            Console.WriteLine("（输入已结束，实验台退出。下次见！）");
            return;
        }

        Console.WriteLine();
        switch (input.Trim())
        {
            case "1": E01_Compilation.Run(); break;
            case "2": E02_JitCompilation.Run(); break;
            case "3": E03_ValueVsReference.Run(); break;
            case "4": E04_ObjectSize.Run(); break;
            case "5": E05_StackAndRecursion.Run(); break;
            case "6": E06_Boxing.Run(); break;
            case "7": E07_Equality.Run(); break;
            case "8": E08_WeakReference.Run(); break;
            case "9": E09_GenerationsAndGc.Run(); break;
            case "10": E10_StringInterning.Run(); break;
            case "11": E11_StringConcat.Run(); break;
            case "12": E12_Closures.Run(); break;
            case "13": E13_GenericsVsArrayList.Run(); break;
            case "14": await E14_AsyncThreadSwitch.RunAsync(); break;
            case "0": return;
            default:
                Console.WriteLine("无效编号，请输入菜单里的数字。");
                break;
        }

        Console.WriteLine();
        Console.WriteLine("──────────────────────────────────────────────────────────────");
        Console.Write("按回车返回菜单...");

        // 这里同样要防 null：否则实验跑完后会陷入空转
        if (Console.ReadLine() is null)
        {
            Console.WriteLine();
            return;
        }
    }
}

static void DrawMenu()
{
    // ⚠️ Console.Clear() 在没有真实控制台时会抛 IOException
    //    （例如输出被重定向到文件、或被管道接走时）。
    //    这也是一个值得知道的底层细节：Console 的一些 API 依赖真实的控制台句柄。
    try
    {
        if (!Console.IsOutputRedirected)
            Console.Clear();
    }
    catch (IOException)
    {
        // 没有控制台，忽略即可
    }

    Console.WriteLine("================================================================");
    Console.WriteLine("            C# 底层实验台  (InternalsLab)");
    Console.WriteLine("================================================================");
    Console.WriteLine();
    Console.WriteLine("  第 1 周：编译与运行");
    Console.WriteLine("    1  编译流水线：bin/obj 里到底有什么");
    Console.WriteLine("    2  JIT 只编译一次");
    Console.WriteLine();
    Console.WriteLine("  第 2 周：内存模型（栈与堆）");
    Console.WriteLine("    3  值语义 vs 引用语义                        ★核心");
    Console.WriteLine("    4  对象在堆上占多少字节（内存对齐）");
    Console.WriteLine("    5  栈帧与递归（含 StackOverflow 演示）");
    Console.WriteLine();
    Console.WriteLine("  第 3-4 周：类型与装箱");
    Console.WriteLine("    6  装箱与拆箱：分配量化 + 常见触发点         ★核心");
    Console.WriteLine("    7  引用相等 vs 值相等");
    Console.WriteLine();
    Console.WriteLine("  第 5-6 周：垃圾回收");
    Console.WriteLine("    8  WeakReference：证明对象真的被回收了        ★核心");
    Console.WriteLine("    9  对象晋升与 GC 计数");
    Console.WriteLine();
    Console.WriteLine("  第 7 周：字符串");
    Console.WriteLine("   10  字符串驻留（Interning）的真相");
    Console.WriteLine("   11  拼接 vs StringBuilder（性能 + 分配）      ★核心");
    Console.WriteLine();
    Console.WriteLine("  第 8 周：委托与闭包");
    Console.WriteLine("   12  闭包会分配对象 + 循环变量捕获的坑");
    Console.WriteLine();
    Console.WriteLine("  第 9 周：泛型");
    Console.WriteLine("   13  List<int> vs ArrayList：装箱的真实代价");
    Console.WriteLine();
    Console.WriteLine("  第 10 周：异步");
    Console.WriteLine("   14  await 之后线程会变吗                        ★核心");
    Console.WriteLine();
    Console.WriteLine("    0  退出");
    Console.WriteLine();
}
