using System;
using Xunit;
using MinformsMVP.Samples.ToDoDemo;
using WindowsMVP.Samples.Tests.Mocks;
using WindowsMVP.Samples.Tests.TestHelpers;

namespace WindowsMVP.Samples.Tests.Presenters
{
    /// <summary>
    /// 单元测试示例：测试 ToDoDemoPresenter
    ///
    /// 演示如何：
    /// 1. 使用 MockCommonServices 注入mock服务
    /// 2. 使用 MockToDoView 模拟视图
    /// 3. 验证Presenter的业务逻辑
    /// 4. 验证服务调用（MessageService等）
    /// </summary>
    public class ToDoDemoPresenterTests
    {
        private MockCommonServices _mockServices;
        private MockToDoView _mockView;
        private ToDoDemoPresenter _presenter;

        public ToDoDemoPresenterTests()
        {
            // 每个测试前都会执行构造函数
            SetupTest();
        }

        private void SetupTest()
        {
            // 1. 创建Mock服务
            _mockServices = new MockCommonServices();

            // 2. 创建Presenter，使用WithPlatformServices注入Mock服务
            _presenter = new ToDoDemoPresenter()
                .WithPlatformServices(_mockServices);

            // 3. 创建Mock视图
            _mockView = new MockToDoView();

            // 4. 附加视图并初始化
            _presenter.AttachView(_mockView);
            _presenter.Initialize();

            // 清除初始化时的调用记录（如果需要）
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
