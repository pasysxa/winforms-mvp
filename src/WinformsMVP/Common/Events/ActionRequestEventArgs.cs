using WinformsMVP.MVP.ViewActions;

namespace WinformsMVP.Common.Events
{
    public class ActionRequestEventArgs
    {
        public ActionRequestEventArgs(ViewAction actionKey)
        {
            ActionKey = actionKey;
        }
        public ViewAction ActionKey { get; }
    }

    public class ActionRequestEventArgs<T> : ActionRequestEventArgs, IActionRequestEventArgsWithValue
    {
        public ActionRequestEventArgs(ViewAction actionKey) : base(actionKey)
        {
        }

        public T Value { get; }
        public object GetValue() => Value;
    }
}
