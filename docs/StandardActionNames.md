# StandardActionNames - 标准动作名称库

## 📖 概述

`StandardActionNames` 提供了一套常见动作的命名标准，帮助确保整个应用中动作命名的一致性。

## 🎯 设计理念

### 核心原则

1. **命名标准，非强制约束** - 提供建议的命名规范，开发者可选择性使用
2. **配合前缀使用** - 建议与 `ViewActionFactory` 配合，创建有模块前缀的动作
3. **灵活性优先** - 业务特定的动作可以直接定义，不必强行套用标准名称

### 为什么是字符串常量而不是 ViewAction？

ViewAction 是 struct，创建后名称固定，无法添加前缀：

```csharp
// ❌ 如果提供 ViewAction（行不通）
public static readonly ViewAction Save = ViewAction.Create("Save");  // 名称固定为 "Save"
// 无法再添加前缀变成 "MyModule.Save"

// ✅ 使用字符串常量（灵活）
public const string Save = "Save";
// 可以配合 Factory 添加前缀：Factory.Create(Save) → "MyModule.Save"
```

---

## 📂 分类结构

### 1. **Crud** - CRUD 操作

常见的增删改查操作：

| 名称 | 值 | 说明 |
|------|-----|------|
| `Add` | "Add" | 添加/新建 |
| `Edit` | "Edit" | 编辑/修改 |
| `Delete` | "Delete" | 删除/移除 |
| `Save` | "Save" | 保存 |
| `Cancel` | "Cancel" | 取消 |
| `Refresh` | "Refresh" | 刷新 |
| `Reset` | "Reset" | 重置 |
| `Remove` | "Remove" | 移除（与 Delete 类似，但语义更轻） |
| `Create` | "Create" | 创建 |
| `Update` | "Update" | 更新 |

### 2. **Dialog** - 对话框操作

对话框中常见的按钮动作：

| 名称 | 值 | 说明 |
|------|-----|------|
| `Ok` | "Ok" | 确定 |
| `Cancel` | "Cancel" | 取消 |
| `Yes` | "Yes" | 是 |
| `No` | "No" | 否 |
| `Apply` | "Apply" | 应用 |
| `Close` | "Close" | 关闭 |
| `Retry` | "Retry" | 重试 |
| `Ignore` | "Ignore" | 忽略 |
| `Abort` | "Abort" | 中止 |

### 3. **Navigation** - 导航操作

页面或记录间的导航：

| 名称 | 值 | 说明 |
|------|-----|------|
| `Next` | "Next" | 下一个 |
| `Previous` | "Previous" | 上一个 |
| `First` | "First" | 第一个 |
| `Last` | "Last" | 最后一个 |
| `GoBack` | "GoBack" | 后退 |
| `GoForward` | "GoForward" | 前进 |
| `GoTo` | "GoTo" | 转到 |
| `Open` | "Open" | 打开 |

### 4. **Data** - 数据操作

数据加载、搜索、筛选等操作：

| 名称 | 值 | 说明 |
|------|-----|------|
| `Load` | "Load" | 加载 |
| `Reload` | "Reload" | 重新加载 |
| `Import` | "Import" | 导入 |
| `Export` | "Export" | 导出 |
| `Filter` | "Filter" | 筛选 |
| `Sort` | "Sort" | 排序 |
| `Search` | "Search" | 搜索 |
| `Find` | "Find" | 查找 |
| `View` | "View" | 查看 |
| `Clear` | "Clear" | 清除 |

### 5. **File** - 文件操作

文件相关的操作：

| 名称 | 值 | 说明 |
|------|-----|------|
| `New` | "New" | 新建 |
| `Open` | "Open" | 打开 |
| `Save` | "Save" | 保存 |
| `SaveAs` | "SaveAs" | 另存为 |
| `Close` | "Close" | 关闭 |
| `Print` | "Print" | 打印 |
| `PrintPreview` | "PrintPreview" | 打印预览 |
| `PageSetup` | "PageSetup" | 页面设置 |
| `PrintSetup` | "PrintSetup" | 打印设置 |

### 6. **Edit** - 编辑操作

文本或内容编辑相关：

| 名称 | 值 | 说明 |
|------|-----|------|
| `Undo` | "Undo" | 撤销 |
| `Redo` | "Redo" | 重做 |
| `Cut` | "Cut" | 剪切 |
| `Copy` | "Copy" | 复制 |
| `Paste` | "Paste" | 粘贴 |
| `Delete` | "Delete" | 删除 |
| `SelectAll` | "SelectAll" | 全选 |

### 7. **View** - 视图/显示操作

界面显示相关的操作：

| 名称 | 值 | 说明 |
|------|-----|------|
| `Show` | "Show" | 显示 |
| `Hide` | "Hide" | 隐藏 |
| `Toggle` | "Toggle" | 切换 |
| `Expand` | "Expand" | 展开 |
| `Collapse` | "Collapse" | 折叠 |
| `ZoomIn` | "ZoomIn" | 放大 |
| `ZoomOut` | "ZoomOut" | 缩小 |
| `FullScreen` | "FullScreen" | 全屏 |

### 8. **Common** - 其他常用操作

其他常见操作：

| 名称 | 值 | 说明 |
|------|-----|------|
| `Submit` | "Submit" | 提交 |
| `Confirm` | "Confirm" | 确认 |
| `Start` | "Start" | 开始 |
| `Stop` | "Stop" | 停止 |
| `Pause` | "Pause" | 暂停 |
| `Resume` | "Resume" | 继续 |
| `Help` | "Help" | 帮助 |
| `Settings` | "Settings" | 设置 |
| `About` | "About" | 关于 |

---

## 💡 使用示例

