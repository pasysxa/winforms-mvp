# ValidationOrderAttribute 使用指南

## 概述

`ValidationOrderAttribute` 是一个属性级别的特性，用于控制验证顺序。与传统的为每个验证特性包装 Order 属性不同，该设计更简洁：

- ✅ 直接使用 .NET 标准 `ValidationAttribute`（无需包装）
- ✅ 属性级别指定 Order（同一属性的多个验证特性按声明顺序执行）
- ✅ 代码更简洁、更直观

## 基本用法

### 示例 1: 用户注册模型

```csharp
using System.ComponentModel.DataAnnotations;
using WinformsMVP.Common.Validation.Attributes;

public class RegisterUserModel : ICloneable
{
    [ValidationOrder(1)]
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在 3-50 之间")]
    [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "用户名只能包含字母、数字和下划线")]
    public string UserName { get; set; }

    [ValidationOrder(2)]
    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string Email { get; set; }

    [ValidationOrder(3)]
    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "密码至少 8 个字符")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "密码必须包含大小写字母和数字")]
    public string Password { get; set; }

    [ValidationOrder(4)]
    [Required(ErrorMessage = "确认密码不能为空")]
    [Compare(nameof(Password), ErrorMessage = "两次输入的密码不一致")]
    public string ConfirmPassword { get; set; }

    public object Clone()
    {
        return new RegisterUserModel
        {
            UserName = this.UserName,
            Email = this.Email,
            Password = this.Password,
            ConfirmPassword = this.ConfirmPassword
        };
    }
}
```

### 验证顺序

当使用 `ModelValidator.For<RegisterUserModel>().ValidateSequential(model)` 时：

```
1. UserName 属性 (Order = 1)
   1.1 Required          ← 声明顺序第一个
   1.2 StringLength      ← 声明顺序第二个
   1.3 RegularExpression ← 声明顺序第三个

2. Email 属性 (Order = 2)
   2.1 Required
   2.2 EmailAddress

3. Password 属性 (Order = 3)
   3.1 Required
   3.2 StringLength
   3.3 RegularExpression

4. ConfirmPassword 属性 (Order = 4)
   4.1 Required
   4.2 Compare
```

**重要特性**：
- ✅ 如果 UserName.Required 失败，**立即返回**，不会继续验证 StringLength 或后续属性
- ✅ 只有 UserName 所有验证通过后，才会验证 Email
- ✅ 防止级联错误（例如：不会在 UserName 为空时报告"用户名格式不正确"）

## 在 Presenter 中使用

### 示例 2: 注册用户 Presenter

```csharp
using WinformsMVP.Common.Validation.Core;
using WinformsMVP.MVP.Presenters;

public class RegisterUserPresenter : WindowPresenterBase<IRegisterUserView>
{
    private readonly IModelValidator _validator;
    private readonly IUserRepository _userRepository;

    public RegisterUserPresenter(IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _validator = ModelValidator.For<RegisterUserModel>();
    }

    protected override void RegisterViewActions()
    {
        _dispatcher.Register(
            CommonActions.Save,
            OnRegister,
            canExecute: () => _validator.IsValid(CreateModelFromView()));

        _dispatcher.Register(CommonActions.Cancel, OnCancel);

        View.ActionBinder.Bind(_dispatcher);
    }

    private void OnRegister()
    {
        var model = CreateModelFromView();

        // 方案 1: 顺序验证（显示第一个错误）
        var error = _validator.ValidateSequential(model);
        if (!error.IsValid)
        {
            Messages.ShowWarning(error.GetFormattedMessage(), "验证失败");
            return;
        }

        // 方案 2: 全部验证（显示所有错误）
        // var errors = _validator.ValidateAll(model);
        // if (errors.Any())
        // {
        //     var errorMessage = string.Join("\n", errors.Select(e => e.GetFormattedMessage()));
        //     Messages.ShowWarning(errorMessage, "验证失败");
        //     return;
        // }

        // 验证通过，注册用户
        _userRepository.RegisterUser(model);
        Messages.ShowInfo("注册成功！", "成功");
        RequestClose(new UserRegistrationResult { Success = true });
    }

    private RegisterUserModel CreateModelFromView()
    {
        return new RegisterUserModel
        {
            UserName = View.UserName,
            Email = View.Email,
            Password = View.Password,
            ConfirmPassword = View.ConfirmPassword
        };
    }
}
```

