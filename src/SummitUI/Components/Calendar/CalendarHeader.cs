using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Container for the calendar header, typically containing navigation buttons and heading.
/// </summary>
public class CalendarHeader : ComponentBase
{
    [CascadingParameter]
    private CalendarContext Context { get; set; } = default!;

    /// <summary>
    /// The child content.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional attributes to apply to the header element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;

        builder.OpenElement(seq++, "header");
        builder.AddAttribute(seq++, "data-summit-calendar-header", true);
        builder.AddMultipleAttributes(seq++, AdditionalAttributes);

        if (ChildContent != null)
        {
            builder.AddContent(seq++, ChildContent);
        }

        builder.CloseElement();
    }
}
