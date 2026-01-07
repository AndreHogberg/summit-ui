using Microsoft.AspNetCore.Components;

using SummitUI.Docs.Design.Utilities;

namespace SummitUI.Docs.Design;

/// <summary>
/// A code block component for displaying multi-line code with optional language label.
/// </summary>
public partial class SuCodeBlock : ComponentBase
{
    /// <summary>
    /// The programming language for the code block (displayed as a label).
    /// </summary>
    [Parameter] public string? Language { get; set; }

    /// <summary>
    /// Whether to show line numbers.
    /// </summary>
    [Parameter] public bool ShowLineNumbers { get; set; }

    /// <summary>
    /// The code content to display. Use this for plain text code.
    /// </summary>
    [Parameter] public string? Code { get; set; }

    /// <summary>
    /// The content to render inside the code block. Alternative to Code parameter.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the container element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private const string ContainerClasses =
        "relative my-4 rounded-lg border border-su-border bg-su-muted/30 dark:bg-gray-900";

    private const string HeaderClasses =
        "flex items-center justify-between border-b border-su-border px-4 py-2 bg-su-muted/50";

    private const string LanguageLabelClasses =
        "text-xs font-medium text-su-muted-foreground uppercase tracking-wider";

    private const string PreClasses = "overflow-x-auto p-4";

    private const string CodeClasses = "font-mono text-sm text-su-foreground leading-relaxed";

    private const string LineNumberClasses =
        "inline-block w-8 text-right pr-4 text-su-muted-foreground select-none";

    private string FinalContainerClass => SuStyles.Cn(ContainerClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);
}
