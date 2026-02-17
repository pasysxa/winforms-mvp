# WinForms MVP Framework - 快速入门

> 5分钟学会使用WinForms MVP框架构建干净、可测试的桌面应用

## 📋 目录

- [环境要求](#环境要求)
- [第一步：Hello World](#第一步hello-world)
- [第二步：添加用户交互](#第二步添加用户交互)
- [第三步：使用ViewAction系统](#第三步使用viewaction系统)
- [第四步：添加服务依赖](#第四步添加服务依赖)
- [完整示例代码](#完整示例代码)
- [下一步学习](#下一步学习)

---

## 环境要求

- .NET Framework 4.8 或更高版本
- Visual Studio 2019+ 或 VS Code
- 基础的C#和WinForms知识

---

## 第一步：Hello World

让我们创建一个最简单的MVP应用，显示"Hello MVP!"。

### 1.1 创建View接口

```csharp
using WinformsMVP.MVP.Views;

namespace MyFirstMVP
{
    /// <summary>
    /// 主窗口的View接口
    /// </summary>
    public interface IMainView : IWindowView
    {
        // 定义View需要显示的数据
        string WelcomeMessage { get; set; }
    }
}
```

**关键点**：
- ✅ View接口继承自`IWindowView`（窗体）或`IViewBase`（UserControl）
- ✅ 只暴露**数据和行为**，不暴露UI控件（如Button、TextBox）

### 1.2 创建Presenter

```csharp
using WinformsMVP.MVP.Presenters;

namespace MyFirstMVP
{
    /// <summary>
    /// 主窗口的Presenter（业务逻辑）
    /// </summary>
    public class MainPresenter : WindowPresenterBase<IMainView>
    {
        protected override void OnInitialize()
        {
            // 初始化时设置欢迎消息
            View.WelcomeMessage = "Hello MVP! 欢迎使用WinForms MVP框架";
        }
    }
}
```

**关键点**：
- ✅ Presenter继承自`WindowPresenterBase<TView>`
- ✅ 在`OnInitialize()`中初始化数据
- ✅ 通过`View`属性访问界面

### 1.3 创建Form（View实现）

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
            // 设置窗体
            this.Text = "我的第一个MVP应用";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 创建标签
            _welcomeLabel = new Label
            {
                Location = new Point(50, 100),
                Size = new Size(400, 50),
                Font = new Font("微软雅黑", 16f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.Add(_welcomeLabel);
        }

        // 实现IMainView接口
        public string WelcomeMessage
        {
            get => _welcomeLabel.Text;
            set => _welcomeLabel.Text = value;
        }
    }
}
```

**关键点**：
- ✅ Form实现了`IMainView`接口
- ✅ 属性通过内部控件实现（`_welcomeLabel`）
- ✅ Presenter不知道Label的存在，只知道`WelcomeMessage`属性

### 1.4 启动应用

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

            // 创建View和Presenter
            var view = new MainForm();
            var presenter = new MainPresenter();

            // 关键步骤：附加View并初始化
            presenter.AttachView(view);
            presenter.Initialize();

            // 显示窗体
            Application.Run(view);
        }
    }
}
```

**运行结果**：
```
┌─────────────────────────────────────┐
│ 我的第一个MVP应用              [_][□][×]│
├─────────────────────────────────────┤
│                                     │
│                                     │
│   Hello MVP! 欢迎使用WinForms MVP框架  │
│                                     │
│                                     │
└─────────────────────────────────────┘
```

🎉 **恭喜！你已经创建了第一个MVP应用！**

---

## 第二步：添加用户交互

现在让我们添加一个按钮，点击后更新消息。

### 2.1 更新View接口

```csharp
public interface IMainView : IWindowView
{
    string WelcomeMessage { get; set; }
    string UserName { get; set; }  // 新增：用户输入

    // 新增：按钮点击事件
    event EventHandler GreetButtonClicked;
}
```

### 2.2 更新Presenter

```csharp
public class MainPresenter : WindowPresenterBase<IMainView>
{
    protected override void OnViewAttached()
    {
        // 订阅View事件
        View.GreetButtonClicked += OnGreetButtonClicked;
    }

    protected override void OnInitialize()
    {
        View.WelcomeMessage = "请输入你的名字，然后点击按钮";
    }

    private void OnGreetButtonClicked(object sender, EventArgs e)
    {
        // 业务逻辑：验证并生成问候语
        if (string.IsNullOrWhiteSpace(View.UserName))
        {
            View.WelcomeMessage = "请输入你的名字！";
            return;
        }

        View.WelcomeMessage = $"你好，{View.UserName}！欢迎使用MVP框架！";
    }

    protected override void Cleanup()
    {
        // 取消订阅
        if (View != null)
        {
            View.GreetButtonClicked -= OnGreetButtonClicked;
        }
        base.Cleanup();
    }
}
```

### 2.3 更新Form

```csharp
public partial class MainForm : Form, IMainView
{
    private Label _welcomeLabel;
    private TextBox _nameTextBox;  // 新增
    private Button _greetButton;   // 新增

    public event EventHandler GreetButtonClicked;

    private void InitializeComponent()
    {
        this.Text = "我的第一个MVP应用";
        this.Size = new Size(500, 300);
        this.StartPosition = FormStartPosition.CenterScreen;

        // 欢迎标签
        _welcomeLabel = new Label
        {
            Location = new Point(50, 50),
            Size = new Size(400, 30),
            Font = new Font("微软雅黑", 12f),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 名字输入框
        _nameTextBox = new TextBox
        {
            Location = new Point(150, 120),
            Size = new Size(200, 25),
            Font = new Font("微软雅黑", 10f)
        };

        // 问候按钮
        _greetButton = new Button
        {
            Text = "问候我！",
            Location = new Point(175, 160),
            Size = new Size(150, 40),
            Font = new Font("微软雅黑", 10f)
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

**运行效果**：
```
┌─────────────────────────────────────┐
│ 我的第一个MVP应用              [_][□][×]│
├─────────────────────────────────────┤
│ 请输入你的名字，然后点击按钮          │
│                                     │
│         ┌─────────────┐             │
│  名字： │  张三        │             │
│         └─────────────┘             │
│                                     │
│         ┌───────────┐               │
│         │ 问候我！  │               │
│         └───────────┘               │
│                                     │
└─────────────────────────────────────┘

点击按钮后：
"你好，张三！欢迎使用MVP框架！"
```

---

## 第三步：使用ViewAction系统

ViewAction系统让你摆脱事件订阅，实现类似WPF ICommand的声明式绑定。

### 3.1 定义Actions

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

### 3.2 更新View接口

```csharp
using WinformsMVP.MVP.ViewActions;

public interface IMainView : IWindowView
{
    string WelcomeMessage { get; set; }
    string UserName { get; set; }
    bool HasUserName { get; }  // 新增：用于CanExecute

    // 新增：暴露ActionBinder
    ViewActionBinder ActionBinder { get; }
}
```

### 3.3 更新Presenter

```csharp
public class MainPresenter : WindowPresenterBase<IMainView>
{
    protected override void OnInitialize()
    {
        View.WelcomeMessage = "请输入你的名字，然后点击按钮";
    }

    protected override void RegisterViewActions()
    {
        // 注册Action处理器（带CanExecute）
        Dispatcher.Register(
            MainViewActions.Greet,
            OnGreet,
            canExecute: () => View.HasUserName);  // 自动enable/disable

        Dispatcher.Register(MainViewActions.Clear, OnClear);

        // 框架会自动调用 View.ActionBinder.Bind(Dispatcher)
    }

    private void OnGreet()
    {
        View.WelcomeMessage = $"你好，{View.UserName}！欢迎使用MVP框架！";
    }

    private void OnClear()
    {
        View.UserName = string.Empty;
        View.WelcomeMessage = "请输入你的名字，然后点击按钮";
    }
}
```

### 3.4 更新Form（使用ActionBinder）

```csharp
public partial class MainForm : Form, IMainView
{
    private ViewActionBinder _binder;
    private Label _welcomeLabel;
    private TextBox _nameTextBox;
    private Button _greetButton;
    private Button _clearButton;  // 新增

    public MainForm()
    {
        InitializeComponent();
        InitializeActionBindings();
    }

    private void InitializeActionBindings()
    {
        _binder = new ViewActionBinder();

        // 声明式绑定：按钮 → Action
        _binder.Add(MainViewActions.Greet, _greetButton);
        _binder.Add(MainViewActions.Clear, _clearButton);

        // ✅ 不需要手动订阅Click事件
        // ✅ 框架会自动根据CanExecute启用/禁用按钮
    }

    private void InitializeComponent()
    {
        this.Text = "我的第一个MVP应用";
        this.Size = new Size(500, 300);
        this.StartPosition = FormStartPosition.CenterScreen;

        _welcomeLabel = new Label
        {
            Location = new Point(50, 50),
            Size = new Size(400, 30),
            Font = new Font("微软雅黑", 12f),
            TextAlign = ContentAlignment.MiddleCenter
        };

        _nameTextBox = new TextBox
        {
            Location = new Point(150, 120),
            Size = new Size(200, 25),
            Font = new Font("微软雅黑", 10f)
        };
        _nameTextBox.TextChanged += (s, e) => Dispatcher?.RaiseCanExecuteChanged();

        _greetButton = new Button
        {
            Text = "问候我！",
            Location = new Point(125, 160),
            Size = new Size(100, 40),
            Font = new Font("微软雅黑", 10f)
        };

        _clearButton = new Button
        {
            Text = "清空",
            Location = new Point(275, 160),
            Size = new Size(100, 40),
            Font = new Font("微软雅黑", 10f)
        };

        this.Controls.Add(_welcomeLabel);
        this.Controls.Add(_nameTextBox);
        this.Controls.Add(_greetButton);
        this.Controls.Add(_clearButton);
    }

    // 属性实现
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

    // ✅ 不需要实现ViewActionDispatcher属性
    // ✅ 基类会自动提供Dispatcher
    private ViewActionDispatcher Dispatcher =>
        (this as dynamic).Dispatcher ?? null;  // 通过基类获取
}
```

**ViewAction的优势**：
```
✅ 声明式绑定（_binder.Add）
✅ 自动启用/禁用（基于CanExecute）
✅ 减少事件订阅代码
✅ 类似WPF ICommand
```

---

## 第四步：添加服务依赖

现在让我们使用框架提供的服务（MessageBox、对话框等）。

### 4.1 使用IMessageService

```csharp
public class MainPresenter : WindowPresenterBase<IMainView>
{
    protected override void OnInitialize()
    {
        View.WelcomeMessage = "请输入你的名字，然后点击按钮";
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
        // ✅ 使用Messages服务（不直接调用MessageBox.Show）
        Messages.ShowInfo(
            $"你好，{View.UserName}！欢迎使用MVP框架！",
            "欢迎");

        View.WelcomeMessage = $"已向 {View.UserName} 问好！";
    }

    private void OnClear()
    {
        // ✅ 使用确认对话框
        if (!Messages.ConfirmYesNo("确定要清空输入吗？", "确认"))
        {
            return;  // 用户点击了"否"
        }

        View.UserName = string.Empty;
        View.WelcomeMessage = "已清空，请重新输入";
    }
}
```

**为什么使用服务而不是直接调用MessageBox.Show？**

```csharp
// ❌ 错误：Presenter直接调用WinForms API
private void OnSave()
{
    SaveData();
    MessageBox.Show("保存成功！");  // 不可测试！
}

// ✅ 正确：通过服务抽象
private void OnSave()
{
    SaveData();
    Messages.ShowInfo("保存成功！");  // 可测试！
}

// 单元测试中
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

### 4.2 可用的内置服务

```csharp
// 1. 消息服务
Messages.ShowInfo("信息", "标题");
Messages.ShowWarning("警告", "标题");
Messages.ShowError("错误", "标题");
bool confirmed = Messages.ConfirmYesNo("确认吗？", "标题");

// 2. 对话框服务
var result = Dialogs.ShowOpenFileDialog(new OpenFileDialogOptions
{
    Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
    Title = "选择文件"
});

if (result.IsSuccess)
{
    string filePath = result.Value;
    // 处理文件
}

// 3. 文件服务
string content = Files.ReadAllText("path.txt");
Files.WriteAllText("path.txt", content);

// 4. 窗口导航服务（需要配置）
var presenter = new UserEditorPresenter();
var result = Navigator.ShowWindowAsModal<UserEditorPresenter, UserResult>(presenter);
```

---

## 完整示例代码

以下是第四步的完整代码（可直接复制运行）：

<details>
<summary>点击展开完整代码</summary>

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
            View.WelcomeMessage = "请输入你的名字，然后点击按钮";
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
                $"你好，{View.UserName}！欢迎使用MVP框架！",
                "欢迎");

            View.WelcomeMessage = $"已向 {View.UserName} 问好！";
        }

        private void OnClear()
        {
            if (!Messages.ConfirmYesNo("确定要清空输入吗？", "确认"))
            {
                return;
            }

            View.UserName = string.Empty;
            View.WelcomeMessage = "已清空，请重新输入";
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
            this.Text = "我的第一个MVP应用";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            _welcomeLabel = new Label
            {
                Location = new Point(50, 50),
                Size = new Size(400, 30),
                Font = new Font("微软雅黑", 12f),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _nameTextBox = new TextBox
            {
                Location = new Point(150, 120),
                Size = new Size(200, 25),
                Font = new Font("微软雅黑", 10f)
            };
            _nameTextBox.TextChanged += (s, e) =>
            {
                // 通知Dispatcher状态改变
                var dispatcher = GetDispatcher();
                dispatcher?.RaiseCanExecuteChanged();
            };

            _greetButton = new Button
            {
                Text = "问候我！",
                Location = new Point(125, 160),
                Size = new Size(100, 40),
                Font = new Font("微软雅黑", 10f)
            };

            _clearButton = new Button
            {
                Text = "清空",
                Location = new Point(275, 160),
                Size = new Size(100, 40),
                Font = new Font("微软雅黑", 10f)
            };

            this.Controls.Add(_welcomeLabel);
            this.Controls.Add(_nameTextBox);
            this.Controls.Add(_greetButton);
            this.Controls.Add(_clearButton);
        }

        // 辅助方法：获取Dispatcher（通过反射）
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

## 下一步学习

恭喜！🎉 你已经掌握了WinForms MVP框架的基础。

### 📚 推荐学习路径

1. **深入MVP模式**
   - 阅读：`CLAUDE.md` - MVP设计规则
   - 理解：Tell, Don't Ask原则
   - 实践：ToDoDemo示例

2. **掌握ViewAction系统**
   - 阅读：`CLAUDE.md` - ViewAction章节
   - 学习：CanExecute动态控制
   - 实践：CheckBoxDemo、BulkBindingDemo

3. **学习Presenter通信**
   - 阅读：`docs/PRESENTER_COMMUNICATION_PATTERNS.md`
   - 对比：Service-Based vs EventAggregator
   - 实践：ComplexInteractionDemo示例

4. **探索高级功能**
   - WindowNavigator（窗口导航）
   - ChangeTracker（变更跟踪）
   - EventAggregator（事件聚合器）

5. **查看完整示例**
   - `src/WinformsMVP.Samples/` - 10个完整示例
   - EmailDemo - 综合性示例

### 🔗 相关链接

- [完整文档](CLAUDE.md)
- [Presenter通信模式](docs/PRESENTER_COMMUNICATION_PATTERNS.md)
- [示例代码](src/WinformsMVP.Samples/)
- [单元测试示例](src/WinformsMVP.Samples.Tests/)

### ❓ 常见问题

**Q: Presenter可以直接调用MessageBox.Show()吗？**

A: ❌ 不可以！必须使用`Messages.ShowInfo()`等服务方法。这样Presenter才能被单元测试。

**Q: View接口可以暴露Button等控件吗？**

A: ❌ 不可以！View接口只能暴露数据属性（如`string UserName`）和行为方法（如`void ShowError()`），不能暴露UI控件类型。

**Q: 什么时候用WindowPresenterBase，什么时候用ControlPresenterBase？**

A:
- `WindowPresenterBase` - 用于**Form（窗体）**
- `ControlPresenterBase` - 用于**UserControl（用户控件）**

**Q: ViewAction和传统事件订阅哪个更好？**

A: ViewAction更现代化，推荐使用。优势：
- ✅ 声明式绑定
- ✅ 自动CanExecute控制
- ✅ 减少代码量
- ✅ 类似WPF ICommand

**Q: 如何测试Presenter？**

A: 创建Mock View，注入依赖：
```csharp
[Fact]
public void OnGreet_ShowsMessage()
{
    var mockView = new MockMainView { UserName = "张三" };
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

## 💡 最佳实践提醒

1. ✅ **始终通过View接口访问UI** - 不要在Presenter中使用具体Form类
2. ✅ **使用服务抽象** - Messages、Dialogs、Files而不是直接WinForms API
3. ✅ **优先使用ViewAction** - 而不是手动订阅事件
4. ✅ **记得Cleanup** - 在Presenter中取消事件订阅
5. ✅ **编写单元测试** - Presenter应该100%可测试

---

## 🚀 开始你的MVP之旅！

现在你已经掌握了基础，可以开始构建自己的应用了。记住MVP的核心原则：

> **Presenter = 业务逻辑（What to do）**
> **View = UI逻辑（How to display）**

祝编码愉快！🎉

---

**需要帮助？**
- 查看示例代码：`src/WinformsMVP.Samples/`
- 阅读完整文档：`CLAUDE.md`
- 提交Issue：[GitHub Issues](https://github.com/yourusername/winforms-mvp/issues)
