# C# 学习计划（面向有 Python 基础的小白）

> 制定日期：本文件生成时
> 你的情况：会 Python（这是很大的优势）、C# 零基础、已装 Rider 2026.2.2 + Visual Studio 2022
> 目标：14 周后能独立写出结构清晰、可维护的 C# / .NET 项目

---

## 一、先说环境体检结果（我实际扫过你的机器）

| 项目 | 检测结果 | 说明 |
|---|---|---|
| .NET SDK | **9.0.203** | 可以编译 C# 13，够用 |
| .NET 运行时 | 8.0.15 / 9.0.4 | 多版本共存正常 |
| WPF / WinForms | 已安装（WindowsDesktop.App 8 + 9） | 想做桌面程序随时能开始 |
| ASP.NET Core | 已安装（AspNetCore.App 8 + 9） | 想做网站/接口随时能开始 |
| Visual Studio 2022 | 已安装 | 需确认装了「.NET 桌面开发」或「ASP.NET 开发」工作负载 |
| JetBrains Rider | 已安装（2026.2.2） | 开箱即用，无需额外工作负载 |

**结论：环境已经完全就绪，不要再折腾安装，直接开始写代码。**

### 关于 .NET 10 的建议
.NET 10 是 LTS（长期支持）版本，.NET 9 是 STS（支持周期较短）。**现在不用装**，你手上的 9.0.203 学习完全够。等 14 周课程走完、你确定要长期投入时再装 .NET 10（多个 SDK 可以并存，不冲突）。

### 关于两个 IDE 怎么用（重要，别踩坑）
- **主力用 Rider**：对 Python 转过来的人更友好（快捷键和 PyCharm 一脉相承），重构、调试、NuGet 管理都更省心，不需要配工作负载。
- **每个阶段至少用一次 VS2022**：因为国内企业招聘 JD 里 VS2022 出现频率更高，而且它看 UI 设计器（WPF/WinForms）体验更好。
- **绝对不要两个 IDE 同时打开同一个项目目录**：会互相锁文件、缓存打架，出现莫名其妙的报错。一次只开一个。

---

## 二、你的真实起点分析

### 你已经具备的优势（别浪费）
- **编程思维已成型**：变量、循环、函数、类、异常、模块，这些概念不用再学。
- **面向对象的基础**：Python 的 class 让你能直接理解 C# 的类，不是从零开始。
- **工具链经验**：pip → NuGet、venv → 项目文件、`if __name__ == "__main__"` → `Main` 方法，都有对应物。
- **调试经验**：断点、单步、看变量，这些技能可迁移。

### 你需要重建的心智模型（这是最大的坎）
Python 是**动态类型 + 解释执行 + 一切皆对象**；C# 是**静态类型 + 编译执行 + 值/引用分离**。前两周的挫败感基本全部来自这里：

| 你的 Python 直觉 | C# 的现实 | 影响程度 |
|---|---|---|
| 变量随时可以改变类型 | 变量类型**编译期锁定**，写错根本编译不过 | ★★★★★ |
| 不用声明类型，跑了才知道 | 必须声明类型，但编译器帮你抓 bug | ★★★★★ |
| 整数和对象都是"引用" | `int`/`struct` 是**值类型**，赋值是复制 | ★★★★★ |
| 缩进决定代码块 | **花括号 `{}` + 分号 `;`** 决定一切 | ★★★★ |
| 不用管内存 | 有 GC，但 `IDisposable` 需要你手动释放 | ★★★★ |
| 拼写错了运行时才报错 | 拼写错了打字的瞬间就标红 | ★★★ |
| 一个文件一个模块 | 项目/解决方案/命名空间三层结构 | ★★★ |
| `None` | `null`，而且有**可空引用类型**体系 | ★★★★ |
| `async/await` 是语法糖 | 也是语法糖，但**线程模型**完全不同 | ★★★ |

**心态提醒**：前 5 天你会觉得"这语言怎么这么啰嗦"，这是正常的。第 10 天左右你会开始体会到静态类型的好处——IDE 能帮你重命名、跳转、找出所有调用点，这是 Python 难以做到的。

---

## 三、总体路线图（14 周，每天 1.5~2 小时）

```
阶段 0  环境与心智转换        第 1 周      ← 最枯燥但最关键
阶段 1  C# 语言核心          第 2-4 周
阶段 2  面向对象与类型系统    第 5-7 周
阶段 3  .NET 进阶特性         第 8-10 周
阶段 4  方向深化（三选一）    第 11-13 周
阶段 5  综合项目与复盘        第 14 周
```

