using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArkUI.Interop;

/// <summary>
/// JavaScript interop service for dropdown menu trigger and portal functionality.
/// All other functionality (positioning, keyboard nav, etc.) is handled by Blazor + FloatingJsInterop.
/// </summary>
public sealed class DropdownMenuJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/ArkUI/arkui.js").AsTask());

    /// <summary>
    /// Create a portal container element at the specified location.
    /// </summary>
    /// <param name="containerId">Unique ID for the portal container.</param>
    /// <param name="targetSelector">CSS selector for the parent element (default: body).</param>
    public async ValueTask CreatePortalAsync(string containerId, string? targetSelector = null)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dropdownMenu_createPortal", containerId, targetSelector);
    }

    /// <summary>
    /// Destroy and remove a portal container element.
    /// </summary>
    /// <param name="containerId">ID of the portal container to remove.</param>
    public async ValueTask DestroyPortalAsync(string containerId)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dropdownMenu_destroyPortal", containerId);
    }

    /// <summary>
    /// Initialize trigger to prevent default scroll on arrow keys.
    /// </summary>
    /// <param name="triggerElement">Reference to the trigger element.</param>
    public async ValueTask InitializeTriggerAsync(ElementReference triggerElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dropdownMenu_initializeTrigger", triggerElement);
    }

    /// <summary>
    /// Cleanup trigger event listeners.
    /// </summary>
    /// <param name="triggerElement">Reference to the trigger element.</param>
    public async ValueTask DestroyTriggerAsync(ElementReference triggerElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dropdownMenu_destroyTrigger", triggerElement);
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
