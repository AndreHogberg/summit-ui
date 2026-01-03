using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Root component for a date/time field with segmented editing.
/// Supports both DateOnly and DateTime values with culture-aware formatting.
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
    
    // Configuration
    [Parameter] public DateFieldGranularity Granularity { get; set; } = DateFieldGranularity.Day;
    [Parameter] public HourCycle HourCycle { get; set; } = HourCycle.Auto;
    [Parameter] public CultureInfo? Locale { get; set; }
    
    /// <summary>
    /// Custom date pattern to override the culture's default short date pattern.
    /// Use standard .NET date format specifiers (e.g., "yyyy-MM-dd", "dd.MM.yyyy").
    /// Only affects date segments; time segments are derived from culture and granularity.
    /// </summary>
    [Parameter] public string? DatePattern { get; set; }
    
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
    /// If DateTimeValue or DateTimeValueChanged is set, or Granularity includes time, use DateTime mode.
    /// </summary>
    private bool IsDateTimeMode => DateTimeValueChanged.HasDelegate || 
                                   DateTimeValue.HasValue || 
                                   Granularity != DateFieldGranularity.Day;

    protected override void OnParametersSet()
    {
        var culture = Locale ?? CultureInfo.CurrentCulture;
        
        // Determine validation state
        var isInvalid = Invalid || IsOutOfRange();
        
        if (IsDateTimeMode)
        {
            _context.SetDateTimeState(
                DateTimeValue,
                DateTimePlaceholder,
                Granularity,
                HourCycle,
                culture,
                DatePattern,
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
                Granularity,
                HourCycle,
                culture,
                DatePattern,
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
