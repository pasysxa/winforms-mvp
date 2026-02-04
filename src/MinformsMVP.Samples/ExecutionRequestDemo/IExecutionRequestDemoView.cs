using System;
using WinformsMVP.Common.Events;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.ViewActions;

namespace MinformsMVP.Samples.ExecutionRequestDemo
{
    /// <summary>
    /// View接口 for ExecutionRequest演示
    ///
    /// ⚠️ 重要：遵守 MVP 三条铁律
    /// 1. 视图接口不能包含 UI 类型（无 DialogResult、Type、Form 等）
    /// 2. 接口方法的参数和返回值必须是纯数据类型
    /// 3. 依赖方向：Presenter → View（视图不依赖 Presenter）
    ///
    /// ExecutionRequest 是为不熟悉 MVP 的开发者提供的学习工具
    /// 关键：参数和返回值必须是业务数据类型，不能是 UI 类型
    /// </summary>
    public interface IExecutionRequestDemoView : IWindowView
    {
        // ==================================================
        // ExecutionRequest 事件定义（符合三条铁律）
        // ==================================================

        /// <summary>
        /// 场景1：请求编辑客户信息
        /// ✅ 参数：CustomerData（要编辑的数据，null = 新建）
        /// ✅ 返回：CustomerData（编辑结果，null = 取消）
        /// </summary>
        event EventHandler<ExecutionRequestEventArgs<CustomerData, CustomerData>>
            EditCustomerRequested;

        /// <summary>
        /// 场景2：请求保存数据
        /// ✅ 参数：CustomerData（业务数据）
        /// ✅ 返回：bool（是否成功）
        /// </summary>
        event EventHandler<ExecutionRequestEventArgs<CustomerData, bool>>
            SaveDataRequested;

        // ==================================================
        // 显示方法
        // ==================================================

        /// <summary>
        /// 显示客户信息
        /// </summary>
        void ShowCustomerInfo(CustomerData data);

        /// <summary>
        /// 显示选中的文件（场景3：使用 IDialogProvider 的示例）
        /// </summary>
        void ShowSelectedFile(string filePath);

        /// <summary>
        /// 更新状态
        /// </summary>
        void UpdateStatus(string message, bool isSuccess);

        /// <summary>
        /// 绑定ViewActions
        /// </summary>
        void BindActions(ViewActionDispatcher dispatcher);
    }

    /// <summary>
    /// 客户数据传输对象
    /// </summary>
    public class CustomerData
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Email}, {Age}岁)";
        }
    }
}
