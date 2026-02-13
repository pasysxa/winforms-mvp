using System;
using System.Text.RegularExpressions;
using WinformsMVP.Common;
using WinformsMVP.Common.Events;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Samples.EmailDemo.Models;
using WinformsMVP.Samples.EmailDemo.Services;

namespace WinformsMVP.Samples.EmailDemo
{
    /// <summary>
    /// ComposeEmailActions definition
    /// </summary>
    public static class ComposeEmailActions
    {
        private static readonly ViewActionFactory Factory = ViewAction.Factory.WithQualifier("ComposeEmail");

        public static readonly ViewAction Send = Factory.Create("Send");
        public static readonly ViewAction SaveDraft = Factory.Create("SaveDraft");
        public static readonly ViewAction Discard = Factory.Create("Discard");
    }

    /// <summary>
    /// Compose email Presenter
    /// Demonstrates ChangeTracker usage, validation, and IRequestClose pattern
    /// </summary>
    public class ComposeEmailPresenter : WindowPresenterBase<IComposeEmailView, ComposeEmailParameters>,
                                          IRequestClose<bool>
    {
        private readonly IEmailRepository _repository;
        private ChangeTracker<EmailMessage> _changeTracker;
        private ComposeMode _mode;
        private EmailMessage _originalEmail;

        public ComposeEmailPresenter(IEmailRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        protected override void OnViewAttached()
        {
            // Subscribe to View input change events
            View.PropertyChanged += OnViewPropertyChanged;
        }

        protected override void RegisterViewActions()
        {
            // Send operation
            _dispatcher.Register(ComposeEmailActions.Send, OnSend,
                canExecute: () => !string.IsNullOrWhiteSpace(View.To) &&
                                  !string.IsNullOrWhiteSpace(View.Subject));

            // Save draft operation
            _dispatcher.Register(ComposeEmailActions.SaveDraft, OnSaveDraft,
                canExecute: () => _changeTracker?.IsChanged == true);

            // Discard operation
            _dispatcher.Register(ComposeEmailActions.Discard, OnDiscard);

            // Bind to View - ActionBinder property pattern
            View.ActionBinder.Bind(_dispatcher);
        }

        protected override void OnInitialize(ComposeEmailParameters parameters)
        {
            _mode = parameters.Mode;
            _originalEmail = parameters.OriginalEmail;

            // Initialize email content based on mode
            var email = CreateEmailFromMode();

            // Use ChangeTracker to track changes (for save draft)
            _changeTracker = new ChangeTracker<EmailMessage>(email);

            // Bind to View
            View.Mode = _mode;
            View.To = email.To;
            View.Subject = email.Subject;
            View.Body = email.Body;

            // Subscribe to ChangeTracker change events
            _changeTracker.IsChangedChanged += (s, e) =>
            {
                View.IsDirty = _changeTracker.IsChanged;
                _dispatcher.RaiseCanExecuteChanged();
            };
        }

        public bool CanClose()
        {
            // If there are unsaved changes, prompt user
            if (_changeTracker.IsChanged)
            {
                var result = Messages.ConfirmYesNoCancel(
                    "You have unsaved changes. Do you want to save as draft?",
                    "Unsaved Changes");

                if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    return false;  // Cancel close
                }

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // Save draft
                    OnSaveDraft();
                }
            }

            return true;
        }

        #region Event Handlers

