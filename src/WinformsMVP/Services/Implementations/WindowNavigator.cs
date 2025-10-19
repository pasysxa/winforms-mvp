using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<Type, Form> _openForms = new Dictionary<Type, Form>();

        public WindowNavigator(IViewMappingRegister viewMappingRegister)
        {
            _viewMappingRegister = viewMappingRegister;
        }

        // ----------------------------------------------------
        // 非模态窗口 (ShowWindow) - 核心修改点
        // ----------------------------------------------------
        public void ShowWindow(IPresenter presenter, IWin32Window owner = null)
        {
            Type viewInterfaceType = presenter.ViewInterfaceType;
            Form existingForm = null;

            if (_openForms.TryGetValue(viewInterfaceType, out Form cachedForm))
            {
                var openFormsSnapshot = Application.OpenForms.Cast<Form>().ToList();
                if (openFormsSnapshot.Contains(cachedForm))
                {
                    existingForm = cachedForm;
                }
                else
                {
                    _openForms.Remove(viewInterfaceType);
                }
            }

            if (existingForm != null)
            {
                existingForm.Activate();
                return;
            }

            var newForm = CreateAndBindForm(presenter, viewInterfaceType);
            if (newForm == null) return;

            EventHandler<CloseRequestedEventArgs<object>> closeHandler = null;
            FormClosingEventHandler formClosingHandler = null;
            FormClosedEventHandler formClosedHandler = null;

            if (presenter is IRequestClose<object> requestCloser)
            {
                // 1. CloseRequested -> Close
                closeHandler = (s, e) =>
                {
                    if (newForm.InvokeRequired)
                        newForm.BeginInvoke(new MethodInvoker(newForm.Close));
                    else
                        newForm.Close();
                };
                requestCloser.CloseRequested += closeHandler;

                // 2. FormClosing -> 可选 CanClose 检查
                formClosingHandler = (s, e) =>
                {
                    if (!requestCloser.CanClose())
                    {
                        e.Cancel = true;
                    }
                };
                newForm.FormClosing += formClosingHandler;

                // 3. FormClosed -> 清理事件 & 从 openForms 移除
                formClosedHandler = (s, e) =>
                {
                    requestCloser.CloseRequested -= closeHandler;
                    newForm.FormClosing -= formClosingHandler;
                    newForm.FormClosed -= formClosedHandler;
                    _openForms.Remove(viewInterfaceType);
                    newForm.Dispose();
                };
                newForm.FormClosed += formClosedHandler;
            }
            else
            {
                // 非 IRequestClose 窗体只需保证从 openForms 移除即可
                newForm.FormClosed += (s, e) =>
                {
                    _openForms.Remove(viewInterfaceType);
                    newForm.Dispose();
                };
            }

            _openForms.Add(viewInterfaceType, newForm);

            if (owner != null) newForm.Show(owner);
            else newForm.Show();
        }


        public InteractionResult<T> ShowWindowAsModal<T>(IPresenter presenter, IWin32Window owner = null)
        {
            if (!(presenter is IRequestClose<T> requestCloser))
                return InteractionResult<T>.Error($"Presenter {presenter.GetType().Name} 未实现 IRequestClose<{typeof(T).Name}>");

            // 创建窗体并绑定 View
            Type viewInterfaceType = presenter.ViewInterfaceType;
            var newForm = CreateAndBindForm(presenter, viewInterfaceType);
            if (newForm == null)
                return InteractionResult<T>.Error($"无法创建 View 实例: {viewInterfaceType.Name}");

            InteractionResult<T> result = InteractionResult<T>.Cancel();
            T pendingResult = default;
            bool closeRequestedFlag = false;

            // 保存事件引用，确保安全取消
            EventHandler<CloseRequestedEventArgs<T>> closeHandler = null;
            FormClosingEventHandler formClosingHandler = null;
            FormClosedEventHandler formClosedHandler = null;

            // 1. CloseRequested 触发窗体关闭
            closeHandler = (s, e) =>
            {
                pendingResult = e.Result;
                closeRequestedFlag = true;

                // Navigator 主动关闭窗体，触发 FormClosing
                newForm.Close();
            };
            requestCloser.CloseRequested += closeHandler;

            // 2. FormClosing 统一检查 CanClose
            formClosingHandler = (s, e) =>
            {
                if (!requestCloser.CanClose())
                {
                    e.Cancel = true; // 阻止关闭
                    return;
                }

                // 设置 InteractionResult
                if (closeRequestedFlag)
                    result = InteractionResult<T>.Ok(pendingResult);
                else
                    result = InteractionResult<T>.Cancel();
            };
            newForm.FormClosing += formClosingHandler;

            // 3. FormClosed 清理事件和资源
            formClosedHandler = (s, e) =>
            {
                requestCloser.CloseRequested -= closeHandler;
                newForm.FormClosing -= formClosingHandler;
                newForm.FormClosed -= formClosedHandler;
                newForm.Dispose();
            };
            newForm.FormClosed += formClosedHandler;

            // 4. 显示模态窗体
            try
            {
                if (owner != null)
                    newForm.ShowDialog(owner);
                else
                    newForm.ShowDialog();
            }
            catch (Exception ex)
            {
                result = InteractionResult<T>.Error("模态对话框显示期间发生运行时错误。", ex);
            }

            return result;
        }



        // ----------------------------------------------------
        // 模态对话框 (非泛型)
        // ----------------------------------------------------
        public InteractionResult ShowWindowAsModal(IPresenter presenter, IWin32Window owner = null)
        {
            // 仍然通过调用泛型版本，T=object
            return ShowWindowAsModal<object>(presenter, owner);
        }

        private Form CreateAndBindForm(IPresenter presenter, Type viewInterfaceType)
        {
            // 1. View インスタンスの作成
            Type viewImplType = _viewMappingRegister.GetViewImplementationType(viewInterfaceType);
            var newForm = Activator.CreateInstance(viewImplType) as Form;
            if (newForm == null) return null;

            // 2. View を Presenter に注入（AttachViewの呼び出し）
            InjectViewIntoPresenter(presenter, newForm as IViewBase);

            // 3. 業務ロジックの初期化 (Initializeの呼び出し)
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
                throw new InvalidOperationException($"Presenter {presenter.GetType().Name} がインターフェース {attacherType.Name} を正しく実装していません。");
            }
        }
    }
}
