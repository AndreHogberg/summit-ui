using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArkUI.Interop;

/// <summary>
/// JavaScript interop service for dropdown menu positioning and keyboard navigation.
/// </summary>
public sealed class DropdownMenuJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/ArkUI/arkui.js").AsTask());

    /// <summary>
    /// Initialize dropdown menu positioning and event listeners.
    /// </summary>
    /// <typeparam name="T">The type of the .NET object reference (must have JSInvokable methods).</typeparam>
    /// <param name="triggerElement">Reference to the trigger button element.</param>
    /// <param name="contentElement">Reference to the menu content element.</param>
    /// <param name="arrowElement">Optional reference to the arrow element.</param>
    /// <param name="dotNetRef">Reference to the .NET object for callbacks.</param>
    /// <param name="options">Positioning and behavior options.</param>
    public async ValueTask InitializeDropdownMenuAsync<T>(
        ElementReference triggerElement,
        ElementReference contentElement,
        ElementReference? arrowElement,
        DotNetObjectReference<T> dotNetRef,
        DropdownMenuPositionOptions options) where T : class
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync(
            "dropdownMenu_initializeDropdownMenu",
            triggerElement,
            contentElement,
            arrowElement,
            dotNetRef,
            options);
    }

    /// <summary>
    /// Destroy dropdown menu positioning and cleanup event listeners.
    /// </summary>
    /// <param name="contentElement">Reference to the menu content element.</param>
    public async ValueTask DestroyDropdownMenuAsync(ElementReference contentElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dropdownMenu_destroyDropdownMenu", contentElement);
    }

    /// <summary>
    /// Manually trigger position update.
    /// </summary>
    /// <param name="contentElement">Reference to the menu content element.</param>
    public async ValueTask UpdatePositionAsync(ElementReference contentElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dropdownMenu_updatePosition", contentElement);
    }

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
    /// Focus the first menu item.
    /// </summary>
    /// <param name="contentElement">Reference to the menu content element.</param>
    public async ValueTask FocusFirstItemAsync(ElementReference contentElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dropdownMenu_focusFirstItem", contentElement);
    }

    /// <summary>
    /// Focus the last menu item.
    /// </summary>
    /// <param name="contentElement">Reference to the menu content element.</param>
    public async ValueTask FocusLastItemAsync(ElementReference contentElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dropdownMenu_focusLastItem", contentElement);
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

/// <summary>
/// Options for dropdown menu positioning and behavior.
/// </summary>
public sealed class DropdownMenuPositionOptions
{
    /// <summary>
    /// Preferred placement side (top, right, bottom, left).
    /// </summary>
    public string Side { get; set; } = "bottom";

    /// <summary>
    /// Offset from the trigger element in pixels.
    /// </summary>
    public int SideOffset { get; set; } = 4;

    /// <summary>
    /// Alignment along the side axis (start, center, end).
    /// </summary>
    public string Align { get; set; } = "start";

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
    /// Whether to close the menu when Escape is pressed.
    /// </summary>
    public bool CloseOnEscape { get; set; } = true;

    /// <summary>
    /// Whether to close the menu when clicking outside.
    /// </summary>
    public bool CloseOnOutsideClick { get; set; } = true;

    /// <summary>
    /// Whether to loop keyboard navigation at ends.
    /// </summary>
    public bool Loop { get; set; } = true;
}
