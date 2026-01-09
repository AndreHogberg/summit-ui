# Toast Component

A succinct message that is displayed temporarily. Follows the [WCAG aria-live requirements](https://www.w3.org/TR/wai-aria/#aria-live).

## Features

- ✅ Automatically closes after a configurable duration
- ✅ Pauses closing on hover, focus, and window blur
- ✅ Supports hotkey (F8 by default) to jump to toast viewport
- ✅ Supports closing via swipe gesture
- ✅ Exposes CSS custom properties for swipe gesture animations
- ✅ Can be controlled or uncontrolled
- ✅ Foreground (assertive) and Background (polite) announcement types
- ✅ Optional service-based API for imperative toast creation

## References

- https://www.w3.org/TR/wai-aria/#aria-live
- https://github.com/radix-ui/primitives/blob/main/packages/react/toast/src/toast.tsx
- https://www.radix-ui.com/primitives/docs/components/toast

## Anatomy

```razor
<ToastProvider>
    <!-- Your app content -->
    
    <ToastViewport>
        <ToastRoot>
            <ToastTitle />
            <ToastDescription />
            <ToastAction AltText="..." />
            <ToastClose />
        </ToastRoot>
    </ToastViewport>
</ToastProvider>
```

## Components

### ToastProvider

The provider that wraps your toasts and toast viewport. Place once at the root of your application.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Label | string | "Notification" | Label for screen readers |
| Duration | int | 5000 | Default duration in milliseconds |
| SwipeDirection | SwipeDirection | Right | Swipe direction to dismiss |
| SwipeThreshold | int | 50 | Pixels before close triggers |

### ToastViewport

The fixed area where toasts appear. Users can jump here via hotkey.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Hotkey | string[] | ["F8"] | Keyboard shortcut to focus viewport |
| Label | string | "Notifications ({hotkey})" | ARIA label with {hotkey} placeholder |
| As | string | "ol" | HTML element to render |

### ToastRoot

Individual toast container that automatically closes.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Open | bool? | null | Controlled open state |
| DefaultOpen | bool | true | Default open state (uncontrolled) |
| OpenChanged | EventCallback<bool> | - | Open state change callback |
| Duration | int? | null | Override provider duration |
| Type | ToastType | Foreground | Screen reader priority |
| ForceMount | bool | false | Force render for animations |
| As | string | "li" | HTML element to render |

**Events:**
- `OnEscapeKeyDown` - When Escape is pressed
- `OnPause` - When auto-close timer pauses
- `OnResume` - When auto-close timer resumes
- `OnSwipeStart/Move/Cancel/End` - Swipe gesture events

**Data Attributes:**
- `data-state` - "open" | "closed"
- `data-swipe` - "start" | "move" | "cancel" | "end"
- `data-swipe-direction` - "up" | "down" | "left" | "right"

**CSS Variables:**
- `--summit-toast-swipe-move-x/y` - Offset during swipe
- `--summit-toast-swipe-end-x/y` - Final offset after swipe

### ToastTitle

Optional title for the toast.

### ToastDescription

The toast message (required for accessibility).

### ToastAction

Action button. **Must include AltText** for screen reader users.

| Parameter | Type | Description |
|-----------|------|-------------|
| AltText | string | **Required.** Alternative text for screen readers |
| OnClick | EventCallback | Click handler |

### ToastClose

Dismiss button. Defaults to "×" if no content provided.

## Usage Examples

### Basic Declarative Usage

```razor
<ToastProvider>
    <div>Your app content here</div>
    
    <ToastViewport class="toast-viewport">
        @if (showToast)
        {
            <ToastRoot @bind-Open="showToast">
                <ToastTitle>Success!</ToastTitle>
                <ToastDescription>Your changes have been saved.</ToastDescription>
                <ToastClose />
            </ToastRoot>
        }
    </ToastViewport>
</ToastProvider>

@code {
    private bool showToast = false;
    
    private void Save()
    {
        // ... save logic
        showToast = true;
    }
}
```

### Using the Toast Service

```razor
@inject IToastService ToastService

<ToastProvider>
    <button @onclick="ShowToast">Show Toast</button>
    
    <ToastViewport class="toast-viewport">
        @foreach (var toast in ToastService.Toasts)
        {
            <ToastRoot @key="toast.Id" Duration="@toast.Duration">
                @if (!string.IsNullOrEmpty(toast.Title))
                {
                    <ToastTitle>@toast.Title</ToastTitle>
                }
                <ToastDescription>@toast.Message</ToastDescription>
                <ToastClose />
            </ToastRoot>
        }
    </ToastViewport>
</ToastProvider>

@code {
    private void ShowToast()
    {
        ToastService.Show("Hello, World!");
    }
}
```

### With Action Button

```razor
<ToastRoot>
    <ToastTitle>File Deleted</ToastTitle>
    <ToastDescription>The file was moved to trash.</ToastDescription>
    <ToastAction AltText="Press to undo file deletion" OnClick="UndoDelete">
        Undo
    </ToastAction>
    <ToastClose />
</ToastRoot>
```

## Creating Custom Toast Abstractions

The `IToastService` is designed to be extended. Here's how to create your own abstraction:

### Step 1: Create Your Interface

```csharp
public interface IMyToastService : IToastService
{
    void ShowSuccess(string message, string? title = null);
    void ShowError(string message, string? title = null);
    void ShowWarning(string message, string? title = null);
    void ShowInfo(string message, string? title = null);
}
```

### Step 2: Implement the Interface

```csharp
public class MyToastService : ToastService, IMyToastService
{
    public void ShowSuccess(string message, string? title = null)
    {
        Show(new ToastOptions
        {
            Title = title ?? "Success",
            Message = message,
            Variant = "success",
            Duration = 3000
        });
    }
    
    public void ShowError(string message, string? title = null)
    {
        Show(new ToastOptions
        {
            Title = title ?? "Error",
            Message = message,
            Variant = "error",
            Duration = 10000 // Longer duration for errors
        });
    }
    
    public void ShowWarning(string message, string? title = null)
    {
        Show(new ToastOptions
        {
            Title = title ?? "Warning",
            Message = message,
            Variant = "warning",
            Duration = 5000
        });
    }
    
    public void ShowInfo(string message, string? title = null)
    {
        Show(new ToastOptions
        {
            Title = title ?? "Info",
            Message = message,
            Variant = "info",
            Duration = 4000
        });
    }
}
```

### Step 3: Register in DI

```csharp
// In Program.cs
builder.Services.AddScoped<IMyToastService, MyToastService>();
// Optionally also register as IToastService
builder.Services.AddScoped<IToastService>(sp => sp.GetRequiredService<IMyToastService>());
```

### Step 4: Style Based on Variant

```css
[data-summit-toast-root][data-variant="success"] {
    border-color: #10b981;
    background: #ecfdf5;
}

[data-summit-toast-root][data-variant="error"] {
    border-color: #ef4444;
    background: #fef2f2;
}

[data-summit-toast-root][data-variant="warning"] {
    border-color: #f59e0b;
    background: #fffbeb;
}

[data-summit-toast-root][data-variant="info"] {
    border-color: #3b82f6;
    background: #eff6ff;
}
```

### Step 5: Use Your Custom Service

```razor
@inject IMyToastService Toast

<button @onclick='() => Toast.ShowSuccess("Saved!")'>Save</button>
<button @onclick='() => Toast.ShowError("Failed to save")'>Force Error</button>
```

## Accessibility

### WCAG Compliance

The Toast component adheres to [WAI-ARIA live region requirements](https://www.w3.org/TR/wai-aria/#aria-live):

- Uses `role="status"` for all toasts
- Uses `aria-live="assertive"` for foreground toasts (immediate announcement)
- Uses `aria-live="polite"` for background toasts (announced at next opportunity)
- Uses `aria-atomic="true"` to announce the entire toast content
- Links title and description via `aria-labelledby` and `aria-describedby`

### Toast Types

**Foreground (assertive):** Use for toasts that are the result of a user action.
```razor
<ToastRoot Type="ToastType.Foreground">
    <ToastDescription>File saved successfully.</ToastDescription>
</ToastRoot>
```

**Background (polite):** Use for toasts generated from background tasks.
```razor
<ToastRoot Type="ToastType.Background">
    <ToastDescription>New version available.</ToastDescription>
</ToastRoot>
```

### Alternative Action Text

The `AltText` prop on `ToastAction` provides screen reader users with an alternative way to action the toast:

```razor
<ToastAction AltText="Go to account settings to upgrade">
    Upgrade
</ToastAction>

<!-- Or with a keyboard shortcut -->
<ToastAction AltText="Undo (Alt+U)">
    Undo <kbd>Alt</kbd>+<kbd>U</kbd>
</ToastAction>
```

### Close Icon Button

When using an icon for the close button, provide an aria-label:

```razor
<ToastClose aria-label="Dismiss notification">
    <span aria-hidden="true">×</span>
</ToastClose>
```

### Keyboard Interactions

| Key | Action |
|-----|--------|
| F8 | Focus the toast viewport |
| Tab | Move focus to next focusable element |
| Shift+Tab | Move focus to previous element |
| Space/Enter | Activate focused action/close button |
| Escape | Close the focused toast |

## Styling

### Basic Viewport Positioning

```css
.toast-viewport {
    position: fixed;
    bottom: 16px;
    right: 16px;
    display: flex;
    flex-direction: column;
    gap: 8px;
    max-width: 400px;
    padding: 0;
    margin: 0;
    list-style: none;
    z-index: 1000;
}
```

### Swipe Animation

```css
[data-summit-toast-root][data-swipe="move"] {
    transform: translateX(var(--summit-toast-swipe-move-x, 0));
}

[data-summit-toast-root][data-swipe="cancel"] {
    transform: translateX(0);
    transition: transform 200ms ease-out;
}

[data-summit-toast-root][data-swipe="end"] {
    animation: toast-slide-out 200ms ease-out forwards;
}

@keyframes toast-slide-out {
    from {
        transform: translateX(var(--summit-toast-swipe-end-x, 0));
    }
    to {
        transform: translateX(100%);
        opacity: 0;
    }
}
```

### Open/Closed Animation

```css
[data-summit-toast-root][data-state="open"] {
    animation: toast-fade-in 200ms ease-out;
}

[data-summit-toast-root][data-state="closed"] {
    animation: toast-fade-out 200ms ease-out;
}

@keyframes toast-fade-in {
    from { opacity: 0; transform: translateY(20px); }
    to { opacity: 1; transform: translateY(0); }
}

@keyframes toast-fade-out {
    from { opacity: 1; }
    to { opacity: 0; }
}
```

## Tests

Tests are located in `tests/SummitUI.Tests.Playwright/Toast/`:

- `ToastAccessibilityTests.cs` - ARIA attributes and WCAG compliance
- `ToastKeyboardTests.cs` - Hotkey and keyboard navigation
- `ToastBasicTests.cs` - Basic functionality and interactions

