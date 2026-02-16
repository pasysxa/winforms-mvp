# ValidationHelper API 使用示例

## API 设计理念

`ValidationHelper` 提供了灵活的静态方法，支持**可选的 UI 反馈**：

- ✅ **传递 IMessageService**：验证失败时自动显示错误
- ✅ **不传递 IMessageService**：静默验证（不显示 UI）
- ✅ **完全可测试**：所有依赖通过参数注入

## 核心方法

### 1. ValidateSequential - 顺序验证

```csharp
public static bool ValidateSequential<T>(
    T model,
    IModelValidator validator,
    IMessageService messageService = null,  // 可选
    string errorTitle = "Validation Failed") where T : class
```

**特点**：
- 按 `ValidationOrder` 顺序验证
- 遇到第一个错误立即停止
- 性能最优（通常只验证几个属性）
- 推荐用于表单提交场景

### 2. ValidateAll - 全部验证

```csharp
public static bool ValidateAll<T>(
    T model,
    IModelValidator validator,
    IMessageService messageService = null,  // 可选
    string errorTitle = "Validation Failed") where T : class
```

**特点**：
- 验证所有属性
- 收集所有错误
- 推荐用于显示验证摘要场景

## 使用场景

### 场景 1: 表单提交（带 UI 反馈）

```csharp
public class RegisterUserPresenter : WindowPresenterBase<IRegisterUserView>
{
    private readonly IModelValidator _validator = ModelValidator.For<UserModel>();

    private void OnRegister()
    {
        var model = CreateModelFromView();

        // ✅ 推荐：验证并显示第一个错误
        if (!ValidationHelper.ValidateSequential(model, _validator, Messages))
            return;

        // 验证通过，继续注册
        _userRepository.RegisterUser(model);
        Messages.ShowInfo("注册成功！");
    }
}
```

### 场景 2: CanExecute 判断（无 UI 反馈）

```csharp
public class EditProfilePresenter : WindowPresenterBase<IEditProfileView>
{
    private readonly IModelValidator _validator = ModelValidator.For<ProfileModel>();

    protected override void RegisterViewActions()
    {
        _dispatcher.Register(
            CommonActions.Save,
            OnSave,
            canExecute: () =>
            {
                var model = GetCurrentModel();

                // ✅ 只验证，不显示 UI（messageService = null）
                return ValidationHelper.ValidateSequential(model, _validator);
            });

        View.ActionBinder.Bind(_dispatcher);
    }

    private void OnSave()
    {
        var model = GetCurrentModel();

        // 再次验证并显示错误（保险起见）
        if (!ValidationHelper.ValidateSequential(model, _validator, Messages))
            return;

        SaveProfile(model);
    }
}
```

### 场景 3: 显示所有验证错误

```csharp
private void OnSave()
{
    var model = CreateModelFromView();

    // ✅ 显示所有错误（用户一次看到所有问题）
    if (!ValidationHelper.ValidateAll(model, _validator, Messages, "输入错误"))
        return;

    SaveModel(model);
}
```

### 场景 4: 静默验证 + 自定义错误处理

```csharp
private void OnSave()
{
    var model = CreateModelFromView();

    // ✅ 静默验证（不显示 UI）
    if (!ValidationHelper.ValidateSequential(model, _validator))
    {
        // 自定义错误处理（例如：记录日志）
        Logger.Warn("User attempted to save invalid data");

        // 然后再验证一次并显示（或自定义 UI）
        ValidationHelper.ValidateSequential(model, _validator, Messages, "请检查输入");
        return;
    }

    SaveModel(model);
}
```

### 场景 5: 实时验证（输入时）

