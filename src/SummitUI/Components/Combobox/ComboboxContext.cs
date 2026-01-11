using Microsoft.AspNetCore.Components;

using SummitUI.Base;

namespace SummitUI;

/// <summary>
/// Cascading context shared between combobox sub-components.
/// Provides state and callbacks for coordinating trigger, input, content, items, and selected values.
/// </summary>
/// <typeparam name="TValue">The type of the combobox value.</typeparam>
public sealed class ComboboxContext<TValue> : IPortalContext where TValue : notnull
{
    /// <summary>
    /// Unique identifier for this combobox instance, used for ARIA relationships.
    /// </summary>
    public string ComboboxId { get; }

    /// <summary>
    /// Currently selected values (multi-select).
    /// </summary>
    public HashSet<TValue> SelectedValues { get; } = new();

    /// <summary>
    /// Current filter text from the input.
    /// </summary>
    public string FilterText { get; internal set; } = "";

    /// <summary>
    /// Currently highlighted key (for keyboard navigation).
    /// This is the string key, not the TValue, for JS interop compatibility.
    /// </summary>
    public string? HighlightedKey { get; internal set; }

    /// <summary>
    /// Registry mapping string keys to TValue items.
    /// Used to look up the actual value when an item is selected via JS interop.
    /// </summary>
    public Dictionary<string, TValue> ItemRegistry { get; } = new();

    /// <summary>
    /// Registry mapping string keys to labels.
    /// Used to look up the label when an item is selected.
    /// </summary>
    public Dictionary<string, string> LabelRegistry { get; } = new();

    /// <summary>
    /// Registry mapping string keys to disabled state.
    /// Used for keyboard navigation to skip disabled items.
    /// </summary>
    public Dictionary<string, bool> DisabledRegistry { get; } = new();

    /// <summary>
    /// Registry mapping values to their display labels for SelectedValues component.
    /// </summary>
    public Dictionary<TValue, string> ValueToLabelRegistry { get; } = new();

    private bool _isOpen;

    /// <summary>
    /// Current open state of the combobox popup.
    /// </summary>
    public bool IsOpen
    {
        get => _isOpen;
        internal set => _isOpen = value;
    }

    /// <summary>
    /// True while close animation is in progress. Portal should stay rendered.
    /// </summary>
    public bool IsAnimatingClosed { get; set; }

    /// <summary>
    /// Whether the combobox is disabled.
    /// </summary>
    public bool Disabled { get; internal set; }

    /// <summary>
    /// Whether the combobox is required.
    /// </summary>
    public bool Required { get; internal set; }

    /// <summary>
    /// Whether the combobox is in an invalid state.
    /// </summary>
    public bool Invalid { get; internal set; }

    /// <summary>
    /// Whether there is an input element for filtering.
    /// </summary>
    public bool HasInput { get; internal set; }

    /// <summary>
    /// Reference to the trigger element (set by ComboboxTrigger).
    /// </summary>
    public ElementReference TriggerElement { get; internal set; }

    /// <summary>
    /// Reference to the input element (set by ComboboxInput).
    /// </summary>
    public ElementReference InputElement { get; internal set; }

    /// <summary>
    /// Reference to the content element (set by ComboboxContent).
    /// </summary>
    public ElementReference ContentElement { get; internal set; }

    /// <summary>
    /// Callback to toggle the combobox open/closed.
    /// </summary>
    public Func<Task> ToggleAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to explicitly open the combobox.
    /// </summary>
    public Func<Task> OpenAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to explicitly close the combobox.
    /// </summary>
    public Func<Task> CloseAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to toggle an item's selection state by its string key.
    /// </summary>
    public Func<string, Task> ToggleItemByKeyAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to select an item by its string key (adds to selection).
    /// </summary>
    public Func<string, Task> SelectItemByKeyAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to deselect an item by its string key (removes from selection).
    /// </summary>
    public Func<string, Task> DeselectItemByKeyAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to deselect an item by its value directly.
    /// </summary>
    public Func<TValue, Task> DeselectValueAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to clear all selected values.
    /// </summary>
    public Func<Task> ClearAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to update the highlighted key.
    /// </summary>
    public Func<string?, Task> SetHighlightedKeyAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to update the filter text.
    /// </summary>
    public Func<string, Task> SetFilterTextAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to focus the trigger element. Set by the content component.
    /// </summary>
    public Func<Task> FocusTriggerAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to focus the input element. Set by the input component.
    /// </summary>
    public Func<Task> FocusInputAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Action to register the trigger element reference.
    /// </summary>
    public Action<ElementReference> RegisterTrigger { get; internal set; } = _ => { };

