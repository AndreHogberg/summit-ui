using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.DropdownMenu;

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
