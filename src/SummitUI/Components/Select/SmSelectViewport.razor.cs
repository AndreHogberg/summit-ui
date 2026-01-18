using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Scrollable viewport container for select items.
/// </summary>
/// <typeparam name="TValue">The type of the select value.</typeparam>
public partial class SmSelectViewport<TValue> : ComponentBase where TValue : notnull
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
}
