using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ArkUI;

/// <summary>
/// Semantic heading wrapper for accordion trigger.
/// Renders with role="heading" and aria-level.
/// </summary>
public class AccordionHeader : ComponentBase
{
    [CascadingParameter]
    private AccordionContext Context { get; set; } = default!;

    /// <summary>
    /// Child content (typically AccordionTrigger).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "h3".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "h3";

    /// <summary>
    /// ARIA heading level (1-6). Defaults to 3.
    /// </summary>
    [Parameter]
    public int Level { get; set; } = 3;

    /// <summary>
    /// Additional HTML attributes to apply.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "role", "heading");
        builder.AddAttribute(2, "aria-level", Level);
        builder.AddAttribute(3, "data-orientation", Context.Orientation.ToString().ToLowerInvariant());
        builder.AddAttribute(4, "data-ark-accordion-header", true);
        builder.AddMultipleAttributes(5, AdditionalAttributes);
        builder.AddContent(6, ChildContent);
        builder.CloseElement();
    }
}
