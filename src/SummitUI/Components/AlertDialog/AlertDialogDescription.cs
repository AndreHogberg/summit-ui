using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Accessible description for the alert dialog. Renders with the id used by aria-describedby.
/// Displays the message from the <see cref="IAlertDialogService.ConfirmAsync"/> call.
/// </summary>
public class AlertDialogDescription : ComponentBase
{
    [CascadingParameter]
    private AlertDialogContext Context { get; set; } = default!;

    /// <summary>
    /// Child content that completely replaces the default message.
    /// If provided, the message from ConfirmAsync is ignored.
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
        builder.AddAttribute(2, "data-summit-alertdialog-description", true);

        if (Context.Options.IsDestructive)
        {
            builder.AddAttribute(3, "data-destructive", "");
        }

        builder.AddMultipleAttributes(4, AdditionalAttributes);

        // Use ChildContent if provided, otherwise use Message
        if (ChildContent is not null)
        {
            builder.AddContent(5, ChildContent);
        }
        else
        {
            builder.AddContent(5, Context.Message);
        }

        builder.CloseElement();
    }
}
