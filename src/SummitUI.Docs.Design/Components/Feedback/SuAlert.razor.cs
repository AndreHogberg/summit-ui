using Microsoft.AspNetCore.Components;

using SummitUI.Docs.Design.Utilities;

namespace SummitUI.Docs.Design;

/// <summary>
/// An alert/callout component for displaying important messages with different severity levels.
/// </summary>
public partial class SuAlert : ComponentBase
{
    /// <summary>
    /// The visual style variant of the alert.
    /// </summary>
    [Parameter] public SuAlertVariant Variant { get; set; } = SuAlertVariant.Default;

    /// <summary>
    /// The content to render inside the alert.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the alert element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private const string BaseClasses =
        "relative w-full rounded-lg border p-4 [&>svg~*]:pl-7 [&>svg]:absolute [&>svg]:left-4 [&>svg]:top-4 [&>svg]:text-current";

    private string FinalClass => SuStyles.Cn(BaseClasses, VariantClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);

    private string VariantClasses => Variant switch
    {
        SuAlertVariant.Default =>
            "bg-su-background text-su-foreground border-su-border",
        SuAlertVariant.Info =>
            "bg-blue-50 dark:bg-blue-950 text-blue-900 dark:text-blue-100 border-blue-200 dark:border-blue-800 [&>svg]:text-blue-600 dark:[&>svg]:text-blue-400",
        SuAlertVariant.Success =>
            "bg-green-50 dark:bg-green-950 text-green-900 dark:text-green-100 border-green-200 dark:border-green-800 [&>svg]:text-green-600 dark:[&>svg]:text-green-400",
        SuAlertVariant.Warning =>
            "bg-amber-50 dark:bg-amber-950 text-amber-900 dark:text-amber-100 border-amber-200 dark:border-amber-800 [&>svg]:text-amber-600 dark:[&>svg]:text-amber-400",
        SuAlertVariant.Destructive =>
            "bg-red-50 dark:bg-red-950 text-red-900 dark:text-red-100 border-red-200 dark:border-red-800 [&>svg]:text-red-600 dark:[&>svg]:text-red-400",
        SuAlertVariant.Tip =>
            "bg-purple-50 dark:bg-purple-950 text-purple-900 dark:text-purple-100 border-purple-200 dark:border-purple-800 [&>svg]:text-purple-600 dark:[&>svg]:text-purple-400",
        _ => ""
    };

    private MarkupString IconMarkup => Variant switch
    {
        SuAlertVariant.Info => new MarkupString("""<svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/></svg>"""),
        SuAlertVariant.Success => new MarkupString("""<svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="m9 12 2 2 4-4"/></svg>"""),
        SuAlertVariant.Warning => new MarkupString("""<svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3"/><path d="M12 9v4"/><path d="M12 17h.01"/></svg>"""),
        SuAlertVariant.Destructive => new MarkupString("""<svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="m15 9-6 6"/><path d="m9 9 6 6"/></svg>"""),
        SuAlertVariant.Tip => new MarkupString("""<svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 14c.2-1 .7-1.7 1.5-2.5 1-.9 1.5-2.2 1.5-3.5A6 6 0 0 0 6 8c0 1 .2 2.2 1.5 3.5.7.7 1.3 1.5 1.5 2.5"/><path d="M9 18h6"/><path d="M10 22h4"/></svg>"""),
        _ => new MarkupString("""<svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/></svg>""")
    };
}
