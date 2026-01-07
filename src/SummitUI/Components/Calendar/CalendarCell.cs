using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Context passed to CalendarCell's child content.
/// </summary>
public class CalendarCellContext
{
    /// <summary>
    /// The date this cell represents.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Whether this date is currently selected.
    /// </summary>
    public required bool IsSelected { get; init; }

    /// <summary>
    /// Whether this date is today.
    /// </summary>
    public required bool IsToday { get; init; }

    /// <summary>
    /// Whether this date is currently focused (for keyboard navigation).
    /// </summary>
    public required bool IsFocused { get; init; }

    /// <summary>
    /// Whether this date is outside the displayed month.
    /// </summary>
    public required bool IsOutsideMonth { get; init; }

    /// <summary>
    /// Whether this date is unavailable (outside min/max or custom disabled).
    /// </summary>
    public required bool IsUnavailable { get; init; }

    /// <summary>
    /// Whether the calendar is disabled.
    /// </summary>
    public required bool IsDisabled { get; init; }
}

/// <summary>
/// A cell in the calendar grid representing a single date.
/// </summary>
public class CalendarCell : ComponentBase
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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Use fixed sequence numbers for stable render tree diffing
        var isSelected = Context.IsSelected(Date);
        var isToday = Context.IsToday(Date);
        var isFocused = Context.IsFocused(Date);
        var isOutsideMonth = Context.IsOutsideMonth(Date);
        var isUnavailable = Context.IsDateUnavailable(Date);
        var isDisabled = Context.Disabled;

        builder.OpenElement(0, "td");
        builder.AddAttribute(1, "role", "gridcell");
        builder.AddAttribute(2, "data-summit-calendar-cell", true);

        // Data attributes - always add or use null to remove
        builder.AddAttribute(3, "data-state", isSelected ? "selected" : null);
        builder.AddAttribute(4, "data-today", isToday ? (object)true : null);
        builder.AddAttribute(5, "data-outside-month", isOutsideMonth ? (object)true : null);
        builder.AddAttribute(6, "data-unavailable", isUnavailable ? (object)true : null);
        builder.AddAttribute(7, "data-disabled", isDisabled ? (object)true : null);

        builder.AddMultipleAttributes(8, AdditionalAttributes);

        // Provide cascading cell context
        var cellContext = new CalendarCellContext
        {
            Date = Date,
            IsSelected = isSelected,
            IsToday = isToday,
            IsFocused = isFocused,
            IsOutsideMonth = isOutsideMonth,
            IsUnavailable = isUnavailable,
            IsDisabled = isDisabled
        };

        builder.OpenComponent<CascadingValue<CalendarCellContext>>(9);
        builder.AddComponentParameter(10, "Value", cellContext);
        builder.AddComponentParameter(11, "IsFixed", false);
        builder.AddComponentParameter(12, "ChildContent", (RenderFragment)(childBuilder =>
        {
            if (ChildContent != null)
            {
                childBuilder.AddContent(0, ChildContent(cellContext));
            }
            else if (SimpleContent != null)
            {
                childBuilder.AddContent(0, SimpleContent);
            }
        }));
        builder.CloseComponent();

        builder.CloseElement();
    }
}