## 与 ChangeTracker 集成

### 示例 3: 编辑用户资料

```csharp
using WinformsMVP.Common;
using WinformsMVP.Common.Validation.Core;
using WinformsMVP.Common.Validation.Extensions;

public class EditProfilePresenter : WindowPresenterBase<IEditProfileView>
{
    private ChangeTracker<UserProfileModel> _changeTracker;
    private readonly IModelValidator _validator;

    public EditProfilePresenter()
    {
        _validator = ModelValidator.For<UserProfileModel>();
    }

    protected override void OnInitialize()
    {
        var profile = LoadUserProfile();
        _changeTracker = new ChangeTracker<UserProfileModel>(profile);

        View.Model = _changeTracker.CurrentValue;
    }

    protected override void RegisterViewActions()
    {
        _dispatcher.Register(
            CommonActions.Save,
            OnSave,
            canExecute: () => _changeTracker.IsChanged &&
                              _changeTracker.IsCurrentValueValid(_validator));

        _dispatcher.Register(
            CommonActions.Reset,
            OnReset,
            canExecute: () => _changeTracker.IsChanged);

        View.ActionBinder.Bind(_dispatcher);
    }

    private void OnSave()
    {
        // 只有验证通过才接受更改
        if (!_changeTracker.AcceptChangesIfValid(_validator, out var errors))
        {
            var errorMessage = string.Join("\n", errors.Select(e => e.GetFormattedMessage()));
            Messages.ShowWarning(errorMessage, "验证失败");
            return;
        }

        SaveUserProfile(_changeTracker.CurrentValue);
        Messages.ShowInfo("资料保存成功！", "成功");
    }

    private void OnReset()
    {
        _changeTracker.RejectChanges();
        View.Model = _changeTracker.CurrentValue;
    }
}
```

## 默认 Order 行为

如果属性没有 `[ValidationOrder]` 特性，其 Order 默认为 `0`：

```csharp
public class SimpleModel
{
    // Order = 0（默认）
    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    // Order = 0（默认）
    [EmailAddress]
    public string Email { get; set; }

    [ValidationOrder(1)]
    [Required]
    public string Description { get; set; }
}
```

验证顺序：
1. Name 和 Email（Order = 0，顺序不确定）
2. Description（Order = 1）

**建议**：为了明确控制顺序，建议所有需要验证的属性都显式指定 `[ValidationOrder]`。

## 跨属性验证

### 示例 4: Compare 验证

```csharp
public class ChangePasswordModel : ICloneable
{
    [ValidationOrder(1)]
    [Required(ErrorMessage = "当前密码不能为空")]
    public string CurrentPassword { get; set; }

    [ValidationOrder(2)]
    [Required(ErrorMessage = "新密码不能为空")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "新密码至少 8 个字符")]
    public string NewPassword { get; set; }

    [ValidationOrder(3)]
    [Required(ErrorMessage = "确认新密码不能为空")]
    [Compare(nameof(NewPassword), ErrorMessage = "两次输入的新密码不一致")]
    public string ConfirmNewPassword { get; set; }

    public object Clone()
    {
        return new ChangePasswordModel
        {
            CurrentPassword = this.CurrentPassword,
            NewPassword = this.NewPassword,
            ConfirmNewPassword = this.ConfirmNewPassword
        };
    }
}
```

**设计原则**：
- `Compare` 验证应该放在 **被比较属性之后**
- 确保先验证 `NewPassword` 是否有效，再验证 `ConfirmNewPassword` 是否匹配

