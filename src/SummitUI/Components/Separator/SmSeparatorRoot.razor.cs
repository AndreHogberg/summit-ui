using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// A separator visually or semantically separates content.
/// </summary>
public partial class SmSeparatorRoot
{
    /// <summary>
    /// The orientation of the separator. Defaults to horizontal.
    /// </summary>
    [Parameter]
    public SeparatorOrientation Orientation { get; set; } = SeparatorOrientation.Horizontal;

    /// <summary>
    /// When true, the separator is purely visual and has no semantic meaning.
    /// This removes the separator role and aria attributes.
    /// </summary>
    [Parameter]
    public bool Decorative { get; set; }

    /// <summary>
    /// Additional attributes to apply to the element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
