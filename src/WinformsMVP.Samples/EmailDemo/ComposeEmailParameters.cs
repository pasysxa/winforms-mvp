using WinformsMVP.Samples.EmailDemo.Models;

namespace WinformsMVP.Samples.EmailDemo
{
    /// <summary>
    /// 撰写邮件模式
    /// </summary>
    public enum ComposeMode
    {
        /// <summary>新建邮件</summary>
        New,
        /// <summary>回复邮件</summary>
        Reply,
        /// <summary>转发邮件</summary>
        Forward
    }

    /// <summary>
    /// 撰写邮件窗口的初始化参数
    /// </summary>
    public class ComposeEmailParameters
    {
        /// <summary>
        /// 撰写模式
        /// </summary>
        public ComposeMode Mode { get; set; }

        /// <summary>
        /// 原始邮件（用于回复或转发）
        /// </summary>
        public EmailMessage OriginalEmail { get; set; }
    }
}
