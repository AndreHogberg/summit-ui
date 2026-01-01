using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArkUI.Interop;

/// <summary>
/// JavaScript interop service for accordion keyboard navigation and animations.
/// </summary>
public sealed class AccordionJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/ArkUI/accordion.js").AsTask());

    /// <summary>
    /// Initialize keyboard navigation for the accordion.
    /// </summary>
    /// <typeparam name="T">The type of the .NET object reference (must have JSInvokable methods).</typeparam>
    /// <param name="rootElement">Reference to the accordion root element.</param>
    /// <param name="dotNetRef">Reference to the .NET object for callbacks.</param>
    /// <param name="options">Navigation options.</param>
    public async ValueTask InitializeAsync<T>(
        ElementReference rootElement,
        DotNetObjectReference<T> dotNetRef,
        AccordionNavigationOptions options) where T : class
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("initializeAccordion", rootElement, dotNetRef, options);
    }

    /// <summary>
    /// Cleanup keyboard event listeners.
    /// </summary>
    /// <param name="rootElement">Reference to the accordion root element.</param>
    public async ValueTask DestroyAsync(ElementReference rootElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("destroyAccordion", rootElement);
    }

    /// <summary>
    /// Focus a specific trigger by value.
    /// </summary>
    /// <param name="rootElement">Reference to the accordion root element.</param>
    /// <param name="value">Value of the trigger to focus.</param>
    public async ValueTask FocusTriggerAsync(ElementReference rootElement, string value)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("focusTrigger", rootElement, value);
    }

    /// <summary>
    /// Set CSS variable for content height (for animations).
    /// </summary>
    /// <param name="contentElement">Reference to the content element.</param>
    public async ValueTask SetContentHeightAsync(ElementReference contentElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("setContentHeight", contentElement);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}

/// <summary>
/// Options for accordion keyboard navigation.
/// </summary>
public sealed class AccordionNavigationOptions
{
    /// <summary>
    /// Orientation of the accordion (vertical or horizontal).
    /// </summary>
    public string Orientation { get; set; } = "vertical";

    /// <summary>
    /// Whether keyboard navigation loops from last to first and vice versa.
    /// </summary>
    public bool Loop { get; set; } = true;
}
