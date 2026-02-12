using WinformsMVP.MVP.ViewActions;

namespace WinformsMVP.Core.Views
{
    /// <summary>
    /// Base interface for all views in the MVP framework.
    /// Provides framework-level abstractions for view-presenter interaction.
    /// </summary>
    public interface IViewBase
    {
        /// <summary>
        /// Gets the ViewActionBinder that maps UI controls to ViewAction keys.
        /// This is a framework-level abstraction similar to WPF's ICommand binding.
        ///
        /// The Presenter accesses this property to bind the ViewActionDispatcher:
        /// <code>
        /// View.ActionBinder.Bind(Dispatcher);
        /// </code>
        ///
        /// IMPORTANT: This is a property (not a method) to prevent accidental execution
        /// of business logic during testing or initialization. Properties expose data,
        /// not behavior, making testing safer and faster.
        ///
        /// Example implementation in View (Form):
        /// <code>
        /// private ViewActionBinder _actionBinder;
        /// public ViewActionBinder ActionBinder => _actionBinder;
        ///
        /// public MyForm()
        /// {
        ///     InitializeComponent();
        ///     InitializeActionBindings();
        /// }
        ///
        /// private void InitializeActionBindings()
        /// {
        ///     _actionBinder = new ViewActionBinder();
        ///     _actionBinder.Add(CommonActions.Save, _saveButton);
        ///     _actionBinder.Add(CommonActions.Delete, _deleteButton);
        /// }
        /// </code>
        /// </summary>
        ViewActionBinder ActionBinder { get; }
    }
}
