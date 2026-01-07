using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Interop;

/// <summary>
/// Represents localized weekday names.
/// </summary>
public record WeekdayNames(string[] Short, string[] Long);

/// <summary>
/// Represents a date converted from Gregorian to another calendar system.
/// </summary>
public record CalendarDateInfo(int Year, int Month, int Day, string Era);

/// <summary>
/// Represents information about a month in a calendar system.
/// </summary>
public record CalendarMonthInfo(int Year, int Month, int Day, string Era, int DaysInMonth, int MonthsInYear);

/// <summary>
/// JavaScript interop for Calendar component.
/// Provides locale detection, Intl API formatting, and calendar system conversions.
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
    /// <param name="locale">The locale to use for formatting.</param>
    /// <param name="year">The Gregorian year.</param>
    /// <param name="month">The Gregorian month (1-12).</param>
    /// <param name="calendarSystem">The calendar system for display.</param>
    public async ValueTask<string> GetMonthNameAsync(string locale, int year, int month, CalendarSystem calendarSystem = CalendarSystem.Gregorian)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("calendar_getMonthName", locale, year, month, (int)calendarSystem);
    }

    /// <summary>
    /// Gets the localized month and year heading.
    /// </summary>
    /// <param name="locale">The locale to use for formatting.</param>
    /// <param name="year">The Gregorian year.</param>
    /// <param name="month">The Gregorian month (1-12).</param>
    /// <param name="calendarSystem">The calendar system for display.</param>
    public async ValueTask<string> GetMonthYearHeadingAsync(string locale, int year, int month, CalendarSystem calendarSystem = CalendarSystem.Gregorian)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("calendar_getMonthYearHeading", locale, year, month, (int)calendarSystem);
    }

    /// <summary>
    /// Gets localized weekday names (short and long forms).
    /// Arrays are indexed from Sunday (0) to Saturday (6).
    /// </summary>
    /// <param name="locale">The locale to use for formatting.</param>
    /// <param name="calendarSystem">The calendar system (some calendars may have different weekday names).</param>
    public async ValueTask<WeekdayNames> GetWeekdayNamesAsync(string locale, CalendarSystem calendarSystem = CalendarSystem.Gregorian)
    {
        var module = await _moduleTask.Value;
        var result = await module.InvokeAsync<Dictionary<string, string[]>>("calendar_getWeekdayNames", locale, (int)calendarSystem);
        return new WeekdayNames(
            result.GetValueOrDefault("short", ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"]),
            result.GetValueOrDefault("long", ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"])
        );
    }

    /// <summary>
    /// Gets the full localized date string for accessibility.
    /// </summary>
    /// <param name="locale">The locale to use for formatting.</param>
    /// <param name="year">The Gregorian year.</param>
    /// <param name="month">The Gregorian month (1-12).</param>
    /// <param name="day">The Gregorian day.</param>
    /// <param name="calendarSystem">The calendar system for display.</param>
    public async ValueTask<string> GetFullDateStringAsync(string locale, int year, int month, int day, CalendarSystem calendarSystem = CalendarSystem.Gregorian)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("calendar_getFullDateString", locale, year, month, day, (int)calendarSystem);
    }

    #region Calendar System Conversion Methods

    /// <summary>
    /// Converts a Gregorian date to a date in the specified calendar system.
    /// </summary>
    /// <param name="gregorianYear">The Gregorian year.</param>
    /// <param name="gregorianMonth">The Gregorian month (1-12).</param>
    /// <param name="gregorianDay">The Gregorian day.</param>
    /// <param name="calendarSystem">The target calendar system.</param>
    /// <returns>The date in the target calendar system.</returns>
    public async ValueTask<CalendarDateInfo> ConvertFromGregorianAsync(int gregorianYear, int gregorianMonth, int gregorianDay, CalendarSystem calendarSystem)
    {
        var module = await _moduleTask.Value;
        var result = await module.InvokeAsync<Dictionary<string, object>>("calendar_convertFromGregorian", gregorianYear, gregorianMonth, gregorianDay, (int)calendarSystem);
        return new CalendarDateInfo(
            Convert.ToInt32(result["year"]),
            Convert.ToInt32(result["month"]),
            Convert.ToInt32(result["day"]),
            result["era"]?.ToString() ?? ""
        );
    }

    /// <summary>
    /// Converts a date from the specified calendar system to Gregorian.
    /// </summary>
    /// <param name="year">The year in the source calendar.</param>
    /// <param name="month">The month in the source calendar (1-based).</param>
    /// <param name="day">The day in the source calendar.</param>
    /// <param name="calendarSystem">The source calendar system.</param>
    /// <returns>The Gregorian date.</returns>
    public async ValueTask<(int Year, int Month, int Day)> ConvertToGregorianAsync(int year, int month, int day, CalendarSystem calendarSystem)
    {
        var module = await _moduleTask.Value;
        var result = await module.InvokeAsync<Dictionary<string, object>>("calendar_convertToGregorian", year, month, day, (int)calendarSystem);
        return (
            Convert.ToInt32(result["year"]),
            Convert.ToInt32(result["month"]),
            Convert.ToInt32(result["day"])
        );
    }

    /// <summary>
    /// Gets calendar month information for a given Gregorian date and calendar system.
    /// </summary>
    /// <param name="gregorianYear">The Gregorian year.</param>
    /// <param name="gregorianMonth">The Gregorian month (1-12).</param>
    /// <param name="calendarSystem">The calendar system.</param>
    /// <returns>Information about the month in the calendar system.</returns>
    public async ValueTask<CalendarMonthInfo> GetCalendarMonthInfoAsync(int gregorianYear, int gregorianMonth, CalendarSystem calendarSystem)
    {
        var module = await _moduleTask.Value;
        var result = await module.InvokeAsync<Dictionary<string, object>>("calendar_getCalendarMonthInfo", gregorianYear, gregorianMonth, (int)calendarSystem);
        return new CalendarMonthInfo(
            Convert.ToInt32(result["year"]),
            Convert.ToInt32(result["month"]),
            Convert.ToInt32(result["day"]),
            result["era"]?.ToString() ?? "",
            Convert.ToInt32(result["daysInMonth"]),
            Convert.ToInt32(result["monthsInYear"])
        );
    }

    /// <summary>
    /// Gets the number of days in a month for a given calendar system.
    /// </summary>
    /// <param name="year">The year in the calendar system.</param>
    /// <param name="month">The month in the calendar system (1-based).</param>
    /// <param name="calendarSystem">The calendar system.</param>
    /// <returns>The number of days in the month.</returns>
    public async ValueTask<int> GetDaysInMonthAsync(int year, int month, CalendarSystem calendarSystem)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<int>("calendar_getDaysInMonth", year, month, (int)calendarSystem);
    }

    /// <summary>
    /// Gets the number of months in a year for a given calendar system.
    /// This is important for calendars like Hebrew which can have 12 or 13 months.
    /// </summary>
    /// <param name="year">The year in the calendar system.</param>
    /// <param name="calendarSystem">The calendar system.</param>
    /// <returns>The number of months in the year.</returns>
    public async ValueTask<int> GetMonthsInYearAsync(int year, CalendarSystem calendarSystem)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<int>("calendar_getMonthsInYear", year, (int)calendarSystem);
    }

    /// <summary>
    /// Batch converts multiple Gregorian dates to the specified calendar system.
    /// More efficient than calling ConvertFromGregorianAsync for each date individually.
    /// </summary>
    /// <param name="locale">The locale for formatting localized date strings.</param>
    /// <param name="dates">Array of Gregorian dates as [year, month, day] arrays.</param>
    /// <param name="calendarSystem">The target calendar system.</param>
    /// <returns>Array of (day number, localized date string) for each input date.</returns>
    public async ValueTask<(int Day, string LocalizedDateString)[]> BatchConvertFromGregorianAsync(
        string locale, 
        DateOnly[] dates, 
        CalendarSystem calendarSystem)
    {
        if (calendarSystem == CalendarSystem.Gregorian || dates.Length == 0)
        {
            // For Gregorian, just return the day numbers with default formatting
            return dates.Select(d => (d.Day, $"{d:dddd, MMMM d, yyyy}")).ToArray();
        }

        var module = await _moduleTask.Value;
        
        // Convert dates to array format for JS: [[year, month, day], ...]
        var datesArray = dates.Select(d => new[] { d.Year, d.Month, d.Day }).ToArray();
        
        var results = await module.InvokeAsync<Dictionary<string, object>[]>(
            "calendar_batchConvertFromGregorian", 
            locale, 
            datesArray, 
            (int)calendarSystem);
        
        return results.Select(r => 
        {
            // Handle JsonElement from JS interop - numbers come as JsonElement, not boxed int
            var dayValue = r["day"];
            int day;
            if (dayValue is System.Text.Json.JsonElement jsonElement)
            {
                day = jsonElement.GetInt32();
            }
            else
            {
                day = Convert.ToInt32(dayValue);
            }
            
            var localizedDate = r["localizedDate"]?.ToString() ?? "";
            return (day, localizedDate);
        }).ToArray();
    }

    #endregion

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
