# MVP Design Rules - WinForms MVP Framework

> **基于**: [Design Rules for Model-View-Presenter](http://st-www.cs.illinois.edu/users/smarch/st-docs/mvc.html) by Kjell-Sverre Jerijærvi
> **适配**: WinForms MVP Framework with ViewAction pattern

本框架采用**Supervising Controller模式的MVP**。MVP是MVC的演化版本，考虑到现代UI框架（如WinForms）的View已经足够智能，可以处理数据绑定、事件处理等任务。**Presenter只应处理用例逻辑（use-case logic），而非View逻辑（view logic）**。

## 🎯 核心原则

### MVP vs MVC

| 方面 | 传统MVC (Controller) | 现代MVP (Presenter) |
|------|---------------------|-------------------|
| **View智能程度** | 简单，需要Controller显式设置每个控件的值 | 智能，支持数据绑定和事件处理 |
| **职责** | Controller处理用例逻辑 + View逻辑 | Presenter只处理用例逻辑 |
| **数据注入** | Controller显式调用 `view.SetLabel("text")` | Presenter注入Model，View自己绑定 `view.Model = data` |
| **示例** | `controller.SetTextBoxValue(textBox, "Hello")` | `view.UserName = "Hello"` (通过属性) |

**关键区别**:
- **Presenter**: 只关心**业务用例流程**（"用户点击保存 → 验证 → 保存到数据库 → 显示成功消息"）
- **View**: 自己处理**UI细节**（"如何显示成功消息 → 用绿色Label还是MessageBox？View决定"）

---

## 📋 14条设计规则

### 规则1: 命名约定 - View

**所有View类和接口都应有 `View` 后缀**

✅ **正确**:
```csharp
public interface ITaskView : IWindowView { }
public class TaskView : Form, ITaskView { }

public interface IUserEditorView : IWindowView { }
public class UserEditorForm : Form, IUserEditorView { }
```

❌ **错误**:
```csharp
public interface ITaskForm { }           // 缺少View后缀
public class TaskDialog : Form { }       // 缺少View后缀
```

### 规则2: 命名约定 - Presenter

**所有Presenter都应有 `Presenter` 后缀**

✅ **正确**:
```csharp
public class TaskViewPresenter : WindowPresenterBase<ITaskView> { }
public class UserEditorPresenter : WindowPresenterBase<IUserEditorView> { }
```

❌ **错误**:
```csharp
public class TaskController { }          // 错误的模式名
public class UserEditorHandler { }       // 缺少Presenter后缀
```

### 规则3: 职责分离

**Presenter处理所有用例处理逻辑，但保持GUI控件细节在View中**

✅ **正确** - Presenter处理用例逻辑:
```csharp
// Presenter - 业务流程
private void OnSave()
{
    // ✅ 验证业务规则
    if (!ValidateUserData())
    {
        View.ShowValidationErrors(_errors);  // 告诉View显示错误
        return;
    }

    // ✅ 调用业务服务
    _userRepository.Save(View.UserModel);

    // ✅ 协调UI状态
    View.ShowSuccessMessage("User saved!");
    View.EnableEditMode(false);
}
```

❌ **错误** - Presenter处理View细节:
```csharp
// Presenter - 不应该知道UI控件细节！
private void OnSave()
{
    // ❌ Presenter不应该知道Label的颜色
    View.SetLabelColor(Color.Red);

    // ❌ Presenter不应该知道Button的位置
    View.MoveButtonToRight(10, 20);

    // ❌ Presenter不应该知道具体用什么控件显示
    View.ShowMessageBoxWithIcon(MessageBoxIcon.Information);
}
```

✅ **正确** - View处理UI细节:
```csharp
// View Implementation
public void ShowValidationErrors(List<string> errors)
{
    // ✅ View决定如何显示错误
    _errorLabel.Text = string.Join("\n", errors);
    _errorLabel.ForeColor = Color.Red;           // View的决定
    _errorPanel.Visible = true;                  // View的决定

    // 或者View可以选择用MessageBox
    // MessageBox.Show(string.Join("\n", errors), "Validation Error");
}
```

### 规则4: Presenter方法命名

**所有由View调用的Presenter方法必须以 `OnXxx()` 开头，因为它们本质上是事件处理器**

✅ **正确**:
```csharp
// Presenter
public void OnViewReady() { }
public void OnSaveRequested() { }
public void OnUserSelected() { }
public void OnFormClosing() { }
```

❌ **错误**:
```csharp
// Presenter
public void Initialize() { }        // ❌ 应该是 OnInitialize()
public void SaveData() { }           // ❌ 应该是 OnSaveRequested()
public void HandleSelection() { }    // ❌ 应该是 OnUserSelected()
```

**注意**: 本框架使用ViewAction系统，View不直接调用Presenter方法，而是通过ActionDispatcher。但内部处理方法仍应遵循 `OnXxx()` 命名：

```csharp
protected override void RegisterViewActions()
{
    _dispatcher.Register(CommonActions.Save, OnSave);     // ✅
    _dispatcher.Register(CommonActions.Delete, OnDelete); // ✅
}

private void OnSave() { }      // ✅ 正确命名
private void OnDelete() { }    // ✅ 正确命名
```

### 规则5: 最小化View到Presenter的调用

**View到Presenter的调用应保持在绝对最小，仅用于"事件"类型的调用**

✅ **正确** - 仅事件通知:
```csharp
// View
protected override void OnLoad(EventArgs e)
{
    base.OnLoad(e);
    _presenter.OnViewReady();  // ✅ 仅通知事件
}
```

❌ **错误** - 获取数据或服务:
```csharp
// View
private void btnLoad_Click(object sender, EventArgs e)
{
    // ❌ 错误 - View直接从Presenter获取数据
    var users = _presenter.GetUsers();
    _listBox.DataSource = users;

    // ❌ 错误 - View通过Presenter访问服务
    _presenter.GetMessageService().ShowInfo("Loaded");
}
```

**本框架的处理**: 使用**ViewAction + ActionBinder**模式，View根本不需要直接调用Presenter：

```csharp
// View只需配置绑定
private void InitializeActionBindings()
{
    _binder = new ViewActionBinder();
    _binder.Add(UserActions.Load, _loadButton);  // ✅ 声明式绑定
    _binder.Add(UserActions.Save, _saveButton);
}

public ViewActionBinder ActionBinder => _binder;  // ✅ 通过属性暴露
```

### 规则6: 禁止通过Presenter引用访问Model或Service

**禁止使用Presenter引用直接访问Model或Service，Presenter方法不应有返回值**

❌ **绝对禁止**:
```csharp
// View
private void btnLoad_Click(object sender, EventArgs e)
{
    // ❌ 严重违规 - View绕过Presenter直接访问Model
    var model = _presenter.Model;
    _textBox.Text = model.Name;

    // ❌ 严重违规 - View绕过Presenter访问Service
    _presenter.UserRepository.GetAll();

    // ❌ 严重违规 - 从Presenter获取返回值
    var result = _presenter.ValidateAndSave();
    if (result) { /* ... */ }
}
```

✅ **正确** - 遵循"Tell, Don't Ask"原则:
```csharp
// Presenter通过接口"告诉"View做什么
private void OnLoad()
{
    var users = _userRepository.GetAll();
    View.Users = users;  // ✅ Tell: 设置数据
    View.EnableEditMode(false);  // ✅ Tell: 设置状态
}

// View接口只有设置方法，没有返回值
public interface IUserListView : IWindowView
{
    IEnumerable<User> Users { set; }      // ✅ 只有setter
    void EnableEditMode(bool enabled);    // ✅ void返回
    void ShowMessage(string message);     // ✅ void返回
}
```

**为什么重要？**
- 可测试性: Mock View时，调用是可记录和验证的
- 关注点分离: View不知道数据从哪来
- 遵循"Tell, Don't Ask"原则

### 规则7: Presenter只能通过接口访问View

**Presenter到View的所有调用必须通过View接口**

✅ **正确**:
```csharp
public class UserEditorPresenter : WindowPresenterBase<IUserEditorView>
{
    private void OnSave()
    {
        // ✅ 通过接口 IUserEditorView
        View.UserName = "John";
        View.ShowSuccessMessage("Saved!");
    }
}
```

❌ **错误**:
```csharp
public class UserEditorPresenter : WindowPresenterBase<IUserEditorView>
{
    private UserEditorForm _concreteForm;  // ❌ 持有具体类型引用

    public void AttachConcreteView(UserEditorForm form)
    {
        _concreteForm = form;  // ❌ 错误！
    }

    private void OnSave()
    {
        // ❌ 直接访问具体Form
        _concreteForm.txtUserName.Text = "John";
        _concreteForm.btnSave.Enabled = false;
    }
}
```

### 规则8: View方法的可见性

**View中的方法不应该是public，除非它们在接口中定义**

✅ **正确**:
```csharp
public class UserEditorForm : Form, IUserEditorView
{
    // ✅ 接口成员 - public
    public string UserName
    {
        get => _txtUserName.Text;
        set => _txtUserName.Text = value;
    }

    // ✅ 接口成员 - public
    public void ShowSuccessMessage(string message)
    {
        _statusLabel.Text = message;
    }

    // ✅ 私有辅助方法 - private
    private void InitializeControls()
    {
        // UI初始化
    }

    // ✅ 私有事件处理 - private
    private void txtUserName_TextChanged(object sender, EventArgs e)
    {
        // 触发View事件
    }
}
```

❌ **错误**:
```csharp
public class UserEditorForm : Form, IUserEditorView
{
    // ❌ 不在接口中定义，却是public
    public void SetButtonColor(Color color)
    {
        _btnSave.BackColor = color;
    }

    // ❌ 暴露内部控件
    public TextBox UserNameTextBox => _txtUserName;
}
```

### 规则9: 禁止从Presenter以外访问View

**禁止从Presenter以外的任何地方访问View，除了加载和显示View**

✅ **正确**:
```csharp
// Program.cs or ModuleController
static void Main()
{
    var navigator = new WindowNavigator(viewMappingRegister);
    var presenter = new UserEditorPresenter();

    // ✅ 可以: 加载和显示View
    navigator.ShowWindow(presenter);
}

// Presenter
public class UserEditorPresenter : WindowPresenterBase<IUserEditorView>
{
    private void OnSave()
    {
        // ✅ 可以: Presenter访问View
        View.ShowSuccessMessage("Saved!");
    }
}
```

❌ **错误**:
```csharp
// Service层
public class UserService
{
    private IUserEditorView _view;  // ❌ Service不应该知道View

    public void SaveUser(User user)
    {
        // 保存逻辑
        _view.ShowSuccessMessage("Saved!");  // ❌ 错误！
    }
}

// 其他Presenter
public class AnotherPresenter
{
    private IUserEditorView _otherView;  // ❌ 不应跨Presenter访问View

    private void OnAction()
    {
        _otherView.UserName = "Test";  // ❌ 错误！
    }
}
```

### 规则10: 接口方法使用长的、有意义的名称

**接口方法应基于用例的领域语言，使用长而有意义的名称（而非"SetDataSource"这样的通用名）**

✅ **正确** - 领域特定命名:
```csharp
public interface ICustomerOrderView : IWindowView
{
    // ✅ 描述业务含义
    void DisplayCustomerOrderHistory(IEnumerable<Order> orders);
    void HighlightOverdueOrders();
    void ShowOrderCancellationConfirmation(Order order);
    void UpdateTotalOrderAmount(decimal amount);
    void MarkOrderAsShipped(int orderId);
}
```

❌ **错误** - 技术性命名:
```csharp
public interface ICustomerOrderView : IWindowView
{
    // ❌ 太通用，不表达业务意图
    void SetDataSource(object data);
    void SetLabel(string text);
    void UpdateGrid();
    void SetColor(Color color);
    void ShowMessage(string msg);
}
```

**为什么？**
- 自文档化: 代码即文档
- 测试可读性: 测试代码变成业务规格说明
- 领域驱动: 反映业务语言，团队沟通更清晰

**示例对比**:
```csharp
// ❌ 技术导向
mockView.Verify(v => v.SetDataSource(It.IsAny<object>()), Times.Once);

// ✅ 业务导向 - 读起来像需求文档
mockView.Verify(v => v.DisplayCustomerOrderHistory(It.IsAny<IEnumerable<Order>>()), Times.Once);
```

### 规则11: 接口应只包含方法，不包含属性

**接口应该只包含方法，没有属性 - 毕竟Presenter通过调用方法驱动用例，而非设置数据**

⚠️ **本框架的调整**: 这条规则在原始MVP中很严格，但在WinForms MVP中，**我们允许单向属性（只有setter）**用于数据注入，因为这更符合WinForms的数据绑定模式。

✅ **推荐** - 优先使用方法:
```csharp
public interface ITaskView : IWindowView
{
    // ✅ 方法 - 表达业务行为
    void DisplayTaskList(IEnumerable<Task> tasks);
    void HighlightSelectedTask(int taskId);
    void ShowTaskCompletionMessage(string taskName);
    void EnableTaskEditing(bool enabled);
}
```

⚠️ **可接受** - 单向属性（仅setter）:
```csharp
public interface ITaskView : IWindowView
{
    // ⚠️ 可接受 - 数据注入用的单向属性
    IEnumerable<Task> Tasks { set; }
    string StatusMessage { set; }
    int TotalCount { set; }
}
```

❌ **避免** - 双向属性（getter + setter）:
```csharp
public interface ITaskView : IWindowView
{
    // ❌ 双向属性违反"Tell, Don't Ask"
    IEnumerable<Task> Tasks { get; set; }

    // ❌ Presenter不应该"询问"View的状态
    bool IsEditMode { get; set; }
    string CurrentFilter { get; set; }
}
```

✅ **正确替代** - 使用事件 + 单向属性:
```csharp
public interface ITaskView : IWindowView
{
    // ✅ Presenter"告诉"View数据
    IEnumerable<Task> Tasks { set; }

    // ✅ View通过事件"通知"Presenter状态变化
    event EventHandler EditModeChanged;
    event EventHandler FilterChanged;
}

// Presenter订阅事件，不询问状态
protected override void OnViewAttached()
{
    View.EditModeChanged += (s, e) => OnEditModeChanged();
    View.FilterChanged += (s, e) => OnFilterChanged();
}
```

### 规则12: 所有数据都应保存在Model中

**MVP组件中的所有数据都应保存在Model中，不应仅作为UI控件的属性存在**

✅ **正确** - 数据在Model中:
```csharp
// Model
public class SearchCriteria
{
    public string Keyword { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string Category { get; set; }
}

// Presenter
private SearchCriteria _searchCriteria;  // ✅ 数据在Model中

private void OnSearch()
{
    // ✅ 从View读取到Model
    _searchCriteria.Keyword = View.Keyword;
    _searchCriteria.DateFrom = View.DateFrom;

    // ✅ 使用Model进行业务操作
    var results = _searchService.Search(_searchCriteria);
    View.Results = results;
}
```

❌ **错误** - 数据只在View中:
```csharp
// Presenter
private void OnSearch()
{
    // ❌ 数据只存在于View的UI控件中
    // Presenter每次都要从View读取，没有Model
    var results = _searchService.Search(
        View.Keyword,      // ❌ 直接从View读取
        View.DateFrom,
        View.DateTo
    );
    View.Results = results;
}

// 问题：
// - 无法在Presenter中操作数据（如验证、转换）
// - 无法跟踪数据变化
// - 难以进行单元测试
```

**Model不仅仅是数据库实体**:
```csharp
// ✅ 搜索View的Model是查询规格对象
public class ProductSearchSpecification
{
    public string Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public bool IsValid() => !string.IsNullOrEmpty(Name) || MinPrice.HasValue;
}

// ✅ 编辑View的Model是领域对象
public class UserEditModel
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    public bool IsNew => UserId == 0;
}
```

### 规则13: View接口方法不应包含UI控件名称引用

**View接口的方法名不应该引用UI控件名（如AddExplorerBarGroup），因为这会让Presenter知道太多View的实现技术**

❌ **错误** - 方法名暴露UI技术:
```csharp
public interface INavigationView : IWindowView
{
    // ❌ 暴露了使用TreeView
    void AddTreeViewNode(string text, int level);

    // ❌ 暴露了使用ExplorerBar
    void AddExplorerBarGroup(string name);

    // ❌ 暴露了使用DataGridView
    void SetDataGridViewRowColor(int row, Color color);

    // ❌ 暴露了使用TabControl
    void AddTabPage(string title);
}
```

✅ **正确** - 方法名反映业务意图:
```csharp
public interface INavigationView : IWindowView
{
    // ✅ 业务语言 - View可以用TreeView、ListBox或任何控件实现
    void AddNavigationItem(string text, int level);

    // ✅ 业务语言 - 不关心用什么控件显示
    void AddModuleGroup(string name);

    // ✅ 业务语言 - View决定如何高亮
    void HighlightRow(int index);

    // ✅ 业务语言 - View决定用Tab还是其他方式
    void ShowSection(string sectionName);
}

// 好处：View实现可以自由变化
public class NavigationForm : Form, INavigationView
{
    // 版本1: 使用TreeView
    public void AddNavigationItem(string text, int level)
    {
        _treeView.Nodes.Add(new TreeNode(text));
    }

    // 版本2: 改用ListView也不影响Presenter
    public void AddNavigationItem(string text, int level)
    {
        _listView.Items.Add(new ListViewItem(text));
    }
}
```

### 规则14: 坚持使用领域命名

**坚持在View方法中使用领域命名（如AddTaskGroupHeader），这使代码更易理解，测试自我描述**

✅ **正确** - 领域驱动命名:
```csharp
public interface IProjectManagementView : IWindowView
{
    // ✅ 反映业务领域
    void AddTaskGroupHeader(string groupName);
    void MarkTaskAsCompleted(int taskId);
    void DisplayProjectMilestones(IEnumerable<Milestone> milestones);
    void HighlightOverdueTasks();
    void ShowResourceAllocationWarning(string resourceName);
}

// 测试代码变成可读的业务规格
[Fact]
public void OnTaskCompleted_ShouldMarkTaskAsCompleted()
{
    // Arrange
    var mockView = new Mock<IProjectManagementView>();
    var presenter = new ProjectManagementPresenter(mockView.Object);

    // Act
    presenter.OnTaskCompleted(taskId: 123);

    // Assert - 读起来像需求文档
    mockView.Verify(v => v.MarkTaskAsCompleted(123), Times.Once);
    mockView.Verify(v => v.HighlightOverdueTasks(), Times.Once);
}
```

❌ **错误** - 技术导向命名:
```csharp
public interface IProjectManagementView : IWindowView
{
    // ❌ 技术细节，不反映业务
    void AddListBoxItem(string text);
    void SetRowBackColor(int index, Color color);
    void UpdateLabel(string text);
    void RefreshGrid();
}

// 测试代码无法表达业务意图
[Fact]
public void OnTaskCompleted_Test()
{
    // 无法理解业务意图
    mockView.Verify(v => v.SetRowBackColor(123, Color.Gray), Times.Once);
    mockView.Verify(v => v.RefreshGrid(), Times.Once);
}
```

---

## 🔄 与本框架的整合

### ViewAction模式 + 规则整合

本框架使用**ViewAction + ActionBinder**模式，这是对原始MVP规则的增强：

| 规则 | 原始MVP | 本框架的ViewAction模式 |
|------|---------|---------------------|
| **规则5** | View通过事件调用Presenter | View通过ActionBinder绑定，无需直接调用 |
| **规则6** | 禁止Presenter返回值 | ActionDispatcher.Dispatch()无返回值 ✅ |
| **规则7** | 通过接口访问View | 同样 ✅ |
| **规则11** | 只用方法，不用属性 | 允许单向属性（setter only）用于数据绑定 |

### 具体实现示例

```csharp
// ===== View Interface =====
public interface IUserEditorView : IWindowView  // 规则1: XxxView后缀
{
    // 规则11调整: 允许单向属性用于数据注入
    string UserName { set; }
    string Email { set; }

    // 规则10: 长而有意义的领域名称
    void DisplayUserDetails(UserModel user);
    void ShowUserSaveSuccessMessage();
    void HighlightValidationErrors(List<string> fieldNames);

    // 规则13: 不暴露UI控件名
    void EnableUserEditing(bool enabled);  // ✅ 而非 EnableTextBoxes()

    // 规则14: 领域命名
    void MarkUserAsActive();  // ✅ 而非 SetLabelColor()

    // 本框架扩展: ActionBinder属性
    ViewActionBinder ActionBinder { get; }  // ViewAction系统集成
}

// ===== Presenter =====
public class UserEditorPresenter : WindowPresenterBase<IUserEditorView>  // 规则2: XxxPresenter后缀
{
    private UserModel _user;  // 规则12: 数据在Model中

    // 规则4: 所有事件处理方法以OnXxx()开头
    protected override void OnViewAttached()
    {
        // 订阅View事件（如果有）
    }

    protected override void RegisterViewActions()
    {
        // 规则6: 方法无返回值
        _dispatcher.Register(UserActions.Save, OnSave);
        _dispatcher.Register(UserActions.Cancel, OnCancel);

        // 规则7: 通过接口访问View
        View.ActionBinder.Bind(_dispatcher);
    }

    // 规则4: OnXxx命名
    private void OnSave()
    {
        // 规则3: Presenter处理用例逻辑
        if (!ValidateUser())  // 业务验证
        {
            View.HighlightValidationErrors(_errors);  // 规则10: 领域命名
            return;
        }

        // 规则12: 操作Model
        _userRepository.Save(_user);

        // 规则7: 通过接口告诉View
        View.ShowUserSaveSuccessMessage();  // 规则10: 领域命名
    }
}

// ===== View Implementation =====
public class UserEditorForm : Form, IUserEditorView  // 规则1: 实现接口
{
    private ViewActionBinder _binder;
    private TextBox _txtUserName;  // 规则8: 私有控件
    private Label _lblStatus;      // 规则8: 私有控件

    // 规则8: 接口成员是public
    public string UserName
    {
        set => _txtUserName.Text = value;  // 单向setter
    }

    // 规则10 + 规则14: 领域命名的方法
    public void DisplayUserDetails(UserModel user)
    {
        // 规则3: View处理UI细节
        _txtUserName.Text = user.Name;
        _txtEmail.Text = user.Email;
        _lblStatus.ForeColor = user.IsActive ? Color.Green : Color.Gray;
    }

    public void ShowUserSaveSuccessMessage()
    {
        // View决定如何显示（Label、MessageBox、Toast等）
        _lblStatus.Text = "User saved successfully!";
        _lblStatus.ForeColor = Color.Green;
    }

    public ViewActionBinder ActionBinder => _binder;

    // 规则8: 私有方法
    private void InitializeActionBindings()
    {
        _binder = new ViewActionBinder();
        _binder.Add(UserActions.Save, _btnSave);
        _binder.Add(UserActions.Cancel, _btnCancel);
    }
}
```

---

## 📊 合规性检查清单

在代码审查时使用此清单：

- [ ] **规则1**: 所有View类/接口都有 `View` 后缀？
- [ ] **规则2**: 所有Presenter都有 `Presenter` 后缀？
- [ ] **规则3**: Presenter只处理用例逻辑，UI细节在View中？
- [ ] **规则4**: Presenter方法以 `OnXxx()` 命名？
- [ ] **规则5**: View到Presenter的调用最小化（或通过ActionBinder）？
- [ ] **规则6**: 没有通过Presenter引用访问Model/Service？
- [ ] **规则7**: Presenter只通过接口访问View？
- [ ] **规则8**: View的public方法都在接口中定义？
- [ ] **规则9**: 只有Presenter访问View（除了初始化）？
- [ ] **规则10**: 接口方法名长而有意义？
- [ ] **规则11**: 接口优先使用方法（属性仅限单向setter）？
- [ ] **规则12**: 数据保存在Model中，不仅在UI控件？
- [ ] **规则13**: 接口方法名不引用UI控件类型？
- [ ] **规则14**: 使用领域驱动命名？

---

## 🎯 总结

这些规则的核心目标是：

1. **清晰的关注点分离**: View = UI细节，Presenter = 业务流程
2. **可测试性**: 通过接口和Mock轻松测试
3. **可维护性**: 长期项目中代码保持清晰
4. **领域驱动**: 代码反映业务语言，团队沟通顺畅
5. **防止滥用**: 规则防止开发者走捷径破坏架构

**记住**:
> "Presenter只处理用例逻辑，不处理View逻辑"
> "Tell, Don't Ask" - 告诉View做什么，不要询问View状态

---

**参考文献**:
- [Design Rules for Model-View-Presenter](http://st-www.cs.illinois.edu/users/smarch/st-docs/mvc.html) - Kjell-Sverre Jerijærvi
- [View to Presenter Communication](https://jeremydmiller.com/) - Jeremy Miller
- [CLAUDE.md](CLAUDE.md) - 本框架架构文档
