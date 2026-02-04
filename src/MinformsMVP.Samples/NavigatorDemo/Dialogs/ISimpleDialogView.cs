using WinformsMVP.Core.Views;
using WinformsMVP.MVP.ViewActions;

namespace MinformsMVP.Samples.NavigatorDemo
{
    public interface ISimpleDialogView : IWindowView
    {
        void SetMessage(string message);
        void BindActions(ViewActionDispatcher dispatcher);
    }
}
