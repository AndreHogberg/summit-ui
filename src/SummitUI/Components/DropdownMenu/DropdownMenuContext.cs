using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Cascading context shared between dropdown menu sub-components.
/// Provides state and callbacks for coordinating trigger, content, and items.
/// </summary>
public sealed class DropdownMenuContext
{
    /// <summary>
    /// Unique identifier for this dropdown menu instance, used for ARIA relationships.
    /// </summary>
    public string MenuId { get; }

    /// <summary>
    /// Current open state of the dropdown menu.
    /// </summary>
    public bool IsOpen { get; internal set; }

    /// <summary>
    /// The ID of the currently highlighted item (for keyboard navigation).
    /// </summary>
    public string? HighlightedItemId { get; internal set; }

    /// <summary>
    /// List of registered menu item IDs in DOM order for keyboard navigation.
    /// </summary>
    public List<string> RegisteredItems { get; } = new();

    /// <summary>
    /// Reference to the trigger element (set by DropdownMenuTrigger).
    /// </summary>
    public ElementReference TriggerElement { get; internal set; }

    /// <summary>
    /// Reference to the content element (set by DropdownMenuContent).
    /// </summary>
    public ElementReference ContentElement { get; internal set; }

    /// <summary>
    /// Callback to toggle the menu state.
    /// </summary>
    public Func<Task> ToggleAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to explicitly open the menu.
    /// </summary>
    public Func<Task> OpenAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to explicitly close the menu.
    /// </summary>
    public Func<Task> CloseAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to close the menu and invoke the item's action.
    /// </summary>
    public Func<Task> SelectItemAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to set the highlighted item.
    /// </summary>
    public Func<string?, Task> SetHighlightedItemAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Action to register the trigger element reference.
    /// </summary>
    public Action<ElementReference> RegisterTrigger { get; internal set; } = _ => { };

    /// <summary>
    /// Action to register the content element reference.
    /// </summary>
    public Action<ElementReference> RegisterContent { get; internal set; } = _ => { };

    /// <summary>
    /// Action to register a menu item for keyboard navigation.
    /// </summary>
    public Action<string> RegisterItem { get; internal set; } = _ => { };

    /// <summary>
    /// Action to unregister a menu item.
    /// </summary>
    public Action<string> UnregisterItem { get; internal set; } = _ => { };

    /// <summary>
    /// Action to register a menu item's label for typeahead.
    /// </summary>
    public Action<string, string> RegisterItemLabel { get; internal set; } = (_, _) => { };

    /// <summary>
    /// Action to unregister a menu item's label.
    /// </summary>
    public Action<string> UnregisterItemLabel { get; internal set; } = _ => { };

    /// <summary>
    /// Callback to focus the trigger element. Set by the content component.
    /// </summary>
    public Func<Task> FocusTriggerAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback for keyboard navigation from items.
    /// </summary>
    public Func<string, Task> HandleKeyDownAsync { get; internal set; } = _ => Task.CompletedTask;

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
    /// Gets the unique ID for a menu item based on its value.
    /// </summary>
    public string GetItemId(string value) => $"{MenuId}-item-{value}";

    public DropdownMenuContext()
    {
        MenuId = $"ark-dropdown-menu-{Guid.NewGuid():N}";
    }
}

/// <summary>
/// Context for radio group state management.
/// </summary>
public sealed class DropdownMenuRadioGroupContext
{
    /// <summary>
    /// The currently selected value in the radio group.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Callback when the value changes.
    /// </summary>
    public Func<string, Task> OnValueChangeAsync { get; set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Unique identifier for this radio group.
    /// </summary>
    public string GroupId { get; } = $"ark-dropdown-radio-{Guid.NewGuid():N}";
}
