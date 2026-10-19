namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ TODO 1~5 的类定义写在这里（在 Lesson05Practice 类的【外面】）↓↓↓
    // ================================================================

    // ────────────────────────────────────────────────────────────────
    // TODO 1 ｜ 你的第一个类：Book
    // ────────────────────────────────────────────────────────────────
    class Book
    {
        // { get; set; } 叫【自动属性】：编译器自动在背后生成一个私有字段来存值
        // ★ public string Title; 这种写法是【字段】，不是属性 ——
        //   它没法加校验、没法只读、也没法用在数据绑定里（第 05 课正课讲了这个区别）
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public double Price { get; set; }

        // ★ 为什么 string 要给 = "" 初值？
        //   string 是【引用类型】，不赋值默认就是 null。
        //   而本项目开了 <Nullable>enable</Nullable>，编译器会报 CS8618 警告：
        //   "不可为 null 的属性在退出构造函数时必须包含非 null 值"。
        //   给它一个空字符串初值，既消掉警告，也避免以后 .Title.Length 直接崩。
        //   double 是值类型，默认 0，不会为 null，所以不用给初值。
    }


    // ────────────────────────────────────────────────────────────────
    // TODO 2 ｜ 给属性加"门禁"：手动属性 + 私有字段
    // ────────────────────────────────────────────────────────────────
    class Product
    {
        // ★ 命名约定：私有字段用 _ 开头、小驼峰（_price）
        //   公开属性用大驼峰（Price）。一眼就能看出哪个是"对外的门"，哪个是"里面的房间"。
        private double _price;

        public string Name { get; set; } = "";

        // ★ 这个属性【不是】自动属性 —— 因为 get/set 的方法体是手写的
        //   只要你不写 { get; set; } 那种简写，编译器就不会自动生成背后字段，
        //   所以必须自己声明 _price 来存数据。
        public double Price
        {
            // get 访问器：别人读 product.Price 时执行这里
            get { return _price; }

            // set 访问器：别人写 product.Price = xxx 时执行这里
            // ★ value 是 C# 的关键字，代表"等号右边传进来的那个值"，不用自己声明
            set
            {
                if (value <= 0)
                {
                    Console.WriteLine($"[拒绝] 价格 {value} 不合法，已忽略");
                    // ★★ 这里的 return 是【结束 set 方法】，不是"返回一个值"
                    //    效果 = 跳过下面那行赋值 → 对象保持原样（这就是"门禁"）
                    return;
                }

                _price = value;
            }
        }
    }


    // ────────────────────────────────────────────────────────────────
    // TODO 3 ｜ 构造函数 + 计算属性（用方法算）
    // ────────────────────────────────────────────────────────────────
    class Rectangle
    {
        // 这两个属性不打算对外改，所以用 { get; } 只读（只能在构造函数里赋值）
        public double Width { get; }
        public double Height { get; }

        // ★ 构造函数三要素：
        //   1. 名字【必须】和类名完全一样
        //   2. 【不能】写返回类型（连 void 都不写）
        //   3. new Rectangle(...) 时自动执行
        public Rectangle(double width, double height)
        {
            // ★ 这里 Width = width 左边是属性、右边是参数，编译器分得清，不用写 this
            //   （什么时候必须写 this？当参数名和属性名一样时，如 this.width = width）
            Width = width;
            Height = height;
        }

        // 表达式体方法 `=> 表达式`，等价于 { return 表达式; }
        public double GetArea() => Width * Height;

        public double GetPerimeter() => 2 * (Width + Height);
    }


    // ────────────────────────────────────────────────────────────────
    // TODO 4 ｜ record：一行等于一整个数据类
    // ────────────────────────────────────────────────────────────────
    // ★ 这一行代码，编译器会替你生成：
    //   · 3 个 { get; init; } 属性（只读，初始化时才能赋）
    //   · 一个构造函数 Employee(string Name, int Age, string Department)
    //   · ToString()  → 输出 "Employee { Name = 张三, Age = 28, Department = 研发部 }"
    //   · Equals / GetHashCode → 【按值比较】
    //   · == 和 != 运算符重载 → 也是按值比较
    //   · 一个 Deconstruct 方法（支持拆包）
    //   写成普通 class 要 30 行以上，还有可能漏写 Equals 导致比较出错。
    record Employee(string Name, int Age, string Department);


    // ────────────────────────────────────────────────────────────────
    // TODO 5 ｜ Student：用 init 属性 + 对象数组
    // ────────────────────────────────────────────────────────────────
    class Student
    {
        // ★ init 和 set 的区别：
        //   set  ：任何时候都能改（s.Name = "x" 合法）
        //   init ：只在【创建对象时】能赋值，之后改就编译报错
        //   这里用 init，因为学生信息创建后不应该再被改 —— 不可变更安全。
        public string Name { get; init; } = "";
        public int Grade { get; init; }
    }




    class Lesson05Practice
    {
        // ────────────────────────────────────────────────────────────
        // TODO 5 的辅助方法：在 Student 数组里找最高分
        // ────────────────────────────────────────────────────────────
        // ★ 为什么这个方法的参数类型能写 Student？
        //   因为 Student 定义在同一个 namespace（LessonPractice）里，
        //   同一个命名空间内的类型【不需要 using】就能直接引用。
        static int FindMaxGrade(Student[] students)
        {
            // ★★ 对比第 03/04 课的写法，这里是本课最重要的进步：
            //   第 03 课： int max = grades[0];       ← 从数组里取
            //   第 05 课： int max = students[0].Grade; ← 从【对象的属性】里取
            //   对象的属性就是"这个对象自己的数据"，取法和普通变量一样。
            int max = students[0].Grade;

            // ★ 这里能写 students.Length 吗？
            //   能 —— 数组无论是 int[] 还是 Student[]，都有 Length。
            //   Length 属于"数组"这个东西，不属于元素类型。
            for (int i = 1; i < students.Length; i++)
            {
                if (students[i].Grade > max)
                {
                    max = students[i].Grade;
                }
            }

            return max;
        }

        static void Main()
        {
            Console.WriteLine("===== 第 05 课练习：类与对象基础 =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 定义你的第一个类
            // ================================================================
            // 【要求】在上面定义一个类 Book：
            //     - 三个【自动属性】：string Title / string Author / double Price
            //   然后创建一个 Book 对象并打印
            // 【预期输出】
            //     书名：C# 入门
            //     作者：张三
            //     价格：59.90
            // 【语法提示】
            //     class Book
            //     {
            //         public string Title { get; set; } = "";
            //         public string Author { get; set; } = "";
            //         public double Price { get; set; }
            //     }
            //     // 创建对象 + 对象初始化器
            //     var book = new Book { Title = "C# 入门", Author = "张三", Price = 59.9 };
            // 【考察点】
            //   · 类必须写在 namespace 里、别的 class 的外面
            //   · 属性要写 { get; set; }，不能只写 public string Title;
            //   · string 属性给个 = "" 初值，避免默认 null 触发 CS8618 警告

            Console.WriteLine("--- TODO 1 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① new 创建对象 + 对象初始化器一次性给属性赋值
            //   ★ 对象初始化器的原理：编译器把它翻译成
            //        var book = new Book();
            //        book.Title = "C# 入门";
            //        book.Author = "张三";
            //        book.Price = 59.9;
            //     所以属性必须有 set 或 init 才能这么写。
            Book book = new Book { Title = "C# 入门", Author = "张三", Price = 59.9 };

            // ② 用 对象名.属性名 读数据
            //   ★ Price 是 double，{book.Price:F2} 里的 F2 保留两位小数 → 59.90
            Console.WriteLine($"书名：{book.Title}");
            Console.WriteLine($"作者：{book.Author}");
            Console.WriteLine($"价格：{book.Price:F2}");

            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ 给属性加校验逻辑
            // ================================================================
            // 【要求】定义类 Product：
            //     - string Name { get; set; }     商品名
            //     - double Price { get; set; }    价格【必须大于 0】
            //   Price 要用"手动属性 + 私有字段"实现：
            //     赋值 <= 0 时打印"价格 X 不合法，已忽略"，并且【不赋值】
            //   测试：先赋 99.5（合法），再赋 -10（非法）
            // 【预期输出】
            //     设置价格 99.5 → 当前价格：99.50
            //     [拒绝] 价格 -10 不合法，已忽略
            //     当前价格仍然是：99.50
            // 【语法提示】
            //     class Product
            //     {
            //         private double _price;              // 私有字段，_ 开头
            //         public string Name { get; set; } = "";
            //         public double Price
            //         {
            //             get { return _price; }
            //             set
            //             {
            //                 if (value <= 0)
            //                 {
            //                     Console.WriteLine($"[拒绝] 价格 {value} 不合法，已忽略");
            //                     return;                 // ★ 不赋值，直接返回
            //                 }
            //                 _price = value;
            //             }
            //         }
            //     }
            // 【考察点】set 里的 return 是"不赋值"，不是"返回值"

            Console.WriteLine("--- TODO 2 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            Product product = new Product { Name = "机械键盘" };
            // ★ 对象初始化器里只写了 Name。Price 没写是有意的 ——
            //   初始化器【不会】触发 Price 的 set（没写就不会调用），
            //   所以 _price 保持默认值 0，正好用来验证"99.5 是真的被写进去了"。

            product.Price = 99.5;      // 走 set → 99.5 > 0 → 通过 → _price = 99.5
            Console.WriteLine($"设置价格 99.5 → 当前价格：{product.Price:F2}");

            // ★ 这一行触发 set 里的拒绝分支：
            //   value = -10 ≤ 0 → 打印 [拒绝] → return 提前结束 set
            //   → 下面那行 _price = value 根本没执行 → _price 仍是 99.5
            product.Price = -10;
            Console.WriteLine($"当前价格仍然是：{product.Price:F2}");

            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ 构造函数 + 计算属性
            // ================================================================
            // 【要求】定义类 Rectangle：
            //     - double Width  { get; set; }
            //     - double Height { get; set; }
            //     - 构造函数 Rectangle(double width, double height)
            //     - double GetArea()       返回面积
            //     - double GetPerimeter()  返回周长
            //   创建两个长方形并打印
            // 【预期输出】
            //     长方形 4 × 6：面积 24.00，周长 20.00
            //     长方形 3.5 × 2：面积 7.00，周长 11.00
            // 【语法提示】
            //     class Rectangle
            //     {
            //         public double Width { get; set; }
            //         public double Height { get; set; }
            //         public Rectangle(double width, double height)   // 名字=类名，无返回类型
            //         { Width = width; Height = height; }
            //         public double GetArea() => Width * Height;      // 表达式体方法
            //         public double GetPerimeter() => 2 * (Width + Height);
            //     }
            // 【考察点】构造函数名字必须和类名一样，且不写返回类型

            Console.WriteLine("--- TODO 3 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ★ 用【构造函数】创建对象：必须传参，顺序和类型要和构造函数对上
            //   第 1 个 double → width，第 2 个 double → height
            Rectangle rect1 = new Rectangle(4, 6);
            Console.WriteLine($"长方形 4 × 6：面积 {rect1.GetArea():F2}，周长 {rect1.GetPerimeter():F2}");

            // ★ 4 和 2 是 int，构造函数要 double —— C# 自动隐式转换（安全方向）
            Rectangle rect2 = new Rectangle(3.5, 2);
            Console.WriteLine($"长方形 3.5 × 2：面积 {rect2.GetArea():F2}，周长 {rect2.GetPerimeter():F2}");

            // ★ 对比记忆两种创建方式：
            //   对象初始化器：new Book { Title = "x" }   ← 花括号，靠属性名，可按需挑几项
            //   构造函数    ：new Rectangle(4, 6)       ← 圆括号，靠参数位置，必须全给
            //   两者可以同时用：new Rectangle(4, 6) { Width = 5 }（先构造再改，别这么绕）

            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ record：不可变数据类
            // ================================================================
            // 【要求】用 record 定义 Employee：
            //     record Employee(string Name, int Age, string Department);
            //   然后：
            //     1. 创建两个内容完全相同的员工对象
            //     2. 用 == 比较，验证 record 是【按值比较】
            //     3. 直接打印某个对象（验证自动生成了 ToString）
            // 【预期输出】
            //     员工1：Employee { Name = 张三, Age = 28, Department = 研发部 }
            //     员工2：Employee { Name = 张三, Age = 28, Department = 研发部 }
            //     内容相同吗？True
            //     ★ record 按值比较，普通 class 按地址比较
            // 【提示】var e1 = new Employee("张三", 28, "研发部");
            //         var e2 = new Employee("张三", 28, "研发部");
            //         Console.WriteLine($"员工1：{e1}");       // record 自动有 ToString
            //         Console.WriteLine($"内容相同吗？{e1 == e2}");
            // 【考察点】record 一行搞定，不用写字段/属性/构造函数/ToString/Equals
            // 【加分】试试 with 表达式：var e3 = e1 with { Age = 30 };

            Console.WriteLine("--- TODO 4 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ★ 注意这里用的是【构造函数】形式（圆括号），因为 record 那行声明
            //   record Employee(string Name, int Age, string Department);
            //   编译器替你生成了一个"位置构造函数"，参数顺序就是声明顺序。
            Employee e1 = new Employee("张三", 28, "研发部");
            Employee e2 = new Employee("张三", 28, "研发部");

            // ① 直接打印对象 → 触发 record 自动生成的 ToString()
            //   ★ 普通 class 打印出来只会是 "LessonPractice.Employee" 这种类型名，
            //     因为 object.ToString() 默认只返回类型全名。
            //     record 重写了 ToString()，把每个属性都列出来。
            Console.WriteLine($"员工1：{e1}");
            Console.WriteLine($"员工2：{e2}");

            // ② 用 == 比较 → record 重载了 == 运算符，走【按值比较】
            //   逐个属性比：Name 相同 && Age 相同 && Department 相同 → True
            Console.WriteLine($"内容相同吗？{e1 == e2}");
            Console.WriteLine("★ record 按值比较，普通 class 按地址比较");

            // ★★ 反面对照：换成普通 class 会是什么结果？
            //   把下面三行取消注释跑一下 —— 输出是 False。
            //   因为两个 new 出来的对象在【堆上地址不同】，
            //   class 的 == 默认比的是"是不是同一个对象"，不是"内容一不一样"。
            //
            //   Book b1 = new Book { Title = "a" };
            //   Book b2 = new Book { Title = "a" };
            //   Console.WriteLine($"普通 class 比较：{b1 == b2}");   // False

            // ③ 加分项：with 表达式 —— 基于现有对象"复制一份，再改指定属性"
            //   ★ 原理：with 会生成一个【新对象】，把 e1 的所有属性抄过去，
            //     再用花括号里的值覆盖 Age。e1 本身完全不受影响（这才是"不可变"）。
            Employee e3 = e1 with { Age = 30 };
            Console.WriteLine($"with 改年龄后：{e3}");
            Console.WriteLine($"原对象没变：{e1.Age}");

            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ ★ 综合：用对象数组重写成绩报告（本课核心）
            // ================================================================
            // 【背景】第 03/04 课你是用两个数组做的成绩报告：
            //     string[] names  = { ... };
            //     int[]    grades = { ... };
            //     靠"索引相同"来关联，很脆弱。
            // 【要求】改造成用 Student 对象数组：
            //     1. 定义类 Student（Name 和 Grade，用 init 属性）
            //     2. 创建 5 个 Student 对象组成数组
            //     3. 用 foreach 打印报告，并统计总分/平均分/最高分
            // 【数据】
            //     张三 88, 李四 92, 王五 76, 赵六 95, 钱七 81
            // 【预期输出】
            //     ========== 成绩报告 ==========
            //     张三：88
            //     李四：92
            //     王五：76
            //     赵六：95  ★
            //     钱七：81
            //     ------------------------------
            //     总分：432
            //     平均分：86.40
            //     最高分：95
            // 【语法提示】
            //     class Student
            //     {
            //         public string Name { get; init; } = "";
            //         public int Grade { get; init; }
            //     }
            //     // 用对象初始化器创建数组
            //     Student[] students =
            //     {
            //         new Student { Name = "张三", Grade = 88 },
            //         new Student { Name = "李四", Grade = 92 },
            //         // ... 剩下 3 个你来写
            //     };
            //     // 直接遍历对象数组，比两个数组清爽得多
            //     foreach (Student s in students) { Console.WriteLine($"{s.Name}：{s.Grade}"); }
            // 【考察点】
            //   · 这就是"类"存在的意义：把相关的数据绑在一起，不用再靠索引关联
            //   · 想加字段（比如年龄）时只要改类定义，不用再加一个数组
            // 【进阶】也可以把 Student 定义成 record：record Student(string Name, int Grade);

            Console.WriteLine("--- TODO 5 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓

            // ① 用对象初始化器创建对象，直接组成数组
            // ★ 对比第 03 课：那边是两个数组靠"索引相同"关联，
            //   这边一个人的名字和分数【天生就在同一个对象里】，
            //   想加"年龄"只需要改 Student 类定义，不用再开第三个数组。
            Student[] students =
            {
                new Student { Name = "张三", Grade = 88 },
                new Student { Name = "李四", Grade = 92 },
                new Student { Name = "王五", Grade = 76 },
                new Student { Name = "赵六", Grade = 95 },
                new Student { Name = "钱七", Grade = 81 },
            };

            // ② 先求最高分（用上面写的辅助方法）
            int topGrade = FindMaxGrade(students);

            // ③ 遍历打印 + 顺便累加总分
            // ★ 为什么用 foreach 而不是 for？
            //   因为这里【不需要索引】—— 不靠下标取东西，只关心"每个学生"。
            //   foreach 的语义就是"把每个元素挨个给我"，比 for 少写三处下标，也更不容易越界。
            Console.WriteLine("========== 成绩报告 ==========");

            int totalScore = 0;
            foreach (Student s in students)
            {
                totalScore += s.Grade;

                // 和第 03 课一样的三元运算符，只是取值从 grades[i] 变成了 s.Grade
                string mark = s.Grade == topGrade ? "  ★" : "";
                Console.WriteLine($"{s.Name}：{s.Grade}{mark}");
            }

            Console.WriteLine("------------------------------");

            // ④ 平均分：★ 先转 double 再除（同一个坑第三次出现，这次别再忘了）
            double averageScore = (double)totalScore / students.Length;

            Console.WriteLine($"总分：{totalScore}");
            Console.WriteLine($"平均分：{averageScore:F2}");
            Console.WriteLine($"最高分：{topGrade}");

            Console.WriteLine();


            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}