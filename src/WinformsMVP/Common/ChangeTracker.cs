using System;
using System.Collections.Generic;
using System.ComponentModel;
using WinformsMVP.Common.Helpers;

namespace WinformsMVP.Common
{
    /// <summary>
    /// Tracks changes to an object of type T and allows accepting or rejecting changes.
    /// T must be a reference type (class) and cloneable (ICloneable).
    /// </summary>
    /// <typeparam name="T">The type to track. Must be a reference type and implement ICloneable.</typeparam>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> The ICloneable.Clone() implementation of T <strong>must return a deep copy</strong>.
    /// </para>
    /// <para>
    /// <strong>Deep copy means:</strong> All properties and nested objects are copied as new instances,
    /// with no shared references to the original object.
    /// </para>
    /// <para>
    /// <strong>Incorrect implementation (shallow copy - DO NOT USE):</strong>
    /// <code>
    /// public object Clone()
    /// {
    ///     return this.MemberwiseClone();  // ❌NG: Shallow copy (references are shared)
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Correct implementation (deep copy):</strong>
    /// <code>
    /// public object Clone()
    /// {
    ///     return new UserModel
    ///     {
    ///         Name = this.Name,
    ///         Email = this.Email,
    ///         Age = this.Age,
    ///         Address = this.Address?.Clone() as Address  // Deep copy nested objects too
    ///     };
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// Using shallow copy will cause shared references with the original object,
    /// and RejectChanges() will not work correctly (original values will also be modified).
    /// </para>
    /// </remarks>
    public class ChangeTracker<T> : IChangeTracking, IRevertibleChangeTracking where T : class, ICloneable
    {
        private T _originalValue;
        private T _currentValue;
        private readonly Func<T, T, bool> _comparer;
        private bool? _cachedIsChanged;
        private readonly object _lock = new object();

        /// <summary>
        /// Occurs when the IsChanged state changes.
        /// </summary>
        public event EventHandler IsChangedChanged;

