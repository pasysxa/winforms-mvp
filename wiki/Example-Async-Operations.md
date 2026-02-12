# Async Operations Example

This example demonstrates proper async/await patterns in the WinForms MVP Framework, including progress tracking, cancellation support, and error handling for long-running operations.

## 📁 Location

`src/WinformsMVP.Samples/AsyncDemo/`

## 🎯 What You'll Learn

- Implementing async/await in Presenters without freezing the UI
- Progress tracking with `IProgress<T>` and progress bars
- Cancellation support using `CancellationToken`
- Error handling in async methods
- UI state management during async operations
- Thread-safe UI updates from async code
- Multiple async operation patterns

## 🏗️ Architecture

### Files Structure

```
AsyncDemo/
├── IAsyncDemoView.cs         # View interface
├── AsyncDemoPresenter.cs     # Async business logic
└── AsyncDemoForm.cs          # WinForms UI with progress display
```

### Async Patterns Demonstrated

| Pattern | Description | Use Case |
|---------|-------------|----------|
| **Simple Async** | Basic async operation | Quick operations (< 5 seconds) |
| **Long Operation with Cancellation** | Multi-step process, user can cancel | Batch processing, data import |
| **Progress Tracking** | Reports progress percentage | File uploads, long calculations |
| **Error Handling** | Demonstrates error in async context | Network failures, validation errors |

## 📝 Implementation Walkthrough

### 1. View Interface - IAsyncDemoView

The View interface manages UI state during async operations:

```csharp
public interface IAsyncDemoView : IWindowView
{
    string StatusMessage { get; set; }
    int ProgressPercentage { get; set; }    // 0-100
    bool ShowProgress { get; set; }         // Show/hide progress bar
    IEnumerable<string> ResultData { get; set; }
    string ElapsedTime { get; set; }
    bool IsOperationRunning { get; set; }   // Used in CanExecute
}
```

**Key Design Points:**
- ✅ `IsOperationRunning` flag for button state management
- ✅ Progress percentage for visual feedback
- ✅ Status message for user communication
- ✅ Result data for displaying operation outcomes

### 2. Presenter - AsyncDemoPresenter

The Presenter implements async operations with proper patterns:

```csharp
public class AsyncDemoPresenter : WindowPresenterBase<IAsyncDemoView>
{
    private CancellationTokenSource _cancellationTokenSource;

    protected override void RegisterViewActions()
    {
        _dispatcher.Register(
            AsyncActions.StartSimpleOperation,
            OnStartSimpleOperation,
            canExecute: () => !View.IsOperationRunning);  // Disable when running

        _dispatcher.Register(
            AsyncActions.StartLongOperation,
            OnStartLongOperation,
            canExecute: () => !View.IsOperationRunning);

        _dispatcher.Register(
            AsyncActions.StartWithProgress,
            OnStartWithProgress,
            canExecute: () => !View.IsOperationRunning);

        _dispatcher.Register(
            AsyncActions.Cancel,
            OnCancel,
            canExecute: () => View.IsOperationRunning);  // Enable only when running

        View.ActionBinder.Bind(_dispatcher);
    }
}
```

### Pattern 1: Simple Async Operation

```csharp
private async void OnStartSimpleOperation()
{
    View.StatusMessage = "Starting simple async operation...";
    View.IsOperationRunning = true;
    _dispatcher.RaiseCanExecuteChanged();  // Update button states

    var stopwatch = Stopwatch.StartNew();

    try
    {
        // Simulate async work
        await Task.Delay(2000);

        stopwatch.Stop();
        View.StatusMessage = "Simple operation completed successfully!";
        View.ElapsedTime = $"Elapsed: {stopwatch.ElapsedMilliseconds}ms";

        Messages.ShowInfo("Simple async operation completed!", "Success");
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        View.StatusMessage = $"Error: {ex.Message}";
        Messages.ShowError($"Operation failed: {ex.Message}", "Error");
    }
    finally
    {
        View.IsOperationRunning = false;
        _dispatcher.RaiseCanExecuteChanged();  // Re-enable buttons
    }
}
```

