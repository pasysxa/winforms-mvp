using WinformsMVP.Core.Presenters;
using WinformsMVP.Core.Views;

namespace WinformsMVP.MVP.Presenters
{
    public abstract class PresenterBase<TView, TParameters> : PresenterBase<TView> where TView : IViewBase
    {
        public abstract void Initialize(TParameters parameters);
    }
}
