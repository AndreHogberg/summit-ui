using System.Globalization;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using SummitUI.Services;

namespace SummitUI;

/// <summary>
/// Root component for a date/time field with segmented editing.
/// Supports both DateOnly and DateTime values with explicit format strings.
/// </summary>
public partial class SmDateFieldRoot : ComponentBase
{
    [Inject] private ILiveAnnouncer? Announcer { get; set; }

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
    /// If not specified, auto-detects based on locale.
    /// </summary>
    [Parameter] public string? Format { get; set; }

    /// <summary>
    /// The culture to use for formatting and localization.
    /// If not specified, uses <see cref="CultureInfo.CurrentCulture"/>.
    /// </summary>
    /// <remarks>
    /// Users can create custom CultureInfo instances with their own translations and calendar configurations.
    /// </remarks>
    [Parameter] public CultureInfo? Culture { get; set; }

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

    /// <summary>
    /// Cascading EditContext for form integration.
    /// </summary>
    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    /// <summary>
    /// Expression identifying the bound value (for EditForm validation).
    /// </summary>
    [Parameter]
    public System.Linq.Expressions.Expression<Func<DateOnly?>>? ValueExpression { get; set; }

    /// <summary>
    /// Expression identifying the bound DateTime value (for EditForm validation).
    /// </summary>
    [Parameter]
    public System.Linq.Expressions.Expression<Func<DateTime?>>? DateTimeValueExpression { get; set; }

    /// <summary>
    /// Function to generate the announcement for screen readers when the date changes.
    /// Receives the formatted date string.
    /// If null, no announcement is made (aria-describedby on segments typically suffices).
    /// </summary>
    /// <example>
    /// GetDateAnnouncement="@(date => $"Date set to {date}")"
    /// </example>
    [Parameter]
    public Func<string, string>? GetDateAnnouncement { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private readonly DateFieldContext _context = new();
    private FieldIdentifier? _fieldIdentifier;
    private DateOnly? _previousDateValue;
    private DateTime? _previousDateTimeValue;

    /// <summary>
    /// Determines if we're in DateTime mode based on which binding is provided.
    /// If DateTimeValue or DateTimeValueChanged is set, use DateTime mode.
    /// </summary>
    private bool IsDateTimeMode => DateTimeValueChanged.HasDelegate || DateTimeValue.HasValue;

    /// <summary>
    /// Determines the invalid state for rendering.
    /// </summary>
    private bool IsInvalid
    {
        get
        {
            var isInvalid = Invalid || IsOutOfRange();

            if (EditContext is not null && _fieldIdentifier.HasValue)
            {
                isInvalid |= EditContext.GetValidationMessages(_fieldIdentifier.Value).Any();
            }

            return isInvalid;
        }
    }

    protected override void OnInitialized()
    {
        // Set up EditContext field identifier for validation
        if (EditContext is not null)
        {
            if (IsDateTimeMode && DateTimeValueExpression is not null)
            {
                _fieldIdentifier = FieldIdentifier.Create(DateTimeValueExpression);
            }
            else if (!IsDateTimeMode && ValueExpression is not null)
            {
                _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
            }

            if (_fieldIdentifier.HasValue)
            {
                EditContext.OnValidationStateChanged += (sender, args) => HandleValidationStateChanged();
            }
        }

        _context.OnStateChanged += HandleStateChanged;
    }

    private void HandleValidationStateChanged()
    {
        if (EditContext is not null && _fieldIdentifier.HasValue)
        {
            var isInvalid = EditContext.GetValidationMessages(_fieldIdentifier.Value).Any();
            if (isInvalid != _context.Invalid)
            {
                _context.SetInvalid(isInvalid);
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

        // Announce date changes to screen readers
        AnnounceValueChange();
    }

    private void AnnounceValueChange()
    {
        if (GetDateAnnouncement is null || Announcer is null)
        {
            return;
        }

        if (IsDateTimeMode)
        {
            // Only announce when we have a complete value and it changed
            if (DateTimeValue.HasValue && DateTimeValue != _previousDateTimeValue)
            {
                var culture = Culture ?? CultureInfo.CurrentCulture;
                var formatted = DateTimeValue.Value.ToString("f", culture); // Full date/time format
                Announcer.Announce(GetDateAnnouncement(formatted));
                _previousDateTimeValue = DateTimeValue;
            }
        }
        else
        {
            // Only announce when we have a complete value and it changed  
            if (Value.HasValue && Value != _previousDateValue)
            {
                var culture = Culture ?? CultureInfo.CurrentCulture;
                var formatted = Value.Value.ToString("D", culture); // Long date format
                Announcer.Announce(GetDateAnnouncement(formatted));
                _previousDateValue = Value;
            }
        }
    }

    protected override void OnParametersSet()
    {
        // Track previous values for announcement detection
        if (_previousDateValue is null && Value.HasValue)
        {
            _previousDateValue = Value;
        }
        if (_previousDateTimeValue is null && DateTimeValue.HasValue)
        {
            _previousDateTimeValue = DateTimeValue;
        }

        // Determine validation state
        var isInvalid = Invalid || IsOutOfRange();

        // Determine effective culture
        var effectiveCulture = Culture ?? CultureInfo.CurrentCulture;

        if (IsDateTimeMode)
        {
            _context.SetDateTimeState(
                DateTimeValue,
                DateTimePlaceholder,
                Format,
                TimeFormat,
                effectiveCulture,
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
                effectiveCulture,
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

    private string GetFormValue()
    {
        if (IsDateTimeMode)
        {
            return DateTimeValue?.ToString("o") ?? ""; // ISO 8601 format
        }
        return Value?.ToString("yyyy-MM-dd") ?? "";
    }
}
