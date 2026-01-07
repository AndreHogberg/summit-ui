using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop service for FloatingUI positioning.
/// This is a thin wrapper that only handles positioning calculations.
/// All event handling (keyboard, focus, outside click) is managed by Blazor.
/// </summary>
public sealed class FloatingJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());

    /// <summary>
    /// Initialize floating positioning for an element.
    /// </summary>
    /// <param name="referenceElement">Reference element (anchor).</param>
    /// <param name="floatingElement">Floating element to position.</param>
    /// <param name="arrowElement">Optional arrow element.</param>
    /// <param name="options">Positioning options.</param>
    /// <returns>Instance ID for later cleanup.</returns>
    public async ValueTask<string?> InitializeAsync(
        ElementReference referenceElement,
        ElementReference floatingElement,
        ElementReference? arrowElement,
        FloatingPositionOptions options)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string?>(
            "floating_initializeFloating",
            referenceElement,
            floatingElement,
            arrowElement,
            options);
    }

    /// <summary>
    /// Destroy floating positioning and cleanup.
    /// </summary>
    /// <param name="instanceId">The instance ID returned from InitializeAsync.</param>
    public async ValueTask DestroyAsync(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId)) return;

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("floating_destroyFloating", instanceId);
    }

    /// <summary>
    /// Manually trigger position update.
    /// </summary>
    /// <param name="instanceId">The instance ID.</param>
    public async ValueTask UpdatePositionAsync(string instanceId)
    {
        if (string.IsNullOrEmpty(instanceId)) return;

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("floating_updatePosition", instanceId);
    }

    /// <summary>
    /// Focus a specific element.
    /// </summary>
    /// <param name="element">Element to focus.</param>
    public async ValueTask FocusElementAsync(ElementReference element)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("floating_focusElement", element);
    }

    /// <summary>
    /// Focus the first focusable element within a container.
    /// </summary>
    /// <param name="containerElement">Container element.</param>
    public async ValueTask FocusFirstElementAsync(ElementReference containerElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("floating_focusFirstElement", containerElement);
    }

    /// <summary>
    /// Focus the last focusable element within a container.
    /// </summary>
    /// <param name="containerElement">Container element.</param>
    public async ValueTask FocusLastElementAsync(ElementReference containerElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("floating_focusLastElement", containerElement);
    }

    /// <summary>
    /// Scroll an element into view within a container.
    /// </summary>
    /// <param name="element">Element to scroll into view.</param>
    /// <param name="container">Scrollable container (optional).</param>
    public async ValueTask ScrollIntoViewAsync(ElementReference element, ElementReference? container = null)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("floating_scrollIntoView", element, container);
    }

    /// <summary>
    /// Scroll an item into view by its data-value attribute within a container.
    /// </summary>
    /// <param name="container">Container element.</param>
    /// <param name="itemValue">The data-value attribute value of the item to scroll to.</param>
    public async ValueTask ScrollItemIntoViewAsync(ElementReference container, string itemValue)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("floating_scrollItemIntoView", container, itemValue);
    }
    /// <summary>
    /// Register an outside click listener.
    /// </summary>
    /// <typeparam name="T">The type containing the callback method.</typeparam>
    /// <param name="referenceElement">Reference element (anchor).</param>
    /// <param name="floatingElement">Floating element.</param>
    /// <param name="dotNetRef">Reference to the .NET object for callbacks.</param>
    /// <param name="methodName">Name of the JSInvokable method to call.</param>
    /// <returns>Listener ID for later cleanup.</returns>
    public async ValueTask<string?> RegisterOutsideClickAsync<T>(
        ElementReference referenceElement,
        ElementReference floatingElement,
        DotNetObjectReference<T> dotNetRef,
        string methodName = "HandleOutsideClick") where T : class
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string?>(
            "floating_registerOutsideClick",
            referenceElement,
            floatingElement,
            dotNetRef,
            methodName);
    }

    /// <summary>
    /// Unregister an outside click listener.
    /// </summary>
    /// <param name="listenerId">The listener ID returned from RegisterOutsideClickAsync.</param>
    public async ValueTask UnregisterOutsideClickAsync(string? listenerId)
    {
        if (string.IsNullOrEmpty(listenerId)) return;

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("floating_unregisterOutsideClick", listenerId);
    }

    /// <summary>
    /// Register an Escape key listener at the document level.
    /// </summary>
    /// <typeparam name="T">The type containing the callback method.</typeparam>
    /// <param name="dotNetRef">Reference to the .NET object for callbacks.</param>
    /// <param name="methodName">Name of the JSInvokable method to call.</param>
    /// <returns>Listener ID for later cleanup.</returns>
    public async ValueTask<string?> RegisterEscapeKeyAsync<T>(
        DotNetObjectReference<T> dotNetRef,
        string methodName = "HandleEscapeKey") where T : class
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string?>(
            "floating_registerEscapeKey",
            dotNetRef,
            methodName);
    }

    /// <summary>
    /// Unregister an Escape key listener.
    /// </summary>
    /// <param name="listenerId">The listener ID returned from RegisterEscapeKeyAsync.</param>
    public async ValueTask UnregisterEscapeKeyAsync(string? listenerId)
    {
        if (string.IsNullOrEmpty(listenerId)) return;

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("floating_unregisterEscapeKey", listenerId);
    }

    /// <summary>
    /// Wait for all CSS animations on an element to complete, then invoke a callback.
    /// If no animations are running, the callback is invoked immediately.
    /// This enables animation-aware presence management.
    /// </summary>
    /// <typeparam name="T">The type containing the callback method.</typeparam>
    /// <param name="element">The element to watch for animations.</param>
    /// <param name="callbackTarget">The object reference that contains the callback method.</param>
    /// <param name="methodName">The name of the method to invoke when animations complete.</param>
    public async ValueTask WaitForAnimationsCompleteAsync<T>(
        ElementReference element,
        DotNetObjectReference<T> callbackTarget,
        string methodName) where T : class
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("floating_waitForAnimationsComplete", element, callbackTarget, methodName);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Cancel any pending animation watcher for an element.
    /// Call this when disposing or when state changes before animations complete.
    /// </summary>
    /// <param name="element">The element to cancel watching.</param>
    public async ValueTask CancelAnimationWatcherAsync(ElementReference element)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("floating_cancelAnimationWatcher", element);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Click an element by its ID.
    /// </summary>
    /// <param name="elementId">The ID of the element to click.</param>
    public async ValueTask ClickElementByIdAsync(string elementId)
    {
        if (string.IsNullOrEmpty(elementId)) return;

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("floating_clickElementById", elementId);
    }

    /// <summary>
    /// Scroll an element into view by its ID.
    /// </summary>
    /// <param name="elementId">The ID of the element to scroll into view.</param>
    public async ValueTask ScrollElementIntoViewByIdAsync(string elementId)
    {
        if (string.IsNullOrEmpty(elementId)) return;

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("floating_scrollElementIntoViewById", elementId);
    }

    /// <summary>
    /// Focus an element by its ID.
    /// </summary>
    /// <param name="elementId">The ID of the element to focus.</param>
    public async ValueTask FocusElementByIdAsync(string elementId)
    {
        if (string.IsNullOrEmpty(elementId)) return;

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("floating_focusElementById", elementId);
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

/// <summary>
/// Options for floating element positioning.
/// </summary>
public sealed class FloatingPositionOptions
{
    /// <summary>
    /// Preferred placement side (top, right, bottom, left).
    /// </summary>
    public string Side { get; set; } = "bottom";

    /// <summary>
    /// Offset from the reference element in pixels (main axis).
    /// </summary>
    public int SideOffset { get; set; }

    /// <summary>
    /// Alignment along the side axis (start, center, end).
    /// </summary>
    public string Align { get; set; } = "center";

    /// <summary>
    /// Offset for alignment in pixels (cross axis).
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
    /// Whether to constrain size to available space (useful for selects/dropdowns).
    /// </summary>
    public bool ConstrainSize { get; set; }
}
