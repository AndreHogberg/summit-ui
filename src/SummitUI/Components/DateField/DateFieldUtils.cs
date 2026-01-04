using System.Text;

namespace SummitUI;

/// <summary>
/// Utility methods for parsing date/time format patterns into segments.
/// </summary>
internal static class DateFieldUtils
{
    /// <summary>
    /// Gets segments for the date field based on granularity and culture.
    /// </summary>
    public static List<DateFieldSegmentState> GetSegments(
        DateFieldContext context)
    {
        var segments = new List<DateFieldSegmentState>();
        var culture = context.Culture;
        
        // Use custom date pattern if provided, otherwise fall back to culture's short date pattern
        var datePattern = !string.IsNullOrEmpty(context.DatePattern) 
            ? context.DatePattern 
            : culture.DateTimeFormat.ShortDatePattern;
        segments.AddRange(ParsePattern(datePattern));
        
        // Add time segments if granularity requires it
        if (context.Granularity != DateFieldGranularity.Day)
        {
            // Add separator between date and time
            segments.Add(new DateFieldSegmentState 
            { 
                Type = DateFieldSegmentType.Literal, 
                LiteralValue = " " 
            });
            
            // Get time segments based on granularity and hour cycle
            var timeSegments = GetTimeSegments(context);
            segments.AddRange(timeSegments);
        }
        
        return segments;
    }

    /// <summary>
    /// Parses a date format pattern into segments.
    /// </summary>
    private static List<DateFieldSegmentState> ParsePattern(string pattern)
    {
        var segments = new List<DateFieldSegmentState>();
        var currentPart = new StringBuilder();
        char? lastDateChar = null;

        foreach (char c in pattern)
        {
            if (IsDateChar(c))
            {
                // If we were building a literal, save it
                if (lastDateChar == null && currentPart.Length > 0)
                {
                    segments.Add(new DateFieldSegmentState 
                    { 
                        Type = DateFieldSegmentType.Literal, 
                        LiteralValue = currentPart.ToString() 
                    });
                    currentPart.Clear();
                }
                // If switching to a different date char, save previous segment
                else if (lastDateChar != null && lastDateChar != c && currentPart.Length > 0)
                {
                    segments.Add(CreateDateSegment(currentPart.ToString()));
                    currentPart.Clear();
                }
                
                currentPart.Append(c);
                lastDateChar = c;
            }
            else
            {
                // If we were building a date segment, save it
                if (lastDateChar != null && currentPart.Length > 0)
                {
                    segments.Add(CreateDateSegment(currentPart.ToString()));
                    currentPart.Clear();
                }
                
                currentPart.Append(c);
                lastDateChar = null;
            }
        }
        
        // Handle remaining content
        if (currentPart.Length > 0)
        {
            if (lastDateChar != null)
            {
                segments.Add(CreateDateSegment(currentPart.ToString()));
            }
            else
            {
                segments.Add(new DateFieldSegmentState 
                { 
                    Type = DateFieldSegmentType.Literal, 
                    LiteralValue = currentPart.ToString() 
                });
            }
        }

        return segments;
    }

    /// <summary>
    /// Gets time segments based on granularity and hour cycle settings.
    /// </summary>
    private static List<DateFieldSegmentState> GetTimeSegments(DateFieldContext context)
    {
        var segments = new List<DateFieldSegmentState>();
        var culture = context.Culture;
        var use12Hour = context.Uses12HourClock();
        var timeSeparator = culture.DateTimeFormat.TimeSeparator;
        
        // Hour
        segments.Add(new DateFieldSegmentState { Type = DateFieldSegmentType.Hour });
        
        // Hour:Minute separator
        if (context.Granularity >= DateFieldGranularity.Minute)
        {
            segments.Add(new DateFieldSegmentState 
            { 
                Type = DateFieldSegmentType.Literal, 
                LiteralValue = timeSeparator 
            });
            segments.Add(new DateFieldSegmentState { Type = DateFieldSegmentType.Minute });
        }
        
        // Minute:Second separator
        if (context.Granularity >= DateFieldGranularity.Second)
        {
            segments.Add(new DateFieldSegmentState 
            { 
                Type = DateFieldSegmentType.Literal, 
                LiteralValue = timeSeparator 
            });
            segments.Add(new DateFieldSegmentState { Type = DateFieldSegmentType.Second });
        }
        
        // AM/PM indicator for 12-hour clocks
        if (use12Hour)
        {
            segments.Add(new DateFieldSegmentState 
            { 
                Type = DateFieldSegmentType.Literal, 
                LiteralValue = " " 
            });
            segments.Add(new DateFieldSegmentState { Type = DateFieldSegmentType.DayPeriod });
        }
        
        return segments;
    }

