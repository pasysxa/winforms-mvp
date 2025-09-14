using System;
using WinformsMVP.Common.Events;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Services;

namespace WinformsMVP.Core.Presenters
{
    public abstract class PresenterBase<TView> where TView : IViewBase
    {
        protected readonly ViewActionDispatcher _dispatcher;
        protected readonly ICommonServices _commonServices;
        protected readonly IAppContext _appContext;

        protected PresenterBase(TView view, ICommonServices commonServices, IAppContext appContext)
        {
            View = view;
            _dispatcher = new ViewActionDispatcher();
            _commonServices = commonServices;
            _appContext = appContext;
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

        protected abstract void RegisterViewActions();

        protected abstract void Cleanup();

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
