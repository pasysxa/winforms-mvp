# 显式模式：处理带参数和不带参数的混合 ActionRequest

## 问题

在显式事件模式中，如何在同一个 View 中同时处理：
1. **简单 Actions**（无参数）：如 Refresh, Clear
2. **参数化 Actions**（带参数）：如 SelectTab(int), LoadDocument(string), SetZoom(int)

## 解决方案

使用**单个 ActionRequest 事件**，利用多态性处理两种类型。

## 完整示例

参见：`src/WinformsMVP.Samples/ViewActionExplicitWithParametersExample.cs`

## 关键实现

### 1. View Interface（单个事件）

```csharp
public interface IMixedParametersDemoView : IWindowView
{
    // ✅ 单个 ActionRequest 事件处理所有类型
    // 使用基类 ActionRequestEventArgs，可以接收派生类型
    event EventHandler<ActionRequestEventArgs> ActionRequest;

    // 数据属性...
}
```

### 2. View 实现（ActionTriggered 处理器）

```csharp
private void InitializeActionBindings()
{
    _binder = new ViewActionBinder();

    // 绑定所有 actions（无论是否有参数）
    _binder.Add(MixedDemoActions.Refresh, _btnRefresh);
    _binder.Add(MixedDemoActions.SelectTab, _btnTab1, _btnTab2, _btnTab3);
    _binder.Add(MixedDemoActions.LoadDocument, _btnLoadDoc);
    _binder.Add(MixedDemoActions.SetZoom, _btnSetZoom);

    // ✅ 订阅 ActionTriggered，根据 action 类型决定如何转发
    _binder.ActionTriggered += (sender, e) =>
    {
        if (e.ActionKey == MixedDemoActions.SelectTab)
        {
            // 需要参数 - 创建 ActionRequestEventArgs<int>
            int tabIndex = GetTabIndexFromSender();
            var paramArgs = new ActionRequestEventArgs<int>(e.ActionKey, tabIndex);
            ActionRequest?.Invoke(this, paramArgs);
        }
        else if (e.ActionKey == MixedDemoActions.LoadDocument)
        {
            // 需要参数 - 从 TextBox 获取
            string docName = _txtDocument.Text;
            var paramArgs = new ActionRequestEventArgs<string>(e.ActionKey, docName);
            ActionRequest?.Invoke(this, paramArgs);
        }
        else if (e.ActionKey == MixedDemoActions.SetZoom)
        {
            // 需要参数 - 从 NumericUpDown 获取
            int zoomLevel = (int)_numZoom.Value;
            var paramArgs = new ActionRequestEventArgs<int>(e.ActionKey, zoomLevel);
            ActionRequest?.Invoke(this, paramArgs);
        }
        else
        {
            // 简单 action - 直接转发原始事件参数
            ActionRequest?.Invoke(this, e);
        }
    };
}
```

### 3. Presenter（超简单！）

```csharp
protected override void OnViewAttached()
{
    // ✅ 直接使用基类提供的 OnViewActionTriggered 方法！
    // 它会自动处理参数检查、提取和分发
    View.ActionRequest += OnViewActionTriggered;
}

protected override void RegisterViewActions()
{
    // 简单 actions（无参数）
    Dispatcher.Register(MixedDemoActions.Refresh, OnRefresh);
    Dispatcher.Register(MixedDemoActions.Clear, OnClear);

    // 参数化 actions（有参数）
    Dispatcher.Register<int>(MixedDemoActions.SelectTab, OnSelectTab);
    Dispatcher.Register<string>(MixedDemoActions.LoadDocument, OnLoadDocument);
    Dispatcher.Register<int>(MixedDemoActions.SetZoom, OnSetZoom);
}

// ✅ 不需要手动写参数检查逻辑！
// OnViewActionTriggered (基类方法) 已经自动处理了：
// - 检查 e is IActionRequestEventArgsWithValue
// - 提取参数 payload = valueProvider.GetValue()
// - 调用 Dispatcher.Dispatch(actionKey, payload)

// Action 处理器
private void OnRefresh() { /* ... */ }
private void OnClear() { /* ... */ }
private void OnSelectTab(int tabIndex) { /* ... */ }
private void OnLoadDocument(string docName) { /* ... */ }
private void OnSetZoom(int zoomLevel) { /* ... */ }
```

**关键点**：基类 `PresenterBase` 的 `OnViewActionTriggered` 方法内部已经实现了参数检查和提取逻辑，你只需要直接订阅即可！

## 事件流图

### 简单 Action（无参数）

```
Button Click (Refresh)
    ↓
ViewActionBinder.ActionTriggered
    ↓
Check: e.ActionKey == MixedDemoActions.Refresh? NO
    ↓
Forward original ActionRequestEventArgs
    ↓
View.ActionRequest event fires
    ↓
Presenter.OnActionRequest
    ↓
Check: e is IActionRequestEventArgsWithValue? NO
    ↓
Dispatcher.Dispatch(actionKey)
    ↓
OnRefresh()
```

### 参数化 Action（带参数）

