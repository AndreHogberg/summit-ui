using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// A row in the calendar grid (either header row or week row).
/// </summary>
public class CalendarGridRow : ComponentBase
{
    [CascadingParameter]
    private CalendarContext Context { get; set; } = default!;

    /// <summary>
    /// The child content.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional attributes to apply to the tr element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;

        builder.OpenElement(seq++, "tr");
        builder.AddAttribute(seq++, "data-summit-calendar-grid-row", true);
        builder.AddMultipleAttributes(seq++, AdditionalAttributes);

        if (ChildContent != null)
        {
            builder.AddContent(seq++, ChildContent);
        }

        builder.CloseElement();
    }
}
