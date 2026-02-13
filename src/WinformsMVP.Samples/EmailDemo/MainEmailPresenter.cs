using System;
using System.Linq;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Samples.EmailDemo.Models;
using WinformsMVP.Samples.EmailDemo.Services;

namespace WinformsMVP.Samples.EmailDemo
{
    /// <summary>
    /// EmailAction定义
    /// </summary>
    public static class EmailActions
    {
        private static readonly ViewActionFactory Factory = ViewAction.Factory.WithQualifier("Email");

        // 邮件操作
        public static readonly ViewAction Compose = Factory.Create("Compose");
        public static readonly ViewAction Reply = Factory.Create("Reply");
        public static readonly ViewAction Forward = Factory.Create("Forward");
        public static readonly ViewAction Delete = Factory.Create("Delete");
        public static readonly ViewAction Refresh = Factory.Create("Refresh");

        // 邮件状态
        public static readonly ViewAction MarkAsRead = Factory.Create("MarkAsRead");
        public static readonly ViewAction MarkAsUnread = Factory.Create("MarkAsUnread");
        public static readonly ViewAction ToggleStar = Factory.Create("ToggleStar");

        // 查看
        public static readonly ViewAction OpenEmail = Factory.Create("OpenEmail");
    }

    /// <summary>
    /// 主邮件视图Presenter
    /// 展示所有MVP框架特性的完整示例
    /// </summary>
    public class MainEmailPresenter : WindowPresenterBase<IMainEmailView>
    {
        private readonly IEmailRepository _repository;
        private EmailFolder _currentFolder = EmailFolder.Inbox;

