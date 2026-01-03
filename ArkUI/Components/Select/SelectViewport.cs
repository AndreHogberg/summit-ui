using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ArkUI;

/// <summary>
/// Scrollable viewport container for select items.
/// </summary>
/// <typeparam name="TValue">The type of the select value.</typeparam>
public class SelectViewport<TValue> : ComponentBase where TValue : notnull
{
    /// <summary>
    /// Child content (SelectItem, SelectGroup, etc.).
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
        builder.AddAttribute(1, "data-ark-select-viewport", "");
        builder.AddAttribute(2, "style", "overflow: auto;");
        builder.AddMultipleAttributes(3, AdditionalAttributes);
        builder.AddContent(4, ChildContent);
        builder.CloseElement();
    }
}
