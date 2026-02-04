using WinformsMVP.Core.Views;
using WinformsMVP.MVP.ViewActions;

namespace MinformsMVP.Samples.NavigatorDemo
{
    /// <summary>
    /// Main demo view for WindowNavigator features.
    /// </summary>
    public interface INavigatorDemoView : IWindowView
    {
        /// <summary>
        /// Update the log/output area
        /// </summary>
        void AppendLog(string message);

        /// <summary>
        /// Bind UI controls to actions
        /// </summary>
        void BindActions(ViewActionDispatcher dispatcher);
    }
}
