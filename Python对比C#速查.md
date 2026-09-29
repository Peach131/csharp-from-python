# Python → C# 速查对照表

> 用法：学新概念时先在这里找对应关系，再看 C# 的具体写法。
> 记住核心差异：**Python 是动态类型 + 解释执行；C# 是静态类型 + 编译执行。**

---

## 1. 项目与工具链

| 用途 | Python | C# / .NET |
|---|---|---|
| 包管理器 | pip | NuGet |
| 依赖清单 | requirements.txt / pyproject.toml | `*.csproj` 里的 `<PackageReference>` |
| 虚拟环境 | venv / conda | 无对应物（SDK 版本由 `global.json` 控制） |
| 安装包 | `pip install requests` | `dotnet add package RestSharp` |
| 还原依赖 | `pip install -r requirements.txt` | `dotnet restore` |
| 运行 | `python main.py` | `dotnet run` |
| 构建/检查 | `python -m compileall .` | `dotnet build` |
| 测试 | `pytest` | `dotnet test`（常用 xUnit） |
| 交互式 | `python` REPL | `dotnet script`（需额外安装）或 Rider 的 C# Interactive |
| 打包发布 | PyInstaller / wheel | `dotnet publish -c Release` |
| 代码格式化 | black | `dotnet format` |
| 静态检查 | mypy / flake8 | 编译器本身 + Roslyn 分析器 |
| 项目入口 | `if __name__ == "__main__":` | `static void Main(string[] args)` 或顶级语句 |

**项目结构三层**（Python 没有直接对应）：
```
MySolution.sln          ← 解决方案：管理多个项目（≈ 仓库根目录）
└─ MyApp/MyApp.csproj   ← 项目：一个编译单元（≈ 一个包）
   └─ Program.cs        ← 源文件（≈ 一个模块）
```
命名空间（namespace）≈ Python 的模块路径，用 `.` 分隔。

---

## 2. 变量与类型

| 场景 | Python | C# |
|---|---|---|
| 声明整数 | `x = 5` | `int x = 5;` |
| 类型推断 | 天生动态 | `var x = 5;`（仍是静态类型，只是省略书写） |
| 重新赋不同类型的值 | `x = "hello"` ✅ | ❌ 编译错误 |
| 整数 | `int`（任意精度） | `int`(32位) / `long`(64位) / `short` / `byte` |
| 浮点 | `float`（双精度） | `float`(需加 f) / `double`（默认）/ **`decimal`（金融用）** |
| 布尔 | `True` / `False` | `true` / `false` |
| 字符 | 长度为 1 的字符串 | `char c = 'A';`（单引号，独立类型） |
| 字符串 | `"abc"` | `"abc"`（**不可变**） |
| 空值 | `None` | `null`（配合可空类型 `int?` / `string?`） |
| 常量 | 约定全大写 | `const int Max = 100;` / `readonly`（运行时确定） |
| 类型判断 | `type(x) == int` / `isinstance` | `x is int` / `x.GetType() == typeof(int)` |
| 类型转换 | `int("5")` | `int.Parse("5")` / `int.TryParse("5", out var n)` |

**关键差异**：
```python
# Python：值语义靠对象，赋值是引用
a = [1, 2]
b = a
b.append(3)
print(a)   # [1, 2, 3]  ← a 也变了
```
```csharp
// C#：值类型复制，引用类型共享
int a = 1;
int b = a;
b = 2;
Console.WriteLine(a);      // 1  ← 没变！

var listA = new List<int> { 1, 2 };
var listB = listA;         // 引用类型：共享同一对象
listB.Add(3);
Console.WriteLine(listA.Count);  // 3  ← 变了
```

---

## 3. 语法形式

| 项目 | Python | C# |
|---|---|---|
| 代码块 | 缩进 | `{ }` 花括号 |
| 语句结束 | 换行 | `;` 分号 |
| 注释 | `# 注释` | `// 单行` / `/* 多行 */` |
| 文档字符串 | `"""docstring"""` | `/// <summary>文档</summary>` |
| 字符串插值 | `f"{name} is {age}"` | `$"{name} is {age}"` |
| 多行字符串 | `"""..."""` | `@"..."`（逐字）或 `"""..."""`（C# 11 原始字符串） |
| 拼接 | `"a" + "b"` | `"a" + "b"` 或 `string.Concat` |
| 相等比较 | `==`（值比较） | `==`（值类型比值；string 已重载；自定义类需重写 `Equals`） |
| 恒等比较 | `is` | `ReferenceEquals(a, b)` |
| 逻辑运算 | `and` / `or` / `not` | `&&` / `\|\|` / `!` |
| 三元 | `a if cond else b` | `cond ? a : b` |
| 空合并 | `a if a is not None else b` | `a ?? b` |
| 空条件 | `a.b if a else None` | `a?.b` |
| 成员访问 | `obj.attr` | `obj.Attr`（属性）/ `obj.Method()` |
| 长度 | `len(x)` | `x.Length`（数组/字符串）/ `x.Count`（集合） |

