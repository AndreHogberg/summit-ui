using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Label for a group of menu items.
/// </summary>
public class SmDropdownMenuGroupLabel : ComponentBase
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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", GroupLabelId);
        builder.AddAttribute(2, "data-summit-dropdown-menu-group-label", "");
        builder.AddMultipleAttributes(3, AdditionalAttributes);
        builder.AddContent(4, ChildContent);
        builder.CloseElement();
    }
}
