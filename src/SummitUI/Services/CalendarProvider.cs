using System.Globalization;

namespace SummitUI.Services;

/// <summary>
/// Provides calendar instances for the supported calendar systems.
/// Maps <see cref="CalendarSystem"/> enum values to .NET <see cref="Calendar"/> instances.
/// </summary>
public sealed class CalendarProvider
{
    /// <summary>
    /// Gets a .NET Calendar instance for the specified calendar system.
    /// </summary>
    /// <param name="calendarSystem">The calendar system to get.</param>
    /// <returns>A Calendar instance for the specified system.</returns>
    public Calendar GetCalendar(CalendarSystem calendarSystem) => calendarSystem switch
    {
        CalendarSystem.Gregorian => new GregorianCalendar(),
        CalendarSystem.Japanese => new JapaneseCalendar(),
        CalendarSystem.Buddhist => new ThaiBuddhistCalendar(),
        CalendarSystem.Taiwan => new TaiwanCalendar(),
        CalendarSystem.Persian => new PersianCalendar(),
        CalendarSystem.IslamicUmalqura => new UmAlQuraCalendar(),
        CalendarSystem.IslamicCivil => new HijriCalendar(),
        CalendarSystem.Hebrew => new HebrewCalendar(),
        _ => new GregorianCalendar()
    };

    /// <summary>
    /// Converts a Gregorian date to the specified calendar system.
    /// </summary>
    /// <param name="gregorianDate">The Gregorian date to convert.</param>
    /// <param name="calendarSystem">The target calendar system.</param>
    /// <returns>The year, month, day, and era in the target calendar.</returns>
    public CalendarDateInfo ConvertFromGregorian(DateOnly gregorianDate, CalendarSystem calendarSystem)
    {
        if (calendarSystem == CalendarSystem.Gregorian)
        {
            return new CalendarDateInfo(gregorianDate.Year, gregorianDate.Month, gregorianDate.Day, string.Empty);
        }

        var calendar = GetCalendar(calendarSystem);
        var dateTime = gregorianDate.ToDateTime(TimeOnly.MinValue);

        var year = calendar.GetYear(dateTime);
        var month = calendar.GetMonth(dateTime);
        var day = calendar.GetDayOfMonth(dateTime);
        var era = GetEraName(calendar, dateTime, calendarSystem);

        return new CalendarDateInfo(year, month, day, era);
    }

    /// <summary>
    /// Converts a date from the specified calendar system to Gregorian.
    /// </summary>
    /// <param name="year">The year in the source calendar.</param>
    /// <param name="month">The month in the source calendar (1-based).</param>
    /// <param name="day">The day in the source calendar.</param>
    /// <param name="calendarSystem">The source calendar system.</param>
    /// <returns>The Gregorian DateOnly.</returns>
    public DateOnly ConvertToGregorian(int year, int month, int day, CalendarSystem calendarSystem)
    {
        if (calendarSystem == CalendarSystem.Gregorian)
        {
            return new DateOnly(year, month, day);
        }

        var calendar = GetCalendar(calendarSystem);
        var era = calendar.GetEra(DateTime.Now); // Use current era for conversion
        var dateTime = calendar.ToDateTime(year, month, day, 0, 0, 0, 0, era);

        return DateOnly.FromDateTime(dateTime);
    }

    /// <summary>
    /// Gets the number of days in a month for the specified calendar system.
    /// </summary>
    /// <param name="year">The year in the calendar system.</param>
    /// <param name="month">The month in the calendar system (1-based).</param>
    /// <param name="calendarSystem">The calendar system.</param>
    /// <returns>The number of days in the month.</returns>
    public int GetDaysInMonth(int year, int month, CalendarSystem calendarSystem)
    {
        var calendar = GetCalendar(calendarSystem);
        var era = calendar.GetEra(DateTime.Now);
        return calendar.GetDaysInMonth(year, month, era);
    }

    /// <summary>
    /// Gets the number of months in a year for the specified calendar system.
    /// This is important for calendars like Hebrew which can have 12 or 13 months.
    /// </summary>
    /// <param name="year">The year in the calendar system.</param>
    /// <param name="calendarSystem">The calendar system.</param>
    /// <returns>The number of months in the year.</returns>
    public int GetMonthsInYear(int year, CalendarSystem calendarSystem)
    {
        var calendar = GetCalendar(calendarSystem);
        var era = calendar.GetEra(DateTime.Now);
        return calendar.GetMonthsInYear(year, era);
    }

