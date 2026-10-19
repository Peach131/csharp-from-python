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
    static class StringHelper
    {
        public static bool IsNullOrEmpty(string? s)
        {
            if (s == null || s.Length == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string Reverse(string s)
            {
                string result = "";
                 for (int i = s.Length - 1; i >= 0; i--)   // ★ 从后往前
                 {
                    result += s[i];
                 }
                 return result;
            }

        public static int CountChar(string s, char c)
        {
            var count = 0;
            foreach (var a in s)
            {
                if (a == c){ count++;}
            }
            return count;
        }
    }

    class Ticket
    {
        public string SeatNumber { get; set; } = "";
        public static int TotalSold { get; private set; }

        public Ticket(string seatNumber)
        {
            SeatNumber = seatNumber;
            TotalSold += 1;
        }
    }
    
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

    class Temperature
    {
        private double _celsius;
        public double Celsius {get { return _celsius; }}
        public double Fahrenheit => _celsius * 9 / 5 + 32;
        public Temperature(double celsius)
        {
            if (celsius < -273.15)
            {
                _celsius = -273.15;
                Console.WriteLine($"绝对零度，设为 -273.15");
            }
            else
            {
                _celsius = celsius;
            }
            
        }
        public bool SetCelsius(double value)
        {
            if (value < -273.15) return false;      // 拒绝
            _celsius = value;
            return true;
        }
    }
    

    // ────────────────────────────────────────────────────────────────
    // TODO 4 ｜ const 和 static readonly 的区别
    // ────────────────────────────────────────────────────────────────
    class AppLimits
    {
        // ★ const：编译期常量。
        //   编译器会把【所有用到它的地方】直接替换成 5（写进 IL 里，叫"内联"），
        //   运行时根本不存在这个变量 —— 所以它必须是编译器当场算得出来的值。
        public const int MaxLoginAttempts = 5;
        public const string DefaultLanguage = "zh-CN";

        // ★ static readonly：运行期只读。
        //   DateTime.Now 必须等程序跑起来才知道，编译器算不出来，
        //   所以【只能】用 static readonly。若改成 const 会编译错误：
        //     CS0133: 赋给 const 的表达式必须是常量
        public static readonly DateTime StartedAt = DateTime.Now;

        // ★ static readonly 也能指向引用类型（数组）。
        //   但要注意 readonly 的局限：它锁的是"这个字段不能再指向别的数组"，
        //   【不锁数组里面的元素】—— 下面这行仍然合法、能编译：
        //       AppLimits.SupportedYears[0] = 2030;   // ⚠️ 能改！
        //   想连元素都锁死，要用 ImmutableArray（后续课程会讲）。
        public static readonly int[] SupportedYears = { 2024, 2025, 2026 };

        // ★ 一张表记住区别：
        //   const            → 编译期定死、隐式就是 static（不用写）、
        //                      只能用于基元类型和 string
        //   static readonly  → 运行期定死、必须显式写 static、
        //                      可以是任何类型（包括 new 出来的对象）
    }


    // ────────────────────────────────────────────────────────────────
    // TODO 5 ｜ ★ 综合：单例模式（本课考核）
    // ────────────────────────────────────────────────────────────────
    // 【需求】整个程序里"应用配置"只能有一份。若存在两个配置对象，
    //        改了这个那个没变 —— 这种 bug 极难排查，要从设计上杜绝。
    //
    // 【单例三件套】下面就是标准实现，请对照记忆：
    //     ① private static 字段 → 存那唯一的一份（static 保证全类共享）
    //     ② private 构造函数    → 堵死"外面自己 new"这条路
    //     ③ public static 方法  → 唯一合法入口，第一次调用时才创建
    class AppConfig
    {
        // ① 静态字段：存唯一实例
        // ★ 为什么类型写 AppConfig?（带问号）？
        //   因为它一开始是 null（还没创建）。项目开了 Nullable，
        //   写成不带 ? 的 AppConfig 会报 CS8618 警告。加 ? 就是告诉编译器：
        //   "我知道它可能为 null，我会在下面检查。"
        private static AppConfig? _instance;

        // ② 私有构造函数
        // ★★ 单例最关键的一步：构造函数是 private，
        //    所以外面写 new AppConfig() 会【编译错误】CS0122
        //    （"AppConfig.AppConfig()"不可访问，因为它具有一定的保护级别）。
        //    → 从语言层面堵死"造第二个"的可能，而不是靠人自觉。
        //    ★ Python 做不到这一点（没有真正的私有），只能靠约定或运行时抛异常模拟。
        private AppConfig()
        {
            // ★ 这行打印是【验证单例的探针】：
            //   单例写对了的话，无论 GetInstance() 调用多少次，
            //   这行都【只应该打印一次】。打印两次 = 单例失败了。
            Console.WriteLine("AppConfig 实例被创建");
        }

        // ③ 静态方法：唯一的获取入口
        public static AppConfig GetInstance()
        {
            // ★ 懒加载（Lazy）：第一次调用时才创建，之后直接返回已有那份。
            //   为什么不在静态字段处直接写 = new AppConfig()？
            //   那样也能跑（叫"饿汉式"），但对象会在程序一启动就创建，
            //   哪怕你根本没用它。懒加载把这份开销推迟到真正需要时。
            if (_instance is null)
            {
                _instance = new AppConfig();     // ← 只有第一次会执行到这里
            }

            return _instance;                    // 之后每次都返回同一份
        }

        // ④ 配置数据（普通实例属性，每个单例对象内部有自己的值）
        public string AppName { get; set; } = "我的应用";
        public int MaxRetry { get; set; } = 3;

        // ⑤ 普通实例方法：打印当前配置
        public void Show()
        {
            Console.WriteLine("===== 当前配置 =====");
            Console.WriteLine($"应用名：{AppName}");
            Console.WriteLine($"最大重试次数：{MaxRetry}");
        }
    }


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
            // ★ 先记一条铁律：调用静态成员用【类名】，不是对象名。
            //   StringHelper.IsNullOrEmpty(...)   ✅
            //   如果你写了 new StringHelper()，编译器直接报错 ——
            //   因为 static class 【不允许】被实例化。

            // ★★ 语法坑：插值字符串里想显示一个【双引号】，
            //    必须用 \" 转义。直接写 "" 会把插值字符串提前结束掉，编译错误。
            //    （这就是你原稿第 249 行报错的原因：
            //      $"...IsNullOrEmpty("")..."  →  字符串在第一个 " 就断了）
            Console.WriteLine($"IsNullOrEmpty(null)      = {StringHelper.IsNullOrEmpty(null)}");
            Console.WriteLine($"IsNullOrEmpty(\"\")        = {StringHelper.IsNullOrEmpty("")}");
            Console.WriteLine($"IsNullOrEmpty(\"abc\")     = {StringHelper.IsNullOrEmpty("abc")}");
            Console.WriteLine($"Reverse(\"hello\")         = {StringHelper.Reverse("hello")}");
            Console.WriteLine($"CountChar(\"banana\", 'a') = {StringHelper.CountChar("banana", 'a')}");

            // ★ 另一条更容易写的路子：想显示引号，用【逐字字符串】@"..." 会更省心，
            //   或者干脆把 "" 换成 string.Empty：
            //     Console.WriteLine($"IsNullOrEmpty(string.Empty) = {StringHelper.IsNullOrEmpty(string.Empty)}");

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
            var Ticketa = new Ticket("A1");
            var Ticketb = new Ticket("A2");
            var Ticketc = new Ticket("A3");
            var Ticketd = new Ticket("A4");
            Console.WriteLine($"卖出 4 张票后，总售出：{Ticket.TotalSold}");
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
            var Tm = new Temperature(25);

            // ★ {Tm.Celsius} 和 {Tm.Fahrenheit:F2} 的区别：
            //   Celsius 是 25（没有小数位，double 打印整数时不显示 .00）
            //   Fahrenheit 要保留 2 位 → 必须加 :F2，否则 77 而不是 77.00
            //   ★★ 关键：Fahrenheit 是【计算属性】，读它的那一刻才现算：
            //        25 * 9 / 5 + 32 = 45 + 32 = 77
            Console.WriteLine($"初始温度：{Tm.Celsius}°C = {Tm.Fahrenheit:F2}°F");

            // ★ SetCelsius 返回的是 bool，直接插值打印出来就是 True / False
            //   这就是第 04 课"方法只能返回一个值，让调用方去判断"的用法
            Console.WriteLine($"设置 30 → {(Tm.SetCelsius(30) ? "成功" : "失败")}");

            // ★ 注意这里输出的是 30 而不是 25 —— 证明 SetCelsius 真的改动了对象状态
            //   （对象是【引用类型】，方法里改 _celsius，外面看到的 Tm 也跟着变）
            Console.WriteLine($"当前温度：{Tm.Celsius}°C = {Tm.Fahrenheit:F2}°F");

            Console.WriteLine($"设置 -300 → {(Tm.SetCelsius(-300) ? "成功" : "失败（温度不能低于绝对零度）")}");

            // ★★ 这一行是 TODO 3 的【真正考点】：验证"非法输入被拒绝后，状态没被破坏"。
            //   输出仍然是 30 —— 说明 SetCelsius 里的 return false 提前退出，
            //   后面那行 _celsius = value 没有执行。
            //   ★ 如果少了这行验证，你无法确认"拒绝"是真的拒绝了。
            Console.WriteLine($"当前温度仍然是：{Tm.Celsius}°C = {Tm.Fahrenheit:F2}°F");

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
            // ★ 全部用【类名】访问，不需要（也不能）new AppLimits()
            Console.WriteLine($"最大登录尝试次数：{AppLimits.MaxLoginAttempts}");
            Console.WriteLine($"默认语言：{AppLimits.DefaultLanguage}");

            // ★ StartedAt 是 static readonly DateTime，一个对象
            //   想只显示时分秒 → 用格式化字符串 "HH:mm:ss"
            //     HH = 24 小时制两位小时（hh 是 12 小时制，别搞混）
            //     mm = 分钟（MM 是"月份"！大小写在这里是会出错的）
            //     ss = 秒
            Console.WriteLine($"程序启动时间：{AppLimits.StartedAt:HH:mm:ss}");

            // ★ SupportedYears 是 int[]，直接插值会得到 "System.Int32[]"
            //   要用 string.Join 把元素用分隔符连起来
            Console.WriteLine($"支持的年份：{string.Join(", ", AppLimits.SupportedYears)}");

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
            // ★ 注意：这里【没有】new AppConfig()，因为构造函数是 private。
            //   想试的话可以在这行写 new AppConfig()，编译器会立刻报
            //   CS0122："AppConfig.AppConfig()"不可访问，因为它具有一定的保护级别。
            //   ★ 这就是单例的"硬约束"—— 靠编译器保证，不靠自觉。

            AppConfig c1 = AppConfig.GetInstance();   // 第 1 次调用 → _instance 是 null → 创建
            AppConfig c2 = AppConfig.GetInstance();   // 第 2 次调用 → _instance 已有 → 直接返回

            // ★ 验证是同一个对象。
            //   AppConfig 是普通 class，class 的 == 默认按【地址/身份】比较。
            //   返回 True 就证明 c1 和 c2 指向堆上【同一个对象】。
            //   ★ 反过来说：如果单例写错了（比如每次 new 一个新的），这里会是 False。
            Console.WriteLine($"两次拿到的是同一个对象吗？{c1 == c2}");

            // ★★ 单例的意义就在下面两行：
            //   通过 c1 改，用 c2 读 —— 能读到改动，因为它们本来就是同一份数据。
            //   如果存在两个配置对象，这里就会读到旧的 "我的应用"，
            //   → 出现"我明明改了配置，程序却还用旧值"这种极难排查的 bug。
            c1.AppName = "修改后的名字";
            Console.WriteLine($"通过 c2 看到 AppName：{c2.AppName}");

            // 用任意一个引用都能打印（因为都一样）
            c2.Show();

            // ★ 最后回头确认一下：整个输出里 "AppConfig 实例被创建" 只出现了【一次】。
            //   出现两次 = 单例失败（说明 GetInstance 里少了 if 判断）。
            Console.WriteLine();

            // ★★ 补充思考题（不影响运行，想清楚就懂单例了）：
            //   Q: 如果 GetInstance 里不写 if (_instance is null) 判断，
            //      直接写 return new AppConfig(); 会怎样？
            //   A: 每次都造新对象 → c1 == c2 变成 False → 配置改动互相看不见
            //      → 单例彻底失效，而且"AppConfig 实例被创建"会打印两次。
            //
            //   Q: 那为什么不干脆全用 static 属性（不搞单例）？
            //   A: static 属性确实"只有一份"，但它无法被当【对象】传递
            //      （不能当参数传给方法、不能实现接口、不能延迟初始化）。
            //      单例 = "只有一个实例的对象"，本质还是对象，只是数量被限死。


            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
