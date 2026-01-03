# Checkbox

A control that allows the user to toggle between checked and not checked.

## Features

- Three states: checked, unchecked, and indeterminate
- Keyboard navigation (Space to toggle)
- Controlled and uncontrolled modes
- Support for HTML forms (hidden input)
- Grouping capabilities via `CheckboxGroup`
- WCAG compliant with proper ARIA attributes

## Import

```razor
@using SummitUI.Components.Checkbox
```

## Anatomy

```razor
<CheckboxRoot>
    <CheckboxIndicator>
        <span>✓</span>
    </CheckboxIndicator>
</CheckboxRoot>
```

## Sub-components

| Component | Description |
|-----------|-------------|
| `CheckboxRoot` | The root checkbox component. |
| `CheckboxIndicator` | Renders when the checkbox is in a checked or indeterminate state. |
| `CheckboxGroup` | A wrapper for managing a group of checkboxes. |
| `CheckboxGroupLabel` | Accessible label for the checkbox group. |

## API Reference

### CheckboxRoot

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Checked` | `bool?` | - | Controlled checked state. |
| `DefaultChecked` | `bool` | `false` | Default checked state for uncontrolled mode. |
| `CheckedChanged` | `EventCallback<bool>` | - | Callback when checked state changes. |
| `Indeterminate` | `bool` | `false` | Controlled indeterminate state. |
| `IndeterminateChanged` | `EventCallback<bool>` | - | Callback when indeterminate state changes. |
| `Disabled` | `bool` | `false` | When true, prevents the user from interacting with the checkbox. |
| `Required` | `bool` | `false` | When true, indicates that the user must check the checkbox before the owning form can be submitted. |
| `Name` | `string?` | - | The name of the checkbox. Submitted with its owning form as part of a name/value pair. |
| `Value` | `string?` | `"on"` | The value given as data when submitted with a `name`. |
| `ChildContent` | `RenderFragment?` | - | The content of the checkbox, usually the indicator. |

### CheckboxIndicator

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ForceMount` | `bool` | `false` | Used to force mounting when more control is needed. Useful when controlling animation with React animation libraries. |
| `ChildContent` | `RenderFragment?` | - | The content to render when checked or indeterminate. |

### CheckboxGroup

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Values` | `IReadOnlyList<string>?` | - | Controlled values of the checked items. |
| `DefaultValues` | `IReadOnlyList<string>?` | - | Default values for uncontrolled mode. |
| `ValuesChanged` | `EventCallback<IReadOnlyList<string>>` | - | Callback when values change. |
| `Disabled` | `bool` | `false` | Disable entire group. |
| `Name` | `string?` | - | The form name for hidden inputs. |
| `ChildContent` | `RenderFragment?` | - | The content containing CheckboxRoot components. |

## Examples

### Basic Usage

```razor
<CheckboxRoot DefaultChecked="true">
    <CheckboxIndicator>
        <span>✓</span>
    </CheckboxIndicator>
</CheckboxRoot>
```

### With Label

It's recommended to wrap the checkbox in a label for better accessibility and click handling.

```razor
<label style="display: flex; gap: 0.5rem; align-items: center;">
    <CheckboxRoot id="c1">
        <CheckboxIndicator>
            <span>✓</span>
        </CheckboxIndicator>
    </CheckboxRoot>
    Accept terms and conditions
</label>
```

### Controlled Mode

```razor
@code {
    private bool isChecked = true;
}

<CheckboxRoot @bind-Checked="isChecked">
    <CheckboxIndicator>
        <span>✓</span>
    </CheckboxIndicator>
</CheckboxRoot>
```

### Indeterminate State

```razor
@code {
    private bool isIndeterminate = true;
}

<CheckboxRoot @bind-Indeterminate="isIndeterminate">
    <CheckboxIndicator>
        @if (isIndeterminate)
        {
            <span>-</span>
        }
        else
        {
            <span>✓</span>
        }
    </CheckboxIndicator>
</CheckboxRoot>
```

### Checkbox Group

```razor
@code {
    private IReadOnlyList<string> values = new[] { "apple" };
}

<CheckboxGroup @bind-Values="values">
    <CheckboxGroupLabel>Favorite Fruits</CheckboxGroupLabel>
    
    <label>
        <CheckboxRoot Value="apple">
            <CheckboxIndicator>✓</CheckboxIndicator>
        </CheckboxRoot>
        Apple
    </label>
    
    <label>
        <CheckboxRoot Value="banana">
            <CheckboxIndicator>✓</CheckboxIndicator>
        </CheckboxRoot>
        Banana
    </label>
</CheckboxGroup>
```

### Form Integration

```razor
<EditForm Model="@model" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <label>
        <CheckboxRoot @bind-Checked="model.AcceptTerms" Name="acceptTerms">
            <CheckboxIndicator>✓</CheckboxIndicator>
        </CheckboxRoot>
        Accept Terms
    </label>
    <ValidationMessage For="@(() => model.AcceptTerms)" />
    
    <button type="submit">Submit</button>
</EditForm>
```

## Accessibility

### Keyboard Navigation

| Key | Action |
|-----|--------|
| `Space` | Toggles the checkbox state. |
| `Enter` | Default behavior is prevented to adhere to WAI-ARIA patterns. |

### Data Attributes

| Attribute | Values | Description |
|-----------|--------|-------------|
| `data-state` | `"checked"` / `"unchecked"` / `"indeterminate"` | Current state of the checkbox. |
| `data-disabled` | Present when disabled | Indicates disabled state. |

### ARIA Attributes

- `role="checkbox"` is set on the root element.
- `aria-checked` reflects the current state (`true`, `false`, or `mixed`).
- `aria-disabled` is set when the checkbox is disabled.
- `aria-required` is set when the checkbox is required.
