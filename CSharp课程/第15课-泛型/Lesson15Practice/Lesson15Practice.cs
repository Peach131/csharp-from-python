// ============================================================================
//  第 15 课 · 练习：泛型
//
//  ★ 本课重点：
//     1. 泛型类/方法：class Box<T>、static T Max<T>(...)
//     2. 泛型约束 where T : ...
//     3. 泛型零装箱；每种 T 的静态字段独立
// ============================================================================

// ★ TODO 4 要用的 ArrayList 属于 System.Collections（注意不是 Collections.Generic）。
//   本项目开了 ImplicitUsings，System / System.Collections.Generic 这些是自动引入的，
//   但 System.Collections 不在其中，所以必须自己写一行 using。
using System.Collections;

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ TODO 1、2 用的泛型类写在这里 ↓↓↓
    // ================================================================

    // ────────────────────────────────────────────────────────────────
    // TODO 1 ｜ MyStack<T>：一个泛型类，int 和 string 都能装
    // ────────────────────────────────────────────────────────────────
    // ★ 类名后面的 <T> 就是【类型参数】：写这个类的时候 T 是占位符，
    //   用的时候才填具体类型（MyStack<int> / MyStack<string>）。
    //   于是"栈"的逻辑只写一遍，装什么由使用方决定。
    class MyStack<T>
    {
        // ★ 泛型类内部可以把 T 当普通类型名来用，包括拿它去拼别的泛型类型。
        //   这里用 List<T> 当底层容器，省掉自己写数组扩容的麻烦。
        //   readonly 防的是"把 _items 换成另一个新列表"，不防往里面 Add/Remove。
        private readonly List<T> _items = new();

        // ★ 表达式体属性：读 Count 就是读内部列表的长度，不再单独存一份计数。
        //   若自己维护 _count 字段，Push/Pop 里就得多写 +1/-1，容易漏写一处。
        public int Count => _items.Count;
        public bool IsEmpty => _items.Count == 0;

        public void Push(T item) => _items.Add(item);

        public T Pop()
        {
            // ★ 先判断再取值：空栈时 _items[^1] 会抛 ArgumentOutOfRangeException，
            //   那是"集合内部实现"的异常。对外抛语义明确的 InvalidOperationException，
            //   调用方才知道是"用法不对（对空栈 Pop）"，而不是"下标越界"。
            if (IsEmpty) throw new InvalidOperationException("栈是空的");

            // ★ ^1 表示倒数第一个元素（和第 03 课的索引写法一样）。
            //   必须先把值取出来再删，删完就取不到了。
            T last = _items[^1];
            _items.RemoveAt(_items.Count - 1);
            return last;
        }

        public T Peek()
        {
            if (IsEmpty) throw new InvalidOperationException("栈是空的");

            // ★ 和 Pop 只差"不删"这一步，所以 Peek 不会改变 Count。
            return _items[^1];
        }
    }


    // ────────────────────────────────────────────────────────────────
    // TODO 3 ｜ TypeCounter<T>：验证"每种 T 的静态字段互相独立"
    // ────────────────────────────────────────────────────────────────
    // ★ 普通类的 static 字段是"全类共用一份"（第 06 课）。
    //   泛型类不一样：TypeCounter<int> 和 TypeCounter<string> 在运行时
    //   是两个【不同的类型】，所以各有一份自己的静态字段，互不影响。
    class TypeCounter<T>
    {
        // ★ 这里必须是可写的 public static 【字段】（不是属性），
        //   因为练习里要直接写 TypeCounter<int>.Count = 1。
        //   int 是值类型默认 0，不用给初值，也不会触发 CS8618。
        public static int Count;
    }


    // ────────────────────────────────────────────────────────────────
    // TODO 5 ｜ IEntity + Repository<T> + Product：泛型仓库（本课考核）
    // ────────────────────────────────────────────────────────────────
    // ★ 这个接口是给泛型约束用的"入场券"：
    //   只要 T 实现了 IEntity，Repository<T> 内部就能放心地写 item.Id。
    interface IEntity
    {
        int Id { get; }
    }

    class Repository<T> where T : IEntity
    {
        private readonly List<T> _items = new();

        public int Count => _items.Count;

        public void Add(T item) => _items.Add(item);

        // ★ 返回类型写成 T? 表示"可能找不到"：T 是引用类型时 T? 就是"可能为 null"，
        //   调用方拿到后必须先判 null 再用（第 12 课的可空检查会盯着他）。
        public T? GetById(int id)
        {
            foreach (T item in _items)
            {
                // ★ 这一行能写 item.Id，全靠 where T : IEntity 这个约束 ——
                //   没有它的话编译器只知道 T 是"某种未知类型"，连 Id 存不存在都不敢假设。
                if (item.Id == id) return item;
            }

            // ★ default 对引用类型就是 null，正好拿来当"没找到"的信号。
            return default;
        }

        public bool Remove(int id)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == id)
                {
                    _items.RemoveAt(i);

                    // ★ 删完立刻 return：RemoveAt 会让后面的元素整体前移一位，
                    //   继续往下遍历会刚好跳过一个元素。
                    return true;
                }
            }
            return false;
        }

        // ★ new(_items) 就是 new List<T>(_items) 的简写：复制一份【副本】再返回。
        //   若直接把内部 _items 交出去，外部一句 GetAll().Clear() 就能清空仓库，
        //   封装就形同虚设 —— 这叫防御性复制。
        public List<T> GetAll() => new(_items);
    }

    class Product : IEntity
    {
        public int Id { get; set; }

        // ★ string 是引用类型，不给初值默认就是 null，在 <Nullable>enable</Nullable>
        //   下会报 CS8618 警告，所以给个空字符串打底。
        public string Name { get; set; } = "";

        // ★ decimal 是值类型默认 0，不用给初值。
        //   钱用 decimal 而不是 double：十进制小数不丢精度（第 01 课的坑）。
        public decimal Price { get; set; }
    }


    class Lesson15Practice
    {
        // ================================================================
        // ↓↓↓ TODO 2、3 用的泛型方法写在这里 ↓↓↓
        // ================================================================

        // ────────────────────────────────────────────────────────────
        // ① TODO 2 ｜ FindMax<T>：带 IComparable<T> 约束的泛型方法
        // ────────────────────────────────────────────────────────────
        // ★ 为什么非要加 where T : IComparable<T>？
        //   方法体里想写 items[i].CompareTo(max)，但 T 是未知类型，
        //   编译器不知道它有没有 CompareTo。约束就是在向编译器"担保"：
        //   传进来的 T 一定实现了这个接口，出了事我负责。
        static T FindMax<T>(List<T> items) where T : IComparable<T>
        {
            // ★ 空列表要单独拦一下：否则下面 items[0] 直接下标越界。
            if (items.Count == 0) throw new InvalidOperationException("列表为空");

            // ★ 最大值初值取【第一个元素】，既不能写 0，也不该写 default：
            //   T 是未知类型，字面量 0 编译不过；写 default(T) 又会在"全是负数"
            //   的列表上错误地返回 0（第 03 课踩过的坑，泛型里更容易犯）。
            T max = items[0];

            for (int i = 1; i < items.Count; i++)
            {
                // ★★ 泛型里比较大小只能用 CompareTo，不能写 items[i] > max：
                //    > 运算符只对具体类型有定义，对未知的 T 编译不过（CS0019）。
                //    CompareTo 的约定：> 0 表示"我比你大"，= 0 相等，< 0 更小。
                if (items[i].CompareTo(max) > 0) max = items[i];
            }

            return max;
        }


        // ────────────────────────────────────────────────────────────
        // ② TODO 2 ｜ Contains<T>：不需要约束（因为用的是 Equals）
        // ────────────────────────────────────────────────────────────
        // ★ 这里不用写 where T : IComparable<T>：只要判断"相等"，
        //   不需要比大小，就别给调用方加没必要的门槛。
        static bool Contains<T>(List<T> items, T target)
        {
            foreach (T item in items)
            {
                // ★★ 不能用 item == target：T 可能是值类型也可能是引用类型，
                //    编译器不知道该用"值相等"还是"引用相等"，索性直接报编译错误。
                //    Equals 是 object 上的方法、所有类型都有，所以泛型比较一律用它。
                if (Equals(item, target)) return true;
            }
            return false;
        }


        // ────────────────────────────────────────────────────────────
        // ③ TODO 2 ｜ TakeFirst<T>：返回前 count 个（不需要约束）
        // ────────────────────────────────────────────────────────────
        static List<T> TakeFirst<T>(List<T> items, int count)
        {
            var result = new List<T>();

            // ★ 两个条件一起管住边界：i < count 管"只要前 count 个"，
            //   i < items.Count 管"别越界"。所以 count 传 100 也不会抛异常，
            //   正好满足"count 超出就返回全部"的要求。
            for (int i = 0; i < items.Count && i < count; i++)
            {
                result.Add(items[i]);
            }

            // ★ 返回的是新列表：调用方改它不会影响传进来的 items（防御性复制）。
            return result;
        }


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

            // ① 把 T 填成 int，就得到一个"只装 int 的栈"。
            //   ★ 对比"用 object 装一切"的写法（第 11 课讲的装箱）：那样 push 进去会被当成
            //     object、每次 Add 都要装箱，取出来还得 (int) 强转。这里放进去是 int、
            //     拿出来还是 int，类型由编译器全程把关，连强转都不用写。
            MyStack<int> intStack = new MyStack<int>();

            Console.WriteLine("===== 整数栈 =====");

            intStack.Push(1);
            intStack.Push(2);
            intStack.Push(3);
            // ★ 后进先出：1 最先压进去、被压在最底下，所以最后才出来。
            Console.WriteLine($"依次压入 1, 2, 3 后，Count = {intStack.Count}");

            // ② Peek 只是"看一眼"栈顶，不弹出，所以 Count 仍然是 3。
            Console.WriteLine($"Peek() = {intStack.Peek()}");

            // ③ 弹出时别"边打印边改栈"：插值字符串里的 {} 是从左到右求值的，写成
            //      $"Pop() = {intStack.Pop()}，剩余 Count = {intStack.Count}"
            //    结果虽然也对，但读的人得在脑子里模拟求值顺序，不如拆成两行清楚。
            int popped = intStack.Pop();
            Console.WriteLine($"Pop() = {popped}，剩余 Count = {intStack.Count}");

            Console.WriteLine($"Pop() = {intStack.Pop()}");

            popped = intStack.Pop();
            Console.WriteLine($"Pop() = {popped}，IsEmpty = {intStack.IsEmpty}");

            // ④ 换个 T 再来一遍：同一份 MyStack<T> 代码，一个字没改就能装 string。
            MyStack<string> strStack = new MyStack<string>();

            Console.WriteLine("===== 字符串栈 =====");

            strStack.Push("A");
            strStack.Push("B");
            Console.WriteLine($"压入 A, B 后 Count = {strStack.Count}");

            Console.WriteLine($"Pop() = {strStack.Pop()}");
            Console.WriteLine($"Pop() = {strStack.Pop()}");

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

            // ① int 列表：调用时不用写 FindMax<int>(...)，编译器从实参把 T 推断成 int。
            List<int> numbers = new List<int> { 5, 12, 8, 20, 7 };

            // ★ 为什么要 string.Join？因为 List 没有重写 ToString，
            //   直接打印只会得到 "System.Collections.Generic.List`1[System.Int32]"。
            //   string.Join(", ", 列表) 才是我们要的 "5, 12, 8, 20, 7"。
            Console.WriteLine($"整数：[{string.Join(", ", numbers)}]");
            Console.WriteLine($"  最大值 = {FindMax(numbers)}");
            Console.WriteLine($"  包含 8 吗？{Contains(numbers, 8)}");
            Console.WriteLine($"  前 3 个：[{string.Join(", ", TakeFirst(numbers, 3))}]");

            // ② string 列表：同一组泛型方法原样复用，一行都不用改。
            List<string> fruits = new List<string> { "apple", "banana", "cherry" };
            Console.WriteLine($"字符串：[{string.Join(", ", fruits)}]");

            // ★ string 实现了 IComparable<string>，所以能进 FindMax；
            //   它按【字典序】比较：cherry > banana > apple，最大值就是 cherry。
            Console.WriteLine($"  最大值 = {FindMax(fruits)}    ← 字符串也能比大小（按字典序）");

            // ★ 想显示英文双引号，字符串里必须写 \" 转义 ——
            //   直接写一个 " 会把字符串提前截断，编译器报语法错误。
            Console.WriteLine($"  包含 \"pear\" 吗？{Contains(fruits, "pear")}");
            Console.WriteLine($"  前 2 个：[{string.Join(", ", TakeFirst(fruits, 2))}]");

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

            // ★ 故意给三种 T 各写一个不同的数：
            //   如果静态字段是"所有 T 共享一份"，三次赋值就会互相覆盖，
            //   最后三行应该都打印 3。实际上打印的是 1 / 2 / 3 → 证明各是各的。
            TypeCounter<int>.Count = 1;
            TypeCounter<string>.Count = 2;
            TypeCounter<double>.Count = 3;

            Console.WriteLine($"TypeCounter<int>.Count    = {TypeCounter<int>.Count}");
            Console.WriteLine($"TypeCounter<string>.Count = {TypeCounter<string>.Count}");
            Console.WriteLine($"TypeCounter<double>.Count = {TypeCounter<double>.Count}");

            // ★ 顺手把结论打出来加深印象：每个具体泛型类型在运行时都是【独立的类型】，
            //   静态字段自然各一份 —— 想"按类型缓存数据"，开个泛型类的静态字段就行，
            //   连 Dictionary 都不用。
            Console.WriteLine("→ 三个是【独立的】静态字段");

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

            const int N = 1_000_000;

            // ① 非泛型 ArrayList：Add 的参数类型是 object，
            //   每加一个 int 都要【装箱】（堆上分一个 24 字节的盒子 —— 第 11 课）。
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            long before = GC.GetAllocatedBytesForCurrentThread();
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var oldList = new ArrayList();
            for (int i = 0; i < N; i++) oldList.Add(i);

            sw.Stop();
            long allocOld = GC.GetAllocatedBytesForCurrentThread() - before;
            double oldMs = sw.Elapsed.TotalMilliseconds;

            // ② 泛型 List<int>：Add 的参数类型就是 int，一个盒子都不用分。
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            before = GC.GetAllocatedBytesForCurrentThread();
            sw.Restart();

            // ★ 先 new List<int>(N) 把容量一次预定好，这样两边都不含"数组扩容"的开销，
            //   比出来的差距才纯粹是装箱造成的。
            var newList = new List<int>(N);
            for (int i = 0; i < N; i++) newList.Add(i);

            sw.Stop();
            long allocNew = GC.GetAllocatedBytesForCurrentThread() - before;
            double newMs = sw.Elapsed.TotalMilliseconds;

            // ★ 下面的耗时和字节数【会随机器、Debug/Release、.NET 版本变化】，看量级就够了：
            //   ArrayList 要为每个 int 额外分 24 字节的盒子（外加数组扩容时的搬运），
            //   List<int> 只有那块 int 数组本身（100 万个 int ≈ 4 MB）。
            Console.WriteLine($"往集合里 Add {N:N0} 个 int：");
            Console.WriteLine($"  ArrayList（装箱）  : {oldMs:F1} ms, 分配 {allocOld / 1024.0 / 1024:F1} MB");
            Console.WriteLine($"  List<int>（泛型）  : {newMs:F1} ms, 分配  {allocNew / 1024.0 / 1024:F1} MB");

            // ★ 倍数当场算出来，别写死：先转 double 再除，整数除法会把小数砍掉。
            //   allocNew 至少有 4 MB，不可能是 0，所以这里除法是安全的。
            double speedRatio = oldMs / newMs;
            double allocRatio = (double)allocOld / allocNew;
            Console.WriteLine($"  → 泛型快了约 {speedRatio:F0} 倍，少分配 {allocRatio:F0} 倍内存");

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

            // ① 建一个"只装 Product 的仓库"：T 填成 Product，别的类型塞不进来。
            Repository<Product> repo = new Repository<Product>();

            // ★ 商品用【对象初始化器】创建（第 05 课）。
            //   Price 是 decimal，字面量末尾要加 m，否则 6999 会被当成 int 而类型不匹配。
            repo.Add(new Product { Id = 1001, Name = "iPhone", Price = 6999m });
            repo.Add(new Product { Id = 1002, Name = "iPad", Price = 3999m });
            repo.Add(new Product { Id = 1003, Name = "MacBook", Price = 12999m });

            Console.WriteLine($"添加 3 个商品后，Count = {repo.Count}");

            // ② 查得到的情况：GetById 返回 Product?（可能为 null），
            //   所以先判 null 再用 —— 判过之后编译器才允许访问 found.Name。
            Product? found = repo.GetById(1002);
            if (found is null)
            {
                Console.WriteLine("查询 Id=1002 → 未找到");
            }
            else
            {
                Console.WriteLine($"查询 Id=1002 → {found.Name}，{found.Price:F2}");
            }

            // ③ 查不到的情况：仓库里没有 Id=9999，GetById 返回 default → null
            Product? missing = repo.GetById(9999);
            if (missing is null)
            {
                Console.WriteLine("查询 Id=9999 → 未找到");
            }
            else
            {
                Console.WriteLine($"查询 Id=9999 → {missing.Name}，{missing.Price:F2}");
            }

            // ④ 删除：Remove 用 bool 返回值告诉你"到底删掉了没有"，
            //   这比"删失败也默默返回"有用得多 —— 调用方能据此提示用户。
            bool removed = repo.Remove(1002);
            if (removed)
            {
                Console.WriteLine("删除 Id=1002 成功");
            }
            else
            {
                Console.WriteLine("删除 Id=1002 失败（没这个 Id）");
            }

            Console.WriteLine($"删除后 Count = {repo.Count}");

            // ⑤ 再查一次同一个 Id，验证它真的从仓库里消失了
            Product? gone = repo.GetById(1002);
            if (gone is null)
            {
                Console.WriteLine("再查 Id=1002 → 未找到");
            }
            else
            {
                Console.WriteLine($"再查 Id=1002 → {gone.Name}，{gone.Price:F2}");
            }

            // ⑥ 列出剩下的商品：GetAll() 给的是副本，遍历它不会碰到仓库内部数据。
            Console.WriteLine("全部商品：");
            foreach (Product p in repo.GetAll())
            {
                // ★ Price 是 decimal，F2 保留两位小数 → 12999.00
                Console.WriteLine($"  {p.Id} {p.Name} {p.Price:F2}");
            }

            Console.WriteLine();


            Console.WriteLine("===== 练习结束 =====");
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }
    }
}
