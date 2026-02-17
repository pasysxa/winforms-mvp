using System;
using WinformsMVP.Common;
using WinformsMVP.Common.Events;
using WinformsMVP.Core.Presenters;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Services;

namespace WinformsMVP.Samples.NavigatorDemo
{
    public class ConfirmDialogParameters
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public bool DefaultYes { get; set; }
    }

    /// <summary>
    /// Confirm dialog - takes parameters, returns bool.
    /// </summary>
    public class ConfirmDialogPresenter : WindowPresenterBase<IConfirmDialogView>,
        IRequestClose<bool>,
        IInitializable<ConfirmDialogParameters>
    {
        private bool _result;
        private ConfirmDialogParameters _parameters;

        public event EventHandler<CloseRequestedEventArgs<bool>> CloseRequested;

        protected override void OnViewAttached()
        {
            // Nothing to do here
        }

        public void Initialize(ConfirmDialogParameters parameters)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

            // Call base Initialize to trigger RegisterViewActions and OnInitialize
            ((IInitializable)this).Initialize();
        }

        protected override void RegisterViewActions()
        {
            _dispatcher.Register(ConfirmDialogActions.Yes, OnYes);
            _dispatcher.Register(ConfirmDialogActions.No, OnNo);
            _dispatcher.Register(ConfirmDialogActions.Cancel, OnCancel);

            // Note: View.ActionBinder.Bind(_dispatcher) is now called automatically by the base class
        }

        protected override void OnInitialize()
        {
            // Apply parameters if they were set
            if (_parameters != null)
            {
                View.SetTitle(_parameters.Title);
                View.SetMessage(_parameters.Message);
                View.SetDefaultChoice(_parameters.DefaultYes);
            }
        }

        private void OnYes()
        {
            _result = true;
            RequestClose(InteractionStatus.Ok);
        }

        private void OnNo()
        {
            _result = false;
            RequestClose(InteractionStatus.Ok);
        }

        private void OnCancel()
        {
            RequestClose(InteractionStatus.Cancel);
        }

        private void RequestClose(InteractionStatus status)
        {
            CloseRequested?.Invoke(this, new CloseRequestedEventArgs<bool>(_result, status));
        }

        public bool CanClose()
        {
            return true;
        }
    }

    public static class ConfirmDialogActions
    {
        // Directly use standard actions (no prefix)
        public static readonly ViewAction Yes = StandardActions.Yes;
        public static readonly ViewAction No = StandardActions.No;
        public static readonly ViewAction Cancel = StandardActions.Cancel;
    }
}
