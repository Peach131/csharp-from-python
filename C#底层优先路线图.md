# C# 底层优先学习路线图（20 周）

> 你的定位：**有 Python 基础 + 明确要学底层机制 + 兴趣在游戏开发**
> 这份计划取代 `C#学习计划.md` 的主干地位，那份文件保留作为「项目实战参考」。

---

## 一、为什么"底层优先"对你是对的（但有一个前提）

### 你的优势在于"不是零基础，而是带着错误默认假设"
零基础的人学 C# 底层是"第一次建立模型"；你学 C# 底层是**替换模型**。替换比建立更快，因为你已经有可对照的参照物：

| 你已经相信的（Python） | 真相（C# / .NET） | 底层机制 |
|---|---|---|
| 变量是个名字，贴在对象上 | 变量是一块**固定大小的内存槽** | 栈帧布局、类型大小 |
| `a = b` 之后两者指向同一个东西 | 取决于**值类型还是引用类型** | 栈/堆、MethodTable |
| 内存不用管，反正有 GC | 有 GC，但**代龄、LOH、终结器**都会咬你 | 分代回收、LOH、Finalization Queue |
| 整数和对象差不多 | 整数在栈上，对象在堆上，**转换要装箱** | Boxing / Unboxing |
| `len(s)` 是查一个字段 | `s.Length` 也是，但字符串**被驻留复用** | String Interning |
| 函数是一等公民 | 委托是**对象**，Lambda 会被编译成**类** | 闭包、Display Class |
| `async` 是协程 | `async` 是**编译器生成的状态机** | Async State Machine |
| 生成器是语言特性 | LINQ 的延迟执行靠**迭代器状态机** | Iterator Method |
| 类型在运行时检查 | 类型信息**存在元数据里**，编译进程序集 | Metadata、CTS |

**前提（必须遵守）**：底层知识**禁止纯看书**。每学一个机制，必须用代码**制造出它的可见现象**。看不出来的知识等于没学。所以这份计划里每一周都配了"可观察实验"。

### 与游戏开发的隐藏收益
Unity 的性能问题几乎全部来自四个底层机制：
1. **GC 触发导致的卡帧**（每帧 new 对象 → 内存碎片 → GC 停顿）
2. **装箱**（把 struct 塞进 `object` 或非泛型容器）
3. **字符串拼接**（不可变 + 每次分配新对象）
4. **缓存不友好的遍历**（数组 vs 链表的局部性差异）

这四条**全部**在下面第 2、4、6、9 周里讲。所以你的底层投入在 Unity 里回报极高。

---

## 二、总路线：20 周，双轨并行

```
主线（底层机制）              副线（动手产出）
────────────────────────────────────────────────
第 1-2 周    编译与内存模型    命令行小工具（练手不占脑力）
第 3-4 周    类型系统与元数据   自己的「类型信息查看器」
第 5 周      ★ 里程碑 1：手写一个能说清内存布局的 Demo 集
第 6 周      装箱与泛型        性能对比基准测试 (BenchmarkDotNet)
第 7 周      垃圾回收          制造并观测 GC 停顿
第 8-9 周    字符串与委托      字符串拼接性能实验 + 事件系统
第 10 周     ★ 里程碑 2：异步状态机可视化 Demo
第 11-12 周  泛型与集合内部实现  手写一个简易 List<T> 和 Dictionary<K,V>
第 13-14 周  LINQ 与迭代器      手写 Where/Select/GroupBy
第 15 周     ★ 里程碑 3：Span/ref struct 零分配解析器
第 16 周     反射与源生成器     写一个属性驱动的序列化器
第 17 周     可空引用类型与代码分析
第 18 周     Unity 桥梁：IL2CPP / Mono / 托管堆
第 19-20 周  ★ 里程碑 4：第一个 Unity 小游戏 + 性能调优报告
```

**时间预算**：每天 1.5-2 小时主线 + 每周额外 2-3 小时副线。总计约 20 周。
**若每天只有 1 小时**：只走主线，副线顺延，约 30 周。
**若每天有 3.5 小时**：主副线合并推进，约 11-12 周。

**不要压缩第 1、2、7 周**。这三周是全部后续内容的地基。

---

## 三、逐周详细安排

### 第 1 周：按下运行时到底发生了什么

**要建立的模型**：C# 源码 → 编译器 → IL → JIT → 机器码 → CPU，以及 CLR 在其中扮演什么角色。

