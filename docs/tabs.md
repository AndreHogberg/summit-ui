# Tabs

A set of layered sections of content, known as tab panels, that are displayed one at a time.

## Features

- Full keyboard navigation
- Horizontal and vertical orientations
- Auto and manual activation modes
- Controlled and uncontrolled modes
- Disabled tabs support
- WCAG compliant with proper ARIA attributes

## Import

```razor
@using ArkUI.Components.Tabs
```

## Anatomy

```razor
<TabsRoot DefaultValue="tab1">
    <TabsList>
        <TabsTrigger Value="tab1">Tab 1</TabsTrigger>
        <TabsTrigger Value="tab2">Tab 2</TabsTrigger>
    </TabsList>
    <TabsContent Value="tab1">Content for Tab 1</TabsContent>
    <TabsContent Value="tab2">Content for Tab 2</TabsContent>
</TabsRoot>
```

## Sub-components

| Component | Description |
|-----------|-------------|
| `TabsRoot` | Root container managing tabs state |
| `TabsList` | Container for tab triggers (tablist role) |
| `TabsTrigger` | Individual tab button (tab role) |
| `TabsContent` | Tab content panel (tabpanel role) |

## API Reference

### TabsRoot

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | TabsList and TabsContent components |
| `Value` | `string?` | - | Controlled active tab value |
| `DefaultValue` | `string?` | - | Default active tab (uncontrolled) |
| `ValueChanged` | `EventCallback<string?>` | - | Value change callback |
| `OnValueChange` | `EventCallback<string?>` | - | Tab activation callback |
| `Orientation` | `TabsOrientation` | `Horizontal` | Affects keyboard navigation |
| `ActivationMode` | `TabsActivationMode` | `Auto` | Auto (on focus) or Manual (on Enter/Space) |
| `Loop` | `bool` | `true` | Whether keyboard nav loops |

### TabsList

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | TabsTrigger components |
| `As` | `string` | `"div"` | HTML element |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### TabsTrigger

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `string` | **Required** | Unique tab identifier |
| `ChildContent` | `RenderFragment?` | - | Tab label |
| `As` | `string` | `"button"` | HTML element |
| `Disabled` | `bool` | `false` | Disable this tab |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### TabsContent

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `string` | **Required** | Matching TabsTrigger value |
| `ChildContent` | `RenderFragment?` | - | Panel content |
| `As` | `string` | `"div"` | HTML element |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

## Enums

### TabsOrientation

```csharp
public enum TabsOrientation
{
    Horizontal,  // Navigate with ArrowLeft/ArrowRight
    Vertical     // Navigate with ArrowUp/ArrowDown
}
```

### TabsActivationMode

```csharp
public enum TabsActivationMode
{
    Auto,   // Tab activates immediately when focused via keyboard
    Manual  // Requires explicit Enter/Space to activate
}
```

## Examples

### Basic Tabs

```razor
<TabsRoot DefaultValue="account">
    <TabsList class="tabs-list">
        <TabsTrigger Value="account" class="tabs-trigger">Account</TabsTrigger>
        <TabsTrigger Value="password" class="tabs-trigger">Password</TabsTrigger>
        <TabsTrigger Value="notifications" class="tabs-trigger">Notifications</TabsTrigger>
    </TabsList>
    
    <TabsContent Value="account" class="tabs-content">
        <h3>Account Settings</h3>
        <p>Manage your account information here.</p>
    </TabsContent>
    <TabsContent Value="password" class="tabs-content">
        <h3>Password Settings</h3>
        <p>Change your password and security options.</p>
    </TabsContent>
    <TabsContent Value="notifications" class="tabs-content">
        <h3>Notification Preferences</h3>
        <p>Configure how you receive notifications.</p>
    </TabsContent>
</TabsRoot>
```

### Vertical Orientation

```razor
<TabsRoot DefaultValue="general" Orientation="TabsOrientation.Vertical">
    <div class="tabs-container-vertical">
        <TabsList class="tabs-list-vertical">
            <TabsTrigger Value="general" class="tabs-trigger">General</TabsTrigger>
            <TabsTrigger Value="privacy" class="tabs-trigger">Privacy</TabsTrigger>
            <TabsTrigger Value="security" class="tabs-trigger">Security</TabsTrigger>
            <TabsTrigger Value="advanced" class="tabs-trigger">Advanced</TabsTrigger>
        </TabsList>
        
        <div class="tabs-content-container">
            <TabsContent Value="general" class="tabs-content">
                General settings content...
            </TabsContent>
            <TabsContent Value="privacy" class="tabs-content">
                Privacy settings content...
            </TabsContent>
            <TabsContent Value="security" class="tabs-content">
                Security settings content...
            </TabsContent>
            <TabsContent Value="advanced" class="tabs-content">
                Advanced settings content...
            </TabsContent>
        </div>
    </div>
</TabsRoot>
```

