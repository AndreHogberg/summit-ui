using Microsoft.AspNetCore.Components;

using SummitUI.Base;

namespace SummitUI;

/// <summary>
/// Cascading context shared between popover sub-components.
/// Provides state and callbacks for coordinating trigger, content, and other parts.
/// </summary>
public sealed class PopoverContext : OpenCloseContextBase
{
    /// <summary>
    /// Creates a new popover context with a unique ID.
    /// </summary>
    public PopoverContext() : base("popover")
    {
    }

    /// <summary>
    /// Unique identifier for this popover instance, used for ARIA relationships.
    /// </summary>
    public string PopoverId => ComponentId;

    /// <summary>
    /// Whether the popover is modal (traps focus when open).
    /// </summary>
    public bool Modal { get; internal set; }

    /// <summary>
    /// Reference to the trigger element (set by PopoverTrigger).
    /// </summary>
    public ElementReference TriggerElement { get; internal set; }

    /// <summary>
    /// Reference to the content element (set by PopoverContent).
    /// </summary>
    public ElementReference ContentElement { get; internal set; }

    /// <summary>
    /// Action to register the trigger element reference.
    /// </summary>
    public Action<ElementReference> RegisterTrigger { get; internal set; } = _ => { };

    /// <summary>
    /// Action to register the content element reference.
    /// </summary>
    public Action<ElementReference> RegisterContent { get; internal set; } = _ => { };
}
