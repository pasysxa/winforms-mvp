# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a WinForms MVP (Model-View-Presenter) framework for .NET Framework 4.8, providing infrastructure for building desktop applications with clean separation of concerns. All projects use SDK-style .csproj format for better maintainability.

## Build and Test Commands

### Building the Solution
```bash
# Build entire solution
dotnet build src/winforms-mvp.sln

# Build specific configuration
dotnet build src/winforms-mvp.sln -c Debug
dotnet build src/winforms-mvp.sln -c Release

# Build specific project
dotnet build src/WinformsMVP/WinformsMVP.csproj
```

### Running Tests
```bash
# Run all tests using xUnit
dotnet test src/WindowsMVP.Samples.Tests/WindowsMVP.Samples.Tests.csproj

# Run tests with detailed output
dotnet test src/WindowsMVP.Samples.Tests/WindowsMVP.Samples.Tests.csproj -v detailed
```

### Running the Sample Application
```bash
# Build and run the sample WinForms application
dotnet run --project src/MinformsMVP.Samples/MinformsMVP.Samples.csproj
```

### Restore NuGet Packages
```bash
# Restore packages for the entire solution
dotnet restore src/winforms-mvp.sln
```

## Architecture

### Core MVP Pattern

The framework implements a passive MVP pattern with clear separation between Form and UserControl scenarios.

**CRITICAL MVP Principle**: Presenters must NEVER directly reference UI elements or WinForms APIs (MessageBox, DialogResult, etc.). All UI interactions must go through:
- **View interfaces** for displaying data and receiving user input
- **Service interfaces** (IMessageService, IDialogProvider, etc.) for dialogs and notifications

This ensures:
- Presenters remain testable without requiring a UI thread
- Business logic is completely decoupled from WinForms implementation
- Code can be easily unit tested and reused across different UI frameworks

**Bad Example (Violates MVP):**
```csharp
private void OnSave()
{
    SaveData();
    MessageBox.Show("Data saved!");  // ❌ WRONG - Direct UI dependency
}
```

**Good Example (Follows MVP):**
```csharp
private readonly IMessageService _messageService;

public MyPresenter(IMessageService messageService)
{
    _messageService = messageService;  // ✅ Dependency injection
}

private void OnSave()
{
    SaveData();
    _messageService.ShowInfo("Data saved!");  // ✅ CORRECT - Service abstraction
}
```

### Presenter Hierarchy

```
PresenterBase<TView>                              [Base class - do not use directly]
│
├─ WindowPresenterBase<TView>                     [Forms - no parameters]
├─ WindowPresenterBase<TView, TParam>             [Forms - with parameters]
├─ ControlPresenterBase<TView>                    [UserControls - no parameters]
└─ ControlPresenterBase<TView, TParam>            [UserControls - with parameters]
```

#### 1. **WindowPresenterBase<TView>** - For Forms (Dialogs, Windows)

Use this for presenters that manage Form views via WindowNavigator without initialization parameters.

```csharp
public class AboutDialogPresenter : WindowPresenterBase<IAboutDialogView>
{
    protected override void OnViewAttached()
    {
        // Called when view is attached (before initialization)
    }

    protected override void OnInitialize()
    {
        // Called after view is attached, before window is shown
        View.Version = GetVersion();
    }

    protected override void RegisterViewActions()
    {
        Dispatcher.Register(AboutActions.Close, OnClose);
    }
}

// Usage
var presenter = new AboutDialogPresenter();
navigator.ShowWindowAsModal(presenter);
```

#### 2. **WindowPresenterBase<TView, TParam>** - For Parameterized Forms

Use this when the presenter requires initialization parameters (e.g., entity ID, edit mode).

```csharp
public class EditUserParameters
{
    public int UserId { get; set; }
    public bool IsReadOnly { get; set; }
}

public class EditUserPresenter : WindowPresenterBase<IEditUserView, EditUserParameters>,
                                  IRequestClose<UserResult>
{
    protected override void OnInitialize(EditUserParameters parameters)
    {
        // Both View and Parameters are available here
        var user = LoadUser(parameters.UserId);
        View.UserName = user.Name;
        View.Email = user.Email;
        View.IsReadOnly = parameters.IsReadOnly;
    }

    // IRequestClose implementation...
}

// Usage
var presenter = new EditUserPresenter();
var parameters = new EditUserParameters { UserId = 123, IsReadOnly = false };
var result = navigator.ShowWindowAsModal<EditUserPresenter, EditUserParameters, UserResult>(
    presenter, parameters);
```

