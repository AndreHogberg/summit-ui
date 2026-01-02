# Accordion

A vertically stacked set of interactive headings that each reveal an associated section of content.

## Features

- Full keyboard navigation
- Supports single or multiple expanded items
- Controlled and uncontrolled modes
- Collapsible option for single mode
- Horizontal and vertical orientations
- WCAG compliant with proper ARIA attributes

## Import

```razor
@using ArkUI.Components.Accordion
```

## Anatomy

```razor
<AccordionRoot>
    <AccordionItem Value="item-1">
        <AccordionHeader>
            <AccordionTrigger>Title</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            Content goes here...
        </AccordionContent>
    </AccordionItem>
</AccordionRoot>
```

## Sub-components

| Component | Description |
|-----------|-------------|
| `AccordionRoot` | Root container managing accordion state |
| `AccordionItem` | Individual collapsible item container |
| `AccordionHeader` | Semantic heading wrapper (renders `role="heading"`) |
| `AccordionTrigger` | Button that toggles the associated content |
| `AccordionContent` | Collapsible content panel (renders `role="region"`) |

## API Reference

### AccordionRoot

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Child content (AccordionItem components) |
| `Type` | `AccordionType` | `Single` | Single or Multiple items expanded |
| `Value` | `string?` | - | Controlled expanded value (single mode) |
| `Values` | `IReadOnlyList<string>?` | - | Controlled expanded values (multiple mode) |
| `DefaultValue` | `string?` | - | Default expanded value (single, uncontrolled) |
| `DefaultValues` | `IReadOnlyList<string>?` | - | Default expanded values (multiple, uncontrolled) |
| `ValueChanged` | `EventCallback<string?>` | - | Callback when value changes (single mode) |
| `ValuesChanged` | `EventCallback<IReadOnlyList<string>>` | - | Callback when values change (multiple mode) |
| `OnValueChange` | `EventCallback<string?>` | - | Callback on item expansion state change |
| `Orientation` | `AccordionOrientation` | `Vertical` | Affects keyboard navigation |
| `Loop` | `bool` | `true` | Whether keyboard nav loops |
| `Disabled` | `bool` | `false` | Disable entire accordion |
| `Collapsible` | `bool` | `true` | Allow closing last open item (single mode) |

### AccordionItem

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `string` | **Required** | Unique identifier for this item |
| `ChildContent` | `RenderFragment?` | - | Child content |
| `As` | `string` | `"div"` | HTML element to render |
| `Disabled` | `bool` | `false` | Disable this specific item |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### AccordionHeader

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Typically AccordionTrigger |
| `As` | `string` | `"h3"` | HTML element to render |
| `Level` | `int` | `3` | ARIA heading level (1-6) |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### AccordionTrigger

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Trigger label |
| `As` | `string` | `"button"` | HTML element to render |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

### AccordionContent

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | - | Panel content |
| `As` | `string` | `"div"` | HTML element to render |
| `ForceMount` | `bool` | `false` | Always render in DOM (for animations) |
| `AdditionalAttributes` | `IDictionary<string, object>?` | - | Extra HTML attributes |

## Enums

### AccordionType

```csharp
public enum AccordionType
{
    Single,   // Only one item can be expanded at a time
    Multiple  // Multiple items can be expanded simultaneously
}
```

### AccordionOrientation

```csharp
public enum AccordionOrientation
{
    Vertical,   // Navigate with ArrowUp/ArrowDown
    Horizontal  // Navigate with ArrowLeft/ArrowRight
}
```

## Examples

### Basic Usage

```razor
<AccordionRoot DefaultValue="item-1">
    <AccordionItem Value="item-1">
        <AccordionHeader>
            <AccordionTrigger>What is ArkUI?</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            <p>ArkUI is a Blazor component library focused on accessibility.</p>
        </AccordionContent>
    </AccordionItem>
    <AccordionItem Value="item-2">
        <AccordionHeader>
            <AccordionTrigger>Is it accessible?</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            <p>Yes! All components are built with WCAG compliance in mind.</p>
        </AccordionContent>
    </AccordionItem>
    <AccordionItem Value="item-3">
        <AccordionHeader>
            <AccordionTrigger>Can I customize it?</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            <p>Absolutely. Components are headless and fully customizable.</p>
        </AccordionContent>
    </AccordionItem>
</AccordionRoot>
```

### Multiple Expanded Items

```razor
<AccordionRoot Type="AccordionType.Multiple" DefaultValues="@(new[] { "feature-1", "feature-2" })">
    <AccordionItem Value="feature-1">
        <AccordionHeader>
            <AccordionTrigger>Feature 1</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            <p>First feature content...</p>
        </AccordionContent>
    </AccordionItem>
    <AccordionItem Value="feature-2">
        <AccordionHeader>
            <AccordionTrigger>Feature 2</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            <p>Second feature content...</p>
        </AccordionContent>
    </AccordionItem>
    <AccordionItem Value="feature-3">
        <AccordionHeader>
            <AccordionTrigger>Feature 3</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            <p>Third feature content...</p>
        </AccordionContent>
    </AccordionItem>
</AccordionRoot>
```

