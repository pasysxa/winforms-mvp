using System;

namespace WinformsMVP.MVP.Presenters
{
    public interface IPresenter
    {
        Type ViewInterfaceType { get; }
    }
}
