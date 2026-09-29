namespace Experiments;

/// <summary>
/// 第 7 周 · 实验 10：字符串驻留（Interning）
/// </summary>
/// <remarks>
/// 结论：
///   1. string 是不可变的引用类型：任何「修改」都产生新对象。
///   2. 字面量字符串会被「驻留」到字符串池，相同的字面量共享同一个对象。
///   3. 运行时拼接的结果【不】自动进池，所以 ReferenceEquals 常为 False。
///   4. string.Intern() 可手动入池，但池里的字符串【永不释放】——
///      所以绝对不要对用户输入/动态字符串调用它（那是内存泄漏）。
///   5. .NET 内部是 UTF-16，每个 char 2 字节；Python 3 用变长编码。
///      所以 "abc".Length 在 C# 是 3（char 数），而 emoji 的 Length 可能是 2！
/// </remarks>
public static class E10_StringInterning
{
    public static void Run()
    {
        Console.WriteLine("【实验 A：字面量被驻留复用】");
        Console.WriteLine();

        string a = "hello";
        string b = "hello";
        Console.WriteLine($"  string a = \"hello\"; string b = \"hello\";");
        Console.WriteLine($"  a == b                : {a == b}                （值比较）");
        Console.WriteLine($"  ReferenceEquals(a, b) : {ReferenceEquals(a, b)}   ← 同一个对象！");
        Console.WriteLine();

        string c = "hel" + "lo";                // 编译期常量折叠 → 仍是字面量
        Console.WriteLine($"  string c = \"hel\" + \"lo\";  （编译期就能算出来）");
        Console.WriteLine($"  ReferenceEquals(a, c) : {ReferenceEquals(a, c)}   ← 还是同一个对象");
        Console.WriteLine();

        string d = string.Concat("hel", "lo");  // 运行时拼接
        Console.WriteLine($"  string d = string.Concat(\"hel\", \"lo\");  （运行时才拼接）");
        Console.WriteLine($"  d == a                : {d == a}                （内容相同）");
        Console.WriteLine($"  ReferenceEquals(a, d) : {ReferenceEquals(a, d)}  ← 不同对象！");
        Console.WriteLine();

        string e = "hel";
        e += "lo";                              // 运行时拼接
        Console.WriteLine($"  string e = \"hel\"; e += \"lo\";");
        Console.WriteLine($"  ReferenceEquals(a, e) : {ReferenceEquals(a, e)}  ← 不同对象");
        Console.WriteLine();

        Console.WriteLine("  ★ 结论：== 比内容，ReferenceEquals 比身份。");
        Console.WriteLine("    字面量会自动共享对象，运行时构造的不会。");
        Console.WriteLine();

        Console.WriteLine("【实验 B：手动驻留与它的危险】");
        Console.WriteLine();

        string dynamic = new string(new[] { 'h', 'e', 'l', 'l', 'o' });
        Console.WriteLine($"  动态构造的字符串 \"hello\"");
        Console.WriteLine($"    ReferenceEquals(a, dynamic)              : {ReferenceEquals(a, dynamic)}");
        string interned = string.Intern(dynamic);
        Console.WriteLine($"    经过 string.Intern 后 ReferenceEquals(a, interned) : {ReferenceEquals(a, interned)}   ← 同一个了");
        Console.WriteLine();
        Console.WriteLine("  ★⚠️ 危险：驻留池里的字符串【直到进程结束都不会被回收】。");
        Console.WriteLine("    千万不要这样写：");
        Console.WriteLine("        string s = string.Intern(userInput);   // ❌ 内存泄漏！");
        Console.WriteLine("    如果 userInput 有 100 万种不同取值，池里就永久留着 100 万个字符串。");
        Console.WriteLine("    正确做法：用 Dictionary<string, T> 做自己的缓存，它会随 GC 回收。");
        Console.WriteLine();

        Console.WriteLine("【实验 C：不可变性的实际影响】");
        Console.WriteLine();

        string original = "Hello";
        string upper = original.ToUpper();

        Console.WriteLine($"  original = \"{original}\"");
        Console.WriteLine($"  upper    = original.ToUpper() = \"{upper}\"");
        Console.WriteLine($"  original 变了吗？ → \"{original}\"   ← 没变！");
        Console.WriteLine($"  ReferenceEquals(original, upper) = {ReferenceEquals(original, upper)}");
        Console.WriteLine();
        Console.WriteLine("  ★ 每一次「修改」都在堆上创建一个全新的字符串对象。");
        Console.WriteLine("    这就是为什么循环里拼接字符串是灾难（见实验 11）。");
        Console.WriteLine();

        Console.WriteLine("【实验 D：UTF-16 vs Python 的变长编码】");
        Console.WriteLine();

        string[] samples = { "a", "中", "𝄞", "👨‍👩‍👧" };
        foreach (var s in samples)
        {
            var bytes = System.Text.Encoding.Unicode.GetByteCount(s);
            Console.WriteLine($"  \"{s}\"  Length={s.Length}  UTF-16字节={bytes,3}  码点={s.EnumerateRunes().Count()}");
        }
        Console.WriteLine();
        Console.WriteLine("  ★ \"𝄞\" 的 Length 是 2 —— 不是 1！因为它需要 2 个 UTF-16 码元（代理对）。");
        Console.WriteLine("    s[0] 会拿到半个字符（乱码）。");
        Console.WriteLine("  ★ 对比 Python：len(\"𝄞\") == 1（Python 按码点计数）。");
        Console.WriteLine("    这个差异在处理 emoji、生僻字、多语言文本时会咬你。");
        Console.WriteLine();
        Console.WriteLine("  正确遍历文本元素的方式：");
        var si = new System.Globalization.StringInfo(samples[3]);
        Console.WriteLine($"    new StringInfo(\"{samples[3]}\").LengthInTextElements = {si.LengthInTextElements}");
        Console.WriteLine();

        Console.WriteLine("【实验 E：验证 UTF-16 的内存占用】");
        Console.WriteLine();

        var ascii = "abcdefghij";
        var chinese = "中文测试文本";
        Console.WriteLine($"  \"{ascii}\"  Length={ascii.Length}  → 约 {ascii.Length * 2} 字节（UTF-16）");
        Console.WriteLine($"  \"{chinese}\"  Length={chinese.Length}  → 约 {chinese.Length * 2} 字节（UTF-16）");
        Console.WriteLine();
        Console.WriteLine("  ★ 在 Python 里，ASCII 字符串每字符 1 字节，中文 2 字节；");
        Console.WriteLine("    在 C# 里都是 2 字节。所以存大量纯英文数据时，C# 的字符串");
        Console.WriteLine("    比 Python 占更多内存 —— 这是 UTF-16 的代价。");
        Console.WriteLine();

        Console.WriteLine("【动手任务】");
        Console.WriteLine("  1. 用 Encoding.UTF8.GetByteCount 对比同一个字符串的 UTF-8 和 UTF-16 字节数");
        Console.WriteLine("     （这解释了为什么网络传输和存文件时要用 UTF-8，而内存里是 UTF-16）");
        Console.WriteLine("  2. 写一个函数，用 EnumerateRunes() 正确反转一个含 emoji 的字符串");
        Console.WriteLine("  3. 打开 ILSpy，看 string.Concat 的内部实现（它有多快？为什么？）");
    }
}
