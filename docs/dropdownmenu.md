# DropdownMenu

A floating menu component with support for items, checkbox items, radio groups, and nested groups.

## Features

- Full keyboard navigation
- Checkbox and radio item support
- Grouped items with labels
- Flexible positioning with collision detection
- Focus trapping in modal mode
- Controlled and uncontrolled modes
- WCAG compliant with proper ARIA attributes

## Import

```razor
@using ArkUI.Components.DropdownMenu
```

## Anatomy

```razor
<DropdownMenuRoot>
    <DropdownMenuTrigger>Open Menu</DropdownMenuTrigger>
    <DropdownMenuPortal>
        <DropdownMenuContent>
            <DropdownMenuItem>Item 1</DropdownMenuItem>
            <DropdownMenuItem>Item 2</DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem>Item 3</DropdownMenuItem>
        </DropdownMenuContent>
    </DropdownMenuPortal>
</DropdownMenuRoot>
```

## Sub-components

| Component | Description |
|-----------|-------------|
| `DropdownMenuRoot` | Root container managing menu state |
| `DropdownMenuTrigger` | Button that toggles the menu |
| `DropdownMenuPortal` | Renders content at document body level |
| `DropdownMenuContent` | Floating content panel with positioning |
| `DropdownMenuItem` | Single selectable menu item |
| `DropdownMenuCheckboxItem` | Toggle-able checkbox menu item |
| `DropdownMenuRadioGroup` | Container for radio items |
| `DropdownMenuRadioItem` | Radio option within a group |
| `DropdownMenuGroup` | Groups related items |
| `DropdownMenuGroupLabel` | Label for a group |
| `DropdownMenuSeparator` | Visual separator |

## API Reference

### DropdownMenuRoot

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Child content |
| `Open` | `bool?` | - | Controlled open state |
| `DefaultOpen` | `bool` | `false` | Default open state (uncontrolled) |
| `OpenChanged` | `EventCallback<bool>` | - | Callback when open state changes |
| `OnOpen` | `EventCallback` | - | Callback when menu opens |
| `OnClose` | `EventCallback` | - | Callback when menu closes |
| `Modal` | `bool` | `true` | Whether to trap focus |
| `Dir` | `string` | `"ltr"` | Reading direction |

### DropdownMenuTrigger

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Trigger content |
| `As` | `string` | `"button"` | HTML element |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### DropdownMenuContent

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Menu content |
| `As` | `string` | `"div"` | HTML element |
| `Side` | `DropdownMenuSide` | `Bottom` | Placement side |
| `SideOffset` | `int` | `4` | Offset from trigger (px) |
| `Align` | `DropdownMenuAlign` | `Start` | Alignment along side axis |
| `AlignOffset` | `int` | `0` | Alignment offset (px) |
| `AvoidCollisions` | `bool` | `true` | Avoid viewport boundaries |
| `CollisionPadding` | `int` | `8` | Viewport padding (px) |
| `EscapeKeyBehavior` | `EscapeKeyBehavior` | `Close` | Escape key behavior |
| `OutsideClickBehavior` | `OutsideClickBehavior` | `Close` | Outside click behavior |
| `OnInteractOutside` | `EventCallback` | - | Outside click callback |
| `OnEscapeKeyDown` | `EventCallback` | - | Escape key callback |
| `OnOpenAutoFocus` | `EventCallback` | - | After open focus callback |
| `OnCloseAutoFocus` | `EventCallback` | - | After close focus callback |
| `Loop` | `bool` | `true` | Loop keyboard navigation |
| `ForceMount` | `bool` | `false` | Always render in DOM |

### DropdownMenuItem

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Item content |
| `Disabled` | `bool` | `false` | Disable item |
| `OnSelect` | `EventCallback` | - | Selection callback |
| `OnClick` | `EventCallback<MouseEventArgs>` | - | Click callback |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### DropdownMenuCheckboxItem

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment<DropdownMenuCheckboxItemContext>?` | - | Templated content |
| `Checked` | `bool` | `false` | Checked state |
| `CheckedChanged` | `EventCallback<bool>` | - | Checked change callback |
| `Indeterminate` | `bool` | `false` | Indeterminate state |
| `IndeterminateChanged` | `EventCallback<bool>` | - | Indeterminate change callback |
| `Disabled` | `bool` | `false` | Disable item |
| `OnSelect` | `EventCallback` | - | Selection callback |

### DropdownMenuRadioGroup

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | RadioItem components |
| `Value` | `string?` | - | Currently selected value |
| `ValueChanged` | `EventCallback<string?>` | - | Value change callback |
| `AriaLabel` | `string?` | - | Accessible label |

### DropdownMenuRadioItem

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment<DropdownMenuRadioItemContext>?` | - | Templated content |
| `Value` | `string` | **Required** | Value of this radio item |
| `Disabled` | `bool` | `false` | Disable item |
| `OnSelect` | `EventCallback` | - | Selection callback |

