using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// Label for a group of menu items.
/// </summary>
public partial class DropdownMenuGroupLabel : ComponentBase
{
    [CascadingParameter(Name = "GroupLabelId")]
    private string GroupLabelId { get; set; } = default!;

    /// <summary>
    /// Child content (label text).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
