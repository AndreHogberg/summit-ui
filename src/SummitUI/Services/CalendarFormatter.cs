using System.Globalization;

namespace SummitUI.Services;

/// <summary>
/// Provides localized date formatting using .NET CultureInfo.
/// </summary>
public sealed class CalendarFormatter
{
    /// <summary>
    /// Gets the first day of the week for the specified culture.
    /// </summary>
    /// <param name="culture">The culture to get the first day of week for.</param>
    /// <returns>The first day of the week.</returns>
    public DayOfWeek GetFirstDayOfWeek(CultureInfo culture)
    {
        return culture.DateTimeFormat.FirstDayOfWeek;
    }

    /// <summary>
    /// Gets localized weekday names (short and long forms).
    /// Arrays are indexed from Sunday (0) to Saturday (6).
    /// </summary>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <returns>Object containing short and long weekday names.</returns>
    public WeekdayNames GetWeekdayNames(CultureInfo culture)
    {
        var dtf = culture.DateTimeFormat;

        // .NET's day name arrays are already indexed from Sunday (0) to Saturday (6)
        var shortNames = dtf.AbbreviatedDayNames;
        var longNames = dtf.DayNames;

        return new WeekdayNames(shortNames, longNames);
    }

    /// <summary>
    /// Gets the localized month name for the specified date.
    /// </summary>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <param name="date">The date.</param>
    /// <returns>The localized month name.</returns>
    public string GetMonthName(CultureInfo culture, DateOnly date)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);
        return dateTime.ToString("MMMM", culture);
    }

    /// <summary>
    /// Gets the localized month and year heading.
    /// </summary>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <param name="date">The date (first day of month).</param>
    /// <returns>The localized month and year string.</returns>
    public string GetMonthYearHeading(CultureInfo culture, DateOnly date)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);

        // Use year/month format pattern, or fall back to "MMMM yyyy"
        return dateTime.ToString("Y", culture);
    }

    /// <summary>
    /// Gets the full localized date string for accessibility (aria-label).
    /// </summary>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <param name="date">The date.</param>
    /// <returns>The full localized date string.</returns>
    public string GetFullDateString(CultureInfo culture, DateOnly date)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);

        // "D" is the long date pattern (e.g., "Friday, January 7, 2026")
        return dateTime.ToString("D", culture);
    }

    /// <summary>
    /// Formats a date using the specified format pattern.
    /// </summary>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <param name="date">The date.</param>
    /// <param name="format">The format pattern.</param>
    /// <returns>The formatted date string.</returns>
    public string FormatDate(CultureInfo culture, DateOnly date, string format)
    {
        var dateTime = date.ToDateTime(TimeOnly.MinValue);
        return dateTime.ToString(format, culture);
    }

    /// <summary>
    /// Gets the month names for the specified culture.
    /// </summary>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <returns>Array of month names (1-indexed, so index 0 is empty or January depending on culture).</returns>
    public string[] GetMonthNames(CultureInfo culture)
    {
        return culture.DateTimeFormat.MonthNames;
    }
}

/// <summary>
/// Represents localized weekday names.
/// </summary>
/// <param name="Short">Abbreviated weekday names (Sun, Mon, etc.), indexed 0-6 from Sunday.</param>
/// <param name="Long">Full weekday names (Sunday, Monday, etc.), indexed 0-6 from Sunday.</param>
public readonly record struct WeekdayNames(string[] Short, string[] Long);
