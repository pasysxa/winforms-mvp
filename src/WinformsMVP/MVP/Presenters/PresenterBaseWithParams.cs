using WinformsMVP.Core.Presenters;
using WinformsMVP.Core.Views;
using WinformsMVP.Services;

namespace WinformsMVP.MVP.Presenters
{
    public abstract class PresenterBase<TView, TParameters> : PresenterBase<TView> where TView : IViewBase
    {
        protected PresenterBase(TView view, ICommonServices services, IAppContext appContext)
            : base(view, services, appContext)
        {
        }

        public abstract void Initialize(TParameters parameters);
    }
}
