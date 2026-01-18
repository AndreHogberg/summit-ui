using System.Linq.Expressions;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using SummitUI.Utilities;

namespace SummitUI;

/// <summary>
/// Root container for a radio group. Provides the radiogroup role and manages
/// selection state for child RadioGroupItem components.
/// Follows WAI-ARIA radio group pattern for accessibility.
/// </summary>
public partial class SmRadioGroupRoot : IAsyncDisposable
{
    [Inject]
    private SummitUtilities SummitUtilities { get; set; } = default!;

    /// <summary>
    /// The controlled selected value. When provided, the component operates in controlled mode.
    /// Use with ValueChanged for two-way binding.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// The default selected value for uncontrolled mode.
    /// </summary>
    [Parameter]
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Callback when the selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Expression identifying the bound value (for EditForm validation).
    /// </summary>
    [Parameter]
    public Expression<Func<string?>>? ValueExpression { get; set; }

    /// <summary>
    /// Additional callback invoked when a radio item is selected.
    /// </summary>
    [Parameter]
    public EventCallback<string?> OnValueChange { get; set; }

    /// <summary>
    /// Whether the entire radio group is disabled.
    /// When true, no items can be selected or focused.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Whether a selection is required for form validation.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// The name attribute for the hidden form input.
    /// When set, a hidden input is rendered for native form submission.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Accessible label for the radio group.
    /// Use when there's no visible label element.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// ID of the element that labels this radio group.
    /// Use when there's a visible label element.
    /// </summary>
    [Parameter]
    public string? AriaLabelledBy { get; set; }

    /// <summary>
    /// ID of the element that describes this radio group.
    /// </summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>
    /// The orientation of the radio group, affects keyboard navigation.
    /// Vertical: ArrowUp/ArrowDown navigate between items.
    /// Horizontal: ArrowLeft/ArrowRight navigate (RTL-aware).
    /// </summary>
    [Parameter]
    public RadioGroupOrientation Orientation { get; set; } = RadioGroupOrientation.Vertical;

    /// <summary>
    /// Whether keyboard navigation loops from last to first and vice versa.
    /// </summary>
    [Parameter]
    public bool Loop { get; set; } = true;

    /// <summary>
    /// Child content containing RadioGroupItem components.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Cascading EditContext for form integration.
    /// </summary>
    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    private readonly RadioGroupContext _context = new();
    private string? _internalValue;
    private bool _isRtlInitialized;
    private FieldIdentifier? _fieldIdentifier;

    /// <summary>
    /// Whether we're operating in controlled mode.
    /// </summary>
    private bool IsControlled => Value is not null;

    /// <summary>
    /// The effective selected value (controlled or uncontrolled).
    /// </summary>
    private string? EffectiveValue => IsControlled ? Value : _internalValue;

    /// <summary>
    /// The orientation string for ARIA and data attributes.
    /// </summary>
    private string OrientationString => Orientation == RadioGroupOrientation.Horizontal ? "horizontal" : "vertical";

    protected override void OnInitialized()
    {
        _internalValue = DefaultValue;

        // Initialize context
        _context.Value = EffectiveValue;
        _context.Orientation = Orientation;
        _context.Disabled = Disabled;
        _context.Loop = Loop;
        _context.SelectValueAsync = SelectValueAsync;
        _context.NotifyStateChanged = () => StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        // Set up EditContext field identifier for validation (needs to happen after cascading parameters are set)
        if (!_fieldIdentifier.HasValue && EditContext is not null && ValueExpression is not null)
        {
            _fieldIdentifier = FieldIdentifier.Create(ValueExpression);
        }

        // Sync context with current parameter values
        _context.Value = EffectiveValue;
        _context.Orientation = Orientation;
        _context.Disabled = Disabled;
        _context.Loop = Loop;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_isRtlInitialized)
        {
            _isRtlInitialized = true;
            _context.IsRtl = await SummitUtilities.IsRtlAsync();
        }
    }

    private async Task SelectValueAsync(string value)
    {
        if (Disabled) return;
        if (EffectiveValue == value) return;

        // Update internal state for uncontrolled mode
        if (!IsControlled)
        {
            _internalValue = value;
        }

        // Update context
        _context.Value = value;

        // Fire callbacks
        await ValueChanged.InvokeAsync(value);
        await OnValueChange.InvokeAsync(value);

        // Notify EditContext of the change for validation
        if (EditContext is not null && _fieldIdentifier.HasValue)
        {
            EditContext.NotifyFieldChanged(_fieldIdentifier.Value);
        }

        // Notify state change
        _context.RaiseStateChanged();
        StateHasChanged();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
