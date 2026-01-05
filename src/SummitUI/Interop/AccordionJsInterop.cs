using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop service for accordion functionality.
/// Handles DOM measurement, scroll prevention, and animation-aware presence management.
/// All other functionality (state management, keyboard nav logic, etc.) is handled by Blazor.
/// </summary>
public sealed class AccordionJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());

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

    /// <summary>
    /// Register trigger element to prevent default scroll behavior on arrow keys.
    /// </summary>
    /// <param name="triggerElement">Reference to the trigger element.</param>
    public async ValueTask RegisterTriggerAsync(ElementReference triggerElement)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("accordion_registerTrigger", triggerElement);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Unregister trigger element keyboard handler.
    /// </summary>
    /// <param name="triggerElement">Reference to the trigger element.</param>
    public async ValueTask UnregisterTriggerAsync(ElementReference triggerElement)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("accordion_unregisterTrigger", triggerElement);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Wait for all CSS animations on an element to complete, then invoke a callback.
    /// If no animations are running, the callback is invoked immediately.
    /// This enables animation-aware presence management.
    /// </summary>
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
            await module.InvokeVoidAsync("accordion_waitForAnimationsComplete", element, callbackTarget, methodName);
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
            await module.InvokeVoidAsync("accordion_cancelAnimationWatcher", element);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Set the hidden attribute on an element.
    /// </summary>
    /// <param name="element">The element to hide.</param>
    public async ValueTask SetHiddenAsync(ElementReference element)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("accordion_setHidden", element);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Remove the hidden attribute from an element.
    /// </summary>
    /// <param name="element">The element to show.</param>
    public async ValueTask RemoveHiddenAsync(ElementReference element)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("accordion_removeHidden", element);
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
