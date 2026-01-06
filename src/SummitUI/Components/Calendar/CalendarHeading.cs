using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Displays the current month and year. Acts as a live region for screen readers.
/// </summary>
public class CalendarHeading : ComponentBase
{
    [CascadingParameter]
    private CalendarContext Context { get; set; } = default!;

    /// <summary>
    /// The heading level (1-6). Defaults to 2.
    /// </summary>
    [Parameter] public int Level { get; set; } = 2;

    /// <summary>
    /// Custom format for the heading. If not provided, uses localized month/year.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional attributes to apply to the heading element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnInitialized()
    {
        Context.OnStateChanged += StateHasChanged;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;
        var tag = $"h{Math.Clamp(Level, 1, 6)}";

        builder.OpenElement(seq++, tag);
        builder.AddAttribute(seq++, "id", Context.HeadingId);
        builder.AddAttribute(seq++, "data-summit-calendar-heading", true);
        builder.AddAttribute(seq++, "aria-live", "polite");
        builder.AddAttribute(seq++, "aria-atomic", "true");
        builder.AddMultipleAttributes(seq++, AdditionalAttributes);

        if (ChildContent != null)
        {
            builder.AddContent(seq++, ChildContent);
        }
        else
        {
            // Default: display localized month and year
            var heading = !string.IsNullOrEmpty(Context.MonthName)
                ? Context.MonthName
                : $"{Context.DisplayedMonth:MMMM yyyy}";
            builder.AddContent(seq++, heading);
        }

        builder.CloseElement();
    }
}
