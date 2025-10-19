using System;
using WinformsMVP.Common.Events;

namespace WinformsMVP.MVP.Presenters
{
    public interface IRequestClose<TResult>
    {
        event EventHandler<CloseRequestedEventArgs<TResult>> CloseRequested;
        bool CanClose();
        bool TryGetResult(out TResult result);
    }
}
