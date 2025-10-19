using System.Drawing;
using System.Drawing.Printing;
using WinformsMVP.Common;
using WinformsMVP.Services.Implementations.DialogOptions;

namespace WinformsMVP.Services
{
    public interface IDialogProvider
    {
        InteractionResult<string> ShowOpenFileDialog(OpenFileDialogOptions options = null);
        InteractionResult<string> ShowSaveFileDialog(SaveFileDialogOptions options = null);
        InteractionResult<string> ShowFolderBrowserDialog(FolderBrowserDialogOptions options = null);
        InteractionResult<Color> ShowColorDialog(ColorDialogOptions options = null);
        InteractionResult<Font> ShowFontDialog(FontDialogOptions options = null);
        InteractionResult<PrinterSettings> ShowPrintDialog(PrintDialogOptions options = null);
        InteractionResult<PageSettings> ShowPageSetupDialog(PageSetupDialogOptions options = null);
    }
}
