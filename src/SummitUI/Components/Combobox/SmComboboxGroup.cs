using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Groups combobox items together with an optional label.
/// </summary>
/// <typeparam name="TValue">The type of the combobox value.</typeparam>
public class SmComboboxGroup<TValue> : ComponentBase where TValue : notnull
{
    [CascadingParameter]
    private ComboboxContext<TValue> Context { get; set; } = default!;

    /// <summary>
    /// Child content (ComboboxGroupLabel and ComboboxItems).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// Unique identifier for this group (used for ARIA relationships).
    /// </summary>
    [Parameter]
    public string? GroupId { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string EffectiveGroupId => GroupId ?? Guid.NewGuid().ToString("N")[..8];

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "role", "group");
        builder.AddAttribute(2, "aria-labelledby", Context.GetGroupLabelId(EffectiveGroupId));
        builder.AddAttribute(3, "data-summit-combobox-group", "");
        builder.AddMultipleAttributes(4, AdditionalAttributes);

        // Provide group ID to children via cascading value
        builder.OpenComponent<CascadingValue<string>>(5);
        builder.AddComponentParameter(6, "Name", "ComboboxGroupId");
        builder.AddComponentParameter(7, "Value", EffectiveGroupId);
        builder.AddComponentParameter(8, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}
