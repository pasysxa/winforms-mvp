using System;

namespace WinformsMVP.Common.Events
{
    /// <summary>
    /// 执行结果包装类，同时管理事件订阅和最终结果
    /// 主要用于打开遗留窗体（非MVP窗体）时的订阅管理
    /// </summary>
    /// <typeparam name="TResult">窗体返回的结果类型</typeparam>
    public class ExecutionResult<TResult> : IDisposable
    {
        private readonly Action _disposeAction;
        private bool _isDisposed;

        /// <summary>
        /// 窗体返回的最终结果（null 表示用户取消）
        /// 对于模态窗体，在 ShowDialog 返回后立即可用
        /// 对于非模态窗体，通过 SetResult 设置后可用
        /// </summary>
        public TResult Result { get; private set; }

        /// <summary>
        /// 非模态窗体关闭时触发（携带最终结果）
        /// </summary>
        public event EventHandler<TResult> Completed;

        /// <summary>
        /// 构造函数：模态窗体 - 传结果和 Action（需要额外清理逻辑）
        /// </summary>
        /// <param name="result">窗体返回的结果</param>
        /// <param name="disposeAction">清理逻辑（解除事件订阅、释放资源等）</param>
        public ExecutionResult(TResult result, Action disposeAction)
        {
            Result = result;
            _disposeAction = disposeAction;
        }

        /// <summary>
        /// 构造函数：模态窗体 - 传结果和 IDisposable（最常用，直接传 Form）
        /// </summary>
        /// <param name="result">窗体返回的结果</param>
        /// <param name="disposable">需要释放的资源（通常是 Form）</param>
        public ExecutionResult(TResult result, IDisposable disposable)
        {
            Result = result;
            _disposeAction = () => disposable?.Dispose();
        }

        /// <summary>
        /// 构造函数：非模态窗体 - 传 Action（result 延迟设置）
        /// </summary>
        /// <param name="disposeAction">清理逻辑（解除事件订阅、释放资源等）</param>
        public ExecutionResult(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        /// <summary>
        /// 构造函数：非模态窗体 - 传 IDisposable（result 延迟设置）
        /// </summary>
        /// <param name="disposable">需要释放的资源（通常是 Form）</param>
        public ExecutionResult(IDisposable disposable)
        {
            _disposeAction = () => disposable?.Dispose();
        }

        /// <summary>
        /// 设置结果并触发 Completed 事件（非模态窗体用）
        /// </summary>
        /// <param name="result">窗体返回的最终结果</param>
        public void SetResult(TResult result)
        {
            Result = result;
            Completed?.Invoke(this, result);
        }

        /// <summary>
        /// 释放资源并执行清理逻辑
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _disposeAction?.Invoke();
                _isDisposed = true;
            }
        }
    }
}
