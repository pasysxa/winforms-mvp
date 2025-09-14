using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WinformsMVP.Common
{
    public class ChangeTracker<T> : IChangeTracking, IRevertibleChangeTracking where T : class, ICloneable
    {
        private T _originalValue;
        private readonly Func<T, T, bool> _comparer;

        public T CurrentValue { get; set; }
        public ChangeTracker() { }
        public ChangeTracker(T initialValue, Func<T, T, bool> comparer = null)
        {
            if (initialValue == null)
                throw new ArgumentNullException(nameof(initialValue));

            CurrentValue = initialValue;
            _originalValue = (T)initialValue.Clone();

            if (comparer != null)
            {
                _comparer = comparer;
            }
            else if (typeof(IEquatable<T>).IsAssignableFrom(typeof(T)))
            {
                _comparer = (x, y) => ((IEquatable<T>)x).Equals(y);
            }
            else
            {
                //_comparer = (x, y) => JsonSerializer.Serialize(x) == JsonSerializer.Serialize(y);
            }
        }

        public ChangeTracker(T initialValue, IEqualityComparer<T> comparer)
            : this(initialValue, comparer.Equals)
        {
        }

        // --- Supervising Controller ---
        public bool IsChanged => !_comparer(_originalValue, CurrentValue);

        public void AcceptChanges()
        {
            _originalValue = (T)CurrentValue.Clone();
        }

        public void RejectChanges()
        {
            CurrentValue = (T)_originalValue.Clone();
        }

        // --- Passive View ---
        public bool IsChangedWith(T currentValueFromView)
        {
            return !_comparer(_originalValue, currentValueFromView);
        }

        public void AcceptChanges(T newValue)
        {
            _originalValue = (T)newValue.Clone();
        }

        public T GetOriginalValue()
        {
            return (T)_originalValue.Clone();
        }
    }
}
