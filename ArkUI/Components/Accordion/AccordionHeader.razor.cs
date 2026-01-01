using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Accordion;

/// <summary>
/// Semantic heading wrapper for accordion trigger.
/// Renders with role="heading" and aria-level.
/// </summary>
public partial class AccordionHeader : ComponentBase
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
}
