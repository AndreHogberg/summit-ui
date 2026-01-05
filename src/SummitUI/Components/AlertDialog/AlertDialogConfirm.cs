using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Confirm button for the alert dialog. Clicking this completes the dialog with a true result.
/// Displays the text from <see cref="AlertDialogOptions.ConfirmText"/>.
/// </summary>
public class AlertDialogConfirm : ComponentBase
{
    [CascadingParameter]
    private AlertDialogContext Context { get; set; } = default!;

    /// <summary>
    /// Child content that completely replaces the default confirm text.
    /// If provided, <see cref="AlertDialogOptions.ConfirmText"/> is ignored.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "button".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "button";

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private void HandleClick() => Context.Complete(true);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "type", "button");
        builder.AddAttribute(2, "data-summit-alertdialog-confirm", true);

        if (Context.Options.IsDestructive)
        {
            builder.AddAttribute(3, "data-destructive", "");
        }

        builder.AddAttribute(4, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
        builder.AddMultipleAttributes(5, AdditionalAttributes);

        // Use ChildContent if provided, otherwise use Options.ConfirmText
        if (ChildContent is not null)
        {
            builder.AddContent(6, ChildContent);
        }
        else
        {
            builder.AddContent(6, Context.Options.ConfirmText);
        }

        builder.CloseElement();
    }
}
