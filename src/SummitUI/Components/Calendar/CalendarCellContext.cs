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
