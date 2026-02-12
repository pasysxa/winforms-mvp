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
dotnet run --project src/WinformsMVP.Samples/WinformsMVP.Samples.csproj
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

#### Why ActionBinder Property Belongs in IViewBase

`ActionBinder` is a **property** (not a method) that exposes the View's `ViewActionBinder` instance. This is a **framework-level abstraction**, similar to WPF's `ICommand` binding. It should be defined in `IViewBase` for these reasons:

**1. Properties Prevent Accidental Code Execution**
- **Property access doesn't execute business logic** - it just returns data
- **Testing is safer and faster** - accessing `View.ActionBinder` won't trigger database calls or network requests
- **Prevents abuse** - developers can't hide complex logic in a property getter (it would be a code smell)

**2. It maintains better MVP separation than exposing UI controls**

❌ **Anti-Pattern (Violates MVP):**
```csharp
// View interface exposes UI controls - WRONG!
public interface IMyView : IWindowView
{
    Button SaveButton { get; }  // ❌ Leaks UI implementation
    Button DeleteButton { get; }
}

// Presenter knows about Button - WRONG!
protected override void OnViewAttached()
{
    _binder.Add(CommonActions.Save, View.SaveButton);  // ❌ Couples to WinForms
}
```

✅ **Recommended Pattern (Follows MVP):**
```csharp
// View interface stays pure - CORRECT!
public interface IMyView : IWindowView
{
    ViewActionBinder ActionBinder { get; }  // ✅ Framework abstraction (property!)
}

// Presenter doesn't know about buttons - CORRECT!
protected override void OnViewAttached()
{
    View.ActionBinder.Bind(Dispatcher);  // ✅ Delegates binding to View
}

// View implementation handles UI details internally - CORRECT!
private ViewActionBinder _binder;
public ViewActionBinder ActionBinder => _binder;

public MyForm()
{
    InitializeComponent();
    InitializeActionBindings();  // Set up bindings in constructor
}

private void InitializeActionBindings()
{
    _binder = new ViewActionBinder();
    _binder.Add(CommonActions.Save, _saveButton);  // Internal UI detail
    _binder.Add(CommonActions.Delete, _deleteButton);
    // No Bind() call here - Presenter will call it!
}
```

**3. Clear separation of responsibilities**

| Component | Responsibility | Knowledge |
|-----------|---------------|-----------|
| **Presenter** | Business logic, when to execute actions, calls `View.ActionBinder.Bind()` | ViewAction keys, execution timing |
| **ViewActionDispatcher** | Route actions to handlers | Action → Handler mapping |
| **IViewBase/View Interface** | Exposes ActionBinder property | Needs to connect UI to actions |
| **View Implementation** | UI details, configures ActionBinder in constructor | Which buttons map to which actions |
| **ViewActionBinder** | Technical binding mechanism | Control events → Action dispatching |

**4. Testability - Even Better Than Method Approach**

Mock views can provide a simple property without any logic:

```csharp
public class MockMyView : IMyView
{
    public ViewActionBinder ActionBinder { get; } = new ViewActionBinder();
}

[Fact]
public void OnViewAttached_ShouldBindActionBinder()
{
    var mockView = new MockMyView();
    var presenter = new MyPresenter();

    presenter.AttachView(mockView);

    // Verify that the ActionBinder was bound (Presenter called Bind)
    Assert.NotNull(mockView.ActionBinder);
}
```

**Key Principle:** `ActionBinder` property is to ViewAction what `ICommand` is to WPF - a framework-level abstraction that maintains clean separation. Using a **property instead of a method** prevents accidental execution of business logic during testing or initialization.

**5. Critical: InitializeActionBindings() is for UI binding ONLY**

⚠️ **WARNING:** The private `InitializeActionBindings()` method in your Form must contain **ONLY** declarative UI binding code. Do NOT include any business logic, database operations, or expensive computations.

❌ **WRONG - Business logic in InitializeActionBindings:**
```csharp
private void InitializeActionBindings()
{
    _binder = new ViewActionBinder();
    _binder.Add(CommonActions.Save, _saveButton);

    // ❌ WRONG - Database operation in InitializeActionBindings!
    var users = _database.GetAllUsers();  // DON'T DO THIS

    // ❌ WRONG - Business logic in InitializeActionBindings!
    if (users.Count > 0)
    {
        _binder.Add(UserActions.Delete, _deleteButton);
    }
}
```

✅ **CORRECT - Pure UI binding only:**
```csharp
private void InitializeActionBindings()
{
    _binder = new ViewActionBinder();

    // ✅ CORRECT - Only declarative binding
    _binder.Add(CommonActions.Save, _saveButton);
    _binder.Add(CommonActions.Delete, _deleteButton);
    _binder.Add(UserActions.Edit, _editButton);

    // ✅ No Bind() call here - Presenter will call View.ActionBinder.Bind()
}
```

**Why this matters:**

