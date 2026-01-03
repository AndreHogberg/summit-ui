using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Shared context for DateField components, managing state and segment coordination.
/// </summary>
public class DateFieldContext
{
    public string Id { get; } = Identifier.NewId();
    public string LabelId => $"{Id}-label";

    // Date value (DateOnly)
    public DateOnly? DateValue { get; private set; }
    public DateOnly DatePlaceholder { get; private set; } = DateOnly.FromDateTime(DateTime.Now);
    
    // DateTime value
    public DateTime? DateTimeValue { get; private set; }
    public DateTime DateTimePlaceholder { get; private set; } = DateTime.Now;
    
    // Indicates whether we're working with DateTime (true) or DateOnly (false)
    public bool IsDateTimeMode { get; private set; }
    
    // Configuration
    public DateFieldGranularity Granularity { get; private set; } = DateFieldGranularity.Day;
    public HourCycle HourCycle { get; private set; } = HourCycle.Auto;
    public CultureInfo Culture { get; private set; } = CultureInfo.CurrentCulture;
    
    // Validation constraints
    public DateOnly? MinDate { get; private set; }
    public DateOnly? MaxDate { get; private set; }
    public DateTime? MinDateTime { get; private set; }
    public DateTime? MaxDateTime { get; private set; }
    
    // States
    public bool Disabled { get; private set; }
    public bool ReadOnly { get; private set; }
    public bool Invalid { get; private set; }

    // Callbacks for value changes
    public EventCallback<DateOnly?> DateValueChanged { get; private set; }
    public EventCallback<DateTime?> DateTimeValueChanged { get; private set; }
    
    public event Action? OnStateChanged;

    /// <summary>
    /// Gets the effective DateTime value for display purposes.
    /// Uses the actual value if set, otherwise returns the placeholder.
    /// </summary>
    public DateTime EffectiveDateTime => IsDateTimeMode
        ? DateTimeValue ?? DateTimePlaceholder
        : (DateValue?.ToDateTime(TimeOnly.MinValue) ?? DatePlaceholder.ToDateTime(TimeOnly.MinValue));
    
    /// <summary>
    /// Gets whether a value has been set (not null).
    /// </summary>
    public bool HasValue => IsDateTimeMode ? DateTimeValue.HasValue : DateValue.HasValue;

