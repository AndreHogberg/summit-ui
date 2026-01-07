using Microsoft.AspNetCore.Components;

using SummitUI.Docs.Design.Utilities;

namespace SummitUI.Docs.Design;

/// <summary>
/// A card container component with multiple style variants.
/// </summary>
public partial class SuCard : ComponentBase
{
    /// <summary>
    /// The visual style variant of the card.
    /// </summary>
    [Parameter] public SuCardVariant Variant { get; set; } = SuCardVariant.Default;

    /// <summary>
    /// Optional padding override. If not specified, uses default padding.
    /// </summary>
    [Parameter] public bool NoPadding { get; set; }

    /// <summary>
    /// The content to render inside the card.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the card element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private const string BaseClasses = "rounded-lg bg-su-card text-su-card-foreground";

    private string FinalClass => SuStyles.Cn(BaseClasses, VariantClasses, PaddingClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);

    private string PaddingClasses => NoPadding ? "" : "p-6";

    private string VariantClasses => Variant switch
    {
        SuCardVariant.Default => "border border-su-border",
        SuCardVariant.Elevated => "shadow-md",
        SuCardVariant.Ghost => "",
        SuCardVariant.Interactive => "border border-su-border hover:border-su-primary/50 hover:shadow-md transition-all cursor-pointer",
        _ => ""
    };
}
