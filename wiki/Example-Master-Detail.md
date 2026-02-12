# Master-Detail Pattern Example

The Master-Detail pattern is one of the most common UI patterns in business applications. This example demonstrates how to implement coordinated parent-child data relationships using the WinForms MVP Framework.

## 📁 Location

`src/WinformsMVP.Samples/MasterDetailDemo/`

## 🎯 What You'll Learn

- Managing parent-child (Master-Detail) data relationships
- Coordinating UI updates across multiple views
- Implementing cascading deletes with confirmation
- Real-time calculations based on selection
- State-driven CanExecute for context-sensitive actions
- Clean separation between data presentation and business logic

## 🏗️ Architecture

### Files Structure

```
MasterDetailDemo/
├── IMasterDetailView.cs        # View interface
├── MasterDetailPresenter.cs    # Business logic
├── MasterDetailForm.cs         # WinForms UI implementation
└── CustomerModel.cs            # Data models (in IMasterDetailView.cs)
    OrderModel.cs
```

### Data Model

```csharp
public class CustomerModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}

public class OrderModel
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
}
```

## 📝 Implementation Walkthrough

### 1. View Interface - IMasterDetailView

The View interface exposes only data and state, never UI controls:

```csharp
public interface IMasterDetailView : IWindowView
{
    // Master data
    IEnumerable<CustomerModel> Customers { get; set; }
    CustomerModel SelectedCustomer { get; }
    bool HasSelectedCustomer { get; }

    // Detail data
    IEnumerable<OrderModel> Orders { get; set; }
    OrderModel SelectedOrder { get; }
    bool HasSelectedOrder { get; }

    // Calculated display
    string TotalAmount { set; }

    // Events for selection changes
    event EventHandler CustomerSelectionChanged;
    event EventHandler OrderSelectionChanged;
}
```

**Key Design Points:**
- ✅ No UI types exposed (no ListBox, DataGridView, etc.)
- ✅ Separate properties for master and detail data
- ✅ Boolean properties for selection state (used in CanExecute)
- ✅ Events notify Presenter when selection changes

### 2. Presenter - MasterDetailPresenter

The Presenter handles all business logic and coordinates the master-detail relationship:

```csharp
public class MasterDetailPresenter : WindowPresenterBase<IMasterDetailView>
{
    private List<CustomerModel> _customers;
    private Dictionary<int, List<OrderModel>> _ordersByCustomer;

    protected override void OnViewAttached()
    {
        // Subscribe to selection changes
        View.CustomerSelectionChanged += OnCustomerSelectionChanged;
        View.OrderSelectionChanged += OnOrderSelectionChanged;
    }

    protected override void RegisterViewActions()
    {
        // Master (Customer) actions
        _dispatcher.Register(
            MasterDetailActions.AddCustomer,
            OnAddCustomer);

        _dispatcher.Register(
            MasterDetailActions.EditCustomer,
            OnEditCustomer,
            canExecute: () => View.HasSelectedCustomer);  // State-driven

        _dispatcher.Register(
            MasterDetailActions.DeleteCustomer,
            OnDeleteCustomer,
            canExecute: () => View.HasSelectedCustomer);

        // Detail (Order) actions
        _dispatcher.Register(
            MasterDetailActions.AddOrder,
            OnAddOrder,
            canExecute: () => View.HasSelectedCustomer);  // Need customer first

        _dispatcher.Register(
            MasterDetailActions.EditOrder,
            OnEditOrder,
            canExecute: () => View.HasSelectedOrder);

        _dispatcher.Register(
            MasterDetailActions.DeleteOrder,
            OnDeleteOrder,
            canExecute: () => View.HasSelectedOrder);

        View.ActionBinder.Bind(_dispatcher);
    }

    private void OnCustomerSelectionChanged(object sender, EventArgs e)
    {
        // Update detail view when master selection changes
        var customer = View.SelectedCustomer;
        if (customer != null && _ordersByCustomer.TryGetValue(customer.Id, out var orders))
        {
            View.Orders = orders;
            CalculateAndDisplayTotal(orders);
        }
        else
        {
            View.Orders = Enumerable.Empty<OrderModel>();
            View.TotalAmount = "$0.00";
        }

        // UI state changed - update button states
        _dispatcher.RaiseCanExecuteChanged();
    }

    private void CalculateAndDisplayTotal(IEnumerable<OrderModel> orders)
    {
        var total = orders.Sum(o => o.Amount);
        View.TotalAmount = $"${total:N2}";
    }
}
```

**Key Implementation Patterns:**

1. **Selection Change Handler**
   - Updates detail view when master selection changes
   - Calculates totals automatically
   - Calls `RaiseCanExecuteChanged()` to update button states

2. **State-Driven CanExecute**
   - Edit/Delete Customer: enabled only when customer is selected
   - Add Order: enabled only when customer is selected (need parent first)
   - Edit/Delete Order: enabled only when order is selected

3. **Cascading Behavior**
   - Deleting a customer also deletes all their orders
   - Confirmation dialog prevents accidental deletion

### 3. Form Implementation - MasterDetailForm

The Form implements the View interface and handles all UI details internally:

