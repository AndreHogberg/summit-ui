using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Base;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop service for popover positioning using Floating UI.
/// </summary>
public sealed class PopoverJsInterop(IJSRuntime jsRuntime) : JsInteropBase(jsRuntime)
{
    /// <summary>
    /// Initialize popover positioning and event listeners.
    /// </summary>
    /// <typeparam name="T">The type of the .NET object reference (must have JSInvokable methods).</typeparam>
    /// <param name="triggerElement">Reference to the trigger button element.</param>
    /// <param name="contentElement">Reference to the popover content element.</param>
    /// <param name="arrowElement">Optional reference to the arrow element.</param>
    /// <param name="dotNetRef">Reference to the .NET object for callbacks.</param>
    /// <param name="options">Positioning and behavior options.</param>
    public async ValueTask InitializePopoverAsync<T>(
        ElementReference triggerElement,
        ElementReference contentElement,
        ElementReference? arrowElement,
        DotNetObjectReference<T> dotNetRef,
        PopoverPositionOptions options) where T : class
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync(
            "popover_initializePopover",
            triggerElement,
            contentElement,
            arrowElement,
            dotNetRef,
            options);
    }

    /// <summary>
    /// Destroy popover positioning and cleanup event listeners.
    /// </summary>
    /// <param name="contentElement">Reference to the popover content element.</param>
    public async ValueTask DestroyPopoverAsync(ElementReference contentElement)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("popover_destroyPopover", contentElement);
    }

    /// <summary>
    /// Manually trigger position update.
    /// </summary>
    /// <param name="contentElement">Reference to the popover content element.</param>
    public async ValueTask UpdatePositionAsync(ElementReference contentElement)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("popover_updatePosition", contentElement);
    }

    /// <summary>
    /// Create a portal container element at the specified location.
    /// </summary>
    /// <param name="containerId">Unique ID for the portal container.</param>
    /// <param name="targetSelector">CSS selector for the parent element (default: body).</param>
    public async ValueTask CreatePortalAsync(string containerId, string? targetSelector = null)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("popover_createPortal", containerId, targetSelector);
    }

    /// <summary>
    /// Destroy and remove a portal container element.
    /// </summary>
    /// <param name="containerId">ID of the portal container to remove.</param>
    public async ValueTask DestroyPortalAsync(string containerId)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("popover_destroyPortal", containerId);
    }

    /// <summary>
    /// Focus the first focusable element within the content.
    /// </summary>
    /// <param name="contentElement">Reference to the popover content element.</param>
    public async ValueTask FocusFirstElementAsync(ElementReference contentElement)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("popover_focusFirstElement", contentElement);
    }

    /// <summary>
    /// Focus the specified element.
    /// </summary>
    /// <param name="element">Reference to the element to focus.</param>
    public async ValueTask FocusElementAsync(ElementReference element)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("popover_focusElement", element);
    }
}

/// <summary>
/// Options for popover positioning and behavior.
/// </summary>
public sealed class PopoverPositionOptions
{
    /// <summary>
    /// Preferred placement side (top, right, bottom, left).
    /// </summary>
    public string Side { get; set; } = "bottom";

    /// <summary>
    /// Offset from the trigger element in pixels.
    /// </summary>
    public int SideOffset { get; set; }

    /// <summary>
    /// Alignment along the side axis (start, center, end).
    /// </summary>
    public string Align { get; set; } = "center";

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
    /// Whether to trap focus within the popover.
    /// </summary>
    public bool TrapFocus { get; set; }

    /// <summary>
    /// Whether to close the popover when Escape is pressed.
    /// </summary>
    public bool CloseOnEscape { get; set; } = true;

    /// <summary>
    /// Whether to close the popover when clicking outside.
    /// </summary>
    public bool CloseOnOutsideClick { get; set; } = true;
}
