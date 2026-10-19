// ============================================================================
//  第 25 课 · 练习：集合的内部实现（手写 MyList 和哈希表）
//
//  ★ 本课重点：
//     1. MyList<T> = T[] 数组 + Count，翻倍扩容（Array.Copy / Array.Resize）
//     2. 索引器 this[int index] 的 get / set
//     3. ★ Capacity（数组长度） vs Count（元素个数）—— 本课最重要的一对概念
//     4. Insert / RemoveAt 要搬元素 → O(n)；链表为什么不是这样
//     5. MyHashMap<K,V>：哈希 → 桶 → 冲突链 → 负载因子 → rehash
//     6. ⚠️ 哈希契约：a.Equals(b) 为 true ⇒ GetHashCode() 必须相等
//
//  ★ 和前 24 课不同：本课要你自己【写类】。
//    TODO 1~4 的类写在 Lesson25Practice 类的【外面】（文件上半部分留了位置），
//    TODO 5 是使用这些类做实测，写在 Main 里面。
//
//  ★ 提示：
//     · 泛型数组 new T[n] 是合法的，元素默认是 default(T)。
//     · 索引器写法：public T this[int index] { get { ... } set { ... } }
//     · 泛型里比较两个值用 EqualityComparer<T>.Default.Equals(a, b)，不要用 a == b
//       （T 可能是没重载 == 的自定义类型，也可能让值类型装箱）。
// ============================================================================

using System.Diagnostics;

namespace LessonPractice
{
    // ================================================================
    // ↓↓↓ TODO 1~4 要实现的类写在这里（在 Lesson25Practice 类的【外面】）↓↓↓
    // ================================================================

    // TODO 1 ｜ 手写 MyList<T>：数组 + Count + 索引器
    // 【要求】实现一个泛型类 MyList<T>，包含：
    //           1. 私有字段 T[] _items，初始长度 4
    //           2. 属性 int Count（只读，外部不能改）
    //           3. 属性 int Capacity（只读，返回 _items.Length）
    //           4. 方法 void Add(T item)：放到下标 Count 处，然后 Count++
    //              （暂时【不用】扩容 —— 那是 TODO 2 的事）
    //           5. 索引器 T this[int index]：get 和 set 都要，越界抛 IndexOutOfRangeException
    //           6. 属性 int ExpandCount（只读，扩容次数计数器，本课后面要用）
    //
    // 【预期输出】（把 Add 上限控制在 4 个元素内就不会越界）
    //     --- TODO 1 ---
    //     Count = 3, Capacity = 4
    //     list[0] = 苹果, list[2] = 橘子
    //     改后 list[1] = 香蕉(改)
    //     越界访问被正确拦截：IndexOutOfRangeException
    //
    // 【提示】
    //     private T[] _items = new T[4];
    //     public int Count { get; private set; }        // ★ private set = 只有类内部能改
    //     public int Capacity => _items.Length;         // ★ 表达式体属性，等价于 { get { return ...; } }
    //
    //     public T this[int index]
    //     {
    //         get
    //         {
    //             if (index < 0 || index >= Count) throw new IndexOutOfRangeException($"下标 {index} 越界");
    //             return _items[index];
    //         }
    //         set
    //         {
    //             if (index < 0 || index >= Count) throw new IndexOutOfRangeException($"下标 {index} 越界");
    //             _items[index] = value;                // ★ 索引器的 set 里，值叫 value
    //         }
    //     }
    //
    //     ★ 越界判断为什么用 Count 而不是 Capacity？
    //       因为 Capacity 那些格子里只是 default(T)，不算数据。
    //       数组长度 4、只装了 3 个，那 list[3] 就该越界。
    //
    // 【考察点】
    //   · 泛型类的声明和泛型数组
    //   · 索引器的 get / set 两个分支
    //   · Count 用 private set 保护起来（这是封装，不是啰嗦）




