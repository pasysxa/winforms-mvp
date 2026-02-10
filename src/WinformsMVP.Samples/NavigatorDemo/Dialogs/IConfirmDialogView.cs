using WinformsMVP.Core.Views;
using WinformsMVP.MVP.ViewActions;

namespace WinformsMVP.Samples.NavigatorDemo
{
    public interface IConfirmDialogView : IWindowView
    {
        void SetTitle(string title);
        void SetMessage(string message);
        void SetDefaultButton(bool yesIsDefault);
        void BindActions(ViewActionDispatcher dispatcher);
    }
}
