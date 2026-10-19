using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LessonPractice
{
    // ============================================================================
    //  第 25 课 · 示例：集合的内部实现（手写 MyList 和哈希表）
    //
    //  第 09 课讲的是"集合怎么【用】"（List/Dictionary/HashSet 怎么选）。
    //  这一课换个角度：把 List 和 Dictionary 的【壳子拆开】，自己动手造一遍。
    //
    //  ★ 本示例分十段，建议按顺序读，每段先看输出再回头看代码：
    //      一、数组的天花板          —— 为什么需要动态数组
    //      二、手写 MyList<T>        —— 数组 + count，索引器
    //      三、Capacity vs Count     —— 翻倍扩容的实测
    //      四、Insert / RemoveAt     —— 为什么它们是 O(n)
    //      五、哈希 → 桶 → 冲突链     —— 哈希表的第一原理
    //      六、手写 MyHashMap<K,V>   —— 负载因子 + rehash
    //      七、复杂度实测            —— List 线性查找 vs Dictionary 哈希查找
    //      八、⚠️ 哈希契约           —— 只重写 Equals 的灾难现场
    //      九、自己动手改坏它         —— 把扩容倍数改成 1.5 会怎样
    //      十、小结
    //
    //  ★ 运行方式：
    //      1. 打开 第25课.sln，把 Lesson25Demo 设为启动项目，Ctrl + F5
    //      2. 或者命令行：dotnet run --project Lesson25Demo
    //
    //  ★ 提醒：性能数字每次都不一样（CPU、Debug/Release、后台程序都有影响）。
    //    要看的是【倍数关系】，不是小数点后的绝对值。
    // ============================================================================
    class Lesson25Demo
    {
        // ════════════════════════════════════════════════════════════════════
        //  第一部分：手写 MyList<T> —— 把 List<T> 的壳子拆开
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// 手写的动态数组。功能对标 List&lt;T&gt; 的核心子集。
        ///
        /// ★ 内部只有两样东西：
        ///     _items —— 真正装数据的数组（它的长度就是 Capacity）
        ///     Count  —— 实际放了几个元素（属性背后就是一个 int 字段）
        ///
        /// ★ 关键认识："_items.Length" 和 "Count" 是两个不同的数字。
        ///   数组有 8 个格子，不代表里面装了 8 个元素。
        ///   元素只住在下标 0 .. Count-1 这段区间里，后面的格子是"预留的空地"。
        /// </summary>
        private sealed class MyList<T>
        {
            // ★ 泛型数组 new T[4] 是合法的。元素默认是 default(T)：
            //   值类型是 0/false，引用类型是 null。
            //   ★ 注意：字段的类型是 T[]（数组），不是裸的 T，
            //     所以 Nullable 不会报 CS8618（"字段未初始化"）——
            //     报的只会是"数组元素可能是 null"，而那不是错误。
            private T[] _items = new T[4];

            /// <summary>列表里实际有多少个元素。</summary>
            public int Count { get; private set; }

            /// <summary>数组现在有多大（能装多少个元素而不扩容）。</summary>
            public int Capacity => _items.Length;

            /// <summary>扩容发生过几次 —— List&lt;T&gt; 没有这个属性，是给我们观察用的。</summary>
            public int ExpandCount { get; private set; }

            /// <summary>默认构造：先开 4 个格子。</summary>
            public MyList() : this(4) { }          // ★ 构造函数链：委托给下面那个

            /// <summary>
            /// 按指定容量【预分配】。
            /// ★ 注意这里必须重新给 _items 赋值 ——
            ///   字段初始化器（上面的 new T[4]）会先跑，构造函数后跑，
            ///   不覆盖的话 new MyList&lt;int&gt;(100_000) 会得到 Capacity = 4（本课最容易踩的坑之一）。
            /// </summary>
            public MyList(int capacity)
            {
                if (capacity < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(capacity), "容量不能是负数");
                }
                _items = new T[capacity];
            }

            /// <summary>
            /// 索引器：让 MyList 也能像数组一样用 list[i] 读写。
            /// ★ get 和 set 分开写，就可以只提供其中一个。
            ///   只读集合只给 get；我们的列表可读可写，所以两个都有。
            /// </summary>
            public T this[int index]
            {
                get
                {
                    // ★ 越界的表现要和数组一致：抛 IndexOutOfRangeException。
                    //   List<T> 实际抛的是 ArgumentOutOfRangeException，
                    //   这里为了和"数组"对照，故意用同一个异常类型。
                    if (index < 0 || index >= Count)
                    {
                        throw new IndexOutOfRangeException(
                            $"下标 {index} 越界（当前 Count = {Count}）");
                    }
                    return _items[index];
                }
                set
                {
                    if (index < 0 || index >= Count)
                    {
                        throw new IndexOutOfRangeException(
                            $"下标 {index} 越界（当前 Count = {Count}）");
                    }
                    _items[index] = value;
                }
            }

            /// <summary>
            /// 添加一个元素。分摊成本 O(1)。
            /// ★ 注意判断条件是 Count == _items.Length，
            ///   不是 Count == Capacity —— 它俩此刻是同一个数字，但前者更直接地表达了
            ///   "装元素的格子用完了"。
            /// </summary>
            public void Add(T item)
            {
                if (Count == _items.Length)
                {
                    Expand();
                }
                _items[Count] = item;
                Count++;
            }

            /// <summary>
            /// 扩容：分配一个【两倍】大的新数组，把旧数据搬过去。
            ///
            /// ★ 这是本课最核心的一行策略：翻倍。
            ///   为什么不是 +1？（那样每加一个元素就搬一次，太慢）
            ///   为什么不是 ×10？（那样浪费内存，而且搬一次要更久）
            ///   翻倍是"总搬运量"和"浪费内存"之间最常用的折中。
            /// </summary>
            private void Expand()
            {
                int newCapacity = _items.Length == 0 ? 4 : _items.Length * 2;

                // ★ 想亲手试试"改成 1.5 倍"？把上面这行换成：
                //      int newCapacity = _items.Length == 0 ? 4 : _items.Length + _items.Length / 2;
                //   然后看第三段输出的扩容次数变化。
                T[] bigger = new T[newCapacity];

                // Array.Copy(源, 目标, 个数) —— 底层是一段很快的内存拷贝
                Array.Copy(_items, bigger, Count);

                _items = bigger;
                ExpandCount++;
            }

            /// <summary>
            /// 在指定位置插入一个元素。O(n)。
            /// ★ 为什么慢？因为后面的元素必须【整体往后挪一格】。
            ///   插在开头最惨（要挪 n 个），插在末尾最快（一个都不用挪）。
            /// </summary>
            public void Insert(int index, T item)
            {
                if (index < 0 || index > Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index),
                        $"插入位置 {index} 越界（合法范围 0 .. {Count}）");
                }

                if (Count == _items.Length)
                {
                    Expand();
                }

                // Array.Copy 自带"重叠区域正确搬运"的能力（相当于 C 的 memmove），
                // 所以可以直接把 [index, Count) 这一段整体右移一格。
                Array.Copy(_items, index, _items, index + 1, Count - index);

                _items[index] = item;
                Count++;
            }

            /// <summary>
            /// 删除指定位置的元素，后面的元素整体往前挪。O(n)。
            /// ★ 注意最后那一步 _items[Count] = default! ——
            ///   把腾出来的格子清成 null。不清的话，那个引用会一直留在数组里，
            ///   GC 就没法回收它指向的对象 → 这就是"内存泄漏"的一种（叫"陈旧引用"）。
            ///   .NET 的 List&lt;T&gt;.RemoveAt 内部做的正是这件事。
            /// </summary>
            public void RemoveAt(int index)
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index),
                        $"删除位置 {index} 越界（合法范围 0 .. {Count - 1}）");
                }

                Count--;
                Array.Copy(_items, index + 1, _items, index, Count - index);
                _items[Count] = default!;
            }

            /// <summary>按值查找，返回下标；找不到返回 -1。O(n)。</summary>
            public int IndexOf(T item)
            {
                // ★ 用 EqualityComparer<T>.Default 而不是 object.Equals：
                //   它对值类型不装箱，是泛型集合的标准做法（第 15 课的"零装箱"）。
                for (int i = 0; i < Count; i++)
                {
                    if (EqualityComparer<T>.Default.Equals(_items[i], item))
                    {
                        return i;
                    }
                }
                return -1;
            }

            /// <summary>是否包含某个值。本质就是 IndexOf 找没找到。</summary>
            public bool Contains(T item) => IndexOf(item) >= 0;

            /// <summary>清空。★ 注意容量【不还回去】—— List&lt;T&gt;.Clear 也是这样。</summary>
            public void Clear()
            {
                Array.Clear(_items, 0, Count);
                Count = 0;
            }

            /// <summary>
            /// 把内部数组直接暴露出来（只为第七段的性能测试和格式化输出用）。
            /// ★ 真实的 List&lt;T&gt; 不会给这个方法 —— 给了就等于把内部数组交给外面乱改。
            /// </summary>
            public T[] GetRawArray() => _items;
        }

        // ════════════════════════════════════════════════════════════════════
        //  第二部分：手写 MyHashMap<K, V> —— 哈希 → 桶 → 冲突链
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// 手写哈希表。功能对标 Dictionary&lt;K,V&gt; 的核心子集。
        ///
        /// ★ 三个关键数字：
        ///     _buckets.Length —— 桶的数量（Capacity）
        ///     _count          —— 存了多少个键值对
        ///     LoadFactor      —— _count / _buckets.Length，超过阈值就 rehash
        ///
        /// ★ 为什么要链表？因为"哈希取模"必然会把不同的 key 映射到同一个桶。
        ///   同一个桶里的多个键值对，就靠 Node.Next 串成一条链，逐个比较查找。
        ///   这个办法叫【链地址法】（separate chaining）。
        /// </summary>
        private sealed class MyHashMap<K, V> where K : notnull
        {
            /// <summary>桶数组：每个格子是"一条链的头节点"，空桶就是 null。</summary>
            private Node?[] _buckets;

            private int _count;

            /// <summary>链上的一个节点：键 + 值 + 下一个节点。</summary>
            private sealed class Node
            {
                public K Key;
                public V Value;
                public Node? Next;

                public Node(K key, V value)
                {
                    Key = key;
                    Value = value;
                }
            }

            /// <summary>负载因子阈值：装到 75% 就扩容。</summary>
            public const double LoadFactorLimit = 0.75;

            /// <summary>rehash 发生过几次。</summary>
            public int RehashCount { get; private set; }

            /// <summary>比较键用的比较器：值类型不装箱，null 语义也交给它处理。</summary>
            private static readonly EqualityComparer<K> Comparer = EqualityComparer<K>.Default;

            public MyHashMap() : this(8) { }

            public MyHashMap(int capacity)
            {
                _buckets = new Node?[capacity < 1 ? 1 : capacity];
            }

            public int Count => _count;
            public int BucketCount => _buckets.Length;
            public double LoadFactor => (double)_count / _buckets.Length;

            /// <summary>
            /// 哈希函数：把任意键变成一个【桶下标】。
            /// ★ 两步：
            ///     ① key.GetHashCode() —— 拿到一个 int 哈希码（可能很大、可能是负数）
            ///     ② 对桶数取模 —— 把它压进 0 .. BucketCount-1
            ///   ★ (hash &amp; 0x7FFFFFFF) 是为了先去掉符号位，
            ///     否则负数取模会得到负下标，直接数组越界。
            /// </summary>
            private int GetBucketIndex(K key, int bucketCount)
            {
                int hash = key.GetHashCode();
                return (hash & 0x7FFFFFFF) % bucketCount;
            }

            /// <summary>添加或更新。平均 O(1)，最坏 O(n)（所有键挤在一条链上）。</summary>
            public void Put(K key, V value)
            {
                // ★ 先看"装了这一对之后会不会超标"。负载因子检查必须在插入【前】做，
                //   否则表会短暂地超过阈值。
                if ((_count + 1) > _buckets.Length * LoadFactorLimit)
                {
                    Rehash();
                }

                int index = GetBucketIndex(key, _buckets.Length);

                // 沿着这条链找：键已经存在就覆盖值（这就是 map[k] = v 的语义）
                for (Node? node = _buckets[index]; node is not null; node = node.Next)
                {
                    if (Comparer.Equals(node.Key, key))
                    {
                        node.Value = value;
                        return;                       // ★ 覆盖不算新增，_count 不变
                    }
                }

                // 没找到 → 新建节点，插到链的【头部】
                // ★ 为什么插头部？因为只用一步，O(1)。插尾部要先走完整条链。
                _buckets[index] = new Node(key, value) { Next = _buckets[index] };
                _count++;
            }

            /// <summary>安全读取。找到返回 true 并把值放到 value；找不到返回 false。</summary>
            public bool TryGetValue(K key, out V value)
            {
                for (Node? node = _buckets[GetBucketIndex(key, _buckets.Length)];
                     node is not null;
                     node = node.Next)
                {
                    if (Comparer.Equals(node.Key, key))
                    {
                        value = node.Value;
                        return true;
                    }
                }

                // ★ 返回 false 时也必须给 value 赋值（编译器强制要求），
                //   所以只能给一个没有意义的 default 值 —— 这正是"调用方必须先看返回值"的原因。
                value = default!;
                return false;
            }

            /// <summary>索引器，语义和 Dictionary 的 [] 一样：读不到就抛异常。</summary>
            public V this[K key]
            {
                get
                {
                    if (TryGetValue(key, out V? found))
                    {
                        return found;
                    }
                    throw new KeyNotFoundException($"键不存在：{key}");
                }
                set => Put(key, value);
            }

            /// <summary>删除。平均 O(1)。★ 删除要维护链表的指针，所以必须记住"前一个节点"。</summary>
            public bool Remove(K key)
            {
                int index = GetBucketIndex(key, _buckets.Length);
                Node? previous = null;

                for (Node? node = _buckets[index]; node is not null; node = node.Next)
                {
                    if (Comparer.Equals(node.Key, key))
                    {
                        if (previous is null)
                        {
                            // 要删的是链头：直接把链头换成下一个
                            _buckets[index] = node.Next;
                        }
                        else
                        {
                            // 要删的是中间/末尾：让前一个节点跳过它
                            previous.Next = node.Next;
                        }
                        _count--;
                        return true;
                    }
                    previous = node;
                }
                return false;
            }

            public bool ContainsKey(K key) => TryGetValue(key, out _);

            /// <summary>
            /// 扩容 + rehash。
            /// ★ rehash 是"重新洗牌"：桶数变了，所有键的 哈希%桶数 都变了，
            ///   所以不能简单拷贝，必须把每个节点拿出来重新算一遍桶下标。
            /// ★ 这也是为什么扩容很贵（O(n)）—— 但因为它发生得越来越少
            ///   （8→16→32→...），平均到每次 Put 上只有 O(1)，这叫【分摊分析】。
            /// </summary>
            private void Rehash()
            {
                Node?[] old = _buckets;
                _buckets = new Node?[old.Length * 2];

                for (int i = 0; i < old.Length; i++)
                {
                    Node? node = old[i];
                    while (node is not null)
                    {
                        // ★ 先记住 next：下面要改写 node.Next，不能边遍历边丢指针
                        Node? next = node.Next;

                        int newIndex = GetBucketIndex(node.Key, _buckets.Length);
                        node.Next = _buckets[newIndex];
                        _buckets[newIndex] = node;

                        node = next;
                    }
                }

                RehashCount++;
            }

            /// <summary>最长的一条链有多长 —— 冲突有多严重的直接体现。</summary>
            public int MaxChainLength()
            {
                int max = 0;
                for (int i = 0; i < _buckets.Length; i++)
                {
                    int length = 0;
                    for (Node? node = _buckets[i]; node is not null; node = node.Next)
                    {
                        length++;
                    }
                    if (length > max)
                    {
                        max = length;
                    }
                }
                return max;
            }

            /// <summary>有多少个桶是空的 —— 空桶越多，说明哈希分布越不均匀。</summary>
            public int UsedBucketCount()
            {
                int used = 0;
                for (int i = 0; i < _buckets.Length; i++)
                {
                    if (_buckets[i] is not null)
                    {
                        used++;
                    }
                }
                return used;
            }

            /// <summary>把全部键值对取出来（只为演示用）。</summary>
            public List<(K Key, V Value)> GetEntries()
            {
                var list = new List<(K, V)>(_count);
                for (int i = 0; i < _buckets.Length; i++)
                {
                    for (Node? node = _buckets[i]; node is not null; node = node.Next)
                    {
                        list.Add((node.Key, node.Value));
                    }
                }
                return list;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  第三部分：第八段用来演示"哈希契约"的几个类型
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// ⚠️ 反面教材：只重写了 Equals，没重写 GetHashCode。
        /// 这是最常见的错误写法（尤其 IDE 只帮你生成了一半的时候）。
        /// </summary>
        private sealed class BadPoint
        {
            public int X { get; init; }
            public int Y { get; init; }

            public override bool Equals(object? obj)
            {
                // 按"值"比较：坐标相同就算相等
                return obj is BadPoint other && other.X == X && other.Y == Y;
            }
            // ★ 故意不写 GetHashCode() → 用 object 的默认实现 → 基于对象地址
        }

        /// <summary>✅ 正确写法：Equals 和 GetHashCode 一起重写，且用了同样的字段。</summary>
        private sealed class GoodPoint
        {
            public int X { get; init; }
            public int Y { get; init; }

            public override bool Equals(object? obj)
            {
                return obj is GoodPoint other && other.X == X && other.Y == Y;
            }

            public override int GetHashCode()
            {
                // ★ 要点：参与 Equals 的字段，必须【全部】参与 GetHashCode。
                //   HashCode.Combine 是 .NET 提供的标准组合工具（第 13 课见过它的兄弟）。
                return HashCode.Combine(X, Y);
            }
        }

        /// <summary>
        /// 另一个极端：GetHashCode 返回常数。
        /// ★ 注意 —— 这【不违反】契约，它是合法的！
        ///   代价是所有键都挤进同一个桶，哈希表退化成链表，查找变成 O(n)。
        ///   "合法但灾难"，这是理解负载因子最好的例子。
        /// </summary>
        private sealed class ConstantHashKey
        {
            public int Id { get; init; }

            public override int GetHashCode() => 42;          // 所有实例的哈希码都一样

            public override bool Equals(object? obj)
            {
                return obj is ConstantHashKey other && other.Id == Id;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  辅助方法
        // ════════════════════════════════════════════════════════════════════

        private static long AllocatedBytes() => GC.GetAllocatedBytesForCurrentThread();

        private static string FormatBytes(long bytes) => $"{bytes / 1024.0 / 1024.0:F2} MB";

        /// <summary>
        /// 把数组拼成一行定宽文本，方便和后面的说明对齐。
        /// ★ 为什么要先拼好再放进插值字符串？
        ///   因为 $"文本{值,-24}" 里的 ,-24 只对【值本身】补空格，
        ///   解决不了"文本长度不固定"的对齐问题。自己先补齐最省事。
        /// </summary>
        private static string FormatRow(int[] values, int width)
        {
            string text = string.Join(", ", values);
            return text.Length >= width ? text : text.PadRight(width);
        }

        /// <summary>只测时间，顺便返回一个校验值防止编译器把整个循环优化掉。</summary>
        private static double TimeOnce(Action action)
        {
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }

        /// <summary>跑多轮取最好成绩（预热 + 取最小值，比取平均更稳定）。</summary>
        private static double TimeBest(Action action, int rounds)
        {
            double best = double.MaxValue;
            for (int r = 0; r < rounds; r++)
            {
                double ms = TimeOnce(action);
                if (ms < best)
                {
                    best = ms;
                }
            }
            return best;
        }

        /// <summary>
        /// 让编译器相信"方法里可能发生任何事"。
        /// ★ 为什么需要它：JIT 如果发现某个变量算完从来没用，可能把整段循环删掉，
        ///   于是你测到的是 0 ms —— 这个坑在第 17 课就提过。
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Sink(object? value) => GC.KeepAlive(value);

        // ════════════════════════════════════════════════════════════════════
        //  Main
        // ════════════════════════════════════════════════════════════════════
        static void Main()
        {
            Console.WriteLine("===== 第 25 课：集合的内部实现 =====");
            Console.WriteLine();
            Console.WriteLine($"  运行时版本 : {Environment.Version}");
            Console.WriteLine($"  64 位进程  : {Environment.Is64BitProcess}");
            Console.WriteLine("  ★ 性能数字每台机器都不同，看【倍数关系】就好。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 一、数组的天花板
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 一、数组的天花板 ----------");
            Console.WriteLine();

            // ★ 数组的长度在创建那一刻就写死了（长度存在对象头里，是只读的）
            int[] fixedArray = new int[3];
            fixedArray[0] = 10;
            fixedArray[1] = 20;
            fixedArray[2] = 30;
            Console.WriteLine($"  int[] 长度 = {fixedArray.Length}，装满了。");
            Console.WriteLine("  想加第 4 个？只能自己新建大数组、搬数据、换引用：");
            Console.WriteLine();

            int[] bigger = new int[4];
            Array.Copy(fixedArray, bigger, fixedArray.Length);   // 这一步就是"搬运"
            bigger[3] = 40;
            fixedArray = bigger;

            Console.WriteLine($"    新数组内容 : [{string.Join(", ", fixedArray)}]");
            Console.WriteLine("  ★ 这段\"新建 + 搬运 + 换引用\"的活儿，就是 List<T> 每天在做的事。");
            Console.WriteLine("    区别只是它会在内部自动帮你做，而且挑了个聪明的时机（翻倍时才做）。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 二、手写 MyList<T>
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 二、手写 MyList<T> ----------");
            Console.WriteLine();

            var list = new MyList<string>();
            list.Add("苹果");
            list.Add("香蕉");
            list.Add("橘子");

            Console.WriteLine($"  list.Add 三次之后：");
            Console.WriteLine($"    Count    = {list.Count}      ← 装了 3 个元素");
            Console.WriteLine($"    Capacity = {list.Capacity}      ← 数组有 4 个格子（多出来的 1 格是预留的）");
            Console.WriteLine($"    list[0]  = {list[0]}    ← 索引器就是转手去问内部数组");
            Console.WriteLine($"    list[2]  = {list[2]}");

            // ★ 索引器也能写：list[1] = "..." 会走 set 分支
            list[1] = "香蕉（改过）";
            Console.WriteLine($"    改完 list[1] 后：[{string.Join(", ", list.GetRawArray()[..list.Count])}]");
            Console.WriteLine($"    IndexOf(\"橘子\") = {list.IndexOf("橘子")}   ← 找不到会返回 -1");
            Console.WriteLine($"    IndexOf(\"西瓜\") = {list.IndexOf("西瓜")}");
            Console.WriteLine();

            Console.WriteLine("  ★ 越界会怎样？（这里故意用 try 接住，不然程序就崩了）");
            try
            {
                int bad = 99;
                _ = list[bad];
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine($"    list[99] → {ex.GetType().Name}: {ex.Message}");
            }
            Console.WriteLine("  ★ 注意：越界判断用的是 Count 还是 Capacity？");
            Console.WriteLine("    是 Count。因为 Capacity 那些格子里的东西是 default(T)，不算数据。");
            Console.WriteLine("    —— 这就是 Count 和 Capacity 最本质的区别。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 三、Capacity vs Count：翻倍扩容的实测
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 三、Capacity vs Count：为什么预分配容量有用 ----------");
            Console.WriteLine();

            const int N = 100_000;

            // ① 不预分配：从 4 格开始，一路翻倍
            var grow = new MyList<int>();
            long allocBefore = AllocatedBytes();
            for (int i = 0; i < N; i++)
            {
                grow.Add(i);
            }
            long allocated = AllocatedBytes() - allocBefore;

            // ★ 手动算一遍"理论总分配量"：每次扩容都新建一个更大的数组，
            //   旧数组就变成了垃圾。把每一步的大小加起来 ≈ 这个列表总共向堆要了多少内存。
            long theoretical = 0;
            int simulatedCapacity = 4;
            int simulatedExpands = 0;
            while (simulatedCapacity < N)
            {
                theoretical += (long)simulatedCapacity * sizeof(int);
                simulatedCapacity *= 2;
                simulatedExpands++;
            }

            Console.WriteLine($"  ① 不预分配（从 4 格开始翻倍，加 {N:N0} 个 int）");
            Console.WriteLine($"     Count            = {grow.Count:N0}");
            Console.WriteLine($"     Capacity         = {grow.Capacity:N0}（最后一个格子基本用不上，但必须留着）");
            Console.WriteLine($"     扩容发生次数      = {grow.ExpandCount} 次");
            Console.WriteLine($"     理论扩容次数      = {simulatedExpands} 次（4 → {simulatedCapacity:N0}）");
            Console.WriteLine($"     本线程总分配      = {FormatBytes(allocated)}");
            Console.WriteLine($"     理论搬运总字节    = {theoretical:N0} 字节（约 {FormatBytes(theoretical)}）");
            Console.WriteLine();

            // ② 预分配：一次开够，全程零扩容
            //    ★ 这里刻意让两边做【完全一样的事】（都是 Add N 次），
            //      唯一区别就是有没有预分配容量 —— 这样对比才公平。
            var preset = new MyList<int>(N);

            allocBefore = AllocatedBytes();
            for (int i = 0; i < N; i++)
            {
                preset.Add(i);
            }
            long allocatedOnce = AllocatedBytes() - allocBefore;

            Console.WriteLine($"  ② 预分配（new MyList<int>({N:N0})，然后同样 Add {N:N0} 次）");
            Console.WriteLine($"     Count            = {preset.Count:N0}（和不预分配那次完全一样）");
            Console.WriteLine($"     Capacity         = {preset.Capacity:N0}（正好够用，一格都不多）");
            Console.WriteLine($"     扩容发生次数      = {preset.ExpandCount} 次");
            Console.WriteLine($"     本线程总分配      = {FormatBytes(allocatedOnce)}");
            Console.WriteLine();

            Console.WriteLine("  ★ 结论：不预分配要分配 " + FormatBytes(allocated) + "，预分配只要 " + FormatBytes(allocatedOnce) + "。");
            Console.WriteLine("     多出来的部分全是\"搬运时留下的旧数组垃圾\"，要靠 GC 去回收。");
            Console.WriteLine("     —— 这就是 new List<T>(capacity) 的价值：不是省时间，是【省垃圾】。");
            Console.WriteLine();

            Console.WriteLine("  ★ 顺便看一个反直觉的点：总搬运量 vs 元素总量");
            Console.WriteLine($"     元素总量   = {N * sizeof(int):N0} 字节");
            Console.WriteLine($"     搬运总量   = {theoretical:N0} 字节（约 {(double)theoretical / (N * sizeof(int)):F1} 倍）");
            Console.WriteLine("     ★ 翻倍扩容的总搬运量是\"元素总量 × 常数\"，不是 n² ——");
            Console.WriteLine("       这正是 Add 能被称作【分摊 O(1)】的原因：");
            Console.WriteLine("       极少数几次很贵的扩容，平摊到 n 次 Add 上，每次只多一点点。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 四、Insert / RemoveAt：为什么它们是 O(n)
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 四、Insert / RemoveAt：元素搬家现场 ----------");
            Console.WriteLine();

            var shift = new MyList<int>();
            for (int i = 1; i <= 6; i++)
            {
                shift.Add(i * 10);
            }
            Console.WriteLine($"  初始           : [{FormatRow(shift.GetRawArray()[..shift.Count], 24)}]  Count = {shift.Count}");

            shift.Insert(0, 5);
            Console.WriteLine($"  Insert(0, 5)   : [{FormatRow(shift.GetRawArray()[..shift.Count], 24)}]  ← 5 个元素全体后移！");

            shift.Insert(3, 35);
            Console.WriteLine($"  Insert(3, 35)  : [{FormatRow(shift.GetRawArray()[..shift.Count], 24)}]  ← 后 3 个后移");

            shift.RemoveAt(0);
            Console.WriteLine($"  RemoveAt(0)    : [{FormatRow(shift.GetRawArray()[..shift.Count], 24)}]  ← 5 个元素全体前移");

            shift.RemoveAt(shift.Count - 1);
            Console.WriteLine($"  RemoveAt(末尾) : [{FormatRow(shift.GetRawArray()[..shift.Count], 24)}]  ← 一个都不用挪");
            Console.WriteLine();

            // ★ 用实测感受"位置决定代价"
            var headList = new MyList<int>();
            var tailList = new MyList<int>();
            for (int i = 0; i < N; i++)
            {
                headList.Add(i);
                tailList.Add(i);
            }

            double msHead = TimeBest(() => headList.Insert(0, -1), 20);
            double msTail = TimeBest(() => tailList.Insert(tailList.Count, -1), 20);

            Console.WriteLine($"  在 {N:N0} 个元素上插入一次（各测 20 轮取最快）：");
            Console.WriteLine($"     插到【开头】Insert(0, x)      : {msHead,8:F4} ms   ← 要搬 {N:N0} 个");
            Console.WriteLine($"     插到【末尾】Insert(Count, x)  : {msTail,8:F4} ms   ← 一个都不用搬");
            Console.WriteLine($"     倍数                        : {msHead / Math.Max(msTail, 0.0001),8:F1} 倍");
            Console.WriteLine();
            Console.WriteLine("  ★ 同样是 Insert，代价能差这么多 —— 所以复杂度写成 O(n) 时，");
            Console.WriteLine("     真正要问的是\"这个 n 到底有多大\"。");
            Console.WriteLine("  ★ 实用推论：少量元素的集合（几十个）根本不用在意这些；");
            Console.WriteLine($"     但如果你在循环里对 {N:N0} 个元素的 List 反复 Insert(0, x)，");
            Console.WriteLine("     那就是 O(n²)，程序会肉眼可见地卡住。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 五、哈希 → 桶 → 冲突链
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 五、哈希 → 桶 → 冲突链 ----------");
            Console.WriteLine();

            string[] words = { "apple", "banana", "cherry", "date", "elderberry", "fig" };
            Console.WriteLine("  string.GetHashCode() 给你一个很大的整数（同一个进程内、同一个字符串，值稳定）：");
            Console.WriteLine();
            Console.WriteLine($"     {"字符串",-14}{"GetHashCode()",16}{"% 5（桶下标）",16}");
            Console.WriteLine("     " + new string('-', 44));
            foreach (string w in words)
            {
                int h = w.GetHashCode();
                Console.WriteLine($"     {w,-14}{h,16}{((h & 0x7FFFFFFF) % 5),16}");
            }
            Console.WriteLine();
            Console.WriteLine("  ★ 注意最后那一列：值域从\"几亿\"压到了 0~4。");
            Console.WriteLine("    这就是哈希函数的全部工作 —— 把无限大的键空间，压进有限个桶。");
            Console.WriteLine("  ★ 而压缩必然带来冲突：不同的键算出同一个桶下标，这是数学上躲不掉的。");
            Console.WriteLine();

            // 故意造一个"烂哈希"：把 int 硬压成 0..9，冲突必然发生
            Console.WriteLine("  故意用一个烂哈希（hash = key % 10 → 只有 10 个桶）：");
            Console.WriteLine();
            int[] keys = { 1, 11, 21, 2, 12, 3, 4, 5, 6, 7 };
            var toyBuckets = new List<int>[10];
            for (int i = 0; i < toyBuckets.Length; i++)
            {
                toyBuckets[i] = new List<int>();
            }
            foreach (int k in keys)
            {
                toyBuckets[k % 10].Add(k);
            }
            for (int i = 0; i < toyBuckets.Length; i++)
            {
                string chain = toyBuckets[i].Count == 0
                    ? "（空）"
                    : string.Join(" → ", toyBuckets[i]);
                Console.WriteLine($"     桶 {i} : {chain}");
            }
            Console.WriteLine();
            Console.WriteLine("  ★ 看桶 1：1 → 11 → 21 三个键挤在同一条链上。");
            Console.WriteLine("    要找 21，得先跟 1 比一次、再跟 11 比一次 —— 这就是哈希表慢下来的唯一原因。");
            Console.WriteLine("  ★ 所以哈希表的 O(1) 有前提：【哈希分布均匀 + 负载因子受控】。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 六、手写 MyHashMap<K,V>
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 六、手写 MyHashMap<K,V> ----------");
            Console.WriteLine();

            var map = new MyHashMap<string, int>(4);      // 故意给小桶数，好看它扩容
            string[] fruits = { "苹果", "香蕉", "橘子", "葡萄", "西瓜", "芒果", "桃子", "梨", "杏", "枣" };

            Console.WriteLine($"     {"操作",-20}{"Count",6}{"桶数",6}{"负载因子",10}{"最长链",8}");
            Console.WriteLine("     " + new string('-', 52));
            for (int i = 0; i < fruits.Length; i++)
            {
                int bucketBefore = map.BucketCount;
                map.Put(fruits[i], (i + 1) * 10);
                string op = $"Put({fruits[i]})";
                // ★ 桶数在这一次 Put 里变了？打一个标记，扩容现场就一目了然
                string mark = map.BucketCount != bucketBefore ? "  ← rehash！" : "";
                Console.WriteLine($"     {op,-20}{map.Count,6}{map.BucketCount,6}{map.LoadFactor,10:F2}{map.MaxChainLength(),8}{mark}");
            }
            Console.WriteLine();
            Console.WriteLine($"  存完 {map.Count} 个键值对：rehash 发生 {map.RehashCount} 次，" +
                              $"最终桶数 {map.BucketCount}，最长链 {map.MaxChainLength()}，空桶 {map.BucketCount - map.UsedBucketCount()} 个");
            Console.WriteLine();
            Console.WriteLine("  ★ 负载因子算法：(Count + 1) > 桶数 × 0.75 就扩容。");
            Console.WriteLine("     4 桶时：装第 1~3 个都是 (2+1) > 3 → False；(3+1) > 3 → True，第 4 个触发扩容。");
            Console.WriteLine("     8 桶时：装到第 6 个是 (5+1) > 6 → False；(6+1) > 6 → True，第 7 个触发扩容。");
            Console.WriteLine("     所以阈值是「不能超过 75%」，不是「装满才扩」——这行 +1 的位置最容易写错。");
            Console.WriteLine("  ★ 为什么必须提前扩容？因为链越长查找越慢。0.75 是「空间浪费」和「链长」的经典折中。");
            Console.WriteLine();

            // 读取 + 更新 + 删除
            Console.WriteLine($"     map[\"西瓜\"]          = {map["西瓜"]}");
            if (map.TryGetValue("不存在的键", out int missing))
            {
                Console.WriteLine($"     居然找到了 {missing}");
            }
            else
            {
                Console.WriteLine("     TryGetValue(\"不存在的键\") = false   ← 安全，不抛异常");
            }

            map["西瓜"] = 999;                                  // 键存在 → 覆盖
            Console.WriteLine($"     map[\"西瓜\"] = 999 之后再读 = {map["西瓜"]}   ← Count 没变，是【改】不是【加】");
            Console.WriteLine($"     Remove(\"芒果\")   = {map.Remove("芒果")}，之后 Count = {map.Count}");
            Console.WriteLine($"     Remove(\"芒果\")   = {map.Remove("芒果")}   ← 第二次删就找不到了");
            Console.WriteLine();

            Console.WriteLine("  遍历一遍（注意顺序不是插入顺序 —— 哈希表本来就没有顺序）：");
            var entries = map.GetEntries();
            var pairs = new List<string>(entries.Count);
            foreach ((string key, int value) in entries)
            {
                pairs.Add($"{key}={value}");
            }
            Console.WriteLine($"     {string.Join("  ", pairs)}");
            Console.WriteLine();
            Console.WriteLine("  ★ 第 09 课说过\"Dictionary 无序\"，现在你知道为什么不只是\"没排序\"，");
            Console.WriteLine("    而是元素根本就是按【哈希值】散落在各个桶里的。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 七、复杂度实测：List 线性查找 vs Dictionary 哈希查找
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 七、复杂度实测：10 万元素里查找 ----------");
            Console.WriteLine();

            const int DataSize = 100_000;
            const int QueryCount = 1_000;

            var dataList = new List<int>(DataSize);
            var dataDict = new Dictionary<int, int>(DataSize);
            for (int i = 0; i < DataSize; i++)
            {
                dataList.Add(i * 2);                 // 偶数：0, 2, 4, ...
                dataDict[i * 2] = i;
            }

            // 一半查得到、一半查不到（"查不到"才是最坏情况：要把整段走完）
            int[] queries = new int[QueryCount];
            var random = new Random(12345);          // ★ 固定种子 = 每次运行结果一致，好对照
            for (int i = 0; i < QueryCount; i++)
            {
                queries[i] = random.Next(0, DataSize * 2);
            }

            long hitsList = 0;
            double msList = TimeBest(() =>
            {
                long hits = 0;
                for (int i = 0; i < QueryCount; i++)
                {
                    if (dataList.Contains(queries[i]))
                    {
                        hits++;
                    }
                }
                Sink(hits);
                hitsList = hits;
            }, 5);

            long hitsDict = 0;
            double msDict = TimeBest(() =>
            {
                long hits = 0;
                for (int i = 0; i < QueryCount; i++)
                {
                    if (dataDict.ContainsKey(queries[i]))
                    {
                        hits++;
                    }
                }
                Sink(hits);
                hitsDict = hits;
            }, 5);

            Console.WriteLine($"  数据量 {DataSize:N0}，查询 {QueryCount:N0} 次（一半命中、一半不命中）：");
            Console.WriteLine();
            Console.WriteLine($"     {"集合",-18}{"方法",-24}{"耗时",12}{"每次查询",14}");
            Console.WriteLine("     " + new string('-', 68));
            Console.WriteLine($"     {"List<int>",-18}{"Contains",-24}{msList,10:F2} ms{msList * 1000 / QueryCount,12:F3} μs");
            Console.WriteLine($"     {"Dictionary",-18}{"ContainsKey",-24}{msDict,10:F2} ms{msDict * 1000 / QueryCount,12:F3} μs");
            Console.WriteLine();
            Console.WriteLine($"     倍数：{msList / Math.Max(msDict, 0.000001):F0} 倍");
            Console.WriteLine($"     （两次命中数一致说明测试是公平的：{hitsList} / {hitsDict}）");
            Console.WriteLine();
            Console.WriteLine("  ★ 为什么差这么多？把每次查找的\"比较次数\"算出来：");
            Console.WriteLine($"     List 最坏要比较 {DataSize:N0} 次（走完整个列表）");
            Console.WriteLine($"     Dictionary 平均比较 1 ~ 2 次（算一次哈希，进桶看一眼）");
            Console.WriteLine($"     ★ 数据量翻 10 倍，List 的耗时就翻 10 倍，Dictionary 几乎一动不动。");
            Console.WriteLine("       这就是 O(n) 和 O(1) 的差别 —— 差距会随数据量【越来越大】。");
            Console.WriteLine();
            Console.WriteLine("  ★ 第 09 课在同一台机器上测出的是 711 倍，这里数字不同很正常：");
            Console.WriteLine("     查询次数、命中率、CPU、Debug/Release 都会影响。要记的是【量级】。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 八、⚠️ 哈希契约
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 八、⚠️ 哈希契约：Equal 的必须同 Hash ----------");
            Console.WriteLine();
            Console.WriteLine("  契约原文：如果 a.Equals(b) 为 true，那么 a.GetHashCode() 必须等于 b.GetHashCode()。");
            Console.WriteLine();

            var badSet = new HashSet<BadPoint>();
            var badA = new BadPoint { X = 1, Y = 2 };
            var badB = new BadPoint { X = 1, Y = 2 };
            badSet.Add(badA);

            Console.WriteLine("  ① 只重写 Equals、不重写 GetHashCode（❌ 违反契约）：");
            Console.WriteLine($"     badA.Equals(badB)              = {badA.Equals(badB)}      ← 按值相等，正确");
            Console.WriteLine($"     badA.GetHashCode() == badB...  = {badA.GetHashCode() == badB.GetHashCode()}      ← 哈希码不一样！");
            Console.WriteLine($"     badSet.Contains(badB)          = {badSet.Contains(badB)}      ← ★ 明明相等，却说\"没有\"");
            Console.WriteLine($"     badSet.Add(badB) 后再看 Count  = {badSet.Add(badB)} / {badSet.Count}   ← ★ 重复元素被放进去了");
            Console.WriteLine();
            Console.WriteLine("     ★ 为什么？HashSet 找人的流程是【先按哈希码找桶，再在桶里比 Equals】。");
            Console.WriteLine("       两个\"相等\"的对象哈希码不同 → 被分到不同的桶 → 它压根没去那个桶里找。");
            Console.WriteLine($"       Equals 那一步永远没机会执行，所以答案当然是 false。");
            Console.WriteLine();

            var goodSet = new HashSet<GoodPoint>();
            var goodA = new GoodPoint { X = 1, Y = 2 };
            var goodB = new GoodPoint { X = 1, Y = 2 };
            goodSet.Add(goodA);

            Console.WriteLine("  ② 两个都重写，且用同样的字段（✅ 正确）：");
            Console.WriteLine($"     goodA.GetHashCode() == goodB... = {goodA.GetHashCode() == goodB.GetHashCode()}");
            Console.WriteLine($"     goodSet.Contains(goodB)         = {goodSet.Contains(goodB)}");
            Console.WriteLine($"     goodSet.Add(goodB)              = {goodSet.Add(goodB)}，Count 还是 {goodSet.Count}");
            Console.WriteLine();
            Console.WriteLine("  ★ 现在你能理解第 09 课那句\"用自定义类当字典键要小心\"到底在说什么了。");
            Console.WriteLine("  ★ 用 record 就没这个问题：编译器自动按【所有字段】同时生成 Equals 和 GetHashCode（第 11 课）。");
            Console.WriteLine();

            var constMap = new Dictionary<ConstantHashKey, string>();
            var constKey = new ConstantHashKey { Id = 7 };
            constMap[constKey] = "值";
            Console.WriteLine("  ③ 危险但合法的写法：GetHashCode() 永远 return 42");
            Console.WriteLine($"     dict[new ConstantHashKey {{ Id = 7 }}] 还能查到吗？ {constMap[new ConstantHashKey { Id = 7 }]}");
            Console.WriteLine("     ★ 查得到！因为哈希码相同 → 进了同一个桶 → 靠 Equals 比出来了。");
            Console.WriteLine("       契约没被违反，只是所有键挤在一条链上 —— 哈希表退化成了链表。");
            Console.WriteLine("       【合法但性能灾难】，这是最阴险的一种写法。");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 九、自己动手改坏它
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 九、自己动手改坏它（改完记得跑一遍看数字） ----------");
            Console.WriteLine();

            var normal = new MyList<int>();
            var grow15 = new Grow15List();

            for (int i = 0; i < N; i++)
            {
                normal.Add(i);
            }
            for (int i = 0; i < N; i++)
            {
                grow15.Add(i);
            }

            Console.WriteLine($"  加 {N:N0} 个 int，两种扩容倍数对比：");
            Console.WriteLine();
            Console.WriteLine($"     {"扩容倍数",-14}{"扩容次数",10}{"最终 Capacity",16}");
            Console.WriteLine("     " + new string('-', 42));
            Console.WriteLine($"     {"×2（本课实现）",-14}{normal.ExpandCount,10}{normal.Capacity,16:N0}");
            Console.WriteLine($"     {"×1.5（改坏的）",-14}{grow15.ExpandCount,10}{grow15.Capacity,16:N0}");
            Console.WriteLine();
            Console.WriteLine($"  ★ ×1.5 的扩容次数多了 {grow15.ExpandCount - normal.ExpandCount} 次（约 {(double)grow15.ExpandCount / normal.ExpandCount:F1} 倍），");
            Console.WriteLine("     最终容量还更小 —— 听起来更省内存，但代价是搬运次数变多。");
            Console.WriteLine("  ★ 更关键的是：1.5 倍扩容在别的语言里是【有意的选择】");
            Console.WriteLine("     （C++ 的 MSVC vector 用 1.5，.NET 的 List 用 2）。");
            Console.WriteLine("     倍数只要 > 1，Add 就都是分摊 O(1)；倍数越接近 1，垃圾越多、常数越差。");
            Console.WriteLine();
            Console.WriteLine("  建议的四个实验（改代码 → 重跑 → 对照数字）：");
            Console.WriteLine("     ① 把 MyList.Expand 里的 newCapacity 改成 1.5 倍，看扩容次数和总分配量");
            Console.WriteLine("     ② 把 MyHashMap 的 LoadFactorLimit 从 0.75 改成 0.3（频繁 rehash）或 2.0（长链）");
            Console.WriteLine("     ③ 把 GetBucketIndex 里的 % 换成 & (bucketCount - 1) 试试（想想为什么只有 2 的幂才成立）");
            Console.WriteLine("     ④ 把 MyList.RemoveAt 里的 _items[Count] = default! 删掉，");
            Console.WriteLine("        然后往列表里塞 10 万个 string，用 GC.GetTotalMemory 看内存下不下去");
            Console.WriteLine();

            // ────────────────────────────────────────────────────────────────
            // 十、小结
            // ────────────────────────────────────────────────────────────────
            Console.WriteLine("---------- 十、一句话总结每个集合 ----------");
            Console.WriteLine();
            Console.WriteLine("  T[]              连续内存，长度写死。快，但不会长。");
            Console.WriteLine("  List<T>          数组 + Count + 翻倍扩容。索引 O(1)、查找 O(n)。");
            Console.WriteLine("  Dictionary<K,V>  哈希 → 桶 → 冲突链 + 负载因子。平均 O(1)。");
            Console.WriteLine("  HashSet<T>       同一个哈希表，只是不存值，只管\"在不在\"。");
            Console.WriteLine("  SortedDictionary 红黑树。慢一点（O(log n)），换来\"永远有序\"。");
            Console.WriteLine("  LinkedList<T>    双向链表。插入删除 O(1)，但查找 O(n) 且不连续 → 实际常常更慢。");
            Console.WriteLine();
            Console.WriteLine("  ★ 最重要的一条：所有 O(1) 都有前提（哈希均匀、负载因子受控）；");
            Console.WriteLine("    所有 O(n) 都有前提（n 到底多大）。");
            Console.WriteLine("    复杂度告诉你【趋势】，实测告诉你【你的场景下到底值不值】。");
            Console.WriteLine();

            Console.WriteLine("===== 示例结束 =====");
            Console.WriteLine();
            Console.Write("按回车键退出...");
            Console.ReadLine();
        }

        /// <summary>
        /// 第九段的道具：把扩容倍数改成 1.5 倍的 MyList。
        /// ★ 故意不直接用 MyList&lt;T&gt;：动态改倍数会让代码里出现一堆 if，
        ///   单独写一个小类对照更清楚。
        /// </summary>
        private sealed class Grow15List
        {
            private int[] _items = new int[4];

            public int Count { get; private set; }
            public int Capacity => _items.Length;
            public int ExpandCount { get; private set; }

            public void Add(int item)
            {
                if (Count == _items.Length)
                {
                    // ★ 1.5 倍：先加一半（整数除法会向下取整，所以要保证至少 +1，否则死循环）
                    int newCapacity = _items.Length + _items.Length / 2;
                    if (newCapacity <= _items.Length)
                    {
                        newCapacity = _items.Length + 1;
                    }
                    Array.Resize(ref _items, newCapacity);
                    ExpandCount++;
                }
                _items[Count] = item;
                Count++;
            }
        }
    }
}