```csharp
public partial class MasterDetailForm : Form, IMasterDetailView
{
    private ListBox _customerListBox;   // Private - not exposed!
    private ListBox _orderListBox;      // Private - not exposed!
    private ViewActionBinder _binder;

    public MasterDetailForm()
    {
        InitializeComponent();
        InitializeActionBindings();
    }

    private void InitializeActionBindings()
    {
        _binder = new ViewActionBinder();

        // Bind buttons to actions
        _binder.Add(MasterDetailActions.AddCustomer, _addCustomerButton);
        _binder.Add(MasterDetailActions.EditCustomer, _editCustomerButton);
        _binder.Add(MasterDetailActions.DeleteCustomer, _deleteCustomerButton);
        _binder.Add(MasterDetailActions.AddOrder, _addOrderButton);
        _binder.Add(MasterDetailActions.EditOrder, _editOrderButton);
        _binder.Add(MasterDetailActions.DeleteOrder, _deleteOrderButton);
        _binder.Add(MasterDetailActions.Refresh, _refreshButton);
    }

    public ViewActionBinder ActionBinder => _binder;

    public IEnumerable<CustomerModel> Customers
    {
        get => _customerListBox.DataSource as IEnumerable<CustomerModel>;
        set => _customerListBox.DataSource = value?.ToList();
    }

    public CustomerModel SelectedCustomer =>
        _customerListBox.SelectedItem as CustomerModel;

    public bool HasSelectedCustomer =>
        _customerListBox.SelectedItem != null;

    // Similar pattern for Orders...

    public event EventHandler CustomerSelectionChanged
    {
        add => _customerListBox.SelectedIndexChanged += value;
        remove => _customerListBox.SelectedIndexChanged -= value;
    }
}
```

**Key Points:**
- All UI controls are **private** - never exposed through interface
- Events are forwarded from ListBox events to View interface events
- Data binding uses `DataSource` for automatic synchronization

## 🎮 User Experience Flow

1. **Application starts** → Presenter loads sample customer data
2. **User selects a customer** →
   - `CustomerSelectionChanged` event fires
   - Presenter loads that customer's orders
   - Presenter calculates and displays total
   - Edit/Delete Customer buttons become enabled
   - Add Order button becomes enabled
3. **User clicks "Delete Customer"** →
   - Confirmation dialog appears (via IMessageService)
   - If confirmed, customer and all orders are deleted
   - View is refreshed
4. **User selects an order** →
   - `OrderSelectionChanged` event fires
   - Edit/Delete Order buttons become enabled

## 🧪 Testing Example

```csharp
[Fact]
public void OnCustomerSelectionChanged_ShouldLoadOrders()
{
    // Arrange
    var mockView = new MockMasterDetailView();
    var mockMessages = new MockMessageService();
    var presenter = new MasterDetailPresenter();
    presenter.AttachView(mockView);
    presenter.Initialize();

    // Simulate selecting a customer
    mockView.SelectedCustomer = mockView.Customers.First();

    // Act
    mockView.RaiseCustomerSelectionChanged();

    // Assert
    Assert.NotEmpty(mockView.Orders);
    Assert.NotNull(mockView.TotalAmount);
    Assert.NotEqual("$0.00", mockView.TotalAmount);
}
```

## 💡 Real-World Variations

This pattern can be extended for different scenarios:

### Database-Backed Version

```csharp
public class MasterDetailPresenter : WindowPresenterBase<IMasterDetailView>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;

    public MasterDetailPresenter(
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository)
    {
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
    }

    private async void OnCustomerSelectionChanged(object sender, EventArgs e)
    {
        var customer = View.SelectedCustomer;
        if (customer != null)
        {
            View.ShowLoadingIndicator(true);
            var orders = await _orderRepository.GetByCustomerIdAsync(customer.Id);
            View.Orders = orders;
            CalculateAndDisplayTotal(orders);
            View.ShowLoadingIndicator(false);
        }

        _dispatcher.RaiseCanExecuteChanged();
    }
}
```

### Multi-Level Hierarchy

Extend the pattern to support three levels (Customer → Order → OrderItem):

```csharp
public interface IMasterDetailView : IWindowView
{
    // Level 1: Customers
    IEnumerable<CustomerModel> Customers { get; set; }
    CustomerModel SelectedCustomer { get; }

    // Level 2: Orders
    IEnumerable<OrderModel> Orders { get; set; }
    OrderModel SelectedOrder { get; }

    // Level 3: Order Items
    IEnumerable<OrderItemModel> OrderItems { get; set; }
    OrderItemModel SelectedOrderItem { get; }

    event EventHandler CustomerSelectionChanged;
    event EventHandler OrderSelectionChanged;
    event EventHandler OrderItemSelectionChanged;
}
```

## 🔗 Related Examples

- [ToDo CRUD Demo](Example-ToDo) - Basic CRUD operations
- [Validation Demo](Example-Validation) - Add validation to master-detail forms
- [Async Operations](Example-Async-Operations) - Load master-detail data asynchronously

## 📚 Further Reading

- [ViewAction System](ViewAction-System) - Understanding action binding
- [Window Navigation](Window-Navigation) - Opening edit dialogs for customers/orders
- [Change Tracking](Change-Tracking) - Adding edit/cancel support

---

**[⬆ Back to Examples](Home#example-applications)**
