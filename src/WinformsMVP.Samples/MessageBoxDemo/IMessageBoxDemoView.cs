using WinformsMVP.Core.Views;
using WinformsMVP.MVP.ViewActions;

namespace WinformsMVP.Samples.MessageBoxDemo
{
    public interface IMessageBoxDemoView : IWindowView
    {
        void BindActions(ViewActionDispatcher dispatcher);
    }
}
