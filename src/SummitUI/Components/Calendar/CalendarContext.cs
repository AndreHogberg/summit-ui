using System.Globalization;

using Microsoft.AspNetCore.Components;

using SummitUI.Services;

namespace SummitUI;

/// <summary>
/// Represents a month in the calendar with its weeks and dates.
/// </summary>
public class CalendarMonth
{
    /// <summary>
    /// A DateOnly representing the first day of the month.
    /// </summary>
    public DateOnly Value { get; init; }

    /// <summary>
    /// A 2D array of dates organized by weeks.
    /// Each inner array represents a week (always 7 days).
    /// May include dates from adjacent months to fill the grid.
    /// </summary>
    public DateOnly[][] Weeks { get; init; } = [];

    /// <summary>
    /// A flat array of all dates in the calendar grid.
    /// Includes dates from adjacent months used to fill the grid.
    /// </summary>
    public DateOnly[] Dates { get; init; } = [];
}

/// <summary>
/// Context data exposed to CalendarRoot's ChildContent.
/// </summary>
public class CalendarChildContext
{
    /// <summary>
    /// The current month being displayed.
    /// </summary>
    public required CalendarMonth CurrentMonth { get; init; }

    /// <summary>
    /// Localized short weekday names (e.g., "Sun", "Mon").
    /// Ordered according to the configured week start day.
    /// </summary>
    public required string[] Weekdays { get; init; }

    /// <summary>
    /// Localized full weekday names (e.g., "Sunday", "Monday").
    /// For use in abbr attributes for screen readers.
    /// </summary>
    public required string[] WeekdaysLong { get; init; }
}

/// <summary>
/// Shared context for Calendar components, managing state and coordination.
/// </summary>
public class CalendarContext
{
    public string Id { get; } = Identifier.NewId();
    public string HeadingId => $"{Id}-heading";
    public string GridId => $"{Id}-grid";

    // Current value
    private DateOnly? _value;
    private DateOnly? _defaultValue;
    private bool _isControlled;

    // Display state
    private DateOnly _focusedDate;
    private DateOnly _displayedMonth;

    // Configuration
    public CultureInfo Culture { get; private set; } = CultureInfo.CurrentCulture;
    public DayOfWeek WeekStartsOn { get; private set; } = DayOfWeek.Sunday;
    public bool FixedWeeks { get; private set; }
    public DateOnly? MinValue { get; private set; }
    public DateOnly? MaxValue { get; private set; }
    public bool Disabled { get; private set; }
    public bool ReadOnly { get; private set; }
    public Func<DateOnly, bool>? IsDateDisabled { get; private set; }

    // Localized strings (populated from culture)
    private string[] _weekdaysShort = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
    private string[] _weekdaysLong = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    private string _monthName = "";

    // Focus management: Only set browser focus when user is actively navigating this calendar
    private bool _shouldFocus;

    // Callbacks
    private EventCallback<DateOnly?> _valueChanged;
    private EventCallback<DateOnly?> _onValueChange;

    // Announcer for screen reader announcements
    private ILiveAnnouncer? _announcer;
    private Func<string, string?>? _getSelectionAnnouncement;

    // Events
    public event Action? OnStateChanged;

    // Reference to root component for JS interop
    public SmCalendarRoot? RootComponent { get; private set; }

    /// <summary>
    /// Sets the root component reference for JS interop.
    /// </summary>
    public void SetRootComponent(SmCalendarRoot root)
    {
        RootComponent = root;
    }

    /// <summary>
    /// Gets the currently selected date value.
    /// </summary>
    public DateOnly? Value => _isControlled ? _value : (_value ?? _defaultValue);

    /// <summary>
    /// Gets the currently focused date for keyboard navigation.
    /// </summary>
    public DateOnly FocusedDate => _focusedDate;

    /// <summary>
    /// Gets the month currently being displayed.
    /// </summary>
    public DateOnly DisplayedMonth => _displayedMonth;

    /// <summary>
    /// Gets the localized name of the currently displayed month.
    /// </summary>
    public string MonthName => _monthName;

    /// <summary>
    /// Gets whether this calendar should programmatically set browser focus.
    /// This is true only when keyboard navigation triggers a focus change,
    /// preventing focus stealing between multiple calendars on the same page.
    /// </summary>
    public bool ShouldFocus => _shouldFocus;