### DropdownMenuGroup

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Group content |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### DropdownMenuGroupLabel

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Label content |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### DropdownMenuSeparator

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

## Enums

### DropdownMenuSide

```csharp
public enum DropdownMenuSide
{
    Top,
    Right,
    Bottom,
    Left
}
```

### DropdownMenuAlign

```csharp
public enum DropdownMenuAlign
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
    Close,  // Close menu on Escape key
    Ignore  // Ignore Escape key
}
```

### OutsideClickBehavior

```csharp
public enum OutsideClickBehavior
{
    Close,  // Close menu on outside click
    Ignore  // Ignore outside clicks
}
```

## Examples

### Basic Menu

```razor
<DropdownMenuRoot>
    <DropdownMenuTrigger class="btn btn-primary">
        Options
    </DropdownMenuTrigger>
    <DropdownMenuPortal>
        <DropdownMenuContent class="dropdown-content" SideOffset="4">
            <DropdownMenuItem OnSelect="@(() => HandleAction("new"))">
                New File
            </DropdownMenuItem>
            <DropdownMenuItem OnSelect="@(() => HandleAction("open"))">
                Open File
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem OnSelect="@(() => HandleAction("save"))">
                Save
            </DropdownMenuItem>
            <DropdownMenuItem OnSelect="@(() => HandleAction("save-as"))">
                Save As...
            </DropdownMenuItem>
        </DropdownMenuContent>
    </DropdownMenuPortal>
</DropdownMenuRoot>

@code {
    private void HandleAction(string action)
    {
        Console.WriteLine($"Action: {action}");
    }
}
```

### With Checkbox Items

```razor
@code {
    private bool showToolbar = true;
    private bool showSidebar = false;
    private bool showStatusBar = true;
}

<DropdownMenuRoot>
    <DropdownMenuTrigger class="btn">View</DropdownMenuTrigger>
    <DropdownMenuPortal>
        <DropdownMenuContent class="dropdown-content">
            <DropdownMenuCheckboxItem @bind-Checked="showToolbar">
                @context.Checked ? "✓" : ""
                <span>Show Toolbar</span>
            </DropdownMenuCheckboxItem>
            <DropdownMenuCheckboxItem @bind-Checked="showSidebar">
                @context.Checked ? "✓" : ""
                <span>Show Sidebar</span>
            </DropdownMenuCheckboxItem>
            <DropdownMenuCheckboxItem @bind-Checked="showStatusBar">
                @context.Checked ? "✓" : ""
                <span>Show Status Bar</span>
            </DropdownMenuCheckboxItem>
        </DropdownMenuContent>
    </DropdownMenuPortal>
</DropdownMenuRoot>
```

### With Radio Group

```razor
@code {
    private string? selectedTheme = "system";
}

<DropdownMenuRoot>
    <DropdownMenuTrigger class="btn">Theme</DropdownMenuTrigger>
    <DropdownMenuPortal>
        <DropdownMenuContent class="dropdown-content">
            <DropdownMenuRadioGroup @bind-Value="selectedTheme" AriaLabel="Theme selection">
                <DropdownMenuRadioItem Value="light">
                    @(context.IsSelected ? "●" : "○") Light
                </DropdownMenuRadioItem>
                <DropdownMenuRadioItem Value="dark">
                    @(context.IsSelected ? "●" : "○") Dark
                </DropdownMenuRadioItem>
                <DropdownMenuRadioItem Value="system">
                    @(context.IsSelected ? "●" : "○") System
                </DropdownMenuRadioItem>
            </DropdownMenuRadioGroup>
        </DropdownMenuContent>
    </DropdownMenuPortal>
</DropdownMenuRoot>
```

### Grouped Items

```razor
<DropdownMenuRoot>
    <DropdownMenuTrigger class="btn">Edit</DropdownMenuTrigger>
    <DropdownMenuPortal>
        <DropdownMenuContent class="dropdown-content">
            <DropdownMenuGroup>
                <DropdownMenuGroupLabel class="dropdown-label">
                    Clipboard
                </DropdownMenuGroupLabel>
                <DropdownMenuItem>Cut</DropdownMenuItem>
                <DropdownMenuItem>Copy</DropdownMenuItem>
                <DropdownMenuItem>Paste</DropdownMenuItem>
            </DropdownMenuGroup>
            <DropdownMenuSeparator />
            <DropdownMenuGroup>
                <DropdownMenuGroupLabel class="dropdown-label">
                    Selection
                </DropdownMenuGroupLabel>
                <DropdownMenuItem>Select All</DropdownMenuItem>
                <DropdownMenuItem>Deselect</DropdownMenuItem>
            </DropdownMenuGroup>
        </DropdownMenuContent>
    </DropdownMenuPortal>
</DropdownMenuRoot>
```

### Different Positions