---

## 4. 流程控制

```python
# Python
if x > 10:
    print("big")
elif x > 5:
    print("mid")
else:
    print("small")

for i in range(10):
    print(i)

for item in items:
    print(item)

for i, item in enumerate(items):
    print(i, item)

while x > 0:
    x -= 1
```
```csharp
// C#
if (x > 10) { Console.WriteLine("big"); }
else if (x > 5) { Console.WriteLine("mid"); }
else { Console.WriteLine("small"); }

for (int i = 0; i < 10; i++) { Console.WriteLine(i); }

foreach (var item in items) { Console.WriteLine(item); }

for (int i = 0; i < items.Count; i++) { Console.WriteLine($"{i} {items[i]}"); }

while (x > 0) { x--; }
```

**switch 语句（C# 8+ 的表达式写法，非常优雅）**：
```csharp
string Describe(int n) => n switch
{
    0 => "zero",
    < 0 => "negative",
    > 0 and < 10 => "small positive",
    _ => "large positive"      // _ 是默认分支，类似 Python 的 case _
};
```

---

## 5. 函数 / 方法

```python
# Python
def add(a, b=10, *args, **kwargs):
    return a + b

result = add(1, b=2)
```
```csharp
// C#
int Add(int a, int b = 10)   // 可选参数
{
    return a + b;
}

var result = Add(1, b: 2);   // 命名参数

// 表达式体方法（单行简写）
int Add2(int a, int b) => a + b;

// 方法重载（Python 没有，靠默认参数/类型判断代替）
int Add(int a, int b) => a + b;
double Add(double a, double b) => a + b;

// out / ref 参数（Python 靠返回值元组代替）
bool TryParse(string s, out int value) { ... }
```

| 概念 | Python | C# |
|---|---|---|
| 默认参数 | `def f(a=1)` | `void F(int a = 1)` |
| 关键字参数 | `f(a=1)` | `F(a: 1)` |
| 可变参数 | `*args` | `params int[] args` |
| 多返回值 | `return a, b` | 元组 `return (a, b);` 或 `out` 参数 |
| 匿名函数 | `lambda x: x*2` | `x => x * 2` |
| 函数作为参数 | 直接传 | `Action` / `Func<T>` 委托 |
| 装饰器 | `@decorator` | 特性 `[Attribute]` + AOP / 中间件（机制不同） |

---

## 6. 数据结构

| Python | C# | 备注 |
|---|---|---|
| `list` | `List<T>` | 动态数组，最常用 |
| `tuple` | `(int, string)` 值元组 / `ValueTuple` | 轻量 |
| `dict` | `Dictionary<K,V>` | 键值对 |
| `set` | `HashSet<T>` | 去重集合 |
| `collections.deque` | `LinkedList<T>` / `Queue<T>` | 队列用 `Queue<T>` |
| `heapq` | `PriorityQueue<T,P>`（.NET 6+） | 优先队列 |
| 固定数组 | `int[]` | **长度不可变** |
| 不可变列表 | `ImmutableList<T>` | 需装 NuGet 包 |
| 有序字典 | `SortedDictionary<K,V>` | 按 key 排序 |

```csharp
// 集合初始化
var list = new List<int> { 1, 2, 3 };
var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
var set = new HashSet<string> { "x", "y" };

// 常用操作对照
list.Add(4);                  // list.append(4)
list.Remove(1);               // list.remove(1)
list.Contains(2);             // 2 in list
list.Count;                   // len(list)
dict.ContainsKey("a");        // "a" in dict
dict.TryGetValue("a", out var v);  // v = dict.get("a") 的安全版本
dict["a"];                    // dict["a"]（键不存在会抛异常，不是 KeyError）
list[0];                      // list[0]
list[^1];                     // list[-1]  ← 从末尾索引
list[1..3];                   // list[1:3] ← 切片（返回新 List）
```

---

## 7. 字符串

```csharp
string s = "Hello World";

s.Length                  // len(s)
s.ToUpper()               // s.upper()
s.ToLower()               // s.lower()
s.Trim()                  // s.strip()
s.Split(',')              // s.split(',')
string.Join(",", list)    // ",".join(list)
s.Replace("a", "b")       // s.replace("a", "b")
s.Contains("ell")         // "ell" in s
s.StartsWith("He")        // s.startswith("He")
s.IndexOf("o")            // s.find("o")
s.Substring(0, 5)         // s[0:5]
s[0]                      // s[0]
$"{name} is {age}"        // f"{name} is {age}"
s == "Hello World"        // 值比较，等价 Python 的 ==
```

