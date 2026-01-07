using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop for DateField component.
/// Handles keyboard event interception for segment navigation.
/// </summary>
/// <remarks>
/// Date conversion and locale formatting have been moved to <see cref="Services.CalendarProvider"/> 
/// and <see cref="Services.CalendarFormatter"/> which use .NET's System.Globalization.
/// </remarks>
public class DateFieldJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());

    /// <summary>
    /// Initializes keyboard handling for a date field segment.
    /// </summary>
    public async ValueTask InitializeSegmentAsync(ElementReference element, DotNetObjectReference<DateFieldSegment> dotNetHelper)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dateField_initializeSegment", element, dotNetHelper);
    }

    /// <summary>
    /// Cleans up keyboard handling for a date field segment.
    /// </summary>
    public async ValueTask DestroySegmentAsync(ElementReference element)
    {
        if (_moduleTask.IsValueCreated)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.InvokeVoidAsync("dateField_destroySegment", element);
            }
            catch (JSDisconnectedException)
            {
                // Ignored - Blazor Server circuit disconnected
            }
            catch (ObjectDisposedException)
            {
                // Ignored - JS object reference already disposed (WebAssembly)
            }
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
                // Ignored - Blazor Server circuit disconnected
            }
            catch (ObjectDisposedException)
            {
                // Ignored - JS object reference already disposed (WebAssembly)
            }
        }
    }
}
