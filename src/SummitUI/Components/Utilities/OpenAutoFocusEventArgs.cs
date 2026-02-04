namespace SummitUI;

/// <summary>
/// Event arguments for the OnOpenAutoFocus event.
/// Allows preventing the default auto-focus behavior.
/// </summary>
public class OpenAutoFocusEventArgs : EventArgs
{
    private bool _defaultPrevented;

    /// <summary>
    /// Gets whether the default auto-focus behavior has been prevented.
    /// </summary>
    public bool DefaultPrevented => _defaultPrevented;

    /// <summary>
    /// Prevents the default auto-focus behavior.
    /// When called, the dialog will not automatically focus
    /// the first focusable element, avoiding browser scroll.
    /// </summary>
    public void PreventDefault() => _defaultPrevented = true;
}
