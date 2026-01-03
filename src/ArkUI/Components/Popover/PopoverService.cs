namespace ArkUI;

/// <summary>
/// Service that coordinates multiple popover instances to ensure only one is open at a time.
/// This is registered as a scoped service and shared across all popover instances.
/// </summary>
public sealed class PopoverService
{
    private readonly List<PopoverContext> _openPopovers = [];

    /// <summary>
    /// Registers a popover as open and closes any other open popovers.
    /// </summary>
    /// <param name="context">The popover context being opened.</param>
    public async Task RegisterOpenAsync(PopoverContext context)
    {
        // Close all other open popovers before registering this one
        var popoversToClose = _openPopovers.Where(p => p != context).ToList();
        
        foreach (var popover in popoversToClose)
        {
            await popover.CloseAsync();
        }

        // Clear and add the new popover
        _openPopovers.Clear();
        _openPopovers.Add(context);
    }

    /// <summary>
    /// Unregisters a popover when it closes.
    /// </summary>
    /// <param name="context">The popover context being closed.</param>
    public void Unregister(PopoverContext context)
    {
        _openPopovers.Remove(context);
    }

    /// <summary>
    /// Closes all open popovers.
    /// </summary>
    public async Task CloseAllAsync()
    {
        var popoversToClose = _openPopovers.ToList();
        
        foreach (var popover in popoversToClose)
        {
            await popover.CloseAsync();
        }

        _openPopovers.Clear();
    }
}
