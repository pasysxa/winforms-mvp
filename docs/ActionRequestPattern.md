# ActionRequest 模式 - 解决复杂画面的事件爆炸问题

## 📖 问题背景

在复杂的 WinForms 应用中，一个画面可能有几十个操作按钮：
- 数据操作：添加、编辑、删除、刷新
- 导入导出：导入、导出、打印
- 数据筛选：筛选、排序、搜索、分组
- 其他功能：设置、帮助、关于...

### ❌ 传统方式的问题：事件爆炸

使用传统的 MVP 模式，每个操作都需要在 View 接口中定义一个单独的事件：

```csharp
public interface IComplexDataGridView : IWindowView
{
    // 😱 需要定义大量事件
    event EventHandler AddRequested;
    event EventHandler EditRequested;
    event EventHandler DeleteRequested;
    event EventHandler RefreshRequested;
    event EventHandler ExportRequested;
    event EventHandler ImportRequested;
    event EventHandler PrintRequested;
    event EventHandler FilterRequested;
    event EventHandler SortRequested;
    event EventHandler SearchRequested;
    event EventHandler SettingsRequested;
    event EventHandler HelpRequested;
    // ... 可能还有 20+ 个事件

    ViewActionBinder ActionBinder { get; }
}
```

**问题**：
1. 😱 **接口臃肿**：10 个操作 = 10 个事件定义
2. 😱 **维护困难**：添加新操作需要修改接口
3. 😱 **代码重复**：每个事件都需要订阅/取消订阅
4. 😱 **难以扩展**：接口越来越复杂

---

## ✅ 解决方案：ActionRequestEventArgs

`ActionRequestEventArgs` 提供了一个统一的事件机制，通过 `ViewAction` 区分不同的操作。

### 核心类定义

```csharp
// 无参数的 ActionRequest
public class ActionRequestEventArgs
{
    public ActionRequestEventArgs(ViewAction actionKey)
    {
        ActionKey = actionKey;
    }
    public ViewAction ActionKey { get; }
}

// 带参数的 ActionRequest
public class ActionRequestEventArgs<T> : ActionRequestEventArgs, IActionRequestEventArgsWithValue
{
    public ActionRequestEventArgs(ViewAction actionKey, T value) : base(actionKey)
    {
        Value = value;
    }

    public T Value { get; }
    public object GetValue() => Value;
}
```

### ✅ 使用 ActionRequest 后的效果

```csharp
public interface IComplexDataGridView : IWindowView
{
    // ✅ 只需要一个事件！
    event EventHandler<ActionRequestEventArgs> ActionRequested;

    ViewActionBinder ActionBinder { get; }
    void UpdateStatus(string message);
}
```

**优势**：
- ✅ **接口简洁**：10 个操作 = 1 个事件
- ✅ **易于维护**：添加新操作不需要修改接口
- ✅ **统一处理**：所有操作使用相同的事件模式
- ✅ **类型安全**：通过 ViewAction 编译时检查

---

## 📋 完整示例

### 1. 定义 ActionKey

```csharp
public static class ComplexDataGridActions
{
    private static readonly ViewActionFactory Factory =
        ViewAction.Factory.WithQualifier("ComplexDataGrid");

    // 定义所有操作的 ActionKey
    public static readonly ViewAction Add = Factory.Create("Add");
    public static readonly ViewAction Edit = Factory.Create("Edit");
    public static readonly ViewAction Delete = Factory.Create("Delete");
    public static readonly ViewAction Refresh = Factory.Create("Refresh");
    public static readonly ViewAction Export = Factory.Create("Export");
    public static readonly ViewAction Import = Factory.Create("Import");
    public static readonly ViewAction Print = Factory.Create("Print");
    public static readonly ViewAction Filter = Factory.Create("Filter");
    public static readonly ViewAction Sort = Factory.Create("Sort");
    public static readonly ViewAction Search = Factory.Create("Search");
}
```

### 2. View 接口定义

```csharp
public interface IComplexDataGridView : IWindowView
{
    // ✅ 只需要一个统一的事件
    event EventHandler<ActionRequestEventArgs> ActionRequested;

    ViewActionBinder ActionBinder { get; }
    void UpdateStatus(string message);
}
```

### 3. View 实现（Form）

```csharp
public class ComplexDataGridForm : Form, IComplexDataGridView
{
    public event EventHandler<ActionRequestEventArgs> ActionRequested;

    private ViewActionBinder _binder;

    public ViewActionBinder ActionBinder => _binder;

    public ComplexDataGridForm()
    {
        InitializeComponent();
        InitializeActionBindings();
    }

    private void InitializeActionBindings()
    {
        _binder = new ViewActionBinder();

        // 绑定所有按钮到对应的 ActionKey
        _binder.Add(ComplexDataGridActions.Add, btnAdd);
        _binder.Add(ComplexDataGridActions.Edit, btnEdit);
        _binder.Add(ComplexDataGridActions.Delete, btnDelete);
        _binder.Add(ComplexDataGridActions.Refresh, btnRefresh);
        _binder.Add(ComplexDataGridActions.Export, btnExport);
        _binder.Add(ComplexDataGridActions.Import, btnImport);
        _binder.Add(ComplexDataGridActions.Print, btnPrint);
        _binder.Add(ComplexDataGridActions.Filter, btnFilter);
        _binder.Add(ComplexDataGridActions.Sort, btnSort);
        _binder.Add(ComplexDataGridActions.Search, btnSearch);

        // 订阅按钮点击事件，触发 ActionRequested（显式事件模式）
        _binder.ActionTriggered += (sender, e) =>
        {
            ActionRequested?.Invoke(this, e);
        };

        // 手动绑定（显式事件模式需要）
        // 注意：如果返回 null，框架不会自动绑定
        _binder.Bind();
    }

    public void UpdateStatus(string message)
    {
        statusLabel.Text = message;
    }
}
```

