# MVP 模式三条铁律

## 📜 铁律内容

### 铁律1：视图接口纯净性

**视图接口以及接口方法的参数、返回值等，不能和 UI 元素有关。MessageBox 都不行。**

```csharp
// ❌ 错误 - 视图接口包含 UI 类型
public interface IMyView : IWindowView
{
    event EventHandler<ExecutionRequestEventArgs<Type, DialogResult>> FormRequested;  // ❌ Type, DialogResult 是 UI 类型
    void ShowMessage(MessageBoxButtons buttons);  // ❌ MessageBoxButtons 是 UI 类型
    Form GetParentForm();  // ❌ Form 是 UI 类型
}

// ✅ 正确 - 视图接口只包含数据类型
public interface IMyView : IWindowView
{
    // 数据属性
    string UserName { get; set; }
    int Age { get; set; }

    // 显示方法 - 参数是业务数据
    void ShowCustomerInfo(CustomerData data);
    void ShowValidationErrors(string[] errors);

    // ViewAction 绑定（属性模式）
    ViewActionBinder ActionBinder { get; }
}
```

**禁止的 UI 类型**：
- `Form`, `Control`, `Button`, `TextBox` 等所有 WinForms 控件
- `DialogResult`, `FormBorderStyle` 等 WinForms 枚举
- `MessageBox`, `MessageBoxButtons`, `MessageBoxIcon` 等
- `Type` (当用于表示 Form 类型时)
- 任何 `System.Windows.Forms` 命名空间下的类型

---

### 铁律2：Presenter 纯净性

**Presenter 中的方法以及方法的参数、返回值等，都不能和 UI 元素有关。MessageBox 都不行。**

```csharp
// ❌ 错误 - Presenter 包含 UI 类型
public class MyPresenter : WindowPresenterBase<IMyView>
{
    public DialogResult OpenDialog(Type formType)  // ❌ DialogResult, Type
    {
        var form = Activator.CreateInstance(formType) as Form;  // ❌ Form
        return form.ShowDialog();  // ❌ DialogResult
    }

    public void ShowMessage(string text, MessageBoxIcon icon)  // ❌ MessageBoxIcon
    {
        MessageBox.Show(text, "Title", MessageBoxButtons.OK, icon);  // ❌ MessageBox
    }
}

// ✅ 正确 - Presenter 使用服务接口
public class MyPresenter : WindowPresenterBase<IMyView>
{
    private readonly ILegacyFormService _legacyFormService;
    private readonly IMessageService _messageService;

    public MyPresenter(
        ILegacyFormService legacyFormService,
        IMessageService messageService)
    {
        _legacyFormService = legacyFormService;
        _messageService = messageService;
    }

    private void OnOpenDialogAction()
    {
        // ✅ 使用服务接口，返回业务数据
        var result = _legacyFormService.OpenForm<CustomerData>("CustomerEditor");

        if (result.IsOk)
        {
            View.ShowCustomerInfo(result.Value);
        }
    }

    private void OnShowMessageAction()
    {
        // ✅ 使用 IMessageService
        _messageService.ShowInfo("操作成功", "提示");
    }
}
```

---

### 铁律3：单向依赖

**视图接口与 Presenter 之间的依赖关系，始终是从 Presenter 到视图，视图不能依赖 Presenter。**

```csharp
// ❌ 错误 - 视图依赖 Presenter
public class MyForm : Form, IMyView
{
    private MyPresenter _presenter;  // ❌ 视图持有 Presenter 引用

    public void SetPresenter(MyPresenter presenter)  // ❌ 视图知道 Presenter
    {
        _presenter = presenter;
    }

    private void OnButtonClick(object sender, EventArgs e)
    {
        _presenter.DoSomething();  // ❌ 视图调用 Presenter
    }
}

// ✅ 正确 - Presenter 依赖视图接口
public class MyPresenter : WindowPresenterBase<IMyView>
{
    // ✅ Presenter 知道 View 接口（通过基类的 View 属性）
    // ✅ View 不知道 Presenter

    protected override void RegisterViewActions()
    {
        Dispatcher.Register(MyActions.DoSomething, OnDoSomethingAction);
        // ✅ 框架自动绑定 View.ActionBinder（如果不为 null）
    }

    private void OnDoSomethingAction()
    {
        // Presenter 的业务逻辑
        View.UpdateStatus("完成");  // ✅ 通过接口方法更新视图
    }
}

// ✅ 正确 - 视图实现接口，不知道 Presenter
public class MyForm : Form, IMyView
{
    private ViewActionBinder _binder;

    public ViewActionBinder ActionBinder => _binder;

    public MyForm()
    {
        InitializeComponent();
        InitializeActionBindings();
    }

    private void InitializeActionBindings()
    {
        _binder = new ViewActionBinder();
        _binder.Add(MyActions.DoSomething, _myButton);
        // ✅ 框架会自动绑定（通过 ActionBinder 属性）
    }

    // ✅ 视图不知道 Presenter 的存在
}
```

