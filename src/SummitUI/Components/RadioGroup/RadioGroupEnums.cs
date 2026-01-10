namespace SummitUI;

/// <summary>
/// Orientation of the radio group component, affects keyboard navigation.
/// </summary>
public enum RadioGroupOrientation
{
    /// <summary>
    /// Vertical radio group - navigate with ArrowUp/ArrowDown.
    /// </summary>
    Vertical,

    /// <summary>
    /// Horizontal radio group - navigate with ArrowLeft/ArrowRight (reversed in RTL).
    /// </summary>
    Horizontal
}
