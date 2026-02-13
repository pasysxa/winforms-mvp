using System;
using System.ComponentModel;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.ViewActions;

namespace WinformsMVP.Samples.EmailDemo
{
    /// <summary>
    /// 撰写邮件视图接口
    /// </summary>
    public interface IComposeEmailView : IWindowView, INotifyPropertyChanged
    {
        /// <summary>
        /// 收件人
        /// </summary>
        string To { get; set; }

        /// <summary>
        /// 邮件主题
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// 邮件正文
        /// </summary>
        string Body { get; set; }

        /// <summary>
        /// 撰写模式（New/Reply/Forward）
        /// </summary>
        ComposeMode Mode { get; set; }

        /// <summary>
        /// 是否正在保存
        /// </summary>
        bool IsSaving { set; }

        /// <summary>
        /// 是否已修改（用于关闭确认）
        /// </summary>
        bool IsDirty { get; set; }

        /// <summary>
        /// 启用/禁用输入控件
        /// </summary>
        bool EnableInput { set; }

        /// <summary>
        /// 绑定ViewAction
        /// </summary>
        void BindActions(ViewActionDispatcher dispatcher);
    }
}
