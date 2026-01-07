using Microsoft.AspNetCore.Components;

using SummitUI.Docs.Design.Utilities;

namespace SummitUI.Docs.Design;

/// <summary>
/// A container component with max-width constraints.
/// </summary>
public partial class SuContainer : ComponentBase
{
    /// <summary>
    /// The maximum width of the container.
    /// </summary>
    [Parameter] public SuContainerSize Size { get; set; } = SuContainerSize.ThreeXl;

    /// <summary>
    /// Whether to center the container horizontally.
    /// </summary>
    [Parameter] public bool Centered { get; set; } = true;

    /// <summary>
    /// Whether to add horizontal padding.
    /// </summary>
    [Parameter] public bool Padded { get; set; } = true;

    /// <summary>
    /// The content to render inside the container.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the container element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string FinalClass => SuStyles.Cn(SizeClasses, CenterClasses, PaddingClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);

    private string CenterClasses => Centered ? "mx-auto" : "";

    private string PaddingClasses => Padded ? "px-4 sm:px-6 lg:px-8" : "";

    private string SizeClasses => Size switch
    {
        SuContainerSize.Small => "max-w-sm",
        SuContainerSize.Medium => "max-w-md",
        SuContainerSize.Large => "max-w-lg",
        SuContainerSize.ExtraLarge => "max-w-xl",
        SuContainerSize.TwoXl => "max-w-2xl",
        SuContainerSize.ThreeXl => "max-w-3xl",
        SuContainerSize.FourXl => "max-w-4xl",
        SuContainerSize.Full => "max-w-full",
        _ => "max-w-3xl"
    };
}
