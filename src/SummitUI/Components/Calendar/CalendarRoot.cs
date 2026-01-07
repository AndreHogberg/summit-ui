using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// The root component for the Calendar. Provides context and manages state.
/// </summary>
public class CalendarRoot : ComponentBase, IAsyncDisposable
{
    private readonly CalendarContext _context = new();
    private bool _localeInitialized;
    private string _effectiveLocale = "en-US";
    private WeekStartsOn _effectiveWeekStart = SummitUI.WeekStartsOn.Sunday;

    [Inject] private CalendarJsInterop JsInterop { get; set; } = default!;

    /// <summary>
    /// Returns true if we need to wait for JS to detect locale/week start.
    /// If both are explicitly provided, we don't need to wait.
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
    /// The placeholder date used to determine the initial displayed month when no value is set.
    /// </summary>
    [Parameter] public DateOnly? Placeholder { get; set; }

    /// <summary>
    /// The locale to use for formatting (e.g., "en-US", "fr-FR").
    /// If not specified, auto-detects from browser.
    /// </summary>
    [Parameter] public string? Locale { get; set; }

    /// <summary>
    /// The day of the week to start on.
    /// If not specified, auto-detects from locale.
    /// </summary>
    [Parameter] public WeekStartsOn? WeekStartsOn { get; set; }

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

        // If WeekStartsOn is explicitly set, use it immediately (don't wait for JS)
        if (WeekStartsOn.HasValue)
        {
            _effectiveWeekStart = WeekStartsOn.Value;
        }

        // If Locale is explicitly set, use it immediately (don't wait for JS)
        if (!string.IsNullOrEmpty(Locale))
        {
            _effectiveLocale = Locale;
        }

