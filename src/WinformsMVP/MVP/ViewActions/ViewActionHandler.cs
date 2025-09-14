using System;

namespace WinformsMVP.MVP.ViewActions
{
    internal class ViewActionHandler : IViewActionHandler
    {
        private readonly Action _action;
        public ViewActionHandler(Action action) => _action = action;
        public void Execute(object payload) => _action?.Invoke();
    }

    public class ViewActionHandler<T> : IViewActionHandler
    {
        private readonly Action<T> _action;
        public ViewActionHandler(Action<T> action) => _action = action;

        public void Execute(object payload)
        {
            if (payload is T typedPayload)
            {
                _action?.Invoke(typedPayload);
            }
            else
            {
            }
        }
    }

}
