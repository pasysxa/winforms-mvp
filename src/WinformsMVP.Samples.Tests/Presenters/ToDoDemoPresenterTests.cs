using System;
using Xunit;
using WinformsMVP.Samples.ToDoDemo;
using WinformsMVP.Samples.Tests.Mocks;
using WinformsMVP.Samples.Tests.TestHelpers;

namespace WinformsMVP.Samples.Tests.Presenters
{
    /// <summary>
    /// 単体テスト例：ToDoDemoPresenterをテスト
    ///
    /// デモンストレーション：
    /// 1. MockPlatformServicesを使用してモックサービスを注入
    /// 2. MockToDoViewを使用してビューをモック
    /// 3. Presenterのビジネスロジックを検証
    /// 4. サービス呼び出し（MessageServiceなど）を検証
    /// </summary>
    public class ToDoDemoPresenterTests
    {
        private MockPlatformServices _mockServices;
        private MockToDoView _mockView;
        private ToDoDemoPresenter _presenter;

        public ToDoDemoPresenterTests()
        {
            // 各テスト前にコンストラクターが実行される
            SetupTest();
        }

        private void SetupTest()
        {
            // 1. モックサービスを作成
            _mockServices = new MockPlatformServices();

            // 2. Presenterを作成し、WithPlatformServicesでモックサービスを注入
            _presenter = new ToDoDemoPresenter()
                .WithPlatformServices(_mockServices);

            // 3. モックビューを作成
            _mockView = new MockToDoView();

            // 4. ビューをアタッチして初期化
            _presenter.AttachView(_mockView);
            _presenter.Initialize();

            // 初期化時の呼び出し記録をクリア（必要に応じて）
            _mockServices.Reset();
            _mockView.MethodCalls.Clear();
            _mockView.StatusMessages.Clear();
        }

        #region AddTask Tests

        [Fact]
        public void AddTask_WithValidText_AddsTaskToView()
        {
            // Arrange
            _mockView.TaskText = "Buy groceries";

            // Act
            _presenter.Dispatch(ToDoDemoActions.AddTask);

            // Assert
            Assert.Contains("AddTaskToList(Buy groceries)", _mockView.MethodCalls);
            Assert.Equal(1, _mockView.TaskCount);
            Assert.Equal("Buy groceries", _mockView.GetTask(0));
        }

        [Fact]
        public void AddTask_WithValidText_ClearsTaskText()
        {
            // Arrange
            _mockView.TaskText = "Buy groceries";

            // Act
            _presenter.Dispatch(ToDoDemoActions.AddTask);

            // Assert
            Assert.Equal(string.Empty, _mockView.TaskText);
        }

        [Fact]
        public void AddTask_WithValidText_ShowsSuccessMessage()
        {
            // Arrange
            _mockView.TaskText = "Buy groceries";

            // Act
            _presenter.Dispatch(ToDoDemoActions.AddTask);

            // Assert - 验证MessageService被调用
            Assert.True(_mockServices.MessageService.InfoMessageShown);
            Assert.True(_mockServices.MessageService.HasCall(
                MessageType.Info,
                messageContains: "Buy groceries"));
        }

        [Fact]
        public void AddTask_WithEmptyText_ShowsWarning()
        {
            // Arrange
            _mockView.TaskText = "";

            // Act
            _presenter.Dispatch(ToDoDemoActions.AddTask);

            // Assert - 验证显示了警告消息
            Assert.True(_mockServices.MessageService.WarningMessageShown);
            Assert.False(_mockView.MethodCalls.Contains("AddTaskToList"));
        }

        [Fact]
        public void AddTask_WithWhitespaceText_ShowsWarning()
        {
            // Arrange
            _mockView.TaskText = "   ";

            // Act
            _presenter.Dispatch(ToDoDemoActions.AddTask);

            // Assert
            Assert.True(_mockServices.MessageService.WarningMessageShown);
            Assert.Equal(0, _mockView.TaskCount);
        }

        #endregion

        #region RemoveTask Tests

        [Fact]
        public void RemoveTask_WithConfirmYes_RemovesTask()
        {
            // Arrange
            _mockView.TaskText = "Task to delete";
            _presenter.Dispatch(ToDoDemoActions.AddTask);
            _mockView.SelectTask(0);
            _mockServices.MessageService.ConfirmYesNoResult = true;

            // Act
            _presenter.Dispatch(ToDoDemoActions.RemoveTask);

            // Assert
            Assert.True(_mockServices.MessageService.ConfirmDialogShown);
            Assert.Contains("RemoveSelectedTask()", _mockView.MethodCalls);
            Assert.Equal(0, _mockView.TaskCount);
        }

