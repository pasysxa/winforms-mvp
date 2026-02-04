using WinformsMVP.Core.Views;
using WinformsMVP.MVP.ViewActions;

namespace MinformsMVP.Samples.NavigatorDemo
{
    public interface INonModalWindowView : IWindowView
    {
        void SetCounter(int value);
        void BindActions(ViewActionDispatcher dispatcher);
    }
}
