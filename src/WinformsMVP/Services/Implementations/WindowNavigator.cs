using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WinformsMVP.Common;
using WinformsMVP.Common.Events;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.Presenters;

namespace WinformsMVP.Services.Implementations
{
    public class WindowNavigator : IWindowNavigator
    {
        private readonly IViewMappingRegister _viewMappingRegister;
        private readonly Dictionary<object, Form> _openForms = new Dictionary<object, Form>();
        private readonly object _lock = new object(); // 用于线程同步

        public WindowNavigator(IViewMappingRegister viewMappingRegister)
        {
            _viewMappingRegister = viewMappingRegister;
        }

        #region Modal (模态)

        public InteractionResult<TResult> ShowWindowAsModal<TPresenter, TResult>(TPresenter presenter, IWin32Window owner = null) where TPresenter : IPresenter
        {
            Type viewInterfaceType = presenter.ViewInterfaceType;
            var form = CreateAndBindForm(presenter, viewInterfaceType);
            if (form == null)
                return InteractionResult<TResult>.Error($"Viewインスタンスを作成できません: {viewInterfaceType.Name}");

            InteractionResult<TResult> result = InteractionResult<TResult>.Cancel();

            // 附加模态关闭处理器
            AttachModalCloseHandlers<TResult>(presenter, form, r => result = r);

            // WinForms 模态阻塞
            if (owner != null)
                form.ShowDialog(owner);
            else
                form.ShowDialog();

            // 模态窗口关闭后，立即释放 Presenter 资源
            (presenter as IDisposable)?.Dispose();

            return result;
        }

        public InteractionResult ShowWindowAsModal<TPresenter>(TPresenter presenter, IWin32Window owner = null)
        where TPresenter : IPresenter
        {
            // 内部调用泛型版本，使用 object 作为占位符结果类型
            return ShowWindowAsModal<TPresenter, object>(presenter, owner);
        }

        #endregion

        #region Non-Modal (非模态)

        public IWindowView ShowWindow<TPresenter, TResult>(
            TPresenter presenter,
            IWin32Window owner = null,
            Func<TPresenter, object> keySelector = null,
            Action<InteractionResult<TResult>> onClosed = null
            )
            where TPresenter : IPresenter
        {
            // 如果 onClosed 为 null，则使用一个安全的空操作，确保 ShowWindowInternal 不必进行 null 检查
            Action<InteractionResult<TResult>> finalOnClosed = onClosed ?? (r => { });

            return ShowWindowInternal(
            presenter,
            owner,
            keySelector,
            finalOnClosed
            );
        }

        public IWindowView ShowWindow<TPresenter>(TPresenter presenter,
        IWin32Window owner = null,
        Func<TPresenter, object> keySelector = null) where TPresenter : IPresenter
        {
            // 内部调用 ShowWindow<TPresenter, TResult> 的完整泛型版本，
            // TResult 设为 object，onClosed 设为 null。
            return ShowWindow<TPresenter, object>(
                presenter,
                owner,
                keySelector,
                onClosed: null // 这里 onClosed 是 null，因为这个重载没有提供回调参数
            );
        }
        #endregion

        #region Core (核心逻辑)

        private IWindowView ShowWindowInternal<TPresenter, TResult>(
            TPresenter presenter,
            IWin32Window owner,
            Func<TPresenter, object> keySelector,
            Action<InteractionResult<TResult>> onClosed)
            where TPresenter : IPresenter
        {
            object instanceKey = null;
            Form existingForm = null;

            // 1. **计算 Key 并检查单例/激活 (线程安全)**
            if (keySelector != null)
            {
                instanceKey = keySelector(presenter);
                lock (_lock)
                {
                    if (instanceKey != null && _openForms.TryGetValue(instanceKey, out existingForm))
                    {
                        if (existingForm != null && !existingForm.IsDisposed)
                        {
                            existingForm.Activate();
                            (presenter as IDisposable)?.Dispose(); // 释放新的 Presenter 实例
                            return (IWindowView)existingForm;
                        }
                        _openForms.Remove(instanceKey); // 清理无效的旧引用
                    }
                }
            }

            // 2. **创建新 Form**
            Type viewInterfaceType = presenter.ViewInterfaceType;
            var newForm = CreateAndBindForm(presenter, viewInterfaceType);

            if (newForm == null)
            {
                (presenter as IDisposable)?.Dispose();
                onClosed.Invoke(InteractionResult<TResult>.Error($"Viewインスタンスを作成できません: {viewInterfaceType.Name}"));
                return null;
            }

            // 3. **处理关闭逻辑 (非模态)**
            AttachNonModalCloseHandlers<TResult>(instanceKey, presenter, newForm, onClosed);

            // 4. **显示窗口**
            if (owner != null)
                newForm.Show(owner);
            else
                newForm.Show();

            // 5. **返回 IWindowView 句柄**
            return (IWindowView)newForm;
        }

