# Switch

A control that allows the user to toggle between checked and not checked.

## Features

- Two states: checked and unchecked
- Keyboard navigation (Space/Enter to toggle)
- Controlled and uncontrolled modes
- Support for HTML forms (hidden input)
- WCAG compliant with proper ARIA attributes

## Import

```razor
@using ArkUI.Components.Switch
```

## Anatomy

```razor
<SwitchRoot>
    <SwitchThumb />
</SwitchRoot>
```

## Sub-components

| Component | Description |
|-----------|-------------|
| `SwitchRoot` | The root switch component containing the button element. |
| `SwitchThumb` | The thumb that visually indicates the checked state. |

## API Reference

### SwitchRoot

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Checked` | `bool?` | - | Controlled checked state. When provided, the component operates in controlled mode. |
| `DefaultChecked` | `bool` | `false` | Default checked state for uncontrolled mode. |
| `CheckedChanged` | `EventCallback<bool>` | - | Callback when checked state changes. |
| `Disabled` | `bool` | `false` | When true, prevents the user from interacting with the switch. |
| `Required` | `bool` | `false` | When true, indicates that the user must check the switch before the owning form can be submitted. |
| `Name` | `string?` | - | The name of the switch. Submitted with its owning form as part of a name/value pair. |
| `Value` | `string?` | `"on"` | The value given as data when submitted with a `name`. |
| `ChildContent` | `RenderFragment?` | - | The content of the switch, usually the thumb. |

### SwitchThumb

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Optional content to render inside the thumb. |

## Examples

### Basic Usage

```razor
<SwitchRoot DefaultChecked="true">
    <SwitchThumb />
</SwitchRoot>
```

### With Label

It's recommended to associate the switch with a label for better accessibility.

```razor
<label style="display: flex; gap: 0.5rem; align-items: center;">
    <SwitchRoot id="airplane-mode">
        <SwitchThumb />
    </SwitchRoot>
    Airplane mode
</label>
```

### Controlled Mode

```razor
@code {
    private bool isChecked = true;
}

<SwitchRoot @bind-Checked="isChecked">
    <SwitchThumb />
</SwitchRoot>

<p>Switch is @(isChecked ? "on" : "off")</p>
```

### Disabled State

```razor
<SwitchRoot Disabled="true">
    <SwitchThumb />
</SwitchRoot>
```

### Form Integration

```razor
<EditForm Model="@model" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <label>
        <SwitchRoot @bind-Checked="model.EnableNotifications" Name="notifications">
            <SwitchThumb />
        </SwitchRoot>
        Enable Notifications
    </label>
    
    <button type="submit">Submit</button>
</EditForm>
```

## Accessibility

### Keyboard Navigation

| Key | Action |
|-----|--------|
| `Space` | Toggles the switch state. |
| `Enter` | Toggles the switch state. |

### Data Attributes

| Attribute | Values | Description |
|-----------|--------|-------------|
| `data-state` | `"checked"` / `"unchecked"` | Current state of the switch. |
| `data-disabled` | Present when disabled | Indicates disabled state. |

### ARIA Attributes

- `role="switch"` is set on the root element.
- `aria-checked` reflects the current state (`true` or `false`).
- `aria-disabled` is set when the switch is disabled.
