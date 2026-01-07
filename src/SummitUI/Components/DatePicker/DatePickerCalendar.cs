using System.Globalization;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using SummitUI.Services;

namespace SummitUI;

/// <summary>
/// The calendar component for DatePicker.
/// Extends CalendarRoot to integrate with DatePickerContext for value synchronization and auto-close.
/// </summary>
public class DatePickerCalendar : ComponentBase, IAsyncDisposable
{
    private readonly CalendarContext _calendarContext = new();
    private CultureInfo _effectiveCulture = CultureInfo.CurrentCulture;
    private DayOfWeek _effectiveWeekStart = DayOfWeek.Sunday;

    // Track previous values to detect changes
    private CalendarSystem _previousCalendarSystem;
    private CultureInfo? _previousCulture;
    private DateOnly _previousDisplayedMonth;

    [Inject] private CalendarProvider CalendarProvider { get; set; } = default!;
    [Inject] private CalendarFormatter CalendarFormatter { get; set; } = default!;

    [CascadingParameter] private DatePickerContext DatePickerContext { get; set; } = default!;

    #region Parameters

    /// <summary>
    /// The currently selected date (controlled).
    /// </summary>
    [Parameter] public DateOnly? Value { get; set; }

    /// <summary>
    /// Event callback when the value changes.
    /// </summary>
    [Parameter] public EventCallback<DateOnly?> ValueChanged { get; set; }

    /// <summary>
    /// The default value when uncontrolled.
    /// </summary>
    [Parameter] public DateOnly? DefaultValue { get; set; }

    /// <summary>
    /// Alternative callback for value changes.
    /// </summary>
    [Parameter] public EventCallback<DateOnly?> OnValueChange { get; set; }

    /// <summary>
    /// The placeholder date used to determine the initial displayed month.
    /// </summary>
    [Parameter] public DateOnly? Placeholder { get; set; }

    /// <summary>
    /// The culture to use for formatting and localization.
    /// If not specified, uses <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    [Parameter] public CultureInfo? Culture { get; set; }

    /// <summary>
    /// The day of the week to start on.
    /// If not specified, uses the culture's default first day of week.
    /// </summary>
    [Parameter] public DayOfWeek? WeekStartsOn { get; set; }

    /// <summary>
    /// The calendar system to use for display.
    /// </summary>
    [Parameter] public CalendarSystem CalendarSystem { get; set; } = CalendarSystem.Gregorian;

    /// <summary>
    /// Whether to always display 6 weeks for consistent height.
    /// </summary>
    [Parameter] public bool FixedWeeks { get; set; }

    /// <summary>
    /// The minimum selectable date.
    /// </summary>
    [Parameter] public DateOnly? MinValue { get; set; }

    /// <summary>
    /// The maximum selectable date.
    /// </summary>
    [Parameter] public DateOnly? MaxValue { get; set; }

    /// <summary>
    /// Whether the calendar is disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Whether the calendar is read-only.
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>
    /// Custom function to determine if a date should be disabled.
    /// </summary>
    [Parameter] public Func<DateOnly, bool>? IsDateDisabled { get; set; }

    /// <summary>
    /// The child content. Receives CalendarChildContext as context.
    /// </summary>
    [Parameter] public RenderFragment<CalendarChildContext>? ChildContent { get; set; }

    /// <summary>
    /// Additional attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    #endregion

    protected override void OnInitialized()
    {
        if (DatePickerContext == null)
            throw new InvalidOperationException("DatePickerCalendar must be used within a DatePickerRoot.");

        _calendarContext.OnStateChanged += HandleStateChanged;
        _calendarContext.SetRootComponent(null!); // We'll handle updates differently
    }

