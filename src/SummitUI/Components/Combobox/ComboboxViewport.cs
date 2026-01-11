using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Scrollable viewport container for combobox items.
/// </summary>
/// <typeparam name="TValue">The type of the combobox value.</typeparam>
public class ComboboxViewport<TValue> : ComponentBase where TValue : notnull
{
    /// <summary>
    /// Child content (ComboboxItem, ComboboxGroup, ComboboxEmpty, etc.).
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
        builder.AddAttribute(1, "data-summit-combobox-viewport", "");
        builder.AddAttribute(2, "style", "overflow: auto;");
        builder.AddMultipleAttributes(3, AdditionalAttributes);
        builder.AddContent(4, ChildContent);
        builder.CloseElement();
    }
}
