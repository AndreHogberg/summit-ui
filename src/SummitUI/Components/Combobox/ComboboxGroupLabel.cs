using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Label for a combobox group.
/// </summary>
/// <typeparam name="TValue">The type of the combobox value.</typeparam>
public class ComboboxGroupLabel<TValue> : ComponentBase where TValue : notnull
{
    [CascadingParameter]
    private ComboboxContext<TValue> Context { get; set; } = default!;

    [CascadingParameter(Name = "ComboboxGroupId")]
    private string? GroupId { get; set; }

    /// <summary>
    /// Label content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        
        if (!string.IsNullOrEmpty(GroupId))
        {
            builder.AddAttribute(1, "id", Context.GetGroupLabelId(GroupId));
        }
        
        builder.AddAttribute(2, "role", "presentation");
        builder.AddAttribute(3, "aria-hidden", "true");
        builder.AddAttribute(4, "data-summit-combobox-group-label", "");
        builder.AddMultipleAttributes(5, AdditionalAttributes);
        builder.AddContent(6, ChildContent);
        builder.CloseElement();
    }
}
