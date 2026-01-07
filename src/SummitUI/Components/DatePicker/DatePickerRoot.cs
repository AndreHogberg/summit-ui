using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Root component for a DatePicker that combines DateField, Calendar, and Popover.
/// Provides unified value binding and coordinates state between all sub-components.
/// </summary>
public class DatePickerRoot : ComponentBase, IAsyncDisposable
{
    [Inject] private PopoverService PopoverService { get; set; } = default!;

    #region Value Parameters

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

    #endregion

    #region Popover State Parameters

    /// <summary>
    /// Controlled open state. When provided, component operates in controlled mode.
    /// </summary>
    [Parameter] public bool? Open { get; set; }

    /// <summary>
    /// Default open state for uncontrolled mode.
    /// </summary>
    [Parameter] public bool DefaultOpen { get; set; }

    /// <summary>
    /// Callback when open state changes.
    /// </summary>
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Callback invoked when popover opens.
    /// </summary>
    [Parameter] public EventCallback OnOpen { get; set; }

    /// <summary>
    /// Callback invoked when popover closes.
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }

    /// <summary>
    /// Whether to close the popover when a date is selected from the calendar.
    /// Defaults to true.
    /// </summary>
    [Parameter] public bool CloseOnSelect { get; set; } = true;

    /// <summary>
    /// Whether the popover is modal (traps focus when open).
    /// </summary>
    [Parameter] public bool Modal { get; set; } = true;

    #endregion

    #region Shared Constraint Parameters

    /// <summary>
    /// The minimum selectable date.
    /// </summary>
    [Parameter] public DateOnly? MinValue { get; set; }

    /// <summary>
    /// The maximum selectable date.
    /// </summary>
    [Parameter] public DateOnly? MaxValue { get; set; }

    /// <summary>
    /// Whether the date picker is disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Whether the date picker is read-only (can view but not edit).
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>
    /// Custom function to determine if a date should be disabled.
    /// </summary>
    [Parameter] public Func<DateOnly, bool>? IsDateDisabled { get; set; }

    #endregion

    #region Format Parameters

    /// <summary>
    /// Date format pattern using standard .NET date format specifiers.
    /// Examples: "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy".
    /// If not specified, auto-detects based on locale.
    /// </summary>
    [Parameter] public string? Format { get; set; }

    /// <summary>
    /// The calendar system to use for display and navigation.
    /// The bound Value remains as DateOnly (Gregorian), but dates are displayed
    /// and navigated using the selected calendar system.
    /// Defaults to Gregorian.
    /// </summary>
    [Parameter] public CalendarSystem CalendarSystem { get; set; } = CalendarSystem.Gregorian;

    /// <summary>
    /// The locale to use for formatting (e.g., "en-US", "fr-FR").
    /// If not specified, auto-detects from browser.
    /// </summary>
    [Parameter] public string? Locale { get; set; }

    /// <summary>
    /// The day of the week to start on.
    /// If not specified, auto-detects from culture.
    /// </summary>
    [Parameter] public DayOfWeek? WeekStartsOn { get; set; }

    /// <summary>
    /// Whether to always display 6 weeks in the calendar for consistent height.
    /// </summary>
    [Parameter] public bool FixedWeeks { get; set; }

    #endregion

    #region Form Integration

    /// <summary>
    /// The name attribute for form submission.
    /// </summary>
    [Parameter] public string? Name { get; set; }

    /// <summary>
    /// Whether the field is required.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Whether the field is in an invalid state.
    /// </summary>
    [Parameter] public bool Invalid { get; set; }

    /// <summary>
    /// Cascading EditContext for form integration.
    /// </summary>
    [CascadingParameter] private EditContext? EditContext { get; set; }

    /// <summary>
    /// Expression identifying the bound value (for EditForm validation).
    /// </summary>
    [Parameter] public System.Linq.Expressions.Expression<Func<DateOnly?>>? ValueExpression { get; set; }

    #endregion

    #region Content and Attributes

    /// <summary>
    /// Child content containing DatePickerField, DatePickerContent, etc.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    #endregion

    private readonly DatePickerContext _context = new();
    private DateOnly? _internalValue;
    private bool _internalOpen;
    private bool _isDisposed;
    private FieldIdentifier? _fieldIdentifier;

    /// <summary>
    /// Effective value state (controlled or uncontrolled).
    /// </summary>
    private DateOnly? EffectiveValue => ValueChanged.HasDelegate ? Value : (_internalValue ?? DefaultValue);

    /// <summary>
    /// Effective open state (controlled or uncontrolled).
    /// </summary>
    private bool IsOpen => Open ?? _internalOpen;

    protected override void OnInitialized()
    {
        _internalOpen = DefaultOpen;
        _internalValue = DefaultValue;

        // Set up EditContext field identifier for validation
        if (EditContext is not null && ValueExpression is not null)
        {
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }

        // Initialize context
        _context.PopoverContext.IsOpen = IsOpen;
        _context.PopoverContext.Modal = Modal;
        _context.PopoverContext.ToggleAsync = ToggleAsync;
        _context.PopoverContext.OpenAsync = OpenPopoverAsync;
        _context.PopoverContext.CloseAsync = ClosePopoverAsync;
        _context.PopoverContext.RegisterTrigger = RegisterTrigger;
        _context.PopoverContext.RegisterContent = RegisterContent;
        _context.PopoverContext.NotifyStateChanged = () => StateHasChanged();

        _context.ToggleAsync = ToggleAsync;
        _context.OpenAsync = OpenPopoverAsync;
        _context.CloseAsync = ClosePopoverAsync;
        _context.RegisterField = RegisterField;
        _context.OnCalendarDateSelectedAsync = HandleCalendarDateSelectedAsync;
        _context.NotifyStateChanged = () => StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        var previousIsOpen = _context.PopoverContext.IsOpen;

        // Sync context with current state
        _context.PopoverContext.IsOpen = IsOpen;
        _context.PopoverContext.Modal = Modal;
        _context.CloseOnSelect = CloseOnSelect;
        _context.Disabled = Disabled;
        _context.ReadOnly = ReadOnly;
        _context.Value = EffectiveValue;
        _context.Placeholder = GetPlaceholder();

        // If controlled open state changed, notify the context so content re-renders
        if (previousIsOpen != IsOpen)
        {
            // When closing via controlled state, set animation flag
            if (!IsOpen && previousIsOpen)
            {
                _context.PopoverContext.IsAnimatingClosed = true;
            }

            _context.PopoverContext.RaiseStateChanged();
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Root wrapper div
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "data-summit-datepicker-root", true);
        builder.AddAttribute(2, "data-state", IsOpen ? "open" : "closed");
        if (Disabled) builder.AddAttribute(3, "data-disabled", "");
        if (ReadOnly) builder.AddAttribute(4, "data-readonly", "");
        builder.AddMultipleAttributes(5, AdditionalAttributes);

        // Cascade DatePickerContext
        builder.OpenComponent<CascadingValue<DatePickerContext>>(6);
        builder.AddComponentParameter(7, "Value", _context);
        builder.AddComponentParameter(8, "IsFixed", false);
        builder.AddComponentParameter(9, "ChildContent", (RenderFragment)(contextBuilder =>
        {
            // Also cascade PopoverContext for popover components
            contextBuilder.OpenComponent<CascadingValue<PopoverContext>>(0);
            contextBuilder.AddComponentParameter(1, "Value", _context.PopoverContext);
            contextBuilder.AddComponentParameter(2, "IsFixed", false);
            contextBuilder.AddComponentParameter(3, "ChildContent", ChildContent);
            contextBuilder.CloseComponent();
        }));
        builder.CloseComponent();

        builder.CloseElement();
    }

    #region Value Management

    /// <summary>
    /// Gets the effective value for child components.
    /// </summary>
    internal DateOnly? GetValue() => EffectiveValue;

    /// <summary>
    /// Gets the placeholder for child components.
    /// </summary>
    internal DateOnly GetPlaceholder() => Placeholder ?? DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Called when the DateField value changes.
    /// </summary>
    internal async Task HandleFieldValueChangedAsync(DateOnly? newValue)
    {
        await UpdateValueAsync(newValue);
    }

    /// <summary>
    /// Called when a date is selected from the calendar.
    /// </summary>
    private async Task HandleCalendarDateSelectedAsync(DateOnly date)
    {
        await UpdateValueAsync(date);

        // Auto-close if configured
        if (CloseOnSelect && IsOpen)
        {
            await ClosePopoverAsync();
        }
    }

    private async Task UpdateValueAsync(DateOnly? newValue)
    {
        if (!ValueChanged.HasDelegate)
        {
            _internalValue = newValue;
        }

        // Update context value so DatePickerField can sync
        _context.Value = ValueChanged.HasDelegate ? newValue : _internalValue;

        await ValueChanged.InvokeAsync(newValue);
        await OnValueChange.InvokeAsync(newValue);

        // Notify EditContext of field change
        if (EditContext is not null && _fieldIdentifier.HasValue)
        {
            EditContext.NotifyFieldChanged(_fieldIdentifier.Value);
        }

        // Notify context subscribers (DatePickerField) of the change
        _context.RaiseStateChanged();
        StateHasChanged();
    }

    #endregion

    #region Popover Management

    private async Task ToggleAsync()
    {
        if (Disabled) return;

        if (IsOpen)
            await ClosePopoverAsync();
        else
            await OpenPopoverAsync();
    }

    private async Task OpenPopoverAsync()
    {
        if (IsOpen || Disabled) return;

        // Only uncontrolled popovers participate in automatic close behavior
        if (Open is null)
        {
            await PopoverService.RegisterOpenAsync(_context.PopoverContext);
            _internalOpen = true;
        }

        _context.PopoverContext.IsOpen = true;
        await OpenChanged.InvokeAsync(true);
        await OnOpen.InvokeAsync();
        StateHasChanged();
    }

    private async Task ClosePopoverAsync()
    {
        if (!IsOpen) return;

        // Only uncontrolled popovers participate in automatic close behavior
        if (Open is null)
        {
            PopoverService.Unregister(_context.PopoverContext);
            _internalOpen = false;
        }

        _context.PopoverContext.IsOpen = false;
        _context.PopoverContext.IsAnimatingClosed = true;
        await OpenChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
        StateHasChanged();
        _context.PopoverContext.RaiseStateChanged();
    }

    private void RegisterTrigger(ElementReference element)
    {
        _context.PopoverContext.TriggerElement = element;
    }

    private void RegisterContent(ElementReference element)
    {
        _context.PopoverContext.ContentElement = element;
    }

    private void RegisterField(ElementReference element)
    {
        _context.FieldElement = element;
        // Use field element as the trigger/anchor for popover positioning
        _context.PopoverContext.TriggerElement = element;
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Only unregister uncontrolled popovers from service
        if (Open is null)
        {
            PopoverService.Unregister(_context.PopoverContext);
        }

        await Task.CompletedTask;
    }
}
