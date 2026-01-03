using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Switch;

public partial class SwitchThumb : ComponentBase
{
    [CascadingParameter] public SwitchContext? Context { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string DataState => Context?.IsChecked == true ? "checked" : "unchecked";
}
