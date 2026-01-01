namespace ArkUI.Components.Popover;

/// <summary>
/// Primary placement side for the popover content relative to the trigger.
/// </summary>
public enum PopoverSide
{
    Top,
    Right,
    Bottom,
    Left
}

/// <summary>
/// Alignment along the placement axis.
/// </summary>
public enum PopoverAlign
{
    Start,
    Center,
    End
}

/// <summary>
/// Behavior when Escape key is pressed while popover is open.
/// </summary>
public enum EscapeKeyBehavior
{
    /// <summary>Close the popover when Escape is pressed.</summary>
    Close,
    /// <summary>Do nothing when Escape is pressed.</summary>
    Ignore
}

/// <summary>
/// Behavior when clicking outside the popover.
/// </summary>
public enum OutsideClickBehavior
{
    /// <summary>Close the popover when clicking outside.</summary>
    Close,
    /// <summary>Do nothing when clicking outside.</summary>
    Ignore
}
