using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// Visual separator between menu items.
/// </summary>
public partial class DropdownMenuSeparator : ComponentBase
{
    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
