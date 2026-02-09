using WinformsMVP.Services;

namespace WindowsMVP.Samples.Tests.Mocks
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
        }

        // 具体的な型として公開（テスト検証用）
        public MockMessageService MessageService { get; }
        public MockDialogProvider DialogProvider { get; }
        public MockFileService FileService { get; }

        // インターフェース実装（同じインスタンスを返す）
        IMessageService IPlatformServices.MessageService => MessageService;
        IDialogProvider IPlatformServices.DialogProvider => DialogProvider;
        IFileService IPlatformServices.FileService => FileService;

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
