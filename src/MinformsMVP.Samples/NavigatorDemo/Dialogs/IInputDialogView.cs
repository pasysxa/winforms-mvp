using WinformsMVP.Core.Views;
using WinformsMVP.MVP.ViewActions;

namespace MinformsMVP.Samples.NavigatorDemo
{
    public interface IInputDialogView : IWindowView
    {
        void SetPrompt(string prompt);
        string GetInput();
        void BindActions(ViewActionDispatcher dispatcher);
    }
}
