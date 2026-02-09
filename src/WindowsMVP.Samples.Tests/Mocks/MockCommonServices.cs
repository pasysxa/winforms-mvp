using WinformsMVP.Services;

namespace WindowsMVP.Samples.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of ICommonServices for testing.
    /// Provides access to all mock services.
    /// </summary>
    public class MockCommonServices : ICommonServices
    {
        public MockCommonServices()
        {
            MessageService = new MockMessageService();
            DialogProvider = new MockDialogProvider();
            FileService = new MockFileService();
        }

        // 暴露为具体类型，方便测试验证
        public MockMessageService MessageService { get; }
        public MockDialogProvider DialogProvider { get; }
        public MockFileService FileService { get; }

        // 实现接口（返回相同实例）
        IMessageService ICommonServices.MessageService => MessageService;
        IDialogProvider ICommonServices.DialogProvider => DialogProvider;
        IFileService ICommonServices.FileService => FileService;

        /// <summary>
        /// 重置所有mock状态
        /// </summary>
        public void Reset()
        {
            MessageService.Clear();
            FileService.Clear();
        }
    }
}
