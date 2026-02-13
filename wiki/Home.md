# WinForms MVP Framework Wiki

Welcome to the **WinForms MVP Framework** wiki! This documentation provides comprehensive guides and examples for building clean, testable WinForms applications using the MVP (Model-View-Presenter) pattern.

## 📖 Available Pages

### 📋 Design Principles

- **[MVP Design Rules](MVP-Design-Rules)** 📐 - Complete guide to 14 essential MVP design rules
  - Naming conventions (View, Presenter)
  - Responsibility separation
  - Interface design best practices
  - Automated enforcement with Roslyn analyzers

### 🎯 Example Applications (Click to view detailed tutorials)

#### Advanced Examples ⭐
- **[Master-Detail Pattern](Example-Master-Detail)** 👥 - Parent-child data relationships (Customers → Orders)
  - Coordinated UI updates across master and detail views
  - Real-time calculations and cascading deletes
  - State-driven CanExecute patterns

- **[Complex Validation](Example-Validation)** ✅ - Real-time multi-field validation
  - Field-level, cross-field, and pattern validation
  - Visual error feedback with colored fields
  - Testing validation logic

- **[Async Operations](Example-Async-Operations)** ⚡ - Proper async/await patterns in MVP
  - Progress tracking with cancellation support
  - Thread-safe UI updates
  - Error handling in async methods

## 🚀 Quick Start

### Running the Examples

All examples are located in the sample application:

1. Open `src/winforms-mvp.sln` in Visual Studio
2. Set `WinformsMVP.Samples` as startup project
3. Run the application (F5)
4. The main form will show all available demos

### Key Framework Concepts

**MVP Pattern**: Separation of UI (View) from business logic (Presenter)
- **View Interface**: Defines what the View can do (no UI types like Button, TextBox)
- **Presenter**: Contains all business logic, no knowledge of WinForms
- **Form Implementation**: Implements View interface, owns UI controls

**ViewAction System**: WPF-style command binding for WinForms
- Declarative action binding via `ActionBinder` property
- Automatic enable/disable based on `CanExecute` predicates
- No more manual button click handlers!

**Service Abstraction**: Never use `MessageBox.Show()` directly
- `IMessageService` for dialogs and messages
- `IDialogProvider` for file/folder dialogs
- Fully testable with mock services

## 📚 Documentation

### Framework Architecture
See **[CLAUDE.md](https://github.com/pasysxa/winforms-mvp/blob/master/CLAUDE.md)** in the repository for comprehensive documentation:
- ViewAction System deep dive
- Service layer design
- Navigation system
- Change tracking
- Best practices and anti-patterns

### README
See **[README.md](https://github.com/pasysxa/winforms-mvp/blob/master/README.md)** for:
- Feature overview with code examples
- Architecture diagrams (Mermaid)
- Quick start guide
- All example descriptions

## 💡 Common Questions

**Q: How do I show a MessageBox from a Presenter?**

A: Use `IMessageService` instead of `MessageBox.Show()`:
```csharp
// ❌ Wrong - Direct UI dependency
MessageBox.Show("Saved!", "Success");

// ✅ Correct - Service abstraction
Messages.ShowInfo("Saved!", "Success");
```

**Q: How do I bind buttons to actions?**

A: Use the `ActionBinder` property pattern:
```csharp
// In Presenter
protected override void RegisterViewActions()
{
    _dispatcher.Register(CommonActions.Save, OnSave,
        canExecute: () => View.HasUnsavedChanges);

    View.ActionBinder.Bind(_dispatcher);
}

// In Form (private UI controls)
private void InitializeActionBindings()
{
    _binder = new ViewActionBinder();
    _binder.Add(CommonActions.Save, _saveButton);
}
```

**Q: How do I validate form input?**

A: See the **[Validation Example](Example-Validation)** for comprehensive patterns including:
- Real-time validation as user types
- Cross-field validation (password confirmation)
- Pattern matching (email, phone)
- Visual error feedback

**Q: How do I handle async operations?**

A: See the **[Async Operations Example](Example-Async-Operations)** for:
- Proper async/await in Presenters
- Progress tracking with `IProgress<T>`
- Cancellation with `CancellationToken`
- Thread-safe UI updates

## 🔗 External Links

- **[GitHub Repository](https://github.com/pasysxa/winforms-mvp)** - Source code
- **[CLAUDE.md](https://github.com/pasysxa/winforms-mvp/blob/master/CLAUDE.md)** - Architecture documentation
- **[Sample Code](https://github.com/pasysxa/winforms-mvp/tree/master/src/WinformsMVP.Samples)** - All examples

## 📦 Project Structure

```
src/
├── WinformsMVP/                    # Core framework
│   ├── MVP/Presenters/             # Presenter base classes
│   ├── MVP/Views/                  # View interfaces
│   ├── MVP/ViewActions/            # ViewAction system
│   ├── Services/                   # Service abstractions
│   └── Common/                     # Utilities (ChangeTracker, etc.)
│
├── WinformsMVP.Samples/            # Example applications
│   ├── MasterDetailDemo/           # Master-Detail pattern
│   ├── ValidationDemo/             # Complex validation
│   ├── AsyncDemo/                  # Async operations
│   ├── ToDoDemo/                   # CRUD operations
│   ├── NavigatorDemo/              # Window navigation
│   └── ... more examples
│
└── WinformsMVP.Samples.Tests/      # Unit tests (41 tests)
```

## 🤝 Contributing

Found an error or want to improve the documentation?

1. Fork the repository
2. Make your changes
3. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

---

**[⬆ Back to Top](#winforms-mvp-framework-wiki)**
