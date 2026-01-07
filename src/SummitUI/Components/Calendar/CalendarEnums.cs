namespace SummitUI;

/// <summary>
/// Specifies the calendar system to use for display and navigation.
/// The bound Value remains as DateOnly (Gregorian), but dates are displayed
/// and navigated using the selected calendar system.
/// </summary>
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
    /// The Indian National Calendar (Saka). Official civil calendar of India.
    /// Years numbered from the Saka era (78 AD Gregorian).
    /// </summary>
    Indian,

    /// <summary>
    /// The Islamic Umm al-Qura calendar. A lunar calendar used in Saudi Arabia.
    /// Based on astronomical calculations predicting crescent moon sightings.
    /// </summary>
    IslamicUmalqura,

    /// <summary>
    /// The Islamic Civil calendar. A tabular/arithmetic lunar calendar.
    /// Uses Friday, July 16, 622 CE (Julian) as epoch.
    /// </summary>
    IslamicCivil,

    /// <summary>
    /// The Islamic Tabular calendar. An algorithmic variant of the Islamic calendar.
    /// Uses Thursday, July 15, 622 CE (Julian) as epoch.
    /// </summary>
    IslamicTabular,

    /// <summary>
    /// The Hebrew calendar. A lunisolar calendar used in Israel and for Jewish religious observances.
    /// Has either 12 or 13 months depending on leap year.
    /// </summary>
    Hebrew,

    /// <summary>
    /// The Coptic calendar. Used by the Coptic Orthodox Church in Egypt.
    /// Has 12 months of 30 days plus 5 or 6 intercalary days.
    /// </summary>
    Coptic,

    /// <summary>
    /// The Ethiopic calendar. Official calendar of Ethiopia.
    /// Has 12 months of 30 days plus 5 or 6 intercalary days. Two eras: 'AA' and 'AM'.
    /// </summary>
    Ethiopic,

    /// <summary>
    /// The Ethiopic Amete Alem calendar. Same as Ethiopic but with a different era calculation.
    /// Uses only the 'AA' (Amete Alem) era.
    /// </summary>
    EthiopicAmeteAlem
}

/// <summary>
/// Specifies which day of the week the calendar should start on.
/// </summary>
public enum WeekStartsOn
{
    /// <summary>
    /// Week starts on Sunday (default for en-US).
    /// </summary>
    Sunday = 0,

    /// <summary>
    /// Week starts on Monday (default for most European locales).
    /// </summary>
    Monday = 1,

    /// <summary>
    /// Week starts on Tuesday.
    /// </summary>
    Tuesday = 2,

    /// <summary>
    /// Week starts on Wednesday.
    /// </summary>
    Wednesday = 3,

    /// <summary>
    /// Week starts on Thursday.
    /// </summary>
    Thursday = 4,

    /// <summary>
    /// Week starts on Friday (default for some Middle Eastern locales).
    /// </summary>
    Friday = 5,

    /// <summary>
    /// Week starts on Saturday (default for some Middle Eastern locales).
    /// </summary>
    Saturday = 6
}
