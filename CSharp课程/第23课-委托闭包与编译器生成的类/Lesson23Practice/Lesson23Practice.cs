// ============================================================================
//  第 23 课 · 练习：委托、闭包与编译器生成的类
//
//  ★ 本课重点（和 14 课的分工：14 课讲"怎么用"，本课讲"编译器生成了什么"）：
//     1. 委托是对象 = { Target（目标对象）, Method（方法指针） }
//     2. 闭包 = 编译器生成的 <>c__DisplayClass（所以每次都堆分配）
//     3. 不捕获变量的 Lambda 会被缓存成 <>c 的静态字段（零分配）
//     4. for 的循环变量只有一个 → 三个 Lambda 捕获同一个变量 → 3 3 3
//     5. Unity 的 Update() 里写 Lambda = 每帧分配 = GC 卡顿
//
//  ★ 数字会因【机器 / Debug 或 Release / .NET 版本】不同而不同，别抄具体值。
// ============================================================================

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ 本课要用的类写在这里（在 Lesson23Practice 类的【外面】）↓↓↓
    // ================================================================

    /// <summary>带实例方法的普通类：用来演示"委托的 Target 指向实例"。</summary>
    sealed class Widget
    {
        public string Name { get; set; } = "";

        /// <summary>实例方法：调用它需要"某个 Widget 对象"。</summary>
        public void Show() => Console.Write("");

        /// <summary>静态方法：调用它不需要任何对象，所以委托的 Target 会是 null。</summary>
        public static void Describe() => Console.Write("");

        public override string ToString() => Name;
    }

    /// <summary>模拟 Unity 里的敌人。</summary>
    sealed class Enemy
    {
        public int Hp { get; set; } = 100;
        public float Speed { get; set; } = 1.0f;
        public float Position { get; set; }

        /// <summary>每帧按固定规则移动 —— 不依赖任何外部变量，所以能安全地缓存成委托。</summary>
        public static void FixedTarget(Enemy enemy) => enemy.Position -= 0.1f;

        /// <summary>受伤。给 TODO 5 用，让"健康检查"这件事有实际后果。</summary>
        public void TakeDamage(int amount)
        {
            Hp -= amount;
            if (Hp < 0) Hp = 0;
        }
    }

    /// <summary>
    /// "带状态的普通对象"：把每帧变化的参数存成字段。
    /// ★ 这样方法本身不捕获任何变量 → 它的委托可以建一次用一辈子。
    /// </summary>
    sealed class EnemyMover
    {
        public float TargetPosition { get; set; }

        public void Move(Enemy enemy)
        {
            if (enemy.Position < TargetPosition) enemy.Position += enemy.Speed * 0.1f;
        }
    }

    class Lesson23Practice
    {
        // ================================================================
        // ↓↓↓ 本课的辅助方法写在这里 ↓↓↓
        // ================================================================

        /// <summary>当前线程累计分配字节数。</summary>
        static long Alloc() => GC.GetAllocatedBytesForCurrentThread();

        /// <summary>
        /// 预热 + 清空 GC + 调用 iterations 次 + 返回"平均每次调用分配了多少字节"。
        /// ★ 为什么必须先预热：第一次调用含 JIT 编译开销，测出来的不是代码本身的开销。
        /// ★ 为什么必须先 GC.Collect：把之前留下的垃圾清干净，读数才是这一段产生的。
        /// </summary>
        static double MeasureAllocPerCall(Action action, int iterations)
        {
            for (int i = 0; i < 100; i++) action();      // 预热

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long before = Alloc();
            for (int i = 0; i < iterations; i++) action();
            return (Alloc() - before) / (double)iterations;
        }

        /// <summary>
        /// 打印委托的 Target（可空，先判空再取 ToString）。
        /// ★ 先存进变量纯粹是为了好读和好调试 —— 【不是】语法要求。
        ///   插值洞里放 ?. 链式调用是合法的：$"{obj?.GetType().Name}" 能编译。
        ///   （插值洞里真正需要括号的是【三元运算符】，因为 ":" 会被当成格式说明符：
        ///     要写 $"{(a ? b : c)}" 而不是 $"{a ? b : c}"）
        /// </summary>
        static string TargetText(Delegate d)
        {
            object? target = d.Target;
            if (target is null) return "(null)";
            return $"{target}（{target.GetType().Name} 的实例）";
        }

        /// <summary>建一批敌人（在"启动时"建一次，不进入每帧的统计区间）。</summary>
        static List<Enemy> BuildEnemies()
        {
            var list = new List<Enemy>(10);
            for (int i = 0; i < 10; i++)
            {
                list.Add(new Enemy { Hp = 100, Speed = i * 0.1f + 0.5f, Position = i });
            }
            return list;
        }

        /// <summary>
        /// 造一个"捕获了局部变量"的闭包 —— 每调用一次就新建一个 DisplayClass + 一个委托。
        /// ★ 为什么写成方法而不是直接写在 Main 里？
        ///   因为被捕获的变量必须活到方法返回之后，这里正好逼出"搬到堆上"这件事。
        /// </summary>
        static Func<int, int> MakeCapturing(int bonus) => x => x + bonus;

        /// <summary>不捕获任何变量的 Lambda，提成静态字段 —— 建一次，用一辈子。</summary>
        static readonly Func<int, int> PlainLambda = x => x + 5;

        /// <summary>挨个执行收集起来的动作（用 for 而不是 foreach，避免多一层间接）。</summary>
        static void RunAll(List<Action> actions)
        {
            for (int k = 0; k < actions.Count; k++)
            {
                actions[k]();
            }
        }

        static void Main()
        {
            Console.WriteLine("===== 第 23 课练习：委托、闭包与编译器生成的类 =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 用 Target 区分"静态方法委托"和"实例方法委托"
            // ================================================================
            // 【要求】
            //   1. 建一个 Widget 对象（Name 随便起，比如 "按钮A"），再建第二个 Widget
            //   2. 造 4 个委托：
            //        a = Widget.Describe          ← 静态方法
            //        b = w1.Show                  ← 实例方法
            //        c = w2.Show                  ← 同一个方法，不同的实例
            //        d = w1.Show                  ← 再拿一次 w1 的方法
            //   3. 用辅助方法 TargetText(...) 打印 a/b/c/d 的 Target
            //   4. 再打印这 4 对比较结果：
            //        b.Method == c.Method      （方法一样吗？）
            //        b.Target == c.Target      （对象一样吗？）
            //        b == c                    （委托相等吗？）
            //        b == d                    （委托相等吗？）
            //
            // 【预期输出】（TargetText 的格式）
            //     a.Target : (null)                     ← 静态方法没有目标对象
            //     b.Target : 按钮A（Widget 的实例）
            //     c.Target : 按钮B（Widget 的实例）
            //     d.Target : 按钮A（Widget 的实例）
            //     b.Method == c.Method : True           ← 同一个方法
            //     b.Target == c.Target : False          ← 不同的对象
            //     b == c               : False          ← 所以委托不等
            //     b == d               : True           ← Target + Method 都相同
            //
            // 【提示】
            //     Action a = Widget.Describe;
            //     var w1 = new Widget { Name = "按钮A" };
            //     Action b = w1.Show;
            //     Console.WriteLine($"a.Target : {TargetText(a)}");
            //     Console.WriteLine($"b == c               : {b == c}");
            //
            // 【考察点】
            //   · 委托对象里存的是 { Target, Method } 两格
            //   · 静态方法的 Target 是 null；实例方法的 Target 是那个对象
            //   · 委托的相等性 = Target 和 Method【都】相等

            Console.WriteLine("--- TODO 1 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 2 ｜ 量化"捕获变量的闭包" vs "不捕获的 Lambda"的分配差异
            // ================================================================
            // 【要求】
            //   1. 用 MeasureAllocPerCall 测两种情况，各跑 10000 次：
            //        ① 捕获了局部变量的 Lambda（闭包）
            //        ② 不捕获任何变量的 Lambda
            //   2. 打印两个"字节/次"，并写一行结论
            //   3. 额外再做一次：把不捕获的那个 Lambda 存进静态字段，反复用（应该也是 0 分配）
            //
            // 【预期输出】（数字会不同，关键是量级）
            //     ① 捕获局部变量的闭包 :  ~60.0 字节/次   ← 每次都要 new DisplayClass + 委托
            //     ② 不捕获任何变量     :    0.0 字节/次   ← 委托被缓存，零分配
            //     ③ 复用静态字段的委托 :    0.0 字节/次
            //
            // 【提示】
            //     // ① 捕获版本：每次调用都要重新造一个 DisplayClass 实例
            //     double captured = MeasureAllocPerCall(() =>
            //     {
            //         int bonus = 5;
            //         Func<int, int> f = x => x + bonus;        // ★ 捕获 bonus
            //         GC.KeepAlive(f);
            //     }, 10000);
            //
            //     // ② 不捕获版本：Lambda 被提成 <>c 的静态方法，委托也缓存好了
            //     double plain = MeasureAllocPerCall(() =>
            //     {
            //         Func<int, int> f = x => x + 5;            // ★ 没碰任何外部变量
            //         GC.KeepAlive(f);
            //     }, 10000);
            //
            //     // ③ 静态字段复用
            //     double reused = MeasureAllocPerCall(() => GC.KeepAlive(PlainLambda(1)), 10000);
            //
            //     Console.WriteLine($"① 捕获局部变量的闭包 : {captured,8:F1} 字节/次");
            //
            // 【考察点】
            //   · 闭包要分配，因为被捕获的变量必须"搬到堆上"才能活得比方法调用更久
            //   · 不捕获的 Lambda 不需要状态 → 编译器把它提成静态方法 + 缓存委托
            //   · 这就是"热路径上把 Lambda 提出来当字段"的理论依据

            Console.WriteLine("--- TODO 2 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 3 ｜ 复现 for 循环捕获陷阱并修复
            // ================================================================
            // 【要求】
            //   用同一个 List<Action>，按顺序做三件事（每次先 Clear）：
            //     ① ❌ for 循环里直接捕获 i        → 期望打印 3 3 3
            //     ② ✅ for 循环体里先 int copy = i → 期望打印 0 1 2
            //     ③ ✅ foreach（Enumerable.Range） → 期望打印 0 1 2
            //   每组都先打印一行标题，再调 RunAll(actions)
            //
            // 【预期输出】
            //     ❌ for 直接捕获  ：3 3 3
            //     ✅ for + 局部副本：0 1 2
            //     ✅ foreach       ：0 1 2
            //
            // 【提示】
            //     var actions = new List<Action>();
            //
            //     for (int i = 0; i < 3; i++)
            //     {
            //         actions.Add(() => Console.Write($"{i} "));     // 捕获的是【变量 i】
            //     }
            //     Console.Write("  ❌ for 直接捕获  ：");
            //     RunAll(actions);
            //     Console.WriteLine();
            //
            //     actions.Clear();
            //     for (int i = 0; i < 3; i++)
            //     {
            //         int copy = i;                                  // ★ 每轮一个新变量
            //         actions.Add(() => Console.Write($"{copy} "));
            //     }
            //
            // 【考察点】
            //   · Lambda 捕获的是【变量】本身，不是创建那一刻的值快照
            //   · 三行输出为什么不同：区别只在"被捕获的那个变量有几个"

            Console.WriteLine("--- TODO 3 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 4 ｜ 证明 DisplayClass 的存在（打印编译器生成的类名）
            // ================================================================
            // 【要求】
            //   1. 造一个捕获了局部变量的委托 cap（比如 int multiplier = 3; 然后 x => x * multiplier）
            //   2. 再造一个不捕获任何变量的委托 plain（比如 x => x * 2）
            //   3. 分别打印它们的"所属类名"和"方法名"：
            //        cap.Method.DeclaringType?.Name
            //        cap.Method.Name
            //        plain.Method.DeclaringType?.Name
            //        plain.Method.Name
            //   4. 再打印两个 IsStatic：
            //        cap.Method.IsStatic      ← False（它是 DisplayClass 的实例方法）
            //        plain.Method.IsStatic    ← True （它是 <>c 的静态方法）
            //   5. 算一下 cap(10) 的结果，确认它真的乘上了 3
            //
            // 【预期输出】（名字里的数字可能不同）
            //     捕获变量的委托 类名 : <>c__DisplayClass0_0
            //     捕获变量的委托 方法名: <Main>b__1_0
            //     不捕获的委托 类名   : <>c
            //     不捕获的委托 方法名 : <Main>b__1_1
            //     cap.Method.IsStatic  : False
            //     plain.Method.IsStatic: True
            //     cap(10) = 30
            //
            // 【提示】
            //     int multiplier = 3;
            //     Func<int, int> cap = x => x * multiplier;
            //     Func<int, int> plain = x => x * 2;
            //
            //     // ★ 不能写 $"类名：{cap.Method.DeclaringType?.Name}" ——
            //     //   插值洞里不允许 "?." 之后紧接着 "."，先取到变量里再插值
            //     string? capClass = cap.Method.DeclaringType?.Name;
            //     Console.WriteLine($"捕获变量的委托 类名 : {capClass}");
            //
            // 【考察点】
            //   · Capture 类的名字是编译器编的，你写不出 <> 开头的标识符
            //   · 不捕获的那个被提到了 <>c（静态缓存类）上，所以它是静态方法
            //   · ★ 做完这题，一定要去 ILSpy 里把这几个类打开看一眼

            Console.WriteLine("--- TODO 4 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 5 ｜ ★ 综合（本课考核）：把"每帧分配"优化到 0 分配
            // ================================================================
            // 【场景】Unity 的 Update() 每帧都要遍历所有敌人，让它们朝"当帧才算出来的"
            //         目标位置移动（真实项目里这段还会顺便剔除死掉的敌人）。
            //         下面这个"反面写法"每帧都会分配闭包 + 委托 → GC 卡顿。
            //         ★ 本 TODO 只测"移动"这一步的分配 —— 优化目标非常明确：字节/帧 = 0。
            //
            // 【要求】
            //   1. 先跑 ❌ 反面写法，打印它的"字节/帧"
            //   2. 再用两种方式改造成 0 分配，分别打印"字节/帧"：
            //        ① 用 Enemy.FixedTarget（静态方法）—— 动作不依赖当帧状态的那种情况
            //        ② 用 EnemyMover：每帧只改 TargetPosition 字段，方法本身不捕获变量
            //   3. 对比三行数字，把"每帧分配 = 多少 KB/秒（按 60 FPS）"也算出来
            //
            // 【数据】List<Enemy> enemies = BuildEnemies();        // 10 个敌人
            //         var target = new Enemy { Position = 1.0f, Speed = 1.0f };   // 相当于缓存下来的 Transform
            //         const int frames = 300;
            //
            // 【预期输出】
            //     ❌ 每帧新建 Lambda      :   ~104.0 字节/帧   ≈ 6.1 KB/秒
            //     ✅ 缓存静态方法委托     :      0.0 字节/帧
            //     ✅ 带状态的普通对象     :      0.0 字节/帧
            //        目标位置 = 1.0，敌人数量 = 10，还活着的 = 7
            //     （0 分配的意思是：这 300 帧里除了启动时建的那几个委托，运行时一个新对象都没产生）
            //
            // 【提示】
            //     // ❌ 反面写法：Lambda 里用了本方法的参数 t → 每帧每个敌人 new 一个 DisplayClass + 委托
            //     double bad = MeasureAllocPerCall(() =>
            //     {
            //         Enemy t = target;                       // 参数被捕获
            //         for (int i = 0; i < enemies.Count; i++)
            //         {
            //             Enemy e = enemies[i];
            //             Action<Enemy> move = en =>
            //             {
            //                 if (en.Position < t.Position) en.Position += en.Speed * 0.1f;
            //             };
            //             move(e);                            // ★ 每帧每个敌人分配一次！
            //         }
            //     }, frames);
            //
            //     // ✅ 改造 1：动作完全不依赖当帧状态 → 用静态方法的委托（建在循环外）
            //     Action<Enemy> cachedFixed = Enemy.FixedTarget;
            //     double fixedCost = MeasureAllocPerCall(() =>
            //     {
            //         for (int i = 0; i < enemies.Count; i++) cachedFixed(enemies[i]);
            //     }, frames);
            //
            //     // ✅ 改造 2：动作依赖当帧状态 → 把状态放进对象字段，方法本身不捕获
            //     var mover = new EnemyMover();
            //     double moverCost = MeasureAllocPerCall(() =>
            //     {
            //         mover.TargetPosition = target.Position;   // 只改字段，不新建任何东西
            //         for (int i = 0; i < enemies.Count; i++) mover.Move(enemies[i]);
            //     }, frames);
            //
            //     Console.WriteLine($"❌ 每帧新建 Lambda      : {bad,8:F1} 字节/帧   ≈ {bad * 60 / 1024:F1} KB/秒");
            //     Console.WriteLine($"✅ 缓存静态方法委托     : {fixedCost,8:F1} 字节/帧");
            //     Console.WriteLine($"✅ 带状态的普通对象     : {moverCost,8:F1} 字节/帧");
            //     Console.WriteLine($"   目标位置 = {target.Position:F1}，敌人数量 = {enemies.Count}");
            //
            // 【考察点】
            //   · 分配来自"创建 Lambda/委托"，不来自"调用" —— 改造成缓存后就是 0
            //   · 区分两种情况：动作不依赖当帧状态（缓存静态方法） vs 依赖（用带状态的对象）
            //   · 这就是第 26 课要用 Profiler 追求 "GC Alloc = 0 B" 的手法

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
