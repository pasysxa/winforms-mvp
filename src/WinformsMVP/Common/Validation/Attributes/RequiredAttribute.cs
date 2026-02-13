using System;
using System.ComponentModel.DataAnnotations;
using ValidationResult = WinformsMVP.Common.Validation.Core.ValidationResult;

namespace WinformsMVP.Common.Validation.Attributes
{
    /// <summary>
    /// Specifies that a data field value is required, with optional ordering support.
    /// Wraps System.ComponentModel.DataAnnotations.RequiredAttribute to add Order property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// RequiredAttribute is typically the first validation in an ordered sequence (Order = 1),
    /// as checking for required fields before validating format prevents cascading errors.
    /// </para>
    ///
    /// <para>
    /// <b>Example:</b>
    /// <code>
    /// [Required(Order = 1, ErrorMessage = "Email is required")]
    /// [EmailAddress(Order = 2, ErrorMessage = "Invalid email format")]
    /// public string Email { get; set; }
    /// </code>
    /// Sequential validation checks Required first, then EmailAddress only if the field is not empty.
    /// </para>
    ///
    /// <para>
    /// <b>Wrapper Pattern:</b>
    /// This attribute delegates validation logic to the standard DataAnnotations RequiredAttribute,
    /// ensuring compatibility with existing validation behavior while adding ordering capability.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
                    AllowMultiple = false)]
    public class RequiredAttribute : OrderedValidationAttribute
    {
        /// <summary>
        /// The wrapped System.ComponentModel.DataAnnotations.RequiredAttribute.
        /// </summary>
        private readonly System.ComponentModel.DataAnnotations.RequiredAttribute _innerAttribute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredAttribute"/> class.
        /// </summary>
        public RequiredAttribute()
        {
            _innerAttribute = new System.ComponentModel.DataAnnotations.RequiredAttribute();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredAttribute"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message to use when validation fails.</param>
        public RequiredAttribute(string errorMessage) : base(errorMessage)
        {
            _innerAttribute = new System.ComponentModel.DataAnnotations.RequiredAttribute
            {
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Gets or sets a value that determines whether an empty string is considered valid.
        /// </summary>
        /// <value>
        /// true if an empty string is valid; otherwise, false. Default is false.
        /// </value>
        /// <remarks>
        /// By default, empty strings are considered invalid. Set this to true if you want
        /// to allow empty strings (e.g., for optional text fields that must be initialized).
        /// </remarks>
        public bool AllowEmptyStrings
        {
            get => _innerAttribute.AllowEmptyStrings;
            set => _innerAttribute.AllowEmptyStrings = value;
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
        /// - null values are invalid
        /// - Empty strings are invalid (unless AllowEmptyStrings = true)
        /// - Whitespace-only strings are invalid
        /// - All other values are valid
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
