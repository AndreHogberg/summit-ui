using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Select;

/// <summary>
/// Label for a select group. Used for accessibility labeling.
/// </summary>
public partial class SelectGroupLabel : ComponentBase
{
    [CascadingParameter]
    private SelectGroupContext GroupContext { get; set; } = default!;

    /// <summary>
    /// Child content (the label text).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
