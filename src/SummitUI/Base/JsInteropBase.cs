using Microsoft.JSInterop;

namespace SummitUI.Base;

/// <summary>
/// Base class for JavaScript interop services.
/// Provides lazy module loading and proper disposal handling for Blazor Server disconnect scenarios.
/// </summary>
public abstract class JsInteropBase : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    /// <summary>
    /// Initializes a new instance of the JS interop base class.
    /// </summary>
    /// <param name="jsRuntime">The JS runtime instance.</param>
    /// <param name="modulePath">The path to the JS module. Defaults to the SummitUI bundle.</param>
    protected JsInteropBase(IJSRuntime jsRuntime, string modulePath = "./_content/SummitUI/summitui.js")
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath).AsTask());
    }

    /// <summary>
    /// Gets the lazily-loaded JS module reference.
    /// </summary>
    /// <returns>The JS module object reference.</returns>
    protected ValueTask<IJSObjectReference> GetModuleAsync() => new(_moduleTask.Value);

    /// <summary>
    /// Disposes the JS module reference.
    /// Safely handles JSDisconnectedException which occurs when the Blazor Server circuit disconnects.
    /// </summary>
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
                // Safe to ignore - JS resources are cleaned up by the browser when the circuit disconnects
            }
        }
    }
}
