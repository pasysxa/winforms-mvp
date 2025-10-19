using System;

namespace WinformsMVP.Services.Implementations
{
    public class DefaultCommonServices : ICommonServices
    {
        private readonly Lazy<IDialogProvider> _dialogProvider = new Lazy<IDialogProvider>(() => new DialogProvider());
        private readonly Lazy<IMessageService> _messageService = new Lazy<IMessageService>(() => new MessageService());
        private readonly Lazy<IFileService> _fileService = new Lazy<IFileService>(() => new FileService());

        public IDialogProvider DialogProvider => _dialogProvider.Value;
        public IMessageService MessageService => _messageService.Value;
        public IFileService FileService => _fileService.Value;
    }
}
