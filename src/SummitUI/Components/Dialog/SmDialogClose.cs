using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Close button for the dialog.
/// </summary>
public class SmDialogClose : ComponentBase
{
    [CascadingParameter]
    private DialogContext Context { get; set; } = default!;

    /// <summary>
    /// Child content (typically button text/icon).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "button".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "button";

    /// <summary>
    /// Accessible label for the close button.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);

        if (As == "button")
        {
            builder.AddAttribute(1, "type", "button");
        }

        builder.AddAttribute(2, "aria-label", AriaLabel ?? "Close dialog");
        builder.AddAttribute(3, "data-summit-dialog-close", true);
        builder.AddMultipleAttributes(4, AdditionalAttributes);
        builder.AddAttribute(5, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        builder.AddContent(6, ChildContent);
        builder.CloseElement();
    }

    private async Task HandleClickAsync()
    {
        await Context.CloseAsync();
    }
}
