using System.Text;

namespace SummitUI;

/// <summary>
/// Utility methods for parsing date/time format patterns into segments.
/// </summary>
internal static class DateFieldUtils
{
    /// <summary>
    /// Gets segments for the date field based on format and mode.
    /// </summary>
    public static List<DateFieldSegmentState> GetSegments(DateFieldContext context)
    {
        var segments = new List<DateFieldSegmentState>();

        // Parse date format into segments
        segments.AddRange(ParsePattern(context.Format));

        // Add time segments if in DateTime mode
        if (context.IsDateTimeMode)
        {
            // Add separator between date and time
            segments.Add(new DateFieldSegmentState
            {
                Type = DateFieldSegmentType.Literal,
                LiteralValue = " "
            });

            // Get time segments based on TimeFormat
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
    /// Gets time segments based on TimeFormat setting.
    /// </summary>
    private static List<DateFieldSegmentState> GetTimeSegments(DateFieldContext context)
    {
        var segments = new List<DateFieldSegmentState>();
        var timeSeparator = context.GetTimeSeparator();
        var use12Hour = context.Uses12HourClock();

        // Hour
        segments.Add(new DateFieldSegmentState { Type = DateFieldSegmentType.Hour });

        // Hour:Minute separator and minute
        segments.Add(new DateFieldSegmentState
        {
            Type = DateFieldSegmentType.Literal,
            LiteralValue = timeSeparator
        });
        segments.Add(new DateFieldSegmentState { Type = DateFieldSegmentType.Minute });

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
            _ => 0
        };
    }

    /// <summary>
    /// Gets the placeholder text for a segment type.
    /// Returns lowercase format indicators like "yyyy", "mm", "dd".
    /// </summary>
    public static string GetSegmentPlaceholder(DateFieldSegmentType type, DateFieldContext context)
    {
        return type switch
        {
            DateFieldSegmentType.Year => "yyyy",
            DateFieldSegmentType.Month => "mm",
            DateFieldSegmentType.Day => "dd",
            DateFieldSegmentType.Hour => "hh",
            DateFieldSegmentType.Minute => "mm",
            DateFieldSegmentType.DayPeriod => context.GetAmDesignator().ToLowerInvariant(),
            _ => ""
        };
    }

    /// <summary>
    /// Formats a segment value for display, showing placeholder if segment has no value.
    /// </summary>
    public static string FormatSegmentValue(DateFieldSegmentType type, DateFieldContext context)
    {
        // Check if this segment has a value
        if (!context.SegmentHasValue(type))
        {
            return GetSegmentPlaceholder(type, context);
        }

        // DayPeriod is special - uses boolean flag
        if (type == DateFieldSegmentType.DayPeriod)
        {
            var isPm = context.GetPartialIsPm();
            if (!isPm.HasValue)
            {
                return GetSegmentPlaceholder(type, context);
            }
            return isPm.Value
                ? context.GetPmDesignator()
                : context.GetAmDesignator();
        }

        // Get the value from context
        var value = context.GetSegmentValue(type);
        if (!value.HasValue)
        {
            return GetSegmentPlaceholder(type, context);
        }

        // Format the value
        return type switch
        {
            DateFieldSegmentType.Year => value.Value.ToString("0000"),
            DateFieldSegmentType.Month => value.Value.ToString("00"),
            DateFieldSegmentType.Day => value.Value.ToString("00"),
            DateFieldSegmentType.Hour => value.Value.ToString("00"),
            DateFieldSegmentType.Minute => value.Value.ToString("00"),
            _ => ""
        };
    }
}
