using System;
using WinformsMVP.Common.Events;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;

namespace WinformsMVP.Core.Presenters
{
    public abstract class PresenterBase<TView> : IPresenter, IViewAttacher<TView>, IInitializable where TView : IViewBase
    {
        protected readonly ViewActionDispatcher _dispatcher;
        //protected readonly ICommonServices _commonServices;
        //protected readonly IAppContext _appContext;

        //protected PresenterBase(TView view, ICommonServices commonServices = null, IAppContext appContext = null)
        //{
        //    View = view;
        //    _dispatcher = new ViewActionDispatcher();
        //    _commonServices = _commonServices ?? new DefaultCommonServices();
        //    _appContext = _appContext ?? new DefaultAppContext();
        //}

        protected PresenterBase()
        {
            _dispatcher = new ViewActionDispatcher();
        }

        public void AttachView(TView view)
        {
            View = view;
            OnViewAttached();
        }

        public Type ViewInterfaceType => typeof(TView);
        protected TView View { get; private set; }
        protected abstract void OnViewAttached();

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
