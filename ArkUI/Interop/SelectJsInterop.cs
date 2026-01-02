using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArkUI.Interop;

/// <summary>
/// JavaScript interop service for select positioning and keyboard navigation.
/// </summary>
public sealed class SelectJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/ArkUI/arkui.js").AsTask());

    /// <summary>
    /// Initialize select positioning and event listeners.
    /// </summary>
    /// <typeparam name="T">The type of the .NET object reference (must have JSInvokable methods).</typeparam>
    /// <param name="triggerElement">Reference to the trigger button element.</param>
    /// <param name="contentElement">Reference to the select content element.</param>
    /// <param name="dotNetRef">Reference to the .NET object for callbacks.</param>
    /// <param name="options">Positioning and behavior options.</param>
    public async ValueTask InitializeSelectAsync<T>(
        ElementReference triggerElement,
        ElementReference contentElement,
        DotNetObjectReference<T> dotNetRef,
        SelectPositionOptions options) where T : class
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync(
            "select_initializeSelect",
            triggerElement,
            contentElement,
            dotNetRef,
            options);
    }

    /// <summary>
    /// Destroy select positioning and cleanup event listeners.
    /// </summary>
    /// <param name="contentElement">Reference to the select content element.</param>
    public async ValueTask DestroySelectAsync(ElementReference contentElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("select_destroySelect", contentElement);
    }

    /// <summary>
    /// Manually trigger position update.
    /// </summary>
    /// <param name="contentElement">Reference to the select content element.</param>
    public async ValueTask UpdatePositionAsync(ElementReference contentElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("select_updatePosition", contentElement);
    }

    /// <summary>
    /// Highlight a specific item by value.
    /// </summary>
    /// <param name="contentElement">Reference to the select content element.</param>
    /// <param name="value">The value of the item to highlight.</param>
    public async ValueTask HighlightItemAsync(ElementReference contentElement, string value)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("select_highlightItem", contentElement, value);
    }

    /// <summary>
    /// Highlight the first non-disabled item.
    /// </summary>
    /// <param name="contentElement">Reference to the select content element.</param>
    public async ValueTask HighlightFirstAsync(ElementReference contentElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("select_highlightFirst", contentElement);
    }

    /// <summary>
    /// Highlight the last non-disabled item.
    /// </summary>
    /// <param name="contentElement">Reference to the select content element.</param>
    public async ValueTask HighlightLastAsync(ElementReference contentElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("select_highlightLast", contentElement);
    }

    /// <summary>
    /// Focus the trigger element.
    /// </summary>
    /// <param name="triggerElement">Reference to the trigger element.</param>
    public async ValueTask FocusTriggerAsync(ElementReference triggerElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("select_focusTrigger", triggerElement);
    }

    /// <summary>
    /// Register trigger element to prevent default scroll behavior on arrow keys.
    /// </summary>
    /// <param name="triggerElement">Reference to the trigger element.</param>
    public async ValueTask RegisterTriggerAsync(ElementReference triggerElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("select_registerTrigger", triggerElement);
    }

    /// <summary>
    /// Unregister trigger element keyboard handler.
    /// </summary>
    /// <param name="triggerElement">Reference to the trigger element.</param>
    public async ValueTask UnregisterTriggerAsync(ElementReference triggerElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("select_unregisterTrigger", triggerElement);
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
/// Options for select positioning and behavior.
/// </summary>
public sealed class SelectPositionOptions
{
    /// <summary>
    /// Preferred placement side (top, right, bottom, left).
    /// </summary>
    public string Side { get; set; } = "bottom";

    /// <summary>
    /// Offset from the trigger element in pixels.
    /// </summary>
    public int SideOffset { get; set; } = 4;

    /// <summary>
    /// Alignment along the side axis (start, center, end).
    /// </summary>
    public string Align { get; set; } = "start";

    /// <summary>
    /// Offset for alignment in pixels.
    /// </summary>
    public int AlignOffset { get; set; }

    /// <summary>
    /// Whether to avoid collisions with viewport boundaries.
    /// </summary>
    public bool AvoidCollisions { get; set; } = true;

    /// <summary>
    /// Padding from viewport edges for collision detection.
    /// </summary>
    public int CollisionPadding { get; set; } = 8;

    /// <summary>
    /// Whether to close the select when Escape is pressed.
    /// </summary>
    public bool CloseOnEscape { get; set; } = true;

    /// <summary>
    /// Whether to close the select when clicking outside.
    /// </summary>
    public bool CloseOnOutsideClick { get; set; } = true;

    /// <summary>
    /// The currently selected value (for initial highlight).
    /// </summary>
    public string? SelectedValue { get; set; }
}
