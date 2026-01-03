namespace ArkUI;

/// <summary>
/// Cascading context for an individual accordion item.
/// Provides item-specific state to AccordionTrigger and AccordionContent.
/// </summary>
public sealed class AccordionItemContext
{
    /// <summary>
    /// Unique value identifying this accordion item.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Whether this specific item is disabled.
    /// </summary>
    public bool Disabled { get; internal set; }

    public AccordionItemContext(string value)
    {
        Value = value;
    }
}
