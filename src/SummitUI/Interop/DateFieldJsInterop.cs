using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Interop;

/// <summary>
/// Represents the AM/PM designators.
/// </summary>
public record DayPeriodDesignators(string Am, string Pm);

/// <summary>
/// Represents the detected locale date format.
/// </summary>
public record LocaleDateFormat(string DateFormat, string DateSeparator);

/// <summary>
/// Represents date info in a calendar system.
/// </summary>
public record CalendarSegmentInfo(int Year, int Month, int Day, string Era, int DaysInMonth, int MonthsInYear);

/// <summary>
/// Internal DTO for deserializing calendar date info from JavaScript.
/// Property names match the JavaScript object property names (camelCase).
/// </summary>
internal record CalendarDateInfoDto(int year, int month, int day, string era, int daysInMonth, int monthsInYear)
{
    public CalendarSegmentInfo ToCalendarSegmentInfo() => new(year, month, day, era ?? "", daysInMonth, monthsInYear);
}

/// <summary>
/// Represents min/max bounds for a segment.
/// </summary>
public record SegmentBounds(int Min, int Max);

/// <summary>
/// Internal DTO for deserializing segment bounds from JavaScript.
/// </summary>
internal record SegmentBoundsDto(int min, int max)
{
    public SegmentBounds ToSegmentBounds() => new(min, max);
}

/// <summary>
/// Represents a Gregorian date result.
/// </summary>
public record GregorianDateResult(int Year, int Month, int Day);

/// <summary>
/// Internal DTO for deserializing Gregorian date from JavaScript.
/// </summary>
internal record GregorianDateDto(int year, int month, int day)
{
    public GregorianDateResult ToGregorianDateResult() => new(year, month, day);
}

