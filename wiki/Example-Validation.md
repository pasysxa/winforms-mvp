# Complex Validation Example

This example demonstrates comprehensive validation patterns in the WinForms MVP Framework, including real-time field-level validation, cross-field validation, pattern matching, and visual error feedback.

## 📁 Location

`src/WinformsMVP.Samples/ValidationDemo/`

## 🎯 What You'll Learn

- Real-time field-level validation as user types
- Cross-field validation (e.g., password confirmation)
- Pattern matching validation (email, phone number)
- Business rule validation (age restrictions)
- Visual error feedback with colored fields
- Validation summary display
- Preventing submission when validation fails
- Clean separation of validation logic from UI

## 🏗️ Architecture

### Files Structure

```
ValidationDemo/
├── IValidationDemoView.cs       # View interface
├── ValidationDemoPresenter.cs   # Validation logic
└── ValidationDemoForm.cs        # WinForms UI with error display
```

### Validation Types Demonstrated

| Validation Type | Example |
|----------------|---------|
| **Required** | All fields except Phone Number |
| **Length** | User name (3-50 chars), Password (min 8 chars) |
| **Pattern** | Email regex, Phone number format |
| **Character Set** | User name (alphanumeric + underscore only) |
| **Complexity** | Password must have uppercase, lowercase, digit, special char |
| **Cross-Field** | Password must match Confirm Password |
| **Range** | Age (18-120) |
| **Business Rule** | Must be 18+ to register |

## 📝 Implementation Walkthrough

### 1. View Interface - IValidationDemoView

The View interface exposes data properties and validation display methods:

```csharp
public interface IValidationDemoView : IWindowView
{
    // Form data
    string UserName { get; set; }
    string Email { get; set; }
    string Password { get; set; }
    string ConfirmPassword { get; set; }
    int Age { get; set; }
    string PhoneNumber { get; set; }

    // Validation state
    bool HasValidationErrors { get; }

    // Error display methods
    void SetFieldError(string fieldName, string errorMessage);
    void ClearFieldError(string fieldName);
    void ClearAllErrors();
    void ShowValidationSummary(IEnumerable<string> errors);

    // Change notification events
    event EventHandler UserNameChanged;
    event EventHandler EmailChanged;
    event EventHandler PasswordChanged;
    event EventHandler ConfirmPasswordChanged;
    event EventHandler AgeChanged;
    event EventHandler PhoneNumberChanged;
}
```

**Key Design Points:**
- ✅ Validation methods are part of the View contract (UI concern)
- ✅ Presenter controls WHEN and WHAT to validate
- ✅ View controls HOW to display errors (colors, labels, icons)
- ✅ Change events enable real-time validation

### 2. Presenter - ValidationDemoPresenter

The Presenter contains all validation logic:

