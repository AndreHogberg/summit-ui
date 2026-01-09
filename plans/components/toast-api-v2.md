# Toast API - React Aria-Inspired Design

## Design Goals

1. **Simple API** - Minimal properties, easy for common use cases
2. **Standard Navigation** - F6/Shift+F6 for landmark navigation (ARIA standard)
3. **Template Support** - Full control over toast rendering via `RenderFragment`
4. **Accessible** - Proper ARIA roles, live regions, focus management
5. **Use C# Queue** - Leverage `System.Collections.Generic.Queue<T>` for FIFO ordering

## Proposed API

### 1. Simplified Queue Interface

```csharp
/// <summary>
/// A queue for managing toast notifications. Generic over content type.
/// </summary>
/// <typeparam name="TContent">User-defined content type (can be anything).</typeparam>
public interface IToastQueue<TContent>
{
    /// <summary>
    /// Add a toast to the queue.
    /// </summary>
    /// <param name="content">User-defined content.</param>
    /// <param name="options">Optional settings (timeout, priority, onClose).</param>
    /// <returns>A key that can be used to close the toast programmatically.</returns>
    string Add(TContent content, ToastOptions? options = null);

    /// <summary>
    /// Close a specific toast by key.
    /// </summary>
    void Close(string key);

    /// <summary>
    /// Close all toasts.
    /// </summary>
    void CloseAll();

    /// <summary>
    /// Pause all auto-dismiss timers (e.g., on hover/focus).
    /// </summary>
    void PauseAll();

    /// <summary>
    /// Resume all auto-dismiss timers.
    /// </summary>
    void ResumeAll();

    /// <summary>
    /// Currently visible toasts.
    /// </summary>
    IReadOnlyList<QueuedToast<TContent>> VisibleToasts { get; }

    /// <summary>
    /// Subscribe to queue changes. Returns unsubscribe action.
    /// </summary>
    Action Subscribe(Action callback);
}
```

### 2. User-Defined Content

Users define their own content type - we don't impose any structure:

```csharp
// Simple string
ToastQueue<string> queue = new();
queue.Add("File saved!");

// Custom record
record MyToast(string Title, string? Description = null, string Variant = "default");
ToastQueue<MyToast> queue = new();
queue.Add(new("Success", "File saved!", "success"));

// Complex object with action
record ActionToast(string Message, Action? OnUndo = null);
ToastQueue<ActionToast> queue = new();
queue.Add(new("Email deleted", OnUndo: () => RestoreEmail()));
```

### 3. Toast Options

```csharp
/// <summary>
/// Options for toast behavior (not content).
/// </summary>
public record ToastOptions
{
    /// <summary>
    /// Auto-dismiss timeout in milliseconds. Minimum 5000 for accessibility.
    /// Null means no auto-dismiss (user must close manually).
    /// </summary>
    public int? Timeout { get; init; }

    /// <summary>
    /// Priority for screen reader announcements.
    /// </summary>
    public ToastPriority Priority { get; init; } = ToastPriority.Polite;

    /// <summary>
    /// Callback when the toast is closed (by user or timeout).
    /// </summary>
    public Action? OnClose { get; init; }
}

public enum ToastPriority
{
    /// <summary>Announced at next graceful opportunity (aria-live="polite").</summary>
    Polite,
    /// <summary>Announced immediately, interrupting current speech (aria-live="assertive").</summary>
    Assertive
}
```

### 4. Queued Toast State

```csharp
/// <summary>
/// Represents a toast in the queue with user-defined content.
/// </summary>
public record QueuedToast<TContent>
{
    /// <summary>Unique key for this toast.</summary>
    public required string Key { get; init; }
    
    /// <summary>User-defined content.</summary>
    public required TContent Content { get; init; }
    
    /// <summary>Auto-dismiss timeout in ms, or null for manual close.</summary>
    public int? Timeout { get; init; }
    
    /// <summary>Screen reader announcement priority.</summary>
    public ToastPriority Priority { get; init; }
    
    /// <summary>When the toast was added.</summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>Internal timer for auto-dismiss.</summary>
    internal Timer? Timer { get; init; }
}
```

## Usage Examples

### 1. Define Your Content Type & Register Queue

```csharp
// Define once in your app
public record MyToast(string Title, string? Description = null);

// Register in Program.cs
builder.Services.AddSingleton<IToastQueue<MyToast>>(new ToastQueue<MyToast>());
```

### 2. Add ToastRegion to Layout