**重要**：C# 字符串**不可变**，循环里 `s += x` 性能极差，用：
```csharp
var sb = new StringBuilder();
foreach (var item in items) sb.Append(item);
var result = sb.ToString();
```

---

## 8. 面向对象

```python
# Python
class Animal:
    def __init__(self, name):
        self.name = name

    def speak(self):
        raise NotImplementedError

class Dog(Animal):
    def speak(self):
        return f"{self.name}: Woof"
```
```csharp
// C#
public abstract class Animal
{
    public string Name { get; }        // 只读属性（构造函数赋值）

    protected Animal(string name) => Name = name;

    public abstract string Speak();    // 抽象方法：子类必须实现
}

public class Dog : Animal
{
    public Dog(string name) : base(name) { }

    public override string Speak() => $"{Name}: Woof";
}
```

| 概念 | Python | C# |
|---|---|---|
| 定义类 | `class Foo:` | `class Foo { }` |
| 构造函数 | `def __init__(self)` | `public Foo() { }` |
| 实例引用 | `self`（显式） | `this`（隐式，只在需要时写） |
| 属性访问 | 直接 `obj.attr` | 属性 `obj.Attr { get; set; }` |
| 私有成员 | `_name`（约定） | `private string _name;`（**编译器强制**） |
| 继承 | `class B(A):` | `class B : A` |
| 调用父类 | `super().__init__()` | `base(...)` |
| 接口 | 抽象基类 ABC / 鸭子类型 | `interface IFoo { }`，`class B : IFoo` |
| 多重继承 | ✅ 支持 | ❌ 类单继承，但可**多实现接口** |
| 抽象方法 | `@abstractmethod` | `abstract` |
| 重写 | 直接覆盖 | 父类 `virtual`，子类 `override` |
| 密封 | 无 | `sealed` |
| 静态方法 | `@staticmethod` | `static` |
| 类方法 | `@classmethod` | `static`（无直接对应，靠工厂模式） |
| 特殊方法 | `__str__` | `public override string ToString()` |
| 相等比较 | `__eq__` | `public override bool Equals(object? obj)` |
| 哈希 | `__hash__` | `public override int GetHashCode()` |
| 数据类 | `@dataclass` | `public record Person(string Name, int Age);` |
| 属性描述符 | `@property` | 属性 `{ get => ...; set => ...; }` |

**访问修饰符对照**（Python 没有真正的强制）：
| C# | 可见范围 | Python 类比 |
|---|---|---|
| `public` | 所有人 | `name` |
| `private` | 仅本类 | `__name`（名字改写，仍可绕过） |
| `protected` | 本类 + 子类 | `_name`（约定） |
| `internal` | 同一程序集（同一项目） | 无对应 |
| `protected internal` | 上面两者并集 | 无对应 |

---

## 9. 集合操作：推导式 vs LINQ

```python
# Python
result = [x * 2 for x in nums if x > 0]
names = [u.name for u in users]
total = sum(u.age for u in users)
even = [x for x in nums if x % 2 == 0]
grouped = {}
for u in users:
    grouped.setdefault(u.dept, []).append(u)
```
```csharp
// C# LINQ
var result = nums.Where(x => x > 0).Select(x => x * 2).ToList();
var names = users.Select(u => u.Name).ToList();
var total = users.Sum(u => u.Age);
var even = nums.Where(x => x % 2 == 0).ToList();
var grouped = users.GroupBy(u => u.Dept).ToDictionary(g => g.Key, g => g.ToList());

// 查询语法（另一种写法，等价）
var query = from u in users
            where u.Age > 18
            orderby u.Name
            select new { u.Name, u.Age };   // 匿名类型，类似 namedtuple
```

**常用 LINQ 算子对照**：
| 目的 | Python | C# LINQ |
|---|---|---|
| 过滤 | `filter` / 推导式 | `Where` |
| 映射 | `map` / 推导式 | `Select` |
| 求和 | `sum` | `Sum` |
| 计数 | `len([...])` | `Count()` |
| 任意一个满足 | `any(...)` | `Any(predicate)` |
| 全部满足 | `all(...)` | `All(predicate)` |
| 存在判断 | `x in list` | `Contains(x)` |
| 第一个 | `next(gen)` | `First()` / `FirstOrDefault()` |
| 排序 | `sorted(key=...)` | `OrderBy` / `OrderByDescending` |
| 分组 | 手动 / itertools | `GroupBy` |
| 去重 | `set(...)` | `Distinct()` |
| 取前 n | `list[:n]` | `Take(n)` |
| 跳过 n | `list[n:]` | `Skip(n)` |
| 展平 | `itertools.chain` | `SelectMany` |
| 连接 | 手动 | `Join` |
| 最大/最小 | `max` / `min` | `Max` / `Min` / `MaxBy` |

