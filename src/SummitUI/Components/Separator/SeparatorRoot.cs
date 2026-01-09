using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// A separator visually or semantically separates content.
/// </summary>
public class SeparatorRoot : ComponentBase
{
    /// <summary>
    /// The orientation of the separator. Defaults to horizontal.
    /// </summary>
    [Parameter]
    public SeparatorOrientation Orientation { get; set; } = SeparatorOrientation.Horizontal;

    /// <summary>
    /// When true, the separator is purely visual and has no semantic meaning.
    /// This removes the separator role and aria attributes.
    /// </summary>
    [Parameter]
    public bool Decorative { get; set; }

    /// <summary>
    /// Additional attributes to apply to the element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var orientationValue = Orientation == SeparatorOrientation.Horizontal ? "horizontal" : "vertical";

        builder.OpenElement(0, "div");

        if (Decorative)
        {
            // Purely decorative - use role="none" to indicate no semantic meaning
            builder.AddAttribute(1, "role", "none");
        }
        else
        {
            // Semantic separator with proper ARIA
            builder.AddAttribute(1, "role", "separator");
            builder.AddAttribute(2, "aria-orientation", orientationValue);
        }

        builder.AddAttribute(3, "data-orientation", orientationValue);
        builder.AddMultipleAttributes(4, AdditionalAttributes);
        builder.CloseElement();
    }
}
