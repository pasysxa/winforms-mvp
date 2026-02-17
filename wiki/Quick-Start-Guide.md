# WinForms MVP Framework - Quick Start Guide

> Learn to build clean, testable desktop applications with WinForms MVP Framework in 5 minutes

## 📋 Table of Contents

- [Prerequisites](#prerequisites)
- [Step 1: Hello World](#step-1-hello-world)
- [Step 2: Add User Interaction](#step-2-add-user-interaction)
- [Step 3: Use ViewAction System](#step-3-use-viewaction-system)
- [Step 4: Add Service Dependencies](#step-4-add-service-dependencies)
- [Complete Example Code](#complete-example-code)
- [Next Steps](#next-steps)

---

## Prerequisites

- .NET Framework 4.8 or higher
- Visual Studio 2019+ or VS Code
- Basic knowledge of C# and WinForms

---

## Step 1: Hello World

Let's create the simplest MVP application to display "Hello MVP!".

### 1.1 Create View Interface

```csharp
using WinformsMVP.MVP.Views;

namespace MyFirstMVP
{
    /// <summary>
    /// View interface for the main window
    /// </summary>
    public interface IMainView : IWindowView
    {
        // Define data the View needs to display
        string WelcomeMessage { get; set; }
    }
}
```

**Key Points**:
- ✅ View interface inherits from `IWindowView` (for Forms) or `IViewBase` (for UserControls)
- ✅ Only expose **data and behavior**, not UI controls (like Button, TextBox)

### 1.2 Create Presenter

```csharp
using WinformsMVP.MVP.Presenters;

namespace MyFirstMVP
{
    /// <summary>
    /// Presenter for the main window (business logic)
    /// </summary>
    public class MainPresenter : WindowPresenterBase<IMainView>
    {
        protected override void OnInitialize()
        {
            // Set welcome message on initialization
            View.WelcomeMessage = "Hello MVP! Welcome to WinForms MVP Framework";
        }
    }
}
```

**Key Points**:
- ✅ Presenter inherits from `WindowPresenterBase<TView>`
- ✅ Initialize data in `OnInitialize()`
- ✅ Access UI through the `View` property

### 1.3 Create Form (View Implementation)

```csharp
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyFirstMVP
{
    public partial class MainForm : Form, IMainView
    {
        private Label _welcomeLabel;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Setup form
            this.Text = "My First MVP Application";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create label
            _welcomeLabel = new Label
            {
                Location = new Point(50, 100),
                Size = new Size(400, 50),
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.Add(_welcomeLabel);
        }

        // Implement IMainView interface
        public string WelcomeMessage
        {
            get => _welcomeLabel.Text;
            set => _welcomeLabel.Text = value;
        }
    }
}
```

**Key Points**:
- ✅ Form implements `IMainView` interface
- ✅ Properties are backed by internal controls (`_welcomeLabel`)
- ✅ Presenter doesn't know about Label, only knows about `WelcomeMessage` property

### 1.4 Launch the Application

```csharp
using System;
using System.Windows.Forms;

namespace MyFirstMVP
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create View and Presenter
            var view = new MainForm();
            var presenter = new MainPresenter();

            // Critical steps: Attach View and Initialize
            presenter.AttachView(view);
            presenter.Initialize();

            // Show form
            Application.Run(view);
        }
    }
}
```

**Running Result**:
```
┌─────────────────────────────────────┐
│ My First MVP Application       [_][□][×]│
├─────────────────────────────────────┤
│                                     │
│                                     │
│   Hello MVP! Welcome to WinForms    │
│         MVP Framework               │
│                                     │
└─────────────────────────────────────┘
```

🎉 **Congratulations! You've created your first MVP application!**

---

## Step 2: Add User Interaction

Now let's add a button that updates the message when clicked.

### 2.1 Update View Interface

```csharp
public interface IMainView : IWindowView
{
    string WelcomeMessage { get; set; }
    string UserName { get; set; }  // New: User input

    // New: Button click event
    event EventHandler GreetButtonClicked;
}
```

### 2.2 Update Presenter

```csharp
public class MainPresenter : WindowPresenterBase<IMainView>
{
    protected override void OnViewAttached()
    {
        // Subscribe to View events
        View.GreetButtonClicked += OnGreetButtonClicked;
    }

    protected override void OnInitialize()
    {
        View.WelcomeMessage = "Please enter your name and click the button";
    }

    private void OnGreetButtonClicked(object sender, EventArgs e)
    {
        // Business logic: validate and generate greeting
        if (string.IsNullOrWhiteSpace(View.UserName))
        {
            View.WelcomeMessage = "Please enter your name!";
            return;
        }

        View.WelcomeMessage = $"Hello, {View.UserName}! Welcome to MVP Framework!";
    }

    protected override void Cleanup()
    {
        // Unsubscribe
        if (View != null)
        {
            View.GreetButtonClicked -= OnGreetButtonClicked;
        }
        base.Cleanup();
    }
}
```

### 2.3 Update Form

```csharp
public partial class MainForm : Form, IMainView
{
    private Label _welcomeLabel;
    private TextBox _nameTextBox;  // New
    private Button _greetButton;   // New

    public event EventHandler GreetButtonClicked;

    private void InitializeComponent()
    {
        this.Text = "My First MVP Application";
        this.Size = new Size(500, 300);
        this.StartPosition = FormStartPosition.CenterScreen;

        // Welcome label
        _welcomeLabel = new Label
        {
            Location = new Point(50, 50),
            Size = new Size(400, 30),
            Font = new Font("Segoe UI", 12f),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // Name input
        _nameTextBox = new TextBox
        {
            Location = new Point(150, 120),
            Size = new Size(200, 25),
            Font = new Font("Segoe UI", 10f)
        };

        // Greet button
        _greetButton = new Button
        {
            Text = "Greet Me!",
            Location = new Point(175, 160),
            Size = new Size(150, 40),
            Font = new Font("Segoe UI", 10f)
        };
        _greetButton.Click += (s, e) => GreetButtonClicked?.Invoke(s, e);

        this.Controls.Add(_welcomeLabel);
        this.Controls.Add(_nameTextBox);
        this.Controls.Add(_greetButton);
    }

    public string WelcomeMessage
    {
        get => _welcomeLabel.Text;
        set => _welcomeLabel.Text = value;
    }

    public string UserName
    {
        get => _nameTextBox.Text;
        set => _nameTextBox.Text = value;
    }
}
```

**Running Result**:
```
┌─────────────────────────────────────┐
│ My First MVP Application       [_][□][×]│
├─────────────────────────────────────┤
│ Please enter your name and click    │
│ the button                          │
│         ┌─────────────┐             │
│  Name:  │  John       │             │
│         └─────────────┘             │
│                                     │
│         ┌───────────┐               │
│         │ Greet Me! │               │
│         └───────────┘               │
│                                     │
└─────────────────────────────────────┘

After clicking:
"Hello, John! Welcome to MVP Framework!"
```

---

## Step 3: Use ViewAction System

The ViewAction system eliminates event subscriptions and provides WPF ICommand-like declarative binding.

### 3.1 Define Actions

```csharp
using WinformsMVP.MVP.ViewActions;

namespace MyFirstMVP
{
    public static class MainViewActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("MainView");

        public static readonly ViewAction Greet = Factory.Create("Greet");
        public static readonly ViewAction Clear = Factory.Create("Clear");
    }
}
```

### 3.2 Update View Interface

```csharp
using WinformsMVP.MVP.ViewActions;

public interface IMainView : IWindowView
{
    string WelcomeMessage { get; set; }
    string UserName { get; set; }
    bool HasUserName { get; }  // New: For CanExecute

    // New: Expose ActionBinder
    ViewActionBinder ActionBinder { get; }
}
```

### 3.3 Update Presenter

```csharp
public class MainPresenter : WindowPresenterBase<IMainView>
{
    protected override void OnInitialize()
    {
        View.WelcomeMessage = "Please enter your name and click the button";
    }

    protected override void RegisterViewActions()
    {
        // Register action handlers (with CanExecute)
        Dispatcher.Register(
            MainViewActions.Greet,
            OnGreet,
            canExecute: () => View.HasUserName);  // Auto enable/disable

        Dispatcher.Register(MainViewActions.Clear, OnClear);

        // Framework automatically calls View.ActionBinder.Bind(Dispatcher)
    }

    private void OnGreet()
    {
        View.WelcomeMessage = $"Hello, {View.UserName}! Welcome to MVP Framework!";
    }

    private void OnClear()
    {
        View.UserName = string.Empty;
        View.WelcomeMessage = "Please enter your name and click the button";
    }
}
```

### 3.4 Update Form (Using ActionBinder)

```csharp
public partial class MainForm : Form, IMainView
{
    private ViewActionBinder _binder;
    private Label _welcomeLabel;
    private TextBox _nameTextBox;
    private Button _greetButton;
    private Button _clearButton;  // New

    public MainForm()
    {
        InitializeComponent();
        InitializeActionBindings();
    }

    private void InitializeActionBindings()
    {
        _binder = new ViewActionBinder();

        // Declarative binding: Button → Action
        _binder.Add(MainViewActions.Greet, _greetButton);
        _binder.Add(MainViewActions.Clear, _clearButton);

        // ✅ No need to manually subscribe to Click events
        // ✅ Framework auto enables/disables buttons based on CanExecute
    }

    private void InitializeComponent()
    {
        this.Text = "My First MVP Application";
        this.Size = new Size(500, 300);
        this.StartPosition = FormStartPosition.CenterScreen;

        _welcomeLabel = new Label
        {
            Location = new Point(50, 50),
            Size = new Size(400, 30),
            Font = new Font("Segoe UI", 12f),
            TextAlign = ContentAlignment.MiddleCenter
        };

        _nameTextBox = new TextBox
        {
            Location = new Point(150, 120),
            Size = new Size(200, 25),
            Font = new Font("Segoe UI", 10f)
        };
        _nameTextBox.TextChanged += (s, e) => Dispatcher?.RaiseCanExecuteChanged();

        _greetButton = new Button
        {
            Text = "Greet Me!",
            Location = new Point(125, 160),
            Size = new Size(100, 40),
            Font = new Font("Segoe UI", 10f)
        };

        _clearButton = new Button
        {
            Text = "Clear",
            Location = new Point(275, 160),
            Size = new Size(100, 40),
            Font = new Font("Segoe UI", 10f)
        };

        this.Controls.Add(_welcomeLabel);
        this.Controls.Add(_nameTextBox);
        this.Controls.Add(_greetButton);
        this.Controls.Add(_clearButton);
    }

    // Property implementations
    public string WelcomeMessage
    {
        get => _welcomeLabel.Text;
        set => _welcomeLabel.Text = value;
    }

    public string UserName
    {
        get => _nameTextBox.Text;
        set => _nameTextBox.Text = value;
    }

    public bool HasUserName => !string.IsNullOrWhiteSpace(_nameTextBox.Text);

    public ViewActionBinder ActionBinder => _binder;

    // ✅ No need to implement ViewActionDispatcher property
    // ✅ Base class automatically provides Dispatcher
    private ViewActionDispatcher Dispatcher =>
        (this as dynamic).Dispatcher ?? null;  // Obtained from base class
}
```

**ViewAction Benefits**:
```
✅ Declarative binding (_binder.Add)
✅ Auto enable/disable (based on CanExecute)
✅ Less event subscription code
✅ Similar to WPF ICommand
```

---

## Step 4: Add Service Dependencies

Now let's use framework-provided services (MessageBox, dialogs, etc.).

### 4.1 Use IMessageService

```csharp
public class MainPresenter : WindowPresenterBase<IMainView>
{
    protected override void OnInitialize()
    {
        View.WelcomeMessage = "Please enter your name and click the button";
    }

    protected override void RegisterViewActions()
    {
        Dispatcher.Register(
            MainViewActions.Greet,
            OnGreet,
            canExecute: () => View.HasUserName);

        Dispatcher.Register(MainViewActions.Clear, OnClear);
    }

    private void OnGreet()
    {
        // ✅ Use Messages service (not MessageBox.Show directly)
        Messages.ShowInfo(
            $"Hello, {View.UserName}! Welcome to MVP Framework!",
            "Welcome");

        View.WelcomeMessage = $"Greeted {View.UserName}!";
    }

    private void OnClear()
    {
        // ✅ Use confirmation dialog
        if (!Messages.ConfirmYesNo("Are you sure you want to clear the input?", "Confirm"))
        {
            return;  // User clicked "No"
        }

        View.UserName = string.Empty;
        View.WelcomeMessage = "Cleared, please enter again";
    }
}
```

**Why use services instead of calling MessageBox.Show directly?**

```csharp
// ❌ Wrong: Presenter directly calls WinForms API
private void OnSave()
{
    SaveData();
    MessageBox.Show("Saved successfully!");  // Not testable!
}

// ✅ Correct: Use service abstraction
private void OnSave()
{
    SaveData();
    Messages.ShowInfo("Saved successfully!");  // Testable!
}

// In unit tests
[Fact]
public void OnSave_ShowsSuccessMessage()
{
    var mockServices = new MockPlatformServices();
    var presenter = new MyPresenter()
        .WithPlatformServices(mockServices);

    presenter.OnSave();

    Assert.True(mockServices.MessageService.InfoMessageShown);
}
```

### 4.2 Available Built-in Services

```csharp
// 1. Message service
Messages.ShowInfo("Information", "Title");
Messages.ShowWarning("Warning", "Title");
Messages.ShowError("Error", "Title");
bool confirmed = Messages.ConfirmYesNo("Are you sure?", "Title");

// 2. Dialog service
var result = Dialogs.ShowOpenFileDialog(new OpenFileDialogOptions
{
    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
    Title = "Select a file"
});

if (result.IsSuccess)
{
    string filePath = result.Value;
    // Process file
}

// 3. File service
string content = Files.ReadAllText("path.txt");
Files.WriteAllText("path.txt", content);

// 4. Window navigation service (requires configuration)
var presenter = new UserEditorPresenter();
var result = Navigator.ShowWindowAsModal<UserEditorPresenter, UserResult>(presenter);
```

---

## Complete Example Code

Below is the complete code for Step 4 (ready to copy and run):

<details>
<summary>Click to expand complete code</summary>

```csharp
// ============= MainViewActions.cs =============
using WinformsMVP.MVP.ViewActions;

namespace MyFirstMVP
{
    public static class MainViewActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("MainView");

        public static readonly ViewAction Greet = Factory.Create("Greet");
        public static readonly ViewAction Clear = Factory.Create("Clear");
    }
}

// ============= IMainView.cs =============
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.MVP.Views;

namespace MyFirstMVP
{
    public interface IMainView : IWindowView
    {
        string WelcomeMessage { get; set; }
        string UserName { get; set; }
        bool HasUserName { get; }
        ViewActionBinder ActionBinder { get; }
    }
}

// ============= MainPresenter.cs =============
using WinformsMVP.MVP.Presenters;

namespace MyFirstMVP
{
    public class MainPresenter : WindowPresenterBase<IMainView>
    {
        protected override void OnInitialize()
        {
            View.WelcomeMessage = "Please enter your name and click the button";
        }

        protected override void RegisterViewActions()
        {
            Dispatcher.Register(
                MainViewActions.Greet,
                OnGreet,
                canExecute: () => View.HasUserName);

            Dispatcher.Register(MainViewActions.Clear, OnClear);
        }

        private void OnGreet()
        {
            Messages.ShowInfo(
                $"Hello, {View.UserName}! Welcome to MVP Framework!",
                "Welcome");

            View.WelcomeMessage = $"Greeted {View.UserName}!";
        }

        private void OnClear()
        {
            if (!Messages.ConfirmYesNo("Are you sure you want to clear the input?", "Confirm"))
            {
                return;
            }

            View.UserName = string.Empty;
            View.WelcomeMessage = "Cleared, please enter again";
        }
    }
}

// ============= MainForm.cs =============
using System;
using System.Drawing;
using System.Windows.Forms;
using WinformsMVP.MVP.ViewActions;

namespace MyFirstMVP
{
    public partial class MainForm : Form, IMainView
    {
        private ViewActionBinder _binder;
        private Label _welcomeLabel;
        private TextBox _nameTextBox;
        private Button _greetButton;
        private Button _clearButton;

        public MainForm()
        {
            InitializeComponent();
            InitializeActionBindings();
        }

        private void InitializeActionBindings()
        {
            _binder = new ViewActionBinder();
            _binder.Add(MainViewActions.Greet, _greetButton);
            _binder.Add(MainViewActions.Clear, _clearButton);
        }

        private void InitializeComponent()
        {
            this.Text = "My First MVP Application";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            _welcomeLabel = new Label
            {
                Location = new Point(50, 50),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 12f),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _nameTextBox = new TextBox
            {
                Location = new Point(150, 120),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10f)
            };
            _nameTextBox.TextChanged += (s, e) =>
            {
                // Notify Dispatcher of state change
                var dispatcher = GetDispatcher();
                dispatcher?.RaiseCanExecuteChanged();
            };

            _greetButton = new Button
            {
                Text = "Greet Me!",
                Location = new Point(125, 160),
                Size = new Size(100, 40),
                Font = new Font("Segoe UI", 10f)
            };

            _clearButton = new Button
            {
                Text = "Clear",
                Location = new Point(275, 160),
                Size = new Size(100, 40),
                Font = new Font("Segoe UI", 10f)
            };

            this.Controls.Add(_welcomeLabel);
            this.Controls.Add(_nameTextBox);
            this.Controls.Add(_greetButton);
            this.Controls.Add(_clearButton);
        }

        // Helper method: Get Dispatcher (via reflection)
        private ViewActionDispatcher GetDispatcher()
        {
            var prop = this.GetType().GetProperty("Dispatcher",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            return prop?.GetValue(this) as ViewActionDispatcher;
        }

        public string WelcomeMessage
        {
            get => _welcomeLabel.Text;
            set => _welcomeLabel.Text = value;
        }

        public string UserName
        {
            get => _nameTextBox.Text;
            set => _nameTextBox.Text = value;
        }

        public bool HasUserName => !string.IsNullOrWhiteSpace(_nameTextBox.Text);

        public ViewActionBinder ActionBinder => _binder;
    }
}

// ============= Program.cs =============
using System;
using System.Windows.Forms;

namespace MyFirstMVP
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var view = new MainForm();
            var presenter = new MainPresenter();

            presenter.AttachView(view);
            presenter.Initialize();

            Application.Run(view);
        }
    }
}
```

</details>

---

## Next Steps

Congratulations! 🎉 You've mastered the basics of WinForms MVP Framework.

### 📚 Recommended Learning Path

1. **Deep Dive into MVP Pattern**
   - Read: `CLAUDE.md` - MVP Design Rules
   - Understand: Tell, Don't Ask Principle
   - Practice: ToDoDemo example

2. **Master ViewAction System**
   - Read: `CLAUDE.md` - ViewAction section
   - Learn: Dynamic CanExecute control
   - Practice: CheckBoxDemo, BulkBindingDemo

3. **Learn Presenter Communication**
   - Read: `docs/PRESENTER_COMMUNICATION_PATTERNS.md`
   - Compare: Service-Based vs EventAggregator
   - Practice: ComplexInteractionDemo example

4. **Explore Advanced Features**
   - WindowNavigator (Window navigation)
   - ChangeTracker (Change tracking)
   - EventAggregator (Event aggregator)

5. **Review Complete Examples**
   - `src/WinformsMVP.Samples/` - 10 complete examples
   - EmailDemo - Comprehensive example

### 🔗 Related Links

- [Complete Documentation](CLAUDE.md)
- [Presenter Communication Patterns](docs/PRESENTER_COMMUNICATION_PATTERNS.md)
- [Sample Code](src/WinformsMVP.Samples/)
- [Unit Test Examples](src/WinformsMVP.Samples.Tests/)

### ❓ FAQ

**Q: Can Presenter directly call MessageBox.Show()?**

A: ❌ No! You must use `Messages.ShowInfo()` and other service methods. This makes Presenter unit testable.

**Q: Can View interface expose Button controls?**

A: ❌ No! View interface should only expose data properties (like `string UserName`) and behavior methods (like `void ShowError()`), not UI control types.

**Q: When to use WindowPresenterBase vs ControlPresenterBase?**

A:
- `WindowPresenterBase` - For **Forms (Windows)**
- `ControlPresenterBase` - For **UserControls**

**Q: ViewAction vs traditional event subscription - which is better?**

A: ViewAction is more modern, recommended. Benefits:
- ✅ Declarative binding
- ✅ Automatic CanExecute control
- ✅ Less code
- ✅ Similar to WPF ICommand

**Q: How to test Presenter?**

A: Create Mock View, inject dependencies:
```csharp
[Fact]
public void OnGreet_ShowsMessage()
{
    var mockView = new MockMainView { UserName = "John" };
    var mockServices = new MockPlatformServices();

    var presenter = new MainPresenter()
        .WithPlatformServices(mockServices);

    presenter.AttachView(mockView);
    presenter.Initialize();
    presenter.OnGreet();

    Assert.True(mockServices.MessageService.InfoMessageShown);
}
```

---

## 💡 Best Practices Reminder

1. ✅ **Always access UI through View interface** - Don't use concrete Form class in Presenter
2. ✅ **Use service abstractions** - Messages, Dialogs, Files instead of direct WinForms API
3. ✅ **Prefer ViewAction** - Over manual event subscription
4. ✅ **Remember Cleanup** - Unsubscribe events in Presenter
5. ✅ **Write unit tests** - Presenter should be 100% testable

---

## 🚀 Start Your MVP Journey!

Now that you've mastered the basics, you can start building your own applications. Remember the core MVP principle:

> **Presenter = Business Logic (What to do)**
> **View = UI Logic (How to display)**

Happy coding! 🎉

---

**Need Help?**
- Check sample code: `src/WinformsMVP.Samples/`
- Read complete docs: `CLAUDE.md`
- Submit Issues: [GitHub Issues](https://github.com/yourusername/winforms-mvp/issues)
