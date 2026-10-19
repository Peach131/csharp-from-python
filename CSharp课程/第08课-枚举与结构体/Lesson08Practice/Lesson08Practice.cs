// ============================================================================
//  第 08 课 · 练习：枚举与结构体
//
//  ★ 本课重点：
//     1. 枚举替代魔法数字
//     2. [Flags] 位标志（值必须是 2 的幂）
//     3. struct 是值类型；判断该用 struct 还是 class 的四条标准
// ============================================================================

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ TODO 1 ｜ 定义枚举
    // ================================================================
    // 在这里定义 enum TrafficLight { Red, Yellow, Green }
    // 和 enum Weekday { Monday, ..., Sunday }（7 天）

    // ★ enum 必须写在 namespace 里、别的 class 的【外面】——
    //   它和 class 一样是"类型声明"，不能塞进方法体里。
    // ★ 枚举的成员名是"给人看的名字"，底层存的其实是整数（这里就是 0、1、2）。
    enum TrafficLight
    {
        // 一个成员都不写值 = 0，编译器自动让后面的成员依次 +1
        Red,        // = 0
        Yellow,     // = 1
        Green       // = 2
    }

    // ★ 命名规范：枚举成员用【大驼峰】（Monday、Red），和类名一样的风格。
    // ★ 注意 TrafficLight.Red 是"编号 0"，不是字符串 "Red"；
    //   真要字符串就写 TrafficLight.Red.ToString()（枚举自带这个方法）。
    enum Weekday
    {
        Monday,     // = 0
        Tuesday,    // = 1
        Wednesday,  // = 2
        Thursday,   // = 3
        Friday,     // = 4
        Saturday,   // = 5
        Sunday      // = 6
    }


    // ================================================================
    // ↓↓↓ TODO 2 ｜ 定义 [Flags] 枚举
    // ================================================================
    // 在这里定义 [Flags] enum Permissions
    //     None=0, Read=1, Write=2, Delete=4, All=Read|Write|Delete

    // ★ [Flags] 是写在声明上面的"特性"（方括号）。它不改变任何数值，
    //   只是告诉 ToString()："请把数值拆成一个个二进制位，再拼成 Read, Write 这样的名字"。
    //   不加它的话，p 会直接打印成数字 3，可读性就没了。
    [Flags]
    enum Permissions
    {
        // ★ 值必须是 2 的幂（1、2、4、8…），也就是二进制里【只有一个 1】：
        //   1=001、2=010、4=100，每个成员各占一位，位运算才不会互相干扰。
        //   如果偷懒写成 1、2、3，那 3=011 已经占了 Read+Write 两位，组合时就串味了。
        None = 0,                             // 000  表示"什么权限都没有"
        Read = 1,                             // 001
        Write = 2,                            // 010
        Delete = 4,                           // 100
        All = Read | Write | Delete           // 111  用 | 把几个成员"按位或"起来
    }




    // ================================================================
    // ↓↓↓ TODO 3、4 ｜ 定义 struct 和 record struct
    // ================================================================



    // ★ 手写版 struct：字段 + 构造函数 + 方法 + ToString，一样都不少。
    //   这里【故意】把字段写成 public，因为 TODO 3 就是演示"值类型的赋值行为"——
    //   得能从外面改 s2.Height，才能看出 s1 没被影响。
    //   （正课里说过：真正写业务代码请用 readonly struct / record struct，见下面 TODO 4）
    struct Size
    {
        public int Width;
        public int Height;

        // ★ struct 的构造函数写法和 class 一样（名字=类型名，不写返回类型），
        //   但多一条硬性规定：构造函数里【必须给所有字段赋值】，漏一个都编译不过。
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int GetArea() => Width * Height;

        // ★ 一定要写 override：不写就是"隐藏"了 object.ToString()，
        //   打印出来只会是类型名 "LessonPractice.Size"，看不到里面的数据。
        public override string ToString() => $"({Width}×{Height})";
    }

    // ★ readonly record struct：编译器自动生成"位置构造函数 + 属性 + Equals
    //   + GetHashCode + ToString + == 运算符"，我们只补真正需要自己算的那点东西。
    //   同样的功能手写要 13 行以上（见上面的 Size），这里一行声明就够了。
    // ★ 加 readonly 表示不可变：所有属性只能是 get，创建完谁也改不了 ——
    //   正好绕开"可变 struct 到处是副本"那个经典坑。
    readonly record struct Temperature(double Celsius)
    {
        // ★ 计算属性：Fahrenheit 不占字段，每次读都现算，所以永远不会和数据不一致。
        //   运算顺序是 (Celsius * 9) / 5 + 32：因为 Celsius 是 double，
        //   一乘就变成 double 了，后面的除法才是浮点除（不会像整数 9/5 那样丢成 1）。
        public double Fahrenheit => Celsius * 9 / 5 + 32;

        // ★ 重写 ToString，覆盖掉编译器自动生成的那个（自动版会带上类型名）。
        //   {Celsius:F1} 里的 F1 = 定点格式保留 1 位小数，25 会显示成 "25.0"。
        public override string ToString() => $"{Celsius:F1}°C / {Fahrenheit:F1}°F";
    }

    // ★ TODO 5 用：订单的四个状态 + 已取消，一共五个成员（默认值 0~4）
    enum OrderState
    {
        PendingPayment,     // = 0  待付款
        Paid,               // = 1  已付款
        Shipped,            // = 2  已发货
        Completed,          // = 3  已完成
        Cancelled           // = 4  已取消
    }


    class Lesson08Practice
    {
        // ================================================================
        // ↓↓↓ TODO 1 用的方法写在这里 ↓↓↓
        // ================================================================




        // ★ switch 表达式：写法是 `值 switch { 模式 => 结果, ... }`，
        //   把"判断 + 返回"压缩成了"一张对照表"，比 if / else if 更适合枚举。
        // ★ 最后的 `_` 是"兜底模式"（默认分支）：枚举理论上不会出现别的值，
        //   但编译器要求 switch 表达式必须覆盖所有可能，写 `_` 才能通过编译。
        static string GetAction(TrafficLight light) => light switch
        {
            TrafficLight.Red => "停车",
            TrafficLight.Yellow => "减速慢行",
            TrafficLight.Green => "通行",
            _ => "未知"
        };

        // ★ Permissions.All 这个成员本身就是用 `|` 算出来的（1|2|4=7）。
        //   枚举成员之间可以直接参与位运算，不用先转成 int。
        static Permissions GetAllPermissions()
            => Permissions.Read | Permissions.Write | Permissions.Delete;

        // ★ 状态机的核心：一个 switch 表达式，描述"当前状态 → 下一个状态"。
        //   `_ => current` 表示"已完成的、已取消的，以及任何没列出的状态，都原地不动"，
        //   这样调用方只要比较 next == current 就知道"走到头了"。
        static OrderState GetNextState(OrderState current) => current switch
        {
            OrderState.PendingPayment => OrderState.Paid,
            OrderState.Paid => OrderState.Shipped,
            OrderState.Shipped => OrderState.Completed,
            _ => current
        };

        // ★ 枚举值 → 中文名。把"显示用的文字"集中在一处，
        //   以后要改文案或做多语言，只动这一个方法，不用满项目改字符串。
        static string GetDisplayName(OrderState state) => state switch
        {
            OrderState.PendingPayment => "待付款",
            OrderState.Paid => "已付款",
            OrderState.Shipped => "已发货",
            OrderState.Completed => "已完成",
            OrderState.Cancelled => "已取消",
            _ => "未知"
        };

        // ★ `is A or B` 是模式匹配，等价于 `s == A || s == B`，但是更短、更不容易写错括号。
        //   业务规则集中在这里：想改"哪些状态能取消"，只需要改这一行。
        static bool CanCancel(OrderState state)
            => state is OrderState.PendingPayment or OrderState.Paid;

        static void Main()
        {
            Console.WriteLine("===== 第 08 课练习：枚举与结构体 =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 枚举的基本使用
            // ================================================================
            // 【要求】
            //   1. 在上面定义 enum TrafficLight { Red, Yellow, Green }
            //   2. 打印每个枚举成员的名字和底层整数值
            //   3. 用 switch 表达式写一个方法 GetAction(TrafficLight light)：
            //         Red    → "停车"
            //         Yellow → "减速慢行"
            //         Green  → "通行"
            //   4. 遍历所有枚举值，打印对应的动作
            //
            // 【预期输出】
            //     Red    = 0
            //     Yellow = 1
            //     Green  = 2
            //     ----------
            //     Red    → 停车
            //     Yellow → 减速慢行
            //     Green  → 通行
            //
            // 【语法提示】
            //     enum TrafficLight { Red, Yellow, Green }
            //
            //     static string GetAction(TrafficLight light) => light switch
            //     {
            //         TrafficLight.Red => "停车",
            //         TrafficLight.Yellow => "减速慢行",
            //         TrafficLight.Green => "通行",
            //         _ => "未知"
            //     };
            //
            //     foreach (TrafficLight t in Enum.GetValues<TrafficLight>())
            //     {
            //         Console.WriteLine($"{t,-7}= {(int)t}");     // ,-7 表示左对齐占 7 格
            //     }
            //
            // 【考察点】
            //   · 枚举成员默认从 0 开始递增
            //   · switch 表达式处理枚举要写 `_` 兜底
            //   · Enum.GetValues<T>() 可以遍历所有值

            Console.WriteLine("--- TODO 1 ---");

            // ① 先按"名字 = 编号"的格式，把三个成员和它们的底层整数值打出来
            //   ★ {t,-7} 里的 -7 表示"左对齐、占 7 格"：名字不足 7 个字符时，
            //     右边自动补空格。这样 "Red" 和 "Yellow" 后面的 = 才能上下对齐。
            //   ★ (int)t 是【显式转换】：枚举的底层本来就是整数，但要转一下才能当数字用。
            //   ★ 枚举不是数组，不能写 TrafficLight[0]，
            //     要遍历所有成员就用 Enum.GetValues<T>() 这个静态方法。
            foreach (TrafficLight t in Enum.GetValues<TrafficLight>())
            {
                Console.WriteLine($"{t,-7}= {(int)t}");
            }

            Console.WriteLine("----------");

            // ② 再遍历一遍，这次用 switch 表达式把"灯"翻译成"该怎么做"
            //   ★ 同一个枚举可以被遍历多次（枚举是只读的常量集合），
            //     所以这里不需要把值存进数组，直接再枚举一次就行。
            foreach (TrafficLight t in Enum.GetValues<TrafficLight>())
            {
                Console.WriteLine($"{t,-7}→ {GetAction(t)}");
            }

            // ③ 顺手验证一下另一个普通枚举：Weekday 也是从 0 开始递增的
            //   ★ 一行里放多个语句用分号隔开是可以的，但一般不建议（这里只是演示"它就是 0~6"）。
            Console.WriteLine($"Weekday 共 {(int)Weekday.Sunday + 1} 天：{Weekday.Monday} ... {Weekday.Sunday}");




            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ [Flags] 位标志
            // ================================================================
            // 【要求】
            //   1. 在上面定义：
            //        [Flags]
            //        enum Permissions
            //        {
            //            None = 0,
            //            Read = 1,
            //            Write = 2,
            //            Delete = 4,
            //            All = Read | Write | Delete
            //        }
            //   2. 演示：
            //        · 组合权限：Read | Write
            //        · 检查权限：HasFlag
            //        · 添加权限：|=
            //        · 移除权限：&= ~
            //
            // 【预期输出】
            //     组合 Read | Write          = Read, Write
            //     包含 Read？                = True
            //     包含 Delete？              = False
            //     添加 Delete 后             = Read, Write, Delete
            //     移除 Write 后              = Read, Delete
            //     全部权限                   = All
            //
            // 【语法提示】
            //     Permissions p = Permissions.Read | Permissions.Write;
            //     Console.WriteLine($"组合 Read | Write          = {p}");
            //     Console.WriteLine($"包含 Read？                = {p.HasFlag(Permissions.Read)}");
            //
            //     p |= Permissions.Delete;              // 添加
            //     p &= ~Permissions.Write;              // 移除（~ 是取反）
            //
            // 【考察点】
            //   · 值必须是 2 的幂（1,2,4,8...），否则位运算会冲突
            //   · `|` 组合、`&=` 检查、`|=` 添加、`&= ~` 移除
            //   · [Flags] 让 ToString 输出 "Read, Write" 而不是数字

            Console.WriteLine("--- TODO 2 ---");

            // ① 用 `|` 组合两个权限：底层就是 001 | 010 = 011
            //   ★ 因为枚举上标了 [Flags]，打印时 ToString() 会自动把 011 拆回
            //     "Read, Write" 这样的名字列表，而不是显示数字 3。
            //   ★ 等下几行的等号要对齐：这里"标签 + 空格"在屏幕上占 26 列宽
            //     （中文按 2 列算），所以等号写在源码里时要自己数好空格。
            Permissions p = Permissions.Read | Permissions.Write;
            Console.WriteLine($"组合 Read | Write                = {p}");

            // ② HasFlag 检查"包含不包含某一位"：
            //   011 & 001 = 001（非 0）→ True；011 & 100 = 000 → False
            Console.WriteLine($"包含 Read？                = {p.HasFlag(Permissions.Read)}");
            Console.WriteLine($"包含 Delete？              = {p.HasFlag(Permissions.Delete)}");

            // ③ 添加权限用 `|=`：按位或会把 Delete 那一位点亮 → 011 | 100 = 111
            p |= Permissions.Delete;
            Console.WriteLine($"添加 Delete 后             = {p}");

            // ④ 移除权限用 `&= ~`：~ 是按位取反，~100 = 011（只把 Delete 那一位变 0），
            //   再和 p 做按位与，就把这一位"关掉"了 → 111 & 011 = 011，即 Read, Delete。
            //   ★ 千万不能写成 p &= Permissions.Write —— 那是"只留 Write"，会把别的权限全清掉。
            p &= ~Permissions.Write;
            Console.WriteLine($"移除 Write 后              = {p}");

            // ⑤ 看看 All 这个成员：它 = Read | Write | Delete = 7
            //   ★ All 的值正好等于"所有位都为 1"，所以 ToString 出来就是名字 All 本身。
            Console.WriteLine($"全部权限                   = {GetAllPermissions()}");




            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ struct：值类型的行为
            // ================================================================
            // 【要求】
            //   1. 在上面定义 struct Size：
            //        int Width; int Height;
            //        构造函数 Size(int width, int height)
            //        int GetArea()  → Width * Height
            //        override ToString() → $"({Width}×{Height})"
            //   2. 验证值类型行为：赋值后改一个，另一个不变
            //   3. 对照：同样内容的 class 会怎样
            //
            // 【预期输出】
            //     struct：s1 = (10×20), s2 = (10×99)   ← s1 没被影响
            //     s1 的面积 = 200
            //     ★ struct 赋值是复制数据，class 赋值是复制地址
            //
            // 【语法提示】
            //     struct Size
            //     {
            //         public int Width;
            //         public int Height;
            //
            //         public Size(int width, int height)
            //         {
            //             Width = width;
            //             Height = height;
            //         }
            //
            //         public int GetArea() => Width * Height;
            //         public override string ToString() => $"({Width}×{Height})";
            //     }
            //
            //     测试：
            //     Size s1 = new Size(10, 20);
            //     Size s2 = s1;
            //     s2.Height = 99;
            //     Console.WriteLine($"struct：s1 = {s1}, s2 = {s2}");
            //
            // 【考察点】
            //   · struct 的赋值是【复制全部数据】
            //   · struct 的字段是 public 时可以直接改（但更好的做法是 readonly struct）

            Console.WriteLine("--- TODO 3 ---");

            // ① 用【构造函数】创建一个 Size（圆括号，参数按位置对应 width、height）
            Size s1 = new Size(10, 20);

            // ② 把 s1 赋给 s2：★ 这是本课最关键的一行 ★
            //   struct 是【值类型】，所以这一步是把 10 和 20 两个数字【复制】一份给 s2。
            //   从此 s1 和 s2 是两份完全独立的数据，谁也不认识谁。
            //   （如果 Size 是 class，这里复制的只是"堆上的地址"，
            //     两个变量会指向同一个对象，改一个另一个也会跟着变。）
            Size s2 = s1;

            // ③ 改 s2 的字段 —— 动的只是 s2 自己那份副本
            //   ★ struct 的字段是 public 时可以直接点出来改；
            //     更推荐的写法是把 struct 做成不可变的（readonly，见 TODO 4），
            //     这样就不会有"我改了谁"的疑惑。
            s2.Height = 99;

            // ④ 打印：★ 打印对象会调用它的 ToString()
            //   s1 仍然是 (10×20) —— 这就是"赋值即复制"最直观的证据
            Console.WriteLine($"struct：s1 = {s1}, s2 = {s2}   ← s1 没被影响");

            // ⑤ 顺便验证 struct 上定义的方法和 class 一样能正常调用
            Console.WriteLine($"s1 的面积 = {s1.GetArea()}");

            // ⑥ 对照结论：同样一份代码，把 struct 换成 class，
            //   上面那行会打印 "cp1 = (10, 99), cp2 = (10, 99)" —— 两个都变了。
            //   ★ 记住这句话就够了：值类型复制数据，引用类型复制地址。
            Console.WriteLine("★ struct 赋值是复制数据，class 赋值是复制地址");




            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ record struct：推荐的值类型写法
            // ================================================================
            // 【要求】
            //   1. 在上面定义：
            //        readonly record struct Temperature(double Celsius)
            //        {
            //            public double Fahrenheit => Celsius * 9 / 5 + 32;
            //            public override string ToString() => $"{Celsius:F1}°C / {Fahrenheit:F1}°F";
            //        }
            //   2. 创建两个相同温度的对象，验证【按值比较】
            //   3. 打印温度（验证自动生成的 ToString 被你的重写覆盖了）
            //
            // 【预期输出】
            //     t1 = 25.0°C / 77.0°F
            //     t2 = 25.0°C / 77.0°F
            //     t1 == t2 ？ True    ← 按值比较
            //     t1 == t3 ？ False
            //
            // 【语法提示】
            //     readonly record struct Temperature(double Celsius)
            //     {
            //         public double Fahrenheit => Celsius * 9 / 5 + 32;
            //         public override string ToString() => $"{Celsius:F1}°C / {Fahrenheit:F1}°F";
            //     }
            //
            //     var t1 = new Temperature(25);
            //     var t2 = new Temperature(25);
            //     var t3 = new Temperature(30);
            //     Console.WriteLine($"t1 == t2 ？ {t1 == t2}");
            //
            // 【考察点】
            //   · record struct 一行就有一行搞定：构造函数 + 属性 + Equals + GetHashCode + ToString
            //   · 加 readonly 表示不可变（推荐）
            //   · 对比 TODO 3 手写的 struct（13 行）vs 这里（5 行）

            Console.WriteLine("--- TODO 4 ---");

            // ① 创建三个温度：t1 和 t2 内容一样，t3 不一样
            //   ★ 这里的圆括号就是 record struct 那行声明
            //     `Temperature(double Celsius)` 自动生成的"位置构造函数"，
            //     参数顺序 = 声明顺序，不用自己写构造函数。
            //   ★ 25 是 int，Celsius 要 double —— C# 自动隐式转换，不用写成 25.0。
            Temperature t1 = new Temperature(25);
            Temperature t2 = new Temperature(25);
            Temperature t3 = new Temperature(30);

            // ② 直接打印对象 → 触发我们自己重写的 ToString()
            //   ★ 如果没写那个 override，默认会打印
            //     "Temperature { Celsius = 25 }" 这种带类型名的样子；
            //     写了 override 就完全由我们说了算。
            //   ★ Fahrenheit 是计算属性，打印时现算 → 25 * 9 / 5 + 32 = 77
            Console.WriteLine($"t1 = {t1}");
            Console.WriteLine($"t2 = {t2}");

            // ③ 用 == 比较：record struct 自动生成了 == 运算符，走【按值比较】
            //   ★ t1 和 t2 是两个不同的变量、位于不同的栈内存，
            //     但里面的 Celsius 都是 25 → True（比的是"内容"，不是"地址"）
            //   ★ 换成手写的 struct（比如上面的 Size）== 就会编译报错，
            //     因为普通 struct 默认没重载 == 运算符 —— 这正是 record struct 省事的地方。
            Console.WriteLine($"t1 == t2 ？ {t1 == t2}    ← 按值比较");

            // ④ t3 的 Celsius 是 30 → 内容不同 → False
            Console.WriteLine($"t1 == t3 ？ {t1 == t3}");

            // ⑤ 加个小对照：record struct 也能用 with 表达式"复制一份再改"
            //   ★ with 会生成【新对象】，t1 本身一点不受影响（不可变的好处）。
            Temperature t4 = t1 with { Celsius = 30 };
            Console.WriteLine($"t1 with {{ Celsius = 30 }} → {t4}");




            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ 综合：订单状态机（本课考核）
            // ================================================================
            // 【背景】订单有四种状态：待付款、已付款、已发货、已完成 / 已取消
            //        状态只能按规则流转，不能乱跳。
            //
            // 【要求】
            //   1. 在上面定义：
            //        enum OrderState { PendingPayment, Paid, Shipped, Completed, Cancelled }
            //   2. 写方法 GetNextState(OrderState current)：
            //        PendingPayment → Paid
            //        Paid           → Shipped
            //        Shipped        → Completed
            //        Completed      → Completed（已完成不能再变）
            //        Cancelled      → Cancelled（已取消不能再变）
            //   3. 写方法 GetDisplayName(OrderState state) 返回中文名
            //   4. 写方法 CanCancel(OrderState state)：
            //        只有 PendingPayment 和 Paid 可以取消
            //   5. 从 PendingPayment 开始，循环推进状态，每次都打印：
            //        当前状态、能否取消、下一个状态
            //      一直推进到状态不再变化为止（最多循环 5 次防止死循环）
            //
            // 【预期输出】
            //     ========== 订单状态流转 ==========
            //     第 1 步：待付款    | 可取消：True  | 下一步：已付款
            //     第 2 步：已付款    | 可取消：True  | 下一步：已发货
            //     第 3 步：已发货    | 可取消：False | 下一步：已完成
            //     第 4 步：已完成    | 可取消：False | 下一步：已完成
            //     （状态不再变化，停止）
            //
            // 【提示】
            //     enum OrderState { PendingPayment, Paid, Shipped, Completed, Cancelled }
            //
            //     static OrderState GetNextState(OrderState current) => current switch
            //     {
            //         OrderState.PendingPayment => OrderState.Paid,
            //         OrderState.Paid           => OrderState.Shipped,
            //         OrderState.Shipped        => OrderState.Completed,
            //         _                         => current        // 其他情况保持不变
            //     };
            //
            //     static string GetDisplayName(OrderState s) => s switch
            //     {
            //         OrderState.PendingPayment => "待付款",
            //         OrderState.Paid           => "已付款",
            //         OrderState.Shipped        => "已发货",
            //         OrderState.Completed      => "已完成",
            //         OrderState.Cancelled      => "已取消",
            //         _                         => "未知"
            //     };
            //
            //     static bool CanCancel(OrderState s)
            //         => s is OrderState.PendingPayment or OrderState.Paid;
            //
            //     主循环：
            //     OrderState state = OrderState.PendingPayment;
            //     for (int step = 1; step <= 5; step++)
            //     {
            //         OrderState next = GetNextState(state);
            //         Console.WriteLine($"第 {step} 步：...");
            //         if (next == state) { Console.WriteLine("（状态不再变化，停止）"); break; }
            //         state = next;
            //     }
            //
            // 【考察点】
            //   · 用枚举表达"有限个状态"是最标准的做法
            //   · switch 表达式根据当前状态推导下一个状态 = "状态机"
            //   · `s is A or B` 是模式匹配的写法（等价 `s == A || s == B`）
            //   · 真实业务系统（订单、审批、工单）大量使用这种状态机

            Console.WriteLine("--- TODO 5 ---");

            Console.WriteLine("========== 订单状态流转 ==========");

            // ① 起点：新下的订单都是"待付款"
            OrderState state = OrderState.PendingPayment;

            // ② 最多循环 5 次（防死循环），每次往前走一步
            //   ★ 为什么要有上限？因为"状态机写错"的典型症状就是原地打转或来回跳，
            //     加个上限能让程序自己停下，而不是卡死。
            for (int step = 1; step <= 5; step++)
            {
                // ③ 先算出"下一步该去哪"，但【先别改 state】——
                //   因为下面还要用 state 判断"是不是已经到头了"。
                OrderState next = GetNextState(state);

                // ④ 一行打印三样东西：中文状态名、能否取消、下一个状态名
                //   ★ {GetDisplayName(state),-7}：逗号后面是【对齐说明】，
                //     -7 = 左对齐、不足 7 个字符就补空格。状态名都是 3 个汉字，
                //     补 1 个空格刚好凑出"3 个字 + 1 个空格"的宽度，四行的竖线 | 才能对齐。
                //   ★ {CanCancel(state),-5} 同理：True / False 长度不同，
                //     统一占 5 格才能让后面的 | 也对齐。
                Console.WriteLine($"第 {step} 步：{GetDisplayName(state),-7}| 可取消：{CanCancel(state),-5} | 下一步：{GetDisplayName(next)}");

                // ⑤ 下一个状态和自己一样 → 已经走到"终点状态"（已完成 / 已取消）→ 收工
                //   ★ 枚举是值类型，== 比的是数值本身，这里就是在问"还是原来那个状态吗"。
                if (next == state)
                {
                    Console.WriteLine("（状态不再变化，停止）");
                    break;      // break 跳出 for 循环，而不是跳出 if
                }

                // ⑥ 状态确实变了，才推进到下一步，进入下一轮循环
                state = next;
            }




            Console.WriteLine();


            // ================================================================
            Console.WriteLine("===== 练习结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
