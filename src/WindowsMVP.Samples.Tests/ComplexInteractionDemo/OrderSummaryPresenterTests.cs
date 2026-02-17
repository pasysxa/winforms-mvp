using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Samples.ComplexInteractionDemo.Models;
using WinformsMVP.Samples.ComplexInteractionDemo.OrderSummary;

namespace WindowsMVP.Samples.Tests.ComplexInteractionDemo
{
    public class OrderSummaryPresenterTests
    {
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
        public void AddProduct_ShouldAddItemToOrder()
        {
            // Arrange
            var view = new MockOrderSummaryView();
            var presenter = new OrderSummaryPresenter(view);
            presenter.AttachView(view);
            presenter.Initialize();

            var product = new Product { Id = 1, Name = "Laptop", Price = 999m, StockQuantity = 10 };

            // Act
            presenter.AddProduct(product, 2);

            // Assert
            Assert.Single(view.OrderItems);
            Assert.Equal(product.Name, view.OrderItems[0].Product.Name);
            Assert.Equal(2, view.OrderItems[0].Quantity);
            Assert.Equal(1998m, view.TotalAmount);
        }

        [Fact]
        public void AddProduct_WithSameProductTwice_ShouldCombineQuantities()
        {
            // Arrange
            var view = new MockOrderSummaryView();
            var presenter = new OrderSummaryPresenter(view);
            presenter.AttachView(view);
            presenter.Initialize();

            var product = new Product { Id = 1, Name = "Mouse", Price = 29.99m, StockQuantity = 50 };

            // Act
            presenter.AddProduct(product, 2);
            presenter.AddProduct(product, 3);

            // Assert
            Assert.Single(view.OrderItems);
            Assert.Equal(5, view.OrderItems[0].Quantity);
            Assert.Equal(149.95m, view.TotalAmount);
        }

        [Fact]
        public void AddProduct_ShouldRaiseTotalChangedEvent()
        {
            // Arrange
            var view = new MockOrderSummaryView();
            var presenter = new OrderSummaryPresenter(view);
            presenter.AttachView(view);
            presenter.Initialize();

            var product = new Product { Id = 1, Name = "Keyboard", Price = 79.99m, StockQuantity = 30 };

            TotalChangedEventArgs eventArgs = null;
            view.TotalChanged += (s, e) => eventArgs = e;

            // Act
            presenter.AddProduct(product, 1);

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal(0m, eventArgs.OldTotal);
            Assert.Equal(79.99m, eventArgs.NewTotal);
        }

        [Fact]
        public void AddProduct_WithNullProduct_ShouldThrowException()
        {
            // Arrange
            var view = new MockOrderSummaryView();
            var presenter = new OrderSummaryPresenter(view);
            presenter.AttachView(view);
            presenter.Initialize();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => presenter.AddProduct(null, 1));
        }

        [Fact]
        public void AddProduct_WithZeroQuantity_ShouldThrowException()
        {
            // Arrange
            var view = new MockOrderSummaryView();
            var presenter = new OrderSummaryPresenter(view);
            presenter.AttachView(view);
            presenter.Initialize();

            var product = new Product { Id = 1, Name = "Test", Price = 10m, StockQuantity = 10 };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => presenter.AddProduct(product, 0));
        }

        [Fact]
        public void OnRemoveItem_WithSelectedItem_ShouldRemoveFromOrder()
        {
            // Arrange
            var view = new MockOrderSummaryView();
            var presenter = new OrderSummaryPresenter(view);
            presenter.AttachView(view);
            presenter.Initialize();

            var product1 = new Product { Id = 1, Name = "Product1", Price = 10m, StockQuantity = 10 };
            var product2 = new Product { Id = 2, Name = "Product2", Price = 20m, StockQuantity = 10 };

            presenter.AddProduct(product1, 1);
            presenter.AddProduct(product2, 1);

            view.SelectedItem = view.OrderItems[0];

            // Act
            presenter.Dispatcher.Dispatch(OrderSummaryActions.RemoveItem);

            // Assert
            Assert.Single(view.OrderItems);
            Assert.Equal("Product2", view.OrderItems[0].Product.Name);
            Assert.Equal(20m, view.TotalAmount);
        }

        [Fact]
        public void OnRemoveItem_ShouldRaiseItemRemovedEvent()
        {
            // Arrange
            var view = new MockOrderSummaryView();
            var presenter = new OrderSummaryPresenter(view);
            presenter.AttachView(view);
            presenter.Initialize();

            var product = new Product { Id = 1, Name = "Test", Price = 10m, StockQuantity = 10 };
            presenter.AddProduct(product, 1);

            view.SelectedItem = view.OrderItems[0];

            OrderItemRemovedEventArgs eventArgs = null;
            view.ItemRemoved += (s, e) => eventArgs = e;

            // Act
            presenter.Dispatcher.Dispatch(OrderSummaryActions.RemoveItem);

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal("Test", eventArgs.Item.Product.Name);
        }

        [Fact]
        public void OnClearAll_ShouldRemoveAllItems()
        {
            // Arrange
            var view = new MockOrderSummaryView();
            var presenter = new OrderSummaryPresenter(view);
            presenter.AttachView(view);
            presenter.Initialize();

            presenter.AddProduct(new Product { Id = 1, Name = "P1", Price = 10m, StockQuantity = 10 }, 1);
            presenter.AddProduct(new Product { Id = 2, Name = "P2", Price = 20m, StockQuantity = 10 }, 1);
            presenter.AddProduct(new Product { Id = 3, Name = "P3", Price = 30m, StockQuantity = 10 }, 1);

            // Act
            presenter.Dispatcher.Dispatch(OrderSummaryActions.ClearAll);

            // Assert
            Assert.Empty(view.OrderItems);
            Assert.Equal(0m, view.TotalAmount);
        }

        [Fact]
        public void GetOrderItems_ShouldReturnReadOnlyList()
        {
            // Arrange
            var view = new MockOrderSummaryView();
            var presenter = new OrderSummaryPresenter(view);
            presenter.AttachView(view);
            presenter.Initialize();

            var product = new Product { Id = 1, Name = "Test", Price = 10m, StockQuantity = 10 };
            presenter.AddProduct(product, 2);

            // Act
            var orderItems = presenter.GetOrderItems();

            // Assert
            Assert.Single(orderItems);
            Assert.Equal("Test", orderItems[0].Product.Name);
            Assert.Equal(2, orderItems[0].Quantity);
        }

        [Fact]
        public void CalculateTotal_WithMultipleItems_ShouldSumCorrectly()
        {
            // Arrange
            var view = new MockOrderSummaryView();
            var presenter = new OrderSummaryPresenter(view);
            presenter.AttachView(view);
            presenter.Initialize();

            // Act
            presenter.AddProduct(new Product { Id = 1, Name = "P1", Price = 10.50m, StockQuantity = 10 }, 2);  // 21.00
            presenter.AddProduct(new Product { Id = 2, Name = "P2", Price = 15.25m, StockQuantity = 10 }, 3);  // 45.75
            presenter.AddProduct(new Product { Id = 3, Name = "P3", Price = 99.99m, StockQuantity = 10 }, 1);  // 99.99

            // Assert
            Assert.Equal(166.74m, view.TotalAmount);
        }
    }
}
