// ============================================================================
//  第 07 课 · 练习：继承、接口与多态
//
//  ★ 本课重点：
//     1. 继承：class 子类 : 父类
//     2. virtual + override 才能实现多态
//     3. abstract 抽象类 vs interface 接口
// ============================================================================

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ 本课的类定义写在这里 ↓↓↓
    // ================================================================

    // ────────────────────────────────────────────────────────────────
    // TODO 1 + TODO 2 ｜ Person（父类）与 Student（子类）
    // ────────────────────────────────────────────────────────────────
    // ★ 这两个类同时是 TODO 1 和 TODO 2 的答案：
    //   TODO 2 要求"改造 TODO 1 的类"，也就是在原来的基础上【加上构造函数】，
    //   所以最终交付的就是下面这一份（属性 + 构造函数 + 自己的方法）。
    class Person
    {
        // ★ 为什么 string 要给 = "" 初值？
        //   本项目开了 <Nullable>enable</Nullable>，string 是引用类型、默认是 null，
        //   编译器会报 CS8618 警告。给个空串初值既消警告，也避免以后 .Length 直接崩。
        //   int 是值类型，默认 0，永远不会是 null，所以不用给初值。
        public string Name { get; set; } = "";
        public int Age { get; set; }

        // ★ 构造函数名必须和类名完全一样，而且不写返回类型（连 void 都不写）。
        //   有了这个构造函数以后，"new Person()" 就不合法了 —— 必须传名字和年龄。
        //   这正是 TODO 2 考察点的前提：父类【没有】无参构造函数。
        public Person(string name, int age)
        {
            // 左边 Name 是属性，右边 name 是参数，编译器分得清，不用写 this
            Name = name;
            Age = age;
        }

        // ★ 表达式体方法 `=> 表达式`，等价于 { Console.WriteLine(表达式); }
        //   输出必须逐字匹配预期："我叫 张三，今年 20 岁"（注意是【全角】逗号）
        public void Introduce() => Console.WriteLine($"我叫 {Name}，今年 {Age} 岁");
    }

    // ★ 冒号 = 继承。Student 自动拥有 Person 的所有 public 成员（Name / Age / Introduce）
    //   C# 一个类【只能有一个父类】；想多继承只能用接口（见 TODO 4）
    class Student : Person
    {
        public string School { get; set; } = "";

        // ★★ 这一行是本课最容易考的点：
        //   Person 只提供了 Person(string, int)，【没有】无参构造函数，
        //   所以子类必须用 `: base(name, age)` 显式告诉编译器"前两个参数交给父类"。
        //   漏写 → 编译错误 CS7036（找不到 Person 的无参构造函数）。
        //   执行顺序：父类构造函数先跑完（Name/Age 先就位），再跑子类的函数体。
        //   Python 对照：: base(...) 就是 super().__init__(...)，
        //   区别是 C# 里的 base(...) 写在参数表后面，而不是函数体第一行。
        public Student(string name, int age, string school) : base(name, age)
        {
            // 只写自己特有的那一个字段；name / age 已经被 base 处理掉了
            School = school;
        }

        // ★ 这里直接用 Name —— 它就是"从父类继承来的属性"，
        //   子类不用重新声明，也不用写 base.Name。
        public void Study() => Console.WriteLine($"{Name} 在 {School} 学习");
    }


    // ────────────────────────────────────────────────────────────────
    // TODO 3 ｜ virtual + override：多态的基础
    // ────────────────────────────────────────────────────────────────
    class Employee
    {
        public string Name { get; set; } = "";

        // ★★ 岗位名称：也用 virtual，理由和 CalculateSalary 完全一样 ——
        //   预期输出里每人有个"（经理）"这样的后缀，这个后缀本来就是
        //   "员工的身份信息"，应该属于【对象自己】，而不是外面另开一个数组去记。
        //   ★ 基类给一个默认值"（普通员工）"，子类各自 override 改掉。
        //   这样 Main 里只要写 e.Title，多态会自动拿到正确的那个。
        public virtual string Title => "（普通员工）";

        // ★ virtual 的含义是"这个方法【允许】被子类重写"。
        //   C# 和 Python 不同：Python 里同名方法天然就是重写，
        //   C# 默认【不允许】悄悄覆盖，想重写必须父类说 virtual、子类说 override。
        //   返回 3000（写 int 也行）—— int 会自动隐式转成 double。
        public virtual double CalculateSalary() => 3000;
    }

    class Manager : Employee
    {
        // ★ 属性的 override 写法：注意不用写类型（string 已经由父类声明了），
        //   只写 override + 属性名 + => 新值。方法重写要重写整个签名，属性只写名字。
        public override string Title => "（经理）";

        // ★ override 的含义是"我要【重写】父类那个 virtual 方法"。
        //   两边都写关键字不是啰嗦，而是让"这个方法能不能被改"在代码里一目了然。
        //   只有当【变量声明类型】和【实际对象类型】不一致时，override 才显出威力（见 Main 里的数组循环）。
        public override double CalculateSalary() => 8000;   // 3000 基本工资 + 5000 管理津贴
    }

    class Salesperson : Employee
    {
        public double SalesAmount { get; set; }

        public override string Title => "（销售）";

        // ★ 5% 要写成 0.05（百分比先除以 100）
        //   30000 × 0.05 = 1500，再加 3000 基本工资 = 4500
        public override double CalculateSalary() => 3000 + SalesAmount * 0.05;
    }


    // ────────────────────────────────────────────────────────────────
    // TODO 4 ｜ interface：表达"能做什么"
    // ────────────────────────────────────────────────────────────────
    // ★ 接口只规定"必须提供哪些成员"，不提供实现，也不存数据。
    //   命名惯例：I 开头（IPrintable），看见 I 就知道这是接口而不是类。
    interface IPrintable
    {
        // ★ 接口成员【不写 public】—— 它默认就是 public 的，写了反而多余。
        //   注意这里只有分号、没有方法体：接口说"你得能打印"，但不说"怎么打"。
        void Print();
    }

    // ★ 实现接口也用冒号，写法看起来和继承一模一样。
    //   区别在于：冒号前面的是类（只能一个）叫继承，是接口（可以多个）叫"实现"。
    class Invoice : IPrintable
    {
        public string Customer { get; set; } = "";

        // ★ decimal 是"精确十进制小数"，专为钱这种不能有误差的场景设计。
        //   double 是二进制浮点，0.1 + 0.2 会出误差，所以金额一律用 decimal。
        //   字面量要加 m 后缀才是 decimal（199.00m），否则 199.00 会被当成 double。
        public decimal Amount { get; set; }

        // ★ 实现接口的方法要写 public（接口成员是 public，实现不能缩水），
        //   但【不写 override】—— 这不是"重写父类方法"，而是"履行接口契约"。
        public void Print()
        {
            // Amount 是 decimal，F2 同样适用 → 199.00（正好两位小数）
            Console.WriteLine($"发票：客户 {Customer}，金额 {Amount:F2}");
        }
    }

    class Receipt : IPrintable
    {
        public string Item { get; set; } = "";
        public int Quantity { get; set; }

        public void Print() => Console.WriteLine($"收据：{Item} × {Quantity}");
    }


    // ────────────────────────────────────────────────────────────────
    // TODO 5 ｜ ★ 综合：抽象类 + 多态（本课核心）
    // ────────────────────────────────────────────────────────────────
    // ★ abstract class 的两条硬规则：
    //   ① 抽象方法【没有方法体】，只写签名、以分号结尾；
    //   ② 抽象类【不能 new】（写 new Shape() 会编译错误）。
    //   换句话说：抽象的父类只负责"规定子类必须会什么"，不负责"怎么做"。
    abstract class Shape
    {
        // 面积怎么算？Shape 自己也不知道，所以只声明、不实现，交给子类 override
        public abstract double GetArea();

        // 抽象属性：只给 get、不给 set，子类必须提供它
        public abstract string ShapeName { get; }

        // ★ 这就是抽象类最妙的地方：这是一个【已实现】的方法，
        //   它内部调用了抽象的 GetArea()。Shape 不知道面积怎么算，
        //   但它确信"任何能通过编译的子类，一定实现了 GetArea"（否则子类自己就编译不过）。
        //   → 所以这个 Describe() 对所有图形都通用，加新图形时一行都不用改。
        public void Describe()
        {
            Console.WriteLine($"{ShapeName}：面积 {GetArea():F2}");
        }
    }

    class Rectangle : Shape
    {
        public double Width { get; set; }
        public double Height { get; set; }

        // ★ 抽象属性用 => "表达式体属性"实现：每次读 ShapeName 都返回这个常量字符串。
        //   override 是必须的 —— 不写就编译不过（子类必须实现所有抽象成员）。
        public override string ShapeName => "长方形";

        public override double GetArea() => Width * Height;
    }

    class Circle : Shape
    {
        public double Radius { get; set; }

        public override string ShapeName => "圆";

        // ★ 圆周率按练习题要求用 3.14159（不用 Math.PI）：
        //   3.14159 × 5 × 5 = 78.53975 → 保留两位小数正好是 78.54
        public override double GetArea() => 3.14159 * Radius * Radius;
    }

    class Triangle : Shape
    {
        // ★ 属性名就叫 Base（底边）。base 虽然是 C# 关键字，
        //   但这里首字母大写、是成员名，编译器不会混淆。
        public double Base { get; set; }
        public double Height { get; set; }

        public override string ShapeName => "三角形";

        // ★ 先乘后除：3 × 8 = 24，再 / 2 = 12。
        //   别写成 Base * (Height / 2) —— Height 是奇数时会先丢精度。
        public override double GetArea() => Base * Height / 2;
    }

    // 【自测加分】正方形：再加一个新图形，主程序的循环和 TotalArea() 一行都不用改 ——
    //   因为主程序只认识 Shape，而 Shape 已经保证了"每个子类都会算面积"。
    //   （本次没有把它放进 shapes 数组，所以不影响预期输出。）
    class Square : Shape
    {
        public double Side { get; set; }

        public override string ShapeName => "正方形";
        public override double GetArea() => Side * Side;
    }




    class Lesson07Practice
    {
        // ────────────────────────────────────────────────────────────
        // TODO 4 的辅助方法：打印任何"能打印的东西"
        // ────────────────────────────────────────────────────────────
        // ★ 参数类型写成【接口】IPrintable，而不是某一个具体的类。
        //   这一个方法就同时吃下了 Invoice、Receipt，以及以后新写的任何实现类。
        //   对比第 05 课的写法：那边只能传 Student[]，来了别的类型就得再写一个方法。
        static void PrintAll(IPrintable[] items)
        {
            // ★ 这里只看得到 Print() 这一个成员 —— 接口规定了什么，方法里就能用什么。
            //   所以下面【不能】写 item.Amount 或 item.Quantity：
            //   接口不认识它们，那是各自实现类自己的东西。
            foreach (IPrintable item in items)
            {
                // ★ 明明是同一句 item.Print()，实际执行的却是各自类里的实现 ——
                //   接口 + 多态，和第 07 课正课里 MakeItFly(IFlyable) 是同一个套路。
                item.Print();
            }
        }

        // ────────────────────────────────────────────────────────────
        // TODO 5 的辅助方法：求所有图形的面积总和
        // ────────────────────────────────────────────────────────────
        // ★ 参数是【抽象父类】数组。方法体里完全找不到"长方形""圆""三角形"这些字眼，
        //   却能算出正确结果 —— 因为它依赖的不是具体形状，而是 Shape 的承诺："我能算面积"。
        static double TotalArea(Shape[] shapes)
        {
            // ★ 求和的累加器初值是 0（这里不是"求最值"那个坑 ——
            //   最值必须拿 arr[0] 当起点，求和用 0 才是对的，因为 0 是加法的单位元）。
            double total = 0;

            foreach (Shape s in shapes)
            {
                // ★ 多态在这里再次生效：循环变量 s 是 Shape，
                //   但 s.GetArea() 会按"实际是哪个子类"去调用对应的实现。
                total += s.GetArea();
            }

            return total;
        }

        static void Main()
        {
            Console.WriteLine("===== 第 07 课练习：继承、接口与多态 =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 最简单的继承
            // ================================================================
            // 【要求】在上面定义两个类：
            //     ① class Person
            //          string Name { get; set; }
            //          int Age { get; set; }
            //          void Introduce()  → 打印 "我叫 XXX，今年 XX 岁"
            //     ② class Student : Person          ← 继承 Person
            //          string School { get; set; }
            //          void Study()      → 打印 "XXX 在 YYY 学习"
            //
            //   然后创建一个 Student，调用 Introduce() 和 Study()
            //
            // 【预期输出】
            //     我叫 张三，今年 20 岁
            //     张三 在 重庆大学 学习
            //
            // 【语法提示】
            //     class Person
            //     {
            //         public string Name { get; set; } = "";
            //         public int Age { get; set; }
            //         public void Introduce() => Console.WriteLine($"我叫 {Name}，今年 {Age} 岁");
            //     }
            //
            //     class Student : Person          // ← 冒号表示继承
            //     {
            //         public string School { get; set; } = "";
            //         public void Study() => Console.WriteLine($"{Name} 在 {School} 学习");
            //     }
            //
            //   ★ 注意 Study() 里直接用 Name —— 那是从父类继承来的
            //
            // 【考察点】子类自动拥有父类的所有 public 成员

            Console.WriteLine("--- TODO 1 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 创建对象：用的是"带参构造函数"这种写法
            //   ★ 类定义只有一份，而 TODO 2 要把 Person 改成【只提供】Person(string, int)
            //     （没有无参构造函数），所以为了让程序从头到尾都能编译，
            //     这里直接用构造函数创建：三个参数按顺序对上 Student(name, age, school)。
            //
            //   ★ 想一想 TODO 1 的原始状态（父类还没加构造函数）：
            //     那时父类有隐式的无参构造函数，"对象初始化器"也是合法的，可以写成
            //         Student s = new Student { Name = "张三", Age = 20, School = "重庆大学" };
            //     两种写法的效果一样，区别在于 ——
            //       构造函数     ：圆括号，按【位置】传值，创建时就必须把参数一次给全
            //       对象初始化器 ：花括号，按【属性名】传值，想给几个给几个
            //     第 05 课已经对比过这两种方式，这里正好再复习一遍。
            Student s1 = new Student("张三", 20, "重庆大学");

            // ② Introduce() 是父类的方法，Study() 是子类自己的方法 —— 两个都能调
            //   ★ 输出里的"全角逗号"是写死在 Person.Introduce() 里的，这里不用管；
            //     "在……学习"这句话里的 Name 也是从父类继承来的属性
            s1.Introduce();
            s1.Study();

            // ★★ 继承最直接的收益：Study() 里能直接用 Name，
            //    因为 Person 里已经写过的成员，Student 不用再抄一遍。
            //    这也是 TODO 2 那句 : base(...) 存在的原因 —— 父类负责的初始化，
            //    子类不能越俎代庖，只能"转交"。

            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ 构造函数和 base
            // ================================================================
            // 【要求】改造 TODO 1 的类：
            //     ① Person 加一个构造函数 Person(string name, int age)
            //     ② Student 加构造函数 Student(string name, int age, string school)
            //          必须用 : base(name, age) 把前两个参数交给父类
            //
            //   然后创建时一次传完所有参数
            //
            // 【预期输出】
            //     我叫 李四，今年 22 岁
            //     李四 在 西南大学 学习
            //
            // 【语法提示】
            //     class Person
            //     {
            //         public string Name { get; set; }
            //         public int Age { get; set; }
            //
            //         public Person(string name, int age)
            //         {
            //             Name = name;
            //             Age = age;
            //         }
            //     }
            //
            //     class Student : Person
            //     {
            //         public string School { get; set; }
            //
            //         // ★ : base(name, age) 把参数转交给父类构造函数
            //         public Student(string name, int age, string school) : base(name, age)
            //         {
            //             School = school;
            //         }
            //     }
            //
            //   创建：var s = new Student("李四", 22, "西南大学");
            //
            // 【考察点】
            //   · 父类构造函数【先】执行
            //   · 父类没有无参构造函数时，子类【必须】写 : base(...)，否则编译错误

            Console.WriteLine("--- TODO 2 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 用【构造函数】创建：三个参数按顺序对上 Student(string name, int age, string school)
            //   ★ 这一次不能再写对象初始化器了 —— Person 已经【没有】无参构造函数，
            //     "new Student { ... }" 会编译报错 CS7036，必须把参数一次传完。
            Student s2 = new Student("李四", 22, "西南大学");

            // ② 调用继承来的 Introduce() 和自己的 Study()
            s2.Introduce();
            s2.Study();

            // ★★ 这三行代码背后的执行顺序（面试和考试都爱问）：
            //   1. 先执行 Person 的构造函数 → Name = "李四"、Age = 22 就位
            //   2. 再执行 Student 的函数体   → School = "西南大学" 就位
            //   3. 对象才算创建完成，s2 拿到它
            //   ★ 推论：子类构造函数里【可以】读 Name（那时父类已经赋过值了），
            //     反过来父类构造函数里读 School 会读到初始值/默认值，因为子类还没跑。

            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ virtual + override（多态的基础）
            // ================================================================
            // 【要求】定义三个类：
            //     ① class Employee（员工）
            //          string Name { get; set; }
            //          virtual double CalculateSalary()   → 返回 3000（基本工资）
            //     ② class Manager : Employee（经理）
            //          override double CalculateSalary()  → 返回 3000 + 5000（管理津贴）
            //     ③ class Salesperson : Employee（销售）
            //          double SalesAmount { get; set; }
            //          override double CalculateSalary()  → 返回 3000 + 销售额 × 5%
            //
            //   然后验证：把三个对象都装进 Employee[] 数组，用循环调用 CalculateSalary()
            //
            // 【预期输出】
            //     ========== 工资表 ==========
            //     张三（普通员工）：3000.00
            //     李四（经理）：8000.00
            //     王五（销售）：4500.00
            //
            // 【提示】
            //     class Employee
            //     {
            //         public string Name { get; set; } = "";
            //         public virtual double CalculateSalary() => 3000;   // ★ virtual
            //     }
            //
            //     class Manager : Employee
            //     {
            //         public override double CalculateSalary() => 8000;   // ★ override
            //     }
            //
            //     class Salesperson : Employee
            //     {
            //         public double SalesAmount { get; set; }
            //         public override double CalculateSalary() => 3000 + SalesAmount * 0.05;
            //     }
            //
            //   关键测试（这就是多态）：
            //     Employee[] staff = {
            //         new Employee { Name = "张三" },
            //         new Manager { Name = "李四" },
            //         new Salesperson { Name = "王五", SalesAmount = 30000 }
            //     };
            //     foreach (Employee e in staff)
            //     {
            //         Console.WriteLine($"{e.Name}：{e.CalculateSalary():F2}");   // ★ 自动调用各自的版本
            //     }
            //
            // 【考察点】
            //   · 父类必须写 virtual，子类必须写 override，多态才生效
            //   · 数组类型是 Employee[]，但里面装的是不同的子类对象

            Console.WriteLine("--- TODO 3 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① ★ 数组的类型是【父类】Employee[]，里面却混装了三种不同的对象
            //   这就是多态的前提：变量（数组元素）的声明类型是 Employee，
            //   而每个元素【实际指向的对象】分别是 Employee / Manager / Salesperson。
            //   数组初始化器的简写 `{ ... }` 在第 03 课学过，这里元素变成了对象而已。
            Employee[] staff =
            {
                new Employee { Name = "张三" },                          // 普通员工 → 基本工资
                new Manager { Name = "李四" },                           // 经理 → override 的 8000
                new Salesperson { Name = "王五", SalesAmount = 30000 }   // 销售 → 3000 + 30000×5%
            };

            // ★ 岗位后缀（"（经理）"这种）怎么办？
            //   最差的做法：再开一个并列的 string[] jobLabels，靠下标对应 ——
            //     那是第 03 课的"平行数组"老办法，第 05 课已经证明它很脆弱：
            //     数组里少写一个不会报错，但下标会整体错位，输出就全乱了。
            //   ★ 正确做法：岗位是"员工自己的信息"，就该放进 Employee 类里，
            //     并且用 virtual + override —— 让【多态】去决定每个人显示什么。
            //     所以上面 Employee/Manager/Salesperson 各有一个 Title 属性。
            //     → Main 里只要写 e.Title，运行时自动拿到该对象的那一份。
            Console.WriteLine("========== 工资表 ==========");

            // ② ★ 这里可以用 foreach 了！因为不再需要下标 ——
            //   岗位、姓名、工资三样数据全部从同一个对象 e 里取，
            //   不存在"两个数组下标要对齐"的问题。
            //   （对比第 03 课那种必须用 for 的场景：那里要同时取 names[i] 和 grades[i]。）
            foreach (Employee e in staff)
            {
                // ★★ 全课最关键的一行：e 声明成 Employee，到底走哪个版本？
                //   · 编译期看左边：Employee 里【有】CalculateSalary（因为父类写了 virtual），所以能编译
                //   · 运行期看右边：运行时根据"e 实际指向的对象"去找最贴近的那个 override 版本
                //   → 同一句代码，三圈循环打出三个不同结果，这就是多态。
                //   ★ 如果父类漏写 virtual（子类只能用 new 隐藏），这里会三行全打印 3000 ——
                //     多态失效，这是本课最大的坑。
                //   ★★ 同理，e.Title 也是多态调用：Manager 的对象返回"（经理）"。
                //      一句 e.Title 同时享受了"多态"和"数据跟着对象走"两个好处。
                //   F2 保留两位小数，把 3000 显示成 3000.00
                Console.WriteLine($"{e.Name}{e.Title}：{e.CalculateSalary():F2}");
            }

            // ③ 再补一个细节：
            //   · SalesAmount 只在 Salesperson 里有，所以只能写在 new Salesperson 的那一项里；
            //     写成 e.SalesAmount 会编译不过 —— 父类类型看不到子类特有的成员。
            //   ★ 对比一下 Title：它声明在【父类】Employee 里（虽然是 virtual），
            //     所以 e.Title 能编译；而 SalesAmount 只在子类里，e.SalesAmount 就不能编译。
            //     → 一条规律：变量的声明类型决定了"你能点出哪些成员"。

            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ 接口（能做什么）
            // ================================================================
            // 【要求】定义接口和两个实现类：
            //     ① interface IPrintable
            //          void Print();
            //     ② class Invoice : IPrintable（发票）
            //          string Customer { get; set; }
            //          decimal Amount { get; set; }
            //          void Print() → 打印 "发票：客户 XXX，金额 XX.XX"
            //     ③ class Receipt : IPrintable（收据）
            //          string Item { get; set; }
            //          int Quantity { get; set; }
            //          void Print() → 打印 "收据：XXX × N"
            //
            //   然后写一个方法 PrintAll(IPrintable[] items)，用循环调用 Print()
            //
            // 【预期输出】
            //     ========== 打印所有单据 ==========
            //     发票：客户 张三，金额 199.00
            //     收据：笔记本 × 3
            //     ========== 打印完成 ==========
            //
            // 【提示】
            //     interface IPrintable              // ★ 惯例 I 开头
            //     {
            //         void Print();                 // 接口成员不写 public，默认就是 public
            //     }
            //
            //     class Invoice : IPrintable        // 实现接口用冒号，和继承写法一样
            //     {
            //         public string Customer { get; set; } = "";
            //         public decimal Amount { get; set; }
            //
            //         public void Print()           // ★ 实现接口方法要写 public，但【不写 override】
            //         {
            //             Console.WriteLine($"发票：客户 {Customer}，金额 {Amount:F2}");
            //         }
            //     }
            //
            //     static void PrintAll(IPrintable[] items)
            //     {
            //         foreach (IPrintable item in items) item.Print();
            //     }
            //
            //   ★ Invoice 和 Receipt 没有继承关系，但都"能打印"，所以都能传进 PrintAll
            //
            // 【考察点】
            //   · 实现接口的方法要 public，但不写 override
            //   · 一个方法接受接口类型 = 接受所有实现了这个接口的类

            Console.WriteLine("--- TODO 4 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① ★ 数组的类型是【接口】IPrintable，里面装的是两个毫不相干的类
            //   Invoice 和 Receipt 之间没有继承关系，唯一的共同点就是都实现了 IPrintable。
            //   接口在这里起的作用是"统一门牌号"：只要挂了这块牌子，就能进同一个数组。
            IPrintable[] documents =
            {
                new Invoice { Customer = "张三", Amount = 199.00m },   // ★ 199.00m 的 m = decimal 字面量
                new Receipt { Item = "笔记本", Quantity = 3 }
            };

            Console.WriteLine("========== 打印所有单据 ==========");

            // ② 把这批单据整批交给 PrintAll 处理
            //   ★ 妙处在于 PrintAll 里【看不到】Invoice 和 Receipt 这两个名字，
            //     它只知道"来的东西都会 Print()"。以后新增一种单据（比如 class Bill : IPrintable），
            //     PrintAll 一个字都不用改 —— 这就是"面向接口编程"。
            PrintAll(documents);

            Console.WriteLine("========== 打印完成 ==========");

            // ★ 多态在接口上同样生效：数组元素声明类型是 IPrintable，
            //   但 item.Print() 实际执行的是各自类里写的那一份实现。
            //   和 TODO 3 的 virtual/override 是同一个道理，只是"契约"换成了接口。

            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ ★ 综合：抽象类 + 多态（本课考核）
            // ================================================================
            // 【背景】需求：多种图形的面积计算，将来还会不断加新图形。
            //        希望"加新图形时，主程序一行都不用改"。
            //
            // 【要求】
            //     ① abstract class Shape
            //          abstract double GetArea();              抽象方法
            //          abstract string ShapeName { get; }      抽象属性
            //          void Describe()  → 打印 "XXX：面积 XX.XX"（已实现，内部调用 GetArea）
            //     ② class Rectangle : Shape    宽 × 高
            //     ③ class Circle : Shape       3.14159 × 半径²
            //     ④ class Triangle : Shape     底 × 高 / 2
            //     ⑤ static double TotalArea(Shape[] shapes)  返回所有图形面积总和
            //
            //   然后创建 3 种图形，用循环 Describe()，最后打印总面积
            //
            // 【预期输出】
            //     ========== 图形面积报表 ==========
            //     长方形：面积 24.00
            //     圆：面积 78.54
            //     三角形：面积 12.00
            //     ----------------------------------
            //     总面积：114.54
            //
            // 【提示】
            //     abstract class Shape
            //     {
            //         public abstract double GetArea();          // ★ 没有方法体，以分号结尾
            //         public abstract string ShapeName { get; }
            //
            //         public void Describe()                     // 已实现的方法
            //         {
            //             Console.WriteLine($"{ShapeName}：面积 {GetArea():F2}");
            //             // ★ 注意：Shape 不知道怎么算面积，但它知道【子类一定实现了】
            //         }
            //     }
            //
            //     class Rectangle : Shape
            //     {
            //         public double Width { get; set; }
            //         public double Height { get; set; }
            //         public override string ShapeName => "长方形";
            //         public override double GetArea() => Width * Height;
            //     }
            //
            //     static double TotalArea(Shape[] shapes)
            //     {
            //         double total = 0;
            //         foreach (Shape s in shapes) total += s.GetArea();   // ★ 多态
            //         return total;
            //     }
            //
            // 【考察点】
            //   · abstract 方法没有方法体
            //   · 抽象类不能 new（试着写 new Shape() 看编译错误）
            //   · 子类必须 override 所有抽象成员
            //   · Describe() 和 TotalArea() 都只认 Shape，不关心具体是什么图形
            //     —— 这就是"加新图形时主程序不用改"的原因
            //
            // 【自测】再加一个 class Square : Shape（正方形），
            //        验证主程序的循环和 TotalArea 一行都不用改

            Console.WriteLine("--- TODO 5 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① ★ 一句话说明本课核心：数组类型是【抽象父类】Shape[]，
            //   而 Shape 根本不能 new —— 数组里装的必然是它的各种子类对象。
            //   （要是写成 new Shape()，编译器会直接报错：无法创建抽象类的实例。）
            Shape[] shapes =
            {
                new Rectangle { Width = 4, Height = 6 },     // 4 × 6 = 24
                new Circle { Radius = 5 },                   // 3.14159 × 25 = 78.53975
                new Triangle { Base = 3, Height = 8 }        // 3 × 8 / 2 = 12
            };

            Console.WriteLine("========== 图形面积报表 ==========");

            // ② ★ Describe() 是【父类已经实现好的】方法，它内部调用了抽象的 GetArea()。
            //   所以这一行循环不需要知道"这是长方形还是圆"，
            //   只要对象是 Shape，它就会自己算对自己的面积。
            //   ★ 以后加一个 Square（正方形），这个循环一个字都不用改 —— 这就是需求里
            //     "加新图形时主程序一行都不用改"的实现方式。
            foreach (Shape shape in shapes)
            {
                shape.Describe();
            }

            Console.WriteLine("----------------------------------");

            // ③ 把整批图形交给 TotalArea 求总和
            //   ★ TotalArea 里同样只认 Shape，它完全不知道有长方形的存在。
            //   打印时格式化成 F2 —— 78.53975 + 24 + 12 = 114.53975 → 114.54
            double total = TotalArea(shapes);
            Console.WriteLine($"总面积：{total:F2}");

            // 【自测】把 class Square（定义在上面）加进数组试试：
            //     new Square { Side = 5 }        // 面积 25，总面积会变成 139.54
            //   ★ 你会发现：只要在数组里加一行，报表和总面积就自动多出一行，
            //     其他代码完全没动 —— 这就是"对扩展开放，对修改关闭"。

            Console.WriteLine();


            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