| Aspect | Why Property is Better |
|--------|------------------------|
| **Performance** | Property access won't execute database calls (they'd be obviously wrong in a getter) |
| **Architecture** | View layer should NOT contain business logic - properties make this clearer |
| **Testability** | Accessing a property in tests is safer than calling a method (no hidden side effects) |
| **Maintainability** | Properties are expected to be simple - complex logic stands out as a code smell |
| **Threading** | Property access won't accidentally freeze the UI thread |

**Think of ActionBinder like a data property:**
- You wouldn't put database queries in a property getter
- `ActionBinder` is just data (a configured binder object)
- Business logic belongs in the Presenter, not in View property getters or private init methods

**If you need conditional binding based on data:**

Do the data loading in the **Presenter**, then use CanExecute predicates:

```csharp
// Presenter - loads data and controls View state
protected override void OnInitialize()
{
    var users = _userRepository.GetAll();
    View.HasUsers = users.Count > 0;  // Set View property
}

protected override void RegisterViewActions()
{
    _dispatcher.Register(
        UserActions.Delete,
        OnDelete,
        canExecute: () => View.HasUsers);  // Use CanExecute, not conditional binding

    View.ActionBinder.Bind(_dispatcher);  // Bind the pre-configured ActionBinder
}

// View - pure binding, no business logic
private void InitializeActionBindings()
{
    _binder = new ViewActionBinder();
    _binder.Add(UserActions.Delete, _deleteButton);  // Always bind
    // CanExecute controls Enabled state, not conditional Add()
}
```

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

**2. Declarative UI Binding with ActionBinder Property Pattern (Recommended)**

Use the `ActionBinder` property to let the View expose its pre-configured action bindings, keeping the Presenter decoupled from UI elements:

**View Interface:**
```csharp
public interface IMyView : IWindowView
{
    string Name { get; set; }

    // Expose ActionBinder property - framework-level abstraction
    ViewActionBinder ActionBinder { get; }
}
```

**Presenter:**
```csharp
protected override void OnViewAttached()
{
    // Presenter accesses property and calls Bind - doesn't know about buttons!
    View.ActionBinder.Bind(Dispatcher);
}
```

**View Implementation (Form):**
```csharp
public class MyForm : Form, IMyView
{
    // Private UI controls - NOT exposed through interface
    private Button _saveButton;
    private Button _deleteButton;
    private ToolStripMenuItem _saveMenuItem;
    private ViewActionBinder _binder;  // Internal to Form

    public ViewActionBinder ActionBinder => _binder;

    public MyForm()
    {
        InitializeComponent();
        InitializeActionBindings();
    }

    private void InitializeActionBindings()
    {
        _binder = new ViewActionBinder();

        // Multiple controls can bind to the same action
        _binder.Add(CommonActions.Save, _saveButton, _saveMenuItem);
        _binder.Add(CommonActions.Delete, _deleteButton);

        // No Bind() call here - Presenter will call it!
    }
}
```

**Why this is better:**
- ✅ Presenter has ZERO knowledge of Button, TextBox, or any UI elements
- ✅ View interface stays pure (no UI types)
- ✅ Form owns all UI binding logic internally (configured in constructor)
- ✅ Follows WPF ICommand pattern (similar to `Command` binding)
- ✅ Easy to test - mock View just returns a simple property
- ✅ **Property access won't execute unexpected code** (safer than calling a method)

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
    View.ActionBinder.Bind(_dispatcher);
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

See `src/WinformsMVP.Samples/ViewActionExample.cs` for a comprehensive example demonstrating:
- Global and module-specific static ActionKey classes
- **ActionBinder property pattern for proper MVP separation** (recommended)
- CanExecute predicates for dynamic enable/disable
- Automatic UI state updates after action execution
- Multiple controls bound to the same action
- Proper dependency injection with IMessageService
- View Interface with ActionBinder property (no UI types exposed)

See `src/WinformsMVP.Samples/ViewActionWithParametersExample.cs` for parameterized action examples.

See `src/WinformsMVP.Samples/ViewActionStateChangedExample.cs` for state-driven update examples:
- Using RaiseCanExecuteChanged() for state changes
- Handling view events (SelectionChanged, DataChanged)
- Async operations with state updates
- Comparison between action-driven and state-driven patterns

See `src/WinformsMVP.Samples/CheckBoxDemo/` for CheckBox/RadioButton examples:
- CheckBox binding with CheckedChanged events
- RadioButton groups for theme selection
- CanExecute controlling Enabled state
- **Proper MVP separation using ActionBinder property pattern** (no UI types in interface)
- Settings UI pattern with Apply/Reset buttons

See `src/WinformsMVP.Samples/BulkBindingDemo/` for bulk binding examples:
- AddRange with tuples for efficient binding
- AddRange with dictionary for alternative syntax
- Handling surveys/questionnaires with many options
- Best practices for binding multiple similar controls

