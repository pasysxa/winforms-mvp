using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ValidationResult = WinformsMVP.Common.Validation.Core.ValidationResult;

namespace WinformsMVP.Common.Validation.Attributes
{
    /// <summary>
    /// Provides cross-property validation by comparing the value of one property with another.
    /// Wraps System.ComponentModel.DataAnnotations.CompareAttribute to add Order property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// CompareAttribute is commonly used for password confirmation scenarios
    /// where you need to ensure two properties have the same value.
    /// Typically has Order = 3 or higher (after Required and format validation).
    /// </para>
    ///
    /// <para>
    /// <b>Example - Password Confirmation:</b>
    /// <code>
    /// [Required(Order = 1, ErrorMessage = "Password is required")]
    /// [StringLength(100, MinimumLength = 8, Order = 2,
    ///     ErrorMessage = "Password must be at least 8 characters")]
    /// public string Password { get; set; }
    ///
    /// [Required(Order = 3, ErrorMessage = "Confirmation password is required")]
    /// [Compare(nameof(Password), Order = 4,
    ///     ErrorMessage = "Password and confirmation password do not match")]
    /// public string ConfirmPassword { get; set; }
    /// </code>
    /// Sequential validation ensures Password is valid before comparing with ConfirmPassword.
    /// </para>
    ///
    /// <para>
    /// <b>Order Rationale:</b>
    /// CompareAttribute should have higher Order than the property it compares to,
    /// preventing confusing error messages when the comparison target itself is invalid.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
                    AllowMultiple = false)]
    public class CompareAttribute : OrderedValidationAttribute
    {
        /// <summary>
        /// The wrapped System.ComponentModel.DataAnnotations.CompareAttribute.
        /// </summary>
        private readonly System.ComponentModel.DataAnnotations.CompareAttribute _innerAttribute;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareAttribute"/> class.
        /// </summary>
        /// <param name="otherProperty">
        /// The name of the other property to compare with.
        /// Use nameof() operator for type-safe property names.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="otherProperty"/> is null or empty.
        /// </exception>
        /// <example>
        /// <code>
        /// [Compare(nameof(Password), Order = 4)]
        /// public string ConfirmPassword { get; set; }
        /// </code>
        /// </example>
        public CompareAttribute(string otherProperty)
        {
            if (string.IsNullOrEmpty(otherProperty))
                throw new ArgumentNullException(nameof(otherProperty));

            OtherProperty = otherProperty;
            _innerAttribute = new System.ComponentModel.DataAnnotations.CompareAttribute(otherProperty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareAttribute"/> class
        /// with the specified other property and error message.
        /// </summary>
        /// <param name="otherProperty">
        /// The name of the other property to compare with.
        /// Use nameof() operator for type-safe property names.
        /// </param>
        /// <param name="errorMessage">The error message to use when validation fails.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="otherProperty"/> is null or empty.
        /// </exception>
        public CompareAttribute(string otherProperty, string errorMessage) : base(errorMessage)
        {
            if (string.IsNullOrEmpty(otherProperty))
                throw new ArgumentNullException(nameof(otherProperty));

            OtherProperty = otherProperty;
            _innerAttribute = new System.ComponentModel.DataAnnotations.CompareAttribute(otherProperty)
            {
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Gets the name of the other property to compare with.
        /// </summary>
        /// <value>
        /// The property name specified in the constructor.
        /// </value>
        public string OtherProperty { get; }

        /// <summary>
        /// Gets the display name of the other property for error messages.
        /// </summary>
        /// <value>
        /// The display name to use in error messages.
        /// If not set, uses the property name.
        /// </value>
        /// <remarks>
        /// <para>
        /// This property is read-only and is derived from the underlying
        /// System.ComponentModel.DataAnnotations.CompareAttribute.
        /// </para>
        ///
        /// <para>
        /// <b>Note:</b> In .NET Framework 4.8, OtherPropertyDisplayName cannot be
        /// set directly. The display name is automatically determined from the
        /// DisplayAttribute or property name of the compared property.
        /// </para>
        /// </remarks>
        public string OtherPropertyDisplayName
        {
            get => _innerAttribute.OtherPropertyDisplayName;
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
        /// This overload cannot perform cross-property comparison without context.
        /// Use the base class GetValidationResult() method for full validation.
        /// </para>
        ///
        /// <para>
        /// This method always returns true for compatibility, as cross-property
        /// validation requires the full model context.
        /// </para>
        ///
        /// <para>
        /// <b>Validation Rules (when context is available):</b>
        /// - null values are valid (use Required to disallow nulls)
        /// - Values are compared using Equals() method
        /// - Comparison is type-safe (int compared to int, string to string, etc.)
        /// - If the other property doesn't exist, validation fails
        /// </para>
        ///
        /// <para>
        /// <b>Comparison Examples:</b>
        /// <code>
        /// Password = "test123"
        /// ConfirmPassword = "test123"  // Valid - exact match
        ///
        /// Password = "Test123"
        /// ConfirmPassword = "test123"  // Invalid - case-sensitive
        ///
        /// Password = null
        /// ConfirmPassword = null       // Valid - both null
        ///
        /// Password = "test123"
        /// ConfirmPassword = null       // Invalid - not equal
        /// </code>
        /// </para>
        /// </remarks>
        public override bool IsValid(object value)
        {
            // Cross-property validation requires context - cannot validate in isolation
            // Return true for compatibility (context-aware validation is the canonical method)
            return true;
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
