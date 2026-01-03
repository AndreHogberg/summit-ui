# Popover

A floating content panel that appears next to a trigger element, commonly used for tooltips, dropdowns, and informational overlays.

## Features

- Flexible positioning with collision detection
- Optional overlay backdrop
- Focus trapping support
- Close button component
- Controlled and uncontrolled modes
- WCAG compliant with proper ARIA attributes

## Import

```razor
@using SummitUI.Components.Popover
```

## Anatomy

```razor
<PopoverRoot>
    <PopoverTrigger>Open</PopoverTrigger>
    <PopoverPortal>
        <PopoverContent>
            <p>Popover content</p>
            <PopoverClose>Close</PopoverClose>
        </PopoverContent>
    </PopoverPortal>
</PopoverRoot>
```

## Sub-components

| Component | Description |
|-----------|-------------|
| `PopoverRoot` | Root container managing popover state |
| `PopoverTrigger` | Button that toggles the popover |
| `PopoverPortal` | Renders content in fixed-position container |
| `PopoverContent` | Floating content panel with positioning |
| `PopoverOverlay` | Optional backdrop overlay |
| `PopoverClose` | Close button within popover |

## API Reference

### PopoverRoot

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Child content |
| `Open` | `bool?` | - | Controlled open state |
| `DefaultOpen` | `bool` | `false` | Default open state (uncontrolled) |
| `OpenChanged` | `EventCallback<bool>` | - | Open state change callback |
| `OnOpen` | `EventCallback` | - | Callback when popover opens |
| `OnClose` | `EventCallback` | - | Callback when popover closes |
| `Modal` | `bool` | `false` | Whether to trap focus |

### PopoverTrigger

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Trigger content |
| `As` | `string` | `"button"` | HTML element |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### PopoverPortal

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Content to portal |
| `ContainerId` | `string?` | - | Optional custom container ID |

### PopoverContent

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Popover content |
| `As` | `string` | `"div"` | HTML element |
| `Side` | `Side` | `Bottom` | Placement side |
| `SideOffset` | `int` | `0` | Offset from trigger (px) |
| `Align` | `Align` | `Center` | Alignment along side axis |
| `AlignOffset` | `int` | `0` | Alignment offset (px) |
| `AvoidCollisions` | `bool` | `true` | Avoid viewport boundaries |
| `CollisionPadding` | `int` | `8` | Viewport padding (px) |
| `TrapFocus` | `bool` | `false` | Whether to trap focus |
| `EscapeKeyBehavior` | `EscapeKeyBehavior` | `Close` | Escape key behavior |
| `OutsideClickBehavior` | `OutsideClickBehavior` | `Close` | Outside click behavior |
| `OnInteractOutside` | `EventCallback` | - | Outside click callback |
| `OnEscapeKeyDown` | `EventCallback` | - | Escape key callback |
| `OnOpenAutoFocus` | `EventCallback` | - | After open focus callback |
| `OnCloseAutoFocus` | `EventCallback` | - | After close focus callback |

### PopoverOverlay

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Optional overlay content |
| `As` | `string` | `"div"` | HTML element |
| `OnClick` | `EventCallback<MouseEventArgs>` | - | Click callback |
| `CloseOnClick` | `bool` | `true` | Close popover on overlay click |

### PopoverClose

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Button content |
| `As` | `string` | `"button"` | HTML element |
| `AriaLabel` | `string?` | `"Close"` | Accessible label |

## Enums

### Side

```csharp
public enum Side
{
    Top,
    Right,
    Bottom,
    Left
}
```

### Align

```csharp
public enum Align
{
    Start,
    Center,
    End
}
```

### EscapeKeyBehavior

```csharp
public enum EscapeKeyBehavior
{
    Close,  // Close popover on Escape key
    Ignore  // Ignore Escape key
}
```

### OutsideClickBehavior

```csharp
public enum OutsideClickBehavior
{
    Close,  // Close popover on outside click
    Ignore  // Ignore outside clicks
}
```

## Examples

### Basic Popover

