namespace SummitUI.Base;

/// <summary>
/// Base class for component contexts that manage open/close state with animation support.
/// Provides common state management, event notifications, and element ID generation.
/// </summary>
public abstract class OpenCloseContextBase : IPortalContext
{
    /// <summary>
    /// Unique identifier for this component instance, used for ARIA relationships and element IDs.
    /// </summary>
    public string ComponentId { get; }

    /// <summary>
    /// Whether the component is currently open.
    /// </summary>
    public bool IsOpen { get; internal set; }

    /// <summary>
    /// True while close animation is in progress. Content should stay rendered during this time.
    /// </summary>
    public bool IsAnimatingClosed { get; set; }

    /// <summary>
    /// Callback to toggle the component open/closed.
    /// </summary>
    public Func<Task> ToggleAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to explicitly open the component.
    /// </summary>
    public Func<Task> OpenAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to explicitly close the component.
    /// </summary>
    public Func<Task> CloseAsync { get; internal set; } = () => Task.CompletedTask;

    /// <summary>
    /// Callback to notify state changes for re-rendering in the root component.
    /// </summary>
    public Action NotifyStateChanged { get; internal set; } = () => { };

    /// <summary>
    /// Event raised when the context state changes.
    /// Child components can subscribe to this to trigger re-renders.
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// Creates a new context with a unique ID.
    /// </summary>
    /// <param name="componentPrefix">The prefix for the component ID (e.g., "dialog", "popover").</param>
    protected OpenCloseContextBase(string componentPrefix)
    {
        ComponentId = $"summit-{componentPrefix}-{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Raises the OnStateChanged event to notify all subscribers.
    /// </summary>
    internal void RaiseStateChanged() => OnStateChanged?.Invoke();

    /// <summary>
    /// Generates an element ID with the specified suffix.
    /// </summary>
    /// <param name="suffix">The suffix to append (e.g., "trigger", "content", "title").</param>
    /// <returns>A unique element ID in the format "{ComponentId}-{suffix}".</returns>
    public string GetElementId(string suffix) => $"{ComponentId}-{suffix}";

    /// <summary>
    /// Gets the portal ID for this component instance.
    /// </summary>
    public virtual string PortalId => GetElementId("portal");
}