**Important:** All sample code uses the **ActionBinder property pattern** as the recommended approach. Avoid exposing UI controls (Button, TextBox, etc.) through View interfaces - let the View implementation configure action bindings internally in its constructor via `InitializeActionBindings()`, and expose them through the `ActionBinder` property.

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

### View Mapping Register

**ViewMappingRegister** (`IViewMappingRegister`) manages the mapping between View interfaces and their Form implementations. WindowNavigator uses this mapping to automatically create View instances.

#### Why View Mapping?

When using WindowNavigator, you want to write code like this:

```csharp
var presenter = new SimpleDialogPresenter();
navigator.ShowWindowAsModal(presenter);  // How does it know which Form to create?
```

The framework needs to know: `ISimpleDialogView` → `SimpleDialogForm`

This is where ViewMappingRegister comes in!

#### Registration Patterns

**Pattern 1: Manual Registration (Explicit Control)**

```csharp
var viewMappingRegister = new ViewMappingRegister();

// Register each View explicitly
viewMappingRegister.Register<ISimpleDialogView, SimpleDialogForm>();
viewMappingRegister.Register<IInputDialogView, InputDialogForm>();
viewMappingRegister.Register<IConfirmDialogView, ConfirmDialogForm>();

var navigator = new WindowNavigator(viewMappingRegister);
```

**When to use:**
- ✅ Small applications (< 10 Views)
- ✅ When you want explicit control
- ✅ When Views have custom initialization

**Pattern 2: Automatic Assembly Scanning (Recommended)**

```csharp
var viewMappingRegister = new ViewMappingRegister();

// Automatically scan and register all Views in the assembly
int registered = viewMappingRegister.RegisterFromAssembly(Assembly.GetExecutingAssembly());
// Scans for all Forms implementing IWindowView/IViewBase and registers them

var navigator = new WindowNavigator(viewMappingRegister);
```

**When to use:**
- ✅ Medium to large applications (10+ Views)
- ✅ Convention-based development
- ✅ Reduce boilerplate code

**Pattern 3: Namespace-Scoped Scanning**

```csharp
var viewMappingRegister = new ViewMappingRegister();

// Only scan specific namespaces
viewMappingRegister.RegisterFromNamespace(
    Assembly.GetExecutingAssembly(),
    "MyApp.Dialogs");  // Only Views in MyApp.Dialogs.*

var navigator = new WindowNavigator(viewMappingRegister);
```

**When to use:**
- ✅ Large applications with multiple modules
- ✅ Avoid accidental registration of test Views
- ✅ Organize Views by feature/module

#### Factory Method Registration (Advanced)

For Views that require constructor parameters or special initialization:

```csharp
// Scenario: ComplexDialogForm needs a configuration object
public class ComplexDialogForm : Form, IComplexDialogView
{
    public ComplexDialogForm(AppSettings settings)  // Needs parameter!
    {
        _settings = settings;
        InitializeComponent();
    }
}

// Solution: Use factory method
var settings = LoadAppSettings();
viewMappingRegister.Register<IComplexDialogView>(() => new ComplexDialogForm(settings));

// Now WindowNavigator can create it with parameters
var presenter = new ComplexDialogPresenter();
navigator.ShowWindowAsModal(presenter);  // Works! Uses factory to create View
```

**When to use:**
- ✅ Views need constructor parameters
- ✅ Views need dependency injection
- ✅ Views require special initialization logic

#### Override Registration (Testing)

Replace real Views with mocks for testing:

```csharp
// Production registration
viewMappingRegister.Register<ISimpleDialogView, SimpleDialogForm>();

// Test override
viewMappingRegister.Register<ISimpleDialogView, MockSimpleDialogForm>(allowOverride: true);
// Now WindowNavigator will use MockSimpleDialogForm instead
```

#### Auto-Scan Requirements

For `RegisterFromAssembly()` to work, your Form classes must:

1. ✅ Inherit from `System.Windows.Forms.Form`
2. ✅ Implement an interface that extends `IWindowView` or `IViewBase`
3. ✅ Have a **public parameterless constructor** (or use factory registration)
4. ✅ Not be abstract

**Example:**
```csharp
// ✅ Will be auto-registered
public class SimpleDialogForm : Form, ISimpleDialogView
{
    public SimpleDialogForm()  // Parameterless constructor
    {
        InitializeComponent();
    }
}

// ❌ Will NOT be auto-registered (needs parameters)
public class ComplexDialogForm : Form, IComplexDialogView
{
    public ComplexDialogForm(AppSettings settings)  // Has parameters
    {
        InitializeComponent();
    }
}
// Solution: Use factory registration for ComplexDialogForm
```

#### Complete Setup Example

