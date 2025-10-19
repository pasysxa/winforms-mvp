using System.Windows.Forms;
using WinformsMVP.Common;
using WinformsMVP.MVP.Presenters;

namespace WinformsMVP.Services
{
    public interface IWindowNavigator
    {
        void ShowWindow(IPresenter presenter, IWin32Window owner = null);

        InteractionResult ShowWindowAsModal(IPresenter presenter, IWin32Window owner = null);

        InteractionResult<T> ShowWindowAsModal<T>(IPresenter presenter, IWin32Window owner = null);
    }
}
