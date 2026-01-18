using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Accessible title for the dialog. Renders with the id used by aria-labelledby.
/// </summary>
public partial class SmDialogTitle : ComponentBase
{
    [CascadingParameter]
    private DialogContext Context { get; set; } = default!;

    /// <summary>
    /// Child content (the title text).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