| 天 | 任务 | 可观察实验 |
|---|---|---|
| 1 | 理解编译流水线 | 用 `dotnet build` 编译项目，打开 `bin/Debug/net9.0/` 看产物：`.dll`、`.pdb`、`.deps.json`、`.runtimeconfig.json` 各自是什么 |
| 2 | 理解 IL | Rider 里装 **IL Viewer**（Tools → IL Viewer），或双击 `.dll` 用 ILSpy 打开，读一个 `Console.WriteLine("Hello")` 的 IL |
| 3 | 理解 JIT | 同一个方法调用 1 次和 100 万次，看耗时曲线（JIT 只编译一次，之后走缓存） |
| 4 | 理解顶级语句的真相 | 写一个只有一行 `Console.WriteLine` 的文件，用 ILSpy 看编译器**生成了什么**（它偷偷生成了 `Program` 类和 `Main` 方法） |
| 5 | 理解项目文件 | 手工编辑 `.csproj`，改 `TargetFramework`、`Nullable`、`ImplicitUsings`、`LangVersion`，看编译行为怎么变 |
| 6 | 理解解决方案 | 手工建第二个项目（类库），让控制台引用它，看编译顺序和输出 dll |
| 7 | 复盘 | 写下：`using System;` 到底做了什么？`obj/` 目录里是什么？为什么 C# 要编译？ |

**关键概念**（必须能用自己的话讲出来）：
- **CLR**（Common Language Runtime）：.NET 的虚拟机，管内存、线程、类型加载、JIT。相当于 Python 的 CPython 解释器。
- **CIL / IL**（中间语言）：类似 Python 的字节码，但是**静态类型**的字节码。
- **JIT**（Just-In-Time）：运行时把 IL 编译成机器码。对比 CPython：**CPython 不生成机器码**，这是 C# 通常快 10-50 倍的第一个原因。
- **程序集（Assembly）**：`.dll` 或 `.exe`，包含 IL + **元数据**。元数据是关键，Python 没有对应物。
- **AOT / ReadyToRun**：提前编译成机器码，省掉启动 JIT 开销。

**产出**：一个 500 字的笔记，画出"C# 源码 → 运行"的流程图。

---

### 第 2 周：内存模型 —— 栈与堆（最重要的两周之一）

**要建立的模型**：变量住在哪里？对象的"身份"是什么？为什么值类型和引用类型行为不同？

| 天 | 任务 | 可观察实验 |
|---|---|---|
| 1 | 进程内存布局 | 理解栈、堆、静态区、代码段。用任务管理器/性能监视器看进程内存随程序运行变化 |
| 2 | 栈帧 | 递归 10 层，在调试器里看 **Call Stack 窗口**，理解每层栈帧。故意无限递归触发 `StackOverflowException`（**注意：这个异常无法 catch，进程直接死**） |
| 3 | 值类型 vs 引用类型 | 写代码：`int a=1; int b=a; b=2;` 和 `var o1=new object(); var o2=o1;` 对比，用 `ReferenceEquals` 验证 |
| 4 | 对象的头部 | 理解每个堆对象有 **Object Header（同步块索引）+ MethodTable 指针**，所以 x64 上最小对象 24 字节。用 `GC.GetTotalMemory(true)` 前后差值测量 |
| 5 | struct 的陷阱 | 定义 `struct Point { public int X, Y; }`，赋给另一个变量，改一个，看另一个变没变 |
| 6 | 可变 struct 的灾难 | 写 `list[0].X = 5;` —— 编译错误！理解为什么（返回的是副本） |
| 7 | 测量 | 打印 `sizeof(int)`、`sizeof(Point)`、`Unsafe.SizeOf<T>()`，和 `Marshal.SizeOf` 对比 |

**必做实验（写进代码里，能跑出结果）**：
```csharp
// 实验 A：值语义 vs 引用语义
struct SPoint { public int X; }
class CPoint { public int X; }

var s1 = new SPoint { X = 1 };
var s2 = s1; s2.X = 99;
Console.WriteLine(s1.X);   // 1   ← 值类型是复制

var c1 = new CPoint { X = 1 };
var c2 = c1; c2.X = 99;
Console.WriteLine(c1.X);   // 99  ← 引用类型共享

// 实验 B：null 到底是什么
CPoint? n = null;
// n 是"不指向任何对象的引用"，不是"指向一个空对象"
// 底层：它就是一个值为 0 的指针
```

**关键概念**：
- **栈**：后进先出，线程私有，快，容量小（默认 1MB），存值类型和引用（指针本身）。
- **堆**：托管堆，进程共享，慢一点，容量大，存对象本体。
- **MethodTable**：每个类型在运行时有一份"类型描述表"，对象头存指向它的指针。这是运行时识别类型的基础。
- **值类型不一定在栈上**：如果它是类的字段，它就**在堆上**（作为对象的一部分）。这是新手最大的误解，必须纠正。

**产出**：一个控制台 Demo 项目，把上面所有实验跑一遍并输出结论。

---

### 第 3 周：类型系统与元数据

**要建立的模型**：C# 的类型信息是**编译进程序集的数据**，不是运行时猜的。

