using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SummitUI.Docs.Design.Utilities;

namespace SummitUI.Docs.Design;

/// <summary>
///     A button component that provides quick access to LLM documentation and AI assistant prompts.
///     Displays a dropdown with options to copy llms.txt link or open AI assistants with pre-filled prompts.
/// </summary>
public partial class SuPromptButton : ComponentBase
{
    private const string ButtonClasses =
        "inline-flex items-center justify-center gap-1.5 rounded-md px-3 py-1.5 text-xs font-medium " +
        "bg-su-primary text-su-primary-foreground shadow-sm " +
        "hover:bg-su-primary/90 " +
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-su-ring focus-visible:ring-offset-2 " +
        "transition-colors duration-150";

    private const string DropdownClasses =
        "w-50 rounded-lg border border-su-border bg-su-card p-1.5 shadow-xl z-50 antialiased font-sans";

    private const string MenuItemClasses =
        "flex w-full items-center gap-3 rounded-md px-3 py-2.5 text-sm text-su-foreground " +
        "hover:bg-su-accent data-[highlighted]:bg-su-accent transition-colors cursor-pointer text-left";

    private bool _isOpen;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    /// <summary>
    ///     The current page/component name for context in the prompt (e.g., "Accordion", "Tabs").
    /// </summary>
    [Parameter]
    public string? ComponentName { get; set; }

    /// <summary>
    ///     The relative URL path to the documentation (e.g., "/docs/accordion" or "/docs/guides/my-guide").
    /// </summary>
    [Parameter]
    public string? Url { get; set; }

    /// <summary>
    ///     The base URL for the documentation site. Used to construct llms.txt URL.
    /// </summary>
    [Parameter]
    public string BaseUrl { get; set; } = "https://www.summitui.com";

    /// <summary>
    ///     Additional HTML attributes to apply to the button element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string FinalButtonClass => SuStyles.Cn(ButtonClasses, UserClass);

    private string? UserClass =>
        AdditionalAttributes?.TryGetValue("class", out object? cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);

    private string ChevronClass => _isOpen ? "transform rotate-180 transition-transform" : "transition-transform";

    private async Task CopyLlmsTxtUrl()
    {
        string url = BuildLlmsTxtUrl();
        await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", url);
    }

    private string BuildLlmsTxtUrl()
    {
        string baseUrl = BaseUrl.TrimEnd('/');
        string path = string.IsNullOrEmpty(Url)
            ? (string.IsNullOrEmpty(ComponentName) ? "" : $"docs/{ComponentName.ToLowerInvariant()}")
            : Url.Trim('/');

        if (string.IsNullOrEmpty(path))
        {
            return $"{baseUrl}/llms.txt";
        }

        return $"{baseUrl}/{path}/llms.txt";
    }

    private async Task OpenChatGpt()
    {
        string prompt = BuildPrompt();
        string encodedPrompt = Uri.EscapeDataString(prompt);
        string url = $"https://chat.openai.com/?q={encodedPrompt}";
        await JsRuntime.InvokeVoidAsync("window.open", url, "_blank");
    }

    private async Task OpenClaude()
    {
        string prompt = BuildPrompt();
        string encodedPrompt = Uri.EscapeDataString(prompt);
        await JsRuntime.InvokeVoidAsync("window.open", $"https://claude.ai/new?q={encodedPrompt}", "_blank");
    }

    private string BuildPrompt()
    {
        string componentContext = string.IsNullOrEmpty(ComponentName)
            ? "SummitUI components"
            : (ComponentName.EndsWith("Composition") || ComponentName.Contains("Guide") 
                ? $"{ComponentName}" 
                : $"the {ComponentName} component");

        string llmsTxtUrl = BuildLlmsTxtUrl();

        return $"""
                I'm using SummitUI, a Blazor component library. Please help me understand {componentContext} in SummitUI.

                You can find the full documentation at: {llmsTxtUrl}

                Please read the documentation and help me with any questions I have.
                """;
    }
}
