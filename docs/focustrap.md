# FocusTrap

A utility component that traps keyboard focus within its children, preventing focus from escaping to elements outside the trap.

## Features

- Traps Tab and Shift+Tab navigation within boundaries
- Automatic focus on activation
- Returns focus on deactivation
- Handles edge cases (no focusable elements)
- Programmatic focus control methods
- Activation/deactivation callbacks

## Import

```razor
@using SummitUI.Components.Utilities
```

## Anatomy

```razor
<FocusTrap IsActive="true">
    <div>
        <input type="text" />
        <button>Submit</button>
    </div>
</FocusTrap>
```

## API Reference

### FocusTrap

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Content to render within the trap |
| `IsActive` | `bool` | `true` | Whether trap is active |
| `AutoFocus` | `bool` | `true` | Auto-focus first element when activated |
| `ReturnFocus` | `bool` | `true` | Return focus to previously focused element on deactivation |
| `OnActivated` | `EventCallback` | - | Callback when trap activates |
| `OnDeactivated` | `EventCallback` | - | Callback when trap deactivates |

### Public Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `FocusFirstAsync()` | `ValueTask` | Manually focus the first focusable element |
| `FocusLastAsync()` | `ValueTask` | Manually focus the last focusable element |

## Examples

### Basic Dialog

```razor
@code {
    private bool showDialog = false;
}

@if (showDialog)
{
    <div class="dialog-overlay">
        <FocusTrap IsActive="true" AutoFocus="true">
            <div class="dialog" role="dialog" aria-modal="true" aria-labelledby="dialog-title">
                <h2 id="dialog-title">Confirm Action</h2>
                <p>Are you sure you want to proceed?</p>
                <div class="dialog-actions">
                    <button @onclick="@(() => showDialog = false)">Cancel</button>
                    <button @onclick="HandleConfirm">Confirm</button>
                </div>
            </div>
        </FocusTrap>
    </div>
}

<button @onclick="@(() => showDialog = true)">Open Dialog</button>

@code {
    private void HandleConfirm()
    {
        // Handle confirmation
        showDialog = false;
    }
}
```

### Modal Form

```razor
@code {
    private bool showModal = false;
    private string name = "";
    private string email = "";
}

@if (showModal)
{
    <div class="modal-overlay" @onclick="@(() => showModal = false)">
        <FocusTrap IsActive="true" AutoFocus="true" ReturnFocus="true">
            <div class="modal" @onclick:stopPropagation="true" role="dialog" aria-modal="true">
                <h2>Sign Up</h2>
                <form @onsubmit="HandleSubmit">
                    <div class="form-field">
                        <label for="name">Name</label>
                        <input type="text" id="name" @bind="name" required />
                    </div>
                    <div class="form-field">
                        <label for="email">Email</label>
                        <input type="email" id="email" @bind="email" required />
                    </div>
                    <div class="form-actions">
                        <button type="button" @onclick="@(() => showModal = false)">
                            Cancel
                        </button>
                        <button type="submit">Submit</button>
                    </div>
                </form>
            </div>
        </FocusTrap>
    </div>
}

<button @onclick="@(() => showModal = true)">Open Sign Up</button>

@code {
    private void HandleSubmit()
    {
        Console.WriteLine($"Submitted: {name}, {email}");
        showModal = false;
    }
}
```

### Controlled Activation

```razor
@code {
    private bool isTrapActive = false;
}

<div class="control-panel">
    <button @onclick="@(() => isTrapActive = !isTrapActive)">
        @(isTrapActive ? "Deactivate" : "Activate") Focus Trap
    </button>
</div>

<FocusTrap IsActive="@isTrapActive" 
           OnActivated="HandleActivated" 
           OnDeactivated="HandleDeactivated">
    <div class="trap-container">
        <h3>Focus Trap Area</h3>
        <input type="text" placeholder="First input" />
        <input type="text" placeholder="Second input" />
        <button>Action Button</button>
    </div>
</FocusTrap>

@code {
    private void HandleActivated()
    {
        Console.WriteLine("Focus trap activated");
    }
    
    private void HandleDeactivated()
    {
        Console.WriteLine("Focus trap deactivated");
    }
}
```

### Without Auto Focus

```razor
<FocusTrap IsActive="true" AutoFocus="false">
    <div class="container">
        <p>Focus is trapped but not automatically set.</p>
        <input type="text" placeholder="Click to focus" />
        <button>Button</button>
    </div>
</FocusTrap>
```

### Without Return Focus

```razor
@code {
    private bool showPanel = false;
}

<button @onclick="@(() => showPanel = true)">Open Panel</button>

@if (showPanel)
{
    <FocusTrap IsActive="true" AutoFocus="true" ReturnFocus="false">
        <div class="panel">
            <p>When closed, focus will not return to the button.</p>
            <button @onclick="@(() => showPanel = false)">Close</button>
        </div>
    </FocusTrap>
}
```

### Programmatic Focus Control

```razor
@code {
    private FocusTrap? focusTrap;
}

<FocusTrap @ref="focusTrap" IsActive="true" AutoFocus="false">
    <div class="container">
        <input type="text" placeholder="First" />
        <input type="text" placeholder="Middle" />
        <input type="text" placeholder="Last" />
    </div>
</FocusTrap>

<div class="controls">
    <button @onclick="FocusFirst">Focus First</button>
    <button @onclick="FocusLast">Focus Last</button>
</div>

@code {
    private async Task FocusFirst()
    {
        if (focusTrap != null)
        {
            await focusTrap.FocusFirstAsync();
        }
    }
    
    private async Task FocusLast()
    {
        if (focusTrap != null)
        {
            await focusTrap.FocusLastAsync();
        }
    }
}
```

