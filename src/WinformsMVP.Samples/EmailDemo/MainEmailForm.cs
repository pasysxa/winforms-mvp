using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WinformsMVP.MVP.ViewActions;
using WinformsMVP.Samples.EmailDemo.Models;

namespace WinformsMVP.Samples.EmailDemo
{
    /// <summary>
    /// 主邮件视图Form实现
    /// 展示完整的邮件客户端UI
    /// </summary>
    public partial class MainEmailForm : Form, IMainEmailView
    {
        private ViewActionBinder _binder;
        private ViewActionDispatcher _dispatcher;
        private EmailMessage _selectedEmail;
        private IEnumerable<EmailMessage> _emails;

        // UI Controls
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private ListBox folderListBox;
        private ListView emailListView;
        private RichTextBox previewTextBox;
        private ToolStrip toolStrip;
        private ToolStripButton composeButton;
        private ToolStripButton replyButton;
        private ToolStripButton forwardButton;
        private ToolStripButton deleteButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton refreshButton;
        private ToolStripButton markReadButton;
        private ToolStripButton markUnreadButton;
        private ToolStripButton toggleStarButton;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripProgressBar progressBar;
        private Label previewFromLabel;
        private Label previewSubjectLabel;
        private Label previewDateLabel;
        private Panel previewHeaderPanel;

        public MainEmailForm()
        {
            InitializeComponent();
            SetupUI();
        }

        #region IMainEmailView Implementation

        public IEnumerable<EmailMessage> Emails
        {
            set
            {
                _emails = value;
            }
        }

        public EmailMessage SelectedEmail => _selectedEmail;

        public bool HasSelection => _selectedEmail != null;

        private EmailFolder _currentFolder = EmailFolder.Inbox;
        public EmailFolder CurrentFolder
        {
            get => _currentFolder;
            set
            {
                _currentFolder = value;
                UpdateFolderSelection();
            }
        }

        public int UnreadCount
        {
            set
            {
                // 更新文件夹列表中的未读数量显示
                UpdateFolderList();
            }
        }

