namespace SummitUI;

/// <summary>
/// Cascading context for RadioGroup components.
/// Manages shared state between RadioGroupRoot and its child items.
/// </summary>
public sealed class RadioGroupContext
{
    private readonly List<ItemRegistration> _items = [];

    /// <summary>
    /// Unique identifier for this radio group instance.
    /// Used for generating ARIA relationships.
    /// </summary>
    public string GroupId { get; } = $"summit-radio-group-{Guid.NewGuid():N}";

    /// <summary>
    /// The currently selected value.
    /// </summary>
    public string? Value { get; internal set; }

    /// <summary>
    /// The orientation of the radio group, affects keyboard navigation.
    /// </summary>
    public RadioGroupOrientation Orientation { get; internal set; } = RadioGroupOrientation.Vertical;

    /// <summary>
    /// Whether the entire radio group is disabled.
    /// </summary>
    public bool Disabled { get; internal set; }

    /// <summary>
    /// Whether keyboard navigation should loop from last to first and vice versa.
    /// </summary>
    public bool Loop { get; internal set; } = true;

    /// <summary>
    /// Whether the document is in RTL mode.
    /// Set asynchronously after first render.
    /// </summary>
    public bool IsRtl { get; internal set; }

    /// <summary>
    /// Callback to select a value. Set by RadioGroupRoot.
    /// </summary>
    public Func<string, Task> SelectValueAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to trigger re-render of the root component.
    /// </summary>
    public Action NotifyStateChanged { get; internal set; } = () => { };

    /// <summary>
    /// Event raised when the radio group state changes.
    /// Child components can subscribe to re-render when needed.
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// Raises the OnStateChanged event to notify subscribers.
    /// </summary>
    internal void RaiseStateChanged() => OnStateChanged?.Invoke();

    /// <summary>
    /// Registers an item with the radio group for keyboard navigation.
    /// Items are tracked in registration order.
    /// </summary>
    /// <param name="value">The unique value of the item.</param>
    /// <param name="disabled">Whether the item is disabled.</param>
    public void RegisterItem(string value, bool disabled)
    {
        // Avoid duplicate registrations
        if (_items.Any(i => i.Value == value))
            return;

        _items.Add(new ItemRegistration(value, disabled));
    }

    /// <summary>
    /// Unregisters an item from the radio group.
    /// Called when an item is disposed.
    /// </summary>
    /// <param name="value">The value of the item to unregister.</param>
    public void UnregisterItem(string value)
    {
        _items.RemoveAll(i => i.Value == value);
    }

    /// <summary>
    /// Updates the disabled state of a registered item.
    /// </summary>
    /// <param name="value">The value of the item.</param>
    /// <param name="disabled">The new disabled state.</param>
    public void UpdateItemDisabled(string value, bool disabled)
    {
        var item = _items.FirstOrDefault(i => i.Value == value);
        if (item is not null)
        {
            item.Disabled = disabled;
        }
    }

    /// <summary>
    /// Gets the value that should receive focus (have tabindex="0").
    /// Returns the selected value if it exists and is enabled,
    /// otherwise returns the first enabled item.
    /// </summary>
    /// <returns>The value that should be focusable, or null if no items.</returns>
    public string? GetFocusableValue()
    {
        // If a value is selected and the item is enabled, it should be focusable
        if (!string.IsNullOrEmpty(Value))
        {
            var selectedItem = _items.FirstOrDefault(i => i.Value == Value);
            if (selectedItem is not null && !selectedItem.Disabled && !Disabled)
            {
                return Value;
            }
        }

        // Otherwise, return the first enabled item
        var firstEnabled = _items.FirstOrDefault(i => !i.Disabled && !Disabled);
        return firstEnabled?.Value;
    }

    /// <summary>
    /// Calculates the next item value for keyboard navigation.
    /// </summary>
    /// <param name="currentValue">The currently focused item's value.</param>
    /// <param name="direction">The navigation direction: 1 for next, -1 for previous.</param>
    /// <returns>The next item's value, or null if navigation is not possible.</returns>
    public string? GetNextValue(string currentValue, int direction)
    {
        var enabledItems = _items.Where(i => !i.Disabled && !Disabled).ToList();
        if (enabledItems.Count == 0)
            return null;

        var currentIndex = enabledItems.FindIndex(i => i.Value == currentValue);
        if (currentIndex == -1)
        {
            // Current item not found in enabled items, return first
            return enabledItems[0].Value;
        }

        var nextIndex = currentIndex + direction;

        if (Loop)
        {
            // Wrap around
            if (nextIndex < 0)
                nextIndex = enabledItems.Count - 1;
            else if (nextIndex >= enabledItems.Count)
                nextIndex = 0;
        }
        else
        {
            // Stop at boundaries
            if (nextIndex < 0 || nextIndex >= enabledItems.Count)
                return null;
        }

        return enabledItems[nextIndex].Value;
    }

    /// <summary>
    /// Gets the element ID for a specific item value.
    /// </summary>
    /// <param name="value">The item value.</param>
    /// <returns>The element ID.</returns>
    public string GetItemId(string value) => $"{GroupId}-item-{value}";

    /// <summary>
    /// Internal class to track registered items.
    /// </summary>
    private sealed class ItemRegistration(string value, bool disabled)
    {
        public string Value { get; } = value;
        public bool Disabled { get; set; } = disabled;
    }
}