| 天 | 任务 | 可观察实验 |
|---|---|---|
| 1 | CTS（通用类型系统） | 理解所有 .NET 类型都继承自 `System.Object`；用 ILSpy 看 `object` 的元数据 |
| 2 | 类型的种类 | class / struct / interface / enum / delegate / record —— 在 IL 里各是什么（都是 `class`！enum 和 struct 是 `valuetype`） |
| 3 | 元数据表 | 用 ILSpy 看 `TypeDef`、`MethodDef`、`Field` 表，理解元数据是结构化表格 |
| 4 | typeof 与 GetType | `typeof(int)` 是编译期拿的，`x.GetType()` 是运行时拿的，性能差异巨大 |
| 5 | Type 对象 | `typeof(string).Assembly`、`.BaseType`、`.GetInterfaces()`、`.IsValueType` |
| 6 | 类型的身份 | 两个同名类在不同命名空间是不同类型；`is` / `as` 运算符的底层 |
| 7 | 泛型类型的身份 | `typeof(List<int>) != typeof(List<string>)`，但 `typeof(List<int>).GetGenericTypeDefinition() == typeof(List<>)` |

**关键概念**：
- **Metadata**：以二进制表格形式存储的类型信息（字段、方法、签名、特性）。IDE 的智能提示、反射、序列化全都依赖它。
- **CTS / CLS**：CTS 是运行时类型系统的规范；CLS 是"所有 .NET 语言都支持的最小子集"。
- **类型标识**：类型由「程序集 + 命名空间 + 名字」共同确定。

**产出**：一个"类型信息查看器"——输入 `typeof(X)`，打印它的所有字段、属性、方法、基类、接口。

---

### 第 4 周：装箱、拆箱与类型转换

**要建立的模型**：把一个值类型当成 `object` 用，运行时会**偷偷在堆上分配一个新对象**。

| 天 | 任务 | 可观察实验 |
|---|---|---|
| 1 | 装箱是什么 | `object o = 42;` 用 ILSpy 看生成的 `box [System.Runtime]System.Int32` 指令 |
| 2 | 拆箱 | `int i = (int)o;` 对应 `unbox.any`，且**必须类型完全匹配**，`(int)(object)1L` 会抛异常 |
| 3 | 装箱的代价 | 用 `GC.GetAllocatedBytesForCurrentThread()` 测量装箱前后的分配字节数 |
| 4 | 哪些地方会偷偷装箱 | 非泛型集合 `ArrayList`、`string.Format` 传值类型、把 struct 存进 `object[]`、对 struct 调 `ToString()` 不重写时 |
| 5 | 泛型如何避免装箱 | `List<int>` vs `ArrayList`，用基准测试对比 1000 万次操作 |
| 6 | 隐式转换与 `as` | `as` 只能用于引用类型和可空类型，失败返回 null 而不抛异常 |
| 7 | 复盘 + 里程碑 1 | 整理"装箱黑名单"，形成自己的避坑清单 |

**必做实验**：
```csharp
// 测量装箱分配
long Before() => GC.GetAllocatedBytesForCurrentThread();

var before = Before();
object boxed = 42;                    // 装箱：堆上分配
var after = Before();
Console.WriteLine($"装箱分配了 {after - before} 字节");   // 约 24 字节

// 对比：泛型不装箱
var list = new List<int>();
before = Before();
for (int i = 0; i < 1000; i++) list.Add(i);
after = Before();
Console.WriteLine($"List<int> 增长分配 {after - before} 字节");
// 注意：只有数组扩容时分配，元素本身不装箱
```

**为什么这个对 Unity 特别重要**：Unity 每帧调用 `Update()`，如果里面有一次装箱，60fps 下每秒 60 次堆分配，几分钟就会触发 GC 造成可见卡顿。**这是 Unity 新手最常见、也最致命的性能问题。**

**★ 里程碑 1**：把你第 2-4 周的所有实验整理成一个可运行项目，每个实验有清晰的输出和注释说明。这是你的"底层知识活页夹"，后面反复回来看。

---

### 第 5 周：GC 基础（体感周）

**要建立的模型**：GC 不是"自动的魔法"，它是有策略、有代价、可预测的。

| 天 | 任务 | 可观察实验 |
|---|---|---|
| 1 | 为什么需要 GC | 对比 C++ 手动管理和 C# 自动管理；理解"托管堆"的含义 |
| 2 | 对象的一生 | 分配 → 存活 → 不可达 → 回收；用 `WeakReference` 验证对象真的被回收了 |
| 3 | 触发时机 | `GC.Collect()` 强制触发；理解正常情况下何时触发（代 0 预算满） |
| 4 | 托管堆的本质 | 为什么托管堆是**连续**的（对比 C 的 malloc）；用 `GC.GetTotalMemory` 观察增长 |
| 5 | 测量工具 | 学会用 `dotnet-counters`、Rider 的内存分析器、VS 的诊断工具看堆快照 |
| 6 | 大对象堆（LOH） | 分配 >85000 字节的对象会进 LOH；LOH 默认不压缩 → **内存碎片** |
| 7 | 停顿时长 | 分配大量对象触发 GC，测量停顿（用 Stopwatch 包住 GC.Collect） |