**如果每天只有 1 小时**：把上面每一周 ×1.5，总时长约 21 周。
**如果每天能投入 3 小时**：把上面每两周压成一周，总时长约 8 周。**但阶段 0 和阶段 1 不要压缩，那是地基。**

---

## 阶段 0：环境熟悉与心智转换（第 1 周）

**目标**：不再怕 IDE，能独立新建、运行、调试一个控制台项目。

### Day 1-2：项目结构三件套
搞明白这三个词的区别（Python 里没有对应物，必须单独理解）：
- **Solution（.sln）**：一个"总文件夹"，管多个项目。类似一个 Git 仓库根目录。
- **Project（.csproj）**：一个可编译单元，产出 exe 或 dll。类似 Python 里一个包。
- **Namespace**：代码里的逻辑分组，类似 Python 的模块路径。

动手做：
1. Rider → New Solution → Console Application，目标框架选 **net9.0**
2. 手动打开 `.csproj` 文件读一遍（就十几行 XML，看清楚 `TargetFramework`、`Nullable`、`ImplicitUsings` 三个节点）
3. 按 `Ctrl+F5`（VS）或绿色三角（Rider）运行，看到输出
4. 在 `Console.WriteLine` 那行打个断点，F9/F5 调试，把变量加进 Watch 窗口
5. 同一个项目用 VS2022 打开一次，重复上面的操作，熟悉两边的界面差异

> 记住：`dotnet run` 等价于 `python main.py`，`dotnet build` 等价于"编译检查"，`dotnet test` 等价于 `pytest`。

### Day 3-4：文件结构与编译概念
- 建一个含两个项目的 Solution（一个类库 + 一个控制台程序），让控制台引用类库，调用里面的方法
- 理解为什么 C# 要编译：编译期就能发现 90% 的低级错误，这是它相对 Python 的最大优势
- 学会看错误列表（Errors）和警告列表（Warnings），**警告不要忽略**，尤其是 CS8618（未初始化非空属性）

### Day 5-7：小练习（都要动键盘，不要只看）
1. 写一个"BMI 计算器"：读控制台输入 → 解析成 double → 计算 → 格式化输出
2. 写一个"猜数字游戏"：随机数 + 循环 + 输入校验（用 `int.TryParse`，不用 try/catch）
3. 写一个"九九乘法表"，练习 `for` 循环和字符串插值 `$"{a} * {b} = {a*b}"`

**阶段验收**：不看教程，从零新建一个控制台项目，写一个能读输入、做判断、循环、输出结果的小程序。

---

## 阶段 1：C# 语言核心（第 2-4 周）

**目标**：把 Python 语法肌肉记忆改写成 C# 版本。这一阶段重点是**反复对比**，详见另一份文件 `Python对比C#速查.md`。

### 第 2 周：类型系统与基础语法
- 内置类型：`int` `long` `double` `decimal` `bool` `char` `string`（重点：**金融计算用 `decimal` 不用 `double`**）
- `var` 关键字：类型推断，但依然是静态类型，不是 Python 的动态变量
- 值类型 vs 引用类型：`int a = 1; int b = a; b = 2;` → a 还是 1（Python 程序员最容易错的地方）
- 字符串：**不可变**、`string.Format`、`$""` 插值、`@""` 逐字字符串、`StringBuilder`
- 类型转换：`int.Parse` vs `int.TryParse` vs `Convert.ToInt32` vs 强制转换 `(int)`
- 运算符与空值：`??`（空合并）、`?.`（空条件）、`??=`（空合并赋值）
- 常量与只读：`const` vs `readonly` vs `static readonly`

**练习**：写一个命令行计算器，支持四则运算 + 除零错误处理 + 非法输入处理。**必须用 `TryParse` 而不是 try/catch。**

### 第 3 周：流程控制、方法、数组与集合
- `if/else`、`switch` 语句 + **`switch` 表达式**（C# 8+ 的现代写法，很优雅）
- 循环：`for` / `while` / `do-while` / `foreach`；`break` / `continue`
- 方法：参数、返回值、`out` / `ref` / `in` 参数、可选参数、命名参数、**方法重载**
- 数组 `int[]`：**长度固定**，和 Python list 完全不同
- 集合三件套：`List<T>`（≈Python list）、`Dictionary<K,V>`（≈dict）、`HashSet<T>`（≈set）
- `IEnumerable<T>` 是什么：可以 foreach 的东西，理解它才能理解 LINQ

