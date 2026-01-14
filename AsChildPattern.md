# AsChild Pattern for Blazor

This document describes the implementation of the React-style `asChild` pattern (popularized by Radix UI) in Blazor.

## Overview

The `asChild` pattern allows a component to pass its logic, attributes, and event handlers to a child element instead of rendering its own wrapper element. This enables greater composability and flexibility.

**React example:**
```jsx
<Button asChild>
  <a href="/">Click me</a>
</Button>
```

**Blazor equivalent:**
```razor
<Button AsChild OnClick="HandleClick">
    <a href="/" @attributes="context.Attrs">Click me</a>
</Button>
```

## Files Created

```
BlazorApp2/
├── Components/
│   └── Primitives/
│       ├── AsChildContext.cs    # Context class for passing attributes
│       └── Button.razor         # Button component with AsChild support
└── _Imports.razor               # Updated with new namespace
```

## Implementation

### AsChildContext.cs

```csharp
namespace BlazorApp2.Components.Primitives;

public class AsChildContext
{
    /// <summary>
    /// Dictionary of attributes to be applied to the child element using @attributes directive.
    /// </summary>
    public Dictionary<string, object> Attrs { get; init; } = new();
}
```

### Button.razor

```razor
@if (AsChild && ChildContent is not null)
{
    @ChildContent(new AsChildContext { Attrs = GetMergedAttributes() })
}
else
{
    <button @attributes="GetMergedAttributes()">
        @if (DefaultContent is not null)
        {
            @DefaultContent
        }
    </button>
}

@code {
    [Parameter] public bool AsChild { get; set; }
    [Parameter] public RenderFragment<AsChildContext>? ChildContent { get; set; }
    [Parameter] public RenderFragment? DefaultContent { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string Variant { get; set; } = "primary";
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private Dictionary<string, object> GetMergedAttributes()
    {
        var attrs = new Dictionary<string, object>();

        // Build CSS class string
        var cssClass = $"btn btn-{Variant}";
        if (!string.IsNullOrWhiteSpace(Class))
        {
            cssClass = $"{cssClass} {Class}";
        }
        attrs["class"] = cssClass;

        // Add disabled attribute
        if (Disabled)
        {
            attrs["disabled"] = true;
            attrs["aria-disabled"] = "true";
        }

        // Add click handler
        if (OnClick.HasDelegate)
        {
            attrs["onclick"] = OnClick;
        }

        // Merge additional attributes
        if (AdditionalAttributes is not null)
        {
            foreach (var (key, value) in AdditionalAttributes)
            {
                attrs[key] = value;
            }
        }

        return attrs;
    }
}
```

## Usage Examples

### Default Mode (renders a `<button>`)

```razor
<Button OnClick="HandleClick" Variant="primary">
    <DefaultContent>Click me</DefaultContent>
</Button>
```

Renders:
```html
<button class="btn btn-primary" onclick="...">Click me</button>
```

### AsChild Mode (renders your custom element)

```razor
<Button AsChild OnClick="HandleClick" Variant="success">
    <a href="/" @attributes="context.Attrs">Go Home</a>
</Button>
```

Renders:
```html
<a href="/" class="btn btn-success" onclick="...">Go Home</a>
```

### Disabled State

```razor
<Button AsChild Disabled="true" Variant="secondary">
    <ChildContent Context="ctx">
        <a href="/" @attributes="ctx.Attrs">Disabled link-button</a>
    </ChildContent>
</Button>
```

Renders:
```html
<a href="/" class="btn btn-secondary" disabled aria-disabled="true">Disabled link-button</a>
```

### With Additional Attributes

```razor
<Button AsChild OnClick="HandleClick" data-testid="my-button" aria-label="Custom label">
    <ChildContent Context="ctx">
        <span @attributes="ctx.Attrs">Custom span button</span>
    </ChildContent>
</Button>
```

## How It Works

1. **AsChild = false (default)**: The component renders its own `<button>` element with all attributes applied
2. **AsChild = true**: The component does NOT render any element. Instead, it passes the merged attributes to `ChildContent` via `AsChildContext`
3. The consumer applies attributes using Blazor's `@attributes="ctx.Attrs"` directive

## Key Differences from React

| Aspect | React (Radix) | Blazor |
|--------|---------------|--------|
| Attribute passing | Automatic via cloneElement | Manual via `@attributes="ctx.Attrs"` |
| Child content | Single child element | `RenderFragment<AsChildContext>` |
| Context access | Props spread automatically | Explicit `Context="ctx"` declaration |

## Extending the Pattern

To create other components with `AsChild` support, follow the same pattern:

1. Add `[Parameter] public bool AsChild { get; set; }`
2. Add `[Parameter] public RenderFragment<AsChildContext>? ChildContent { get; set; }`
3. Create a `GetMergedAttributes()` method with your component's specific attributes
4. Conditionally render based on `AsChild` value