**In Program.cs:**
```csharp
static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Setup ViewMappingRegister
        var viewMappingRegister = new ViewMappingRegister();

        // Option A: Auto-scan (recommended for most apps)
        viewMappingRegister.RegisterFromAssembly(Assembly.GetExecutingAssembly());

        // Option B: Manual registration (more control)
        // viewMappingRegister.Register<IMainView, MainForm>();
        // viewMappingRegister.Register<IAboutView, AboutForm>();

        // Option C: Factory registration for special cases
        var settings = LoadSettings();
        viewMappingRegister.Register<ISettingsView>(() => new SettingsForm(settings));

        // Create WindowNavigator
        var navigator = new WindowNavigator(viewMappingRegister);

        // Create and show main window
        var mainPresenter = new MainPresenter(navigator);
        navigator.ShowWindow(mainPresenter);

        Application.Run();
    }
}
```

#### Checking Registration Status

```csharp
// Check if a View is registered
if (viewMappingRegister.IsRegistered(typeof(ISimpleDialogView)))
{
    Console.WriteLine("SimpleDialogView is registered");
}

// Get registered count from auto-scan
int count = viewMappingRegister.RegisterFromAssembly(Assembly.GetExecutingAssembly());
Console.WriteLine($"Registered {count} Views automatically");
```

#### Common Patterns

**Pattern: Module-Based Registration**

```csharp
// For large apps, organize by module
public static class ViewMappingConfiguration
{
    public static void ConfigureViews(IViewMappingRegister register)
    {
        // Core dialogs
        RegisterCoreViews(register);

        // Feature modules
        RegisterUserManagementViews(register);
        RegisterReportingViews(register);
        RegisterSettingsViews(register);
    }

    private static void RegisterCoreViews(IViewMappingRegister register)
    {
        register.RegisterFromNamespace(Assembly.GetExecutingAssembly(), "MyApp.Core.Dialogs");
    }

    private static void RegisterUserManagementViews(IViewMappingRegister register)
    {
        register.RegisterFromNamespace(Assembly.GetExecutingAssembly(), "MyApp.UserManagement.Views");
    }

    // ...
}
```

**Pattern: Test Mocking**

```csharp
public class MyPresenterTests
{
    [Fact]
    public void Test_ShowDialog()
    {
        // Arrange
        var viewMappingRegister = new ViewMappingRegister();

        // Use mock Views instead of real Forms
        viewMappingRegister.Register<ISimpleDialogView, MockSimpleDialogForm>();

        var navigator = new WindowNavigator(viewMappingRegister);
        var presenter = new MyPresenter(navigator);

        // Act
        presenter.ShowDialog();

        // Assert
        // ...
    }
}
```

#### Key Benefits

| Benefit | Manual Registration | Auto-Scan | Factory Method |
|---------|---------------------|-----------|----------------|
| **Type Safety** | ✅ Compile-time | ✅ Runtime | ✅ Compile-time |
| **Less Code** | ❌ Verbose | ✅ Minimal | ⚠️ Moderate |
| **Explicit Control** | ✅ Full control | ❌ Convention-based | ✅ Full control |
| **Constructor Parameters** | ❌ Not supported | ❌ Not supported | ✅ Fully supported |
| **Best For** | Small apps | Medium/large apps | Complex Views |

#### Troubleshooting

**Error: "View インターフェース IXxxView に対応する実装型が見つかりません"**

Solution:
1. Check that you registered the View: `viewMappingRegister.Register<IXxxView, XxxForm>();`
2. Or use auto-scan: `viewMappingRegister.RegisterFromAssembly(Assembly.GetExecutingAssembly());`
3. Verify XxxForm has a parameterless constructor

**Error: "既に登録されています"**

Solution:
- Remove duplicate registrations, OR
- Use `allowOverride: true` if you want to replace existing mapping

**Auto-scan finds 0 Views**

Check that your Forms:
1. Implement an interface extending `IWindowView` or `IViewBase`
2. Have public parameterless constructors
3. Are not abstract classes
4. Are in the correct assembly/namespace

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

### Dependency Injection Patterns

The framework supports **three dependency injection patterns** to accommodate different scenarios from legacy code to modern practices.

#### Pattern 1: Service Locator (Recommended for Simple Presenters)

Use the built-in `PlatformServices.Default` for platform services. No constructor needed!

```csharp
public class SimpleDemoPresenter : WindowPresenterBase<ISimpleDemoView>
{
    // No constructor - uses PlatformServices.Default automatically

    protected override void OnInitialize()
    {
        // Direct access via convenience properties
        Messages.ShowInfo("Hello!");
        var file = Dialogs.ShowOpenFileDialog();
    }
}
```

**When to use:**
- ✅ Simple presenters that only need platform services (IMessageService, IDialogProvider, IFileService)
- ✅ Rapid prototyping
- ✅ Legacy code migration (no constructor changes needed)

**Advantages:**
- Zero boilerplate - no constructor needed
- Automatic service access via properties (`Messages`, `Dialogs`, `Files`)
- Easy for beginners

**Disadvantages:**
- Harder to unit test (requires `WithPlatformServices()` helper)
- Global state dependency

#### Pattern 2: Constructor Injection (Recommended for Testable Code)

