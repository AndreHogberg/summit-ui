using Microsoft.AspNetCore.Components;

namespace SummitUI.Docs.Design;

/// <summary>
/// An inline code component for displaying code snippets within text.
/// </summary>
public partial class SuCode : ComponentBase
{
    /// <summary>
    /// The content to render inside the code element.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the code element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private const string BaseClasses =
        "relative rounded bg-su-muted px-[0.3rem] py-[0.2rem] font-mono text-sm text-su-foreground";

    private string FinalClass => SuStyles.Cn(BaseClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);
}
