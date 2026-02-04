using System;

namespace WinformsMVP.MVP.ViewActions
{
    /// <summary>
    /// 标准动作名称库 - 提供常见动作的命名标准
    ///
    /// 用途：
    /// 1. 确保应用中动作命名的一致性
    /// 2. 配合 ViewActionFactory 创建有前缀的动作
    /// 3. 作为参考，开发者可选择性使用
    ///
    /// 注意：
    /// - 这些是命名建议，不是强制要求
    /// - 建议配合模块前缀使用（通过 ViewActionFactory）
    /// - 业务特定的动作可以直接定义
    ///
    /// 使用示例：
    /// <code>
    /// public static class MyModuleActions
    /// {
    ///     private static readonly ViewActionFactory Factory =
    ///         ViewAction.Factory.WithQualifier("MyModule");
    ///
    ///     // 使用标准名称
    ///     public static readonly ViewAction Save = Factory.Create(StandardActionNames.Crud.Save);
    ///
    ///     // 业务特定的动作
    ///     public static readonly ViewAction ProcessOrder = Factory.Create("ProcessOrder");
    /// }
    /// </code>
    /// </summary>
    public static class StandardActionNames
    {
        /// <summary>
        /// CRUD（增删改查）操作
        /// </summary>
        public static class Crud
        {
            /// <summary>添加/新建</summary>
            public const string Add = "Add";

            /// <summary>编辑/修改</summary>
            public const string Edit = "Edit";

            /// <summary>删除/移除</summary>
            public const string Delete = "Delete";

            /// <summary>保存</summary>
            public const string Save = "Save";

            /// <summary>取消</summary>
            public const string Cancel = "Cancel";

            /// <summary>刷新</summary>
            public const string Refresh = "Refresh";

            /// <summary>重置</summary>
            public const string Reset = "Reset";

            /// <summary>移除（与 Delete 类似，但语义更轻）</summary>
            public const string Remove = "Remove";

            /// <summary>创建</summary>
            public const string Create = "Create";

            /// <summary>更新</summary>
            public const string Update = "Update";
        }

        /// <summary>
        /// 对话框操作
        /// </summary>
        public static class Dialog
        {
            /// <summary>确定</summary>
            public const string Ok = "Ok";

            /// <summary>取消</summary>
            public const string Cancel = "Cancel";

            /// <summary>是</summary>
            public const string Yes = "Yes";

            /// <summary>否</summary>
            public const string No = "No";

            /// <summary>应用</summary>
            public const string Apply = "Apply";

            /// <summary>关闭</summary>
            public const string Close = "Close";

            /// <summary>重试</summary>
            public const string Retry = "Retry";

            /// <summary>忽略</summary>
            public const string Ignore = "Ignore";

            /// <summary>中止</summary>
            public const string Abort = "Abort";
        }

        /// <summary>
        /// 导航操作
        /// </summary>
        public static class Navigation
        {
            /// <summary>下一个</summary>
            public const string Next = "Next";

            /// <summary>上一个</summary>
            public const string Previous = "Previous";

            /// <summary>第一个</summary>
            public const string First = "First";

            /// <summary>最后一个</summary>
            public const string Last = "Last";

            /// <summary>后退</summary>
            public const string GoBack = "GoBack";

            /// <summary>前进</summary>
            public const string GoForward = "GoForward";

            /// <summary>转到</summary>
            public const string GoTo = "GoTo";

            /// <summary>打开</summary>
            public const string Open = "Open";
        }

        /// <summary>
        /// 数据操作
        /// </summary>
        public static class Data
        {
            /// <summary>加载</summary>
            public const string Load = "Load";

            /// <summary>重新加载</summary>
            public const string Reload = "Reload";

            /// <summary>导入</summary>
            public const string Import = "Import";

            /// <summary>导出</summary>
            public const string Export = "Export";

            /// <summary>筛选</summary>
            public const string Filter = "Filter";

            /// <summary>排序</summary>
            public const string Sort = "Sort";

            /// <summary>搜索</summary>
            public const string Search = "Search";

            /// <summary>查找</summary>
            public const string Find = "Find";

            /// <summary>查看</summary>
            public const string View = "View";

            /// <summary>清除</summary>
            public const string Clear = "Clear";
        }

        /// <summary>
        /// 文件操作
        /// </summary>
        public static class File
        {
            /// <summary>新建</summary>
            public const string New = "New";

            /// <summary>打开</summary>
            public const string Open = "Open";

            /// <summary>保存</summary>
            public const string Save = "Save";

            /// <summary>另存为</summary>
            public const string SaveAs = "SaveAs";

            /// <summary>关闭</summary>
            public const string Close = "Close";

            /// <summary>打印</summary>
            public const string Print = "Print";

            /// <summary>打印预览</summary>
            public const string PrintPreview = "PrintPreview";

            /// <summary>页面设置</summary>
            public const string PageSetup = "PageSetup";

            /// <summary>打印设置</summary>
            public const string PrintSetup = "PrintSetup";
        }

        /// <summary>
        /// 编辑操作
        /// </summary>
        public static class Edit
        {
            /// <summary>撤销</summary>
            public const string Undo = "Undo";

            /// <summary>重做</summary>
            public const string Redo = "Redo";

            /// <summary>剪切</summary>
            public const string Cut = "Cut";

            /// <summary>复制</summary>
            public const string Copy = "Copy";

            /// <summary>粘贴</summary>
            public const string Paste = "Paste";

            /// <summary>删除</summary>
            public const string Delete = "Delete";

            /// <summary>全选</summary>
            public const string SelectAll = "SelectAll";
        }

        /// <summary>
        /// 视图/显示操作
        /// </summary>
        public static class View
        {
            /// <summary>显示</summary>
            public const string Show = "Show";

            /// <summary>隐藏</summary>
            public const string Hide = "Hide";

            /// <summary>切换</summary>
            public const string Toggle = "Toggle";

            /// <summary>展开</summary>
            public const string Expand = "Expand";

            /// <summary>折叠</summary>
            public const string Collapse = "Collapse";

            /// <summary>放大</summary>
            public const string ZoomIn = "ZoomIn";

            /// <summary>缩小</summary>
            public const string ZoomOut = "ZoomOut";

            /// <summary>全屏</summary>
            public const string FullScreen = "FullScreen";
        }

        /// <summary>
        /// 其他常用操作
        /// </summary>
        public static class Common
        {
            /// <summary>提交</summary>
            public const string Submit = "Submit";

            /// <summary>确认</summary>
            public const string Confirm = "Confirm";

            /// <summary>开始</summary>
            public const string Start = "Start";

            /// <summary>停止</summary>
            public const string Stop = "Stop";

            /// <summary>暂停</summary>
            public const string Pause = "Pause";

            /// <summary>继续</summary>
            public const string Resume = "Resume";

            /// <summary>帮助</summary>
            public const string Help = "Help";

            /// <summary>设置</summary>
            public const string Settings = "Settings";

            /// <summary>关于</summary>
            public const string About = "About";
        }
    }
}
