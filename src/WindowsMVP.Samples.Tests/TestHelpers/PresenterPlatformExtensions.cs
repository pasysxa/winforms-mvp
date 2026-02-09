using WinformsMVP.Core.Presenters;
using WinformsMVP.Core.Views;
using WinformsMVP.Services;

namespace WindowsMVP.Samples.Tests.TestHelpers
{
    /// <summary>
    /// プレゼンターをテスト用のモックプラットフォームサービスで拡張するためのメソッド。
    /// プレゼンター初期化前にモックサービスを注入するためのFluentAPIを提供します。
    /// </summary>
    public static class PresenterPlatformExtensions
    {
        /// <summary>
        /// テスト用にモックプラットフォームサービスを注入します。
        /// AttachView() または Initialize() より前に呼び出す必要があります。
        /// </summary>
        /// <typeparam name="T">プレゼンターの型</typeparam>
        /// <param name="presenter">プレゼンターインスタンス</param>
        /// <param name="platform">モックプラットフォームサービス</param>
        /// <returns>メソッドチェーン用のプレゼンター</returns>
        /// <example>
        /// <code>
        /// var mockServices = new MockPlatformServices();
        /// var presenter = new MyPresenter()
        ///     .WithPlatformServices(mockServices);  // 初期化前に！
        ///
        /// presenter.AttachView(mockView);
        /// presenter.Initialize();
        /// </code>
        /// </example>
        public static T WithPlatformServices<T>(this T presenter, IPlatformServices platform)
            where T : class
        {
            // InternalsVisibleTo属性により内部メソッドにアクセス可能
            // dynamicを使用してSetPlatformServicesを呼び出し
            dynamic dynamicPresenter = presenter;
            dynamicPresenter.SetPlatformServices(platform);
            return presenter;
        }
    }
}
