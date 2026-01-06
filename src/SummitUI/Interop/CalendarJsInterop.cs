using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Interop;

/// <summary>
/// Represents localized weekday names.
/// </summary>
public record WeekdayNames(string[] Short, string[] Long);

/// <summary>
/// JavaScript interop for Calendar component.
/// Provides locale detection and Intl API formatting.
/// </summary>
public sealed class CalendarJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());

    /// <summary>
    /// Gets the browser's current locale.
    /// </summary>
    public async ValueTask<string> GetBrowserLocaleAsync()
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("calendar_getBrowserLocale");
    }

    /// <summary>
    /// Gets the first day of the week for a locale.
    /// </summary>
    /// <param name="locale">The locale to check (e.g., "en-US", "de-DE").</param>
    /// <returns>The first day of the week (0 = Sunday, 1 = Monday, etc.).</returns>
    public async ValueTask<int> GetFirstDayOfWeekAsync(string locale)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<int>("calendar_getFirstDayOfWeek", locale);
    }

    /// <summary>
    /// Gets the localized month name.
    /// </summary>
    public async ValueTask<string> GetMonthNameAsync(string locale, int year, int month)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("calendar_getMonthName", locale, year, month);
    }

    /// <summary>
    /// Gets the localized month and year heading.
    /// </summary>
    public async ValueTask<string> GetMonthYearHeadingAsync(string locale, int year, int month)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("calendar_getMonthYearHeading", locale, year, month);
    }

    /// <summary>
    /// Gets localized weekday names (short and long forms).
    /// Arrays are indexed from Sunday (0) to Saturday (6).
    /// </summary>
    public async ValueTask<WeekdayNames> GetWeekdayNamesAsync(string locale)
    {
        var module = await _moduleTask.Value;
        var result = await module.InvokeAsync<Dictionary<string, string[]>>("calendar_getWeekdayNames", locale);
        return new WeekdayNames(
            result.GetValueOrDefault("short", ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"]),
            result.GetValueOrDefault("long", ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"])
        );
    }

    /// <summary>
    /// Gets the full localized date string for accessibility.
    /// </summary>
    public async ValueTask<string> GetFullDateStringAsync(string locale, int year, int month, int day)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("calendar_getFullDateString", locale, year, month, day);
    }

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