**练习**：学生成绩管理（控制台）：用 `List<Student>` 存数据，能增删查改、按分数排序、算平均分最高分。

### 第 4 周：异常处理、命名空间、.NET 基础库
- `try/catch/finally`、异常筛选器 `when`、自定义异常类
- **异常不要用来做流程控制**（这点和 Python 习惯一致）
- 命名空间与 `using` 指令；`using` 声明（自动释放资源，和 Python 的 `with` 神似）
- 文件操作：`File.ReadAllText` / `File.WriteAllLines`，`Path.Combine`
- 日期时间：`DateTime` / `DateTimeOffset` / `TimeSpan`，**格式化用 `ToString("yyyy-MM-dd")`**
- 常用静态类：`Math`、`Console`、`Environment`、`Guid`

**练习**：把第 3 周的成绩管理改造成"数据持久化版"——启动时从 txt/csv 读入，退出时写回。这一步会让你第一次体会到"程序"而不是"脚本"。

**阶段验收**：能独立写 300 行左右的多文件控制台程序，会拆方法、会用集合、会做文件读写和异常处理。

---

## 阶段 2：面向对象与类型系统（第 5-7 周）

**目标**：这是 C# 的主战场。Python 的 OOP 是"轻量级"的，C# 的 OOP 是"重型武器"，必须学扎实。

### 第 5 周：类与对象
- 类、字段、方法、构造函数、`this`
- **属性（Property）**：`public string Name { get; set; }` —— 这是 C# 独有的重头戏，Python 的 `@property` 只是它的简化版
- 访问修饰符：`public` / `private` / `protected` / `internal`（Python 的 `_` 前缀只是约定，C# 是**编译器强制**）
- 静态成员、静态构造函数、`static class`
- 对象初始化器 `new Person { Name = "Tom" }`
- `record` 类型（C# 9+）：一行搞定不可变数据类，极其实用
- `struct` vs `class`：什么时候用值类型（小、不可变、生命周期短）

**练习**：设计一个"银行账户"体系——账户类、交易记录类、余额校验逻辑。重点练属性的 get/set 校验。

### 第 6 周：继承、接口、多态
- 继承：`:` 语法、`base()` 调用、`virtual` / `override` / `new`
- `abstract` 抽象类 vs `interface` 接口：**核心区别要背下来**（抽象类表达"是什么"，接口表达"能做什么"；单继承 vs 多实现）
- 接口的现代用法：默认实现、显式实现
- 多态：父类引用指向子类对象，运行时决定调用哪个方法
- 密封类 `sealed`、`object` 基类、`ToString` / `Equals` / `GetHashCode` 的正确重写
- **组合优于继承**——这个原则在两个语言里都成立

**练习**：设计"图形面积计算"——抽象基类 `Shape`，多个实现（圆/矩形/三角形），一个统一的面积计算入口。再加一个 `IComparable` 实现用于排序。

### 第 7 周：泛型、委托、事件、Lambda
- **泛型 `<T>`**：`List<T>` 你已经用了，现在学会自己写 `Repository<T>`。理解为什么泛型比 `object` 好（类型安全 + 无装箱）
- 泛型约束：`where T : class` / `where T : new()` / `where T : IComparable<T>`
- **委托（delegate）**：`Action` / `Func<T>` / `Predicate<T>`，这就是 C# 的函数指针
- **Lambda 表达式**：`x => x * 2`，和 Python 的 `lambda x: x*2` 对应
- **事件（event）**：发布订阅模式，理解 `EventHandler` 和 `EventArgs`
- 扩展方法：`public static void Foo(this string s)` —— 给已有类型"加"方法，非常好用

**练习**：写一个"文件搜索工具"：用委托/Lambda 传入筛选条件（按大小、按扩展名、按修改时间），用事件在找到文件时通知订阅者。

**阶段验收**：能说清 class / struct / interface / abstract / record 各自的使用场景，能自己写泛型类，能用 Lambda 和委托代替大量 if-else。

---

## 阶段 3：.NET 进阶特性（第 8-10 周）