```razor
@* In MainLayout.razor or App.razor *@
<ToastRegion TContent="MyToast" AriaLabel="Notifications">
    <ToastTemplate Context="toast">
        <Toast Toast="toast" class="bg-slate-800 text-white p-4 rounded-lg">
            <span>@toast.Content.Title</span>
            <ToastCloseButton>×</ToastCloseButton>
        </Toast>
    </ToastTemplate>
</ToastRegion>

@Body
```

### 3. Show Toasts from Anywhere

```razor
@inject IToastQueue<MyToast> Toasts

<button @onclick="ShowToast">Save File</button>

@code {
    void ShowToast() => Toasts.Add(
        new("File saved!", "Your changes have been saved successfully.")
    );
}
```

### With Timeout

```csharp
@inject IToastQueue<MyToast> Toasts

Toasts.Add(
    new("Upload complete", "3 files uploaded successfully."),
    new() { Timeout = 5000 }
);
```

### Programmatic Dismissal

```csharp
@inject IToastQueue<MyToast> Toasts

string? _processingKey;

void StartProcess()
{
    _processingKey = Toasts.Add(
        new("Processing..."),
        new() { OnClose = () => _processingKey = null }
    );
}

void CancelProcess()
{
    if (_processingKey is not null)
    {
        Queue.Close(_processingKey);
    }
}
```

### Assertive (Important) Toast

```csharp
@inject IToastQueue<MyToast> Toasts

Toasts.Add(
    new("Error", "Failed to save changes. Please try again."),
    new() { Priority = ToastPriority.Assertive, Timeout = 8000 }
);
```

### With Custom Actions

```csharp
// Content type with actions
record UndoableToast(string Message, Action OnUndo);

var queue = new ToastQueue<UndoableToast>();
queue.Add(new("Email deleted", () => RestoreEmail()));

// In template:
<ToastTemplate Context="toast">
    <Toast Toast="toast">
        <span>@toast.Content.Message</span>
        <button @onclick="toast.Content.OnUndo">Undo</button>
        <ToastCloseButton>×</ToastCloseButton>
    </Toast>
</ToastTemplate>
```

## Component API

### ToastRegion (Template-based)

```razor
@* ToastRegion injects IToastQueue<TContent> via DI *@
@* No need to pass queue as parameter *@
<ToastRegion 
    TContent="MyToast"
    AriaLabel="Notifikationer"
    class="fixed bottom-4 right-4 flex flex-col-reverse gap-2">
    <ToastTemplate Context="toast">
        <Toast Toast="toast" class="flex items-center gap-4 bg-blue-600 p-4 rounded-lg">
            @* User's own markup - complete control *@
            <div class="flex flex-col flex-1">
                <span class="font-semibold">@toast.Content.Title</span>
                @if (toast.Content.Description is not null)
                {
                    <span class="text-sm opacity-90">@toast.Content.Description</span>
                }
            </div>
            <ToastCloseButton aria-label="Stäng" class="p-2 rounded hover:bg-white/10">
                ×
            </ToastCloseButton>
        </Toast>
    </ToastTemplate>
</ToastRegion>
```

### DI Registration

```csharp
// In Program.cs - register your queue as singleton
builder.Services.AddSingleton<IToastQueue<MyToast>>(new ToastQueue<MyToast>());

// Or use the extension method
builder.Services.AddToastQueue<MyToast>();
```

### ToastRegion Component (Internal)

```csharp
public partial class ToastRegion<TContent> : ComponentBase, IDisposable
{
    [Inject] private IToastQueue<TContent> Queue { get; set; } = default!;
    
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public RenderFragment<QueuedToast<TContent>>? ToastTemplate { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] 
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
    
    private Action? _unsubscribe;
    
    protected override void OnInitialized()
    {
        _unsubscribe = Queue.Subscribe(StateHasChanged);
    }
    
    public void Dispose() => _unsubscribe?.Invoke();
}
```

### Anatomy

```
ToastRegion (landmark region, role="region", handles focus)
└── ToastTemplate (RenderFragment<QueuedToast> - user's template)
    └── Toast (individual toast, role="alertdialog")
        ├── [User's own content markup]
        └── ToastCloseButton (closes toast on click)
```

We provide:
- `ToastRegion` - Container with ARIA landmark
- `Toast` - Wrapper with alertdialog role, timeout handling
- `ToastCloseButton` - Calls `Queue.Close(key)` on click

User provides:
- All content markup (title, description, icons, etc.)
- All styling

## Keyboard Navigation

Following ARIA Landmark Region patterns:

| Key | Action |
|-----|--------|
| F6 | Move focus to next landmark region (including toast region) |
| Shift + F6 | Move focus to previous landmark region |
| Tab | Navigate between focusable elements within toast |
| Escape | Close the focused toast |

