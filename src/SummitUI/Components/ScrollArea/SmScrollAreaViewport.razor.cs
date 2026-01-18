using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// The viewport area of the scroll area. Contains the scrollable content.
/// This component renders a div that has native scrolling enabled but
/// hides the native scrollbars (which are replaced by custom scrollbars).
/// </summary>
public partial class SmScrollAreaViewport : ComponentBase
{
    private void RenderContent(RenderTreeBuilder builder)
    {
        if (Context is not null)
        {
            builder.OpenElement(0, As);
            builder.AddAttribute(1, "id", Context.GetViewportId());
            builder.AddAttribute(2, "data-summit-scroll-area-viewport", "true");
            builder.AddAttribute(3, "style", CombinedStyle);
            builder.AddMultipleAttributes(4, AdditionalAttributes);
            builder.AddElementReferenceCapture(5, elemRef => _elementRef = elemRef);
            builder.AddContent(6, ChildContent);
            builder.CloseElement();
        }
    }

    /// <summary>
    /// The scroll area context provided by ScrollAreaRoot.
    /// </summary>
    [CascadingParameter]
    public ScrollAreaContext? Context { get; set; }

    /// <summary>
    /// Child content to render inside the viewport.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

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

    private string CombinedStyle
    {
        get
        {
            var existingStyle = AdditionalAttributes?.TryGetValue("style", out var styleObj) == true
                ? styleObj?.ToString() ?? ""
                : "";

            var scrollbarHideStyles = "overflow: scroll; scrollbar-width: none; -ms-overflow-style: none;";
            return string.IsNullOrEmpty(existingStyle)
                ? scrollbarHideStyles
                : $"{existingStyle}; {scrollbarHideStyles}";
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && Context is not null)
        {
            // Register this viewport's element reference with the context
            Context.SetViewportRef(_elementRef);
        }
    }
}