### 第 8 周：LINQ（语言集成查询）
这是 C# 最有生产力的特性，**必须练到形成条件反射**。
- 查询语法 `from x in list where ... select ...` vs 方法语法 `list.Where().Select()`
- 核心算子：`Where` / `Select` / `OrderBy` / `GroupBy` / `Join` / `FirstOrDefault` / `Any` / `All` / `Sum` / `Count` / `Distinct` / `Take` / `Skip`
- **延迟执行（Deferred Execution）**：这是最大的坑，`IEnumerable` 在 foreach 时才真正执行
- `IEnumerable<T>` vs `IQueryable<T>` vs `List<T>` 的区别（做数据库时极其重要）
- LINQ 的等价物：Python 的列表推导 + `filter` + `map` + `itertools`，但功能强得多

**练习**：给一份（自己造的）销售数据，用 LINQ 做：月度汇总、Top 10 商品、按客户分组、同比环比。**全部用 LINQ 写，不要用 for 循环。**

### 第 9 周：异步编程与并发
- `async` / `await`：和 Python 的写法神似，但底层是**状态机 + Task**，不是协程
- `Task` / `Task<T>` / `ValueTask`，`Task.Run` 用于 CPU 密集任务
- **`async void` 只在事件处理器里用**，其他一律 `async Task`（这是经典事故来源）
- `CancellationToken`：可取消的异步操作
- 并发集合、`lock`、`Interlocked`
- **死锁与 `ConfigureAwait(false)`**：库代码里要加，UI 代码里不能加

**练习**：写一个"并发下载器"：给定一批 URL，用 `Task.WhenAll` 并发下载，显示进度，支持 `Ctrl+C` 取消。

### 第 10 周：工程化与测试
- **NuGet**：就是 pip。`dotnet add package Newtonsoft.Json` ≈ `pip install`
- `System.Text.Json`：内置 JSON 序列化/反序列化（重点练）
- **依赖注入（DI）**：.NET 的核心设计模式，`IServiceCollection` / `AddSingleton` / `AddScoped` / `AddTransient`
- **配置系统**：`appsettings.json` + `IConfiguration` + Options 模式
- **日志**：`ILogger<T>`，结构化日志
- **单元测试**：xUnit（推荐）+ `Assert` + `[Fact]` / `[Theory]` + Mock（Moq 或 NSubstitute）
  - 对照：xUnit ≈ pytest，`[Fact]` ≈ `def test_xxx()`
- **可空引用类型**：`string?` vs `string`，编译期防空指针，这是 C# 8 之后的重大改进

**练习**：给第 4 周的成绩管理项目写 15 个以上单元测试，覆盖率尽量上 70%。用 `dotnet test` 跑。

**阶段验收**：能用 LINQ 重构旧代码；能写出正确的异步方法；能给项目加配置文件、日志和单元测试。

---

## 阶段 4：方向深化（第 11-13 周，三选一）

到这里 C# 语言本身已经学完了，接下来是"用它做什么"。**强烈建议三选一，不要都学。**

### 方向 A：Web 后端 / API（最推荐，岗位最多）
**适合**：想找工作、想做服务端、想转 .NET 开发岗。

- **ASP.NET Core Web API**：Controller、路由、模型绑定、模型验证
- **Minimal API**：更简洁的写法，适合小服务
- **Entity Framework Core**：ORM，对照 Python 的 SQLAlchemy / Django ORM
  - `DbContext`、`DbSet`、迁移（Migrations）、LINQ to SQL
  - **必须理解 N+1 查询问题**（`Include` 的用法）
- **RESTful 设计**：状态码、资源命名、分页、过滤、排序
- **认证授权**：JWT、`[Authorize]`、策略授权
- **Swagger / OpenAPI**：自动生成接口文档
- **中间件管道**：`app.UseXxx()` 的执行顺序

**实战项目**：一个完整的"待办事项 API"或"博客 API"，包含用户注册登录、JWT 鉴权、EF Core + SQLite/PostgreSQL、分页查询、全局异常处理、Swagger 文档、单元测试。

---

### 方向 B：桌面应用（对你现有技能最友好，见效最快）
**适合**：想给自己/公司写 Windows 工具、想做信息化系统。

- **WPF**（推荐，比 WinForms 现代）
  - XAML 布局：`Grid` / `StackPanel` / `DockPanel`
  - **数据绑定（DataBinding）**：核心中的核心，`INotifyPropertyChanged`
  - **MVVM 模式**：Model-View-ViewModel，理解 View 和逻辑如何分离
  - `ICommand`、`ObservableCollection<T>`
  - 样式、模板、触发器、资源字典
