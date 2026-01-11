using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Renders children in a fixed-position container to avoid z-index and overflow issues.
/// Content is visually "portaled" to the top of the stacking context.
/// </summary>
/// <typeparam name="TValue">The type of the combobox value.</typeparam>
public class ComboboxPortal<TValue> : ComponentBase, IDisposable where TValue : notnull
{
    [CascadingParameter]
    private ComboboxContext<TValue> Context { get; set; } = default!;

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
    private bool _isSubscribed;
    private bool _isDisposed;

    private string ActualContainerId => ContainerId ?? $"{Context.ComboboxId}-portal";

    protected override void OnInitialized()
    {
        // Subscribe to context state changes
        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!Context.IsOpen && !Context.IsAnimatingClosed) return;

        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", ActualContainerId);
        builder.AddAttribute(2, "data-summit-combobox-portal", "");
        builder.AddAttribute(3, "style", "position: absolute; top: 0; left: 0; z-index: 9999; pointer-events: none;");
        builder.AddElementReferenceCapture(4, elementRef => _containerRef = elementRef);

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