    /// <summary>
    /// Gets calendar month information for a given Gregorian date and calendar system.
    /// </summary>
    /// <param name="gregorianDate">The first day of the Gregorian month.</param>
    /// <param name="calendarSystem">The calendar system.</param>
    /// <returns>Information about the month in the calendar system.</returns>
    public CalendarMonthInfo GetCalendarMonthInfo(DateOnly gregorianDate, CalendarSystem calendarSystem)
    {
        var dateInfo = ConvertFromGregorian(gregorianDate, calendarSystem);
        var daysInMonth = GetDaysInMonth(dateInfo.Year, dateInfo.Month, calendarSystem);
        var monthsInYear = GetMonthsInYear(dateInfo.Year, calendarSystem);

        return new CalendarMonthInfo(
            dateInfo.Year,
            dateInfo.Month,
            dateInfo.Day,
            dateInfo.Era,
            daysInMonth,
            monthsInYear
        );
    }

    /// <summary>
    /// Batch converts multiple Gregorian dates to the specified calendar system.
    /// </summary>
    /// <param name="culture">The culture for formatting localized date strings.</param>
    /// <param name="dates">The Gregorian dates to convert.</param>
    /// <param name="calendarSystem">The target calendar system.</param>
    /// <returns>Array of (day number, localized date string) for each input date.</returns>
    public (int Day, string LocalizedDateString)[] BatchConvertFromGregorian(
        CultureInfo culture,
        DateOnly[] dates,
        CalendarSystem calendarSystem)
    {
        if (dates.Length == 0)
        {
            return [];
        }

        var results = new (int Day, string LocalizedDateString)[dates.Length];
        var calendar = GetCalendar(calendarSystem);

        // Create a culture with the specified calendar for proper formatting
        var formatCulture = CreateCultureWithCalendar(culture, calendar);

        for (var i = 0; i < dates.Length; i++)
        {
            var date = dates[i];
            var dateTime = date.ToDateTime(TimeOnly.MinValue);

            int day;
            string localizedDate;

            if (calendarSystem == CalendarSystem.Gregorian)
            {
                day = date.Day;
                localizedDate = dateTime.ToString("D", formatCulture);
            }
            else
            {
                day = calendar.GetDayOfMonth(dateTime);
                localizedDate = dateTime.ToString("D", formatCulture);
            }

            results[i] = (day, localizedDate);
        }

        return results;
    }

    /// <summary>
    /// Gets the era name for the given date in the specified calendar.
    /// </summary>
    private static string GetEraName(Calendar calendar, DateTime dateTime, CalendarSystem calendarSystem)
    {
        var era = calendar.GetEra(dateTime);

        // Only some calendars have meaningful era names
        return calendarSystem switch
        {
            CalendarSystem.Japanese => GetJapaneseEraName(era),
            _ => string.Empty
        };
    }

    /// <summary>
    /// Gets the Japanese era name for the given era number.
    /// </summary>
    private static string GetJapaneseEraName(int era) => era switch
    {
        5 => "令和", // Reiwa (2019-)
        4 => "平成", // Heisei (1989-2019)
        3 => "昭和", // Showa (1926-1989)
        2 => "大正", // Taisho (1912-1926)
        1 => "明治", // Meiji (1868-1912)
        _ => string.Empty
    };

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
/// Represents a date converted from Gregorian to another calendar system.
/// </summary>
/// <param name="Year">The year in the target calendar.</param>
/// <param name="Month">The month in the target calendar (1-based).</param>
/// <param name="Day">The day in the target calendar.</param>
/// <param name="Era">The era name (for era-based calendars like Japanese).</param>
public readonly record struct CalendarDateInfo(int Year, int Month, int Day, string Era);

/// <summary>
/// Represents information about a month in a calendar system.
/// </summary>
/// <param name="Year">The year in the calendar system.</param>
/// <param name="Month">The month in the calendar system (1-based).</param>
/// <param name="Day">The day in the calendar system.</param>
/// <param name="Era">The era name (for era-based calendars).</param>
/// <param name="DaysInMonth">Number of days in this month.</param>
/// <param name="MonthsInYear">Number of months in this year.</param>
public readonly record struct CalendarMonthInfo(int Year, int Month, int Day, string Era, int DaysInMonth, int MonthsInYear);
