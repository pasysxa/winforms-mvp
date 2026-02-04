using System;

namespace WinformsMVP.Common.Events
{
    /// <summary>
    /// ExecutionRequest 事件参数 - 单参数版本
    /// View 通过此事件请求 Presenter 执行特定逻辑，并通过回调获取结果
    /// </summary>
    public class ExecutionRequestEventArgs<T, TResult> : EventArgs
    {
        public T Param { get; }
        public Action<TResult> Callback { get; }

        public ExecutionRequestEventArgs(T param, Action<TResult> callback)
        {
            Param = param;
            Callback = callback;
        }
    }

    /// <summary>
    /// ExecutionRequest 事件参数 - 双参数版本
    /// </summary>
    public class ExecutionRequestEventArgs<T1, T2, TResult> : EventArgs
    {
        public T1 Param1 { get; }
        public T2 Param2 { get; }
        public Action<TResult> Callback { get; }

        public ExecutionRequestEventArgs(T1 param1, T2 param2, Action<TResult> callback)
        {
            Param1 = param1;
            Param2 = param2;
            Callback = callback;
        }
    }

    /// <summary>
    /// ExecutionRequest 事件参数 - 三参数版本
    /// </summary>
    public class ExecutionRequestEventArgs<T1, T2, T3, TResult> : EventArgs
    {
        public T1 Param1 { get; }
        public T2 Param2 { get; }
        public T3 Param3 { get; }
        public Action<TResult> Callback { get; }

        public ExecutionRequestEventArgs(T1 param1, T2 param2, T3 param3, Action<TResult> callback)
        {
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
            Callback = callback;
        }
    }
}