        public MainEmailPresenter(IEmailRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        protected override void OnViewAttached()
        {
            // 订阅View事件
            View.EmailSelectionChanged += OnEmailSelectionChanged;
            View.FolderChanged += OnFolderChanged;
        }

        protected override void RegisterViewActions()
        {
            // 邮件操作
            _dispatcher.Register(EmailActions.Compose, OnCompose);
            _dispatcher.Register(EmailActions.Reply, OnReply,
                canExecute: () => View.HasSelection);
            _dispatcher.Register(EmailActions.Forward, OnForward,
                canExecute: () => View.HasSelection);
            _dispatcher.Register(EmailActions.Delete, OnDelete,
                canExecute: () => View.HasSelection);
            _dispatcher.Register(EmailActions.Refresh, OnRefresh);

            // 邮件状态操作
            _dispatcher.Register(EmailActions.MarkAsRead, OnMarkAsRead,
                canExecute: () => View.HasSelection && View.SelectedEmail != null && !View.SelectedEmail.IsRead);
            _dispatcher.Register(EmailActions.MarkAsUnread, OnMarkAsUnread,
                canExecute: () => View.HasSelection && View.SelectedEmail != null && View.SelectedEmail.IsRead);
            _dispatcher.Register(EmailActions.ToggleStar, OnToggleStar,
                canExecute: () => View.HasSelection);

            // 查看操作
            _dispatcher.Register(EmailActions.OpenEmail, OnOpenEmail,
                canExecute: () => View.HasSelection);

            // 绑定到View
            View.BindActions(_dispatcher);
        }

        protected override void OnInitialize()
        {
            // 初始加载收件箱
            View.CurrentFolder = EmailFolder.Inbox;
            LoadEmailsAsync(_currentFolder);
        }

        #region Event Handlers

        private void OnEmailSelectionChanged(object sender, EventArgs e)
        {
            // 更新CanExecute状态
            _dispatcher.RaiseCanExecuteChanged();

            // 显示邮件预览
            if (View.HasSelection && View.SelectedEmail != null)
            {
                View.ShowEmailPreview(View.SelectedEmail);

                // 自动标记为已读
                if (!View.SelectedEmail.IsRead)
                {
                    _ = MarkEmailAsReadAsync(View.SelectedEmail.Id, true);  // Fire-and-forget
                }
            }
            else
            {
                View.ClearEmailPreview();
            }
        }

        private void OnFolderChanged(object sender, EmailFolder folder)
        {
            _currentFolder = folder;
            View.CurrentFolder = folder;
            LoadEmailsAsync(folder);
        }

        #endregion

        #region Action Handlers

        private void OnCompose()
        {
            var composePresenter = new ComposeEmailPresenter(_repository);
            var result = Navigator.ShowWindowAsModal<ComposeEmailPresenter, ComposeEmailParameters, bool>(
                composePresenter,
                new ComposeEmailParameters
                {
                    Mode = ComposeMode.New
                });

            if (result.IsOk && result.Value)
            {
                // 邮件已发送，刷新列表
                Messages.ShowInfo("Email sent successfully!", "Success");
                LoadEmailsAsync(_currentFolder);
            }
        }

        private void OnReply()
        {
            var originalEmail = View.SelectedEmail;
            if (originalEmail == null) return;

            var composePresenter = new ComposeEmailPresenter(_repository);
            var result = Navigator.ShowWindowAsModal<ComposeEmailPresenter, ComposeEmailParameters, bool>(
                composePresenter,
                new ComposeEmailParameters
                {
                    Mode = ComposeMode.Reply,
                    OriginalEmail = originalEmail
                });

            if (result.IsOk && result.Value)
            {
                Messages.ShowInfo("Reply sent successfully!", "Success");
                LoadEmailsAsync(_currentFolder);
            }
        }

        private void OnForward()
        {
            var originalEmail = View.SelectedEmail;
            if (originalEmail == null) return;

            var composePresenter = new ComposeEmailPresenter(_repository);
            var result = Navigator.ShowWindowAsModal<ComposeEmailPresenter, ComposeEmailParameters, bool>(
                composePresenter,
                new ComposeEmailParameters
                {
                    Mode = ComposeMode.Forward,
                    OriginalEmail = originalEmail
                });

            if (result.IsOk && result.Value)
            {
                Messages.ShowInfo("Email forwarded successfully!", "Success");
                LoadEmailsAsync(_currentFolder);
            }
        }

        private async void OnDelete()
        {
            var email = View.SelectedEmail;
            if (email == null) return;

            var confirmMessage = _currentFolder == EmailFolder.Trash
                ? "Permanently delete this email? This cannot be undone."
                : "Move this email to Trash?";

            if (!Messages.ConfirmYesNo(confirmMessage, "Delete Email"))
                return;

            View.IsLoading = true;

            try
            {
                bool success;
                if (_currentFolder == EmailFolder.Trash)
                {
                    success = await _repository.PermanentlyDeleteEmailAsync(email.Id);
                }
                else
                {
                    success = await _repository.DeleteEmailAsync(email.Id);
                }

                if (success)
                {
                    Messages.ShowInfo("Email deleted.", "Success");
                    LoadEmailsAsync(_currentFolder);
                }
                else
                {
                    Messages.ShowError("Failed to delete email.", "Error");
                }
            }
            catch (Exception ex)
            {
                Messages.ShowError($"Error deleting email: {ex.Message}", "Error");
            }
            finally
            {
                View.IsLoading = false;
            }
        }

        private void OnRefresh()
        {
            LoadEmailsAsync(_currentFolder);
        }

        private async void OnMarkAsRead()
        {
            var email = View.SelectedEmail;
            if (email == null) return;

            await MarkEmailAsReadAsync(email.Id, true);
            _dispatcher.RaiseCanExecuteChanged();
        }

        private async void OnMarkAsUnread()
        {
            var email = View.SelectedEmail;
            if (email == null) return;

            await MarkEmailAsReadAsync(email.Id, false);
            _dispatcher.RaiseCanExecuteChanged();
        }

        private async void OnToggleStar()
        {
            var email = View.SelectedEmail;
            if (email == null) return;

            View.IsLoading = true;

            try
            {
                await _repository.ToggleStarAsync(email.Id);
                LoadEmailsAsync(_currentFolder);
            }
            catch (Exception ex)
            {
                Messages.ShowError($"Error toggling star: {ex.Message}", "Error");
            }
            finally
            {
                View.IsLoading = false;
            }
        }

        private void OnOpenEmail()
        {
            var email = View.SelectedEmail;
            if (email == null) return;

            // TODO: 打开详情窗口
            Messages.ShowInfo("Email detail window - Coming soon!", "Info");
        }

        #endregion

        #region Helper Methods

        private async void LoadEmailsAsync(EmailFolder folder)
        {
            View.IsLoading = true;
            View.StatusMessage = "Loading emails...";

            try
            {
                var emails = await _repository.GetEmailsByFolderAsync(folder);
                var emailList = emails.ToList();

                View.Emails = emailList;
                View.RefreshEmailList();

                // 更新未读数量
                var unreadCount = emailList.Count(e => !e.IsRead);
                View.UnreadCount = unreadCount;

                View.StatusMessage = $"{emailList.Count} emails ({unreadCount} unread)";
            }
            catch (Exception ex)
            {
                Messages.ShowError($"Error loading emails: {ex.Message}", "Error");
                View.StatusMessage = "Error loading emails";
            }
            finally
            {
                View.IsLoading = false;
            }
        }

        private async System.Threading.Tasks.Task MarkEmailAsReadAsync(int emailId, bool isRead)
        {
            try
            {
                await _repository.MarkAsReadAsync(emailId, isRead);
                LoadEmailsAsync(_currentFolder);
            }
            catch (Exception ex)
            {
                Messages.ShowError($"Error updating email status: {ex.Message}", "Error");
            }
        }

        #endregion
    }
}
