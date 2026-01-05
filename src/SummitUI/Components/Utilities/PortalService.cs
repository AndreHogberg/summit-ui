using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Default implementation of <see cref="IPortalService"/>.
/// Manages a collection of portal content keyed by ID, maintaining registration order.
/// </summary>
public class PortalService : IPortalService
{
    // Use a list to maintain registration order (important for z-index stacking)
    private readonly List<(string Id, RenderFragment Content)> _portals = [];

    /// <inheritdoc />
    public event Action? OnChange;

    /// <inheritdoc />
    public void Register(string portalId, RenderFragment content)
    {
        // Remove existing portal with same ID if present
        var existingIndex = _portals.FindIndex(p => p.Id == portalId);
        if (existingIndex >= 0)
        {
            _portals[existingIndex] = (portalId, content);
        }
        else
        {
            _portals.Add((portalId, content));
        }

        OnChange?.Invoke();
    }

    /// <inheritdoc />
    public void Unregister(string portalId)
    {
        var index = _portals.FindIndex(p => p.Id == portalId);
        if (index >= 0)
        {
            _portals.RemoveAt(index);
            OnChange?.Invoke();
        }
    }

    /// <inheritdoc />
    public IEnumerable<(string Id, RenderFragment Content)> GetPortals() => _portals;

    /// <inheritdoc />
    public bool HasPortal(string portalId) => _portals.Exists(p => p.Id == portalId);
}
