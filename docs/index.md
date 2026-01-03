# SummitUI Component Library

SummitUI is a Blazor component library focused on WCAG-compliant, fully customizable headless components. The library targets performance across all three Blazor render modes: WebAssembly (WASM), Server-Side Rendering (SSR), and Interactive Server.

## Design Philosophy

- **Headless components** - Logic without opinionated styling
- **WCAG accessibility compliance** - Built-in accessibility features
- **Customization via attributes** - Full control over styling and behavior
- **Cross-render-mode compatibility** - Works with WASM, SSR, and Interactive Server

## Installation

```bash
dotnet add package SummitUI
```

## Components

| Component | Description |
|-----------|-------------|
| [Accordion](./accordion.md) | Collapsible content sections with keyboard navigation |
| [DropdownMenu](./dropdownmenu.md) | Floating menu with items, checkboxes, and radio groups |
| [Popover](./popover.md) | Floating content panels with flexible positioning |
| [Select](./select.md) | Custom dropdown select with full accessibility |
| [Tabs](./tabs.md) | Tabbed interface with keyboard navigation |
| [FocusTrap](./focustrap.md) | Utility to trap keyboard focus within a container |

## Common Patterns

### Controlled vs Uncontrolled Mode

All components support both controlled and uncontrolled modes:

```razor
@* Uncontrolled (internal state) *@
<AccordionRoot DefaultValue="item-1">...</AccordionRoot>

@* Controlled (external state) *@
<AccordionRoot Value="@expandedItem" ValueChanged="@(v => expandedItem = v)">...</AccordionRoot>
```

### Data Attributes for Styling

All components expose data attributes for CSS styling hooks:

- `data-state="open"` / `data-state="closed"`
- `data-state="active"` / `data-state="inactive"`
- `data-state="checked"` / `data-state="unchecked"` / `data-state="indeterminate"`
- `data-disabled`
- `data-highlighted`
- `data-invalid`
- `data-placeholder`

Example CSS:

```css
.accordion-trigger[data-state="open"] {
    background-color: #f0f0f0;
}

.accordion-trigger[data-disabled] {
    opacity: 0.5;
    cursor: not-allowed;
}
```

### Custom Element Rendering

Most components support rendering as different HTML elements via the `As` parameter:

```razor
<AccordionHeader As="h2" Level="2">...</AccordionHeader>
<PopoverTrigger As="div">...</PopoverTrigger>
```

### Additional Attributes

All components support spreading custom HTML attributes:

```razor
<AccordionItem Value="item-1" class="my-class" data-testid="accordion-1">
    ...
</AccordionItem>
```

## Accessibility

SummitUI components are built with accessibility as a core principle:

- Proper ARIA roles and attributes
- Keyboard navigation support
- Focus management
- Screen reader compatibility

Each component documentation includes specific accessibility notes and keyboard shortcuts.
