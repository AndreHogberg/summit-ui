using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Checkbox;

/// <summary>
/// An accessible label for a CheckboxGroup.
/// Automatically associates with the group via aria-labelledby.
/// </summary>
public partial class CheckboxGroupLabel : ComponentBase
{
    /// <summary>
    /// The parent checkbox group context.
    /// </summary>
    [CascadingParameter]
    private CheckboxGroupContext? GroupContext { get; set; }

    /// <summary>
    /// Child content of the label.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "span".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "span";

    /// <summary>
    /// Additional HTML attributes to apply to the label element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string _labelId = "";

    protected override void OnInitialized()
    {
        // Generate label ID from group context if available
        _labelId = GroupContext is not null
            ? $"{GroupContext.GroupId}-label"
            : $"ark-checkbox-group-label-{Guid.NewGuid():N}";
    }
}
