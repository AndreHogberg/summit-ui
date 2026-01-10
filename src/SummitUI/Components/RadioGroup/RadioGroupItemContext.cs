namespace SummitUI;

/// <summary>
/// Context passed to RadioGroupItem child content.
/// Provides information about the current state of the radio item.
/// </summary>
public sealed class RadioGroupItemContext
{
    /// <summary>
    /// Whether this radio item is currently checked/selected.
    /// </summary>
    public bool Checked { get; init; }

    /// <summary>
    /// Whether this radio item is disabled.
    /// </summary>
    public bool Disabled { get; init; }
}
