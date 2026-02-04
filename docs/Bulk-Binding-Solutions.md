# RadioButton 批量绑定解决方案

## 问题描述

**当有很多 RadioButton 时，每个都要单独绑定非常繁琐：**

```csharp
_binder.Add(QuestionActions.OptionA, _radioA);
_binder.Add(QuestionActions.OptionB, _radioB);
_binder.Add(QuestionActions.OptionC, _radioC);
_binder.Add(QuestionActions.OptionD, _radioD);
_binder.Add(QuestionActions.OptionE, _radioE);
_binder.Add(QuestionActions.OptionF, _radioF);
// ... 可能有 10、20 个选项
```

**真实场景：**
- 问卷调查（5-10 个选项）
- 配置界面（多个互斥选项）
- 向导步骤选择
- 主题/语言选择

---

## 解决方案对比

### 方案 1: AddRange 方法（推荐 ⭐⭐⭐⭐⭐）

**实现：**
```csharp
// 扩展 ViewActionBinder
public void AddRange(IEnumerable<(ViewAction, Component)> bindings)
{
    foreach (var (action, component) in bindings)
    {
        Add(action, component);
    }
}

// 或者更通用的重载
public void AddRange(params (ViewAction action, Component component)[] bindings)
{
    foreach (var (action, component) in bindings)
    {
        Add(action, component);
    }
}
```

**使用：**
```csharp
_binder.AddRange(
    (QuestionActions.OptionA, _radioA),
    (QuestionActions.OptionB, _radioB),
    (QuestionActions.OptionC, _radioC),
    (QuestionActions.OptionD, _radioD),
    (QuestionActions.OptionE, _radioE)
);
```

**优点：**
- ✅ 简洁明了
- ✅ 类型安全（编译时检查）
- ✅ 易于阅读和维护
- ✅ 不需要额外约定

**缺点：**
- ⚠️ 仍需手动列出所有配对

---

### 方案 2: 字典映射 + AddRange（推荐 ⭐⭐⭐⭐）

**使用：**
```csharp
var radioMapping = new Dictionary<ViewAction, RadioButton>
{
    [QuestionActions.OptionA] = _radioA,
    [QuestionActions.OptionB] = _radioB,
    [QuestionActions.OptionC] = _radioC,
    [QuestionActions.OptionD] = _radioD,
    [QuestionActions.OptionE] = _radioE
};

_binder.AddRange(radioMapping.Select(kvp => (kvp.Key, (Component)kvp.Value)));

// 或者提供专门的重载
_binder.AddRange(radioMapping);
```

**扩展方法：**
```csharp
public void AddRange(IDictionary<ViewAction, Component> mappings)
{
    foreach (var kvp in mappings)
    {
        Add(kvp.Key, kvp.Value);
    }
}
```

**优点：**
- ✅ 字典初始化器语法优雅
- ✅ 可以在其他地方复用映射表
- ✅ 易于动态修改

**缺点：**
- ⚠️ 语法稍微复杂一点

---

### 方案 3: Tag 约定（❌ 已删除 - 代码不明确）

**实现：**
```csharp
public void AddByTag(params Component[] components)
{
    foreach (var component in components)
    {
        if (component.Tag is ViewAction action)
        {
            Add(action, component);
        }
        else
        {
            throw new InvalidOperationException(
                $"Component {component.Name} does not have a ViewAction in its Tag property.");
        }
    }
}
```

**使用：**
```csharp
// 在 InitializeComponent 或构造函数中设置 Tag
_radioA.Tag = QuestionActions.OptionA;
_radioB.Tag = QuestionActions.OptionB;
_radioC.Tag = QuestionActions.OptionC;
_radioD.Tag = QuestionActions.OptionD;
_radioE.Tag = QuestionActions.OptionE;

// 批量绑定
_binder.AddByTag(_radioA, _radioB, _radioC, _radioD, _radioE);

// 或者自动查找容器中的所有控件
_binder.AddByTagFromContainer(_optionsPanel);
```

