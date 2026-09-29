# InternalsLab —— C# 底层实验台

这不是教程代码，是**实验场**。每个实验都能跑出真实数字，用来验证你在文档里读到的结论。

> 配套文档：`../C#底层优先路线图.md`（学习顺序）和 `../C#底层核心知识精讲.md`（知识点详解）

---

## 一、怎么打开

**用 Rider（推荐）**
1. Rider → Open → 选择 `InternalsLab.sln`（或直接选 `InternalsLab` 文件夹）
2. 等待右下角进度条走完（首次会 restore NuGet 包，约 10 秒）
3. 按 `Ctrl + F5`（不调试运行）或 `Shift + F10`
4. 控制台出现菜单，输入编号回车

**用 Visual Studio 2022**
1. VS → 打开项目或解决方案 → 选 `InternalsLab.sln`
2. 右键 `Experiment01` 项目 → 设为启动项目
3. `Ctrl + F5` 运行

> ⚠️ **不要同时用两个 IDE 打开这个目录**，会锁文件冲突。

**命令行**（如果 `dotnet` 命令能跑）
```powershell
cd InternalsLab
dotnet run --project Experiment01
```

---

## 二、实验清单

| 编号 | 实验 | 对应周 | 重要度 |
|---|---|---|---|
| 1 | 编译流水线：bin/obj 里有什么 | 第 1 周 | |
| 2 | JIT 只编译一次（预热的重要性） | 第 1 周 | |
| **3** | **值语义 vs 引用语义** | 第 2 周 | ★核心 |
| 4 | 对象在堆上占多少字节（内存对齐） | 第 2 周 | |
| 5 | 栈帧与递归（含 StackOverflow 演示） | 第 2 周 | |
| **6** | **装箱与拆箱：分配量化 + Unity 场景** | 第 3-4 周 | ★核心 |
| 7 | 引用相等 vs 值相等（Equals/GetHashCode 契约） | 第 3 周 | |
| **8** | **WeakReference：证明对象真的被回收了** | 第 5-6 周 | ★核心 |
| 9 | 分代、晋升、大对象堆、ArrayPool | 第 6 周 | |
| 10 | 字符串驻留 + UTF-16 vs Python 变长编码 | 第 7 周 | |
| **11** | **拼接 vs StringBuilder（数字说话）** | 第 7 周 | ★核心 |
| 12 | 闭包会分配对象 + 循环变量捕获的坑 | 第 8 周 | |
| 13 | List\<int\> vs ArrayList：装箱的真实代价 | 第 9 周 | |
| **14** | **await 之后线程会变吗** | 第 10 周 | ★核心 |

---

## 三、怎么用这个实验台（重要）

### ❌ 错误用法
按顺序把 1-14 跑一遍，看完输出，然后说"哦，懂了"。

### ✅ 正确用法：**跑 → 猜 → 改坏 → 再猜**

对每个实验做这四步：

1. **先跑一遍**，看输出
2. **看代码，猜为什么会这样**（先别读文件顶部的注释结论）
3. **故意改坏它**：
   - 把 `struct` 改成 `class`，看输出怎么变
   - 把 `int` 改成 `long`，看分配量变不变
   - 把循环次数从 1 万改成 100 万，看曲线是不是线性
   - 把 `GC.Collect()` 注释掉，看对象还活着吗
4. **再看注释里的结论**，验证你的猜测。猜错了就重点记下来 —— 猜错的地方才是你真正需要学的地方。

### 每个实验结束后做三件事

1. **打开 ILSpy**，反编译刚才那个实验的 dll，找到关键方法的 IL
   （比如实验 6 里找 `box` 指令，实验 12 里找 `<>c__DisplayClass`）
2. **在 Rider 里打断点**，看 Variables 面板里变量的实际内容
   （比如实验 3 看 `s1` 和 `c1` 在调试器里的展开方式完全不同）
3. **在文件顶部的注释里补充你自己的发现**
   ```
   // 【我的发现 第X周】把 byte[100000] 改成 byte[1000000] 之后，
   //   Gen0 GC 次数从 3 次变成 0 次，但 Gen2 从 0 变成 2 次 —— 确认进了 LOH。
   ```

---

## 三·五、一个真实的测量教训（请务必读）

这个实验台的第一版里，实验 6 的「重写 ToString 能不能避免装箱」是这样写的：

```csharp
private struct BadStruct  { public int X; }                                  // 没重写
private struct GoodStruct { public int X;
    public override string ToString() => $"GoodStruct({X})"; }               // 重写了
```

测量结果（Debug 模式）：

```
BadStruct.ToString()  ← 未重写         24.2 B/次
GoodStruct.ToString() ← 已重写         48.5 B/次     ← 反了！
```

**「重写了反而分配更多」** —— 结论被推翻了。原因不是装箱，而是：
`GoodStruct` 的 ToString 返回的字符串更长（`"GoodStruct(1)"` 14 个字符），
而 `BadStruct` 走 `ValueType.ToString()` 只返回类型名 `"E06_Boxing+BadStruct"`。
**字符串长度的差异（约 24 字节）远超装箱的成本（24 字节的盒子）**，
两个效应叠在一起，把真正想观察的东西掩盖了。