public class DateFieldJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());

    public async ValueTask InitializeSegmentAsync(ElementReference element, DotNetObjectReference<DateFieldSegment> dotNetHelper)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dateField_initializeSegment", element, dotNetHelper);
    }

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
                // Ignored
            }
        }
    }

    /// <summary>
    /// Gets the browser's current locale (navigator.language).
    /// </summary>
    /// <returns>The browser locale string (e.g., "en-US", "sv-SE").</returns>
    public async ValueTask<string> GetBrowserLocaleAsync()
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("dateField_getBrowserLocale");
    }

    /// <summary>
    /// Gets localized segment labels from the browser using Intl.DisplayNames.
    /// </summary>
    /// <param name="locale">The locale to use (e.g., "en-US", "sv-SE").</param>
    /// <returns>Dictionary of segment labels keyed by segment type name.</returns>
    public async ValueTask<Dictionary<string, string>> GetSegmentLabelsAsync(string locale)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<Dictionary<string, string>>("dateField_getSegmentLabels", locale);
    }

    /// <summary>
    /// Gets localized AM/PM designators from the browser using Intl.DateTimeFormat.
    /// </summary>
    /// <param name="locale">The locale to use (e.g., "en-US", "sv-SE").</param>
    /// <returns>The AM and PM designator strings.</returns>
    public async ValueTask<DayPeriodDesignators> GetDayPeriodDesignatorsAsync(string locale)
    {
        var module = await _moduleTask.Value;
        var result = await module.InvokeAsync<Dictionary<string, string>>("dateField_getDayPeriodDesignators", locale);
        return new DayPeriodDesignators(
            result.GetValueOrDefault("am", "AM"),
            result.GetValueOrDefault("pm", "PM")
        );
    }

    #region Calendar System Support

    /// <summary>
    /// Gets the locale's preferred date format pattern.
    /// </summary>
    /// <param name="locale">The locale to detect format for.</param>
    /// <returns>The detected date format and separator.</returns>
    public async ValueTask<LocaleDateFormat> GetLocaleDateFormatAsync(string locale)
    {
        var module = await _moduleTask.Value;
        var result = await module.InvokeAsync<Dictionary<string, string>>("dateField_getLocaleDateFormat", locale);
        return new LocaleDateFormat(
            result.GetValueOrDefault("dateFormat", "yyyy-MM-dd"),
            result.GetValueOrDefault("dateSeparator", "-")
        );
    }

    /// <summary>
    /// Converts a Gregorian date to the specified calendar system and returns full info.
    /// </summary>
    /// <param name="gregorianYear">The Gregorian year.</param>
    /// <param name="gregorianMonth">The Gregorian month (1-12).</param>
    /// <param name="gregorianDay">The Gregorian day.</param>
    /// <param name="calendarSystem">The target calendar system.</param>
    /// <returns>Calendar date info including days in month and months in year.</returns>
    public async ValueTask<CalendarSegmentInfo> GetCalendarDateInfoAsync(int gregorianYear, int gregorianMonth, int gregorianDay, CalendarSystem calendarSystem)
    {
        var module = await _moduleTask.Value;
        var result = await module.InvokeAsync<CalendarDateInfoDto>("dateField_getCalendarDateInfo", gregorianYear, gregorianMonth, gregorianDay, (int)calendarSystem);
        return result.ToCalendarSegmentInfo();
    }

    /// <summary>
    /// Gets min/max values for a segment in a calendar system.
    /// </summary>
    /// <param name="segmentType">The segment type name (year, month, day, hour, minute).</param>
    /// <param name="gregorianYear">The Gregorian year.</param>
    /// <param name="gregorianMonth">The Gregorian month (1-12).</param>
    /// <param name="gregorianDay">The Gregorian day.</param>
    /// <param name="calendarSystem">The calendar system.</param>
    /// <param name="use12Hour">Whether using 12-hour clock.</param>
    /// <returns>The min and max values for the segment.</returns>
    public async ValueTask<SegmentBounds> GetSegmentBoundsAsync(string segmentType, int gregorianYear, int gregorianMonth, int gregorianDay, CalendarSystem calendarSystem, bool use12Hour)
    {
        var module = await _moduleTask.Value;
        var result = await module.InvokeAsync<SegmentBoundsDto>("dateField_getSegmentBounds", segmentType, gregorianYear, gregorianMonth, gregorianDay, (int)calendarSystem, use12Hour);
        return result.ToSegmentBounds();
    }

    /// <summary>
    /// Adds a value to a date segment respecting calendar system.
    /// </summary>
    /// <param name="gregorianYear">The Gregorian year.</param>
    /// <param name="gregorianMonth">The Gregorian month (1-12).</param>
    /// <param name="gregorianDay">The Gregorian day.</param>
    /// <param name="segmentType">The segment type (year, month, day).</param>
    /// <param name="amount">Amount to add (can be negative).</param>
    /// <param name="calendarSystem">The calendar system.</param>
    /// <returns>The new Gregorian date.</returns>
    public async ValueTask<GregorianDateResult> AddToDateSegmentAsync(int gregorianYear, int gregorianMonth, int gregorianDay, string segmentType, int amount, CalendarSystem calendarSystem)
    {
        var module = await _moduleTask.Value;
        var result = await module.InvokeAsync<GregorianDateDto>("dateField_addToDateSegment", gregorianYear, gregorianMonth, gregorianDay, segmentType, amount, (int)calendarSystem);
        return result.ToGregorianDateResult();
    }

    /// <summary>
    /// Sets a specific segment value in the calendar system and returns the new Gregorian date.
    /// </summary>
    /// <param name="gregorianYear">The Gregorian year.</param>
    /// <param name="gregorianMonth">The Gregorian month (1-12).</param>
    /// <param name="gregorianDay">The Gregorian day.</param>
    /// <param name="segmentType">The segment type (year, month, day).</param>
    /// <param name="newValue">The new value in the calendar system.</param>
    /// <param name="calendarSystem">The calendar system.</param>
    /// <returns>The new Gregorian date.</returns>
    public async ValueTask<GregorianDateResult> SetDateSegmentValueAsync(int gregorianYear, int gregorianMonth, int gregorianDay, string segmentType, int newValue, CalendarSystem calendarSystem)
    {
        var module = await _moduleTask.Value;
        var result = await module.InvokeAsync<GregorianDateDto>("dateField_setDateSegmentValue", gregorianYear, gregorianMonth, gregorianDay, segmentType, newValue, (int)calendarSystem);
        return result.ToGregorianDateResult();
    }

    #endregion

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
                // Ignored
            }
        }
    }
}
