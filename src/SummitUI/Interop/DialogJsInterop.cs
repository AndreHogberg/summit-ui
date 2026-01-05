using Microsoft.JSInterop;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop service for dialog functionality including scroll locking and portal management.
/// </summary>
public sealed class DialogJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());

    /// <summary>
    /// Locks body scroll to prevent background scrolling when a modal dialog is open.
    /// Supports nested locks - multiple dialogs can call this, and scroll
    /// will only be restored when all dialogs are closed.
    /// </summary>
    public async ValueTask LockScrollAsync()
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dialog_lockScroll");
    }

    /// <summary>
    /// Unlocks body scroll. Only actually unlocks when all nested locks are released.
    /// </summary>
    public async ValueTask UnlockScrollAsync()
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dialog_unlockScroll");
    }

    /// <summary>
    /// Force unlocks all scroll locks. Use for cleanup during navigation or errors.
    /// </summary>
    public async ValueTask ForceUnlockScrollAsync()
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dialog_forceUnlockScroll");
    }

    /// <summary>
    /// Creates a portal element in document.body for rendering dialog content.
    /// </summary>
    /// <param name="id">Unique ID for the portal element.</param>
    public async ValueTask CreatePortalAsync(string id)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dialog_createPortal", id);
    }

    /// <summary>
    /// Destroys a portal element.
    /// </summary>
    /// <param name="id">ID of the portal element to destroy.</param>
    public async ValueTask DestroyPortalAsync(string id)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dialog_destroyPortal", id);
    }

    /// <summary>
    /// Gets the current scroll lock count (useful for debugging).
    /// </summary>
    /// <returns>The number of active scroll locks.</returns>
    public async ValueTask<int> GetScrollLockCountAsync()
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<int>("dialog_getScrollLockCount");
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
