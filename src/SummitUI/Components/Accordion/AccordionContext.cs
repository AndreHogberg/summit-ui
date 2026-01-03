namespace SummitUI;

/// <summary>
/// Cascading context shared between accordion sub-components.
/// Provides state and callbacks for coordinating AccordionItem, AccordionTrigger, and AccordionContent.
/// </summary>
public sealed class AccordionContext
{
    /// <summary>
    /// Unique identifier for this accordion instance, used for ARIA relationships.
    /// </summary>
    public string AccordionId { get; }

    /// <summary>
    /// Type of accordion (single or multiple expansion).
    /// </summary>
    public AccordionType Type { get; internal set; } = AccordionType.Single;

    /// <summary>
    /// Set of currently expanded item values.
    /// </summary>
    public HashSet<string> ExpandedValues { get; internal set; } = [];

    /// <summary>
    /// Orientation of the accordion (affects keyboard navigation).
    /// </summary>
    public AccordionOrientation Orientation { get; internal set; } = AccordionOrientation.Vertical;

    /// <summary>
    /// Whether keyboard navigation loops from last to first and vice versa.
    /// </summary>
    public bool Loop { get; internal set; } = true;

    /// <summary>
    /// Whether the entire accordion is disabled.
    /// </summary>
    public bool Disabled { get; internal set; }

    /// <summary>
    /// Whether items can be collapsed when in single mode (closing the last open item).
    /// </summary>
    public bool Collapsible { get; internal set; } = true;

    /// <summary>
    /// Callback to toggle an item's expanded state.
    /// </summary>
    public Func<string, Task> ToggleItemAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Callback to notify state changes for re-rendering.
    /// </summary>
    public Action NotifyStateChanged { get; internal set; } = () => { };

    /// <summary>
    /// Callback to focus a trigger by its value. Set by the AccordionRoot.
    /// </summary>
    public Func<string, Task> FocusTriggerAsync { get; internal set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Ordered list of registered trigger values (in DOM order).
    /// Triggers register themselves when they initialize.
    /// </summary>
    private readonly List<TriggerRegistration> _triggers = [];

    public AccordionContext()
    {
        AccordionId = $"summit-accordion-{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Checks if the specified item is expanded.
    /// </summary>
    public bool IsExpanded(string value) => ExpandedValues.Contains(value);

    /// <summary>
    /// Generates a unique trigger ID for ARIA relationships.
    /// </summary>
    public string GetTriggerId(string value) => $"{AccordionId}-trigger-{value}";

    /// <summary>
    /// Generates a unique content panel ID for ARIA relationships.
    /// </summary>
    public string GetContentId(string value) => $"{AccordionId}-content-{value}";

    /// <summary>
    /// Registers a trigger with the context. Called by AccordionTrigger on initialization.
    /// </summary>
    /// <param name="value">The value identifying the accordion item.</param>
    /// <param name="isDisabled">Whether the trigger is disabled.</param>
    public void RegisterTrigger(string value, bool isDisabled)
    {
        // Avoid duplicates
        if (_triggers.Any(t => t.Value == value))
            return;

        _triggers.Add(new TriggerRegistration(value, isDisabled));
    }

    /// <summary>
    /// Unregisters a trigger from the context. Called by AccordionTrigger on disposal.
    /// </summary>
    /// <param name="value">The value identifying the accordion item.</param>
    public void UnregisterTrigger(string value)
    {
        _triggers.RemoveAll(t => t.Value == value);
    }

    /// <summary>
    /// Updates the disabled state for a registered trigger.
    /// </summary>
    /// <param name="value">The value identifying the accordion item.</param>
    /// <param name="isDisabled">Whether the trigger is disabled.</param>
    public void UpdateTriggerDisabled(string value, bool isDisabled)
    {
        var trigger = _triggers.FirstOrDefault(t => t.Value == value);
        if (trigger is not null)
        {
            trigger.IsDisabled = isDisabled;
        }
    }

    /// <summary>
    /// Gets the list of non-disabled trigger values in registration order.
    /// </summary>
    public IReadOnlyList<string> GetEnabledTriggerValues()
    {
        return _triggers.Where(t => !t.IsDisabled).Select(t => t.Value).ToList();
    }

    /// <summary>
    /// Gets the next trigger value to navigate to based on direction.
    /// </summary>
    /// <param name="currentValue">The current trigger's value.</param>
    /// <param name="direction">Navigation direction: -1 for previous, 1 for next.</param>
    /// <returns>The value of the trigger to focus, or null if navigation is not possible.</returns>
    public string? GetNavigationTarget(string currentValue, int direction)
    {
        var enabledTriggers = GetEnabledTriggerValues();
        if (enabledTriggers.Count == 0)
            return null;

        var currentIndex = enabledTriggers.ToList().IndexOf(currentValue);
        if (currentIndex == -1)
            return null;

        var newIndex = currentIndex + direction;

        if (newIndex < 0)
        {
            newIndex = Loop ? enabledTriggers.Count - 1 : 0;
        }
        else if (newIndex >= enabledTriggers.Count)
        {
            newIndex = Loop ? 0 : enabledTriggers.Count - 1;
        }

        return newIndex != currentIndex ? enabledTriggers[newIndex] : null;
    }

    /// <summary>
    /// Gets the first enabled trigger value.
    /// </summary>
    public string? GetFirstTriggerValue()
    {
        var enabledTriggers = GetEnabledTriggerValues();
        return enabledTriggers.Count > 0 ? enabledTriggers[0] : null;
    }

    /// <summary>
    /// Gets the last enabled trigger value.
    /// </summary>
    public string? GetLastTriggerValue()
    {
        var enabledTriggers = GetEnabledTriggerValues();
        return enabledTriggers.Count > 0 ? enabledTriggers[^1] : null;
    }

    /// <summary>
    /// Internal class to track trigger registrations.
    /// </summary>
    private sealed class TriggerRegistration(string value, bool isDisabled)
    {
        public string Value { get; } = value;
        public bool IsDisabled { get; set; } = isDisabled;
    }
}
