// ============================================================================
//  第 21 课 · 练习：装箱与拆箱
//
//  ★ 本课重点：
//     1. 装箱 = 把值类型【复制】到堆上的一个盒子里（至少 24 字节）
//     2. 会装箱的地方：赋给 object、赋给接口、非泛型集合、string.Format、
//        未重写 ToString 的 struct、值类型放进 object[]
//     3. 不会装箱：List<int>、$"{42}"、重写过那三个方法的 struct、泛型约束
//     4. 拆箱必须【类型精确匹配】，(long)(object)42 抛 InvalidCastException
//     5. ⚠️ Unity：Update() 里每帧装箱 → 每秒 60 次分配 → GC 卡顿
//
//  ★ 本课唯一的测量工具：
//        GC.GetAllocatedBytesForCurrentThread()
//     它返回【当前线程累计分配了多少字节】，只增不减。
//     在代码前后各读一次，相减 = 这段代码产生了多少垃圾。
//     装箱是"堆分配"，所以它藏不住 —— 这是本课最有力的证据。
//
//  ★ 三条测量纪律（不遵守就会得出错误结论）：
//     ① 先【预热】：第一次调用含 JIT 编译开销，会把数字带偏
//     ② 先 GC.Collect()：把之前攒的垃圾清掉，否则会算到你头上
//     ③ 【严格对照组】：两个版本除了你要证明的那一项，其它必须完全一样
// ============================================================================

