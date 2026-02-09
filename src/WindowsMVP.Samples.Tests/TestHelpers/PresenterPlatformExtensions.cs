using WinformsMVP.Core.Presenters;
using WinformsMVP.Core.Views;
using WinformsMVP.Services;

namespace WindowsMVP.Samples.Tests.TestHelpers
{
    /// <summary>
    /// Extension methods for testing presenters with mock platform services.
    /// Provides a fluent API for injecting mock services before presenter initialization.
    /// </summary>
    public static class PresenterPlatformExtensions
    {
        /// <summary>
        /// Injects mock platform services for testing.
        /// MUST be called before AttachView() or Initialize().
        /// </summary>
        /// <typeparam name="T">The presenter type</typeparam>
        /// <param name="presenter">The presenter instance</param>
        /// <param name="platform">The mock platform services</param>
        /// <returns>The presenter for fluent chaining</returns>
        /// <example>
        /// <code>
        /// var mockServices = new MockCommonServices();
        /// var presenter = new MyPresenter()
        ///     .WithPlatformServices(mockServices);  // Before initialization!
        ///
        /// presenter.AttachView(mockView);
        /// presenter.Initialize();
        /// </code>
        /// </example>
        public static T WithPlatformServices<T>(this T presenter, ICommonServices platform)
            where T : class
        {
            // Use dynamic to call internal SetPlatformServices method
            // This works because of InternalsVisibleTo attribute
            dynamic dynamicPresenter = presenter;
            dynamicPresenter.SetPlatformServices(platform);
            return presenter;
        }
    }
}
