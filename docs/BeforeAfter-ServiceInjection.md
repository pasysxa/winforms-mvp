# 服务注入改进：Before & After 对比

## 📋 问题描述

**原问题**：每次创建Presenter都需要显式创建服务并传入，代码繁琐。

```csharp
// ❌ 太麻烦了！
var messageService = new MessageService();
var dialogProvider = new DialogProvider();
var fileService = new FileService();

var presenter = new MyPresenter(messageService, dialogProvider, fileService);
```

---

## ✨ 解决方案

引入 **CommonServices 静态单例 + 可选参数** 模式。

---

## 🔄 对比示例

### 示例1：Presenter定义

#### Before（旧方式）

```csharp
public class ToDoDemoPresenter : WindowPresenterBase<IToDoView>
{
    private readonly IMessageService _messageService;

    // 必须传入服务
    public ToDoDemoPresenter(IMessageService messageService)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
    }

    private void OnSave()
    {
        _messageService.ShowInfo("保存成功！");
    }
}
```

#### After（新方式）

```csharp
public class ToDoDemoPresenter : WindowPresenterBase<IToDoView>
{
    private readonly ICommonServices _services;

    // 服务参数可选，默认使用 CommonServices.Default
    public ToDoDemoPresenter(ICommonServices services = null)
    {
        _services = services ?? CommonServices.Default;
    }

    private void OnSave()
    {
        _services.MessageService.ShowInfo("保存成功！");
    }
}
```

**改进点**：
- ✅ 构造函数参数可选
- ✅ 自动使用默认服务
- ✅ 仍然支持依赖注入（测试时传入mock）

---

### 示例2：生产环境使用

#### Before（旧方式）

```csharp
// ❌ 每次都要创建服务实例
private void LaunchToDoDemo()
{
    var messageService = new MessageService();  // 手动创建
    var view = new ToDoDemoForm();
    var presenter = new ToDoDemoPresenter(messageService);  // 必须传入

    presenter.AttachView(view);
    presenter.Initialize();
    view.Show();
}
```

#### After（新方式）

```csharp
// ✅ 超简单！无需创建服务
private void LaunchToDoDemo()
{
    var view = new ToDoDemoForm();
    var presenter = new ToDoDemoPresenter();  // 自动使用默认服务

    presenter.AttachView(view);
    presenter.Initialize();
    view.Show();
}
```

**改进点**：
- ✅ 减少2行代码
- ✅ 无需手动管理服务实例
- ✅ 代码更清晰

---

### 示例3：多个服务的场景

#### Before（旧方式）

```csharp
// ❌ 参数太多！
public class ExecutionRequestDemoPresenter : WindowPresenterBase<IExecutionRequestDemoView>
{
    private readonly IMessageService _messageService;
    private readonly IDialogProvider _dialogProvider;

    public ExecutionRequestDemoPresenter(
        IMessageService messageService,
        IDialogProvider dialogProvider)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _dialogProvider = dialogProvider ?? throw new ArgumentNullException(nameof(dialogProvider));
    }
}

// 使用时：
var messageService = new MessageService();
var dialogProvider = new DialogProvider();
var presenter = new ExecutionRequestDemoPresenter(messageService, dialogProvider);
```

#### After（新方式）

```csharp
// ✅ 一个参数搞定所有常用服务
public class ExecutionRequestDemoPresenter : WindowPresenterBase<IExecutionRequestDemoView>
{
    private readonly ICommonServices _services;

    public ExecutionRequestDemoPresenter(ICommonServices services = null)
    {
        _services = services ?? CommonServices.Default;
    }

    private void OnSave()
    {
        _services.MessageService.ShowInfo("保存成功！");
        var result = _services.DialogProvider.ShowOpenFileDialog(...);
        var content = _services.FileService.ReadAllText(result.FileName);
    }
}

// 使用时：
var presenter = new ExecutionRequestDemoPresenter();  // 搞定！
```

**改进点**：
- ✅ 构造函数从2个参数减少到0个
- ✅ 创建代码从3行减少到1行
- ✅ 减少60%的代码量

---

### 示例4：测试场景（可测试性保持不变）

#### Before（旧方式）