    // TODO 2 ｜ 给 MyList<T> 加自动扩容 + 预分配容量
    // 【要求】在上面的 MyList<T> 里补两件事：
    //           1. 构造函数 MyList(int capacity)：按指定容量分配数组
    //              —— 这样 new MyList<int>(100_000) 就能一次分配到位
    //           2. 私有方法 Expand()：把数组变成【两倍】大，把旧数据搬过去，ExpandCount++
    //           3. 修改 Add()：Count == _items.Length 时先调 Expand()
    //
    // 【预期输出】
    //     --- TODO 2 ---
    //     加 100000 个元素（从 4 格开始翻倍）：
    //       Count = 100000, Capacity = 131072
    //       扩容次数 = 15
    //       理论次数 = 15
    //     预分配 100000：
    //       扩容次数 = 0, Capacity = 100000
    //       （容量不再翻倍，因为没有触发扩容）
    //
    // 【提示】
    //     private void Expand()
    //     {
    //         int newCapacity = _items.Length == 0 ? 4 : _items.Length * 2;
    //         T[] bigger = new T[newCapacity];
    //         Array.Copy(_items, bigger, Count);     // 只搬 Count 个（后面的是空地，不用搬）
    //         _items = bigger;
    //         ExpandCount++;
    //     }
    //
    //     ★ 也可以用 Array.Resize(ref _items, newCapacity) —— 它内部做的就是
    //       "新建数组 + Array.Copy + 换引用"，一行顶三行。两种都行。
    //     ★ 扩容次数怎么手算？4 → 8 → 16 → ... → 131072，数一数几次。
    //       100000 落在 65536 和 131072 之间，所以最后一次扩容到 131072。
    //
    // 【考察点】
    //   · 翻倍扩容：为什么是 ×2 而不是 +1（+1 就是 O(n²) 的灾难）
    //   · Capacity 永远 >= Count，且通常是 2 的幂
    //   · 预分配容量为什么能省掉全部扩容 —— 这才是 new List<T>(capacity) 的意义

    // TODO 3 ｜ 加 Insert / RemoveAt / IndexOf
    // 【要求】给 MyList<T> 补三个方法：
    //           1. void Insert(int index, T item)：在 index 处插入，后面的元素整体后移
    //              合法范围 0 .. Count（Count 就是「插到末尾」），越界抛 ArgumentOutOfRangeException
    //              注意：满了要先 Expand()
    //           2. void RemoveAt(int index)：删掉 index 处的元素，后面的整体前移
    //              合法范围 0 .. Count-1，越界抛 ArgumentOutOfRangeException
    //              ★ 最后要写 _items[Count] = default!; 把腾出来的格子清空
    //           3. int IndexOf(T item)：找到返回下标，找不到返回 -1
    //              比较用 EqualityComparer<T>.Default.Equals(...)
    //
    // 【预期输出】
    //     --- TODO 3 ---
    //     初始           : [20, 30, 40, 50]            Count = 4
    //     Insert(0, 10)  : [10, 20, 30, 40, 50]        ← 4 个元素全体后移
    //     Insert(3, 35)  : [10, 20, 30, 35, 40, 50]    ← 后 3 个后移
    //     RemoveAt(0)    : [20, 30, 35, 40, 50]        ← 4 个元素全体前移
    //     IndexOf(35)    = 2
    //     IndexOf(999)   = -1
    //     末尾删除最后一项: [20, 30, 35, 40]
    //
    // 【提示】
    //     // 插入：先把 [index, Count) 这段整体右移一格，再往空出来的位置写
    //     if (Count == _items.Length) Expand();
    //     Array.Copy(_items, index, _items, index + 1, Count - index);
    //     _items[index] = item;
    //     Count++;
    //
    //     // 删除：先把 Count 减 1，再把 [index+1, Count+1) 这段整体左移一格
    //     Count--;
    //     Array.Copy(_items, index + 1, _items, index, Count - index);
    //     _items[Count] = default!;
    //
    //     ★ Array.Copy 允许源和目标重叠（相当于 C 的 memmove），所以可以原地搬。
    //     ★ 先 Count-- 再算长度，可以少写一次 (Count - index - 1)，两个写法都对。
    //     ★ _items[Count] = default! 里的 ! 是告诉编译器"我知道这儿可能是 null"，
    //       不写它 Nullable 会警告。不清空的话，被删掉的对象会被数组一直抓着，GC 回收不了。
    //
    // 【考察点】
    //   · 亲手体验"插入/删除要搬元素"——这就是它们 O(n) 的全部原因
    //   · 插到末尾 vs 插到开头的代价差别
    //   · 陈旧引用（内存泄漏的一种）