```
Button Click (Load Document)
    ↓
ViewActionBinder.ActionTriggered
    ↓
Check: e.ActionKey == MixedDemoActions.LoadDocument? YES
    ↓
Extract parameter: docName = _txtDocument.Text
    ↓
Create ActionRequestEventArgs<string>(actionKey, docName)
    ↓
View.ActionRequest event fires (with parameter)
    ↓
Presenter.OnViewActionTriggered (基类方法)
    ↓
自动检查: e is IActionRequestEventArgsWithValue? YES
    ↓
自动提取: payload = valueProvider.GetValue()
    ↓
Dispatcher.Dispatch(actionKey, payload)
    ↓
OnLoadDocument(string docName)
```

## 其他可选方案

### 方案 1：单个事件（当前推荐）✅

```csharp
// View Interface
event EventHandler<ActionRequestEventArgs> ActionRequest;
```

**优点**：
- ✅ 接口简洁（只有一个事件）
- ✅ 使用多态性（ActionRequestEventArgs<T> 继承自 ActionRequestEventArgs）
- ✅ 单个订阅点

**缺点**：
- ⚠️ 需要在运行时检查参数类型

### 方案 2：多个类型化事件（更显式）

```csharp
// View Interface
event EventHandler<ActionRequestEventArgs> SimpleActions;
event EventHandler<ActionRequestEventArgs<int>> IntActions;
event EventHandler<ActionRequestEventArgs<string>> StringActions;
```

**优点**：
- ✅ 编译时类型安全
- ✅ 清晰的参数类型分离

**缺点**：
- ❌ 更多事件需要管理
- ❌ Presenter 需要订阅多个事件
- ❌ View Interface 更复杂

### 方案 3：泛型事件（不推荐）❌

```csharp
// View Interface
event EventHandler<ActionRequestEventArgs<object>> ActionRequest;
```

**缺点**：
- ❌ 失去类型安全
- ❌ 需要运行时类型转换
- ❌ 容易出错

## 最佳实践

### 1. 使用基类 OnViewActionTriggered 方法（最重要！）

```csharp
// ✅ BEST - 直接使用基类方法
protected override void OnViewAttached()
{
    View.ActionRequest += OnViewActionTriggered;  // 基类自动处理参数！
}

// ❌ BAD - 手动写参数检查逻辑（多余）
protected override void OnViewAttached()
{
    View.ActionRequest += (s, e) =>
    {
        if (e is IActionRequestEventArgsWithValue valueProvider)
        {
            var value = valueProvider.GetValue();
            Dispatcher.Dispatch(e.ActionKey, value);
        }
        else
        {
            Dispatcher.Dispatch(e.ActionKey);
        }
    };
}
```

**原因**：基类的 `OnViewActionTriggered` 方法已经实现了参数检查和提取逻辑，无需重复编写！

### 2. 参数提取在 View 层完成

```csharp
// ✅ GOOD - View 知道控件
int zoomLevel = (int)_numZoom.Value;
var args = new ActionRequestEventArgs<int>(actionKey, zoomLevel);

// ❌ BAD - Presenter 不应该知道控件
// Presenter 不应该访问 _numZoom
```

**原因**：View 拥有 UI 控件，Presenter 不应该知道它们的存在。

### 3. 在 Presenter 中验证参数

```csharp
private void OnLoadDocument(string documentName)
{
    // ✅ GOOD - 业务验证在 Presenter
    if (string.IsNullOrWhiteSpace(documentName))
    {
        _messageService.ShowWarning("Please enter a document name.", "Validation");
        return;
    }

    // 继续处理...
}
```

## 对比表格

| 方面 | 简单 Action | 参数化 Action |
|------|------------|--------------|
| **Dispatcher 注册** | `Register(action, handler)` | `Register<T>(action, handler)` |
| **事件参数类型** | `ActionRequestEventArgs` | `ActionRequestEventArgs<T>` |
| **参数来源** | 无 | View 控件（TextBox, NumericUpDown 等） |
| **Presenter 处理器** | `void OnAction()` | `void OnAction(T parameter)` |
| **参数验证** | 无需验证 | 需要验证（null, 范围等） |

## 运行示例

构建并运行示例：

```bash
dotnet build src/WinformsMVP.Samples/WinformsMVP.Samples.csproj
dotnet run --project src/WinformsMVP.Samples/WinformsMVP.Samples.csproj
```

然后选择 "Explicit Pattern - Mixed Parameters Demo"。

## 总结

使用**单个 ActionRequest 事件**配合多态性是处理混合参数的最佳方案：

- ✅ View Interface 简洁（一个事件）
- ✅ 利用继承关系（ActionRequestEventArgs<T> : ActionRequestEventArgs）
- ✅ 运行时参数检查（IActionRequestEventArgsWithValue）
- ✅ 灵活且易于扩展
- ✅ 符合框架设计理念

---

**参考文件**：
- 完整示例：[ViewActionExplicitWithParametersExample.cs](src/WinformsMVP.Samples/ViewActionExplicitWithParametersExample.cs)
- 相关优化：[OPTIMIZATION-SUMMARY.md](OPTIMIZATION-SUMMARY.md)