**关键概念**：
- **根（Root）**：栈上的变量、静态字段、CPU 寄存器、GC Handle。GC 从根出发遍历，找不到的对象就是垃圾。
- **托管堆是连续内存**：所以分配很快（只是移动指针，≈1 条指令）。这是 .NET 分配对象比 C 的 `malloc` 快的原因。
- **GC 是"回收不可达对象"而不是"释放内存给操作系统"**：内存归还 OS 是另一回事。

**产出**：一个能可视化"分配 → 回收"过程的控制台 Demo，用 `WeakReference` 证明对象真的消失了。

---

### 第 6 周：GC 分代、终结器与 IDisposable

**要建立的模型**：GC 用"代龄假设"优化性能；但**非托管资源必须手动管理**。

| 天 | 任务 | 可观察实验 |
|---|---|---|
| 1 | 分代假设 | 核心假设："大多数对象很快就死"。所以新对象（代 0）检查频繁，老对象（代 2）检查稀疏 |
| 2 | 三代结构 | `GC.GetGeneration(obj)` 观察对象代龄变化；`GC.CollectionCount(0/1/2)` 看各代回收次数 |
| 3 | 晋升 | 经过一次 GC 还活着的对象晋升到下一代；故意让一个对象活很久，看它升到代 2 |
| 4 | 终结器（Finalizer） | 写 `~MyClass()` 看它何时被调用（**不确定时间！**）；终结器会让对象多活一轮 GC |
| 5 | Dispose 模式 | `IDisposable` + `using` + `Dispose(bool)` 的标准实现；对照 Python 的 `with` / `__exit__` |
| 6 | 为什么要两者都有 | 终结器是"最后防线"，Dispose 是"主动释放"。**写代码时永远用 Dispose，不依赖终结器** |
| 7 | 复盘 | 列出你知道的所有需要 `using` 的类型：文件流、网络连接、数据库连接、GDI 对象…… |

**必做实验**：
```csharp
// 证明终结器的时机不可控
class Tracker
{
    public Tracker() => Console.WriteLine("创建");
    ~Tracker() => Console.WriteLine("终结器执行");   // 何时打印？不知道！
}

static void Test()
{
    var t = new Tracker();
    Console.WriteLine("方法即将结束");
}   // 方法结束后 t 不可达，但终结器可能几秒后才跑

Test();
Console.WriteLine("回到 Main");
GC.Collect();
GC.WaitForPendingFinalizers();   // 强制等待，才会看到"终结器执行"
```

**为什么这个对 Unity 重要**：Unity 的 MonoBehaviour 有自己的生命周期，`OnDestroy` 类似 Dispose 的角色。而 `IDisposable` 在 Unity 里用得少，但**理解它才能理解为什么 Unity 不推荐在 Update 里分配对象**。

---

### 第 7 周：字符串的底层真相

**要建立的模型**：字符串是**不可变的引用类型**，且会被**驻留复用**。

| 天 | 任务 | 可观察实验 |
|---|---|---|
| 1 | 不可变性 | 理解 `s.ToUpper()` 返回**新字符串**，原字符串永不变；`s[0] = 'x'` 编译不过 |
| 2 | 字符串驻留（Interning） | `ReferenceEquals("abc", "abc")` → **true**！字面量被复用。但 `ReferenceEquals(s1, s2)` 对拼接结果常是 false |
| 3 | 为什么要驻留 | 字面量常量复用，省内存；用 `string.Intern()` 手动驻留 |
| 4 | 拼接的代价 | 循环里 `s += x` 是 O(n²) 分配。用 `GC.GetAllocatedBytesForCurrentThread()` 量化 |
| 5 | StringBuilder | 理解它是可变的 char 缓冲区。对比实验：拼接 10 万次的耗时差 |
| 6 | 编码与内部表示 | .NET 内部是 **UTF-16**（每个 char 2 字节）；Python 3 是变长编码（ASCII 1 字节）。中文字符串内存占用差异就在这里 |
| 7 | 现代替代 | `string.Create`、`DefaultInterpolatedStringHandler`（编译器对 `$""` 的优化） |

**必做性能对照实验**：
```csharp
var sw = Stopwatch.StartNew();
string s = "";
for (int i = 0; i < 100_000; i++) s += i;      // 灾难：每次都分配新字符串
sw.Stop();
Console.WriteLine($"拼接耗时 {sw.ElapsedMilliseconds} ms");

sw.Restart();
var sb = new StringBuilder();
for (int i = 0; i < 100_000; i++) sb.Append(i);  // 快几十倍
sw.Stop();
Console.WriteLine($"StringBuilder 耗时 {sw.ElapsedMilliseconds} ms");
```

---

### 第 8 周：委托、闭包与编译器生成的类

**要建立的模型**：委托是**对象**；Lambda 捕获变量时，编译器会**生成一个类**来装这些变量。

