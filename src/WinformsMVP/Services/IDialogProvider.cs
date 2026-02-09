using System.Drawing;
using System.Drawing.Printing;
using WinformsMVP.Common;
using WinformsMVP.Services.Implementations.DialogOptions;

namespace WinformsMVP.Services
{
    /// <summary>
    /// Provides standard WinForms dialog abstractions for file, folder, color, font, and print operations.
    /// </summary>
    public interface IDialogProvider
    {
        // File dialogs
        InteractionResult<string> ShowOpenFileDialog(OpenFileDialogOptions options = null);
        InteractionResult<string[]> ShowOpenMultipleFilesDialog(OpenFileDialogOptions options = null);
        InteractionResult<string> ShowSaveFileDialog(SaveFileDialogOptions options = null);
        InteractionResult<string> ShowFolderBrowserDialog(FolderBrowserDialogOptions options = null);

        // Style dialogs
        InteractionResult<Color> ShowColorDialog(ColorDialogOptions options = null);
        InteractionResult<Font> ShowFontDialog(FontDialogOptions options = null);

        // Print dialogs
        InteractionResult<PrinterSettings> ShowPrintDialog(PrintDialogOptions options = null);
        InteractionResult<PageSettings> ShowPageSetupDialog(PageSetupDialogOptions options = null);
        InteractionResult<bool> ShowPrintPreviewDialog(PrintPreviewDialogOptions options = null);
    }
}
