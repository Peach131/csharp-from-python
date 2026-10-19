namespace LessonPractice
{
    // ============================================================================
    //  第 23 课 · 示例：委托、闭包与编译器生成的类
    //
    //  ★ 前置：第 14 课已经讲过委托 / Lambda / 闭包的【用法】。
    //     本课不讲怎么用，只回答一个问题：【编译器到底生成了什么？】
    //
    //  ★ 运行方式：
    //      1. 打开 第23课.sln，把 Lesson23Demo 设为启动项目，Ctrl + F5
    //      2. 或者命令行：dotnet run --project Lesson23Demo
    //
    //  ★ 跑完务必做两件事：
    //      ① 去 bin\Debug\net9.0\ 找到 Lesson23Demo.dll
    //      ② 用 ILSpy 打开它，找 <>c__DisplayClass —— 你会亲眼看到本课的主角
    // ============================================================================
    class Lesson23Demo
    {
        // ════════════════════════════════════════════════════════════════
        //  本课要用的类（写在 Lesson23Demo 的【外面】，见文件末尾）
        //    · Counter : 提供"实例方法"，用来演示 Target 不为 null
        //    · EnemyActor : 模拟 Unity 的敌人，用来演示 Update() 里的分配
        // ════════════════════════════════════════════════════════════════

        // ════════════════════════════════════════════════════════════════
        //  辅助方法
        // ════════════════════════════════════════════════════════════════

        /// <summary>一个普通静态方法 —— 用来演示"静态方法的委托 Target 是 null"。</summary>
        static void StaticHello() => Console.Write("");     // 内容无所谓，我们只看它的 Target

        /// <summary>当前线程累计分配字节数（第 19 课学过：用它测"产生了多少垃圾"）。</summary>
        static long Alloc() => GC.GetAllocatedBytesForCurrentThread();

        /// <summary>
        /// 打印委托的 Target。
        /// ★ Target 的类型是 object?（可空），所以先判空再取 ToString() —— 这是 Nullable enable 下的标准写法。
        /// </summary>
        static string DescribeTarget(Delegate d)
        {
            object? target = d.Target;
            if (target is null) return "(null)";
            return $"{target}（{target.GetType().Name} 的实例）";
        }

        /// <summary>对一个无参委托做"预热 + 清空 GC + 循环调用 + 取平均分配"，返回每次调用的平均字节数。</summary>
        static double MeasureAllocPerCall(Action action, int iterations)
        {
            for (int i = 0; i < 100; i++) action();      // ★ 预热：第一次调用含 JIT 开销

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long before = Alloc();
            for (int i = 0; i < iterations; i++) action();
            return (Alloc() - before) / (double)iterations;
        }

        // ════════════════════════════════════════════════════════════════
        //  Unity 演示用的字段：预先算好的敌人数组
        //  这些字段在【类型加载时】就建好了，永远不会进入"每帧"的统计区间
        // ════════════════════════════════════════════════════════════════

        static readonly List<EnemyActor> DemoEnemies = BuildEnemies();

        /// <summary>临时的"最近的敌人"，在 Unity 里相当于缓存下来的 Transform。</summary>
        static readonly EnemyActor DemoTarget = new EnemyActor { Hp = 50, Speed = 1.0f, Position = 1.0f };

        /// <summary>不捕获任何东西的静态方法 —— 把它当委托用，可以一次性缓存。</summary>
        static void DemoTickSlow(EnemyActor e) => e.Position -= 0.1f;

        /// <summary>把"对每个敌人做同一件事"抽成方法，动作由参数传进来。</summary>
        static void AllEnemies(Action<EnemyActor> action)
        {
            foreach (EnemyActor e in DemoEnemies) action(e);
        }

        /// <summary>每帧都要判断一次：目标在哪一侧？</summary>
        static bool DemoIsBehind(EnemyActor e, float targetPosition) => e.Position < targetPosition;

        static List<EnemyActor> BuildEnemies()
        {
            var list = new List<EnemyActor>(10);
            for (int i = 0; i < 10; i++)
            {
                list.Add(new EnemyActor { Hp = 100, Speed = i * 0.1f + 0.5f, Position = i });
            }
            return list;
        }

        // ════════════════════════════════════════════════════════════════
        //  一、委托是对象：Target + Method
        // ════════════════════════════════════════════════════════════════

        static void ShowDelegateIsObject()
        {
            Console.WriteLine("---------- 一、委托是对象：Target + Method ----------");
            Console.WriteLine();

            // ★ 静态方法 → 不需要任何对象就能调用，所以 Target 是 null
            Action staticDel = StaticHello;
            Console.WriteLine($"  Action a = StaticHello;               ← 静态方法");
            Console.WriteLine($"    a.Target : {DescribeTarget(staticDel)}");
            Console.WriteLine($"    a.Method : {staticDel.Method.Name}");
            Console.WriteLine();

            // ★ 实例方法 → 必须记住"哪个对象上的方法"，所以 Target 指向那个实例
            var counter = new Counter { Label = "counter-1" };
            Action instanceDel = counter.Hello;
            Console.WriteLine($"  Action b = counter.Hello;             ← 实例方法");
            Console.WriteLine($"    b.Target : {DescribeTarget(instanceDel)}");
            Console.WriteLine($"    b.Method : {instanceDel.Method.Name}");
            Console.WriteLine();

            // ★ 关键证据：同一个方法，两个不同的实例 → Method 一样，Target 不同
            var counter2 = new Counter { Label = "counter-2" };
            Action instanceDel2 = counter2.Hello;
            Console.WriteLine($"  Action c = counter2.Hello;            ← 同一个方法，另一个实例");
            Console.WriteLine($"    c.Target : {DescribeTarget(instanceDel2)}");
            Console.WriteLine($"    b.Method == c.Method ? {instanceDel.Method == instanceDel2.Method}   ← 方法一样");
            Console.WriteLine($"    b.Target == c.Target ? {ReferenceEquals(instanceDel.Target, instanceDel2.Target)}   ← 对象不同");
            Console.WriteLine();

            // ★ 委托的相等性：Target 和 Method 【都】相同才算相等
            Action instanceDel3 = counter.Hello;
            Console.WriteLine($"  Action d = counter.Hello;             ← 又拿了一次同一个实例的方法");
            Console.WriteLine($"    b == d ? {instanceDel == instanceDel3}");
            Console.WriteLine();

            Console.WriteLine("  ★ 结论：委托 = { Target（目标对象）, Method（方法指针） } 两格。");
            Console.WriteLine("     只知道 Method 而不知道 Target，就不知道该在哪个对象上执行 ——");
            Console.WriteLine("     所以委托【不是】函数指针，它是一个装着 (对象, 方法) 的堆对象。");
            // ★ Lambda 也是一种方法：它的方法名是编译器编出来的（形如 <Main>b__0_0）。
            //
            // ★★ 关于"插值洞里能不能放 ?."（2026-10-06 订正，原来这里写错了）
            //   可以放。 $"x = {obj?.Prop}" 是【合法】的，?. 后面接着 . 也没问题：
            //       object? o = null;
            //       Console.WriteLine($"{o?.GetType().Name}");   // ✅ 合法，输出空字符串
            //   下面这样就【不合法】—— 但原因不是 ?.，而是【三元运算符的冒号】：
            //       Console.WriteLine($"{o is null ? "空" : "非空"}");   // ❌ CS8361
            //   因为插值洞里 ":" 是"格式说明符"的分隔符，编译器把三元的 : 当成了格式。
            //   官方原文：A conditional expression cannot be used directly in a string
            //   interpolation because the ':' ends the interpolation. Parenthesize it.
            //   → 加一层括号就好：$"{(o is null ? "空" : "非空")}"   ✅
            //
            //   那为什么下面还是先存进变量？纯粹是为了【好读】+ 能在调试器里看到中间值，
            //   不是语法要求。
            Func<int, int> lambda = x => x + 1;
            string? lambdaClass = lambda.Method.DeclaringType?.Name;
            Console.WriteLine("  ★ 小知识：Lambda 也是一种方法，名字由编译器编出来。");
            Console.WriteLine($"    lambda 的类名   ：{lambdaClass}");
            Console.WriteLine($"    lambda 的方法名 ：{lambda.Method.Name}");
            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════
        //  二、闭包 = 编译器生成的 <>c__DisplayClass
        // ════════════════════════════════════════════════════════════════

        static void ShowClosureAllocation()
        {
            Console.WriteLine("---------- 二、闭包 = 编译器生成的类（含实测） ----------");
            Console.WriteLine();

            // ① 捕获了局部变量 → 每次都要新建一个"显示类"实例来装它
            Func<Func<int, int>> makeCapturing = () =>
            {
                int multiplier = 3;
                return x => x * multiplier;              // 捕获 multiplier
            };
            Func<int, int> captured = makeCapturing();

            // ② 不捕获任何变量 → 编译器的优化：Lambda 提成静态方法 + 委托缓存一次
            Func<int, int> plain = x => x * 2;

            // ★ 同一条规则：先把 ?. 的结果取出来，再放进插值洞
            string? capturedClass = captured.Method.DeclaringType?.Name;
            string? plainClass = plain.Method.DeclaringType?.Name;

            Console.WriteLine($"  捕获了局部变量的 Lambda 的类名 : {capturedClass}");
            Console.WriteLine($"  不捕获任何变量的 Lambda 的类名 : {plainClass}");
            Console.WriteLine($"  不捕获的那个 Lambda 的方法名   : {plain.Method.Name}");
            Console.WriteLine();
            Console.WriteLine("  ★ 名字里的 <> 是编译器专用符号，你写不出这种类名 —— 它不可能来自手写代码。");
            Console.WriteLine("  ★ 注意编译器的优化：");
            Console.WriteLine("     · 捕获了变量的 Lambda → 必须有一个 DisplayClass 实例来装变量，所以每次都要 new");
            Console.WriteLine("     · 不捕获任何变量的 Lambda → 它不需要任何状态，编译器干脆把它提成【静态方法】，");
            Console.WriteLine("       并把委托对象缓存进 <>c 这个类的一个静态字段 → 反复拿到的都是同一个委托");
            Console.WriteLine();
            Console.WriteLine("  ★ 怎么证明"它被提成了静态方法"？看 Method.IsStatic ——");
            Console.WriteLine($"      捕获变量的那个 Lambda，IsStatic = {captured.Method.IsStatic}  ← 它是 DisplayClass 的【实例】方法");
            Console.WriteLine($"      不捕获的那个 Lambda，  IsStatic = {plain.Method.IsStatic}   ← 它是 <>c 的【静态】方法，可以缓存");
            Console.WriteLine();

            const int iterations = 10_000;
            double capturedBytes = MeasureAllocPerCall(() =>
            {
                Func<int, int> f = makeCapturing();       // ★ 每次调用都新建 DisplayClass + 委托
                GC.KeepAlive(f);
            }, iterations);

            double plainBytes = MeasureAllocPerCall(() =>
            {
                Func<int, int> f = x => x * 2;            // ★ 不捕获 → 拿的是缓存的静态委托
                GC.KeepAlive(f);
            }, iterations);

            Console.WriteLine($"  循环 {iterations:N0} 次创建 Lambda：");
            Console.WriteLine($"    捕获局部变量（闭包） : {capturedBytes,8:F1} 字节/次");
            Console.WriteLine($"    不捕获任何变量       : {plainBytes,8:F1} 字节/次");
            Console.WriteLine();
            Console.WriteLine("  ★ 结论：闭包不是"语法糖"，它有真实的堆成本。");
            Console.WriteLine("     被捕获的变量被搬进了堆上的 DisplayClass 对象里，Lambda 变成它的方法。");
            Console.WriteLine("  ★ 数字为什么不是整数？因为每次分配还牵扯委托对象（24~32 字节）和对象头，");
            Console.WriteLine("     不同机器的结果会有差异 —— 重要的是【捕获的那一列不是 0】。");
            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════
        //  三、捕获的是"变量"本身，不是快照
        // ════════════════════════════════════════════════════════════════

        static void ShowCaptureSemantics()
        {
            Console.WriteLine("---------- 三、捕获的是"变量"不是"值" ----------");
            Console.WriteLine();

            int counter = 10;
            Func<int> read = () => counter;              // 捕获【变量 counter 本身】

            Console.WriteLine($"  定义 lambda 之后，counter = {counter}");
            Console.WriteLine($"  read() 返回          ：{read()}");

            counter = 99;                                 // ★ 改的是那个变量，不是 lambda
            Console.WriteLine($"  把 counter 改成 99 之后……");
            Console.WriteLine($"  read() 返回          ：{read()}      ← 跟着变了！证明捕获的是变量");
            Console.WriteLine();

            Func<int> independent = () => 10;             // 不捕获：常量直接被编译进方法体
            Console.WriteLine($"  对照：不捕获的 () => 10  永远是 {independent()}");
            Console.WriteLine();

            Console.WriteLine("  ★ 如果编译器的做法是"把 10 复制一份存起来"，read() 就该永远是 10。");
            Console.WriteLine("     它给出 99，说明 DisplayClass 里的字段和原来的局部变量是【同一个存储】。");
            Console.WriteLine("  ★ Python 对照：Python 用的是 cell 对象，行为完全一样（见课文第二节 2.5）。");
            Console.WriteLine();
        }

        // ════════════════════════════════════════════════════════════════
        //  四、⚠️ Unity 实战：Update() 里写 Lambda = 每帧分配
        // ════════════════════════════════════════════════════════════════

        static void ShowUnityScenario()
        {
            Console.WriteLine("---------- 四、⚠️ Unity 实战：每帧分配 vs 零分配 ----------");
            Console.WriteLine();

            const int frames = 300;

            // ── ❌ 反面写法：Lambda 里用了本方法的参数 target → 每帧新建一个 DisplayClass ──
            double badBytes = MeasureAllocPerCall(() =>
            {
                float target = DemoTarget.Position;
                AllEnemies(e => DemoIsBehind(e, target));
            }, frames);

            // ── ✅ 正确写法 1：动作完全不依赖当帧的状态 → 用一个静态字段的委托 ──
            double cachedBytes = MeasureAllocPerCall(() =>
            {
                AllEnemies(CachedTickSlow);
            }, frames);

            // ── ✅ 正确写法 2：动作依赖当帧状态 → 用"带状态的普通对象"，每帧【复用】同一个委托 ──
            var mover = new EnemyFrameMover();
            double moverBytes = MeasureAllocPerCall(() =>
            {
                mover.TargetPosition = DemoTarget.Position;
                AllEnemies(mover.Tick);
            }, frames);

            Console.WriteLine($"  模拟 {frames} 帧，每帧遍历 {DemoEnemies.Count} 个敌人：");
            Console.WriteLine($"    ❌ AllEnemies(e => IsBehind(e, target))  : {badBytes,8:F1} 字节/帧  ← 闭包 + 委托");
            Console.WriteLine($"    ✅ AllEnemies(CachedTickSlow)            : {cachedBytes,8:F1} 字节/帧  ← 缓存委托");
            Console.WriteLine($"    ✅ AllEnemies(mover.Tick)                : {moverBytes,8:F1} 字节/帧  ← 复用同一个委托");
            Console.WriteLine();
            Console.WriteLine("  ★ 按 60 FPS 算，第一行每秒产生 " + $"{badBytes * 60 / 1024:F1}" + " KB 垃圾；一小时就是 "
                              + $"{badBytes * 60 * 3600 / 1024 / 1024:F1}" + " MB。");
            Console.WriteLine("  ★ Unity 用的 Boehm GC 非分代、非压缩（第 26 课细讲），");
            Console.WriteLine("     所以"每帧都有垃圾"最终一定会变成玩家感觉到的卡顿（GC Spike）。");
            Console.WriteLine();

            // ★ 多播委托也能一次缓存好，完全不用每帧 +=
            //   （下面这两行在"启动时"执行一次，不在每帧里 —— 这正是关键）
            Action<int> multicast = DemoTickFast;
            multicast += DemoTickSlow;

            double singleBytes = MeasureAllocPerCall(() => DemoTickFast(0), frames);
            double multicastBytes = MeasureAllocPerCall(() => multicast(0), frames);
            Console.WriteLine($"  顺带一测（同样是每帧调用一次）：");
            Console.WriteLine($"    单个委托调用                        : {singleBytes,8:F1} 字节/帧");
            Console.WriteLine($"    两个方法的多播委托调用              : {multicastBytes,8:F1} 字节/帧");
            Console.WriteLine("  ★ 多播委托的【调用】本身零分配 —— 贵的从来不是"调用"，而是"每帧重新创建"。");
            Console.WriteLine("     创建委托（new / Lambda / 方法组转换）才会分配，调用不会。");
            Console.WriteLine();
        }

        /// <summary>缓存起来的委托（静态字段）—— 建一次，用一辈子。</summary>
        static readonly Action<EnemyActor> CachedTickSlow = DemoTickSlow;

        static void DemoTickFast(EnemyActor e) => e.Position += 0.1f;

        // ════════════════════════════════════════════════════════════════
        //  五、for 循环捕获陷阱
        // ════════════════════════════════════════════════════════════════

        static void ShowLoopTrap()
        {
            Console.WriteLine("---------- 五、for 循环捕获陷阱 ----------");
            Console.WriteLine();

            var actions = new List<Action>();

            // ❌ 陷阱：for 的 i 从头到尾只有【一个变量】，三个 Lambda 捕获的是同一个它
            for (int i = 0; i < 3; i++)
            {
                actions.Add(() => Console.Write($"{i} "));
            }
            Console.Write("  ❌ for 直接捕获        ：");
            RunAll(actions);
            Console.WriteLine();

            // ✅ 修复：每轮在循环体里新建一个局部变量
            actions.Clear();
            for (int i = 0; i < 3; i++)
            {
                int copy = i;                             // ★ 每一轮都是【新的一个变量】
                actions.Add(() => Console.Write($"{copy} "));
            }
            Console.Write("  ✅ for + 局部副本      ：");
            RunAll(actions);
            Console.WriteLine();

            // ✅ foreach：C# 5 起，每次迭代的变量都是新的，天然安全
            actions.Clear();
            foreach (int i in Enumerable.Range(0, 3))
            {
                actions.Add(() => Console.Write($"{i} "));
            }
            Console.Write("  ✅ foreach（C# 5+）    ：");
            RunAll(actions);
            Console.WriteLine();

            // ✅ while：在循环体内声明，每次迭代也是新变量
            actions.Clear();
            int j = 0;
            while (j < 3)
            {
                int copy = j;
                actions.Add(() => Console.Write($"{copy} "));
                j++;
            }
            Console.Write("  ✅ while + 局部副本    ：");
            RunAll(actions);
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("  ★ 一句话记住：Lambda 捕获的是【变量】，不是【值的快照】。");
            Console.WriteLine("  ★ 为什么 for 会中招而 foreach 不会？");
            Console.WriteLine("     · for 的 i 声明在循环【外面】，整个循环只有一份存储");
            Console.WriteLine("     · foreach 的迭代变量声明在循环【里面】，C# 5 规范规定每次迭代新建一个");
            Console.WriteLine("  ★ Python 对照：Python 连 for 都是同一个变量 → 所有循环都会踩这个坑：");
            Console.WriteLine("       funcs = [lambda: i for i in range(3)]     # 全部返回 2");
            Console.WriteLine("       修法一样：lambda i=i: i                    # 绑定当前值");
            Console.WriteLine();
        }

        static void RunAll(List<Action> actions)
        {
            for (int k = 0; k < actions.Count; k++)
            {
                actions[k]();
            }
        }

        // ════════════════════════════════════════════════════════════════
        //  Main
        // ════════════════════════════════════════════════════════════════

        static void Main()
        {
            Console.WriteLine("===== 第 23 课：委托、闭包与编译器生成的类 =====");
            Console.WriteLine();

            ShowDelegateIsObject();
            ShowClosureAllocation();
            ShowCaptureSemantics();
            ShowUnityScenario();
            ShowLoopTrap();

            // ────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 六、下一步：用 ILSpy 亲眼看 ----------");
            Console.WriteLine();

            string exePath = Environment.ProcessPath ?? "";
            string dllPath = exePath.Replace(".exe", ".dll");
            Console.WriteLine("  本示例编译出的程序集：");
            Console.WriteLine($"    {dllPath}");
            Console.WriteLine();
            Console.WriteLine("  ① 打开 ILSpy（免费开源）；");
            Console.WriteLine("  ② 把这个 dll 拖进去；");
            Console.WriteLine("  ③ 展开 Lesson23Demo → 找到 <>c 和 <>c__DisplayClass 这两个编译器生成的类；");
            Console.WriteLine("  ④ 展开 ShowLoopTrap，对照 IL 看那句 int copy = i 到底生成了什么。");
            Console.WriteLine();
            Console.WriteLine("  ★ 第 17 课你看到的是"顶级语句被包成 <Main>$"这种小把戏，");
            Console.WriteLine("     本课你要看到的是【编译器替你写了一个完整的类】—— 这是同一件事的放大版。");
            Console.WriteLine();

            Console.WriteLine("===== 示例结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }

    // ============================================================================
    //  以下是本课要用的类（都在 Lesson23Demo 的【外面】）
    // ============================================================================

    /// <summary>提供实例方法，用来演示"委托的 Target 指向实例"。</summary>
    sealed class Counter
    {
        public string Label { get; set; } = "";

        public void Hello() => Console.Write("");       // 内容无所谓，我们只看它的 Target

        public override string ToString() => Label;
    }

    /// <summary>模拟 Unity 里的敌人。</summary>
    sealed class EnemyActor
    {
        public int Hp { get; set; } = 100;
        public float Speed { get; set; } = 1.0f;
        public float Position { get; set; }

        /// <summary>把"每帧移动"提取成不捕获任何东西的静态方法 —— 这样它才能被缓存成委托。</summary>
        public static void FixedTarget(EnemyActor enemy) => enemy.Position -= 0.1f;
    }

    /// <summary>
    /// "带状态的普通对象"：把每帧变化的参数存成字段，方法本身不捕获任何变量。
    /// ★ 这样一来，mover.Tick 这个委托可以建一次、用一辈子 —— 零分配。
    /// </summary>
    sealed class EnemyFrameMover
    {
        public float TargetPosition { get; set; }

        public void Tick(EnemyActor enemy)
        {
            if (enemy.Position < TargetPosition) enemy.Position += enemy.Speed * 0.1f;
        }
    }
}
