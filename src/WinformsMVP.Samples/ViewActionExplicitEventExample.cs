using System;
using System.Windows.Forms;
using WinformsMVP.Common.Events;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Services;

namespace WinformsMVP.Samples
{
    // ========================================
    // Explicit Event-Based ViewAction Pattern
    // ========================================
    //
    // This example demonstrates the EXPLICIT event-based pattern for ViewActions.
    //
    // Key difference from implicit pattern:
    // - IMPLICIT: View.ActionBinder.Bind(Dispatcher) - "magic" connection
    // - EXPLICIT: View.ActionRequest += OnActionRequest - clear subscription
    //
    // Benefits of explicit pattern:
    // ✅ Clear, visible event subscription code
    // ✅ F12 navigation works (jump to handler)
    // ✅ IDE refactoring support (rename, find references)
    // ✅ Easy to debug (set breakpoint on event handler)
    // ✅ Familiar pattern (standard .NET events)
    //
    // Use when:
    // - You need explicit control flow visibility
    // - Team prefers explicit over implicit
    // - Complex scenarios with conditional routing

    #region View Actions

    public static class ExplicitDemoActions
    {
        public static readonly ViewAction Save = ViewAction.Create("ExplicitDemo.Save");
        public static readonly ViewAction Delete = ViewAction.Create("ExplicitDemo.Delete");
        public static readonly ViewAction Refresh = ViewAction.Create("ExplicitDemo.Refresh");
    }

    #endregion

    #region View Interface

    /// <summary>
    /// View interface using explicit event-based pattern.
    /// Exposes ActionRequest event instead of relying on ActionBinder.Bind(Dispatcher).
    /// </summary>
    public interface IExplicitEventDemoView : IWindowView
    {
        // Data properties
        string ItemName { get; set; }
        int ItemCount { get; set; }
        string StatusMessage { set; }

        // State properties for CanExecute
        bool HasUnsavedChanges { get; }
        bool HasSelection { get; }

        // ✅ Explicit event for ViewAction handling
        event EventHandler<ActionRequestEventArgs> ActionRequest;

        // Methods
        void ShowItems(string[] items);
        void ClearForm();
    }

    #endregion

    #region View Implementation

    public partial class ExplicitEventDemoForm : Form, IExplicitEventDemoView
    {
        private ViewActionBinder _binder;

        // ✅ Implement IViewBase.ActionBinder (required by framework)
        public ViewActionBinder ActionBinder => _binder;

        // ✅ Expose ActionRequest event (explicit pattern)
        public event EventHandler<ActionRequestEventArgs> ActionRequest;

        // Form controls (designer-initialized)
        private TextBox _txtItemName;
        private NumericUpDown _numItemCount;
        private ListBox _lstItems;
        private Label _lblStatus;
        private Button _btnSave;
        private Button _btnDelete;
        private Button _btnRefresh;

        public ExplicitEventDemoForm()
        {
            InitializeComponent();
            InitializeActionBindings();
        }

        private void InitializeComponent()
        {
            // Form setup
            this.Text = "Explicit Event-Based ViewAction Demo";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Controls initialization
            _txtItemName = new TextBox { Left = 120, Top = 20, Width = 200 };
            _numItemCount = new NumericUpDown { Left = 120, Top = 50, Width = 100, Maximum = 1000 };
            _lstItems = new ListBox { Left = 20, Top = 100, Width = 300, Height = 200 };
            _lblStatus = new Label { Left = 20, Top = 320, Width = 450, Height = 20 };

            _btnSave = new Button { Text = "Save", Left = 350, Top = 100, Width = 100 };
            _btnDelete = new Button { Text = "Delete", Left = 350, Top = 140, Width = 100 };
            _btnRefresh = new Button { Text = "Refresh", Left = 350, Top = 180, Width = 100 };

            var lblName = new Label { Text = "Item Name:", Left = 20, Top = 23, Width = 90 };
            var lblCount = new Label { Text = "Count:", Left = 20, Top = 53, Width = 90 };

            this.Controls.Add(lblName);
            this.Controls.Add(lblCount);
            this.Controls.Add(_txtItemName);
            this.Controls.Add(_numItemCount);
            this.Controls.Add(_lstItems);
            this.Controls.Add(_lblStatus);
            this.Controls.Add(_btnSave);
            this.Controls.Add(_btnDelete);
            this.Controls.Add(_btnRefresh);

            // Track changes for CanExecute
            _txtItemName.TextChanged += (s, e) => OnDataChanged();
            _numItemCount.ValueChanged += (s, e) => OnDataChanged();
            _lstItems.SelectedIndexChanged += (s, e) => OnSelectionChanged();
        }

        private void InitializeActionBindings()
        {
            _binder = new ViewActionBinder();

            // Map buttons to ViewActions
            _binder.Add(ExplicitDemoActions.Save, _btnSave);
            _binder.Add(ExplicitDemoActions.Delete, _btnDelete);
            _binder.Add(ExplicitDemoActions.Refresh, _btnRefresh);

            // ✅ EXPLICIT PATTERN: Subscribe to ActionTriggered and forward to ActionRequest
            _binder.ActionTriggered += (sender, e) =>
            {
                // Forward to public ActionRequest event
                ActionRequest?.Invoke(this, e);
            };

            // Bind controls to actions (event-only, no dispatcher)
            _binder.Bind();
        }

        // IExplicitEventDemoView implementation

