using System;
using System.ComponentModel.DataAnnotations;

namespace WinformsMVP.Common.Validation.Attributes
{
    /// <summary>
    /// Base class for validation attributes that support ordered validation.
    /// Extends System.ComponentModel.DataAnnotations.ValidationAttribute with ordering capability.
    ///
    /// The Order property allows validation attributes to be executed in a specific sequence,
    /// enabling "stop on first error" validation scenarios while maintaining compatibility
    /// with standard DataAnnotations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Order Behavior:</b>
    /// - Lower Order values execute first (e.g., Order=1 before Order=2)
    /// - Default Order is 0 (unordered attributes validate first)
    /// - Attributes with the same Order may validate in any order
    /// </para>
    ///
    /// <para>
    /// <b>Validation Modes:</b>
    /// - ValidateAll(): Validates all attributes regardless of Order, collects all errors
    /// - ValidateSequential(): Validates in Order sequence, stops at first error
    /// </para>
    ///
    /// <para>
    /// <b>Example:</b>
    /// <code>
    /// [Required(Order = 1, ErrorMessage = "Email is required")]
    /// [EmailAddress(Order = 2, ErrorMessage = "Invalid email format")]
    /// public string Email { get; set; }
    /// </code>
    /// Sequential validation will check Required first, then EmailAddress only if Required passes.
    /// </para>
    /// </remarks>
    public abstract class OrderedValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Gets or sets the order in which this validation should be executed.
        /// Lower values execute first. Attributes with the same Order may execute in any order.
        /// Default is 0.
        /// </summary>
        /// <value>
        /// The execution order for this validation attribute. Default is 0.
        /// </value>
        /// <remarks>
        /// Use this property to define sequential validation flow:
        /// - Order = 1: Required field validation (must exist before checking format)
        /// - Order = 2: Format validation (e.g., email, regex pattern)
        /// - Order = 3: Cross-property validation (e.g., password confirmation)
        /// - Order = 10: Business rules and custom validation
        /// </remarks>
        public int Order { get; set; } = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedValidationAttribute"/> class.
        /// </summary>
        protected OrderedValidationAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedValidationAttribute"/> class
        /// with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message to associate with this validation attribute.</param>
        protected OrderedValidationAttribute(string errorMessage) : base(errorMessage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedValidationAttribute"/> class
        /// with a function that supplies the error message.
        /// </summary>
        /// <param name="errorMessageAccessor">
        /// A function that returns the error message to associate with this validation attribute.
        /// </param>
        protected OrderedValidationAttribute(Func<string> errorMessageAccessor)
            : base(errorMessageAccessor)
        {
        }
    }
}
