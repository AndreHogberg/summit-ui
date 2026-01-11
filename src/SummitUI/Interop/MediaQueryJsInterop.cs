using Microsoft.JSInterop;

using SummitUI.Base;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop service for media query functionality.
/// Provides reactive media query matching using the browser's matchMedia API.
/// </summary>
public sealed class MediaQueryJsInterop(IJSRuntime jsRuntime) : JsInteropBase(jsRuntime)
{
    /// <summary>
    /// Register a media query listener that will invoke a callback when the match state changes.
    /// </summary>
    /// <param name="listenerId">Unique identifier for this listener.</param>
    /// <param name="query">CSS media query string (e.g., "(min-width: 800px)").</param>
    /// <param name="dotNetRef">DotNet object reference for callbacks.</param>
    /// <returns>The initial match state of the media query.</returns>
    public async ValueTask<bool> RegisterAsync<T>(string listenerId, string query, DotNetObjectReference<T> dotNetRef)
        where T : class
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<bool>("mediaQuery_register", listenerId, query, dotNetRef);
    }

    /// <summary>
    /// Unregister a media query listener.
    /// </summary>
    /// <param name="listenerId">The listener ID to unregister.</param>
    public async ValueTask UnregisterAsync(string listenerId)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("mediaQuery_unregister", listenerId);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, JS resources cleaned up by browser
        }
    }

    /// <summary>
    /// Evaluate a media query without registering a listener.
    /// </summary>
    /// <param name="query">CSS media query string.</param>
    /// <returns>Whether the media query currently matches.</returns>
    public async ValueTask<bool> EvaluateAsync(string query)
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<bool>("mediaQuery_evaluate", query);
    }
}
