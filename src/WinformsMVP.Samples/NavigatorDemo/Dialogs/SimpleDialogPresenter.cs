using System;
using WinformsMVP.Common;
using WinformsMVP.Common.Events;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Services;

namespace WinformsMVP.Samples.NavigatorDemo
{
    /// <summary>
    /// Simple modal dialog - no parameters, no return value.
    /// </summary>
    public class SimpleDialogPresenter : WindowPresenterBase<ISimpleDialogView>, IRequestClose<object>
    {
        public event EventHandler<CloseRequestedEventArgs<object>> CloseRequested;

        protected override void OnViewAttached()
        {
            // Nothing to do here
        }

        protected override void RegisterViewActions()
        {
            _dispatcher.Register(SimpleDialogActions.Ok, OnOk);
            _dispatcher.Register(SimpleDialogActions.Cancel, OnCancel);

            View.BindActions(_dispatcher);
        }

        protected override void OnInitialize()
        {
            View.SetMessage("This is a simple modal dialog.\nClick OK to confirm or Cancel to dismiss.");
        }

        private void OnOk()
        {
            Messages.ShowInfo("You clicked OK!", "Simple Dialog");
            RequestClose(InteractionStatus.Ok);
        }

        private void OnCancel()
        {
            RequestClose(InteractionStatus.Cancel);
        }

        private void RequestClose(InteractionStatus status)
        {
            CloseRequested?.Invoke(this, new CloseRequestedEventArgs<object>(null, status));
        }

        public bool CanClose()
        {
            // Always allow close
            return true;
        }
    }

    public static class SimpleDialogActions
    {
        // 直接使用标准动作（无前缀）
        public static readonly ViewAction Ok = StandardActions.Ok;
        public static readonly ViewAction Cancel = StandardActions.Cancel;
    }
}
