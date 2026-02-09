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
    /// <typeparam name="T">追跡する型。参照型であり、ICloneableを実装する必要があります。</typeparam>
    /// <remarks>
    /// <para>
    /// <strong>重要:</strong> T の ICloneable.Clone() 実装は<strong>必ず深いコピー（ディープコピー）</strong>を返す必要があります。
    /// </para>
    /// <para>
    /// <strong>深いコピーとは:</strong> すべてのプロパティとネストされたオブジェクトが新しいインスタンスとしてコピーされ、
    /// 元のオブジェクトと共有参照を持たないことを意味します。
    /// </para>
    /// <para>
    /// <strong>誤った実装例（浅いコピー - 使用禁止）:</strong>
    /// <code>
    /// public object Clone()
    /// {
    ///     return this.MemberwiseClone();  // ❌NG: 浅いコピー（参照が共有される）
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// <strong>正しい実装例（深いコピー）:</strong>
    /// <code>
    /// public object Clone()
    /// {
    ///     return new UserModel
    ///     {
    ///         Name = this.Name,
    ///         Email = this.Email,
    ///         Age = this.Age,
    ///         Address = this.Address?.Clone() as Address  // ネストされたオブジェクトも深くコピー
    ///     };
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// 浅いコピーを使用すると、元のオブジェクトと共有参照が発生し、
    /// RejectChanges() が正しく動作しません（元の値も変更されてしまう）。
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
        /// IsChanged 状態が変更されたときに発生します。
        /// </summary>
        public event EventHandler IsChangedChanged;

        /// <summary>
        /// 現在追跡している値を取得します。
        /// 値を変更する場合は UpdateCurrentValue() メソッドを使用してください。
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

                        // IsChanged 状態が変わった場合のみイベント発火
                        if (wasChanged != IsChanged)
                        {
                            OnIsChangedChanged();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 初期値を指定して ChangeTracker を初期化します。
        /// </summary>
        /// <param name="initialValue">追跡を開始する初期値。</param>
        /// <param name="comparer">値の比較に使用するカスタム比較関数。nullの場合はEqualityHelperが使用されます。</param>
        /// <exception cref="ArgumentNullException">initialValueがnullの場合。</exception>
        /// <remarks>
        /// <strong>重要:</strong> initialValue の型 T は ICloneable.Clone() で<strong>深いコピー</strong>を返す必要があります。
        /// 浅いコピー（MemberwiseClone）を返すと、変更追跡が正しく動作しません。
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
        /// 初期値と IEqualityComparer を指定して ChangeTracker を初期化します。
        /// </summary>
        /// <param name="initialValue">追跡を開始する初期値。</param>
        /// <param name="comparer">値の比較に使用する IEqualityComparer。</param>
        public ChangeTracker(T initialValue, IEqualityComparer<T> comparer)
            : this(initialValue, comparer != null ? (Func<T, T, bool>)comparer.Equals : null)
        {
        }

        // ----------------------------------------------------------------------------------
        // IChangeTracking / IRevertibleChangeTracking インターフェース実装
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// 現在の値が元の値と異なるかどうかを示す値を取得します。
        /// このプロパティはパフォーマンスのためにキャッシュを使用します。
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
        /// 現在の値を新しいベースラインとしてコミットします。
        /// </summary>
        /// <remarks>
        /// 内部で Clone() を呼び出して現在の値の深いコピーを作成します。
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
        /// 現在の値を最後に受け入れたベースラインに戻します。
        /// </summary>
        /// <remarks>
        /// 内部で Clone() を呼び出して元の値の深いコピーを作成し、CurrentValueに設定します。
        /// これにより、元の値が保護され、CurrentValueとの参照共有を防ぎます。
        /// </remarks>
        public void RejectChanges()
        {
            lock (_lock)
            {
                var wasChanged = IsChanged;
                // クローンを使用して、参照が同一にならないようにします
                _currentValue = (T)_originalValue.Clone();
                InvalidateIsChanged();

                if (wasChanged)
                {
                    OnIsChangedChanged();
                }
            }
        }

        // ----------------------------------------------------------------------------------
        // 補助メソッド (Passive View / Supervising Controller パターン用)
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// ビューからの値が元のベースラインと異なるかどうかを確認します。
        /// Passive View / Supervising Controller パターンに便利です。
        /// </summary>
        /// <param name="currentValueFromView">ビューから取得した現在の値。</param>
        /// <returns>ビューの値が元の値と異なる場合は true、それ以外の場合は false。</returns>
        public bool IsChangedWith(T currentValueFromView)
        {
            lock (_lock)
            {
                return !_comparer(_originalValue, currentValueFromView);
            }
        }

        /// <summary>
        /// 新しい値をベースラインとしてコミットし、現在の値を更新します。
        /// </summary>
        /// <param name="newValue">新しいベースラインおよび現在値として設定する値。</param>
        /// <exception cref="ArgumentNullException">newValueがnullの場合。</exception>
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
        /// 元のベースライン値のコピーを取得します。
        /// </summary>
        /// <returns>元の値の深いコピー。</returns>
        public T GetOriginalValue()
        {
            lock (_lock)
            {
                return (T)_originalValue.Clone();
            }
        }

        /// <summary>
        /// 現在追跡している値を更新します。
        /// </summary>
        /// <param name="newValue">新しい現在値。</param>
        /// <exception cref="ArgumentNullException">newValueがnullの場合。</exception>
        /// <remarks>
        /// このメソッドは CurrentValue プロパティを更新し、
        /// IsChanged 状態が変化した場合は IsChangedChanged イベントを発火します。
        /// </remarks>
        public void UpdateCurrentValue(T newValue)
        {
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            CurrentValue = newValue;
        }

        // ----------------------------------------------------------------------------------
        // 検証サポート
        // ----------------------------------------------------------------------------------

        /// <summary>
        /// 変更を受け入れることができるかどうかを確認します。
        /// </summary>
        /// <param name="error">受け入れできない場合のエラーメッセージ。受け入れ可能な場合はnull。</param>
        /// <returns>変更を受け入れることができる場合は true、それ以外の場合は false。</returns>
        /// <remarks>
        /// デフォルト実装では常に true を返します。
        /// カスタム検証が必要な場合は、派生クラスでこのメソッドをオーバーライドしてください。
        /// </remarks>
        public virtual bool CanAcceptChanges(out string error)
        {
            error = null;
            return true;
        }

        /// <summary>
        /// 変更を破棄することができるかどうかを確認します。
        /// </summary>
        /// <param name="error">破棄できない場合のエラーメッセージ。破棄可能な場合はnull。</param>
        /// <returns>変更を破棄することができる場合は true、それ以外の場合は false。</returns>
        /// <remarks>
        /// デフォルト実装では常に true を返します。
        /// カスタム検証が必要な場合は、派生クラスでこのメソッドをオーバーライドしてください。
        /// </remarks>
        public virtual bool CanRejectChanges(out string error)
        {
            error = null;
            return true;
        }

        // ----------------------------------------------------------------------------------
        // プライベートヘルパーメソッド
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
