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
    /// Reading direction. Used for submenu arrow key navigation.
    /// "ltr" = ArrowRight opens submenu, ArrowLeft closes.
    /// "rtl" = ArrowLeft opens submenu, ArrowRight closes.
    /// </summary>
    public string Dir { get; internal set; } = "ltr";

    /// <summary>
    /// Current open state of the dropdown menu.
    /// </summary>
    public bool IsOpen { get; internal set; }

    /// <summary>
    /// True while close animation is in progress. Portal should stay rendered.
    /// </summary>
    public bool IsAnimatingClosed { get; set; }

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
    /// Currently open submenu context at this level.
    /// </summary>
    public DropdownMenuSubContext? ActiveSubContext { get; internal set; }

    /// <summary>
    /// Callback to notify when a submenu is opening (to close siblings).
    /// </summary>
    public Func<DropdownMenuSubContext, Task> NotifySubMenuOpeningAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to close all open submenus.
    /// </summary>
    public Func<Task> CloseAllSubMenusAsync { get; internal set; } = () => Task.CompletedTask;

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

    /// <summary>
    /// Registry of all submenu contexts, keyed by SubMenuId.
    /// Used to reliably establish parent-child relationships for nested submenus.
    /// </summary>
    private readonly Dictionary<string, DropdownMenuSubContext> _subContextRegistry = new();

    /// <summary>
    /// Registers a submenu context with this menu, establishing the parent-child relationship.
    /// </summary>
    /// <param name="context">The submenu context to register.</param>
    /// <param name="parentSubMenuId">The SubMenuId of the parent submenu, or null if direct child of root.</param>
    public void RegisterSubContext(DropdownMenuSubContext context, string? parentSubMenuId)
    {
        _subContextRegistry[context.SubMenuId] = context;

        // Set up parent relationship from the registry
        if (parentSubMenuId != null && _subContextRegistry.TryGetValue(parentSubMenuId, out var parent))
        {
            context.ParentSubContext = parent;
            context.Depth = parent.Depth + 1;
        }
        else
        {
            context.ParentSubContext = null;
            context.Depth = 1;
        }
    }

    /// <summary>
    /// Unregisters a submenu context from this menu.
    /// </summary>
    /// <param name="subMenuId">The SubMenuId to unregister.</param>
    public void UnregisterSubContext(string subMenuId)
    {
        _subContextRegistry.Remove(subMenuId);
    }

    /// <summary>
    /// Gets a submenu context by its ID.
    /// </summary>
    public DropdownMenuSubContext? GetSubContext(string subMenuId)
    {
        _subContextRegistry.TryGetValue(subMenuId, out var context);
        return context;
    }

    public DropdownMenuContext()
    {
        MenuId = $"summit-dropdown-menu-{Guid.NewGuid():N}";
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
    public string GroupId { get; } = $"summit-dropdown-radio-{Guid.NewGuid():N}";
}
