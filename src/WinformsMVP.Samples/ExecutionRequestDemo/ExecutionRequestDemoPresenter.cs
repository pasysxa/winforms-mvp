using System;
using System.IO;
using System.Windows.Forms;
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
            View.UpdateStatus("准备就绪。场景1、3 使用 ExecutionRequest（学习工具），场景2 使用 IDialogProvider（推荐）。", true);
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
        /// </summary>
        private void OnEditCustomerRequested(object sender,
            ExecutionRequestEventArgs<CustomerData, CustomerData> e)
        {
            try
            {
                // Presenter 自己决定如何编辑（可以打开遗留窗体、新窗体、Web 页面等）
                var initialData = e.Param;  // ✅ 业务数据
                var result = EditCustomerUsingLegacyForm(initialData);

                // 通过回调返回结果
                e.Callback?.Invoke(result);  // ✅ 返回业务数据或 null
            }
            catch (Exception ex)
            {
                Messages.ShowError($"编辑失败: {ex.Message}", "错误");
                e.Callback?.Invoke(null);  // ✅ 失败返回 null
            }
        }


        /// <summary>
        /// 场景3：处理保存数据请求
        /// ✅ 符合铁律：参数和返回值都是业务数据类型
        /// </summary>
        private void OnSaveDataRequested(object sender,
            ExecutionRequestEventArgs<CustomerData, bool> e)
        {
            try
            {
                // Presenter 自己执行逻辑
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
        /// 使用遗留窗体编辑客户信息
        /// ✅ 注意：这个方法在 Presenter 内部，可以使用 UI 类型（Form、DialogResult）
        /// ✅ 但它不暴露给外部，接口层仍然是纯业务数据
        /// </summary>
        private CustomerData EditCustomerUsingLegacyForm(CustomerData initialData)
        {
            // 创建遗留窗体实例
            var form = new LegacyCustomerForm();

            // 如果有初始数据，预填充
            if (initialData != null)
            {
                form.CustomerName = initialData.Name;
                form.Email = initialData.Email;
                form.Age = initialData.Age;
            }

            // 以模态方式打开（这里可以使用 UI 类型，因为是实现细节）
            var dialogResult = form.ShowDialog();

            // 获取结果
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                var editedData = new CustomerData
                {
                    Name = form.CustomerName,
                    Email = form.Email,
                    Age = form.Age
                };

                // 保存当前数据
                _currentCustomer = editedData;

                // 更新 CanExecute
                _dispatcher.RaiseCanExecuteChanged();

                return editedData;  // ✅ 返回业务数据
            }

            form.Dispose();
            return null;  // ✅ 用户取消返回 null（不使用 DialogResult）
        }


        /// <summary>
        /// Presenter提供的保存数据逻辑
        /// </summary>
        public bool SaveCustomerData(CustomerData data)
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
