# ExecutionRequest 模式 - 学习工具与过渡方案

## 📖 定位

`ExecutionRequestEventArgs` 是为**不熟悉 MVP 的开发者**提供的**学习工具和过渡方案**。

### 核心价值

1. **学习路径** - 帮助开发者理解 View → Presenter 通信
2. **过渡方案** - 从旧代码逐步迁移到 MVP
3. **符合铁律** - 必须只使用**业务数据类型**，不能使用 UI 类型

---

## ⚠️ MVP 三条铁律（必须遵守）

### 铁律 1：视图接口纯净性
**视图接口的参数和返回值不能包含 UI 类型**

```csharp
// ❌ 错误 - 包含 UI 类型
event EventHandler<ExecutionRequestEventArgs<Type, DialogResult>> OpenFormRequested;

// ✅ 正确 - 只包含业务数据类型
event EventHandler<ExecutionRequestEventArgs<CustomerData, CustomerData>> EditCustomerRequested;
```

### 铁律 2：Presenter 纯净性
**Presenter 方法的参数和返回值不能包含 UI 类型**

```csharp
// ❌ 错误
public DialogResult OpenForm(Type formType) { }

// ✅ 正确
public CustomerData EditCustomer(CustomerData initialData) { }
```

### 铁律 3：单向依赖
**依赖方向：Presenter → View（视图不依赖 Presenter）**

```csharp
// ❌ 错误 - 视图持有 Presenter 引用
public class MyForm : Form, IMyView
{
    private MyPresenter _presenter;  // ❌
}

// ✅ 正确 - 通过事件和回调通信
public class MyForm : Form, IMyView
{
    public event EventHandler<ExecutionRequestEventArgs<T, TResult>> SomeRequested;  // ✅
}
```

---

## 🎯 推荐使用优先级

```
1. 服务接口（IDialogProvider、IMessageService 等） > ExecutionRequest
2. ExecutionRequest 作为学习工具和特殊场景补充
```

### 已有的服务接口（优先使用）

| 功能 | 服务接口 | 说明 |
|------|---------|------|
| 系统对话框 | `IDialogProvider` | OpenFile、SaveFile、Color、Font 等 |
| 消息提示 | `IMessageService` | Info、Warning、Error、Confirm |
| 窗体导航 | `INavigator` | 打开 MVP 窗体 |

### 何时使用 ExecutionRequest

- ✅ **学习 MVP 模式** - 理解 View → Presenter 通信
- ✅ **特殊业务逻辑** - 无法通过现有服务接口解决
- ✅ **遗留代码集成** - 可以自己创建服务接口（见下文）

---

## 📋 类定义

框架提供了三个重载，支持不同数量的参数：

### 单参数版本

```csharp
public class ExecutionRequestEventArgs<T, TResult> : EventArgs
{
    public T Param { get; }                    // ✅ 业务数据参数
    public Action<TResult> Callback { get; }   // ✅ 回调函数

    public ExecutionRequestEventArgs(T param, Action<TResult> callback)
    {
        Param = param;
        Callback = callback;
    }
}
```

### 双参数版本

```csharp
public class ExecutionRequestEventArgs<T1, T2, TResult> : EventArgs
{
    public T1 Param1 { get; }
    public T2 Param2 { get; }
    public Action<TResult> Callback { get; }
}
```

### 三参数版本

```csharp
public class ExecutionRequestEventArgs<T1, T2, T3, TResult> : EventArgs
{
    public T1 Param1 { get; }
    public T2 Param2 { get; }
    public T3 Param3 { get; }
    public Action<TResult> Callback { get; }
}
```

**关键**：所有类型参数（`T`、`TResult`）必须是业务数据类型，不能是 UI 类型！

---

## 🔧 正确使用示例

### 场景1：编辑客户信息

```csharp
// ✅ View 接口 - 只使用业务数据类型
public interface ICustomerView : IWindowView
{
    // 参数：CustomerData（要编辑的数据，null = 新建）
    // 返回：CustomerData（编辑结果，null = 取消）
    event EventHandler<ExecutionRequestEventArgs<CustomerData, CustomerData>>
        EditCustomerRequested;
}

// ✅ View 实现 - 触发请求
public class CustomerForm : Form, ICustomerView
{
    public event EventHandler<ExecutionRequestEventArgs<CustomerData, CustomerData>>
        EditCustomerRequested;

    private void OnEditButtonClick(object sender, EventArgs e)
    {
        var currentCustomer = GetCurrentCustomer();

        var request = new ExecutionRequestEventArgs<CustomerData, CustomerData>(
            param: currentCustomer,       // ✅ 业务数据
            callback: OnCustomerEdited    // ✅ 回调
        );

        EditCustomerRequested?.Invoke(this, request);
    }

    private void OnCustomerEdited(CustomerData editedCustomer)
    {
        if (editedCustomer != null)
        {
            // 用户确认了编辑
            UpdateCustomerDisplay(editedCustomer);
        }
        else
        {
            // 用户取消了
            ShowStatus("操作已取消");
        }
    }
}

// ✅ Presenter 实现 - 处理请求
public class CustomerPresenter : WindowPresenterBase<ICustomerView>
{
    protected override void OnViewAttached()
    {
        View.EditCustomerRequested += OnEditCustomerRequested;
    }

    private void OnEditCustomerRequested(object sender,
        ExecutionRequestEventArgs<CustomerData, CustomerData> e)
    {
        // Presenter 决定如何编辑（可以使用遗留窗体、新窗体等）
        var result = EditCustomerUsingLegacyForm(e.Param);
        e.Callback?.Invoke(result);  // ✅ 返回业务数据或 null
    }

    // ✅ 注意：这个方法是 private，可以使用 UI 类型（实现细节）
    private CustomerData EditCustomerUsingLegacyForm(CustomerData initialData)
    {
        var form = new LegacyCustomerForm();

        if (initialData != null)
        {
            form.SetData(initialData);
        }

        var dialogResult = form.ShowDialog();  // ⚠️ 这里可以用 DialogResult（private 方法）

        if (dialogResult == DialogResult.OK)
        {
            return form.GetData();  // ✅ 返回业务数据
        }

        return null;  // ✅ 取消返回 null（不返回 DialogResult）
    }
}
```

