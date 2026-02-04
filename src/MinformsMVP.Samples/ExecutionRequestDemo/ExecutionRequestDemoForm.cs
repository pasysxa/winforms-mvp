using System;
using System.Drawing;
using System.Windows.Forms;
using WinformsMVP.Common;
using WinformsMVP.Common.Events;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.ViewActions;

namespace MinformsMVP.Samples.ExecutionRequestDemo
{
    /// <summary>
    /// ExecutionRequest模式演示窗体
    /// </summary>
    public partial class ExecutionRequestDemoForm : Form, IExecutionRequestDemoView
    {
        // UI Controls
        private Button _openLegacyButton;
        private Button _selectFileButton;
        private Button _saveDataButton;
        private Label _customerInfoLabel;
        private Label _filePathLabel;
        private Label _statusLabel;
        private Panel _statusPanel;

        private ViewActionBinder _viewActionBinder;

        // ExecutionRequest事件（符合三条铁律：只使用业务数据类型）
        public event EventHandler<ExecutionRequestEventArgs<CustomerData, CustomerData>>
            EditCustomerRequested;

        public event EventHandler<ExecutionRequestEventArgs<CustomerData, bool>>
            SaveDataRequested;

        public ExecutionRequestDemoForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "ExecutionRequest模式演示";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9f);

            // 标题
            var titleLabel = new Label
            {
                Text = "ExecutionRequest 模式演示",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                Location = new Point(30, 20),
                Size = new Size(640, 35),
                ForeColor = Color.DarkBlue
            };