#### 3. **ControlPresenterBase<TView>** - For UserControls

Use this for presenters that manage UserControl views (embedded controls, panels).

```csharp
public class SettingsPanelPresenter : ControlPresenterBase<ISettingsPanelView>
{
    public SettingsPanelPresenter(ISettingsPanelView view) : base(view)
    {
        // View is automatically attached and initialized
    }

    protected override void OnInitialize()
    {
        LoadSettings();
    }

    protected override void OnControlLoad(object sender, EventArgs e)
    {
        // Called when UserControl is loaded (has parent and handle)
    }

    protected override void RegisterViewActions()
    {
        Dispatcher.Register(SettingsActions.Save, OnSave);
    }
}

// Usage (in parent Form)
var presenter = new SettingsPanelPresenter(settingsPanelControl1);
// Presenter is automatically initialized and disposed with the control
```

#### 4. **ControlPresenterBase<TView, TParam>** - For Parameterized UserControls

Use this when UserControl presenters require initialization parameters.

```csharp
public class SearchParameters
{
    public string DefaultKeyword { get; set; }
    public DateTime? DateFrom { get; set; }
}

public class SearchPanelPresenter : ControlPresenterBase<ISearchPanelView, SearchParameters>
{
    public SearchPanelPresenter(ISearchPanelView view, SearchParameters parameters)
        : base(view, parameters)
    {
    }

    protected override void OnInitialize(SearchParameters parameters)
    {
        View.Keyword = parameters.DefaultKeyword;
        View.DateFrom = parameters.DateFrom;
    }
}

// Usage
var parameters = new SearchParameters { DefaultKeyword = "test" };
var presenter = new SearchPanelPresenter(searchPanelControl1, parameters);
```

### View-Presenter Lifecycle

**For Forms (WindowNavigator pattern):**
1. Create presenter instance
2. WindowNavigator creates view instance using `IViewMappingRegister`
3. View is injected into presenter via `IViewAttacher<TView>.AttachView()`
4. `OnViewAttached()` is called on the presenter
5. `Initialize()` or `Initialize(parameters)` is called
6. Window is shown (modal or non-modal)

**For UserControls (Manual pattern):**
1. UserControl already exists (created in designer or code)
2. Create presenter with view (and optional parameters) in constructor
3. Presenter automatically attaches view and initializes
4. Presenter subscribes to control lifecycle events
5. Presenter is automatically disposed when control is disposed

### ViewAction System

A command/action dispatch system that decouples UI events from presenter logic. This system provides WPF ICommand-like functionality for WinForms, including declarative UI binding and dynamic enable/disable based on application state.

#### Core Components

- **ViewAction**: Immutable struct representing an action key created via `ViewAction.Create(string)`
- **ViewActionBinder**: Declaratively maps UI controls (buttons, menu items) to action keys
- **ViewActionDispatcher**: Routes actions to registered handlers with optional CanExecute predicates

#### Best Practices

**1. Use Static Classes for ActionKeys**

Always define ActionKeys in static classes, not as raw strings. Organize them by scope:

```csharp
// Global actions used across the entire application
public static class CommonActions
{
    public static readonly ViewAction Save = ViewAction.Create("Common.Save");
    public static readonly ViewAction Cancel = ViewAction.Create("Common.Cancel");
    public static readonly ViewAction Delete = ViewAction.Create("Common.Delete");
    public static readonly ViewAction Refresh = ViewAction.Create("Common.Refresh");
}

// Module/feature-specific actions
public static class UserEditorActions
{
    public static readonly ViewAction EditUser = ViewAction.Create("UserEditor.Edit");
    public static readonly ViewAction AddUser = ViewAction.Create("UserEditor.Add");
    public static readonly ViewAction ChangePassword = ViewAction.Create("UserEditor.ChangePassword");
}
```

**2. Declarative UI Binding with ViewActionBinder**

Use `ViewActionBinder` to map controls to actions in `OnViewAttached()`:

```csharp
private readonly ViewActionBinder _binder = new ViewActionBinder();

protected override void OnViewAttached()
{
    // Multiple controls can bind to the same action
    _binder.Add(CommonActions.Save, View.SaveButton, View.SaveMenuItem);
    _binder.Add(CommonActions.Delete, View.DeleteButton, View.DeleteMenuItem);
    _binder.Add(UserEditorActions.EditUser, View.EditButton);
}
```

