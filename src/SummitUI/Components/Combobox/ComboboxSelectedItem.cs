namespace SummitUI;

/// <summary>
/// Model for a selected item that includes the value, label, and a delegate to deselect it.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public sealed class ComboboxSelectedItem<TValue> where TValue : notnull
{
    /// <summary>
    /// The selected value.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// The display label for this value (if available from registration).
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Delegate to deselect this item. Call this to remove the item from the selection.
    /// </summary>
    public Func<Task> DeselectAsync { get; }

    public ComboboxSelectedItem(TValue value, string label, Func<Task> deselectAsync)
    {
        Value = value;
        Label = label;
        DeselectAsync = deselectAsync;
    }
}
