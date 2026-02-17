using WinformsMVP.Services;

namespace WinformsMVP.Samples.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of platform services for testing.
    /// Provides access to all mock services.
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

        // Expose as concrete types (for test verification)
        public MockMessageService MessageService { get; }
        public MockDialogProvider DialogProvider { get; }
        public MockFileService FileService { get; }
        public MockWindowNavigator WindowNavigator { get; }

        // Interface implementation (returns the same instances)
        IMessageService IPlatformServices.MessageService => MessageService;
        IDialogProvider IPlatformServices.DialogProvider => DialogProvider;
        IFileService IPlatformServices.FileService => FileService;
        IWindowNavigator IPlatformServices.WindowNavigator => WindowNavigator;

        /// <summary>
        /// Resets all mock states
        /// </summary>
        public void Reset()
        {
            MessageService.Clear();
            FileService.Clear();
        }
    }
}