### Manual Activation

```razor
<TabsRoot DefaultValue="tab1" ActivationMode="TabsActivationMode.Manual">
    <TabsList class="tabs-list">
        <TabsTrigger Value="tab1" class="tabs-trigger">Tab 1</TabsTrigger>
        <TabsTrigger Value="tab2" class="tabs-trigger">Tab 2</TabsTrigger>
        <TabsTrigger Value="tab3" class="tabs-trigger">Tab 3</TabsTrigger>
    </TabsList>
    
    <TabsContent Value="tab1" class="tabs-content">
        <p>In manual mode, use Enter or Space to activate a tab after focusing it.</p>
    </TabsContent>
    <TabsContent Value="tab2" class="tabs-content">
        <p>Tab 2 content</p>
    </TabsContent>
    <TabsContent Value="tab3" class="tabs-content">
        <p>Tab 3 content</p>
    </TabsContent>
</TabsRoot>
```

### Disabled Tabs

```razor
<TabsRoot DefaultValue="active1">
    <TabsList class="tabs-list">
        <TabsTrigger Value="active1" class="tabs-trigger">Active Tab</TabsTrigger>
        <TabsTrigger Value="disabled" class="tabs-trigger" Disabled="true">
            Disabled Tab
        </TabsTrigger>
        <TabsTrigger Value="active2" class="tabs-trigger">Another Active</TabsTrigger>
    </TabsList>
    
    <TabsContent Value="active1" class="tabs-content">
        <p>This tab is accessible.</p>
    </TabsContent>
    <TabsContent Value="disabled" class="tabs-content">
        <p>This content is not accessible via the disabled tab.</p>
    </TabsContent>
    <TabsContent Value="active2" class="tabs-content">
        <p>This tab is also accessible.</p>
    </TabsContent>
</TabsRoot>
```

### Controlled Mode

```razor
@code {
    private string activeTab = "overview";
}

<TabsRoot Value="@activeTab" ValueChanged="@(v => activeTab = v)">
    <TabsList class="tabs-list">
        <TabsTrigger Value="overview" class="tabs-trigger">Overview</TabsTrigger>
        <TabsTrigger Value="details" class="tabs-trigger">Details</TabsTrigger>
        <TabsTrigger Value="history" class="tabs-trigger">History</TabsTrigger>
    </TabsList>
    
    <TabsContent Value="overview" class="tabs-content">
        Overview content...
    </TabsContent>
    <TabsContent Value="details" class="tabs-content">
        Details content...
    </TabsContent>
    <TabsContent Value="history" class="tabs-content">
        History content...
    </TabsContent>
</TabsRoot>

<p>Active tab: @activeTab</p>

<div class="button-group">
    <button @onclick="@(() => activeTab = "overview")">Go to Overview</button>
    <button @onclick="@(() => activeTab = "details")">Go to Details</button>
    <button @onclick="@(() => activeTab = "history")">Go to History</button>
</div>
```

### With Event Callbacks

```razor
@code {
    private string? previousTab;
    private string? currentTab;
    
    private void HandleTabChange(string? newTab)
    {
        previousTab = currentTab;
        currentTab = newTab;
        Console.WriteLine($"Tab changed from {previousTab} to {currentTab}");
    }
}

<TabsRoot DefaultValue="tab1" OnValueChange="HandleTabChange">
    <TabsList class="tabs-list">
        <TabsTrigger Value="tab1" class="tabs-trigger">Tab 1</TabsTrigger>
        <TabsTrigger Value="tab2" class="tabs-trigger">Tab 2</TabsTrigger>
    </TabsList>
    
    <TabsContent Value="tab1" class="tabs-content">Tab 1 content</TabsContent>
    <TabsContent Value="tab2" class="tabs-content">Tab 2 content</TabsContent>
</TabsRoot>
```

### Without Looping

```razor
<TabsRoot DefaultValue="first" Loop="false">
    <TabsList class="tabs-list">
        <TabsTrigger Value="first" class="tabs-trigger">First</TabsTrigger>
        <TabsTrigger Value="middle" class="tabs-trigger">Middle</TabsTrigger>
        <TabsTrigger Value="last" class="tabs-trigger">Last</TabsTrigger>
    </TabsList>
    
    <TabsContent Value="first" class="tabs-content">
        <p>Keyboard navigation stops at the first and last tabs (no looping).</p>
    </TabsContent>
    <TabsContent Value="middle" class="tabs-content">
        <p>Middle tab content</p>
    </TabsContent>
    <TabsContent Value="last" class="tabs-content">
        <p>Last tab content</p>
    </TabsContent>
</TabsRoot>
```

### With Icons

