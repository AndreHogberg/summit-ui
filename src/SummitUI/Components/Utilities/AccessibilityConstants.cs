namespace SummitUI;

/// <summary>
/// Constants for accessibility-related styling and attributes.
/// </summary>
public static class AccessibilityConstants
{
    /// <summary>
    /// CSS for visually hiding content while keeping it accessible to screen readers.
    /// </summary>
    /// <remarks>
    /// This pattern hides content visually but keeps it in the accessibility tree,
    /// allowing screen readers to announce the content. Use this for:
    /// <list type="bullet">
    /// <item><description>ARIA live regions for announcements</description></item>
    /// <item><description>Skip links that should only be visible on focus</description></item>
    /// <item><description>Labels that are visually redundant but semantically necessary</description></item>
    /// </list>
    /// </remarks>
    public const string VisuallyHiddenStyle =
        "position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px; " +
        "overflow: hidden; clip: rect(0, 0, 0, 0); white-space: nowrap; border: 0;";
}