### 4. Presenter 实现

```csharp
public class ComplexDataGridPresenter : WindowPresenterBase<IComplexDataGridView>
{
    protected override void OnViewAttached()
    {
        // ✅ 只需要订阅一个事件（显式事件模式）
        View.ActionRequested += OnViewActionTriggered;  // 使用基类提供的辅助方法
    }

    protected override void RegisterViewActions()
    {
        // 注册所有操作的处理器
        Dispatcher.Register(ComplexDataGridActions.Add, OnAdd);
        Dispatcher.Register(ComplexDataGridActions.Edit, OnEdit);
        Dispatcher.Register(ComplexDataGridActions.Delete, OnDelete,
            canExecute: () => HasSelection());
        Dispatcher.Register(ComplexDataGridActions.Refresh, OnRefresh);
        Dispatcher.Register(ComplexDataGridActions.Export, OnExport);
        Dispatcher.Register(ComplexDataGridActions.Import, OnImport);
        Dispatcher.Register(ComplexDataGridActions.Print, OnPrint);
        Dispatcher.Register(ComplexDataGridActions.Filter, OnFilter);
        Dispatcher.Register(ComplexDataGridActions.Sort, OnSort);
        Dispatcher.Register(ComplexDataGridActions.Search, OnSearch);

        // 注意：此示例使用显式事件模式，View 自己调用 Bind()
        // 如果 View.ActionBinder 返回有效实例，框架会自动绑定
    }

    protected override void OnInitialize()
    {
        View.UpdateStatus("准备就绪");
    }

    private void OnAdd()
    {
        View.UpdateStatus("添加新记录...");
        // 实现添加逻辑
    }

    private void OnEdit()
    {
        View.UpdateStatus("编辑记录...");
        // 实现编辑逻辑
    }

    private void OnDelete()
    {
        View.UpdateStatus("删除记录...");
        // 实现删除逻辑
    }

    // ... 其他处理器

    private bool HasSelection()
    {
        // 检查是否有选中项
        return true;
    }

    protected override void Cleanup()
    {
        if (View != null)
        {
            View.ActionRequested -= OnViewActionTriggered;
        }
    }
}
```

---

## 🎯 带参数的 ActionRequest

某些操作需要传递参数（如搜索关键字、筛选条件等），可以使用 `ActionRequestEventArgs<T>`。

### 示例：搜索功能

#### 1. 定义 ActionKey

```csharp
public static class SearchActions
{
    private static readonly ViewActionFactory Factory =
        ViewAction.Factory.WithQualifier("Search");

    public static readonly ViewAction SearchByKeyword = Factory.Create("SearchByKeyword");
    public static readonly ViewAction FilterByCategory = Factory.Create("FilterByCategory");
}
```

#### 2. View 接口

```csharp
public interface ISearchableDataGridView : IWindowView
{
    // 无参数的操作
    event EventHandler<ActionRequestEventArgs> ActionRequested;

    // 带参数的操作（如搜索关键字）
    event EventHandler<ActionRequestEventArgs<string>> SearchActionRequested;

    ViewActionBinder ActionBinder { get; }
    void UpdateStatus(string message);
}
```

#### 3. View 实现

```csharp
public class SearchableDataGridForm : Form, ISearchableDataGridView
{
    public event EventHandler<ActionRequestEventArgs> ActionRequested;
    public event EventHandler<ActionRequestEventArgs<string>> SearchActionRequested;

    private void OnSearchButtonClick(object sender, EventArgs e)
    {
        var keyword = txtSearch.Text;

        // 触发带参数的 ActionRequest
        var args = new ActionRequestEventArgs<string>(
            SearchActions.SearchByKeyword,
            keyword);

        SearchActionRequested?.Invoke(this, args);
    }

    private void OnCategoryChanged(object sender, EventArgs e)
    {
        var category = cmbCategory.SelectedItem?.ToString();

        var args = new ActionRequestEventArgs<string>(
            SearchActions.FilterByCategory,
            category);

        SearchActionRequested?.Invoke(this, args);
    }
}
```

#### 4. Presenter 处理

