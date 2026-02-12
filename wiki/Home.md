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
