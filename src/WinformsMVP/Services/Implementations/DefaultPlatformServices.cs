using System;

namespace WinformsMVP.Services.Implementations
{
    /// <summary>
    /// プラットフォームサービスのデフォルト実装。
    /// 各サービスは遅延初期化されます。
    /// </summary>
    public class DefaultPlatformServices : IPlatformServices
    {
        private readonly Lazy<IDialogProvider> _dialogProvider = new Lazy<IDialogProvider>(() => new DialogProvider());
        private readonly Lazy<IMessageService> _messageService = new Lazy<IMessageService>(() => new MessageService());
        private readonly Lazy<IFileService> _fileService = new Lazy<IFileService>(() => new FileService());
        private readonly Lazy<IWindowNavigator> _windowNavigator;

        /// <summary>
        /// デフォルトコンストラクタ - ViewMappingRegisterなしで初期化
        /// </summary>
        public DefaultPlatformServices() : this(null)
        {
        }

        /// <summary>
        /// ViewMappingRegisterを指定して初期化
        /// </summary>
        /// <param name="viewMappingRegister">ビューマッピングレジスタ（nullの場合は新規作成）</param>
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
