using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WinformsMVP.Services.Implementations
{
    public class ViewMappingRegister : IViewMappingRegister
    {
        private readonly Dictionary<Type, Type> _mappings = new Dictionary<Type, Type>();

        public void Register<TViewInterface, TViewImplementation>() where TViewImplementation : Form, TViewInterface
        {
            var interfaceType = typeof(TViewInterface);
            var implementationType = typeof(TViewImplementation);

            if (_mappings.ContainsKey(interfaceType))
            {
                throw new InvalidOperationException($"Viewインターフェース {interfaceType.Name} は既に登録されています。");
            }

            _mappings.Add(interfaceType, implementationType);
        }

        public Type GetViewImplementationType(Type viewInterfaceType)
        {
            if (_mappings.TryGetValue(viewInterfaceType, out Type implementationType))
            {
                return implementationType;
            }

            throw new KeyNotFoundException($"Viewインターフェース {viewInterfaceType.Name} に対応する実装型が見つかりません。アプリケーションの起動時に登録されているか確認してください。");
        }
    }
}
