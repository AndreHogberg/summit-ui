namespace SummitUI.Docs.Design;

/// <summary>
/// Represents a single property/parameter in an API table.
/// </summary>
public record SuApiProperty
{
    /// <summary>
    /// The name of the property/parameter.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The type of the property (e.g., "string", "bool", "RenderFragment").
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// The default value of the property, if any.
    /// </summary>
    public string? Default { get; init; }

    /// <summary>
    /// A description of what the property does.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Whether this property is required.
    /// </summary>
    public bool Required { get; init; }
}