        /// <summary>
        /// Gets the currently tracked value.
        /// To change the value, use the UpdateCurrentValue() method.
        /// </summary>
        public T CurrentValue
        {
            get
            {
                lock (_lock)
                {
                    return _currentValue;
                }
            }
            private set
            {
                lock (_lock)
                {
                    if (!ReferenceEquals(_currentValue, value))
                    {
                        var wasChanged = IsChanged;
                        _currentValue = value;
                        InvalidateIsChanged();

                        // Fire event only if IsChanged state changed
                        if (wasChanged != IsChanged)
                        {
                            OnIsChangedChanged();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a ChangeTracker with the specified initial value.
        /// </summary>
        /// <param name="initialValue">The initial value to start tracking.</param>
        /// <param name="comparer">Custom comparison function for values. If null, EqualityHelper is used.</param>
        /// <exception cref="ArgumentNullException">When initialValue is null.</exception>
        /// <remarks>
        /// <strong>Important:</strong> Type T of initialValue must return a <strong>deep copy</strong> from ICloneable.Clone().
        /// Returning a shallow copy (MemberwiseClone) will cause change tracking to not work correctly.
        /// </remarks>
        public ChangeTracker(T initialValue, Func<T, T, bool> comparer = null)
        {
            if (initialValue == null)
                throw new ArgumentNullException(nameof(initialValue));

            _currentValue = (T)initialValue.Clone();
            _originalValue = (T)initialValue.Clone();
            _comparer = comparer ?? EqualityHelper.Equals;
        }

        /// <summary>
        /// Initializes a ChangeTracker with the specified initial value and IEqualityComparer.
        /// </summary>
        /// <param name="initialValue">The initial value to start tracking.</param>
        /// <param name="comparer">The IEqualityComparer to use for value comparison.</param>
        public ChangeTracker(T initialValue, IEqualityComparer<T> comparer)
            : this(initialValue, comparer != null ? (Func<T, T, bool>)comparer.Equals : null)
        {
        }

        // ----------------------------------------------------------------------------------
        // IChangeTracking / IRevertibleChangeTracking interface implementation
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether the current value differs from the original value.
        /// This property uses caching for performance.
        /// </summary>
        public bool IsChanged
        {
            get
            {
                lock (_lock)
                {
                    if (!_cachedIsChanged.HasValue)
                    {
                        _cachedIsChanged = !_comparer(_originalValue, _currentValue);
                    }
                    return _cachedIsChanged.Value;
                }
            }
        }

        /// <summary>
        /// Commits the current value as the new baseline.
        /// </summary>
        /// <remarks>
        /// Internally calls Clone() to create a deep copy of the current value.
        /// </remarks>
        public void AcceptChanges()
        {
            lock (_lock)
            {
                var wasChanged = IsChanged;
                _originalValue = (T)_currentValue.Clone();
                InvalidateIsChanged();

                if (wasChanged)
                {
                    OnIsChangedChanged();
                }
            }
        }

        /// <summary>
        /// Reverts the current value to the last accepted baseline.
        /// </summary>
        /// <remarks>
        /// Internally calls Clone() to create a deep copy of the original value and sets it to CurrentValue.
        /// This protects the original value and prevents reference sharing with CurrentValue.
        /// </remarks>
        public void RejectChanges()
        {
            lock (_lock)
            {
                var wasChanged = IsChanged;
                // Use clone to prevent reference identity
                _currentValue = (T)_originalValue.Clone();
                InvalidateIsChanged();

                if (wasChanged)
                {
                    OnIsChangedChanged();
                }
            }
        }

        // ----------------------------------------------------------------------------------
        // Helper methods (for Passive View / Supervising Controller patterns)
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// Checks whether the value from the view differs from the original baseline.
        /// Useful for Passive View / Supervising Controller patterns.
        /// </summary>
        /// <param name="currentValueFromView">The current value retrieved from the view.</param>
        /// <returns>True if the view value differs from the original value, otherwise false.</returns>
        public bool IsChangedWith(T currentValueFromView)
        {
            lock (_lock)
            {
                return !_comparer(_originalValue, currentValueFromView);
            }
        }

        /// <summary>
        /// Commits a new value as the baseline and updates the current value.
        /// </summary>
        /// <param name="newValue">The value to set as the new baseline and current value.</param>
        /// <exception cref="ArgumentNullException">When newValue is null.</exception>
        public void AcceptChanges(T newValue)
        {
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            lock (_lock)
            {
                var wasChanged = IsChanged;
                _originalValue = (T)newValue.Clone();
                _currentValue = (T)newValue.Clone();
                InvalidateIsChanged();

                if (wasChanged)
                {
                    OnIsChangedChanged();
                }
            }
        }

        /// <summary>
        /// Gets a copy of the original baseline value.
        /// </summary>
        /// <returns>A deep copy of the original value.</returns>
        public T GetOriginalValue()
        {
            lock (_lock)
            {
                return (T)_originalValue.Clone();
            }
        }

        /// <summary>
        /// Updates the currently tracked value.
        /// </summary>
        /// <param name="newValue">The new current value.</param>
        /// <exception cref="ArgumentNullException">When newValue is null.</exception>
        /// <remarks>
        /// This method updates the CurrentValue property and
        /// fires the IsChangedChanged event if the IsChanged state changes.
        /// </remarks>
        public void UpdateCurrentValue(T newValue)
        {
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            CurrentValue = newValue;
        }

        // ----------------------------------------------------------------------------------
        // Validation support
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// Checks whether changes can be accepted.
        /// </summary>
        /// <param name="error">Error message if changes cannot be accepted. Null if acceptable.</param>
        /// <returns>True if changes can be accepted, otherwise false.</returns>
        /// <remarks>
        /// Default implementation always returns true.
        /// Override this method in derived classes if custom validation is needed.
        /// </remarks>
        public virtual bool CanAcceptChanges(out string error)
        {
            error = null;
            return true;
        }

        /// <summary>
        /// Checks whether changes can be rejected.
        /// </summary>
        /// <param name="error">Error message if changes cannot be rejected. Null if rejectable.</param>
        /// <returns>True if changes can be rejected, otherwise false.</returns>
        /// <remarks>
        /// Default implementation always returns true.
        /// Override this method in derived classes if custom validation is needed.
        /// </remarks>
        public virtual bool CanRejectChanges(out string error)
        {
            error = null;
            return true;
        }

        // ----------------------------------------------------------------------------------
        // Private helper methods
        // ----------------------------------------------------------------------------------

        private void InvalidateIsChanged()
        {
            _cachedIsChanged = null;
        }

        private void OnIsChangedChanged()
        {
            IsChangedChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
