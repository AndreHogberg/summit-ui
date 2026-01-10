using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop service for dropdown menu trigger, portal, and submenu functionality.
/// All other functionality (positioning, keyboard nav, etc.) is handled by Blazor + FloatingJsInterop.
/// </summary>
public sealed class DropdownMenuJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());

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

    /// <summary>
    /// Initialize sub trigger for hover intent.
    /// </summary>
    /// <param name="triggerElement">Reference to the sub trigger element.</param>
    /// <param name="dotNetRef">DotNet object reference for callbacks.</param>
    /// <param name="openDelay">Delay before opening in milliseconds.</param>
    /// <param name="closeDelay">Delay before closing in milliseconds.</param>
    public async ValueTask InitializeSubTriggerAsync(
        ElementReference triggerElement,
        DotNetObjectReference<DropdownMenuSubTrigger> dotNetRef,
        int openDelay,
        int closeDelay)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dropdownMenu_initializeSubTrigger", triggerElement, dotNetRef, openDelay, closeDelay);
    }

    /// <summary>
    /// Cleanup sub trigger event listeners and state.
    /// </summary>
    /// <param name="triggerElement">Reference to the sub trigger element.</param>
    public async ValueTask DestroySubTriggerAsync(ElementReference triggerElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dropdownMenu_destroySubTrigger", triggerElement);
    }

    /// <summary>
    /// Cancel the close timer for a sub trigger.
    /// </summary>
    /// <param name="triggerElement">Reference to the sub trigger element.</param>
    public async ValueTask CancelSubTriggerCloseAsync(ElementReference triggerElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dropdownMenu_cancelSubTriggerClose", triggerElement);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Safe to ignore, JS resources are cleaned up by the browser
        }
    }
}
