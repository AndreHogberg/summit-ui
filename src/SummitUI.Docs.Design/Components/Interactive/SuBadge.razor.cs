using Microsoft.AspNetCore.Components;

using SummitUI.Docs.Design.Utilities;

namespace SummitUI.Docs.Design;

/// <summary>
/// A badge/tag component for displaying labels, statuses, or counts.
/// </summary>
public partial class SuBadge : ComponentBase
{
    /// <summary>
    /// The visual style variant of the badge.
    /// </summary>
    [Parameter] public SuBadgeVariant Variant { get; set; } = SuBadgeVariant.Default;

    /// <summary>
    /// The size of the badge.
    /// </summary>
    [Parameter] public SuSize Size { get; set; } = SuSize.Medium;

    /// <summary>
    /// The content to render inside the badge.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the badge element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private const string BaseClasses = "inline-flex items-center rounded-full font-medium transition-colors";

    private string FinalClass => SuStyles.Cn(BaseClasses, VariantClasses, SizeClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);

    private string VariantClasses => Variant switch
    {
        SuBadgeVariant.Default => "bg-su-muted text-su-foreground",
        SuBadgeVariant.Primary => "bg-su-primary text-su-primary-foreground",
        SuBadgeVariant.Secondary => "bg-su-accent text-su-accent-foreground",
        SuBadgeVariant.Outline => "border border-su-border bg-transparent text-su-foreground",
        SuBadgeVariant.Destructive => "bg-su-destructive text-su-destructive-foreground",
        SuBadgeVariant.Success => "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-100",
        SuBadgeVariant.Warning => "bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-100",
        _ => ""
    };

    private string SizeClasses => Size switch
    {
        SuSize.ExtraSmall => "px-1.5 py-0.5 text-[10px]",
        SuSize.Small => "px-2 py-0.5 text-xs",
        SuSize.Medium => "px-2.5 py-0.5 text-xs",
        SuSize.Large => "px-3 py-1 text-sm",
        SuSize.ExtraLarge => "px-4 py-1.5 text-sm",
        _ => "px-2.5 py-0.5 text-xs"
    };
}
