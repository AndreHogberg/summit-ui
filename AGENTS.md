# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ArkUI is a Blazor component library focused on WCAG-compliant, fully customizable headless components. The library targets performance across all three Blazor render modes: WebAssembly (WASM), Server-Side Rendering (SSR), and Interactive Server.

**Design Philosophy:**
- Headless components (logic without opinionated styling)
- WCAG accessibility compliance built-in
- Customization via attributes
- Maximize JavaScript for cross-render-mode compatibility
- Expose minimal, clean APIs to consumers

## Build Commands

```bash
# Build the library
dotnet build

# Clean build artifacts
dotnet clean

# Create NuGet package
dotnet pack
```

## Architecture

### Project Structure

```
ArkUI/
├── ArkUI.slnx          # Solution file
└── ArkUI/
    ├── ArkUI.csproj    # Razor class library targeting .NET 10.0
    ├── _Imports.razor  # Global Razor imports
    └── wwwroot/        # Static assets (JS modules, CSS, images)
```

### JavaScript Interop Pattern

The library uses lazy-loaded ES6 modules for JavaScript interop. This pattern ensures:
- Modules load only when needed
- Single module instance is cached and reused
- Proper cleanup via `IAsyncDisposable`

**C# Interop Class Pattern:**
```csharp
public class ComponentJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask =
        new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/ArkUI/componentName.js").AsTask());

    public async ValueTask<T> CallMethodAsync<T>(...)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<T>("methodName", ...);
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
```

**JS Module Pattern (ES6):**
```javascript
// wwwroot/componentName.js
export function methodName(params) {
    // Implementation
}
```

**Asset Path Convention:** `./_content/ArkUI/{filename}.js`

### Component Guidelines

- Components should be headless (expose structure and behavior, not styling)
- Use semantic HTML elements
- Include ARIA attributes for accessibility
- Support keyboard navigation
- Expose customization through Blazor attributes and `AdditionalAttributes`
- Ensure functionality works across WASM, SSR, and Interactive Server modes