Explicitly inject dependencies through the constructor for better testability and clarity.

```csharp
public class UserEditorPresenter : WindowPresenterBase<IUserEditorView>
{
    private readonly IMessageService _messageService;
    private readonly IUserRepository _userRepository;

    public UserEditorPresenter(
        IMessageService messageService,
        IUserRepository userRepository)
    {
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    protected override void OnInitialize()
    {
        var users = _userRepository.GetAll();
        View.Users = users;
    }

    private void OnSave()
    {
        _userRepository.Save(userData);
        _messageService.ShowInfo("User saved!", "Success");
    }
}
```

**When to use:**
- ✅ Production code that needs unit testing
- ✅ When you have custom business services (IUserRepository, IOrderService, etc.)
- ✅ When dependencies are explicit and documented

**Advantages:**
- Excellent testability - easy to inject mocks
- Explicit dependencies visible in constructor
- Follows SOLID principles
- Platform services still available via `Messages`, `Dialogs`, `Files` properties

**Disadvantages:**
- More boilerplate code
- Requires manual instantiation or DI container

#### Pattern 3: Hybrid Pattern (Best of Both Worlds)

Use constructor injection for business services, convenience properties for platform services.

```csharp
public class OrderProcessorPresenter : WindowPresenterBase<IOrderProcessorView>
{
    private readonly IOrderService _orderService;  // Business service via constructor

    public OrderProcessorPresenter(IOrderService orderService)
    {
        _orderService = orderService;
    }

    protected override void OnInitialize()
    {
        var orders = _orderService.GetPendingOrders();
        View.Orders = orders;
    }

    private void OnProcess()
    {
        try
        {
            _orderService.ProcessOrders(selectedOrders);
            Messages.ShowInfo("Orders processed!", "Success");  // Platform service via property
        }
        catch (Exception ex)
        {
            Messages.ShowError($"Processing failed: {ex.Message}", "Error");
        }
    }

    private void OnExport()
    {
        var result = Dialogs.ShowSaveFileDialog("Export Orders");  // Platform service via property
        if (result.IsSuccess)
        {
            _orderService.ExportTo(result.Value);
        }
    }
}
```

**When to use:**
- ✅ Most production scenarios
- ✅ Mix of business logic and UI interactions
- ✅ Keep constructor focused on business dependencies

**Advantages:**
- Clean constructor (only business services)
- Easy access to platform services (Messages, Dialogs, Files)
- Great testability
- Best readability

#### Migration from Legacy Code

**Scenario 1: Migrating from MessageBox.Show()**

Before (Legacy WinForms):
```csharp
private void btnSave_Click(object sender, EventArgs e)
{
    try
    {
        SaveData();
        MessageBox.Show("Data saved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

After (MVP with Service Locator - easiest migration):
```csharp
public class MyPresenter : WindowPresenterBase<IMyView>
{
    // No constructor change needed!

    private void OnSave()
    {
        try
        {
            SaveData();
            Messages.ShowInfo("Data saved!", "Success");  // Replace MessageBox with Messages property
        }
        catch (Exception ex)
        {
            Messages.ShowError($"Error: {ex.Message}", "Error");
        }
    }
}
```

After (MVP with Constructor Injection - better testability):
```csharp
public class MyPresenter : WindowPresenterBase<IMyView>
{
    private readonly IMessageService _messageService;

    public MyPresenter(IMessageService messageService)
    {
        _messageService = messageService;
    }

