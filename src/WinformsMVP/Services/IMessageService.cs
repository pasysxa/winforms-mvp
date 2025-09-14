using System.Windows.Forms;
using WinformsMVP.Common;

namespace WinformsMVP.Services
{
    public interface IMessageService
    {
        bool ConfirmYesNo(string text, string caption = "");
        bool ConfirmOkCancel(string text, string caption = "");
        DialogResult ConfirmYesNoCancel(string text, string caption = "");
        void ShowInfo(string text, string caption = "");
        void ShowWarning(string text, string caption = "");
        void ShowError(string text, string caption = "");

        bool ConfirmYesNoAt(string text, string caption = "");
        bool ConfirmOkCancelAt(string text, string caption = "");
        DialogResult ConfirmYesNoCancelAt(string text, string caption = "");
        void ShowInfoAt(string text, string caption = "");
        void ShowWarningAt(string text, string caption = "");
        void ShowErrorAt(string text, string caption = "");

        void ShowTotast(string text, ToastType type, int duration = 1000);

    }
}
