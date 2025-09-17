using System;

namespace WinformsMVP.Services.Implementations
{
    public class DefaultCommonServices : ICommonServices
    {
        private readonly Lazy<IDialogService> _dialogService = new Lazy<IDialogService>(() => new DialogService());
        private readonly Lazy<IMessageService> _messageService = new Lazy<IMessageService>(() => new MessageService());
        private readonly Lazy<IFileService> _fileService = new Lazy<IFileService>(() => new FileService());

        public IDialogService DialogService => _dialogService.Value;
        public IMessageService MessageService => _messageService.Value;
        public IFileService FileService => _fileService.Value;
    }
}
