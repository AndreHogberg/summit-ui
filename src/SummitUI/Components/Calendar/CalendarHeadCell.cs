using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// A weekday header cell in the calendar grid.
/// Uses abbr attribute to provide full day name for screen readers.
/// </summary>
public class CalendarHeadCell : ComponentBase
{
    [CascadingParameter]
    private CalendarContext Context { get; set; } = default!;

    /// <summary>
    /// The abbreviated weekday name to display.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The full weekday name for the abbr attribute (screen reader accessibility).
    /// </summary>
    [Parameter] public string? Abbreviation { get; set; }

    /// <summary>
    /// Additional attributes to apply to the th element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;

        builder.OpenElement(seq++, "th");
        builder.AddAttribute(seq++, "scope", "col");
        builder.AddAttribute(seq++, "data-summit-calendar-head-cell", true);

        if (!string.IsNullOrEmpty(Abbreviation))
        {
            builder.AddAttribute(seq++, "abbr", Abbreviation);
        }

        builder.AddMultipleAttributes(seq++, AdditionalAttributes);

        if (ChildContent != null)
        {
            builder.AddContent(seq++, ChildContent);
        }

        builder.CloseElement();
    }
}
