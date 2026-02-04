using System;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Services;

namespace MinformsMVP.Samples.ToDoDemo
{
    /// <summary>
    /// ActionKeys for ToDo demo.
    /// Best practice: Use static classes and ViewActionFactory with StandardActionNames.
    /// </summary>
    public static class ToDoDemoActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("ToDoDemo");

        // 使用 StandardActionNames 确保命名一致性
        public static readonly ViewAction AddTask = Factory.Create("AddTask");        // 业务特定
        public static readonly ViewAction RemoveTask = Factory.Create("RemoveTask");  // 业务特定
        public static readonly ViewAction CompleteTask = Factory.Create("CompleteTask"); // 业务特定
        public static readonly ViewAction SaveAll = Factory.Create(StandardActionNames.Crud.Save); // "ToDoDemo.Save"（使用标准名称）
    }

    /// <summary>
    /// Presenter for ToDo demo.
    ///
    /// IMPORTANT - Proper MVP Design:
    /// - This presenter has ZERO knowledge of UI elements (no Button, TextBox, etc.)
    /// - Only interacts with view through the IToDoView interface (data and behavior)
    /// - Registers actions with dispatcher, but doesn't know what UI controls trigger them
    /// - View implementation (Form) is responsible for binding UI controls to actions
    ///
    /// Demonstrates:
    /// 1. Static ActionKey classes
    /// 2. Proper MVP separation (no UI types in Presenter)
    /// 3. CanExecute predicates that control action availability
    /// 4. AUTOMATIC UI updates after actions (action-driven)
    /// 5. MANUAL UI updates from view events (state-driven via RaiseCanExecuteChanged)
    /// 6. Dependency injection with IMessageService
    /// </summary>
    public class ToDoDemoPresenter : WindowPresenterBase<IToDoView>
    {
        private readonly IMessageService _messageService;

        public ToDoDemoPresenter(IMessageService messageService)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }

        protected override void OnViewAttached()
        {
            // Nothing to do here - we don't access any UI elements
            // The view will bind its UI controls in BindActions()
        }

        protected override void RegisterViewActions()
        {
            // Register action handlers with CanExecute predicates
            // The dispatcher will manage which actions can execute
            // The view will decide which UI controls trigger which actions

            // AddTask: Always enabled (no CanExecute = always true)
            _dispatcher.Register(ToDoDemoActions.AddTask, OnAddTask);

            // RemoveTask: Only enabled when a task is selected
            _dispatcher.Register(
                ToDoDemoActions.RemoveTask,
                OnRemoveTask,
                canExecute: () => View.HasSelectedTask);

            // CompleteTask: Only enabled when a task is selected
            _dispatcher.Register(
                ToDoDemoActions.CompleteTask,
                OnCompleteTask,
                canExecute: () => View.HasSelectedTask);

            // SaveAll: Only enabled when there are unsaved changes
            _dispatcher.Register(
                ToDoDemoActions.SaveAll,
                OnSaveAll,
                canExecute: () => View.HasPendingChanges);

            // Ask the view to bind its UI controls to the dispatcher
            // We don't know (or care) what controls the view uses
            View.BindActions(_dispatcher);
        }

        protected override void OnInitialize()
        {
            // Subscribe to view events for STATE-DRIVEN updates
            SubscribeToViewEvents();

            View.UpdateStatus("Ready! Try adding tasks and watch buttons enable/disable automatically.");
        }

        private void SubscribeToViewEvents()
        {
            // STATE-DRIVEN UPDATE PATTERN:
            // When state changes from view events (not from our actions),
            // we call RaiseCanExecuteChanged() to refresh action availability.
            // This is similar to WPF's ICommand.RaiseCanExecuteChanged().

            View.SelectionChanged += (sender, e) =>
            {
                // User selected/deselected a task
                // Notify dispatcher to re-evaluate CanExecute for all actions
                _dispatcher.RaiseCanExecuteChanged();

                // Update status
                if (View.HasSelectedTask)
                {
                    View.UpdateStatus("Task selected. Delete and Complete actions are now available.");
                }
                else
                {
                    View.UpdateStatus("No task selected. Delete and Complete actions are disabled.");
                }
            };

            View.DataChanged += (sender, e) =>
            {
                // Data changed externally (e.g., checkbox toggled)
                _dispatcher.RaiseCanExecuteChanged();
            };
        }

        #region Action Handlers - ACTION-DRIVEN Updates

        // These handlers demonstrate ACTION-DRIVEN automatic updates.
        // After each action completes, UI state automatically refreshes
        // because the dispatcher triggers ActionExecuted event.
        // NO manual UpdateCanExecuteStates() or RaiseCanExecuteChanged() needed!

        private void OnAddTask()
        {
            var taskText = View.TaskText;

            if (string.IsNullOrWhiteSpace(taskText))
            {
                _messageService.ShowWarning("Please enter a task description.", "Empty Task");
                return;
            }

            // Add task to view
            View.AddTaskToList(taskText);
            View.TaskText = string.Empty;

            // Show confirmation
            _messageService.ShowInfo($"Task added: {taskText}", "Success");

            // UI state automatically updates here!
            // - Save action will become available (HasPendingChanges = true)
            // No manual code needed - ActionExecuted event handles it
        }

        private void OnRemoveTask()
        {
            // Confirm deletion
            if (!_messageService.ConfirmYesNo("Are you sure you want to delete this task?", "Confirm Delete"))
            {
                return;
            }

            // Delete task
            View.RemoveSelectedTask();

            // UI state automatically updates here!
            // - Delete/Complete actions may become unavailable (if no selection after delete)
            // - Save action will become available (HasPendingChanges = true)
            // No manual code needed - ActionExecuted event handles it
        }

        private void OnCompleteTask()
        {
            // Mark task as completed
            View.CompleteSelectedTask();

            // UI state automatically updates here!
            // - Save action will become available (HasPendingChanges = true)
            // No manual code needed - ActionExecuted event handles it
        }

        private void OnSaveAll()
        {
            // Simulate saving
            View.ClearPendingChanges();

            _messageService.ShowInfo("All changes have been saved!", "Save Successful");

            // UI state automatically updates here!
            // - Save action will become unavailable (HasPendingChanges = false)
            // No manual code needed - ActionExecuted event handles it
        }

        #endregion
    }
}
