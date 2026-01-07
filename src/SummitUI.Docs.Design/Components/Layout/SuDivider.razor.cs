using Microsoft.AspNetCore.Components;

using SummitUI.Docs.Design.Utilities;

namespace SummitUI.Docs.Design;

/// <summary>
/// A divider/separator component.
/// </summary>
public partial class SuDivider : ComponentBase
{
    /// <summary>
    /// The orientation of the divider.
    /// </summary>
    [Parameter] public SuOrientation Orientation { get; set; } = SuOrientation.Horizontal;

    /// <summary>
    /// Additional HTML attributes to apply to the divider element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string FinalClass => SuStyles.Cn("shrink-0 bg-su-border", OrientationClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);

    private string AriaOrientation => Orientation == SuOrientation.Vertical ? "vertical" : "horizontal";

    private string OrientationClasses => Orientation switch
    {
        SuOrientation.Horizontal => "h-px w-full",
        SuOrientation.Vertical => "h-full w-px",
        _ => "h-px w-full"
    };
}
