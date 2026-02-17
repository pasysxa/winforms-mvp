using System;
using System.Collections.Generic;
using Xunit;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Samples.ComplexInteractionDemo.Models;
using WinformsMVP.Samples.ComplexInteractionDemo.ProductSelector;

namespace WindowsMVP.Samples.Tests.ComplexInteractionDemo
{
    public class ProductSelectorPresenterTests
    {
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

            public string LastErrorMessage { get; private set; }

            public void ShowError(string message)
            {
                LastErrorMessage = message;
            }

            public void RaiseProductAdded(Product product, int quantity)
            {
                ProductAdded?.Invoke(this, new ProductAddedEventArgs(product, quantity));
            }
        }

        [Fact]
        public void OnInitialize_ShouldSetDefaultQuantityToOne()
        {
            // Arrange
            var view = new MockProductSelectorView();

            // Act - Constructor automatically attaches and initializes
            var presenter = new ProductSelectorPresenter(view);

            // Assert
            Assert.Equal(1, view.Quantity);
        }

        [Fact]
        public void LoadProducts_ShouldSetProductsOnView()
        {
            // Arrange
            var view = new MockProductSelectorView();
            var presenter = new ProductSelectorPresenter(view);

            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 10m },
                new Product { Id = 2, Name = "Product 2", Price = 20m }
            };

            // Act
            presenter.LoadProducts(products);

            // Assert
            Assert.Equal(products, view.Products);
        }

        [Fact]
        public void OnAddToOrder_WithValidSelection_ShouldRaiseProductAddedEvent()
        {
            // Arrange
            var view = new MockProductSelectorView();
            var presenter = new ProductSelectorPresenter(view);

            var product = new Product { Id = 1, Name = "Laptop", Price = 999m, StockQuantity = 10 };
            view.SelectedProduct = product;
            view.Quantity = 2;

            ProductAddedEventArgs eventArgs = null;
            view.ProductAdded += (s, e) => eventArgs = e;

            // Act
            presenter.AddToOrder();

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal(product, eventArgs.Product);
            Assert.Equal(2, eventArgs.Quantity);
        }

        [Fact]
        public void OnAddToOrder_WithNoSelection_ShouldShowError()
        {
            // Arrange
            var view = new MockProductSelectorView();
            var presenter = new ProductSelectorPresenter(view);

            view.SelectedProduct = null;
            view.Quantity = 1;

            // Act
            presenter.AddToOrder();

            // Assert
            Assert.Contains("select a product", view.LastErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void OnAddToOrder_WithInvalidQuantity_ShouldShowError()
        {
            // Arrange
            var view = new MockProductSelectorView();
            var presenter = new ProductSelectorPresenter(view);

            var product = new Product { Id = 1, Name = "Laptop", Price = 999m, StockQuantity = 10 };
            view.SelectedProduct = product;
            view.Quantity = 0;

            // Act
            presenter.AddToOrder();

            // Assert
            Assert.Contains("greater than 0", view.LastErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void OnAddToOrder_WithQuantityExceedingStock_ShouldShowError()
        {
            // Arrange
            var view = new MockProductSelectorView();
            var presenter = new ProductSelectorPresenter(view);

            var product = new Product { Id = 1, Name = "Laptop", Price = 999m, StockQuantity = 5 };
            view.SelectedProduct = product;
            view.Quantity = 10;

            // Act
            presenter.AddToOrder();

            // Assert
            Assert.Contains("available in stock", view.LastErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void OnAddToOrder_AfterSuccess_ShouldResetQuantityToOne()
        {
            // Arrange
            var view = new MockProductSelectorView();
            var presenter = new ProductSelectorPresenter(view);

            var product = new Product { Id = 1, Name = "Laptop", Price = 999m, StockQuantity = 10 };
            view.SelectedProduct = product;
            view.Quantity = 5;

            // Act
            presenter.AddToOrder();

            // Assert
            Assert.Equal(1, view.Quantity);
        }
    }
}
