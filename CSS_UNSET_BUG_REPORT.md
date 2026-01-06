# Bug Report: CSS `all: unset` Interfering with Blazor Component Rendering

## Summary
During the refactoring of Playwright tests for the `Tabs` component, we discovered that applying the CSS property `all: unset` to classes used by Blazor components can cause them to fail to render entirely in the browser.

## Symptoms
- Components appear as empty Blazor placeholders (`<!--!-->`) in the DOM.
- Playwright tests fail with `TimeoutException` or `Locator not found` because the expected HTML is never rendered.
- The issue is intermittent but highly correlated with Interactive WebAssembly rendering mode and the use of the `all: unset` property.

## Technical Details
In the test pages (e.g., `TabsBasicTests.razor`), we initially used `all: unset` to provide a clean slate for the headless `TabsTrigger` components (which render as buttons by default):

```css
/* problematic CSS */
.tabs-trigger {
    all: unset; 
    padding: 8px 16px;
    cursor: pointer;
}
```

When this was active, the browser-rendered source showed:
```html
<div class="tabs-list" data-testid="basic-tabs-list">
    <!--!-->
    <!--!-->
    <!--!-->
</div>
```
Instead of the expected `<button>` elements.

## Root Cause Analysis
`all: unset` resets every CSS property to its initial or inherited value. While the exact interaction with Blazor's rendering engine isn't fully clear, it likely interferes with how the browser handles the element during the hydration phase or how Blazor's JavaScript interop calculates layout/visibility for components. It may also reset properties that are critical for the browser to treat the element as a valid target for Blazor's DOM manipulations.

## Resolution
Avoid using `all: unset` for resetting component styles. Instead, use targeted resets for the specific properties you want to clear.

**Corrected Pattern:**
```css
.tabs-trigger {
    border: none;
    background: transparent;
    padding: 8px 16px;
    cursor: pointer;
    /* Add other specific resets as needed */
}
```

## Note on Boolean Attributes
Additionally, it was noted that Blazor renders boolean attributes (like `data-summit-tabs-trigger="true"`) as empty strings in the DOM (e.g., `data-summit-tabs-trigger=""`). Playwright assertions should be updated accordingly:

```csharp
// Use this
await Expect(trigger).ToHaveAttributeAsync("data-summit-tabs-trigger", "");

// Instead of this
await Expect(trigger).ToHaveAttributeAsync("data-summit-tabs-trigger", "true");
```
