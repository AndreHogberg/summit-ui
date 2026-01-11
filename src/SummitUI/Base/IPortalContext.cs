namespace SummitUI.Base;

/// <summary>
/// Interface for component contexts that support portal rendering.
/// Provides state information needed by portal components to determine when to render.
/// </summary>
public interface IPortalContext
{
    /// <summary>
    /// Whether the component is currently open.
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// True while close animation is in progress. Portal should stay rendered during this time.
    /// </summary>
    bool IsAnimatingClosed { get; }

    /// <summary>
    /// Event raised when the context state changes.
    /// Portal components subscribe to this to trigger re-renders.
    /// </summary>
    event Action? OnStateChanged;

    /// <summary>
    /// Gets the unique portal ID for this component instance.
    /// </summary>
    string PortalId { get; }
}