```csharp
public class RegisterUserPresenter : WindowPresenterBase<IRegisterUserView>
{
    private readonly IModelValidator _validator = ModelValidator.For<UserModel>();

    protected override void OnViewAttached()
    {
        // 订阅输入变化事件
        View.InputChanged += OnInputChanged;
    }

    private void OnInputChanged(object sender, EventArgs e)
    {
        var model = CreateModelFromView();

        // ✅ 实时验证，不显示 MessageBox（避免干扰用户输入）
        bool isValid = ValidationHelper.ValidateSequential(model, _validator);

        // 更新 UI 状态（例如：保存按钮启用/禁用）
        _dispatcher.RaiseCanExecuteChanged();

        // 或者在 View 中显示实时错误提示（Label、Tooltip 等）
        if (!isValid)
        {
            var error = _validator.ValidateSequential(model);
            View.ShowInlineError(error.GetFormattedMessage());
        }
        else
        {
            View.ClearInlineError();
        }
    }

    protected override void RegisterViewActions()
    {
        _dispatcher.Register(
            CommonActions.Save,
            OnSave,
            canExecute: () =>
            {
                var model = CreateModelFromView();
                return ValidationHelper.ValidateSequential(model, _validator);
            });

        View.ActionBinder.Bind(_dispatcher);
    }

    private void OnSave()
    {
        var model = CreateModelFromView();

        // 提交时显示错误（以防万一）
        if (!ValidationHelper.ValidateSequential(model, _validator, Messages))
            return;

        SaveUser(model);
    }
}
```

### 场景 6: 批量验证

```csharp
private void OnSaveBatch()
{
    var models = GetAllModelsFromView();
    var invalidModels = new List<string>();

    foreach (var model in models)
    {
        // ✅ 静默验证每个 model
        if (!ValidationHelper.ValidateSequential(model, _validator))
        {
            invalidModels.Add($"第 {model.Index} 行");
        }
    }

    if (invalidModels.Any())
    {
        var errorMessage = $"以下数据验证失败：\n{string.Join("\n", invalidModels)}";
        Messages.ShowWarning(errorMessage, "批量验证失败");
        return;
    }

    // 全部验证通过
    SaveBatch(models);
}
```

## 测试示例

### 测试 - 验证失败时不显示 UI

```csharp
[Fact]
public void ValidateSequential_WithInvalidModel_NoMessageService_ReturnsFalse()
{
    // Arrange
    var mockValidator = new Mock<IModelValidator>();
    mockValidator.Setup(v => v.ValidateSequential(It.IsAny<object>()))
        .Returns(new ValidationResult("错误", new[] { "Name" }, 1));

    var model = new UserModel { Name = "" };

    // Act
    var result = ValidationHelper.ValidateSequential(
        model,
        mockValidator.Object);  // ✅ 不传 messageService

    // Assert
    Assert.False(result);
    // ✅ 无需 mock messageService，因为根本不会调用
}
```

### 测试 - 验证失败时显示 UI

```csharp
[Fact]
public void ValidateSequential_WithInvalidModel_WithMessageService_ShowsWarning()
{
    // Arrange
    var mockValidator = new Mock<IModelValidator>();
    mockValidator.Setup(v => v.ValidateSequential(It.IsAny<object>()))
        .Returns(new ValidationResult("用户名必填", new[] { "UserName" }, 1));

    var mockMessageService = new Mock<IMessageService>();
    var model = new UserModel { UserName = "" };

    // Act
    var result = ValidationHelper.ValidateSequential(
        model,
        mockValidator.Object,
        mockMessageService.Object,  // ✅ 传 messageService
        "验证失败");

    // Assert
    Assert.False(result);

    // ✅ 验证 ShowWarning 被调用
    mockMessageService.Verify(
        m => m.ShowWarning(
            It.Is<string>(s => s.Contains("用户名必填")),
            "验证失败"),
        Times.Once);
}
```

### 测试 - 验证成功时不显示 UI

