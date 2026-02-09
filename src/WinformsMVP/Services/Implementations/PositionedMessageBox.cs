using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinformsMVP.Services.Implementations
{
    /// <summary>
    /// A message box that can be positioned at a specific screen location.
    /// </summary>
    internal class PositionedMessageBox : Form
    {
        private Label _messageLabel;
        private Panel _buttonPanel;
        private Button _primaryButton;
        private Button _secondaryButton;
        private Button _tertiaryButton;

        public DialogResult Result { get; private set; }

        public PositionedMessageBox(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, Point location)
        {
            InitializeComponent(text, caption, buttons, icon);
            this.StartPosition = FormStartPosition.Manual;
            this.Location = location;
        }

        private void InitializeComponent(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            this.Text = caption;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.Size = new Size(400, 150);
            this.Font = new Font("Segoe UI", 9f);

            // Message label with icon
            _messageLabel = new Label
            {
                Text = text,
                AutoSize = false,
                Size = new Size(340, 60),
                Location = new Point(50, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Icon (simplified - just show text indicator)
            var iconLabel = new Label
            {
                Text = GetIconText(icon),
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                Size = new Size(40, 40),
                Location = new Point(10, 20),
                ForeColor = GetIconColor(icon),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Button panel
            _buttonPanel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Bottom,
                Padding = new Padding(0, 5, 10, 5)
            };

            // Create buttons based on MessageBoxButtons
            CreateButtons(buttons);

            this.Controls.Add(iconLabel);
            this.Controls.Add(_messageLabel);
            this.Controls.Add(_buttonPanel);
        }

        private void CreateButtons(MessageBoxButtons buttons)
        {
            int buttonWidth = 80;
            int buttonHeight = 30;
            int spacing = 10;
            int rightMargin = 10;

            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    _primaryButton = CreateButton("OK", DialogResult.OK, rightMargin, buttonWidth, buttonHeight);
                    _buttonPanel.Controls.Add(_primaryButton);
                    this.AcceptButton = _primaryButton;
                    break;

                case MessageBoxButtons.OKCancel:
                    _secondaryButton = CreateButton("Cancel", DialogResult.Cancel, rightMargin, buttonWidth, buttonHeight);
                    _primaryButton = CreateButton("OK", DialogResult.OK, rightMargin + buttonWidth + spacing, buttonWidth, buttonHeight);
                    _buttonPanel.Controls.Add(_secondaryButton);
                    _buttonPanel.Controls.Add(_primaryButton);
                    this.AcceptButton = _primaryButton;
                    this.CancelButton = _secondaryButton;
                    break;

                case MessageBoxButtons.YesNo:
                    _secondaryButton = CreateButton("No", DialogResult.No, rightMargin, buttonWidth, buttonHeight);
                    _primaryButton = CreateButton("Yes", DialogResult.Yes, rightMargin + buttonWidth + spacing, buttonWidth, buttonHeight);
                    _buttonPanel.Controls.Add(_secondaryButton);
                    _buttonPanel.Controls.Add(_primaryButton);
                    this.AcceptButton = _primaryButton;
                    break;

                case MessageBoxButtons.YesNoCancel:
                    _tertiaryButton = CreateButton("Cancel", DialogResult.Cancel, rightMargin, buttonWidth, buttonHeight);
                    _secondaryButton = CreateButton("No", DialogResult.No, rightMargin + buttonWidth + spacing, buttonWidth, buttonHeight);
                    _primaryButton = CreateButton("Yes", DialogResult.Yes, rightMargin + (buttonWidth + spacing) * 2, buttonWidth, buttonHeight);
                    _buttonPanel.Controls.Add(_tertiaryButton);
                    _buttonPanel.Controls.Add(_secondaryButton);
                    _buttonPanel.Controls.Add(_primaryButton);
                    this.AcceptButton = _primaryButton;
                    this.CancelButton = _tertiaryButton;
                    break;
            }
        }

        private Button CreateButton(string text, DialogResult result, int rightOffset, int width, int height)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(width, height),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(_buttonPanel.Width - rightOffset - width, 5)
            };
            button.Click += (s, e) =>
            {
                this.Result = result;
                this.DialogResult = result;
                this.Close();
            };
            return button;
        }

        private string GetIconText(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Information:
                    return "ℹ";
                case MessageBoxIcon.Warning:  // Warning and Exclamation are same value
                    return "⚠";
                case MessageBoxIcon.Error:  // Error and Stop are same value
                    return "✖";
                case MessageBoxIcon.Question:
                    return "?";
                default:
                    return "";
            }
        }

        private Color GetIconColor(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Information:
                    return Color.FromArgb(0, 120, 215);
                case MessageBoxIcon.Warning:  // Warning and Exclamation are same value
                    return Color.Orange;
                case MessageBoxIcon.Error:  // Error and Stop are same value
                    return Color.Red;
                case MessageBoxIcon.Question:
                    return Color.FromArgb(0, 120, 215);
                default:
                    return Color.Black;
            }
        }
    }
}