```razor
@* Bottom-aligned (default) *@
<DropdownMenuContent Side="DropdownMenuSide.Bottom" Align="DropdownMenuAlign.Start">
    ...
</DropdownMenuContent>

@* Right side *@
<DropdownMenuContent Side="DropdownMenuSide.Right" Align="DropdownMenuAlign.Start">
    ...
</DropdownMenuContent>

@* Top, centered *@
<DropdownMenuContent Side="DropdownMenuSide.Top" Align="DropdownMenuAlign.Center">
    ...
</DropdownMenuContent>

@* Left, end-aligned *@
<DropdownMenuContent Side="DropdownMenuSide.Left" Align="DropdownMenuAlign.End">
    ...
</DropdownMenuContent>
```

### Controlled Mode

```razor
@code {
    private bool isOpen = false;
}

<DropdownMenuRoot Open="@isOpen" OpenChanged="@(v => isOpen = v)">
    <DropdownMenuTrigger class="btn">
        @(isOpen ? "Close" : "Open") Menu
    </DropdownMenuTrigger>
    <DropdownMenuPortal>
        <DropdownMenuContent class="dropdown-content">
            <DropdownMenuItem>Item 1</DropdownMenuItem>
            <DropdownMenuItem>Item 2</DropdownMenuItem>
        </DropdownMenuContent>
    </DropdownMenuPortal>
</DropdownMenuRoot>

<button @onclick="@(() => isOpen = true)">Open Programmatically</button>
```

### Disabled Items

```razor
<DropdownMenuRoot>
    <DropdownMenuTrigger class="btn">Actions</DropdownMenuTrigger>
    <DropdownMenuPortal>
        <DropdownMenuContent class="dropdown-content">
            <DropdownMenuItem>Edit</DropdownMenuItem>
            <DropdownMenuItem>Duplicate</DropdownMenuItem>
            <DropdownMenuSeparator />
            <DropdownMenuItem Disabled="true">
                Delete (Disabled)
            </DropdownMenuItem>
        </DropdownMenuContent>
    </DropdownMenuPortal>
</DropdownMenuRoot>
```

### With Styling

```razor
<DropdownMenuRoot>
    <DropdownMenuTrigger class="dropdown-trigger">
        <span>Menu</span>
        <span class="dropdown-arrow">▼</span>
    </DropdownMenuTrigger>
    <DropdownMenuPortal>
        <DropdownMenuContent class="dropdown-content" SideOffset="4">
            <DropdownMenuItem class="dropdown-item">New</DropdownMenuItem>
            <DropdownMenuItem class="dropdown-item">Open</DropdownMenuItem>
            <DropdownMenuSeparator class="dropdown-separator" />
            <DropdownMenuItem class="dropdown-item">Settings</DropdownMenuItem>
        </DropdownMenuContent>
    </DropdownMenuPortal>
</DropdownMenuRoot>
```

```css
.dropdown-trigger {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 8px 16px;
    background: white;
    border: 1px solid #ccc;
    border-radius: 4px;
    cursor: pointer;
}

.dropdown-trigger[data-state="open"] {
    background: #f0f0f0;
}

.dropdown-trigger[data-state="open"] .dropdown-arrow {
    transform: rotate(180deg);
}

.dropdown-content {
    min-width: 200px;
    background: white;
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    padding: 4px;
}

.dropdown-item {
    display: flex;
    align-items: center;
    padding: 8px 12px;
    border-radius: 4px;
    cursor: pointer;
}

.dropdown-item[data-highlighted] {
    background: #f0f0f0;
}

.dropdown-item[data-disabled] {
    opacity: 0.5;
    cursor: not-allowed;
}

.dropdown-separator {
    height: 1px;
    background: #e0e0e0;
    margin: 4px 0;
}
```

## Accessibility

### Keyboard Navigation

| Key | Action |
|-----|--------|
| `Enter` / `Space` | Open menu (on trigger) or select item |
| `ArrowDown` | Move focus to next item |
| `ArrowUp` | Move focus to previous item |
| `Home` | Move focus to first item |
| `End` | Move focus to last item |
| `Escape` | Close menu |
| `Tab` | Close menu and move focus |

### ARIA Attributes

- Trigger has `aria-haspopup="menu"` and `aria-expanded`
- Content has `role="menu"`
- Items have `role="menuitem"`
- Checkbox items have `role="menuitemcheckbox"` with `aria-checked`
- Radio items have `role="menuitemradio"` with `aria-checked`
- Groups have `role="group"` with `aria-labelledby`
- Separators have `role="separator"`

### Data Attributes

| Attribute | Values | Description |
|-----------|--------|-------------|
| `data-state` | `"open"` / `"closed"` | Menu open state |
| `data-side` | `"top"` / `"right"` / `"bottom"` / `"left"` | Current placement side |
| `data-align` | `"start"` / `"center"` / `"end"` | Current alignment |
| `data-highlighted` | Present when focused | Item is keyboard-focused |
| `data-disabled` | Present when disabled | Item is disabled |
