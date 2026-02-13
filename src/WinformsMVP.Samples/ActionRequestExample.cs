using System;
using WinformsMVP.Common.Events;
using WinformsMVP.Core.Views;
using WinformsMVP.MVP.Presenters;
using WinformsMVP.MVP.ViewActions;

namespace WinformsMVP.Samples
{
    /// <summary>
    /// 示例：使用 ActionRequestEventArgs 解决复杂画面中的事件爆炸问题
    ///
    /// 问题场景：
    /// 在复杂的表单中，可能有几十个按钮/操作（保存、取消、删除、导出、打印、刷新等）
    /// 如果为每个操作都定义一个单独的事件，View 接口会变得非常臃肿。
    ///
    /// 解决方案：
    /// 使用统一的 ActionRequest 事件，通过 ViewAction 区分不同的操作。
    /// </summary>
    public static class ComplexDataGridActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("ComplexDataGrid");

        // 定义所有操作的 ActionKey
        public static readonly ViewAction Add = Factory.Create("Add");
        public static readonly ViewAction Edit = Factory.Create("Edit");
        public static readonly ViewAction Delete = Factory.Create("Delete");
        public static readonly ViewAction Refresh = Factory.Create("Refresh");
        public static readonly ViewAction Export = Factory.Create("Export");
        public static readonly ViewAction Import = Factory.Create("Import");
        public static readonly ViewAction Print = Factory.Create("Print");
        public static readonly ViewAction Filter = Factory.Create("Filter");
        public static readonly ViewAction Sort = Factory.Create("Sort");
        public static readonly ViewAction Search = Factory.Create("Search");
    }

    #region 传统方式（事件爆炸） vs ActionRequest 方式对比

    /// <summary>
    /// ❌ 传统方式 - 事件爆炸问题
    /// 每个操作都需要定义一个单独的事件，导致接口臃肿
    /// </summary>
    public interface IDataGridView_Traditional : IWindowView
    {
        // 😱 需要定义大量事件
        event EventHandler AddRequested;
        event EventHandler EditRequested;
        event EventHandler DeleteRequested;
        event EventHandler RefreshRequested;
        event EventHandler ExportRequested;
        event EventHandler ImportRequested;
        event EventHandler PrintRequested;
        event EventHandler FilterRequested;
        event EventHandler SortRequested;
        event EventHandler SearchRequested;
        // ... 可能还有更多操作
    }

    /// <summary>
    /// ✅ ActionRequest 方式 - 简洁优雅
    /// 只需要一个统一的事件，通过 ActionKey 区分操作
    /// </summary>
    public interface IDataGridView : IWindowView
    {
        // ✅ 只需要一个事件！
        event EventHandler<ActionRequestEventArgs> ActionRequested;

        void UpdateStatus(string message);
    }

    #endregion

    #region Presenter 实现 - 使用 ActionRequest

    /// <summary>
    /// Presenter 使用 ActionRequestEventArgs 处理所有操作
    /// </summary>
    public class DataGridPresenter : WindowPresenterBase<IDataGridView>
    {
        protected override void OnViewAttached()
        {
            // ✅ 只需要订阅一个事件
            View.ActionRequested += OnViewActionTriggered;  // 使用基类提供的辅助方法
        }

        protected override void RegisterViewActions()
        {
            // 注册所有操作的处理器
            _dispatcher.Register(ComplexDataGridActions.Add, OnAdd);
            _dispatcher.Register(ComplexDataGridActions.Edit, OnEdit);
            _dispatcher.Register(ComplexDataGridActions.Delete, OnDelete, canExecute: () => HasSelection());
            _dispatcher.Register(ComplexDataGridActions.Refresh, OnRefresh);
            _dispatcher.Register(ComplexDataGridActions.Export, OnExport);
            _dispatcher.Register(ComplexDataGridActions.Import, OnImport);
            _dispatcher.Register(ComplexDataGridActions.Print, OnPrint);
            _dispatcher.Register(ComplexDataGridActions.Filter, OnFilter);
            _dispatcher.Register(ComplexDataGridActions.Sort, OnSort);
            _dispatcher.Register(ComplexDataGridActions.Search, OnSearch);

            // Note: View.ActionBinder.Bind(_dispatcher) is now called automatically by the base class
        }

        protected override void OnInitialize()
        {
            View.UpdateStatus("准备就绪");
        }

        #region Action Handlers

        private void OnAdd()
        {
            View.UpdateStatus("添加新记录...");
            // 实现添加逻辑
        }

        private void OnEdit()
        {
            View.UpdateStatus("编辑记录...");
            // 实现编辑逻辑
        }

        private void OnDelete()
        {
            View.UpdateStatus("删除记录...");
            // 实现删除逻辑
        }

        private void OnRefresh()
        {
            View.UpdateStatus("刷新数据...");
            // 实现刷新逻辑
        }

        private void OnExport()
        {
            View.UpdateStatus("导出数据...");
            // 实现导出逻辑
        }

        private void OnImport()
        {
            View.UpdateStatus("导入数据...");
            // 实现导入逻辑
        }

        private void OnPrint()
        {
            View.UpdateStatus("打印...");
            // 实现打印逻辑
        }

        private void OnFilter()
        {
            View.UpdateStatus("筛选数据...");
            // 实现筛选逻辑
        }

        private void OnSort()
        {
            View.UpdateStatus("排序数据...");
            // 实现排序逻辑
        }

        private void OnSearch()
        {
            View.UpdateStatus("搜索数据...");
            // 实现搜索逻辑
        }

        private bool HasSelection()
        {
            // 检查是否有选中项
            return true;  // 示例
        }

        #endregion

        protected override void Cleanup()
        {
            if (View != null)
            {
                View.ActionRequested -= OnViewActionTriggered;
            }
        }
    }

    #endregion

    #region 带参数的 ActionRequest 示例

    /// <summary>
    /// 示例：带参数的 ActionRequest
    /// 用于需要传递数据的操作（如搜索关键字、筛选条件等）
    /// </summary>
    public static class SearchActions
    {
        private static readonly ViewActionFactory Factory =
            ViewAction.Factory.WithQualifier("Search");

        public static readonly ViewAction SearchByKeyword = Factory.Create("SearchByKeyword");
        public static readonly ViewAction FilterByCategory = Factory.Create("FilterByCategory");
    }

    /// <summary>
    /// View 接口 - 支持带参数的 ActionRequest
    /// </summary>
    public interface ISearchableDataGridView : IWindowView
    {
        // 无参数的操作
        event EventHandler<ActionRequestEventArgs> ActionRequested;

        // 带参数的操作（如搜索关键字）
        event EventHandler<ActionRequestEventArgs<string>> SearchActionRequested;

        void UpdateStatus(string message);
    }

    /// <summary>
    /// Presenter - 处理带参数的 ActionRequest
    /// </summary>
    public class SearchableDataGridPresenter : WindowPresenterBase<ISearchableDataGridView>
    {
        protected override void OnViewAttached()
        {
            View.ActionRequested += OnViewActionTriggered;
            View.SearchActionRequested += OnSearchActionTriggered;  // 带参数的事件
        }

        protected override void RegisterViewActions()
        {
            // 注册带参数的操作
            _dispatcher.Register<string>(
                SearchActions.SearchByKeyword,
                OnSearchByKeyword);

            _dispatcher.Register<string>(
                SearchActions.FilterByCategory,
                OnFilterByCategory);

            // Note: View.ActionBinder.Bind(_dispatcher) is now called automatically by the base class
        }

        protected override void OnInitialize()
        {
            View.UpdateStatus("准备就绪");
        }

        // 处理带参数的 SearchAction 事件
        private void OnSearchActionTriggered(object sender, ActionRequestEventArgs<string> e)
        {
            DispatchAction(e);  // 使用基类的 DispatchAction 方法
        }

        private void OnSearchByKeyword(string keyword)
        {
            View.UpdateStatus($"搜索关键字: {keyword}");
            // 实现搜索逻辑
        }

        private void OnFilterByCategory(string category)
        {
            View.UpdateStatus($"筛选分类: {category}");
            // 实现筛选逻辑
        }

        protected override void Cleanup()
        {
            if (View != null)
            {
                View.ActionRequested -= OnViewActionTriggered;
                View.SearchActionRequested -= OnSearchActionTriggered;
            }
        }
    }

    #endregion

    #region 总结和对比

    /*
     * ActionRequestEventArgs 的优势：
     *
     * 1. ✅ 解决事件爆炸问题
     *    - 传统方式：10 个操作 = 10 个事件
     *    - ActionRequest：10 个操作 = 1 个事件
     *
     * 2. ✅ View 接口更简洁
     *    - 不需要为每个操作定义单独的事件
     *    - 接口更容易维护和扩展
     *
     * 3. ✅ 统一的事件处理模式
     *    - 所有操作都通过 ActionRequestEventArgs 传递
     *    - Presenter 使用统一的 DispatchAction 方法处理
     *
     * 4. ✅ 支持带参数的操作
     *    - ActionRequestEventArgs<T> 支持传递参数
     *    - 类型安全，编译时检查
     *
     * 使用建议：
     *
     * - 简单画面（< 5 个操作）：可以使用传统的独立事件
     * - 复杂画面（> 5 个操作）：推荐使用 ActionRequestEventArgs
     * - 需要传递参数的操作：使用 ActionRequestEventArgs<T>
     */

    #endregion
}
