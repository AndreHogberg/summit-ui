using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// The draggable thumb inside a ScrollAreaScrollbar.
/// Size and position are automatically calculated based on scroll position and content size.
/// </summary>
public partial class SmScrollAreaThumb : ComponentBase
{
    private void RenderContent(RenderTreeBuilder builder)
    {
        if (Context is not null && Scrollbar is not null)
        {
            builder.OpenElement(0, As);
            builder.AddAttribute(1, "id", Context.GetThumbId(Scrollbar.Orientation));
            builder.AddAttribute(2, "data-summit-scroll-area-thumb", "true");
            builder.AddAttribute(3, "data-state", Context.GetScrollbarState(Scrollbar.Orientation));
            builder.AddAttribute(4, "data-summit-scroll-area-thumb-y", Scrollbar.Orientation == ScrollAreaOrientation.Vertical ? true : null);
            builder.AddAttribute(5, "data-summit-scroll-area-thumb-x", Scrollbar.Orientation == ScrollAreaOrientation.Horizontal ? true : null);
            builder.AddMultipleAttributes(6, AdditionalAttributes);
            builder.AddElementReferenceCapture(7, elemRef => _elementRef = elemRef);
            builder.CloseElement();
        }
    }

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

    private ElementReference _elementRef = default!;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && Scrollbar is not null)
        {
            // Register this thumb's element reference with the parent scrollbar
            Scrollbar.ThumbRef = _elementRef;
        }
    }
}
