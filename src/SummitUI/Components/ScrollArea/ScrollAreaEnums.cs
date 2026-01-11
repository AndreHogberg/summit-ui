namespace SummitUI;

/// <summary>
/// Determines the visibility behavior of scroll area scrollbars.
/// </summary>
public enum ScrollAreaType
{
    /// <summary>
    /// Show scrollbars when hovering over the scroll area, hide after a delay.
    /// This is the default behavior.
    /// </summary>
    Hover,

    /// <summary>
    /// Show scrollbars when actively scrolling, hide after a delay.
    /// Similar to macOS scrollbar behavior.
    /// </summary>
    Scroll,

    /// <summary>
    /// Show scrollbars automatically when content overflows.
    /// Scrollbars remain visible while overflow exists.
    /// </summary>
    Auto,

    /// <summary>
    /// Always show scrollbars regardless of content overflow.
    /// Similar to setting overflow: scroll in CSS.
    /// </summary>
    Always
}

/// <summary>
/// Orientation of a scrollbar.
/// </summary>
public enum ScrollAreaOrientation
{
    /// <summary>
    /// Vertical scrollbar (for vertical scrolling).
    /// </summary>
    Vertical,

    /// <summary>
    /// Horizontal scrollbar (for horizontal scrolling).
    /// </summary>
    Horizontal
}
