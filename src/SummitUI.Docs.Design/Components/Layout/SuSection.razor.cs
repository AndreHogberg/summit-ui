using Microsoft.AspNetCore.Components;

using SummitUI.Docs.Design.Utilities;

namespace SummitUI.Docs.Design;

/// <summary>
/// A section component for organizing page content with consistent spacing.
/// </summary>
public partial class SuSection : ComponentBase
{
    /// <summary>
    /// Optional title for the section.
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Optional description for the section.
    /// </summary>
    [Parameter] public string? Description { get; set; }

    /// <summary>
    /// The content to render inside the section.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the section element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private const string BaseClasses = "flex flex-col gap-4";

    private string FinalClass => SuStyles.Cn(BaseClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);
}