**⚠️ 最大的坑：延迟执行**
```csharp
var query = nums.Where(x => x > 0);   // 此时什么都没算！
nums.Add(100);                         // 改动会反映到 query 里
foreach (var n in query) { }           // 真正执行发生在这一刻

// 想立刻固定结果：
var list = nums.Where(x => x > 0).ToList();
```

---

## 10. 异常处理

```python
# Python
try:
    risky()
except ValueError as e:
    print(f"bad value: {e}")
except Exception as e:
    raise
finally:
    cleanup()
```
```csharp
// C#
try
{
    Risky();
}
catch (ArgumentException e) when (e.ParamName == "id")   // 异常筛选器
{
    Console.WriteLine($"bad value: {e.Message}");
}
catch (Exception e)
{
    _logger.LogError(e, "failed");
    throw;                        // 重新抛出，保留堆栈（不要写 throw e;）
}
finally
{
    Cleanup();
}
```

| Python | C# |
|---|---|
| `raise ValueError("msg")` | `throw new ArgumentException("msg");` |
| `except X as e` | `catch (X e)` |
| `finally` | `finally` |
| `raise`（重抛） | `throw;`（**不是 `throw e;`**） |
| `with open(...)` | `using (var f = ...)` / `using var f = ...;` |
| 自定义异常 | `class MyException : Exception { }` |
| `assert` | `Debug.Assert` / 测试断言 |

**资源释放（`using` ≈ `with`）**：
```csharp
using var reader = new StreamReader("a.txt");   // 作用域结束自动 Dispose
var text = reader.ReadToEnd();
```

---

## 11. 异步编程

```python
# Python
import asyncio

async def fetch(url):
    async with aiohttp.ClientSession() as s:
        async with s.get(url) as r:
            return await r.text()

async def main():
    results = await asyncio.gather(fetch(u1), fetch(u2))
```
```csharp
// C#
async Task<string> FetchAsync(string url)
{
    using var client = new HttpClient();
    return await client.GetStringAsync(url);
}

async Task Main()
{
    var tasks = new[] { FetchAsync(u1), FetchAsync(u2) };
    var results = await Task.WhenAll(tasks);   // ≈ asyncio.gather
}
```

| Python | C# |
|---|---|
| `async def` | `async Task` / `async Task<T>` |
| `await` | `await` |
| `asyncio.gather` | `Task.WhenAll` |
| `asyncio.wait_for` | `task.WaitAsync(timeout)` |
| 取消 | `CancellationToken` |
| 并发原语 | `SemaphoreSlim` / `lock`（**lock 是同步的**） |
| 事件循环 | 无显式循环，由 SynchronizationContext / 线程池管理 |

**铁律**：
- 只有事件处理器可以写 `async void`，其他一律 `async Task`
- 不要用 `.Result` / `.Wait()` 同步阻塞异步代码 → 死锁
- 方法名以 `Async` 结尾（约定）

---

## 12. 模块化与依赖

| Python | C# |
|---|---|
| `import os` | `using System.IO;` |
| `from x import y` | `using X;`（然后在代码里用 `Y`） |
| 别名 `import numpy as np` | `using Np = System.Numerics;`（少用） |
| 模块 = 文件 | 命名空间可跨文件；类 = 文件（惯例一文件一类） |
| `__init__.py` | 无对应 |
| 循环导入问题 | 命名空间无此问题 |
| `pip install` | `dotnet add package X` |
| `requirements.txt` | `.csproj` 里的 `PackageReference` |

**代码组织惯例**：
```
MyApp/
├─ MyApp.csproj
├─ Program.cs              入口
├─ Models/                 数据模型（≈ pydantic models）
├─ Services/               业务逻辑
├─ Repositories/           数据访问
├─ Interfaces/             接口定义
└─ appsettings.json        配置（≈ .env / config.yaml）
```

---

## 13. 命名惯例（重要，最好第一天就养成）