### With No Focusable Elements

```razor
<FocusTrap IsActive="true" AutoFocus="true">
    <div tabindex="-1" @onkeydown="HandleKeyDown" class="message-container">
        <p>This container has no focusable elements.</p>
        <p>Press Escape to close.</p>
    </div>
</FocusTrap>

@code {
    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            // Handle escape
        }
    }
}
```

### Nested Focus Traps

```razor
@code {
    private bool showInnerDialog = false;
}

<FocusTrap IsActive="true">
    <div class="outer-dialog">
        <h2>Outer Dialog</h2>
        <input type="text" placeholder="Outer input" />
        <button @onclick="@(() => showInnerDialog = true)">Open Inner</button>
        
        @if (showInnerDialog)
        {
            <FocusTrap IsActive="true" AutoFocus="true">
                <div class="inner-dialog">
                    <h3>Inner Dialog</h3>
                    <input type="text" placeholder="Inner input" />
                    <button @onclick="@(() => showInnerDialog = false)">Close</button>
                </div>
            </FocusTrap>
        }
    </div>
</FocusTrap>
```

### Sidebar Navigation

```razor
@code {
    private bool sidebarOpen = false;
}

<button @onclick="@(() => sidebarOpen = true)" aria-expanded="@sidebarOpen">
    Open Menu
</button>

@if (sidebarOpen)
{
    <div class="sidebar-overlay" @onclick="@(() => sidebarOpen = false)">
        <FocusTrap IsActive="true" AutoFocus="true" ReturnFocus="true">
            <nav class="sidebar" @onclick:stopPropagation="true" role="navigation">
                <button class="close-btn" @onclick="@(() => sidebarOpen = false)" 
                        aria-label="Close menu">
                    ✕
                </button>
                <a href="/home">Home</a>
                <a href="/about">About</a>
                <a href="/services">Services</a>
                <a href="/contact">Contact</a>
            </nav>
        </FocusTrap>
    </div>
}
```

### With Styling

```css
.dialog-overlay {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
}

.dialog {
    background: white;
    padding: 24px;
    border-radius: 8px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.2);
    max-width: 400px;
    width: 100%;
}

.dialog h2 {
    margin-top: 0;
}

.dialog-actions {
    display: flex;
    gap: 12px;
    justify-content: flex-end;
    margin-top: 24px;
}

.trap-container {
    border: 2px solid #0066cc;
    padding: 20px;
    border-radius: 8px;
}

.trap-container[data-focus-trap-active="true"] {
    border-color: #00cc66;
}
```

## How It Works

### Focus Trapping Mechanism

1. When activated, the component identifies all focusable elements within its boundaries
2. It intercepts Tab and Shift+Tab key presses
3. When Tab is pressed on the last focusable element, focus moves to the first
4. When Shift+Tab is pressed on the first focusable element, focus moves to the last
5. This creates a circular focus loop within the trapped area

### Focusable Elements

The following elements are considered focusable:
- `<a>` with `href` attribute
- `<button>` (not disabled)
- `<input>` (not disabled, not `type="hidden"`)
- `<select>` (not disabled)
- `<textarea>` (not disabled)
- Elements with `tabindex` >= 0
- Elements with `contenteditable`

### Edge Cases

- **No focusable elements**: If there are no focusable elements, the trap container itself receives focus (if it has `tabindex="-1"`)
- **Single focusable element**: Focus stays on that element
- **Dynamically added elements**: New focusable elements are automatically included in the trap

## Accessibility

### Best Practices

1. **Always use with modals/dialogs**: Focus trapping is essential for modal accessibility
2. **Provide an escape route**: Always include a way to close/deactivate the trap (close button, Escape key)
3. **Use with `role="dialog"`**: Combine with proper ARIA roles for screen readers
4. **Return focus**: Keep `ReturnFocus` enabled to maintain user context

### Screen Reader Considerations

- The focus trap should be used in conjunction with `aria-modal="true"` for dialogs
- Include `aria-labelledby` or `aria-label` on the dialog container
- Announce the dialog opening/closing to screen readers if needed

### Example Accessible Dialog

```razor
<FocusTrap IsActive="true" AutoFocus="true" ReturnFocus="true">
    <div role="dialog" 
         aria-modal="true" 
         aria-labelledby="dialog-title"
         aria-describedby="dialog-description">
        <h2 id="dialog-title">Delete Item</h2>
        <p id="dialog-description">
            Are you sure you want to delete this item? This action cannot be undone.
        </p>
        <button @onclick="Cancel">Cancel</button>
        <button @onclick="Delete">Delete</button>
    </div>
</FocusTrap>
```

## Common Use Cases

| Use Case | Configuration |
|----------|---------------|
| Modal dialog | `IsActive="true" AutoFocus="true" ReturnFocus="true"` |
| Dropdown menu | `IsActive="@isOpen" AutoFocus="true" ReturnFocus="true"` |
| Sidebar navigation | `IsActive="@isOpen" AutoFocus="true" ReturnFocus="true"` |
| Inline editor | `IsActive="@isEditing" AutoFocus="true" ReturnFocus="true"` |
| Toast/notification | `IsActive="false"` (usually not needed) |
