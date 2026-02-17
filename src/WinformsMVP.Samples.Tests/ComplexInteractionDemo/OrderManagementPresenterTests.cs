using System;
using System.Collections.Generic;
using Xunit;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Samples.ComplexInteractionDemo.Models;
using WinformsMVP.Samples.ComplexInteractionDemo.OrderManagement;
using WinformsMVP.Samples.ComplexInteractionDemo.ProductSelector;
using WinformsMVP.Samples.ComplexInteractionDemo.OrderSummary;

namespace WindowsMVP.Samples.Tests.ComplexInteractionDemo
{
    public class OrderManagementPresenterTests
    {
        private class MockOrderManagementView : IOrderManagementView
        {
            public string StatusMessage { get; set; }
            public decimal CurrentTotal { get; set; }
            public bool CanSaveOrder { get; set; }

            public event EventHandler SaveRequested;

            public ViewActionBinder ActionBinder { get; } = new ViewActionBinder();

            public string LastSuccessMessage { get; private set; }
            public string LastErrorMessage { get; private set; }

            // IWindowView members
            public IntPtr Handle => IntPtr.Zero;
            public bool IsDisposed => false;
            public void Activate() { }

            public void ShowSuccessMessage(string message)
            {
                LastSuccessMessage = message;
            }

            public void ShowErrorMessage(string message)
            {
                LastErrorMessage = message;
            }
        }

        private class MockProductSelectorView : IProductSelectorView
        {
            public IList<Product> Products { get; set; }
            public Product SelectedProduct { get; set; }
            public int Quantity { get; set; } = 1;
            public bool HasSelection => SelectedProduct != null;
            public bool HasValidQuantity => Quantity > 0;

            public event EventHandler<ProductAddedEventArgs> ProductAdded;
            public event EventHandler SelectionChanged;

            public ViewActionBinder ActionBinder { get; } = new ViewActionBinder();

            public void ShowError(string message) { }

            public void RaiseProductAdded(Product product, int quantity)
            {
                ProductAdded?.Invoke(this, new ProductAddedEventArgs(product, quantity));
            }
        }

        private class MockOrderSummaryView : IOrderSummaryView
        {
            public IList<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
            public decimal TotalAmount { get; set; }
            public OrderItem SelectedItem { get; set; }
            public bool HasSelection => SelectedItem != null;
            public bool HasItems => OrderItems.Count > 0;

            public event EventHandler<OrderItemRemovedEventArgs> ItemRemoved;
            public event EventHandler<TotalChangedEventArgs> TotalChanged;
            public event EventHandler SelectionChanged;

            public ViewActionBinder ActionBinder { get; } = new ViewActionBinder();

            public void RaiseItemRemoved(OrderItem item)
            {
                ItemRemoved?.Invoke(this, new OrderItemRemovedEventArgs(item));
            }

            public void RaiseTotalChanged(decimal oldTotal, decimal newTotal)
            {
                TotalChanged?.Invoke(this, new TotalChangedEventArgs(oldTotal, newTotal));
            }
        }