**Key Points:**
- ✅ `async void` is acceptable for event handlers
- ✅ `try-catch-finally` ensures cleanup happens
- ✅ `finally` block restores UI state
- ✅ `RaiseCanExecuteChanged()` updates button states

### Pattern 2: Long Operation with Cancellation

```csharp
private async void OnStartLongOperation()
{
    View.StatusMessage = "Starting long-running operation...";
    View.IsOperationRunning = true;
    _dispatcher.RaiseCanExecuteChanged();

    _cancellationTokenSource = new CancellationTokenSource();
    var stopwatch = Stopwatch.StartNew();

    try
    {
        var results = await PerformLongOperationAsync(_cancellationTokenSource.Token);

        stopwatch.Stop();
        View.ResultData = results;
        View.StatusMessage = $"Long operation completed! Processed {results.Count} items.";
        View.ElapsedTime = $"Elapsed: {stopwatch.Elapsed.TotalSeconds:F2}s";

        Messages.ShowInfo($"Processed {results.Count} items successfully!", "Success");
    }
    catch (OperationCanceledException)
    {
        stopwatch.Stop();
        View.StatusMessage = "Operation was cancelled by user.";
        View.ElapsedTime = $"Elapsed: {stopwatch.Elapsed.TotalSeconds:F2}s";
        Messages.ShowWarning("Operation was cancelled.", "Cancelled");
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        View.StatusMessage = $"Error: {ex.Message}";
        Messages.ShowError($"Operation failed: {ex.Message}", "Error");
    }
    finally
    {
        View.IsOperationRunning = false;
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _dispatcher.RaiseCanExecuteChanged();
    }
}

private async Task<List<string>> PerformLongOperationAsync(CancellationToken cancellationToken)
{
    var results = new List<string>();
    const int itemCount = 20;

    for (int i = 1; i <= itemCount; i++)
    {
        // Check for cancellation
        cancellationToken.ThrowIfCancellationRequested();

        // Simulate work
        await Task.Delay(300, cancellationToken);

        results.Add($"Item {i} processed at {DateTime.Now:HH:mm:ss}");
    }

    return results;
}

private void OnCancel()
{
    if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
    {
        View.StatusMessage = "Cancelling operation...";
        _cancellationTokenSource.Cancel();
    }
}
```

**Key Points:**
- ✅ `CancellationTokenSource` manages cancellation
- ✅ `ThrowIfCancellationRequested()` checks for cancellation
- ✅ `OperationCanceledException` is caught separately
- ✅ Cleanup in `finally` block disposes token source

### Pattern 3: Progress Tracking

```csharp
private async void OnStartWithProgress()
{
    View.StatusMessage = "Starting operation with progress tracking...";
    View.ShowProgress = true;
    View.ProgressPercentage = 0;
    View.IsOperationRunning = true;
    _dispatcher.RaiseCanExecuteChanged();

    _cancellationTokenSource = new CancellationTokenSource();
    var stopwatch = Stopwatch.StartNew();

    try
    {
        // Create progress reporter
        var progress = new Progress<int>(percent =>
        {
            View.ProgressPercentage = percent;
            View.StatusMessage = $"Processing... {percent}% complete";
        });

        var results = await PerformOperationWithProgressAsync(progress, _cancellationTokenSource.Token);

        stopwatch.Stop();
        View.ResultData = results;
        View.StatusMessage = $"Operation completed! Processed {results.Count} items.";
        View.ElapsedTime = $"Elapsed: {stopwatch.Elapsed.TotalSeconds:F2}s";
        View.ProgressPercentage = 100;

        Messages.ShowInfo($"Processed {results.Count} items successfully!", "Success");
    }
    catch (OperationCanceledException)
    {
        stopwatch.Stop();
        View.StatusMessage = "Operation was cancelled by user.";
        Messages.ShowWarning("Operation was cancelled.", "Cancelled");
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        View.StatusMessage = $"Error: {ex.Message}";
        Messages.ShowError($"Operation failed: {ex.Message}", "Error");
    }
    finally
    {
        View.ShowProgress = false;
        View.IsOperationRunning = false;
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _dispatcher.RaiseCanExecuteChanged();
    }
}

private async Task<List<string>> PerformOperationWithProgressAsync(
    IProgress<int> progress,
    CancellationToken cancellationToken)
{
    var results = new List<string>();
    const int itemCount = 50;

    for (int i = 1; i <= itemCount; i++)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Simulate work
        await Task.Delay(100, cancellationToken);

        results.Add($"Item {i} processed at {DateTime.Now:HH:mm:ss.fff}");

        // Report progress (automatically marshals to UI thread)
        var percentComplete = (i * 100) / itemCount;
        progress?.Report(percentComplete);
    }

    return results;
}
```

