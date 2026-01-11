using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// The viewport area of the scroll area. Contains the scrollable content.
/// This component renders a div that has native scrolling enabled but
/// hides the native scrollbars (which are replaced by custom scrollbars).
/// </summary>
public class ScrollAreaViewport : ComponentBase
{
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

    private ElementReference _elementRef;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && Context is not null)
        {
            // Register this viewport's element reference with the context
            Context.SetViewportRef(_elementRef);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Context is null) return;

        var seq = 0;
        builder.OpenElement(seq++, As);

        // Apply additional attributes first so our attributes can override if needed
        builder.AddMultipleAttributes(seq++, AdditionalAttributes);

        // Core attributes
        builder.AddAttribute(seq++, "id", Context.GetViewportId());
        builder.AddAttribute(seq++, "data-summit-scroll-area-viewport", true);

        // Inline styles to enable scrolling and hide native scrollbars
        // Users can override these with CSS if needed
        var existingStyle = AdditionalAttributes?.TryGetValue("style", out var styleObj) == true
            ? styleObj?.ToString() ?? ""
            : "";

        var scrollbarHideStyles = "overflow: scroll; scrollbar-width: none; -ms-overflow-style: none;";
        var combinedStyle = string.IsNullOrEmpty(existingStyle)
            ? scrollbarHideStyles
            : $"{existingStyle}; {scrollbarHideStyles}";

        builder.AddAttribute(seq++, "style", combinedStyle);

        // Element reference for JS interop
        builder.AddElementReferenceCapture(seq++, capturedRef => _elementRef = capturedRef);

        // Child content
        builder.AddContent(seq++, ChildContent);

        builder.CloseElement();
    }
}