## Accessibility Improvements

1. **Pre-existing Live Region** - The live region must exist in DOM before content is added
2. **Proper Priorities** - `polite` for non-critical, `assertive` for important
3. **Focus Management** - When last toast closes, return focus to previous location
4. **Minimum Timeout** - Enforce 5000ms minimum for accessibility
5. **Landmark Region** - Toast region is a landmark, navigable via F6

## Queue Implementation Details

### Why Queue<T>?

C#'s `Queue<T>` provides:
- **FIFO ordering** - First toast added is first shown
- **O(1) Enqueue/Dequeue** - Efficient add/remove operations
- **Clear semantics** - `Enqueue()`, `Dequeue()`, `Peek()` are intuitive

### Thread Safety

For Blazor Server scenarios, we need thread-safe operations:

```csharp
public sealed class ToastQueue<TContent> : IToastQueue<TContent>
{
    private readonly Lock _lock = new();
    private readonly Queue<QueuedToast<TContent>> _queue = new();
    
    public string Add(TContent content, ToastOptions? options = null)
    {
        lock (_lock)
        {
            var toast = CreateToast(content, options);
            _queue.Enqueue(toast);
            NotifySubscribers();
            return toast.Key;
        }
    }
    
    public void Close(string key)
    {
        lock (_lock)
        {
            var remaining = _queue.Where(t => t.Key != key).ToList();
            var closed = _queue.FirstOrDefault(t => t.Key == key);
            
            _queue.Clear();
            foreach (var t in remaining)
                _queue.Enqueue(t);
            
            closed?.Timer?.Dispose();
            NotifySubscribers();
        }
    }
}
```

## Implementation Notes

1. **Keep v1 API** - Don't break existing users; v2 is additive
2. **Shared State** - Both APIs can share the same underlying toast state
3. **Static Instance** - For non-DI scenarios, provide `ToastQueue.Default`
4. **View Transitions** - Support CSS View Transitions API when available

## React Spectrum Implementation Insights

### ToastQueue Lives Outside React/Blazor

React Spectrum's `ToastQueue` is a standalone class that exists outside the React tree, making it callable from anywhere (event handlers, services, API callbacks):

```csharp
// Generic queue - user defines their content type
public sealed class ToastQueue<TContent> : IToastQueue<TContent>
{
    // Use C#'s Queue<T> for proper FIFO ordering
    private readonly Queue<QueuedToast<TContent>> _queue = new();
    private readonly HashSet<Action> _subscriptions = [];
    
    public int MaxVisibleToasts { get; init; } = 5;
    
    public IReadOnlyList<QueuedToast<TContent>> VisibleToasts => 
        _queue.Take(MaxVisibleToasts).ToList();
    
    public string Add(TContent content, ToastOptions? options = null)
    {
        var key = Guid.NewGuid().ToString("N")[..8];
        var toast = new QueuedToast<TContent>
        {
            Key = key,
            Content = content,
            Timeout = options?.Timeout,
            Priority = options?.Priority ?? ToastPriority.Polite,
            CreatedAt = DateTime.UtcNow
        };
        
        _queue.Enqueue(toast);
        NotifySubscribers();
        
        // Start auto-dismiss timer if timeout specified
        if (options?.Timeout is int timeout)
            StartTimer(key, timeout, options.OnClose);
        
        return key;
    }
    
    public void Close(string key)
    {
        // Rebuild queue without the closed toast
        var remaining = _queue.Where(t => t.Key != key).ToList();
        var closed = _queue.FirstOrDefault(t => t.Key == key);
        
        _queue.Clear();
        foreach (var toast in remaining)
            _queue.Enqueue(toast);
        
        closed?.Timer?.Dispose();
        NotifySubscribers();
    }
}
```

### Timer Pausing on Hover/Focus

When users interact with the toast region, all timers pause. This is critical for accessibility - users need time to read and interact:

```csharp
public void PauseAll()
{
    foreach (var toast in _queue)
        toast.Timer?.Pause();
}

public void ResumeAll()
{
    foreach (var toast in _queue)
        toast.Timer?.Resume();
}
```

### Toast as AlertDialog

Each toast uses `role="alertdialog"` with proper ARIA attributes:

```html
<div role="alertdialog" 
     aria-labelledby="toast-title-123"
     aria-describedby="toast-desc-123"
     aria-modal="false">
```

### Focus Management

1. When a focused toast is removed, focus moves to next/previous toast
2. When all toasts close, focus returns to the last focused element
3. Track `lastFocused` element to restore focus properly

