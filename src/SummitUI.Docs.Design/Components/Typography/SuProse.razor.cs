using Microsoft.AspNetCore.Components;

using SummitUI.Docs.Design.Utilities;

namespace SummitUI.Docs.Design;

/// <summary>
/// A prose container for rich text content with typography styling.
/// </summary>
public partial class SuProse : ComponentBase
{
    /// <summary>
    /// The content to render inside the prose container.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the prose element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private const string BaseClasses =
        "prose prose-neutral dark:prose-invert max-w-none " +
        "prose-headings:text-su-foreground prose-headings:font-semibold " +
        "prose-p:text-su-foreground prose-p:leading-relaxed " +
        "prose-a:text-su-primary prose-a:no-underline hover:prose-a:underline " +
        "prose-strong:text-su-foreground prose-strong:font-semibold " +
        "prose-code:text-su-foreground prose-code:bg-su-muted prose-code:px-1 prose-code:py-0.5 prose-code:rounded prose-code:text-sm " +
        "prose-pre:bg-su-card prose-pre:border prose-pre:border-su-border " +
        "prose-blockquote:border-su-border prose-blockquote:text-su-muted-foreground " +
        "prose-hr:border-su-border " +
        "prose-th:text-su-foreground prose-td:text-su-muted-foreground";

    private string FinalClass => SuStyles.Cn(BaseClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);
}
