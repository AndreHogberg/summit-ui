using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Root component for a date/time field with segmented editing.
/// Supports both DateOnly and DateTime values with explicit format strings.
/// </summary>
public class DateFieldRoot : ComponentBase
{
    // DateOnly binding
    [Parameter] public DateOnly? Value { get; set; }
    [Parameter] public EventCallback<DateOnly?> ValueChanged { get; set; }
    [Parameter] public DateOnly Placeholder { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    
    // DateTime binding
    [Parameter] public DateTime? DateTimeValue { get; set; }
    [Parameter] public EventCallback<DateTime?> DateTimeValueChanged { get; set; }
    [Parameter] public DateTime DateTimePlaceholder { get; set; } = DateTime.Now;
    
    // Format configuration
    /// <summary>
    /// Date format pattern using standard .NET date format specifiers.
    /// Examples: "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy".
    /// </summary>
    [Parameter, EditorRequired] public string Format { get; set; } = default!;
    
    /// <summary>
    /// Time format pattern for DateTime mode. Only used when binding to DateTimeValue.
    /// Use "HH:mm" for 24-hour format, "hh:mm" for 12-hour format (shows AM/PM).
    /// The separator character in the pattern determines the time separator displayed.
    /// Defaults to "HH:mm" (24-hour format with colon separator).
    /// </summary>
    [Parameter] public string TimeFormat { get; set; } = "HH:mm";
    
    // Validation constraints
    [Parameter] public DateOnly? MinValue { get; set; }
    [Parameter] public DateOnly? MaxValue { get; set; }
    [Parameter] public DateTime? MinDateTime { get; set; }
    [Parameter] public DateTime? MaxDateTime { get; set; }
    
    // States
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool Invalid { get; set; }
    
    // Form integration
    [Parameter] public string? Name { get; set; }
    [Parameter] public bool Required { get; set; }
    
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private readonly DateFieldContext _context = new();
    
    /// <summary>
    /// Determines if we're in DateTime mode based on which binding is provided.
    /// If DateTimeValue or DateTimeValueChanged is set, use DateTime mode.
    /// </summary>
    private bool IsDateTimeMode => DateTimeValueChanged.HasDelegate || DateTimeValue.HasValue;

    protected override void OnParametersSet()
    {
        // Determine validation state
        var isInvalid = Invalid || IsOutOfRange();
        
        if (IsDateTimeMode)
        {
            _context.SetDateTimeState(
                DateTimeValue,
                DateTimePlaceholder,
                Format,
                TimeFormat,
                Disabled,
                ReadOnly,
                isInvalid,
                MinDateTime,
                MaxDateTime,
                DateTimeValueChanged);
        }
        else
        {
            _context.SetDateState(
                Value,
                Placeholder,
                Format,
                Disabled,
                ReadOnly,
                isInvalid,
                MinValue,
                MaxValue,
                ValueChanged);
        }
    }

    private bool IsOutOfRange()
    {
        if (IsDateTimeMode)
        {
            if (!DateTimeValue.HasValue) return false;
            if (MinDateTime.HasValue && DateTimeValue.Value < MinDateTime.Value) return true;
            if (MaxDateTime.HasValue && DateTimeValue.Value > MaxDateTime.Value) return true;
        }
        else
        {
            if (!Value.HasValue) return false;
            if (MinValue.HasValue && Value.Value < MinValue.Value) return true;
            if (MaxValue.HasValue && Value.Value > MaxValue.Value) return true;
        }
        return false;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var isInvalid = Invalid || IsOutOfRange();
        
        builder.OpenComponent<CascadingValue<DateFieldContext>>(0);
        builder.AddAttribute(1, "Value", _context);
        builder.AddAttribute(2, "IsFixed", true);
        builder.AddAttribute(3, "ChildContent", (RenderFragment)(builder2 =>
        {
            builder2.OpenElement(4, "div");
            builder2.AddAttribute(5, "role", "group");
            builder2.AddAttribute(6, "id", _context.Id);
            builder2.AddAttribute(7, "aria-labelledby", _context.LabelId);
            
            // Data attributes for styling hooks
            if (Disabled) builder2.AddAttribute(8, "data-disabled", "");
            if (ReadOnly) builder2.AddAttribute(9, "data-readonly", "");
            if (isInvalid) builder2.AddAttribute(10, "data-invalid", "");
            
            builder2.AddMultipleAttributes(11, AdditionalAttributes);
            builder2.AddContent(12, ChildContent);
            
            // Hidden input for form submission
            if (!string.IsNullOrEmpty(Name))
            {
                builder2.OpenElement(13, "input");
                builder2.AddAttribute(14, "type", "hidden");
                builder2.AddAttribute(15, "name", Name);
                builder2.AddAttribute(16, "value", GetFormValue());
                if (Required) builder2.AddAttribute(17, "required", true);
                builder2.CloseElement();
            }
            
            builder2.CloseElement();
        }));
        builder.CloseComponent();
    }

    private string GetFormValue()
    {
        if (IsDateTimeMode)
        {
            return DateTimeValue?.ToString("o") ?? ""; // ISO 8601 format
        }
        return Value?.ToString("yyyy-MM-dd") ?? "";
    }
}
