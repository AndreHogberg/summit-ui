using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Utilities;

/// <summary>
/// Core utility service for SummitUI components.
/// Provides minimal JavaScript interop for operations that cannot be done in pure Blazor.
/// </summary>
public sealed class SummitUtilities(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());
    private bool? _cachedIsRtl;

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
    /// Checks if a specific element's direction is right-to-left (RTL).
    /// This uses getComputedStyle to check the element's effective direction,
    /// accounting for inherited dir attributes from parent elements.
    /// </summary>
    /// <param name="elementId">The ID of the element to check.</param>
    /// <returns>True if the element is in RTL mode, false otherwise.</returns>
    public async ValueTask<bool> IsElementRtlAsync(string elementId)
    {
        try
        {
            var module = await _moduleTask.Value;
            return await module.InvokeAsync<bool>("utilities_isElementRtl", elementId);
        }
        catch (JSDisconnectedException)
        {
            return false;
        }
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

    /// <summary>
    /// Initializes a checkbox element to prevent Enter key from activating it.
    /// Checkboxes should only respond to Space key per WAI-ARIA patterns.
    /// </summary>
    /// <param name="element">The checkbox button element.</param>
    public async ValueTask InitializeCheckboxAsync(ElementReference element)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("utilities_initializeCheckbox", element);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Cleans up checkbox event handlers.
    /// </summary>
    /// <param name="element">The checkbox button element.</param>
    public async ValueTask DestroyCheckboxAsync(ElementReference element)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("utilities_destroyCheckbox", element);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Initializes a radio item element to prevent arrow keys from scrolling the page.
    /// </summary>
    /// <param name="element">The radio item button element.</param>
    public async ValueTask InitializeRadioItemAsync(ElementReference element)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("utilities_initializeRadioItem", element);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Cleans up radio item event handlers.
    /// </summary>
    /// <param name="element">The radio item button element.</param>
    public async ValueTask DestroyRadioItemAsync(ElementReference element)
    {
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("utilities_destroyRadioItem", element);
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
