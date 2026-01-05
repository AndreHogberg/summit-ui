using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Cascading context shared between popover sub-components.
/// Provides state and callbacks for coordinating trigger, content, and other parts.
/// </summary>
public sealed class PopoverContext
{
    /// <summary>
    /// Unique identifier for this popover instance, used for ARIA relationships.
    /// </summary>
    public string PopoverId { get; }

    /// <summary>
    /// Current open state of the popover.
    /// </summary>
    public bool IsOpen { get; internal set; }

    /// <summary>
    /// True while close animation is in progress. Portal should stay rendered.
    /// </summary>
    public bool IsAnimatingClosed { get; set; }

    /// <summary>
    /// Reference to the trigger element (set by PopoverTrigger).
    /// </summary>
    public ElementReference TriggerElement { get; internal set; }

    /// <summary>
    /// Reference to the content element (set by PopoverContent).
    /// </summary>
    public ElementReference ContentElement { get; internal set; }

    /// <summary>
    /// Callback to toggle the popover state.
    /// </summary>
    public Func<Task> ToggleAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to explicitly open the popover.
    /// </summary>
    public Func<Task> OpenAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to explicitly close the popover.
    /// </summary>
    public Func<Task> CloseAsync { get; internal set; } = () => Task.CompletedTask;

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

    public PopoverContext()
    {
        PopoverId = $"summit-popover-{Guid.NewGuid():N}";
    }
}