```razor
<TabsRoot DefaultValue="home">
    <TabsList class="tabs-list">
        <TabsTrigger Value="home" class="tabs-trigger">
            <span class="tab-icon">🏠</span>
            <span>Home</span>
        </TabsTrigger>
        <TabsTrigger Value="profile" class="tabs-trigger">
            <span class="tab-icon">👤</span>
            <span>Profile</span>
        </TabsTrigger>
        <TabsTrigger Value="settings" class="tabs-trigger">
            <span class="tab-icon">⚙️</span>
            <span>Settings</span>
        </TabsTrigger>
    </TabsList>
    
    <TabsContent Value="home" class="tabs-content">Home content</TabsContent>
    <TabsContent Value="profile" class="tabs-content">Profile content</TabsContent>
    <TabsContent Value="settings" class="tabs-content">Settings content</TabsContent>
</TabsRoot>
```

### With Styling

```razor
<TabsRoot DefaultValue="tab1" class="tabs">
    <TabsList class="tabs-list">
        <TabsTrigger Value="tab1" class="tabs-trigger">Dashboard</TabsTrigger>
        <TabsTrigger Value="tab2" class="tabs-trigger">Analytics</TabsTrigger>
        <TabsTrigger Value="tab3" class="tabs-trigger">Reports</TabsTrigger>
    </TabsList>
    
    <TabsContent Value="tab1" class="tabs-content">
        <h3>Dashboard</h3>
        <p>Your dashboard overview...</p>
    </TabsContent>
    <TabsContent Value="tab2" class="tabs-content">
        <h3>Analytics</h3>
        <p>Detailed analytics...</p>
    </TabsContent>
    <TabsContent Value="tab3" class="tabs-content">
        <h3>Reports</h3>
        <p>Generated reports...</p>
    </TabsContent>
</TabsRoot>
```

```css
.tabs {
    width: 100%;
}

.tabs-list {
    display: flex;
    border-bottom: 1px solid #e0e0e0;
    gap: 4px;
}

.tabs-list[data-orientation="vertical"] {
    flex-direction: column;
    border-bottom: none;
    border-right: 1px solid #e0e0e0;
}

.tabs-trigger {
    padding: 12px 24px;
    background: transparent;
    border: none;
    border-bottom: 2px solid transparent;
    cursor: pointer;
    font-weight: 500;
    color: #666;
    transition: all 0.2s;
}

.tabs-trigger:hover {
    color: #333;
    background: #f5f5f5;
}

.tabs-trigger[data-state="active"] {
    color: #0066cc;
    border-bottom-color: #0066cc;
}

.tabs-trigger[data-disabled] {
    opacity: 0.5;
    cursor: not-allowed;
}

.tabs-trigger:focus-visible {
    outline: 2px solid #0066cc;
    outline-offset: -2px;
}

.tabs-content {
    padding: 24px;
}

.tabs-content[data-state="inactive"] {
    display: none;
}

/* Vertical tabs layout */
.tabs-container-vertical {
    display: flex;
    gap: 24px;
}

.tabs-list-vertical {
    flex-direction: column;
    min-width: 200px;
}

.tabs-list-vertical .tabs-trigger {
    text-align: left;
    border-bottom: none;
    border-left: 2px solid transparent;
}

.tabs-list-vertical .tabs-trigger[data-state="active"] {
    border-left-color: #0066cc;
}
```

## Accessibility

### Keyboard Navigation

#### Horizontal Orientation

| Key | Action |
|-----|--------|
| `ArrowRight` | Move focus to next tab |
| `ArrowLeft` | Move focus to previous tab |
| `Home` | Move focus to first tab |
| `End` | Move focus to last tab |
| `Enter` / `Space` | Activate focused tab (manual mode) |

#### Vertical Orientation

| Key | Action |
|-----|--------|
| `ArrowDown` | Move focus to next tab |
| `ArrowUp` | Move focus to previous tab |
| `Home` | Move focus to first tab |
| `End` | Move focus to last tab |
| `Enter` / `Space` | Activate focused tab (manual mode) |

### Activation Modes

- **Auto mode**: Tabs activate immediately when focused via keyboard navigation. This is the default and recommended for most use cases.
- **Manual mode**: Tabs require an explicit `Enter` or `Space` keypress to activate. Use this when tab content is expensive to load or when you want users to confirm their selection.

### ARIA Attributes

- `TabsList` has `role="tablist"` with `aria-orientation`
- `TabsTrigger` has `role="tab"` with `aria-selected` and `aria-controls`
- `TabsContent` has `role="tabpanel"` with `aria-labelledby`
- Disabled tabs have `aria-disabled`

### Data Attributes

| Attribute | Values | Description |
|-----------|--------|-------------|
| `data-state` | `"active"` / `"inactive"` | Tab/panel activation state |
| `data-disabled` | Present when disabled | Tab is disabled |
| `data-orientation` | `"horizontal"` / `"vertical"` | Current orientation |
