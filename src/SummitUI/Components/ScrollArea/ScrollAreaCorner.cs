using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// The corner element that appears between vertical and horizontal scrollbars.
/// Only rendered when both scrollbars are present.
/// </summary>
public class ScrollAreaCorner : ComponentBase
{
    /// <summary>
    /// The scroll area context provided by ScrollAreaRoot.
    /// </summary>
    [CascadingParameter]
    public ScrollAreaContext? Context { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// Additional HTML attributes to apply.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Context is null) return;

        // Only render if both scrollbars are present and visible
        var showVertical = Context.HasVerticalScrollbar && Context.ShouldShowScrollbar(ScrollAreaOrientation.Vertical);
        var showHorizontal = Context.HasHorizontalScrollbar && Context.ShouldShowScrollbar(ScrollAreaOrientation.Horizontal);

        if (!showVertical || !showHorizontal)
        {
            return;
        }

        var seq = 0;
        builder.OpenElement(seq++, As);

        // Apply additional attributes first
        builder.AddMultipleAttributes(seq++, AdditionalAttributes);

        // Core attributes
        builder.AddAttribute(seq++, "data-summit-scroll-area-corner", true);

        builder.CloseElement();
    }
}
