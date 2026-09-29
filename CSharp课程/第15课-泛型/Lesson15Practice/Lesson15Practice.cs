// ============================================================================
//  第 15 课 · 练习：泛型
//
//  ★ 本课重点：
//     1. 泛型类/方法：class Box<T>、static T Max<T>(...)
//     2. 泛型约束 where T : ...
//     3. 泛型零装箱；每种 T 的静态字段独立
// ============================================================================

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ TODO 1、2 用的泛型类写在这里 ↓↓↓
    // ================================================================




    class Lesson15Practice
    {
        // ================================================================
        // ↓↓↓ TODO 2、3 用的泛型方法写在这里 ↓↓↓
        // ================================================================




        static void Main()
        {
            Console.WriteLine("===== 第 15 课练习：泛型 =====\n");

            // ================================================================
            // TODO 1 ｜ 写一个泛型类
            // ================================================================
            // 【要求】写一个泛型类 MyStack<T>，模拟"栈"（后进先出）：
            //     - 内部用一个 List<T> 存数据
            //     - void Push(T item)     压入
            //     - T Pop()               弹出最后一个；如果栈空抛 InvalidOperationException
            //     - T Peek()              看最后一个但不弹出；栈空也抛异常
            //     - int Count { get; }    元素个数
            //     - bool IsEmpty { get; } 是否为空
            //
            //   然后用 int 和 string 各测一遍
            //
            // 【预期输出】
            //     ===== 整数栈 =====
            //     依次压入 1, 2, 3 后，Count = 3
            //     Peek() = 3
            //     Pop() = 3，剩余 Count = 2
            //     Pop() = 2
            //     Pop() = 1，IsEmpty = True
            //     ===== 字符串栈 =====
            //     压入 A, B 后 Count = 2
            //     Pop() = B
            //     Pop() = A
            //
            // 【提示】
            //     class MyStack<T>
            //     {
            //         private readonly List<T> _items = new();
            //
            //         public int Count => _items.Count;
            //         public bool IsEmpty => _items.Count == 0;
            //
            //         public void Push(T item) => _items.Add(item);
            //
            //         public T Pop()
            //         {
            //             if (IsEmpty) throw new InvalidOperationException("栈是空的");
            //             T last = _items[^1];
            //             _items.RemoveAt(_items.Count - 1);
            //             return last;
            //         }
            //
            //         public T Peek()
            //         {
            //             if (IsEmpty) throw new InvalidOperationException("栈是空的");
            //             return _items[^1];
            //         }
            //     }
            //
            // 【考察点】一个泛型类，int 和 string 都能用，而且类型安全

            Console.WriteLine("--- TODO 1 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 2 ｜ 泛型方法 + 约束
            // ================================================================
            // 【要求】写三个泛型方法：
            //   ① static T FindMax<T>(List<T> items) where T : IComparable<T>
            //        返回最大值（列表为空抛异常）
            //   ② static bool Contains<T>(List<T> items, T target)
            //        判断是否包含（不需要约束，因为用的是 Equals）
            //   ③ static List<T> TakeFirst<T>(List<T> items, int count)
            //        返回前 count 个元素（count 超出就返回全部）
            //
            //   用 int 和 string 列表各测一遍
            //
            // 【预期输出】
            //     整数：[5, 12, 8, 20, 7]
            //       最大值 = 20
            //       包含 8 吗？True
            //       前 3 个：[5, 12, 8]
            //     字符串：[apple, banana, cherry]
            //       最大值 = cherry    ← 字符串也能比大小（按字典序）
            //       包含 "pear" 吗？False
            //       前 2 个：[apple, banana]
            //
            // 【提示】
            //     static T FindMax<T>(List<T> items) where T : IComparable<T>
            //     {
            //         if (items.Count == 0) throw new InvalidOperationException("列表为空");
            //         T max = items[0];
            //         for (int i = 1; i < items.Count; i++)
            //         {
            //             if (items[i].CompareTo(max) > 0) max = items[i];
            //         }
            //         return max;
            //     }
            //
            //     static bool Contains<T>(List<T> items, T target)
            //     {
            //         foreach (T item in items)
            //         {
            //             if (Equals(item, target)) return true;   // ★ 泛型里比较用 Equals
            //         }
            //         return false;
            //     }
            //
            //     static List<T> TakeFirst<T>(List<T> items, int count)
            //     {
            //         var result = new List<T>();
            //         for (int i = 0; i < items.Count && i < count; i++)
            //         {
            //             result.Add(items[i]);
            //         }
            //         return result;
            //     }
            //
            // 【考察点】
            //   · `where T : IComparable<T>` 让编译器知道 T 能比较
            //   · 没有约束的 Contains 用 Equals 比较（不能用 ==，因为泛型可能是 struct）
            //   · List<T> 的输出用 string.Join(", ", list)

            Console.WriteLine("--- TODO 2 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 3 ｜ 验证泛型静态字段的独立性
            // ================================================================
            // 【要求】
            //   1. 在上面定义一个泛型类：
            //        class TypeCounter<T> { public static int Count; }
            //   2. 给三种不同类型各加一个计数，验证它们互不影响
            //
            // 【预期输出】
            //     TypeCounter<int>.Count    = 1
            //     TypeCounter<string>.Count = 2
            //     TypeCounter<double>.Count = 3
            //     → 三个是【独立的】静态字段
            //
            // 【提示】
            //     class TypeCounter<T> { public static int Count; }
            //
            //     TypeCounter<int>.Count = 1;
            //     TypeCounter<string>.Count = 2;
            //     TypeCounter<double>.Count = 3;
            //
            //     Console.WriteLine($"TypeCounter<int>.Count    = {TypeCounter<int>.Count}");
            //     ...
            //
            // 【考察点】
            //   · 每个具体泛型类型在运行时是独立类型
            //   · 这个特性可以用来做"按类型自动缓存"

            Console.WriteLine("--- TODO 3 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 4 ｜ 装箱对照（体会泛型的性能价值）
            // ================================================================
            // 【要求】
            //   1. 用 System.Collections.ArrayList（非泛型）往里面 Add 100 万个 int
            //   2. 用 List<int>（泛型）做同样的事
            //   3. 分别测量耗时和分配的内存（GC.GetAllocatedBytesForCurrentThread）
            //   4. 打印对比并写一句结论
            //
            // 【预期输出】（数字会不同）
            //     往集合里 Add 1,000,000 个 int：
            //       ArrayList（装箱）  : XXX ms, 分配 XXX.X MB
            //       List<int>（泛型）  : X.X ms, 分配  X.X MB
            //       → 泛型快了约 XX 倍，少分配 XXX 倍内存
            //
            // 【提示】
            //     using System.Collections;      // ArrayList 在这个命名空间
            //
            //     const int N = 1_000_000;
            //
            //     GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            //     long before = GC.GetAllocatedBytesForCurrentThread();
            //     var sw = System.Diagnostics.Stopwatch.StartNew();
            //
            //     var oldList = new ArrayList();
            //     for (int i = 0; i < N; i++) oldList.Add(i);      // ★ 每次装箱
            //
            //     sw.Stop();
            //     long allocOld = GC.GetAllocatedBytesForCurrentThread() - before;
            //
            //     // List<int> 同理
            //     Console.WriteLine($"ArrayList（装箱）  : {sw.Elapsed.TotalMilliseconds:F1} ms, 分配 {allocOld/1024.0/1024:F1} MB");
            //
            // 【考察点】
            //   · 装箱 = 每次 Add 都在堆上分配一个盒子（第 11 课）
            //   · 这才是泛型最大的价值：性能，而不是代码复用

            Console.WriteLine("--- TODO 4 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            // ================================================================
            // TODO 5 ｜ 综合：泛型仓库（本课考核）
            // ================================================================
            // 【要求】写一个泛型仓库类 Repository<T>，要求：
            //     - T 必须有一个 Id 属性 → 用接口约束实现：
            //         interface IEntity { int Id { get; } }
            //         class Repository<T> where T : IEntity
            //     - void Add(T item)              添加
            //     - T? GetById(int id)            按 Id 查找，找不到返回 null
            //     - bool Remove(int id)           按 Id 删除，返回是否删掉了
            //     - List<T> GetAll()              返回全部
            //     - int Count { get; }
            //
            //   然后定义一个类实现 IEntity：
            //     class Product : IEntity
            //     {
            //         public int Id { get; set; }
            //         public string Name { get; set; } = "";
            //         public decimal Price { get; set; }
            //     }
            //
            //   测试：添加 3 个商品，按 Id 查询，删除一个，再查询
            //
            // 【预期输出】
            //     添加 3 个商品后，Count = 3
            //     查询 Id=1002 → iPad，3999.00
            //     查询 Id=9999 → 未找到
            //     删除 Id=1002 成功
            //     删除后 Count = 2
            //     再查 Id=1002 → 未找到
            //     全部商品：
            //       1001 iPhone 6999.00
            //       1003 MacBook 12999.00
            //
            // 【提示】
            //     interface IEntity { int Id { get; } }
            //
            //     class Repository<T> where T : IEntity
            //     {
            //         private readonly List<T> _items = new();
            //
            //         public int Count => _items.Count;
            //
            //         public void Add(T item) => _items.Add(item);
            //
            //         public T? GetById(int id)
            //         {
            //             foreach (T item in _items)
            //             {
            //                 if (item.Id == id) return item;   // ★ 因为有 IEntity 约束，能访问 .Id
            //             }
            //             return default;                       // 引用类型返回 null
            //         }
            //
            //         public bool Remove(int id)
            //         {
            //             for (int i = 0; i < _items.Count; i++)
            //             {
            //                 if (_items[i].Id == id) { _items.RemoveAt(i); return true; }
            //             }
            //             return false;
            //         }
            //
            //         public List<T> GetAll() => new(_items);       // 返回副本，防止外部改内部
            //     }
            //
            //     class Product : IEntity
            //     {
            //         public int Id { get; set; }
            //         public string Name { get; set; } = "";
            //         public decimal Price { get; set; }
            //     }
            //
            // 【考察点】
            //   · `where T : IEntity` 让泛型内部能访问 item.Id
            //   · 这就是"泛型仓库模式"—— 真实项目里非常常见
            //   · T? 在泛型方法里表示"可能返回 null"（引用类型时）
            //   · GetAll 返回副本而不是内部列表，是防御性编程

            Console.WriteLine("--- TODO 5 ---");
            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
