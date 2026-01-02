namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// Primary placement side for the dropdown menu relative to the trigger.
/// </summary>
public enum DropdownMenuSide
{
    Top,
    Right,
    Bottom,
    Left
}

/// <summary>
/// Alignment along the placement axis.
/// </summary>
public enum DropdownMenuAlign
{
    Start,
    Center,
    End
}

/// <summary>
/// Behavior when Escape key is pressed while menu is open.
/// </summary>
public enum EscapeKeyBehavior
{
    /// <summary>Close the menu when Escape is pressed.</summary>
    Close,
    /// <summary>Do nothing when Escape is pressed.</summary>
    Ignore
}

/// <summary>
/// Behavior when clicking outside the menu.
/// </summary>
public enum OutsideClickBehavior
{
    /// <summary>Close the menu when clicking outside.</summary>
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
