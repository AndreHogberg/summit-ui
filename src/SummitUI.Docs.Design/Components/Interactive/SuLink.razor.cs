using Microsoft.AspNetCore.Components;

using SummitUI.Docs.Design.Utilities;

namespace SummitUI.Docs.Design;

/// <summary>
/// A styled link component.
/// </summary>
public partial class SuLink : ComponentBase
{
    /// <summary>
    /// The URL the link points to.
    /// </summary>
    [Parameter] public string? Href { get; set; }

    /// <summary>
    /// Whether this is an external link (opens in new tab).
    /// </summary>
    [Parameter] public bool External { get; set; }

    /// <summary>
    /// Whether to use muted styling.
    /// </summary>
    [Parameter] public bool Muted { get; set; }

    /// <summary>
    /// The content to render inside the link.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the anchor element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string FinalClass => SuStyles.Cn(BaseClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);

    private string BaseClasses => Muted
        ? "text-su-muted-foreground hover:text-su-foreground underline-offset-4 hover:underline transition-colors"
        : "text-su-primary hover:text-su-primary/80 underline-offset-4 hover:underline transition-colors";

    private string? Target => External ? "_blank" : null;

    private string? Rel => External ? "noopener noreferrer" : null;
}