```razor
<PopoverRoot>
    <PopoverTrigger class="btn btn-primary">
        Click me
    </PopoverTrigger>
    <PopoverPortal>
        <PopoverContent class="popover-content" SideOffset="8">
            <h3>Basic Popover</h3>
            <p>This is a simple popover with some content.</p>
            <PopoverClose class="popover-close">Close</PopoverClose>
        </PopoverContent>
    </PopoverPortal>
</PopoverRoot>
```

### With Overlay

```razor
<PopoverRoot>
    <PopoverTrigger class="btn">Open with Overlay</PopoverTrigger>
    <PopoverPortal>
        <PopoverOverlay class="popover-overlay" />
        <PopoverContent class="popover-content" SideOffset="8">
            <h3>Modal-like Popover</h3>
            <p>Click the overlay or press Escape to close.</p>
            <PopoverClose class="btn">Done</PopoverClose>
        </PopoverContent>
    </PopoverPortal>
</PopoverRoot>
```

### With Focus Trap

```razor
<PopoverRoot Modal="true">
    <PopoverTrigger class="btn">Open Form</PopoverTrigger>
    <PopoverPortal>
        <PopoverOverlay class="popover-overlay" />
        <PopoverContent class="popover-content" TrapFocus="true" SideOffset="8">
            <h3>Sign Up</h3>
            <form>
                <div>
                    <label for="name">Name</label>
                    <input type="text" id="name" placeholder="Enter your name" />
                </div>
                <div>
                    <label for="email">Email</label>
                    <input type="email" id="email" placeholder="Enter your email" />
                </div>
                <div class="button-group">
                    <PopoverClose class="btn btn-secondary">Cancel</PopoverClose>
                    <button type="submit" class="btn btn-primary">Submit</button>
                </div>
            </form>
        </PopoverContent>
    </PopoverPortal>
</PopoverRoot>
```

### Different Positions

```razor
@* Bottom (default) *@
<PopoverRoot>
    <PopoverTrigger>Bottom</PopoverTrigger>
    <PopoverPortal>
        <PopoverContent Side="Side.Bottom" SideOffset="8">
            Content appears below
        </PopoverContent>
    </PopoverPortal>
</PopoverRoot>

@* Top *@
<PopoverRoot>
    <PopoverTrigger>Top</PopoverTrigger>
    <PopoverPortal>
        <PopoverContent Side="Side.Top" SideOffset="8">
            Content appears above
        </PopoverContent>
    </PopoverPortal>
</PopoverRoot>

@* Left *@
<PopoverRoot>
    <PopoverTrigger>Left</PopoverTrigger>
    <PopoverPortal>
        <PopoverContent Side="Side.Left" SideOffset="8">
            Content appears to the left
        </PopoverContent>
    </PopoverPortal>
</PopoverRoot>

@* Right *@
<PopoverRoot>
    <PopoverTrigger>Right</PopoverTrigger>
    <PopoverPortal>
        <PopoverContent Side="Side.Right" SideOffset="8">
            Content appears to the right
        </PopoverContent>
    </PopoverPortal>
</PopoverRoot>
```

### Alignment Options

```razor
@* Start aligned *@
<PopoverContent Side="Side.Bottom" Align="Align.Start" SideOffset="8">
    Aligned to start
</PopoverContent>

@* Center aligned (default) *@
<PopoverContent Side="Side.Bottom" Align="Align.Center" SideOffset="8">
    Centered
</PopoverContent>

@* End aligned *@
<PopoverContent Side="Side.Bottom" Align="Align.End" SideOffset="8">
    Aligned to end
</PopoverContent>
```

### Controlled Mode

```razor
@code {
    private bool isOpen = false;
}

<PopoverRoot Open="@isOpen" OpenChanged="@(v => isOpen = v)">
    <PopoverTrigger class="btn">
        @(isOpen ? "Close" : "Open")
    </PopoverTrigger>
    <PopoverPortal>
        <PopoverContent class="popover-content" SideOffset="8">
            <p>Controlled popover content</p>
        </PopoverContent>
    </PopoverPortal>
</PopoverRoot>

<button @onclick="@(() => isOpen = !isOpen)">
    Toggle Externally
</button>
```