        [Fact]
        public void OnInitialize_ShouldLoadProductsIntoProductSelector()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Price = 10m },
                new Product { Id = 2, Name = "Product2", Price = 20m }
            };

            var mainView = new MockOrderManagementView();
            var productSelectorView = new MockProductSelectorView();
            var orderSummaryView = new MockOrderSummaryView();

            var productSelectorPresenter = new ProductSelectorPresenter(productSelectorView);
            var orderSummaryPresenter = new OrderSummaryPresenter(orderSummaryView);

            var mainPresenter = new OrderManagementPresenter(
                productSelectorPresenter,
                orderSummaryPresenter,
                products);

            // Act
            mainPresenter.AttachView(mainView);
            mainPresenter.Initialize();

            // Assert
            Assert.Equal(products, productSelectorView.Products);
            Assert.Equal("Ready to take orders", mainView.StatusMessage);
            Assert.Equal(0m, mainView.CurrentTotal);
        }

        [Fact]
        public void ProductAdded_ShouldAddToOrderSummaryAndUpdateStatus()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Price = 999m, StockQuantity = 10 }
            };

            var mainView = new MockOrderManagementView();
            var productSelectorView = new MockProductSelectorView();
            var orderSummaryView = new MockOrderSummaryView();

            var productSelectorPresenter = new ProductSelectorPresenter(productSelectorView);
            var orderSummaryPresenter = new OrderSummaryPresenter(orderSummaryView);

            var mainPresenter = new OrderManagementPresenter(
                productSelectorPresenter,
                orderSummaryPresenter,
                products);

            mainPresenter.AttachView(mainView);
            mainPresenter.Initialize();

            // Act
            productSelectorView.RaiseProductAdded(products[0], 2);

            // Assert
            Assert.Single(orderSummaryView.OrderItems);
            Assert.Equal("Laptop", orderSummaryView.OrderItems[0].Product.Name);
            Assert.Equal(2, orderSummaryView.OrderItems[0].Quantity);
            Assert.Contains("Added 2 x Laptop", mainView.StatusMessage);
        }

        [Fact]
        public void TotalChanged_ShouldUpdateMainViewTotal()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Mouse", Price = 29.99m, StockQuantity = 50 }
            };

            var mainView = new MockOrderManagementView();
            var productSelectorView = new MockProductSelectorView();
            var orderSummaryView = new MockOrderSummaryView();

            var productSelectorPresenter = new ProductSelectorPresenter(productSelectorView);
            var orderSummaryPresenter = new OrderSummaryPresenter(orderSummaryView);

            var mainPresenter = new OrderManagementPresenter(
                productSelectorPresenter,
                orderSummaryPresenter,
                products);

            mainPresenter.AttachView(mainView);
            mainPresenter.Initialize();

            // Act
            productSelectorView.RaiseProductAdded(products[0], 3);

            // Assert
            Assert.Equal(89.97m, mainView.CurrentTotal);
            // Note: OnProductAdded sets the status message after AddProduct,
            // which overwrites the TotalChanged message
            Assert.Contains("Added 3 x Mouse", mainView.StatusMessage);
        }

        [Fact]
        public void OnSaveOrder_WithItems_ShouldShowSuccessMessage()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Keyboard", Price = 79.99m, StockQuantity = 30 }
            };

            var mainView = new MockOrderManagementView();
            var productSelectorView = new MockProductSelectorView();
            var orderSummaryView = new MockOrderSummaryView();

            var productSelectorPresenter = new ProductSelectorPresenter(productSelectorView);
            var orderSummaryPresenter = new OrderSummaryPresenter(orderSummaryView);

            var mainPresenter = new OrderManagementPresenter(
                productSelectorPresenter,
                orderSummaryPresenter,
                products);

            mainPresenter.AttachView(mainView);
            mainPresenter.Initialize();

            // Add product to order
            productSelectorView.RaiseProductAdded(products[0], 2);

            // Act
            mainPresenter.Dispatcher.Dispatch(OrderManagementActions.SaveOrder);

            // Assert
            Assert.NotNull(mainView.LastSuccessMessage);
            Assert.Contains("Order saved successfully", mainView.LastSuccessMessage);
            Assert.Contains("Items: 1", mainView.LastSuccessMessage);  // 1 order line item
            Assert.Contains("159.98", mainView.LastSuccessMessage);  // Total (2 * 79.99)
        }

        [Fact]
        public void OnSaveOrder_WithNoItems_ShouldShowErrorMessage()
        {
            // Arrange
            var products = new List<Product>();

            var mainView = new MockOrderManagementView();
            var productSelectorView = new MockProductSelectorView();
            var orderSummaryView = new MockOrderSummaryView();

            var productSelectorPresenter = new ProductSelectorPresenter(productSelectorView);
            var orderSummaryPresenter = new OrderSummaryPresenter(orderSummaryView);

            var mainPresenter = new OrderManagementPresenter(
                productSelectorPresenter,
                orderSummaryPresenter,
                products);

            mainPresenter.AttachView(mainView);
            mainPresenter.Initialize();

            // Act
            mainPresenter.Dispatcher.Dispatch(OrderManagementActions.SaveOrder);

            // Assert
            Assert.NotNull(mainView.LastErrorMessage);
            Assert.Contains("empty order", mainView.LastErrorMessage);
        }

        [Fact]
        public void OnClearOrder_ShouldClearOrderSummary()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product1", Price = 10m, StockQuantity = 10 },
                new Product { Id = 2, Name = "Product2", Price = 20m, StockQuantity = 10 }
            };

            var mainView = new MockOrderManagementView();
            var productSelectorView = new MockProductSelectorView();
            var orderSummaryView = new MockOrderSummaryView();

            var productSelectorPresenter = new ProductSelectorPresenter(productSelectorView);
            var orderSummaryPresenter = new OrderSummaryPresenter(orderSummaryView);

            var mainPresenter = new OrderManagementPresenter(
                productSelectorPresenter,
                orderSummaryPresenter,
                products);

            mainPresenter.AttachView(mainView);
            mainPresenter.Initialize();

            // Add products
            productSelectorView.RaiseProductAdded(products[0], 1);
            productSelectorView.RaiseProductAdded(products[1], 1);

            // Act
            mainPresenter.Dispatcher.Dispatch(OrderManagementActions.ClearOrder);

            // Assert
            Assert.Empty(orderSummaryView.OrderItems);
            Assert.Equal(0m, orderSummaryView.TotalAmount);
            Assert.Contains("cleared", mainView.StatusMessage);
        }

        [Fact]
        public void InteractionFlow_CompleteOrder_ShouldWorkCorrectly()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Laptop", Price = 999m, StockQuantity = 10 },
                new Product { Id = 2, Name = "Mouse", Price = 29.99m, StockQuantity = 50 }
            };

            var mainView = new MockOrderManagementView();
            var productSelectorView = new MockProductSelectorView();
            var orderSummaryView = new MockOrderSummaryView();

            var productSelectorPresenter = new ProductSelectorPresenter(productSelectorView);
            var orderSummaryPresenter = new OrderSummaryPresenter(orderSummaryView);

            var mainPresenter = new OrderManagementPresenter(
                productSelectorPresenter,
                orderSummaryPresenter,
                products);

            mainPresenter.AttachView(mainView);
            mainPresenter.Initialize();

            // Act - Complete order workflow
            // 1. Add laptop
            productSelectorView.RaiseProductAdded(products[0], 1);
            Assert.Equal(999m, mainView.CurrentTotal);

            // 2. Add mouse
            productSelectorView.RaiseProductAdded(products[1], 2);
            Assert.Equal(1058.98m, mainView.CurrentTotal);

            // 3. Check order items
            Assert.Equal(2, orderSummaryView.OrderItems.Count);

            // 4. Save order
            mainPresenter.Dispatcher.Dispatch(OrderManagementActions.SaveOrder);
            Assert.NotNull(mainView.LastSuccessMessage);

            // Assert final state
            Assert.Contains("Order saved", mainView.StatusMessage);
        }
    }
}
