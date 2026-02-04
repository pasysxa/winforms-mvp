using System;
using System.Drawing;
using System.Windows.Forms;

namespace MinformsMVP.Samples.ExecutionRequestDemo
{
    /// <summary>
    /// 遗留的旧窗体 - 不是MVP模式
    ///
    /// 这代表项目中的旧代码，无法或不便修改为MVP架构。
    /// 通过ExecutionRequest模式，可以从新的MVP窗体中打开它。
    /// </summary>
    public class LegacyCustomerForm : Form
    {
        private TextBox _customerNameTextBox;
        private TextBox _emailTextBox;
        private NumericUpDown _ageNumericUpDown;
        private Button _saveButton;
        private Button _cancelButton;

        // 公开属性供调用者获取结果
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }

        public LegacyCustomerForm()
        {
            InitializeComponent();
            SetupEventHandlers();
        }

        private void InitializeComponent()
        {
            this.Text = "客户信息编辑（遗留窗体 - 非MVP）";
            this.Size = new Size(450, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 标题
            var titleLabel = new Label
            {
                Text = "⚠️ 这是遗留的旧窗体（非MVP模式）",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.DarkOrange,
                Location = new Point(20, 20),
                Size = new Size(400, 30)
            };

            var infoLabel = new Label
            {
                Text = "这个窗体代表项目中的旧代码，通过ExecutionRequest模式集成",
                ForeColor = Color.Gray,
                Location = new Point(20, 50),
                Size = new Size(400, 20)
            };

            // 客户名称
            var nameLabel = new Label
            {
                Text = "客户名称:",
                Location = new Point(30, 90),
                Size = new Size(100, 20)
            };
            _customerNameTextBox = new TextBox
            {
                Location = new Point(140, 88),
                Size = new Size(270, 23)
            };

            // 邮箱
            var emailLabel = new Label
            {
                Text = "邮箱:",
                Location = new Point(30, 125),
                Size = new Size(100, 20)
            };
            _emailTextBox = new TextBox
            {
                Location = new Point(140, 123),
                Size = new Size(270, 23)
            };

            // 年龄
            var ageLabel = new Label
            {
                Text = "年龄:",
                Location = new Point(30, 160),
                Size = new Size(100, 20)
            };
            _ageNumericUpDown = new NumericUpDown
            {
                Location = new Point(140, 158),
                Size = new Size(100, 23),
                Minimum = 18,
                Maximum = 120,
                Value = 30
            };

            // 按钮
            _saveButton = new Button
            {
                Text = "保存",
                Location = new Point(240, 210),
                Size = new Size(80, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _saveButton.FlatAppearance.BorderSize = 0;

            _cancelButton = new Button
            {
                Text = "取消",
                Location = new Point(330, 210),
                Size = new Size(80, 35)
            };

            // 添加控件
            this.Controls.AddRange(new Control[] {
                titleLabel, infoLabel,
                nameLabel, _customerNameTextBox,
                emailLabel, _emailTextBox,
                ageLabel, _ageNumericUpDown,
                _saveButton, _cancelButton
            });
        }

        private void SetupEventHandlers()
        {
            _saveButton.Click += OnSaveClick;
            _cancelButton.Click += OnCancelClick;
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            // 简单验证
            if (string.IsNullOrWhiteSpace(_customerNameTextBox.Text))
            {
                MessageBox.Show("请输入客户名称", "验证失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_emailTextBox.Text))
            {
                MessageBox.Show("请输入邮箱", "验证失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 保存数据到公开属性
            CustomerName = _customerNameTextBox.Text;
            Email = _emailTextBox.Text;
            Age = (int)_ageNumericUpDown.Value;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