```csharp
[TestMethod]
public void Test_SaveAction_ShowsMessage()
{
    // Arrange
    var mockMessageService = new MockMessageService();
    var presenter = new ToDoDemoPresenter(mockMessageService);

    // Act
    presenter.OnSave();

    // Assert
    Assert.IsTrue(mockMessageService.InfoMessageShown);
}
```

#### After（新方式）

```csharp
[TestMethod]
public void Test_SaveAction_ShowsMessage()
{
    // Arrange
    var mockServices = new MockCommonServices();
    var presenter = new ToDoDemoPresenter(mockServices);  // 仍然可以注入mock

    // Act
    presenter.OnSave();

    // Assert
    Assert.IsTrue(mockServices.MessageService.InfoMessageShown);
}
```

**改进点**：
- ✅ 测试代码基本不变
- ✅ 可测试性完全保留
- ✅ Mock管理更简单（一个mock对象管理所有服务）

---

### 示例5：特殊服务 + 常用服务（混合模式）

#### Before（旧方式）

```csharp
// ❌ 参数列表越来越长
public class NavigatorDemoPresenter : WindowPresenterBase<INavigatorDemoView>
{
    private readonly IWindowNavigator _navigator;
    private readonly IMessageService _messageService;
    private readonly IDialogProvider _dialogProvider;

    public NavigatorDemoPresenter(
        IWindowNavigator navigator,
        IMessageService messageService,
        IDialogProvider dialogProvider)
    {
        _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _dialogProvider = dialogProvider ?? throw new ArgumentNullException(nameof(dialogProvider));
    }
}

// 使用时：
var navigator = new WindowNavigator(...);
var messageService = new MessageService();
var dialogProvider = new DialogProvider();
var presenter = new NavigatorDemoPresenter(navigator, messageService, dialogProvider);
```

#### After（新方式）

```csharp
// ✅ 混合模式：特殊服务必填，常用服务可选
public class NavigatorDemoPresenter : WindowPresenterBase<INavigatorDemoView>
{
    private readonly IWindowNavigator _navigator;
    private readonly ICommonServices _services;

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
        _navigator.ShowWindow(...);
    }
}

// 使用时：
var navigator = new WindowNavigator(...);
var presenter = new NavigatorDemoPresenter(navigator);  // 只需传特殊服务！
```

**改进点**：
- ✅ 构造函数从3个参数减少到1个（必填）
- ✅ 创建代码从4行减少到2行
- ✅ 减少50%的代码量

---

## 📊 整体改进统计

| 场景 | Before | After | 改进幅度 |
|------|--------|-------|---------|
| **简单Presenter（1个服务）** | 2行创建代码 | 1行创建代码 | 减少50% |
| **中等Presenter（2-3个服务）** | 4行创建代码 | 1行创建代码 | 减少75% |
| **复杂Presenter（特殊+常用服务）** | 5行创建代码 | 2行创建代码 | 减少60% |
| **构造函数参数（常用服务）** | 2-3个必填参数 | 0个参数 | 减少100% |
| **测试Mock管理** | 需要mock多个服务 | Mock一个ICommonServices | 简化管理 |

---

## ✅ 最佳实践总结

### 1. 默认使用新模式

```csharp
// ✅ 推荐
public MyPresenter(ICommonServices services = null)
{
    _services = services ?? CommonServices.Default;
}
```

### 2. 特殊服务显式注入

```csharp
// ✅ 推荐
public MyPresenter(
    IWindowNavigator navigator,      // 特殊服务 - 必填
    ICommonServices services = null)  // 常用服务 - 可选
```

### 3. 生产环境无参数创建

```csharp
// ✅ 超简单
var presenter = new MyPresenter();
```

### 4. 测试时注入Mock

```csharp
// ✅ 灵活可测试
var mockServices = new MockCommonServices();
var presenter = new MyPresenter(mockServices);
```

---

## 🎯 总结

**新模式的优势**：
1. ✅ **生产环境**：代码量减少50%-75%，超简单
2. ✅ **测试环境**：可测试性完全保留，Mock管理更简单
3. ✅ **向后兼容**：旧代码可以继续使用显式注入
4. ✅ **渐进式迁移**：可以逐步迁移到新模式

**核心原则**：
- 便利性和可测试性的完美平衡
- 默认使用静态单例，需要时可以覆盖
- 特殊服务仍然显式注入，保持依赖明确

这就是你想要的效果！🎉