        #endregion

        #region Close Handlers (关闭事件处理)

        private void AttachModalCloseHandlers<TResult>(
            IPresenter presenter,
            Form form,
            Action<InteractionResult<TResult>> setResultCallback)
        {
            var requestCloser = presenter as IRequestClose<TResult>;
            TResult finalResult = default(TResult);
            bool isCanceled = true; // 默认操作被用户取消

            EventHandler<CloseRequestedEventArgs<TResult>> closeRequestedHandler = null;
            FormClosingEventHandler formClosingHandler = null;
            FormClosedEventHandler formClosedHandler = null;

            // 1. **Presenter 主动请求关闭 (IRequestClose.CloseRequested)**
            if (requestCloser != null)
            {
                closeRequestedHandler = (s, e) =>
                {
                    // Presenter 已设置结果
                    finalResult = e.Result;
                    isCanceled = false; // 标记为 Presenter 主动关闭

                    // 触发 Form 关闭。这会接着触发 FormClosing 事件。
                    form.Close();
                };
                requestCloser.CloseRequested += closeRequestedHandler;
            }

            // 2. **View 询问是否可以关闭 (Form.FormClosing)**
            formClosingHandler = (s, e) =>
            {
                // 只有在被动关闭 (用户点击 X) 且 Presenter 不允许关闭时，才取消关闭。
                if (requestCloser != null && isCanceled && !requestCloser.CanClose())
                {
                    e.Cancel = true; // 取消关闭
                }
            };
            form.FormClosing += formClosingHandler;

            // 3. **View 实际关闭后 (Form.FormClosed)**
            formClosedHandler = (s, e) =>
            {
                // A. 立即解除所有事件订阅，防止泄漏
                form.FormClosed -= formClosedHandler;
                form.FormClosing -= formClosingHandler;
                if (requestCloser != null)
                {
                    requestCloser.CloseRequested -= closeRequestedHandler;
                }

                // B. 封装最终结果
                InteractionResult<TResult> result;
                if (!isCanceled)
                {
                    // 结果由 Presenter 设定 (通过 CloseRequested)
                    result = InteractionResult<TResult>.Ok(finalResult);
                }
                else if (requestCloser != null && requestCloser.TryGetResult(out finalResult))
                {
                    // 用户取消，但 Presenter 仍持有有效结果
                    result = InteractionResult<TResult>.Ok(finalResult);
                }
                else
                {
                    // 默认取消
                    result = InteractionResult<TResult>.Cancel();
                }

                // C. 通过回调返回结果
                setResultCallback.Invoke(result);

                // D. 释放 Form 资源
                form.Dispose();
            };
            form.FormClosed += formClosedHandler;
        }

