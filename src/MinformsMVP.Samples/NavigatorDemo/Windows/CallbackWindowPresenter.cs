using System;
using WinformsMVP.Common;
using WinformsMVP.Common.Events;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Services;

namespace MinformsMVP.Samples.NavigatorDemo
{
    /// <summary>
    /// Non-modal window with callback result.
    /// </summary>
    public class CallbackWindowPresenter : WindowPresenterBase<ICallbackWindowView>, IRequestClose<string>
    {
        private string _result;

        public event EventHandler<CloseRequestedEventArgs<string>> CloseRequested;

        protected override void OnViewAttached()
        {
            // Nothing to do here
        }

        protected override void RegisterViewActions()
        {
            _dispatcher.Register(CallbackWindowActions.SaveAndClose, OnSaveAndClose);
            _dispatcher.Register(CallbackWindowActions.Cancel, OnCancel);

            View.BindActions(_dispatcher);
        }

        protected override void OnInitialize()
        {
            View.SetMessage("Enter some text and click 'Save & Close'.\nThe main window will receive the result via callback.");
        }

        private void OnSaveAndClose()
        {
            var text = View.GetText();
            if (string.IsNullOrWhiteSpace(text))
            {
                Messages.ShowWarning("Please enter some text.", "Input Required");
                return;
            }

            _result = text;
            RequestClose(InteractionStatus.Ok);

            // Close the window
            if (View is System.Windows.Forms.Form form)
            {
                form.Close();
            }
        }

        private void OnCancel()
        {
            RequestClose(InteractionStatus.Cancel);

            if (View is System.Windows.Forms.Form form)
            {
                form.Close();
            }
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

    public static class CallbackWindowActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("Callback");

        public static readonly ViewAction SaveAndClose = Factory.Create("SaveAndClose");  // 业务特定
        public static readonly ViewAction Cancel = Factory.Create(StandardActionNames.Dialog.Cancel);
    }
}