- **WinForms**（简单老派，公司里存量代码多，学 2 天够用）
- 可选：**Avalonia**（跨平台版 WPF，能跑 Linux/Mac）

**实战项目**：一个"个人记账本"或"文件批量重命名工具"，要求 MVVM 架构、数据绑定、本地 SQLite 存储、导出 Excel/CSV。

---

### 方向 C：游戏开发（Unity）
**适合**：明确想做游戏。

- **Unity + C#**：GameObject / Component 模型、`MonoBehaviour` 生命周期（`Awake`/`Start`/`Update`/`FixedUpdate`）
- 输入系统、物理系统、碰撞检测
- 协程（Coroutine）与异步的区别
- UI 系统、场景管理、ScriptableObject
- 性能优化：对象池、避免每帧 GC

**注意**：Unity 用的是自己的 .NET 版本（Mono/IL2CPP），部分最新 C# 特性可能不支持，且阶段 3 的异步/DI 知识用得较少。**走这条路的话，阶段 3 可以适当减负。**

---

## 阶段 5：综合项目与复盘（第 14 周）

### 做一个完整项目（不要再做小练习）
选一个你**真的想用**的东西，要求：
- 至少 3 层结构（表现层 / 业务逻辑层 / 数据访问层）
- 至少有 1 个接口 + 2 个实现（练多态和 DI）
- 有单元测试（≥20 个）
- 有配置文件 + 日志
- 有 README，说明怎么运行
- 用 Git 管理，**每一笔功能一个 commit**，不要一次性提交

推荐选题：
1. **命令行 RSS 阅读器**：抓取、解析、存储、LINQ 筛选、导出 Markdown
2. **本地文件整理工具**：按规则自动分类文件，支持预览和撤销
3. **简易库存管理系统**（WPF 版）：适合方向 B
4. **RESTful 待办 API**（带 JWT 和 EF Core）：适合方向 A

### 复盘清单
- [ ] 哪些地方我还在用 Python 的思维写 C#？（列出来专门改）
- [ ] 我的代码里有没有 `async void`、`.Result`、`.Wait()`？（有就改掉）
- [ ] 我的类里属性用对了吗？有没有把字段直接暴露成 `public`？
- [ ] 我能不能说清 `IEnumerable` 和 `List` 的区别？
- [ ] 我的异常处理是不是在"吞异常"（catch 了但什么都不做）？

### 迁移练习（检验真本事）
把你自己写过的一个 Python 脚本（100~300 行），**完整重写成 C#**。这是最快的查漏补缺方式。

---

## 四、常见坑清单（Python 转 C# 必踩）

| # | 坑 | 正确做法 |
|---|---|---|
| 1 | 以为 `int a = b` 是引用赋值 | 值类型是**复制**，改 b 不影响 a |
| 2 | 用 `double` 算钱 | 用 `decimal`，否则 `0.1+0.2 != 0.3` 的翻版会找上你 |
| 3 | `async void` 到处用 | 只有事件处理器能用；其他写 `async Task` |
| 4 | 用 `.Result` / `.Wait()` 同步等异步 | 死锁高发区，一路 `await` 到底 |
| 5 | `catch (Exception) { }` 吞掉异常 | 至少记日志；能处理的才 catch |
| 6 | 忽略编译器警告（黄色波浪线） | 尤其 CS8618/CS0168，它们是 bug 的前身 |
| 7 | 字符串循环里用 `+=` 拼接 | 用 `StringBuilder` 或 `string.Join` |
| 8 | 用 `==` 比较字符串内容 | C# 的 `==` 对 string 已重载为值比较，但**自定义类要重写 `Equals`** |
| 9 | 直接暴露 `public string name;` | 用属性 `public string Name { get; set; }` |
| 10 | 集合遍历时修改集合 | 会抛 `InvalidOperationException`，先 `ToList()` 再遍历 |
| 11 | 以为 `List` 能像 Python list 存任意类型 | 泛型是强类型，要存混合类型得用基类或 `object` |
| 12 | 命名用下划线 `my_var` | C# 惯例是 PascalCase（公开成员）/ camelCase（局部变量） |
| 13 | 忘了 `IDisposable` 要用 `using` | 文件、连接、流都要 `using`，等价 Python 的 `with` |
| 14 | LINQ 查询写了但没执行 | LINQ 是**延迟执行**，要 `ToList()` 才真正查询 |
| 15 | 在一个 IDE 里改了，另一个 IDE 报错 | 不要同时开两个 IDE 打开同一目录 |

