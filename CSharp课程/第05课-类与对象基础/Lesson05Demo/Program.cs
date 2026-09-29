// ============================================================================
//  第 05 课 · 示例代码
//  类与对象基础 —— 每一行都有注释，请逐行读完再运行
//
//  ★ 注意这个文件的结构：类是写在 namespace 里面、class Program 的【外面】
// ============================================================================

namespace Lesson05Demo
{
    // ========================================================================
    // 一、最基础的类：只有字段
    //
    // ⚠️ 注意这段 #pragma：编译器会对下面这个类的 Name 字段发出 CS8618 警告
    //    （"不可为 null 的字段必须包含非 null 值"）。
    //    这个警告正是本课要讲的"字段默认值是 null"问题，所以这里先临时关掉它，
    //    让示例保持 0 警告。演示 2 会专门展示这个问题。
    // ========================================================================
#pragma warning disable CS8618
    class StudentV1
    {
        public string Name;         // 字段：默认值 null  ← CS8618 警告说的就是它
        public int Grade;           // 字段：默认值 0
        public bool IsPassed = false;   // 字段：给个初值，避免 CS0649 警告
    }
#pragma warning restore CS8618


    // ========================================================================
    // 二、用属性代替字段（推荐做法）
    // ========================================================================
    class StudentV2
    {
        // 私有字段：外面看不见，命名惯例是 _ 开头 + camelCase
        private int _grade;

        // 自动属性：编译期帮你生成隐藏字段
        public string Name { get; set; } = "";     // = "" 是给它一个非 null 的初值

        // 手动属性：带校验逻辑
        public int Grade
        {
            get { return _grade; }                 // 读的时候执行
            set                                    // 写的时候执行，value 是新值
            {
                if (value < 0 || value > 100)
                {
                    Console.WriteLine($"      [拒绝] 成绩 {value} 不合法（必须 0~100）");
                    return;                        // 不赋值，直接退出
                }
                _grade = value;
            }
        }
    }


    // ========================================================================
    // 三、带构造函数的类
    // ========================================================================
    class StudentV3
    {
        public string Name { get; set; }
        public int Grade { get; set; }

        // 构造函数：名字必须和类名一样，而且【没有返回类型】
        public StudentV3(string name, int grade)
        {
            Name = name;
            Grade = grade;
        }

        // 构造函数重载：一个参数，用 : this(...) 复用上面的
        public StudentV3(string name) : this(name, 0)
        {
        }

        // 无参构造函数
        public StudentV3()
        {
            Name = "未命名";
            Grade = 0;
        }

        // 普通方法：类里也可以有方法
        public string GetLevel()
        {
            if (Grade >= 90) return "优秀";
            if (Grade >= 60) return "及格";
            return "不及格";
        }
    }


    // ========================================================================
    // 四、只读属性
    // ========================================================================
    class Circle
    {
        public double Radius { get; }              // 只有 get：只能在构造函数里赋值

        public Circle(double radius)
        {
            Radius = radius;                       // ✅ 构造函数里可以赋值
        }

        // 计算属性：每次读都重新算（没有对应的存储字段）
        public double Area => 3.14159 * Radius * Radius;

        public double Perimeter => 2 * 3.14159 * Radius;
    }


    // ========================================================================
    // 五、record：一行搞定的不可变数据类
    // ========================================================================
    record Point(int X, int Y);

    // record 也可以有方法
    record Money(decimal Amount, string Currency)
    {
        public string Describe() => $"{Amount:F2} {Currency}";
    }