```csharp
[Fact]
public void ValidateSequential_WithValidModel_WithMessageService_NoWarning()
{
    // Arrange
    var mockValidator = new Mock<IModelValidator>();
    mockValidator.Setup(v => v.ValidateSequential(It.IsAny<object>()))
        .Returns(ValidationResult.Success);

    var mockMessageService = new Mock<IMessageService>();
    var model = new UserModel { UserName = "test" };

    // Act
    var result = ValidationHelper.ValidateSequential(
        model,
        mockValidator.Object,
        mockMessageService.Object);

    // Assert
    Assert.True(result);

    // ✅ 验证 ShowWarning 未被调用
    mockMessageService.Verify(
        m => m.ShowWarning(It.IsAny<string>(), It.IsAny<string>()),
        Times.Never);
}
```

## API 对比

### 旧 API（已废弃）

```csharp
// ❌ 旧 API：强制需要 messageService
if (!ValidationHelper.ValidateSequentialAndShow(model, validator, Messages))
    return;

// 如果不想显示 UI，需要直接调用 validator（繁琐）
var error = validator.ValidateSequential(model);
if (!error.IsValid)
    return;
```

### 新 API（推荐）

```csharp
// ✅ 新 API：灵活控制是否显示 UI

// 方式 1：只验证
if (!ValidationHelper.ValidateSequential(model, validator))
    return;

// 方式 2：验证并显示
if (!ValidationHelper.ValidateSequential(model, validator, Messages))
    return;

// 方式 3：验证并显示（自定义标题）
if (!ValidationHelper.ValidateSequential(model, validator, Messages, "输入错误"))
    return;
```

## 推荐模式

### 模式 1：表单提交（推荐）

```csharp
private void OnSave()
{
    var model = CreateModelFromView();

    // ✅ 简洁：一行代码完成验证 + 显示错误
    if (!ValidationHelper.ValidateSequential(model, _validator, Messages))
        return;

    SaveModel(model);
    Messages.ShowInfo("保存成功！");
}
```

### 模式 2：CanExecute 判断（推荐）

```csharp
protected override void RegisterViewActions()
{
    _dispatcher.Register(
        CommonActions.Save,
        OnSave,
        canExecute: () =>
        {
            var model = GetCurrentModel();

            // ✅ 静默验证（不显示 UI，避免频繁弹窗）
            return ValidationHelper.ValidateSequential(model, _validator);
        });

    View.ActionBinder.Bind(_dispatcher);
}
```

### 模式 3：与 ChangeTracker 集成

```csharp
private ChangeTracker<UserModel> _changeTracker;
private readonly IModelValidator _validator = ModelValidator.For<UserModel>();

protected override void RegisterViewActions()
{
    _dispatcher.Register(
        CommonActions.Save,
        OnSave,
        canExecute: () =>
        {
            // ✅ 结合 ChangeTracker 和静默验证
            return _changeTracker.IsChanged &&
                   ValidationHelper.ValidateSequential(
                       _changeTracker.CurrentValue,
                       _validator);
        });

    View.ActionBinder.Bind(_dispatcher);
}

private void OnSave()
{
    // ✅ 提交时验证并显示错误
    if (!ValidationHelper.ValidateSequential(
        _changeTracker.CurrentValue,
        _validator,
        Messages))
        return;

    _changeTracker.AcceptChanges();
    SaveModel(_changeTracker.CurrentValue);
}
```

## 总结

新的 `ValidationHelper` API 设计：

✅ **优点**：
- 灵活：可选的 UI 反馈（传 messageService 或不传）
- 简洁：一个方法支持两种场景
- 可测试：所有依赖通过参数注入
- 向后兼容：旧代码只需删除 `AndShow` 后缀

✅ **推荐用法**：
- **表单提交**：`ValidationHelper.ValidateSequential(model, validator, Messages)`
- **CanExecute**：`ValidationHelper.ValidateSequential(model, validator)`（无 UI）
- **批量验证**：循环中静默验证，最后统一显示错误

✅ **命名改进**：
- ❌ 旧名：`ValidateSequentialAndShow`（强制显示 UI）
- ✅ 新名：`ValidateSequential`（可选显示 UI）
