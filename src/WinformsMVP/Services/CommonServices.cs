using System;
using WinformsMVP.Services.Implementations;

namespace WinformsMVP.Services
{
    /// <summary>
    /// 提供对常用服务的静态访问。
    ///
    /// 使用方式：
    ///
    /// 1. 生产环境（使用默认服务）：
    ///    var presenter = new MyPresenter();  // 自动使用 CommonServices.Default
    ///
    /// 2. 测试环境（注入mock）：
    ///    var mockServices = new MockCommonServices();
    ///    var presenter = new MyPresenter(mockServices);
    ///
    /// 3. 自定义全局服务：
    ///    // 在应用启动时设置
    ///    CommonServices.Default = new CustomCommonServices();
    ///
    /// </summary>
    public static class CommonServices
    {
        private static ICommonServices _default;
        private static readonly object _lock = new object();

        /// <summary>
        /// 获取或设置默认的服务实例。
        /// 线程安全的延迟初始化。
        /// </summary>
        public static ICommonServices Default
        {
            get
            {
                if (_default == null)
                {
                    lock (_lock)
                    {
                        if (_default == null)
                        {
                            _default = new DefaultCommonServices();
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
        /// 重置为默认实现（主要用于测试场景）。
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
