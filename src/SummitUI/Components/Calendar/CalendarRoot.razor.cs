using System.Globalization;

using Microsoft.AspNetCore.Components;

using SummitUI.Interop;
using SummitUI.Services;

namespace SummitUI;

/// <summary>
/// The root component for the Calendar. Provides context and manages state.
/// </summary>
public partial class CalendarRoot : IAsyncDisposable
{
    private readonly CalendarContext _context = new();
    private CultureInfo _effectiveCulture = CultureInfo.CurrentCulture;
    private DayOfWeek _effectiveWeekStart = DayOfWeek.Sunday;

    // Track previous values to detect changes that require re-computation
    private CultureInfo? _previousCulture;
    private DateOnly _previousDisplayedMonth;

    [Inject] private CalendarJsInterop JsInterop { get; set; } = default!;
    [Inject] private CalendarFormatter CalendarFormatter { get; set; } = default!;

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
    /// The placeholder date used to determine the initial displayed month when no value is set.
    /// </summary>
    [Parameter] public DateOnly? Placeholder { get; set; }

    /// <summary>
    /// The culture to use for formatting and localization.
    /// If not specified, uses <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    /// <remarks>
    /// Users can create custom CultureInfo instances with their own translations and calendar configurations.
    /// </remarks>
    [Parameter] public CultureInfo? Culture { get; set; }

    /// <summary>
    /// The day of the week to start on.
    /// If not specified, uses the culture's default first day of week.
    /// </summary>
    [Parameter] public DayOfWeek? WeekStartsOn { get; set; }

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
    /// Whether the calendar is read-only (can navigate but not select).
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
        _context.OnStateChanged += StateHasChanged;
        _context.SetRootComponent(this);
    }

    protected override void OnParametersSet()
    {
        var isControlled = ValueChanged.HasDelegate;

        // Determine effective culture
        _effectiveCulture = Culture ?? CultureInfo.CurrentCulture;

        // Determine effective week start
        _effectiveWeekStart = WeekStartsOn ?? CalendarFormatter.GetFirstDayOfWeek(_effectiveCulture);

        // Get localized weekday names synchronously from culture
        var weekdayNames = CalendarFormatter.GetWeekdayNames(_effectiveCulture);
        _context.SetWeekdayNames(weekdayNames.Short, weekdayNames.Long);

        _context.SetState(
            value: Value,
            defaultValue: DefaultValue,
            isControlled: isControlled,
            placeholder: Placeholder,
            culture: _effectiveCulture,
            weekStartsOn: _effectiveWeekStart,
            fixedWeeks: FixedWeeks,
            minValue: MinValue,
            maxValue: MaxValue,
            disabled: Disabled,
            readOnly: ReadOnly,
            isDateDisabled: IsDateDisabled,
            valueChanged: ValueChanged,
            onValueChange: OnValueChange
        );

        // Update month name if culture or month changed
        var cultureChanged = _previousCulture != _effectiveCulture;
        var monthChanged = _previousDisplayedMonth != _context.DisplayedMonth;

        if (cultureChanged || monthChanged || _previousCulture == null)
        {
            UpdateMonthName();

            _previousCulture = _effectiveCulture;
            _previousDisplayedMonth = _context.DisplayedMonth;
        }
    }

    private void UpdateMonthName()
    {
        var heading = CalendarFormatter.GetMonthYearHeading(
            _effectiveCulture,
            _context.DisplayedMonth
        );
        _context.SetMonthName(heading);
    }

    #region Internal Methods for Context

    /// <summary>
    /// Gets the JS interop instance for focus management.
    /// </summary>
    internal CalendarJsInterop GetJsInterop() => JsInterop;

    /// <summary>
    /// Updates the month heading when navigation occurs.
    /// Called by CalendarContext when the displayed month changes.
    /// </summary>
    internal void OnMonthChanged()
    {
        UpdateMonthName();
        _previousDisplayedMonth = _context.DisplayedMonth;
    }

    #endregion

    public ValueTask DisposeAsync()
    {
        _context.OnStateChanged -= StateHasChanged;

        // Note: Do NOT dispose JsInterop here.
        // CalendarJsInterop is a scoped service managed by DI.
        // In WebAssembly, scoped services are effectively singletons,
        // so disposing it here would break the calendar when navigating back.

        return ValueTask.CompletedTask;
    }
}
