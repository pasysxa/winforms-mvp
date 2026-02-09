# 服务注入模式 - Service Injection Patterns

## 📖 概述

框架提供了多种服务注入方式，平衡生产环境的便利性和测试环境的灵活性。

---

## 🎯 推荐模式

### **模式1：使用静态默认服务（最简单）**

**适用场景**：大多数Presenter，只需要常用服务（IMessageService、IDialogProvider等）

#### 代码示例：

```csharp
using WinformsMVP.MVP.Presenters;
using WinformsMVP.Services;

public class ToDoDemoPresenter : WindowPresenterBase<IToDoView>
{
    private readonly ICommonServices _services;

    // 可选参数 - 默认使用静态单例
    public ToDoDemoPresenter(ICommonServices services = null)
    {
        _services = services ?? CommonServices.Default;
    }

    private void OnSave()
    {
        _services.MessageService.ShowInfo("保存成功！", "提示");
    }

    private void OnOpenFile()
    {
        var result = _services.DialogProvider.ShowOpenFileDialog(
            title: "选择文件",
            filter: "文本文件|*.txt");

        if (result.IsOk)
        {
            var content = _services.FileService.ReadAllText(result.FileName);
            // ...
        }
    }
}
```

#### 使用方式：

```csharp
// ✅ 生产环境 - 超简单！
var presenter = new ToDoDemoPresenter();  // 自动使用默认服务

// ✅ 测试环境 - 注入mock
var mockServices = new MockCommonServices();
var presenter = new ToDoDemoPresenter(mockServices);
```

---

### **模式2：混合注入（常用服务 + 特殊服务）**

**适用场景**：需要特殊服务（如IWindowNavigator）的Presenter

#### 代码示例：

```csharp
public class NavigatorDemoPresenter : WindowPresenterBase<INavigatorDemoView>
{
    private readonly ICommonServices _services;
    private readonly IWindowNavigator _navigator;

    // 常用服务可选，特殊服务必填
    public NavigatorDemoPresenter(
        IWindowNavigator navigator,
        ICommonServices services = null)
    {
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        _services = services ?? CommonServices.Default;
    }

    private void OnShowDialog()
    {
        _services.MessageService.ShowInfo("打开对话框...");

        var dialogPresenter = new SimpleDialogPresenter();
        _navigator.ShowWindowAsModal(dialogPresenter);
    }
}
```

#### 使用方式：

```csharp
// ✅ 生产环境 - 只需传入特殊服务
var navigator = new WindowNavigator(...);
var presenter = new NavigatorDemoPresenter(navigator);  // CommonServices自动使用默认值

// ✅ 测试环境 - 全部mock
var mockNavigator = new MockNavigator();
var mockServices = new MockCommonServices();
var presenter = new NavigatorDemoPresenter(mockNavigator, mockServices);
```

---

### **模式3：完全向后兼容（显式服务注入）**

**适用场景**：需要明确依赖关系，或者遗留代码

#### 代码示例：

```csharp
public class UserEditorPresenter : WindowPresenterBase<IUserEditorView>
{
    private readonly IMessageService _messageService;
    private readonly IDialogProvider _dialogProvider;

    // 传统方式 - 显式注入每个服务
    public UserEditorPresenter(
        IMessageService messageService,
        IDialogProvider dialogProvider)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _dialogProvider = dialogProvider ?? throw new ArgumentNullException(nameof(dialogProvider));
    }

    private void OnSave()
    {
        _messageService.ShowInfo("保存成功！");
    }
}
```

#### 使用方式：

```csharp
// 生产环境
var services = CommonServices.Default;
var presenter = new UserEditorPresenter(
    services.MessageService,
    services.DialogProvider);

// 测试环境
var mockMessageService = new MockMessageService();
var mockDialogProvider = new MockDialogProvider();
var presenter = new UserEditorPresenter(mockMessageService, mockDialogProvider);
```

---

## 🔧 CommonServices 静态访问点

### 基本用法

```csharp
// 获取默认服务（自动初始化）
var services = CommonServices.Default;

// 使用服务
services.MessageService.ShowInfo("提示信息");
services.DialogProvider.ShowOpenFileDialog(...);
services.FileService.ReadAllText(path);
```

### 自定义全局服务

在应用启动时设置自定义服务：

```csharp
// Program.cs
[STAThread]
static void Main()
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    // 自定义全局服务（可选）
    CommonServices.Default = new CustomCommonServices();

    // 启动应用
    var mainPresenter = new MainFormPresenter();  // 自动使用自定义服务
    // ...
}
```