| 天 | 任务 | 可观察实验 |
|---|---|---|
| 1 | 委托是对象 | `Action a = () => Console.WriteLine("hi");` 用 ILSpy 看它其实是个对象实例 |
| 2 | 委托的内部结构 | 委托含 **Target（目标对象）+ Method（方法指针）**，可以看成"绑定方法的函数指针" |
| 3 | 闭包 | Lambda 捕获外部变量 `int x = 1; Action a = () => Console.WriteLine(x);` —— 用 ILSpy 看编译器生成的 `<>c__DisplayClass` |
| 4 | 闭包的经典坑 | 循环里捕获循环变量 —— C# 5 之后 `foreach` 已修复，但 `for` 循环仍有坑 |
| 5 | 多播委托 | `+=` 会创建**新的委托对象**（不可变），所以是 O(n²) 的；事件订阅频繁时要注意 |
| 6 | 事件 | 事件的本质是"受限的委托字段"（只能 +=/-=，不能直接赋值），看编译后的 `add_X` / `remove_X` 方法 |
| 7 | 静态 Lambda 优化 | C# 9+ 对不捕获任何变量的 Lambda 会缓存成静态单例，避免每次分配 |

**为什么这对 Unity 重要**：Unity 里 `Update()` 里写 `list.ForEach(x => ...)` 会**每次分配委托 + 闭包对象**。正确做法是把 Lambda 提出来存成字段，或改用 `for` 循环。

---

### 第 9 周：泛型的真身（运行时特化）

**要建立的模型**：泛型不是"复制粘贴代码"，也不是"擦除类型"，而是**运行时特化**。

| 天 | 任务 | 可观察实验 |
|---|---|---|
| 1 | 三种泛型实现对比 | Java 是类型擦除（运行时丢失类型），Python 是 duck typing（无检查），C# 是 **reified generics**（运行时保留类型） |
| 2 | 值类型特化 | `List<int>` 和 `List<double>` 在运行时是**不同的原生代码**，字段内联，无装箱 |
| 3 | 引用类型共享 | `List<string>` 和 `List<object>` **共享同一份 JIT 代码**（因为指针大小一样） |
| 4 | 泛型约束 | `where T : class` / `struct` / `new()` / `IComparable<T>`，看编译器如何利用约束直接调用方法（不装箱） |
| 5 | 静态字段与泛型 | `MyClass<int>.Value` 和 `MyClass<string>.Value` 是**两个不同的静态字段** |
| 6 | 协变与逆变 | `IEnumerable<out T>` / `Action<in T>`，理解 `out` / `in` 关键字 |
| 7 | 手写 List\<T\> | 不看源码，自己实现一个带自动扩容的 `List<T>`，理解 `Capacity` 和 `Count` 的区别 |

**关键概念**：
- **装箱的唯一原因是"把值类型当引用类型用"**。泛型让你在编译期就锁定类型，所以能彻底避免装箱。这是泛型最大的价值，性能收益大于代码复用收益。

---

### 第 10 周：异步状态机 + 里程碑 2

**要建立的模型**：`async/await` 是**编译器重写**，把方法变成状态机。

| 天 | 任务 | 可观察实验 |
|---|---|---|
| 1 | 线程 vs 任务 | 线程是 OS 概念（昂贵），Task 是调度单元（轻量）。`Thread.Sleep` vs `Task.Delay` 的本质区别 |
| 2 | 状态机 | 写一个含 2 个 `await` 的 `async` 方法，用 ILSpy 看编译器生成的 `MoveNext()` 和 `<>d__0` 结构 |
| 3 | 同步上下文 | 理解 `SynchronizationContext`：UI 程序里有它，控制台没有。这是"await 后回到原线程"的原因 |
| 4 | 死锁成因 | 复现死锁：UI 线程 `.Wait()` 一个要回 UI 线程的 Task |
| 5 | ConfigureAwait | `false` 表示"不用回原上下文"，库代码必须加。看为什么 |
| 6 | Task 组合 | `WhenAll` / `WhenAny` / `ContinueWith`；理解它们是**并行**还是**并发** |
| 7 | 与 Python 对比 | Python 的 `async` 是**单线程事件循环协作式**；C# 的 `async` 是**多线程 + 状态机**。为什么 Python 里 `await` 后不换线程而 C# 会 |

**★ 里程碑 2**：写一个 Demo，可视化展示 `async` 方法的执行流程——在每个 `await` 前后打印线程 ID 和时间戳，证明"await 之后可能在线程 A，也可能在线程 B"，以及控制台程序下和 WPF 程序下的差异。

---

### 第 11-12 周：集合的内部实现

**要建立的模型**：不同集合的性能差异来自**底层数据结构**，选错集合就是选错复杂度。

