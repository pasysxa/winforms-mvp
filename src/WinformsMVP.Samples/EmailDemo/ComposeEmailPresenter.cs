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
    /// ComposeEmailActions定义
    /// </summary>
    public static class ComposeEmailActions
    {
        private static readonly ViewActionFactory Factory = ViewAction.Factory.WithQualifier("ComposeEmail");

        public static readonly ViewAction Send = Factory.Create("Send");
        public static readonly ViewAction SaveDraft = Factory.Create("SaveDraft");
        public static readonly ViewAction Discard = Factory.Create("Discard");
    }

    /// <summary>
    /// 撰写邮件Presenter
    /// 展示ChangeTracker用法、验证、以及IRequestClose模式
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
            // 订阅View输入变化事件
            View.PropertyChanged += OnViewPropertyChanged;
        }

        protected override void RegisterViewActions()
        {
            // 发送操作
            _dispatcher.Register(ComposeEmailActions.Send, OnSend,
                canExecute: () => !string.IsNullOrWhiteSpace(View.To) &&
                                  !string.IsNullOrWhiteSpace(View.Subject));

            // 保存草稿操作
            _dispatcher.Register(ComposeEmailActions.SaveDraft, OnSaveDraft,
                canExecute: () => _changeTracker?.IsChanged == true);

            // 放弃操作
            _dispatcher.Register(ComposeEmailActions.Discard, OnDiscard);

            // 绑定到View
            View.BindActions(_dispatcher);
        }

        protected override void OnInitialize(ComposeEmailParameters parameters)
        {
            _mode = parameters.Mode;
            _originalEmail = parameters.OriginalEmail;

            // 根据模式初始化邮件内容
            var email = CreateEmailFromMode();

            // 使用ChangeTracker追踪变化（用于保存草稿）
            _changeTracker = new ChangeTracker<EmailMessage>(email);

            // 绑定到View
            View.Mode = _mode;
            View.To = email.To;
            View.Subject = email.Subject;
            View.Body = email.Body;

            // 订阅ChangeTracker变化事件
            _changeTracker.IsChangedChanged += (s, e) =>
            {
                View.IsDirty = _changeTracker.IsChanged;
                _dispatcher.RaiseCanExecuteChanged();
            };
        }

        public bool CanClose()
        {
            // 如果有未保存的更改，提示用户
            if (_changeTracker.IsChanged)
            {
                var result = Messages.ConfirmYesNoCancel(
                    "You have unsaved changes. Do you want to save as draft?",
                    "Unsaved Changes");

                if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    return false;  // 取消关闭
                }

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    // 保存草稿
                    OnSaveDraft();
                }
            }

            return true;
        }

        #region Event Handlers

        private void OnViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // View输入变化时，更新ChangeTracker
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

                // 触发CanExecute更新
                _dispatcher.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Action Handlers

        private async void OnSend()
        {
            // 验证输入
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
                    _changeTracker.AcceptChanges();  // 接受更改，标记为未修改
                    RequestClose(true);  // 返回true表示已发送
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

                _changeTracker.AcceptChanges();  // 接受更改
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

            RequestClose(false);  // 返回false表示未发送
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
                From = "me@mycompany.com",  // 当前用户
                To = View.To,
                Subject = View.Subject,
                Body = View.Body,
                Date = DateTime.Now,
                IsRead = true,
                IsStarred = false,
                Folder = EmailFolder.Sent  // 发送后进入已发送文件夹
            };
        }

        private bool ValidateInput(out string errorMessage)
        {
            // 验证收件人
            if (string.IsNullOrWhiteSpace(View.To))
            {
                errorMessage = "Recipient (To) is required.";
                return false;
            }

            // 验证邮箱格式
            if (!IsValidEmail(View.To))
            {
                errorMessage = "Invalid email address format.";
                return false;
            }

            // 验证主题
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
                // 简单的邮箱格式验证
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
            result = false;  // false表示未发送
            return true;
        }

        #endregion
    }
}
