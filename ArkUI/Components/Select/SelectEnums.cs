namespace ArkUI;

/// <summary>
/// Primary placement side for the select content relative to the trigger.
/// </summary>
public enum SelectSide
{
    Top,
    Right,
    Bottom,
    Left
}

/// <summary>
/// Alignment along the placement axis.
/// </summary>
public enum SelectAlign
{
    Start,
    Center,
    End
}

/// <summary>
/// Behavior when Escape key is pressed while select is open.
/// </summary>
public enum SelectEscapeKeyBehavior
{
    /// <summary>Close the select when Escape is pressed.</summary>
    Close,
    /// <summary>Do nothing when Escape is pressed.</summary>
    Ignore
}

/// <summary>
/// Behavior when clicking outside the select.
/// </summary>
public enum SelectOutsideClickBehavior
{
    /// <summary>Close the select when clicking outside.</summary>
    Close,
    /// <summary>Do nothing when clicking outside.</summary>
    Ignore
}
