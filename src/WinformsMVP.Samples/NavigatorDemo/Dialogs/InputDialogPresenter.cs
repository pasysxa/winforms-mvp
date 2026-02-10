using System;
using WinformsMVP.Common;
using WinformsMVP.Common.Events;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Services;

namespace WinformsMVP.Samples.NavigatorDemo
{
    /// <summary>
    /// Input dialog - returns string value.
    /// </summary>
    public class InputDialogPresenter : WindowPresenterBase<IInputDialogView>, IRequestClose<string>
    {
        private string _result;

        public event EventHandler<CloseRequestedEventArgs<string>> CloseRequested;

        protected override void OnViewAttached()
        {
            // Nothing to do here
        }

        protected override void RegisterViewActions()
        {
            _dispatcher.Register(InputDialogActions.Ok, OnOk);
            _dispatcher.Register(InputDialogActions.Cancel, OnCancel);

            View.BindActions(_dispatcher);
        }

        protected override void OnInitialize()
        {
            View.SetPrompt("Please enter your name:");
        }

        private void OnOk()
        {
            var input = View.GetInput();
            if (string.IsNullOrWhiteSpace(input))
            {
                Messages.ShowWarning("Please enter a value.", "Input Required");
                return;
            }

            _result = input;
            RequestClose(InteractionStatus.Ok);
        }

        private void OnCancel()
        {
            RequestClose(InteractionStatus.Cancel);
        }

        private void RequestClose(InteractionStatus status)
        {
            CloseRequested?.Invoke(this, new CloseRequestedEventArgs<string>(_result, status));
        }

        public bool CanClose()
        {
            return true;
        }
    }

    public static class InputDialogActions
    {
        // 直接使用标准动作（无前缀）
        public static readonly ViewAction Ok = StandardActions.Ok;
        public static readonly ViewAction Cancel = StandardActions.Cancel;
    }
}
