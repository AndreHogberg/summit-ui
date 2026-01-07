namespace SummitUI;

/// <summary>
/// Specifies the calendar system to use for display and navigation.
/// The bound Value remains as DateOnly (Gregorian), but dates are displayed
/// and navigated using the selected calendar system.
/// </summary>
/// <remarks>
/// Calendar systems are backed by .NET's System.Globalization.Calendar classes.
/// The available calendars are those natively supported by .NET.
/// </remarks>
public enum CalendarSystem
{
    /// <summary>
    /// The Gregorian calendar (default). The most commonly used calendar system worldwide.
    /// </summary>
    Gregorian,

    /// <summary>
    /// The Japanese calendar. Based on Gregorian but uses eras for each emperor's reign.
    /// Era names include Reiwa (令和), Heisei (平成), Showa (昭和), etc.
    /// </summary>
    Japanese,

    /// <summary>
    /// The Buddhist calendar. Same as Gregorian but years counted from Buddha's birth (543 BC).
    /// Year = Gregorian year + 543. Used in Thailand, Cambodia, Laos, Myanmar, Sri Lanka.
    /// </summary>
    Buddhist,

    /// <summary>
    /// The Taiwan (Republic of China) calendar. Same as Gregorian but years counted from 1912.
    /// Year = Gregorian year - 1911. Used in Taiwan.
    /// </summary>
    Taiwan,

    /// <summary>
    /// The Persian (Jalali) calendar. A solar calendar used in Iran and Afghanistan.
    /// Has 12 months with the first 6 having 31 days, next 5 having 30 days, and the last having 29 or 30.
    /// </summary>
    Persian,

    /// <summary>
    /// The Islamic Umm al-Qura calendar. A lunar calendar used in Saudi Arabia.
    /// Based on astronomical calculations predicting crescent moon sightings.
    /// </summary>
    IslamicUmalqura,

    /// <summary>
    /// The Islamic Civil calendar. A tabular/arithmetic lunar calendar.
    /// Uses the Hijri calendar with standard civil calculation.
    /// </summary>
    IslamicCivil,

    /// <summary>
    /// The Hebrew calendar. A lunisolar calendar used in Israel and for Jewish religious observances.
    /// Has either 12 or 13 months depending on leap year.
    /// </summary>
    Hebrew
}
