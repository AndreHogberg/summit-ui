using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// The draggable thumb inside a ScrollAreaScrollbar.
/// Size and position are automatically calculated based on scroll position and content size.
/// </summary>
public class SmScrollAreaThumb : ComponentBase
{
    /// <summary>
    /// The scroll area context provided by ScrollAreaRoot.
    /// </summary>
    [CascadingParameter]
    public ScrollAreaContext? Context { get; set; }

    /// <summary>
    /// The parent scrollbar component.
    /// </summary>
    [CascadingParameter]
    public SmScrollAreaScrollbar? Scrollbar { get; set; }

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

    private ElementReference _elementRef;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && Scrollbar is not null)
        {
            // Register this thumb's element reference with the parent scrollbar
            Scrollbar.ThumbRef = _elementRef;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Context is null || Scrollbar is null) return;

        var orientation = Scrollbar.Orientation;
        var state = Context.GetScrollbarState(orientation);

        var seq = 0;
        builder.OpenElement(seq++, As);

        // Apply additional attributes first
        builder.AddMultipleAttributes(seq++, AdditionalAttributes);

        // Core attributes
        builder.AddAttribute(seq++, "id", Context.GetThumbId(orientation));
        builder.AddAttribute(seq++, "data-summit-scroll-area-thumb", true);
        builder.AddAttribute(seq++, "data-state", state);

        // Orientation-specific data attribute for easier CSS targeting
        if (orientation == ScrollAreaOrientation.Vertical)
        {
            builder.AddAttribute(seq++, "data-summit-scroll-area-thumb-y", true);
        }
        else
        {
            builder.AddAttribute(seq++, "data-summit-scroll-area-thumb-x", true);
        }

        // Element reference
        builder.AddElementReferenceCapture(seq++, capturedRef => _elementRef = capturedRef);

        builder.CloseElement();
    }
}
