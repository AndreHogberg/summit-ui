using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// The thumb element of a switch that visually indicates the checked state.
/// Must be used inside a SmSwitchRoot component.
/// </summary>
public partial class SmSwitchThumb : ComponentBase
{
    /// <summary>
    /// The switch context provided by the parent SmSwitchRoot.
    /// </summary>
    [CascadingParameter]
    public SwitchContext? Context { get; set; }

    /// <summary>
    /// Child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string DataState => Context?.IsChecked == true ? "checked" : "unchecked";
}