        _context.SetState(
            value: Value,
            defaultValue: DefaultValue,
            isControlled: isControlled,
            placeholder: Placeholder,
            locale: _effectiveLocale,
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
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && NeedsLocaleDetection)
        {
            await InitializeLocaleAsync();
            _localeInitialized = true;
            StateHasChanged();
        }
    }

    private async Task InitializeLocaleAsync()
    {
        // Determine effective locale
        if (!string.IsNullOrEmpty(Locale))
        {
            _effectiveLocale = Locale;
        }
        else
        {
            _effectiveLocale = await JsInterop.GetBrowserLocaleAsync();
        }

        // Determine effective week start
        if (WeekStartsOn.HasValue)
        {
            _effectiveWeekStart = WeekStartsOn.Value;
        }
        else
        {
            var firstDay = await JsInterop.GetFirstDayOfWeekAsync(_effectiveLocale);
            _effectiveWeekStart = (WeekStartsOn)firstDay;
        }

        // Get localized weekday names
        var weekdayNames = await JsInterop.GetWeekdayNamesAsync(_effectiveLocale);
        _context.SetWeekdayNames(weekdayNames.Short, weekdayNames.Long);

        // Get localized month name
        await UpdateMonthNameAsync();

        // Update context with effective values
        _context.SetState(
            value: Value,
            defaultValue: DefaultValue,
            isControlled: ValueChanged.HasDelegate,
            placeholder: Placeholder,
            locale: _effectiveLocale,
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
    }

    private async Task UpdateMonthNameAsync()
    {
        var heading = await JsInterop.GetMonthYearHeadingAsync(
            _effectiveLocale,
            _context.DisplayedMonth.Year,
            _context.DisplayedMonth.Month
        );
        _context.SetMonthName(heading);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Use fixed sequence numbers for stable render tree diffing
        // Root div
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "data-summit-calendar-root", true);
        builder.AddAttribute(2, "data-disabled", Disabled ? "true" : null);
        builder.AddAttribute(3, "data-readonly", ReadOnly ? "true" : null);

        builder.AddMultipleAttributes(4, AdditionalAttributes);

        // Don't render calendar content until locale is ready
        // This prevents the calendar from shifting when week start is auto-detected
        if (!IsReady)
        {
            // Render nothing inside the root div - it will re-render when ready
            builder.CloseElement();
            return;
        }

        // Visually hidden live region for screen reader announcements
        // This is needed because role="application" on the grid means screen readers
        // won't automatically announce focus changes - we must announce explicitly
        builder.OpenElement(5, "div");
        builder.AddAttribute(6, "aria-live", "assertive");
        builder.AddAttribute(7, "aria-atomic", "true");
        builder.AddAttribute(8, "style", "position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px; overflow: hidden; clip: rect(0, 0, 0, 0); white-space: nowrap; border: 0;");
        builder.AddContent(9, _context.FocusAnnouncement);
        builder.CloseElement();

        // Cascading CalendarRoot for Grid to access
        builder.OpenComponent<CascadingValue<CalendarRoot>>(10);
        builder.AddComponentParameter(11, "Value", this);
        builder.AddComponentParameter(12, "IsFixed", true);
        builder.AddComponentParameter(13, "ChildContent", (RenderFragment)(cascadeBuilder =>
        {
            // Cascading context - NOT IsFixed to allow re-renders to propagate
            cascadeBuilder.OpenComponent<CascadingValue<CalendarContext>>(0);
            cascadeBuilder.AddComponentParameter(1, "Value", _context);
            cascadeBuilder.AddComponentParameter(2, "IsFixed", false);
            cascadeBuilder.AddComponentParameter(3, "ChildContent", (RenderFragment)(childBuilder =>
            {
                if (ChildContent != null)
                {
                    childBuilder.AddContent(0, ChildContent(_context.GetChildContext()));
                }
            }));
            cascadeBuilder.CloseComponent();
        }));
        builder.CloseComponent();

        builder.CloseElement();
    }

    #region JS Interop Callbacks

    /// <summary>
    /// Called from JS when arrow key moves focus by days.
    /// </summary>
    [JSInvokable]
    public void MoveFocus(int days)
    {
        _context.MoveFocus(days);
        _ = UpdateMonthNameIfChangedAsync();
    }

    /// <summary>
    /// Called from JS when arrow key moves focus by weeks.
    /// </summary>
    [JSInvokable]
    public void MoveFocusWeeks(int weeks)
    {
        _context.MoveFocusWeeks(weeks);
        _ = UpdateMonthNameIfChangedAsync();
    }

    /// <summary>
    /// Called from JS when Home key is pressed.
    /// </summary>
    [JSInvokable]
    public void FocusStartOfWeek()
    {
        _context.FocusStartOfWeek();
    }

    /// <summary>
    /// Called from JS when End key is pressed.
    /// </summary>
    [JSInvokable]
    public void FocusEndOfWeek()
    {
        _context.FocusEndOfWeek();
    }

    /// <summary>
    /// Called from JS when PageUp is pressed.
    /// </summary>
    [JSInvokable]
    public async Task PreviousMonth()
    {
        _context.PreviousMonth();
        await UpdateMonthNameAsync();
    }

    /// <summary>
    /// Called from JS when PageDown is pressed.
    /// </summary>
    [JSInvokable]
    public async Task NextMonth()
    {
        _context.NextMonth();
        await UpdateMonthNameAsync();
    }

    /// <summary>
    /// Called from JS when Shift+PageUp is pressed.
    /// </summary>
    [JSInvokable]
    public async Task PreviousYear()
    {
        _context.PreviousYear();
        await UpdateMonthNameAsync();
    }

    /// <summary>
    /// Called from JS when Shift+PageDown is pressed.
    /// </summary>
    [JSInvokable]
    public async Task NextYear()
    {
        _context.NextYear();
        await UpdateMonthNameAsync();
    }

    /// <summary>
    /// Called from JS when Enter/Space is pressed.
    /// </summary>
    [JSInvokable]
    public async Task SelectFocusedDate()
    {
        await _context.SelectDateAsync(_context.FocusedDate);
    }

    private async Task UpdateMonthNameIfChangedAsync()
    {
        // Check if month changed and update name
        await UpdateMonthNameAsync();
        StateHasChanged();
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
