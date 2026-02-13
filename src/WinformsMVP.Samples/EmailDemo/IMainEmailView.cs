using System;
using System.Collections.Generic;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Samples.EmailDemo.Models;

namespace WinformsMVP.Samples.EmailDemo
{
    /// <summary>
    /// 主邮件视图接口
    /// </summary>
    public interface IMainEmailView : IWindowView
    {
        /// <summary>
        /// 邮件列表
        /// </summary>
        IEnumerable<EmailMessage> Emails { set; }

        /// <summary>
        /// 当前选中的邮件
        /// </summary>
        EmailMessage SelectedEmail { get; }

        /// <summary>
        /// 是否有选中的邮件
        /// </summary>
        bool HasSelection { get; }

        /// <summary>
        /// 当前文件夹
        /// </summary>
        EmailFolder CurrentFolder { get; set; }

        /// <summary>
        /// 未读邮件数量
        /// </summary>
        int UnreadCount { set; }

        /// <summary>
        /// 状态栏消息
        /// </summary>
        string StatusMessage { set; }

        /// <summary>
        /// 是否正在加载
        /// </summary>
        bool IsLoading { set; }

        /// <summary>
        /// 显示邮件预览面板
        /// </summary>
        void ShowEmailPreview(EmailMessage email);

        /// <summary>
        /// 清除邮件预览
        /// </summary>
        void ClearEmailPreview();

        /// <summary>
        /// 刷新邮件列表显示
        /// </summary>
        void RefreshEmailList();

        /// <summary>
        /// 邮件选择改变事件
        /// </summary>
        event EventHandler EmailSelectionChanged;

        /// <summary>
        /// 文件夹改变事件
        /// </summary>
        event EventHandler<EmailFolder> FolderChanged;

        /// <summary>
        /// 绑定ViewAction
        /// </summary>
        void BindActions(ViewActionDispatcher dispatcher);
    }
}
