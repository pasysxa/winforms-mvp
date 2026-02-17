# Complex Interaction Demo - Order Management System

## 📖 Overview

This demo showcases a **real-world complex scenario** where a main window (Form) coordinates two UserControls, all using the MVP pattern. It demonstrates:

- ✅ Main window with MVP pattern (`OrderManagementForm` + `OrderManagementPresenter`)
- ✅ Two UserControls with MVP pattern (`ProductSelectorView` + `OrderSummaryView`)
- ✅ **Parent-child Presenter coordination**
- ✅ **Cross-component communication** via events
- ✅ **Data flow** between UserControls through the parent Presenter
- ✅ Complete unit test coverage

## 🏗️ Architecture

```
┌───────────────────────────────────────────────────────────────────────┐
│                      OrderManagementForm (Main Window)                │
│                                                                       │
│  ┌─────────────────────────────────┐  ┌───────────────────────────┐ │
│  │   ProductSelectorView (UC)      │  │  OrderSummaryView (UC)    │ │
│  │   - List products               │  │  - Display order items    │ │
│  │   - Select quantity             │  │  - Show total             │ │
│  │   - Add to order button         │  │  - Remove item button     │ │
│  └─────────────────────────────────┘  └───────────────────────────┘ │
│                                                                       │
│  [Current Total: $0.00]               [Save Order] [Clear Order]     │
│  Status: Ready to take orders                                        │
└───────────────────────────────────────────────────────────────────────┘

                                    ↓

┌───────────────────────────────────────────────────────────────────────┐
│                          Presenter Layer                              │
│                                                                       │
│                  OrderManagementPresenter (Parent)                    │
│                            ┌───┴───┐                                  │
│                            │ Coordinates                              │
│                            │ Events                                   │
│                            │ State                                    │
│                      ┌─────┴─────┬──────┐                            │
│                      ▼           ▼                                    │
│        ProductSelectorPresenter  OrderSummaryPresenter                │
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘
```

## 🔄 Component Interactions

### 1. Initialization Flow

```
Program.Main()
    │
    ├─→ Create OrderManagementForm (main window)
    │   └─→ Contains ProductSelectorView + OrderSummaryView (child controls)
    │
    ├─→ Create ProductSelectorPresenter(productSelectorView)
    │   └─→ Attaches to ProductSelectorView
    │
    ├─→ Create OrderSummaryPresenter(orderSummaryView)
    │   └─→ Attaches to OrderSummaryView
    │
    └─→ Create OrderManagementPresenter(
            productSelectorPresenter,
            orderSummaryPresenter,
            products)
        │
        ├─→ Subscribe to child events:
        │   • ProductAdded (from ProductSelector)
        │   • TotalChanged (from OrderSummary)
        │   • ItemRemoved (from OrderSummary)
        │
        └─→ Initialize():
            └─→ Load products into ProductSelector
```

### 2. ProductSelector → OrderSummary Communication

```
User selects product + quantity → Clicks "Add to Order"
    │
    ├─→ ProductSelectorView.btnAddToOrder_Click
    │       │
    │       └─→ Dispatch(ProductSelectorActions.AddToOrder)
    │               │
    │               └─→ ProductSelectorPresenter.OnAddToOrder()
    │                       │
    │                       ├─→ Validate selection and quantity
    │                       └─→ Raise ProductAdded event
    │
    ├─→ OrderManagementPresenter.OnProductAdded() ← (event handler)
    │       │
    │       ├─→ Forward to OrderSummaryPresenter.AddProduct()
    │       └─→ Update status message
    │
    └─→ OrderSummaryPresenter.AddProduct()
            │
            ├─→ Add/update item in order list
            ├─→ Update view (items + total)
            └─→ Raise TotalChanged event
```

### 3. OrderSummary → Main Window Communication

```
OrderSummary total changes
    │
    └─→ OrderSummaryPresenter raises TotalChanged event
            │
            └─→ OrderManagementPresenter.OnTotalChanged()
                    │
                    ├─→ Update MainView.CurrentTotal
                    ├─→ Update status message
                    └─→ Trigger CanExecute refresh (for Save button)
```

### 4. Main Window → OrderSummary Communication

```
User clicks "Clear Order"
    │
    └─→ OrderManagementPresenter.OnClearOrder()
            │
            └─→ Dispatch action to child:
                orderSummaryPresenter.Dispatcher.Dispatch(
                    OrderSummaryActions.ClearAll)
                    │
                    └─→ OrderSummaryPresenter.OnClearAll()
                            │
                            ├─→ Clear all items
                            ├─→ Update view
                            └─→ Raise TotalChanged event
```