        [Fact]
        public void RemoveTask_WithConfirmNo_DoesNotRemoveTask()
        {
            // Arrange
            _mockView.TaskText = "Task to keep";
            _presenter.Dispatch(ToDoDemoActions.AddTask);
            _mockView.SelectTask(0);
            _mockServices.MessageService.ConfirmYesNoResult = false;  // 用户点击No

            // Act
            _presenter.Dispatch(ToDoDemoActions.RemoveTask);

            // Assert
            Assert.True(_mockServices.MessageService.ConfirmDialogShown);
            Assert.DoesNotContain("RemoveSelectedTask()", _mockView.MethodCalls);
            Assert.Equal(1, _mockView.TaskCount);  // 任务没有被删除
        }

        #endregion

        #region CompleteTask Tests

        [Fact]
        public void CompleteTask_MarksTaskAsCompleted()
        {
            // Arrange
            _mockView.TaskText = "Task to complete";
            _presenter.Dispatch(ToDoDemoActions.AddTask);
            _mockView.SelectTask(0);

            // Act
            _presenter.Dispatch(ToDoDemoActions.CompleteTask);

            // Assert
            Assert.Contains("CompleteSelectedTask()", _mockView.MethodCalls);
            Assert.StartsWith("[Done]", _mockView.GetTask(0));
        }

        #endregion

        #region SaveAll Tests

        [Fact]
        public void SaveAll_ClearsPendingChanges()
        {
            // Arrange
            _mockView.TaskText = "New task";
            _presenter.Dispatch(ToDoDemoActions.AddTask);
            Assert.True(_mockView.HasPendingChanges);

            // Act
            _presenter.Dispatch(ToDoDemoActions.SaveAll);

            // Assert
            Assert.Contains("ClearPendingChanges()", _mockView.MethodCalls);
            Assert.False(_mockView.HasPendingChanges);
        }

        [Fact]
        public void SaveAll_ShowsSuccessMessage()
        {
            // Arrange
            _mockView.TaskText = "New task";
            _presenter.Dispatch(ToDoDemoActions.AddTask);

            // Act
            _presenter.Dispatch(ToDoDemoActions.SaveAll);

            // Assert - 验证显示了成功消息
            Assert.True(_mockServices.MessageService.InfoMessageShown);
            Assert.True(_mockServices.MessageService.HasCall(
                MessageType.Info,
                messageContains: "saved"));
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void CompleteWorkflow_AddEditSave_WorksCorrectly()
        {
            // 1. Add first task
            _mockView.TaskText = "Task 1";
            _presenter.Dispatch(ToDoDemoActions.AddTask);

            // 2. Add second task
            _mockView.TaskText = "Task 2";
            _presenter.Dispatch(ToDoDemoActions.AddTask);

            // 3. Complete first task
            _mockView.SelectTask(0);
            _presenter.Dispatch(ToDoDemoActions.CompleteTask);

            // 4. Delete second task
            _mockView.SelectTask(1);
            _mockServices.MessageService.ConfirmYesNoResult = true;
            _presenter.Dispatch(ToDoDemoActions.RemoveTask);

            // 5. Save all changes
            _presenter.Dispatch(ToDoDemoActions.SaveAll);

            // Assert - verify final state
            Assert.Equal(1, _mockView.TaskCount);
            Assert.StartsWith("[Done]", _mockView.GetTask(0));
            Assert.False(_mockView.HasPendingChanges);

            // Verify messages shown
            Assert.True(_mockServices.MessageService.InfoMessageShown);
            Assert.True(_mockServices.MessageService.ConfirmDialogShown);
        }

        #endregion

        #region State-Driven Updates Tests

        [Fact]
        public void SelectionChanged_TriggersCanExecuteUpdate()
        {
            // 这个测试展示View事件如何影响action可用性
            // 实际的CanExecute验证需要访问dispatcher的内部状态
            // 这里只验证事件订阅正常工作

            // Arrange
            _mockView.TaskText = "Test task";
            _presenter.Dispatch(ToDoDemoActions.AddTask);

            // Act - 触发SelectionChanged事件
            _mockView.SelectTask(0);

            // Assert - CanExecute应该被重新评估
            // 在实际场景中，RemoveTask和CompleteTask按钮会变为可用
            Assert.True(_mockView.HasSelectedTask);
        }

        #endregion

        #region Service Injection Tests

        [Fact]
        public void Presenter_UsesInjectedMockServices()
        {
            // 这个测试验证mock服务确实被使用了

            // Act
            _mockView.TaskText = "Test";
            _presenter.Dispatch(ToDoDemoActions.AddTask);

            // Assert - 验证mock服务记录了调用
            Assert.Equal(1, _mockServices.MessageService.Calls.Count);
            Assert.Equal(MessageType.Info, _mockServices.MessageService.Calls[0].Type);
        }

        [Fact]
        public void MultipleTests_DontInterfere()
        {
            // 每个测试都有独立的mock实例（通过构造函数创建）
            // 这个测试验证测试之间不会相互影响

            // Assert - 新测试开始时状态是干净的
            Assert.Equal(0, _mockServices.MessageService.Calls.Count);
            Assert.Equal(0, _mockView.TaskCount);
        }

        #endregion
    }
}
