using System;

namespace WinformsMVP.Common.Events
{
    public class ExecutionRequestEventArgs<T, TResult> : EventArgs
    {
        public Func<T, TResult> Executor { get; }
        public T Param { get; }
        public Action<TResult> Callback { get; }

        public ExecutionRequestEventArgs(Func<T, TResult> executor, T param, Action<TResult> callback)
        {
            Executor = executor;
            Param = param;
            Callback = callback;
        }
    }
    public class ExecutionRequestEventArgs<T1, T2, TResult> : EventArgs
    {
        public Func<T1, T2, TResult> Executor { get; }
        public T1 Param1 { get; }
        public T2 Param2 { get; }
        public Action<TResult> Callback { get; }

        public ExecutionRequestEventArgs(Func<T1, T2, TResult> executor, T1 param1, T2 param2, Action<TResult> callback)
        {
            Executor = executor;
            Param1 = param1;
            Param2 = param2;
            Callback = callback;
        }
    }

    public class ExecutionRequestEventArgs<T1, T2, T3, TResult> : EventArgs
    {
        public Func<T1, T2, T3, TResult> Executor { get; }
        public T1 Param1 { get; }
        public T2 Param2 { get; }
        public T3 Param3 { get; }
        public Action<TResult> Callback { get; }

        public ExecutionRequestEventArgs(Func<T1, T2, T3, TResult> executor, T1 param1, T2 param2, T3 param3, Action<TResult> callback)
        {
            Executor = executor;
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
            Callback = callback;
        }
    }
}
