using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Service interface for managing portal content rendering.
/// Portals allow components to render their content at the root of the DOM tree,
/// outside their parent component hierarchy. This is essential for:
/// - Dialogs and modals that need to escape overflow:hidden containers
/// - Nested dialogs that need to render as siblings, not parent-child
/// - Tooltips/popovers that need to escape stacking contexts
/// </summary>
public interface IPortalService
{
    /// <summary>
    /// Event raised when the portal content changes and needs re-rendering.
    /// </summary>
    event Action? OnChange;

    /// <summary>
    /// Registers a portal with the given ID and content.
    /// If a portal with the same ID exists, it will be replaced.
    /// </summary>
    /// <param name="portalId">Unique identifier for the portal.</param>
    /// <param name="content">The render fragment to display in the portal.</param>
    void Register(string portalId, RenderFragment content);

    /// <summary>
    /// Unregisters a portal by its ID.
    /// </summary>
    /// <param name="portalId">The portal ID to remove.</param>
    void Unregister(string portalId);

    /// <summary>
    /// Gets all currently registered portals in registration order.
    /// </summary>
    /// <returns>Collection of portal ID and content pairs.</returns>
    IEnumerable<(string Id, RenderFragment Content)> GetPortals();

    /// <summary>
    /// Checks if a portal with the given ID is registered.
    /// </summary>
    /// <param name="portalId">The portal ID to check.</param>
    /// <returns>True if the portal exists, false otherwise.</returns>
    bool HasPortal(string portalId);
}
