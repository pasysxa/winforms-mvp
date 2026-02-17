using System;
using System.Collections.Generic;
using WinformsMVP.Core.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Samples.ComplexInteractionDemo.Models;

namespace WinformsMVP.Samples.ComplexInteractionDemo.ProductSelector
{
    /// <summary>
    /// Static action keys for ProductSelector
    /// </summary>
    public static class ProductSelectorActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("ProductSelector");

        public static readonly ViewAction AddToOrder = Factory.Create("AddToOrder");
    }

    /// <summary>
    /// Presenter for product selection control
    /// </summary>
    public class ProductSelectorPresenter : ControlPresenterBase<IProductSelectorView>
    {
        public ProductSelectorPresenter(IProductSelectorView view) : base(view)
        {
        }

        protected override void OnViewAttached()
        {
            // Subscribe to selection changes to update CanExecute state
            View.SelectionChanged += (s, e) => Dispatcher.RaiseCanExecuteChanged();
        }

        protected override void RegisterViewActions()
        {
            Dispatcher.Register(
                ProductSelectorActions.AddToOrder,
                OnAddToOrder,
                canExecute: () => View.HasSelection && View.HasValidQuantity);

            // Framework automatically binds View.ActionBinder
        }

        protected override void OnInitialize()
        {
            // Initialize default quantity
            View.Quantity = 1;
        }

        /// <summary>
        /// Initializes the product list
        /// </summary>
        public void LoadProducts(IList<Product> products)
        {
            View.Products = products;
        }

        /// <summary>
        /// Handles adding product to order
        /// </summary>
        private void OnAddToOrder()
        {
            var product = View.SelectedProduct;
            var quantity = View.Quantity;

            if (product == null)
            {
                View.ShowError("Please select a product");
                return;
            }

            if (quantity <= 0)
            {
                View.ShowError("Quantity must be greater than 0");
                return;
            }

            if (quantity > product.StockQuantity)
            {
                View.ShowError($"Only {product.StockQuantity} items available in stock");
                return;
            }

            // Raise event for parent to handle
            View.ProductAdded?.Invoke(this, new ProductAddedEventArgs(product, quantity));

            // Reset quantity after adding
            View.Quantity = 1;
        }
    }
}
