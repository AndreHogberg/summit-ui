using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// A backdrop overlay that covers the page when the dialog is open.
/// Clicking the overlay typically closes the dialog.
/// Supports nested dialog styling via data attributes and CSS custom properties.
/// </summary>
public partial class SmDialogOverlay : ComponentBase, IAsyncDisposable
{
    [Inject]
    private FloatingJsInterop FloatingInterop { get; set; } = default!;

    [CascadingParameter]
    private DialogContext Context { get; set; } = default!;

    /// <summary>
    /// Optional child content for the overlay.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Callback when the overlay is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Whether clicking the overlay closes the dialog. Defaults to true.
    /// </summary>
    [Parameter]
    public bool CloseOnClick { get; set; } = true;

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string DataState => Context.IsOpen ? "open" : "closed";

    /// <summary>
    /// CSS custom properties for nested dialog styling.
    /// </summary>
    private string CssVariables =>
        $"--summit-dialog-depth: {Context.Depth}; --summit-dialog-nested-count: {Context.NestedOpenCount};";

    private ElementReference _elementRef;
    private DotNetObjectReference<SmDialogOverlay>? _dotNetRef;
    private bool _wasOpen;
    private bool _animationWatcherRegistered;
    private bool _isDisposed;

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        await OnClick.InvokeAsync(args);

        if (CloseOnClick)
        {
            await Context.CloseAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!RendererInfo.IsInteractive) return;

        if (Context.IsOpen && !_wasOpen)
        {
            // Dialog just opened - cancel any pending animation watcher if reopening
            if (Context.IsOverlayAnimatingClosed)
            {
                await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
                Context.IsOverlayAnimatingClosed = false;
            }
            _animationWatcherRegistered = false;
        }
        else if (!Context.IsOpen && _wasOpen && !_animationWatcherRegistered)
        {
            // Dialog just closed - start waiting for close animations to complete
            _animationWatcherRegistered = true;
            Context.IsOverlayAnimatingClosed = true;
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await FloatingInterop.WaitForAnimationsCompleteAsync(
                _elementRef,
                _dotNetRef,
                nameof(OnCloseAnimationsComplete));
        }

        _wasOpen = Context.IsOpen;
    }

    /// <summary>
    /// Called from JavaScript when all close animations have completed.
    /// </summary>
    [JSInvokable]
    public async Task OnCloseAnimationsComplete()
    {
        if (_isDisposed) return;

        Context.IsOverlayAnimatingClosed = false;

        // Trigger re-render to potentially remove element from DOM
        // (only if content is also done animating)
        Context.RaiseStateChanged();
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Cancel any pending animation watcher
        if (Context.IsOverlayAnimatingClosed)
        {
            try
            {
                await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
            }
            catch (JSDisconnectedException)
            {
                // Ignore
            }
            Context.IsOverlayAnimatingClosed = false;
        }

        _dotNetRef?.Dispose();
    }
}
