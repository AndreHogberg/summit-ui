using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Select;

/// <summary>
/// Text content wrapper for a select item.
/// </summary>
public partial class SelectItemText : ComponentBase
{
    /// <summary>
    /// Child content (the text to display).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "span".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "span";

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
