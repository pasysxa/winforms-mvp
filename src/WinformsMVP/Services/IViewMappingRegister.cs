using System;
using System.Windows.Forms;

namespace WinformsMVP.Services
{
    /// <summary>
    /// View インターフェースと実装 Form のマッピングを管理します。
    /// WindowNavigator が View インスタンスを自動的に作成するために使用されます。
    /// </summary>
    public interface IViewMappingRegister
    {
        /// <summary>
        /// View インターフェースに対応する実装型を取得します。
        /// </summary>
        /// <param name="viewInterfaceType">View インターフェース型</param>
        /// <returns>実装 Form 型</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">対応する実装型が見つからない場合</exception>
        Type GetViewImplementationType(Type viewInterfaceType);

        /// <summary>
        /// View インターフェースと実装 Form のマッピングを登録します（型ベース）。
        /// </summary>
        /// <typeparam name="TViewInterface">View インターフェース型</typeparam>
        /// <typeparam name="TViewImplementation">実装 Form 型</typeparam>
        /// <param name="allowOverride">既存のマッピングを上書きするかどうか（デフォルト: false）</param>
        /// <exception cref="InvalidOperationException">既に登録されている場合（allowOverride = false のとき）</exception>
        void Register<TViewInterface, TViewImplementation>(bool allowOverride = false)
            where TViewImplementation : Form, TViewInterface;

        /// <summary>
        /// View インターフェースと実装 Form のマッピングを登録します（ファクトリメソッド）。
        /// コンストラクタにパラメータが必要な View や、特殊な初期化が必要な場合に使用します。
        /// </summary>
        /// <typeparam name="TViewInterface">View インターフェース型</typeparam>
        /// <param name="factory">View インスタンスを作成するファクトリメソッド</param>
        /// <param name="allowOverride">既存のマッピングを上書きするかどうか（デフォルト: false）</param>
        /// <exception cref="InvalidOperationException">既に登録されている場合（allowOverride = false のとき）</exception>
        void Register<TViewInterface>(Func<TViewInterface> factory, bool allowOverride = false)
            where TViewInterface : class;

        /// <summary>
        /// View インターフェースに対応する実装インスタンスを作成します。
        /// </summary>
        /// <param name="viewInterfaceType">View インターフェース型</param>
        /// <returns>作成された View インスタンス</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">対応する実装型が見つからない場合</exception>
        object CreateInstance(Type viewInterfaceType);

        /// <summary>
        /// 指定された View インターフェースが登録されているかを確認します。
        /// </summary>
        /// <param name="viewInterfaceType">View インターフェース型</param>
        /// <returns>登録されている場合 true</returns>
        bool IsRegistered(Type viewInterfaceType);
    }
}
