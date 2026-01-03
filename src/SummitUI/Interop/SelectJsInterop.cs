using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop service for select trigger functionality.
/// Only handles preventing default scroll behavior on the trigger element.
/// All other functionality (positioning, keyboard nav, etc.) is handled by Blazor + FloatingJsInterop.
/// </summary>
public sealed class SelectJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());

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
            try
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit is already disconnected, no need to dispose JS resources
            }
        }
    }
}
