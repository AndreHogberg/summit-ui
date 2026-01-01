using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Popover;

/// <summary>
/// Optional visual arrow pointing from the popover content to the trigger.
/// </summary>
public partial class PopoverArrow : ComponentBase
{
    [CascadingParameter]
    private PopoverContext Context { get; set; } = default!;

    /// <summary>
    /// Optional child content for custom arrow rendering.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// Width of the arrow in pixels.
    /// </summary>
    [Parameter]
    public int Width { get; set; } = 10;

    /// <summary>
    /// Height of the arrow in pixels.
    /// </summary>
    [Parameter]
    public int Height { get; set; } = 5;

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Context.RegisterArrow(_elementRef);
        }
    }
}
