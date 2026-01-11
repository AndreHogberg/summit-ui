using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Base;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop for Calendar component.
/// Handles keyboard event interception and focus management that require DOM access.
/// </summary>
/// <remarks>
/// Date conversion and locale formatting have been moved to <see cref="Services.CalendarProvider"/> 
/// and <see cref="Services.CalendarFormatter"/> which use .NET's System.Globalization.
/// </remarks>
public sealed class CalendarJsInterop(IJSRuntime jsRuntime) : JsInteropBase(jsRuntime)
{
    /// <summary>
    /// Initializes keyboard navigation support for the calendar grid.
    /// This sets up event handlers that prevent default browser behavior for navigation keys.
    /// </summary>
    public async ValueTask InitializeCalendarAsync(ElementReference element)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("calendar_initializeCalendar", element);
    }

    /// <summary>
    /// Focuses a specific date button element.
    /// </summary>
    public async ValueTask FocusDateAsync(ElementReference element)
    {
        try
        {
            var module = await GetModuleAsync();
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

    /// <summary>
    /// Cleanup the calendar event listeners.
    /// </summary>
    public async ValueTask DestroyCalendarAsync(ElementReference element)
    {
        try
        {
            var module = await GetModuleAsync();
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
