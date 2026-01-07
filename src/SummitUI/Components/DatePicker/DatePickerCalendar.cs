using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// The calendar component for DatePicker.
/// Extends CalendarRoot to integrate with DatePickerContext for value synchronization and auto-close.
/// </summary>
public class DatePickerCalendar : ComponentBase, IAsyncDisposable
{
    private readonly CalendarContext _calendarContext = new();
    private bool _localeInitialized;
    private string _effectiveLocale = "en-US";
    private WeekStartsOn _effectiveWeekStart = SummitUI.WeekStartsOn.Sunday;

    // Track previous values to detect changes
    private CalendarSystem _previousCalendarSystem;
    private string? _previousLocale;
    private DateOnly _previousDisplayedMonth;

    [Inject] private CalendarJsInterop JsInterop { get; set; } = default!;

    [CascadingParameter] private DatePickerContext DatePickerContext { get; set; } = default!;

    /// <summary>
    /// Returns true if we need to wait for JS to detect locale/week start.
    /// </summary>
    private bool NeedsLocaleDetection => !WeekStartsOn.HasValue || string.IsNullOrEmpty(Locale);

    /// <summary>
    /// Returns true if the calendar is ready to render its content.
    /// </summary>
    private bool IsReady => _localeInitialized || !NeedsLocaleDetection;

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
    /// The locale to use for formatting.
    /// </summary>
    [Parameter] public string? Locale { get; set; }

    /// <summary>
    /// The day of the week to start on.
    /// </summary>
    [Parameter] public WeekStartsOn? WeekStartsOn { get; set; }

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

        _calendarContext.OnStateChanged += StateHasChanged;
        _calendarContext.SetRootComponent(null!); // We'll handle JS interop differently
    }

    protected override void OnParametersSet()
    {
        var isControlled = ValueChanged.HasDelegate;

        // Inherit disabled/readonly from DatePickerContext if not explicitly set
        var effectiveDisabled = Disabled || DatePickerContext.Disabled;
        var effectiveReadOnly = ReadOnly || DatePickerContext.ReadOnly;

        if (WeekStartsOn.HasValue)
        {
            _effectiveWeekStart = WeekStartsOn.Value;
        }

        if (!string.IsNullOrEmpty(Locale))
        {
            _effectiveLocale = Locale;
        }

        _calendarContext.SetState(
            value: Value,
            defaultValue: DefaultValue,
            isControlled: isControlled,
            placeholder: Placeholder,
            locale: _effectiveLocale,
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && NeedsLocaleDetection)
        {
            await InitializeLocaleAsync();
            _localeInitialized = true;

            _previousCalendarSystem = CalendarSystem;
            _previousLocale = Locale;
            _previousDisplayedMonth = _calendarContext.DisplayedMonth;

            await UpdateConvertedDatesAsync();

            StateHasChanged();
        }
        else if (firstRender)
        {
            _previousCalendarSystem = CalendarSystem;
            _previousLocale = Locale;
            _previousDisplayedMonth = _calendarContext.DisplayedMonth;

            var weekdayNames = await JsInterop.GetWeekdayNamesAsync(_effectiveLocale, CalendarSystem);
            _calendarContext.SetWeekdayNames(weekdayNames.Short, weekdayNames.Long);
            await UpdateMonthNameAsync();

            await UpdateConvertedDatesAsync();

            StateHasChanged();
        }
        else if (_localeInitialized || !NeedsLocaleDetection)
        {
            var calendarChanged = _previousCalendarSystem != CalendarSystem;
            var localeChanged = _previousLocale != Locale;
            var monthChanged = _previousDisplayedMonth != _calendarContext.DisplayedMonth;

            if (calendarChanged || localeChanged || monthChanged)
            {
                _previousCalendarSystem = CalendarSystem;
                _previousLocale = Locale;
                _previousDisplayedMonth = _calendarContext.DisplayedMonth;

                if (calendarChanged || localeChanged)
                {
                    var weekdayNames = await JsInterop.GetWeekdayNamesAsync(_effectiveLocale, CalendarSystem);
                    _calendarContext.SetWeekdayNames(weekdayNames.Short, weekdayNames.Long);
                }

                await UpdateMonthNameAsync();
                await UpdateConvertedDatesAsync();

                StateHasChanged();
            }
        }
    }

    private async Task InitializeLocaleAsync()
    {
        if (!string.IsNullOrEmpty(Locale))
        {
            _effectiveLocale = Locale;
        }
        else
        {
            _effectiveLocale = await JsInterop.GetBrowserLocaleAsync();
        }

        if (WeekStartsOn.HasValue)
        {
            _effectiveWeekStart = WeekStartsOn.Value;
        }
        else
        {
            var firstDay = await JsInterop.GetFirstDayOfWeekAsync(_effectiveLocale);
            _effectiveWeekStart = (WeekStartsOn)firstDay;
        }

        var weekdayNames = await JsInterop.GetWeekdayNamesAsync(_effectiveLocale, CalendarSystem);
        _calendarContext.SetWeekdayNames(weekdayNames.Short, weekdayNames.Long);

        await UpdateMonthNameAsync();

        // Inherit disabled/readonly from DatePickerContext
        var effectiveDisabled = Disabled || DatePickerContext.Disabled;
        var effectiveReadOnly = ReadOnly || DatePickerContext.ReadOnly;

        _calendarContext.SetState(
            value: Value,
            defaultValue: DefaultValue,
            isControlled: ValueChanged.HasDelegate,
            placeholder: Placeholder,
            locale: _effectiveLocale,
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
    }

    private async Task UpdateMonthNameAsync()
    {
        var heading = await JsInterop.GetMonthYearHeadingAsync(
            _effectiveLocale,
            _calendarContext.DisplayedMonth.Year,
            _calendarContext.DisplayedMonth.Month,
            CalendarSystem
        );
        _calendarContext.SetMonthName(heading);
    }

    private async Task UpdateConvertedDatesAsync()
    {
        var month = _calendarContext.GenerateMonth();

        var results = await JsInterop.BatchConvertFromGregorianAsync(
            _effectiveLocale,
            month.Dates,
            CalendarSystem
        );

        var convertedDates = new Dictionary<DateOnly, (int Day, string LocalizedDateString)>();
        for (int i = 0; i < month.Dates.Length && i < results.Length; i++)
        {
            convertedDates[month.Dates[i]] = results[i];
        }

        _calendarContext.SetConvertedDates(convertedDates);
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

        if (!IsReady)
        {
            builder.CloseElement();
            return;
        }

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
        _calendarContext.OnStateChanged -= StateHasChanged;
        return ValueTask.CompletedTask;
    }
}