### 场景2：保存数据

```csharp
// ✅ View 接口
public interface IDataView : IWindowView
{
    // 参数：CustomerData
    // 返回：bool（是否成功）
    event EventHandler<ExecutionRequestEventArgs<CustomerData, bool>>
        SaveDataRequested;
}

// View 触发
private void OnSaveClick(object sender, EventArgs e)
{
    var data = CollectData();

    var request = new ExecutionRequestEventArgs<CustomerData, bool>(
        param: data,
        callback: success =>
        {
            if (success)
            {
                ShowSuccess("保存成功");
            }
            else
            {
                ShowError("保存失败");
            }
        }
    );

    SaveDataRequested?.Invoke(this, request);
}

// Presenter 处理
private void OnSaveDataRequested(object sender,
    ExecutionRequestEventArgs<CustomerData, bool> e)
{
    var result = _dataService.Save(e.Param);
    e.Callback?.Invoke(result);
}
```

---

## ❌ 错误示例（违反三条铁律）

### 错误1：使用 UI 类型作为参数

```csharp
// ❌ 严重错误 - Type 和 DialogResult 是 UI 类型
public interface IMyView : IWindowView
{
    event EventHandler<ExecutionRequestEventArgs<Type, (DialogResult, CustomerData)>>
        OpenFormRequested;  // ❌ 违反铁律1
}

// ✅ 正确 - 使用业务数据类型
public interface IMyView : IWindowView
{
    event EventHandler<ExecutionRequestEventArgs<string, CustomerData>>
        EditCustomerRequested;  // ✅ 用 string 作为标识符，或直接传递 CustomerData
}
```

### 错误2：Presenter 方法暴露 UI 类型

```csharp
// ❌ 错误 - public 方法返回 UI 类型
public class MyPresenter
{
    public DialogResult OpenForm(Type formType)  // ❌ 违反铁律2
    {
        // ...
    }
}

// ✅ 正确 - private 方法可以使用 UI 类型
public class MyPresenter
{
    private CustomerData EditCustomerInternal(CustomerData data)
    {
        var form = new LegacyForm();  // ✅ private 方法，实现细节
        var dialogResult = form.ShowDialog();  // ✅ OK
        return dialogResult == DialogResult.OK ? form.GetData() : null;
    }
}
```

---

## 💡 处理遗留代码的推荐方式

### 方式1：创建自己的服务接口（推荐）

虽然框架不提供特定的遗留窗体服务接口，但你可以根据项目需求创建自己的：

```csharp
// 你自己创建的服务接口
public interface ILegacyFormService
{
    CustomerData EditCustomer(CustomerData initialData);
    OrderData EditOrder(OrderData initialData);
    // ... 根据项目需求定义
}

// 实现（可以使用 UI 类型）
public class LegacyFormService : ILegacyFormService
{
    public CustomerData EditCustomer(CustomerData initialData)
    {
        var form = new LegacyCustomerForm();
        if (initialData != null)
        {
            form.SetData(initialData);
        }

        var result = form.ShowDialog();  // ✅ 实现层可以用 UI 类型
        return result == DialogResult.OK ? form.GetData() : null;
    }
}

// Presenter 使用
public class MyPresenter : WindowPresenterBase<IMyView>
{
    private readonly ILegacyFormService _legacyFormService;

    public MyPresenter(ILegacyFormService legacyFormService)
    {
        _legacyFormService = legacyFormService;
    }

    private void OnEditAction()
    {
        var result = _legacyFormService.EditCustomer(_currentCustomer);
        if (result != null)
        {
            View.ShowCustomer(result);
        }
    }
}
```

### 方式2：使用 ExecutionRequest（学习工具）

```csharp
// View 接口
public interface IMyView : IWindowView
{
    event EventHandler<ExecutionRequestEventArgs<CustomerData, CustomerData>>
        EditCustomerRequested;
}

// Presenter 处理
private void OnEditCustomerRequested(object sender,
    ExecutionRequestEventArgs<CustomerData, CustomerData> e)
{
    var result = EditCustomerUsingLegacyForm(e.Param);
    e.Callback?.Invoke(result);
}
```

---

## 📝 总结

### ✅ 核心要点

1. **ExecutionRequest 是学习工具** - 帮助理解 MVP 通信
2. **必须符合三条铁律** - 只使用业务数据类型
3. **优先使用服务接口** - IDialogProvider、IMessageService 等
4. **可以创建自己的服务接口** - 处理特定业务逻辑

### ⚠️ 关键原则

- 接口层：只能有业务数据类型
- 实现层（private 方法）：可以使用 UI 类型
- 依赖方向：Presenter → View

### 🎓 学习建议

1. 先学习使用框架提供的服务接口
2. 理解 ExecutionRequest 的正确用法
3. 根据项目需求创建自己的服务接口
4. 逐步从 ExecutionRequest 迁移到服务接口

**记住：ExecutionRequest 本身没有问题，问题在于使用时违反了三条铁律！**
