using System;
using System.ComponentModel.DataAnnotations;
using ValidationResult = WinformsMVP.Common.Validation.Core.ValidationResult;

namespace WinformsMVP.Common.Validation.Attributes
{
    /// <summary>
    /// Specifies the minimum and maximum length of string data, with optional ordering support.
    /// Wraps System.ComponentModel.DataAnnotations.StringLengthAttribute to add Order property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// StringLengthAttribute typically has Order = 2 (after Required validation),
    /// as checking length on a null/empty string would cause confusing error messages.
    /// </para>
    ///
    /// <para>
    /// <b>Example:</b>
    /// <code>
    /// [Required(Order = 1, ErrorMessage = "Name is required")]
    /// [StringLength(50, MinimumLength = 3, Order = 2,
    ///     ErrorMessage = "Name must be between 3 and 50 characters")]
    /// public string Name { get; set; }
    /// </code>
    /// Sequential validation checks Required first, then StringLength only if the field is not empty.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
                    AllowMultiple = false)]
    public class StringLengthAttribute : OrderedValidationAttribute
    {
        /// <summary>
        /// The wrapped System.ComponentModel.DataAnnotations.StringLengthAttribute.
        /// </summary>
        private readonly System.ComponentModel.DataAnnotations.StringLengthAttribute _innerAttribute;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringLengthAttribute"/> class
        /// with the specified maximum length.
        /// </summary>
        /// <param name="maximumLength">The maximum allowable length of string data.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="maximumLength"/> is less than zero.
        /// </exception>
        public StringLengthAttribute(int maximumLength)
        {
            _innerAttribute = new System.ComponentModel.DataAnnotations.StringLengthAttribute(maximumLength);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringLengthAttribute"/> class
        /// with the specified maximum length and error message.
        /// </summary>
        /// <param name="maximumLength">The maximum allowable length of string data.</param>
        /// <param name="errorMessage">The error message to use when validation fails.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="maximumLength"/> is less than zero.
        /// </exception>
        public StringLengthAttribute(int maximumLength, string errorMessage) : base(errorMessage)
        {
            _innerAttribute = new System.ComponentModel.DataAnnotations.StringLengthAttribute(maximumLength)
            {
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Gets the maximum acceptable length of the string.
        /// </summary>
        /// <value>
        /// The maximum length of the string.
        /// </value>
        public int MaximumLength => _innerAttribute.MaximumLength;

        /// <summary>
        /// Gets or sets the minimum acceptable length of the string.
        /// </summary>
        /// <value>
        /// The minimum length of the string. Default is 0.
        /// </value>
        /// <remarks>
        /// Set MinimumLength to enforce a minimum length requirement.
        /// For example, passwords often have a minimum length of 8 characters.
        /// </remarks>
        public int MinimumLength
        {
            get => _innerAttribute.MinimumLength;
            set => _innerAttribute.MinimumLength = value;
        }

        /// <summary>
        /// Determines whether the specified value is valid.
        /// </summary>
        /// <param name="value">The value of the object to validate.</param>
        /// <returns>
        /// true if the specified value is valid; otherwise, false.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>Validation Rules:</b>
        /// - null values are valid (use Required to disallow nulls)
        /// - String length must be between MinimumLength and MaximumLength (inclusive)
        /// - Empty strings are valid unless MinimumLength > 0
        /// </para>
        /// </remarks>
        public override bool IsValid(object value)
        {
            return _innerAttribute.IsValid(value);
        }

        /// <summary>
        /// Applies formatting to an error message, based on the data field where the error occurred.
        /// </summary>
        /// <param name="name">The name to include in the formatted message.</param>
        /// <returns>
        /// A formatted error message string.
        /// </returns>
        public override string FormatErrorMessage(string name)
        {
            return _innerAttribute.FormatErrorMessage(name);
        }
    }
}
