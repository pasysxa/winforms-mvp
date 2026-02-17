using System;
using System.Collections.Generic;
using WinformsMVP.Core.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Samples.ComplexInteractionDemo.Models;
using WinformsMVP.Samples.ComplexInteractionDemo.ProductSelector;
using WinformsMVP.Samples.ComplexInteractionDemo.OrderSummary;

namespace WinformsMVP.Samples.ComplexInteractionDemo.OrderManagement
{
    /// <summary>
    /// Static action keys for OrderManagement
    /// </summary>
    public static class OrderManagementActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("OrderManagement");

        public static readonly ViewAction SaveOrder = Factory.Create("SaveOrder");
        public static readonly ViewAction ClearOrder = Factory.Create("ClearOrder");
    }

    /// <summary>
    /// Presenter for the main order management window
    /// Coordinates between ProductSelector and OrderSummary child controls
    /// </summary>
    public class OrderManagementPresenter : WindowPresenterBase<IOrderManagementView>
    {
        private readonly ProductSelectorPresenter _productSelectorPresenter;
        private readonly OrderSummaryPresenter _orderSummaryPresenter;
        private readonly IList<Product> _availableProducts;

        /// <summary>
        /// Constructor with child presenters and product data
        /// </summary>
        public OrderManagementPresenter(
            ProductSelectorPresenter productSelectorPresenter,
            OrderSummaryPresenter orderSummaryPresenter,
            IList<Product> availableProducts)
        {
            _productSelectorPresenter = productSelectorPresenter ??
                throw new ArgumentNullException(nameof(productSelectorPresenter));
            _orderSummaryPresenter = orderSummaryPresenter ??
                throw new ArgumentNullException(nameof(orderSummaryPresenter));
            _availableProducts = availableProducts ?? new List<Product>();
        }

        protected override void OnViewAttached()
        {
            // Subscribe to child control events

            // ProductSelector → OrderSummary communication
            _productSelectorPresenter.View.ProductAdded += OnProductAdded;

            // OrderSummary → Main window communication
            _orderSummaryPresenter.View.TotalChanged += OnTotalChanged;
            _orderSummaryPresenter.View.ItemRemoved += OnItemRemoved;
        }

        protected override void RegisterViewActions()
        {
            Dispatcher.Register(
                OrderManagementActions.SaveOrder,
                OnSaveOrder,
                canExecute: () => View.CanSaveOrder);

            Dispatcher.Register(
                OrderManagementActions.ClearOrder,
                OnClearOrder,
                canExecute: () => _orderSummaryPresenter.GetOrderItems().Count > 0);

            // Framework automatically binds View.ActionBinder
        }

        protected override void OnInitialize()
        {
            // Initialize child controls
            _productSelectorPresenter.LoadProducts(_availableProducts);

            // Set initial status
            View.StatusMessage = "Ready to take orders";
            View.CurrentTotal = 0;
        }

        /// <summary>
        /// Handle product added from ProductSelector
        /// Forward to OrderSummary
        /// </summary>
        private void OnProductAdded(object sender, ProductAddedEventArgs e)
        {
            try
            {
                // Add product to order summary
                _orderSummaryPresenter.AddProduct(e.Product, e.Quantity);

                // Update status
                View.StatusMessage = $"Added {e.Quantity} x {e.Product.Name} to order";

                // Trigger CanExecute update for Save button
                Dispatcher.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                View.ShowErrorMessage($"Failed to add product: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle total changed from OrderSummary
        /// Update main window display
        /// </summary>
        private void OnTotalChanged(object sender, TotalChangedEventArgs e)
        {
            View.CurrentTotal = e.NewTotal;
            View.StatusMessage = $"Order total updated: ${e.NewTotal:F2}";

            // Update CanExecute state for Save button
            Dispatcher.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Handle item removed from OrderSummary
        /// Update status
        /// </summary>
        private void OnItemRemoved(object sender, OrderItemRemovedEventArgs e)
        {
            View.StatusMessage = $"Removed {e.Item.Product.Name} from order";

            // Update CanExecute state
            Dispatcher.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Save the current order
        /// </summary>
        private void OnSaveOrder()
        {
            var orderItems = _orderSummaryPresenter.GetOrderItems();

            if (orderItems.Count == 0)
            {
                View.ShowErrorMessage("Cannot save an empty order");
                return;
            }

            // Simulate saving to database
            // In real application, this would call a service/repository

            var totalAmount = View.CurrentTotal;
            var itemCount = orderItems.Count;

            View.ShowSuccessMessage(
                $"Order saved successfully!\n\n" +
                $"Items: {itemCount}\n" +
                $"Total: ${totalAmount:F2}");

            View.StatusMessage = $"Order saved - {itemCount} items, ${totalAmount:F2}";
        }

        /// <summary>
        /// Clear the current order
        /// </summary>
        private void OnClearOrder()
        {
            // This will trigger OrderSummary.ClearAll
            _orderSummaryPresenter.Dispatcher.Dispatch(OrderSummaryActions.ClearAll);

            View.StatusMessage = "Order cleared";
            Dispatcher.RaiseCanExecuteChanged();
        }

        protected override void Cleanup()
        {
            // Unsubscribe from child events
            if (_productSelectorPresenter?.View != null)
            {
                _productSelectorPresenter.View.ProductAdded -= OnProductAdded;
            }

            if (_orderSummaryPresenter?.View != null)
            {
                _orderSummaryPresenter.View.TotalChanged -= OnTotalChanged;
                _orderSummaryPresenter.View.ItemRemoved -= OnItemRemoved;
            }

            base.Cleanup();
        }
    }
}
