namespace SummitUI;

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