**依赖方向图**：

```
┌─────────────┐
│  Presenter  │───depends on───▶│  IView Interface  │
└─────────────┘                 └───────────────────┘
                                          ▲
                                          │
                                    implements
                                          │
                                    ┌─────┴──────┐
                                    │  View Form │
                                    └────────────┘

✅ Presenter → IView ✅
❌ View ← Presenter ❌  (绝对禁止)
```

---

## 🎯 实际应用

### 场景1：打开遗留窗体

```csharp
// ✅ 正确做法：通过 ILegacyFormService
public class MyPresenter : WindowPresenterBase<IMyView>
{
    private readonly ILegacyFormService _legacyFormService;

    private void OnOpenLegacyFormAction()
    {
        // ✅ 使用服务接口，完全没有 UI 类型
        var result = _legacyFormService.OpenForm<CustomerData>("CustomerEditor");

        if (result.IsOk)
        {
            View.ShowCustomerInfo(result.Value);
        }
        else if (result.IsCancelled)
        {
            View.UpdateStatus("用户取消了操作", false);
        }
        else
        {
            View.UpdateStatus($"错误: {result.ErrorMessage}", false);
        }
    }
}
```

### 场景2：文件选择对话框

```csharp
// ✅ 正确做法：通过 IDialogProvider
public class MyPresenter : WindowPresenterBase<IMyView>
{
    private readonly IDialogProvider _dialogProvider;

    private void OnSelectFileAction()
    {
        var options = new OpenFileDialogOptions
        {
            Filter = "CSV Files|*.csv|All Files|*.*",
            Title = "选择文件"
        };

        var result = _dialogProvider.ShowOpenFileDialog(options);

        if (result.IsOk)
        {
            View.ShowSelectedFile(result.Value);  // ✅ result.Value 是 string
        }
    }
}
```

### 场景3：消息提示

```csharp
// ✅ 正确做法：通过 IMessageService
public class MyPresenter : WindowPresenterBase<IMyView>
{
    private readonly IMessageService _messageService;

    private void OnSaveAction()
    {
        try
        {
            // 保存逻辑...
            _messageService.ShowInfo("保存成功", "提示");
            View.UpdateStatus("数据已保存", true);
        }
        catch (Exception ex)
        {
            _messageService.ShowError($"保存失败: {ex.Message}", "错误");
            View.UpdateStatus("保存失败", false);
        }
    }
}
```

---

## ❌ 常见违规示例

### 1. ExecutionRequest 模式（完全违规）

```csharp
// ❌ 严重违规
public interface IMyView : IWindowView
{
    // ❌ 违反铁律1：Type 和 DialogResult 是 UI 类型
    event EventHandler<ExecutionRequestEventArgs<Type, DialogResult>>
        OpenFormRequested;
}

public class MyPresenter : WindowPresenterBase<IMyView>
{
    // ❌ 违反铁律2：返回值包含 DialogResult
    public DialogResult OpenForm(Type formType)
    {
        var form = Activator.CreateInstance(formType) as Form;
        return form.ShowDialog();
    }
}
```

**结论：ExecutionRequest 模式应该被完全废弃！**

### 2. 视图持有 Presenter 引用

```csharp
// ❌ 违反铁律3
public class MyForm : Form, IMyView
{
    private MyPresenter _presenter;  // ❌ 视图依赖 Presenter

    public void SetPresenter(MyPresenter presenter)
    {
        _presenter = presenter;
    }
}
```

---

## ✅ 框架提供的正确服务

所有 UI 交互都应该通过服务接口：

| 功能 | 服务接口 | 说明 |
|------|---------|------|
| 系统对话框 | `IDialogProvider` | OpenFile, SaveFile, Color, Font 等 |
| 消息提示 | `IMessageService` | Info, Warning, Error, Confirm |
| 窗体导航 | `INavigator` | 打开 MVP 窗体 |
| 遗留窗体 | `ILegacyFormService` | 打开非 MVP 遗留窗体 |

---

## 📌 检查清单

在写代码前，务必检查：

- [ ] 视图接口中没有任何 `System.Windows.Forms` 类型？
- [ ] Presenter 方法签名中没有任何 UI 类型？
- [ ] 视图不持有 Presenter 的引用？
- [ ] 所有 UI 交互都通过服务接口？
- [ ] 使用 ViewAction 作为视图到 Presenter 的通信方式？

**记住：违反任何一条铁律，都不是真正的 MVP！**
