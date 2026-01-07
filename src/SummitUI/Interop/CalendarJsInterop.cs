using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop for Calendar component.
/// Handles keyboard event interception and focus management that require DOM access.
/// </summary>
/// <remarks>
/// Date conversion and locale formatting have been moved to <see cref="Services.CalendarProvider"/> 
/// and <see cref="Services.CalendarFormatter"/> which use .NET's System.Globalization.
/// </remarks>
public sealed class CalendarJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());

    /// <summary>
    /// Initializes keyboard navigation support for the calendar grid.
    /// This sets up event handlers that prevent default browser behavior for navigation keys.
    /// </summary>
    public async ValueTask InitializeCalendarAsync(ElementReference element)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("calendar_initializeCalendar", element);
    }

    /// <summary>
    /// Focuses a specific date button element.
    /// </summary>
    public async ValueTask FocusDateAsync(ElementReference element)
    {
        if (_moduleTask.IsValueCreated)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.InvokeVoidAsync("calendar_focusDate", element);
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

    /// <summary>
    /// Cleanup the calendar event listeners.
    /// </summary>
    public async ValueTask DestroyCalendarAsync(ElementReference element)
    {
        if (_moduleTask.IsValueCreated)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.InvokeVoidAsync("calendar_destroyCalendar", element);
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