```csharp
public class ValidationDemoPresenter : WindowPresenterBase<IValidationDemoView>
{
    private readonly Dictionary<string, List<string>> _fieldErrors =
        new Dictionary<string, List<string>>();

    protected override void OnViewAttached()
    {
        // Subscribe to field changes for real-time validation
        View.UserNameChanged += (s, e) => ValidateUserName();
        View.EmailChanged += (s, e) => ValidateEmail();
        View.PasswordChanged += (s, e) =>
        {
            ValidatePassword();
            ValidatePasswordConfirmation();  // Re-validate confirmation
        };
        View.ConfirmPasswordChanged += (s, e) => ValidatePasswordConfirmation();
        View.AgeChanged += (s, e) => ValidateAge();
        View.PhoneNumberChanged += (s, e) => ValidatePhoneNumber();
    }

    protected override void RegisterViewActions()
    {
        _dispatcher.Register(
            ValidationActions.Submit,
            OnSubmit,
            canExecute: () => !View.HasValidationErrors);  // Disable if errors

        _dispatcher.Register(ValidationActions.Reset, OnReset);
        _dispatcher.Register(ValidationActions.ValidateAll, OnValidateAll);

        View.ActionBinder.Bind(_dispatcher);
    }

    private void ValidateUserName()
    {
        var fieldName = "UserName";
        ClearFieldErrors(fieldName);

        var value = View.UserName;

        // Required validation
        if (string.IsNullOrWhiteSpace(value))
        {
            AddFieldError(fieldName, "User name is required.");
            UpdateFieldErrorDisplay(fieldName);
            return;
        }

        // Length validation
        if (value.Length < 3)
        {
            AddFieldError(fieldName, "User name must be at least 3 characters.");
        }

        if (value.Length > 50)
        {
            AddFieldError(fieldName, "User name must not exceed 50 characters.");
        }

        // Pattern validation
        if (!Regex.IsMatch(value, @"^[a-zA-Z0-9_]+$"))
        {
            AddFieldError(fieldName,
                "User name can only contain letters, numbers, and underscores.");
        }

        UpdateFieldErrorDisplay(fieldName);
    }

    private void ValidateEmail()
    {
        var fieldName = "Email";
        ClearFieldErrors(fieldName);

        var value = View.Email;

        if (string.IsNullOrWhiteSpace(value))
        {
            AddFieldError(fieldName, "Email is required.");
            UpdateFieldErrorDisplay(fieldName);
            return;
        }

        // Email pattern validation
        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(value, emailPattern))
        {
            AddFieldError(fieldName, "Email format is invalid.");
        }

        UpdateFieldErrorDisplay(fieldName);
    }

    private void ValidatePassword()
    {
        var fieldName = "Password";
        ClearFieldErrors(fieldName);

        var value = View.Password;

        if (string.IsNullOrWhiteSpace(value))
        {
            AddFieldError(fieldName, "Password is required.");
            UpdateFieldErrorDisplay(fieldName);
            return;
        }

        // Password complexity rules
        if (value.Length < 8)
        {
            AddFieldError(fieldName, "Password must be at least 8 characters.");
        }

        if (!Regex.IsMatch(value, @"[A-Z]"))
        {
            AddFieldError(fieldName,
                "Password must contain at least one uppercase letter.");
        }

        if (!Regex.IsMatch(value, @"[a-z]"))
        {
            AddFieldError(fieldName,
                "Password must contain at least one lowercase letter.");
        }

        if (!Regex.IsMatch(value, @"[0-9]"))
        {
            AddFieldError(fieldName,
                "Password must contain at least one digit.");
        }

        if (!Regex.IsMatch(value, @"[!@#$%^&*(),.?""':{}|<>]"))
        {
            AddFieldError(fieldName,
                "Password must contain at least one special character.");
        }

        UpdateFieldErrorDisplay(fieldName);
    }

    private void ValidatePasswordConfirmation()
    {
        var fieldName = "ConfirmPassword";
        ClearFieldErrors(fieldName);

        var password = View.Password;
        var confirmPassword = View.ConfirmPassword;

        if (string.IsNullOrWhiteSpace(confirmPassword))
        {
            AddFieldError(fieldName, "Password confirmation is required.");
            UpdateFieldErrorDisplay(fieldName);
            return;
        }

        // Cross-field validation
        if (password != confirmPassword)
        {
            AddFieldError(fieldName, "Passwords do not match.");
        }

        UpdateFieldErrorDisplay(fieldName);
    }

    private void ValidateAge()
    {
        var fieldName = "Age";
        ClearFieldErrors(fieldName);

        var value = View.Age;

        if (value <= 0)
        {
            AddFieldError(fieldName, "Age is required.");
            UpdateFieldErrorDisplay(fieldName);
            return;
        }

        // Business rule: Must be 18+
        if (value < 18)
        {
            AddFieldError(fieldName, "You must be at least 18 years old.");
        }

        if (value > 120)
        {
            AddFieldError(fieldName, "Age must not exceed 120 years.");
        }

        UpdateFieldErrorDisplay(fieldName);
    }

    private void UpdateFieldErrorDisplay(string fieldName)
    {
        if (_fieldErrors.ContainsKey(fieldName) && _fieldErrors[fieldName].Any())
        {
            var errorMessage = string.Join("\n", _fieldErrors[fieldName]);
            View.SetFieldError(fieldName, errorMessage);
        }
        else
        {
            View.ClearFieldError(fieldName);
        }

        // Update Submit button state
        _dispatcher.RaiseCanExecuteChanged();
    }
}
```