        private void AttachNonModalCloseHandlers<TResult>(
        object instanceKey,
        IPresenter presenter,
        Form form,
        Action<InteractionResult<TResult>> onClosed)
        {
            var requestCloser = presenter as IRequestClose<TResult>;

            // 我们需要声明事件处理器变量，以便在 FormClosed 中能够解除订阅
            EventHandler<CloseRequestedEventArgs<TResult>> closeRequestedHandler = null;
            FormClosingEventHandler formClosingHandler = null;
            FormClosedEventHandler formClosedHandler = null;

            // 默认假设关闭是被动（用户）行为，需要走 CanClose 流程
            bool isPresenterClosing = false;

            // 1. **注册到 _openForms 字典 (线程安全)**
            if (instanceKey != null)
            {
                lock (_lock)
                {
                    if (!_openForms.ContainsKey(instanceKey))
                    {
                        _openForms.Add(instanceKey, form);
                    }
                }
            }

            // 2. **处理 Presenter 主动请求关闭 (IRequestClose.CloseRequested)**
            if (requestCloser != null)
            {
                closeRequestedHandler = (s, e) =>
                {
                    // 在 Form.Close() 之前设置标志，避免 FormClosing 错误地调用 CanClose
                    isPresenterClosing = true;

                    // Note: 结果 e.Result 会通过 TryGetResult 的内部机制在 Presenter 中缓存，
                    // 最终在 FormClosed 中通过 TryGetResult 提取。

                    form.Close();
                };
                requestCloser.CloseRequested += closeRequestedHandler;
            }

            // 3. **处理 View 询问是否可以关闭 (Form.FormClosing)**
            formClosingHandler = (s, e) =>
            {
                // 只有当不是 Presenter 主动关闭时，才需要检查 CanClose
                if (!isPresenterClosing && requestCloser != null && !requestCloser.CanClose())
                {
                    e.Cancel = true; // 阻止关闭
                }
            };
            form.FormClosing += formClosingHandler;


            // 4. **处理 FormClosed 事件：解除框架引用、释放资源和回调**
            formClosedHandler = (s, e) =>
            {
                // A. 清理框架状态 (线程安全)
                if (instanceKey != null)
                {
                    lock (_lock)
                    {
                        _openForms.Remove(instanceKey);
                    }
                }

                // B. 获取结果并触发回调
                InteractionResult<TResult> result;
                TResult closeResult;

                // 检查结果是否就绪
                if (requestCloser != null && requestCloser.TryGetResult(out closeResult))
                {
                    // 无论是 Presenter 还是用户关闭，只要 Presenter 成功设置了结果
                    result = InteractionResult<TResult>.Ok(closeResult);
                }
                else
                {
                    // 否则视为取消
                    result = InteractionResult<TResult>.Cancel();
                }

                onClosed.Invoke(result);

                // C. 释放 Presenter 资源
                (presenter as IDisposable)?.Dispose();

                // D. 清理所有事件订阅和 Form 资源
                form.FormClosed -= formClosedHandler;
                form.FormClosing -= formClosingHandler; // 解除 FormClosing
                if (requestCloser != null)
                {
                    requestCloser.CloseRequested -= closeRequestedHandler; // 解除 Presenter 事件
                }
                form.Dispose();
            };
            form.FormClosed += formClosedHandler;
        }

        #endregion

        #region Utility (工具方法)

        private Form CreateAndBindForm(IPresenter presenter, Type viewInterfaceType)
        {
            // 1. View インスタンスの作成 (查找映射和创建实例)
            Type viewImplType = _viewMappingRegister.GetViewImplementationType(viewInterfaceType);
            var newForm = Activator.CreateInstance(viewImplType) as Form;
            if (newForm == null) return null;

            // 关键：确保 View 实现了 IWindowView 接口
            if (!(newForm is IWindowView))
            {
                throw new InvalidOperationException($"View {newForm.GetType().Name} 必须实现 IWindowView 接口以支持 WindowNavigator 的非模态功能.");
            }

            // 2. View を Presenter に注入
            InjectViewIntoPresenter(presenter, newForm as IViewBase);

            // 3. 業務ロジックの初期化
            if (presenter is IInitializable initializable)
            {
                initializable.Initialize();
            }

            return newForm;
        }

        private void InjectViewIntoPresenter(IPresenter presenter, IViewBase viewInstance)
        {
            Type viewInterfaceType = presenter.ViewInterfaceType;
            Type attacherType = typeof(IViewAttacher<>).MakeGenericType(viewInterfaceType);

            var attachMethod = attacherType.GetMethod("AttachView");

            if (attachMethod != null)
            {
                attachMethod.Invoke(presenter, new object[] { viewInstance });
            }
            else
            {
                throw new InvalidOperationException($"Presenter {presenter.GetType().Name} がインターフェース {attacherType.Name} を正しく実装していません。");
            }
        }

        #endregion
    }
}
