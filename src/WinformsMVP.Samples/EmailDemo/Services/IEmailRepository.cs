using System.Collections.Generic;
using System.Threading.Tasks;
using WinformsMVP.Samples.EmailDemo.Models;

namespace WinformsMVP.Samples.EmailDemo.Services
{
    /// <summary>
    /// 邮件存储库接口
    /// </summary>
    public interface IEmailRepository
    {
        /// <summary>
        /// 获取指定文件夹的所有邮件
        /// </summary>
        Task<IEnumerable<EmailMessage>> GetEmailsByFolderAsync(EmailFolder folder);

        /// <summary>
        /// 根据ID获取邮件
        /// </summary>
        Task<EmailMessage> GetEmailByIdAsync(int id);

        /// <summary>
        /// 发送邮件（移动到已发送文件夹）
        /// </summary>
        Task<bool> SendEmailAsync(EmailMessage email);

        /// <summary>
        /// 保存草稿
        /// </summary>
        Task<int> SaveDraftAsync(EmailMessage email);

        /// <summary>
        /// 删除邮件（移动到垃圾箱）
        /// </summary>
        Task<bool> DeleteEmailAsync(int id);

        /// <summary>
        /// 永久删除邮件
        /// </summary>
        Task<bool> PermanentlyDeleteEmailAsync(int id);

        /// <summary>
        /// 标记邮件为已读/未读
        /// </summary>
        Task<bool> MarkAsReadAsync(int id, bool isRead);

        /// <summary>
        /// 标记邮件加星/取消加星
        /// </summary>
        Task<bool> ToggleStarAsync(int id);

        /// <summary>
        /// 搜索邮件
        /// </summary>
        Task<IEnumerable<EmailMessage>> SearchEmailsAsync(string keyword, EmailFolder? folder = null);

        /// <summary>
        /// 获取未读邮件数量
        /// </summary>
        Task<int> GetUnreadCountAsync(EmailFolder folder);
    }
}
