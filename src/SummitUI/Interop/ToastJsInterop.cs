using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI;

/// <summary>
/// JavaScript interop for toast functionality including portal, hotkey, and swipe support.
/// </summary>
public class ToastJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask =
        new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());

    /// <summary>
    /// Creates a portal container for toasts at the end of body.
    /// </summary>
    /// <param name="containerId">Unique ID for the container.</param>
    public async ValueTask CreatePortalAsync(string containerId)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("toast_createPortal", containerId);
    }

    /// <summary>
    /// Destroys and removes a toast portal container.
    /// </summary>
    /// <param name="containerId">ID of the container to remove.</param>
    public async ValueTask DestroyPortalAsync(string containerId)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("toast_destroyPortal", containerId);
    }

    /// <summary>
    /// Registers a keyboard hotkey to focus the toast region.
    /// </summary>
    /// <param name="element">The element to focus when hotkey is pressed.</param>
    /// <param name="hotkey">Array of key codes (e.g., ["F8"] or ["altKey", "KeyT"]).</param>
    /// <param name="dotNetRef">Reference for callbacks.</param>
    /// <param name="methodName">Method to invoke on hotkey press.</param>
    public async ValueTask RegisterHotkeyAsync(
        ElementReference element,
        string[] hotkey,
        DotNetObjectReference<object> dotNetRef,
        string methodName)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("toast_registerHotkey", element, hotkey, dotNetRef, methodName);
    }

    /// <summary>
    /// Unregisters the keyboard hotkey handler.
    /// </summary>
    /// <param name="element">The element to unregister.</param>
    public async ValueTask UnregisterHotkeyAsync(ElementReference element)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("toast_unregisterHotkey", element);
    }

    /// <summary>
    /// Registers swipe gesture handling for a toast element.
    /// </summary>
    /// <param name="element">The toast element.</param>
    /// <param name="direction">Swipe direction ("left", "right", "up", "down").</param>
    /// <param name="threshold">Distance in pixels to trigger swipe close.</param>
    /// <param name="dotNetRef">Reference for swipe callbacks.</param>
    /// <typeparam name="T">The type of the .NET object reference.</typeparam>
    public async ValueTask RegisterSwipeAsync<T>(
        ElementReference element,
        string direction,
        int threshold,
        DotNetObjectReference<T> dotNetRef) where T : class
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("toast_registerSwipe", element, direction, threshold, dotNetRef);
    }

    /// <summary>
    /// Unregisters swipe gesture handling for a toast element.
    /// </summary>
    /// <param name="element">The toast element.</param>
    public async ValueTask UnregisterSwipeAsync(ElementReference element)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("toast_unregisterSwipe", element);
    }

    /// <inheritdoc />
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