    /// <summary>
    /// Clears the should-focus flag after focus has been set.
    /// </summary>
    public void ClearShouldFocus()
    {
        _shouldFocus = false;
    }

    /// <summary>
    /// Gets the year of the currently displayed month.
    /// </summary>
    public int Year => _displayedMonth.Year;

    /// <summary>
    /// Gets today's date.
    /// </summary>
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Gets the short weekday names ordered by week start.
    /// </summary>
    public string[] WeekdaysShort => GetOrderedWeekdays(_weekdaysShort);

    /// <summary>
    /// Gets the long weekday names ordered by week start.
    /// </summary>
    public string[] WeekdaysLongOrdered => GetOrderedWeekdays(_weekdaysLong);

    /// <summary>
    /// Sets the calendar state from CalendarRoot parameters.
    /// </summary>
    public void SetState(
        DateOnly? value,
        DateOnly? defaultValue,
        bool isControlled,
        DateOnly? placeholder,
        CultureInfo culture,
        DayOfWeek weekStartsOn,
        bool fixedWeeks,
        DateOnly? minValue,
        DateOnly? maxValue,
        bool disabled,
        bool readOnly,
        Func<DateOnly, bool>? isDateDisabled,
        EventCallback<DateOnly?> valueChanged,
        EventCallback<DateOnly?> onValueChange,
        ILiveAnnouncer? announcer,
        Func<string, string?>? getSelectionAnnouncement)
    {
        _value = value;
        _defaultValue = defaultValue;
        _isControlled = isControlled;
        Culture = culture;
        WeekStartsOn = weekStartsOn;
        FixedWeeks = fixedWeeks;
        MinValue = minValue;
        MaxValue = maxValue;
        Disabled = disabled;
        ReadOnly = readOnly;
        IsDateDisabled = isDateDisabled;
        _valueChanged = valueChanged;
        _onValueChange = onValueChange;
        _announcer = announcer;
        _getSelectionAnnouncement = getSelectionAnnouncement;

        // Initialize focused date and displayed month
        var effectiveValue = Value ?? placeholder ?? Today;
        if (_focusedDate == default)
        {
            _focusedDate = effectiveValue;
        }
        if (_displayedMonth == default)
        {
            _displayedMonth = new DateOnly(effectiveValue.Year, effectiveValue.Month, 1);
        }
    }

    /// <summary>
    /// Sets the localized weekday names.
    /// </summary>
    public void SetWeekdayNames(string[] shortNames, string[] longNames)
    {
        if (shortNames.Length == 7)
            _weekdaysShort = shortNames;
        if (longNames.Length == 7)
            _weekdaysLong = longNames;
    }

    /// <summary>
    /// Sets the localized month name.
    /// </summary>
    public void SetMonthName(string monthName)
    {
        if (_monthName == monthName)
            return;
        
        _monthName = monthName;
        NotifyStateChanged();
    }

    /// <summary>
    /// Gets the display day number for a date.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The day number.</returns>
    public int GetDisplayDay(DateOnly date) => date.Day;

    /// <summary>
    /// Gets the localized date string for accessibility (aria-label).
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>A localized date string.</returns>
    public string GetLocalizedDateString(DateOnly date) => date.ToString("D", Culture);

    /// <summary>
    /// Selects a date.
    /// </summary>
    public async Task SelectDateAsync(DateOnly date)
    {
        if (Disabled || ReadOnly) return;
        if (IsDateUnavailable(date)) return;

        _value = date;
        _focusedDate = date;

        // Announce selection to screen readers
        AnnounceSelection(date);

        await _valueChanged.InvokeAsync(date);
        await _onValueChange.InvokeAsync(date);

        NotifyStateChanged();
    }

    /// <summary>
    /// Announces the date selection to screen readers.
    /// </summary>
    private void AnnounceSelection(DateOnly date)
    {
        if (_announcer is null) return;

        var formattedDate = GetLocalizedDateString(date);

        // Use custom announcement if provided, otherwise use default
        var announcement = _getSelectionAnnouncement is not null
            ? _getSelectionAnnouncement(formattedDate)
            : $"{formattedDate} selected";

        if (!string.IsNullOrEmpty(announcement))
        {
            _announcer.Announce(announcement);
        }
    }

