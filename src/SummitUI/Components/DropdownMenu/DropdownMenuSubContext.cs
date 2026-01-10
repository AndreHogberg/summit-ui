using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Cascading context shared between submenu sub-components.
/// Provides state and callbacks for coordinating sub trigger, sub content, and items.
/// </summary>
public sealed class DropdownMenuSubContext
{
    /// <summary>
    /// Unique identifier for this submenu instance, used for ARIA relationships.
    /// </summary>
    public string SubMenuId { get; }

    /// <summary>
    /// The ID of the sub trigger element. Set by DropdownMenuSubTrigger.
    /// Used for aria-labelledby on the SubContent.
    /// </summary>
    public string? TriggerId { get; internal set; }

    /// <summary>
    /// Current open state of the submenu.
    /// </summary>
    public bool IsOpen { get; internal set; }

    /// <summary>
    /// True while close animation is in progress. Content should stay rendered.
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
    /// Reference to the parent menu context.
    /// </summary>
    public DropdownMenuContext ParentMenuContext { get; }

    /// <summary>
    /// Reference to the parent submenu context (for nested submenus).
    /// Null if this is a direct child of the root menu.
    /// </summary>
    public DropdownMenuSubContext? ParentSubContext { get; internal set; }

    /// <summary>
    /// The nesting depth of this submenu. 1 = direct child of root menu.
    /// </summary>
    public int Depth { get; internal set; } = 1;

    /// <summary>
    /// Reading direction inherited from root. Used for keyboard navigation.
    /// </summary>
    public string Dir { get; internal set; } = "ltr";

    /// <summary>
    /// Reference to the sub trigger element (set by DropdownMenuSubTrigger).
    /// </summary>
    public ElementReference TriggerElement { get; internal set; }

    /// <summary>
    /// Reference to the sub content element (set by DropdownMenuSubContent).
    /// </summary>
    public ElementReference ContentElement { get; internal set; }

    /// <summary>
    /// Callback to explicitly open the submenu.
    /// </summary>
    public Func<Task> OpenAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to explicitly close the submenu.
    /// </summary>
    public Func<Task> CloseAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to close this submenu and all nested submenus.
    /// </summary>
    public Func<Task> CloseAllAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to set the highlighted item.
    /// </summary>
    public Func<string?, Task> SetHighlightedItemAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Action to register the sub trigger element reference.
    /// </summary>
    public Action<ElementReference> RegisterTrigger { get; internal set; } = _ => { };

    /// <summary>
    /// Action to register the sub content element reference.
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
    /// Callback to focus the sub trigger element. Set by the content component.
    /// </summary>
    public Func<Task> FocusTriggerAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback for keyboard navigation from items.
    /// </summary>
    public Func<string, Task> HandleKeyDownAsync { get; internal set; } = _ => Task.CompletedTask;

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
    public string GetItemId(string value) => $"{SubMenuId}-item-{value}";

    /// <summary>
    /// Currently open nested submenu context at this level.
    /// </summary>
    public DropdownMenuSubContext? ActiveNestedSubContext { get; internal set; }

    /// <summary>
    /// Callback to close any sibling submenus when this one opens.
    /// </summary>
    public Func<DropdownMenuSubContext, Task> NotifySubMenuOpeningAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to cancel the pending close timer on this submenu's trigger.
    /// Used when pointer enters the submenu content to prevent the submenu from closing.
    /// </summary>
    public Action CancelPendingClose { get; internal set; } = () => { };

    public DropdownMenuSubContext(DropdownMenuContext parentMenuContext)
    {
        ParentMenuContext = parentMenuContext ?? throw new ArgumentNullException(nameof(parentMenuContext));
        SubMenuId = $"summit-dropdown-submenu-{Guid.NewGuid():N}";
    }
}
