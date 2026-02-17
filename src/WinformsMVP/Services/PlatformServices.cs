using System;
using WinformsMVP.Services.Implementations;

namespace WinformsMVP.Services
{
    /// <summary>
    /// Provides static access to platform services.
    ///
    /// Usage:
    ///
    /// 1. Production environment (using default services):
    ///    var presenter = new MyPresenter();  // Automatically uses PlatformServices.Default
    ///
    /// 2. Test environment (inject mocks):
    ///    var mockServices = new MockPlatformServices();
    ///    var presenter = new MyPresenter().WithPlatformServices(mockServices);
    ///
    /// 3. Custom global services:
    ///    // Set at application startup
    ///    PlatformServices.Default = new CustomPlatformServices();
    ///
    /// </summary>
    public static class PlatformServices
    {
        private static IPlatformServices _default;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets or sets the default service instance.
        /// Thread-safe lazy initialization.
        /// </summary>
        public static IPlatformServices Default
        {
            get
            {
                if (_default == null)
                {
                    lock (_lock)
                    {
                        if (_default == null)
                        {
                            _default = new DefaultPlatformServices();
                        }
                    }
                }
                return _default;
            }
            set
            {
                lock (_lock)
                {
                    _default = value;
                }
            }
        }

        /// <summary>
        /// Resets to default implementation (primarily for test scenarios).
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _default = null;
            }
        }
    }
}
