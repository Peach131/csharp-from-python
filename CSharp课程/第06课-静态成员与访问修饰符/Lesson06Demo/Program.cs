// ============================================================================
//  第 06 课 · 示例代码
//  静态成员与访问修饰符
//  ★ 本课要搞懂你从第 00 课就一直在写的 static 到底是什么
// ============================================================================

namespace Lesson06Demo
{
    // ========================================================================
    // 一、实例成员 vs 静态成员
    // ========================================================================
    class Counter
    {
        public int InstanceCount = 0;              // 实例字段：每个对象各一份
        public static int TotalCount = 0;          // 静态字段：所有对象共享一份

        public void AddInstance()                  // 实例方法
        {
            InstanceCount++;
            TotalCount++;                          // 实例方法可以访问静态字段
        }

        public static void AddStatic()             // 静态方法
        {
            TotalCount++;
            // InstanceCount++;                    // ❌ 编译错误：静态方法不能访问实例字段
        }
    }


    // ========================================================================
    // 二、用静态字段做"创建计数器"
    // ========================================================================
    class Student
    {
        public string Name { get; set; }
        public static int TotalCreated = 0;        // 全局共享的计数

        public Student(string name)
        {
            Name = name;
            TotalCreated++;                        // 每次创建对象就 +1
        }
    }


    // ========================================================================
    // 三、静态类：全是静态成员，不能 new
    // ========================================================================
    static class MathHelper
    {
        public static int Square(int x) => x * x;
        public static bool IsEven(int n) => n % 2 == 0;
        public static double ToFahrenheit(double celsius) => celsius * 9 / 5 + 32;

        // ★ 静态类里不能有实例成员
        // public int Value = 0;                   // ❌ 编译错误
    }


    // ========================================================================
    // 四、静态构造函数：只执行一次
    // ========================================================================
    class Config
    {
        public static string AppName;
        public static DateTime LoadedAt;

        static Config()                            // 静态构造函数：无参数、无访问修饰符
        {
            Console.WriteLine("      >>> Config 的静态构造函数执行了（整个程序只会看到一次）");
            AppName = "我的应用";
            LoadedAt = DateTime.Now;
        }

        public static string Show() => $"{AppName}（加载于 {LoadedAt:HH:mm:ss}）";
    }


    // ========================================================================
    // 五、访问修饰符演示
    // ========================================================================
    class BankAccount
    {
        private decimal _balance;                  // 只有本类能访问
        internal string BankCode = "ICBC";         // 同项目能访问
        public string AccountNumber { get; set; } = "";   // 所有人能访问

        public decimal Balance => _balance;        // 只读属性，对外暴露余额

        // 只能通过这个方法改余额 —— 有校验，可控
        public bool Deposit(decimal amount)
        {
            if (amount <= 0)
            {
                return false;                      // 拒绝非法金额
            }
            _balance += amount;
            return true;
        }

        public bool Withdraw(decimal amount)
        {
            if (amount <= 0 || amount > _balance)
            {
                return false;                      // 余额不够也拒绝
            }
            _balance -= amount;
            return true;
        }
    }


    // ========================================================================
    // 六、const vs static readonly
    // ========================================================================
    class Limits
    {
        public const int MaxRetry = 3;                              // 编译期常量
        public static readonly DateTime CreatedAt = DateTime.Now;    // 运行期才确定
        public const double Pi = 3.14159;
    }


    // ========================================================================
    // 七、单例模式：全局只有一个对象
    // ========================================================================
    class AppSettings
    {
        private static AppSettings? _instance;     // ① 静态字段存唯一的实例

        private AppSettings()                      // ② 私有构造函数：外面不能 new
        {
            Console.WriteLine("      >>> AppSettings 被创建（只会看到一次）");
        }

        public static AppSettings GetInstance()    // ③ 静态方法提供访问
        {
            if (_instance is null)
            {
                _instance = new AppSettings();
            }
            return _instance;
        }

        public string Theme { get; set; } = "深色";
    }


