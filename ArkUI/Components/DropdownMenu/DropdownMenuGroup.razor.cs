using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// Groups related menu items together.
/// </summary>
public partial class DropdownMenuGroup : ComponentBase
{
    /// <summary>
    /// Accessible label for the group (alternative to GroupLabel).
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Child content (typically GroupLabel and Items).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