    // TODO 4 ｜ 手写简易哈希表 MyHashMap<K,V>
    // 【要求】实现一个泛型类 MyHashMap<K, V>（约束 where K : notnull），包含：
    //           1. 私有节点类 Node { K Key; V Value; Node? Next; }
    //           2. 私有字段 Node?[] _buckets、int _count、int RehashCount
    //           3. 构造函数 MyHashMap() → 8 个桶；MyHashMap(int capacity) → 指定桶数
    //           4. 属性 Count / BucketCount（桶数）/ LoadFactor（_count / 桶数）
    //           5. 私有 int GetBucketIndex(K key, int bucketCount)：
    //              返回 key.GetHashCode() 取绝对值后对 bucketCount 取模
    //           6. void Put(K key, V value)：键已存在就覆盖 Value，不存在就新建节点插到链头
    //              ★ 插入前先判断负载因子：(Count + 1) > 桶数 * 0.75 就 Rehash()
    //           7. bool TryGetValue(K key, out V value)
    //           8. V this[K key] { get; set; }：get 找不到抛 KeyNotFoundException，set 走 Put
    //           9. bool Remove(K key)：删除节点（注意维护链表的 Next 指针）
    //          10. 私有 void Rehash()：桶数 ×2，把所有节点重新算一遍桶下标
    //          11. int MaxChainLength()：最长的一条冲突链有多长
    //
    // 【预期输出】
    //     --- TODO 4 ---
    //     存 10 个键值对后：Count = 10, 桶数 = 16, rehash = 1 次
    //       ★ 为什么是 1 次？默认 8 个桶 × 0.75 = 6，
    //         装第 7 个时 (6 + 1) > 6 成立 → 扩容到 16。
    //         之后 16 × 0.75 = 12 还没到，所以不再扩容。
    //     （逐行的表格在 Main 里 TODO 4 那一节）
    //     map["西瓜"] = 50
    //     TryGetValue("不存在") = False
    //     map["西瓜"] = 999 之后：ReadBack = 999, Count 仍然是 10（是改不是加）
    //     Remove("西瓜") = True, 之后 Count = 9
    //     Remove("西瓜") = False（第二次删就找不到了）
    //
    // 【提示】
    //     private int GetBucketIndex(K key, int bucketCount)
    //     {
    //         int hash = key.GetHashCode();
    //         return (hash & 0x7FFFFFFF) % bucketCount;   // 去掉符号位再取模
    //     }
    //     ★ 为什么不能用 Math.Abs？因为 int.MinValue 取绝对值还是负数（溢出）。
    //       hash & 0x7FFFFFFF 把最高位（符号位）清 0，永远得到非负数。
    //
    //     // Put 的骨架
    //     if ((_count + 1) > _buckets.Length * 0.75) Rehash();
    //     int index = GetBucketIndex(key, _buckets.Length);
    //     for (Node? node = _buckets[index]; node is not null; node = node.Next)
    //         if (EqualityComparer<K>.Default.Equals(node.Key, key)) { node.Value = value; return; }
    //     _buckets[index] = new Node(key, value) { Next = _buckets[index] };
    //     _count++;
    //
    //     // Rehash 的骨架（小心：不能边遍历边丢指针）
    //     Node?[] old = _buckets;
    //     _buckets = new Node?[old.Length * 2];
    //     for (int i = 0; i < old.Length; i++)
    //         for (Node? node = old[i]; node is not null; )
    //         {
    //             Node? next = node.Next;                        // ★ 先记住下一个
    //             int ni = GetBucketIndex(node.Key, _buckets.Length);
    //             node.Next = _buckets[ni];
    //             _buckets[ni] = node;
    //             node = next;
    //         }
    //     RehashCount++;
    //
    // 【考察点】
    //   · 哈希 → 桶 → 冲突链 的完整闭环
    //   · 负载因子为什么要留在 0.75 左右（太小浪费内存，太大链表变长）
    //   · rehash 为什么必须"重新算一遍"而不是"直接拷贝"

    // TODO 5 ｜ ★ 综合（本课考核）不需要新类，题目在 Main 里（往下翻）
    //   ★ 提醒：如果 TODO 5 用到了 BadPoint / GoodPoint 这两个小类，
    //     记得把它们也写在 Lesson25Practice 类的【外面】，和 TODO 1~4 的类放一起。

