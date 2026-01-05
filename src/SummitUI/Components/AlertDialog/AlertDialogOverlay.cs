using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Backdrop overlay for the alert dialog.
/// Click behavior is controlled by <see cref="AlertDialogOptions.AllowOverlayClose"/>.
/// </summary>
public class AlertDialogOverlay : ComponentBase
{
    [CascadingParameter]
    private AlertDialogContext Context { get; set; } = default!;

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

    private void HandleClick()
    {
        if (Context.Options.AllowOverlayClose)
        {
            Context.Complete(false);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Only render when open or animating closed
        if (!Context.IsOpen && !Context.IsAnimatingClosed) return;

        builder.OpenElement(0, As);
        builder.AddAttribute(1, "data-summit-alertdialog-overlay", true);
        builder.AddAttribute(2, "data-state", DataState);

        if (Context.Options.IsDestructive)
        {
            builder.AddAttribute(3, "data-destructive", "");
        }

        builder.AddAttribute(4, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
        builder.AddMultipleAttributes(5, AdditionalAttributes);
        builder.CloseElement();
    }
}
