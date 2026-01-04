using Microsoft.AspNetCore.Components;

namespace SummitUI.Docs.Design;

/// <summary>
/// The title component for an alert.
/// </summary>
public partial class SuAlertTitle : ComponentBase
{
    /// <summary>
    /// The content to render as the alert title.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the title element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private const string BaseClasses = "mb-1 font-medium leading-none tracking-tight";

    private string FinalClass => SuStyles.Cn(BaseClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);
}
