namespace SummitUI;

/// <summary>
/// The type of a date field segment.
/// </summary>
public enum DateFieldSegmentType
{
    Day,
    Month,
    Year,
    Hour,
    Minute,
    Second,
    DayPeriod,
    Literal
}

/// <summary>
/// Specifies the granularity level for DateField.
/// Determines which segments are displayed.
/// </summary>
public enum DateFieldGranularity
{
    /// <summary>
    /// Show only date segments (Year, Month, Day).
    /// </summary>
    Day,
    
    /// <summary>
    /// Show date and hour segments.
    /// </summary>
    Hour,
    
    /// <summary>
    /// Show date, hour, and minute segments.
    /// </summary>
    Minute,
    
    /// <summary>
    /// Show date, hour, minute, and second segments.
    /// </summary>
    Second
}

/// <summary>
/// Specifies the hour cycle format for time display.
/// </summary>
public enum HourCycle
{
    /// <summary>
    /// Use the locale's default hour cycle.
    /// </summary>
    Auto,
    
    /// <summary>
    /// 12-hour cycle with AM/PM (1-12).
    /// </summary>
    H12,
    
    /// <summary>
    /// 24-hour cycle (0-23).
    /// </summary>
    H23,
    
    /// <summary>
    /// 24-hour cycle (1-24).
    /// </summary>
    H24,
    
    /// <summary>
    /// 12-hour cycle (0-11).
    /// </summary>
    H11
}