**自动查找实现：**
```csharp
public void AddByTagFromContainer(Control container, bool recursive = false)
{
    foreach (Control control in container.Controls)
    {
        if (control.Tag is ViewAction action && control is Component component)
        {
            Add(action, component);
        }

        if (recursive && control.HasChildren)
        {
            AddByTagFromContainer(control, recursive: true);
        }
    }
}
```

**优点：**
- ✅ 绑定代码极简
- ✅ 支持容器自动查找
- ✅ 适合可视化设计器（可在属性窗口设置 Tag）

**缺点（导致删除的原因）：**
- ❌ **代码不明确** - 无法从绑定代码中看出哪个控件绑定到哪个动作
- ❌ **Tag 是 object 类型** - 完全失去类型安全
- ❌ **需要额外约定** - Tag 必须是 ViewAction，容易出错
- ❌ **运行时错误风险** - 编译时无法检查，只能运行时发现问题
- ❌ **维护困难** - 需要在两个地方查看（Tag 设置 + 绑定调用）

**结论：此方案已从框架中删除。使用 AddRange 替代。**

---

### 方案 4: Fluent API（优雅 ⭐⭐⭐⭐）

**实现：**
```csharp
// 修改 Add 方法返回 this
public ViewActionBinder Add(ViewAction actionKey, params Component[] controls)
{
    _bindings.Add(new ActionBinding(actionKey, controls));
    return this;  // 返回自身支持链式调用
}
```

**使用：**
```csharp
_binder
    .Add(QuestionActions.OptionA, _radioA)
    .Add(QuestionActions.OptionB, _radioB)
    .Add(QuestionActions.OptionC, _radioC)
    .Add(QuestionActions.OptionD, _radioD)
    .Add(QuestionActions.OptionE, _radioE)
    .Bind(_dispatcher);
```

**优点：**
- ✅ 流畅的 API 风格
- ✅ 类型安全
- ✅ 易于阅读

**缺点：**
- ⚠️ 并没有真正减少代码量

---

### 方案 5: 命名约定 + 反射（高级 ⭐⭐⭐）

**实现：**
```csharp
public void AddByNamingConvention(object view, string prefix, ViewAction[] actions)
{
    var viewType = view.GetType();

    for (int i = 0; i < actions.Length; i++)
    {
        var fieldName = $"_{prefix}{(char)('A' + i)}";  // _radioA, _radioB, ...
        var field = viewType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

        if (field != null && field.GetValue(view) is Component component)
        {
            Add(actions[i], component);
        }
    }
}
```

**使用：**
```csharp
// Form 中字段命名遵循约定：_radioA, _radioB, _radioC...
_binder.AddByNamingConvention(
    this,
    "radio",
    new[] {
        QuestionActions.OptionA,
        QuestionActions.OptionB,
        QuestionActions.OptionC,
        QuestionActions.OptionD,
        QuestionActions.OptionE
    }
);
```

**优点：**
- ✅ 极简的绑定代码
- ✅ 适合规律命名的场景

**缺点：**
- ❌ 严重依赖命名约定
- ❌ 反射性能开销
- ❌ 重构不友好（重命名字段会破坏绑定）
- ❌ 不推荐用于生产代码

---

### 方案 6: 数据驱动 + 动态创建（高级 ⭐⭐⭐⭐⭐）

**适用场景：RadioButton 是动态生成的（非设计器创建）**

**实现：**
```csharp
// 定义选项数据
public class QuestionOption
{
    public ViewAction Action { get; set; }
    public string Text { get; set; }
    public string Description { get; set; }
}

// 数据驱动生成
public void CreateAndBindOptions(Panel container, QuestionOption[] options)
{
    int y = 10;
    foreach (var option in options)
    {
        var radio = new RadioButton
        {
            Text = option.Text,
            Location = new Point(10, y),
            Size = new Size(400, 24),
            AutoSize = true
        };

        container.Controls.Add(radio);
        _binder.Add(option.Action, radio);

        y += 30;
    }
}
```

**使用：**
```csharp
var options = new[]
{
    new QuestionOption { Action = QuestionActions.OptionA, Text = "Strongly Disagree" },
    new QuestionOption { Action = QuestionActions.OptionB, Text = "Disagree" },
    new QuestionOption { Action = QuestionActions.OptionC, Text = "Neutral" },
    new QuestionOption { Action = QuestionActions.OptionD, Text = "Agree" },
    new QuestionOption { Action = QuestionActions.OptionE, Text = "Strongly Agree" }
};

CreateAndBindOptions(_optionsPanel, options);
```

