using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Renders children in a fixed-position container to avoid z-index and overflow issues.
/// Content is visually "portaled" to the top of the stacking context.
/// </summary>
public class PopoverPortal : ComponentBase
{
    [CascadingParameter]
    private PopoverContext Context { get; set; } = default!;

    /// <summary>
    /// Content to render in the portal.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Optional custom container ID.
    /// </summary>
    [Parameter]
    public string? ContainerId { get; set; }

    private ElementReference _containerRef;

    private string ActualContainerId => ContainerId ?? $"{Context.PopoverId}-portal";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!Context.IsOpen) return;

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", ActualContainerId);
        builder.AddAttribute(2, "data-ark-popover-portal", true);
        builder.AddAttribute(3, "style", "position: fixed; top: 0; left: 0; z-index: 9999; pointer-events: none;");
        builder.AddElementReferenceCapture(4, elementRef => _containerRef = elementRef);

        builder.OpenElement(5, "div");
        builder.AddAttribute(6, "style", "pointer-events: auto;");
        builder.AddContent(7, ChildContent);
        builder.CloseElement();

        builder.CloseElement();
    }
}
