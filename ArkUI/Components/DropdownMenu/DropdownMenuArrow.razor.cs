using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// Arrow element pointing from the content to the trigger.
/// </summary>
public partial class DropdownMenuArrow : ComponentBase
{
    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

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
    /// Custom arrow content. If not provided, a default SVG arrow is used.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

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
