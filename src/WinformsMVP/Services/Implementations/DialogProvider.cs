using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using WinformsMVP.Common;
using WinformsMVP.Services.Implementations.DialogOptions;

namespace WinformsMVP.Services.Implementations
{
    public class DialogProvider : IDialogProvider
    {
        public InteractionResult<string> ShowOpenFileDialog(OpenFileDialogOptions options = null)
        {
            try
            {
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Filter = options?.Filter ?? "すべてのファイル (*.*)|*.*";
                    dialog.Multiselect = options?.Multiselect ?? false;
                    dialog.InitialDirectory = options?.InitialDirectory ?? "";
                    dialog.Title = options?.Title ?? "ファイルを開く";

                    var result = dialog.ShowDialog();
                    return result == DialogResult.OK
                        ? InteractionResult<string>.Ok(dialog.FileName)
                        : InteractionResult<string>.Cancel();
                }
            }
            catch (Exception ex)
            {
                return InteractionResult<string>.Error("ファイルを開くに失敗しました", ex);
            }
        }

        public InteractionResult<string> ShowSaveFileDialog(SaveFileDialogOptions options = null)
        {
            try
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = options?.Filter ?? "すべてのファイル (*.*)|*.*";
                    dialog.InitialDirectory = options?.InitialDirectory ?? "";
                    dialog.Title = options?.Title ?? "ファイルを保存";

                    var result = dialog.ShowDialog();
                    return result == DialogResult.OK
                        ? InteractionResult<string>.Ok(dialog.FileName)
                        : InteractionResult<string>.Cancel();
                }
            }
            catch (Exception ex)
            {
                return InteractionResult<string>.Error("ファイルの保存に失敗しました", ex);
            }
        }

        public InteractionResult<string> ShowFolderBrowserDialog(FolderBrowserDialogOptions options = null)
        {
            try
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    dialog.Description = options?.Description ?? "フォルダを選択してください";
                    dialog.SelectedPath = options?.SelectedPath ?? "";

                    var result = dialog.ShowDialog();
                    return result == DialogResult.OK
                        ? InteractionResult<string>.Ok(dialog.SelectedPath)
                        : InteractionResult<string>.Cancel();
                }
            }
            catch (Exception ex)
            {
                return InteractionResult<string>.Error("フォルダ選択に失敗しました", ex);
            }
        }

        // -----------------------
        // スタイル選択
        // -----------------------
        public InteractionResult<Color> ShowColorDialog(ColorDialogOptions options = null)
        {
            try
            {
                using (var dialog = new ColorDialog())
                {
                    dialog.Color = options?.InitialColor ?? Color.Black;

                    var result = dialog.ShowDialog();
                    return result == DialogResult.OK
                        ? InteractionResult<Color>.Ok(dialog.Color)
                        : InteractionResult<Color>.Cancel();
                }
            }
            catch (Exception ex)
            {
                return InteractionResult<Color>.Error("色選択に失敗しました", ex);
            }
        }

        public InteractionResult<Font> ShowFontDialog(FontDialogOptions options = null)
        {
            try
            {
                using (var dialog = new FontDialog())
                {
                    dialog.Font = options?.InitialFont ?? SystemFonts.DefaultFont;

                    var result = dialog.ShowDialog();
                    return result == DialogResult.OK
                        ? InteractionResult<Font>.Ok(dialog.Font)
                        : InteractionResult<Font>.Cancel();
                }
            }
            catch (Exception ex)
            {
                return InteractionResult<Font>.Error("フォント選択に失敗しました", ex);
            }
        }

        // -----------------------
        // 印刷設定
        // -----------------------
        public InteractionResult<PrinterSettings> ShowPrintDialog(PrintDialogOptions options = null)
        {
            try
            {
                using (var dialog = new PrintDialog())
                {
                    dialog.AllowSomePages = options?.AllowSomePages ?? true;
                    dialog.PrinterSettings = options?.PrinterSettings ?? new PrinterSettings();

                    var result = dialog.ShowDialog();
                    return result == DialogResult.OK
                        ? InteractionResult<PrinterSettings>.Ok(dialog.PrinterSettings)
                        : InteractionResult<PrinterSettings>.Cancel();
                }
            }
            catch (Exception ex)
            {
                return InteractionResult<PrinterSettings>.Error("印刷ダイアログに失敗しました", ex);
            }
        }

        public InteractionResult<PageSettings> ShowPageSetupDialog(PageSetupDialogOptions options = null)
        {
            try
            {
                using (var dialog = new PageSetupDialog())
                {
                    dialog.PageSettings = options?.PageSettings ?? new PageSettings();

                    var result = dialog.ShowDialog();
                    return result == DialogResult.OK
                        ? InteractionResult<PageSettings>.Ok(dialog.PageSettings)
                        : InteractionResult<PageSettings>.Cancel();
                }
            }
            catch (Exception ex)
            {
                return InteractionResult<PageSettings>.Error("ページ設定に失敗しました", ex);
            }
        }

        public InteractionResult<bool> ShowPrintPreviewDialog(PrintPreviewDialogOptions options = null)
        {
            try
            {
                using (var dialog = new PrintPreviewDialog())
                {
                    dialog.Document = options?.Document;

                    var result = dialog.ShowDialog();
                    return result == DialogResult.OK
                        ? InteractionResult<bool>.Ok(true)
                        : InteractionResult<bool>.Cancel();
                }
            }
            catch (Exception ex)
            {
                return InteractionResult<bool>.Error("印刷プレビューに失敗しました", ex);
            }
        }

    }
}
