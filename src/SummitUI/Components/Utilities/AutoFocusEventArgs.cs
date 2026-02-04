namespace SummitUI;

/// <summary>
/// Event arguments for auto-focus events (OnOpenAutoFocus, OnCloseAutoFocus).
/// Allows preventing the default auto-focus behavior.
/// </summary>
public class AutoFocusEventArgs : EventArgs
{
    private bool _defaultPrevented;

    /// <summary>
    /// Gets whether the default auto-focus behavior has been prevented.
    /// </summary>
    public bool DefaultPrevented => _defaultPrevented;

    /// <summary>
    /// Prevents the default auto-focus behavior.
    /// When called on open, the component will not automatically focus
    /// the first focusable element, avoiding browser scroll.
    /// When called on close, the component will not return focus to the trigger element.
    /// </summary>
    public void PreventDefault() => _defaultPrevented = true;
}
