using System;
using System.ComponentModel;
using System.Text;
using WinformsMVP.Core.Models;

namespace WinformsMVP.Samples.MVPComparisonDemo.SupervisingController
{
    /// <summary>
    /// User model for Supervising Controller pattern.
    ///
    /// In Supervising Controller:
    /// - Model inherits from BindableBase (implements INotifyPropertyChanged)
    /// - Model contains business data and simple validation
    /// - View can bind directly to the model
    /// - Presenter focuses on complex logic and coordination
    ///
    /// This reduces boilerplate code for simple property synchronization.
    /// </summary>
    public class UserModel : BindableBase, ICloneable
    {
        private string _name;
        private string _email;
        private int _age;
        private bool _isActive;
        private string _validationErrors;

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    // Automatically validate when property changes
                    ValidateAndNotify();
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ValidateAndNotify();
                }
            }
        }

        public int Age
        {
            get => _age;
            set
            {
                if (SetProperty(ref _age, value))
                {
                    ValidateAndNotify();
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public string ValidationErrors
        {
            get => _validationErrors;
            private set => SetProperty(ref _validationErrors, value);
        }

        /// <summary>
        /// In Supervising Controller, Model can contain simple validation logic
        /// </summary>
        public bool IsValid => string.IsNullOrEmpty(ValidationErrors);

        private void ValidateAndNotify()
        {
            var errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.AppendLine("• Name is required");
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                errors.AppendLine("• Email is required");
            }
            else if (!Email.Contains("@"))
            {
                errors.AppendLine("• Email must contain @");
            }

            if (Age < 18 || Age > 120)
            {
                errors.AppendLine("• Age must be between 18 and 120");
            }

            ValidationErrors = errors.ToString().Trim();
        }

        /// <summary>
        /// このUserModelインスタンスの深いコピー（ディープコピー）を作成します。
        /// </summary>
        /// <returns>すべてのプロパティが独立してコピーされた新しいUserModelインスタンス。</returns>
        /// <remarks>
        /// <para>
        /// このメソッドは<strong>深いコピー</strong>を実装しています。
        /// すべてのプロパティ値が新しいインスタンスにコピーされ、元のオブジェクトと参照を共有しません。
        /// </para>
        /// <para>
        /// <strong>注意:</strong> このモデルにネストされたオブジェクト（例：Address、List等）がある場合、
        /// それらも深くコピーする必要があります：
        /// <code>
        /// Address = this.Address?.Clone() as Address,
        /// Tags = this.Tags != null ? new List&lt;string&gt;(this.Tags) : null
        /// </code>
        /// </para>
        /// <para>
        /// MemberwiseClone() は使用しないでください。浅いコピーとなり、
        /// ChangeTracker&lt;T&gt; での使用時に問題が発生します。
        /// </para>
        /// </remarks>
        public object Clone()
        {
            return new UserModel
            {
                Name = this.Name,
                Email = this.Email,
                Age = this.Age,
                IsActive = this.IsActive
                // ValidationErrorsは自動的に再計算されるため、コピー不要
            };
        }

        /// <summary>
        /// Restore values from another model
        /// </summary>
        public void CopyFrom(UserModel other)
        {
            Name = other.Name;
            Email = other.Email;
            Age = other.Age;
            IsActive = other.IsActive;
        }
    }
}