    /// <summary>
    /// Action to register the input element reference.
    /// </summary>
    public Action<ElementReference> RegisterInput { get; internal set; } = _ => { };

    /// <summary>
    /// Action to register the content element reference.
    /// </summary>
    public Action<ElementReference> RegisterContent { get; internal set; } = _ => { };

    /// <summary>
    /// Callback to notify state changes for re-rendering.
    /// </summary>
    public Action NotifyStateChanged { get; internal set; } = () => { };

    /// <summary>
    /// Event raised when the context state changes.
    /// Child components can subscribe to this to trigger re-renders.
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// Raises the OnStateChanged event to notify all subscribers.
    /// </summary>
    internal void RaiseStateChanged()
    {
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Registers an item with the context.
    /// </summary>
    /// <param name="key">The string key for JS interop.</param>
    /// <param name="value">The TValue to associate with this key.</param>
    /// <param name="label">The display label for this item.</param>
    /// <param name="disabled">Whether the item is disabled.</param>
    public void RegisterItem(string key, TValue value, string label, bool disabled = false)
    {
        ItemRegistry[key] = value;
        LabelRegistry[key] = label;
        DisabledRegistry[key] = disabled;
        ValueToLabelRegistry[value] = label;
    }

    /// <summary>
    /// Unregisters an item from the context.
    /// </summary>
    /// <param name="key">The string key to remove.</param>
    public void UnregisterItem(string key)
    {
        if (ItemRegistry.TryGetValue(key, out var value))
        {
            ValueToLabelRegistry.Remove(value);
        }
        ItemRegistry.Remove(key);
        LabelRegistry.Remove(key);
        DisabledRegistry.Remove(key);
    }

    /// <summary>
    /// Clears all registered items from the context.
    /// Called when the combobox opens to ensure fresh registration order.
    /// </summary>
    public void ClearItemRegistry()
    {
        ItemRegistry.Clear();
        LabelRegistry.Clear();
        DisabledRegistry.Clear();
        ValueToLabelRegistry.Clear();
    }

    /// <summary>
    /// Gets the string key for a value, if registered.
    /// </summary>
    /// <param name="value">The value to look up.</param>
    /// <returns>The key if found, null otherwise.</returns>
    public string? GetKeyForValue(TValue? value)
    {
        if (value is null) return null;

        foreach (var kvp in ItemRegistry)
        {
            if (EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
            {
                return kvp.Key;
            }
        }
        return null;
    }

    /// <summary>
    /// Checks if a value is currently selected.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is selected.</returns>
    public bool IsSelected(TValue value)
    {
        return SelectedValues.Contains(value);
    }

    /// <summary>
    /// Checks if an item (by key) passes the current filter.
    /// </summary>
    /// <param name="key">The item key.</param>
    /// <returns>True if the item matches the filter or filter is empty.</returns>
    public bool MatchesFilter(string key)
    {
        if (string.IsNullOrEmpty(FilterText)) return true;

        if (LabelRegistry.TryGetValue(key, out var label))
        {
            return label.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    /// <summary>
    /// Generates the trigger element ID for ARIA relationships.
    /// </summary>
    public string TriggerId => $"{ComboboxId}-trigger";

    /// <summary>
    /// Generates the input element ID for ARIA relationships.
    /// </summary>
    public string InputId => $"{ComboboxId}-input";

    /// <summary>
    /// Generates the content element ID for ARIA relationships.
    /// </summary>
    public string ContentId => $"{ComboboxId}-content";

    /// <summary>
    /// Generates a unique item ID for ARIA relationships.
    /// </summary>
    public string GetItemId(string key) => $"{ComboboxId}-item-{key}";

    /// <summary>
    /// Generates a unique group label ID for ARIA relationships.
    /// </summary>
    public string GetGroupLabelId(string groupId) => $"{ComboboxId}-group-label-{groupId}";

    /// <summary>
    /// Gets the portal ID for this combobox instance.
    /// </summary>
    public string PortalId => $"{ComboboxId}-portal";

    public ComboboxContext()
    {
        ComboboxId = $"summit-combobox-{Guid.NewGuid():N}";
    }
}