**3. Register Action Handlers with CanExecute**

Register handlers in `RegisterViewActions()` with optional `canExecute` predicates. The binder automatically enables/disables controls based on these predicates:

```csharp
protected override void RegisterViewActions()
{
    // Simple action - always enabled
    _dispatcher.Register(CommonActions.Cancel, OnCancel);

    // Action with CanExecute - only enabled when conditions are met
    _dispatcher.Register(
        CommonActions.Save,
        OnSave,
        canExecute: () => View.HasUnsavedChanges);

    _dispatcher.Register(
        CommonActions.Delete,
        OnDelete,
        canExecute: () => View.HasSelectedItem);

    // Bind to dispatcher for automatic CanExecute support
    _binder.Bind(_dispatcher);
}
```

**4. Parameterized Actions**

Actions can accept typed parameters using the generic `Register<T>` method:

```csharp
// Register handler that accepts a parameter
_dispatcher.Register<string>(
    DocumentActions.Open,
    OnOpenDocument,
    canExecute: () => true);

// Dispatch with parameter
_dispatcher.Dispatch(DocumentActions.Open, filePath);

// Handler implementation
private void OnOpenDocument(string filePath)
{
    // Load document from filePath
}
```

**5. Automatic UI State Updates - Two Patterns**

The framework supports both **action-driven** and **state-driven** UI updates (similar to WPF's ICommand pattern):

**Pattern 1: Action-Driven Updates (Automatic)**

After executing an action, UI state automatically updates. No manual code needed:

```csharp
private void OnSave()
{
    SaveDocument();

    // UI state automatically updates - no manual refresh needed!
    // (ActionExecuted event is automatically triggered)
}

private void OnDelete()
{
    DeleteItem();

    // UI automatically reflects new CanExecute state
}
```

**Pattern 2: State-Driven Updates (Manual Trigger)**

When state changes outside of action execution (e.g., user selects a row, async operation completes), use `RaiseCanExecuteChanged()`:

```csharp
protected override void OnInitialize()
{
    // Subscribe to view events that change state
    View.SelectionChanged += (s, e) =>
    {
        // Notify dispatcher that CanExecute state may have changed
        // Similar to WPF's ICommand.RaiseCanExecuteChanged()
        _dispatcher.RaiseCanExecuteChanged();
    };
}

private async void OnStartBackgroundTask()
{
    _isRunning = true;

    // State changed, notify dispatcher
    _dispatcher.RaiseCanExecuteChanged();

    await DoWorkAsync();

    _isRunning = false;

    // State changed again, notify dispatcher
    _dispatcher.RaiseCanExecuteChanged();
}
```

**Comparison with WPF:**

| Feature | WPF ICommand | This Framework |
|---------|--------------|----------------|
| Event Name | CanExecuteChanged | CanExecuteChanged |
| Manual Trigger | command.RaiseCanExecuteChanged() | dispatcher.RaiseCanExecuteChanged() |
| Auto Trigger | N/A | ActionExecuted event (bonus feature!) |
| Purpose | State changes | State changes + Action completion |

#### Supported Control Types

The binder supports these control types out of the box:
- `CheckBox` - binds to **CheckedChanged** event (triggers when Checked state changes)
- `RadioButton` - binds to **CheckedChanged** event (triggers when Checked state changes)
- `ButtonBase` - binds to **Click** event (regular buttons)
- `ToolStripItem` - binds to **Click** event (ToolStripButton, ToolStripMenuItem, etc.)
- `Control` - binds to **Click** event (fallback for any control)

**Important:** CheckBox and RadioButton bind to `CheckedChanged` rather than `Click`. This means actions trigger whenever the checked state changes, whether by user click, keyboard navigation, or programmatic change.

```csharp
// CheckBox example - triggers when checked state changes
_binder.Add(SettingsActions.ToggleAutoSave, _autoSaveCheckBox);

// RadioButton example - triggers when selected
_binder.Add(ThemeActions.SelectLightTheme, _lightThemeRadio);
_binder.Add(ThemeActions.SelectDarkTheme, _darkThemeRadio);

// Regular Button - triggers on click
_binder.Add(CommonActions.Apply, _applyButton);
```

**CanExecute with CheckBox/RadioButton:**

CanExecute still controls the `Enabled` property (grayed out when false), not the `Checked` property:

```csharp
// This checkbox is only enabled when user is logged in
_dispatcher.Register(
    SettingsActions.ToggleAutoSave,
    OnToggleAutoSave,
    canExecute: () => _isUserLoggedIn);
```

Custom control binding strategies can be registered via `RegisterStrategy<T>()`.

#### Bulk Binding Extensions

When binding many similar controls (like RadioButtons in a survey), use the `AddRange` extension method to reduce code repetition:

**Method 1: AddRange with tuples (recommended)**

```csharp
_binder.AddRange(
    (ThemeActions.Light, _lightRadio),
    (ThemeActions.Dark, _darkRadio),
    (ThemeActions.Auto, _autoRadio),
    (ThemeActions.HighContrast, _highContrastRadio)
);
```

**Method 2: AddRange with dictionary (alternative syntax)**

```csharp
var mapping = new Dictionary<ViewAction, RadioButton>
{
    [QuestionActions.StronglyDisagree] = _radio1,
    [QuestionActions.Disagree] = _radio2,
    [QuestionActions.Neutral] = _radio3,
    [QuestionActions.Agree] = _radio4,
    [QuestionActions.StronglyAgree] = _radio5
};
_binder.AddRange(mapping);
```

**Benefits:**
- Reduces code from N lines to 1 line
- Easier to maintain when adding/removing options
- Type-safe and explicit (no magic conventions)
- Clear at a glance which action binds to which control

#### Complete Example

**Complete Examples:**

See `src/MinformsMVP.Samples/ViewActionExample.cs` for a comprehensive example demonstrating:
- Global and module-specific static ActionKey classes
- ViewActionBinder declarative binding
- CanExecute predicates for dynamic enable/disable
- Automatic UI state updates after action execution
- Multiple controls bound to the same action
- Proper dependency injection with IMessageService

See `src/MinformsMVP.Samples/ViewActionWithParametersExample.cs` for parameterized action examples.

See `src/MinformsMVP.Samples/ViewActionStateChangedExample.cs` for state-driven update examples:
- Using RaiseCanExecuteChanged() for state changes
- Handling view events (SelectionChanged, DataChanged)
- Async operations with state updates
- Comparison between action-driven and state-driven patterns

See `src/MinformsMVP.Samples/CheckBoxDemo/` for CheckBox/RadioButton examples:
- CheckBox binding with CheckedChanged events
- RadioButton groups for theme selection
- CanExecute controlling Enabled state
- Proper MVP separation (no UI types in interface)
- Settings UI pattern with Apply/Reset buttons

See `src/MinformsMVP.Samples/BulkBindingDemo/` for bulk binding examples:
- AddRange with tuples for efficient binding
- AddRange with dictionary for alternative syntax
- Handling surveys/questionnaires with many options
- Best practices for binding multiple similar controls

### Navigation System

**WindowNavigator** (`IWindowNavigator`) manages window lifecycle:

**Modal windows:**
```csharp
// Without parameters
var result = navigator.ShowWindowAsModal<TPresenter, TResult>(presenter);

// With parameters
var result = navigator.ShowWindowAsModal<TPresenter, TParam, TResult>(presenter, parameters);
```

**Non-modal windows:**
```csharp
// Without parameters
var window = navigator.ShowWindow<TPresenter>(presenter);

// With parameters
var window = navigator.ShowWindow<TPresenter, TParam>(presenter, parameters);

// With singleton support (keySelector)
var window = navigator.ShowWindow<TPresenter>(
    presenter,
    keySelector: p => documentId  // Only one window per document
);
```

**Window closing flow:**
- Presenter can request close via `IRequestClose.CloseRequested` event
- Framework checks `CanClose()` before allowing user-initiated closes
- Results are retrieved via `TryGetResult()` and wrapped in `InteractionResult<T>`

### Services Layer

Common application services accessed via `ICommonServices`. These services are essential for maintaining MVP principles by abstracting WinForms UI dependencies.

**Service Interfaces:**

- **IMessageService**: Message boxes and toast notifications
  - Use instead of `MessageBox.Show()`
  - Methods: `ShowInfo()`, `ShowWarning()`, `ShowError()`, `ConfirmYesNo()`, etc.
  - Supports both centered and positioned dialogs (`ShowInfoAt()`, etc.)

- **IDialogProvider**: Standard WinForms dialogs (OpenFile, SaveFile, FolderBrowser, etc.)
  - Use instead of directly creating dialog instances
  - Returns results through abstracted interfaces

- **IFileService**: File I/O operations
  - Abstracts file system access for testability

- **IAppContext**: Application-wide context and settings
  - Provides access to application configuration

### Platform Services Access

All presenters have **built-in access** to platform services through convenience properties. No constructor injection needed for basic services!

**Using services in presenters:**

```csharp
public class MyPresenter : WindowPresenterBase<IMyView>
{
    // No constructor needed - Platform services automatically available!

    protected override void OnInitialize()
    {
        // Use convenience properties
        Messages.ShowInfo("Welcome!", "App");  // IMessageService

        var file = Dialogs.ShowOpenFileDialog("Select file");  // IDialogProvider
        if (file != null)
        {
            var content = Files.ReadAllText(file);  // IFileService
        }
    }

    private void OnSave()
    {
        SaveData();
        Messages.ShowInfo("Data saved!");  // Simple and clean!
    }
}
```

**Available convenience properties:**
- `Messages` - IMessageService (ShowInfo, ShowError, ConfirmYesNo, etc.)
- `Dialogs` - IDialogProvider (OpenFileDialog, SaveFileDialog, FolderBrowser, etc.)
- `Files` - IFileService (ReadAllText, WriteAllText, file operations)
- `Platform` - ICommonServices (full service container for custom services)

**Testing with mock services:**

```csharp
[Fact]
public void Test_MyPresenter()
{
    // 1. Create mock services
    var mockServices = new MockCommonServices();

    // 2. Create presenter with mock platform
    var presenter = new MyPresenter()
        .WithPlatformServices(mockServices);  // Inject mocks before initialization

    // 3. Attach view and initialize
    presenter.AttachView(mockView);
    presenter.Initialize();

    // 4. Verify mock calls
    Assert.True(mockServices.MessageService.InfoMessageShown);
}
```

**Legacy pattern (still supported):**

If you need to inject services manually (e.g., for business-specific services), you can still use constructor injection:

```csharp
public class MyPresenter : WindowPresenterBase<IMyView>
{
    private readonly IUserRepository _users;  // Business service

    public MyPresenter(IUserRepository users)
    {
        _users = users;  // Manual injection for custom services
    }

    private void OnSave()
    {
        _users.Save(userData);
        Messages.ShowInfo("Saved!");  // Platform services still available
    }
}
```

### Change Tracking

**ChangeTracker<T>** (`WinformsMVP.Common.ChangeTracker`) は編集/キャンセルシナリオのための堅牢な変更追跡を提供します。`IChangeTracking`および`IRevertibleChangeTracking`インターフェースを実装しています。

**重要な要件：**

ChangeTracker<T> を使用する型は `ICloneable` を実装し、**必ず深いコピー（ディープコピー）**を返す必要があります。

**深いコピーの実装：**

```csharp
public class UserModel : ICloneable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address Address { get; set; }

    // ✅ 正しい実装（深いコピー）
    public object Clone()
    {
        return new UserModel
        {
            Id = this.Id,
            Name = this.Name,
            Address = this.Address?.Clone() as Address  // ネストされたオブジェクトも深くコピー
        };
    }
}

public class Address : ICloneable
{
    public string City { get; set; }

    public object Clone()
    {
        return new Address { City = this.City };
    }
}
```

**❌ 誤った実装（浅いコピー - 使用禁止）：**

```csharp
// ❌ NG: MemberwiseClone は浅いコピー
public object Clone()
{
    return this.MemberwiseClone();  // 参照型プロパティは共有される！
}

// ❌ NG: ネストされたオブジェクトを深くコピーしていない
public object Clone()
{
    return new UserModel
    {
        Id = this.Id,
        Name = this.Name,
        Address = this.Address  // 同じAddressインスタンスを共有！
    };
}
```

**ChangeTrackerの使用：**

```csharp
public class EditUserPresenter : WindowPresenterBase<IEditUserView>
{
    private ChangeTracker<UserModel> _changeTracker;

    protected override void OnInitialize()
    {
        var user = LoadUser(userId);
        _changeTracker = new ChangeTracker<UserModel>(user);

        View.Model = _changeTracker.CurrentValue;
    }

    protected override void RegisterViewActions()
    {
        _dispatcher.Register(
            CommonActions.Save,
            OnSave,
            canExecute: () => _changeTracker.IsChanged);

        _dispatcher.Register(CommonActions.Reset, OnReset);
    }

    private void OnSave()
    {
        SaveUser(_changeTracker.CurrentValue);
        _changeTracker.AcceptChanges();  // 新しいベースライン
    }

    private void OnReset()
    {
        _changeTracker.RejectChanges();  // ベースラインに戻す
        View.Model = _changeTracker.CurrentValue;
    }
}
```

**深いコピーが必要な理由：**

浅いコピーを使用すると、元のオブジェクトとコピーで参照型プロパティが共有されます。
これにより、`RejectChanges()`を呼び出しても元の値に戻らない問題が発生します：

```csharp
// 浅いコピーの問題例
var user = new UserModel { Address = new Address { City = "Tokyo" } };
var tracker = new ChangeTracker<UserModel>(user);

// AddressがCloneされていないため、同じインスタンスを共有
tracker.CurrentValue.Address.City = "Osaka";

// RejectChanges()を呼び出しても...
tracker.RejectChanges();

// 元の値も変更されてしまう！
Console.WriteLine(tracker.CurrentValue.Address.City);  // "Osaka" (期待値: "Tokyo")
```

**ベストプラクティス：**

1. **すべての参照型プロパティを深くコピー**
   - ネストされたオブジェクト: `Address = this.Address?.Clone() as Address`
   - コレクション: `Tags = this.Tags != null ? new List<string>(this.Tags) : null`

2. **MemberwiseCloneを使用しない**
   - 参照型プロパティが共有される
   - 変更追跡が正しく動作しない

3. **実装をテストする**
   ```csharp
   [Fact]
   public void Clone_CreatesIndependentCopy()
   {
       var original = new UserModel { Name = "John" };
       var clone = original.Clone() as UserModel;

       clone.Name = "Jane";

       Assert.Equal("John", original.Name);  // 元は変更されない
       Assert.Equal("Jane", clone.Name);
   }
   ```

**主要API：**

- `CurrentValue` - 現在追跡している値（読み取り専用）
- `UpdateCurrentValue(T)` - 現在の値を更新（IsChangedChangedイベントを発火）
- `IsChanged` - 現在の値が元の値と異なるかどうか（キャッシュ付き）
- `AcceptChanges()` - 現在の値を新しいベースラインとして確定
- `RejectChanges()` - ベースラインに戻す
- `IsChangedWith(T)` - 指定された値が元の値と異なるかチェック
- `GetOriginalValue()` - 元の値のコピーを取得
- `CanAcceptChanges(out string)` - 変更を受け入れ可能かチェック（検証用）
- `CanRejectChanges(out string)` - 変更を破棄可能かチェック（検証用）

**イベント：**

- `IsChangedChanged` - IsChanged 状態が変わったときに発火

**高度な機能：**

1. **変更通知イベント**
   ```csharp
   var tracker = new ChangeTracker<UserModel>(user);

   // IsChanged 状態の変化を監視
   tracker.IsChangedChanged += (s, e) =>
   {
       // UI更新（例：保存ボタンの有効化/無効化）
       _dispatcher.RaiseCanExecuteChanged();
   };
   ```

2. **スレッドセーフ**
   - すべての操作は内部でロック保護されており、マルチスレッド環境で安全に使用可能

3. **パフォーマンス最適化**
   - IsChanged プロパティは結果をキャッシュし、値が変更されるまで再計算しない

4. **検証サポート**
   ```csharp
   // カスタム検証ロジック（派生クラスで実装）
   public class ValidatedChangeTracker<T> : ChangeTracker<T> where T : class, ICloneable
   {
       public override bool CanAcceptChanges(out string error)
       {
           if (CurrentValue is IValidatable validatable && !validatable.IsValid)
           {
               error = "Validation failed";
               return false;
           }
           error = null;
           return true;
       }
   }
   ```

## Project Structure

- `src/WinformsMVP/`: Core framework library (SDK-style project)
  - `MVP/Presenters/`: Presenter base classes and interfaces
    - `PresenterBase.cs` - Common base (do not inherit directly)
    - `WindowPresenterBase.cs` - For Forms without parameters
    - `WindowPresenterBaseWithParams.cs` - For Forms with parameters
    - `ControlPresenterBase.cs` - For UserControls without parameters
    - `ControlPresenterBaseWithParams.cs` - For UserControls with parameters
  - `MVP/Views/`: View interfaces
  - `MVP/Models/`: Model base classes
  - `MVP/ViewActions/`: Action dispatch system
  - `Services/`: Application service interfaces and implementations
  - `Common/`: Shared utilities, events, helpers

- `src/MinformsMVP.Samples/`: Sample WinForms application (SDK-style project)

- `src/WindowsMVP.Samples.Tests/`: xUnit test project (SDK-style project)

## Development Notes

### MVP Principles - CRITICAL

When writing MVP code, you MUST follow these rules:

**❌ NEVER do this in Presenters or View Interfaces:**
- `MessageBox.Show()` in Presenter - Use `IMessageService` instead
- `new OpenFileDialog()` in Presenter - Use `IDialogProvider` instead
- `Button`, `TextBox`, `Control` in View Interface - Expose only data and behavior
- Direct WinForms API calls in Presenter - Use service abstractions instead
- Any UI element types in Presenter or View Interface

**❌ NEVER expose UI elements in View Interfaces:**
```csharp
// WRONG - View interface exposes UI types
public interface IMyView : IWindowView
{
    Button SaveButton { get; }      // ❌ Never expose Button
    TextBox NameTextBox { get; }    // ❌ Never expose TextBox
    ListBox ItemsListBox { get; }   // ❌ Never expose ListBox
}
```

**✅ ALWAYS do this in View Interfaces:**
```csharp
// CORRECT - View interface exposes only data and behavior
public interface IMyView : IWindowView
{
    // Data properties (no UI types)
    string Name { get; set; }
    bool HasSelection { get; }

    // Behavior methods
    void AddItem(string item);
    void ClearItems();
    void UpdateStatus(string message);

    // Events for state changes
    event EventHandler SelectionChanged;

    // ViewAction integration (allows Form to bind UI controls internally)
    void BindActions(ViewActionDispatcher dispatcher);
}
```

**✅ Presenter interacts only through abstracted interfaces:**
- Inject services (IMessageService, IDialogProvider, etc.) via constructor
- Access View only through the interface (data properties and methods)
- Call `View.BindActions(dispatcher)` to let View bind its UI controls
- Presenter has ZERO knowledge of Button, TextBox, or any UI elements

**✅ View implementation (Form) owns UI elements:**
```csharp
public class MyForm : Form, IMyView
{
    // Private UI controls - NOT exposed through interface
    private Button _saveButton;
    private TextBox _nameTextBox;
    private ViewActionBinder _binder;  // Internal to Form

    // Implement BindActions - Form knows about buttons, Presenter doesn't
    public void BindActions(ViewActionDispatcher dispatcher)
    {
        _binder = new ViewActionBinder();
        _binder.Add(MyActions.Save, _saveButton);  // Map button to action
        _binder.Bind(dispatcher);  // Enable CanExecute support
    }
}
```

All sample code in this repository demonstrates proper MVP principles with complete separation of concerns.

### Target Framework
All projects target .NET Framework 4.8 using SDK-style project format. This provides:
- Automatic file inclusion (no need to manually list .cs files)
- Simplified project files
- Modern tooling support
- PackageReference instead of packages.config

### Testing Framework
Tests use xUnit 2.9.3 with Visual Studio test runner.

### Common Extension Points

When implementing a new feature:

**For Forms/Dialogs:**
1. Define view interface extending `IWindowView`
2. Create presenter extending `WindowPresenterBase<TView>` or `WindowPresenterBase<TView, TParam>`
3. Implement WinForms form with the view interface
4. Register view-to-implementation mapping in `IViewMappingRegister`
5. Use `WindowNavigator` to show the window

**For UserControls:**
1. Define view interface extending `IViewBase`
2. Create presenter extending `ControlPresenterBase<TView>` or `ControlPresenterBase<TView, TParam>`
3. Implement UserControl with the view interface
4. Create presenter in parent form/control, passing the view (and optional parameters)
5. Presenter handles its own lifecycle

### Key Architectural Decisions

1. **Form vs UserControl Separation**: Forms use WindowNavigator for dynamic creation; UserControls use constructor injection
2. **Parameter Support**: Both Forms and UserControls support optional parameterized initialization
3. **Lifecycle Management**: UserControl presenters automatically dispose with their controls
4. **View Injection**: Forms use late binding via WindowNavigator; UserControls use constructor injection
