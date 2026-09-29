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




    class Lesson07Practice
    {
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




            Console.WriteLine();


            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
