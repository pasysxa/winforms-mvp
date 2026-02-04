using System;
using WinformsMVP.Common.Events;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;

namespace WinformsMVP.Core.Presenters
{
    /// <summary>
    /// Base class for all presenters providing common functionality.
    /// This class should not be used directly - use WindowPresenterBase or ControlPresenterBase instead.
    /// </summary>
    public abstract class PresenterBase<TView> : IPresenter where TView : IViewBase
    {
        protected TView View { get; private set; }
        protected readonly ViewActionDispatcher _dispatcher;

        protected PresenterBase()
        {
            _dispatcher = new ViewActionDispatcher();
        }

        /// <summary>
        /// Sets the view for this presenter. Called by derived classes.
        /// </summary>
        protected void SetView(TView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            View = view;
            OnViewAttached();
        }

        /// <summary>
        /// Called when the view is attached to this presenter.
        /// </summary>
        protected abstract void OnViewAttached();

        public Type ViewInterfaceType => typeof(TView);

        protected ViewActionDispatcher Dispatcher => _dispatcher;

        /// <summary>
        /// Helper method to dispatch actions from view events.
        /// </summary>
        protected void OnViewActionTriggered(object sender, ActionRequestEventArgs e)
        {
            DispatchAction(e);
        }

        /// <summary>
        /// Dispatches an action request to the registered handler.
        /// </summary>
        protected void DispatchAction(ActionRequestEventArgs e)
        {
            if (e == null) return;

            var key = e.ActionKey;
            object payload = null;

            if (e is IActionRequestEventArgsWithValue valueProvider)
            {
                payload = valueProvider.GetValue();
            }

            Dispatcher.Dispatch(key, payload);
        }

        // IDisposable implementation
        private bool _isDisposed = false;

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Cleanup();
                _isDisposed = true;
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Override to clean up resources when the presenter is disposed.
        /// </summary>
        protected virtual void Cleanup() { }
    }
}