## 自定义验证特性

你可以创建自定义的 `ValidationAttribute`，它会自动集成到 Order 系统中：

```csharp
public class UniqueUserNameAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var userName = value as string;
        if (string.IsNullOrEmpty(userName))
            return ValidationResult.Success;  // Required 已经验证过了

        // 检查用户名唯一性（示例）
        var userRepository = validationContext.GetService(typeof(IUserRepository)) as IUserRepository;
        if (userRepository?.UserNameExists(userName) == true)
        {
            return new ValidationResult("用户名已被占用");
        }

        return ValidationResult.Success;
    }
}

// 使用
public class RegisterUserModel
{
    [ValidationOrder(1)]
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [UniqueUserName]  // 自定义验证，按声明顺序执行（第三个）
    public string UserName { get; set; }
}
```

## 性能优化

### ValidateSequential vs ValidateAll

```csharp
// 顺序验证 - 停在第一个错误（推荐用于表单提交）
var error = _validator.ValidateSequential(model);
if (!error.IsValid)
{
    Messages.ShowWarning(error.GetFormattedMessage());
    return;
}

// 全部验证 - 收集所有错误（推荐用于显示验证摘要）
var errors = _validator.ValidateAll(model);
if (errors.Any())
{
    var summary = string.Join("\n", errors.Select(e => e.GetFormattedMessage()));
    Messages.ShowWarning(summary);
    return;
}
```

**性能对比**：
- `ValidateSequential`：O(N)，遇到第一个错误即停止
- `ValidateAll`：O(N)，验证所有属性

两者性能接近，但 `ValidateSequential` 通常更快（平均情况下验证更少的属性）。

## 最佳实践

1. **明确指定 Order**
   ```csharp
   // ✅ 推荐
   [ValidationOrder(1)]
   [Required]
   public string Name { get; set; }

   // ❌ 不推荐（依赖默认 Order = 0）
   [Required]
   public string Name { get; set; }
   ```

2. **Required 放在第一位**
   ```csharp
   [ValidationOrder(1)]
   [Required]           // ✅ 先验证是否有值
   [StringLength(50)]   // 然后验证长度
   [EmailAddress]       // 最后验证格式
   public string Email { get; set; }
   ```

3. **跨属性验证放在最后**
   ```csharp
   [ValidationOrder(1)]
   [Required]
   public string Password { get; set; }

   [ValidationOrder(2)]  // ✅ 确保 Password 先验证
   [Required]
   [Compare(nameof(Password))]
   public string ConfirmPassword { get; set; }
   ```

4. **使用 ValidationHelper 简化代码**
   ```csharp
   using WinformsMVP.Common.Validation.Helpers;

   private void OnSave()
   {
       var model = CreateModelFromView();

       // ✅ 一行代码完成验证 + 显示错误
       if (!ValidationHelper.ValidateSequential(model, _validator, Messages))
           return;

       // 验证通过，继续保存
       SaveModel(model);
   }
   ```

   **不显示 UI 的验证（例如 CanExecute）：**
   ```csharp
   protected override void RegisterViewActions()
   {
       _dispatcher.Register(
           CommonActions.Save,
           OnSave,
           canExecute: () =>
           {
               var model = GetCurrentModel();
               // ✅ 静默验证（不传 messageService）
               return ValidationHelper.ValidateSequential(model, _validator);
           });
   }
   ```

## 总结

`ValidationOrderAttribute` 提供了一个简洁、强大的属性级别验证排序机制：

- ✅ **简洁**：直接使用标准 ValidationAttribute，无需包装类
- ✅ **直观**：属性按 Order 排序，属性内特性按声明顺序
- ✅ **性能**：静态缓存，ValidationContext 重用，O(N) 复杂度
- ✅ **灵活**：支持标准特性、自定义特性、IValidatableObject
- ✅ **可测试**：非泛型接口，易于 mock

从繁琐的包装类设计迁移到属性级别 Order，让验证代码更清晰、更易维护！