### 3. Form Implementation - Visual Error Feedback

The Form provides visual feedback for validation errors:

```csharp
public partial class ValidationDemoForm : Form, IValidationDemoView
{
    private readonly Dictionary<string, Label> _errorLabels =
        new Dictionary<string, Label>();
    private readonly Dictionary<string, Control> _fieldControls =
        new Dictionary<string, Control>();

    public bool HasValidationErrors =>
        _errorLabels.Values.Any(label => !string.IsNullOrWhiteSpace(label.Text));

    public void SetFieldError(string fieldName, string errorMessage)
    {
        // Show error message in red label below field
        if (_errorLabels.TryGetValue(fieldName, out var errorLabel))
        {
            errorLabel.Text = errorMessage;
        }

        // Change field background to light red
        if (_fieldControls.TryGetValue(fieldName, out var control))
        {
            control.BackColor = Color.FromArgb(255, 240, 240);  // Light red
        }
    }

    public void ClearFieldError(string fieldName)
    {
        // Clear error message
        if (_errorLabels.TryGetValue(fieldName, out var errorLabel))
        {
            errorLabel.Text = string.Empty;
        }

        // Restore field background to white
        if (_fieldControls.TryGetValue(fieldName, out var control))
        {
            control.BackColor = Color.White;
        }
    }

    public void ShowValidationSummary(IEnumerable<string> errors)
    {
        _validationSummaryTextBox.Text = string.Join(Environment.NewLine, errors);
    }
}
```

**Visual Feedback Features:**
- ❌ Fields with errors have light red background
- ❌ Error messages appear in red label below each field
- ✅ Fields with valid data have white background
- 📋 Validation summary shows all errors at once

## 🎮 User Experience Flow

1. **User starts typing in User Name field**
   - `UserNameChanged` event fires on each keystroke
   - Presenter validates immediately
   - Field turns red if invalid, white if valid
   - Error message appears/disappears in real-time

2. **User enters password**
   - Password complexity is validated in real-time
   - Each rule shows separate error message
   - User sees exactly what's missing

3. **User enters confirmation password**
   - Cross-field validation compares with password
   - If mismatch, error appears immediately

4. **User changes password after confirming**
   - Both Password and ConfirmPassword re-validate
   - Prevents user from forgetting to update confirmation

5. **User clicks Submit**
   - Final validation runs on all fields
   - If any errors, button is disabled (CanExecute = false)
   - Validation summary shows all problems

## 🧪 Testing Example

```csharp
[Fact]
public void ValidateEmail_WithInvalidFormat_ShouldShowError()
{
    // Arrange
    var mockView = new MockValidationView();
    var presenter = new ValidationDemoPresenter();
    presenter.AttachView(mockView);
    presenter.Initialize();

    // Act
    mockView.Email = "invalid-email";  // Missing @ and domain
    mockView.RaiseEmailChanged();

    // Assert
    Assert.True(mockView.HasFieldError("Email"));
    Assert.Contains("invalid", mockView.GetFieldError("Email"));
}

[Fact]
public void ValidatePassword_WithWeakPassword_ShouldShowMultipleErrors()
{
    // Arrange
    var mockView = new MockValidationView();
    var presenter = new ValidationDemoPresenter();
    presenter.AttachView(mockView);
    presenter.Initialize();

    // Act
    mockView.Password = "weak";  // Too short, no uppercase, no digit, no special
    mockView.RaisePasswordChanged();

    // Assert
    var errors = mockView.GetFieldErrors("Password");
    Assert.Contains(e => e.Contains("8 characters"), errors);
    Assert.Contains(e => e.Contains("uppercase"), errors);
    Assert.Contains(e => e.Contains("digit"), errors);
    Assert.Contains(e => e.Contains("special"), errors);
}

[Fact]
public void ValidatePasswordConfirmation_WhenPasswordChanges_ShouldRevalidate()
{
    // Arrange
    var mockView = new MockValidationView();
    var presenter = new ValidationDemoPresenter();
    presenter.AttachView(mockView);
    presenter.Initialize();

    mockView.Password = "Password123!";
    mockView.ConfirmPassword = "Password123!";
    mockView.RaisePasswordChanged();
    mockView.RaiseConfirmPasswordChanged();
    Assert.False(mockView.HasFieldError("ConfirmPassword"));  // Initially valid

    // Act - Change password
    mockView.Password = "NewPassword123!";
    mockView.RaisePasswordChanged();  // This should trigger confirmation re-validation

    // Assert
    Assert.True(mockView.HasFieldError("ConfirmPassword"));  // Now invalid!
}
```