### 基本用法

```csharp
public static class MyModuleActions
{
    private static readonly ViewActionFactory Factory =
        ViewAction.Factory.WithQualifier("MyModule");

    // ✅ 使用标准名称（推荐用于通用操作）
    public static readonly ViewAction Save = Factory.Create(StandardActionNames.Crud.Save);      // "MyModule.Save"
    public static readonly ViewAction Cancel = Factory.Create(StandardActionNames.Crud.Cancel);  // "MyModule.Cancel"
    public static readonly ViewAction Delete = Factory.Create(StandardActionNames.Crud.Delete);  // "MyModule.Delete"

    // ✅ 业务特定的动作（直接定义）
    public static readonly ViewAction ProcessOrder = Factory.Create("ProcessOrder");
    public static readonly ViewAction SendEmail = Factory.Create("SendEmail");
}
```

### 对话框示例

```csharp
public static class ConfirmDialogActions
{
    private static readonly ViewActionFactory Factory =
        ViewAction.Factory.WithQualifier("ConfirmDialog");

    // 使用标准对话框动作名称
    public static readonly ViewAction Yes = Factory.Create(StandardActionNames.Dialog.Yes);     // "ConfirmDialog.Yes"
    public static readonly ViewAction No = Factory.Create(StandardActionNames.Dialog.No);       // "ConfirmDialog.No"
    public static readonly ViewAction Cancel = Factory.Create(StandardActionNames.Dialog.Cancel); // "ConfirmDialog.Cancel"
}
```

### 混合使用示例

```csharp
public static class DataGridActions
{
    private static readonly ViewActionFactory Factory =
        ViewAction.Factory.WithQualifier("DataGrid");

    // 标准 CRUD 操作
    public static readonly ViewAction Add = Factory.Create(StandardActionNames.Crud.Add);
    public static readonly ViewAction Edit = Factory.Create(StandardActionNames.Crud.Edit);
    public static readonly ViewAction Delete = Factory.Create(StandardActionNames.Crud.Delete);

    // 标准数据操作
    public static readonly ViewAction Export = Factory.Create(StandardActionNames.Data.Export);
    public static readonly ViewAction Filter = Factory.Create(StandardActionNames.Data.Filter);

    // 业务特定操作
    public static readonly ViewAction CalculateTotal = Factory.Create("CalculateTotal");
    public static readonly ViewAction MergeRows = Factory.Create("MergeRows");
}
```

---

## ✅ 最佳实践

### 1. 优先使用标准名称（通用操作）

```csharp
// ✅ 好 - 使用标准名称确保一致性
public static readonly ViewAction Save = Factory.Create(StandardActionNames.Crud.Save);
public static readonly ViewAction Cancel = Factory.Create(StandardActionNames.Dialog.Cancel);

// ❌ 避免 - 同样的概念使用不同的名称
public static readonly ViewAction Save = Factory.Create("SaveData");      // 不一致
public static readonly ViewAction Cancel = Factory.Create("CancelAction"); // 不一致
```

### 2. 业务特定动作直接定义

```csharp
// ✅ 好 - 业务特定的动作直接使用描述性名称
public static readonly ViewAction CompleteTask = Factory.Create("CompleteTask");
public static readonly ViewAction MarkAsImportant = Factory.Create("MarkAsImportant");

// ❌ 不要强行套用标准名称
public static readonly ViewAction CompleteTask = Factory.Create(StandardActionNames.Common.Submit); // 语义不清晰
```

### 3. 添加注释说明动作用途

```csharp
public static class OrderActions
{
    private static readonly ViewActionFactory Factory =
        ViewAction.Factory.WithQualifier("Order");

    // 标准操作
    public static readonly ViewAction Save = Factory.Create(StandardActionNames.Crud.Save);

    // 业务特定操作
    public static readonly ViewAction ProcessOrder = Factory.Create("ProcessOrder");    // 处理订单
    public static readonly ViewAction CancelOrder = Factory.Create("CancelOrder");      // 取消订单
    public static readonly ViewAction ShipOrder = Factory.Create("ShipOrder");          // 发货
}
```

---

## 🔧 扩展标准名称

如果项目中有常用但标准库未提供的动作名称，可以创建自己的扩展：

```csharp
/// <summary>
/// 项目特定的标准动作名称
/// </summary>
public static class ProjectActionNames
{
    /// <summary>审批相关操作</summary>
    public static class Approval
    {
        public const string Approve = "Approve";
        public const string Reject = "Reject";
        public const string Withdraw = "Withdraw";
        public const string Forward = "Forward";
    }

    /// <summary>报表相关操作</summary>
    public static class Report
    {
        public const string Generate = "Generate";
        public const string Schedule = "Schedule";
        public const string Email = "Email";
    }
}
```

---

## 📊 总结

### 优势

1. **一致性** - 确保整个应用使用统一的动作命名
2. **可读性** - 标准名称易于理解和维护
3. **灵活性** - 可选择性使用，不强制
4. **零成本** - 编译时内联，无运行时开销

### 何时使用

- ✅ **通用操作** - Save, Cancel, Delete, Ok, Yes, No 等
- ✅ **对话框按钮** - 使用 `Dialog` 类中的标准名称
- ✅ **CRUD 操作** - 使用 `Crud` 类中的标准名称

### 何时不使用

- ❌ **业务特定逻辑** - ProcessOrder, SendEmail, CalculateTotal 等
- ❌ **领域特定术语** - Approve, Reject（审批）、Ship（发货）等
- ❌ **复合操作** - SaveAndClose, DeleteAll 等

---

**记住：StandardActionNames 是命名建议，不是强制约束。使用它来提高代码一致性，但不要强行套用！**
