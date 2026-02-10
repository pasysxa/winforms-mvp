using System;
using System.Windows.Forms;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Services;

namespace WinformsMVP.Samples
{
    /// <summary>
    /// Example showing ActionKeys for actions that require parameters.
    /// </summary>
    public static class DocumentActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("Document");

        public static readonly ViewAction Open = Factory.Create("Open");
        public static readonly ViewAction SaveAs = Factory.Create("SaveAs");
        public static readonly ViewAction Export = Factory.Create("Export");
    }

    #region View Interface

    public interface IDocumentEditorView : IWindowView
    {
        Button OpenButton { get; }
        Button SaveAsButton { get; }
        Button ExportButton { get; }
        ComboBox ExportFormatComboBox { get; }

        string DocumentPath { get; set; }
        bool IsDocumentLoaded { get; }
    }

    #endregion

    /// <summary>
    /// Example presenter demonstrating ViewAction with typed parameters.
    /// Shows how to use the ViewActionDispatcher's generic Register method.
    /// Best practice: Uses IMessageService instead of MessageBox (follows MVP principle).
    /// </summary>
    public class DocumentEditorPresenter : WindowPresenterBase<IDocumentEditorView>
    {
        private readonly ViewActionBinder _binder = new ViewActionBinder();
        private readonly IMessageService _messageService;

        public DocumentEditorPresenter(IMessageService messageService)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }

        protected override void OnViewAttached()
        {
            _binder.Add(DocumentActions.Open, View.OpenButton);
            _binder.Add(DocumentActions.SaveAs, View.SaveAsButton);
            _binder.Add(DocumentActions.Export, View.ExportButton);
        }

        protected override void RegisterViewActions()
        {
            // Register action with string parameter
            _dispatcher.Register<string>(
                DocumentActions.Open,
                OnOpenDocument,
                canExecute: () => true);

            // Register action with string parameter and CanExecute
            _dispatcher.Register<string>(
                DocumentActions.SaveAs,
                OnSaveDocumentAs,
                canExecute: () => View.IsDocumentLoaded);

            // Register action with custom type parameter
            _dispatcher.Register<ExportOptions>(
                DocumentActions.Export,
                OnExportDocument,
                canExecute: () => View.IsDocumentLoaded);

            // Bind controls to dispatcher
            _binder.Bind(_dispatcher);

            // Wire up button clicks to dispatch with parameters
            WireUpParameterizedActions();
        }

        protected override void OnInitialize()
        {
            // Initialize
        }

        private void WireUpParameterizedActions()
        {
            // For parameterized actions, you need to manually wire the parameter passing
            // The binder handles the click event, but parameter construction is custom

            // Alternative approach: Don't use binder for these, wire directly
            View.OpenButton.Click += (s, e) =>
            {
                using (var dialog = new OpenFileDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _dispatcher.Dispatch(DocumentActions.Open, dialog.FileName);
                    }
                }
            };

            View.SaveAsButton.Click += (s, e) =>
            {
                using (var dialog = new SaveFileDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _dispatcher.Dispatch(DocumentActions.SaveAs, dialog.FileName);
                    }
                }
            };

            View.ExportButton.Click += (s, e) =>
            {
                var options = new ExportOptions
                {
                    Format = View.ExportFormatComboBox.SelectedItem?.ToString() ?? "PDF",
                    IncludeImages = true
                };
                _dispatcher.Dispatch(DocumentActions.Export, options);
            };
        }

        #region Action Handlers

        private void OnOpenDocument(string filePath)
        {
            View.DocumentPath = filePath;
            // Best practice: Use IMessageService instead of MessageBox (MVP principle)
            _messageService.ShowInfo($"Opening document: {filePath}");

            // Note: UI state automatically updates after action execution
        }

        private void OnSaveDocumentAs(string filePath)
        {
            // Best practice: Use IMessageService instead of MessageBox (MVP principle)
            _messageService.ShowInfo($"Saving document as: {filePath}");
        }

        private void OnExportDocument(ExportOptions options)
        {
            // Best practice: Use IMessageService instead of MessageBox (MVP principle)
            _messageService.ShowInfo($"Exporting document to {options.Format}");
        }

        #endregion
    }

    /// <summary>
    /// Example of a custom parameter type for actions.
    /// </summary>
    public class ExportOptions
    {
        public string Format { get; set; }
        public bool IncludeImages { get; set; }
        public int Quality { get; set; } = 90;
    }
}
