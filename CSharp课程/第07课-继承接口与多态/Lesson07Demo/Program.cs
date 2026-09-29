// ============================================================================
//  第 07 课 · 示例代码
//  继承、接口与多态 —— 本课是 OOP 的重头戏
// ============================================================================

namespace Lesson07Demo
{
    // ========================================================================
    // 一、抽象父类 + virtual 方法 + 继承
    // ========================================================================
    abstract class Animal                              // abstract：不能 new
    {
        public string Name { get; set; }

        // 父类构造函数需要 name
        protected Animal(string name)                  // protected：只有子类能调用
        {
            Name = name;
            Console.WriteLine($"      [Animal 构造函数] {name}");
        }

        // 抽象方法：没有方法体，子类【必须】实现
        public abstract void Speak();

        // 已实现的方法：子类直接继承，不用重写
        public void Eat() => Console.WriteLine($"      {Name} 在吃东西");

        // virtual 方法：子类【可以】重写
        public virtual void Describe()
        {
            Console.WriteLine($"      我是 {Name}");
        }
    }

    class Dog : Animal
    {
        public string Breed { get; set; }

        // ★ 用 : base(name) 把 name 交给父类构造函数
        public Dog(string name, string breed) : base(name)
        {
            Breed = breed;
            Console.WriteLine($"      [Dog 构造函数] 品种 {breed}");
        }

        public override void Speak() => Console.WriteLine($"      {Name}：汪汪！");

        public override void Describe()
        {
            base.Describe();                           // ★ 先调用父类的版本
            Console.WriteLine($"      品种是 {Breed}");   // 再加上自己的
        }

        public void Bark() => Console.WriteLine($"      {Name} 在汪汪叫（Dog 特有）");
    }

    class Cat : Animal
    {
        public Cat(string name) : base(name) { }

        public override void Speak() => Console.WriteLine($"      {Name}：喵喵～");
    }


    // ========================================================================
    // 二、接口：表达"能做什么"
    // ========================================================================
    interface IFlyable
    {
        void Fly();                                    // 接口成员默认就是 public
        int MaxAltitude { get; }                       // 属性也可以
    }

    // 鸟：既是一个 Animal，又能飞
    class Bird : Animal, IFlyable                      // ★ 父类在前，接口在后
    {
        public Bird(string name) : base(name) { }

        public override void Speak() => Console.WriteLine($"      {Name}：啾啾！");

        public void Fly() => Console.WriteLine($"      {Name} 在飞");
        public int MaxAltitude => 1000;
    }

    // 飞机：不是动物，但也能飞 —— 接口把"能做什么"和"是什么"解耦了
    class Airplane : IFlyable
    {
        public string Model { get; set; } = "波音 737";
        public void Fly() => Console.WriteLine($"      {Model} 在飞");
        public int MaxAltitude => 12000;
    }


    // ========================================================================
    // 三、抽象类 + 多态：图形面积计算
    // ========================================================================
    abstract class Shape
    {
        public abstract double GetArea();              // 面积怎么算，父类不知道

        public abstract string ShapeName { get; }

        // ★ 已实现的方法里调用了【抽象方法】—— 这就是抽象类的威力
        public void Describe()
        {
            Console.WriteLine($"      {ShapeName}：面积 {GetArea():F2}");
        }
    }

