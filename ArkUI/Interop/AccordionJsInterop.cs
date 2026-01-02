using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArkUI.Interop;

/// <summary>
/// JavaScript interop service for accordion animations.
/// Only contains functionality that requires DOM measurement (cannot be done in pure Blazor).
/// </summary>
public sealed class AccordionJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/ArkUI/arkui.js").AsTask());

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
