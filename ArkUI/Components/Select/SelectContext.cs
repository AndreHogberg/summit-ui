using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Select;

/// <summary>
/// Cascading context shared between select sub-components.
/// Provides state and callbacks for coordinating trigger, content, items, and other parts.
/// </summary>
/// <typeparam name="TValue">The type of the select value.</typeparam>
public sealed class SelectContext<TValue> where TValue : notnull
{
    /// <summary>
    /// Unique identifier for this select instance, used for ARIA relationships.
    /// </summary>
    public string SelectId { get; }

    /// <summary>
    /// Currently selected value.
    /// </summary>
    public TValue? Value { get; internal set; }

    /// <summary>
    /// Currently highlighted key (for keyboard navigation).
    /// This is the string key, not the TValue, for JS interop compatibility.
    /// </summary>
    public string? HighlightedKey { get; internal set; }

    /// <summary>
    /// Label of the currently selected item (for display in trigger).
    /// </summary>
    public string? SelectedLabel { get; internal set; }

    /// <summary>
    /// Registry mapping string keys to TValue items.
    /// Used to look up the actual value when an item is selected via JS interop.
    /// </summary>
    public Dictionary<string, TValue> ItemRegistry { get; } = new();

    /// <summary>
    /// Registry mapping string keys to labels.
    /// Used to look up the label when an item is selected via JS interop.
    /// </summary>
    public Dictionary<string, string> LabelRegistry { get; } = new();

    private bool _isOpen;
    
    /// <summary>
    /// Current open state of the select.
    /// </summary>
    public bool IsOpen 
    { 
        get => _isOpen;
        internal set => _isOpen = value;
    }

    /// <summary>
    /// Whether the select is disabled.
    /// </summary>
    public bool Disabled { get; internal set; }

    /// <summary>
    /// Whether the select is required.
    /// </summary>
    public bool Required { get; internal set; }

    /// <summary>
    /// Whether the select is in an invalid state.
    /// </summary>
    public bool Invalid { get; internal set; }

    /// <summary>
    /// Reference to the trigger element (set by SelectTrigger).
    /// </summary>
    public ElementReference TriggerElement { get; internal set; }

    /// <summary>
    /// Reference to the content element (set by SelectContent).
    /// </summary>
    public ElementReference ContentElement { get; internal set; }

    /// <summary>
    /// Callback to toggle the select open/closed.
    /// </summary>
    public Func<Task> ToggleAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to explicitly open the select.
    /// </summary>
    public Func<Task> OpenAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to explicitly close the select.
    /// </summary>
    public Func<Task> CloseAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to select an item by its string key.
    /// The context will look up the TValue from the registry.
    /// </summary>
    public Func<string, Task> SelectItemByKeyAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to select an item directly by value.
    /// </summary>
    public Func<TValue, string?, Task> SelectItemAsync { get; internal set; } = (_, _) => Task.CompletedTask;

    /// <summary>
    /// Callback to update the highlighted key.
    /// </summary>
    public Func<string?, Task> SetHighlightedKeyAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Action to register the trigger element reference.
    /// </summary>
    public Action<ElementReference> RegisterTrigger { get; internal set; } = _ => { };

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
    public void RegisterItem(string key, TValue value, string label)
    {
        ItemRegistry[key] = value;
        LabelRegistry[key] = label;
    }

    /// <summary>
    /// Unregisters an item from the context.
    /// </summary>
    /// <param name="key">The string key to remove.</param>
    public void UnregisterItem(string key)
    {
        ItemRegistry.Remove(key);
        LabelRegistry.Remove(key);
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
    /// Generates the trigger element ID for ARIA relationships.
    /// </summary>
    public string TriggerId => $"{SelectId}-trigger";

    /// <summary>
    /// Generates the content element ID for ARIA relationships.
    /// </summary>
    public string ContentId => $"{SelectId}-content";

    /// <summary>
    /// Generates a unique item ID for ARIA relationships.
    /// </summary>
    public string GetItemId(string key) => $"{SelectId}-item-{key}";

    /// <summary>
    /// Generates a unique group label ID for ARIA relationships.
    /// </summary>
    public string GetGroupLabelId(string groupId) => $"{SelectId}-group-label-{groupId}";

    public SelectContext()
    {
        SelectId = $"ark-select-{Guid.NewGuid():N}";
    }
}
