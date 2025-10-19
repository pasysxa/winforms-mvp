using WinformsMVP.Core.Presenters;
using WinformsMVP.Core.Views;

namespace WinformsMVP.MVP.Presenters
{
    public abstract class PresenterBase<TView, TParameters> : PresenterBase<TView> where TView : IViewBase
    {
        //protected PresenterBase(TView view, ICommonServices services = null, IAppContext appContext = null)
        //    : base(view, services, appContext)
        //{
        //}

        public abstract void Initialize(TParameters parameters);
    }
}
