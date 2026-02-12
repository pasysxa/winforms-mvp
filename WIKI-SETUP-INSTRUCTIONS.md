# 📖 Wiki设置说明 - 仅需2步！

由于GitHub需要先创建第一个wiki页面才能初始化wiki仓库，请按以下简单步骤操作：

## ✅ 第1步：创建第一个Wiki页面（仅需1次）

1. **打开浏览器，访问你的仓库wiki页面**：
   ```
   https://github.com/pasysxa/winforms-mvp/wiki
   ```

2. **点击 "Create the first page" 按钮**

3. **复制下面的内容，粘贴到编辑框中**：

---

**复制下面的所有内容** ⬇️

```markdown
# WinForms MVP Framework Wiki

Welcome to the **WinForms MVP Framework** wiki! This documentation provides comprehensive guides and examples for building clean, testable WinForms applications using the MVP (Model-View-Presenter) pattern.

## 📖 Table of Contents

### Getting Started
- [Home](Home) - You are here
- [Quick Start Guide](Quick-Start-Guide)
- [Core Concepts](Core-Concepts)
- [Architecture Overview](Architecture-Overview)

### Core Framework Features
- [ViewAction System](ViewAction-System) - WPF-style command binding for WinForms
- [Service Layer](Service-Layer) - IMessageService, IDialogProvider, and more
- [Window Navigation](Window-Navigation) - Modal/non-modal window management
- [Change Tracking](Change-Tracking) - Edit/cancel support for forms
- [Dependency Injection](Dependency-Injection) - DI patterns and best practices

### Example Applications

#### Basic Examples
- [ViewAction Example](Example-ViewAction) - Basic ViewAction usage
- [CheckBox Demo](Example-CheckBox) - CheckBox and RadioButton binding
- [Bulk Binding Demo](Example-Bulk-Binding) - Efficient multi-control binding

#### Intermediate Examples
- [ToDo CRUD Demo](Example-ToDo) - Full CRUD operations with state management
- [Navigator Demo](Example-Navigator) - Window lifecycle and navigation patterns
- [MessageBox Positioning](Example-MessageBox) - Native MessageBox with positioning

#### Advanced Examples
- [Master-Detail Pattern](Example-Master-Detail) 👥 **NEW!** - Parent-child data relationships
- [Complex Validation](Example-Validation) ✅ **NEW!** - Real-time multi-field validation
- [Async Operations](Example-Async-Operations) ⚡ **NEW!** - Async/await patterns

#### Pattern Comparisons
- [MVP Pattern Comparison](Example-MVP-Comparison) - Passive View vs Supervising Controller

### Best Practices
- [MVP Principles](Best-Practices-MVP) - Maintaining clean separation of concerns
- [Testing Presenters](Best-Practices-Testing) - Unit testing strategies
- [Error Handling](Best-Practices-Error-Handling) - Robust error handling patterns
- [Performance Tips](Best-Practices-Performance) - Optimizing WinForms MVP apps

### Advanced Topics
- [Custom ViewAction Strategies](Advanced-Custom-Actions) - Extending the ViewAction system
- [Async Validation](Advanced-Async-Validation) - Server-side validation patterns
- [Multi-Window Coordination](Advanced-Multi-Window) - Complex window interactions
- [Legacy Code Migration](Advanced-Legacy-Migration) - Migrating existing WinForms apps

## 🚀 Quick Links

### Most Popular Pages
1. [ViewAction System](ViewAction-System) - Learn the command binding pattern
2. [Master-Detail Example](Example-Master-Detail) - Parent-child data relationships
3. [Validation Example](Example-Validation) - Complex validation patterns
4. [Async Operations](Example-Async-Operations) - Proper async/await in MVP

### Common Questions
- **Q: How do I show a MessageBox from a Presenter?**
  A: Use [IMessageService](Service-Layer#imessageservice) instead of `MessageBox.Show()`

- **Q: How do I bind buttons to actions?**
  A: Use the [ViewActionBinder](ViewAction-System#viewactionbinder) property pattern

- **Q: How do I validate form input?**
  A: Check out the [Validation Example](Example-Validation)

- **Q: How do I handle async operations?**
  A: See the [Async Operations Example](Example-Async-Operations)

## 🤝 Contributing

Found an error or want to improve the documentation? Contributions are welcome!

1. Fork the repository
2. Make your changes
3. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

---

**[⬆ Back to Top](#winforms-mvp-framework-wiki)**
```

---

4. **页面标题保持默认 "Home"**

5. **点击 "Save Page" 按钮**

完成！第一个wiki页面已创建 ✅

---

## ✅ 第2步：自动部署其他Wiki页面

第一个页面创建后，wiki仓库就已初始化。现在运行部署脚本即可自动部署所有wiki页面：

**在PowerShell中运行**：
```powershell
cd wiki
.\deploy-wiki.ps1
```

**或在Git Bash中运行**：
```bash
cd wiki
./deploy-wiki.sh
```

脚本会自动：
- 克隆wiki仓库
- 复制所有wiki页面
- 提交并推送到GitHub
- 打开浏览器查看结果

---

## 🎉 完成！

访问你的wiki：https://github.com/pasysxa/winforms-mvp/wiki

你会看到：
- ✅ Home主页（刚刚手动创建的）
- ✅ Master-Detail Pattern示例（脚本自动部署）
- ✅ Complex Validation示例（脚本自动部署）
- ✅ Async Operations示例（脚本自动部署）

---

## 💡 提示

如果你想完全手动创建（不用脚本），可以通过GitHub网页界面逐个创建页面：

1. 点击 "New Page"
2. 复制 `wiki/Example-Master-Detail.md` 的内容
3. 页面标题填写：`Example-Master-Detail`
4. 保存

重复以上步骤创建其他页面。

但使用脚本更快更方便！😊
