using System;
using System.Drawing;
using System.Windows.Forms;
using WinformsMVP.Common;

namespace WinformsMVP.Services.Implementations
{
    /// <summary>
    /// A non-blocking toast notification that appears temporarily at the bottom-right of the screen.
    /// </summary>
    internal class ToastNotification : Form
    {
        private Label _messageLabel;
        private Timer _fadeTimer;
        private Timer _closeTimer;
        private double _opacity = 1.0;

        public ToastNotification(string message, ToastType type, int duration)
        {
            InitializeComponent(message, type);
            SetupTimers(duration);
            PositionToast();
        }

        private void InitializeComponent(string message, ToastType type)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.Size = new Size(350, 80);
            this.BackColor = GetBackgroundColor(type);
            this.Opacity = 0.95;

            // Add subtle shadow effect
            this.Padding = new Padding(10);

            // Icon
            var iconLabel = new Label
            {
                Text = GetIconText(type),
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(40, 60),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // Message
            _messageLabel = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(280, 60),
                Location = new Point(60, 10),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            // Close button
            var closeButton = new Label
            {
                Text = "✖",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(20, 20),
                Location = new Point(320, 5),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            closeButton.Click += (s, e) => this.Close();

            this.Controls.Add(iconLabel);
            this.Controls.Add(_messageLabel);
            this.Controls.Add(closeButton);

            // Click to close
            this.Click += (s, e) => this.Close();
            _messageLabel.Click += (s, e) => this.Close();
        }

        private void SetupTimers(int duration)
        {
            // Close timer - starts the fade out
            _closeTimer = new Timer();
            _closeTimer.Interval = duration;
            _closeTimer.Tick += (s, e) =>
            {
                _closeTimer.Stop();
                StartFadeOut();
            };

            // Fade timer - gradually reduces opacity
            _fadeTimer = new Timer();
            _fadeTimer.Interval = 50; // 50ms for smooth fade
            _fadeTimer.Tick += FadeTimer_Tick;
        }

        private void PositionToast()
        {
            // Position at bottom-right of screen with margin
            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(
                screen.Right - this.Width - 20,
                screen.Bottom - this.Height - 20
            );
        }

        private void StartFadeOut()
        {
            _fadeTimer.Start();
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            _opacity -= 0.1;
            if (_opacity <= 0)
            {
                _fadeTimer.Stop();
                this.Close();
            }
            else
            {
                this.Opacity = _opacity;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _closeTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _closeTimer?.Dispose();
                _fadeTimer?.Dispose();
            }
            base.Dispose(disposing);
        }

        private Color GetBackgroundColor(ToastType type)
        {
            switch (type)
            {
                case ToastType.Success:
                    return Color.FromArgb(16, 137, 62); // Green
                case ToastType.Warning:
                    return Color.FromArgb(255, 140, 0); // Orange
                case ToastType.Error:
                    return Color.FromArgb(232, 17, 35); // Red
                case ToastType.Info:
                default:
                    return Color.FromArgb(0, 120, 215); // Blue
            }
        }

        private string GetIconText(ToastType type)
        {
            switch (type)
            {
                case ToastType.Success:
                    return "✓";
                case ToastType.Warning:
                    return "⚠";
                case ToastType.Error:
                    return "✖";
                case ToastType.Info:
                default:
                    return "ℹ";
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // Make the form semi-transparent and click-through for better UX
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00080000; // WS_EX_LAYERED
                return cp;
            }
        }
    }
}
