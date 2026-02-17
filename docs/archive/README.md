# Archived Documentation

This directory contains obsolete documentation that has been superseded by newer patterns.

## BindActions Pattern (Archived)

The following documents describe the obsolete `BindActions()` method pattern:

- `BindActions-Caller-Analysis.md`
- `BindActions-Final-Analysis.md`
- `BindActions-Location-Analysis.md`
- `BindActions-Reality-Check.md`

### Why Archived?

The `BindActions(ViewActionDispatcher dispatcher)` method pattern has been replaced by the **ActionBinder Property Pattern**.

**Old Pattern (Deprecated):**
```csharp
public interface IMyView : IWindowView
{
    void BindActions(ViewActionDispatcher dispatcher);  // ❌ Obsolete
}

public class MyPresenter : WindowPresenterBase<IMyView>
{
    protected override void OnViewAttached()
    {
        View.BindActions(Dispatcher);  // ❌ Manual binding
    }
}
```

**New Pattern (Current):**
```csharp
public interface IMyView : IWindowView
{
    ViewActionBinder ActionBinder { get; }  // ✅ Property pattern
}

public class MyPresenter : WindowPresenterBase<IMyView>
{
    protected override void RegisterViewActions()
    {
        Dispatcher.Register(MyActions.Save, OnSave);
        // Framework automatically calls View.ActionBinder?.Bind(_dispatcher)
    }
}
```

### Benefits of New Pattern

1. **Automatic Binding** - Framework handles binding lifecycle
2. **Property instead of Method** - Prevents accidental code execution in tests
3. **Cleaner Architecture** - Less boilerplate in presenters
4. **Consistent with WPF** - Similar to ICommand pattern

For current documentation, see:
- `CLAUDE.md` - Main project guide
- `wiki/MVP-Design-Rules.md` - MVP design principles
- Sample code in `src/WinformsMVP.Samples/`
