namespace ArkUI.Components.Tabs;

/// <summary>
/// Cascading context shared between tabs sub-components.
/// Provides state and callbacks for coordinating TabsList, TabsTrigger, and TabsContent.
/// </summary>
public sealed class TabsContext
{
    /// <summary>
    /// Unique identifier for this tabs instance, used for ARIA relationships.
    /// </summary>
    public string TabsId { get; }

    /// <summary>
    /// Currently active tab value.
    /// </summary>
    public string? Value { get; internal set; }

    /// <summary>
    /// Orientation of the tabs (horizontal or vertical).
    /// </summary>
    public TabsOrientation Orientation { get; internal set; } = TabsOrientation.Horizontal;

    /// <summary>
    /// Activation mode (auto activates on focus, manual requires explicit activation).
    /// </summary>
    public TabsActivationMode ActivationMode { get; internal set; } = TabsActivationMode.Auto;

    /// <summary>
    /// Whether keyboard navigation loops from last to first and vice versa.
    /// </summary>
    public bool Loop { get; internal set; } = true;

    /// <summary>
    /// Callback to activate a specific tab by value.
    /// </summary>
    public Func<string, Task> ActivateTabAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to notify state changes for re-rendering.
    /// </summary>
    public Action NotifyStateChanged { get; internal set; } = () => { };

    /// <summary>
    /// Generates a unique trigger ID for ARIA relationships.
    /// </summary>
    public string GetTriggerId(string value) => $"{TabsId}-trigger-{value}";

    /// <summary>
    /// Generates a unique content panel ID for ARIA relationships.
    /// </summary>
    public string GetContentId(string value) => $"{TabsId}-content-{value}";

    public TabsContext()
    {
        TabsId = $"ark-tabs-{Guid.NewGuid():N}";
    }
}
