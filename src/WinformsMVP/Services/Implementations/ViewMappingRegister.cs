using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WinformsMVP.Services.Implementations
{
    /// <summary>
    /// View インターフェースと実装 Form のマッピングを管理する標準実装。
    /// 型ベースのマッピングとファクトリメソッドの両方をサポートします。
    /// </summary>
    public class ViewMappingRegister : IViewMappingRegister
    {
        private readonly Dictionary<Type, Type> _typeMappings = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, Func<object>> _factoryMappings = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// View インターフェースと実装 Form のマッピングを登録します（型ベース）。
        /// </summary>
        public void Register<TViewInterface, TViewImplementation>(bool allowOverride = false)
            where TViewImplementation : Form, TViewInterface
        {
            var interfaceType = typeof(TViewInterface);
            var implementationType = typeof(TViewImplementation);

            // 既存チェック
            if (!allowOverride && (_typeMappings.ContainsKey(interfaceType) || _factoryMappings.ContainsKey(interfaceType)))
            {
                throw new InvalidOperationException(
                    $"View インターフェース {interfaceType.Name} は既に登録されています。" +
                    $"上書きする場合は allowOverride = true を指定してください。");
            }

            // ファクトリマッピングを削除（型ベース優先）
            _factoryMappings.Remove(interfaceType);

            // 型ベースマッピングを登録
            _typeMappings[interfaceType] = implementationType;
        }

        /// <summary>
        /// View インターフェースと実装 Form のマッピングを登録します（ファクトリメソッド）。
        /// </summary>
        public void Register<TViewInterface>(Func<TViewInterface> factory, bool allowOverride = false)
            where TViewInterface : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            var interfaceType = typeof(TViewInterface);

            // 既存チェック
            if (!allowOverride && (_typeMappings.ContainsKey(interfaceType) || _factoryMappings.ContainsKey(interfaceType)))
            {
                throw new InvalidOperationException(
                    $"View インターフェース {interfaceType.Name} は既に登録されています。" +
                    $"上書きする場合は allowOverride = true を指定してください。");
            }

            // 型ベースマッピングを削除（ファクトリ優先）
            _typeMappings.Remove(interfaceType);

            // ファクトリマッピングを登録（型消去してobjectで保存）
            _factoryMappings[interfaceType] = () => factory();
        }

        /// <summary>
        /// View インターフェースに対応する実装型を取得します。
        /// ファクトリメソッドで登録されている場合は null を返します。
        /// </summary>
        public Type GetViewImplementationType(Type viewInterfaceType)
        {
            // 型ベースマッピングをチェック
            if (_typeMappings.TryGetValue(viewInterfaceType, out Type implementationType))
            {
                return implementationType;
            }

            // ファクトリマッピングの場合は null（型情報なし）
            if (_factoryMappings.ContainsKey(viewInterfaceType))
            {
                return null;  // ファクトリは型を返さない
            }

            throw new KeyNotFoundException(
                $"View インターフェース {viewInterfaceType.Name} に対応する実装型が見つかりません。" +
                $"アプリケーションの起動時に Register<{viewInterfaceType.Name}, TImplementation>() で登録されているか確認してください。");
        }

        /// <summary>
        /// View インターフェースに対応する実装インスタンスを作成します。
        /// </summary>
        public object CreateInstance(Type viewInterfaceType)
        {
            // ファクトリマッピングを優先
            if (_factoryMappings.TryGetValue(viewInterfaceType, out Func<object> factory))
            {
                return factory();
            }

            // 型ベースマッピング
            if (_typeMappings.TryGetValue(viewInterfaceType, out Type implementationType))
            {
                try
                {
                    return Activator.CreateInstance(implementationType);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"View 実装型 {implementationType.Name} のインスタンス作成に失敗しました。" +
                        $"パラメータなしのコンストラクタが存在するか確認してください。" +
                        $"パラメータが必要な場合は Register<{viewInterfaceType.Name}>(factory) を使用してください。",
                        ex);
                }
            }

            throw new KeyNotFoundException(
                $"View インターフェース {viewInterfaceType.Name} に対応する実装が見つかりません。" +
                $"アプリケーションの起動時に Register() で登録されているか確認してください。");
        }

        /// <summary>
        /// 指定された View インターフェースが登録されているかを確認します。
        /// </summary>
        public bool IsRegistered(Type viewInterfaceType)
        {
            return _typeMappings.ContainsKey(viewInterfaceType) ||
                   _factoryMappings.ContainsKey(viewInterfaceType);
        }
    }
}