    class Rectangle : Shape
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public override string ShapeName => "长方形";
        public override double GetArea() => Width * Height;
    }

    class Circle : Shape
    {
        public double Radius { get; set; }
        public override string ShapeName => "圆";
        public override double GetArea() => 3.14159 * Radius * Radius;
    }

    class Triangle : Shape
    {
        public double Base { get; set; }
        public double Height { get; set; }
        public override string ShapeName => "三角形";
        public override double GetArea() => Base * Height / 2;
    }


    // ========================================================================
    // 四、演示 virtual vs new（多态是否生效）
    // ========================================================================
    class BaseClass
    {
        public virtual void VirtualMethod() => Console.WriteLine("      Base.VirtualMethod");
        public void NormalMethod() => Console.WriteLine("      Base.NormalMethod");
    }

    class DerivedClass : BaseClass
    {
        public override void VirtualMethod() => Console.WriteLine("      Derived.VirtualMethod（override）");
        public new void NormalMethod() => Console.WriteLine("      Derived.NormalMethod（new 隐藏）");
    }


    class Program
    {
        static void Main()
        {
            Console.WriteLine("========== 第 07 课：继承、接口与多态 ==========");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 1：继承和构造函数的执行顺序 ----");
            Console.WriteLine("   创建 new Dog(\"旺财\", \"柴犬\")：");
            var dog = new Dog("旺财", "柴犬");
            Console.WriteLine("   ★ 父类构造函数【先】执行，然后才是子类");
            Console.WriteLine();

            Console.WriteLine("   子类能用的成员：");
            dog.Eat();          // 从父类继承
            dog.Speak();        // 子类实现
            dog.Bark();         // 子类特有
            Console.WriteLine();

            Console.WriteLine("   Describe() 里用 base.Describe() 先调父类：");
            dog.Describe();
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 2：★ 多态 ----");

            // ★ 父类类型的变量，装子类对象
            Animal a1 = new Dog("大黄", "金毛");
            Animal a2 = new Cat("咪咪");

            Console.WriteLine("   a1 和 a2 都声明为 Animal，但实际是不同对象：");
            a1.Speak();         // 汪汪 —— 调用的是 Dog 的版本
            a2.Speak();         // 喵喵 —— 调用的是 Cat 的版本
            Console.WriteLine();

            Console.WriteLine("   一个方法处理所有动物（将来加新动物也不用改）：");
            Animal[] zoo = { new Dog("阿黄", "土狗"), new Cat("小花"), new Bird("小雀") };
            foreach (Animal a in zoo)
            {
                a.Speak();
            }
            Console.WriteLine("   ★ 同一句 a.Speak()，执行哪个版本取决于【实际是什么对象】");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 3：接口（能做什么）----");

            // 一个方法，接受"任何能飞的"
            void MakeItFly(IFlyable thing)
            {
                thing.Fly();
                Console.WriteLine($"      最高飞到 {thing.MaxAltitude} 米");
            }

            Console.WriteLine("   鸟能飞：");
            MakeItFly(new Bird("大雁"));
            Console.WriteLine("   飞机也能飞（虽然它不是动物 —— 它和 Bird 没有继承关系）：");
            MakeItFly(new Airplane());
            Console.WriteLine("   ★ 接口把'能做什么'和'是什么'解耦了");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 4：抽象类 + 多态（图形面积）----");

            Shape[] shapes =
            {
                new Rectangle { Width = 4, Height = 6 },
                new Circle { Radius = 5 },
                new Triangle { Base = 3, Height = 8 }
            };

            foreach (Shape s in shapes)
            {
                s.Describe();       // ★ Describe 是父类实现的，但它会调用子类的 GetArea
            }
            Console.WriteLine("   ★ Shape 不知道面积怎么算，但它知道【子类一定实现了 GetArea】");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 5：★ virtual+override vs new（多态是否生效）----");

            var d = new DerivedClass();

            Console.WriteLine("   情况 1：用子类变量调用 —— 都是子类版本");
            d.VirtualMethod();
            d.NormalMethod();
            Console.WriteLine();

            Console.WriteLine("   情况 2：用【父类变量】装子类对象 —— 区别出来了！");
            BaseClass b = new DerivedClass();
            b.VirtualMethod();      // ★ override → 调用子类版本
            b.NormalMethod();       // ★ new 隐藏 → 调用【父类】版本！
            Console.WriteLine();
            Console.WriteLine("   ★ 结论：只有 override 才能享受多态。new 只是隐藏，多态不生效。");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 6：类型判断 is / as ----");

            Animal mystery = new Dog("神秘狗", "未知");

            // 方式 1：is 判断 + 转换（推荐）
            if (mystery is Dog dogCheck)
            {
                Console.WriteLine($"   用 is 判断出来是 Dog，可以调用 Bark()：");
                dogCheck.Bark();
            }

            // 方式 2：as 安全转换
            var asCat = mystery as Cat;
            Console.WriteLine($"   用 as 转成 Cat 的结果：{(asCat is null ? "null（它不是猫）" : "成功")}");

            // 方式 3：强制转换失败会抛异常（这里不演示，因为会崩）
            Console.WriteLine("   强制转换 (Cat)mystery 会抛 InvalidCastException —— 所以优先用 is / as");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson07Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
