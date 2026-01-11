using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Base;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop service for scroll area functionality.
/// Handles scroll tracking, thumb sizing/positioning, and visibility management.
/// </summary>
public sealed class ScrollAreaJsInterop(IJSRuntime jsRuntime) : JsInteropBase(jsRuntime)
{
    /// <summary>
    /// Initialize a scroll area instance.
    /// </summary>
    /// <param name="viewportElement">Reference to the viewport element.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="callbackTarget">Object reference for callbacks.</param>
    /// <returns>Instance ID for subsequent operations.</returns>
    public async ValueTask<string?> InitializeAsync<T>(
        ElementReference viewportElement,
        ScrollAreaOptions options,
        DotNetObjectReference<T> callbackTarget) where T : class
    {
        try
        {
            var module = await GetModuleAsync();
            return await module.InvokeAsync<string?>(
                "scrollArea_initialize",
                viewportElement,
                options,
                callbackTarget);
        }
        catch (JSDisconnectedException)
        {
            return null;
        }
    }

    /// <summary>
    /// Register a scrollbar with the scroll area.
    /// </summary>
    /// <param name="instanceId">The scroll area instance ID.</param>
    /// <param name="orientation">Scrollbar orientation ('vertical' or 'horizontal').</param>
    /// <param name="scrollbarElement">Reference to the scrollbar track element.</param>
    /// <param name="thumbElement">Reference to the thumb element.</param>
    public async ValueTask RegisterScrollbarAsync(
        string instanceId,
        string orientation,
        ElementReference scrollbarElement,
        ElementReference thumbElement)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync(
                "scrollArea_registerScrollbar",
                instanceId,
                orientation,
                scrollbarElement,
                thumbElement);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Unregister a scrollbar from the scroll area.
    /// </summary>
    /// <param name="instanceId">The scroll area instance ID.</param>
    /// <param name="orientation">Scrollbar orientation ('vertical' or 'horizontal').</param>
    public async ValueTask UnregisterScrollbarAsync(string instanceId, string orientation)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("scrollArea_unregisterScrollbar", instanceId, orientation);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Update the thumb size and position for a scrollbar.
    /// </summary>
    /// <param name="instanceId">The scroll area instance ID.</param>
    /// <param name="orientation">Scrollbar orientation ('vertical' or 'horizontal').</param>
    public async ValueTask UpdateThumbAsync(string instanceId, string orientation)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("scrollArea_updateThumb", instanceId, orientation);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Scroll viewport to a specific position ratio (0-1).
    /// </summary>
    /// <param name="instanceId">The scroll area instance ID.</param>
    /// <param name="orientation">Scroll direction ('vertical' or 'horizontal').</param>
    /// <param name="ratio">Position ratio (0 = start, 1 = end).</param>
    public async ValueTask ScrollToPositionAsync(string instanceId, string orientation, double ratio)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("scrollArea_scrollToPosition", instanceId, orientation, ratio);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Get current scroll information.
    /// </summary>
    /// <param name="instanceId">The scroll area instance ID.</param>
    /// <returns>Scroll information or null if not available.</returns>
    public async ValueTask<ScrollAreaInfo?> GetScrollInfoAsync(string instanceId)
    {
        try
        {
            var module = await GetModuleAsync();
            return await module.InvokeAsync<ScrollAreaInfo?>("scrollArea_getScrollInfo", instanceId);
        }
        catch (JSDisconnectedException)
        {
            return null;
        }
    }

    /// <summary>
    /// Destroy a scroll area instance and clean up resources.
    /// </summary>
    /// <param name="instanceId">The scroll area instance ID.</param>
    public async ValueTask DestroyAsync(string? instanceId)
    {
        if (string.IsNullOrEmpty(instanceId)) return;

        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("scrollArea_destroy", instanceId);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }
}

/// <summary>
/// Options for initializing a scroll area.
/// </summary>
public class ScrollAreaOptions
{
    /// <summary>
    /// The visibility behavior type.
    /// </summary>
    public string Type { get; set; } = "hover";

    /// <summary>
    /// Delay in milliseconds before hiding scrollbars.
    /// </summary>
    public int ScrollHideDelay { get; set; } = 600;

    /// <summary>
    /// Text direction (ltr or rtl).
    /// </summary>
    public string Dir { get; set; } = "ltr";
}

/// <summary>
/// Scroll position and size information.
/// </summary>
public class ScrollAreaInfo
{
    /// <summary>
    /// Current vertical scroll position.
    /// </summary>
    public double ScrollTop { get; set; }

    /// <summary>
    /// Current horizontal scroll position.
    /// </summary>
    public double ScrollLeft { get; set; }

    /// <summary>
    /// Total scrollable height.
    /// </summary>
    public double ScrollHeight { get; set; }

    /// <summary>
    /// Total scrollable width.
    /// </summary>
    public double ScrollWidth { get; set; }

    /// <summary>
    /// Visible viewport height.
    /// </summary>
    public double ClientHeight { get; set; }

    /// <summary>
    /// Visible viewport width.
    /// </summary>
    public double ClientWidth { get; set; }

    /// <summary>
    /// Whether content overflows vertically.
    /// </summary>
    public bool HasVerticalOverflow { get; set; }

    /// <summary>
    /// Whether content overflows horizontally.
    /// </summary>
    public bool HasHorizontalOverflow { get; set; }
}
