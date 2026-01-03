namespace SummitUI;

/// <summary>
/// Primary placement side for floating elements relative to their trigger.
/// </summary>
public enum Side
{
    Top,
    Right,
    Bottom,
    Left
}

/// <summary>
/// Alignment along the placement axis.
/// </summary>
public enum Align
{
    Start,
    Center,
    End
}

/// <summary>
/// Behavior when Escape key is pressed while a component is open.
/// </summary>
public enum EscapeKeyBehavior
{
    /// <summary>Close the component when Escape is pressed.</summary>
    Close,
    /// <summary>Do nothing when Escape is pressed.</summary>
    Ignore
}

/// <summary>
/// Behavior when clicking outside a component.
/// </summary>
public enum OutsideClickBehavior
{
    /// <summary>Close the component when clicking outside.</summary>
    Close,
    /// <summary>Do nothing when clicking outside.</summary>
    Ignore
}

/// <summary>
/// Direction for keyboard navigation.
/// </summary>
public enum NavigationDirection
{
    Up,
    Down,
    Left,
    Right
}