## 📂 File Structure

```
ComplexInteractionDemo/
│
├── Models/
│   ├── Product.cs                      # Product entity (ICloneable)
│   └── OrderItem.cs                    # Order line item
│
├── ProductSelector/
│   ├── IProductSelectorView.cs         # View interface
│   ├── ProductSelectorPresenter.cs     # Presenter (ControlPresenterBase)
│   ├── ProductSelectorView.cs          # UserControl implementation
│   └── ProductSelectorView.Designer.cs
│
├── OrderSummary/
│   ├── IOrderSummaryView.cs            # View interface
│   ├── OrderSummaryPresenter.cs        # Presenter (ControlPresenterBase)
│   ├── OrderSummaryView.cs             # UserControl implementation
│   └── OrderSummaryView.Designer.cs
│
├── OrderManagement/
│   ├── IOrderManagementView.cs         # Main view interface
│   ├── OrderManagementPresenter.cs     # Main presenter (WindowPresenterBase)
│   ├── OrderManagementForm.cs          # Main Form
│   └── OrderManagementForm.Designer.cs
│
├── Program.cs                          # Entry point
└── README.md                           # This file

Tests/
└── ComplexInteractionDemo/
    ├── ProductSelectorPresenterTests.cs    # 7 tests
    ├── OrderSummaryPresenterTests.cs       # 12 tests
    └── OrderManagementPresenterTests.cs    # 8 tests (27 total)
```

## 🎯 Key Design Patterns

### 1. Parent Presenter Coordinates Children

```csharp
public class OrderManagementPresenter : WindowPresenterBase<IOrderManagementView>
{
    private readonly ProductSelectorPresenter _productSelectorPresenter;
    private readonly OrderSummaryPresenter _orderSummaryPresenter;

    public OrderManagementPresenter(
        ProductSelectorPresenter productSelectorPresenter,
        OrderSummaryPresenter orderSummaryPresenter,
        IList<Product> availableProducts)
    {
        // Parent holds references to child presenters
        _productSelectorPresenter = productSelectorPresenter;
        _orderSummaryPresenter = orderSummaryPresenter;
        _availableProducts = availableProducts;
    }

    protected override void OnViewAttached()
    {
        // Subscribe to child events to coordinate behavior
        _productSelectorPresenter.View.ProductAdded += OnProductAdded;
        _orderSummaryPresenter.View.TotalChanged += OnTotalChanged;
    }
}
```

### 2. Event-Based Communication

```csharp
// Child raises domain event
public interface IProductSelectorView : IViewBase
{
    event EventHandler<ProductAddedEventArgs> ProductAdded;
}

// Parent handles coordination
private void OnProductAdded(object sender, ProductAddedEventArgs e)
{
    // Forward to another child
    _orderSummaryPresenter.AddProduct(e.Product, e.Quantity);

    // Update main view
    View.StatusMessage = $"Added {e.Quantity} x {e.Product.Name}";
}
```

### 3. Cross-Presenter Communication

```csharp
// Parent can dispatch actions to child presenters
private void OnClearOrder()
{
    // Trigger child action through its dispatcher
    _orderSummaryPresenter.Dispatcher.Dispatch(
        OrderSummaryActions.ClearAll);
}
```

### 4. ViewAction System in UserControls

```csharp
// UserControl Presenter uses ControlPresenterBase
public class ProductSelectorPresenter : ControlPresenterBase<IProductSelectorView>
{
    public ProductSelectorPresenter(IProductSelectorView view) : base(view)
    {
        // View is injected via constructor
    }

    protected override void RegisterViewActions()
    {
        Dispatcher.Register(
            ProductSelectorActions.AddToOrder,
            OnAddToOrder,
            canExecute: () => View.HasSelection && View.HasValidQuantity);

        // Framework automatically binds View.ActionBinder
    }
}
```

## 🧪 Testing

### Unit Test Coverage

**ProductSelectorPresenterTests** (7 tests):
- ✅ Default quantity initialization
- ✅ Product loading
- ✅ Valid product addition
- ✅ Validation errors (no selection, invalid quantity, stock exceeded)
- ✅ Quantity reset after add

**OrderSummaryPresenterTests** (12 tests):
- ✅ Adding products to order
- ✅ Combining quantities for same product
- ✅ Total calculation
- ✅ TotalChanged event
- ✅ Removing items
- ✅ ItemRemoved event
- ✅ Clear all functionality
- ✅ Read-only order access

