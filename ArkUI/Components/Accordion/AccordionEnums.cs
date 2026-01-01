namespace ArkUI.Components.Accordion;

/// <summary>
/// Type of accordion expansion behavior.
/// </summary>
public enum AccordionType
{
    /// <summary>
    /// Only one item can be expanded at a time.
    /// </summary>
    Single,

    /// <summary>
    /// Multiple items can be expanded simultaneously.
    /// </summary>
    Multiple
}

/// <summary>
/// Orientation of the accordion, affects keyboard navigation.
/// </summary>
public enum AccordionOrientation
{
    /// <summary>
    /// Vertical accordion - navigate with ArrowUp/ArrowDown.
    /// </summary>
    Vertical,

    /// <summary>
    /// Horizontal accordion - navigate with ArrowLeft/ArrowRight (reversed in RTL).
    /// </summary>
    Horizontal
}
