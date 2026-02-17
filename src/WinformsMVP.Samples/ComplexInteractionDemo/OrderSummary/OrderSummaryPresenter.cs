using System;
using System.Collections.Generic;
using System.Linq;
using WinformsMVP.Core.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Samples.ComplexInteractionDemo.Models;

namespace WinformsMVP.Samples.ComplexInteractionDemo.OrderSummary
{
    /// <summary>
    /// Static action keys for OrderSummary
    /// </summary>
    public static class OrderSummaryActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("OrderSummary");

        public static readonly ViewAction RemoveItem = Factory.Create("RemoveItem");
        public static readonly ViewAction ClearAll = Factory.Create("ClearAll");
    }

    /// <summary>
    /// Presenter for order summary control
    /// </summary>
    public class OrderSummaryPresenter : ControlPresenterBase<IOrderSummaryView>
    {
        private List<OrderItem> _orderItems = new List<OrderItem>();

        public OrderSummaryPresenter(IOrderSummaryView view) : base(view)
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
                OrderSummaryActions.RemoveItem,
                OnRemoveItem,
                canExecute: () => View.HasSelection);

            Dispatcher.Register(
                OrderSummaryActions.ClearAll,
                OnClearAll,
                canExecute: () => View.HasItems);

            // Framework automatically binds View.ActionBinder
        }

        /// <summary>
        /// Adds a product to the order
        /// </summary>
        public void AddProduct(Product product, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

            // Check if product already exists in order
            var existingItem = _orderItems.FirstOrDefault(i => i.Product.Id == product.Id);

            decimal oldTotal = CalculateTotal();

            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += quantity;
            }
            else
            {
                // Add new item
                _orderItems.Add(new OrderItem
                {
                    Product = product,
                    Quantity = quantity
                });
            }

            RefreshView();

            decimal newTotal = CalculateTotal();
            RaiseTotalChanged(oldTotal, newTotal);
        }

        /// <summary>
        /// Removes an item from the order
        /// </summary>
        private void OnRemoveItem()
        {
            var item = View.SelectedItem;
            if (item == null)
                return;

            decimal oldTotal = CalculateTotal();

            _orderItems.Remove(item);
            RefreshView();

            decimal newTotal = CalculateTotal();
            RaiseTotalChanged(oldTotal, newTotal);

            // Raise event for parent
            View.ItemRemoved?.Invoke(this, new OrderItemRemovedEventArgs(item));
        }

        /// <summary>
        /// Clears all items from the order
        /// </summary>
        private void OnClearAll()
        {
            decimal oldTotal = CalculateTotal();

            _orderItems.Clear();
            RefreshView();

            RaiseTotalChanged(oldTotal, 0);
        }

        /// <summary>
        /// Gets all order items (for testing/external access)
        /// </summary>
        public IReadOnlyList<OrderItem> GetOrderItems()
        {
            return _orderItems.AsReadOnly();
        }

        private void RefreshView()
        {
            View.OrderItems = new List<OrderItem>(_orderItems);
            View.TotalAmount = CalculateTotal();
            Dispatcher.RaiseCanExecuteChanged();
        }

        private decimal CalculateTotal()
        {
            return _orderItems.Sum(i => i.Subtotal);
        }

        private void RaiseTotalChanged(decimal oldTotal, decimal newTotal)
        {
            if (oldTotal != newTotal)
            {
                View.TotalChanged?.Invoke(this, new TotalChangedEventArgs(oldTotal, newTotal));
            }
        }
    }
}
