using System;
using System.Drawing;
using System.Windows.Forms;
using MinformsMVP.Samples.ToDoDemo;
using MinformsMVP.Samples.CheckBoxDemo;
using MinformsMVP.Samples.BulkBindingDemo;
using MinformsMVP.Samples.NavigatorDemo;
using MinformsMVP.Samples.MVPComparisonDemo;
using MinformsMVP.Samples.ExecutionRequestDemo;
using MinformsMVP.Samples.MessageBoxDemo;
using WinformsMVP.Services;
using WinformsMVP.Services.Implementations;

namespace MinformsMVP.Samples
{
    /// <summary>
    /// Sample launcher for WinForms MVP demos.
    /// </summary>
    public class SampleLauncherForm : Form
    {
        public SampleLauncherForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form settings
            this.Text = "WinForms MVP - Sample Launcher";
            this.Size = new Size(500, 810);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9f);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title
            var titleLabel = new Label
            {
                Text = "WinForms MVP Framework - Samples",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                Location = new Point(30, 30),
                Size = new Size(440, 30),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var subtitleLabel = new Label
            {
                Text = "Select a demo to explore ViewAction features",
                Location = new Point(30, 65),
                Size = new Size(440, 20),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // ToDo Demo Button
            var todoButton = new Button
            {
                Text = "ToDo List Demo",
                Location = new Point(100, 120),
                Size = new Size(300, 50),
                Font = new Font("Segoe UI", 11f),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            todoButton.FlatAppearance.BorderSize = 0;
            todoButton.Click += (s, e) => LaunchToDoDemo();

            var todoInfoLabel = new Label
            {
                Text = "Action-driven updates • CanExecute • State management",
                Location = new Point(100, 175),
                Size = new Size(300, 20),
                ForeColor = Color.DarkGray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // CheckBox/RadioButton Demo Button
            var checkboxButton = new Button
            {
                Text = "CheckBox/RadioButton Demo",
                Location = new Point(100, 210),
                Size = new Size(300, 50),
                Font = new Font("Segoe UI", 11f),
                BackColor = Color.FromArgb(16, 137, 62),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            checkboxButton.FlatAppearance.BorderSize = 0;
            checkboxButton.Click += (s, e) => LaunchCheckBoxDemo();

            var checkboxInfoLabel = new Label
            {
                Text = "CheckedChanged events • Settings UI • MVP pattern",
                Location = new Point(100, 265),
                Size = new Size(300, 20),
                ForeColor = Color.DarkGray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Bulk Binding Demo Button
            var bulkBindingButton = new Button
            {
                Text = "Bulk Binding Demo (Survey)",
                Location = new Point(100, 300),
                Size = new Size(300, 50),
                Font = new Font("Segoe UI", 11f),
                BackColor = Color.FromArgb(139, 69, 19),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            bulkBindingButton.FlatAppearance.BorderSize = 0;
            bulkBindingButton.Click += (s, e) => LaunchBulkBindingDemo();

            var bulkBindingInfoLabel = new Label
            {
                Text = "AddRange • AddByTag • Many RadioButtons",
                Location = new Point(100, 355),
                Size = new Size(300, 20),
                ForeColor = Color.DarkGray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // WindowNavigator Demo Button
            var navigatorButton = new Button
            {
                Text = "WindowNavigator Demo",
                Location = new Point(100, 390),
                Size = new Size(300, 50),
                Font = new Font("Segoe UI", 11f),
                BackColor = Color.FromArgb(75, 0, 130),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            navigatorButton.FlatAppearance.BorderSize = 0;
            navigatorButton.Click += (s, e) => LaunchNavigatorDemo();

            var navigatorInfoLabel = new Label
            {
                Text = "Modal • Non-Modal • Parameters • Results",
                Location = new Point(100, 445),
                Size = new Size(300, 20),
                ForeColor = Color.DarkGray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // MVP Comparison Demo Button
            var mvpComparisonButton = new Button
            {
                Text = "MVP Pattern Comparison",
                Location = new Point(100, 480),
                Size = new Size(300, 50),
                Font = new Font("Segoe UI", 11f),
                BackColor = Color.FromArgb(220, 20, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            mvpComparisonButton.FlatAppearance.BorderSize = 0;
            mvpComparisonButton.Click += (s, e) => LaunchMVPComparisonDemo();

            var mvpComparisonInfoLabel = new Label
            {
                Text = "Passive View vs Supervising Controller",
                Location = new Point(100, 535),
                Size = new Size(300, 20),
                ForeColor = Color.DarkGray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // ExecutionRequest Demo Button
            var executionRequestButton = new Button
            {
                Text = "ExecutionRequest Pattern",
                Location = new Point(100, 570),
                Size = new Size(300, 50),
                Font = new Font("Segoe UI", 11f),
                BackColor = Color.FromArgb(128, 0, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            executionRequestButton.FlatAppearance.BorderSize = 0;
            executionRequestButton.Click += (s, e) => LaunchExecutionRequestDemo();

            var executionRequestInfoLabel = new Label
            {
                Text = "Legacy Integration • Delayed Execution",
                Location = new Point(100, 625),
                Size = new Size(300, 20),
                ForeColor = Color.DarkGray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // MessageBox Positioning Demo Button
            var messageBoxButton = new Button
            {
                Text = "MessageBox Positioning Demo",
                Location = new Point(100, 660),
                Size = new Size(300, 50),
                Font = new Font("Segoe UI", 11f),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            messageBoxButton.FlatAppearance.BorderSize = 0;
            messageBoxButton.Click += (s, e) => LaunchMessageBoxDemo();

            var messageBoxInfoLabel = new Label
            {
                Text = "Native MessageBox • Windows API Hook • Positioning",
                Location = new Point(100, 715),
                Size = new Size(300, 20),
                ForeColor = Color.DarkGray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Add controls
            this.Controls.AddRange(new Control[] {
                titleLabel, subtitleLabel,
                todoButton, todoInfoLabel,
                checkboxButton, checkboxInfoLabel,
                bulkBindingButton, bulkBindingInfoLabel,
                navigatorButton, navigatorInfoLabel,
                mvpComparisonButton, mvpComparisonInfoLabel,
                executionRequestButton, executionRequestInfoLabel,
                messageBoxButton, messageBoxInfoLabel
            });
        }

        private void LaunchToDoDemo()
        {
            var view = new ToDoDemoForm();
            var presenter = new ToDoDemoPresenter();  // Uses CommonServices.Default

            presenter.AttachView(view);
            presenter.Initialize();  // Must call Initialize!
            view.Show();

            this.Hide();
            view.FormClosed += (s, e) => this.Show();
        }

        private void LaunchCheckBoxDemo()
        {
            var view = new SettingsDemoForm();
            var presenter = new SettingsDemoPresenter();

            presenter.AttachView(view);
            presenter.Initialize();  // Must call Initialize!
            view.Show();

            this.Hide();
            view.FormClosed += (s, e) => this.Show();
        }

        private void LaunchBulkBindingDemo()
        {
            var view = new SurveyDemoForm();
            var presenter = new SurveyDemoPresenter();

            presenter.AttachView(view);
            presenter.Initialize();  // Must call Initialize!
            view.Show();

            this.Hide();
            view.FormClosed += (s, e) => this.Show();
        }

        private void LaunchNavigatorDemo()
        {
            var messageService = new MessageService();

            // Create and configure ViewMappingRegister
            var viewMappingRegister = new ViewMappingRegister();

            // Option 1: Manual registration (commented out - old way)
            // viewMappingRegister.Register<INavigatorDemoView, NavigatorDemoForm>();
            // viewMappingRegister.Register<ISimpleDialogView, SimpleDialogForm>();
            // viewMappingRegister.Register<IInputDialogView, InputDialogForm>();
            // viewMappingRegister.Register<IConfirmDialogView, ConfirmDialogForm>();
            // viewMappingRegister.Register<INonModalWindowView, NonModalWindowForm>();
            // viewMappingRegister.Register<ICallbackWindowView, CallbackWindowForm>();
            // viewMappingRegister.Register<ISingletonWindowView, SingletonWindowForm>();

            // Option 2: Automatic assembly scanning (NEW - recommended!)
            int registered = viewMappingRegister.RegisterFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            System.Diagnostics.Debug.WriteLine($"ViewMappingRegister: Auto-registered {registered} Views from assembly");

            var navigator = new WindowNavigator(viewMappingRegister);
            var view = new NavigatorDemoForm();
            var presenter = new NavigatorDemoPresenter(navigator);

            presenter.AttachView(view);
            presenter.Initialize();
            view.Show();

            this.Hide();
            view.FormClosed += (s, e) => this.Show();
        }

        private void LaunchMVPComparisonDemo()
        {
            var launcher = new MVPComparisonLauncher();
            launcher.Show();

            this.Hide();
            launcher.FormClosed += (s, e) => this.Show();
        }

        private void LaunchExecutionRequestDemo()
        {
            var view = new ExecutionRequestDemoForm();
            var presenter = new ExecutionRequestDemoPresenter();

            presenter.AttachView(view);
            presenter.Initialize();
            view.Show();

            this.Hide();
            view.FormClosed += (s, e) => this.Show();
        }

        private void LaunchMessageBoxDemo()
        {
            var view = new MessageBoxDemoForm();
            var presenter = new MessageBoxDemoPresenter();

            presenter.AttachView(view);
            presenter.Initialize();
            view.Show();

            this.Hide();
            view.FormClosed += (s, e) => this.Show();
        }
    }
}
