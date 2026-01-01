using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArkUI.Interop;

/// <summary>
/// JavaScript interop service for tabs keyboard navigation.
/// </summary>
public sealed class TabsJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/ArkUI/tabs.js").AsTask());

    /// <summary>
    /// Initialize keyboard navigation for the tabs list.
    /// </summary>
    /// <typeparam name="T">The type of the .NET object reference (must have JSInvokable methods).</typeparam>
    /// <param name="listElement">Reference to the tabs list element.</param>
    /// <param name="dotNetRef">Reference to the .NET object for callbacks.</param>
    /// <param name="options">Navigation options.</param>
    public async ValueTask InitializeAsync<T>(
        ElementReference listElement,
        DotNetObjectReference<T> dotNetRef,
        TabsNavigationOptions options) where T : class
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("initializeTabs", listElement, dotNetRef, options);
    }

    /// <summary>
    /// Cleanup keyboard event listeners.
    /// </summary>
    /// <param name="listElement">Reference to the tabs list element.</param>
    public async ValueTask DestroyAsync(ElementReference listElement)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("destroyTabs", listElement);
    }

    /// <summary>
    /// Focus a specific trigger by value.
    /// </summary>
    /// <param name="listElement">Reference to the tabs list element.</param>
    /// <param name="value">Value of the trigger to focus.</param>
    public async ValueTask FocusTriggerAsync(ElementReference listElement, string value)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("focusTrigger", listElement, value);
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
/// Options for tabs keyboard navigation.
/// </summary>
public sealed class TabsNavigationOptions
{
    /// <summary>
    /// Orientation of the tabs (horizontal or vertical).
    /// </summary>
    public string Orientation { get; set; } = "horizontal";

    /// <summary>
    /// Whether keyboard navigation loops from last to first and vice versa.
    /// </summary>
    public bool Loop { get; set; } = true;

    /// <summary>
    /// Activation mode (auto or manual).
    /// </summary>
    public string ActivationMode { get; set; } = "auto";
}