    private static bool IsDateChar(char c) => c == 'd' || c == 'M' || c == 'y';

    private static DateFieldSegmentState CreateDateSegment(string format)
    {
        var type = format[0] switch
        {
            'd' => DateFieldSegmentType.Day,
            'M' => DateFieldSegmentType.Month,
            'y' => DateFieldSegmentType.Year,
            _ => DateFieldSegmentType.Literal
        };
        
        if (type == DateFieldSegmentType.Literal)
        {
            return new DateFieldSegmentState { Type = type, LiteralValue = format };
        }
        
        return new DateFieldSegmentState { Type = type };
    }

    /// <summary>
    /// Gets the minimum value for a segment type.
    /// </summary>
    public static int GetSegmentMin(DateFieldSegmentType type, DateFieldContext context)
    {
        return type switch
        {
            DateFieldSegmentType.Year => 1,
            DateFieldSegmentType.Month => 1,
            DateFieldSegmentType.Day => 1,
            DateFieldSegmentType.Hour => context.Uses12HourClock() ? 1 : 0,
            DateFieldSegmentType.Minute => 0,
            DateFieldSegmentType.Second => 0,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the maximum value for a segment type.
    /// </summary>
    public static int GetSegmentMax(DateFieldSegmentType type, DateFieldContext context)
    {
        var effectiveDate = context.EffectiveDateTime;
        
        return type switch
        {
            DateFieldSegmentType.Year => 9999,
            DateFieldSegmentType.Month => 12,
            DateFieldSegmentType.Day => DateTime.DaysInMonth(effectiveDate.Year, effectiveDate.Month),
            DateFieldSegmentType.Hour => context.Uses12HourClock() ? 12 : 23,
            DateFieldSegmentType.Minute => 59,
            DateFieldSegmentType.Second => 59,
            _ => 0
        };
    }

    /// <summary>
    /// Gets the current value for a segment from the DateTime.
    /// </summary>
    public static int GetSegmentValue(DateFieldSegmentType type, DateTime dateTime, bool use12Hour)
    {
        return type switch
        {
            DateFieldSegmentType.Year => dateTime.Year,
            DateFieldSegmentType.Month => dateTime.Month,
            DateFieldSegmentType.Day => dateTime.Day,
            DateFieldSegmentType.Hour => use12Hour ? (dateTime.Hour % 12 == 0 ? 12 : dateTime.Hour % 12) : dateTime.Hour,
            DateFieldSegmentType.Minute => dateTime.Minute,
            DateFieldSegmentType.Second => dateTime.Second,
            _ => 0
        };
    }

    /// <summary>
    /// Formats a segment value for display.
    /// </summary>
    public static string FormatSegmentValue(DateFieldSegmentType type, DateTime dateTime, DateFieldContext context)
    {
        var culture = context.Culture;
        var use12Hour = context.Uses12HourClock();
        
        return type switch
        {
            DateFieldSegmentType.Year => dateTime.Year.ToString("0000"),
            DateFieldSegmentType.Month => dateTime.Month.ToString("00"),
            DateFieldSegmentType.Day => dateTime.Day.ToString("00"),
            DateFieldSegmentType.Hour => use12Hour 
                ? (dateTime.Hour % 12 == 0 ? 12 : dateTime.Hour % 12).ToString("00")
                : dateTime.Hour.ToString("00"),
            DateFieldSegmentType.Minute => dateTime.Minute.ToString("00"),
            DateFieldSegmentType.Second => dateTime.Second.ToString("00"),
            DateFieldSegmentType.DayPeriod => dateTime.Hour < 12 
                ? culture.DateTimeFormat.AMDesignator 
                : culture.DateTimeFormat.PMDesignator,
            _ => ""
        };
    }
}
