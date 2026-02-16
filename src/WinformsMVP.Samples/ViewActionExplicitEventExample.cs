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
    // - IMPLICIT: Framework auto-calls View.ActionBinder.Bind(_dispatcher) after RegisterViewActions()
    // - EXPLICIT: View.ActionRequest += OnActionRequest - clear subscription you write yourself
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

        // ✅ State change events for manual CanExecute updates
        event EventHandler SelectionChanged;
        event EventHandler DataChanged;

        // Methods
        void ShowItems(string[] items);
        void ClearForm();
    }

    #endregion

    #region View Implementation

    public partial class ExplicitEventDemoForm : Form, IExplicitEventDemoView
    {
        private ViewActionBinder _binder;

        // ✅ Return null to prevent framework auto-binding (explicit pattern uses events only)
        // This ensures the framework doesn't call Bind(_dispatcher) after RegisterViewActions(),
        // which would cause double-dispatch (both event and callback paths active)
        public ViewActionBinder ActionBinder => null;

        // ✅ Expose ActionRequest event (explicit pattern)
        public event EventHandler<ActionRequestEventArgs> ActionRequest;

        // ✅ State change events (for manual CanExecute updates)
        public event EventHandler SelectionChanged;
        public event EventHandler DataChanged;

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

            // Call Bind() to set up control event subscriptions (event-only mode)
            // Framework's auto-binding won't happen because ActionBinder property returns null
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

        // Helper methods
        private void OnDataChanged()
        {
            HasUnsavedChanges = true;
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnSelectionChanged()
        {
            // Notify presenter that selection changed (affects CanExecute)
            SelectionChanged?.Invoke(this, EventArgs.Empty);
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
            // ✅ SIMPLE EXPLICIT SUBSCRIPTION - Use base class helper method!
            // OnViewActionTriggered is a built-in helper that forwards to Dispatcher
            View.ActionRequest += OnViewActionTriggered;  // One line - super clean!

            // Load initial data
            View.ShowItems(_items);
            View.StatusMessage = "Ready";
        }

        protected override void RegisterViewActions()
        {
            // ✅ Register handlers with CanExecute predicates
            Dispatcher.Register(
                ExplicitDemoActions.Save,
                OnSave,
                canExecute: () => View.HasUnsavedChanges);

            Dispatcher.Register(
                ExplicitDemoActions.Delete,
                OnDelete,
                canExecute: () => View.HasSelection);

            Dispatcher.Register(ExplicitDemoActions.Refresh, OnRefresh);

            // Note: Framework tries to call View.ActionBinder?.Bind(_dispatcher) here,
            // but ActionBinder returns null in explicit mode to prevent auto-binding.
            //
            // Trade-off: Explicit pattern loses automatic CanExecute UI updates.
            // You must manually call Dispatcher.RaiseCanExecuteChanged() when state changes
            // that affect CanExecute predicates (e.g., when View.HasSelection changes).
        }

        protected override void OnInitialize()
        {
            // Subscribe to view state change events to manually trigger CanExecute updates
            View.SelectionChanged += (s, e) => Dispatcher.RaiseCanExecuteChanged();
            View.DataChanged += (s, e) => Dispatcher.RaiseCanExecuteChanged();
        }

        // ✅ Note: No need for manual OnActionRequest handler!
        // OnViewActionTriggered (base class method) handles routing to Dispatcher

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
     * // View Interface
     * public interface IMyView : IWindowView
     * {
     *     ViewActionBinder ActionBinder { get; }  // Returns _binder instance
     * }
     *
     * // Presenter
     * protected override void RegisterViewActions()
     * {
     *     Dispatcher.Register(CommonActions.Save, OnSave,
     *         canExecute: () => View.HasUnsavedChanges);
     *
     *     // Framework auto-calls View.ActionBinder.Bind(_dispatcher) here!
     * }
     *
     * ✅ Pros: Less code, automatic CanExecute UI updates, auto-binding by framework
     * ❌ Cons: Implicit connection (framework magic), harder to debug event flow
     *
     *
     * EXPLICIT PATTERN (ActionRequest Event + Helper Method):
     * --------------------------------------------------------
     * // View Interface
     * public interface IMyView : IWindowView
     * {
     *     ViewActionBinder ActionBinder { get; }  // Returns null to prevent auto-binding!
     *     event EventHandler<ActionRequestEventArgs> ActionRequest;
     *     event EventHandler SelectionChanged;  // For manual CanExecute updates
     * }
     *
     * // View Implementation
     * public ViewActionBinder ActionBinder => null;  // Prevent framework auto-binding
     * _binder.ActionTriggered += (s, e) => ActionRequest?.Invoke(this, e);
     * _binder.Bind();  // Manual event-only binding
     *
     * // Presenter - Simple with helper method!
     * protected override void OnViewAttached()
     * {
     *     View.ActionRequest += OnViewActionTriggered;  // ✅ One line using base helper!
     * }
     *
     * protected override void OnInitialize()
     * {
     *     // Manual CanExecute updates when state changes
     *     View.SelectionChanged += (s, e) => Dispatcher.RaiseCanExecuteChanged();
     * }
     *
     * protected override void RegisterViewActions()
     * {
     *     Dispatcher.Register(CommonActions.Save, OnSave,
     *         canExecute: () => View.HasUnsavedChanges);
     * }
     *
     * ✅ Pros: Explicit, debuggable, familiar .NET event pattern, F12 navigation works
     * ❌ Cons: Manual CanExecute updates needed, more View events to expose
     *
     *
     * WHEN TO USE EACH:
     * -----------------
     * Implicit: Simple CRUD, standard workflows, automatic CanExecute updates desired
     * Explicit: Complex logic, debugging needs, team prefers explicit code, learning scenarios
     *
     * TRADE-OFFS:
     * -----------
     * - Implicit pattern gives automatic CanExecute UI updates (buttons auto enable/disable)
     * - Explicit pattern requires manual Dispatcher.RaiseCanExecuteChanged() calls
     * - Explicit pattern prevents double-dispatch by returning null from ActionBinder property
     * - Both patterns can use Dispatcher.Register() with CanExecute predicates
     */

    #endregion
}
