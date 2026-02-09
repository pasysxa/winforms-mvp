using System;
using WinformsMVP.Services.Implementations;

namespace WinformsMVP.Services
{
    /// <summary>
    /// プラットフォームサービスへの静的アクセスを提供します。
    ///
    /// 使用方法：
    ///
    /// 1. 本番環境（デフォルトサービスを使用）：
    ///    var presenter = new MyPresenter();  // 自動的に PlatformServices.Default を使用
    ///
    /// 2. テスト環境（mockを注入）：
    ///    var mockServices = new MockPlatformServices();
    ///    var presenter = new MyPresenter().WithPlatformServices(mockServices);
    ///
    /// 3. カスタムグローバルサービス：
    ///    // アプリケーション起動時に設定
    ///    PlatformServices.Default = new CustomPlatformServices();
    ///
    /// </summary>
    public static class PlatformServices
    {
        private static IPlatformServices _default;
        private static readonly object _lock = new object();

        /// <summary>
        /// デフォルトのサービスインスタンスを取得または設定します。
        /// スレッドセーフな遅延初期化。
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
        /// デフォルト実装にリセットします（主にテストシナリオ用）。
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
