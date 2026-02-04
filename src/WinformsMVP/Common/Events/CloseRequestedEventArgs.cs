using System;
using WinformsMVP.Common;

namespace WinformsMVP.Common.Events
{
    public class CloseRequestedEventArgs<TResult> : EventArgs
    {
        public TResult Result { get; }
        public InteractionStatus Status { get; }

        public CloseRequestedEventArgs(TResult result, InteractionStatus status = InteractionStatus.Ok)
        {
            Result = result;
            Status = status;
        }
    }
}