    /// <summary>
    /// Navigates to the previous month.
    /// </summary>
    public void PreviousMonth()
    {
        if (Disabled) return;

        _displayedMonth = _displayedMonth.AddMonths(-1);

        // Keep focused date in sync if it's in the new month
        if (_focusedDate.Month != _displayedMonth.Month || _focusedDate.Year != _displayedMonth.Year)
        {
            _focusedDate = ClampToMonth(_focusedDate, _displayedMonth);
        }

        // Notify root to update month name
        RootComponent?.OnMonthChanged();

        NotifyStateChanged();
    }

    /// <summary>
    /// Navigates to the next month.
    /// </summary>
    public void NextMonth()
    {
        if (Disabled) return;

        _displayedMonth = _displayedMonth.AddMonths(1);

        // Keep focused date in sync if it's in the new month
        if (_focusedDate.Month != _displayedMonth.Month || _focusedDate.Year != _displayedMonth.Year)
        {
            _focusedDate = ClampToMonth(_focusedDate, _displayedMonth);
        }

        // Notify root to update month name
        RootComponent?.OnMonthChanged();

        NotifyStateChanged();
    }

    /// <summary>
    /// Navigates to the previous year.
    /// </summary>
    public void PreviousYear()
    {
        if (Disabled) return;

        _displayedMonth = _displayedMonth.AddYears(-1);
        _focusedDate = ClampToValidDate(_focusedDate.AddYears(-1));

        // Notify root to update month name
        RootComponent?.OnMonthChanged();

        NotifyStateChanged();
    }

    /// <summary>
    /// Navigates to the next year.
    /// </summary>
    public void NextYear()
    {
        if (Disabled) return;

        _displayedMonth = _displayedMonth.AddYears(1);
        _focusedDate = ClampToValidDate(_focusedDate.AddYears(1));

        // Notify root to update month name
        RootComponent?.OnMonthChanged();

        NotifyStateChanged();
    }

    /// <summary>
    /// Moves focus to a specific date (keyboard navigation).
    /// </summary>
    public void FocusDate(DateOnly date)
    {
        if (Disabled) return;

        date = ClampToValidDate(date);
        _focusedDate = date;

        // Update displayed month if focused date is outside current view
        if (date.Year != _displayedMonth.Year || date.Month != _displayedMonth.Month)
        {
            _displayedMonth = new DateOnly(date.Year, date.Month, 1);
            
            // Notify root to update month name
            RootComponent?.OnMonthChanged();
        }

        // Mark that this calendar should receive browser focus
        _shouldFocus = true;

        NotifyStateChanged();
    }

    /// <summary>
    /// Moves focus by a number of days.
    /// </summary>
    public void MoveFocus(int days)
    {
        FocusDate(_focusedDate.AddDays(days));
    }

    /// <summary>
    /// Moves focus by a number of weeks.
    /// </summary>
    public void MoveFocusWeeks(int weeks)
    {
        FocusDate(_focusedDate.AddDays(weeks * 7));
    }

    /// <summary>
    /// Moves focus to the first day of the current week.
    /// </summary>
    public void FocusStartOfWeek()
    {
        var dayOfWeek = (int)_focusedDate.DayOfWeek;
        var weekStart = (int)WeekStartsOn;
        var daysToSubtract = (dayOfWeek - weekStart + 7) % 7;
        FocusDate(_focusedDate.AddDays(-daysToSubtract));
    }

    /// <summary>
    /// Moves focus to the last day of the current week.
    /// </summary>
    public void FocusEndOfWeek()
    {
        var dayOfWeek = (int)_focusedDate.DayOfWeek;
        var weekStart = (int)WeekStartsOn;
        var weekEnd = (weekStart + 6) % 7;
        var daysToAdd = (weekEnd - dayOfWeek + 7) % 7;
        FocusDate(_focusedDate.AddDays(daysToAdd));
    }

    /// <summary>
    /// Checks if a date is the currently selected date.
    /// </summary>
    public bool IsSelected(DateOnly date) => Value == date;

    /// <summary>
    /// Checks if a date is today.
    /// </summary>
    public bool IsToday(DateOnly date) => date == Today;

    /// <summary>
    /// Checks if a date is the currently focused date.
    /// </summary>
    public bool IsFocused(DateOnly date) => _focusedDate == date;

    /// <summary>
    /// Checks if a date is outside the currently displayed month.
    /// </summary>
    public bool IsOutsideMonth(DateOnly date) =>
        date.Year != _displayedMonth.Year || date.Month != _displayedMonth.Month;

