# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SummitUI is a Blazor component library focused on WCAG-compliant, fully customizable headless components. The library targets performance across all three Blazor render modes: WebAssembly (WASM), Server-Side Rendering (SSR), and Interactive Server.

**Design Philosophy:**
- Headless components (logic without opinionated styling)
- Build components using .cs files only
- WCAG accessibility compliance built-in
- Customization via attributes
- Prefer C# implementation; use JavaScript only for tasks impossible in C# or for external package dependencies
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
SummitUI/
├── SummitUI.slnx          # Solution file
└── SummitUI/
    ├── SummitUI.csproj    # Razor class library targeting .NET 10.0
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
            "import", "./_content/SummitUI/componentName.js").AsTask());

    public async ValueTask<T> CallMethodAsync<T>(...)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<T>("methodName", ...);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Safe to ignore, JS resources are cleaned up by the browser
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

**Asset Path Convention:** `./_content/SummitUI/{filename}.js`

### Component Guidelines

- Components should be headless (expose structure and behavior, not styling)
- Use semantic HTML elements
- Include ARIA attributes for accessibility
- Support keyboard navigation
- Expose customization through Blazor attributes and `AdditionalAttributes`
- Ensure functionality works across WASM, SSR, and Interactive Server modes

## Testing

This project uses **TUnit** as the testing library with **Playwright** for end-to-end testing.

**Testing Requirements:**
- Every feature must have corresponding Playwright tests
- Tests are located in `SummitUI.Tests.Playwright/`
- Test files follow the naming convention `{ComponentName}AccessibilityTests.cs`

**Running Tests:**
```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test SummitUI.Tests.Playwright/
```

**Test Guidelines:**
- Write Playwright tests that verify component behavior and accessibility
- Test keyboard navigation and ARIA attributes
- Ensure tests cover all three render modes where applicable

# Development Guidelines

## Blazor JS Interop Disposal

When implementing `IAsyncDisposable` in JS interop classes, always wrap the module disposal in a try-catch for `JSDisconnectedException`:

````````csharp
public async ValueTask DisposeAsync()
{
    try
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
    catch (JSDisconnectedException)
    {
        // Safe to ignore, JS resources are cleaned up by the browser
    }
}
````````

**Why:** In Blazor Server, when a user navigates away or closes the browser, the SignalR circuit disconnects before component disposal completes. Any JS interop calls during `DisposeAsync` will throw `JSDisconnectedException`. Since the browser automatically cleans up JS resources when the connection closes, catching and ignoring this exception is safe and expected.

## Asset Management

### No CDN Dependencies

Never use external CDN links for CSS or JavaScript. All assets must be bundled locally:

- **Why:** External CDNs add latency (DNS lookup, TLS handshake), create single points of failure, and prevent offline functionality
- **How:** Install packages via npm and bundle with esbuild into `wwwroot/`

**Example - Adding a third-party library:**
```bash
# Install the package
npm install some-library --save

# Create a bundle script in Scripts/
# Update package.json build step to bundle it
```

### Bundle and Minify

All JavaScript and CSS assets must be bundled and minified for production:

- Use **esbuild** for JavaScript bundling (already configured in `package.json`)
- Use **Tailwind CSS** with `--minify` flag for CSS
- Output files to `wwwroot/` with `.min.js` or `.min.css` extensions
- Reference assets using the `@Assets["filename"]` pattern for fingerprinted URLs

## SummitUI.Docs Projects

### Use Razor Files

In `SummitUI.Docs` and `SummitUI.Docs.Client` projects, prefer `.razor` files over `.cs` files for components:

- Documentation components benefit from Razor's declarative syntax for readability
- Use `.razor` files for page components and UI-focused components
- Use `.cs` files only for services, utilities, and complex logic classes

**Note:** This differs from the main `SummitUI` library, which uses `.cs` files exclusively for headless components.