        public string StatusMessage
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => statusLabel.Text = value));
                }
                else
                {
                    statusLabel.Text = value;
                }
            }
        }

        public bool IsLoading
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        progressBar.Visible = value;
                        toolStrip.Enabled = !value;
                    }));
                }
                else
                {
                    progressBar.Visible = value;
                    toolStrip.Enabled = !value;
                }
            }
        }

        public void ShowEmailPreview(EmailMessage email)
        {
            if (email == null) return;

            previewFromLabel.Text = $"From: {email.From}";
            previewSubjectLabel.Text = email.Subject;
            previewDateLabel.Text = email.Date.ToString("yyyy-MM-dd HH:mm");
            previewTextBox.Text = email.Body;

            // 粗体显示未读邮件
            previewSubjectLabel.Font = email.IsRead
                ? new Font(previewSubjectLabel.Font, FontStyle.Regular)
                : new Font(previewSubjectLabel.Font, FontStyle.Bold);
        }

        public void ClearEmailPreview()
        {
            previewFromLabel.Text = "";
            previewSubjectLabel.Text = "";
            previewDateLabel.Text = "";
            previewTextBox.Text = "";
        }

        public void RefreshEmailList()
        {
            emailListView.Items.Clear();

            if (_emails == null) return;

            foreach (var email in _emails)
            {
                var item = new ListViewItem(new[]
                {
                    email.IsStarred ? "★" : "",
                    email.IsRead ? "" : "●",
                    email.GetFromDisplayName(),
                    email.Subject,
                    email.GetBodyPreview(50),
                    email.Date.ToString("MM/dd HH:mm")
                })
                {
                    Tag = email,
                    Font = email.IsRead
                        ? emailListView.Font
                        : new Font(emailListView.Font, FontStyle.Bold)
                };

                emailListView.Items.Add(item);
            }

            // 调整列宽
            emailListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        public event EventHandler EmailSelectionChanged;
        public event EventHandler<EmailFolder> FolderChanged;

        public void BindActions(ViewActionDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _binder = new ViewActionBinder();

            // 邮件操作
            _binder.Add(EmailActions.Compose, composeButton);
            _binder.Add(EmailActions.Reply, replyButton);
            _binder.Add(EmailActions.Forward, forwardButton);
            _binder.Add(EmailActions.Delete, deleteButton);
            _binder.Add(EmailActions.Refresh, refreshButton);

            // 邮件状态操作
            _binder.Add(EmailActions.MarkAsRead, markReadButton);
            _binder.Add(EmailActions.MarkAsUnread, markUnreadButton);
            _binder.Add(EmailActions.ToggleStar, toggleStarButton);

            _binder.Bind(dispatcher);
        }

        public ViewActionBinder ActionBinder => _binder;  // Inherited from IViewBase

        #endregion

        #region UI Setup

        private void InitializeComponent()
        {
            this.components = new Container();
            this.Text = "Email Demo - WinForms MVP Framework";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create controls
            CreateSplitContainers();
            CreateFolderList();
            CreateEmailList();
            CreatePreviewPane();
            CreateToolStrip();
            CreateStatusStrip();

            // Add to form
            this.Controls.Add(splitContainer1);
            this.Controls.Add(toolStrip);
            this.Controls.Add(statusStrip);
        }

        private void CreateSplitContainers()
        {
            // Main split (folders | emails+preview)
            splitContainer1 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 200,
                FixedPanel = FixedPanel.Panel1
            };

            // Email split (list | preview)
            splitContainer2 = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 400
            };

            splitContainer1.Panel2.Controls.Add(splitContainer2);
        }

        private void CreateFolderList()
        {
            var label = new Label
            {
                Text = "Folders",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5),
                Font = new Font(this.Font, FontStyle.Bold)
            };

            folderListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false
            };

            folderListBox.Items.Add("📥 Inbox");
            folderListBox.Items.Add("📤 Sent");
            folderListBox.Items.Add("📝 Drafts");
            folderListBox.Items.Add("🗑️ Trash");

            folderListBox.SelectedIndex = 0;
            folderListBox.SelectedIndexChanged += FolderListBox_SelectedIndexChanged;

            splitContainer1.Panel1.Controls.Add(folderListBox);
            splitContainer1.Panel1.Controls.Add(label);
        }

        private void CreateEmailList()
        {
            emailListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };

            emailListView.Columns.Add("", 30);          // Star
            emailListView.Columns.Add("", 30);          // Unread indicator
            emailListView.Columns.Add("From", 150);
            emailListView.Columns.Add("Subject", 250);
            emailListView.Columns.Add("Preview", 300);
            emailListView.Columns.Add("Date", 100);

            emailListView.SelectedIndexChanged += EmailListView_SelectedIndexChanged;
            emailListView.DoubleClick += EmailListView_DoubleClick;

            splitContainer2.Panel1.Controls.Add(emailListView);
        }

        private void CreatePreviewPane()
        {
            previewHeaderPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(10),
                BackColor = SystemColors.Control
            };

            previewFromLabel = new Label
            {
                Location = new Point(10, 10),
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 9f)
            };

            previewSubjectLabel = new Label
            {
                Location = new Point(10, 30),
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 10f, FontStyle.Bold)
            };

            previewDateLabel = new Label
            {
                Location = new Point(10, 55),
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 8.5f),
                ForeColor = Color.Gray
            };

            previewHeaderPanel.Controls.Add(previewFromLabel);
            previewHeaderPanel.Controls.Add(previewSubjectLabel);
            previewHeaderPanel.Controls.Add(previewDateLabel);

            previewTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(10)
            };

            splitContainer2.Panel2.Controls.Add(previewTextBox);
            splitContainer2.Panel2.Controls.Add(previewHeaderPanel);
        }

        private void CreateToolStrip()
        {
            toolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden
            };

            composeButton = new ToolStripButton("Compose", null) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText };
            replyButton = new ToolStripButton("Reply", null) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText };
            forwardButton = new ToolStripButton("Forward", null) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText };
            deleteButton = new ToolStripButton("Delete", null) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText };

            toolStripSeparator1 = new ToolStripSeparator();

            refreshButton = new ToolStripButton("Refresh", null) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText };
            markReadButton = new ToolStripButton("Mark Read", null) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText };
            markUnreadButton = new ToolStripButton("Mark Unread", null) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText };
            toggleStarButton = new ToolStripButton("Toggle Star", null) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText };

            toolStrip.Items.Add(composeButton);
            toolStrip.Items.Add(replyButton);
            toolStrip.Items.Add(forwardButton);
            toolStrip.Items.Add(deleteButton);
            toolStrip.Items.Add(toolStripSeparator1);
            toolStrip.Items.Add(refreshButton);
            toolStrip.Items.Add(markReadButton);
            toolStrip.Items.Add(markUnreadButton);
            toolStrip.Items.Add(toggleStarButton);
        }

        private void CreateStatusStrip()
        {
            statusStrip = new StatusStrip();

            statusLabel = new ToolStripStatusLabel
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            progressBar = new ToolStripProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            statusStrip.Items.Add(statusLabel);
            statusStrip.Items.Add(progressBar);
        }

        private void SetupUI()
        {
            // 设置ListView样式
            emailListView.View = View.Details;
            emailListView.FullRowSelect = true;

            // 初始化状态
            statusLabel.Text = "Ready";
        }

        private void UpdateFolderList()
        {
            // 可以在这里更新未读数量显示
            // 例如: "📥 Inbox (5)"
        }

        private void UpdateFolderSelection()
        {
            folderListBox.SelectedIndex = (int)_currentFolder;
        }

        #endregion

        #region Event Handlers

        private void FolderListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (folderListBox.SelectedIndex < 0) return;

            var folder = (EmailFolder)folderListBox.SelectedIndex;
            FolderChanged?.Invoke(this, folder);
        }

        private void EmailListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (emailListView.SelectedItems.Count > 0)
            {
                _selectedEmail = emailListView.SelectedItems[0].Tag as EmailMessage;
            }
            else
            {
                _selectedEmail = null;
            }

            EmailSelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void EmailListView_DoubleClick(object sender, EventArgs e)
        {
            if (HasSelection)
            {
                // 双击打开邮件详情
                _dispatcher?.Dispatch(EmailActions.OpenEmail);
            }
        }

        #endregion

        #region IDisposable

        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
