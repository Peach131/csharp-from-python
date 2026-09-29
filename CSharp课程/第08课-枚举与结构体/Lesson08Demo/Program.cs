// ============================================================================
//  第 08 课 · 示例代码
//  枚举与结构体
// ============================================================================

namespace Lesson08Demo
{
    // ========================================================================
    // 一、普通枚举
    // ========================================================================
    enum OrderStatus
    {
        Pending,        // = 0  待处理
        Shipped,        // = 1  已发货
        Delivered,      // = 2  已送达
        Cancelled       // = 3  已取消
    }

    // 显式指定底层值
    enum HttpStatus
    {
        OK = 200,
        NotFound = 404,
        ServerError = 500
    }


    // ========================================================================
    // 二、[Flags] 位标志枚举：值必须是 2 的幂
    // ========================================================================
    [Flags]
    enum FilePermission
    {
        None = 0,                              // 0000
        Read = 1,                              // 0001
        Write = 2,                             // 0010
        Execute = 4,                           // 0100
        All = Read | Write | Execute           // 0111
    }


    // ========================================================================
    // 三、struct：值类型
    // ========================================================================
    struct PointStruct
    {
        public int X;
        public int Y;

        public PointStruct(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"({X}, {Y})";
    }

    // 对照用的 class（字段完全一样，只有 class/struct 的区别）
    class PointClass
    {
        public int X;
        public int Y;

        public PointClass(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"({X}, {Y})";
    }


    // ========================================================================
    // 四、readonly record struct：推荐的值类型写法
    // ========================================================================
    enum Currency { CNY, USD, EUR }

    readonly record struct Money(decimal Amount, Currency Currency)
    {
        public override string ToString() => Currency switch
        {
            Currency.CNY => $"¥{Amount:F2}",
            Currency.USD => $"${Amount:F2}",
            Currency.EUR => $"€{Amount:F2}",
            _ => $"{Amount:F2} {Currency}"
        };
    }


    class Program
    {
        // 用 switch 表达式处理枚举（最常用的方式）
        static string Describe(OrderStatus status) => status switch
        {
            OrderStatus.Pending => "等待处理中",
            OrderStatus.Shipped => "已经发货，正在路上",
            OrderStatus.Delivered => "已送达",
            OrderStatus.Cancelled => "已取消",
            _ => "未知状态"
        };