| 元素 | Python | C# |
|---|---|---|
| 类名 | `MyClass` | `MyClass`（PascalCase） |
| 方法名 | `my_method` | `MyMethod`（PascalCase） |
| 公开属性 | `my_attr` | `MyAttr`（PascalCase） |
| 局部变量/参数 | `my_var` | `myVar`（camelCase） |
| 私有字段 | `_my_var` | `_myVar`（下划线 + camelCase） |
| 常量 | `MY_CONST` | `MyConst`（PascalCase） |
| 接口 | 无统一惯例 | `IMyInterface`（**I 开头**） |
| 泛型参数 | `T` | `T` / `TKey` / `TValue` |
| 命名空间 | `my_package` | `MyCompany.MyProduct` |
| 文件名 | `my_module.py` | `MyClass.cs`（与类名一致） |

---

## 14. 十五个最易犯的错（自查清单）

1. 期望 `int b = a` 之后改 b 会影响 a —— **值类型是复制**
2. 用 `double` 做金额计算 —— **用 `decimal`**
3. `async void` 满天飞 —— **只有事件处理器能用**
4. `.Result` / `.Wait()` 阻塞异步 —— **一路 `await` 到底**
5. `catch (Exception) { }` 吞异常 —— **至少记日志**
6. `public string name;` 暴露字段 —— **写成属性 `public string Name { get; set; }`**
7. `foreach` 里改集合 —— 先 `.ToList()`
8. 循环里 `str += x` —— 用 `StringBuilder`
9. 以为 LINQ 立即执行 —— **延迟执行，要 `ToList()`**
10. 用 `int.Parse` 处理用户输入 —— **用 `int.TryParse`**
11. 忘了 `using` 释放资源 —— **文件/连接/流必须 `using`**
12. 用 Python 的命名风格 `my_var` —— **C# 是 PascalCase/camelCase**
13. 同时用两个 IDE 打开同一项目 —— **会锁文件冲突**
14. 忽略黄色警告 —— **CS8618 等警告是真 bug 的前兆**
15. 把 `string` 当可变对象改 —— **C# 字符串不可变**

---

## 15. 一天速成对照示例

同一个需求，两种语言各写一遍，对比感受差异。

**需求**：读一个学生名单文件，算平均分，输出及格率，按分数排序。

```python
# Python
from dataclasses import dataclass

@dataclass
class Student:
    name: str
    score: float

def load(path: str) -> list[Student]:
    result = []
    with open(path, encoding="utf-8") as f:
        for line in f:
            name, score = line.strip().split(",")
            result.append(Student(name, float(score)))
    return result

def main():
    students = load("students.csv")
    if not students:
        print("没有数据")
        return
    avg = sum(s.score for s in students) / len(students)
    passed = [s for s in students if s.score >= 60]
    rate = len(passed) / len(students) * 100
    print(f"平均分: {avg:.2f}, 及格率: {rate:.1f}%")
    for s in sorted(students, key=lambda s: -s.score):
        print(f"{s.name}: {s.score}")

if __name__ == "__main__":
    main()
```

```csharp
// C#
using System.Globalization;

namespace StudentStats;

public record Student(string Name, double Score);

public static class Program
{
    public static void Main()
    {
        var students = Load("students.csv");

        if (students.Count == 0)
        {
            Console.WriteLine("没有数据");
            return;
        }

        var avg = students.Average(s => s.Score);
        var passRate = students.Count(s => s.Score >= 60) * 100.0 / students.Count;
        Console.WriteLine($"平均分: {avg:F2}, 及格率: {passRate:F1}%");

        foreach (var s in students.OrderByDescending(s => s.Score))
            Console.WriteLine($"{s.Name}: {s.Score}");
    }

    private static List<Student> Load(string path)
    {
        var result = new List<Student>();
        foreach (var line in File.ReadLines(path))
        {
            var parts = line.Split(',');
            if (parts.Length != 2) continue;                      // 跳过脏数据
            if (!double.TryParse(parts[1], NumberStyles.Any,
                                 CultureInfo.InvariantCulture, out var score))
                continue;                                          // 跳过非法分数
            result.Add(new Student(parts[0], score));
        }
        return result;
    }
}
```

**对比要点**：
- C# 多了 `namespace`、`using`、访问修饰符、类型标注——**啰嗦，但错了编译不过**
- C# 的 `record` 一行替代 `@dataclass`
- C# 的 LINQ（`Average`/`Count`/`OrderByDescending`）替代生成器表达式
- C# 必须显式处理解析失败（`TryParse`），Python 的 `float()` 会抛异常
- C# 的类型标注让 IDE 能 100% 准确地补全和重构——这是最大的收益

---

**最后提醒**：这份速查表是用来"查"的，不是用来"背"的。真正记住它们的方式是——**每天写代码时遇到不确定的地方就来查一次**，查三次以上自然就记住了。
