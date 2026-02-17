using System;

namespace WinformsMVP.Services.Implementations
{
    /// <summary>
    /// Default implementation of platform services.
    /// Each service is lazy-initialized.
    /// </summary>
    public class DefaultPlatformServices : IPlatformServices
    {
        private readonly Lazy<IDialogProvider> _dialogProvider = new Lazy<IDialogProvider>(() => new DialogProvider());
        private readonly Lazy<IMessageService> _messageService = new Lazy<IMessageService>(() => new MessageService());
        private readonly Lazy<IFileService> _fileService = new Lazy<IFileService>(() => new FileService());
        private readonly Lazy<IWindowNavigator> _windowNavigator;

        /// <summary>
        /// Default constructor - Initializes without ViewMappingRegister
        /// </summary>
        public DefaultPlatformServices() : this(null)
        {
        }

        /// <summary>
        /// Initializes with a specified ViewMappingRegister
        /// </summary>
        /// <param name="viewMappingRegister">View mapping register (creates new one if null)</param>
        public DefaultPlatformServices(IViewMappingRegister viewMappingRegister)
        {
            _windowNavigator = new Lazy<IWindowNavigator>(() =>
            {
                var register = viewMappingRegister ?? new ViewMappingRegister();
                return new WindowNavigator(register);
            });
        }

        public IDialogProvider DialogProvider => _dialogProvider.Value;
        public IMessageService MessageService => _messageService.Value;
        public IFileService FileService => _fileService.Value;
        public IWindowNavigator WindowNavigator => _windowNavigator.Value;
    }
}