```csharp
public class SearchableDataGridPresenter : WindowPresenterBase<ISearchableDataGridView>
{
    protected override void OnViewAttached()
    {
        View.ActionRequested += OnViewActionTriggered;
        View.SearchActionRequested += OnSearchActionTriggered;  // 带参数的事件
    }

    protected override void RegisterViewActions()
    {
        // 注册带参数的操作
        Dispatcher.Register<string>(
            SearchActions.SearchByKeyword,
            OnSearchByKeyword);

        Dispatcher.Register<string>(
            SearchActions.FilterByCategory,
            OnFilterByCategory);

        // 注意：框架会自动绑定 View.ActionBinder（如果不为 null）
    }

    // 处理带参数的 SearchAction 事件
    private void OnSearchActionTriggered(object sender, ActionRequestEventArgs<string> e)
    {
        DispatchAction(e);  // 使用基类的 DispatchAction 方法
    }

    private void OnSearchByKeyword(string keyword)
    {
        View.UpdateStatus($"搜索关键字: {keyword}");
        // 实现搜索逻辑
    }

    private void OnFilterByCategory(string category)
    {
        View.UpdateStatus($"筛选分类: {category}");
        // 实现筛选逻辑
    }

    protected override void Cleanup()
    {
        if (View != null)
        {
            View.ActionRequested -= OnViewActionTriggered;
            View.SearchActionRequested -= OnSearchActionTriggered;
        }
    }
}
```

---

## 📊 传统方式 vs ActionRequest 对比

| 维度 | 传统方式（独立事件） | ActionRequest 方式 |
|------|---------------------|-------------------|
| **View 接口复杂度** | 高（N 个事件） | 低（1-2 个事件） |
| **添加新操作** | 需修改接口 | 只需添加 ActionKey |
| **事件订阅/取消** | N 次订阅/取消 | 1 次订阅/取消 |
| **代码可读性** | 较低（事件分散） | 高（统一处理） |
| **类型安全** | ✅ | ✅ |
| **CanExecute 支持** | ✅ | ✅ |
| **适用场景** | 简单画面（< 5 个操作） | 复杂画面（> 5 个操作） |

---

## 💡 最佳实践

### 何时使用 ActionRequest

✅ **推荐使用的场景**：
- 复杂画面，操作数量 > 5 个
- 需要统一的操作处理逻辑
- 经常需要添加/删除操作
- 多个操作共享相似的处理流程

❌ **不推荐使用的场景**：
- 简单画面，操作数量 < 5 个（使用传统事件更直观）
- 每个操作的处理逻辑差异很大
- 需要非常明确的事件语义

### 混合使用

可以同时使用传统事件和 ActionRequest：

```csharp
public interface IMyView : IWindowView
{
    // 复杂的批量操作使用 ActionRequest
    event EventHandler<ActionRequestEventArgs> DataOperationRequested;

    // 特殊的、需要明确语义的操作使用独立事件
    event EventHandler<ExecutionRequestEventArgs<CustomerData, CustomerData>>
        EditLegacyCustomerRequested;

    ViewActionBinder ActionBinder { get; }
}
```

### 命名约定

- **ActionKey 类**：使用复数形式，如 `DataGridActions`、`SearchActions`
- **事件名**：使用 `ActionRequested`、`SearchActionRequested` 等清晰的名称
- **ActionKey 成员**：使用动词，如 `Add`、`Edit`、`Delete`、`Search`

---

## 🔧 基类支持

`PresenterBase` 提供了内置的辅助方法来简化 ActionRequest 的处理：

```csharp
public abstract class PresenterBase<TView>
{
    // 辅助方法：处理 ActionRequest 事件
    protected void OnViewActionTriggered(object sender, ActionRequestEventArgs e)
    {
        DispatchAction(e);
    }

    // 核心方法：将 ActionRequest 分发到注册的处理器
    protected void DispatchAction(ActionRequestEventArgs e)
    {
        if (e == null) return;

        var key = e.ActionKey;
        object payload = null;

        // 如果是带参数的 ActionRequest，提取参数
        if (e is IActionRequestEventArgsWithValue valueProvider)
        {
            payload = valueProvider.GetValue();
        }

        // 分发到 ViewActionDispatcher
        _dispatcher.Dispatch(key, payload);
    }
}
```

**使用方式**：

```csharp
protected override void OnViewAttached()
{
    // 直接使用基类提供的辅助方法
    View.ActionRequested += OnViewActionTriggered;
}
```

---

## 📝 总结

### ActionRequest 模式的价值

1. ✅ **解决事件爆炸**：统一的事件机制，避免接口臃肿
2. ✅ **简化维护**：添加新操作不需要修改接口
3. ✅ **统一处理**：所有操作使用相同的分发机制
4. ✅ **类型安全**：支持带类型参数的 ActionRequest
5. ✅ **框架支持**：基类提供内置的辅助方法

### 推荐使用流程

1. **简单画面（< 5 个操作）**：使用传统的独立事件
2. **复杂画面（> 5 个操作）**：使用 ActionRequest 模式
3. **混合使用**：批量操作用 ActionRequest，特殊操作用独立事件
4. **带参数操作**：使用 `ActionRequestEventArgs<T>`

**ActionRequest 是 ViewAction 系统的重要补充，让复杂画面的 MVP 实现更加优雅！**