**OrderManagementPresenterTests** (8 tests):
- ✅ Initialization flow
- ✅ Product addition coordination
- ✅ Total update coordination
- ✅ Save order with validation
- ✅ Clear order functionality
- ✅ Complete order workflow

### Running Tests

```bash
dotnet test src/WindowsMVP.Samples.Tests/WindowsMVP.Samples.Tests.csproj --filter "FullyQualifiedName~ComplexInteractionDemo"
```

Expected output:
```
Passed!  - Failed:     0, Passed:    27, Skipped:     0, Total:    27
```

## 🚀 Running the Demo

### Option 1: Standalone Run

```csharp
// In Program.cs
[STAThread]
static void Main()
{
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    var products = CreateSampleProducts();
    var form = new OrderManagementForm();

    var productSelectorPresenter = new ProductSelectorPresenter(form.ProductSelectorView);
    var orderSummaryPresenter = new OrderSummaryPresenter(form.OrderSummaryView);

    var mainPresenter = new OrderManagementPresenter(
        productSelectorPresenter,
        orderSummaryPresenter,
        products);

    mainPresenter.AttachView(form);
    mainPresenter.Initialize();

    Application.Run(form);
}
```

### Option 2: Build and Run

```bash
# Build
dotnet build src/winforms-mvp.sln

# Run the demo
dotnet run --project src/WinformsMVP.Samples/WinformsMVP.Samples.csproj
# Then select "Complex Interaction Demo" from the menu
```

## 📚 What You'll Learn

### 1. **Structuring Complex UIs**
- How to break down complex screens into reusable UserControls
- Each UserControl has its own MVP triad
- Parent Presenter coordinates child Presenters

### 2. **Event-Driven Communication**
- Child → Parent: Domain events (`ProductAdded`, `TotalChanged`)
- Parent → Child: Method calls or action dispatch
- Sibling communication: Always through parent coordinator

### 3. **Dependency Injection**
- UserControl Presenters use constructor injection
- Child Views injected into child Presenters
- Child Presenters injected into parent Presenter

### 4. **Testing Isolation**
- Each Presenter can be tested independently
- Mock child Presenters/Views for parent tests
- Event-based communication is easy to test

### 5. **CanExecute Management**
- Parent coordinates state changes
- Child events trigger `RaiseCanExecuteChanged()` in parent
- Buttons auto-enable/disable based on application state

## 🎓 Best Practices Demonstrated

✅ **Separation of Concerns**: Each component has clear responsibilities

✅ **Loose Coupling**: Components communicate via events and interfaces

✅ **Testability**: 100% unit test coverage without UI automation

✅ **Reusability**: UserControls can be reused in different contexts

✅ **Scalability**: Easy to add new UserControls or modify existing ones

✅ **Maintainability**: Changes isolated to specific components

## 🔍 Common Patterns

### Pattern 1: Child-to-Parent Notification

```csharp
// Child raises domain event
View.ProductAdded?.Invoke(this, new ProductAddedEventArgs(product, quantity));

// Parent handles coordination
_productSelectorPresenter.View.ProductAdded += OnProductAdded;
```

### Pattern 2: Parent-to-Child Command

```csharp
// Parent calls child method
_orderSummaryPresenter.AddProduct(product, quantity);

// Or dispatches action
_orderSummaryPresenter.Dispatcher.Dispatch(OrderSummaryActions.ClearAll);
```

### Pattern 3: State Synchronization

```csharp
// Child updates state and notifies
View.TotalChanged?.Invoke(this, new TotalChangedEventArgs(oldTotal, newTotal));

// Parent synchronizes main view
private void OnTotalChanged(object sender, TotalChangedEventArgs e)
{
    View.CurrentTotal = e.NewTotal;
    Dispatcher.RaiseCanExecuteChanged();  // Update button states
}
```

## 💡 When to Use This Pattern

✅ **Use When:**
- Building complex forms with multiple sections
- Sections need to communicate and coordinate
- Each section has significant logic worth separating
- Want to reuse sections across different forms
- Need comprehensive testing

❌ **Avoid When:**
- Simple forms with just a few controls
- No need for component reuse
- Minimal interaction between sections

## 📖 Related Documentation

- [MVP Design Rules](../../../wiki/MVP-Design-Rules.md) - Core MVP principles
- [CLAUDE.md](../../../CLAUDE.md) - Complete framework guide
- [ControlPresenterBase](../../../CLAUDE.md#controlpresenterbase) - UserControl presenter pattern
- [ViewAction System](../../../CLAUDE.md#viewaction-system) - Command binding

---

**This demo is production-ready code demonstrating enterprise-level MVP architecture in WinForms applications.**