## 💡 Advanced Patterns

### Asynchronous Validation

For server-side validation (e.g., checking if username already exists):

```csharp
private async void ValidateUserName()
{
    var fieldName = "UserName";
    ClearFieldErrors(fieldName);

    var value = View.UserName;

    // ... local validation first ...

    // Async server-side validation
    if (!string.IsNullOrWhiteSpace(value))
    {
        View.ShowFieldValidating(fieldName, true);  // Show spinner

        try
        {
            bool isAvailable = await _userService.IsUserNameAvailableAsync(value);
            if (!isAvailable)
            {
                AddFieldError(fieldName, "User name is already taken.");
            }
        }
        catch (Exception ex)
        {
            AddFieldError(fieldName, "Could not validate user name. Please try again.");
        }
        finally
        {
            View.ShowFieldValidating(fieldName, false);  // Hide spinner
        }
    }

    UpdateFieldErrorDisplay(fieldName);
}
```

### Custom Validation Rules

Encapsulate validation logic in reusable rules:

```csharp
public interface IValidationRule<T>
{
    string ErrorMessage { get; }
    bool IsValid(T value);
}

public class EmailFormatRule : IValidationRule<string>
{
    public string ErrorMessage => "Email format is invalid.";

    public bool IsValid(string value)
    {
        return Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}

// Usage in Presenter
private void ValidateEmail()
{
    var rules = new List<IValidationRule<string>>
    {
        new RequiredRule(),
        new EmailFormatRule(),
        new MaxLengthRule(100)
    };

    foreach (var rule in rules)
    {
        if (!rule.IsValid(View.Email))
        {
            AddFieldError("Email", rule.ErrorMessage);
        }
    }

    UpdateFieldErrorDisplay("Email");
}
```

### Validation with Data Annotations

Leverage .NET Data Annotations:

```csharp
public class RegistrationModel
{
    [Required(ErrorMessage = "User name is required")]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9_]+$",
        ErrorMessage = "Only letters, numbers, and underscores allowed")]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]",
        ErrorMessage = "Password must contain uppercase, lowercase, digit, and special character")]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }

    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int Age { get; set; }

    [Phone]
    public string PhoneNumber { get; set; }
}

// Presenter validates using Validator class
private void ValidateModel()
{
    var model = new RegistrationModel
    {
        UserName = View.UserName,
        Email = View.Email,
        Password = View.Password,
        ConfirmPassword = View.ConfirmPassword,
        Age = View.Age,
        PhoneNumber = View.PhoneNumber
    };

    var context = new ValidationContext(model);
    var results = new List<ValidationResult>();

    if (!Validator.TryValidateObject(model, context, results, validateAllProperties: true))
    {
        foreach (var result in results)
        {
            foreach (var memberName in result.MemberNames)
            {
                AddFieldError(memberName, result.ErrorMessage);
                UpdateFieldErrorDisplay(memberName);
            }
        }
    }
}
```

## 🔗 Related Examples

- [Master-Detail Demo](Example-Master-Detail) - Add validation to master-detail forms
- [Async Operations](Example-Async-Operations) - Server-side async validation
- [ToDo CRUD Demo](Example-ToDo) - Basic validation patterns

## 📚 Further Reading

- [Best Practices: Error Handling](Best-Practices-Error-Handling)
- [Service Layer](Service-Layer#imessageservice) - Showing validation errors
- [ViewAction System](ViewAction-System) - CanExecute for submit button

---

**[⬆ Back to Examples](Home#example-applications)**