        static void Main()
        {
            Console.WriteLine("========== 第 08 课：枚举与结构体 ==========");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 1：枚举比魔法数字好在哪 ----");

            // ❌ 用数字：看不懂，还容易记错
            int badStatus = 1;
            Console.WriteLine($"   用数字：status = {badStatus}    ← 1 是什么意思？");

            // ✅ 用枚举：一眼看懂，而且 IDE 能补全
            OrderStatus goodStatus = OrderStatus.Shipped;
            Console.WriteLine($"   用枚举：status = {goodStatus}    ← 清楚多了");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 2：枚举的底层是整数 ----");

            Console.WriteLine($"   OrderStatus.Pending   = {(int)OrderStatus.Pending}   ← 默认从 0 开始");
            Console.WriteLine($"   OrderStatus.Shipped   = {(int)OrderStatus.Shipped}");
            Console.WriteLine($"   OrderStatus.Delivered = {(int)OrderStatus.Delivered}");
            Console.WriteLine($"   OrderStatus.Cancelled = {(int)OrderStatus.Cancelled}");
            Console.WriteLine();

            Console.WriteLine("   显式指定值的枚举：");
            Console.WriteLine($"   HttpStatus.OK          = {(int)HttpStatus.OK}");
            Console.WriteLine($"   HttpStatus.NotFound    = {(int)HttpStatus.NotFound}");
            Console.WriteLine($"   HttpStatus.ServerError = {(int)HttpStatus.ServerError}");
            Console.WriteLine();

            Console.WriteLine("   互相转换（需要显式转换）：");
            int num = (int)OrderStatus.Delivered;
            OrderStatus back = (OrderStatus)2;
            Console.WriteLine($"     枚举 → 数字：(int)OrderStatus.Delivered = {num}");
            Console.WriteLine($"     数字 → 枚举：(OrderStatus)2 = {back}");
            Console.WriteLine("   ⚠️ (OrderStatus)999 也能编译通过，即使 999 不是任何成员 —— 所以别乱转");
            Console.WriteLine();

            Console.WriteLine("   遍历所有枚举值：");
            foreach (OrderStatus s in Enum.GetValues<OrderStatus>())
            {
                Console.Write($"     {s}={(int)s}");
            }
            Console.WriteLine();
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 3：用 switch 表达式处理枚举 ----");

            foreach (OrderStatus s in Enum.GetValues<OrderStatus>())
            {
                Console.WriteLine($"   {s,-10} → {Describe(s)}");
            }
            Console.WriteLine("   ★ 加新枚举成员时，编译器会提醒你 switch 可能没覆盖全");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 4：字符串和枚举互转 ----");

            string input = "Shipped";
            if (Enum.TryParse<OrderStatus>(input, out var parsed))
            {
                Console.WriteLine($"   Enum.TryParse(\"{input}\") 成功 → {parsed}（值 {(int)parsed}）");
            }

            if (!Enum.TryParse<OrderStatus>("NotAStatus", out var bad))
            {
                Console.WriteLine($"   Enum.TryParse(\"NotAStatus\") 失败（和 int.TryParse 一个套路）");
            }
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 5：★ [Flags] 位标志枚举 ----");

            // 组合多个状态用 |
            FilePermission perm = FilePermission.Read | FilePermission.Write;
            Console.WriteLine($"   Read | Write                    = {perm}");

            // 检查是否包含某个标志
            Console.WriteLine($"   perm.HasFlag(Read)              = {perm.HasFlag(FilePermission.Read)}");
            Console.WriteLine($"   perm.HasFlag(Execute)           = {perm.HasFlag(FilePermission.Execute)}");

            // 添加权限用 |=
            perm |= FilePermission.Execute;
            Console.WriteLine($"   perm |= Execute 之后             = {perm}");

            // 移除权限用 &= ~
            perm &= ~FilePermission.Write;
            Console.WriteLine($"   perm &= ~Write 之后              = {perm}");
            Console.WriteLine();

            Console.WriteLine("   底层的位运算（这就是为什么值必须是 2 的幂）：");
            Console.WriteLine($"      Read    = {Convert.ToString((int)FilePermission.Read, 2).PadLeft(4, '0')}");
            Console.WriteLine($"      Write   = {Convert.ToString((int)FilePermission.Write, 2).PadLeft(4, '0')}");
            Console.WriteLine($"      Read|W  = {Convert.ToString((int)(FilePermission.Read | FilePermission.Write), 2).PadLeft(4, '0')}   ← 按位或");
            Console.WriteLine($"      All     = {Convert.ToString((int)FilePermission.All, 2).PadLeft(4, '0')}");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 6：★ struct vs class 的赋值行为 ----");

            PointStruct sp1 = new PointStruct(1, 2);
            PointStruct sp2 = sp1;                  // struct：复制整个数据
            sp2.X = 99;
            Console.WriteLine($"   struct：sp1 = {sp1}, sp2 = {sp2}   ← sp1 没被影响");

            PointClass cp1 = new PointClass(1, 2);
            PointClass cp2 = cp1;                   // class：复制地址
            cp2.X = 99;
            Console.WriteLine($"   class ：cp1 = {cp1}, cp2 = {cp2}   ← cp1 也变了");
            Console.WriteLine("   ★ 这就是值类型和引用类型的区别（第 01 课讲过，这里用自定义类型再看一遍）");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("---- 演示 7：record struct —— 推荐的值类型写法 ----");

            var price1 = new Money(199.9m, Currency.CNY);
            var price2 = new Money(199.9m, Currency.CNY);
            var price3 = new Money(29.99m, Currency.USD);

            Console.WriteLine($"   price1            = {price1}");
            Console.WriteLine($"   price3            = {price3}");
            Console.WriteLine($"   price1 == price2  = {price1 == price2}   ← 按值比较！");
            Console.WriteLine($"   price1 == price3  = {price1 == price3}");
            Console.WriteLine();

            // 枚举 + record struct 组合的好处：不可变、按值比较、不会为 null
            Console.WriteLine("   ★ 为什么这样设计好：");
            Console.WriteLine("       · record struct 是【不可变】的，不会被意外改动");
            Console.WriteLine("       · 按【值】比较，两个 ¥199.90 就是相等");
            Console.WriteLine("       · 是【值类型】，不会为 null，少一类 bug");
            Console.WriteLine("       · 比用 decimal + string 两个变量清晰得多");
            Console.WriteLine();


            // ----------------------------------------------------------------
            Console.WriteLine("========== 示例结束 ==========");
            Console.WriteLine();
            Console.WriteLine("接下来请打开：练习\\LessonPractice\\Lesson08Practice.cs");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
