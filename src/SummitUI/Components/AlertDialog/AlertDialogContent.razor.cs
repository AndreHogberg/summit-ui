using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// The main content panel of the alert dialog with focus trapping, scroll locking,
/// and keyboard event handling.
///
/// When <see cref="AlertDialogOptions.IsDestructive"/> is true, the cancel button
/// (marked with data-summit-alertdialog-cancel) will receive initial focus.
/// </summary>
public partial class AlertDialogContent : IAsyncDisposable
{
    [Inject]
    private DialogJsInterop DialogInterop { get; set; } = default!;

    [Inject]
    private FloatingJsInterop FloatingInterop { get; set; } = default!;

    [CascadingParameter]
    private AlertDialogContext Context { get; set; } = default!;

    /// <summary>
    /// Child content of the alert dialog.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Whether to trap focus within the dialog content. Defaults to true.
    /// </summary>
    [Parameter]
    public bool TrapFocus { get; set; } = true;

    /// <summary>
    /// Whether to prevent body scroll when the dialog is open. Defaults to true.
    /// </summary>
    [Parameter]
    public bool PreventScroll { get; set; } = true;

    /// <summary>
    /// Callback invoked when Escape key is pressed (before closing).
    /// </summary>
    [Parameter]
    public EventCallback OnEscapeKeyDown { get; set; }

    /// <summary>
    /// Callback invoked after dialog opens and focuses.
    /// </summary>
    [Parameter]
    public EventCallback OnOpenAutoFocus { get; set; }

    /// <summary>
    /// Callback invoked when dialog closes and returns focus.
    /// </summary>
    [Parameter]
    public EventCallback OnCloseAutoFocus { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private DotNetObjectReference<AlertDialogContent>? _dotNetRef;
    private string? _escapeKeyListenerId;
    private bool _isInitialized;
    private bool _isInitializing;
    private bool _isDisposed;
    private bool _wasOpen;
    private bool _animationWatcherRegistered;
    private bool _scrollLocked;

    private string DataState => Context.IsOpen ? "open" : "closed";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!RendererInfo.IsInteractive) return;

        if (Context.IsOpen && !_isInitialized && !_isInitializing)
        {
            _isInitializing = true;

            try
            {
                if (Context.IsAnimatingClosed)
                {
                    await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
                    Context.IsAnimatingClosed = false;
                }
                _animationWatcherRegistered = false;

                _dotNetRef ??= DotNetObjectReference.Create(this);

                if (PreventScroll && !_scrollLocked)
                {
                    await DialogInterop.LockScrollAsync();
                    _scrollLocked = true;
                }

                if (Context.Options.AllowEscapeClose || OnEscapeKeyDown.HasDelegate)
                {
                    _escapeKeyListenerId = await FloatingInterop.RegisterEscapeKeyAsync(
                        _dotNetRef,
                        nameof(HandleEscapeKey));
                }

                _isInitialized = true;

                if (!TrapFocus)
                {
                    await FloatingInterop.FocusFirstElementAsync(_elementRef);
                }

                await OnOpenAutoFocus.InvokeAsync();
            }
            finally
            {
                _isInitializing = false;
            }
        }
        else if (!Context.IsOpen && _wasOpen && !_animationWatcherRegistered)
        {
            if (!string.IsNullOrEmpty(_escapeKeyListenerId))
            {
                await FloatingInterop.UnregisterEscapeKeyAsync(_escapeKeyListenerId);
                _escapeKeyListenerId = null;
            }

            _animationWatcherRegistered = true;
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await FloatingInterop.WaitForAnimationsCompleteAsync(
                _elementRef,
                _dotNetRef,
                nameof(OnCloseAnimationsComplete));
        }

        _wasOpen = Context.IsOpen;
    }

    private async Task CleanupAsync()
    {
        if (!_isInitialized) return;

        _isInitialized = false;

        try
        {
            if (!string.IsNullOrEmpty(_escapeKeyListenerId))
            {
                await FloatingInterop.UnregisterEscapeKeyAsync(_escapeKeyListenerId);
                _escapeKeyListenerId = null;
            }

            if (_scrollLocked)
            {
                await DialogInterop.UnlockScrollAsync();
                _scrollLocked = false;
            }
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
        catch (ObjectDisposedException)
        {
            // Component already disposed, ignore
        }
    }

    /// <summary>
    /// Called from JavaScript when Escape key is pressed.
    /// </summary>
    [JSInvokable]
    public async Task HandleEscapeKey()
    {
        if (_isDisposed || !Context.IsOpen) return;

        if (!string.IsNullOrEmpty(_escapeKeyListenerId))
        {
            await FloatingInterop.UnregisterEscapeKeyAsync(_escapeKeyListenerId);
            _escapeKeyListenerId = null;
        }

        await OnEscapeKeyDown.InvokeAsync();

        if (Context.Options.AllowEscapeClose)
        {
            Context.Complete(false);
        }
    }

    /// <summary>
    /// Called from JavaScript when all close animations have completed.
    /// </summary>
    [JSInvokable]
    public async Task OnCloseAnimationsComplete()
    {
        if (_isDisposed) return;

        Context.IsAnimatingClosed = false;

        if (!Context.IsOpen)
        {
            await CleanupAsync();
            await OnCloseAutoFocus.InvokeAsync();
        }

        Context.NotifyStateChanged();
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (Context.IsAnimatingClosed)
        {
            try
            {
                await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
            }
            catch (JSDisconnectedException)
            {
                // Ignore
            }
            Context.IsAnimatingClosed = false;
        }

        await CleanupAsync();
        _dotNetRef?.Dispose();
    }
}
