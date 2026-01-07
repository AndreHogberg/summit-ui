using Microsoft.AspNetCore.Components;

using SummitUI.Docs.Design.Utilities;

namespace SummitUI.Docs.Design;

/// <summary>
/// A card header component.
/// </summary>
public partial class SuCardHeader : ComponentBase
{
    /// <summary>
    /// The content to render inside the card header.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private const string BaseClasses = "flex flex-col space-y-1.5 p-6";

    private string FinalClass => SuStyles.Cn(BaseClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);
}
