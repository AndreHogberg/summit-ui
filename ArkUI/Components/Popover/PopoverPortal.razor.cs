using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Popover;

/// <summary>
/// Renders children in a fixed-position container to avoid z-index and overflow issues.
/// Content is visually "portaled" to the top of the stacking context.
/// </summary>
public partial class PopoverPortal : ComponentBase
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
}
