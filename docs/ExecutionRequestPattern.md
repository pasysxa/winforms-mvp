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
    public T Param { get; }                                         // ✅ 业务数据参数
    public Action<TResult> Callback { get; }                        // ✅ 回调函数（Callback 版本）
    public Func<T, ExecutionResult<TResult>> Executor { get; }      // ✅ 执行器（Executor 版本）

    // 构造函数 - Callback 版本（向后兼容）
    public ExecutionRequestEventArgs(T param, Action<TResult> callback)
    {
        Param = param;
        Callback = callback;
    }

    // 构造函数 - Executor 版本（用于遗留窗体集成）
    public ExecutionRequestEventArgs(Func<T, ExecutionResult<TResult>> executor, T param)
    {
        Executor = executor;
        Param = param;
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

---

## 🚀 ExecutionResult - 遗留窗体集成（Executor 版本）

`ExecutionResult<TResult>` 是为打开遗留窗体（非MVP窗体）设计的辅助类，支持：
- 管理窗体的事件订阅生命周期（IDisposable）
- 获取窗体的最终返回结果
- 支持模态和非模态窗体

### ExecutionResult 类定义

```csharp
public class ExecutionResult<TResult> : IDisposable
{
    public TResult Result { get; private set; }           // 窗体返回的最终结果
    public event EventHandler<TResult> Completed;         // 非模态窗体关闭时触发

    // 构造1: 模态窗体 - 传结果和清理逻辑
    public ExecutionResult(TResult result, Action disposeAction) { }

    // 构造2: 模态窗体 - 直接传 Form（最常用）
    public ExecutionResult(TResult result, IDisposable disposable) { }

    // 构造3: 非模态窗体 - 传清理逻辑
    public ExecutionResult(Action disposeAction) { }

    // 构造4: 非模态窗体 - 直接传 Form
    public ExecutionResult(IDisposable disposable) { }

    // 设置结果并触发 Completed 事件（非模态窗体用）
    public void SetResult(TResult result) { }

    public void Dispose() { }  // 自动清理事件订阅和释放资源
}
```

### 使用场景对比

| 维度 | Callback 版本 | Executor 版本 |
|------|---------------|---------------|
| **用途** | 通用业务逻辑 | 打开遗留窗体 |
| **事件订阅** | 手动管理 | 自动管理（IDisposable） |
| **窗体类型** | 不直接涉及 | 支持模态和非模态 |
| **复杂度** | 简单 | 稍复杂（适合遗留集成） |
| **推荐场景** | 一般业务逻辑 | 遗留窗体集成 |

---

## 📚 完整示例：模态遗留窗体集成

### 场景：打开旧系统的客户编辑窗体

#### 1. View 接口定义

```csharp
public interface ICustomerManagementView : IWindowView
{
    // 使用 Executor 版本，专门用于打开遗留窗体
    event EventHandler<ExecutionRequestEventArgs<CustomerData, CustomerData>>
        EditLegacyCustomerRequested;

    void ShowCustomerInfo(CustomerData data);
    void BindActions(ViewActionDispatcher dispatcher);
}
```

#### 2. View 实现 - 触发事件（打开遗留窗体）

```csharp
public class CustomerManagementForm : Form, ICustomerManagementView
{
    public event EventHandler<ExecutionRequestEventArgs<CustomerData, CustomerData>>
        EditLegacyCustomerRequested;

    private void OnEditButtonClick(object sender, EventArgs e)
    {
        var currentCustomer = GetCurrentCustomer();

        // ✅ 使用 Executor 版本的构造函数
        var args = new ExecutionRequestEventArgs<CustomerData, CustomerData>(
            executor: (initialData) =>
            {
                // ★ 创建遗留窗体
                var legacyForm = new LegacyCustomerEditForm();

                // ★ 预填充数据
                if (initialData != null)
                {
                    legacyForm.txtName.Text = initialData.Name;
                    legacyForm.txtEmail.Text = initialData.Email;
                    legacyForm.numAge.Value = initialData.Age;
                }

                // ★ 订阅中间事件（实时验证）
                EventHandler<string> validationHandler = (s, msg) =>
                {
                    this.statusLabel.Text = msg;  // 更新主窗体状态栏
                };
                legacyForm.ValidationMessageReceived += validationHandler;

                // ★ 显示模态窗体（阻塞）
                var dialogResult = legacyForm.ShowDialog();

                // ★ 获取最终结果
                CustomerData result = null;
                if (dialogResult == DialogResult.OK)
                {
                    result = new CustomerData
                    {
                        Name = legacyForm.txtName.Text,
                        Email = legacyForm.txtEmail.Text,
                        Age = (int)legacyForm.numAge.Value
                    };
                }

                // ★ 返回 ExecutionResult（自动管理资源）
                return new ExecutionResult<CustomerData>(result, () =>
                {
                    // 清理：解除事件订阅
                    legacyForm.ValidationMessageReceived -= validationHandler;
                    legacyForm.Dispose();
                });
            },
            param: currentCustomer
        );

        EditLegacyCustomerRequested?.Invoke(this, args);
    }
}
```

#### 3. Presenter 处理 - 使用 Executor

```csharp
public class CustomerManagementPresenter : WindowPresenterBase<ICustomerManagementView>
{
    protected override void OnViewAttached()
    {
        View.EditLegacyCustomerRequested += OnEditLegacyCustomerRequested;
    }

    private void OnEditLegacyCustomerRequested(object sender,
        ExecutionRequestEventArgs<CustomerData, CustomerData> e)
    {
        // ★ 调用 Executor（View 会打开窗体并管理事件）
        using var execResult = e.Executor(e.Param);

        // ★ 拿到最终结果
        var result = execResult.Result;

        if (result != null)
        {
            // 业务逻辑验证
            if (!IsValidEmail(result.Email))
            {
                Messages.ShowWarning("邮箱格式不正确", "验证失败");
                return;
            }

            // 保存到数据库
            SaveCustomerToDatabase(result);

            // 更新 View 显示
            View.ShowCustomerInfo(result);

            Messages.ShowInfo("客户信息已保存", "成功");
        }
        else
        {
            Messages.ShowInfo("操作已取消", "提示");
        }

        // ← using 结束，自动调用 Dispose，清理事件订阅
    }
}
```

#### 4. 遗留窗体（非MVP）

```csharp
public class LegacyCustomerEditForm : Form
{
    public TextBox txtName;
    public TextBox txtEmail;
    public NumericUpDown numAge;

    // 中间事件（实时验证）
    public event EventHandler<string> ValidationMessageReceived;

    public LegacyCustomerEditForm()
    {
        InitializeComponent();

        // 实时验证触发中间事件
        txtEmail.TextChanged += (s, e) =>
        {
            if (!string.IsNullOrEmpty(txtEmail.Text))
            {
                bool valid = txtEmail.Text.Contains("@");
                ValidationMessageReceived?.Invoke(this,
                    valid ? "邮箱格式正确" : "邮箱格式不正确");
            }
        };
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("姓名不能为空", "错误");
            return;
        }

        this.DialogResult = DialogResult.OK;
        this.Close();
    }
}
```

### 关键优势

1. **自动资源管理**：`using` 语句自动清理事件订阅
2. **中间事件支持**：可以订阅窗体的进度、验证等事件
3. **符合MVP原则**：Presenter 调用抽象的 Executor，不直接依赖窗体类型
4. **简洁直观**：90% 场景只需直接传 Form 即可

---

## 🔄 推荐方案对比：ExecutionResult vs 服务接口

虽然 `ExecutionResult` 提供了便利的遗留窗体集成，但**服务接口包装**仍然是长期维护的推荐方案。

### 方案1：ExecutionResult（过渡方案）

**优点**：
- ✅ 快速集成遗留窗体
- ✅ 支持订阅中间事件
- ✅ 自动管理资源

**缺点**：
- ⚠️ MVP 纯度较低（Executor 仍然接触 UI）
- ⚠️ 不易于单元测试
- ⚠️ 不便于替换实现

**适用场景**：
- 临时集成遗留窗体，未来会重构
- 需要订阅窗体的中间事件
- 快速原型开发

### 方案2：服务接口包装（推荐）

```csharp
// 步骤1：定义服务接口
public interface ILegacyCustomerService
{
    CustomerData EditCustomer(CustomerData initialData);
}

// 步骤2：实现服务（可以使用 UI 类型）
public class LegacyCustomerService : ILegacyCustomerService
{
    public CustomerData EditCustomer(CustomerData initialData)
    {
        var form = new LegacyCustomerEditForm();
        if (initialData != null)
        {
            form.SetData(initialData);
        }

        var result = form.ShowDialog();
        return result == DialogResult.OK ? form.GetData() : null;
    }
}

// 步骤3：Presenter 使用服务
public class CustomerManagementPresenter : WindowPresenterBase<ICustomerManagementView>
{
    private readonly ILegacyCustomerService _legacyCustomerService;

    public CustomerManagementPresenter(ILegacyCustomerService legacyCustomerService)
    {
        _legacyCustomerService = legacyCustomerService;
    }

    private void OnEditCustomerAction()
    {
        var result = _legacyCustomerService.EditCustomer(_currentCustomer);
        if (result != null)
        {
            View.ShowCustomerInfo(result);
        }
    }
}
```

**优点**：
- ✅ 完全符合 MVP 原则
- ✅ 易于单元测试（可以 Mock）
- ✅ 易于替换实现
- ✅ 隐藏了所有 UI 类型

**缺点**：
- ⚠️ 需要额外定义接口
- ⚠️ 代码量稍多

**适用场景**：
- ✅ 长期维护的遗留代码
- ✅ 需要高度可测试性
- ✅ 希望逐步重构遗留代码

### 何时使用哪种方案

| 场景 | ExecutionResult | 服务接口包装 |
|------|----------------|-------------|
| 快速原型 | ✅ 推荐 | ❌ 太重 |
| 临时集成（会重构） | ✅ 推荐 | ⚠️ 可选 |
| 长期维护 | ⚠️ 不推荐 | ✅ 强烈推荐 |
| 需要订阅中间事件 | ✅ 推荐 | ⚠️ 需手动管理 |
| 需要单元测试 | ⚠️ 难测试 | ✅ 易测试 |
| 团队协作 | ⚠️ 需文档 | ✅ 接口自说明 |

---

## 📖 最佳实践建议

1. **新功能开发**：优先使用框架提供的服务接口（`IDialogProvider`、`IMessageService` 等）
2. **遗留代码集成（短期）**：使用 `ExecutionResult` 快速集成
3. **遗留代码集成（长期）**：创建服务接口包装，逐步重构
4. **学习 MVP 模式**：使用 `ExecutionRequest` 理解 View → Presenter 通信
5. **始终遵守三条铁律**：无论使用哪种方式，都必须只使用业务数据类型