    protected override void OnParametersSet()
    {
        var isControlled = ValueChanged.HasDelegate;

        // Inherit disabled/readonly from DatePickerContext if not explicitly set
        var effectiveDisabled = Disabled || DatePickerContext.Disabled;
        var effectiveReadOnly = ReadOnly || DatePickerContext.ReadOnly;

        // Determine effective culture
        _effectiveCulture = Culture ?? CultureInfo.CurrentCulture;

        // Determine effective week start
        _effectiveWeekStart = WeekStartsOn ?? CalendarFormatter.GetFirstDayOfWeek(_effectiveCulture);

        // Get localized weekday names synchronously from culture
        var weekdayNames = CalendarFormatter.GetWeekdayNames(_effectiveCulture);
        _calendarContext.SetWeekdayNames(weekdayNames.Short, weekdayNames.Long);

        _calendarContext.SetState(
            value: Value,
            defaultValue: DefaultValue,
            isControlled: isControlled,
            placeholder: Placeholder,
            culture: _effectiveCulture,
            calendarSystem: CalendarSystem,
            weekStartsOn: _effectiveWeekStart,
            fixedWeeks: FixedWeeks,
            minValue: MinValue,
            maxValue: MaxValue,
            disabled: effectiveDisabled,
            readOnly: effectiveReadOnly,
            isDateDisabled: IsDateDisabled,
            valueChanged: EventCallback.Factory.Create<DateOnly?>(this, HandleValueChangedAsync),
            onValueChange: OnValueChange
        );

        // Update month name and converted dates if calendar/culture/month changed
        var cultureChanged = _previousCulture != _effectiveCulture;
        var calendarChanged = _previousCalendarSystem != CalendarSystem;
        var monthChanged = _previousDisplayedMonth != _calendarContext.DisplayedMonth;

        if (cultureChanged || calendarChanged || monthChanged || _previousCulture == null)
        {
            UpdateMonthName();
            UpdateConvertedDates();
            
            _previousCulture = _effectiveCulture;
            _previousCalendarSystem = CalendarSystem;
            _previousDisplayedMonth = _calendarContext.DisplayedMonth;
        }
    }

    private void HandleStateChanged()
    {
        // Check if month changed (from navigation)
        if (_previousDisplayedMonth != _calendarContext.DisplayedMonth)
        {
            UpdateMonthName();
            UpdateConvertedDates();
            _previousDisplayedMonth = _calendarContext.DisplayedMonth;
        }
        
        StateHasChanged();
    }

    private void UpdateMonthName()
    {
        var heading = CalendarFormatter.GetMonthYearHeading(
            _effectiveCulture,
            _calendarContext.DisplayedMonth,
            CalendarSystem
        );
        _calendarContext.SetMonthName(heading);
    }

    private void UpdateConvertedDates()
    {
        // Generate the month grid to get all dates that need conversion
        var month = _calendarContext.GenerateMonth();

        // Batch convert all dates in the grid
        var results = CalendarProvider.BatchConvertFromGregorian(
            _effectiveCulture,
            month.Dates,
            CalendarSystem
        );

        // Build the dictionary mapping Gregorian dates to converted info
        var convertedDates = new Dictionary<DateOnly, (int Day, string LocalizedDateString)>();
        for (int i = 0; i < month.Dates.Length && i < results.Length; i++)
        {
            convertedDates[month.Dates[i]] = results[i];
        }

        _calendarContext.SetConvertedDates(convertedDates);
    }

    /// <summary>
    /// Handles value changes from the calendar and notifies DatePickerContext.
    /// </summary>
    private async Task HandleValueChangedAsync(DateOnly? newValue)
    {
        // Notify parent via ValueChanged
        await ValueChanged.InvokeAsync(newValue);

        // Notify DatePickerContext for auto-close and value sync
        if (newValue.HasValue)
        {
            await DatePickerContext.OnCalendarDateSelectedAsync(newValue.Value);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var effectiveDisabled = Disabled || DatePickerContext.Disabled;
        var effectiveReadOnly = ReadOnly || DatePickerContext.ReadOnly;

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "data-summit-datepicker-calendar", true);
        builder.AddAttribute(2, "data-disabled", effectiveDisabled ? "true" : null);
        builder.AddAttribute(3, "data-readonly", effectiveReadOnly ? "true" : null);

        builder.AddMultipleAttributes(4, AdditionalAttributes);

        // Live region for screen reader announcements
        builder.OpenElement(5, "div");
        builder.AddAttribute(6, "aria-live", "assertive");
        builder.AddAttribute(7, "aria-atomic", "true");
        builder.AddAttribute(8, "style", "position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px; overflow: hidden; clip: rect(0, 0, 0, 0); white-space: nowrap; border: 0;");
        builder.AddContent(9, _calendarContext.FocusAnnouncement);
        builder.CloseElement();

        // Cascade CalendarContext
        builder.OpenComponent<CascadingValue<CalendarContext>>(10);
        builder.AddComponentParameter(11, "Value", _calendarContext);
        builder.AddComponentParameter(12, "IsFixed", false);
        builder.AddComponentParameter(13, "ChildContent", (RenderFragment)(childBuilder =>
        {
            if (ChildContent != null)
            {
                childBuilder.AddContent(0, ChildContent(_calendarContext.GetChildContext()));
            }
        }));
        builder.CloseComponent();

        builder.CloseElement();
    }

    public ValueTask DisposeAsync()
    {
        _calendarContext.OnStateChanged -= HandleStateChanged;
        return ValueTask.CompletedTask;
    }
}
