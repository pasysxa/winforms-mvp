# ViewAction 模式快速参考

本文档总结了 WinForms MVP 框架中 ViewAction 系统的所有使用模式。

## 📋 目录

1. [隐式模式（Implicit Pattern）](#隐式模式)
2. [显式模式（Explicit Pattern）](#显式模式)
3. [显式模式 + 混合参数](#显式模式--混合参数)
4. [模式对比](#模式对比)
5. [如何选择](#如何选择)

---

## 隐式模式

**最简洁，推荐用于大多数场景**

### 代码示例

```csharp
// ========================================
// View Interface
// ========================================
public interface IMyView : IWindowView
{
    // 数据属性
    string Name { get; set; }
    bool HasUnsavedChanges { get; }

    // ✅ 返回 ActionBinder（框架自动绑定）
    ViewActionBinder ActionBinder { get; }
}

// ========================================
// View 实现
// ========================================
public class MyForm : Form, IMyView
{
    private ViewActionBinder _binder;
    public ViewActionBinder ActionBinder => _binder;

    private void InitializeActionBindings()
    {
        _binder = new ViewActionBinder();
        _binder.Add(CommonActions.Save, _btnSave);
        _binder.Add(CommonActions.Cancel, _btnCancel);
        // 不需要调用 Bind() - 框架自动处理
    }
}

// ========================================
// Presenter
// ========================================
public class MyPresenter : WindowPresenterBase<IMyView>
{
    protected override void RegisterViewActions()
    {
        Dispatcher.Register(
            CommonActions.Save,
            OnSave,
            canExecute: () => View.HasUnsavedChanges);

        Dispatcher.Register(CommonActions.Cancel, OnCancel);

        // 框架自动调用：View.ActionBinder.Bind(_dispatcher)
    }

    private void OnSave() { /* ... */ }
    private void OnCancel() { /* ... */ }
}
```

### 特点

| 方面 | 说明 |
|------|------|
| **代码量** | 最少 |
| **事件流** | 隐式（框架自动绑定） |
| **CanExecute 更新** | ✅ 自动 |
| **调试** | 稍难（事件流不明显） |
| **适用场景** | 标准 CRUD、简单流程 |

---

## 显式模式

**事件流清晰，适合复杂场景或学习**

### 代码示例

```csharp
// ========================================
// View Interface
// ========================================
public interface IMyView : IWindowView
{
    // 数据属性
    string Name { get; set; }
    bool HasUnsavedChanges { get; }

    // ✅ 返回 ActionBinder（自动 CanExecute 更新）
    ViewActionBinder ActionBinder { get; }

    // ✅ 显式 ActionRequest 事件
    event EventHandler<ActionRequestEventArgs> ActionRequest;
}

// ========================================
// View 实现
// ========================================
public class MyForm : Form, IMyView
{
    private ViewActionBinder _binder;
    public ViewActionBinder ActionBinder => _binder;

    public event EventHandler<ActionRequestEventArgs> ActionRequest;

    private void InitializeActionBindings()
    {
        _binder = new ViewActionBinder();
        _binder.Add(CommonActions.Save, _btnSave);
        _binder.Add(CommonActions.Cancel, _btnCancel);

        // ✅ 订阅 ActionTriggered 并转发到 ActionRequest
        _binder.ActionTriggered += (s, e) =>
        {
            ActionRequest?.Invoke(this, e);
        };

        // 不需要调用 Bind() - 框架自动处理
    }
}

// ========================================
// Presenter
// ========================================
public class MyPresenter : WindowPresenterBase<IMyView>
{
    protected override void OnViewAttached()
    {
        // ✅ 显式订阅 ActionRequest 事件
        View.ActionRequest += OnViewActionTriggered;
    }

    protected override void RegisterViewActions()
    {
        Dispatcher.Register(
            CommonActions.Save,
            OnSave,
            canExecute: () => View.HasUnsavedChanges);

        Dispatcher.Register(CommonActions.Cancel, OnCancel);

        // 框架自动调用：View.ActionBinder.Bind(_dispatcher)
        // 模式检测防止双重分发
    }

    // ✅ OnViewActionTriggered 是基类提供的辅助方法
    // 自动路由到 Dispatcher

    private void OnSave() { /* ... */ }
    private void OnCancel() { /* ... */ }
}
```

### 特点

| 方面 | 说明 |
|------|------|
| **代码量** | 中等 |
| **事件流** | 显式（可见事件订阅） |
| **CanExecute 更新** | ✅ 自动（优化后） |
| **调试** | 容易（F12 跳转、断点） |
| **适用场景** | 复杂逻辑、学习、调试 |

---

## 显式模式 + 混合参数

**同时处理带参数和不带参数的 Actions**

### 代码示例

```csharp
// ========================================
// View Interface
// ========================================
public interface IMyView : IWindowView
{
    // ✅ 单个事件处理所有类型（多态）
    event EventHandler<ActionRequestEventArgs> ActionRequest;
    ViewActionBinder ActionBinder { get; }
}

// ========================================
// View 实现
// ========================================
public class MyForm : Form, IMyView
{
    private ViewActionBinder _binder;
    public ViewActionBinder ActionBinder => _binder;
    public event EventHandler<ActionRequestEventArgs> ActionRequest;

    private void InitializeActionBindings()
    {
        _binder = new ViewActionBinder();

        // 绑定所有 actions
        _binder.Add(CommonActions.Refresh, _btnRefresh);
        _binder.Add(TabActions.SelectTab, _btnTab1, _btnTab2, _btnTab3);
        _binder.Add(DocumentActions.Load, _btnLoad);

        // ✅ 根据 action 类型决定是否附加参数
        _binder.ActionTriggered += (s, e) =>
        {
            if (e.ActionKey == TabActions.SelectTab)
            {
                // 需要参数 - 创建 ActionRequestEventArgs<int>
                int tabIndex = GetTabIndex();
                var args = new ActionRequestEventArgs<int>(e.ActionKey, tabIndex);
                ActionRequest?.Invoke(this, args);
            }
            else if (e.ActionKey == DocumentActions.Load)
            {
                // 需要参数 - 创建 ActionRequestEventArgs<string>
                string docName = _txtDocument.Text;
                var args = new ActionRequestEventArgs<string>(e.ActionKey, docName);
                ActionRequest?.Invoke(this, args);
            }
            else
            {
                // 无参数 - 直接转发
                ActionRequest?.Invoke(this, e);
            }
        };
    }
}

// ========================================
// Presenter
// ========================================
public class MyPresenter : WindowPresenterBase<IMyView>
{
    protected override void OnViewAttached()
    {
        // ✅ 直接使用基类方法，自动处理参数！
        View.ActionRequest += OnViewActionTriggered;
    }

    protected override void RegisterViewActions()
    {
        // 简单 actions
        Dispatcher.Register(CommonActions.Refresh, OnRefresh);

        // 参数化 actions
        Dispatcher.Register<int>(TabActions.SelectTab, OnSelectTab);
        Dispatcher.Register<string>(DocumentActions.Load, OnLoadDocument);
    }

    // ✅ 不需要手动检查参数！OnViewActionTriggered (基类方法) 自动处理了：
    // - 检查 e is IActionRequestEventArgsWithValue
    // - 提取 payload = valueProvider.GetValue()
    // - 调用 Dispatcher.Dispatch(actionKey, payload)

    private void OnRefresh() { /* ... */ }
    private void OnSelectTab(int tabIndex) { /* ... */ }
    private void OnLoadDocument(string docName) { /* ... */ }
}
```

### 特点

| 方面 | 说明 |
|------|------|
| **代码量** | 中等偏多 |
| **参数支持** | ✅ 完整（任意类型） |
| **类型安全** | 运行时检查 |
| **灵活性** | 最高 |
| **适用场景** | 需要混合参数类型的复杂 UI |

---

## 模式对比

### 事件流对比

#### 隐式模式

```
Button Click
    ↓
ViewActionBinder (内部 handler)
    ↓
callback: dispatcher.Dispatch(actionKey)
    ↓
OnSave()
    ↓
ActionExecuted event
    ↓
ViewActionBinder.UpdateCanExecuteStates() ✅ 自动
```

#### 显式模式

```
Button Click
    ↓
ViewActionBinder (内部 handler)
    ↓
ActionTriggered event
    ↓
View.ActionRequest event
    ↓
Presenter.OnViewActionTriggered
    ↓
Dispatcher.Dispatch(actionKey)
    ↓
OnSave()
    ↓
ActionExecuted event
    ↓
ViewActionBinder.UpdateCanExecuteStates() ✅ 自动
```

#### 显式模式 + 参数

```
Button Click
    ↓
ViewActionBinder (内部 handler)
    ↓
ActionTriggered event
    ↓
View: 检查是否需要参数
    ↓
如需参数: ActionRequestEventArgs<T>(actionKey, parameter)
如不需要: ActionRequestEventArgs(actionKey)
    ↓
View.ActionRequest event
    ↓
Presenter.OnActionRequest
    ↓
检查: e is IActionRequestEventArgsWithValue?
    ↓
Dispatcher.Dispatch(actionKey, parameter?)
    ↓
OnAction(parameter?)
```

### 代码量对比

| 模式 | View Interface 事件 | View 代码行数 | Presenter 代码行数 |
|------|-------------------|--------------|------------------|
| 隐式 | 0 | ~10 | ~10 |
| 显式 | 1 | ~20 | ~15 |
| 显式+混合参数 | 1 | ~40 | ~25 |

### 功能对比

| 功能 | 隐式 | 显式 | 显式+混合参数 |
|------|-----|------|-------------|
| 自动 CanExecute | ✅ | ✅ | ✅ |
| 显式事件订阅 | ❌ | ✅ | ✅ |
| F12 导航 | ❌ | ✅ | ✅ |
| IDE 重构支持 | ⚠️ | ✅ | ✅ |
| 调试容易度 | ⚠️ | ✅ | ✅ |
| 参数支持 | ✅ | ✅ | ✅ |
| 混合参数 | ❌ | ❌ | ✅ |
| 代码简洁度 | ✅✅✅ | ✅✅ | ✅ |

---

## 如何选择

### 决策树

```
开始
  │
  ├─ 需要显式事件订阅（调试/学习）？
  │   ├─ YES → 需要混合参数类型？
  │   │         ├─ YES → 显式模式 + 混合参数
  │   │         └─ NO  → 显式模式
  │   │
  │   └─ NO  → 隐式模式（推荐）
```

### 推荐场景

#### 隐式模式

- ✅ 标准 CRUD 应用
- ✅ 简单表单
- ✅ 快速原型
- ✅ 团队熟悉框架
- ✅ 不需要复杂调试

#### 显式模式

- ✅ 复杂业务逻辑
- ✅ 需要频繁调试
- ✅ 学习 MVP 模式
- ✅ 团队偏好显式代码
- ✅ 需要 F12 导航到事件处理器

#### 显式模式 + 混合参数

- ✅ 同一 View 中有多种参数类型
- ✅ 复杂 UI（如包含多个选项卡、文档编辑器等）
- ✅ 需要从不同控件提取参数
- ✅ 参数来源多样化

---

## 示例代码文件

| 模式 | 文件路径 |
|------|---------|
| 隐式模式 | `src/WinformsMVP.Samples/ViewActionExample.cs` |
| 显式模式 | `src/WinformsMVP.Samples/ViewActionExplicitEventExample.cs` |
| 显式+混合参数 | `src/WinformsMVP.Samples/ViewActionExplicitWithParametersExample.cs` |
| 参数化 Actions | `src/WinformsMVP.Samples/ViewActionWithParametersExample.cs` |

---

## 核心要点

### 所有模式共享的特性

1. ✅ **自动 CanExecute UI 更新**（优化后）
   - 按钮自动启用/禁用
   - 无需手动调用 `RaiseCanExecuteChanged()`

2. ✅ **防止双重分发**
   - 框架自动检测模式
   - 智能跳过重复执行

3. ✅ **类型安全**
   - 编译时检查 Action 定义
   - 运行时检查参数类型

4. ✅ **可测试**
   - Mock View 和 Services
   - 独立测试 Presenter 逻辑

### 关键设计原则

1. **View 拥有 UI 控件**
   - View 负责参数提取
   - Presenter 不知道控件存在

2. **Presenter 处理业务逻辑**
   - 验证参数
   - 执行业务操作
   - 调用服务

3. **单一职责**
   - ViewActionBinder：控件绑定
   - ViewActionDispatcher：Action 路由
   - Presenter：业务逻辑

---

## 相关文档

- [优化总结](OPTIMIZATION-SUMMARY.md) - 显式模式优化详情
- [混合参数模式](MIXED-PARAMETERS-PATTERN.md) - 混合参数详细说明
- [CLAUDE.md](CLAUDE.md) - 完整框架文档

---

**快速开始**：大多数情况下使用**隐式模式**即可。只有在需要显式事件流或调试时才使用显式模式。

✨ **新功能**：显式模式现在也支持自动 CanExecute UI 更新了！
