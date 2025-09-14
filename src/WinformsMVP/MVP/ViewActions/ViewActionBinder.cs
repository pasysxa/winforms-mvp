using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace WinformsMVP.MVP.ViewActions
{
    public class ViewActionBinder : IEnumerable<ActionBinding>
    {
        private readonly List<ActionBinding> _bindings = new List<ActionBinding>();
        private readonly Dictionary<Type, (Action<Component, Delegate> Attacher, Action<Component, Delegate> Detacher)> _bindingStrategies
            = new Dictionary<Type, (Action<Component, Delegate>, Action<Component, Delegate>)>();
        private readonly List<(Component Control, Delegate Handler, Action<Component, Delegate> Detacher)> _activeSubscriptions
            = new List<(Component, Delegate, Action<Component, Delegate>)>();

        public ViewActionBinder()
        {
            RegisterDefaultStrategies();
        }

        private void RegisterDefaultStrategies()
        {
            // Button, CheckBox, RadioButton 
            RegisterStrategy<ButtonBase>(
                (c, h) => c.Click += (EventHandler)h,
                (c, h) => c.Click -= (EventHandler)h);

            // ToolStripMenuItem, ToolStripButton, ToolStripDropDownButton, ToolStripStatusLabel 
            RegisterStrategy<ToolStripItem>(
                (c, h) => c.Click += (EventHandler)h,
                (c, h) => c.Click -= (EventHandler)h);

            // Default fallback for any Control with Click event
            RegisterStrategy<Control>(
                (c, h) => c.Click += (EventHandler)h,
                (c, h) => c.Click -= (EventHandler)h);
        }

        public void RegisterStrategy<T>(Action<T, Delegate> attacher, Action<T, Delegate> detacher) where T : Component
        {
            _bindingStrategies[typeof(T)] = ((comp, handler) => attacher((T)comp, handler), (comp, handler) => detacher((T)comp, handler));
        }

        public void Add(ViewAction actionKey, params Component[] controls)
        {
            _bindings.Add(new ActionBinding(actionKey, controls));
        }

        public void Bind(Action<ViewAction> onActionTriggered)
        {
            Unbind();

            var actionMap = new Dictionary<object, ViewAction>();
            foreach (var binding in _bindings)
            {
                foreach (var component in binding.Components)
                {
                    actionMap[component] = binding.ActionKey;
                }
            }

            Delegate handler = new EventHandler((sender, args) =>
            {
                if (actionMap.TryGetValue(sender, out var key))
                {
                    onActionTriggered?.Invoke(key);
                }
            });

            foreach (var control in actionMap.Keys)
            {
                ApplyBindingStrategy((Component)control, handler);
            }
        }

        public void Unbind()
        {
            foreach (var (control, handler, detacher) in _activeSubscriptions)
            {
                detacher(control, handler);
            }
            _activeSubscriptions.Clear();
        }

        private void ApplyBindingStrategy(Component control, Delegate handler)
        {
            Type currentType = control.GetType();
            while (currentType != null && currentType != typeof(object))
            {
                if (_bindingStrategies.TryGetValue(currentType, out var strategy))
                {
                    Delegate specificHandler = handler;
                    strategy.Attacher(control, specificHandler);
                    _activeSubscriptions.Add((control, specificHandler, strategy.Detacher));
                    return;
                }
                currentType = currentType.BaseType;
            }
        }

        IEnumerator<ActionBinding> IEnumerable<ActionBinding>.GetEnumerator() => _bindings.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ActionBinding>)this).GetEnumerator();
    }
}
