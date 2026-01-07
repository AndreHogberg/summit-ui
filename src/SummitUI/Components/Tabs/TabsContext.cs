using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Represents a registered tab trigger with its metadata for navigation.
/// </summary>
internal sealed class TabTriggerInfo
{
    public required string Value { get; init; }
    public required ElementReference ElementRef { get; init; }
    public required bool Disabled { get; init; }
}

/// <summary>
/// Cascading context shared between tabs sub-components.
/// Provides state and callbacks for coordinating TabsList, TabsTrigger, and TabsContent.
/// </summary>
public sealed class TabsContext
{
    private readonly List<TabTriggerInfo> _triggers = new();

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
    /// Callback to focus a trigger element by its ID.
    /// Set by TabsList to provide focus functionality.
    /// </summary>
    internal Func<string, ValueTask>? FocusTriggerByIdAsync { get; set; }

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
        TabsId = $"summit-tabs-{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Registers a trigger with the context for keyboard navigation.
    /// Called by TabsTrigger on initialization.
    /// </summary>
    internal void RegisterTrigger(string value, ElementReference elementRef, bool disabled)
    {
        // Update existing or add new
        var existing = _triggers.FindIndex(t => t.Value == value);
        if (existing >= 0)
        {
            _triggers[existing] = new TabTriggerInfo
            {
                Value = value,
                ElementRef = elementRef,
                Disabled = disabled
            };
        }
        else
        {
            _triggers.Add(new TabTriggerInfo
            {
                Value = value,
                ElementRef = elementRef,
                Disabled = disabled
            });
        }
    }

    /// <summary>
    /// Unregisters a trigger from the context.
    /// Called by TabsTrigger on disposal.
    /// </summary>
    internal void UnregisterTrigger(string value)
    {
        _triggers.RemoveAll(t => t.Value == value);
    }

    /// <summary>
    /// Updates the disabled state of a registered trigger.
    /// </summary>
    internal void UpdateTriggerDisabled(string value, bool disabled)
    {
        var index = _triggers.FindIndex(t => t.Value == value);
        if (index >= 0)
        {
            _triggers[index] = new TabTriggerInfo
            {
                Value = _triggers[index].Value,
                ElementRef = _triggers[index].ElementRef,
                Disabled = disabled
            };
        }
    }

    /// <summary>
    /// Gets the list of enabled triggers in registration order.
    /// </summary>
    internal IReadOnlyList<TabTriggerInfo> GetEnabledTriggers()
    {
        return _triggers.Where(t => !t.Disabled).ToList();
    }

    /// <summary>
    /// Gets the current trigger index within enabled triggers.
    /// </summary>
    internal int GetCurrentTriggerIndex(string currentValue)
    {
        var enabled = GetEnabledTriggers();
        return enabled.ToList().FindIndex(t => t.Value == currentValue);
    }

    /// <summary>
    /// Calculates the next trigger index based on direction and loop settings.
    /// </summary>
    internal int GetNextTriggerIndex(int currentIndex, int direction, bool isRtl)
    {
        var enabled = GetEnabledTriggers();
        if (enabled.Count == 0) return -1;

        // For horizontal orientation, RTL reverses the direction
        var effectiveDirection = direction;
        if (Orientation == TabsOrientation.Horizontal && isRtl)
        {
            effectiveDirection = -direction;
        }

        var newIndex = currentIndex + effectiveDirection;

        if (Loop)
        {
            if (newIndex < 0) return enabled.Count - 1;
            if (newIndex >= enabled.Count) return 0;
        }
        else
        {
            if (newIndex < 0) return 0;
            if (newIndex >= enabled.Count) return enabled.Count - 1;
        }

        return newIndex;
    }

    /// <summary>
    /// Gets the trigger at a specific index within enabled triggers.
    /// </summary>
    internal TabTriggerInfo? GetTriggerAtIndex(int index)
    {
        var enabled = GetEnabledTriggers();
        if (index < 0 || index >= enabled.Count) return null;
        return enabled[index];
    }

    /// <summary>
    /// Gets the first enabled trigger.
    /// </summary>
    internal TabTriggerInfo? GetFirstTrigger()
    {
        var enabled = GetEnabledTriggers();
        return enabled.FirstOrDefault();
    }

    /// <summary>
    /// Gets the last enabled trigger.
    /// </summary>
    internal TabTriggerInfo? GetLastTrigger()
    {
        var enabled = GetEnabledTriggers();
        return enabled.LastOrDefault();
    }
}