    private void OnSave()
    {
        try
        {
            SaveData();
            _messageService.ShowInfo("Data saved!", "Success");
        }
        catch (Exception ex)
        {
            _messageService.ShowError($"Error: {ex.Message}", "Error");
        }
    }
}
```

**Scenario 2: Migrating from OpenFileDialog**

Before (Legacy):
```csharp
private void btnOpen_Click(object sender, EventArgs e)
{
    using (var dialog = new OpenFileDialog())
    {
        dialog.Filter = "Text files (*.txt)|*.txt";
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadFile(dialog.FileName);
        }
    }
}
```

After (MVP):
```csharp
private void OnOpenFile()
{
    var result = Dialogs.ShowOpenFileDialog(new OpenFileDialogOptions
    {
        Filter = "Text files (*.txt)|*.txt"
    });

    if (result.IsSuccess)
    {
        LoadFile(result.Value);
    }
}
```

#### Testing Patterns

**Testing with Service Locator (requires helper):**

```csharp
[Fact]
public void Test_SimpleDemoPresenter()
{
    // Arrange
    var mockServices = new MockPlatformServices();
    var mockView = new MockSimpleDemoView();

    var presenter = new SimpleDemoPresenter()
        .WithPlatformServices(mockServices);  // Inject mocks before Initialize

    // Act
    presenter.AttachView(mockView);
    presenter.Initialize();

    // Assert
    Assert.True(mockServices.MessageService.InfoMessageShown);
}
```

**Testing with Constructor Injection (natural):**

```csharp
[Fact]
public void Test_UserEditorPresenter()
{
    // Arrange
    var mockMessageService = new MockMessageService();
    var mockUserRepository = new MockUserRepository();
    var mockView = new MockUserEditorView();

    var presenter = new UserEditorPresenter(mockMessageService, mockUserRepository);

    // Act
    presenter.AttachView(mockView);
    presenter.Initialize();

    // Assert
    Assert.True(mockMessageService.InfoMessageShown);
    Assert.Equal(3, mockView.Users.Count);
}
```

#### Integration with Microsoft.Extensions.DependencyInjection (Optional)

For projects that want to use a full DI container, you can integrate `Microsoft.Extensions.DependencyInjection` while maintaining compatibility with legacy code.

**1. Setup in Program.cs:**

```csharp
using Microsoft.Extensions.DependencyInjection;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Build DI container
        var services = new ServiceCollection();

        // Register framework services
        services.AddSingleton<IPlatformServices, DefaultPlatformServices>();
        services.AddSingleton<IMessageService, MessageService>();
        services.AddSingleton<IDialogProvider, DialogProvider>();
        services.AddSingleton<IFileService, FileService>();

        // Register business services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderService, OrderService>();

        // Register presenters
        services.AddTransient<UserEditorPresenter>();
        services.AddTransient<OrderProcessorPresenter>();

        var serviceProvider = services.BuildServiceProvider();

        // Configure PlatformServices to use the container
        PlatformServices.Default = serviceProvider.GetRequiredService<IPlatformServices>();

        // Create and show main form
        var mainForm = new MainForm(serviceProvider);
        Application.Run(mainForm);
    }
}
```

**2. Using the container in MainForm:**

```csharp
public class MainForm : Form
{
    private readonly IServiceProvider _serviceProvider;

    public MainForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeComponent();
    }

    private void btnEditUser_Click(object sender, EventArgs e)
    {
        // Resolve presenter from container
        var presenter = _serviceProvider.GetRequiredService<UserEditorPresenter>();

        // Show window using WindowNavigator
        var navigator = new WindowNavigator();
        navigator.ShowWindowAsModal(presenter);
    }
}
```

**Benefits of DI container integration:**
- ✅ Centralized service registration
- ✅ Lifetime management (Singleton, Scoped, Transient)
- ✅ Automatic dependency resolution
- ✅ Legacy code still works (uses PlatformServices.Default)

**Important:** DI container integration is **optional**. The framework works perfectly without it using Service Locator or manual Constructor Injection.

#### Choosing the Right Pattern

| Scenario | Recommended Pattern | Reason |
|----------|---------------------|--------|
| Simple demo/prototype | Service Locator | Fastest, least code |
| Legacy code migration | Service Locator | No constructor changes |
| Testable business logic | Constructor Injection | Explicit dependencies |
| Production app (mixed) | Hybrid | Business services in constructor, platform services via properties |
| Large enterprise app | DI Container + Constructor Injection | Centralized configuration, automatic resolution |

### Error Handling Best Practices

The framework provides **multiple layers of error handling** to ensure robust applications and good user experience.

#### Layer 1: IMessageService - User-Facing Error Display

Always use `IMessageService` instead of `MessageBox.Show()` for displaying errors to users.

**Basic error display:**

```csharp
private void OnSave()
{
    try
    {
        ValidateInput();
        SaveData();
        Messages.ShowInfo("Data saved successfully!", "Success");
    }
    catch (ValidationException ex)
    {
        Messages.ShowWarning(ex.Message, "Validation Failed");
    }
    catch (Exception ex)
    {
        Messages.ShowError($"Failed to save data: {ex.Message}", "Error");
    }
}
```

**Confirmation before destructive actions:**

```csharp
private void OnDelete()
{
    if (!Messages.ConfirmYesNo("Are you sure you want to delete this item?", "Confirm Delete"))
    {
        return;  // User cancelled
    }

    try
    {
        DeleteItem();
        Messages.ShowInfo("Item deleted.", "Success");
    }
    catch (Exception ex)
    {
        Messages.ShowError($"Failed to delete: {ex.Message}", "Error");
    }
}
```

**Available methods:**
- `ShowInfo(text, caption)` - Informational messages
- `ShowWarning(text, caption)` - Warnings that don't block workflow
- `ShowError(text, caption)` - Error messages
- `ConfirmYesNo(text, caption)` - Returns `true` if user clicks Yes
- `ShowToast(text, type, duration)` - Non-blocking notifications (optional)

#### Layer 2: InteractionResult<T> - Operation Result Wrapping

Use `InteractionResult<T>` for operations that can fail (dialogs, file I/O, network calls).

**File operations:**

```csharp
private void OnOpenFile()
{
    var result = Dialogs.ShowOpenFileDialog(new OpenFileDialogOptions
    {
        Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
        Title = "Select a file to open"
    });

    if (result.IsSuccess)
    {
        LoadFile(result.Value);  // result.Value is the file path
    }
    else if (result.IsCancelled)
    {
        // User clicked Cancel - no action needed
        return;
    }
    else if (result.IsError)
    {
        Messages.ShowError(result.ErrorMessage, "Error");
    }
}
```

**Custom operations:**

```csharp
public InteractionResult<Customer> LoadCustomer(int customerId)
{
    try
    {
        var customer = _repository.GetById(customerId);
        if (customer == null)
        {
            return InteractionResult<Customer>.Error("Customer not found");
        }
        return InteractionResult<Customer>.Ok(customer);
    }
    catch (Exception ex)
    {
        return InteractionResult<Customer>.Error("Failed to load customer", ex);
    }
}

