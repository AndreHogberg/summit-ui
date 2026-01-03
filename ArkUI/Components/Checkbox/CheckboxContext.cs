namespace ArkUI;

/// <summary>
/// Context passed to child content of CheckboxRoot.
/// Provides state information for rendering custom indicators.
/// </summary>
public sealed class CheckboxContext
{
    /// <summary>
    /// The current checked state of the checkbox.
    /// </summary>
    public CheckedState State { get; init; }

    /// <summary>
    /// Whether the checkbox is checked (convenience property).
    /// </summary>
    public bool IsChecked => State == CheckedState.Checked;

    /// <summary>
    /// Whether the checkbox is in an indeterminate state (convenience property).
    /// </summary>
    public bool IsIndeterminate => State == CheckedState.Indeterminate;

    /// <summary>
    /// Whether the checkbox is unchecked (convenience property).
    /// </summary>
    public bool IsUnchecked => State == CheckedState.Unchecked;

    /// <summary>
    /// Whether the checkbox is disabled.
    /// </summary>
    public bool IsDisabled { get; init; }
}
