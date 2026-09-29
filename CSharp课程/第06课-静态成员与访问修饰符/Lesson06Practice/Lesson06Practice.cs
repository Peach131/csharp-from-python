// ============================================================================
//  第 06 课 · 练习：静态成员与访问修饰符
//
//  ★ 本课重点：
//     1. 静态成员属于【类】，用【类名】访问
//     2. 静态方法【不能】访问实例字段
//     3. private 是编译器强制的（不像 Python 只是约定）
// ============================================================================

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ 本课的类定义写在这里 ↓↓↓
    // ================================================================




    class Lesson06Practice
    {
        static void Main()
        {
            Console.WriteLine("===== 第 06 课练习：静态成员与访问修饰符 =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 静态工具方法
            // ================================================================
            // 【要求】在上面定义一个 static class StringHelper，包含：
            //     ① static bool IsNullOrEmpty(string? s)
            //          如果 s 是 null 或者空字符串，返回 true，否则 false
            //     ② static string Reverse(string s)
            //          把字符串倒过来（提示：可以用循环从后往前拼）
            //     ③ static int CountChar(string s, char c)
            //          统计字符 c 在 s 里出现了几次
            //
            //   然后在下面调用并打印
            //
            // 【预期输出】
            //     IsNullOrEmpty(null)      = True
            //     IsNullOrEmpty("")        = True
            //     IsNullOrEmpty("abc")     = False
            //     Reverse("hello")         = olleh
            //     CountChar("banana", 'a') = 3
            //
            // 【语法提示】
            //     static class StringHelper
            //     {
            //         public static bool IsNullOrEmpty(string? s)
            //         {
            //             return s is null || s.Length == 0;
            //         }
            //
            //         public static string Reverse(string s)
            //         {
            //             string result = "";
            //             for (int i = s.Length - 1; i >= 0; i--)   // ★ 从后往前
            //             {
            //                 result += s[i];
            //             }
            //             return result;
            //         }
            //     }
            //
            //   调用：StringHelper.IsNullOrEmpty(null)      ← 用【类名】，不是对象名
            //
            // 【考察点】
            //   · 静态类不能 new，也不能有实例成员
            //   · `string?` 里的 `?` 表示"这个参数可能是 null"（下一批课程会详细讲）

            Console.WriteLine("--- TODO 1 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ 用静态字段做计数器
            // ================================================================
            // 【要求】定义类 Ticket（门票）：
            //     - string SeatNumber { get; set; }       座位号
            //     - static int TotalSold { get; private set; }   ★ 已售总数
            //     - 构造函数 Ticket(string seatNumber)
            //         里面让 TotalSold 加 1
            //
            //   然后创建 4 张票，打印总数
            //
            // 【预期输出】
            //     卖出 4 张票后，总售出：4
            //
            // 【语法提示】
            //     class Ticket
            //     {
            //         public string SeatNumber { get; set; } = "";
            //         public static int TotalSold { get; private set; }
            //
            //         public Ticket(string seatNumber)
            //         {
            //             SeatNumber = seatNumber;
            //             TotalSold++;                  // 静态属性，直接改
            //         }
            //     }
            //
            //   ★ `{ get; private set; }` 表示：谁都能读，但只有本类能改。
            //     这是很常见的写法，比 public set 更安全。
            //
            // 【考察点】
            //   · 静态属性在所有对象之间共享
            //   · 构造函数里改静态属性 = 每次创建对象都在累加

            Console.WriteLine("--- TODO 2 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ 用访问修饰符保护数据
            // ================================================================
            // 【要求】定义类 Temperature（温度计）：
            //     - private double _celsius;                     私有字段
            //     - public double Celsius { get; }               只读属性
            //     - public double Fahrenheit => ...;             计算属性：°C × 9/5 + 32
            //     - 构造函数 Temperature(double celsius)
            //         如果传入的值 < -273.15（绝对零度），就设为 -273.15 并提示
            //     - public bool SetCelsius(double value)         修改温度的方法
            //         合法返回 true，非法返回 false（不修改）
            //
            //   然后测试：创建一个合法温度，再试图设一个非法温度
            //
            // 【预期输出】
            //     初始温度：25°C = 77.00°F
            //     设置 30 → 成功
            //     当前温度：30°C = 86.00°F
            //     设置 -300 → 失败（温度不能低于绝对零度）
            //     当前温度仍然是：30°C = 86.00°F
            //
            // 【提示】
            //     class Temperature
            //     {
            //         private double _celsius;
            //
            //         public double Celsius => _celsius;          // 只读属性（表达式体）
            //         public double Fahrenheit => _celsius * 9 / 5 + 32;
            //
            //         public Temperature(double celsius)
            //         {
            //             if (celsius < -273.15)
            //             {
            //                 Console.WriteLine("警告：低于绝对零度，已修正为 -273.15");
            //                 _celsius = -273.15;
            //             }
            //             else
            //             {
            //                 _celsius = celsius;
            //             }
            //         }
            //
            //         public bool SetCelsius(double value)
            //         {
            //             if (value < -273.15) return false;      // 拒绝
            //             _celsius = value;
            //             return true;
            //         }
            //     }
            //
            // 【考察点】这就是"私有字段 + 受控修改"的标准模式。
            //          注意 Fahrenheit 是【计算属性】，没有存储字段，每次读都重新算。

            Console.WriteLine("--- TODO 3 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ const 和 static readonly
            // ================================================================
            // 【要求】定义类 AppLimits：
            //     - public const int MaxLoginAttempts = 5;                 编译期常量
            //     - public const string DefaultLanguage = "zh-CN";
            //     - public static readonly DateTime StartedAt = DateTime.Now;  运行期只读
            //     - public static readonly int[] SupportedYears = { 2024, 2025, 2026 };
            //
            //   然后打印它们
            //
            // 【预期输出】（时间会不同）
            //     最大登录尝试次数：5
            //     默认语言：zh-CN
            //     程序启动时间：HH:mm:ss
            //     支持的年份：2024, 2025, 2026
            //
            // 【提示】
            //     class AppLimits
            //     {
            //         public const int MaxLoginAttempts = 5;
            //         public const string DefaultLanguage = "zh-CN";
            //         public static readonly DateTime StartedAt = DateTime.Now;
            //         public static readonly int[] SupportedYears = { 2024, 2025, 2026 };
            //     }
            //
            //     Console.WriteLine($"支持的年份：{string.Join(", ", AppLimits.SupportedYears)}");
            //
            // 【考察点】
            //   · const 不能是 DateTime.Now（编译期算不出来）
            //   · static readonly 可以是 DateTime.Now，也可以是数组
            //   · 试着把 const 改成 static readonly，看看哪些能编译、哪些不能

            Console.WriteLine("--- TODO 4 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ 综合：单例模式（本课考核）
            // ================================================================
            // 【背景】需求：整个程序里"应用配置"只能有一份，
            //        不能出现两个配置对象（否则改了这个那个没变，很难查）。
            //
            // 【要求】定义类 AppConfig，实现单例：
            //     ① private static AppConfig? _instance;          静态字段存唯一实例
            //     ② private AppConfig() { }                       私有构造函数
            //          里面打印 "AppConfig 实例被创建"
            //     ③ public static AppConfig GetInstance()         静态方法给口子
            //          第一次调用时创建，之后都返回同一个
            //     ④ 两个属性（给默认值）：
            //          string AppName { get; set; } = "我的应用";
            //          int MaxRetry  { get; set; } = 3;
            //     ⑤ public void Show()   打印当前配置
            //
            //   然后：
            //     1. 调两次 GetInstance()，验证是同一个对象（用 == 比较）
            //     2. 通过第一个引用改 AppName
            //     3. 用第二个引用读 AppName，验证改动能看到
            //
            // 【预期输出】
            //     AppConfig 实例被创建
            //     两次拿到的是同一个对象吗？True
            //     通过 c2 看到 AppName：修改后的名字
            //     ===== 当前配置 =====
            //     应用名：修改后的名字
            //     最大重试次数：3
            //
            // 【提示】
            //     class AppConfig
            //     {
            //         private static AppConfig? _instance;
            //
            //         private AppConfig()
            //         {
            //             Console.WriteLine("AppConfig 实例被创建");
            //         }
            //
            //         public static AppConfig GetInstance()
            //         {
            //             if (_instance is null)
            //             {
            //                 _instance = new AppConfig();
            //             }
            //             return _instance;
            //         }
            //
            //         public string AppName { get; set; } = "我的应用";
            //         public int MaxRetry { get; set; } = 3;
            //
            //         public void Show()
            //         {
            //             Console.WriteLine("===== 当前配置 =====");
            //             Console.WriteLine($"应用名：{AppName}");
            //             Console.WriteLine($"最大重试次数：{MaxRetry}");
            //         }
            //     }
            //
            //   测试代码：
            //     var c1 = AppConfig.GetInstance();
            //     var c2 = AppConfig.GetInstance();
            //     Console.WriteLine($"两次拿到的是同一个对象吗？{c1 == c2}");
            //     c1.AppName = "修改后的名字";
            //     Console.WriteLine($"通过 c2 看到 AppName：{c2.AppName}");
            //     c2.Show();
            //
            // 【考察点】
            //   · 私有构造函数 → 外面 new 不了（试着写一下，看编译错误）
            //   · 静态字段 → 全局只有一份
            //   · "AppConfig 实例被创建" 只应该打印【一次】—— 这是验证单例的关键

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