**Key Points:**
- ✅ `Progress<int>` automatically marshals to UI thread
- ✅ No need for `Invoke()` when using `IProgress<T>`
- ✅ Progress bar shown/hidden appropriately
- ✅ Percentage calculated and reported during operation

### Pattern 4: Error Handling

```csharp
private async void OnStartWithError()
{
    View.StatusMessage = "Starting operation that will fail...";
    View.IsOperationRunning = true;
    _dispatcher.RaiseCanExecuteChanged();

    var stopwatch = Stopwatch.StartNew();

    try
    {
        // Simulate operation that fails
        await Task.Delay(1000);
        throw new InvalidOperationException("This is a simulated error!");
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        View.StatusMessage = $"Error occurred: {ex.Message}";
        View.ElapsedTime = $"Elapsed: {stopwatch.ElapsedMilliseconds}ms";

        Messages.ShowError(
            $"Operation failed with error:\n\n{ex.Message}\n\n" +
            "This demonstrates proper async error handling.",
            "Error Demonstration");
    }
    finally
    {
        View.IsOperationRunning = false;
        _dispatcher.RaiseCanExecuteChanged();
    }
}
```

### 3. Form Implementation - Thread-Safe UI Updates

The Form ensures thread-safe UI updates:

```csharp
public partial class AsyncDemoForm : Form, IAsyncDemoView
{
    public string StatusMessage
    {
        get => _statusLabel.Text;
        set
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => _statusLabel.Text = value));
            }
            else
            {
                _statusLabel.Text = value;
            }
        }
    }

    public int ProgressPercentage
    {
        get => _progressBar.Value;
        set
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => _progressBar.Value = Math.Min(100, Math.Max(0, value))));
            }
            else
            {
                _progressBar.Value = Math.Min(100, Math.Max(0, value));
            }
        }
    }

    public IEnumerable<string> ResultData
    {
        get => _resultsListBox.Items.Cast<string>();
        set
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    _resultsListBox.Items.Clear();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            _resultsListBox.Items.Add(item);
                        }
                    }
                }));
            }
            else
            {
                _resultsListBox.Items.Clear();
                if (value != null)
                {
                    foreach (var item in value)
                    {
                        _resultsListBox.Items.Add(item);
                    }
                }
            }
        }
    }
}
```

**Key Points:**
- ✅ `InvokeRequired` checks if marshaling needed
- ✅ `Invoke()` marshals to UI thread
- ✅ All property setters are thread-safe
- ⚠️ **Note:** When using `IProgress<T>`, marshaling is automatic!

## 🎮 User Experience Flow

1. **User clicks "Long Operation"**
   - Button becomes disabled immediately
   - Cancel button becomes enabled
   - Status message shows "Starting..."
   - Operation begins processing 20 items

