using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// The corner element that appears between vertical and horizontal scrollbars.
/// Only rendered when both scrollbars are present.
/// </summary>
public partial class SmScrollAreaCorner : ComponentBase
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

    private bool ShouldRenderContent
    {
        get
        {
            if (Context is null) return false;

            // Only render if both scrollbars are present and visible
            var showVertical = Context.HasVerticalScrollbar && Context.ShouldShowScrollbar(ScrollAreaOrientation.Vertical);
            var showHorizontal = Context.HasHorizontalScrollbar && Context.ShouldShowScrollbar(ScrollAreaOrientation.Horizontal);

            return showVertical && showHorizontal;
        }
    }
}
