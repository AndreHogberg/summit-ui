using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Cancel button for the alert dialog. Clicking this completes the dialog with a false result.
/// Displays the text from <see cref="AlertDialogOptions.CancelText"/>.
/// 
/// When <see cref="AlertDialogOptions.IsDestructive"/> is true, this button will receive
/// initial focus to prevent accidental confirmation of destructive actions.
/// </summary>
public class AlertDialogCancel : ComponentBase
{
    [CascadingParameter]
    private AlertDialogContext Context { get; set; } = default!;

    /// <summary>
    /// Child content that completely replaces the default cancel text.
    /// If provided, <see cref="AlertDialogOptions.CancelText"/> is ignored.
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

    private void HandleClick() => Context.Complete(false);

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "type", "button");
        builder.AddAttribute(2, "data-summit-alertdialog-cancel", true);

        if (Context.Options.IsDestructive)
        {
            builder.AddAttribute(3, "data-destructive", "");
        }

        builder.AddAttribute(4, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
        builder.AddMultipleAttributes(5, AdditionalAttributes);

        // Use ChildContent if provided, otherwise use Options.CancelText
        if (ChildContent is not null)
        {
            builder.AddContent(6, ChildContent);
        }
        else
        {
            builder.AddContent(6, Context.Options.CancelText);
        }

        builder.CloseElement();
    }
}
