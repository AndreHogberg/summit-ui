using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Renders dialog children in a fixed-position container at the top of the stacking context.
/// This avoids z-index and overflow issues that can occur with nested DOM structures.
/// Uses depth-based z-index calculation for proper nested dialog stacking.
/// </summary>
public class DialogPortal : ComponentBase, IDisposable
{
    [CascadingParameter]
    private DialogContext Context { get; set; } = default!;

    /// <summary>
    /// Content to render in the portal (typically DialogOverlay and DialogContent).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Optional custom container ID.
    /// </summary>
    [Parameter]
    public string? ContainerId { get; set; }

    private bool _isSubscribed;
    private bool _isDisposed;

    private string ActualContainerId => ContainerId ?? Context.PortalId;

    protected override void OnInitialized()
    {
        // Subscribe to context state changes for animation awareness
        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Only render when open or during close animation
        if (!Context.IsOpen && !Context.IsAnimatingClosed) return;

        // Calculate z-index based on depth (base 9999, increment by 10 per depth level)
        var zIndex = 9999 + (Context.Depth * 10);

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", ActualContainerId);
        builder.AddAttribute(2, "data-summit-dialog-portal", true);
        builder.AddAttribute(3, "data-depth", Context.Depth);
        builder.AddAttribute(4, "style", $"position: fixed; top: 0; left: 0; right: 0; bottom: 0; z-index: {zIndex}; pointer-events: none;");

        builder.OpenElement(5, "div");
        builder.AddAttribute(6, "style", "pointer-events: auto;");
        builder.AddContent(7, ChildContent);
        builder.CloseElement();

        builder.CloseElement();
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }
    }
}