    class Lesson25Practice
    {
        // ================================================================
        // ↓↓↓ 本课的辅助方法写在这里 ↓↓↓
        // ================================================================

        /// <summary>
        /// 把数组拼成一行定宽文本，方便和说明对齐（TODO 3 会用）。
        /// </summary>
        static string FormatRow(int[] values, int width)
        {
            string text = string.Join(", ", values);
            return text.Length >= width ? text : text.PadRight(width);
        }

        /// <summary>
        /// 跑多轮取最快的一次（TODO 5 会用）。
        /// ★ 为什么要跑多轮：第一次调用包含 JIT 编译开销（第 17 课），
        ///   取最小值比取平均值更能排除这种干扰。
        /// </summary>
        static double TimeBest(Action action, int rounds)
        {
            double best = double.MaxValue;
            for (int r = 0; r < rounds; r++)
            {
                var sw = Stopwatch.StartNew();
                action();
                sw.Stop();
                if (sw.Elapsed.TotalMilliseconds < best)
                {
                    best = sw.Elapsed.TotalMilliseconds;
                }
            }
            return best;
        }

        static void Main()
        {
            Console.WriteLine("===== 第 25 课练习：集合的内部实现 =====");
            Console.WriteLine();

            // ================================================================
            // TODO 1 ｜ 测试你手写的 MyList<T>
            // ================================================================
            // 【要求】
            //   1. new 一个 MyList<string>()
            //   2. Add 三个水果（苹果 / 香蕉 / 橘子）
            //   3. 打印 Count 和 Capacity，确认 Capacity 是 4 而 Count 是 3
            //   4. 用索引器读 list[0] 和 list[2]
            //   5. 用索引器【写】list[1] = "香蕉(改)"，再读出来验证
            //   6. 用 try / catch 故意访问 list[99]，打印异常类型名
            //
            // 【预期输出】
            //     Count = 3, Capacity = 4
            //     list[0] = 苹果, list[2] = 橘子
            //     改后 list[1] = 香蕉(改)
            //     越界访问被正确拦截：IndexOutOfRangeException
            //
            // 【提示】
            //     var list = new MyList<string>();
            //     list.Add("苹果"); list.Add("香蕉"); list.Add("橘子");
            //     Console.WriteLine($"Count = {list.Count}, Capacity = {list.Capacity}");
            //     Console.WriteLine($"list[0] = {list[0]}, list[2] = {list[2]}");
            //
            //     list[1] = "香蕉(改)";
            //     Console.WriteLine($"改后 list[1] = {list[1]}");
            //
            //     try { _ = list[99]; }
            //     catch (IndexOutOfRangeException ex)
            //     {
            //         Console.WriteLine($"越界访问被正确拦截：{ex.GetType().Name}");
            //     }
            //
            //   ★ 抄完提示也要自己检查一遍：MyList<T> 里 Count 是不是只读的？
            //     如果外面能写 list.Count = 99，那就说明封装没做到位。
            //
            // 【考察点】
            //   · 泛型类的实例化、索引器读写、越界异常

            Console.WriteLine("--- TODO 1 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 2 ｜ 扩容实测：不预分配 vs 预分配
            // ================================================================
            // 【要求】
            //   1. 用不带容量的构造函数建一个 MyList<int>()，
            //      加 100000 个元素，打印 Count / Capacity / ExpandCount
            //   2. 手算理论扩容次数（4 一路翻倍到 >= 100000 要几次），对照
            //   3. 用 new MyList<int>(100000) 再建一个，加同样多元素，
            //      打印 ExpandCount，确认是 0
            //   4. 打印第二个列表的 Capacity，和第一个比较
            //
            // 【预期输出】
            //     加 100000 个元素（从 4 格开始翻倍）：
            //       Count = 100000, Capacity = 131072
            //       扩容次数 = 15
            //       理论次数 = 15
            //     预分配 100000：
            //       扩容次数 = 0, Capacity = 100000
            //       （容量不再翻倍，因为没有触发扩容）
            //
            // 【提示】
            //     var grow = new MyList<int>();
            //     for (int i = 0; i < 100_000; i++) grow.Add(i);
            //     Console.WriteLine($"  Count = {grow.Count}, Capacity = {grow.Capacity}");
            //     Console.WriteLine($"  扩容次数 = {grow.ExpandCount}");
            //
            //     // 手算：4 → 8 → 16 → ... 一直乘到 >= 100000 为止，数乘了几次
            //     int simulated = 4, times = 0;
            //     while (simulated < 100_000) { simulated *= 2; times++; }
            //     Console.WriteLine($"  理论次数 = {times}（4 → {simulated}）");
            //
            //     var preset = new MyList<int>(100_000);
            //     for (int i = 0; i < 100_000; i++) preset.Add(i);
            //     Console.WriteLine($"  扩容次数 = {preset.ExpandCount}, Capacity = {preset.Capacity}");
            //
            // 【考察点】
            //   · Capacity 为什么总是 2 的幂、为什么总是 >= Count
            //   · 预分配容量的真正收益：不是"更快"，而是"不产生扩容垃圾"
            //   · 自己动手算一遍，比背"翻倍扩容"四个字有用得多

            Console.WriteLine("--- TODO 2 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 3 ｜ Insert / RemoveAt / IndexOf：亲手感受"搬家"
            // ================================================================
            // 【要求】
            //   1. new 一个 MyList<int>()，依次 Add 20 30 40 50
            //   2. 打印初始内容（用 FormatRow(list.GetRawArray()[..list.Count], 24) 对齐）
            //   3. Insert(0, 10) 打印 → 观察全体后移
            //   4. Insert(3, 35) 打印 → 观察部分后移
            //   5. RemoveAt(0) 打印 → 观察全体前移
            //   6. 打印 IndexOf(35) 和 IndexOf(999)
            //   7. RemoveAt(Count - 1) 删掉最后一项，打印
            //
            // 【预期输出】
            //     初始           : [20, 30, 40, 50]            Count = 4
            //     Insert(0, 10)  : [10, 20, 30, 40, 50]        ← 4 个元素全体后移
            //     Insert(3, 35)  : [10, 20, 30, 35, 40, 50]    ← 后 3 个后移
            //     RemoveAt(0)    : [20, 30, 35, 40, 50]        ← 4 个元素全体前移
            //     IndexOf(35)    = 2
            //     IndexOf(999)   = -1
            //     末尾删除最后一项: [20, 30, 35, 40]
            //
            // 【提示】
            //     var list = new MyList<int>();
            //     for (int i = 2; i <= 5; i++) list.Add(i * 10);       // 20 30 40 50
            //     Console.WriteLine($"  初始           : [{FormatRow(list.GetRawArray()[..list.Count], 24)}]  Count = {list.Count}");
            //
            //     list.Insert(0, 10);
            //     Console.WriteLine($"  Insert(0, 10)  : [{FormatRow(list.GetRawArray()[..list.Count], 24)}]  ← 4 个元素全体后移");
            //
            //     ★ list.GetRawArray()[..list.Count] 是 C# 8 的范围语法（第 03 课讲过 ^1），
            //       [..n] 意思是"取前 n 个"，得到一个刚好是数据部分的数组。
            //       → 需要你在 MyList<T> 里加一个 T[] GetRawArray() => _items;（只为练习输出方便）
            //
            //     ★ 想验证"搬家"真的发生了？在 Insert 里临时插一句
            //       Console.WriteLine($"要搬 {Count - index} 个"); 然后删掉。
            //
            // 【考察点】
            //   · 亲手看到"数组里的元素在挪动"
            //   · IndexOf 找不到返回 -1，而 RemoveAt 越界是抛异常 —— 两种风格别混
            //   · RemoveAt 末尾 vs 开头的代价差别（一个都不用搬 vs 搬 n 个）

            Console.WriteLine("--- TODO 3 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 4 ｜ 测试你手写的 MyHashMap<K,V>
            // ================================================================
            // 【要求】
            //   1. new MyHashMap<string, int>()
            //   2. 依次放入 10 个键值对：“苹果”=10、“香蕉”=20 ... “枣”=100
            //      （用数组 string[] fruits 和 for 循环，别手写 10 行）
            //   3. 每放一个就打印一行：Count / 桶数 / 负载因子 / 最长链
            //   4. 存完后打印：Count、桶数、RehashCount
            //   5. 读 map["西瓜"]，并用 TryGetValue 测一个不存在的键
            //   6. map["西瓜"] = 999 覆盖，再读出来，确认 Count 没变
            //   7. Remove("西瓜") 两次，第二次应该是 False
            //
            // 【预期输出】
            //        Count  桶数  负载因子  最长链
            //       ------  ----  --------  ------
            //            1     8      0.12       1
            //            2     8      0.25       1
            //            3     8      0.38       1
            //            4     8      0.50       1
            //            5     8      0.62       1
            //            6     8      0.75       1   ← 6 = 8 × 0.75，刚好卡在上限（还没触发）
            //            7    16      0.44       2   ← ★ 装第 7 个时触发了 rehash！
            //            8    16      0.50       2
            //            9    16      0.56       2
            //           10    16      0.62       2
            //     存完 10 个键值对：Count = 10, 桶数 = 16, rehash = 1 次
            //     map["西瓜"] = 50
            //     TryGetValue("不存在") = False
            //     map["西瓜"] = 999 之后：ReadBack = 999, Count 仍然是 10（是改不是加）
            //     Remove("西瓜") = True, 之后 Count = 9
            //     Remove("西瓜") = False（第二次删就找不到了）
            //     ------------------------------------------------------------------
            //     ★ 为什么是第 7 个触发？把每一次的判断都算一遍（这是本课最值得手算的一步）：
            //         _count=0 时：(0+1) > 8 × 0.75  →  1 > 6  → False
            //         _count=1 时：(1+1) > 6         →  2 > 6  → False
            //         _count=3 时：(3+1) > 6         →  4 > 6  → False
            //         _count=5 时：(5+1) > 6         →  6 > 6  → False   ← 第 6 个进来时还不扩
            //         _count=6 时：(6+1) > 6         →  7 > 6  → True    ← ★ 第 7 个进来，扩容！
            //       ★ 结论："负载因子上限 0.75"的真实含义是【8 个桶里最多放 6 个】，
            //         而不是"能放 8 个"。这个 +1 的位置（写在插入【前】）是本课最容易写错的地方。
            //     ★ 上表的桶数/最长链还取决于字符串的真实哈希码，你跑出来可能略有不同；
            //       要看的【趋势】是：桶数 8→16 一跳，负载因子立刻从 0.75 掉到 0.44
            //       （分母变大了），这就是 rehash 存在的意义。
            //
            // 【提示】
            //     var map = new MyHashMap<string, int>();
            //     string[] fruits = { "苹果", "香蕉", "橘子", "葡萄", "西瓜", "芒果", "桃子", "梨", "杏", "枣" };
            //     for (int i = 0; i < fruits.Length; i++)
            //     {
            //         map.Put(fruits[i], (i + 1) * 10);
            //         Console.WriteLine($"     {map.Count,6}{map.BucketCount,6}{map.LoadFactor,10:F2}{map.MaxChainLength(),8}");
            //     }
            //     Console.WriteLine($"存完 {map.Count} 个键值对：Count = {map.Count}, 桶数 = {map.BucketCount}, rehash = {map.RehashCount} 次");
            //
            //     if (map.TryGetValue("不存在", out int v)) { /* 用 v */ }
            //     else Console.WriteLine("TryGetValue(\"不存在\") = False");
            //
            // 【考察点】
            //   · rehash 的触发时机（负载因子超过 0.75 的那一刻）
            //   · 「改」和「加」的区别：Put 一个已存在的键，Count 不涨
            //   · 桶数是 2 的幂这件事不是偶然（和取模的性能有关）

            Console.WriteLine("--- TODO 4 ---");

            // ↓↓↓ 在这里写你的代码 ↓↓↓




            Console.WriteLine();

            // ================================================================
            // TODO 5 ｜ ★ 综合（本课考核）：复杂度实测 + 哈希契约
            // ================================================================
            // 【要求】做两件事：
            //
            // 【A】复杂度实测：10 万个元素里查 1000 次
            //   1. 建 List<int> 和 Dictionary<int, int>，各装 10 万个元素
            //      （List 里放偶数 0,2,4,...；Dictionary 的键也放这些偶数）
            //   2. 用固定种子 new Random(12345) 生成 1000 个查询值，
            //      范围 0 .. 199999（一半能查到，一半查不到）
            //   3. 用 TimeBest 分别测 List.Contains 和 Dictionary.ContainsKey，各跑 5 轮
            //   4. 打印两个耗时、每次查询的微秒数、以及倍数
            //
            // 【B】哈希契约反面教材
            //   5. 写一个类 BadPoint（X、Y 两个属性），【只重写 Equals】按值比较，
            //      【故意不重写 GetHashCode】
            //   6. 两个 X=1, Y=2 的实例 a 和 b：打印 a.Equals(b) 的结果
            //   7. 把它们放进一个 HashSet<BadPoint>，打印 set.Contains(b) 的结果
            //      —— 应该是 False！（明明 Equals 说是相等的）
            //   8. 再 new 一个 HashSet，把 a、b 都 Add 进去，打印 Count
            //      —— 应该是 2！（重复元素没被去重）
            //   9. 【修好它】再写一个 GoodPoint，Equals 和 GetHashCode 都重写
            //      （GetHashCode 用 HashCode.Combine(X, Y)），重复第 6~8 步，
            //      这次 Contains 应该是 True、Count 应该是 1
            //
            // 【预期输出】
            //     --- TODO 5 ---
            //     【A】10 万元素里查找 1000 次：
            //       List.Contains        =  6.80 ms   （每次 6.800 μs）
            //       Dictionary.ContainsKey = 0.02 ms   （每次 0.020 μs）
            //       倍数 = 340 倍
            //     【B】哈希契约：
            //       badA.Equals(badB)       = True
            //       badSet.Contains(badB)   = False   ← ★ 违反契约，找不到！
            //       badSet.Add(badB) 后 Count = 2      ← ★ 重复元素进来了
            //       goodSet.Contains(goodB) = True    ← 修好后正常
            //       goodSet Count           = 1
            //     ★ 你的数字会和上面不同（CPU/Release/Debug 都影响），
            //       要看的是"倍数是多少"和"False/2 这两个反直觉的结果有没有复现"。
            //
            // 【提示】
            //     // A 部分
            //     var list = new List<int>(100_000);
            //     var dict = new Dictionary<int, int>(100_000);
            //     for (int i = 0; i < 100_000; i++) { list.Add(i * 2); dict[i * 2] = i; }
            //
            //     var random = new Random(12345);           // ★ 固定种子，结果可复现
            //     int[] queries = new int[1000];
            //     for (int i = 0; i < queries.Length; i++) queries[i] = random.Next(0, 200_000);
            //
            //     double msList = TimeBest(() =>
            //     {
            //         int hits = 0;
            //         foreach (int q in queries) if (list.Contains(q)) hits++;
            //         GC.KeepAlive(hits);                   // ★ 防止 JIT 把整个循环优化掉
            //     }, 5);
            //
            //     Console.WriteLine($"  List.Contains          = {msList,6:F2} ms   （每次 {msList * 1000 / queries.Length:F3} μs）");
            //     Console.WriteLine($"  倍数 = {msList / msDict:F0} 倍");
            //
            //     // B 部分（BadPoint / GoodPoint 两个类写在 Lesson25Practice 类的外面）
            //     private sealed class BadPoint
            //     {
            //         public int X { get; init; }
            //         public int Y { get; init; }
            //         public override bool Equals(object? obj)
            //             => obj is BadPoint o && o.X == X && o.Y == Y;
            //         // ★ 这里【故意不写】GetHashCode
            //     }
            //
            //     private sealed class GoodPoint
            //     {
            //         public int X { get; init; }
            //         public int Y { get; init; }
            //         public override bool Equals(object? obj)
            //             => obj is GoodPoint o && o.X == X && o.Y == Y;
            //         public override int GetHashCode() => HashCode.Combine(X, Y);   // ★ 补上它
            //     }
            //
            //     ★ 为什么 badSet.Contains(badB) 是 False？
            //       哈希集合的查找分两步：① 按 GetHashCode 找到桶 ② 在桶里比 Equals。
            //       没重写 GetHashCode 时，两个对象的哈希码来自【对象地址】，必然不同，
            //       于是它们落在不同的桶里 —— 第 ② 步压根没机会执行。
            //
            // 【考察点】
            //   · 用实测数据说话，而不是背"O(1) 比 O(n) 快"
            //   · 固定随机种子让测试可复现（这是工程习惯，不是小题大做）
            //   · ⚠️ 哈希契约的违反后果 —— 本课最该记住的一条
            //   · 这一题同时用到了 TODO 1~4 的全部知识：能独立做完 = 本课通过

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
