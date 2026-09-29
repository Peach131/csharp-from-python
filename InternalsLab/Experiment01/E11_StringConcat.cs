using System.Diagnostics;
using System.Text;

namespace Experiments;

/// <summary>
/// 第 7 周 · 实验 11：字符串拼接 vs StringBuilder  ★核心
/// </summary>
/// <remarks>
/// 结论：
///   1. 循环里 s += x 是 O(n²)：每次都在堆上分配新字符串并复制全部已有内容。
///   2. 10000 次拼接的分配量约等于 10000²/2 个字符 = 5000 万个字符 ≈ 100MB。
///   3. StringBuilder 内部是可变 char 数组（自动扩容），是 O(n)。
///   4. 已知最终长度时，直接构造 char 数组最快（零浪费）。
///   5. 现代替代：string.Join（序列）、string.Create（精确长度）、
///      插值字符串处理器（$"" 编译器已优化）。
/// </remarks>
public static class E11_StringConcat
{
    private static long Allocated() => GC.GetAllocatedBytesForCurrentThread();

    private static (double Ms, double Mb) Measure(Action action, int warmup = 1)
    {
        for (int i = 0; i < warmup; i++) action();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = Allocated();
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        var allocated = Allocated() - before;

        return (sw.Elapsed.TotalMilliseconds, allocated / 1024.0 / 1024.0);
    }

    private static void Report(string label, (double Ms, double Mb) r)
    {
        Console.WriteLine($"  {label,-34} {r.Ms,9:F1} ms {r.Mb,10:F2} MB");
    }

    public static void Run()
    {
        Console.WriteLine("【实验 A：不同拼接方式的耗时与分配】");
        Console.WriteLine();
        Console.WriteLine($"  {"方式",-34} {"耗时",9} {"分配",10}");
        Console.WriteLine("  " + new string('─', 56));

        var strings = Enumerable.Range(0, 10_000).Select(i => i.ToString()).ToArray();
        var result = "";

        Report("s += i（灾难）", Measure(() =>
        {
            string s = "";
            for (int i = 0; i < 10_000; i++) s += i;
            result = s;
        }));

        Report("StringBuilder（正确）", Measure(() =>
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 10_000; i++) sb.Append(i);
            result = sb.ToString();
        }));

        Report("StringBuilder(预分配容量)", Measure(() =>
        {
            var sb = new StringBuilder(capacity: 50_000);
            for (int i = 0; i < 10_000; i++) sb.Append(i);
            result = sb.ToString();
        }));

        Report("string.Join（序列最优解）", Measure(() =>
        {
            var parts = new string[10_000];
            for (int i = 0; i < 10_000; i++) parts[i] = i.ToString();
            result = string.Concat(parts);
        }));

        Report("已有数组 + string.Concat", Measure(() =>
        {
            result = string.Concat(strings);
        }));

        Console.WriteLine();
        Console.WriteLine($"  （最终结果长度 {result.Length:N0} 字符，用 GC.KeepAlive 防止被优化）");
        GC.KeepAlive(result);
        Console.WriteLine();

        Console.WriteLine("【实验 B：为什么 += 是 O(n²) —— 用数字说话】");
        Console.WriteLine();

        Console.WriteLine($"  {"拼接次数",-12} {"耗时",10} {"分配",12} {"分配/次",12}");
        Console.WriteLine("  " + new string('─', 50));

        foreach (var n in new[] { 1_000, 2_000, 4_000, 8_000, 16_000 })
        {
            var (ms, mb) = Measure(() =>
            {
                string s = "";
                for (int i = 0; i < n; i++) s += "x";
                GC.KeepAlive(s);
            });
            Console.WriteLine($"  {n,-12:N0} {ms,8:F1} ms {mb,10:F2} MB {mb * 1024 * 1024 / n,10:F0} B");
        }
        Console.WriteLine();
        Console.WriteLine("  ★ 观察「分配/次」这一列：拼接次数翻倍，每次的平均分配【也在翻倍】。");
        Console.WriteLine("    这就是 O(n²) 的特征 —— 总分配量随 n 平方增长。");
        Console.WriteLine("    n=1000 时总分配约 0.5MB；n=16000 时约 128MB（增长了 256 倍）。");
        Console.WriteLine();

        Console.WriteLine("【实验 C：真实场景 —— 生成 CSV】");
        Console.WriteLine();

        var rows = Enumerable.Range(0, 50_000)
            .Select(i => new { Id = i, Name = $"User{i}", Score = i * 1.5 })
            .ToList();

        Report("朴素拼接（每行 +=）", Measure(() =>
        {
            string csv = "Id,Name,Score\n";
            foreach (var r in rows)
                csv += $"{r.Id},{r.Name},{r.Score}\n";
            GC.KeepAlive(csv);
        }));

        Report("StringBuilder + AppendLine", Measure(() =>
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Score");
            foreach (var r in rows)
                sb.Append(r.Id).Append(',').Append(r.Name).Append(',').Append(r.Score).Append('\n');
            GC.KeepAlive(sb.ToString());
        }));

        Report("string.Join(rows, '\\n')", Measure(() =>
        {
            var lines = new string[rows.Count + 1];
            lines[0] = "Id,Name,Score";
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                lines[i + 1] = $"{r.Id},{r.Name},{r.Score}";
            }
            GC.KeepAlive(string.Join('\n', lines));
        }));

        Console.WriteLine();
        Console.WriteLine("  ★ 注意 StringBuilder 的 Append 链式调用：Append(int) 有【专门的重载】，");
        Console.WriteLine("    不会装箱。而 Append(object) 会装箱 —— 编译器会优先选最匹配的重载。");
        Console.WriteLine();

        Console.WriteLine("【实验 D：插值字符串的现代优化】");
        Console.WriteLine();

        int id = 42;
        double score = 3.14159;

        Report("$\"{id},{score}\"（插值）", Measure(() =>
        {
            for (int i = 0; i < 100_000; i++)
                GC.KeepAlive($"{id},{score}");
        }, warmup: 100));

        Report("string.Format(...)", Measure(() =>
        {
            for (int i = 0; i < 100_000; i++)
                GC.KeepAlive(string.Format("{0},{1}", id, score));
        }, warmup: 100));

        Report("id.ToString() + \",\" + ...", Measure(() =>
        {
            for (int i = 0; i < 100_000; i++)
                GC.KeepAlive(id.ToString() + "," + score.ToString());
        }, warmup: 100));

        Console.WriteLine();
        Console.WriteLine("  ★ .NET 6+ 中，插值字符串会被编译成 DefaultInterpolatedStringHandler，");
        Console.WriteLine("    它直接在栈上/池化缓冲区里拼，不调用 string.Format，也不装箱。");
        Console.WriteLine("    所以：用 $\"...\"，淘汰 string.Format。");
        Console.WriteLine();

        Console.WriteLine("【动手任务】");
        Console.WriteLine("  1. 打开 ILSpy，对比 s += x 和 sb.Append(x) 编译后的 IL");
        Console.WriteLine("  2. 写一个「生成 100 万行日志」的程序，对比三种写法的时间和内存峰值");
        Console.WriteLine("  3. Unity 相关：写一个每帧更新分数文本的 Update()，");
        Console.WriteLine("     用 Profiler 看 GC Alloc 那一列，然后优化到 0 B");
    }
}