// Usage
private void OnEditCustomer(int id)
{
    var result = LoadCustomer(id);
    if (result.IsSuccess)
    {
        View.Customer = result.Value;
    }
    else
    {
        Messages.ShowError(result.ErrorMessage, "Error");
    }
}
```

**Benefits:**
- ✅ Avoids exception-based control flow
- ✅ Explicit success/cancel/error states
- ✅ Preserves exception information when needed
- ✅ Composable and chainable

#### Layer 3: Centralized Error Messages (DialogDefaults)

Use `DialogDefaults` to configure error messages globally for consistency.

**Setup in Program.cs or app startup:**

```csharp
static void Main()
{
    // Configure default error messages
    DialogDefaults.FileOpenErrorMessage = "ファイルを開くに失敗しました";
    DialogDefaults.FileSaveErrorMessage = "ファイルの保存に失敗しました";
    DialogDefaults.FolderBrowseErrorMessage = "フォルダの選択に失敗しました";
    DialogDefaults.DefaultErrorCaption = "エラー";

    Application.Run(new MainForm());
}
```

**Benefits:**
- ✅ Consistent error messages across the app
- ✅ Easy localization
- ✅ Single place to update messages

#### Layer 4: Global Exception Handler (Optional)

For catching unhandled exceptions at the application level.

**Setup in Program.cs:**

```csharp
static class Program
{
    [STAThread]
    static void Main()
    {
        // Configure global exception handlers
        Application.ThreadException += OnThreadException;
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }

