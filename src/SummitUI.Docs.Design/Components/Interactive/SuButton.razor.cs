using Microsoft.AspNetCore.Components;

namespace SummitUI.Docs.Design;

/// <summary>
/// A styled button component with multiple variants and sizes.
/// </summary>
public partial class SuButton : ComponentBase
{
    /// <summary>
    /// The visual style variant of the button.
    /// </summary>
    [Parameter] public SuButtonVariant Variant { get; set; } = SuButtonVariant.Primary;

    /// <summary>
    /// The size of the button.
    /// </summary>
    [Parameter] public SuSize Size { get; set; } = SuSize.Medium;

    /// <summary>
    /// Whether the button is in a loading state.
    /// </summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>
    /// Whether the button is disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// The button type attribute (button, submit, reset).
    /// </summary>
    [Parameter] public string Type { get; set; } = "button";

    /// <summary>
    /// The content to render inside the button.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the button element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private const string BaseClasses =
        "inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md font-medium " +
        "transition-all duration-150 " +
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-su-ring focus-visible:ring-offset-2 focus-visible:ring-offset-su-background " +
        "disabled:pointer-events-none disabled:opacity-50";

    private bool IsDisabled => Disabled || Loading;

    private string FinalClass => SuStyles.Cn(BaseClasses, VariantClasses, SizeClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);

    private string VariantClasses => Variant switch
    {
        SuButtonVariant.Primary => "bg-su-primary text-su-primary-foreground shadow-sm hover:bg-blue-900",
        SuButtonVariant.Secondary => "bg-su-muted text-su-foreground shadow-sm hover:bg-gray-200 dark:hover:bg-gray-700",
        SuButtonVariant.Ghost => "text-su-foreground hover:bg-su-accent hover:text-su-accent-foreground",
        SuButtonVariant.Outline => "border border-su-border bg-su-background text-su-foreground shadow-sm hover:bg-su-accent hover:text-su-accent-foreground",
        SuButtonVariant.Destructive => "bg-su-destructive text-su-destructive-foreground shadow-sm hover:bg-red-600 dark:hover:bg-red-500",
        SuButtonVariant.Link => "text-su-primary underline-offset-4 hover:underline",
        _ => ""
    };

    private string SizeClasses => Size switch
    {
        SuSize.ExtraSmall => "h-7 px-2 text-xs rounded",
        SuSize.Small => "h-8 px-3 text-xs",
        SuSize.Medium => "h-10 px-4 text-sm",
        SuSize.Large => "h-11 px-6 text-base",
        SuSize.ExtraLarge => "h-12 px-8 text-lg",
        _ => "h-10 px-4 text-sm"
    };
}
