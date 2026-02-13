using System;
using System.ComponentModel.DataAnnotations;
using ValidationResult = WinformsMVP.Common.Validation.Core.ValidationResult;

namespace WinformsMVP.Common.Validation.Attributes
{
    /// <summary>
    /// Validates that a property value is a valid email address, with optional ordering support.
    /// Wraps System.ComponentModel.DataAnnotations.EmailAddressAttribute to add Order property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// EmailAddressAttribute typically has Order = 2 (after Required validation),
    /// as checking email format on a null/empty string would cause confusing error messages.
    /// </para>
    ///
    /// <para>
    /// <b>Example:</b>
    /// <code>
    /// [Required(Order = 1, ErrorMessage = "Email is required")]
    /// [EmailAddress(Order = 2, ErrorMessage = "Invalid email address format")]
    /// public string Email { get; set; }
    /// </code>
    /// Sequential validation checks Required first, then EmailAddress only if the field is not empty.
    /// </para>
    ///
    /// <para>
    /// <b>Validation Pattern:</b>
    /// Uses the standard .NET regular expression for email validation, which accepts most
    /// common email formats. Note that full RFC 5322 compliance is not guaranteed,
    /// but the pattern works for typical business scenarios.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
                    AllowMultiple = false)]
    public class EmailAddressAttribute : OrderedValidationAttribute
    {
        /// <summary>
        /// The wrapped System.ComponentModel.DataAnnotations.EmailAddressAttribute.
        /// </summary>
        private readonly System.ComponentModel.DataAnnotations.EmailAddressAttribute _innerAttribute;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddressAttribute"/> class.
        /// </summary>
        public EmailAddressAttribute()
        {
            _innerAttribute = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAddressAttribute"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message to use when validation fails.</param>
        public EmailAddressAttribute(string errorMessage) : base(errorMessage)
        {
            _innerAttribute = new System.ComponentModel.DataAnnotations.EmailAddressAttribute
            {
                ErrorMessage = errorMessage
            };
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
        /// - Empty strings are valid (use Required to disallow empty)
        /// - Non-empty strings must match email format pattern
        /// - Pattern: localpart@domain (e.g., user@example.com)
        /// - Accepts most common email formats
        /// </para>
        ///
        /// <para>
        /// <b>Valid Examples:</b>
        /// - user@example.com
        /// - john.doe@company.co.uk
        /// - info+tag@domain.org
        /// </para>
        ///
        /// <para>
        /// <b>Invalid Examples:</b>
        /// - @example.com (missing local part)
        /// - user@  (missing domain)
        /// - user@domain (missing top-level domain)
        /// - user name@example.com (contains space)
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
