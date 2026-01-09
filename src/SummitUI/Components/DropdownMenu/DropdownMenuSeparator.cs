using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Visual separator between menu items.
/// Inherits from SeparatorRoot for consistent separator behavior.
/// </summary>
public class DropdownMenuSeparator : SeparatorRoot
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Merge the dropdown-specific attribute with any additional attributes
        var mergedAttributes = new Dictionary<string, object>
        {
            ["data-summit-dropdown-menu-separator"] = ""
        };

        if (AdditionalAttributes != null)
        {
            foreach (var attr in AdditionalAttributes)
            {
                mergedAttributes[attr.Key] = attr.Value;
            }
        }

        // Temporarily replace AdditionalAttributes for base rendering
        var originalAttributes = AdditionalAttributes;
        AdditionalAttributes = mergedAttributes;

        base.BuildRenderTree(builder);

        AdditionalAttributes = originalAttributes;
    }
}
