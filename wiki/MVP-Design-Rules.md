# MVP Design Rules

**📋 14 Essential Rules for Clean MVP Architecture**

This guide covers the complete set of Model-View-Presenter design rules based on the Supervising Controller pattern. These rules ensure maintainable, testable, and well-structured WinForms applications.

---

## Table of Contents

1. [Rule 1: View Naming Convention](#rule-1-view-naming-convention)
2. [Rule 2: Presenter Naming Convention](#rule-2-presenter-naming-convention)
3. [Rule 3: Responsibility Separation](#rule-3-responsibility-separation)
4. [Rule 4: OnXxx() Naming for Event Handlers](#rule-4-onxxx-naming-for-event-handlers)
5. [Rule 5: Minimize View-to-Presenter Calls](#rule-5-minimize-view-to-presenter-calls)
6. [Rule 6: No Return Values from Presenter Methods](#rule-6-no-return-values-from-presenter-methods)
7. [Rule 7: Access View Only Through Interface](#rule-7-access-view-only-through-interface)
8. [Rule 8: View Method Visibility](#rule-8-view-method-visibility)
9. [Rule 9: Only Presenter Accesses View](#rule-9-only-presenter-accesses-view)
10. [Rule 10: Long Meaningful Names](#rule-10-long-meaningful-names)
11. [Rule 11: Prefer Methods Over Properties](#rule-11-prefer-methods-over-properties)
12. [Rule 12: All Data in Model](#rule-12-all-data-in-model)
13. [Rule 13: No UI Control Names in Interface](#rule-13-no-ui-control-names-in-interface)
14. [Rule 14: Domain-Driven Naming](#rule-14-domain-driven-naming)

---

## Rule 1: View Naming Convention

**All View interfaces must end with 'View' suffix.**

### ✅ Correct Examples

```csharp
public interface ITaskView : IWindowView { }
public interface IUserEditorView : IWindowView { }
public interface ICustomerListView : IWindowView { }
public interface ISettingsPanelView : IViewBase { }
```

### ❌ Incorrect Examples

```csharp
public interface ITaskDialog : IWindowView { }          // ❌ Should be ITaskDialogView
public interface IUserEditor : IWindowView { }          // ❌ Should be IUserEditorView
public interface ICustomerManagement : IWindowView { }  // ❌ Should be ICustomerManagementView
```

### Why This Matters

- **Clarity**: Immediately identifies View interfaces in the codebase
- **Consistency**: Follows .NET naming conventions (like `XxxController`, `XxxService`)
- **Tooling**: Enables automated scanning and code generation
- **Communication**: Makes architecture discussions clearer ("the TaskView", not "the Task")

### Analyzer Support

**Diagnostic**: `MVP001`
**Severity**: Warning

The Roslyn analyzer automatically detects View interfaces that don't follow this convention.

---

## Rule 2: Presenter Naming Convention

**All Presenter classes must end with 'Presenter' suffix.**

### ✅ Correct Examples

```csharp
public class TaskPresenter : WindowPresenterBase<ITaskView> { }
public class UserEditorPresenter : WindowPresenterBase<IUserEditorView> { }
public class CustomerListPresenter : ControlPresenterBase<ICustomerListView> { }
```

### ❌ Incorrect Examples

```csharp
public class TaskController : WindowPresenterBase<ITaskView> { }      // ❌ Should be TaskPresenter
public class UserEditorLogic : WindowPresenterBase<IUserEditorView> { }  // ❌ Should be UserEditorPresenter
public class CustomerManager : ControlPresenterBase<ICustomerListView> { }  // ❌ Should be CustomerListPresenter
```

### Why This Matters

- **Pattern Recognition**: Clearly identifies business logic layer
- **Separation**: Distinguishes Presenters from Controllers, Services, and Managers
- **Testability**: Makes it easy to find and test all use-case logic

### Analyzer Support

**Diagnostic**: `MVP002`
**Severity**: Warning

---

## Rule 3: Responsibility Separation

**Presenter handles use-case logic only. View handles UI logic.**

This is the **most critical rule** in MVP architecture.

### Presenter Responsibilities (Use-Case Logic)

- Business rule validation
- Coordinating data flow
- Calling services/repositories
- Orchestrating user workflows
- Managing application state

### View Responsibilities (UI Logic)

- Creating and arranging controls
- Data binding
- Visual styling (colors, fonts, layout)
- Animations and transitions
- Responding to View events

### ✅ Correct Example

```csharp
// Presenter - handles business logic
public class OrderPresenter : WindowPresenterBase<IOrderView>
{
    private void OnSubmitOrder()
    {
        // ✅ Business validation
        if (!ValidateOrder(View.Order))
        {
            View.ShowValidationErrors(_validationErrors);
            return;
        }

        // ✅ Business logic
        _orderService.Submit(View.Order);

        // ✅ Tell View what to do
        View.ShowSuccessMessage();
        View.ClearForm();
    }
}

// View - handles UI details
public class OrderForm : Form, IOrderView
{
    public void ShowValidationErrors(IEnumerable<string> errors)
    {
        // ✅ View decides how to display errors
        _errorPanel.BackColor = Color.LightPink;
        _errorLabel.ForeColor = Color.DarkRed;
        _errorLabel.Text = string.Join("\n", errors);
        _errorPanel.Visible = true;
    }
}
```

### ❌ Incorrect Example

```csharp
// ❌ Presenter creating UI controls - WRONG!
public class OrderPresenter : WindowPresenterBase<IOrderView>
{
    private void OnSubmitOrder()
    {
        // ❌ Creating UI controls in Presenter
        var errorLabel = new Label();
        errorLabel.Text = "Validation failed";
        errorLabel.ForeColor = Color.Red;

        // ❌ Manipulating UI properties
        View.ErrorPanel.BackColor = Color.LightPink;
        View.ErrorPanel.Controls.Add(errorLabel);
    }
}
```

### Common Violations

| ❌ Violation | ✅ Correct Approach |
|-------------|-------------------|
| `new Button()` in Presenter | Let View create controls |
| `View.TextBox.BackColor = Color.Red` | `View.HighlightError()` |
| `View.Grid.Rows[0].Cells[1].Value = x` | `View.UpdateCell(row, col, value)` |
| `MessageBox.Show()` in Presenter | Use `IMessageService` |

### Analyzer Support

**Diagnostic**: `MVP003`
**Severity**: Error

The analyzer detects when Presenters create UI control instances (`new Button()`, `new TextBox()`, etc.).

---

## Rule 4: OnXxx() Naming for Event Handlers

**Private/protected methods that handle events should be named `OnXxx()`.**

### ✅ Correct Examples

```csharp
public class TaskPresenter : WindowPresenterBase<ITaskView>
{
    protected override void OnViewAttached()
    {
        View.SaveRequested += OnSave;
        View.CancelRequested += OnCancel;
    }

    private void OnSave(object sender, EventArgs e)
    {
        SaveTask();
    }

    private void OnCancel(object sender, EventArgs e)
    {
        RequestClose();
    }

    private void OnTaskSelectionChanged()
    {
        UpdateTaskDetails();
    }
}
```

### ❌ Incorrect Examples

```csharp
// ❌ Missing 'On' prefix
private void Save() { }
private void HandleSave() { }
private void SaveEventHandler() { }

// ❌ Incorrect prefix
private void DoSave() { }
private void ProcessSave() { }
```

### Acceptable Exceptions

Some helper methods don't need the `On` prefix:

```csharp
// ✅ Helper methods (not event handlers)
private void ValidateInput() { }
private void CalculateTotal() { }
private void UpdateDisplay() { }
private void RequestClose() { }  // Framework method for IRequestClose
```

### Why This Matters

- **Readability**: Clearly identifies event handlers at a glance
- **Consistency**: Follows .NET Framework conventions (`OnLoad`, `OnClick`, etc.)
- **Searchability**: Easy to find all event handlers in a class

---

## Rule 5: Minimize View-to-Presenter Calls

**View should rarely call Presenter methods directly. Use ViewAction system instead.**

### The Problem

In traditional code, Views are littered with presenter method calls:

```csharp
// ❌ Anti-pattern - View knows too much about Presenter
public class TaskForm : Form, ITaskView
{
    private TaskPresenter _presenter;

    private void saveButton_Click(object sender, EventArgs e)
    {
        _presenter.OnSave();  // ❌ Direct coupling
    }

    private void deleteButton_Click(object sender, EventArgs e)
    {
        _presenter.OnDelete();  // ❌ Direct coupling
    }

    private void refreshButton_Click(object sender, EventArgs e)
    {
        _presenter.OnRefresh();  // ❌ Direct coupling
    }
}
```

### The Solution: ViewAction System

```csharp
// ✅ Correct - View uses action binding
public class TaskForm : Form, ITaskView
{
    private ViewActionBinder _binder;

    public void BindActions(ViewActionDispatcher dispatcher)
    {
        _binder = new ViewActionBinder();

        // Declarative binding - no Presenter knowledge needed
        _binder.Add(CommonActions.Save, _saveButton);
        _binder.Add(CommonActions.Delete, _deleteButton);
        _binder.Add(CommonActions.Refresh, _refreshButton);

        _binder.Bind(dispatcher);
    }
}

// Presenter - registers handlers
public class TaskPresenter : WindowPresenterBase<ITaskView>
{
    protected override void RegisterViewActions()
    {
        Dispatcher.Register(CommonActions.Save, OnSave);
        Dispatcher.Register(CommonActions.Delete, OnDelete);
        Dispatcher.Register(CommonActions.Refresh, OnRefresh);
    }
}
```

### Benefits

- **Decoupling**: View doesn't need reference to Presenter
- **Testability**: View can be tested without Presenter
- **Reusability**: Same View can work with different Presenters
- **CanExecute**: Automatic button enable/disable based on state

### See Also

- [ViewAction System Documentation](https://github.com/pasysxa/winforms-mvp/blob/master/CLAUDE.md#viewaction-system)
- [ViewAction Example](Example-ViewAction-Pattern)

---

## Rule 6: No Return Values from Presenter Methods

**Public Presenter methods should return `void` (Tell, Don't Ask principle).**

### ✅ Correct Examples

```csharp
public class UserEditorPresenter : WindowPresenterBase<IUserEditorView>
{
    // ✅ Tell View what to do, don't ask for data
    private void OnSave()
    {
        var user = new User
        {
            Name = View.UserName,
            Email = View.Email
        };

        _userService.Save(user);

        View.ShowSuccessMessage();  // Tell
        View.ClearForm();           // Tell
    }

    // ✅ Query View state through properties
    protected override void RegisterViewActions()
    {
        Dispatcher.Register(
            CommonActions.Save,
            OnSave,
            canExecute: () => View.HasUnsavedChanges);  // Query via property
    }
}
```

### ❌ Incorrect Examples

```csharp
// ❌ Asking View for data via method return
public class UserEditorPresenter : WindowPresenterBase<IUserEditorView>
{
    // ❌ Returns data - violates Tell, Don't Ask
    public bool OnSave()
    {
        if (!IsValid())
            return false;  // ❌ Asking

        SaveUser();
        return true;  // ❌ Asking
    }

    // ❌ Returning validation result
    public ValidationResult ValidateInput()
    {
        // ❌ Should use View properties or Tell pattern
        return new ValidationResult();
    }
}
```

### Why This Matters

- **Command-Query Separation**: Methods either do something (command) or return data (query), not both
- **Simplicity**: Caller doesn't need to handle return values
- **Testability**: Easier to mock and verify behavior
- **Async-Friendly**: `void` methods can be easily converted to `async Task`

### Acceptable Exceptions

Framework methods and override requirements:

```csharp
// ✅ Override framework methods
protected override bool CanClose() { return true; }

// ✅ IRequestClose interface requirement
public bool TryGetResult(out UserResult result) { ... }

// ✅ Private helper methods (not public API)
private bool IsValid() { return true; }
```

### Analyzer Support

**Diagnostic**: `MVP006`
**Severity**: Warning

---

## Rule 7: Access View Only Through Interface

**Presenter must never reference concrete Form types. Always use View interfaces.**

### ✅ Correct Examples

```csharp
public class UserEditorPresenter : WindowPresenterBase<IUserEditorView>
{
    // ✅ Access View through interface
    protected override void OnInitialize()
    {
        View.UserName = LoadUserName();  // IUserEditorView
        View.Email = LoadEmail();        // IUserEditorView
    }

    // ✅ No fields of concrete Form type
    // View property is IUserEditorView (from base class)
}
```

### ❌ Incorrect Examples

```csharp
public class UserEditorPresenter : WindowPresenterBase<IUserEditorView>
{
    // ❌ Field with concrete Form type
    private UserEditorForm _form;  // WRONG!

    public UserEditorPresenter(UserEditorForm form)  // ❌ WRONG!
    {
        _form = form;
    }

    protected override void OnInitialize()
    {
        // ❌ Accessing concrete Form
        _form.textBoxUserName.Text = "John";  // Violates encapsulation
        _form.BackColor = Color.Blue;         // UI manipulation in Presenter
    }
}
```

### Why This Matters

- **Testability**: Can mock View interface without creating Form
- **Substitutability**: Can swap View implementations (WinForms, WPF, Web)
- **Encapsulation**: Presenter doesn't know about UI implementation details
- **Flexibility**: View can change internal controls without affecting Presenter

### Acceptable Pattern for Window Operations

For framework operations like `Close()`, use pattern matching:

```csharp
// ✅ Acceptable - only for framework operations
protected override void OnCancel()
{
    if (View is Form form)
    {
        form.Close();  // No alternative in IViewBase
    }
}
```

### Analyzer Support

**Diagnostic**: `MVP007`
**Severity**: Error

The analyzer detects fields and properties with concrete Form types in Presenter classes.

---

## Rule 8: View Method Visibility

**Public methods in View implementations must be defined in the View interface.**

### ✅ Correct Examples

```csharp
// View interface
public interface ITaskView : IWindowView
{
    void AddTask(TaskModel task);      // ✅ Defined in interface
    void RemoveTask(int taskId);       // ✅ Defined in interface
    void HighlightTask(int taskId);    // ✅ Defined in interface
}

// View implementation
public class TaskForm : Form, ITaskView
{
    // ✅ Public methods are in interface
    public void AddTask(TaskModel task)
    {
        _listView.Items.Add(CreateTaskItem(task));
    }

    public void RemoveTask(int taskId)
    {
        var item = FindTaskItem(taskId);
        _listView.Items.Remove(item);
    }

    // ✅ Private helpers - OK
    private ListViewItem CreateTaskItem(TaskModel task)
    {
        return new ListViewItem(task.Title);
    }
}
```

### ❌ Incorrect Examples

```csharp
// View interface
public interface ITaskView : IWindowView
{
    void AddTask(TaskModel task);
    // ❌ UpdateTaskColor not defined
}

// View implementation
public class TaskForm : Form, ITaskView
{
    public void AddTask(TaskModel task) { }

    // ❌ Public method not in interface
    public void UpdateTaskColor(int taskId, Color color)
    {
        // This breaks the contract!
    }
}
```

### Why This Matters

- **Contract Enforcement**: Interface defines the complete View API
- **Testability**: Mock Views must implement all public methods
- **Discoverability**: All View capabilities are documented in interface
- **Refactoring Safety**: Can't accidentally break Presenter by changing View

### Exceptions

WinForms framework methods are excluded:

```csharp
// ✅ Framework methods don't need to be in interface
public void Dispose() { }              // IDisposable
protected override void OnLoad() { }   // Form lifecycle
public void InitializeComponent() { }  // Designer-generated
```

### Analyzer Support

**Diagnostic**: `MVP008`
**Severity**: Warning

---

## Rule 9: Only Presenter Accesses View

**View should never be accessed from outside the Presenter.**

### ✅ Correct Example

```csharp
// ✅ Only Presenter talks to View
public class MainPresenter : WindowPresenterBase<IMainView>
{
    private readonly INavigator _navigator;
    private readonly TaskPresenter _taskPresenter;

    private void OnShowTasks()
    {
        // ✅ Tell child Presenter to show, not View directly
        _navigator.ShowWindow(_taskPresenter);

        // ❌ NEVER do: _taskPresenter.View.Show();
    }
}
```

### ❌ Incorrect Example

```csharp
// ❌ Other components accessing View directly
public class ReportGenerator
{
    public void GenerateReport(TaskPresenter presenter)
    {
        // ❌ Accessing View from outside Presenter
        var tasks = presenter.View.GetSelectedTasks();  // WRONG!

        // ❌ This breaks encapsulation
        presenter.View.ShowProgress(true);  // WRONG!
    }
}
```

### Why This Matters

- **Single Responsibility**: Only Presenter coordinates View behavior
- **Testability**: Clear ownership of View interactions
- **Maintainability**: Changes to View-Presenter interaction are localized
- **Thread Safety**: Presenter can ensure proper UI thread marshaling

---

## Rule 10: Long Meaningful Names

**Use descriptive, domain-specific names that clearly express intent.**

### ✅ Correct Examples

```csharp
public interface IOrderView : IWindowView
{
    // ✅ Long, descriptive, business-focused
    void DisplayCustomerOrderHistory(IEnumerable<OrderModel> orders);
    void HighlightOverdueOrders();
    void ShowOrderShippedNotification(string orderNumber);
    void MarkOrderAsReadyForShipping(int orderId);
    void UpdateEstimatedDeliveryDate(int orderId, DateTime date);
}
```

### ❌ Incorrect Examples

```csharp
public interface IOrderView : IWindowView
{
    // ❌ Too short, unclear intent
    void Show(List<Order> o);
    void Update();
    void Set(int id);
    void Get();

    // ❌ Generic, non-descriptive
    void DoWork();
    void Process();
    void Handle();
}
```

### Guidelines

| Pattern | ✅ Good | ❌ Bad |
|---------|--------|--------|
| **Verbs** | Display, Highlight, Calculate, Validate | Do, Handle, Process |
| **Length** | 3-6 words | 1 word or abbreviations |
| **Domain Terms** | "Customer", "Order", "Shipment" | "Data", "Item", "Thing" |
| **Specificity** | `MarkOrderAsShipped` | `UpdateOrder` |

### Why This Matters

- **Self-Documenting**: Code is easier to understand without comments
- **Searchability**: Unique names are easier to find
- **Communication**: Business stakeholders can understand method names
- **Refactoring**: Clear intent makes it easier to identify responsibilities

---

## Rule 11: Prefer Methods Over Properties

**Interfaces should contain methods, not properties. Exception: WinForms data binding scenarios.**

### Framework Position

**The WinForms MVP Framework makes a deliberate exception to the classic MVP rule** for bidirectional properties.

### ✅ Preferred Pattern

```csharp
// ✅ Setter-only properties (one-way flow)
public interface IReportView : IWindowView
{
    string StatusMessage { set; }
    int ProgressPercentage { set; }
    bool IsProcessing { set; }
}

// ✅ Getter-only properties (state queries)
public interface ITaskView : IWindowView
{
    bool HasSelection { get; }
    bool HasUnsavedChanges { get; }
    bool IsValid { get; }
}

// ✅ Methods for complex operations
public interface ITaskView : IWindowView
{
    void DisplayTasks(IEnumerable<TaskModel> tasks);
    void ClearForm();
    void ShowValidationErrors(IEnumerable<string> errors);
}
```

### ⚠️ Accepted Pattern (WinForms Data Binding)

```csharp
// ⚠️ Bidirectional properties - ACCEPTED for form inputs
public interface IUserEditorView : IWindowView
{
    string UserName { get; set; }  // ⚠️ Warning, but acceptable
    string Email { get; set; }     // Enables data binding
    DateTime BirthDate { get; set; }
}
```

### Why Bidirectional Properties Are Acceptable

**1. WinForms Data Binding Support**

```csharp
// View implementation uses data binding
_textBox.DataBindings.Add("Text", viewModel, nameof(UserName));
```

**2. Read-Back for Validation**

```csharp
// Presenter needs to read current input
private void OnSave()
{
    if (string.IsNullOrEmpty(View.UserName))  // Need getter
    {
        View.ShowValidationError("Name required");
        return;
    }
}
```

**3. CanExecute Predicates**

```csharp
Dispatcher.Register(
    CommonActions.Save,
    OnSave,
    canExecute: () => View.HasUnsavedChanges);  // Need getter
```

### When to Avoid Bidirectional Properties

```csharp
// ❌ Complex objects should use methods
public interface IReportView : IWindowView
{
    ReportData Data { get; set; }  // ❌ Use DisplayReport(ReportData) instead
}

// ✅ Better: Method makes intent clear
public interface IReportView : IWindowView
{
    void DisplayReport(ReportData data);
    void ClearReport();
}
```

### Summary Table

| Pattern | Status | Use Case |
|---------|--------|----------|
| **Setter-only** `{ set; }` | ✅ Preferred | One-way data flow from Presenter to View |
| **Getter-only** `{ get; }` | ✅ Preferred | State queries (HasSelection, IsValid) |
| **Bidirectional** `{ get; set; }` | ⚠️ Accepted | Form inputs, data binding scenarios |
| **Methods** | ✅ Preferred | Complex operations, collections, void operations |

---

## Rule 12: All Data in Model

**Business data should be in Model classes, not just in UI controls.**

### ✅ Correct Example

```csharp
// Model - owns the data
public class TaskModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public TaskStatus Status { get; set; }
}

// Presenter - works with Model
public class TaskPresenter : WindowPresenterBase<ITaskView>
{
    private TaskModel _task;

    private void OnSave()
    {
        // ✅ Data lives in Model
        _task.Title = View.TaskTitle;
        _task.Description = View.TaskDescription;
        _task.DueDate = View.DueDate;

        _taskRepository.Save(_task);
    }
}

// View - displays Model data
public interface ITaskView : IWindowView
{
    string TaskTitle { get; set; }
    string TaskDescription { get; set; }
    DateTime DueDate { get; set; }
}
```

### ❌ Incorrect Example

```csharp
// ❌ No Model - data only exists in View/controls
public class TaskPresenter : WindowPresenterBase<ITaskView>
{
    private void OnSave()
    {
        // ❌ Pulling data directly from View without Model
        var title = View.TaskTitle;
        var description = View.TaskDescription;
        var dueDate = View.DueDate;

        // ❌ Passing individual fields instead of Model object
        _taskRepository.Save(title, description, dueDate);
    }
}
```

### Why This Matters

- **Separation of Concerns**: Business data separate from UI representation
- **Testability**: Can test business logic without UI
- **Reusability**: Same Model can be used across different Views
- **Validation**: Business rules belong with the data
- **Persistence**: Easy to serialize/deserialize Model objects

### Advanced Pattern: ChangeTracker

```csharp
public class UserEditorPresenter : WindowPresenterBase<IUserEditorView>
{
    private ChangeTracker<UserModel> _changeTracker;

    protected override void OnInitialize()
    {
        var user = _userRepository.GetById(userId);
        _changeTracker = new ChangeTracker<UserModel>(user);

        // Bind Model to View
        View.Model = _changeTracker.CurrentValue;
    }

    protected override void RegisterViewActions()
    {
        Dispatcher.Register(
            CommonActions.Save,
            OnSave,
            canExecute: () => _changeTracker.IsChanged);  // Track changes
    }

    private void OnReset()
    {
        _changeTracker.RejectChanges();  // Revert to original
        View.Model = _changeTracker.CurrentValue;
    }
}
```

---

## Rule 13: No UI Control Names in Interface

**View interface methods should use domain-specific names, not UI control type names.**

### ✅ Correct Examples

```csharp
public interface INavigationView : IWindowView
{
    // ✅ Domain language, not UI types
    void AddNavigationItem(string text, string category);
    void RemoveNavigationItem(string id);
    void HighlightCurrentSection(string sectionName);
    void ExpandCategory(string categoryName);
}

public interface ICustomerView : IWindowView
{
    // ✅ Business operations, not control names
    void DisplayCustomers(IEnumerable<CustomerModel> customers);
    void HighlightSelectedCustomer(int customerId);
    void UpdateCustomerStatus(int customerId, string status);
}
```

### ❌ Incorrect Examples

```csharp
public interface INavigationView : IWindowView
{
    // ❌ Exposes UI control types
    void AddTreeViewNode(string text);           // ❌ "TreeView"
    void RemoveListBoxItem(int index);           // ❌ "ListBox"
    void SetDataGridViewColor(Color color);      // ❌ "DataGridView"
    void UpdateTextBox(string text);             // ❌ "TextBox"
}

public interface ICustomerView : IWindowView
{
    // ❌ Control-centric naming
    void FillComboBox(IEnumerable<CustomerModel> customers);  // ❌ "ComboBox"
    void SelectListViewItem(int index);                       // ❌ "ListView"
    void EnableButton(bool enable);                           // ❌ "Button"
}
```

### Why This Matters

- **Abstraction**: View can change UI controls without affecting contract
- **Testability**: Mock Views don't need to know about WinForms controls
- **Maintainability**: If you change TreeView to ListView, interface doesn't change
- **Business Focus**: Interface describes what happens, not how

### Migration Example

```csharp
// ❌ Before (UI-centric)
public interface ITaskView : IWindowView
{
    void AddTreeViewNode(string taskName);  // ❌ Tied to TreeView
}

// ✅ After (domain-centric)
public interface ITaskView : IWindowView
{
    void AddTask(TaskModel task);  // ✅ Can use any control internally
}

// View can now freely change implementation:
public class TaskForm : Form, ITaskView
{
    // Internal implementation detail - can change anytime
    public void AddTask(TaskModel task)
    {
        // Could be TreeView, ListView, DataGridView, or custom control
        _taskTreeView.Nodes.Add(CreateTaskNode(task));
    }
}
```

### Analyzer Support

**Diagnostic**: `MVP013`
**Severity**: Warning

The analyzer automatically detects UI control type names in interface methods.

**Detected control types**: Button, TextBox, Label, ListBox, DataGrid, DataGridView, TreeView, ListView, ComboBox, CheckBox, RadioButton, and 20+ more.

---

## Rule 14: Domain-Driven Naming

**Use business domain language throughout the interface, not technical UI terms.**

### ✅ Correct Examples

```csharp
public interface IOrderManagementView : IWindowView
{
    // ✅ Business domain language
    void DisplayPendingOrders(IEnumerable<OrderModel> orders);
    void MarkOrderAsShipped(int orderId);
    void HighlightOverdueOrders();
    void ShowShippingLabelPrintDialog(int orderId);
    void UpdateInventoryStatus(string sku, int quantity);
    void CalculateAndDisplayOrderTotal(OrderModel order);
}

public interface ICustomerSupportView : IWindowView
{
    // ✅ Domain-specific actions
    void DisplayCustomerTickets(IEnumerable<TicketModel> tickets);
    void EscalateTicketToPriority(int ticketId);
    void AssignTicketToAgent(int ticketId, string agentName);
    void ShowCustomerCommunicationHistory(int customerId);
}
```

### ❌ Incorrect Examples

```csharp
public interface IOrderManagementView : IWindowView
{
    // ❌ Technical UI language
    void SetDataSource(IEnumerable<object> data);      // ❌ "DataSource"
    void SetRowColor(int rowIndex, Color color);       // ❌ "Row"
    void UpdateLabel(string text);                     // ❌ "Label"
    void ShowDialog();                                 // ❌ Generic
    void RefreshGrid();                                // ❌ "Grid"
}
```

### Comparison Table

| ❌ UI-Centric | ✅ Domain-Centric |
|--------------|------------------|
| `SetDataSource()` | `DisplayCustomers()` |
| `UpdateGrid()` | `RefreshOrderList()` |
| `SetRowColor()` | `HighlightOverdueOrder()` |
| `FillComboBox()` | `LoadCategories()` |
| `EnableButton()` | `AllowOrderSubmission()` |
| `SetLabelText()` | `UpdateOrderStatus()` |

### Why This Matters

- **Communication**: Business stakeholders can understand the interface
- **Documentation**: Interface serves as domain glossary
- **Maintainability**: Changes in UI technology don't affect domain language
- **Onboarding**: New developers understand business logic faster

### Example: Before and After

```csharp
// ❌ Before - Technical language
public interface IReportView : IWindowView
{
    void PopulateDropDown(IEnumerable<string> items);
    void SetProgressBarValue(int value);
    void UpdateStatusLabel(string text);
    void EnableGenerateButton(bool enabled);
}

// ✅ After - Domain language
public interface IReportView : IWindowView
{
    void LoadAvailableReportTypes(IEnumerable<ReportType> types);
    void UpdateReportGenerationProgress(int percentComplete);
    void DisplayReportStatus(string statusMessage);
    void AllowReportGeneration(bool isReady);
}
```

---

## Automated Enforcement

### Roslyn Analyzers

All rules marked with **Analyzer Support** are automatically enforced by the WinForms MVP Analyzers NuGet package.

**Installation**:

```bash
dotnet add package WinformsMVP.Analyzers
```

**Diagnostics**:

| Rule | Diagnostic ID | Severity |
|------|---------------|----------|
| Rule 1: View Naming | MVP001 | Warning |
| Rule 2: Presenter Naming | MVP002 | Warning |
| Rule 3: No UI in Presenter | MVP003 | Error |
| Rule 6: No Return Values | MVP006 | Warning |
| Rule 7: View Interface Only | MVP007 | Error |
| Rule 8: View Method Visibility | MVP008 | Warning |
| Rule 13: No UI Control Names | MVP013 | Warning |

### PowerShell Compliance Checker

For comprehensive compliance checking, run the PowerShell script:

```powershell
cd tools
.\check-mvp-compliance.ps1 -Verbose
```

This generates a detailed compliance report covering all 14 rules.

---

## Related Documentation

- **[MVP-DESIGN-RULES.md](https://github.com/pasysxa/winforms-mvp/blob/master/MVP-DESIGN-RULES.md)** - Complete reference guide
- **[CLAUDE.md](https://github.com/pasysxa/winforms-mvp/blob/master/CLAUDE.md)** - Framework architecture
- **[MVP-COMPLIANCE-REPORT.md](https://github.com/pasysxa/winforms-mvp/blob/master/MVP-COMPLIANCE-REPORT.md)** - Compliance analysis

---

## Quick Reference Checklist

Use this checklist during code reviews:

- [ ] **Rule 1**: All View interfaces end with `View`
- [ ] **Rule 2**: All Presenters end with `Presenter`
- [ ] **Rule 3**: Presenter handles use-case logic only
- [ ] **Rule 4**: Event handlers named `OnXxx()`
- [ ] **Rule 5**: Using ViewAction system (minimal direct calls)
- [ ] **Rule 6**: No return values from Presenter methods
- [ ] **Rule 7**: Access View only through interface
- [ ] **Rule 8**: View public methods are in interface
- [ ] **Rule 9**: Only Presenter accesses View
- [ ] **Rule 10**: Long meaningful method names
- [ ] **Rule 11**: Properties used appropriately
- [ ] **Rule 12**: Data in Model, not just UI controls
- [ ] **Rule 13**: No UI control names in interface
- [ ] **Rule 14**: Domain-driven naming

---

**📊 Framework Compliance**: The WinForms MVP Framework sample code achieves **100% compliance** with all 14 design rules.