### Non-Collapsible (Always One Open)

```razor
<AccordionRoot DefaultValue="always-open" Collapsible="false">
    <AccordionItem Value="always-open">
        <AccordionHeader>
            <AccordionTrigger>Always Visible</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            <p>This content cannot be collapsed once opened.</p>
        </AccordionContent>
    </AccordionItem>
    <AccordionItem Value="another">
        <AccordionHeader>
            <AccordionTrigger>Another Section</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            <p>Opening this will close the other.</p>
        </AccordionContent>
    </AccordionItem>
</AccordionRoot>
```

### Controlled Mode

```razor
@code {
    private string? expandedItem = "item-1";
}

<AccordionRoot Value="@expandedItem" ValueChanged="@(v => expandedItem = v)">
    <AccordionItem Value="item-1">
        <AccordionHeader>
            <AccordionTrigger>Section 1</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            <p>Content for section 1</p>
        </AccordionContent>
    </AccordionItem>
    <AccordionItem Value="item-2">
        <AccordionHeader>
            <AccordionTrigger>Section 2</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            <p>Content for section 2</p>
        </AccordionContent>
    </AccordionItem>
</AccordionRoot>

<p>Currently expanded: @(expandedItem ?? "None")</p>
<button @onclick="@(() => expandedItem = "item-2")">Open Section 2</button>
```

### Disabled Items

```razor
<AccordionRoot DefaultValue="item-1">
    <AccordionItem Value="item-1">
        <AccordionHeader>
            <AccordionTrigger>Enabled Section</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            <p>This section works normally.</p>
        </AccordionContent>
    </AccordionItem>
    <AccordionItem Value="item-2" Disabled="true">
        <AccordionHeader>
            <AccordionTrigger>Disabled Section</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            <p>This content cannot be accessed.</p>
        </AccordionContent>
    </AccordionItem>
</AccordionRoot>
```

### Custom Heading Level

```razor
<AccordionRoot DefaultValue="item-1">
    <AccordionItem Value="item-1">
        <AccordionHeader As="h2" Level="2">
            <AccordionTrigger>H2 Heading</AccordionTrigger>
        </AccordionHeader>
        <AccordionContent>
            <p>This uses an h2 element for the header.</p>
        </AccordionContent>
    </AccordionItem>
</AccordionRoot>
```

### With Styling

```razor
<AccordionRoot DefaultValue="item-1" class="accordion">
    <AccordionItem Value="item-1" class="accordion-item">
        <AccordionHeader class="accordion-header">
            <AccordionTrigger class="accordion-trigger">
                <span>Click to expand</span>
                <span class="accordion-icon">+</span>
            </AccordionTrigger>
        </AccordionHeader>
        <AccordionContent class="accordion-content">
            <div class="accordion-content-inner">
                <p>Styled content goes here.</p>
            </div>
        </AccordionContent>
    </AccordionItem>
</AccordionRoot>
```

```css
.accordion {
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    overflow: hidden;
}

.accordion-trigger {
    display: flex;
    justify-content: space-between;
    align-items: center;
    width: 100%;
    padding: 16px;
    background: white;
    border: none;
    cursor: pointer;
}

.accordion-trigger[data-state="open"] {
    background-color: #f5f5f5;
}

.accordion-trigger[data-state="open"] .accordion-icon {
    transform: rotate(45deg);
}

.accordion-trigger[data-disabled] {
    opacity: 0.5;
    cursor: not-allowed;
}

.accordion-content {
    overflow: hidden;
}

.accordion-content[data-state="closed"] {
    display: none;
}

.accordion-content-inner {
    padding: 16px;
}
```

## Accessibility

### Keyboard Navigation

| Key | Action |
|-----|--------|
| `Enter` / `Space` | Toggle the focused accordion item |
| `ArrowDown` | Move focus to the next trigger (vertical orientation) |
| `ArrowUp` | Move focus to the previous trigger (vertical orientation) |
| `ArrowRight` | Move focus to the next trigger (horizontal orientation) |
| `ArrowLeft` | Move focus to the previous trigger (horizontal orientation) |
| `Home` | Move focus to the first trigger |
| `End` | Move focus to the last trigger |

### ARIA Attributes

- `AccordionHeader` renders with `role="heading"` and appropriate `aria-level`
- `AccordionTrigger` renders as a button with `aria-expanded` and `aria-controls`
- `AccordionContent` renders with `role="region"` and `aria-labelledby`

### Data Attributes

| Attribute | Values | Description |
|-----------|--------|-------------|
| `data-state` | `"open"` / `"closed"` | Current expansion state |
| `data-disabled` | Present when disabled | Indicates disabled state |
| `data-orientation` | `"vertical"` / `"horizontal"` | Current orientation |
