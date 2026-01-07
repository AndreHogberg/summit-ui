using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// The date field component for DatePicker. Provides segmented date editing.
/// This component extends DateFieldRoot and integrates with DatePickerContext.
/// </summary>
public class DatePickerField : ComponentBase
{
    [CascadingParameter] private DatePickerContext DatePickerContext { get; set; } = default!;
    [CascadingParameter] private EditContext? EditContext { get; set; }

    #region Parameters (forwarded to internal DateFieldRoot)

    /// <summary>
    /// The currently selected date value.
    /// If not provided, uses the value from DatePickerRoot.
    /// </summary>
    [Parameter] public DateOnly? Value { get; set; }

    /// <summary>
    /// Event callback when the value changes.
    /// </summary>
    [Parameter] public EventCallback<DateOnly?> ValueChanged { get; set; }

    /// <summary>
    /// The placeholder date used when no value is set.
    /// </summary>
    [Parameter] public DateOnly? Placeholder { get; set; }

    /// <summary>
    /// Date format pattern using standard .NET date format specifiers.
    /// </summary>
    [Parameter] public string? Format { get; set; }

    /// <summary>
    /// The calendar system to use for display.
    /// </summary>
    [Parameter] public CalendarSystem CalendarSystem { get; set; } = CalendarSystem.Gregorian;

    /// <summary>
    /// The locale to use for formatting.
    /// </summary>
    [Parameter] public string? Locale { get; set; }

    /// <summary>
    /// The minimum selectable date.
    /// </summary>
    [Parameter] public DateOnly? MinValue { get; set; }

    /// <summary>
    /// The maximum selectable date.
    /// </summary>
    [Parameter] public DateOnly? MaxValue { get; set; }

    /// <summary>
    /// Whether the field is disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Whether the field is read-only.
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>
    /// Whether the field is in an invalid state.
    /// </summary>
    [Parameter] public bool Invalid { get; set; }

    /// <summary>
    /// The name attribute for form submission.
    /// </summary>
    [Parameter] public string? Name { get; set; }

    /// <summary>
    /// Whether the field is required.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Expression identifying the bound value (for EditForm validation).
    /// </summary>
    [Parameter] public System.Linq.Expressions.Expression<Func<DateOnly?>>? ValueExpression { get; set; }

    /// <summary>
    /// Child content (DatePickerLabel, DatePickerInput, DatePickerTrigger, etc.).
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    #endregion

    private readonly DateFieldContext _dateFieldContext = new();
    private ElementReference _fieldRef;
    private FieldIdentifier? _fieldIdentifier;

    protected override void OnInitialized()
    {
        if (DatePickerContext == null)
            throw new InvalidOperationException("DatePickerField must be used within a DatePickerRoot.");

        // Set up EditContext field identifier for validation
        if (EditContext is not null && ValueExpression is not null)
        {
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
            EditContext.OnValidationStateChanged += (sender, args) => HandleValidationStateChanged();
        }

        _dateFieldContext.OnStateChanged += HandleStateChanged;
    }

    private void HandleValidationStateChanged()
    {
        if (EditContext is not null && _fieldIdentifier.HasValue)
        {
            var isInvalid = EditContext.GetValidationMessages(_fieldIdentifier.Value).Any();
            if (isInvalid != _dateFieldContext.Invalid)
            {
                _dateFieldContext.SetInvalid(isInvalid);
                StateHasChanged();
            }
        }
    }

    private void HandleStateChanged()
    {
        if (EditContext is not null && _fieldIdentifier.HasValue)
        {
            EditContext.NotifyFieldChanged(_fieldIdentifier.Value);
        }
    }

    protected override void OnParametersSet()
    {
        // Determine effective disabled/readonly state (inherit from DatePickerContext if not explicitly set)
        var effectiveDisabled = Disabled || DatePickerContext.Disabled;
        var effectiveReadOnly = ReadOnly || DatePickerContext.ReadOnly;

        // Determine validation state
        var isInvalid = Invalid || IsOutOfRange();

        _dateFieldContext.SetDateState(
            Value,
            Placeholder ?? DateOnly.FromDateTime(DateTime.Today),
            Format,
            CalendarSystem,
            Locale,
            effectiveDisabled,
            effectiveReadOnly,
            isInvalid,
            MinValue,
            MaxValue,
            EventCallback.Factory.Create<DateOnly?>(this, HandleValueChangedAsync));
    }

    private bool IsOutOfRange()
    {
        if (!Value.HasValue) return false;
        if (MinValue.HasValue && Value.Value < MinValue.Value) return true;
        if (MaxValue.HasValue && Value.Value > MaxValue.Value) return true;
        return false;
    }

    private async Task HandleValueChangedAsync(DateOnly? newValue)
    {
        await ValueChanged.InvokeAsync(newValue);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            // Register field element with DatePickerContext for popover anchoring
            DatePickerContext.RegisterField(_fieldRef);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var effectiveDisabled = Disabled || DatePickerContext.Disabled;
        var effectiveReadOnly = ReadOnly || DatePickerContext.ReadOnly;
        var isInvalid = Invalid || IsOutOfRange();

        if (EditContext is not null && _fieldIdentifier.HasValue)
        {
            isInvalid |= EditContext.GetValidationMessages(_fieldIdentifier.Value).Any();
        }

        // Cascade DateFieldContext
        builder.OpenComponent<CascadingValue<DateFieldContext>>(0);
        builder.AddComponentParameter(1, "Value", _dateFieldContext);
        builder.AddComponentParameter(2, "IsFixed", true);
        builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(contextBuilder =>
        {
            // Render wrapper div that captures element reference
            contextBuilder.OpenElement(0, "div");
            contextBuilder.AddAttribute(1, "role", "group");
            contextBuilder.AddAttribute(2, "id", _dateFieldContext.Id);
            contextBuilder.AddAttribute(3, "aria-labelledby", _dateFieldContext.LabelId);
            contextBuilder.AddAttribute(4, "data-summit-datepicker-field", true);

            // Data attributes for styling hooks
            if (effectiveDisabled) contextBuilder.AddAttribute(5, "data-disabled", "");
            if (effectiveReadOnly) contextBuilder.AddAttribute(6, "data-readonly", "");
            if (isInvalid) contextBuilder.AddAttribute(7, "data-invalid", "");

            contextBuilder.AddMultipleAttributes(8, AdditionalAttributes);
            contextBuilder.AddElementReferenceCapture(9, elementRef => _fieldRef = elementRef);
            contextBuilder.AddContent(10, ChildContent);

            // Hidden input for form submission
            if (!string.IsNullOrEmpty(Name))
            {
                contextBuilder.OpenElement(11, "input");
                contextBuilder.AddAttribute(12, "type", "hidden");
                contextBuilder.AddAttribute(13, "name", Name);
                contextBuilder.AddAttribute(14, "value", Value?.ToString("yyyy-MM-dd") ?? "");
                if (Required) contextBuilder.AddAttribute(15, "required", true);
                contextBuilder.CloseElement();
            }

            contextBuilder.CloseElement();
        }));
        builder.CloseComponent();
    }
}
