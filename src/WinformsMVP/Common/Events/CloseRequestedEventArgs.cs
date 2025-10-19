using System;

namespace WinformsMVP.Common.Events
{
    public class CloseRequestedEventArgs<TResult> : EventArgs
    {
        public TResult Result { get; }

        public CloseRequestedEventArgs(TResult result)
        {
            Result = result;
        }
    }
}
