using Microsoft.AspNetCore.Components;

namespace SummitUI.Docs.Design;

/// <summary>
/// A component for the table header section (thead).
/// </summary>
public partial class SuTableHeader : ComponentBase
{
    /// <summary>
    /// The content of the header (typically SuTableRow).
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private const string BaseClasses = "bg-su-muted/50 border-b border-su-border";

    private string FinalClass => SuStyles.Cn(BaseClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);
}
