using System;
using System.Windows.Forms;
using WinformsMVP.Common;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.Presenters;

namespace WinformsMVP.Services
{
    public interface IWindowNavigator
    {
        IWindowView ShowWindow<TPresenter, TResult>(
        TPresenter presenter,
        IWin32Window owner = null,
        Func<TPresenter, object> keySelector = null,
        Action<InteractionResult<TResult>> onClosed = null) where TPresenter : IPresenter;

        IWindowView ShowWindow<TPresenter>(TPresenter presenter,
        IWin32Window owner = null,
        Func<TPresenter, object> keySelector = null)
        where TPresenter : IPresenter;

        InteractionResult ShowWindowAsModal<TPresenter>(TPresenter presenter, IWin32Window owner = null) where TPresenter : IPresenter;

        InteractionResult<TResult> ShowWindowAsModal<TPresenter, TResult>(TPresenter presenter, IWin32Window owner = null) where TPresenter : IPresenter;
    }
}