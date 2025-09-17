using System;
using WinformsMVP.Common.Events;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Services;
using WinformsMVP.Services.Implementations;

namespace WinformsMVP.Core.Presenters
{
    public abstract class PresenterBase<TView> where TView : IViewBase
    {
        protected readonly ViewActionDispatcher _dispatcher;
        protected readonly ICommonServices _commonServices;
        protected readonly IAppContext _appContext;

        protected PresenterBase(TView view, ICommonServices commonServices = null, IAppContext appContext = null)
        {
            View = view;
            _dispatcher = new ViewActionDispatcher();
            _commonServices = _commonServices ?? new DefaultCommonServices();
            _appContext = _appContext ?? new DefaultAppContext();
        }

        protected TView View { get; }
        protected ViewActionDispatcher Dispatcher => _dispatcher;

        public virtual void Initialize()
        {
            RegisterViewActions();
        }

        protected void OnViewActionTriggered(object sender, ActionRequestEventArgs e)
        {
            DispatchAction(e);
        }

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

        protected virtual void RegisterViewActions() { }

        protected virtual void Cleanup() { }

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
    }
}