| 周 | 天 | 任务 |
|---|---|---|
| 11 | 1 | 数组：连续内存、O(1) 随机访问、为什么 CPU 缓存友好 |
| 11 | 2 | `List<T>`：动态数组，扩容策略（翻倍），`Capacity` vs `Count` |
| 11 | 3 | 手写 List\<T\>：Add / Insert / RemoveAt / 索引器 / 扩容 |
| 11 | 4 | `Dictionary<K,V>`：哈希表，哈希函数、桶、冲突解决（链地址法） |
| 11 | 5 | 哈希的关键：`GetHashCode` 和 `Equals` 的契约（相等必同哈希） |
| 11 | 6 | 手写简易哈希表 |
| 11 | 7 | 复杂度实测：List 查找 O(n) vs Dictionary 查找 O(1)，用 10 万元素对比 |
| 12 | 1 | `HashSet<T>`：只有键的字典 |
| 12 | 2 | `Queue<T>` / `Stack<T>` / `LinkedList<T>` 的内部结构 |
| 12 | 3 | `SortedDictionary` vs `Dictionary`：红黑树 vs 哈希表 |
| 12 | 4 | `PriorityQueue<T,P>`：二叉堆 |
| 12 | 5 | 不可变集合 `ImmutableList<T>`：结构共享，为什么"修改"很便宜 |
| 12 | 6 | 并发集合：`ConcurrentDictionary` 怎么做到无锁/细粒度锁 |
| 12 | 7 | 选型决策表：什么场景用什么集合（写进自己的笔记） |

**★ 里程碑（并入第 12 周）**：一份"集合选型速查表"，含复杂度、内存开销、适用场景，以及自己实测的基准数据。

---

### 第 13-14 周：LINQ 与迭代器的底层

**要建立的模型**：LINQ 的延迟执行来自**迭代器状态机**；LINQ 的优雅是有性能代价的。

| 周 | 天 | 任务 |
|---|---|---|
| 13 | 1 | `IEnumerable<T>` / `IEnumerator<T>`：`MoveNext()` + `Current` 协议 |
| 13 | 2 | `yield return` 的真相：用 ILSpy 看编译器生成的状态机类 |
| 13 | 3 | 手写 `MyWhere` / `MySelect`，用自己的迭代器实现 |
| 13 | 4 | 延迟执行的坑：`var q = list.Where(...)` 之后修改 list 会怎样 |
| 13 | 5 | `IEnumerable` / `ICollection` / `IList` / `IQueryable` 的区别 |
| 13 | 6 | 表达式树入门：`Expression<Func<int,bool>>` 和委托的区别 |
| 13 | 7 | 为什么 EF Core 需要表达式树（要翻译成 SQL，不能直接执行） |
| 14 | 1 | LINQ 的性能代价：委托调用、迭代器分配、多次遍历 |
| 14 | 2 | 用一个循环替代 3 个 LINQ 链，基准对比 |
| 14 | 3 | `ToArray()` vs `ToList()` vs 保持 `IEnumerable` |
| 14 | 4 | `GroupBy` / `Join` 的内部实现（查哈希表） |
| 14 | 5 | `Span<T>` 与 LINQ 的结合：现代零分配写法 |
| 14 | 6 | 手写 `MyGroupBy` |
| 14 | 7 | 复盘：什么时候该用 LINQ，什么时候该用循环 |

**关键结论**（自己验证后写下来）：**LINQ 让代码更清晰，但不是免费的。热路径上用循环，冷路径上用 LINQ。**

---

### 第 15 周：Span、Memory 与零分配 + 里程碑 3

**要建立的模型**：`Span<T>` 是**栈上的一片连续内存视图**，不拥有数据、不分配堆内存。

| 天 | 任务 | 可观察实验 |
|---|---|---|
| 1 | 为什么需要 Span | `Substring` 会分配新字符串，`AsSpan().Slice()` 不分配 |
| 2 | ref struct | `Span<T>` 是 `ref struct`，只能活在栈上，不能作为类字段、不能用 async |
| 3 | 用 Span 重写字符串解析 | 把 CSV 解析从"split 出一堆字符串"改成"切片零分配"，测量分配字节数 |
| 4 | `Memory<T>` | 需要跨 await 时的选择 |
| 5 | `ArrayPool<T>` | 复用数组，避免 LOH 分配 |
| 6 | 基准测试 | 学会用 BenchmarkDotNet 写正式基准（不要用 Stopwatch 下结论） |
| 7 | 复盘 + 里程碑 3 | 一份"从分配 1MB 优化到 0 字节"的完整优化报告 |

**★ 里程碑 3**：一个零分配的 CSV 解析器，含优化前后的内存分配对比数据。

---

### 第 16 周：反射、特性与源生成器

| 天 | 任务 |
|---|---|
| 1 | 反射基础：`Type` / `MethodInfo` / `PropertyInfo`，运行时创建对象、调用方法 |
| 2 | 反射的代价：比直接调用慢 100-1000 倍，为什么 |
| 3 | 特性（Attribute）：本质是元数据里的一个表项，用 `[MyAttr]` 标记类/方法 |
| 4 | 自定义特性 + 反射读取，理解"约定优于配置" |
| 5 | 源生成器（Source Generator）：编译期生成代码，零反射开销。这是 .NET 现代方案 |
| 6 | `System.Text.Json` 的源生成模式：对比反射模式和生成模式的性能 |
| 7 | 实战：写一个属性驱动的简易序列化器 |

