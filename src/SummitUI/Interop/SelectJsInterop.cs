using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Base;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop service for select trigger functionality.
/// Only handles preventing default scroll behavior on the trigger element.
/// All other functionality (positioning, keyboard nav, etc.) is handled by Blazor + FloatingJsInterop.
/// </summary>
public sealed class SelectJsInterop(IJSRuntime jsRuntime) : JsInteropBase(jsRuntime)
{
    /// <summary>
    /// Register trigger element to prevent default scroll behavior on arrow keys.
    /// </summary>
    /// <param name="triggerElement">Reference to the trigger element.</param>
    public async ValueTask RegisterTriggerAsync(ElementReference triggerElement)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("select_registerTrigger", triggerElement);
    }

    /// <summary>
    /// Unregister trigger element keyboard handler.
    /// </summary>
    /// <param name="triggerElement">Reference to the trigger element.</param>
    public async ValueTask UnregisterTriggerAsync(ElementReference triggerElement)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("select_unregisterTrigger", triggerElement);
    }
}
