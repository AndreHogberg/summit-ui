using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Context passed to child content when AsChild is enabled.
/// Contains merged attributes that must be applied to the child element.
/// </summary>
public sealed class AsChildContext
{
    /// <summary>
    /// Dictionary of attributes to apply to the child element via @attributes directive.
    /// Includes ARIA attributes, event handlers, data attributes, and any additional attributes.
    /// </summary>
    public required IReadOnlyDictionary<string, object> Attrs { get; init; }

    /// <summary>
    /// Callback to capture the element reference for focus management.
    /// Apply via @ref="context.RefCallback" on the child element.
    /// </summary>
    public Action<ElementReference>? RefCallback { get; init; }
}