        private void OnViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // When View input changes, update ChangeTracker
            if (e.PropertyName == nameof(View.To) ||
                e.PropertyName == nameof(View.Subject) ||
                e.PropertyName == nameof(View.Body))
            {
                var currentEmail = new EmailMessage
                {
                    To = View.To,
                    Subject = View.Subject,
                    Body = View.Body
                };

                _changeTracker.UpdateCurrentValue(currentEmail);

                // Trigger CanExecute update
                _dispatcher.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Action Handlers

        private async void OnSend()
        {
            // Validate input
            if (!ValidateInput(out string errorMessage))
            {
                Messages.ShowWarning(errorMessage, "Validation Failed");
                return;
            }

            View.IsSaving = true;
            View.EnableInput = false;

            try
            {
                var email = CreateEmailMessage();
                bool success = await _repository.SendEmailAsync(email);

                if (success)
                {
                    _changeTracker.AcceptChanges();  // Accept changes, mark as unmodified
                    RequestClose(true);  // Return true indicating sent
                }
                else
                {
                    Messages.ShowError("Failed to send email.", "Error");
                }
            }
            catch (Exception ex)
            {
                Messages.ShowError($"Error sending email: {ex.Message}", "Error");
            }
            finally
            {
                View.IsSaving = false;
                View.EnableInput = true;
            }
        }

        private async void OnSaveDraft()
        {
            View.IsSaving = true;

            try
            {
                var email = CreateEmailMessage();
                int draftId = await _repository.SaveDraftAsync(email);

                _changeTracker.AcceptChanges();  // Accept changes
                Messages.ShowInfo("Draft saved.", "Success");
            }
            catch (Exception ex)
            {
                Messages.ShowError($"Error saving draft: {ex.Message}", "Error");
            }
            finally
            {
                View.IsSaving = false;
            }
        }

        private void OnDiscard()
        {
            if (_changeTracker.IsChanged)
            {
                if (!Messages.ConfirmYesNo("Discard all changes?", "Confirm Discard"))
                {
                    return;
                }
            }

            RequestClose(false);  // Return false indicating not sent
        }

        #endregion

        #region Helper Methods

        private EmailMessage CreateEmailFromMode()
        {
            switch (_mode)
            {
                case ComposeMode.Reply:
                    if (_originalEmail == null)
                        throw new InvalidOperationException("Original email required for Reply mode");

                    return new EmailMessage
                    {
                        To = _originalEmail.From,
                        Subject = "Re: " + _originalEmail.Subject,
                        Body = $"\n\n--- Original Message ---\nFrom: {_originalEmail.From}\nDate: {_originalEmail.Date}\n\n{_originalEmail.Body}"
                    };

                case ComposeMode.Forward:
                    if (_originalEmail == null)
                        throw new InvalidOperationException("Original email required for Forward mode");

                    return new EmailMessage
                    {
                        To = "",
                        Subject = "Fwd: " + _originalEmail.Subject,
                        Body = $"\n\n--- Forwarded Message ---\nFrom: {_originalEmail.From}\nDate: {_originalEmail.Date}\nSubject: {_originalEmail.Subject}\n\n{_originalEmail.Body}"
                    };

                case ComposeMode.New:
                default:
                    return new EmailMessage
                    {
                        To = "",
                        Subject = "",
                        Body = ""
                    };
            }
        }

        private EmailMessage CreateEmailMessage()
        {
            return new EmailMessage
            {
                From = "me@mycompany.com",  // Current user
                To = View.To,
                Subject = View.Subject,
                Body = View.Body,
                Date = DateTime.Now,
                IsRead = true,
                IsStarred = false,
                Folder = EmailFolder.Sent  // Sent emails go to Sent folder
            };
        }

        private bool ValidateInput(out string errorMessage)
        {
            // Validate recipient
            if (string.IsNullOrWhiteSpace(View.To))
            {
                errorMessage = "Recipient (To) is required.";
                return false;
            }

            // Validate email format
            if (!IsValidEmail(View.To))
            {
                errorMessage = "Invalid email address format.";
                return false;
            }

            // Validate subject
            if (string.IsNullOrWhiteSpace(View.Subject))
            {
                errorMessage = "Subject is required.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Simple email format validation
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region IRequestClose Implementation

        public event EventHandler<CloseRequestedEventArgs<bool>> CloseRequested;

        private void RequestClose(bool result)
        {
            var args = new CloseRequestedEventArgs<bool>(result);
            CloseRequested?.Invoke(this, args);
        }

        public bool TryGetResult(out bool result)
        {
            result = false;  // false indicates not sent
            return true;
        }

        #endregion
    }
}
