using Microsoft.AspNetCore.Components;

namespace SummitUI.Docs.Design;

/// <summary>
/// A heading component (h1-h6) with consistent styling.
/// </summary>
public partial class SuHeading : ComponentBase
{
    /// <summary>
    /// The heading level (h1-h6).
    /// </summary>
    [Parameter] public SuHeadingLevel Level { get; set; } = SuHeadingLevel.H2;

    /// <summary>
    /// Whether to render the heading as a different element (for visual styling without semantic meaning).
    /// </summary>
    [Parameter] public SuHeadingLevel? AsLevel { get; set; }

    /// <summary>
    /// The content to render inside the heading.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the heading element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string FinalClass => SuStyles.Cn("font-bold tracking-tight text-su-foreground", SizeClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);

    private SuHeadingLevel VisualLevel => AsLevel ?? Level;

    private string SizeClasses => VisualLevel switch
    {
        SuHeadingLevel.H1 => "text-4xl lg:text-5xl",
        SuHeadingLevel.H2 => "text-2xl lg:text-3xl",
        SuHeadingLevel.H3 => "text-xl lg:text-2xl",
        SuHeadingLevel.H4 => "text-lg lg:text-xl",
        SuHeadingLevel.H5 => "text-base lg:text-lg",
        SuHeadingLevel.H6 => "text-sm lg:text-base",
        _ => "text-2xl"
    };
}