---

## 五、学习方法建议（针对你的情况）

1. **对比学习法（最高效）**：每学一个 C# 概念，先问"Python 里对应什么？哪里不同？"。配套文件 `Python对比C#速查.md` 就是干这个用的。
2. **不要只看视频**：输入 >> 阅读 >> 观看。每天至少敲 40 分钟代码，哪怕是把示例重敲一遍。
3. **读官方文档，不要只读博客**：`learn.microsoft.com/dotnet/csharp` 质量极高，且是中文的。遇到概念先查官方。
4. **让编译器教你**：故意写错类型，看编译器报什么错。错误信息是最好的老师。
5. **IDE 是你的老师**：看到波浪线就按 `Alt+Enter`（Rider）/ `Ctrl+.`（VS）看它建议怎么改，这是免费的架构课。
6. **每周末输出一篇笔记**：用自己的话写下这周学了什么、踩了什么坑。写不出来说明没学会。
7. **代码库用 Git 从第一天开始**：`git init` → 每周至少 3 次 commit。以后能回看自己的成长轨迹。

---

## 六、推荐资源

### 官方（首选）
- C# 官方文档（中文）：https://learn.microsoft.com/zh-cn/dotnet/csharp/
- .NET 官方文档（中文）：https://learn.microsoft.com/zh-cn/dotnet/
- C# 语言规范与特性版本：https://learn.microsoft.com/zh-cn/dotnet/csharp/whats-new/
- ASP.NET Core 文档：https://learn.microsoft.com/zh-cn/aspnet/core/
- EF Core 文档：https://learn.microsoft.com/zh-cn/ef/core/
- WPF 文档：https://learn.microsoft.com/zh-cn/dotnet/desktop/wpf/

### 交互式练习（碎片时间用）
- Microsoft Learn 免费学习路径：https://learn.microsoft.com/zh-cn/training/paths/csharp-first-steps/
- Exercism C# 轨道（有导师批改）：https://exercism.org/tracks/csharp
- LeetCode（用 C# 刷，练 LINQ 和集合）

### 视频与书籍
- **《C# 图解教程》**：中文，图多，最适合零基础转过来的人
- **《深入理解 C#》（Jon Skeet）**：进阶必读，讲清语言设计动机
- **《CLR via C#》**：想深入运行时看，前期不用碰
- Tim Corey 的 YouTube 频道：英文，但讲解极清晰，配字幕看

### 速查
- C# 与 Python 语法对照：见同目录 `Python对比C#速查.md`
- .NET API 浏览器：https://learn.microsoft.com/zh-cn/dotnet/api/

---

## 七、每周打卡表（建议复制到自己的笔记里）

| 周 | 主题 | 完成情况 | 本周最大收获 | 本周卡住的点 |
|---|---|---|---|---|
| 1 | 环境与心智转换 | ☐ | | |
| 2 | 类型系统与基础语法 | ☐ | | |
| 3 | 流程控制、方法、集合 | ☐ | | |
| 4 | 异常、命名空间、基础库 | ☐ | | |
| 5 | 类与对象、属性 | ☐ | | |
| 6 | 继承、接口、多态 | ☐ | | |
| 7 | 泛型、委托、事件、Lambda | ☐ | | |
| 8 | LINQ | ☐ | | |
| 9 | 异步与并发 | ☐ | | |
| 10 | NuGet、DI、测试 | ☐ | | |
| 11 | 方向深化（一） | ☐ | | |
| 12 | 方向深化（二） | ☐ | | |
| 13 | 方向深化（三） | ☐ | | |
| 14 | 综合项目与复盘 | ☐ | | |

---

## 八、判断自己学会了的标准

**第 4 周结束时**：能独立写文件读写 + 集合操作的控制台程序。
**第 7 周结束时**：能说清"什么时候用 interface，什么时候用 abstract class"。
**第 10 周结束时**：看别人的 C# 代码，能看懂 80%，能指出哪里写得不好。
**第 14 周结束时**：给一个需求，你能自己拆解、设计、写出来、测试、发布。

**最后一句**：你现在最大的敌人不是 C# 难，而是"Python 已经会了，所以看 C# 觉得啰嗦"这种心态。前两周忍住这种不适感，后面会豁然开朗——静态类型带来的重构信心、IDE 智能提示的准确度、编译期发现 90% 错误的安全感，是 Python 给不了的。**动手写，别只读。**