            var descLabel = new Label
            {
                Text = "演示如何使用ExecutionRequest模式集成遗留代码和处理特殊逻辑",
                Location = new Point(30, 60),
                Size = new Size(640, 25),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10f)
            };

            // 场景1: 编辑客户信息（ExecutionRequest）
            var scenario1Label = new Label
            {
                Text = "场景1: 编辑客户信息（ExecutionRequest 模式）",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location = new Point(30, 110),
                Size = new Size(640, 25),
                ForeColor = Color.DarkGreen
            };

            var scenario1DescLabel = new Label
            {
                Text = "✅ 正确用法：参数和返回值都是业务数据（CustomerData），无 UI 类型",
                Location = new Point(30, 135),
                Size = new Size(640, 20),
                ForeColor = Color.DarkGreen
            };

            _openLegacyButton = new Button
            {
                Text = "编辑客户信息",
                Location = new Point(30, 165),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f)
            };
            _openLegacyButton.FlatAppearance.BorderSize = 0;

            _customerInfoLabel = new Label
            {
                Text = "客户信息：（未设置）",
                Location = new Point(250, 170),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.Gray
            };

            // 场景2: 文件选择对话框
            var scenario2Label = new Label
            {
                Text = "场景2: 文件选择对话框（推荐：服务接口）",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location = new Point(30, 230),
                Size = new Size(640, 25),
                ForeColor = Color.DarkGreen
            };

            var scenario2DescLabel = new Label
            {
                Text = "✅ 推荐做法：使用 IDialogProvider 服务接口（更简单、更易测试）",
                Location = new Point(30, 255),
                Size = new Size(640, 20),
                ForeColor = Color.DarkGreen
            };

            _selectFileButton = new Button
            {
                Text = "选择文件",
                Location = new Point(30, 285),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(16, 137, 62),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f)
            };
            _selectFileButton.FlatAppearance.BorderSize = 0;

            _filePathLabel = new Label
            {
                Text = "选中的文件：（未选择）",
                Location = new Point(250, 290),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.Gray,
                AutoEllipsis = true
            };

            // 场景3: 保存数据
            var scenario3Label = new Label
            {
                Text = "场景3: 保存业务数据（ExecutionRequest 模式）",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location = new Point(30, 350),
                Size = new Size(640, 25),
                ForeColor = Color.DarkGreen
            };

            var scenario3DescLabel = new Label
            {
                Text = "✅ 正确用法：参数是业务数据（CustomerData），返回值是业务结果（bool）",
                Location = new Point(30, 375),
                Size = new Size(640, 20),
                ForeColor = Color.DarkGreen
            };

            _saveDataButton = new Button
            {
                Text = "保存客户数据",
                Location = new Point(30, 405),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(139, 69, 19),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f),
                Enabled = false  // 初始禁用，有数据后启用
            };
            _saveDataButton.FlatAppearance.BorderSize = 0;

            var saveHintLabel = new Label
            {
                Text = "💡 提示：先打开遗留窗体添加客户数据，然后才能保存",
                Location = new Point(250, 410),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Orange
            };

            // 状态栏
            _statusPanel = new Panel
            {
                Location = new Point(0, 480),
                Size = new Size(700, 70),
                BackColor = Color.FromArgb(240, 240, 240),
                Dock = DockStyle.Bottom
            };

            var statusTitleLabel = new Label
            {
                Text = "状态:",
                Location = new Point(20, 15),
                Size = new Size(50, 20),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            _statusLabel = new Label
            {
                Text = "准备就绪",
                Location = new Point(70, 15),
                Size = new Size(600, 40),
                ForeColor = Color.FromArgb(64, 64, 64),
                Font = new Font("Segoe UI", 9f)
            };

            _statusPanel.Controls.AddRange(new Control[] {
                statusTitleLabel, _statusLabel
            });

            // 添加所有控件
            this.Controls.AddRange(new Control[] {
                titleLabel, descLabel,
                scenario1Label, scenario1DescLabel, _openLegacyButton, _customerInfoLabel,
                scenario2Label, scenario2DescLabel, _selectFileButton, _filePathLabel,
                scenario3Label, scenario3DescLabel, _saveDataButton, saveHintLabel,
                _statusPanel
            });
        }

        #region IExecutionRequestDemoView Implementation

        public void ShowCustomerInfo(CustomerData data)
        {
            _customerInfoLabel.Text = $"客户信息：{data}";
            _customerInfoLabel.ForeColor = Color.DarkGreen;
        }

        public void ShowSelectedFile(string filePath)
        {
            _filePathLabel.Text = $"选中的文件：{filePath}";
            _filePathLabel.ForeColor = Color.DarkGreen;
        }

        public void UpdateStatus(string message, bool isSuccess)
        {
            _statusLabel.Text = message;
            _statusLabel.ForeColor = isSuccess
                ? Color.Green
                : Color.Red;
        }

        public void BindActions(ViewActionDispatcher dispatcher)
        {
            _viewActionBinder = new ViewActionBinder();
            _viewActionBinder.Add(ExecutionRequestDemoActions.EditCustomer, _openLegacyButton);
            _viewActionBinder.Add(ExecutionRequestDemoActions.SelectFile, _selectFileButton);  // 直接触发 ViewAction，Presenter 用 IDialogProvider
            _viewActionBinder.Add(ExecutionRequestDemoActions.SaveData, _saveDataButton);
            _viewActionBinder.Bind(dispatcher);

            // 场景1和场景3使用 ExecutionRequest
            // 场景2直接通过 ViewAction + IDialogProvider
            _openLegacyButton.Click += OnEditCustomerButtonClick;
            _saveDataButton.Click += OnSaveDataButtonClick;
        }

        #endregion

        #region Event Handlers - 触发ExecutionRequest

        /// <summary>
        /// 场景1：编辑客户信息
        /// ✅ 符合铁律：只传递业务数据（CustomerData），不传递 UI 类型
        /// </summary>
        private void OnEditCustomerButtonClick(object sender, EventArgs e)
        {
            // 获取当前客户数据（null 表示新建）
            var currentCustomer = GetCurrentCustomerData();

            // 创建 ExecutionRequest - View 只传递业务数据和回调
            var request = new ExecutionRequestEventArgs<CustomerData, CustomerData>(
                param: currentCustomer,         // ✅ 业务数据
                callback: OnCustomerEdited      // ✅ 回调函数
            );

            // 触发事件
            EditCustomerRequested?.Invoke(this, request);
        }

        // ✅ 场景2：文件选择 - 不使用 ExecutionRequest
        // _selectFileButton 直接绑定到 ViewAction，Presenter 使用 IDialogProvider

        /// <summary>
        /// 场景3：保存数据
        /// ✅ 符合铁律：参数和返回值都是业务数据类型
        /// </summary>
        private void OnSaveDataButtonClick(object sender, EventArgs e)
        {
            // 获取当前客户数据
            var customerData = GetCurrentCustomerData();
            if (customerData == null)
            {
                UpdateStatus("没有可保存的数据", false);
                return;
            }

            // 创建 ExecutionRequest
            var request = new ExecutionRequestEventArgs<CustomerData, bool>(
                param: customerData,    // ✅ 业务数据
                callback: OnDataSaved   // ✅ 回调函数
            );

            SaveDataRequested?.Invoke(this, request);
        }

        #endregion

        #region Callbacks - 处理ExecutionRequest的结果

        /// <summary>
        /// 客户编辑完成的回调
        /// </summary>
        private void OnCustomerEdited(CustomerData editedCustomer)
        {
            if (editedCustomer != null)
            {
                // 用户确认了编辑
                ShowCustomerInfo(editedCustomer);
                UpdateStatus($"客户信息已更新：{editedCustomer.Name}", true);
            }
            else
            {
                // 用户取消了编辑
                UpdateStatus("用户取消了操作", false);
            }
        }

        /// <summary>
        /// 数据保存完成的回调
        /// </summary>
        private void OnDataSaved(bool success)
        {
            if (success)
            {
                UpdateStatus("数据保存成功！", true);
            }
            else
            {
                UpdateStatus("数据保存失败", false);
            }
        }

        #endregion

        #region Helper Methods

        private CustomerData GetCurrentCustomerData()
        {
            // 这里应该从实际的数据源获取
            // 为了演示，我们从customerInfoLabel解析
            var text = _customerInfoLabel.Text;
            if (text.Contains("未设置"))
                return null;

            // 简化处理：实际应用中应该保存在字段中
            return new CustomerData
            {
                Name = "示例客户",
                Email = "example@test.com",
                Age = 30
            };
        }

        #endregion

        #region IWindowView Implementation

        bool IWindowView.IsDisposed => base.IsDisposed;

        void IWindowView.Activate()
        {
            this.Activate();
        }

        #endregion
    }
}
