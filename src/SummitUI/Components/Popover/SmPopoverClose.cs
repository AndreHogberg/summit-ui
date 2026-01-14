using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Close button for the popover.
/// </summary>
public class SmPopoverClose : ComponentBase
{
    [CascadingParameter]
    private PopoverContext Context { get; set; } = default!;

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
        builder.AddAttribute(1, "aria-label", AriaLabel ?? "Close");
        builder.AddAttribute(2, "data-summit-popover-close", true);
        builder.AddMultipleAttributes(3, AdditionalAttributes);
        builder.AddAttribute(4, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        builder.AddContent(5, ChildContent);
        builder.CloseElement();
    }

    private async Task HandleClickAsync()
    {
        await Context.CloseAsync();
    }
}
