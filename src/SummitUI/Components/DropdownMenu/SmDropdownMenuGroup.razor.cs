using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Groups related menu items together.
/// </summary>
public partial class SmDropdownMenuGroup : ComponentBase
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

    private readonly string _labelId = $"summit-dropdown-menu-group-{Guid.NewGuid():N}";
}
