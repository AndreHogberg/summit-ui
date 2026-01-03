namespace SummitUI;

/// <summary>
/// Orientation of the tabs component, affects keyboard navigation.
/// </summary>
public enum TabsOrientation
{
    /// <summary>
    /// Horizontal tabs - navigate with ArrowLeft/ArrowRight (reversed in RTL).
    /// </summary>
    Horizontal,

    /// <summary>
    /// Vertical tabs - navigate with ArrowUp/ArrowDown.
    /// </summary>
    Vertical
}

/// <summary>
/// Mode for activating tabs.
/// </summary>
public enum TabsActivationMode
{
    /// <summary>
    /// Tab activates immediately when focused via keyboard navigation.
    /// </summary>
    Auto,

    /// <summary>
    /// Tab requires explicit click or Enter/Space key to activate.
    /// </summary>
    Manual
}
