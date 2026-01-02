using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArkUI.Utilities;

/// <summary>
/// Core utility service for ArkUI components.
/// Provides minimal JavaScript interop for operations that cannot be done in pure Blazor.
/// </summary>
public sealed class ArkUtilities : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private bool? _cachedIsRtl;

    public ArkUtilities(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
            jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/ArkUI/arkui.js").AsTask());
    }

    /// <summary>
    /// Checks if the document direction is right-to-left (RTL).
    /// The result is cached after the first call for performance.
    /// </summary>
    /// <returns>True if the document is in RTL mode, false otherwise.</returns>
    public async ValueTask<bool> IsRtlAsync()
    {
        if (_cachedIsRtl.HasValue)
            return _cachedIsRtl.Value;

        try
        {
            var module = await _moduleTask.Value;
            _cachedIsRtl = await module.InvokeAsync<bool>("utilities_isRtl");
            return _cachedIsRtl.Value;
        }
        catch (JSDisconnectedException)
        {
            return false;
        }
    }

    /// <summary>
    /// Clears the cached RTL value, forcing a fresh check on the next call.
    /// Useful when the document direction might change dynamically.
    /// </summary>
    public void ClearRtlCache()
    {
        _cachedIsRtl = null;
    }

    /// <summary>
    /// Focuses a specific element referenced by an ElementReference.
    /// </summary>
    /// <param name="element">The element to focus.</param>
    public async ValueTask FocusElementAsync(ElementReference element)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("utilities_focusElement", element);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Focuses a specific element by its ID.
    /// </summary>
    /// <param name="elementId">The ID of the element to focus.</param>
    public async ValueTask FocusElementByIdAsync(string elementId)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("utilities_focusElementById", elementId);
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