    private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
        HandleUnhandledException(e.Exception);
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            HandleUnhandledException(ex);
        }
    }

    private static void HandleUnhandledException(Exception ex)
    {
        // Log the exception (use your logging framework)
        LogException(ex);

        // Show user-friendly message
        var messageService = PlatformServices.Default.MessageService;
        messageService.ShowError(
            $"An unexpected error occurred:\n\n{ex.Message}\n\nThe application may be in an unstable state.",
            "Unhandled Error");

        // Optionally exit the application
        // Application.Exit();
    }

    private static void LogException(Exception ex)
    {
        // TODO: Integrate with logging framework (NLog, Serilog, etc.)
        System.Diagnostics.Debug.WriteLine($"UNHANDLED EXCEPTION: {ex}");
    }
}
```

**When to use:**
- ✅ Production applications that need crash reporting
- ✅ When integrating with logging frameworks
- ✅ To prevent application crashes from unknown bugs

**Caution:**
- Catching all exceptions can hide bugs during development
- Consider using only in Release builds or with conditional compilation

#### Error Handling Patterns by Scenario

**Scenario 1: Validation Errors**

```csharp
private void OnSave()
{
    // Validate early, fail fast
    var validationErrors = ValidateInput();
    if (validationErrors.Any())
    {
        var errorMessage = string.Join("\n", validationErrors);
        Messages.ShowWarning(errorMessage, "Validation Failed");
        return;  // Don't proceed with save
    }

    try
    {
        SaveData();
        Messages.ShowInfo("Data saved!", "Success");
    }
    catch (Exception ex)
    {
        Messages.ShowError($"Failed to save: {ex.Message}", "Error");
    }
}
```

**Scenario 2: Network/Database Operations**

```csharp
private async void OnRefreshData()
{
    View.ShowLoadingIndicator(true);

    try
    {
        var data = await _repository.GetDataAsync();
        View.Data = data;
        Messages.ShowInfo($"Loaded {data.Count} items", "Success");
    }
    catch (TimeoutException)
    {
        Messages.ShowError("The operation timed out. Please check your network connection.", "Timeout");
    }
    catch (UnauthorizedAccessException)
    {
        Messages.ShowError("You don't have permission to access this data.", "Access Denied");
    }
    catch (Exception ex)
    {
        Messages.ShowError($"Failed to load data: {ex.Message}", "Error");
    }
    finally
    {
        View.ShowLoadingIndicator(false);
    }
}
```

**Scenario 3: File Operations with InteractionResult**

```csharp
private void OnExport()
{
    var fileResult = Dialogs.ShowSaveFileDialog(new SaveFileDialogOptions
    {
        Filter = "CSV files (*.csv)|*.csv",
        DefaultExt = "csv",
        Title = "Export Data"
    });

    if (fileResult.IsCancelled)
    {
        return;  // User cancelled
    }

    if (fileResult.IsError)
    {
        Messages.ShowError(fileResult.ErrorMessage, "Error");
        return;
    }

    try
    {
        ExportToCsv(fileResult.Value);
        Messages.ShowInfo($"Data exported to {Path.GetFileName(fileResult.Value)}", "Success");
    }
    catch (IOException ex)
    {
        Messages.ShowError($"Failed to write file: {ex.Message}", "Export Failed");
    }
}
```

**Scenario 4: Long-Running Operations with Progress**

```csharp
private async void OnProcessBatch()
{
    var itemsToProcess = View.SelectedItems;
    if (!itemsToProcess.Any())
    {
        Messages.ShowWarning("Please select items to process.", "No Selection");
        return;
    }

    View.ShowProgress(true);
    int successCount = 0;
    int errorCount = 0;

    foreach (var item in itemsToProcess)
    {
        try
        {
            await ProcessItemAsync(item);
            successCount++;
            View.UpdateProgress(successCount, itemsToProcess.Count);
        }
        catch (Exception ex)
        {
            errorCount++;
            // Log error but continue processing
            LogError($"Failed to process item {item.Id}: {ex.Message}");
        }
    }

    View.ShowProgress(false);

    // Show summary
    var summary = $"Processing complete:\n\nSuccess: {successCount}\nFailed: {errorCount}";
    if (errorCount > 0)
    {
        Messages.ShowWarning(summary, "Processing Complete");
    }
    else
    {
        Messages.ShowInfo(summary, "Success");
    }
}
```

#### Testing Error Handling

**Testing that errors are displayed correctly:**

```csharp
[Fact]
public void OnSave_WhenExceptionThrown_ShowsError()
{
    // Arrange
    var mockMessageService = new MockMessageService();
    var mockRepository = new MockUserRepository();
    mockRepository.SaveShouldThrow = true;  // Simulate exception

    var presenter = new UserEditorPresenter(mockMessageService, mockRepository);
    presenter.AttachView(new MockUserEditorView());
    presenter.Initialize();

    // Act
    presenter.OnSave();

    // Assert
    Assert.True(mockMessageService.ErrorMessageShown);
    Assert.Contains("Failed to save", mockMessageService.LastErrorMessage);
}
```

**Testing InteractionResult handling:**

```csharp
[Fact]
public void OnOpenFile_WhenUserCancels_DoesNotLoadFile()
{
    // Arrange
    var mockDialogs = new MockDialogProvider();
    mockDialogs.OpenFileDialogResult = InteractionResult<string>.Cancel();

    var presenter = new MyPresenter(mockDialogs);
    presenter.AttachView(new MockMyView());
    presenter.Initialize();

    // Act
    presenter.OnOpenFile();

    // Assert
    Assert.False(presenter.FileWasLoaded);
}
```

#### Key Principles

1. **Never use MessageBox.Show() in presenters** - Always use `IMessageService`
2. **Use InteractionResult<T> for failable operations** - Avoid exception-based control flow
3. **Validate early** - Check inputs before expensive operations
4. **Handle specific exceptions first** - Catch specific exceptions before generic `Exception`
5. **Always inform the user** - Don't silently swallow exceptions
6. **Log errors for debugging** - Especially in production
7. **Test error paths** - Write unit tests for error scenarios
8. **Keep error messages user-friendly** - No stack traces to end users

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

- `src/WinformsMVP.Samples/`: Sample WinForms application (SDK-style project)

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

    // ViewAction integration (Form exposes configured action binder)
    ViewActionBinder ActionBinder { get; }
}
```

**✅ Presenter interacts only through abstracted interfaces:**
- Inject services (IMessageService, IDialogProvider, etc.) via constructor
- Access View only through the interface (data properties and methods)
- Call `View.ActionBinder.Bind(Dispatcher)` to bind the View's pre-configured action mappings
- Use ViewAction system for command binding (see "ViewAction System" section)
- Presenter has ZERO knowledge of Button, TextBox, or any UI elements

**✅ View implementation (Form) owns UI elements:**
```csharp
public class MyForm : Form, IMyView
{
    // Private UI controls - NOT exposed through interface
    private Button _saveButton;
    private TextBox _nameTextBox;
    private ViewActionBinder _binder;  // Internal to Form

    public ViewActionBinder ActionBinder => _binder;

    public MyForm()
    {
        InitializeComponent();
        InitializeActionBindings();
    }

    // Configure action bindings in constructor - Form knows about buttons, Presenter doesn't
    private void InitializeActionBindings()
    {
        _binder = new ViewActionBinder();
        _binder.Add(MyActions.Save, _saveButton);  // Map button to action
        // No Bind() call here - Presenter will call View.ActionBinder.Bind(Dispatcher)
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
