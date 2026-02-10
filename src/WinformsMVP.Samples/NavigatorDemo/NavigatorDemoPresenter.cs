using System;
using WinformsMVP.Common;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Services;

namespace WinformsMVP.Samples.NavigatorDemo
{
    /// <summary>
    /// Action keys for Navigator demo.
    /// </summary>
    public static class NavigatorDemoActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("Navigator");

        public static readonly ViewAction ShowModalSimple = Factory.Create("ShowModalSimple");
        public static readonly ViewAction ShowModalWithResult = Factory.Create("ShowModalWithResult");
        public static readonly ViewAction ShowModalWithParams = Factory.Create("ShowModalWithParams");
        public static readonly ViewAction ShowNonModalSimple = Factory.Create("ShowNonModalSimple");
        public static readonly ViewAction ShowNonModalWithCallback = Factory.Create("ShowNonModalWithCallback");
        public static readonly ViewAction ShowSingletonWindow = Factory.Create("ShowSingletonWindow");
        public static readonly ViewAction ClearLog = Factory.Create("ClearLog");
    }

    /// <summary>
    /// Presenter for WindowNavigator demo.
    /// Demonstrates all WindowNavigator features.
    /// </summary>
    public class NavigatorDemoPresenter : WindowPresenterBase<INavigatorDemoView>
    {
        private readonly IWindowNavigator _navigator;
        private int _singletonCounter = 0;

        public NavigatorDemoPresenter(IWindowNavigator navigator)
        {
            _navigator = navigator ?? throw new ArgumentNullException(nameof(navigator));
        }

        protected override void OnViewAttached()
        {
            // Nothing to do here
        }

        protected override void RegisterViewActions()
        {
            _dispatcher.Register(NavigatorDemoActions.ShowModalSimple, OnShowModalSimple);
            _dispatcher.Register(NavigatorDemoActions.ShowModalWithResult, OnShowModalWithResult);
            _dispatcher.Register(NavigatorDemoActions.ShowModalWithParams, OnShowModalWithParams);
            _dispatcher.Register(NavigatorDemoActions.ShowNonModalSimple, OnShowNonModalSimple);
            _dispatcher.Register(NavigatorDemoActions.ShowNonModalWithCallback, OnShowNonModalWithCallback);
            _dispatcher.Register(NavigatorDemoActions.ShowSingletonWindow, OnShowSingletonWindow);
            _dispatcher.Register(NavigatorDemoActions.ClearLog, OnClearLog);

            View.BindActions(_dispatcher);
        }

        protected override void OnInitialize()
        {
            View.AppendLog("Navigator demo initialized.");
        }

        #region Action Handlers

        private void OnShowModalSimple()
        {
            View.AppendLog("Opening simple modal dialog...");

            var presenter = new SimpleDialogPresenter();
            var result = _navigator.ShowWindowAsModal(presenter);

            View.AppendLog($"Modal dialog closed. Status: {result.Status}");
        }

        private void OnShowModalWithResult()
        {
            View.AppendLog("Opening modal dialog with return value...");

            var presenter = new InputDialogPresenter();
            var result = _navigator.ShowWindowAsModal<InputDialogPresenter, string>(presenter);

            if (result.IsOk)
            {
                View.AppendLog($"User entered: '{result.Value}'");
            }
            else
            {
                View.AppendLog($"Dialog cancelled. Status: {result.Status}");
            }
        }

        private void OnShowModalWithParams()
        {
            View.AppendLog("Opening modal dialog with parameters...");

            var parameters = new ConfirmDialogParameters
            {
                Title = "Confirm Action",
                Message = "Do you want to proceed with this operation?",
                DefaultYes = true
            };

            var presenter = new ConfirmDialogPresenter();
            var result = _navigator.ShowWindowAsModal<ConfirmDialogPresenter, ConfirmDialogParameters, bool>(
                presenter, parameters);

            if (result.IsOk)
            {
                View.AppendLog($"User choice: {(result.Value ? "Yes" : "No")}");
            }
            else
            {
                View.AppendLog($"Dialog cancelled. Status: {result.Status}");
            }
        }

        private void OnShowNonModalSimple()
        {
            View.AppendLog("Opening non-modal window (will not block)...");

            var presenter = new NonModalWindowPresenter();
            _navigator.ShowWindow(presenter);

            View.AppendLog("Non-modal window opened. You can continue using main window.");
        }

        private void OnShowNonModalWithCallback()
        {
            View.AppendLog("Opening non-modal window with callback...");

            var presenter = new CallbackWindowPresenter();
            _navigator.ShowWindow<CallbackWindowPresenter, string>(
                presenter,
                onClosed: result =>
                {
                    if (result.IsOk)
                    {
                        View.AppendLog($"Non-modal window closed with result: '{result.Value}'");
                    }
                    else
                    {
                        View.AppendLog($"Non-modal window closed. Status: {result.Status}");
                    }
                });

            View.AppendLog("Non-modal window opened with callback registered.");
        }

        private void OnShowSingletonWindow()
        {
            View.AppendLog("Opening singleton window (only one instance allowed)...");

            _singletonCounter++;
            var instanceId = _singletonCounter;

            var presenter = new SingletonWindowPresenter(instanceId);
            _navigator.ShowWindow<SingletonWindowPresenter, object>(
                presenter,
                keySelector: p => "SingletonKey", // Same key = same instance
                onClosed: result =>
                {
                    View.AppendLog($"Singleton window #{instanceId} closed.");
                });

            View.AppendLog($"Singleton window #{instanceId} requested. If already open, will activate existing.");
        }

        private void OnClearLog()
        {
            // Clear the log - we'll need to add this to the view interface
            View.AppendLog("--- Log cleared ---");
        }

        #endregion
    }
}
