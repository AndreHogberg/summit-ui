using Microsoft.AspNetCore.Components;

namespace SummitUI.Docs.Design;

/// <summary>
/// An icon-only button component.
/// </summary>
public partial class SuIconButton : ComponentBase
{
    /// <summary>
    /// The visual style variant of the button.
    /// </summary>
    [Parameter] public SuButtonVariant Variant { get; set; } = SuButtonVariant.Ghost;

    /// <summary>
    /// The size of the button.
    /// </summary>
    [Parameter] public SuSize Size { get; set; } = SuSize.Medium;

    /// <summary>
    /// Whether the button is disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// The accessible label for the button (required for icon-only buttons).
    /// </summary>
    [Parameter] public string? AriaLabel { get; set; }

    /// <summary>
    /// The button type attribute.
    /// </summary>
    [Parameter] public string Type { get; set; } = "button";

    /// <summary>
    /// The icon content to render inside the button.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the button element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private const string BaseClasses =
        "inline-flex items-center justify-center rounded-md " +
        "transition-all duration-150 " +
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-su-ring focus-visible:ring-offset-2 focus-visible:ring-offset-su-background " +
        "disabled:pointer-events-none disabled:opacity-50";

    private string FinalClass => SuStyles.Cn(BaseClasses, VariantClasses, SizeClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);

    private string VariantClasses => Variant switch
    {
        SuButtonVariant.Primary => "bg-su-primary text-su-primary-foreground shadow-sm hover:bg-blue-600 dark:hover:bg-blue-500",
        SuButtonVariant.Secondary => "bg-su-muted text-su-foreground shadow-sm hover:bg-gray-200 dark:hover:bg-gray-700",
        SuButtonVariant.Ghost => "text-su-muted-foreground hover:bg-su-accent hover:text-su-foreground",
        SuButtonVariant.Outline => "border border-su-border bg-su-background text-su-foreground shadow-sm hover:bg-su-accent",
        SuButtonVariant.Destructive => "bg-su-destructive text-su-destructive-foreground shadow-sm hover:bg-red-600 dark:hover:bg-red-500",
        SuButtonVariant.Link => "text-su-primary hover:text-blue-600 dark:hover:text-blue-400",
        _ => ""
    };

    private string SizeClasses => Size switch
    {
        SuSize.ExtraSmall => "h-6 w-6",
        SuSize.Small => "h-8 w-8",
        SuSize.Medium => "h-10 w-10",
        SuSize.Large => "h-11 w-11",
        SuSize.ExtraLarge => "h-12 w-12",
        _ => "h-10 w-10"
    };
}