```csharp
private ElementReference? _lastFocused;

void OnFocusWithin(FocusEventArgs e)
{
    _lastFocused = e.RelatedTarget;
    PauseAll();
}

void OnBlurWithin()
{
    _lastFocused = null;
    ResumeAll();
}

void RestoreFocus()
{
    if (_lastFocused is not null && VisibleToasts.Count == 0)
        _lastFocused.FocusAsync();
}
```

### Landmark Region with Localizable Label

The toast region is an ARIA landmark. Labels are passed as parameters, not hardcoded:

```csharp
// ToastRegion accepts localized labels as parameters
[Parameter] public string? AriaLabel { get; set; }
[Parameter] public Func<int, string>? AriaLabelFormat { get; set; }

// Usage with any language:
<ToastRegion AriaLabel="Notifikationer" />

// Or with count formatting:
<ToastRegion AriaLabelFormat="@(count => count switch {
    0 => \"Notifikationer\",
    1 => \"Notifikationer (1 avisering)\",
    _ => $\"Notifikationer ({count} aviseringar)\"
})" />

// Default (if neither provided) uses simple "Notifications"
string GetAriaLabel() => AriaLabelFormat?.Invoke(VisibleToasts.Count) 
    ?? AriaLabel 
    ?? "Notifications";
```

For apps using `IStringLocalizer`:

```csharp
@inject IStringLocalizer<ToastResources> L

<ToastRegion AriaLabelFormat="@(count => L["ToastRegionLabel", count])" />
```

### View Transitions for Smooth Animations

Wrap state updates in CSS View Transitions:

```javascript
// In JS interop
export function wrapInViewTransition(callback) {
    if ('startViewTransition' in document) {
        document.startViewTransition(() => callback());
    } else {
        callback();
    }
}
```

### Top Layer Marker

Mark toast region as "top layer" so it's not aria-hidden when overlays (dialogs, popovers) are open:

```csharp
// data-react-aria-top-layer equivalent for SummitUI
[Parameter] public bool IsTopLayer { get; set; } = true;

// This ensures toasts remain accessible even when dialogs are open
```

### Convenience Methods (User-Defined)

Since content is user-defined, convenience methods are in user's code:

```csharp
// User defines their own toast type with variant
record AppToast(string Title, string? Description = null, ToastVariant Variant = ToastVariant.Default);

enum ToastVariant { Default, Success, Error, Warning, Info }

// User creates their own extension methods
public static class ToastExtensions
{
    public static string Success(this IToastQueue<AppToast> queue, string title, string? description = null)
        => queue.Add(new(title, description, ToastVariant.Success));
    
    public static string Error(this IToastQueue<AppToast> queue, string title, string? description = null)
        => queue.Add(new(title, description, ToastVariant.Error), 
            new() { Priority = ToastPriority.Assertive });
}

// Usage - inject and call extension methods
@inject IToastQueue<AppToast> Toasts

Toasts.Success("File saved!");
Toasts.Error("Upload failed", "Network error");
```

## Comparison with React Aria

| Feature | React Aria | SummitUI v2 |
|---------|------------|-------------|
| Queue API | `queue.add()` | `Queue.Add()` |
| Content | `{title, description}` | `new(title, description)` |
| Options | `{timeout}` | `new() { Timeout = }` |
| Template | `{({toast}) => ...}` | `<Template Context="toast">` |
| Navigation | F6/Shift+F6 | F6/Shift+F6 |
| Close | `queue.close(key)` | `Queue.Close(key)` |

## Full Implementation Plan

### Phase 1: Core Queue (Non-breaking)
1. Create `ToastQueue` class with `Add`, `Close`, `PauseAll`, `ResumeAll`
2. Add `QueuedToast` record with Timer support
3. Implement subscription pattern for UI updates
4. Add `ToastQueue.Default` static instance

### Phase 2: New Components
1. Create `ToastRegion` with template support and F6 navigation
2. Create `ToastItem` (renamed from Toast to avoid confusion) with alertdialog role
3. Create `ToastContent` and `ToastCloseButton` 
4. Add hover/focus pause behavior

### Phase 3: Accessibility
1. Implement proper landmark region with dynamic aria-label
2. Add focus management (restore focus on close)
3. Implement F6/Shift+F6 landmark navigation
4. Ensure top-layer behavior with overlays

### Phase 4: Animations
1. Add JS interop for CSS View Transitions
2. Support `viewTransitionName` per toast for individual animations
3. Add data attributes for CSS animation hooks

### Phase 5: Documentation
1. Update Toast.razor docs page with new API
2. Update ToastComposition guide with queue patterns
3. Add examples for common scenarios
