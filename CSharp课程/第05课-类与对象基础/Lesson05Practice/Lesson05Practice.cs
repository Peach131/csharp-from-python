namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ TODO 1~5 的类定义写在这里（在 Lesson05Practice 类的【外面】）↓↓↓
    // ================================================================




    class Lesson05Practice
    {
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




            Console.WriteLine();


            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}