using System.Collections;                        // ★ ArrayList 在这个命名空间里
using System.Diagnostics;                        // Stopwatch

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ 本课要用的类型写在这里（在 Lesson21Practice 类的【外面】）↓↓↓
    // ================================================================

    /// <summary>
    /// 一个"什么都没重写"的 struct。
    /// ★ 它没有重写 ToString —— 所以对它调用 ToString() 会【装箱】，
    ///   走的是 ValueType.ToString()（用反射拿类型名）。
    /// </summary>
    struct Player
    {
        public string Name;      // ★ 引用类型字段没关系，这里关心的是 struct 本身
        public int Health;

        public Player(string name, int health)
        {
            Name = name;
            Health = health;
        }
    }

    /// <summary>
    /// 重写了 ToString 的 struct。
    /// ★ 和 Player 唯一的差别就是下面这一行 —— 这就是"严格对照组"。
    /// </summary>
    struct PlayerGood
    {
        public string Name;
        public int Health;

        public PlayerGood(string name, int health)
        {
            Name = name;
            Health = health;
        }

        public override string ToString() => $"{Name}({Health})";
    }

    class Lesson21Practice
    {
        // ================================================================
        // ↓↓↓ 本课的辅助方法写在这里 ↓↓↓
        // ================================================================

        /// <summary>读当前线程累计分配的字节数。</summary>
        static long Allocated() => GC.GetAllocatedBytesForCurrentThread();

        /// <summary>
        /// 量"每执行一次 action，平均分配多少字节"。TODO 1 会用到它。
        /// ★ 注意三步：预热 → 收垃圾 → 取基准再跑。
        /// </summary>
        static double MeasureAlloc(Action action, int iterations)
        {
            for (int i = 0; i < 100; i++) action();          // ① 预热，让 JIT 先编译完

            GC.Collect();                                     // ② 清掉之前的垃圾
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long before = Allocated();                        // ③ 取基准
            for (int i = 0; i < iterations; i++) action();
            long total = Allocated() - before;

            return (double)total / iterations;
        }

        // TODO 5 要用的"每帧日志"缓存（模拟 MonoBehaviour 上的两个成员变量）
        //
        // ⚠️ 【关于 CS0414 警告 —— 请先读这段，别以为自己写错了】
        //   什么都没做的时候，这个项目会报 **1 条警告**：
        //       CS0414: 字段"_lastScore"已被赋值，但从未使用过它的值
        //
        //   这不是 bug，是【故意留的提示】：
        //     · _labelCache 不报 —— 因为 TODO 5 的预期输出示例里会【读】它
        //     · _lastScore 报   —— 因为它只在 TODO 5 的 if 判断里被读，
        //                          你做完 TODO 5，这条警告就自己消失了
        //
        //   ★ 课程要求的是"**你写完之后** 0 警告"，不是"骨架本身 0 警告"。
        //     在这门课里，警告一直是"你还没写完的作业清单"——这句在第 02 课就说过。
        //     所以：看到这条警告 = 提醒你 TODO 5 还没做，不用去动它。
        static string _labelCache = "";
        static int _lastScore = int.MinValue;

        static void Main()
        {
            Console.WriteLine("===== 第 21 课练习：装箱与拆箱 =====");
            Console.WriteLine();

            // ----------------------------------------------------------------
            // ★ 开胃菜：先看测量本身长什么样（这一小段不用改，直接跑）
            //
            //   下面用 MeasureAlloc 量了两条几乎一样的语句，唯一差别是
            //   第一条把 int 赋给 int，第二条把 int 赋给 object。
            //   20,000 次之后，两个数字会把你后面要学的全部内容先剧透一遍。
            // ----------------------------------------------------------------
            double noBox = MeasureAlloc(() =>
            {
                int v = 42;
                GC.KeepAlive(v);
            }, 20_000);

            double oneBox = MeasureAlloc(() =>
            {
                object o = 42;
                GC.KeepAlive(o);
            }, 20_000);

            Console.WriteLine("【开胃菜】同样一条赋值，只差一个类型：");
            Console.WriteLine($"    int    v = 42;     {noBox,8:F1} B/次");
            Console.WriteLine($"    object o = 42;     {oneBox,8:F1} B/次   ← 多出来的就是那个盒子");
            Console.WriteLine("    下面 TODO 1 就是把这件事一个写法一个写法的量清楚。");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 量化一次装箱：它到底值多少字节
            // ================================================================
            // 【要求】
            //   用上面的 MeasureAlloc 依次测出下面 5 种写法的"每次分配字节数"，
            //   打印成一张表（B/次，保留 1 位小数）：
            //     ① object o = 42;
            //     ② IComparable c = 42;
            //     ③ (object)42 之后再拆回来
            //     ④ List<int> l = new List<int>(1); l.Add(42);
            //     ⑤ string s = $"{42}";
            //   然后打印一句结论：哪几种是 24 B/次，哪几种是 0。
            //
            // 【预期输出】（尾数会随机器变化，看量级）
            //     object o = 42;                  24.0 B/次
            //     IComparable c = 42;             24.0 B/次
            //     装箱再拆箱 (int)(object)42      24.0 B/次
            //     List<int>.Add(42);               0.0 B/次
            //     $"{42}"                          0.0 B/次
            //     → 24 B/次 就是那个盒子；0 B/次 说明这条路径根本没造盒子。
            //
            // 【提示】
            //     double per1 = MeasureAlloc(() =>
            //     {
            //         object o = 42;
            //         GC.KeepAlive(o);            // ★ 不写这句，JIT 可能把 o 直接优化掉
            //     }, 20_000);
            //     Console.WriteLine($"{"object o = 42;",-32}{per1,8:F1} B/次");
            //
            //     // ★ 为什么是 24 字节？
            //     //   8（Object Header）+ 8（MethodTable 指针）+ 4（int 数据）+ 4（填充）= 24
            //     //   和第 18 课算出来的对象大小是同一套规则。
            //     // ★ 为什么 $"{42}" 是 0？
            //     //   .NET 6 以后，插值字符串在编译期就被改写成
            //     //   DefaultInterpolatedStringHandler.AppendFormatted(int) ——
            //     //   参数类型是 int，不是 object，所以不装箱。
            //
            // 【考察点】
            //   · 装箱是一个可测量的堆分配，不是抽象概念
            //   · 预热 / GC.Collect / GC.KeepAlive 在测量里各起什么作用
            //   · 24 这个数字的来源（对照第 18 课的对象布局）

            Console.WriteLine("--- TODO 1 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 2 ｜ List<int> vs ArrayList：把它测到没有借口
            // ================================================================
            // 【要求】
            //   往集合里各 Add 100,000 个 int，测出两者的【分配量】和【耗时】：
            //     ① ArrayList（非泛型集合，Add 的参数是 object）→ 每次 Add 都装箱
            //     ② List<int>（泛型集合，Add 的参数就是 int）→ 不装箱
            //   打印：总分配 MB、平均每个元素的字节数、耗时 ms、Gen0 GC 次数。
            //
            // ★ 关键：两边【都要】在构造时把容量一次预定好：
            //       var oldList = new ArrayList(100_000);
            //       var newList = new List<int>(100_000);
            //   否则测到的主要是"数组翻倍扩容 + 搬运"，不是装箱。
            //   这是本课最重要的一次"严格对照组"。
            //
            // 【预期输出】（数字会不同）
            //     往集合里 Add 100,000 个 int：
            //       ArrayList（装箱）  : 分配 2.4 MB, 平均 24.0 B/元素, 耗时 3.1 ms, Gen0 GC 1 次
            //       List<int>（不装箱）: 分配 0.4 MB, 平均  4.0 B/元素, 耗时 0.6 ms, Gen0 GC 0 次
            //       → 差值 2.0 MB ÷ 100,000 = 20.4 B/元素  ← 这个额外开销就是盒子
            //
            // 【提示】
            //     const int N = 100_000;
            //
            //     GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            //     int gcBefore = GC.CollectionCount(0);          // Gen0 回收次数
            //     long before = Allocated();
            //     var sw = Stopwatch.StartNew();
            //
            //     var oldList = new ArrayList(N);
            //     for (int i = 0; i < N; i++) oldList.Add(i);    // ★ 每次 Add 都装箱
            //
            //     sw.Stop();
            //     long allocOld = Allocated() - before;
            //     int gcOld = GC.CollectionCount(0) - gcBefore;
            //
            //     // List<int> 那边照抄一遍，只改集合类型
            //     // ★ 平均每元素字节 = allocOld / (double)N
            //     // ★ 最后必须用一下两个 list，否则会有"变量已赋值但从未使用"的警告：
            //     //     GC.KeepAlive(oldList);  GC.KeepAlive(newList);
            //
            // 【考察点】
            //   · 泛型容器为什么快：不是语法糖，是【类型特化】（第 15 课）
            //   · 为什么"预分配容量"是让对照实验干净的关键
            //   · 会看 Gen0 GC 次数：分配量真正伤害的是 GC

            Console.WriteLine("--- TODO 2 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 3 ｜ 找出"隐藏的装箱"：自定义 struct 进了 object[]
            // ================================================================
            // 【要求】
            //   1. 造一个 Player struct：new Player("张三", 100)
            //   2. 用 MeasureAlloc 测三种写法的分配量：
            //        ① p.ToString()          （p 是 Player，未重写 ToString）
            //        ② g.ToString()          （g 是 PlayerGood，已重写 ToString）
            //        ③ boxed[0] = p;         （把 struct 塞进 object[]）
            //   3. 把同一个 p 分别装进 object[] 的第 0 格和第 1 格，
            //      用 ReferenceEquals 证明"两次装箱 = 两个不同的盒子"。
            //   4. 打印结论：①② 差多少字节，这个差从哪来。
            //
            // 【预期输出】（数字会不同）
            //     Player.ToString()（未重写）  : 24.0 B/次   ← 装箱
            //     PlayerGood.ToString()（已重写）: 0.0 B/次   ← 不装箱
            //     objectArray[0] = p;          : 24.0 B/次   ← 装箱
            //     → 未重写的 struct 调用 ToString 会装箱，固定多花一个盒子
            //     两次装箱是两个盒子吗？False   ← False 才对！
            //
            // 【提示】
            //     var p = new Player("张三", 100);
            //     var g = new PlayerGood("李四", 80);
            //     var boxed = new object[2];        // ★ 数组在测量【外面】建好
            //
            //     double perBad = MeasureAlloc(() =>
            //     {
            //         GC.KeepAlive(p.ToString());   // ★ p 是 struct，ToString 是继承来的 → 装箱
            //     }, 20_000);
            //
            //     // ⚠️ 严格对照组的坑：两边都会【新建字符串】，字符串本身的分配会
            //     //    混进测量结果。所以这里先别管绝对数字，看的是"未重写"比"已重写"
            //     //    多出来的那一份 —— 那一份是盒子。
            //     //    （想看到最干净的 24 / 0，请把测的方法换成 GetHashCode ——
            //     //      它返回 int，不产生字符串。参考答案 TODO 3 里给了完整写法。）
            //
            //     boxed[0] = p;                      // ★ 装箱发生在这里
            //     boxed[1] = p;                      // ★ 又装一次
            //     Console.WriteLine($"两次装箱是两个盒子吗？{ReferenceEquals(boxed[0], boxed[1])}");
            //
            // 【考察点】
            //   · struct 继承自 ValueType 的 ToString/GetHashCode/Equals 都会装箱
            //   · 装箱是"新建对象"，所以同一个值装两次得到两个盒子
            //   · 比较装箱对象只能用 ReferenceEquals（== 会比值，看不出来）

            Console.WriteLine("--- TODO 3 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 4 ｜ 拆箱类型不匹配：(long)(object)42 会怎样
            // ================================================================
            // 【要求】
            //   1. 造 object boxed = 42; 用 (int) 取出来 → 打印成功
            //   2. 用 (long) 取出来，放在 try/catch (InvalidCastException) 里
            //      → 打印"抛了什么异常"和异常消息
            //   3. 造 object boxedLong = 42L; 用 (long) 取出来 → 打印成功
            //   4. 用 Convert.ToInt64(boxed) 取出来 → 打印成功，并说明它和拆箱的区别
            //   5. 加一个枚举的坑：object o = DayOfWeek.Monday;
            //      (int)o 会抛异常，(int)(DayOfWeek)o 才是对的。两行都实测一下。
            //
            // 【预期输出】
            //     (int)(object)42            → 42        ✅ 成功
            //     (long)(object)42           → ❌ InvalidCastException
            //         运行时说：Unable to cast object of type 'System.Int32' to type 'System.Int64'.
            //     (long)(object)42L          → 42        ✅ 成功
            //     Convert.ToInt64(boxed)     → 42        ✅ 成功（这是【转换】，不是拆箱）
            //     (int)(object)DayOfWeek.Monday       → ❌ InvalidCastException
            //     (int)(DayOfWeek)(object)DayOfWeek.Monday → 1    ✅ 成功
            //
            // 【提示】
            //     object boxed = 42;
            //     int ok = (int)boxed;
            //     Console.WriteLine($"{"(int)(object)42",-34}→ {ok,-8}  ✅ 成功");
            //
            //     try
            //     {
            //         long bad = (long)boxed;
            //         Console.WriteLine(bad);              // 不该走到这里
            //     }
            //     catch (InvalidCastException ex)
            //     {
            //         Console.WriteLine($"{"(long)(object)42",-34}→ ❌ {ex.GetType().Name}");
            //         Console.WriteLine($"    运行时说：{ex.Message}");
            //     }
            //
            //     // ★ 拆箱不是类型转换！它只检查"盒子上的类型标记"是不是你要的类型：
            //     //   是 → 把内容原样拿出来
            //     //   不是 → 直接抛 InvalidCastException（没有任何数值转换的余地）
            //     // ★ 枚举的盒子上的标记是【枚举类型】，不是它的底层 int。
            //     // ★ 想真的转换数值，用 Convert.ToInt64 —— 但那会做装箱/拆箱 + 一次转换。
            //
            // 【考察点】
            //   · 拆箱 = 类型检查 + 取值，两步，没有第三步
            //   · 枚举装箱后不能直接拆成 int（这是新手高频踩的坑）
            //   · "拆箱"和"类型转换"是两件不同的事

            Console.WriteLine("--- TODO 4 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 5 ｜ ★ 综合（本课考核）：把一个"每帧装箱"的循环优化到 0 分配
            // ================================================================
            // 【背景】你在一家做 Unity 游戏的公司。策划说"分数显示怎么老卡"。
            //         你打开 Profiler：GC Alloc 每帧几十字节。原因就在下面这段。
            //
            // 【要求】
            //   1. 先写一个【坏版本】：循环 3600 帧（60 fps 跑 60 秒），每帧执行
            //          object boxed = score;
            //          _labelCache = "Score: " + boxed;
            //      量出：总分配字节、每帧平均字节、Gen0 GC 次数。
            //
            //   2. 再写一个【半好版本】（★ 强烈建议做这一档，它是本题的重点）：
            //          只去掉装箱，但仍然【每帧】拼一次字符串。
            //      量出同样的三个数字 —— 你会发现垃圾只少了一半！
            //
            //   3. 最后写【最好版本】：
            //          · 只在 score 真的变化时才重建字符串
            //          · 拼接时【不要】经过 object
            //      量出同样的三个数字。
            //
            //   4. 打印三档对比表 + 一句结论：
            //          · 坏版本每帧的两个对象各值多少字节
            //          · 半好版本证明了什么（为什么"只做一半优化"是陷阱）
            //          · 最好版本省了多少
            //          · 为什么这在 Unity 里是"生死问题"
            //
            //   5. ★ 额外要求（本课考核点）：写两句注释，分别指出坏版本里
            //      【哪一行】产生了【什么类型】的对象。
            //
            // 【预期输出】（数字会不同）
            //     模拟 Update()：3600 帧，score 每 20 帧变一次
            //
            //     ❌ 坏版本（每帧装箱 + 每帧拼字符串）
            //         总分配      : 172,800 字节  （168.8 KB）
            //         每帧平均    :      48.0 字节  ← 24 B 盒子 + 24 B 字符串
            //         Gen0 GC 次数: 1
            //
            //     🔸 半好版本（去掉装箱，但仍每帧拼字符串）
            //         总分配      :  86,400 字节  （ 84.4 KB）
            //         每帧平均    :      24.0 字节  ← 只剩字符串那份
            //         Gen0 GC 次数: 0
            //
            //     ✅ 最好版本（值没变就 return + 不经过 object）
            //         总分配      :   4,320 字节  （  4.2 KB）
            //         每帧平均    :       1.2 字节
            //         Gen0 GC 次数: 0
            //
            //     → 坏 → 半好：省下   86,400 字节（那 24 B/帧 的盒子）
            //     → 半好 → 最好：又省下 82,080 字节（多余的 3420 次重建）
            //     → 总计省下 168,480 字节（164.5 KB），GC 次数 1 → 0
            //
            // 【提示】
            //     常量：const int frames = 3600;  const int changeEvery = 20;
            //     帧里这样造 score（保证每 20 帧变一次）：
            //         for (int frame = 0; frame < frames; frame++)
            //         {
            //             int score = (frame / changeEvery) % 100;
            //             ... 你的每帧逻辑 ...
            //         }
            //
            //     坏版本骨架：
            //         _labelCache = "";
            //         GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            //         int gcBefore = GC.CollectionCount(0);
            //         long before = Allocated();
            //         for (...) { object boxed = score; _labelCache = "Score: " + boxed; }
            //         long badAlloc = Allocated() - before;
            //
            //     好版本骨架（三个要点）：
            //         _labelCache = "";  _lastScore = int.MinValue;
            //         ...
            //         for (...)
            //         {
            //             if (score != _lastScore)          // ★ 要点 1：只在变化时才干活
            //             {
            //                 _lastScore = score;
            //                 object boxed = score;         // ← 先保留这句，看数字
            //                 _labelCache = "Score: " + boxed;
            //             }
            //         }
            //         // ★ 要点 2：把上面那句 object boxed = score; 删掉，直接
            //         //            _labelCache = "Score: " + score;
            //         //            再看一次数字 —— 每帧的 24 B 盒子消失了。
            //         // ★ 要点 3：_labelCache / _lastScore 是【成员变量】（本类里已经声明好了），
            //         //            不是每帧 new 出来的局部变量。
            //         // ★ 最后用一下 _labelCache，否则它可能被优化掉：
            //         //            Console.WriteLine($"  最后一帧的日志：{_labelCache}");
            //
            // 【考察点】
            //   · 这是本课的最终目标：看到"每帧分配"就知道怎么改
            //   · 两条独立的优化：① 减少次数（变化才重建）② 去掉装箱（别经过 object）
            //   · 能量化"优化前 vs 优化后"，这是 Unity 优化报告的核心能力

            Console.WriteLine("--- TODO 5 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
