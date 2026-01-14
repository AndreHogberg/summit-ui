using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Accessible description for the dialog. Renders with the id used by aria-describedby.
/// </summary>
public class SmDialogDescription : ComponentBase
{
    [CascadingParameter]
    private DialogContext Context { get; set; } = default!;

    /// <summary>
    /// Child content (the description text).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "p".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "p";

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "id", Context.DescriptionId);
        builder.AddAttribute(2, "data-summit-dialog-description", true);
        builder.AddMultipleAttributes(3, AdditionalAttributes);
        builder.AddContent(4, ChildContent);
        builder.CloseElement();
    }
}
