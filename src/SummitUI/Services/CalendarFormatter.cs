using System.Globalization;

namespace SummitUI.Services;

/// <summary>
/// Provides localized date formatting using .NET CultureInfo.
/// </summary>
public sealed class CalendarFormatter
{
    private readonly CalendarProvider _calendarProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CalendarFormatter"/> class.
    /// </summary>
    /// <param name="calendarProvider">The calendar provider for calendar system operations.</param>
    public CalendarFormatter(CalendarProvider calendarProvider)
    {
        _calendarProvider = calendarProvider;
    }

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
    /// Gets the localized month name for the specified calendar date.
    /// </summary>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <param name="gregorianDate">The Gregorian date.</param>
    /// <param name="calendarSystem">The calendar system for display.</param>
    /// <returns>The localized month name.</returns>
    public string GetMonthName(CultureInfo culture, DateOnly gregorianDate, CalendarSystem calendarSystem)
    {
        var calendar = _calendarProvider.GetCalendar(calendarSystem);
        var formatCulture = CreateCultureWithCalendar(culture, calendar);

        var dateTime = gregorianDate.ToDateTime(TimeOnly.MinValue);
        return dateTime.ToString("MMMM", formatCulture);
    }

    /// <summary>
    /// Gets the localized month and year heading.
    /// </summary>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <param name="gregorianDate">The Gregorian date (first day of month).</param>
    /// <param name="calendarSystem">The calendar system for display.</param>
    /// <returns>The localized month and year string.</returns>
    public string GetMonthYearHeading(CultureInfo culture, DateOnly gregorianDate, CalendarSystem calendarSystem)
    {
        var calendar = _calendarProvider.GetCalendar(calendarSystem);
        var formatCulture = CreateCultureWithCalendar(culture, calendar);

        var dateTime = gregorianDate.ToDateTime(TimeOnly.MinValue);

        // Use year/month format pattern, or fall back to "MMMM yyyy"
        return dateTime.ToString("Y", formatCulture);
    }

    /// <summary>
    /// Gets the full localized date string for accessibility (aria-label).
    /// </summary>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <param name="gregorianDate">The Gregorian date.</param>
    /// <param name="calendarSystem">The calendar system for display.</param>
    /// <returns>The full localized date string.</returns>
    public string GetFullDateString(CultureInfo culture, DateOnly gregorianDate, CalendarSystem calendarSystem)
    {
        var calendar = _calendarProvider.GetCalendar(calendarSystem);
        var formatCulture = CreateCultureWithCalendar(culture, calendar);

        var dateTime = gregorianDate.ToDateTime(TimeOnly.MinValue);

        // "D" is the long date pattern (e.g., "Friday, January 7, 2026")
        return dateTime.ToString("D", formatCulture);
    }

    /// <summary>
    /// Formats a date using the specified format pattern.
    /// </summary>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <param name="gregorianDate">The Gregorian date.</param>
    /// <param name="format">The format pattern.</param>
    /// <param name="calendarSystem">The calendar system for display.</param>
    /// <returns>The formatted date string.</returns>
    public string FormatDate(CultureInfo culture, DateOnly gregorianDate, string format, CalendarSystem calendarSystem)
    {
        var calendar = _calendarProvider.GetCalendar(calendarSystem);
        var formatCulture = CreateCultureWithCalendar(culture, calendar);

        var dateTime = gregorianDate.ToDateTime(TimeOnly.MinValue);
        return dateTime.ToString(format, formatCulture);
    }

    /// <summary>
    /// Gets the month names for the specified culture and calendar.
    /// </summary>
    /// <param name="culture">The culture to use for formatting.</param>
    /// <param name="calendarSystem">The calendar system.</param>
    /// <returns>Array of month names (1-indexed, so index 0 is empty or January depending on culture).</returns>
    public string[] GetMonthNames(CultureInfo culture, CalendarSystem calendarSystem)
    {
        var calendar = _calendarProvider.GetCalendar(calendarSystem);
        var formatCulture = CreateCultureWithCalendar(culture, calendar);

        return formatCulture.DateTimeFormat.MonthNames;
    }

    /// <summary>
    /// Creates a clone of the culture with the specified calendar set.
    /// </summary>
    private static CultureInfo CreateCultureWithCalendar(CultureInfo culture, Calendar calendar)
    {
        // Always try to set the calendar on a cloned culture
        var clone = (CultureInfo)culture.Clone();
        
        try
        {
            clone.DateTimeFormat.Calendar = calendar;
            return clone;
        }
        catch (ArgumentException)
        {
            // The calendar is not in the culture's OptionalCalendars list.
            // Try to find a culture that natively supports this calendar.
            var fallbackCulture = FindCultureForCalendar(calendar);
            if (fallbackCulture != null)
            {
                var fallbackClone = (CultureInfo)fallbackCulture.Clone();
                try
                {
                    fallbackClone.DateTimeFormat.Calendar = calendar;
                    return fallbackClone;
                }
                catch (ArgumentException)
                {
                    // Still failed, return original culture
                }
            }
            
            return culture;
        }
    }
    
    /// <summary>
    /// Finds a culture that natively supports the given calendar type.
    /// </summary>
    private static CultureInfo? FindCultureForCalendar(Calendar calendar)
    {
        return calendar switch
        {
            HebrewCalendar => new CultureInfo("he-IL"),
            HijriCalendar => new CultureInfo("ar-SA"),
            UmAlQuraCalendar => new CultureInfo("ar-SA"),
            PersianCalendar => new CultureInfo("fa-IR"),
            JapaneseCalendar => new CultureInfo("ja-JP"),
            ThaiBuddhistCalendar => new CultureInfo("th-TH"),
            TaiwanCalendar => new CultureInfo("zh-TW"),
            _ => null
        };
    }
}

/// <summary>
/// Represents localized weekday names.
/// </summary>
/// <param name="Short">Abbreviated weekday names (Sun, Mon, etc.), indexed 0-6 from Sunday.</param>
/// <param name="Long">Full weekday names (Sunday, Monday, etc.), indexed 0-6 from Sunday.</param>
public readonly record struct WeekdayNames(string[] Short, string[] Long);
