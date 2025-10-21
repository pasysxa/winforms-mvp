using System;
using System.Collections.Generic;
using System.ComponentModel;
using WinformsMVP.Common.Helpers;

namespace WinformsMVP.Common
{
    /// <summary>
    /// オブジェクト T の変更を追跡し、変更の受け入れまたは破棄を可能にするクラスです。
    /// T は参照型 (class) であり、クローン可能 (ICloneable) である必要があります。
    /// </summary>
    public class ChangeTracker<T> : IChangeTracking, IRevertibleChangeTracking where T : class, ICloneable
    {
        private T _originalValue;
        private readonly Func<T, T, bool> _comparer;

        public T CurrentValue { get; set; }

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
            else
            {
                _comparer = EqualityHelper.Equals;
            }
        }

        public ChangeTracker(T initialValue, IEqualityComparer<T> comparer)
            : this(initialValue, comparer.Equals)
        {
        }

        // ----------------------------------------------------------------------------------
        // IChangeTracking / IRevertibleChangeTracking インターフェース実装
        // ----------------------------------------------------------------------------------

        public bool IsChanged => !_comparer(_originalValue, CurrentValue);

        public void AcceptChanges()
        {
            _originalValue = (T)CurrentValue.Clone();
        }

        public void RejectChanges()
        {
            // クローンを使用して、参照が同一にならないようにします
            CurrentValue = (T)_originalValue.Clone();
        }

        // ----------------------------------------------------------------------------------
        // 補助メソッド (Passive View / Supervising Controller パターン用)
        // ----------------------------------------------------------------------------------
        public bool IsChangedWith(T currentValueFromView)
        {
            return !_comparer(_originalValue, currentValueFromView);
        }

        public void AcceptChanges(T newValue)
        {
            _originalValue = (T)newValue.Clone();
            CurrentValue = (T)newValue.Clone();
        }

        public T GetOriginalValue()
        {
            return (T)_originalValue.Clone();
        }
    }
}
