using System;
using System.Windows.Forms;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Services;

namespace WinformsMVP.Samples
{
    /// <summary>
    /// Global ActionKeys used across the entire application.
    /// These represent common actions that might be used in multiple modules.
    /// </summary>
    public static class CommonActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("Common");

        public static readonly ViewAction Save = Factory.Create("Save");
        public static readonly ViewAction Cancel = Factory.Create("Cancel");
        public static readonly ViewAction Delete = Factory.Create("Delete");
        public static readonly ViewAction Refresh = Factory.Create("Refresh");
        public static readonly ViewAction Help = Factory.Create("Help");
    }

    /// <summary>
    /// Module-specific ActionKeys for user editing functionality.
    /// Grouped by feature/module to improve organization.
    /// </summary>
    public static class UserEditorActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("UserEditor");

        public static readonly ViewAction EditUser = Factory.Create("Edit");
        public static readonly ViewAction AddUser = Factory.Create("Add");
        public static readonly ViewAction RemoveUser = Factory.Create("Remove");
        public static readonly ViewAction ChangePassword = Factory.Create("ChangePassword");
        public static readonly ViewAction ResetPermissions = Factory.Create("ResetPermissions");
    }

    #region View Interface

    public interface IUserEditorView : IWindowView
    {
        Button SaveButton { get; }
        Button CancelButton { get; }
        Button DeleteButton { get; }
        Button AddUserButton { get; }
        Button EditUserButton { get; }
        Button ChangePasswordButton { get; }

        ToolStripMenuItem SaveMenuItem { get; }
        ToolStripMenuItem DeleteMenuItem { get; }

        string UserName { get; set; }
        bool HasUnsavedChanges { get; }
        bool HasSelectedUser { get; }
    }

    #endregion

    /// <summary>
    /// Example presenter demonstrating ViewAction best practices:
    /// 1. Using static ActionKey classes instead of raw strings
    /// 2. Implementing CanExecute predicates for dynamic enable/disable
    /// 3. Using ViewActionBinder for declarative UI binding
    /// 4. Automatic UI state updates after action execution
    /// 5. Using IMessageService instead of MessageBox (follows MVP principle)
    /// </summary>
    public class UserEditorPresenter : WindowPresenterBase<IUserEditorView>
    {
        private readonly ViewActionBinder _binder = new ViewActionBinder();
        private readonly IMessageService _messageService;

        public UserEditorPresenter(IMessageService messageService)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }

        protected override void OnViewAttached()
        {
            // Setup bindings declaratively
            // Multiple controls can be bound to the same action
            _binder.Add(CommonActions.Save, View.SaveButton, View.SaveMenuItem);
            _binder.Add(CommonActions.Cancel, View.CancelButton);
            _binder.Add(CommonActions.Delete, View.DeleteButton, View.DeleteMenuItem);
            _binder.Add(UserEditorActions.AddUser, View.AddUserButton);
            _binder.Add(UserEditorActions.EditUser, View.EditUserButton);
            _binder.Add(UserEditorActions.ChangePassword, View.ChangePasswordButton);
        }

        protected override void RegisterViewActions()
        {
            // Register action handlers with CanExecute predicates
            // The binder will automatically enable/disable controls based on these predicates

            // Save is only enabled when there are unsaved changes
            _dispatcher.Register(
                CommonActions.Save,
                OnSave,
                canExecute: () => View.HasUnsavedChanges);

            // Cancel is always enabled
            _dispatcher.Register(
                CommonActions.Cancel,
                OnCancel);

            // Delete is only enabled when a user is selected
            _dispatcher.Register(
                CommonActions.Delete,
                OnDelete,
                canExecute: () => View.HasSelectedUser);

            // Add user is always enabled
            _dispatcher.Register(
                UserEditorActions.AddUser,
                OnAddUser);

            // Edit user is only enabled when a user is selected
            _dispatcher.Register(
                UserEditorActions.EditUser,
                OnEditUser,
                canExecute: () => View.HasSelectedUser);

            // Change password is only enabled when a user is selected
            _dispatcher.Register(
                UserEditorActions.ChangePassword,
                OnChangePassword,
                canExecute: () => View.HasSelectedUser);

            // Bind all controls to the dispatcher
            // This enables automatic CanExecute support
            _binder.Bind(_dispatcher);
        }

        protected override void OnInitialize()
        {
            // Subscribe to view events if needed
            SubscribeToStateChanges();
        }

        private void SubscribeToStateChanges()
        {
            // TWO PATTERNS FOR UI STATE UPDATES:
            //
            // 1. ACTION-DRIVEN (automatic):
            //    When actions execute, UI automatically updates via ActionExecuted event.
            //    No code needed - this example uses this pattern.
            //
            // 2. STATE-DRIVEN (manual trigger):
            //    When state changes from view events, call RaiseCanExecuteChanged():
            //
            // View.SelectionChanged += (s, e) =>
            // {
            //     // State changed outside of action - notify dispatcher
            //     _dispatcher.RaiseCanExecuteChanged();
            // };
            //
            // See ViewActionStateChangedExample.cs for a complete state-driven example.
        }

        #region Action Handlers

        private void OnSave()
        {
            // Save logic here
            _messageService.ShowInfo($"Saving user: {View.UserName}");

            // Note: UI state automatically updates after action execution
        }

        private void OnCancel()
        {
            // Cancel logic here
            _messageService.ShowInfo("Cancelled");
        }

        private void OnDelete()
        {
            // Best practice: Use IMessageService for confirmations (MVP principle)
            if (_messageService.ConfirmYesNo("Are you sure?", "Confirm Delete"))
            {
                // Delete logic here
                _messageService.ShowInfo("User deleted");

                // Note: UI state automatically updates after action execution
            }
        }

        private void OnAddUser()
        {
            // Add user logic here
            _messageService.ShowInfo("Adding new user");

            // Note: UI state automatically updates after action execution
        }

        private void OnEditUser()
        {
            // Edit user logic here
            _messageService.ShowInfo("Editing selected user");
        }

        private void OnChangePassword()
        {
            // Change password logic here
            _messageService.ShowInfo("Changing password for selected user");
        }

        #endregion
    }
}
