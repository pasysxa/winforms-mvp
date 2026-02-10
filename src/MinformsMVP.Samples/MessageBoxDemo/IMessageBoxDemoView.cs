using WinformsMVP.Core.Views;
using WinformsMVP.MVP.ViewActions;

namespace MinformsMVP.Samples.MessageBoxDemo
{
    public interface IMessageBoxDemoView : IWindowView
    {
        void BindActions(ViewActionDispatcher dispatcher);
    }
}
