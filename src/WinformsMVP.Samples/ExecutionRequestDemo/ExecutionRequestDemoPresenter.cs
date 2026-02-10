using System;
using System.IO;
using WinformsMVP.Common.Events;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Services;

namespace WinformsMVP.Samples.ExecutionRequestDemo
{
    public static class ExecutionRequestDemoActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("ExecReqDemo");

        public static readonly ViewAction EditCustomer = Factory.Create("EditCustomer");
        public static readonly ViewAction SelectFile = Factory.Create("SelectFile");  // 使用 IDialogProvider
        public static readonly ViewAction SaveData = Factory.Create("SaveData");
    }

    /// <summary>
    /// Presenter for ExecutionRequest演示
    /// 演示 ExecutionRequest 正确用法和依赖注入的对比
    /// </summary>
    public class ExecutionRequestDemoPresenter : WindowPresenterBase<IExecutionRequestDemoView>
    {
        private CustomerData _currentCustomer;
        private string _currentFilePath;

        protected override void OnViewAttached()
        {
            // 订阅 ExecutionRequest 事件（学习工具，符合三条铁律）
            View.EditCustomerRequested += OnEditCustomerRequested;
            View.SaveDataRequested += OnSaveDataRequested;
        }

        protected override void RegisterViewActions()
        {
            // 注册 ViewActions
            _dispatcher.Register(
                ExecutionRequestDemoActions.EditCustomer,
                OnEditCustomerAction);

            // ✅ 推荐做法 - 使用 IDialogProvider 处理文件选择
            _dispatcher.Register(
                ExecutionRequestDemoActions.SelectFile,
                OnSelectFileAction);

            _dispatcher.Register(
                ExecutionRequestDemoActions.SaveData,
                OnSaveDataAction,
                canExecute: () => _currentCustomer != null);

            View.BindActions(_dispatcher);
        }

        protected override void OnInitialize()
        {
            View.UpdateStatus("准备就绪。场景1 使用 ExecutionRequest（学习工具，View打开遗留窗体），场景2 使用 IDialogProvider（推荐）。", true);
        }

        #region ViewAction Handlers

        private void OnEditCustomerAction()
        {
            // ViewAction 触发，实际逻辑由 ExecutionRequest 处理
            // 这里什么都不做，真正的逻辑在 OnEditCustomerRequested 中
        }

        /// <summary>
        /// ✅ 正确做法：直接使用 IDialogProvider，不使用 ExecutionRequest
        /// </summary>
        private void OnSelectFileAction()
        {
            var options = new WinformsMVP.Services.Implementations.DialogOptions.OpenFileDialogOptions
            {
                Filter = "Text Files|*.txt|CSV Files|*.csv|All Files|*.*",
                Title = "选择文件"
            };

            var result = Dialogs.ShowOpenFileDialog(options);

            if (result.Status == WinformsMVP.Common.InteractionStatus.Ok)
            {
                _currentFilePath = result.Value;
                View.ShowSelectedFile(_currentFilePath);
                View.UpdateStatus($"文件已选择：{System.IO.Path.GetFileName(_currentFilePath)}", true);
            }
            else
            {
                View.UpdateStatus("未选择文件", false);
            }
        }

        private void OnSaveDataAction()
        {
            // ViewAction 触发，实际逻辑由 ExecutionRequest 处理
        }

        #endregion

        #region ExecutionRequest Handlers

        /// <summary>
        /// 场景1：处理编辑客户信息的请求
        /// ✅ 符合铁律：参数和返回值都是业务数据类型（CustomerData）
        ///
        /// 正确的ExecutionRequest流程：
        /// 1. View自己打开遗留窗体获取用户输入（View负责UI）
        /// 2. View将编辑后的业务数据通过ExecutionRequest事件传递给Presenter
        /// 3. Presenter处理业务逻辑（验证、保存等）
        /// 4. Presenter通过回调返回处理结果
        /// </summary>
        private void OnEditCustomerRequested(object sender,
            ExecutionRequestEventArgs<CustomerData, CustomerData> e)
        {
            try
            {
                // View已经打开遗留窗体并获取了编辑后的数据
                // Presenter只负责业务逻辑处理
                var editedData = e.Param;  // ✅ View传递的业务数据

                if (editedData != null)
                {
                    // 执行业务逻辑（验证、处理等）
                    // 这里可以添加验证逻辑
                    if (string.IsNullOrWhiteSpace(editedData.Name))
                    {
                        Messages.ShowWarning("客户姓名不能为空", "验证失败");
                        e.Callback?.Invoke(null);
                        return;
                    }

                    // 保存当前数据
                    _currentCustomer = editedData;

                    // 更新 CanExecute
                    _dispatcher.RaiseCanExecuteChanged();

                    // 返回处理后的数据
                    e.Callback?.Invoke(editedData);  // ✅ 返回业务数据
                }
                else
                {
                    // 用户取消编辑
                    e.Callback?.Invoke(null);
                }
            }
            catch (Exception ex)
            {
                Messages.ShowError($"处理失败: {ex.Message}", "错误");
                e.Callback?.Invoke(null);  // ✅ 失败返回 null
            }
        }


        /// <summary>
        /// 场景2：处理保存数据请求
        /// ✅ 符合铁律：参数和返回值都是业务数据类型
        /// </summary>
        private void OnSaveDataRequested(object sender,
            ExecutionRequestEventArgs<CustomerData, bool> e)
        {
            try
            {
                // Presenter 执行业务逻辑
                var result = SaveCustomerData(e.Param);
                e.Callback?.Invoke(result);  // ✅ 返回 bool
            }
            catch (Exception ex)
            {
                Messages.ShowError($"保存失败: {ex.Message}", "错误");
                e.Callback?.Invoke(false);
            }
        }

        #endregion

        #region Business Logic - Presenter 的业务逻辑实现

        /// <summary>
        /// Presenter提供的保存数据逻辑
        /// ✅ 只处理业务逻辑，不涉及UI
        /// </summary>
        private bool SaveCustomerData(CustomerData data)
        {
            try
            {
                // 模拟保存到文件或数据库
                var message = $"客户数据已保存:\n" +
                             $"姓名: {data.Name}\n" +
                             $"邮箱: {data.Email}\n" +
                             $"年龄: {data.Age}";

                Messages.ShowInfo(message, "保存成功");
                View.UpdateStatus("客户数据保存成功", true);

                return true;
            }
            catch (Exception ex)
            {
                Messages.ShowError($"保存失败: {ex.Message}", "错误");
                View.UpdateStatus("保存失败", false);
                return false;
            }
        }

        #endregion

        protected override void Cleanup()
        {
            // 取消事件订阅
            if (View != null)
            {
                View.EditCustomerRequested -= OnEditCustomerRequested;
                View.SaveDataRequested -= OnSaveDataRequested;
            }
        }
    }
}