        public string ItemName
        {
            get => _txtItemName.Text;
            set => _txtItemName.Text = value;
        }

        public int ItemCount
        {
            get => (int)_numItemCount.Value;
            set => _numItemCount.Value = value;
        }

        public string StatusMessage
        {
            set => _lblStatus.Text = value;
        }

        public bool HasUnsavedChanges { get; private set; }

        public bool HasSelection => _lstItems.SelectedIndex >= 0;

        public void ShowItems(string[] items)
        {
            _lstItems.Items.Clear();
            if (items != null)
            {
                _lstItems.Items.AddRange(items);
            }
        }

        public void ClearForm()
        {
            _txtItemName.Clear();
            _numItemCount.Value = 0;
            HasUnsavedChanges = false;
        }

        // Helper events
        private void OnDataChanged()
        {
            HasUnsavedChanges = true;
        }

        private void OnSelectionChanged()
        {
            // Notify presenter that selection changed (affects CanExecute)
        }
    }

    #endregion

    #region Presenter

    public class ExplicitEventDemoPresenter : WindowPresenterBase<IExplicitEventDemoView>
    {
        private readonly IMessageService _messageService;
        private string[] _items = new[] { "Item 1", "Item 2", "Item 3" };

        public ExplicitEventDemoPresenter(IMessageService messageService)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }

        protected override void OnViewAttached()
        {
            // ✅ EXPLICIT SUBSCRIPTION - Clear and visible!
            View.ActionRequest += OnActionRequest;

            // Load initial data
            View.ShowItems(_items);
            View.StatusMessage = "Ready";
        }

        // ✅ EXPLICIT EVENT HANDLER - F12 navigation works!
        private void OnActionRequest(object sender, ActionRequestEventArgs e)
        {
            // Route to specific handlers based on action
            if (e.ActionKey == ExplicitDemoActions.Save)
            {
                // Check CanExecute manually (no automatic CanExecute in explicit mode)
                if (View.HasUnsavedChanges)
                {
                    OnSave();
                }
                else
                {
                    _messageService.ShowWarning("No changes to save.", "Info");
                }
            }
            else if (e.ActionKey == ExplicitDemoActions.Delete)
            {
                if (View.HasSelection)
                {
                    OnDelete();
                }
                else
                {
                    _messageService.ShowWarning("Please select an item to delete.", "Info");
                }
            }
            else if (e.ActionKey == ExplicitDemoActions.Refresh)
            {
                OnRefresh();
            }
        }

        private void OnSave()
        {
            var item = $"{View.ItemName} (x{View.ItemCount})";
            Array.Resize(ref _items, _items.Length + 1);
            _items[_items.Length - 1] = item;

            View.ShowItems(_items);
            View.ClearForm();
            View.StatusMessage = $"Saved: {item}";

            _messageService.ShowInfo($"Item saved: {item}", "Success");
        }

        private void OnDelete()
        {
            // Delete selected item
            var index = Array.IndexOf(_items, "Item 1"); // Simplified
            if (index >= 0)
            {
                var newItems = new string[_items.Length - 1];
                Array.Copy(_items, 0, newItems, 0, index);
                Array.Copy(_items, index + 1, newItems, index, _items.Length - index - 1);
                _items = newItems;
            }

            View.ShowItems(_items);
            View.StatusMessage = "Item deleted";
            _messageService.ShowInfo("Item deleted successfully.", "Success");
        }

        private void OnRefresh()
        {
            View.ShowItems(_items);
            View.StatusMessage = "Refreshed";
            _messageService.ShowInfo("Data refreshed.", "Info");
        }
    }

    #endregion

    #region Comparison Summary

    /*
     * ========================================
     * IMPLICIT vs EXPLICIT Pattern Comparison
     * ========================================
     *
     * IMPLICIT PATTERN (ActionBinder + Dispatcher):
     * ----------------------------------------------
     * // Presenter
     * protected override void RegisterViewActions()
     * {
     *     Dispatcher.Register(ExplicitDemoActions.Save, OnSave,
     *         canExecute: () => View.HasUnsavedChanges);
     *
     *     View.ActionBinder.Bind(Dispatcher);  // 🔗 Magic connection
     * }
     *
     * ✅ Pros: Less code, automatic CanExecute
     * ❌ Cons: Implicit connection, harder to debug
     *
     *
     * EXPLICIT PATTERN (ActionRequest Event):
     * ----------------------------------------
     * // View
     * public event EventHandler<ActionRequestEventArgs> ActionRequest;
     * _binder.ActionTriggered += (s, e) => ActionRequest?.Invoke(this, e);
     *
     * // Presenter
     * View.ActionRequest += OnActionRequest;  // ✅ Clear subscription
     *
     * private void OnActionRequest(object sender, ActionRequestEventArgs e)
     * {
     *     if (e.ActionKey == ExplicitDemoActions.Save && View.HasUnsavedChanges)
     *         OnSave();
     * }
     *
     * ✅ Pros: Explicit, debuggable, familiar pattern
     * ❌ Cons: More code, manual CanExecute checks
     *
     *
     * WHEN TO USE EACH:
     * -----------------
     * Implicit: Simple CRUD, standard workflows, team comfortable with framework
     * Explicit: Complex logic, debugging needs, team prefers explicit code
     *
     * MIXED APPROACH (Best of Both):
     * -------------------------------
     * Use implicit for simple actions (Save, Delete, Refresh)
     * Use explicit events for complex scenarios (custom logic, parameters)
     */

    #endregion
}
