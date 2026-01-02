using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Select;

/// <summary>
/// Renders children in a fixed-position container to avoid z-index and overflow issues.
/// Content is visually "portaled" to the top of the stacking context.
/// </summary>
/// <typeparam name="TValue">The type of the select value.</typeparam>
public partial class SelectPortal<TValue> : ComponentBase, IDisposable where TValue : notnull
{
    [CascadingParameter]
    private SelectContext<TValue> Context { get; set; } = default!;

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

    private string ActualContainerId => ContainerId ?? $"{Context.SelectId}-portal";

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
