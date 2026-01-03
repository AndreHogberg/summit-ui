using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// Groups related menu items together.
/// </summary>
public class DropdownMenuGroup : ComponentBase
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

    private readonly string _labelId = $"ark-dropdown-menu-group-{Guid.NewGuid():N}";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "role", "group");
        builder.AddAttribute(2, "aria-labelledby", _labelId);
        builder.AddAttribute(3, "aria-label", AriaLabel);
        builder.AddAttribute(4, "data-ark-dropdown-menu-group", "");
        builder.AddMultipleAttributes(5, AdditionalAttributes);

        builder.OpenComponent<CascadingValue<string>>(6);
        builder.AddComponentParameter(7, "Value", _labelId);
        builder.AddComponentParameter(8, "Name", "GroupLabelId");
        builder.AddComponentParameter(9, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}
