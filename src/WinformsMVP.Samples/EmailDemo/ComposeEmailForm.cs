using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WinformsMVP.MVP.ViewActions;

namespace WinformsMVP.Samples.EmailDemo
{
    /// <summary>
    /// 撰写邮件Form实现
    /// 展示ChangeTracker和验证功能
    /// </summary>
    public partial class ComposeEmailForm : Form, IComposeEmailView, INotifyPropertyChanged
    {
        private ViewActionBinder _binder;
        private bool _isDirty;

        // UI Controls
        private Label toLabel;
        private TextBox toTextBox;
        private Label subjectLabel;
        private TextBox subjectTextBox;
        private Label bodyLabel;
        private TextBox bodyTextBox;
        private Button sendButton;
        private Button saveDraftButton;
        private Button discardButton;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripProgressBar progressBar;
        private Panel buttonPanel;

        public ComposeEmailForm()
        {
            InitializeComponent();
            SetupUI();
            AttachEventHandlers();
        }

        #region IComposeEmailView Implementation

        public string To
        {
            get => toTextBox.Text;
            set
            {
                if (toTextBox.Text != value)
                {
                    toTextBox.Text = value;
                    OnPropertyChanged(nameof(To));
                }
            }
        }

        public string Subject
        {
            get => subjectTextBox.Text;
            set
            {
                if (subjectTextBox.Text != value)
                {
                    subjectTextBox.Text = value;
                    OnPropertyChanged(nameof(Subject));
                }
            }
        }

        public string Body
        {
            get => bodyTextBox.Text;
            set
            {
                if (bodyTextBox.Text != value)
                {
                    bodyTextBox.Text = value;
                    OnPropertyChanged(nameof(Body));
                }
            }
        }

        public ComposeMode Mode { get; set; }

        public bool IsSaving
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        progressBar.Visible = value;
                        statusLabel.Text = value ? "Sending..." : "Ready";
                    }));
                }
                else
                {
                    progressBar.Visible = value;
                    statusLabel.Text = value ? "Sending..." : "Ready";
                }
            }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    UpdateTitle();
                }
            }
        }

        public bool EnableInput
        {
            set
            {
                toTextBox.Enabled = value;
                subjectTextBox.Enabled = value;
                bodyTextBox.Enabled = value;
                sendButton.Enabled = value;
                saveDraftButton.Enabled = value;
                discardButton.Enabled = value;
            }
        }

        public void BindActions(ViewActionDispatcher dispatcher)
        {
            _binder = new ViewActionBinder();

            _binder.Add(ComposeEmailActions.Send, sendButton);
            _binder.Add(ComposeEmailActions.SaveDraft, saveDraftButton);
            _binder.Add(ComposeEmailActions.Discard, discardButton);

            _binder.Bind(dispatcher);
        }

        public ViewActionBinder ActionBinder => _binder;  // Inherited from IViewBase

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region UI Setup

        private void InitializeComponent()
        {
            this.components = new Container();
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(600, 400);

            CreateControls();
            LayoutControls();
        }

        private void CreateControls()
        {
            // To field
            toLabel = new Label
            {
                Text = "To:",
                AutoSize = true,
                Location = new Point(20, 20)
            };

            toTextBox = new TextBox
            {
                Location = new Point(100, 17),
                Width = 650,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Subject field
            subjectLabel = new Label
            {
                Text = "Subject:",
                AutoSize = true,
                Location = new Point(20, 55)
            };

            subjectTextBox = new TextBox
            {
                Location = new Point(100, 52),
                Width = 650,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Body field
            bodyLabel = new Label
            {
                Text = "Body:",
                AutoSize = true,
                Location = new Point(20, 90)
            };

            bodyTextBox = new TextBox
            {
                Location = new Point(20, 110),
                Width = 730,
                Height = 350,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                AcceptsReturn = true,
                AcceptsTab = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Button panel
            buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };

            sendButton = new Button
            {
                Text = "Send",
                Width = 100,
                Height = 30,
                Location = new Point(10, 10)
            };

            saveDraftButton = new Button
            {
                Text = "Save Draft",
                Width = 100,
                Height = 30,
                Location = new Point(120, 10)
            };

            discardButton = new Button
            {
                Text = "Discard",
                Width = 100,
                Height = 30,
                Location = new Point(230, 10)
            };

            buttonPanel.Controls.Add(sendButton);
            buttonPanel.Controls.Add(saveDraftButton);
            buttonPanel.Controls.Add(discardButton);

            // Status strip
            statusStrip = new StatusStrip();

            statusLabel = new ToolStripStatusLabel
            {
                Text = "Ready",
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

        private void LayoutControls()
        {
            this.Controls.Add(toLabel);
            this.Controls.Add(toTextBox);
            this.Controls.Add(subjectLabel);
            this.Controls.Add(subjectTextBox);
            this.Controls.Add(bodyLabel);
            this.Controls.Add(bodyTextBox);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(statusStrip);
        }

        private void SetupUI()
        {
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            var modeText = Mode switch
            {
                ComposeMode.Reply => "Reply",
                ComposeMode.Forward => "Forward",
                _ => "New Message"
            };

            this.Text = _isDirty ? $"{modeText} *" : modeText;
        }

        private void AttachEventHandlers()
        {
            // 监听输入变化以触发PropertyChanged
            toTextBox.TextChanged += (s, e) => OnPropertyChanged(nameof(To));
            subjectTextBox.TextChanged += (s, e) => OnPropertyChanged(nameof(Subject));
            bodyTextBox.TextChanged += (s, e) => OnPropertyChanged(nameof(Body));

            // 回车键焦点切换
            toTextBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    subjectTextBox.Focus();
                    e.SuppressKeyPress = true;
                }
            };

            subjectTextBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    bodyTextBox.Focus();
                    e.SuppressKeyPress = true;
                }
            };

            // Ctrl+Enter 快捷键发送
            bodyTextBox.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.Enter)
                {
                    sendButton.PerformClick();
                    e.SuppressKeyPress = true;
                }
            };
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
