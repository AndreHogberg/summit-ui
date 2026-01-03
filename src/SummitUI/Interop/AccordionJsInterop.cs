using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop service for accordion functionality.
/// Handles DOM measurement and preventing default scroll behavior on arrow keys.
/// All other functionality (state management, keyboard nav logic, etc.) is handled by Blazor.
/// </summary>
public sealed class AccordionJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());

    /// <summary>
    /// Set CSS variables for content height and width (for animations).
    /// This requires DOM measurement which cannot be done in pure Blazor.
    /// </summary>
    /// <param name="contentElement">Reference to the content element.</param>
    public async ValueTask SetContentHeightAsync(ElementReference contentElement)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("accordion_setContentHeight", contentElement);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Register trigger element to prevent default scroll behavior on arrow keys.
    /// </summary>
    /// <param name="triggerElement">Reference to the trigger element.</param>
    public async ValueTask RegisterTriggerAsync(ElementReference triggerElement)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("accordion_registerTrigger", triggerElement);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Unregister trigger element keyboard handler.
    /// </summary>
    /// <param name="triggerElement">Reference to the trigger element.</param>
    public async ValueTask UnregisterTriggerAsync(ElementReference triggerElement)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("accordion_unregisterTrigger", triggerElement);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit already disconnected
            }
        }
    }
}
