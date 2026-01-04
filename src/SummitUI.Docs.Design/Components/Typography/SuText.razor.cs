using Microsoft.AspNetCore.Components;

namespace SummitUI.Docs.Design;

/// <summary>
/// A text component with size and style variants.
/// </summary>
public partial class SuText : ComponentBase
{
    /// <summary>
    /// The text size.
    /// </summary>
    [Parameter] public SuTextSize Size { get; set; } = SuTextSize.Base;

    /// <summary>
    /// Whether to use muted (secondary) text color.
    /// </summary>
    [Parameter] public bool Muted { get; set; }

    /// <summary>
    /// Whether this is lead/intro text (larger line height).
    /// </summary>
    [Parameter] public bool Lead { get; set; }

    /// <summary>
    /// The HTML element to render (p, span, div).
    /// </summary>
    [Parameter] public string Element { get; set; } = "p";

    /// <summary>
    /// The content to render inside the text element.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the text element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string FinalClass => SuStyles.Cn(SizeClasses, ColorClasses, LeadClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);

    private string ColorClasses => Muted ? "text-su-muted-foreground" : "text-su-foreground";

    private string LeadClasses => Lead ? "leading-relaxed" : "";

    private string SizeClasses => Size switch
    {
        SuTextSize.ExtraSmall => "text-xs",
        SuTextSize.Small => "text-sm",
        SuTextSize.Base => "text-base",
        SuTextSize.Large => "text-lg",
        SuTextSize.ExtraLarge => "text-xl",
        _ => "text-base"
    };
}
