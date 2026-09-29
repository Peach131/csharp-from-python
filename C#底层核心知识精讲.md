# C# 底层核心知识精讲

> 配套：`C#底层优先路线图.md`
> 用法：每个知识点都有**可运行代码**。不要只读——复制到你的项目里跑一遍，看输出，然后**故意改坏它**，看会怎样。

---

## 目录
1. [编译流水线：从源码到 CPU](#1-编译流水线从源码到-cpu)
2. [内存模型：栈与堆](#2-内存模型栈与堆)
3. [对象的内存布局](#3-对象的内存布局)
4. [值类型 vs 引用类型](#4-值类型-vs-引用类型)
5. [装箱与拆箱](#5-装箱与拆箱)
6. [垃圾回收（GC）](#6-垃圾回收gc)
7. [分代回收与大对象堆](#7-分代回收与大对象堆)
8. [终结器与 IDisposable](#8-终结器与-idisposable)
9. [字符串的底层真相](#9-字符串的底层真相)
10. [委托与闭包](#10-委托与闭包)
11. [泛型的运行时特化](#11-泛型的运行时特化)
12. [async/await 状态机](#12-asyncawait-状态机)
13. [集合的内部实现](#13-集合的内部实现)
14. [Python 对照总表](#14-python-对照总表)

---

## 1. 编译流水线：从源码到 CPU

### 流程

```
  Program.cs
      │  ① C# 编译器 (Roslyn / csc.exe)
      ▼
  IL 中间语言 + 元数据   ──►  打包成 Program.dll (程序集)
      │  ② 运行时加载 (CLR)
      ▼
  JIT 编译器 (只编译被调用的方法)
      ▼
  原生机器码  ──►  CPU 执行
```

### 三个关键角色的类比

| .NET | Python | 说明 |
|---|---|---|
| C# 源码 | .py 源码 | 一样 |
| IL（中间语言） | 字节码（.pyc） | IL 是**静态类型**的字节码，字节码是动态的 |
| **元数据** | ❌ 无对应物 | C# 独有：类型信息以二进制表格形式存在 dll 里 |
| JIT 编译成机器码 | ❌ CPython 不生成机器码 | **这是 C# 通常快 10-50 倍的首要原因** |
| CLR | CPython 解释器 | 都是"运行时" |

### 为什么 C# 需要编译？（这不是缺点，是特性）

Python 的错误分两类：写错的（`if x = 5`）运行前就报语法错误；逻辑错的（拼错变量名）**要跑到那一行才知道**。

C# 把**几乎所有类型相关的错误都提前到编译期**：
```csharp
int count = "hello";        // 编译错误，根本编译不过
user.Nmae = "x";            // 编译错误（拼写）
SomeMethod(1, "a", true);   // 参数不匹配，编译错误
```
代价是"啰嗦"（必须写类型），收益是 IDE 能 100% 准确地补全、跳转、重命名、找引用。

### 实验 1-1：看你的代码被编译成了什么

```csharp
// 文件里只写这一行（顶级语句）
Console.WriteLine("Hello");
```
用 ILSpy 打开编译出的 dll，你会看到编译器**自动生成了**：
```csharp
internal class Program
{
    private static void <Main>$(string[] args)
    {
        Console.WriteLine("Hello");
    }
}
```
**结论**：顶级语句只是语法糖，C# 程序**必须**有一个入口方法。

### 实验 1-2：JIT 只编译一次

```csharp
static int Add(int a, int b) => a + b;

// 第一次调用包含 JIT 编译开销
var sw = Stopwatch.StartNew();
Add(1, 2);
var first = sw.ElapsedTicks;

// 之后走已编译的机器码
sw.Restart();
for (int i = 0; i < 1_000_000; i++) Add(1, 2);
var million = sw.ElapsedTicks;

Console.WriteLine($"第一次: {first} ticks");
Console.WriteLine($"100万次: {million} ticks");
// 观察：第一次的耗时明显偏高（包含了 JIT 编译），后续极快
```

### 理解 bin 和 obj 目录

| 文件 | 作用 |
|---|---|
| `bin/Debug/net9.0/MyApp.dll` | **你的程序集**（IL + 元数据） |
| `bin/Debug/net9.0/MyApp.exe` | Windows 上的启动器（很小，只负责调用运行时） |
| `bin/Debug/net9.0/MyApp.pdb` | 调试符号（行号 ↔ IL 的映射） |
| `MyApp.deps.json` | 依赖清单 |
| `MyApp.runtimeconfig.json` | 运行时配置（框架版本、GC 模式等） |
| `obj/` | 中间产物（编译缓存），可以安全删除 |

---

## 2. 内存模型：栈与堆

### 进程内存布局（简化）

```
高地址
  ┌─────────────────────┐
  │   内核空间           │
  ├─────────────────────┤
  │   栈 Stack          │ ← 线程私有，向下增长，默认 1MB
  │   （局部变量、参数）  │    极快（只是移动栈指针）
  ├─────────────────────┤
  │         ↓           │
  │                     │
  │         ↑           │
  ├─────────────────────┤
  │   堆 Heap           │ ← 进程共享，向上增长
  │   （对象本体）        │    快（托管堆是连续的，只是移动指针）
  ├─────────────────────┤
  │   BSS / 数据段       │
  │   （静态字段）        │
  ├─────────────────────┤
  │   代码段             │
  └─────────────────────┘
低地址
```

### 栈 vs 堆

| | 栈 (Stack) | 托管堆 (Heap) |
|---|---|---|
| 存什么 | 局部变量、方法参数、返回地址 | 对象本体 |
| 谁管理 | 编译器（预先算好大小）+ CPU 栈指针 | GC |
| 分配速度 | 极快（`sub rsp, N` 一条指令） | 快（移动指针），但比栈慢 |
| 释放 | 方法返回时自动（移动栈指针） | GC 决定，不确定时机 |
| 容量 | 小（默认 1MB/线程） | 大（可用到物理内存上限） |
| 碎片 | 不会 | **会** |
| 可见性 | 线程私有 | 所有线程共享 |

### 最容易误解的一点 ⚠️

> **"值类型在栈上，引用类型在堆上" —— 这句话是错的（至少是不完整的）。**

正确说法：
- **局部变量**是值类型 → 它的值在**栈**上
- **类的字段**是值类型 → 它的值在**堆上**，作为对象的一部分
- **数组的元素**是值类型 → 在**堆上**，作为数组的一部分
- **装箱后的值类型** → 在**堆上**

```csharp
class Holder
{
    public int Value;        // ← 这个 int 在堆上！（Holder 对象的一部分）
}

struct Point
{
    public int X, Y;         // ← 取决于 Point 这个变量在哪
}

void Demo()
{
    int local = 5;                  // 栈上
    var holder = new Holder();      // holder 引用在栈上，Holder 对象在堆上
                                    // holder.Value 也在堆上
    Point p = new Point();          // 栈上
    Point[] arr = new Point[10];    // 数组在堆上，10 个 Point 也在堆上
}
```

**判断方法**：问"这个值是不是某个堆对象的一部分？"是 → 在堆上。

### 实验 2-1：证明值类型是复制

```csharp
struct SPoint { public int X; }
class  CPoint { public int X; }

var s1 = new SPoint { X = 1 };
var s2 = s1;              // 复制整个 struct
s2.X = 99;
Console.WriteLine($"struct: s1.X={s1.X}, s2.X={s2.X}");   // 1, 99

var c1 = new CPoint { X = 1 };
var c2 = c1;              // 只复制引用（指针）
c2.X = 99;
Console.WriteLine($"class:  c1.X={c1.X}, c2.X={c2.X}");   // 99, 99
```

### 实验 2-2：栈溢出（Stack Overflow）

```csharp
static void InfiniteRecursion(int depth)
{
    Console.WriteLine(depth);
    InfiniteRecursion(depth + 1);      // 每层调用都占栈帧
}

// InfiniteRecursion(0);
// 结果：StackOverflowException
// ⚠️ 这个异常【无法 catch】！进程直接崩溃，try/catch 完全没用。
// 原因：栈溢出时已经没有栈空间来运行 catch 块了。
```

**这告诉你两件事**：
1. 栈是**有限**的（默认 1MB），递归必须收敛
2. 有些错误**无法用异常机制处理**，只能靠写对代码

### 实验 2-3：查看调用栈

在调试器里，对下面代码打断点，打开 **Call Stack（调用堆栈）** 窗口：
```csharp
static void A() => B();
static void B() => C();
static void C() { /* 在这里打断点 */ }
```
你会看到 `Main → A → B → C` 的栈帧列表，每个栈帧对应一块栈内存。**理解栈帧是理解递归、闭包、异常堆栈的基础。**

---

## 3. 对象的内存布局

### x64 上一个对象的实际布局

```
        托管堆上
  ┌──────────────────────────────┐
  │  Object Header (8 字节)       │ ← 同步块索引：用于 lock、GetHashCode 缓存
  ├──────────────────────────────┤
  │  MethodTable 指针 (8 字节)     │ ← 指向类型描述表，运行时靠它识别类型
  ├──────────────────────────────┤
  │  字段 1                       │
  │  字段 2                       │ ← 按对齐规则排列
  │  ...                         │
  └──────────────────────────────┘
         ↑
     变量里存的是这个地址（引用）
```

**关键结论**：
- 任何对象至少占 **24 字节**（8 header + 8 MT + 至少 8 字节字段）
- `new object()` 在 x64 上占 24 字节，不是 0 字节！
- **MethodTable 指针是"运行时类型识别"的基础**：`obj.GetType()` 就是读这个指针

### MethodTable（类型描述表）里有什么

每个类型在运行时**只有一份** MethodTable，包含：
- 类型名、命名空间、程序集
- 基类指针、接口列表
- 方法表（虚方法地址数组）← **多态的底层机制**
- 静态字段存储区
- 实例字段大小、对齐要求

**多态是怎么实现的**：调用 `animal.Speak()` 时，运行时从对象头的 MethodTable 指针 → 找到方法表 → 按**固定槽位索引**取方法地址 → 调用。这个查找是 O(1) 的（槽位在编译期就确定了）。

### 实验 3-1：测量对象大小

```csharp
class Empty { }                          // 没有任何字段
class OneInt { public int A; }
class TwoInt { public int A, B; }

// 测量托管堆分配
long Measure<T>() where T : new()
{
    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    var before = GC.GetTotalMemory(true);
    var arr = new T[1000];
    GC.KeepAlive(arr);
    var after = GC.GetTotalMemory(true);
    return (after - before) / 1000;
}

Console.WriteLine($"Empty:  {Measure<Empty>()} 字节");     // 24
Console.WriteLine($"OneInt: {Measure<OneInt>()} 字节");    // 24（int 塞进了填充空间）
Console.WriteLine($"TwoInt: {Measure<TwoInt>()} 字节");    // 32
```

**发现**：`Empty` 和 `OneInt` 一样大！因为对象头占了 16 字节，剩下 8 字节的"填充位"刚好放下一个 int。**这就是"内存对齐"的实际影响。**

### 实验 3-2：引用相等的真相

```csharp
object a = new object();
object b = a;
object c = new object();

Console.WriteLine(ReferenceEquals(a, b));   // True  ← 同一个对象
Console.WriteLine(ReferenceEquals(a, c));   // False ← 不同对象

// 结构体：装箱后每次都是新对象
struct P { public int X; }
P p1 = new P { X = 1 };
object o1 = p1, o2 = p1;
Console.WriteLine(ReferenceEquals(o1, o2));  // False ← 两次独立装箱！
```

---

## 4. 值类型 vs 引用类型

### 对比表

| | 值类型 (struct / enum / 基础类型) | 引用类型 (class / interface / array / string / delegate) |
|---|---|---|
| 变量里存什么 | **数据本身** | **指向数据的地址**（指针） |
| 赋值 `b = a` | 复制全部数据 | 复制地址（两个变量指向同一对象） |
| 默认值 | 各字段为 0/false | `null` |
| 能否为 null | 不能（除非 `int?`） | 能 |
| 继承 | 不能继承（隐式继承 `ValueType`） | 能 |
| 内存位置 | 看上下文（栈 / 堆内嵌） | 堆 |
| 分配成本 | 无（跟着宿主走） | 有（堆分配 + GC 压力） |
| 相等比较 | 默认逐字段比较（`ValueType.Equals`，慢，靠反射） | 默认引用比较（`ReferenceEquals`） |
| 适合场景 | 小、短命、不可变的数据（坐标、颜色、金额） | 大部分业务对象 |

### struct 的设计准则（微软官方建议）

**只有同时满足以下条件才用 struct**：
1. 逻辑上表示**单个值**（像一个数字、一个坐标）
2. 实例大小 **< 16 字节**（再大复制开销就超过收益）
3. **不可变**（immutable）
4. 不需要频繁装箱

**违反的后果**：一个 100 字节的 struct 在方法间传递会被复制多次，比传引用慢得多。

### ⚠️ 可变 struct 的经典陷阱

```csharp
struct Counter { public int Value; public void Increment() => Value++; }

var list = new List<Counter> { new Counter() };

// list[0].Increment();     // ❌ 编译错误：无法修改返回值，因为它不是变量
// 因为 list[0] 返回的是【副本】！

// 正确做法 1：取出 → 改 → 放回
var c = list[0];
c.Increment();
list[0] = c;

// 正确做法 2：不要设计可变的 struct
```

**如果你在 Python 里习惯"就地修改一切"，这个坑一定会踩。** Python 里所有东西都是引用，`list[0].x = 5` 天经地义；C# 里 struct 是值，做不到。

### 实验 4-1：值类型作为字段

```csharp
class Wrapper { public int Value; }      // 这个 int 在堆上
struct Wrapper2 { public int Value; }    // 取决于 Wrapper2 变量在哪

// 证明：修改 struct 副本不影响原对象（当 struct 是局部变量时）
Wrapper2 w2a = new Wrapper2 { Value = 1 };
Wrapper2 w2b = w2a;
w2b.Value = 99;
Console.WriteLine(w2a.Value);   // 1

// 但 struct 作为数组元素时，可以就地改
Wrapper2[] arr = new Wrapper2[1];
arr[0].Value = 42;               // ✅ 可以！数组索引器返回的是【可寻址的引用】
Console.WriteLine(arr[0].Value); // 42
```

**为什么不一致？** 数组的索引器返回的是"元素位置的引用"，而 `List<T>` 的索引器返回的是"值的副本"。**这是 BCL 设计里的一个历史不一致**，记住就好。

---

## 5. 装箱与拆箱

### 定义

- **装箱（Boxing）**：把值类型放进堆上的一个"盒子"（`object`），值被复制进去
- **拆箱（Unboxing）**：从盒子里把值复制出来，**类型必须完全匹配**

```
装箱前：  栈: [42]              装箱后：  栈: [引用] ──┐
                                                 ▼
                                        堆: [Header][MT:Int32][42]
```

### 代价

| 成本 | 说明 |
|---|---|
| 堆分配 | 至少 24 字节，产生 GC 压力 |
| 内存复制 | 值被复制进盒子 |
| 拆箱的类型检查 | 运行时校验类型，不匹配抛 `InvalidCastException` |
| 缓存不友好 | 盒子散落在堆上 |

### 什么时候会装箱？（背下来）

```csharp
// 1. 赋值给 object 或接口
object o = 42;                       // 装箱
IComparable c = 42;                  // 装箱（接口也是引用类型）

// 2. 非泛型集合
var al = new ArrayList();
al.Add(42);                          // 装箱

// 3. string.Format / string.Concat 传值类型
string s = string.Format("{0}", 42); // 装箱

// 4. 对未重写 ToString 的 struct 调用 ToString
struct Bad { public int X; }          // 没重写 ToString
var b = new Bad();
b.ToString();                        // 装箱！调用的是 ValueType.ToString

// 5. 把 struct 存进 object[] 或非泛型容器
object[] arr = { new Bad() };        // 装箱
```

### 不会装箱的情况

```csharp
List<int> list = new List<int>();
list.Add(42);                        // ✅ 不装箱（泛型特化）
int x = list[0];                     // ✅ 不拆箱

string s = $"{42}";                  // ✅ 现代编译器用 InterpolatedStringHandler，不装箱
```
> 注意：`$""` 插值在 .NET 6+ 已优化，但 `string.Format` 仍然装箱。**用插值，别用 string.Format。**

### 实验 5-1：测量装箱

```csharp
long Alloc() => GC.GetAllocatedBytesForCurrentThread();

var before = Alloc();
for (int i = 0; i < 1000; i++)
{
    object boxed = i;               // 1000 次装箱
}
var after = Alloc();
Console.WriteLine($"1000 次装箱分配了 {after - before} 字节");   // ≈ 24000

before = Alloc();
var list = new List<int>(1000);
for (int i = 0; i < 1000; i++) list.Add(i);   // 无装箱
after = Alloc();
Console.WriteLine($"List<int> 分配了 {after - before} 字节");      // ≈ 4000（只有预分配的数组）
```

### 实验 5-2：拆箱的严格性

```csharp
object o = 42;          // 装箱 int
// long l = (long)o;    // ❌ InvalidCastException！必须精确匹配 Int32
int i = (int)o;         // ✅ 正确

// 但用 Convert 可以转换
long l2 = Convert.ToInt64(o);   // ✅ 会做转换，但有额外开销
```

### 为什么这对 Unity 是生死问题

Unity 的 `Update()` 每帧执行（60fps = 每秒 60 次）：
```csharp
void Update()
{
    // ❌ 每次调用都装箱 + 分配字符串
    Debug.Log("Score: " + score);           // 字符串拼接

    // ❌ 闭包 + 委托分配
    items.ForEach(x => DoSomething(x));

    // ❌ 装箱（如果 score 是 int）
    object o = score;

    // ✅ 正确写法
    if (score != _lastScore)                 // 只在变化时更新
    {
        _scoreText.text = score.ToString();  // 缓存 Text 组件
        _lastScore = score;
    }
}
```
**累积效果**：每秒 180 次分配 × 每帧几十字节 → 几分钟后堆满 → **GC 停顿 → 肉眼可见的卡顿**。

---

## 6. 垃圾回收（GC）

### GC 要解决的问题

Python 用**引用计数 + 循环检测**：
- 优点：对象不可达时**立即**释放
- 缺点：维护计数有开销；循环引用需要额外检测器；多线程下要加锁

C# 用**追踪式 GC（可达性分析）**：
- 从**根（Root）**出发，标记所有可达对象，未标记的即垃圾
- 优点：天然处理循环引用；分配极快；可分代优化
- 缺点：释放时机不确定；有停顿

### 根（Root）包括什么

1. **栈上的局部变量和参数**（所有线程）
2. **静态字段**（类的静态变量）
3. **CPU 寄存器**里的引用
4. **GC Handle**（如 `GCHandle.Alloc`、`WeakReference`）
5. **终结队列**中的对象

### 托管堆为什么分配快

托管堆维护一个指针 `alloc_ptr`，分配只需：
```
分配 = 检查剩余空间 → 返回 alloc_ptr → alloc_ptr += 对象大小
```
**就这么简单，约等于一条 CPU 指令。** 这比 C 的 `malloc`（要查找合适的空闲块）快得多。

代价是：**GC 时必须做整理（压缩）**，否则会产生碎片。

### GC 的三个阶段

```
① 标记（Mark）  ：从根遍历所有可达对象，打标记
② 清除（Sweep） ：回收未标记的对象（并可能压缩/移动存活对象）
③ 压缩（Compact）：把存活对象移到一起，消除碎片，更新所有引用
```

> **注意**：对象一旦被 GC 移动了地址，原来指向它的引用**全部**要更新。这也是为什么 .NET 里不能随便拿对象的裸指针。

### 实验 6-1：用 WeakReference 证明 GC 真的回收了

```csharp
// WeakReference：不阻止对象被回收的"弱"引用
static WeakReference CreateWeak()
{
    var obj = new object();
    return new WeakReference(obj);   // obj 出作用域后就不可达
}

var weak = CreateWeak();
GC.Collect();                        // 强制回收
GC.WaitForPendingFinalizers();

Console.WriteLine($"对象还活着吗？ {weak.IsAlive}");    // False ← 被回收了
Console.WriteLine($"Target: {weak.Target ?? "null"}");

// 对照：强引用会阻止回收
var strong = new object();
var weak2 = new WeakReference(strong);
GC.Collect();
Console.WriteLine($"有强引用时：{weak2.IsAlive}");       // True
```

### 什么时候 GC 会自动触发

1. **代 0 预算满**（最常见）：新对象累计到一定量
2. **分配时空间不足**：需要向 OS 申请更多内存
3. **显式调用** `GC.Collect()`（**生产代码不要调用！**）
4. **系统内存压力**：OS 发出低内存通知
5. **AppDomain 卸载**、`GC.AddMemoryPressure` 等特殊情况

> ⚠️ **永远不要在业务代码里写 `GC.Collect()`**。GC 自己判断的时机比你准得多。只有测试和诊断时才用。

### 实验 6-2：观测 GC 触发

```csharp
Console.WriteLine($"开始: Gen0={GC.CollectionCount(0)}, 堆={GC.GetTotalMemory(false) / 1024}KB");

var list = new List<byte[]>();
for (int i = 0; i < 1000; i++)
{
    list.Add(new byte[10_000]);       // 每次分配 1 万字节
    if (i % 200 == 0)
        Console.WriteLine($"i={i,4}: Gen0回收={GC.CollectionCount(0),3} 次, " +
                          $"Gen1={GC.CollectionCount(1),2}, Gen2={GC.CollectionCount(2),2}, " +
                          $"堆={GC.GetTotalMemory(false) / 1024,7}KB");
}
```

---

## 7. 分代回收与大对象堆

### 代龄假设（Generational Hypothesis）

> **"绝大多数对象活不过第一次 GC。"**

这是实测得出的经验规律（通常 80-95% 的对象是短命的临时对象）。基于它，GC 分成三代：

| 代 | 内容 | 回收频率 | 扫描范围 |
|---|---|---|---|
| **Gen 0** | 新分配的对象 | 非常高（可能每秒几十次） | 只扫 Gen0，**很快** |
| **Gen 1** | Gen0 存活一次的对象（缓冲区） | 中等 | Gen0 + Gen1 |
| **Gen 2** | 长期存活的对象 | 低（可能几分钟一次） | 整个堆，**慢** |

**核心思想**：既然大多数对象很快死，就**只频繁检查新对象**，老对象很少检查。这样每次 GC 的扫描量很小。

```
   新对象分配
       │
       ▼
   ┌────────┐  GC 后还活着   ┌────────┐  GC 后还活着   ┌────────┐
   │ Gen 0  │ ─────────────► │ Gen 1  │ ─────────────► │ Gen 2  │
   └────────┘   晋升         └────────┘   晋升         └────────┘
       │                        │                        │
   回收快                   回收中等                 回收慢（整堆）
```

### 大对象堆（LOH, Large Object Heap）

**超过 85,000 字节**的对象直接分配到 LOH（属于 Gen 2）。

**为什么单独处理大对象？**
- 大对象复制成本高（压缩时要移动几十 MB）
- 大对象数量少，代龄优化没意义

**后果**：**LOH 默认不压缩** → 反复分配/释放不同大小的数组 → **内存碎片** → 明明有空间却分配失败（`OutOfMemoryException`）。

**解法**：
```csharp
// 1. 避免频繁分配大数组：用 ArrayPool 复用
var pool = ArrayPool<byte>.Shared;
byte[] buffer = pool.Rent(100_000);
try
{
    // 使用 buffer
}
finally
{
    pool.Return(buffer);            // 归还，下次复用
}

// 2. 极端情况下手动压缩 LOH（只在大操作后做一次）
GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
GC.Collect();
```

### 实验 7-1：观测对象晋升

```csharp
var obj = new object();
Console.WriteLine($"新对象: Gen{GC.GetGeneration(obj)}");   // Gen0

GC.Collect();
Console.WriteLine($"GC 一次后: Gen{GC.GetGeneration(obj)}"); // Gen1

GC.Collect();
Console.WriteLine($"GC 两次后: Gen{GC.GetGeneration(obj)}"); // Gen2

GC.Collect();
Console.WriteLine($"GC 三次后: Gen{GC.GetGeneration(obj)}"); // Gen2（封顶）
```

### 实验 7-2：制造 LOH 碎片

```csharp
// 交替分配大数组，制造碎片
var holders = new List<byte[]>();
for (int i = 0; i < 20; i++)
{
    holders.Add(new byte[100_000]);        // 每个都进 LOH
    if (i % 3 == 0) holders.RemoveAt(0);   // 释放中间的几个
}
// 结果：LOH 里出现"空洞"，用 GC.GetGCMemoryInfo() 观察
var info = GC.GetGCMemoryInfo();
Console.WriteLine($"堆总大小: {info.HeapSizeBytes / 1024}KB");
Console.WriteLine($"碎片化:   {info.FragmentedBytes / 1024}KB");
```

### 服务器 GC vs 工作站 GC

| 模式 | 说明 | 适用 |
|---|---|---|
| **工作站 GC**（默认） | 每个 CPU 一个 GC 堆，暂停短 | 桌面/客户端应用（**Unity 是这一类**） |
| **服务器 GC** | 每个 CPU 一个 GC 堆 + 专用 GC 线程 | 高吞吐服务器（ASP.NET Core 可选） |

配置（在 `.csproj` 或 `runtimeconfig.json`）：
```xml
<PropertyGroup>
  <ServerGarbageCollection>true</ServerGarbageCollection>
</PropertyGroup>
```

---

## 8. 终结器与 IDisposable

### 问题的本质

**GC 只管"托管内存"，不管"非托管资源"**：
- 文件句柄
- 网络连接
- 数据库连接
- 窗口句柄、GDI 对象
- 原生内存（`Marshal.AllocHGlobal`）

GC 不知道这些资源的存在，所以**必须手动释放**。

### 两种机制

| | 终结器 `~ClassName()` | `IDisposable.Dispose()` |
|---|---|---|
| 谁调用 | **GC**（不确定何时） | **你**（确定何时） |
| 触发条件 | 对象被回收时 | 你显式调用，或 `using` 结束 |
| 线程 | 专用终结器线程 | 调用线程 |
| 能否访问其他托管对象 | **不安全**（可能已被回收） | 安全 |
| 开销 | 会让对象多活一轮 GC | 无 |
| 用途 | 最后防线（防止彻底泄漏） | **主要手段** |

### 为什么要"多活一轮"

有终结器的对象被判定为垃圾后，GC 会：
1. 把它移到**终结队列（f-reachable queue）**
2. 该对象**这一轮不被回收**（因为它还要被终结器访问）
3. 终结器线程调用 `~ClassName()`
4. **下一轮 GC** 才真正回收

**所以：有终结器的对象需要两次 GC 才能被回收，且会晋升到 Gen1/Gen2。** 这就是"不要随便写终结器"的原因。

### 标准 Dispose 模式（背下来）

```csharp
public class ResourceHolder : IDisposable
{
    private bool _disposed;
    private readonly IntPtr _nativeResource;      // 非托管资源
    private readonly Stream _managedResource;     // 托管资源（本身也是 IDisposable）

    public ResourceHolder()
    {
        _nativeResource = Marshal.AllocHGlobal(1024);
        _managedResource = File.OpenRead("data.txt");
    }

    // 只有【直接持有非托管资源】时才需要终结器
    ~ResourceHolder() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);   // 关键：告诉 GC 不用再调终结器了
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // 只有主动 Dispose 时才碰托管对象（终结器线程上碰它们不安全）
            _managedResource?.Dispose();
        }

        // 非托管资源无论如何都要释放
        Marshal.FreeHGlobal(_nativeResource);

        _disposed = true;
    }

    public void DoWork()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);   // .NET 7+
        // ...
    }
}
```

**关键点**：
- `GC.SuppressFinalize(this)`：主动释放过了，就不用再走终结器（省一轮 GC）
- `disposing` 参数区分"主动释放"和"终结器释放"
- 只有**直接持有非托管资源**才写终结器；只是包装了别的 `IDisposable`，不需要

### using 的两种写法

```csharp
// 写法 1：作用域（传统）
using (var stream = File.OpenRead("a.txt"))
{
    // ...
}   // 这里自动 Dispose

// 写法 2：using 声明（C# 8+，推荐）
using var stream = File.OpenRead("a.txt");
// ... 代码
// 方法结束时自动 Dispose（无论正常返回还是异常）
```

**对照 Python**：`using` ≈ `with`，`Dispose()` ≈ `__exit__`，`~ClassName()` ≈ `__del__`（但 C# 的终结器更不可靠，Python 的 `__del__` 在 CPython 里通常立即调用）。

### 实验 8-1：观测终结器时机

```csharp
class Tracked
{
    private readonly int _id;
    public Tracked(int id) { _id = id; Console.WriteLine($"  [{_id}] 创建"); }
    ~Tracked() => Console.WriteLine($"  [{_id}] 终结器执行 (线程 {Environment.CurrentManagedThreadId})");
}

void Scope()
{
    var a = new Tracked(1);
    var b = new Tracked(2);
    Console.WriteLine("  Scope 即将返回");
}

Console.WriteLine("=== 进入 Scope ===");
Scope();
Console.WriteLine("=== Scope 已返回，但终结器还没跑 ===");
Thread.Sleep(100);
Console.WriteLine("=== 手动触发 GC ===");
GC.Collect();
GC.WaitForPendingFinalizers();      // 不等待的话，终结器可能还没执行
Console.WriteLine("=== 结束 ===");
```
**观察**：终结器在**专用线程**上执行，且时间点不确定——这就是为什么不能依赖终结器。

---

## 9. 字符串的底层真相

### 不可变性

```csharp
string s = "hello";
s.ToUpper();                    // 返回新字符串 "HELLO"
Console.WriteLine(s);           // "hello" ← 原字符串没变！

s = s.ToUpper();                // 必须重新赋值
// s[0] = 'H';                  // ❌ 编译错误：索引器只读
```

**内存视角**：每次"修改"字符串都是**在堆上创建一个全新的字符串对象**。

### 字符串驻留（String Interning）

**运行时会维护一个"字符串池"**，所有**字面量**字符串会被复用同一个对象：

```csharp
string a = "hello";
string b = "hello";
Console.WriteLine(ReferenceEquals(a, b));   // True！同一个对象

// 但运行时拼接的结果不在池里
string c = "hel" + "lo";                    // 编译期常量折叠 → 仍是 "hello" 字面量
string d = string.Concat("hel", "lo");      // 运行时拼接
Console.WriteLine(ReferenceEquals(a, d));   // False！

// 手动驻留
string e = string.Intern(d);
Console.WriteLine(ReferenceEquals(a, e));   // True
```

**目的**：字面量常量在程序中可能重复出现几百次，驻留让它们共享一个对象，**省内存**。

**⚠️ 注意**：驻留的字符串**永不释放**（直到进程结束）。所以**绝对不要**对用户输入、动态生成的字符串调用 `string.Intern()`——那是内存泄漏。

### 内部编码：UTF-16

| | .NET `string` | Python 3 `str` |
|---|---|---|
| 内部编码 | **UTF-16**，每个 `char` 2 字节 | 变长（Latin-1 / UCS-2 / UCS-4 自适应） |
| `"a"` 内存 | 2 字节 | 1 字节 |
| `"中"` 内存 | 2 字节（1 个 char） | 2 字节（UCS-2） |
| `"𝄞"`（音乐符号） | **4 字节（2 个 char！）** | 4 字节（UCS-4） |
| `Length` 的含义 | **char 个数**，不是字符个数 | **码点个数** |

**这个差异会咬你**：
```csharp
string emoji = "𝄞";
Console.WriteLine(emoji.Length);        // 2  ← 不是 1！
Console.WriteLine(emoji[0]);            // 半个字符（乱码）

// 正确做法：按文本元素遍历
var si = new StringInfo(emoji);
Console.WriteLine(si.LengthInTextElements);   // 1
```

### 拼接的性能灾难

```csharp
// ❌ O(n²)：每次 += 都分配新字符串并复制全部内容
string s = "";
for (int i = 0; i < 100_000; i++) s += i;

// ✅ O(n)：StringBuilder 内部是可变的 char 数组
var sb = new StringBuilder();
for (int i = 0; i < 100_000; i++) sb.Append(i);
string result = sb.ToString();

// ✅ 已知数量时，用 string.Join / Concat
string joined = string.Join(",", items);
```

### 实验 9-1：量化拼接的代价

```csharp
const int N = 100_000;

long Alloc() => GC.GetAllocatedBytesForCurrentThread();

var before = Alloc();
var sw = Stopwatch.StartNew();
string s = "";
for (int i = 0; i < N; i++) s += i;
sw.Stop();
Console.WriteLine($"字符串拼接:     {sw.ElapsedMilliseconds,6} ms, 分配 {(Alloc() - before) / 1024 / 1024,6} MB");

before = Alloc();
sw.Restart();
var sb = new StringBuilder();
for (int i = 0; i < N; i++) sb.Append(i);
var s2 = sb.ToString();
sw.Stop();
Console.WriteLine($"StringBuilder: {sw.ElapsedMilliseconds,6} ms, 分配 {(Alloc() - before) / 1024 / 1024,6} MB");

// 典型结果：拼接几百 ms + 几百 MB 分配；StringBuilder 几 ms + 几 MB
```

---

## 10. 委托与闭包

### 委托是对象

```csharp
Action a = () => Console.WriteLine("hi");
```
`a` **不是**函数指针，而是一个**堆上的对象**，包含：
- `Target`：目标对象（实例方法时是 `this`，静态方法时为 null）
- `Method`：方法指针（MethodDesc）

**所以每次创建 Lambda 都可能产生一次堆分配。** 用 ILSpy 看：
```
newobj instance void class [System.Runtime]System.Action::.ctor(object, native int)
```

### 闭包 = 编译器生成的类

```csharp
int multiplier = 3;
Func<int, int> f = x => x * multiplier;
```
编译器会生成一个类来"装"被捕获的变量：
```csharp
// 编译器生成的等价代码
[CompilerGenerated]
private sealed class <>c__DisplayClass0_0
{
    public int multiplier;                     // 捕获的变量变成字段
    public int <Main>b__0(int x) => x * multiplier;
}

// 你的代码变成：
var displayClass = new <>c__DisplayClass0_0();
displayClass.multiplier = 3;
Func<int, int> f = new Func<int, int>(displayClass.<Main>b__0);
```

**关键理解**：
- 捕获变量的 Lambda **必然分配一个对象**（display class）
- 不捕获任何变量的 Lambda，C# 9+ 会缓存成**静态单例**（零分配）
- 捕获的是**变量本身**（引用），不是值的快照

### 经典陷阱：捕获循环变量

```csharp
// foreach：C# 5 之后每次迭代是【新变量】，安全
var actions = new List<Action>();
foreach (var i in Enumerable.Range(0, 3))
    actions.Add(() => Console.WriteLine(i));
foreach (var a in actions) a();       // 输出 0, 1, 2 ✅

// for：循环变量只有【一个】，全部捕获到同一个！
actions.Clear();
for (int i = 0; i < 3; i++)
    actions.Add(() => Console.WriteLine(i));
foreach (var a in actions) a();       // 输出 3, 3, 3 ❌

// 修复：在循环体内创建局部副本
actions.Clear();
for (int i = 0; i < 3; i++)
{
    int copy = i;
    actions.Add(() => Console.WriteLine(copy));   // ✅ 0, 1, 2
}
```

> Python 里 `lambda: i` 也是同样的问题（延迟绑定），**这个坑两个语言通用**。但 Python 连 `for` 和 `foreach` 都不区分——所有循环变量都是同一个。

### 多播委托是不可变的

```csharp
Action a = () => Console.WriteLine("A");
var original = a;
a += () => Console.WriteLine("B");
// a 现在指向一个【新的】委托对象（内部是委托数组）
// original 仍然只包含 "A"
Console.WriteLine(ReferenceEquals(a, original));   // False
```
**含义**：`+=` 是 O(n) 操作（要复制数组）。**高频订阅时要小心**。

### 事件的本质

```csharp
public event EventHandler? Clicked;

// 编译后等价于：
private EventHandler? _clicked;
public void add_Clicked(EventHandler? value) { /* 线程安全的 += 逻辑 */ }
public void remove_Clicked(EventHandler? value) { /* 线程安全的 -= 逻辑 */ }
```
**区别**：事件外面只能 `+=` / `-=`，不能 `=` 覆盖，也不能直接 `Invoke`。这是封装。

### 实验 10-1：证明闭包会分配

```csharp
long Alloc() => GC.GetAllocatedBytesForCurrentThread();

// 捕获变量 → 分配 display class
int captured = 10;
var before = Alloc();
for (int i = 0; i < 1000; i++)
{
    Func<int> f = () => captured + i;
    GC.KeepAlive(f);
}
Console.WriteLine($"捕获变量: {Alloc() - before} 字节");   // 每轮都分配

// 不捕获 → C# 9+ 静态缓存
before = Alloc();
for (int i = 0; i < 1000; i++)
{
    Func<int, int> f = x => x * 2;      // 无捕获
    GC.KeepAlive(f);
}
Console.WriteLine($"无捕获:   {Alloc() - before} 字节");   // 接近 0
```

---

## 11. 泛型的运行时特化

### 三种语言的泛型实现对比

| 语言 | 机制 | 运行时类型信息 | 性能 |
|---|---|---|---|
| **Java** | 类型擦除 | ❌ 丢失（都是 `Object`） | 需要装箱/强制转换 |
| **Python** | 无泛型（duck typing） | 运行时才有 | 全动态，慢 |
| **C#** | **Reified Generics（具现化）** | ✅ 完整保留 | **值类型特化，零装箱** |

### C# 的两个关键机制

**① 值类型特化**：`List<int>` 和 `List<double>` 在运行时是**两份不同的原生代码**，`int` 直接内联在数组里。

**② 引用类型共享**：`List<string>` 和 `List<object>` **共享同一份 JIT 代码**（因为引用都是 8 字节指针）。

```csharp
// 验证 ①
Console.WriteLine(typeof(List<int>) == typeof(List<double>));   // False（不同类型）

// 验证 ②：共享代码但类型身份不同
Console.WriteLine(typeof(List<string>) == typeof(List<object>)); // False
// 但 JIT 内部只生成了一份 List<object> 的代码给两者共用
```

### 泛型约束如何提升性能

```csharp
// ❌ 无约束：T 被视为 object，调用方法要装箱
T Max<T>(T a, T b)
{
    return ((IComparable)a).CompareTo(b) > 0 ? a : b;   // 装箱！
}

// ✅ 有约束：编译器知道 T 实现了 IComparable<T>，直接调用
T Max<T>(T a, T b) where T : IComparable<T>
{
    return a.CompareTo(b) > 0 ? a : b;                   // 无装箱，无虚调用
}
```

**常用约束**：
| 约束 | 含义 |
|---|---|
| `where T : class` | 必须是引用类型 |
| `where T : struct` | 必须是值类型（**且不能为 null**） |
| `where T : new()` | 必须有公开无参构造函数 |
| `where T : IComparable<T>` | 必须实现该接口 |
| `where T : unmanaged` | 非托管类型（可在 unsafe/Span 里用） |
| `where T : notnull` | 不可为空 |

### 静态字段与泛型

```csharp
class Counter<T>
{
    public static int Count;
}

Counter<int>.Count = 1;
Counter<string>.Count = 2;
Console.WriteLine(Counter<int>.Count);      // 1 ← 不同的静态字段！
Console.WriteLine(Counter<string>.Count);   // 2
```
**每个"具体泛型类型"在运行时都是一个独立类型**，有自己的静态字段和方法表。

### 实验 11-1：泛型 vs 非泛型性能

```csharp
const int N = 10_000_000;

var sw = Stopwatch.StartNew();
var al = new ArrayList();
for (int i = 0; i < N; i++) al.Add(i);           // 每次装箱！
sw.Stop();
Console.WriteLine($"ArrayList: {sw.ElapsedMilliseconds} ms");

sw.Restart();
var list = new List<int>(N);
for (int i = 0; i < N; i++) list.Add(i);         // 无装箱
sw.Stop();
Console.WriteLine($"List<int>: {sw.ElapsedMilliseconds} ms");

// 典型结果：ArrayList 慢 5-20 倍，且分配几百 MB
```

---

## 12. async/await 状态机

### 编译器做了什么

```csharp
async Task<int> GetDataAsync()
{
    Console.WriteLine("开始");
    await Task.Delay(1000);              // 挂起点 1
    Console.WriteLine("中间");
    await Task.Delay(1000);              // 挂起点 2
    return 42;
}
```

编译器把它**重写**成一个状态机 `struct`（引用类型时是 class）：

```csharp
// 概念上的等价代码（简化）
private struct <GetDataAsync>d__0 : IAsyncStateMachine
{
    public int <>1__state;              // 当前状态（-1 完成，0 起始，1/2 挂起点）
    public AsyncTaskMethodBuilder<int> <>t__builder;
    private TaskAwaiter <>u__1;          // 保存 awaiter

    public void MoveNext()
    {
        switch (<>1__state)
        {
            case 0:                      // 首次执行
                Console.WriteLine("开始");
                var awaiter = Task.Delay(1000).GetAwaiter();
                if (!awaiter.IsCompleted)
                {
                    <>1__state = 1;                       // 记住状态
                    <>u__1 = awaiter;
                    <>t__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);  // 注册回调
                    return;                                // ★ 关键：方法【返回】了！
                }
                goto case 1;
            case 1:                      // 从挂起点恢复
                <>u__1.GetResult();
                Console.WriteLine("中间");
                // ... 第二个 await 同理
                goto case 2;
            case 2:
                Console.WriteLine("结束");
                <>t__builder.SetResult(42);
                return;
        }
    }
}
```

### 最关键的认知 ⚠️

> **`await` 不是"暂停线程"，而是"把方法拆成多段并返回"。**

- `await` 遇到未完成的任务时，**方法的当前调用就直接 return 了**（返回一个未完成的 Task）
- 调用方的线程**被释放**，可以去做别的事
- 任务完成后，由**线程池线程**（或 SynchronizationContext）**再次调用 `MoveNext()`** 继续执行
- 所以 **`await` 之后可能在不同的线程上**！

```csharp
async Task Demo()
{
    Console.WriteLine($"await 前: 线程 {Environment.CurrentManagedThreadId}");
    await Task.Delay(100);
    Console.WriteLine($"await 后: 线程 {Environment.CurrentManagedThreadId}");   // 可能不同！
    await Task.Delay(100);
    Console.WriteLine($"await 后: 线程 {Environment.CurrentManagedThreadId}");   // 可能又不同！
}
```

### 与 Python asyncio 的本质差异

| | C# async/await | Python asyncio |
|---|---|---|
| 执行模型 | 状态机 + 线程池（**真并行**） | 事件循环（**单线程并发**） |
| `await` 后线程 | **可能变化** | **永远是同一个线程** |
| 底层机制 | 编译器生成状态机 | 协程对象 + 事件循环调度 |
| 阻塞调用的后果 | 只阻塞当前线程池线程 | **阻塞整个事件循环**（灾难） |
| 没有 `await` 的 CPU 密集代码 | 会占用一个线程池线程 | 会卡死整个程序 |
| 全局解释器锁 | 无 | 有（GIL） |

**这个差异非常重要**：Python 里你习惯"一切都是单线程"，到了 C# 必须建立"多线程"直觉。

### SynchronizationContext（同步上下文）

**为什么 UI 程序里 `await` 后能回来更新控件？**

因为 UI 框架（WPF/WinForms）安装了一个 `SynchronizationContext`，`await` 恢复时会通过它把续体**调度回 UI 线程**。

```csharp
// WPF 里
private async void Button_Click(object sender, RoutedEventArgs e)
{
    var data = await FetchAsync();       // 在 UI 线程开始
    // 这里仍在 UI 线程！（因为 SynchronizationContext 把续体投递回了 UI 线程）
    label.Content = data;                // ✅ 可以安全更新控件
}
```

**控制台程序没有 SynchronizationContext**，所以 `await` 后会在**线程池线程**上恢复。

### 死锁的经典成因

```csharp
// ❌ 在 UI 线程上这样写会死锁
var data = FetchAsync().Result;      // 同步阻塞 UI 线程
// 1. FetchAsync 内部 await 完成后，SynchronizationContext 想把续体投递回 UI 线程
// 2. 但 UI 线程正卡在 .Result 上，永远不会去执行那个续体
// 3. 死锁

// ✅ 正确：一路 await 到底
var data = await FetchAsync();
```

**铁律**：
1. **绝不**在异步代码上用 `.Result` 或 `.Wait()`
2. **绝不**写 `async void`（除事件处理器）
3. 库代码里用 `.ConfigureAwait(false)`（不需要回原上下文，避免死锁 + 提升性能）
4. UI 代码里**不加** `ConfigureAwait(false)`（需要回 UI 线程）

### async void 为什么危险

```csharp
async void BadAsync()          // ❌
{
    await Task.Delay(100);
    throw new Exception("崩了");   // 这个异常【无法被 catch】！
    // 它会直接抛到 SynchronizationContext 上，通常导致进程崩溃
}

async Task GoodAsync()         // ✅
{
    await Task.Delay(100);
    throw new Exception("崩了");   // 异常被包装进 Task，调用方可以 await + catch
}
```

### 实验 12-1：观测线程变化

```csharp
async Task Main()
{
    Console.WriteLine($"主线程: {Environment.CurrentManagedThreadId}");

    for (int i = 0; i < 5; i++)
    {
        Console.WriteLine($"[{i}] await 前: 线程 {Environment.CurrentManagedThreadId}, " +
                          $"是否线程池: {Thread.CurrentThread.IsThreadPoolThread}");
        await Task.Delay(50);
        Console.WriteLine($"[{i}] await 后: 线程 {Environment.CurrentManagedThreadId}, " +
                          $"是否线程池: {Thread.CurrentThread.IsThreadPoolThread}");
    }
}
// 观察：await 后的线程 ID 会变化，且标记为线程池线程
```

---

## 13. 集合的内部实现

### 核心集合的底层结构

| 集合 | 底层结构 | 索引访问 | 查找 | 插入 | 删除 |
|---|---|---|---|---|---|
| `T[]` | 连续内存 | O(1) | O(n) | ❌ 不支持 | ❌ |
| `List<T>` | 动态数组（翻倍扩容） | O(1) | O(n) | O(n) | O(n) |
| `Dictionary<K,V>` | 哈希表（链地址法） | — | **O(1)** | O(1) | O(1) |
| `HashSet<T>` | 哈希表（只有键） | — | O(1) | O(1) | O(1) |
| `SortedDictionary<K,V>` | 红黑树 | — | O(log n) | O(log n) | O(log n) |
| `LinkedList<T>` | 双向链表 | O(n) | O(n) | **O(1)**（已知节点） | O(1) |
| `Queue<T>` | 环形数组 | — | O(n) | O(1) 入队 | O(1) 出队 |
| `Stack<T>` | 数组 | — | O(n) | O(1) | O(1) |
| `PriorityQueue<T,P>` | 二叉堆 | — | O(n) | O(log n) | O(log n) |

### List\<T\> 的扩容机制

```csharp
var list = new List<int>();           // Capacity = 0
list.Add(1);                          // 扩容到 4
list.Add(2); list.Add(3); list.Add(4);
list.Add(5);                          // 扩容到 8（翻倍）
// 之后：16, 32, 64, 128...

Console.WriteLine($"Count={list.Count}, Capacity={list.Capacity}");
// Count = 元素个数；Capacity = 已分配的数组大小
```

**扩容的代价**：分配新数组 + 复制全部元素。**如果知道元素数量，用 `new List<T>(capacity)` 预分配**，避免多次扩容。

### Dictionary 的哈希机制

```
插入 key="apple", value=5:
  1. 计算 hash = "apple".GetHashCode()
  2. 取模定位桶: bucketIndex = hash % buckets.Length
  3. 如果该桶已被占用（哈希冲突），用链表/数组往后挂
  4. 存 entry: { hashCode, next, key, value }

查找 key="apple":
  1. 算 hash → 定位桶 → 遍历该桶的链
  2. 对每个候选先比 hashCode（快），再调 Equals（慢）
  3. 找到则返回
```

**关键契约**：如果 `a.Equals(b)` 为 true，那么 `a.GetHashCode()` **必须**等于 `b.GetHashCode()`。违反会导致 Dictionary 找不到已存在的键。

```csharp
// ❌ 错误的自定义类型
class BadKey
{
    public int Id;
    public override bool Equals(object? o) => o is BadKey k && k.Id == Id;
    // 忘了重写 GetHashCode！→ 两个"相等"的对象哈希不同 → Dictionary 失效
}

// ✅ 正确
class GoodKey
{
    public int Id { get; init; }
    public override bool Equals(object? o) => o is GoodKey k && k.Id == Id;
    public override int GetHashCode() => Id.GetHashCode();
}

// ✅ 最省事：直接用 record（自动生成 Equals 和 GetHashCode）
public record KeyRecord(int Id);
```

### 缓存友好性：为什么数组比链表快得多

现代 CPU 从内存读数据是**按缓存行（64 字节）**整块读的。

```
数组：[1][2][3][4][5][6][7][8]     ← 连续，读 1 个元素顺带缓存了 7 个
链表：[1]→[2]→[3]→[4]             ← 散落各处，每次跳转都可能缓存未命中
```

**后果**：遍历 100 万个元素的数组可能比链表**快 5-10 倍**，即使两者复杂度都是 O(n)。

**Unity 应用**：这就是为什么 Unity 里**优先用数组/List 而不是 LinkedList**，即使理论上 LinkedList 的插入是 O(1)。

### 实验 13-1：复杂度实测

```csharp
const int N = 100_000;

// List 查找 vs Dictionary 查找
var list = Enumerable.Range(0, N).ToList();
var dict = Enumerable.Range(0, N).ToDictionary(x => x);
var hashSet = Enumerable.Range(0, N).ToHashSet();

int target = N - 1;   // 最坏情况：在末尾
int iterations = 1000;

var sw = Stopwatch.StartNew();
for (int i = 0; i < iterations; i++) list.Contains(target);
sw.Stop();
Console.WriteLine($"List.Contains:       {sw.Elapsed.TotalMilliseconds,8:F2} ms  (O(n))");

sw.Restart();
for (int i = 0; i < iterations; i++) dict.ContainsKey(target);
sw.Stop();
Console.WriteLine($"Dictionary.Contains: {sw.Elapsed.TotalMilliseconds,8:F2} ms  (O(1))");

sw.Restart();
for (int i = 0; i < iterations; i++) hashSet.Contains(target);
sw.Stop();
Console.WriteLine($"HashSet.Contains:    {sw.Elapsed.TotalMilliseconds,8:F2} ms  (O(1))");
```

### 集合选型决策树

```
需要按键查找？
├─ 是 → 需要有序遍历？
│       ├─ 是 → SortedDictionary / SortedList
│       └─ 否 → Dictionary<K,V>   ← 90% 的情况
└─ 否 → 需要去重？
        ├─ 是 → HashSet<T>
        └─ 否 → 需要频繁在中间插入/删除？
                ├─ 是 → LinkedList<T>（但注意缓存不友好）
                └─ 否 → List<T>     ← 默认选择
```

---

## 14. Python 对照总表

| 机制 | Python | C# | 关键差异 |
|---|---|---|---|
| 执行方式 | 解释字节码 | JIT 编译成机器码 | C# 通常快 10-50 倍 |
| 类型检查 | 运行时 | **编译期 + 运行时元数据** | C# 错误提前暴露 |
| 类型信息 | 存在对象里（`__class__`） | 存在元数据 + 对象头 MethodTable 指针 | C# 静态可查 |
| 变量 | 名字绑定到对象 | **固定大小的内存槽** | 根本模型不同 |
| 赋值语义 | 总是引用 | 值类型复制 / 引用类型共享 | **最大的坑** |
| 内存管理 | 引用计数 + 循环 GC | 追踪式分代 GC | 释放时机、循环引用处理不同 |
| 释放时机 | 通常立即（CPython） | 不确定 | C# 必须用 `using` |
| 循环引用 | 需 `gc` 模块检测 | 天然处理 | — |
| 字符串 | 不可变，变长编码 | 不可变，**UTF-16** + 驻留 | 内存占用、`Length` 含义不同 |
| 整数 | 任意精度对象 | 固定位宽值类型 | C# 会溢出（除非用 `BigInteger`） |
| 函数 | 一等公民对象 | 委托对象 | 都有分配成本 |
| 闭包 | `cell` 对象 | 编译器生成的 display class | 机制类似 |
| 泛型 | 无（duck typing） | **运行时特化** | C# 零装箱 |
| 集合 | list/dict/set | List/Dictionary/HashSet | 结构类似，语义不同 |
| 异步 | 事件循环，单线程 | 状态机 + 线程池，多线程 | **线程模型完全不同** |
| 模块 | 文件即模块 | 命名空间 + 程序集 | C# 有编译单元概念 |
| 包管理 | pip + venv | NuGet + csproj | — |
| 私有性 | `_` 约定 | 编译器强制 | — |
| 空值 | `None` | `null` + 可空引用类型（编译期检查） | C# 能编译期发现空引用 |

---

## 附录：把实验代码跑起来的建议

1. 在 `Experiments` 项目里，**每个实验一个文件**，文件名如 `E01_BoxingAllocation.cs`
2. 用**顶级语句**（每个文件一个 `Main` 会冲突，所以用 `#region` 分隔或建成独立方法调用）
3. 文件顶部写三行注释：
   ```csharp
   // 结论：装箱每次分配 24 字节，List<int> 零装箱。
   // 疑问：为什么 Empty 和 OneInt 一样大？（待查：内存对齐）
   // 日期：第 X 周
   ```
4. **保留所有实验**。三个月后回看这些注释，是你成长的最好记录。

> ⚠️ 如果 `dotnet` 命令在你的终端里报"拒绝访问"，那是当前会话的沙箱限制，**不是环境问题**。直接在 Rider 里点运行即可，IDE 不受影响。
