namespace Experiments;

/// <summary>
/// 第 3 周 · 实验 7：引用相等 vs 值相等
/// </summary>
/// <remarks>
/// 结论：
///   1. == 和 Equals 对 class 默认都是「引用相等」（比地址）。
///   2. string 是特例：== 被重载成了值比较。
///   3. 自定义 class 想按值比较，必须重写 Equals 和 GetHashCode（两者必须成对重写）。
///   4. record 会自动生成值语义的 Equals / GetHashCode / ToString —— 省事且正确。
///   5. class vs struct 的默认行为完全不同：
///        - class 默认引用相等
///        - struct 默认【逐字段值比较】（通过反射实现，很慢！所以要自己重写）
/// </remarks>
public static class E07_Equality
{
    private class PlainClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private class ValueClass : IEquatable<ValueClass>
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";

        public bool Equals(ValueClass? other)
            => other is not null && Id == other.Id && Name == other.Name;

        public override bool Equals(object? obj) => Equals(obj as ValueClass);

        public override int GetHashCode() => HashCode.Combine(Id, Name);
    }

    private record PersonRecord(int Id, string Name);

    private struct PlainStruct
    {
        public int Id;
        public string Name;
    }

    private static void Row(string label, object? a, object? b)
    {
        Console.WriteLine($"  {label,-52} {a,-8} {b}");
    }

    public static void Run()
    {
        Console.WriteLine("【实验 A：各种类型的默认相等行为】");
        Console.WriteLine();
        Console.WriteLine($"  {"比较方式",-52} {"== ",8} {"Equals()"}");
        Console.WriteLine("  " + new string('─', 72));

        var c1 = new PlainClass { Id = 1, Name = "A" };
        var c2 = new PlainClass { Id = 1, Name = "A" };
        Row("普通 class（字段相同，不同实例）", c1 == c2, c1.Equals(c2));

        var c3 = c1;
        Row("普通 class（同一个实例的两个引用）", c1 == c3, c1.Equals(c3));

        var v1 = new ValueClass { Id = 1, Name = "A" };
        var v2 = new ValueClass { Id = 1, Name = "A" };
        Row("重写了 Equals 的 class", v1 == v2, v1.Equals(v2));
        Console.WriteLine("      ↑ 注意 == 仍然是 False！因为重写 Equals 不会自动重载 == 运算符");
        Console.WriteLine();

        var r1 = new PersonRecord(1, "A");
        var r2 = new PersonRecord(1, "A");
        Row("record（自动生成值语义）", r1 == r2, r1.Equals(r2));

        var s1 = new PlainStruct { Id = 1, Name = "A" };
        var s2 = new PlainStruct { Id = 1, Name = "A" };
        Row("struct（默认为逐字段比较）", s1.Equals(s2), s1.Equals(s2));
        Console.WriteLine("      ↑ struct 默认就是值比较（但通过反射实现，很慢）");
        Console.WriteLine();

        string str1 = "hello";
        string str2 = "hel" + "lo";                        // 编译期常量折叠
        string str3 = string.Concat("hel", "lo");          // 运行时拼接
        Row("string 字面量 vs 编译期拼接", str1 == str2, str1.Equals(str2));
        Row("string vs 运行时拼接", str1 == str3, str1.Equals(str3));
        Console.WriteLine("      ↑ == 和 Equals 都是 True（值比较）");
        Console.WriteLine($"      但 ReferenceEquals(str1, str3) = {ReferenceEquals(str1, str3)}  ← 不同对象！");
        Console.WriteLine();

        Console.WriteLine("【实验 B：Equals 和 GetHashCode 必须成对重写】");
        Console.WriteLine();

        var bad1 = new BadKey { Id = 1 };
        var bad2 = new BadKey { Id = 1 };
        Console.WriteLine($"  BadKey（只重写 Equals）:");
        Console.WriteLine($"    bad1.Equals(bad2)                     = {bad1.Equals(bad2)}   ✅ 相等");
        Console.WriteLine($"    bad1.GetHashCode() == bad2.GetHashCode() = {bad1.GetHashCode() == bad2.GetHashCode()}   ❌ 哈希不同！");

        var dict = new Dictionary<BadKey, string> { [bad1] = "value" };
        var found = dict.TryGetValue(bad2, out var v);
        Console.WriteLine($"    用 bad2 去 Dictionary 里查 bad1 存的键 → {(found ? "找到: " + v : "❌ 找不到！")}");
        Console.WriteLine("    ★ 这就是「相等必同哈希」契约被违反的后果。Dictionary 先比哈希，");
        Console.WriteLine("      哈希不同就直接判定为不同的键，根本不会去调 Equals。");
        Console.WriteLine();

        var good1 = new GoodKey { Id = 1 };
        var good2 = new GoodKey { Id = 1 };
        var dict2 = new Dictionary<GoodKey, string> { [good1] = "value" };
        var found2 = dict2.TryGetValue(good2, out var goodValue);
        Console.WriteLine($"  GoodKey（正确重写了两个）:");
        Console.WriteLine($"    用 good2 查 good1 存的键 → {(found2 ? "✅ 找到: " + goodValue : "找不到")}");
        Console.WriteLine();

        Console.WriteLine("  ★ 省事方案：直接用 record，编译器帮你生成正确的实现：");
        var rd = new Dictionary<PersonRecord, string> { [new PersonRecord(1, "A")] = "ok" };
        Console.WriteLine($"    用新 record 实例查 Dictionary → {rd[new PersonRecord(1, "A")]}");
        Console.WriteLine();

        Console.WriteLine("【实验 C：struct 默认 Equals 的性能陷阱】");
        Console.WriteLine();

        var structs = new List<PlainStruct>();
        for (int i = 0; i < 1_000_000; i++)
            structs.Add(new PlainStruct { Id = i, Name = "x" });

        var target = new PlainStruct { Id = 999_999, Name = "x" };

        var sw = System.Diagnostics.Stopwatch.StartNew();
        bool foundInList = structs.Contains(target);
        sw.Stop();
        var slowMs = sw.Elapsed.TotalMilliseconds;

        var noReflection = new NoReflectStruct { Id = 999_999, Name = "x" };
        var fastList = new List<NoReflectStruct>();
        for (int i = 0; i < 1_000_000; i++)
            fastList.Add(new NoReflectStruct { Id = i, Name = "x" });

        sw.Restart();
        bool foundFast = fastList.Contains(noReflection);
        sw.Stop();
        var fastMs = sw.Elapsed.TotalMilliseconds;

        Console.WriteLine($"  未重写 Equals 的 struct  (反射实现): {slowMs,8:F1} ms  (找到={foundInList})");
        Console.WriteLine($"  已重写 Equals 的 struct  (直接比较): {fastMs,8:F1} ms  (找到={foundFast})");
        Console.WriteLine($"  → 快了 {slowMs / Math.Max(0.01, fastMs):F1} 倍");
        Console.WriteLine();
        Console.WriteLine("  ★ 未重写的 struct 用 ValueType.Equals，它靠【反射】逐字段比较。");
        Console.WriteLine("    struct 要用在集合里，必须重写 Equals / GetHashCode（或用 record struct）。");
        Console.WriteLine();

        Console.WriteLine("【动手任务】");
        Console.WriteLine("  1. 打开 ILSpy，看 record 编译后生成了哪些方法（Equals / GetHashCode / ToString / ==）");
        Console.WriteLine("  2. 给 PlainClass 加上 IEquatable<T> 和 == 运算符重载，让 v1 == v2 也返回 True");
        Console.WriteLine("  3. 记住决策：需要值语义的数据 → 用 record 或 record struct，别自己手写");
    }

    /// <summary>
    /// 故意只重写 Equals，不重写 GetHashCode —— 本实验要演示的【错误做法】。
    /// 编译器会为此发出 CS0659 警告（这正是重点：编译器会主动提醒你），
    /// 这里临时关掉，让「0 警告」成为你日常编码的基线。
    /// </summary>
#pragma warning disable CS0659
    private class BadKey
    {
        public int Id { get; set; }
        public override bool Equals(object? obj) => obj is BadKey k && k.Id == Id;
        // 故意不重写 GetHashCode() —— 后果见实验 B
    }
#pragma warning restore CS0659

    private class GoodKey
    {
        public int Id { get; set; }
        public override bool Equals(object? obj) => obj is GoodKey k && k.Id == Id;
        public override int GetHashCode() => Id.GetHashCode();
    }

    private struct NoReflectStruct : IEquatable<NoReflectStruct>
    {
        public int Id;
        public string Name;

        public bool Equals(NoReflectStruct other) => Id == other.Id && Name == other.Name;
        public override bool Equals(object? obj) => obj is NoReflectStruct s && Equals(s);
        public override int GetHashCode() => HashCode.Combine(Id, Name);
    }
}
