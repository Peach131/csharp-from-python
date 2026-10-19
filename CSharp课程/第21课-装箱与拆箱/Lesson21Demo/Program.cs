// ============================================================================
//  第 21 课 · 示例：装箱与拆箱（Boxing / Unboxing）
//
//  ★ 本课的核心实验方法：
//        用 GC.GetAllocatedBytesForCurrentThread() 量出【每次写法分配了多少字节】。
//        装箱是一个【会分配堆内存】的动作，所以它能被这个 API 直接抓住 ——
//        这是本课最有力的证据：不靠猜，看数字。
//
//  ★ 运行方式：
//        1. 打开 第21课.sln，把 Lesson21Demo 设为启动项目，Ctrl + F5
//        2. 或者命令行：dotnet run --project Lesson21Demo
//
//  ★ 读数须知（三条，非常重要）：
//        ① 绝对数值会随机器/运行时版本/Release-Debug 变化，
//           你要看的是【量级】和【两次测量之间的差】，不是小数点。
//        ② 必须【预热】：第一次调用含 JIT 编译开销，会把数字带偏。
//           下面的 Measure() 已经内置了 100 次预热。
//        ③ 严格对照组：想证明"某写法多花了 24 字节"，两个版本的其它
//           开销必须完全一样，唯一差异只能是你要证明的那一项。
// ============================================================================

using System.Collections;                        // ★ ArrayList 在这个命名空间里
using System.Runtime.CompilerServices;           // MethodImpl

namespace LessonPractice
{
    // ========================================================================
    //  ↓↓↓ 本课要用的类型写在这里（在 Lesson21Demo 类的【外面】）↓↓↓
    // ========================================================================

    /// <summary>
    /// 一个"什么都没重写"的 struct：没重写 ToString / GetHashCode / Equals。
    /// ★ 调用它继承自 ValueType 的这三个方法，会【把它整个装箱】。
    /// </summary>
    struct Tiny
    {
        public int X;
    }

    /// <summary>
    /// 重写了 GetHashCode 的 struct。
    /// ★ 和 Tiny 唯一的差别就是这一行 —— 这就是"严格对照组"。
    /// </summary>
    struct TinyHash
    {
        public int X;
        public override int GetHashCode() => X;
    }

    /// <summary>
    /// 实现了 IComparable&lt;TinyComparable&gt; 的 struct。
    /// ★ 注意：TinyComparable 本身【不是引用类型】，它只是"能比较"。
    /// </summary>
    struct TinyComparable : IComparable<TinyComparable>
    {
        public int X;
        public int CompareTo(TinyComparable other) => X.CompareTo(other.X);
    }

    // ========================================================================
    class Lesson21Demo
    {
        // ====================================================================
        //  ↓↓↓ 本课的辅助方法写在这里 ↓↓↓
        // ====================================================================

        /// <summary>读当前线程累计分配的字节数（一个只增不减的计数器）。</summary>
        private static long Allocated() => GC.GetAllocatedBytesForCurrentThread();

        /// <summary>
        /// 量一次"每条写法平均分配多少字节"。
        /// ★ 流程：预热 → 强制 GC（把之前的垃圾清干净，避免误算）→ 取基准 → 跑 → 再取一次。
        /// </summary>
        private static void Measure(string label, Action action, int iterations)
        {
            // ① 预热：让 JIT 先把这段代码编译成机器码，否则测到的主要是编译开销
            for (int i = 0; i < 100; i++) action();

            // ② 把此前攒下的垃圾收掉，让后面的差值只反映 action 本身
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long before = Allocated();
            for (int i = 0; i < iterations; i++) action();
            long total = Allocated() - before;

            double perIteration = (double)total / iterations;
            Console.WriteLine($"  {label,-42}{perIteration,8:F1} B/次");
        }

        /// <summary>章节标题。</summary>
        private static void Section(string title)
        {
            Console.WriteLine();
            Console.WriteLine($"---------- {title} ----------");
            Console.WriteLine();
        }

        // Unity 那一段要用的"每帧日志"目标。写成静态字段，模拟 MonoBehaviour 上的一个成员。
        private static string _labelCache = "";
        private static int _lastScore = int.MinValue;

