using System.Linq;
using WinformsMVP.Common.Validation.Core;
using WinformsMVP.Services;

namespace WinformsMVP.Common.Validation.Helpers
{
    /// <summary>
    /// Provides helper methods for common validation scenarios.
    /// Simplifies integration between ModelValidator and IMessageService.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ValidationHelper eliminates boilerplate code by combining validation and error display:
    /// </para>
    ///
    /// <para>
    /// <b>Without Helper:</b>
    /// <code>
    /// var errors = validator.ValidateAll(model);
    /// if (errors.Any())
    /// {
    ///     var errorMessage = string.Join("\n", errors.Select(e => e.GetFormattedMessage()));
    ///     messageService.ShowWarning(errorMessage, "Validation Failed");
    ///     return false;
    /// }
    /// return true;
    /// </code>
    /// </para>
    ///
    /// <para>
    /// <b>With Helper:</b>
    /// <code>
    /// if (!ValidationHelper.ValidateAndShow(model, validator, messageService))
    ///     return;
    /// </code>
    /// </para>
    /// </remarks>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates the model and displays all errors via IMessageService.
        /// </summary>
        /// <typeparam name="T">The type of model to validate.</typeparam>
        /// <param name="model">The model instance to validate.</param>
        /// <param name="validator">The validator to use for validation.</param>
        /// <param name="messageService">The message service to display errors.</param>
        /// <param name="errorTitle">
        /// The title for the error message dialog. Default is "Validation Failed".
        /// </param>
        /// <returns>
        /// true if the model is valid (no errors); otherwise, false.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method performs ValidateAll() to collect all validation errors,
        /// then displays them in a single message dialog. This provides better
        /// user experience by showing all issues at once.
        /// </para>
        ///
        /// <para>
        /// <b>Example Usage in Presenter:</b>
        /// <code>
        /// private void OnSave()
        /// {
        ///     var currentEmail = CreateEmailMessage();
        ///
        ///     if (!ValidationHelper.ValidateAndShow(currentEmail, _validator, Messages))
        ///         return;  // Validation failed, user was notified
        ///
        ///     // Proceed with save...
        ///     SaveEmail(currentEmail);
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        public static bool ValidateAndShow<T>(
            T model,
            ModelValidator<T> validator,
            IMessageService messageService,
            string errorTitle = "Validation Failed") where T : class
        {
            var errors = validator.ValidateAll(model);
            if (!errors.Any())
                return true;

            var errorMessage = string.Join("\n", errors.Select(e => e.GetFormattedMessage()));
            messageService.ShowWarning(errorMessage, errorTitle);
            return false;
        }

        /// <summary>
        /// Validates the model sequentially and displays the first error via IMessageService.
        /// </summary>
        /// <typeparam name="T">The type of model to validate.</typeparam>
        /// <param name="model">The model instance to validate.</param>
        /// <param name="validator">The validator to use for validation.</param>
        /// <param name="messageService">The message service to display errors.</param>
        /// <param name="errorTitle">
        /// The title for the error message dialog. Default is "Validation Failed".
        /// </param>
        /// <returns>
        /// true if the model is valid (no errors); otherwise, false.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method performs ValidateSequential() to find the first validation error
        /// (by Order), then displays it. This is useful for:
        /// - Guiding users through fixing errors one at a time
        /// - Performance (stops at first error)
        /// - Preventing cascading errors
        /// </para>
        ///
        /// <para>
        /// <b>Example Usage in Presenter:</b>
        /// <code>
        /// private void OnSend()
        /// {
        ///     var currentEmail = CreateEmailMessage();
        ///
        ///     if (!ValidationHelper.ValidateSequentialAndShow(currentEmail, _validator, Messages))
        ///         return;  // First validation error was shown, user was notified
        ///
        ///     // Proceed with send...
        ///     SendEmail(currentEmail);
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        public static bool ValidateSequentialAndShow<T>(
            T model,
            ModelValidator<T> validator,
            IMessageService messageService,
            string errorTitle = "Validation Failed") where T : class
        {
            var error = validator.ValidateSequential(model);
            if (error.IsValid)
                return true;

            messageService.ShowWarning(error.GetFormattedMessage(), errorTitle);
            return false;
        }

        /// <summary>
        /// Validates a single property and displays errors via IMessageService.
        /// </summary>
        /// <typeparam name="T">The type of model to validate.</typeparam>
        /// <param name="model">The model instance to validate.</param>
        /// <param name="propertyName">The name of the property to validate.</param>
        /// <param name="validator">The validator to use for validation.</param>
        /// <param name="messageService">The message service to display errors.</param>
        /// <param name="errorTitle">
        /// The title for the error message dialog. Default is "Validation Error".
        /// </param>
        /// <returns>
        /// true if the property is valid (no errors); otherwise, false.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method is useful for real-time validation scenarios where you want
        /// to validate a property as the user types or changes focus.
        /// </para>
        ///
        /// <para>
        /// <b>Example Usage in Property Setter:</b>
        /// <code>
        /// public string Email
        /// {
        ///     set
        ///     {
        ///         _email = value;
        ///         ValidationHelper.ValidatePropertyAndShow(
        ///             this,
        ///             nameof(Email),
        ///             _validator,
        ///             _messageService);
        ///     }
        /// }
        /// </code>
        /// </para>
        /// </remarks>
        public static bool ValidatePropertyAndShow<T>(
            T model,
            string propertyName,
            ModelValidator<T> validator,
            IMessageService messageService,
            string errorTitle = "Validation Error") where T : class
        {
            var errors = validator.ValidateProperty(model, propertyName);
            if (!errors.Any())
                return true;

            var errorMessage = string.Join("\n", errors.Select(e => e.GetFormattedMessage()));
            messageService.ShowWarning(errorMessage, errorTitle);
            return false;
        }
    }
}
