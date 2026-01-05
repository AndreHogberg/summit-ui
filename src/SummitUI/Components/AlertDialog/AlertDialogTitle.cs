using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Accessible title for the alert dialog. Renders with the id used by aria-labelledby.
/// Displays the title from <see cref="AlertDialogOptions.Title"/>.
/// </summary>
public class AlertDialogTitle : ComponentBase
{
    [CascadingParameter]
    private AlertDialogContext Context { get; set; } = default!;

    /// <summary>
    /// Child content that completely replaces the default title text.
    /// If provided, <see cref="AlertDialogOptions.Title"/> is ignored.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "h2".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "h2";

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "id", Context.TitleId);
        builder.AddAttribute(2, "data-summit-alertdialog-title", true);

        if (Context.Options.IsDestructive)
        {
            builder.AddAttribute(3, "data-destructive", "");
        }

        builder.AddMultipleAttributes(4, AdditionalAttributes);

        // Use ChildContent if provided, otherwise use Options.Title
        if (ChildContent is not null)
        {
            builder.AddContent(5, ChildContent);
        }
        else
        {
            builder.AddContent(5, Context.Options.Title);
        }

        builder.CloseElement();
    }
}