        // ★ 单独抽成方法并禁止内联，这样才能明确地"按次测量"它
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UpdateBad(int score)
        {
            // ❌ 典型的新手写法：每帧把一个 int 塞进 object
            object boxed = score;
            // ❌ 每帧字符串拼接（"Score: " 是字面量不分配，结果字符串每次都分配）
            _labelCache = "Score: " + boxed;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UpdateGood(int score)
        {
            // 🔸 只去掉装箱，但仍然每帧拼一次字符串 —— "改了一半"
            string next = "Score: " + score;
            if (!string.Equals(next, _labelCache, StringComparison.Ordinal))
            {
                _labelCache = next;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void UpdateBest(int score)
        {
            // ✅ 两条优化都做齐：
            //    ① 值没变就一行都不干（省掉绝大多数帧）
            //    ② 拼接时【不经过 object】（省掉每帧 24 B 的盒子）
            if (score == _lastScore) return;
            _lastScore = score;
            _labelCache = "Score: " + score;
        }

        // ====================================================================
        static void Main()
        {
            Console.WriteLine("===== 第 21 课：装箱与拆箱 =====");
            Console.WriteLine($"  运行时版本：{Environment.Version}    64 位进程：{Environment.Is64BitProcess}");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 一、装箱长什么样：一张图
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 一、装箱：把值类型复制到堆上的盒子里 ----------");
            Console.WriteLine();
            Console.WriteLine("  装箱【前】：一个 int 局部变量，值就在栈上");
            Console.WriteLine();
            Console.WriteLine("        栈                     托管堆");
            Console.WriteLine("      ┌──────────┐");
            Console.WriteLine("      │  x = 42  │          （空）");
            Console.WriteLine("      └──────────┘");
            Console.WriteLine();
            Console.WriteLine("  执行  object o = x;  之后：");
            Console.WriteLine();
            Console.WriteLine("        栈                     托管堆");
            Console.WriteLine("      ┌──────────┐          ┌────────────────────┐");
            Console.WriteLine("      │  x = 42  │          │ Object Header   8B │ ← 同步块索引");
            Console.WriteLine("      ├──────────┤          ├────────────────────┤");
            Console.WriteLine("      │  o ──────┼────────► │ MethodTable 指针 8B│ ← GetType() 读这里");
            Console.WriteLine("      └──────────┘          ├────────────────────┤");
            Console.WriteLine("                            │ 数据：42        4B │ ← 值被【复制】进来");
            Console.WriteLine("                            ├────────────────────┤");
            Console.WriteLine("                            │ 填充           4B │ ← 对齐到 8 字节");
            Console.WriteLine("                            └────────────────────┘");
            Console.WriteLine("                                   合计 = 24 字节");
            Console.WriteLine();
            Console.WriteLine("  ★ 三个关键点：");
            Console.WriteLine("     ① 栈上那个 o 只有 8 字节 —— 它是一个【引用】（地址），不是值本身");
            Console.WriteLine("     ② 堆上那个盒子是一个【完整的对象】，有头部，能被 GC 管");
            Console.WriteLine("     ③ 42 被【复制】了一份。以后改 x，盒子里的 42 不会跟着变");
            Console.WriteLine();
            Console.WriteLine("  所以：装箱 = 一次堆分配 + 一次内存复制 + 一个指向盒子的引用。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 二、实测：各种写法每次分配多少字节
            // ────────────────────────────────────────────────────────────────
            Section("二、实测：哪种写法在分配内存");
            Console.WriteLine("  写法                                        分配");
            Console.WriteLine("  " + new string('─', 58));

            // ① 赋给 object —— 最直白的装箱
            Measure("object o = 42;                      ← 装箱", () =>
            {
                object o = 42;
                GC.KeepAlive(o);                    // ★ 防止 JIT 把没人用的 o 直接优化掉
            }, 20_000);

            // ② 赋给接口 —— 同样是装箱，很多人看不出这一步
            Measure("IComparable c = 42;                  ← 装箱", () =>
            {
                IComparable c = 42;
                GC.KeepAlive(c);
            }, 20_000);

            // ③ 非泛型集合 —— 灾难现场
            Measure("ArrayList.Add(42);                   ← 装箱", () =>
            {
                var list = new ArrayList(1);        // ★ 预分配容量 1，让数组扩容不参与测量
                list.Add(42);
                GC.KeepAlive(list);
            }, 20_000);

            // ④ string.Format 传值类型
            Measure("string.Format(\"{0}\", 42);            ← 装箱", () =>
            {
                string s = string.Format("{0}", 42);
                GC.KeepAlive(s);
            }, 20_000);

            // ⑤ 未重写 GetHashCode 的 struct
            //    ★ 为什么用 GetHashCode 而不是 ToString？
            //      因为 GetHashCode 返回 int，【不产生字符串】。
            //      如果测 ToString，字符串本身的分配会把装箱的 24 字节盖住 ——
            //      这是本课"测量教训"的重点，下面第五节会专门演示这个坑。
            Measure("new Tiny().GetHashCode();   ← 未重写，装箱", () =>
            {
                var t = new Tiny { X = 1 };
                GC.KeepAlive(t.GetHashCode());
            }, 20_000);

            // ⑥ struct 放进 object[] —— 数组本身在测量之外建好，每次只测写入
            var objectArray = new object[1];
            Measure("objectArray[0] = someStruct;           ← 装箱", () =>
            {
                var t = new Tiny { X = 1 };
                objectArray[0] = t;
                GC.KeepAlive(objectArray);
            }, 20_000);

            Console.WriteLine();
            Console.WriteLine("  ★ 上面每一行都是 24 B/次（或它的整数倍）—— 同一个盒子。");
            Console.WriteLine("     24 = 8（Object Header）+ 8（MethodTable 指针）+ 4（int 数据）+ 4（填充）。");
            Console.WriteLine("     这个数字和第 18 课算出来的 Empty/OneInt 对象大小完全一致：");
            Console.WriteLine("     因为盒子里装的，就是第 18 课那个最普通的对象布局。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 三、不会装箱的写法（对照）
            // ────────────────────────────────────────────────────────────────
            Section("三、实测：哪种写法【不】分配（对照）");
            Console.WriteLine("  写法                                        分配");
            Console.WriteLine("  " + new string('─', 58));

            Measure("List<int>.Add(42);                   ← 不装箱", () =>
            {
                var list = new List<int>(1);        // 容量 1，扩容不参与测量
                list.Add(42);
                GC.KeepAlive(list);
            }, 20_000);

            // ★ 严格对照组的关键：TinyHash 和 Tiny 的唯一差别就是"有没有重写 GetHashCode"
            Measure("new TinyHash().GetHashCode(); ← 已重写，不装箱", () =>
            {
                var t = new TinyHash { X = 1 };
                GC.KeepAlive(t.GetHashCode());
            }, 20_000);

            // ★ 泛型 + 约束：编译器知道 T 一定能比较，直接调用，不需要转成 object
            Measure("TinyComparable.CompareTo（泛型约束）   ← 不装箱", () =>
            {
                var a = new TinyComparable { X = 1 };
                var b = new TinyComparable { X = 2 };
                GC.KeepAlive(a.CompareTo(b));
            }, 20_000);

            // ★ struct 一旦实现了 IComparable<TinyComparable>，就能被泛型比较器直接用
            Measure("Comparer<TinyComparable>.Default.Compare ← 不装箱", () =>
            {
                var a = new TinyComparable { X = 1 };
                var b = new TinyComparable { X = 2 };
                GC.KeepAlive(Comparer<TinyComparable>.Default.Compare(a, b));
            }, 20_000);

            Console.WriteLine();
            Console.WriteLine("  ★ Tiny 和 TinyHash 的源码只差一行（有没有 override GetHashCode），");
            Console.WriteLine("     分配量却差了一整个盒子。这就是【严格对照组】的威力：");
            Console.WriteLine("     除了你要证明的那一项，其它条件必须一模一样。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 四、看清真相：盒子是【新对象】，而且每次都是新的
            // ────────────────────────────────────────────────────────────────
            Section("四、同一个值装两次盒，是两个不同的对象");
            Console.WriteLine();

            object a1 = 42;
            object a2 = 42;

            // ★ 注意这一行：故意不用插值。字符串里出现 { } 时，用普通字面量最省事，
            //   否则编译器会把它当成占位符去找参数（CS1733 之类的报错就是这么来的）。
            Console.WriteLine("  object a1 = 42;  object a2 = 42;");
            Console.WriteLine();
            Console.WriteLine($"  a1 == a2                       : {a1 == a2}");
            Console.WriteLine($"  ReferenceEquals(a1, a2)        : {ReferenceEquals(a1, a2)}   ← ★ 两个不同的盒子！");
            Console.WriteLine($"  a1.Equals(a2)                  : {a1.Equals(a2)}");
            Console.WriteLine($"  ReferenceEquals(a1, a1)        : {ReferenceEquals(a1, a1)}");
            Console.WriteLine();
            Console.WriteLine("  ★ 为什么 ReferenceEquals 是 False？");
            Console.WriteLine("     装箱的语义是「在堆上新建一个对象，把值复制进去」。");
            Console.WriteLine("     装两次盒 = 新建两个对象 = 两次 24 字节的分配。");
            Console.WriteLine("  ★ 那 a1 == a2 为什么是 True？");
            Console.WriteLine("     因为 object 的 == 走的是 Object.Equals（虚方法），");
            Console.WriteLine("     Int32 重写了它 → 比的是【值】42 == 42。");
            Console.WriteLine("  ★ 教训：判断两个装箱对象是不是同一个盒子，只能用 ReferenceEquals。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 五、拆箱：必须精确匹配
            // ────────────────────────────────────────────────────────────────
            Section("五、拆箱：必须【精确匹配】，否则抛异常");
            Console.WriteLine();

            object boxedInt = 42;

            // ✅ 盒子里装的是 Int32，用 int 取出来 —— 精确匹配
            int okInt = (int)boxedInt;
            Console.WriteLine($"  int  v = (int)(object)42;    → {okInt,-6}  ✅ 成功");

            // ❌ 盒子里装的是 Int32，你想用 long 取出来 —— 类型不匹配
            //    ★ 这是本课最容易误解的一点：拆箱【不做数值转换】！
            //      它只做一件事：检查盒子上的类型标记是不是你要的类型。
            //      是 → 把内容原样拿出来；不是 → 抛 InvalidCastException。
            try
            {
                long badLong = (long)boxedInt;
                Console.WriteLine($"  long v = (long)(object)42;   → {badLong,-6}  （不该走到这里）");
            }
            catch (InvalidCastException ex)
            {
                Console.WriteLine($"  long v = (long)(object)42;   → ❌ {ex.GetType().Name}");
                Console.WriteLine($"      运行时说：{ex.Message}");
            }

            // ✅ 想装 long 就装 long —— 盒子的类型标记是 Int64
            object boxedLong = 42L;
            long okLong = (long)boxedLong;
            Console.WriteLine($"  long v = (long)(object)42L;  → {okLong,-6}  ✅ 成功（盒子里本来就是 Int64）");

            // ✅ 想转换类型，用 Convert —— 但那是一次真正的数值转换，不是拆箱
            long converted = Convert.ToInt64(boxedInt);
            Console.WriteLine($"  long v = Convert.ToInt64(obj);→ {converted,-6}  ✅ 成功（【转换】，不是拆箱）");

            Console.WriteLine();
            Console.WriteLine("  ⚠️ 枚举也是同样的道理：盒子的类型标记是【枚举类型】，不是它的底层类型。");
            Console.WriteLine();

            object boxedDay = DayOfWeek.Monday;
            try
            {
                int badDay = (int)boxedDay;
                Console.WriteLine($"  (int)(object)DayOfWeek.Monday → {badDay}   （不该走到这里）");
            }
            catch (InvalidCastException)
            {
                Console.WriteLine("  (int)(object)DayOfWeek.Monday → ❌ InvalidCastException");
                Console.WriteLine("      必须先拆成 DayOfWeek，再转 int：");
            }
            int okDay = (int)(DayOfWeek)boxedDay;
            Console.WriteLine($"      (int)(DayOfWeek)(object)DayOfWeek.Monday = {okDay}   ✅ 成功");

            Console.WriteLine();
            Console.WriteLine("  ★ 一句话记住：拆箱不是类型转换，是「把盒子里的东西原样拿出来」。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 六、⚠️ Unity 生死问题：Update() 里的每帧装箱
            // ────────────────────────────────────────────────────────────────
            Section("六、⚠️ Unity 生死问题：Update() 每帧装箱");
            Console.WriteLine("  模拟一个 MonoBehaviour 的 Update()，60 fps 跑 60 秒 = 3600 帧。");
            Console.WriteLine();

            const int frames = 3600;
            const int changeEvery = 20;                  // 分数每 20 帧变一次 → 3600/20 = 180 次变化

            // ---- 坏写法 ----
            _labelCache = "";
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            int gen0BeforeBad = GC.CollectionCount(0);
            long beforeBad = Allocated();

            for (int frame = 0; frame < frames; frame++)
            {
                UpdateBad((frame / changeEvery) % 100);
            }

            long badAlloc = Allocated() - beforeBad;
            int badGc = GC.CollectionCount(0) - gen0BeforeBad;

            Console.WriteLine("  ❌ 坏写法：每帧装箱 + 每帧拼字符串");
            Console.WriteLine("       object boxed = score;   _label = \"Score: \" + boxed;");
            Console.WriteLine($"       分配        : {badAlloc,12:N0} 字节  （{badAlloc / 1024.0,8:F1} KB）");
            Console.WriteLine($"       每帧平均    : {(double)badAlloc / frames,12:F1} 字节  ← 盒子 24 + 字符串 24");
            Console.WriteLine($"       Gen0 GC 次数: {badGc,12} 次");
            Console.WriteLine();

            // ---- 半好写法：只去掉装箱 ----
            _labelCache = "";
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            int gen0BeforeGood = GC.CollectionCount(0);
            long beforeGood = Allocated();

            for (int frame = 0; frame < frames; frame++)
            {
                UpdateGood((frame / changeEvery) % 100);
            }

            long goodAlloc = Allocated() - beforeGood;
            int goodGc = GC.CollectionCount(0) - gen0BeforeGood;

            Console.WriteLine("  🔸 半好写法：只去掉装箱（直接 \"Score: \" + score，不经过 object），");
            Console.WriteLine("                但仍然【每帧】拼一次字符串");
            Console.WriteLine($"       分配        : {goodAlloc,12:N0} 字节  （{goodAlloc / 1024.0,8:F1} KB）");
            Console.WriteLine($"       每帧平均    : {(double)goodAlloc / frames,12:F1} 字节  ← 只剩字符串那份");
            Console.WriteLine($"       Gen0 GC 次数: {goodGc,12} 次");
            Console.WriteLine();

            // ---- 最好写法：两条优化都做 ----
            _labelCache = "";
            _lastScore = int.MinValue;
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            int gen0BeforeBest = GC.CollectionCount(0);
            long beforeBest = Allocated();

            for (int frame = 0; frame < frames; frame++)
            {
                UpdateBest((frame / changeEvery) % 100);
            }

            long bestAlloc = Allocated() - beforeBest;
            int bestGc = GC.CollectionCount(0) - gen0BeforeBest;

            Console.WriteLine("  ✅ 最好写法：值没变就直接 return + 拼接不经过 object");
            Console.WriteLine($"       分配        : {bestAlloc,12:N0} 字节  （{bestAlloc / 1024.0,8:F1} KB）");
            Console.WriteLine($"       每帧平均    : {(double)bestAlloc / frames,12:F1} 字节  ← 只有 180 次变化在花钱");
            Console.WriteLine($"       Gen0 GC 次数: {bestGc,12} 次");
            Console.WriteLine();

            Console.WriteLine($"  → 坏 → 半好：省了 {(badAlloc - goodAlloc):N0} 字节（那 24 B/帧 的盒子）");
            Console.WriteLine($"  → 半好 → 最好：又省了 {(goodAlloc - bestAlloc):N0} 字节（那 3600 次里多余的 3420 次）");
            Console.WriteLine($"  → 总计省下 {(badAlloc - bestAlloc) / 1024.0:F1} KB，GC 次数从 {badGc} 降到 {bestGc}。");
            Console.WriteLine();
            Console.WriteLine("  ★ 注意「半好」这一步：很多人以为「不装箱就完事了」，");
            Console.WriteLine("     但字符串照样每帧分配 —— 优化做一半，垃圾只少了一半。");
            Console.WriteLine("     两条优化是【独立】的，必须都做。");
            Console.WriteLine();
            Console.WriteLine("  ★ 这只是一行日志、一个 int。真实项目里几十个这样的调用，");
            Console.WriteLine("     就是玩家能感觉到的、每隔几秒卡一下的元凶。");
            Console.WriteLine();
            Console.WriteLine("  ★ Unity 为什么比 .NET 更不能容忍这件事？（第 26 课细讲）");
            Console.WriteLine("     Unity 用的是 Boehm GC：不分代、不压缩。");
            Console.WriteLine("     .NET 的分代 GC 里，这种刚生就死的垃圾几乎不要钱（Gen0 极快）；");
            Console.WriteLine("     Unity 里它们全堆在一起，触发一次就是明显的掉帧。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 七、背下来：这份清单看到就要警觉
            // ────────────────────────────────────────────────────────────────
            Section("七、背下来：会装箱的地方（看到就警觉）");
            Console.WriteLine();
            Console.WriteLine("  ❌ 会装箱：");
            Console.WriteLine("      object o = 42;                    把值类型赋给 object");
            Console.WriteLine("      IComparable c = 42;               把值类型赋给接口");
            Console.WriteLine("      new ArrayList().Add(42);          非泛型集合 ArrayList / Hashtable");
            Console.WriteLine("      string.Format(\"{0}\", 42);          传值类型给 params object[]");
            Console.WriteLine("      struct 未重写 ToString() 时调用它  继承自 ValueType 的三个方法");
            Console.WriteLine("      object[] arr = { 1, 2, 3 };       值类型放进 object[]");
            Console.WriteLine("      对 struct 用非泛型接口变量调用方法  例如 IComparable c = s; c.CompareTo(...)");
            Console.WriteLine();
            Console.WriteLine("  ✅ 不装箱：");
            Console.WriteLine("      List<int>.Add(42);                泛型容器，int 内联在数组里");
            // ★ 这一行故意用普通字面量：字符串里有 {42}，写进插值里会被当成占位符，
            //   要么转义成 {{42}}，要么就像这里一样干脆不用插值。
            Console.WriteLine("      $\"{42}\"                            插值字符串，编译器按类型直接格式化");
            Console.WriteLine("      struct 重写过的 ToString/GetHashCode/Equals");
            Console.WriteLine("      T 上有泛型约束时的方法调用         where T : IComparable<T>");
            Console.WriteLine("      struct 自己的强类型 Equals(T) 重载  IEquatable<T>");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 八、动手改坏它
            // ────────────────────────────────────────────────────────────────
            Section("八、动手：自己把它改坏，再看数字");
            Console.WriteLine();
            Console.WriteLine("  ★ 实验 1：把「半好写法」改回装箱，看那 24 B/帧 回来");
            Console.WriteLine("      找到上面 UpdateGood 里的那行：");
            Console.WriteLine("          string next = \"Score: \" + score;");
            Console.WriteLine("      改成：");
            Console.WriteLine("          object boxed = score;   string next = \"Score: \" + boxed;");
            Console.WriteLine("      再跑一遍。「半好」那一段的分配量会从 24 B/帧 变成 48 B/帧，");
            Console.WriteLine("      —— 因为每个 int 又开始造盒子了。这就是装箱的价格。");
            Console.WriteLine();
            Console.WriteLine("  ★ 实验 2：把「最好写法」的护栏拆掉");
            Console.WriteLine("      把 UpdateBest 里的这两行注释掉：");
            Console.WriteLine("          if (score == _lastScore) return;");
            Console.WriteLine("          _lastScore = score;");
            Console.WriteLine("      再跑一遍。「最好」那一段会立刻退化成「半好」的 24 B/帧 ——");
            Console.WriteLine("      证明【变化才重建】这条优化是独立生效的，不是白写的。");
            Console.WriteLine();
            Console.WriteLine("  ★ 实验 3：给 struct 加一个没重写的 ToString");
            Console.WriteLine("      把 TinyHash 的 GetHashCode 注释掉，换成：");
            Console.WriteLine("          public override string ToString() => \"\";");
            Console.WriteLine("      问题是：为什么改成 ToString 之后，数字会变得不好读？");
            Console.WriteLine("      → 因为字符串本身的分配混进来了。这正是第五节讲的测量陷阱。");
            Console.WriteLine("      （想让数字继续干净，就去测 GetHashCode —— 它返回 int，不产生字符串。）");
            Console.WriteLine();
            Console.WriteLine("  ★ 实验 4：用 ILSpy 找 box 指令（最硬的证据）");
            Console.WriteLine("      打开 bin\\Debug\\net9.0\\Lesson21Demo.dll，找到 UpdateBad，");
            Console.WriteLine("      你会看到一条 box [System.Runtime]System.Int32。");
            Console.WriteLine("      再去 UpdateGood / UpdateBest 里找 —— 都没有 box。");
            Console.WriteLine("      ★ 分配量是【结果】，box 指令是【原因】。能看到原因，才算真懂。");
            Console.WriteLine();
            Console.WriteLine("  ★ 实验 5：把预热去掉");
            Console.WriteLine("      把 Measure() 里那段预热循环注释掉再跑。");
            Console.WriteLine("      第一次测量的数字会明显变大 —— 那是 JIT 编译的分配，不是装箱。");
            Console.WriteLine();

            Console.WriteLine("===== 示例结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
