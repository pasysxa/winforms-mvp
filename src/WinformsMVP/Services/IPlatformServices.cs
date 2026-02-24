using Microsoft.Extensions.Logging;

namespace WinformsMVP.Services
{
    /// <summary>
    /// Interface for platform services.
    /// Provides infrastructure services such as messages, dialogs, logging, and file operations.
    /// </summary>
    public interface IPlatformServices
    {
        /// <summary>
        /// Dialog provider (file open, save, folder selection, etc.)
        /// </summary>
        IDialogProvider DialogProvider { get; }

        /// <summary>
        /// Message service (message boxes, toast notifications, etc.)
        /// </summary>
        IMessageService MessageService { get; }

        /// <summary>
        /// File service (file read/write, directory operations, etc.)
        /// </summary>
        IFileService FileService { get; }

        /// <summary>
        /// Window navigator (modal/non-modal window display)
        /// </summary>
        IWindowNavigator WindowNavigator { get; }

        /// <summary>
        /// Logger factory for creating loggers.
        /// Default implementation uses Debug provider for development.
        /// Can be replaced with custom providers (Console, File, Application Insights, Seq, etc.)
        /// </summary>
        ILoggerFactory LoggerFactory { get; }
    }
}