**优点：**
- ✅ 数据和 UI 分离
- ✅ 易于从配置/数据库加载
- ✅ 支持动态数量的选项
- ✅ 绑定自动完成

**缺点：**
- ⚠️ 只适用于动态生成的场景
- ⚠️ 失去可视化设计器的便利

---

## 推荐方案（更新版）

### 场景 1: 设计器创建的固定 RadioButton

**推荐：方案 1 (AddRange 元组) 或 方案 2 (AddRange 字典)**

```csharp
public void BindActions(ViewActionDispatcher dispatcher)
{
    _binder = new ViewActionBinder();

    // 方式 A: 直接 AddRange
    _binder.AddRange(
        (ThemeActions.Light, _lightRadio),
        (ThemeActions.Dark, _darkRadio),
        (ThemeActions.Auto, _autoRadio)
    );

    // 方式 B: 字典（适合选项很多时）
    var themeMapping = new Dictionary<ViewAction, RadioButton>
    {
        [ThemeActions.Light] = _lightRadio,
        [ThemeActions.Dark] = _darkRadio,
        [ThemeActions.Auto] = _autoRadio
    };
    _binder.AddRange(themeMapping);

    _binder.Bind(dispatcher);
}
```

---

### 场景 2: 动态生成的 RadioButton

**推荐：方案 6 (数据驱动)**

```csharp
public void BindActions(ViewActionDispatcher dispatcher)
{
    _binder = new ViewActionBinder();

    // 从数据创建并绑定
    var questions = GetSurveyQuestions();  // 从数据库/配置加载
    foreach (var question in questions)
    {
        var radio = CreateRadioButton(question);
        _binder.Add(question.Action, radio);
        _optionsPanel.Controls.Add(radio);
    }

    _binder.Bind(dispatcher);
}
```

---

### 场景 3: 可视化设计器 + 很多选项

**推荐：方案 1 (AddRange 元组)**

```csharp
public void BindActions(ViewActionDispatcher dispatcher)
{
    _binder = new ViewActionBinder();

    // 明确、类型安全的绑定
    _binder.AddRange(
        (QuestionActions.OptionA, _radioA),
        (QuestionActions.OptionB, _radioB),
        (QuestionActions.OptionC, _radioC)
        // ... 虽然代码多一些，但清晰明确
    );

    _binder.Bind(dispatcher);
}
```

**说明：** Tag 方案虽然代码更少，但牺牲了代码的明确性和类型安全，已被删除。

---

## 实现建议

### 为 ViewActionBinder 添加扩展方法

```csharp
namespace WinformsMVP.MVP.ViewActions
{
    public static class ViewActionBinderExtensions
    {
        /// <summary>
        /// 批量添加绑定（元组数组）
        /// </summary>
        public static ViewActionBinder AddRange(
            this ViewActionBinder binder,
            params (ViewAction action, Component component)[] bindings)
        {
            foreach (var (action, component) in bindings)
            {
                binder.Add(action, component);
            }
            return binder;
        }

        /// <summary>
        /// 批量添加绑定（字典）
        /// </summary>
        public static ViewActionBinder AddRange(
            this ViewActionBinder binder,
            IDictionary<ViewAction, Component> mappings)
        {
            foreach (var kvp in mappings)
            {
                binder.Add(kvp.Key, kvp.Value);
            }
            return binder;
        }

        /// <summary>
        /// 通过 Tag 属性批量添加
        /// </summary>
        public static ViewActionBinder AddByTag(
            this ViewActionBinder binder,
            params Component[] components)
        {
            foreach (var component in components)
            {
                if (component.Tag is ViewAction action)
                {
                    binder.Add(action, component);
                }
                else if (component.Tag != null)
                {
                    throw new InvalidOperationException(
                        $"Component '{component.GetType().Name}' Tag is not a ViewAction (Tag: {component.Tag})");
                }
            }
            return binder;
        }

        /// <summary>
        /// 从容器中自动查找并绑定（通过 Tag）
        /// </summary>
        public static ViewActionBinder AddByTagFromContainer(
            this ViewActionBinder binder,
            Control container,
            bool recursive = false)
        {
            foreach (Control control in container.Controls)
            {
                if (control.Tag is ViewAction action && control is Component component)
                {
                    binder.Add(action, component);
                }

                if (recursive && control.HasChildren)
                {
                    binder.AddByTagFromContainer(control, recursive: true);
                }
            }
            return binder;
        }
    }
}
```

