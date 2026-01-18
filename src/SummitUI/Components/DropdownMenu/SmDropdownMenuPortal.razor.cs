using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// Portal component that renders dropdown menu content at the document body level.
/// Helps avoid z-index and overflow issues.
/// </summary>
public partial class SmDropdownMenuPortal : ComponentBase, IAsyncDisposable
{
    [Inject]
    private DropdownMenuJsInterop JsInterop { get; set; } = default!;

    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

    /// <summary>
    /// Child content to be portaled.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// CSS selector for the container to portal to. Defaults to body.
    /// </summary>
    [Parameter]
    public string? Container { get; set; }

    /// <summary>
    /// Whether to force mount the content (for animation purposes).
    /// </summary>
    [Parameter]
    public bool ForceMount { get; set; }

    private readonly string _portalId = $"summit-dropdown-menu-portal-{Guid.NewGuid():N}";
    private bool _isDisposed;
    private bool _portalCreated;
    private bool _isSubscribed;

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!RendererInfo.IsInteractive) return;

        if (Context.IsOpen && !_portalCreated)
        {
            try
            {
                await JsInterop.CreatePortalAsync(_portalId, Container);
                _portalCreated = true;
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, ignore
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }

        if (!_portalCreated) return;

        try
        {
            await JsInterop.DestroyPortalAsync(_portalId);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }
}
