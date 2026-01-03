using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArkUI.Interop;

/// <summary>
/// JavaScript interop service for focus trap functionality.
/// Reusable for modals, dialogs, popovers, and other overlay components.
/// </summary>
public sealed class FocusTrapJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/ArkUI/arkui.js").AsTask());

    /// <summary>
    /// Activate a focus trap on a container element.
    /// </summary>
    /// <param name="containerElement">The element to trap focus within.</param>
    /// <param name="options">Configuration options for the focus trap.</param>
    /// <returns>A trap ID that can be used to deactivate the trap later.</returns>
    public async ValueTask<string?> ActivateAsync(
        ElementReference containerElement,
        FocusTrapOptions? options = null)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string?>("focusTrap_activate", containerElement, options ?? new FocusTrapOptions());
    }

    /// <summary>
    /// Deactivate a focus trap by its ID.
    /// </summary>
    /// <param name="trapId">The trap ID returned from ActivateAsync.</param>
    public async ValueTask DeactivateAsync(string trapId)
    {
        if (string.IsNullOrEmpty(trapId)) return;

        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("focusTrap_deactivate", trapId);
    }

    /// <summary>
    /// Deactivate a focus trap by its container element.
    /// </summary>
    /// <param name="containerElement">The container element.</param>
    public async ValueTask DeactivateByContainerAsync(ElementReference containerElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("focusTrap_deactivateByContainer", containerElement);
    }

    /// <summary>
    /// Focus the first focusable element in a container.
    /// </summary>
    /// <param name="containerElement">The container element.</param>
    public async ValueTask FocusFirstAsync(ElementReference containerElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("focusTrap_focusFirst", containerElement);
    }

    /// <summary>
    /// Focus the last focusable element in a container.
    /// </summary>
    /// <param name="containerElement">The container element.</param>
    public async ValueTask FocusLastAsync(ElementReference containerElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("focusTrap_focusLast", containerElement);
    }

    /// <summary>
    /// Focus a specific element.
    /// </summary>
    /// <param name="element">The element to focus.</param>
    public async ValueTask FocusElementAsync(ElementReference element)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("focusTrap_focusElement", element);
    }

    /// <summary>
    /// Get the count of focusable elements in a container.
    /// </summary>
    /// <param name="containerElement">The container element.</param>
    /// <returns>The number of focusable elements.</returns>
    public async ValueTask<int> GetFocusableCountAsync(ElementReference containerElement)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<int>("focusTrap_getFocusableCount", containerElement);
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
/// Options for configuring a focus trap.
/// </summary>
public sealed class FocusTrapOptions
{
    /// <summary>
    /// Whether to automatically focus the first focusable element when activated.
    /// Default: true.
    /// </summary>
    public bool AutoFocus { get; set; } = true;

    /// <summary>
    /// Whether to return focus to the previously focused element when deactivated.
    /// Default: true.
    /// </summary>
    public bool ReturnFocus { get; set; } = true;
}
