using System;
using WinformsMVP.Common;
using WinformsMVP.Common.Events;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Services;

namespace MinformsMVP.Samples.NavigatorDemo
{
    /// <summary>
    /// Simple modal dialog - no parameters, no return value.
    /// </summary>
    public class SimpleDialogPresenter : WindowPresenterBase<ISimpleDialogView>, IRequestClose<object>
    {
        private readonly IMessageService _messageService;

        public event EventHandler<CloseRequestedEventArgs<object>> CloseRequested;

        public SimpleDialogPresenter(IMessageService messageService)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }

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
            _messageService.ShowInfo("You clicked OK!", "Simple Dialog");
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
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("SimpleDialog");

        // 使用标准对话框动作名称
        public static readonly ViewAction Ok = Factory.Create(StandardActionNames.Dialog.Ok);
        public static readonly ViewAction Cancel = Factory.Create(StandardActionNames.Dialog.Cancel);
    }
}
