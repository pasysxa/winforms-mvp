using WinformsMVP.Services;

namespace WinformsMVP.Samples.Tests.Mocks
{
    /// <summary>
    /// プラットフォームサービスのモック実装（テスト用）。
    /// すべてのモックサービスへのアクセスを提供します。
    /// </summary>
    public class MockPlatformServices : IPlatformServices
    {
        public MockPlatformServices()
        {
            MessageService = new MockMessageService();
            DialogProvider = new MockDialogProvider();
            FileService = new MockFileService();
            WindowNavigator = new MockWindowNavigator();
        }

        // 具体的な型として公開（テスト検証用）
        public MockMessageService MessageService { get; }
        public MockDialogProvider DialogProvider { get; }
        public MockFileService FileService { get; }
        public MockWindowNavigator WindowNavigator { get; }

        // インターフェース実装（同じインスタンスを返す）
        IMessageService IPlatformServices.MessageService => MessageService;
        IDialogProvider IPlatformServices.DialogProvider => DialogProvider;
        IFileService IPlatformServices.FileService => FileService;
        IWindowNavigator IPlatformServices.WindowNavigator => WindowNavigator;

        /// <summary>
        /// すべてのモック状態をリセットします
        /// </summary>
        public void Reset()
        {
            MessageService.Clear();
            FileService.Clear();
        }
    }
}
