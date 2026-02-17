# Presenter Communication Patterns

This document compares different approaches for inter-presenter communication in the WinForms MVP framework, using the Order Management sample as a concrete example.

## Table of Contents

- [Overview](#overview)
- [Pattern 1: Direct Method Calls (Original)](#pattern-1-direct-method-calls-original)
- [Pattern 2: Shared Service Layer](#pattern-2-shared-service-layer)
- [Pattern 3: Event Aggregator (Pub-Sub)](#pattern-3-event-aggregator-pub-sub)
- [Decision Matrix](#decision-matrix)
- [Performance Comparison](#performance-comparison)
- [Testing Comparison](#testing-comparison)
- [When to Use Which Pattern](#when-to-use-which-pattern)

## Overview

In complex WinForms MVP applications, presenters often need to communicate with each other. This document demonstrates three architectural patterns using the **Order Management** scenario:

- **ProductSelector** - Allows adding products to an order
- **OrderSummary** - Displays current order items and total
- **OrderManagement** - Coordinates the overall workflow

## Pattern 1: Direct Method Calls (Original)

**Location**: `src/WinformsMVP.Samples/ComplexInteractionDemo/`

### Architecture

```
OrderManagementPresenter (Parent)
    ├─ Holds reference to ProductSelectorPresenter
    ├─ Holds reference to OrderSummaryPresenter
    └─ Calls public methods directly
```

### Implementation

```csharp
public class OrderManagementPresenter : WindowPresenterBase<IOrderManagementView>
{
    private readonly ProductSelectorPresenter _productSelectorPresenter;
    private readonly OrderSummaryPresenter _orderSummaryPresenter;

    public OrderManagementPresenter(ProductSelectorPresenter productSelector,
                                    OrderSummaryPresenter orderSummary)
    {
        _productSelectorPresenter = productSelector;
        _orderSummaryPresenter = orderSummary;
    }

    private void OnClearOrder()
    {
        // Direct method call
        _orderSummaryPresenter.ClearAll();
        View.StatusMessage = "Order cleared";
    }
}

public class ProductSelectorPresenter : ControlPresenterBase<IProductSelectorView>
{
    // Exposed public method for parent to call
    public void AddToOrder()
    {
        OnAddToOrder();
    }
}
```

### Pros

- ✅ **Simple and explicit** - Easy to understand control flow
- ✅ **Type-safe** - Compile-time checked method calls
- ✅ **Easy debugging** - F12 navigation works perfectly
- ✅ **No infrastructure needed** - No additional services or event systems
- ✅ **Best for parent-child** - Natural for hierarchical relationships

### Cons

- ❌ **Tight coupling** - Parent knows about child implementation
- ❌ **Hard to test in isolation** - Must create all child presenters
- ❌ **Limited reusability** - Hard to reuse child presenters elsewhere
- ❌ **Scalability issues** - Becomes unwieldy with many children
- ❌ **Parent responsibility** - Parent manages coordination logic

### Use Cases

- Simple parent-child presenter relationships
- Forms with 2-3 embedded UserControls
- Coordination logic is straightforward
- Children don't need to communicate with each other

## Pattern 2: Shared Service Layer

**Location**: `src/WinformsMVP.Samples/ComplexInteractionDemo_ServiceBased/`

### Architecture

```
IOrderManagementService (Shared State)
    ↑                  ↑                  ↑
    |                  |                  |
ProductSelector    OrderSummary    OrderManagement
    |                  |                  |
    └─ AddProduct      └─ Subscribe       └─ ClearOrder
```

### Implementation

```csharp
// Service owns the state
public interface IOrderManagementService
{
    IReadOnlyList<OrderItem> OrderItems { get; }
    decimal TotalAmount { get; }

    void AddProduct(Product product, int quantity);
    void RemoveItem(OrderItem item);
    void ClearOrder();

    event EventHandler<ProductAddedEventArgs> ProductAdded;
    event EventHandler<TotalChangedEventArgs> TotalChanged;
    event EventHandler OrderCleared;
}

// Presenter A: Calls service methods
public class ProductSelectorPresenter : ControlPresenterBase<IProductSelectorView>
{
    private readonly IOrderManagementService _orderService;

    private void OnAddToOrder()
    {
        _orderService.AddProduct(product, quantity);  // Modify state
    }
}

// Presenter B: Subscribes to service events
public class OrderSummaryPresenter : ControlPresenterBase<IOrderSummaryView>
{
    private readonly IOrderManagementService _orderService;

    protected override void OnViewAttached()
    {
        _orderService.ProductAdded += OnProductAdded;  // React to changes
    }

    private void OnProductAdded(object sender, ProductAddedEventArgs e)
    {
        View.OrderItems = _orderService.OrderItems;  // Query state
    }
}

// Presenter C: No references to other presenters!
public class OrderManagementPresenter : WindowPresenterBase<IOrderManagementView>
{
    private readonly IOrderManagementService _orderService;

    private void OnClearOrder()
    {
        _orderService.ClearOrder();  // Service handles it
    }
}
```

### Pros

- ✅ **Zero presenter coupling** - Presenters don't know about each other
- ✅ **Single source of truth** - Service owns all state
- ✅ **Easy to test** - Mock service in unit tests
- ✅ **Direct query support** - `var items = _orderService.OrderItems`
- ✅ **Transactional operations** - Service can enforce business rules
- ✅ **Clear ownership** - Service owns data lifecycle

### Cons

- ❌ **Additional abstraction** - Need to create service interface
- ❌ **Service instance management** - Must inject same instance everywhere
- ❌ **Potential for God Service** - Service can become too large
- ❌ **Event subscription complexity** - Must manage event subscriptions

### Use Cases

- Shared state management (orders, shopping cart, user session)
- Business logic needs to be centralized
- Multiple presenters need to query/modify the same data
- Transactional operations (e.g., validate before save)
- Need to enforce business rules centrally

## Pattern 3: Event Aggregator (Pub-Sub)

**Location**: `src/WinformsMVP.Samples/ComplexInteractionDemo_EventBased/`

### Architecture

```
EventAggregator (Message Bus)
    ↑                  ↑                  ↑
    |                  |                  |
ProductSelector    OrderSummary    OrderManagement
    |                  |                  |
    └─ Publish         ├─ Subscribe       └─ Subscribe
       messages        └─ Publish           to all messages
                         messages
```

### Implementation

```csharp
// Define messages
public class ProductAddedMessage
{
    public Product Product { get; set; }
    public int Quantity { get; set; }
}

public class ClearOrderMessage { }

public class OrderClearedMessage { }

// Presenter A: Publishes messages
public class ProductSelectorPresenter : ControlPresenterBase<IProductSelectorView>
{
    private readonly IEventAggregator _eventAggregator;

    private void OnAddToOrder()
    {
        _eventAggregator.Publish(new ProductAddedMessage
        {
            Product = product,
            Quantity = quantity
        });
    }
}

// Presenter B: Subscribes to and publishes messages
public class OrderSummaryPresenter : ControlPresenterBase<IOrderSummaryView>
{
    private readonly IEventAggregator _eventAggregator;
    private List<OrderItem> _orderItems = new List<OrderItem>();  // Local state!

    protected override void OnViewAttached()
    {
        _productAddedSubscription = _eventAggregator.Subscribe<ProductAddedMessage>(OnProductAdded);
        _clearOrderSubscription = _eventAggregator.Subscribe<ClearOrderMessage>(msg => ClearAll());
    }

    private void OnProductAdded(ProductAddedMessage msg)
    {
        // Update local state
        _orderItems.Add(new OrderItem { Product = msg.Product, Quantity = msg.Quantity });

        // Publish new message
        _eventAggregator.Publish(new TotalChangedMessage { ... });
    }
}

// Presenter C: Subscribes to all messages for status updates
public class OrderManagementPresenter : WindowPresenterBase<IOrderManagementView>
{
    private readonly IEventAggregator _eventAggregator;

    protected override void OnViewAttached()
    {
        _eventAggregator.Subscribe<ProductAddedMessage>(msg =>
            View.StatusMessage = $"Added {msg.Quantity} x {msg.Product.Name}");

        _eventAggregator.Subscribe<TotalChangedMessage>(msg =>
            View.CurrentTotal = msg.NewTotal);
    }

    private void OnClearOrder()
    {
        _eventAggregator.Publish(new ClearOrderMessage());
    }
}
```

### Pros

- ✅ **Zero coupling** - Presenters completely independent
- ✅ **Maximum flexibility** - Easy to add/remove presenters
- ✅ **Cross-cutting concerns** - Any presenter can react to any event
- ✅ **UI thread marshaling** - Background threads automatically handled
- ✅ **Testable** - Easy to mock EventAggregator
- ✅ **Weak references** - Automatic memory cleanup

### Cons

- ❌ **Indirect flow** - Harder to trace message flow
- ❌ **No compile-time safety** - Message typos only caught at runtime
- ❌ **State ownership unclear** - Who owns OrderItems list?
- ❌ **Request-response awkward** - Need workarounds for queries
- ❌ **Can be overused** - Easy to create "message soup"
- ❌ **Debugging challenges** - Can't F12 to message handler

### Use Cases

- Cross-cutting notifications (status updates, logging)
- Loosely coupled modules
- Plugin architecture
- Background task notifications
- Multiple unrelated presenters need to react to same event
- Components in different UI hierarchies

## Decision Matrix

| Scenario | Recommended Pattern | Reason |
|----------|---------------------|--------|
| Parent Form with 2-3 UserControls | **Direct Method Calls** | Simple, explicit, easy to maintain |
| Shopping Cart / Order Management | **Shared Service** | Centralized state, business rules, queries |
| Status Bar Updates from Any Presenter | **Event Aggregator** | Cross-cutting concern, no coupling |
| User Login/Logout Notifications | **Event Aggregator** | Application-wide event |
| Parent coordinating child workflow | **Direct Method Calls** | Natural parent-child relationship |
| Multiple presenters editing same data | **Shared Service** | Single source of truth needed |
| Plugin system / Extensibility | **Event Aggregator** | Maximum decoupling |
| Background task notifications | **Event Aggregator** | UI thread marshaling |

## Performance Comparison

Based on stress tests (`EventAggregatorTests.cs`):

| Pattern | Overhead | Scalability | Memory |
|---------|----------|-------------|--------|
| **Direct Method Calls** | None (direct call) | Poor (tight coupling) | Low |
| **Shared Service** | Low (event subscription) | Good (centralized) | Low |
| **Event Aggregator** | Moderate (message routing) | Excellent (decoupled) | Low (weak refs) |

**Event Aggregator Performance:**
- 10,000 subscribers: ~1 second per message
- 100,000 messages: ~16ms (1 subscriber)
- Throughput: > 50,000 messages/second
- Thread-safe concurrent operations

## Testing Comparison

### Direct Method Calls

```csharp
[Fact]
public void Test_OrderManagement_ClearsOrder()
{
    // Must create all child presenters
    var productSelectorView = new MockProductSelectorView();
    var orderSummaryView = new MockOrderSummaryView();
    var mainView = new MockOrderManagementView();

    var productSelector = new ProductSelectorPresenter(productSelectorView);
    var orderSummary = new OrderSummaryPresenter(orderSummaryView);

    var presenter = new OrderManagementPresenter(productSelector, orderSummary);
    presenter.AttachView(mainView);

    // Test requires all dependencies
    presenter.ClearOrder();

    Assert.Empty(orderSummary.GetOrderItems());
}
```

### Shared Service

```csharp
[Fact]
public void Test_ProductSelector_AddsProduct()
{
    // Only need to mock service!
    var mockService = new MockOrderManagementService();
    var view = new MockProductSelectorView();

    var presenter = new ProductSelectorPresenter(view, mockService);
    presenter.AttachView(view);

    presenter.AddToOrder();

    Assert.Equal(1, mockService.AddProductCallCount);
}
```

### Event Aggregator

```csharp
[Fact]
public void Test_ProductAddedMessage_ReceivedByOrderSummary()
{
    var eventAggregator = new EventAggregator();
    var productSelectorView = new MockProductSelectorView();
    var orderSummaryView = new MockOrderSummaryView();

    var productSelector = new ProductSelectorPresenter(productSelectorView, eventAggregator);
    var orderSummary = new OrderSummaryPresenter(orderSummaryView, eventAggregator);

    productSelector.AttachView(productSelectorView);
    orderSummary.AttachView(orderSummaryView);

    productSelector.AddToOrder();  // Publishes message

    Assert.Equal(1, orderSummaryView.Items.Count);  // OrderSummary received it
}
```

## When to Use Which Pattern

### Use Direct Method Calls When

- ✅ Parent-child presenter relationship (Form → UserControl)
- ✅ Simple coordination (2-3 child presenters)
- ✅ One-way command flow (parent tells child what to do)
- ✅ Immediate feedback needed
- ✅ Team prefers explicit over implicit

### Use Shared Service When

- ✅ Multiple presenters share state (shopping cart, user session)
- ✅ Business logic needs centralization
- ✅ Need transactional operations
- ✅ Direct queries are important (`orderService.TotalAmount`)
- ✅ Data ownership must be clear
- ✅ Strong consistency requirements

### Use Event Aggregator When

- ✅ Cross-cutting concerns (logging, status updates)
- ✅ Presenters are in different UI hierarchies
- ✅ Plugin/extensibility architecture
- ✅ Background thread communication
- ✅ Multiple unrelated presenters react to same event
- ✅ Maximum decoupling required

### Anti-Patterns to Avoid

❌ **Using Event Aggregator for State Management**
```csharp
// Bad: Who owns the OrderItems?
_eventAggregator.Publish(new AddProductMessage(product));
var request = new GetOrderSnapshotRequest();
_eventAggregator.Publish(request);  // Awkward!
return request.Snapshot;
```

✅ **Use Shared Service instead**
```csharp
// Good: Service owns state
_orderService.AddProduct(product);
return _orderService.OrderItems;  // Direct query
```

---

❌ **Using Shared Service for Notifications Only**
```csharp
// Bad: Service has no state, just events
public interface INotificationService
{
    event EventHandler<string> MessageReceived;
    void Notify(string message);
}
```

✅ **Use Event Aggregator instead**
```csharp
// Good: Event Aggregator is designed for this
_eventAggregator.Publish(new NotificationMessage { Text = message });
```

---

❌ **Parent Presenter Managing All Child Logic**
```csharp
// Bad: Parent doing child's work
public class OrderManagementPresenter
{
    private void OnAddProduct()
    {
        // Doing product selector's job!
        var product = _productSelectorView.SelectedProduct;
        _orderSummaryPresenter.AddProduct(product);
    }
}
```

✅ **Let children handle their own logic**
```csharp
// Good: Each presenter handles its own concerns
public class ProductSelectorPresenter
{
    private void OnAddToOrder()
    {
        var product = View.SelectedProduct;
        _orderService.AddProduct(product);  // or publish message
    }
}
```

## Hybrid Approach

You can combine patterns for optimal results:

```csharp
public class OrderManagementPresenter
{
    // Shared Service for state management
    private readonly IOrderManagementService _orderService;

    // Event Aggregator for cross-cutting notifications
    private readonly IEventAggregator _eventAggregator;

    // Direct reference for simple coordination
    private readonly InvoicePresenter _invoicePresenter;

    protected override void OnViewAttached()
    {
        // Service event for data changes
        _orderService.OrderCompleted += OnOrderCompleted;

        // Event Aggregator for application-wide events
        _eventAggregator.Subscribe<UserLoggedOutMessage>(OnUserLoggedOut);
    }

    private void OnPrintInvoice()
    {
        // Direct method call for simple command
        _invoicePresenter.Print(_orderService.CurrentOrder);
    }
}
```

## Summary

| Pattern | Coupling | State Ownership | Queries | Best For |
|---------|----------|-----------------|---------|----------|
| **Direct Method Calls** | High | Each presenter | Via public methods | Parent-child coordination |
| **Shared Service** | Low | Service | Direct (`service.Data`) | Shared state management |
| **Event Aggregator** | None | Each presenter | Awkward (request-response) | Cross-cutting notifications |

**Golden Rule**: Choose the simplest pattern that meets your needs. Start with Direct Method Calls for parent-child relationships, use Shared Service when multiple presenters need shared state, and reserve Event Aggregator for cross-cutting concerns and maximum decoupling.

## Examples in Codebase

- **Direct Method Calls**: `src/WinformsMVP.Samples/ComplexInteractionDemo/`
- **Shared Service**: `src/WinformsMVP.Samples/ComplexInteractionDemo_ServiceBased/`
- **Event Aggregator**: `src/WinformsMVP.Samples/ComplexInteractionDemo_EventBased/`
- **Tests**: `src/WinformsMVP.Samples.Tests/Common/EventAggregatorTests.cs`

All three implementations use the **same Views and Models**, demonstrating that architectural patterns are presenter concerns.