### 测试中重置服务

```csharp
[TestClass]
public class MyPresenterTests
{
    [TestInitialize]
    public void Setup()
    {
        // 为测试设置mock服务
        CommonServices.Default = new MockCommonServices();
    }

    [TestCleanup]
    public void Cleanup()
    {
        // 重置为默认实现
        CommonServices.Reset();
    }

    [TestMethod]
    public void Test_SaveAction_ShowsSuccessMessage()
    {
        // CommonServices.Default 是 MockCommonServices
        var presenter = new MyPresenter();  // 使用mock服务
        presenter.Initialize();

        // ... 测试代码
    }
}
```

---

## 📊 模式对比

| 模式 | 生产环境便利性 | 测试灵活性 | 依赖明确性 | 推荐场景 |
|------|--------------|----------|----------|---------|
| **模式1：静态默认服务** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐ | 大多数Presenter |
| **模式2：混合注入** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | 需要特殊服务 |
| **模式3：显式注入** | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | 遗留代码、依赖明确 |

---

## ✅ 最佳实践

### 1. 优先使用模式1（静态默认服务）

```csharp
// ✅ 推荐 - 简洁明了
public MyPresenter(ICommonServices services = null)
{
    _services = services ?? CommonServices.Default;
}

// ❌ 避免 - 太多参数
public MyPresenter(IMessageService msg, IDialogProvider dlg, IFileService file)
{
    // ...
}
```

### 2. 特殊服务显式注入

```csharp
// ✅ 推荐 - 特殊服务明确，常用服务简化
public NavigatorDemoPresenter(
    IWindowNavigator navigator,         // 特殊服务 - 必填
    ICommonServices services = null)     // 常用服务 - 可选
{
    _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
    _services = services ?? CommonServices.Default;
}
```

### 3. 在应用启动时自定义全局服务（可选）

```csharp
// Program.cs
static void Main()
{
    // 如果需要自定义服务实现，在这里设置
    CommonServices.Default = new MyCustomServices();

    // 之后所有Presenter都会使用自定义服务
    Application.Run(new MainForm());
}
```

### 4. 测试中始终注入Mock

```csharp
// ✅ 推荐 - 显式传入mock
var mockServices = new MockCommonServices();
var presenter = new MyPresenter(mockServices);

// ⚠️ 可用但不推荐 - 修改全局状态
CommonServices.Default = new MockCommonServices();
var presenter = new MyPresenter();  // 依赖全局mock
```

---

## 🚫 反模式（避免使用）

### 反模式1：在Presenter内部直接new服务

```csharp
// ❌ 错误 - 不可测试
public class MyPresenter : WindowPresenterBase<IMyView>
{
    private readonly IMessageService _messageService = new MessageService();

    // 无法在测试中替换为mock
}
```

### 反模式2：静态服务方法

```csharp
// ❌ 错误 - 全局状态，难以测试
public class MyPresenter : WindowPresenterBase<IMyView>
{
    private void OnSave()
    {
        MessageBox.Show("保存成功！");  // 直接调用WinForms API
    }
}
```

### 反模式3：Service Locator模式（过度使用）

```csharp
// ❌ 错误 - 隐藏依赖关系
public class MyPresenter : WindowPresenterBase<IMyView>
{
    private void OnSave()
    {
        var service = ServiceLocator.GetService<IMessageService>();
        service.ShowInfo("保存成功！");
    }
}

// ✅ 正确 - 构造函数注入
public class MyPresenter : WindowPresenterBase<IMyView>
{
    private readonly ICommonServices _services;

    public MyPresenter(ICommonServices services = null)
    {
        _services = services ?? CommonServices.Default;
    }

    private void OnSave()
    {
        _services.MessageService.ShowInfo("保存成功！");
    }
}
```

---

## 📝 总结

**推荐的注入策略**：

1. **默认选择**：模式1（静态默认服务）
   - 生产环境无参数：`new MyPresenter()`
   - 测试环境注入mock：`new MyPresenter(mockServices)`

2. **需要特殊服务**：模式2（混合注入）
   - 特殊服务必填，常用服务可选

3. **遗留代码**：模式3（显式注入）
   - 保持向后兼容

**核心原则**：
- ✅ 始终通过构造函数注入依赖
- ✅ 使用 `CommonServices.Default` 简化生产环境代码
- ✅ 测试时显式传入mock服务
- ❌ 避免在Presenter内部直接new服务
- ❌ 避免直接调用WinForms API（MessageBox等）

这样既保证了可测试性，又提供了极简的使用体验！