---

## 使用示例对比

### 当前方式（繁琐）
```csharp
_binder.Add(QuestionActions.StronglyDisagree, _radio1);
_binder.Add(QuestionActions.Disagree, _radio2);
_binder.Add(QuestionActions.Neutral, _radio3);
_binder.Add(QuestionActions.Agree, _radio4);
_binder.Add(QuestionActions.StronglyAgree, _radio5);
_binder.Bind(dispatcher);
// 5 个选项 = 5 行代码
```

### 使用 AddRange（改进）
```csharp
_binder.AddRange(
    (QuestionActions.StronglyDisagree, _radio1),
    (QuestionActions.Disagree, _radio2),
    (QuestionActions.Neutral, _radio3),
    (QuestionActions.Agree, _radio4),
    (QuestionActions.StronglyAgree, _radio5)
).Bind(dispatcher);
// 5 个选项 = 1 行代码（多行格式化）
```

### 使用 Tag（最简）
```csharp
// 在 InitializeComponent 中设置 Tag
_radio1.Tag = QuestionActions.StronglyDisagree;
_radio2.Tag = QuestionActions.Disagree;
_radio3.Tag = QuestionActions.Neutral;
_radio4.Tag = QuestionActions.Agree;
_radio5.Tag = QuestionActions.StronglyAgree;

// 绑定
_binder.AddByTag(_radio1, _radio2, _radio3, _radio4, _radio5)
       .Bind(dispatcher);

// 或者更简单（如果在同一个容器中）
_binder.AddByTagFromContainer(_optionsPanel)
       .Bind(dispatcher);
// 5 个选项 = 1 行代码
```

---

## 我的最终推荐

### 🏆 最佳实践：

**1. 常规场景（5-10 个选项）**
→ 使用 **AddRange + 元组**

```csharp
_binder.AddRange(
    (ThemeActions.Light, _lightRadio),
    (ThemeActions.Dark, _darkRadio),
    (ThemeActions.Auto, _autoRadio)
);
```

**2. 很多选项（10+ 个）**
→ 使用 **Tag + AddByTagFromContainer**

```csharp
// 设置 Tag（在设计器或代码中）
_binder.AddByTagFromContainer(_optionsPanel);
```

**3. 动态生成**
→ 使用 **数据驱动**

```csharp
foreach (var option in options)
{
    var radio = CreateRadio(option);
    _binder.Add(option.Action, radio);
}
```

---

## 总结

| 方案 | 代码量 | 类型安全 | 代码明确性 | 推荐度 |
|------|--------|---------|-----------|--------|
| AddRange (元组) | ⭐⭐⭐⭐ | ✅ | ✅ **明确** | ⭐⭐⭐⭐⭐ |
| AddRange (字典) | ⭐⭐⭐⭐ | ✅ | ✅ **明确** | ⭐⭐⭐⭐ |
| ~~Tag + 容器查找~~ | ⭐⭐⭐⭐⭐ | ❌ | ❌ **不明确** | ❌ **已删除** |
| Fluent API | ⭐⭐⭐ | ✅ | ✅ | ⭐⭐⭐ |
| 数据驱动 | ⭐⭐⭐⭐⭐ | ✅ | ✅ | ⭐⭐⭐⭐⭐ (动态场景) |
| 反射 + 命名约定 | ⭐⭐⭐⭐⭐ | ❌ | ❌ | ⭐⭐ (不推荐) |

**最终实现：只提供 AddRange 扩展方法（元组和字典两个重载）。**

**删除原因：** Tag 方案虽然代码量最少，但代码不明确，无法一眼看出绑定关系，违反了"代码即文档"的原则。