---

### 第 17 周：可空引用类型与代码分析

| 天 | 任务 |
|---|---|
| 1 | `Nullable<T>`（`int?`）的底层：struct + `HasValue` + `Value` |
| 2 | 可空引用类型（`string?`）：**编译期分析**，运行时零开销 |
| 3 | 消除 CS8618 等警告的正确姿势：`required` / `= null!` / 构造函数保证 |
| 4 | 空状态分析：`?.` / `??` / `is null` 模式匹配 |
| 5 | Roslyn 分析器：理解编辑器里的波浪线从哪来 |
| 6 | EditorConfig 与代码规范：团队协作必备 |
| 7 | 复盘：给自己的项目开启严格模式（`TreatWarningsAsErrors`） |

---

### 第 18 周：Unity 桥梁（底层知识落地）

**关键认知**：Unity 用的**不是** .NET 9 的运行时。它有两个脚本后端：

| 后端 | 说明 | 影响 |
|---|---|---|
| **Mono** | 把 IL 在运行时 JIT 编译 | 支持大部分 C# 特性，开发期用（Editor 里就是它） |
| **IL2CPP** | 把 IL 转成 C++ 再编译成本地代码 | 发布版用，性能更好，但**部分反射/动态代码受限** |

**Unity 的 GC 与 .NET 的 GC 不同**：
- Unity 用的是 **Boehm GC**（非分代、非压缩），比 .NET Core 的 GC **弱得多**
- 后果：**堆碎片化严重，且不能移动对象**，所以 GC 停顿更明显
- 结论：**在 Unity 里，"少分配"比在 .NET 里重要 10 倍**

**必须掌握的 Unity 底层优化清单**：
| 问题 | 底层原因 | 解法 |
|---|---|---|
| 每帧卡顿 | Update 里 new 对象 → 触发 GC | 缓存对象、对象池、用字段代替局部对象 |
| `foreach` 有时分配 | 老版本 Mono 的 `IEnumerator` 装箱 | 直接用 `for` 遍历数组/List |
| 字符串拼接卡顿 | 不可变 + 每次分配 | 缓存 `ToString()` 结果、用 `StringBuilder` |
| `GetComponent<T>()` 慢 | 内部有查找 + 可能的装箱 | 在 `Awake()` 里取一次存成字段 |
| `Camera.main` 慢 | 每次都在场景里查找对象 | 缓存到字段 |
| struct 反而变慢 | 装箱（存进 `object` 或非泛型容器） | 用泛型容器、避免 `object` |
| Vector3 运算 | struct，是值类型，赋值会复制 | 这是正常的，但要注意大 struct 的复制开销 |

**本周任务**：装 Unity（Unity Hub + 一个 LTS 版本，如 Unity 6 LTS），建一个空 2D 项目，把上面清单里的每一条**亲手制造出来并观察**（用 Profiler 窗口看 GC Alloc 那一列）。

---

### 第 19-20 周：第一个 Unity 小游戏 + 性能调优报告

**★ 里程碑 4**：一个能玩的小游戏（推荐：贪吃蛇 / 打砖块 / 2D 平台跳跃三选一），要求：
- 完整的游戏循环（开始 → 游玩 → 死亡 → 重开）
- 用 Profiler 检查并优化到 **每帧 GC Alloc 为 0 B**
- 写一份优化报告：优化前后对比、每个优化点对应的底层原因

**这份报告就是你学习成果的最好证明**——它同时展示了 C# 语言能力、底层理解、和工程实践。

---

## 四、底层必读清单（按优先级）

**第一梯队（必须理解，否则后面全是黑魔法）**
1. 栈与堆的区别，值类型和引用类型分别住在哪
2. 对象的头部结构（同步块 + MethodTable 指针）
3. 装箱/拆箱的触发条件与代价
4. GC 的三代模型和触发时机
5. 字符串不可变 + 驻留
6. 委托 = 对象，Lambda 捕获 = 生成类

**第二梯队（决定你能不能写出高性能代码）**
7. 泛型的运行时特化，为什么能免装箱
8. async 状态机与 SynchronizationContext
9. 集合的数据结构与复杂度
10. LINQ 延迟执行的机制
11. IDisposable 与终结器的分工
12. 大对象堆与内存碎片

**第三梯队（进阶，按需学）**
13. Span\<T\> / ref struct / 栈上分配
14. 表达式树与 IQueryable
15. 反射、特性、源生成器
16. 可空引用类型的编译期分析
17. IL2CPP 与 Mono 的差异

---

## 五、学习方法（底层知识专用）

1. **ILSpy 是你的第二双眼睛**
   每一个"为什么"的答案都在 IL 里。装 ILSpy（免费开源）或 Rider 的 IL Viewer，养成"看不懂就去看 IL"的习惯。

