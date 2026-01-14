using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Tab content panel. Renders with role="tabpanel".
/// Only renders when the associated tab is active.
/// </summary>
public class SmTabsContent : ComponentBase
{
    [CascadingParameter]
    private TabsContext Context { get; set; } = default!;

    /// <summary>
    /// Value matching the associated TabsTrigger. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public string Value { get; set; } = "";

    /// <summary>
    /// Panel content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// Additional HTML attributes to apply.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private bool IsActive => Context.Value == Value;
    private string DataState => IsActive ? "active" : "inactive";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!IsActive) return;

        builder.OpenElement(0, As);
        builder.AddAttribute(1, "role", "tabpanel");
        builder.AddAttribute(2, "id", Context.GetContentId(Value));
        builder.AddAttribute(3, "aria-labelledby", Context.GetTriggerId(Value));
        builder.AddAttribute(4, "tabindex", "0");
        builder.AddAttribute(5, "data-state", DataState);
        builder.AddAttribute(6, "data-orientation", Context.Orientation.ToString().ToLowerInvariant());
        builder.AddAttribute(7, "data-summit-tabs-content", true);
        builder.AddMultipleAttributes(8, AdditionalAttributes);
        builder.AddContent(9, ChildContent);
        builder.CloseElement();
    }
}
