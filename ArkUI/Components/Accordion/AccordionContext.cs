namespace ArkUI.Components.Accordion;

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

    public AccordionContext()
    {
        AccordionId = $"ark-accordion-{Guid.NewGuid():N}";
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
}
