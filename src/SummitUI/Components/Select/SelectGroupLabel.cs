using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Label for a select group. Used for accessibility labeling.
/// </summary>
public class SelectGroupLabel : ComponentBase
{
    [CascadingParameter]
    private SelectGroupContext GroupContext { get; set; } = default!;

    /// <summary>
    /// Child content (the label text).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "role", "presentation");
        builder.AddAttribute(2, "id", GroupContext.LabelId);
        builder.AddAttribute(3, "data-summit-select-group-label", "");
        builder.AddMultipleAttributes(4, AdditionalAttributes);
        builder.AddContent(5, ChildContent);
        builder.CloseElement();
    }
}