    /// <summary>
    /// Gets whether the current value is out of the min/max range.
    /// </summary>
    public bool IsOutOfRange
    {
        get
        {
            if (IsDateTimeMode)
            {
                if (!DateTimeValue.HasValue) return false;
                if (MinDateTime.HasValue && DateTimeValue.Value < MinDateTime.Value) return true;
                if (MaxDateTime.HasValue && DateTimeValue.Value > MaxDateTime.Value) return true;
            }
            else
            {
                if (!DateValue.HasValue) return false;
                if (MinDate.HasValue && DateValue.Value < MinDate.Value) return true;
                if (MaxDate.HasValue && DateValue.Value > MaxDate.Value) return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Sets the state for DateOnly mode.
    /// </summary>
    public void SetDateState(
        DateOnly? value,
        DateOnly placeholder,
        DateFieldGranularity granularity,
        HourCycle hourCycle,
        CultureInfo culture,
        bool disabled,
        bool readOnly,
        bool invalid,
        DateOnly? minValue,
        DateOnly? maxValue,
        EventCallback<DateOnly?> valueChanged)
    {
        IsDateTimeMode = false;
        DateValue = value;
        DatePlaceholder = placeholder;
        Granularity = granularity;
        HourCycle = hourCycle;
        Culture = culture;
        Disabled = disabled;
        ReadOnly = readOnly;
        Invalid = invalid;
        MinDate = minValue;
        MaxDate = maxValue;
        DateValueChanged = valueChanged;
        NotifyStateChanged();
    }

    /// <summary>
    /// Sets the state for DateTime mode.
    /// </summary>
    public void SetDateTimeState(
        DateTime? value,
        DateTime placeholder,
        DateFieldGranularity granularity,
        HourCycle hourCycle,
        CultureInfo culture,
        bool disabled,
        bool readOnly,
        bool invalid,
        DateTime? minValue,
        DateTime? maxValue,
        EventCallback<DateTime?> valueChanged)
    {
        IsDateTimeMode = true;
        DateTimeValue = value;
        DateTimePlaceholder = placeholder;
        Granularity = granularity;
        HourCycle = hourCycle;
        Culture = culture;
        Disabled = disabled;
        ReadOnly = readOnly;
        Invalid = invalid;
        MinDateTime = minValue;
        MaxDateTime = maxValue;
        DateTimeValueChanged = valueChanged;
        NotifyStateChanged();
    }

    /// <summary>
    /// Increments the specified segment type.
    /// </summary>
    public async Task IncrementSegmentAsync(DateFieldSegmentType segmentType)
    {
        if (Disabled || ReadOnly) return;
        
        var current = EffectiveDateTime;
        var newValue = segmentType switch
        {
            DateFieldSegmentType.Year => current.AddYears(1),
            DateFieldSegmentType.Month => current.AddMonths(1),
            DateFieldSegmentType.Day => current.AddDays(1),
            DateFieldSegmentType.Hour => current.AddHours(1),
            DateFieldSegmentType.Minute => current.AddMinutes(1),
            DateFieldSegmentType.Second => current.AddSeconds(1),
            DateFieldSegmentType.DayPeriod => current.AddHours(12), // Toggle AM/PM
            _ => current
        };
        
        await UpdateValueAsync(newValue);
    }

    /// <summary>
    /// Decrements the specified segment type.
    /// </summary>
    public async Task DecrementSegmentAsync(DateFieldSegmentType segmentType)
    {
        if (Disabled || ReadOnly) return;
        
        var current = EffectiveDateTime;
        var newValue = segmentType switch
        {
            DateFieldSegmentType.Year => current.AddYears(-1),
            DateFieldSegmentType.Month => current.AddMonths(-1),
            DateFieldSegmentType.Day => current.AddDays(-1),
            DateFieldSegmentType.Hour => current.AddHours(-1),
            DateFieldSegmentType.Minute => current.AddMinutes(-1),
            DateFieldSegmentType.Second => current.AddSeconds(-1),
            DateFieldSegmentType.DayPeriod => current.AddHours(-12), // Toggle AM/PM
            _ => current
        };
        
        await UpdateValueAsync(newValue);
    }

    /// <summary>
    /// Sets a specific segment to a new numeric value.
    /// </summary>
    public async Task SetSegmentValueAsync(DateFieldSegmentType segmentType, int value)
    {
        if (Disabled || ReadOnly) return;
        
        var current = EffectiveDateTime;
        DateTime newValue;
        
        // For 12-hour clock, need to convert hour value properly
        if (segmentType == DateFieldSegmentType.Hour && Uses12HourClock())
        {
            // Convert 12-hour input to 24-hour
            var isPm = current.Hour >= 12;
            if (value == 12)
            {
                value = isPm ? 12 : 0;
            }
            else if (isPm)
            {
                value += 12;
            }
        }
        
        try
        {
            newValue = segmentType switch
            {
                DateFieldSegmentType.Year => new DateTime(value, current.Month, Math.Min(current.Day, DateTime.DaysInMonth(value, current.Month)), current.Hour, current.Minute, current.Second),
                DateFieldSegmentType.Month => new DateTime(current.Year, value, Math.Min(current.Day, DateTime.DaysInMonth(current.Year, value)), current.Hour, current.Minute, current.Second),
                DateFieldSegmentType.Day => new DateTime(current.Year, current.Month, value, current.Hour, current.Minute, current.Second),
                DateFieldSegmentType.Hour => new DateTime(current.Year, current.Month, current.Day, value, current.Minute, current.Second),
                DateFieldSegmentType.Minute => new DateTime(current.Year, current.Month, current.Day, current.Hour, value, current.Second),
                DateFieldSegmentType.Second => new DateTime(current.Year, current.Month, current.Day, current.Hour, current.Minute, value),
                _ => current
            };
        }
        catch (ArgumentOutOfRangeException)
        {
            // Invalid date value, ignore
            return;
        }
        
        await UpdateValueAsync(newValue);
    }

    /// <summary>
    /// Clears the value (sets to null).
    /// </summary>
    public async Task ClearSegmentAsync(DateFieldSegmentType segmentType)
    {
        if (Disabled || ReadOnly) return;
        
        // Clear the entire value
        if (IsDateTimeMode)
        {
            DateTimeValue = null;
            await DateTimeValueChanged.InvokeAsync(null);
        }
        else
        {
            DateValue = null;
            await DateValueChanged.InvokeAsync(null);
        }
        
        NotifyStateChanged();
    }

    /// <summary>
    /// Sets the AM/PM period.
    /// </summary>
    public async Task SetDayPeriodAsync(string period)
    {
        if (Disabled || ReadOnly) return;
        
        var current = EffectiveDateTime;
        var isCurrentlyPm = current.Hour >= 12;
        var wantPm = period.Equals("PM", StringComparison.OrdinalIgnoreCase);
        
        if (isCurrentlyPm != wantPm)
        {
            var newValue = wantPm ? current.AddHours(12) : current.AddHours(-12);
            await UpdateValueAsync(newValue);
        }
    }

    private async Task UpdateValueAsync(DateTime newValue)
    {
        if (IsDateTimeMode)
        {
            DateTimeValue = newValue;
            await DateTimeValueChanged.InvokeAsync(newValue);
        }
        else
        {
            DateValue = DateOnly.FromDateTime(newValue);
            await DateValueChanged.InvokeAsync(DateValue);
        }
        
        NotifyStateChanged();
    }

    /// <summary>
    /// Determines if the culture uses 12-hour time format.
    /// </summary>
    public bool Uses12HourClock()
    {
        if (HourCycle == HourCycle.H12 || HourCycle == HourCycle.H11)
            return true;
        if (HourCycle == HourCycle.H23 || HourCycle == HourCycle.H24)
            return false;
        
        // Auto: detect from culture
        var pattern = Culture.DateTimeFormat.ShortTimePattern;
        return pattern.Contains("h") || pattern.Contains("t");
    }

    public void NotifyStateChanged() => OnStateChanged?.Invoke();
}

/// <summary>
/// Represents the state of a single date field segment.
/// </summary>
public class DateFieldSegmentState
{
    public string Id { get; set; } = Identifier.NewId();
    public DateFieldSegmentType Type { get; set; }
    public string? LiteralValue { get; set; }
}
