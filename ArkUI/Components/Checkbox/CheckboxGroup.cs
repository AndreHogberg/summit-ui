using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ArkUI;

/// <summary>
/// A container for a group of related checkboxes.
/// Manages collective state and can render hidden inputs for form submission.
/// </summary>
public class CheckboxGroup : ComponentBase
{
    /// <summary>
    /// Child content containing CheckboxRoot components and optionally a CheckboxGroupLabel.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The controlled values (checked items) in the group.
    /// When provided, the component operates in controlled mode.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? Values { get; set; }

    /// <summary>
    /// Callback when the values change.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<string>> ValuesChanged { get; set; }

    /// <summary>
    /// Default values for uncontrolled mode.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? DefaultValues { get; set; }

    /// <summary>
    /// The form name for hidden inputs.
    /// When set, hidden inputs are rendered for each checked checkbox in the group.
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Whether the entire group is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Callback invoked when the group values change.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyList<string>> OnValueChange { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the group element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private readonly CheckboxGroupContext _context = new();
    private HashSet<string> _internalValues = [];
    private string _labelId = "";

    /// <summary>
    /// Whether we're in controlled mode.
    /// </summary>
    private bool IsControlled => Values is not null;

    /// <summary>
    /// The effective values (controlled or uncontrolled).
    /// </summary>
    private HashSet<string> ActiveValues => IsControlled ? [.. Values!] : _internalValues;

    protected override void OnInitialized()
    {
        // Initialize label ID for ARIA relationship
        _labelId = $"{_context.GroupId}-label";

        // Initialize internal state from defaults
        if (DefaultValues is not null)
        {
            _internalValues = [.. DefaultValues];
        }

        SyncContext();
        _context.ToggleValueAsync = ToggleValueAsync;
        _context.NotifyStateChanged = () => StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        SyncContext();
    }

    private void SyncContext()
    {
        _context.Name = Name;
        _context.Values = ActiveValues;
        _context.Disabled = Disabled;
    }

    /// <summary>
    /// Gets the label ID for ARIA relationship.
    /// </summary>
    internal string GetLabelId() => _labelId;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<CheckboxGroupContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "IsFixed", false);
        builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(childBuilder =>
        {
            childBuilder.OpenElement(0, "div");
            childBuilder.AddAttribute(1, "role", "group");
            childBuilder.AddAttribute(2, "id", _context.GroupId);
            childBuilder.AddAttribute(3, "aria-labelledby", _labelId);
            childBuilder.AddAttribute(4, "aria-disabled", Disabled ? "true" : null);
            childBuilder.AddAttribute(5, "data-ark-checkbox-group", "");
            childBuilder.AddAttribute(6, "data-disabled", Disabled ? "" : null);
            childBuilder.AddMultipleAttributes(7, AdditionalAttributes);
            childBuilder.AddContent(8, ChildContent);
            childBuilder.CloseElement();
        }));
        builder.CloseComponent();
    }

    private async Task ToggleValueAsync(string value)
    {
        if (Disabled) return;

        HashSet<string> newValues;

        if (ActiveValues.Contains(value))
        {
            // Remove value
            newValues = [.. ActiveValues];
            newValues.Remove(value);
        }
        else
        {
            // Add value
            newValues = [.. ActiveValues, value];
        }

        // Update internal state for uncontrolled mode
        if (!IsControlled)
        {
            _internalValues = newValues;
        }

        _context.Values = newValues;

        await ValuesChanged.InvokeAsync([.. newValues]);
        await OnValueChange.InvokeAsync([.. newValues]);

        StateHasChanged();
        _context.RaiseStateChanged();
    }
}
