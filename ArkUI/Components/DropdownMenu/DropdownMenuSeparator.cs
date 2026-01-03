using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ArkUI;

/// <summary>
/// Visual separator between menu items.
/// </summary>
public class DropdownMenuSeparator : ComponentBase
{
    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "role", "separator");
        builder.AddAttribute(2, "aria-orientation", "horizontal");
        builder.AddAttribute(3, "data-ark-dropdown-menu-separator", "");
        builder.AddMultipleAttributes(4, AdditionalAttributes);
        builder.CloseElement();
    }
}
