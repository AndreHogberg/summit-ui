using Microsoft.AspNetCore.Components;
using SummitUI.Utilities;

namespace SummitUI;

/// <summary>
///     Root component that manages the state of the accordion.
///     Provides cascading context to child components.
/// </summary>
public partial class AccordionRoot : ComponentBase
{
    private readonly AccordionContext _context = new();
    private HashSet<string> _internalValues = [];

    [Inject] private SummitUtilities SummitUtilities { get; set; } = default!;

    /// <summary>
    ///     Child content containing AccordionItem components.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    ///     Type of accordion expansion (single or multiple).
    /// </summary>
    [Parameter]
    public AccordionType Type { get; set; } = AccordionType.Single;

    /// <summary>
    ///     Controlled expanded value for single mode. When provided, component operates in controlled mode.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    ///     Controlled expanded values for multiple mode. When provided, component operates in controlled mode.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? Values { get; set; }

    /// <summary>
    ///     Default expanded value for single mode (uncontrolled).
    /// </summary>
    [Parameter]
    public string? DefaultValue { get; set; }

    /// <summary>
    ///     Default expanded values for multiple mode (uncontrolled).
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? DefaultValues { get; set; }

    /// <summary>
    ///     Callback when expanded value changes (single mode).
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    ///     Callback when expanded values change (multiple mode).
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<string>> ValuesChanged { get; set; }

    /// <summary>
    ///     Callback invoked when an item's expansion state changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> OnValueChange { get; set; }

    /// <summary>
    ///     Orientation of the accordion (affects keyboard navigation).
    /// </summary>
    [Parameter]
    public AccordionOrientation Orientation { get; set; } = AccordionOrientation.Vertical;

    /// <summary>
    ///     Whether keyboard navigation loops from last to first and vice versa.
    /// </summary>
    [Parameter]
    public bool Loop { get; set; } = true;

    /// <summary>
    ///     Whether the entire accordion is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    ///     Whether items can be collapsed in single mode (allows closing the last open item).
    ///     Defaults to true.
    /// </summary>
    [Parameter]
    public bool Collapsible { get; set; } = true;

    /// <summary>
    ///     Additional HTML attributes to apply.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    ///     Determines if we're in controlled mode for single-expansion.
    /// </summary>
    private bool IsControlledSingle => Type == AccordionType.Single && Value is not null;

    /// <summary>
    ///     Determines if we're in controlled mode for multiple-expansion.
    /// </summary>
    private bool IsControlledMultiple => Type == AccordionType.Multiple && Values is not null;

    /// <summary>
    ///     Effective expanded values (controlled or uncontrolled).
    /// </summary>
    private HashSet<string> ActiveValues
    {
        get
        {
            if (Type == AccordionType.Single)
            {
                string? value = Value ?? (_internalValues.Count > 0 ? _internalValues.First() : null);
                return value is not null ? [value] : [];
            }

            return Values is not null ? [.. Values] : _internalValues;
        }
    }

    protected override void OnInitialized()
    {
        // Initialize internal state from defaults
        if (DefaultValue is not null)
        {
            _internalValues = [DefaultValue];
        }
        else if (DefaultValues is not null)
        {
            _internalValues = [.. DefaultValues];
        }

        SyncContext();
        _context.ToggleItemAsync = ToggleItemAsync;
        _context.FocusTriggerAsync = FocusTriggerAsync;
    }

    protected override void OnParametersSet() => SyncContext();

    private void SyncContext()
    {
        _context.ExpandedValues = ActiveValues;
        _context.Orientation = Orientation;
        _context.Loop = Loop;
        _context.Disabled = Disabled;
    }

    private async Task FocusTriggerAsync(string value)
    {
        string triggerId = _context.GetTriggerId(value);
        await SummitUtilities.FocusElementByIdAsync(triggerId);
    }

    private async Task ToggleItemAsync(string value)
    {
        if (Disabled)
        {
            return;
        }

        bool isCurrentlyExpanded = ActiveValues.Contains(value);

        if (Type == AccordionType.Single)
        {
            await ToggleSingleAsync(value, isCurrentlyExpanded);
        }
        else
        {
            await ToggleMultipleAsync(value, isCurrentlyExpanded);
        }
    }

    private async Task ToggleSingleAsync(string value, bool isCurrentlyExpanded)
    {
        string? newValue;

        if (isCurrentlyExpanded)
        {
            // Collapsing - only allowed if Collapsible is true
            if (!Collapsible)
            {
                return;
            }

            newValue = null;
        }
        else
        {
            // Expanding
            newValue = value;
        }

        // Update internal state for uncontrolled mode
        if (!IsControlledSingle)
        {
            _internalValues = newValue is not null ? [newValue] : [];
        }

        _context.ExpandedValues = newValue is not null ? [newValue] : [];
        await ValueChanged.InvokeAsync(newValue);
        await OnValueChange.InvokeAsync(newValue);
        StateHasChanged();
    }

    private async Task ToggleMultipleAsync(string value, bool isCurrentlyExpanded)
    {
        HashSet<string> newValues;

        if (isCurrentlyExpanded)
        {
            newValues = [.. ActiveValues];
            newValues.Remove(value);
        }
        else
        {
            newValues = [.. ActiveValues, value];
        }

        // Update internal state for uncontrolled mode
        if (!IsControlledMultiple)
        {
            _internalValues = newValues;
        }

        _context.ExpandedValues = newValues;
        await ValuesChanged.InvokeAsync([.. newValues]);
        await OnValueChange.InvokeAsync(value);
        StateHasChanged();
    }
}