    /// <summary>
    /// Checks if a date is unavailable (outside min/max or custom disabled).
    /// </summary>
    public bool IsDateUnavailable(DateOnly date)
    {
        if (MinValue.HasValue && date < MinValue.Value) return true;
        if (MaxValue.HasValue && date > MaxValue.Value) return true;
        if (IsDateDisabled?.Invoke(date) == true) return true;
        return false;
    }

    /// <summary>
    /// Checks if the previous month navigation is available.
    /// </summary>
    public bool CanNavigatePrevious =>
        !MinValue.HasValue || _displayedMonth.AddMonths(-1) >= new DateOnly(MinValue.Value.Year, MinValue.Value.Month, 1);

    /// <summary>
    /// Checks if the next month navigation is available.
    /// </summary>
    public bool CanNavigateNext =>
        !MaxValue.HasValue || _displayedMonth.AddMonths(1) <= new DateOnly(MaxValue.Value.Year, MaxValue.Value.Month, 1);

    /// <summary>
    /// Generates the calendar month data for rendering.
    /// </summary>
    public CalendarMonth GenerateMonth()
    {
        var firstDayOfMonth = new DateOnly(_displayedMonth.Year, _displayedMonth.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(_displayedMonth.Year, _displayedMonth.Month);
        var lastDayOfMonth = new DateOnly(_displayedMonth.Year, _displayedMonth.Month, daysInMonth);

        // Find the first day to display (may be in previous month)
        var firstDayOfWeek = (int)WeekStartsOn;
        var firstDayOffset = ((int)firstDayOfMonth.DayOfWeek - firstDayOfWeek + 7) % 7;
        var startDate = firstDayOfMonth.AddDays(-firstDayOffset);

        // Find the last day to display (may be in next month)
        var lastDayOffset = (firstDayOfWeek + 6 - (int)lastDayOfMonth.DayOfWeek + 7) % 7;
        var endDate = lastDayOfMonth.AddDays(lastDayOffset);

        // Calculate number of weeks
        var totalDays = endDate.DayNumber - startDate.DayNumber + 1;
        var numWeeks = totalDays / 7;

        // If FixedWeeks is enabled, ensure we have 6 weeks
        if (FixedWeeks && numWeeks < 6)
        {
            var additionalDays = (6 - numWeeks) * 7;
            endDate = endDate.AddDays(additionalDays);
            numWeeks = 6;
        }

        // Build the weeks array
        var weeks = new List<DateOnly[]>();
        var allDates = new List<DateOnly>();
        var currentDate = startDate;

        for (int week = 0; week < numWeeks; week++)
        {
            var weekDates = new DateOnly[7];
            for (int day = 0; day < 7; day++)
            {
                weekDates[day] = currentDate;
                allDates.Add(currentDate);
                currentDate = currentDate.AddDays(1);
            }
            weeks.Add(weekDates);
        }

        return new CalendarMonth
        {
            Value = firstDayOfMonth,
            Weeks = [.. weeks],
            Dates = [.. allDates]
        };
    }

    /// <summary>
    /// Gets the CalendarChildContext for rendering.
    /// </summary>
    public CalendarChildContext GetChildContext()
    {
        return new CalendarChildContext
        {
            CurrentMonth = GenerateMonth(),
            Weekdays = WeekdaysShort,
            WeekdaysLong = WeekdaysLongOrdered
        };
    }

    private string[] GetOrderedWeekdays(string[] weekdays)
    {
        var startIndex = (int)WeekStartsOn;
        var ordered = new string[7];
        for (int i = 0; i < 7; i++)
        {
            ordered[i] = weekdays[(startIndex + i) % 7];
        }
        return ordered;
    }

    private DateOnly ClampToMonth(DateOnly date, DateOnly month)
    {
        var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
        var day = Math.Min(date.Day, daysInMonth);
        return new DateOnly(month.Year, month.Month, day);
    }

    private DateOnly ClampToValidDate(DateOnly date)
    {
        // Handle February 29 edge case
        try
        {
            return date;
        }
        catch
        {
            // If date is invalid (e.g., Feb 29 in non-leap year), adjust
            var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            return new DateOnly(date.Year, date.Month, Math.Min(date.Day, daysInMonth));
        }
    }

    public void NotifyStateChanged() => OnStateChanged?.Invoke();
}