2. **能测量就不要猜**
   `Stopwatch` 测时间、`GC.GetAllocatedBytesForCurrentThread()` 测分配、`GC.CollectionCount()` 测 GC 次数、`GC.GetTotalMemory(true)` 测堆大小。有数字才有结论。

3. **主动制造问题，而不是等它发生**
   不要只读"在循环里拼接字符串很慢"——**亲手写出来，测出耗时，看到数字**。亲手触发过一次 `StackOverflowException` 导致的进程崩溃，你一辈子都记得。

4. **画图**
   每学完一个机制，画一张图：内存里长什么样、执行流程是什么。画不出来说明没懂。

5. **写"可观察实验"代码并保留**
   把你所有的实验放进一个 `Experiments` 项目，每个实验一个文件，顶部写结论注释。三个月后回来看，你会惊讶于自己的进步。

6. **Python 对照法**
   学每个机制时先问"Python 里怎么做的？为什么不同？"。例如：Python 的 `id()` 和 C# 的 `ReferenceEquals`、Python 的引用计数 GC 和 C# 的分代追踪 GC（Python 用引用计数 + 循环检测，所以有 `sys.getrefcount`；C# 用可达性分析，所以能处理循环引用而无需额外机制）。

7. **⚠️ 不要陷入深渊**
   底层学习最大的风险是**无限深入**（去看 CoreCLR 源码、去看 CPU 缓存行对齐、去看 JIT 的寄存器分配）。**这三条线不能越**：
   - 不读 CoreCLR/C# 编译器源码（除非你确定要做运行时开发）
   - 不追 CPU 微架构细节（那是 C++ 的领域）
   - 不用 unsafe / 手写指针（学习阶段完全不需要）

   判断标准：**如果一个知识不能让你写出更快或更正确的代码，就跳过它。**

---

## 六、节奏控制与检验点

| 时间点 | 检验标准（做到了才算过关） |
|---|---|
| 第 2 周末 | 能画出一段代码的内存布局草图，说清哪些在栈、哪些在堆 |
| 第 4 周末 | 看到一段代码能指出哪里会发生装箱 |
| 第 6 周末 | 能说清"为什么需要 IDisposable，光有 GC 不够" |
| 第 9 周末 | 能解释为什么 `List<int>` 比 `ArrayList` 快 |
| 第 10 周末 | 能画出 `async` 方法的执行流程图，说清线程切换点 |
| 第 12 周末 | 能根据场景 3 秒内选出正确的集合类型 |
| 第 15 周末 | 能用 Span 把一个字符串解析函数优化到零分配 |
| 第 20 周末 | 有一个每帧 0 GC Alloc 的可玩 Unity 游戏 |

**每周末必做**：写一篇 300 字笔记，回答"这周我搞懂了什么？还有什么不清楚？"。**写不出"不清楚"的说明你没深入。**

---

## 七、资源

**底层机制**
- 《CLR via C#》（Jeffrey Richter）—— 底层圣经，第 4 版。**第 5-8 章（类型基础、装箱、GC）必读**
- 《Pro .NET Memory Management》—— 内存管理专著，免费在线阅读
- 《Writing High-Performance .NET Code》（Ben Watson）—— 性能优化实战
- .NET 官方 GC 文档：https://learn.microsoft.com/zh-cn/dotnet/standard/garbage-collection/
- .NET 运行时架构：https://learn.microsoft.com/zh-cn/dotnet/core/runtime-config/

**工具**
- **ILSpy**（免费开源反编译器）：https://github.com/icsharpcode/ILSpy
- **BenchmarkDotNet**：性能基准测试的事实标准
- **dotnet-counters / dotnet-dump**：命令行诊断工具
- **Rider 内置**：IL Viewer、Memory 分析器、dotMemory（付费）、dotTrace（付费）
- **VS 内置**：诊断工具（Diagnostic Tools）、性能探查器（Performance Profiler）

**Unity**
- Unity 官方性能优化手册：https://docs.unity3d.com/Manual/UnderstandingPerformance.html
- 《Unity 性能优化实战》
- Unity 官方 Learn 平台免费教程

**社区**
- .NET 官方博客（看设计动机）：https://devblogs.microsoft.com/dotnet/
- Stack Overflow：`[c#]` `[.net]` 标签
- r/csharp、r/dotnet

---

## 八、和你第一份计划的关系

- `C#学习计划.md`（上一份）：**保留**。它的阶段 1-3 语言语法部分依然有用，只是**顺序变了**——现在你是"边学机制边补语法"，而不是先学完语法再看机制。
- `Python对比C#速查.md`：**继续用**。学每个机制时对照着看。
- 本文件：**新的主干**。按这个走 20 周。

**一句话总结路线**：先搞清楚**内存和类型**（第 1-4 周），再搞清楚**分配与回收**（第 5-7 周），然后搞清楚**抽象的成本**（第 8-14 周），最后把这些知识**砸进 Unity**（第 18-20 周）。
