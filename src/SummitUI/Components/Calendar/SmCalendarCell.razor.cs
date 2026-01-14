using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// A cell in the calendar grid representing a single date.
/// </summary>
public partial class SmCalendarCell
{
    [CascadingParameter]
    private CalendarContext Context { get; set; } = default!;

    /// <summary>
    /// The date this cell represents.
    /// </summary>
    [Parameter, EditorRequired]
    public DateOnly Date { get; set; }

    /// <summary>
    /// The child content. Receives CalendarCellContext.
    /// </summary>
    [Parameter] public RenderFragment<CalendarCellContext>? ChildContent { get; set; }

    /// <summary>
    /// Simple child content without context.
    /// </summary>
    [Parameter] public RenderFragment? SimpleContent { get; set; }

    /// <summary>
    /// Additional attributes to apply to the td element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnInitialized()
    {
        Context.OnStateChanged += StateHasChanged;
    }
}