    class Program
    {
        static void Main()
        {
            Console.WriteLine("========== 第 06 课：静态成员与访问修饰符 ==========");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 1：实例字段 vs 静态字段 ----");

            var c1 = new Counter();
            var c2 = new Counter();

            c1.AddInstance();
            c1.AddInstance();
            c2.AddInstance();

            Console.WriteLine($"   c1.InstanceCount = {c1.InstanceCount}   ← 每个对象各一份");
            Console.WriteLine($"   c2.InstanceCount = {c2.InstanceCount}");
            Console.WriteLine($"   Counter.TotalCount = {Counter.TotalCount}  ← 全局共享一份，用【类名】访问");
            Console.WriteLine();

            Counter.AddStatic();                       // 静态方法也用类名调用
            Console.WriteLine($"   调用静态方法 Counter.AddStatic() 后：");
            Console.WriteLine($"   Counter.TotalCount = {Counter.TotalCount}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 2：静态字段做计数器 ----");

            Console.WriteLine($"   创建前：TotalCreated = {Student.TotalCreated}");
            new Student("张三");
            new Student("李四");
            new Student("王五");
            Console.WriteLine($"   创建 3 个学生后：TotalCreated = {Student.TotalCreated}");
            Console.WriteLine("   ★ 实例字段做不到这件事（每个对象各算各的）");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 3：静态类（工具方法集合）----");

            Console.WriteLine($"   MathHelper.Square(5)            = {MathHelper.Square(5)}");
            Console.WriteLine($"   MathHelper.IsEven(7)            = {MathHelper.IsEven(7)}");
            Console.WriteLine($"   MathHelper.ToFahrenheit(100)    = {MathHelper.ToFahrenheit(100)}");
            // var h = new MathHelper();       // ❌ 编译错误：静态类不能创建对象
            Console.WriteLine("   ★ 静态类不能 new，里面所有成员都必须是 static");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 4：静态构造函数只执行一次 ----");

            Console.WriteLine("   准备第一次访问 Config...");
            Console.WriteLine($"   结果：{Config.Show()}");
            Console.WriteLine("   准备第二次访问 Config...");
            Console.WriteLine($"   结果：{Config.Show()}   ← 静态构造函数没有再执行");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 5：★ 访问修饰符保护数据 ----");

            var account = new BankAccount { AccountNumber = "6222-0000-1111" };
            Console.WriteLine($"   账号：{account.AccountNumber}");
            Console.WriteLine($"   初始余额：{account.Balance}");

            // account._balance = 999999;      // ❌ 编译错误：_balance 是 private
            // account.Balance = 999999;       // ❌ 编译错误：Balance 只有 get

            Console.Write("   存入 1000 → ");
            Console.WriteLine(account.Deposit(1000) ? $"成功，余额 {account.Balance}" : "失败");

            Console.Write("   存入 -500 → ");
            Console.WriteLine(account.Deposit(-500) ? "成功" : $"被拒绝，余额仍是 {account.Balance}");

            Console.Write("   取出 5000 → ");
            Console.WriteLine(account.Withdraw(5000) ? "成功" : $"被拒绝（余额不足），余额仍是 {account.Balance}");

            Console.WriteLine("   ★ private 字段 + 公开方法 = 数据只能通过受控的方式修改");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 6：const vs static readonly ----");

            Console.WriteLine($"   Limits.MaxRetry      = {Limits.MaxRetry}        ← const，编译期就有值");
            Console.WriteLine($"   Limits.CreatedAt     = {Limits.CreatedAt:HH:mm:ss}   ← static readonly，运行期算出来");
            Console.WriteLine();
            Console.WriteLine("   const 只能是【编译期就能算出来】的值（数字、字符串、bool）");
            Console.WriteLine("   想放 DateTime.Now 就只能用 static readonly");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 7：单例模式 ----");

            var s1 = AppSettings.GetInstance();
            var s2 = AppSettings.GetInstance();

            s1.Theme = "浅色";
            Console.WriteLine($"   s1 == s2 ? {s1 == s2}   ← 两次拿到的是同一个对象");
            Console.WriteLine($"   改 s1.Theme 之后，s2.Theme = {s2.Theme}   ← 因为是同一个对象");
            // var bad = new AppSettings();    // ❌ 编译错误：构造函数是 private
            Console.WriteLine("   ★ 三个关键：私有构造函数 + 静态字段存实例 + 静态方法给口子");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson06Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
