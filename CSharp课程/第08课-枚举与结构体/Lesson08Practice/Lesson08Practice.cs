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




    // ================================================================
    // ↓↓↓ TODO 2 ｜ 定义 [Flags] 枚举
    // ================================================================
    // 在这里定义 [Flags] enum Permissions
    //     None=0, Read=1, Write=2, Delete=4, All=Read|Write|Delete




    // ================================================================
    // ↓↓↓ TODO 3、4 ｜ 定义 struct 和 record struct
    // ================================================================



    class Lesson08Practice
    {
        // ================================================================
        // ↓↓↓ TODO 1 用的方法写在这里 ↓↓↓
        // ================================================================




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

            // ↓↓↓ 在这里写你的代码 ↓↓↓




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

            // ↓↓↓ 在这里写你的代码 ↓↓↓




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

            // ↓↓↓ 在这里写你的代码 ↓↓↓




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

            // ↓↓↓ 在这里写你的代码 ↓↓↓




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
