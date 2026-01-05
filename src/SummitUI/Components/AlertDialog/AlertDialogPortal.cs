using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Fixed-position container for the alert dialog.
/// Only renders when the dialog is open or animating closed.
/// </summary>
public class AlertDialogPortal : ComponentBase
{
    [CascadingParameter]
    private AlertDialogContext Context { get; set; } = default!;

    /// <summary>
    /// Child content of the portal.
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

    private string DataState => Context.IsOpen ? "open" : "closed";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Only render when open or animating closed
        if (!Context.IsOpen && !Context.IsAnimatingClosed) return;

        builder.OpenElement(0, As);
        builder.AddAttribute(1, "id", Context.PortalId);
        builder.AddAttribute(2, "data-summit-alertdialog-portal", true);
        builder.AddAttribute(3, "data-state", DataState);

        if (Context.Options.IsDestructive)
        {
            builder.AddAttribute(4, "data-destructive", "");
        }

        builder.AddMultipleAttributes(5, AdditionalAttributes);
        builder.AddContent(6, ChildContent);
        builder.CloseElement();
    }
}
