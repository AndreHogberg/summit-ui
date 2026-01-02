using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Select;

/// <summary>
/// Cascading context shared between select sub-components.
/// Provides state and callbacks for coordinating trigger, content, items, and other parts.
/// </summary>
public sealed class SelectContext
{
    /// <summary>
    /// Unique identifier for this select instance, used for ARIA relationships.
    /// </summary>
    public string SelectId { get; }

    /// <summary>
    /// Currently selected value.
    /// </summary>
    public string? Value { get; internal set; }

    /// <summary>
    /// Currently highlighted value (for keyboard navigation).
    /// </summary>
    public string? HighlightedValue { get; internal set; }

    /// <summary>
    /// Label of the currently selected item (for display in trigger).
    /// </summary>
    public string? SelectedLabel { get; internal set; }

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
    /// Callback to select an item by value.
    /// </summary>
    public Func<string, string?, Task> SelectItemAsync { get; internal set; } = (_, _) => Task.CompletedTask;

    /// <summary>
    /// Callback to update the highlighted value.
    /// </summary>
    public Func<string?, Task> SetHighlightedValueAsync { get; internal set; } = _ => Task.CompletedTask;

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
    public string GetItemId(string value) => $"{SelectId}-item-{value}";

    /// <summary>
    /// Generates a unique group label ID for ARIA relationships.
    /// </summary>
    public string GetGroupLabelId(string groupId) => $"{SelectId}-group-label-{groupId}";

    public SelectContext()
    {
        SelectId = $"ark-select-{Guid.NewGuid():N}";
    }
}
