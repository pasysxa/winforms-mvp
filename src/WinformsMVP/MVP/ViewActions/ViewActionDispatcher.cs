using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformsMVP.MVP.ViewActions
{
    public class ViewActionDispatcher
    {
        // 字典的键是 ActionKey
        private readonly Dictionary<ViewAction, IViewActionHandler> _handlers = new Dictionary<ViewAction, IViewActionHandler>();

        public void Register(ViewAction actionKey, Action handler)
        {
            if (actionKey != null)
            {
                _handlers[actionKey] = new ViewActionHandler(handler);
            }
        }

        public void Register<T>(ViewAction actionKey, Action<T> handler)
        {
            if (actionKey != null)
            {
                _handlers[actionKey] = new ViewActionHandler<T>(handler);
            }
        }

        public void Dispatch(ViewAction actionKey, object payload = null)
        {
            if (actionKey != null && _handlers.TryGetValue(actionKey, out var handlerWrapper))
            {
                handlerWrapper.Execute(payload);
            }
        }
    }
}
