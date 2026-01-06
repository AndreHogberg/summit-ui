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
    
    // Format configuration
    public string Format { get; private set; } = "yyyy-MM-dd";
    public string TimeFormat { get; private set; } = "HH:mm";
    
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
    
    // Cached segment labels from JavaScript Intl.DisplayNames
    private Dictionary<DateFieldSegmentType, string>? _segmentLabels;
    
    // Cached AM/PM designators from JavaScript Intl.DateTimeFormat
    private string _amDesignator = "AM";
    private string _pmDesignator = "PM";
    
    // Per-segment state tracking for partial value entry
    private HashSet<DateFieldSegmentType> _filledSegments = new();
    private int? _partialYear;
    private int? _partialMonth;
    private int? _partialDay;
    private int? _partialHour;
    private int? _partialMinute;
    private bool? _partialIsPm;
    
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
    /// Checks if a specific segment has a value (either from bound value or partial entry).
    /// </summary>
    public bool SegmentHasValue(DateFieldSegmentType segmentType)
    {
        if (HasValue) return true;
        return _filledSegments.Contains(segmentType);
    }

    /// <summary>
    /// Gets the numeric value for a segment, or null if in placeholder state.
    /// </summary>
    public int? GetSegmentValue(DateFieldSegmentType segmentType)
    {
        if (HasValue)
        {
            return DateFieldUtils.GetSegmentValue(segmentType, EffectiveDateTime, Uses12HourClock());
        }
        
        return segmentType switch
        {
            DateFieldSegmentType.Year => _partialYear,
            DateFieldSegmentType.Month => _partialMonth,
            DateFieldSegmentType.Day => _partialDay,
            DateFieldSegmentType.Hour => Uses12HourClock() && _partialHour.HasValue
                ? (_partialHour.Value % 12 == 0 ? 12 : _partialHour.Value % 12)
                : _partialHour,
            DateFieldSegmentType.Minute => _partialMinute,
            _ => null
        };
    }

    /// <summary>
    /// Gets whether the day period is PM, or null if in placeholder state.
    /// </summary>
    public bool? GetPartialIsPm()
    {
        if (HasValue) return EffectiveDateTime.Hour >= 12;
        return _partialIsPm;
    }

    /// <summary>
    /// Gets the current value is out of the min/max range.
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
        string format,
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
        Format = format;
        TimeFormat = "HH:mm"; // Not used in DateOnly mode, but keep default
        Disabled = disabled;
        ReadOnly = readOnly;
        Invalid = invalid;
        MinDate = minValue;
        MaxDate = maxValue;
        DateValueChanged = valueChanged;
        
        // Clear partial state when value is set externally
        if (value.HasValue)
        {
            ClearPartialState();
        }
        
        NotifyStateChanged();
    }

    /// <summary>
    /// Sets the state for DateTime mode.
    /// </summary>
    public void SetDateTimeState(
        DateTime? value,
        DateTime placeholder,
        string format,
        string timeFormat,
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
        Format = format;
        TimeFormat = timeFormat;
        Disabled = disabled;
        ReadOnly = readOnly;
        Invalid = invalid;
        MinDateTime = minValue;
        MaxDateTime = maxValue;
        DateTimeValueChanged = valueChanged;
        
        // Clear partial state when value is set externally
        if (value.HasValue)
        {
            ClearPartialState();
        }
        
        NotifyStateChanged();
    }

    /// <summary>
    /// Increments the specified segment type.
    /// </summary>
    public async Task IncrementSegmentAsync(DateFieldSegmentType segmentType)
    {
        if (Disabled || ReadOnly) return;
        
        // If we have a full value, update it directly (original behavior)
        if (HasValue)
        {
            var current = EffectiveDateTime;
            var newValue = segmentType switch
            {
                DateFieldSegmentType.Year => current.AddYears(1),
                DateFieldSegmentType.Month => current.AddMonths(1),
                DateFieldSegmentType.Day => current.AddDays(1),
                DateFieldSegmentType.Hour => current.AddHours(1),
                DateFieldSegmentType.Minute => current.AddMinutes(1),
                DateFieldSegmentType.DayPeriod => current.AddHours(12), // Toggle AM/PM
                _ => current
            };
            
            await UpdateValueAsync(newValue);
            return;
        }
        
        // We're in partial state - get effective value for this segment (uses placeholder if not set)
        var effectiveCurrent = GetEffectiveDateTimeForSegment();
        var incrementedValue = segmentType switch
        {
            DateFieldSegmentType.Year => effectiveCurrent.AddYears(1),
            DateFieldSegmentType.Month => effectiveCurrent.AddMonths(1),
            DateFieldSegmentType.Day => effectiveCurrent.AddDays(1),
            DateFieldSegmentType.Hour => effectiveCurrent.AddHours(1),
            DateFieldSegmentType.Minute => effectiveCurrent.AddMinutes(1),
            DateFieldSegmentType.DayPeriod => effectiveCurrent.AddHours(12), // Toggle AM/PM
            _ => effectiveCurrent
        };
        
        // Update the specific segment in partial state
        await SetSegmentFromDateTimeAsync(segmentType, incrementedValue);
    }

    /// <summary>
    /// Decrements the specified segment type.
    /// </summary>
    public async Task DecrementSegmentAsync(DateFieldSegmentType segmentType)
    {
        if (Disabled || ReadOnly) return;
        
        // If we have a full value, update it directly (original behavior)
        if (HasValue)
        {
            var current = EffectiveDateTime;
            var newValue = segmentType switch
            {
                DateFieldSegmentType.Year => current.AddYears(-1),
                DateFieldSegmentType.Month => current.AddMonths(-1),
                DateFieldSegmentType.Day => current.AddDays(-1),
                DateFieldSegmentType.Hour => current.AddHours(-1),
                DateFieldSegmentType.Minute => current.AddMinutes(-1),
                DateFieldSegmentType.DayPeriod => current.AddHours(-12), // Toggle AM/PM
                _ => current
            };
            
            await UpdateValueAsync(newValue);
            return;
        }
        
        // We're in partial state - get effective value for this segment (uses placeholder if not set)
        var effectiveCurrent = GetEffectiveDateTimeForSegment();
        var decrementedValue = segmentType switch
        {
            DateFieldSegmentType.Year => effectiveCurrent.AddYears(-1),
            DateFieldSegmentType.Month => effectiveCurrent.AddMonths(-1),
            DateFieldSegmentType.Day => effectiveCurrent.AddDays(-1),
            DateFieldSegmentType.Hour => effectiveCurrent.AddHours(-1),
            DateFieldSegmentType.Minute => effectiveCurrent.AddMinutes(-1),
            DateFieldSegmentType.DayPeriod => effectiveCurrent.AddHours(-12), // Toggle AM/PM
            _ => effectiveCurrent
        };
        
        // Update the specific segment in partial state
        await SetSegmentFromDateTimeAsync(segmentType, decrementedValue);
    }

    /// <summary>
    /// Gets the effective DateTime for segment operations, using partial values where available.
    /// </summary>
    private DateTime GetEffectiveDateTimeForSegment()
    {
        if (HasValue) return EffectiveDateTime;
        
        // Build from partial values, falling back to placeholder
        var placeholder = EffectiveDateTime;
        return new DateTime(
            _partialYear ?? placeholder.Year,
            _partialMonth ?? placeholder.Month,
            Math.Min(_partialDay ?? placeholder.Day, DateTime.DaysInMonth(_partialYear ?? placeholder.Year, _partialMonth ?? placeholder.Month)),
            _partialHour ?? placeholder.Hour,
            _partialMinute ?? placeholder.Minute,
            0
        );
    }

    /// <summary>
    /// Sets a segment value from a DateTime result (used by increment/decrement).
    /// </summary>
    private async Task SetSegmentFromDateTimeAsync(DateFieldSegmentType segmentType, DateTime newValue)
    {
        // Extract the relevant value from the DateTime
        var value = segmentType switch
        {
            DateFieldSegmentType.Year => newValue.Year,
            DateFieldSegmentType.Month => newValue.Month,
            DateFieldSegmentType.Day => newValue.Day,
            DateFieldSegmentType.Hour => newValue.Hour,
            DateFieldSegmentType.Minute => newValue.Minute,
            _ => 0
        };
        
        // Handle DayPeriod specially
        if (segmentType == DateFieldSegmentType.DayPeriod)
        {
            _partialIsPm = newValue.Hour >= 12;
            // Also update the hour if we have one
            if (_partialHour.HasValue)
            {
                var currentHour = _partialHour.Value;
                var isPm = _partialIsPm.Value;
                if (isPm && currentHour < 12)
                {
                    _partialHour = currentHour + 12;
                }
                else if (!isPm && currentHour >= 12)
                {
                    _partialHour = currentHour - 12;
                }
            }
            _filledSegments.Add(DateFieldSegmentType.DayPeriod);
        }
        else
        {
            SetPartialValue(segmentType, value);
            _filledSegments.Add(segmentType);
        }
        
        // Check if all required segments are now filled
        if (AllRequiredSegmentsFilled())
        {
            await TryComposeAndSetValueAsync();
        }
        else
        {
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Sets a specific segment to a new numeric value.
    /// </summary>
    public async Task SetSegmentValueAsync(DateFieldSegmentType segmentType, int value)
    {
        if (Disabled || ReadOnly) return;
        
        // If we already have a full value, update it in place (original behavior)
        if (HasValue)
        {
            var current = EffectiveDateTime;
            DateTime newValue;
            
            // For 12-hour clock, need to convert hour value properly
            if (segmentType == DateFieldSegmentType.Hour && Uses12HourClock())
            {
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
                    DateFieldSegmentType.Year => new DateTime(value, current.Month, Math.Min(current.Day, DateTime.DaysInMonth(value, current.Month)), current.Hour, current.Minute, 0),
                    DateFieldSegmentType.Month => new DateTime(current.Year, value, Math.Min(current.Day, DateTime.DaysInMonth(current.Year, value)), current.Hour, current.Minute, 0),
                    DateFieldSegmentType.Day => new DateTime(current.Year, current.Month, value, current.Hour, current.Minute, 0),
                    DateFieldSegmentType.Hour => new DateTime(current.Year, current.Month, current.Day, value, current.Minute, 0),
                    DateFieldSegmentType.Minute => new DateTime(current.Year, current.Month, current.Day, current.Hour, value, 0),
                    _ => current
                };
            }
            catch (ArgumentOutOfRangeException)
            {
                // Invalid date value, ignore
                return;
            }
            
            await UpdateValueAsync(newValue);
            return;
        }
        
        // We're in partial state - update the partial value
        // For 12-hour clock, need to convert hour value properly
        if (segmentType == DateFieldSegmentType.Hour && Uses12HourClock())
        {
            // Get current PM state, defaulting to placeholder if not set
            var isPm = _partialIsPm ?? (EffectiveDateTime.Hour >= 12);
            if (value == 12)
            {
                value = isPm ? 12 : 0;
            }
            else if (isPm)
            {
                value += 12;
            }
        }
        
        // Store the partial value
        SetPartialValue(segmentType, value);
        _filledSegments.Add(segmentType);
        
        // Check if all required segments are now filled
        if (AllRequiredSegmentsFilled())
        {
            await TryComposeAndSetValueAsync();
        }
        else
        {
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Clears a specific segment (sets it to placeholder state).
    /// </summary>
    public async Task ClearSegmentAsync(DateFieldSegmentType segmentType)
    {
        if (Disabled || ReadOnly) return;
        
        // If we have a full value, decompose it into partial values first
        if (HasValue)
        {
            DecomposeToPartialValues();
        }
        
        // Clear the specific segment
        _filledSegments.Remove(segmentType);
        ClearPartialValue(segmentType);
        
        // Set bound value to null since we no longer have a complete date
        if (IsDateTimeMode)
        {
            if (DateTimeValue.HasValue)
            {
                DateTimeValue = null;
                await DateTimeValueChanged.InvokeAsync(null);
            }
        }
        else
        {
            if (DateValue.HasValue)
            {
                DateValue = null;
                await DateValueChanged.InvokeAsync(null);
            }
        }
        
        NotifyStateChanged();
    }
    
    /// <summary>
    /// Decomposes the current bound value into partial values for segment-level editing.
    /// </summary>
    private void DecomposeToPartialValues()
    {
        var dt = EffectiveDateTime;
        _partialYear = dt.Year;
        _partialMonth = dt.Month;
        _partialDay = dt.Day;
        _partialHour = dt.Hour;
        _partialMinute = dt.Minute;
        _partialIsPm = dt.Hour >= 12;
        
        _filledSegments = new HashSet<DateFieldSegmentType>
        {
            DateFieldSegmentType.Year,
            DateFieldSegmentType.Month,
            DateFieldSegmentType.Day
        };
        
        if (IsDateTimeMode)
        {
            _filledSegments.Add(DateFieldSegmentType.Hour);
            _filledSegments.Add(DateFieldSegmentType.Minute);
            if (Uses12HourClock())
                _filledSegments.Add(DateFieldSegmentType.DayPeriod);
        }
    }
    
    /// <summary>
    /// Clears a specific partial value.
    /// </summary>
    private void ClearPartialValue(DateFieldSegmentType segmentType)
    {
        switch (segmentType)
        {
            case DateFieldSegmentType.Year: _partialYear = null; break;
            case DateFieldSegmentType.Month: _partialMonth = null; break;
            case DateFieldSegmentType.Day: _partialDay = null; break;
            case DateFieldSegmentType.Hour: _partialHour = null; break;
            case DateFieldSegmentType.Minute: _partialMinute = null; break;
            case DateFieldSegmentType.DayPeriod: _partialIsPm = null; break;
        }
    }
    
    /// <summary>
    /// Sets a specific partial value.
    /// </summary>
    private void SetPartialValue(DateFieldSegmentType segmentType, int value)
    {
        switch (segmentType)
        {
            case DateFieldSegmentType.Year: _partialYear = value; break;
            case DateFieldSegmentType.Month: _partialMonth = value; break;
            case DateFieldSegmentType.Day: _partialDay = value; break;
            case DateFieldSegmentType.Hour: _partialHour = value; break;
            case DateFieldSegmentType.Minute: _partialMinute = value; break;
        }
    }
    
    /// <summary>
    /// Checks if all required segments for the current mode are filled.
    /// </summary>
    private bool AllRequiredSegmentsFilled()
    {
        // Date segments always required
        if (!_filledSegments.Contains(DateFieldSegmentType.Year)) return false;
        if (!_filledSegments.Contains(DateFieldSegmentType.Month)) return false;
        if (!_filledSegments.Contains(DateFieldSegmentType.Day)) return false;
        
        // Time segments required in DateTime mode
        if (IsDateTimeMode)
        {
            if (!_filledSegments.Contains(DateFieldSegmentType.Hour)) return false;
            if (!_filledSegments.Contains(DateFieldSegmentType.Minute)) return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Attempts to compose partial values into a full DateTime and set it as the bound value.
    /// </summary>
    private async Task TryComposeAndSetValueAsync()
    {
        try
        {
            var newValue = new DateTime(
                _partialYear!.Value,
                _partialMonth!.Value,
                Math.Min(_partialDay!.Value, DateTime.DaysInMonth(_partialYear.Value, _partialMonth.Value)),
                _partialHour ?? 0,
                _partialMinute ?? 0,
                0
            );
            
            await UpdateValueAsync(newValue);
            ClearPartialState(); // Clear partial state since we now have a full value
        }
        catch (ArgumentOutOfRangeException)
        {
            // Invalid date combination, remain in partial state
            NotifyStateChanged();
        }
    }
    
    /// <summary>
    /// Clears all partial state values.
    /// </summary>
    private void ClearPartialState()
    {
        _filledSegments.Clear();
        _partialYear = null;
        _partialMonth = null;
        _partialDay = null;
        _partialHour = null;
        _partialMinute = null;
        _partialIsPm = null;
    }

    /// <summary>
    /// Sets the AM/PM period.
    /// </summary>
    public async Task SetDayPeriodAsync(string period)
    {
        if (Disabled || ReadOnly) return;
        
        var wantPm = period.Equals("PM", StringComparison.OrdinalIgnoreCase);
        
        // If we have a full value, update it directly (original behavior)
        if (HasValue)
        {
            var current = EffectiveDateTime;
            var isCurrentlyPm = current.Hour >= 12;
            
            if (isCurrentlyPm != wantPm)
            {
                var newValue = wantPm ? current.AddHours(12) : current.AddHours(-12);
                await UpdateValueAsync(newValue);
            }
            return;
        }
        
        // We're in partial state
        var isCurrentlyPmPartial = _partialIsPm ?? (EffectiveDateTime.Hour >= 12);
        
        if (isCurrentlyPmPartial != wantPm)
        {
            _partialIsPm = wantPm;
            _filledSegments.Add(DateFieldSegmentType.DayPeriod);
            
            // Also update the hour if we have one
            if (_partialHour.HasValue)
            {
                if (wantPm && _partialHour.Value < 12)
                {
                    _partialHour += 12;
                }
                else if (!wantPm && _partialHour.Value >= 12)
                {
                    _partialHour -= 12;
                }
            }
            
            // Check if all required segments are now filled
            if (AllRequiredSegmentsFilled())
            {
                await TryComposeAndSetValueAsync();
            }
            else
            {
                NotifyStateChanged();
            }
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
    /// Determines if the time format uses 12-hour clock.
    /// Checks for lowercase 'h' in the TimeFormat pattern.
    /// </summary>
    public bool Uses12HourClock()
    {
        // Look for lowercase 'h' which indicates 12-hour format
        // Uppercase 'H' indicates 24-hour format
        return TimeFormat.Contains('h');
    }

    /// <summary>
    /// Gets the time separator from the TimeFormat pattern.
    /// </summary>
    public string GetTimeSeparator()
    {
        // Find the separator between hour and minute in the format
        // e.g., "HH:mm" -> ":", "HH.mm" -> "."
        foreach (char c in TimeFormat)
        {
            if (c != 'H' && c != 'h' && c != 'm' && c != 's')
            {
                return c.ToString();
            }
        }
        return ":"; // Default fallback
    }

    /// <summary>
    /// Gets the AM designator.
    /// </summary>
    public string GetAmDesignator() => _amDesignator;

    /// <summary>
    /// Gets the PM designator.
    /// </summary>
    public string GetPmDesignator() => _pmDesignator;

    public void NotifyStateChanged() => OnStateChanged?.Invoke();

    public void SetInvalid(bool invalid)
    {
        Invalid = invalid;
    }

    /// <summary>
    /// Sets the cached segment labels from JavaScript Intl.DisplayNames.
    /// </summary>
    public void SetSegmentLabels(Dictionary<string, string> labels)
    {
        _segmentLabels = new Dictionary<DateFieldSegmentType, string>
        {
            [DateFieldSegmentType.Year] = labels.GetValueOrDefault("year", "Year"),
            [DateFieldSegmentType.Month] = labels.GetValueOrDefault("month", "Month"),
            [DateFieldSegmentType.Day] = labels.GetValueOrDefault("day", "Day"),
            [DateFieldSegmentType.Hour] = labels.GetValueOrDefault("hour", "Hour"),
            [DateFieldSegmentType.Minute] = labels.GetValueOrDefault("minute", "Minute"),
            [DateFieldSegmentType.DayPeriod] = labels.GetValueOrDefault("dayPeriod", "AM/PM")
        };
    }

    /// <summary>
    /// Sets the cached AM/PM designators from JavaScript Intl.DateTimeFormat.
    /// </summary>
    public void SetDayPeriodDesignators(string am, string pm)
    {
        _amDesignator = am;
        _pmDesignator = pm;
    }

    /// <summary>
    /// Gets the localized label for a segment type.
    /// Falls back to English if labels haven't been loaded yet.
    /// </summary>
    public string GetSegmentLabel(DateFieldSegmentType type)
    {
        if (_segmentLabels != null && _segmentLabels.TryGetValue(type, out var label))
        {
            return label;
        }
        
        // Fallback to English defaults
        return type switch
        {
            DateFieldSegmentType.Year => "Year",
            DateFieldSegmentType.Month => "Month",
            DateFieldSegmentType.Day => "Day",
            DateFieldSegmentType.Hour => "Hour",
            DateFieldSegmentType.Minute => "Minute",
            DateFieldSegmentType.DayPeriod => "AM/PM",
            _ => type.ToString()
        };
    }
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
