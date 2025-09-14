using System.ComponentModel;

namespace WinformsMVP.MVP.ViewActions
{
    internal class ActionBinding
    {
        public ViewAction ActionKey { get; }
        public Component[] Components { get; }

        public ActionBinding(ViewAction actionkey, Component[] components)
        {
            ActionKey = actionkey;
            Components = components;
        }
    }
}
