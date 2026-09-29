namespace Experiments;

/// <summary>
/// 第 1 周 · 实验 1：编译流水线
/// </summary>
/// <remarks>
/// 结论（先自己跑一遍再看）：
///   1. 你的 C# 源码被编译成 IL + 元数据，打包进 .dll（程序集）。
///   2. .exe 只是一个很小的启动器，真正干活的是 .dll。
///   3. .pdb 是调试符号（行号 ↔ IL 的映射），Release 发布时可以不带。
///   4. obj/ 是编译中间产物，可以安全删除；删掉后下次会重新生成。
///   5. 相对于 Python：Python 的 .pyc 只是缓存字节码，而 .NET 的 dll 同时
///      包含【类型元数据】——这是 C# 能编译期检查、IDE 能精确补全的根本原因。
/// </remarks>
public static class E01_Compilation
{
    public static void Run()
    {
        var asm = typeof(E01_Compilation).Assembly;

        Console.WriteLine("【当前程序集信息】");
        Console.WriteLine($"  名称       : {asm.GetName().Name}");
        Console.WriteLine($"  版本       : {asm.GetName().Version}");
        Console.WriteLine($"  文件位置   : {asm.Location}");
        Console.WriteLine($"  运行时版本 : {asm.ImageRuntimeVersion}");
        Console.WriteLine();

        Console.WriteLine("【程序集里有哪些类型？（这就是元数据）】");
        var types = asm.GetTypes().OrderBy(t => t.Name);
        foreach (var t in types)
            Console.WriteLine($"  {(t.IsValueType ? "struct" : "class ")} {t.FullName}");
        Console.WriteLine();

        Console.WriteLine("【手动去这些目录里翻一翻】");
        var baseDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        Console.WriteLine($"  输出目录 : {baseDir}");

        if (Directory.Exists(baseDir))
        {
            Console.WriteLine("  里面的文件：");
            foreach (var f in Directory.GetFiles(baseDir).OrderBy(f => f))
            {
                var info = new FileInfo(f);
                Console.WriteLine($"    {info.Name,-40} {info.Length,10:N0} 字节");
            }
        }
        Console.WriteLine();

        Console.WriteLine("【动手任务】");
        Console.WriteLine("  1. 在资源管理器里打开上面的输出目录，找到 .dll 文件");
        Console.WriteLine("  2. 双击它 —— 你会发现打不开（因为它不是可执行的东西，是 IL+元数据）");
        Console.WriteLine("  3. 用 ILSpy 打开它，找到 Program 类和 E01_Compilation 类");
        Console.WriteLine("  4. 回到项目根目录，找到 obj/ 目录，看看里面是什么（提示：_ 开头的临时文件）");
        Console.WriteLine();
        Console.WriteLine("【去 ILSpy 里看什么】");
        Console.WriteLine("  打开 Program.cs 编译后的 <Main>$ 方法，观察：");
        Console.WriteLine("    - 顶级语句被编译器包装成了一个 class + 一个 Main 方法");
        Console.WriteLine("    - Console.WriteLine 在 IL 里是一条 call 指令");
        Console.WriteLine("  下载 ILSpy: https://github.com/icsharpcode/ILSpy/releases");
    }
}