### Non-Dismissible

```razor
<PopoverRoot>
    <PopoverTrigger class="btn">Open Persistent</PopoverTrigger>
    <PopoverPortal>
        <PopoverContent 
            class="popover-content" 
            SideOffset="8"
            EscapeKeyBehavior="EscapeKeyBehavior.Ignore"
            OutsideClickBehavior="OutsideClickBehavior.Ignore">
            <h3>Persistent Popover</h3>
            <p>You must use the close button to dismiss this.</p>
            <PopoverClose class="btn">Close</PopoverClose>
        </PopoverContent>
    </PopoverPortal>
</PopoverRoot>
```

### With Callbacks

```razor
<PopoverRoot OnOpen="HandleOpen" OnClose="HandleClose">
    <PopoverTrigger class="btn">Open</PopoverTrigger>
    <PopoverPortal>
        <PopoverContent 
            class="popover-content" 
            SideOffset="8"
            OnInteractOutside="HandleOutsideClick"
            OnEscapeKeyDown="HandleEscape">
            <p>Try clicking outside or pressing Escape</p>
        </PopoverContent>
    </PopoverPortal>
</PopoverRoot>

@code {
    private void HandleOpen() => Console.WriteLine("Popover opened");
    private void HandleClose() => Console.WriteLine("Popover closed");
    private void HandleOutsideClick() => Console.WriteLine("Clicked outside");
    private void HandleEscape() => Console.WriteLine("Escape pressed");
}
```

### With Styling

```razor
<PopoverRoot>
    <PopoverTrigger class="popover-trigger">
        <span>Info</span>
        <span class="info-icon">i</span>
    </PopoverTrigger>
    <PopoverPortal>
        <PopoverOverlay class="popover-overlay" />
        <PopoverContent class="popover-content" SideOffset="8">
            <div class="popover-header">
                <h3>Information</h3>
                <PopoverClose class="popover-close-btn" AriaLabel="Close">
                    ✕
                </PopoverClose>
            </div>
            <div class="popover-body">
                <p>Here's some helpful information.</p>
            </div>
        </PopoverContent>
    </PopoverPortal>
</PopoverRoot>
```

```css
.popover-trigger {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 8px 16px;
    background: white;
    border: 1px solid #ccc;
    border-radius: 4px;
    cursor: pointer;
}

.popover-trigger[data-state="open"] {
    background: #f0f0f0;
}

.popover-overlay {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.4);
}

.popover-content {
    background: white;
    border-radius: 8px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
    min-width: 280px;
    max-width: 400px;
}

.popover-content[data-state="closed"] {
    display: none;
}

.popover-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 16px;
    border-bottom: 1px solid #e0e0e0;
}

.popover-header h3 {
    margin: 0;
}

.popover-close-btn {
    background: none;
    border: none;
    cursor: pointer;
    padding: 4px;
    color: #666;
}

.popover-close-btn:hover {
    color: #333;
}

.popover-body {
    padding: 16px;
}
```

## Accessibility

### Keyboard Navigation

| Key | Action |
|-----|--------|
| `Enter` / `Space` | Toggle popover (on trigger) |
| `Escape` | Close popover |
| `Tab` | Move focus within popover (when focus trapped) |

### ARIA Attributes

- Trigger has `aria-haspopup="dialog"` and `aria-expanded`
- Content is linked to trigger via `aria-controls`
- Close button has `aria-label` for screen readers

### Focus Management

- When opened, focus moves to the first focusable element in the content
- When closed, focus returns to the trigger
- Optional focus trapping keeps focus within the popover

### Data Attributes

| Attribute | Values | Description |
|-----------|--------|-------------|
| `data-state` | `"open"` / `"closed"` | Popover open state |
| `data-side` | `"top"` / `"right"` / `"bottom"` / `"left"` | Current placement side |
| `data-align` | `"start"` / `"center"` / `"end"` | Current alignment |
