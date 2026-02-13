using System;

namespace WinformsMVP.Samples.EmailDemo.Models
{
    /// <summary>
    /// 邮件消息数据模型
    /// </summary>
    public class EmailMessage : ICloneable
    {
        /// <summary>邮件ID</summary>
        public int Id { get; set; }

        /// <summary>发件人邮箱</summary>
        public string From { get; set; }

        /// <summary>收件人邮箱</summary>
        public string To { get; set; }

        /// <summary>邮件主题</summary>
        public string Subject { get; set; }

        /// <summary>邮件正文</summary>
        public string Body { get; set; }

        /// <summary>接收/发送日期</summary>
        public DateTime Date { get; set; }

        /// <summary>是否已读</summary>
        public bool IsRead { get; set; }

        /// <summary>是否加星标</summary>
        public bool IsStarred { get; set; }

        /// <summary>所属文件夹</summary>
        public EmailFolder Folder { get; set; }

        /// <summary>
        /// 创建邮件的深拷贝（用于ChangeTracker）
        /// </summary>
        public object Clone()
        {
            return new EmailMessage
            {
                Id = this.Id,
                From = this.From,
                To = this.To,
                Subject = this.Subject,
                Body = this.Body,
                Date = this.Date,
                IsRead = this.IsRead,
                IsStarred = this.IsStarred,
                Folder = this.Folder
            };
        }

        /// <summary>
        /// 获取邮件预览摘要（用于列表显示）
        /// </summary>
        public string GetBodyPreview(int maxLength = 50)
        {
            if (string.IsNullOrWhiteSpace(Body))
                return "(No content)";

            var preview = Body.Replace("\r\n", " ").Replace("\n", " ");
            if (preview.Length > maxLength)
                return preview.Substring(0, maxLength) + "...";

            return preview;
        }

        /// <summary>
        /// 获取显示用的发件人名称（简化显示）
        /// </summary>
        public string GetFromDisplayName()
        {
            if (string.IsNullOrWhiteSpace(From))
                return "Unknown";

            // 如果包含@，提取用户名部分
            int atIndex = From.IndexOf('@');
            if (atIndex > 0)
                return From.Substring(0, atIndex);

            return From;
        }
    }
}