2. **During operation**
   - ListBox populates with results as they complete
   - User can click Cancel at any time
   - UI remains responsive (doesn't freeze)

3. **User clicks Cancel**
   - Operation checks `CancellationToken` on next iteration
   - Throws `OperationCanceledException`
   - Caught and handled gracefully
   - Partial results are shown

4. **Operation completes or is cancelled**
   - Buttons return to normal state
   - Final status message shown
   - Elapsed time displayed

## 🧪 Testing Async Code

```csharp
[Fact]
public async Task OnStartSimpleOperation_ShouldCompleteSuccessfully()
{
    // Arrange
    var mockView = new MockAsyncDemoView();
    var mockMessages = new MockMessageService();
    var presenter = new AsyncDemoPresenter();
    presenter.AttachView(mockView);
    presenter.Initialize();

    // Act
    presenter.OnStartSimpleOperation();  // async void, can't await directly

    // Wait for operation to complete
    await Task.Delay(2500);  // Slightly longer than operation

    // Assert
    Assert.False(mockView.IsOperationRunning);
    Assert.Contains("completed", mockView.StatusMessage);
    Assert.True(mockMessages.InfoMessageShown);
}

[Fact]
public async Task OnCancel_ShouldCancelRunningOperation()
{
    // Arrange
    var mockView = new MockAsyncDemoView();
    var presenter = new AsyncDemoPresenter();
    presenter.AttachView(mockView);
    presenter.Initialize();

    // Act - Start long operation
    presenter.OnStartLongOperation();
    await Task.Delay(500);  // Let it start

    // Cancel
    presenter.OnCancel();
    await Task.Delay(1000);  // Wait for cancellation

    // Assert
    Assert.False(mockView.IsOperationRunning);
    Assert.Contains("cancelled", mockView.StatusMessage.ToLower());
}
```

**Testing Tips:**
- Use `async Task` for test methods (not `async void`)
- Wait for async operations to complete with `Task.Delay()`
- Or use `TaskCompletionSource<T>` for precise control

## 💡 Advanced Patterns

### Database Operations

```csharp
private async void OnLoadData()
{
    View.StatusMessage = "Loading data from database...";
    View.IsOperationRunning = true;
    _dispatcher.RaiseCanExecuteChanged();

    try
    {
        var data = await _repository.GetAllAsync();
        View.Data = data;
        View.StatusMessage = $"Loaded {data.Count} records";
    }
    catch (DbException ex)
    {
        View.StatusMessage = "Database error occurred";
        Messages.ShowError($"Database error: {ex.Message}", "Error");
    }
    finally
    {
        View.IsOperationRunning = false;
        _dispatcher.RaiseCanExecuteChanged();
    }
}
```

### HTTP API Calls

```csharp
private async void OnFetchFromApi()
{
    View.StatusMessage = "Fetching data from API...";
    View.IsOperationRunning = true;
    _dispatcher.RaiseCanExecuteChanged();

    try
    {
        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(30);

            var response = await client.GetAsync("https://api.example.com/data");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<DataModel>>(json);

            View.Data = data;
            View.StatusMessage = $"Fetched {data.Count} items from API";
        }
    }
    catch (HttpRequestException ex)
    {
        Messages.ShowError($"Network error: {ex.Message}", "Error");
    }
    catch (TaskCanceledException)
    {
        Messages.ShowWarning("Request timed out", "Timeout");
    }
    finally
    {
        View.IsOperationRunning = false;
        _dispatcher.RaiseCanExecuteChanged();
    }
}
```

### Fire-and-Forget (Use with Caution)

```csharp
// ⚠️ Not recommended for most scenarios
private void OnStartBackgroundTask()
{
    // Fire-and-forget - no await
    _ = Task.Run(async () =>
    {
        try
        {
            await DoBackgroundWorkAsync();
        }
        catch (Exception ex)
        {
            // Must handle exceptions - no one is listening!
            LogError(ex);
        }
    });

    Messages.ShowInfo("Background task started", "Info");
}
```

**Caution:** Only use fire-and-forget when:
- ✅ Failure doesn't affect user workflow
- ✅ You don't need to update UI with results
- ✅ Exceptions are logged/handled internally

## 🔗 Related Examples

- [Master-Detail Demo](Example-Master-Detail) - Async data loading
- [Validation Demo](Example-Validation) - Async server-side validation
- [ToDo CRUD Demo](Example-ToDo) - Async CRUD operations

## 📚 Further Reading

- [Best Practices: Error Handling](Best-Practices-Error-Handling)
- [Testing Presenters](Best-Practices-Testing) - Testing async code
- [ViewAction System](ViewAction-System) - CanExecute with async state

---

**[⬆ Back to Examples](Home#example-applications)**