修正方式：让两个版本的 ToString **都只返回一个字符**，这样唯一的差异就是是否装箱。

**这个教训比实验本身更重要**，它是性能测量的第一条铁律：

> **没有严格对照组的性能测量，得出的结论经常是错的。**
> 变量必须只有一个。凡是会影响测量结果的因素（字符串长度、
> Debug/Release、预热、迭代次数、GC 时机），都要么固定住，要么单独测量。

所以：
- 看到"快了 3 倍"先问：**对照组是什么？变量控制住了吗？**
- 想不通为什么数字反了，去查**还有什么因素在影响它**
- 正式测量请用 BenchmarkDotNet，它替你处理了这些陷阱

（顺便：`GetHashCode` 是观察装箱最干净的窗口 —— 它不产生字符串，
所以未重写版本多出来的那 24 字节就是纯装箱成本。实验 6 的 A2 节就是这么设计的。）

---

## 四、关于内存数字的说明

你会看到很多 `GC.GetAllocatedBytesForCurrentThread()` 和 `GC.GetTotalMemory(true)`。理解它们的区别：

| API | 含义 | 用途 |
|---|---|---|
| `GC.GetAllocatedBytesForCurrentThread()` | 当前线程**累计分配**的字节数（只增不减） | 测量"这段代码产生多少垃圾" |
| `GC.GetTotalMemory(false)` | 当前**存活**的托管堆大小（不触发 GC） | 粗略看堆占用 |
| `GC.GetTotalMemory(true)` | 当前存活堆大小（**先强制 GC**） | 测量"对象实际占多少" |
| `GC.CollectionCount(n)` | 第 n 代 GC 发生的次数 | 测量"触发了几次停顿" |
| `GC.GetGCMemoryInfo()` | 堆大小、碎片、提交内存等 | 观察碎片化 |

**注意**：具体数字会随 .NET 版本、CPU、Debug/Release 模式变化。**不要记数字，要记量级和趋势**。比如"ArrayList 比 List\<int\> 慢 5-10 倍、多分配几百 MB"是可复现的结论；"ArrayList 用了 1234 ms"不是。

**另外**：Debug 模式下的代码没有经过优化，性能数字会比 Release 差很多。做性能结论时切换到 Release 模式重跑一遍。

---

## 五、当前项目的配置说明

`Experiment01.csproj`：
```xml
<TargetFramework>net9.0</TargetFramework>    <!-- 目标框架，对应你装的 SDK 9.0.203 -->
<ImplicitUsings>enable</ImplicitUsings>      <!-- 自动 using System 等常用命名空间 -->
<Nullable>enable</Nullable>                  <!-- 开启可空引用类型分析（强烈建议保持开启） -->
<AllowUnsafeBlocks>true</AllowUnsafeBlocks>  <!-- 仅用于 sizeof(T)，不要用来写指针代码 -->
<WarningLevel>9999</WarningLevel>            <!-- 打开所有警告 -->
<AnalysisLevel>latest</AnalysisLevel>        <!-- 用最新的代码分析器 -->
```

**动手任务**：把 `Nullable` 改成 `disable`，重新编译，看警告列表的变化。这也是第 1 周实验 1 的一部分。

---

## 六、你自己加实验的规范

每学一个新机制，加一个新文件，遵循这个模板：

```csharp
namespace Experiments;

/// <summary>
/// 第 X 周 · 实验 NN：一句话说明这个实验验证什么
/// </summary>
/// <remarks>
/// 结论（先自己跑一遍再看）：
///   1. ...
///   2. ...
/// </remarks>
public static class ENN_YourTopic
{
    public static void Run()
    {
        // 实验代码
    }
}
```

然后在 `Program.cs` 的菜单和 switch 里各加一行。

**命名规范**：`ENN_主题.cs`，例如 `E15_Reflection.cs`、`E16_SpanZeroAlloc.cs`。

---

## 七、下一步：这里没有的实验

以下内容值得你自己新建实验（路线图第 11-17 周）：

- **集合内部实现**：手写一个带扩容的 `MyList<T>`，手写一个简易哈希表
- **LINQ 与迭代器**：用 `yield return` 手写 `MyWhere` / `MySelect`，观察延迟执行
- **Span\<T\> 零分配**：把 CSV 解析从"split 出一堆字符串"改成零分配切片
- **反射与源生成器**：写一个属性驱动的序列化器，对比反射版和源生成版的性能
- **终结器与 Dispose**：写一个持有 `Marshal.AllocHGlobal` 的类，观察终结器时机

建议新建一个 `Experiment02` 项目（用 `dotnet new console -o Experiment02`，或 Rider 里右键解决方案 → 添加项目），把后续实验放在里面，保持 `Experiment01` 作为"基础篇"归档。
