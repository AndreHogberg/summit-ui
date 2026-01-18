using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Accessible description for the dialog. Renders with the id used by aria-describedby.
/// </summary>
public partial class SmDialogDescription : ComponentBase
{
    [CascadingParameter]
    private DialogContext Context { get; set; } = default!;

    /// <summary>
    /// Child content (the description text).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