    // ========================================================================
    //  程序入口
    // ========================================================================
    class Program
    {
        static void Main()
        {
            Console.WriteLine("========== 第 05 课：类与对象基础 ==========");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 1：类 vs 对象 ----");
            // 类 StudentV1 是【模板】，下面创建了两个【对象】

            StudentV1 a = new StudentV1();
            StudentV1 b = new StudentV1();

            a.Name = "张三";
            a.Grade = 88;
            b.Name = "李四";
            b.Grade = 92;

            Console.WriteLine($"   对象 a：{a.Name}, {a.Grade}");
            Console.WriteLine($"   对象 b：{b.Name}, {b.Grade}");
            Console.WriteLine("   ★ a 和 b 是独立的两块内存，互不影响");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 2：字段的默认值 ----");

            StudentV1 fresh = new StudentV1();
            Console.WriteLine($"   new 之后，未赋值的字段：");
            Console.WriteLine($"      Name     = {(fresh.Name is null ? "null（不是空字符串！）" : fresh.Name)}");
            Console.WriteLine($"      Grade    = {fresh.Grade}");
            Console.WriteLine($"      IsPassed = {fresh.IsPassed}");
            Console.WriteLine("   ★ string 字段默认是 null，直接访问 .Length 会崩溃");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 3：★ 为什么用属性而不是裸字段 ----");

            StudentV2 s2 = new StudentV2();

            Console.Write("   合法赋值 s2.Grade = 88  →  ");
            s2.Grade = 88;
            Console.WriteLine($"Grade 现在是 {s2.Grade}");

            Console.Write("   非法赋值 s2.Grade = 999 →  ");
            s2.Grade = 999;                          // 会被属性的 set 拦住
            Console.WriteLine($"Grade 还是 {s2.Grade}（非法值被拒绝了）");

            Console.Write("   非法赋值 s2.Grade = -50 →  ");
            s2.Grade = -50;
            Console.WriteLine($"Grade 还是 {s2.Grade}");
            Console.WriteLine("   ★ 属性 = 给数据加了门禁。裸字段谁都能塞非法值。");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 4：构造函数 ----");

            var t1 = new StudentV3("张三", 88);         // 调 2 参数构造函数
            var t2 = new StudentV3("李四");             // 调 1 参数构造函数（成绩默认 0）
            var t3 = new StudentV3();                   // 调无参构造函数

            Console.WriteLine($"   new StudentV3(\"张三\", 88) → {t1.Name}, {t1.Grade}, {t1.GetLevel()}");
            Console.WriteLine($"   new StudentV3(\"李四\")     → {t2.Name}, {t2.Grade}, {t2.GetLevel()}");
            Console.WriteLine($"   new StudentV3()            → {t3.Name}, {t3.Grade}, {t3.GetLevel()}");
            Console.WriteLine("   ★ new 的时候构造函数【自动】执行，用来做初始化");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 5：对象初始化器 ----");

            // 方式 1：先创建再赋值
            var u1 = new StudentV2();
            u1.Name = "王五";
            u1.Grade = 76;

            // 方式 2：对象初始化器（一步到位）
            var u2 = new StudentV2 { Name = "赵六", Grade = 95 };

            Console.WriteLine($"   方式1：{u1.Name}, {u1.Grade}");
            Console.WriteLine($"   方式2：{u2.Name}, {u2.Grade}   ← 大括号里直接赋值");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 6：只读属性和计算属性 ----");

            var circle = new Circle(5);
            Console.WriteLine($"   半径 = {circle.Radius}");
            Console.WriteLine($"   面积 = {circle.Area:F4}    ← 每次读都重新算");
            Console.WriteLine($"   周长 = {circle.Perimeter:F4}");
            // circle.Radius = 10;      // ❌ 编译错误：Radius 只有 get，是只读的
            Console.WriteLine("   ★ Radius 只有 get，所以创建后改不了（取消注释那行会编译错误）");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 7：★ record vs class 的相等比较 ----");

            // 普通 class：两个内容相同的对象
            var c1 = new StudentV3("张三", 88);
            var c2 = new StudentV3("张三", 88);

            Console.WriteLine($"   普通 class：内容相同的两个对象");
            Console.WriteLine($"      c1 == c2        → {c1 == c2}   ← 按【地址】比较，所以是 False");
            Console.WriteLine($"      c1.Equals(c2)   → {c1.Equals(c2)}");
            Console.WriteLine();

            // record：内容相同就是相等
            var p1 = new Point(1, 2);
            var p2 = new Point(1, 2);

            Console.WriteLine($"   record：内容相同的两个对象");
            Console.WriteLine($"      p1 == p2        → {p1 == p2}    ← 按【值】比较，所以是 True");
            Console.WriteLine($"      p1              → {p1}    ← record 自动生成了 ToString");
            Console.WriteLine();

            var money1 = new Money(19.99m, "CNY");
            var money2 = new Money(19.99m, "CNY");
            Console.WriteLine($"   record 也可以有方法：{money1.Describe()}");
            Console.WriteLine($"      内容相同 → {money1 == money2}");
            Console.WriteLine();

            Console.WriteLine("   ★ 一句话：只装数据、希望按值比较 → 用 record");
            Console.WriteLine("              需要行为、需要按地址区分 → 用 class");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson05Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
