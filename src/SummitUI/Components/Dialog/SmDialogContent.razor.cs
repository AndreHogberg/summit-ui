using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// The main content panel of the dialog with focus trapping, scroll locking,
/// and keyboard event handling.
/// </summary>
public partial class SmDialogContent : ComponentBase, IAsyncDisposable
{
    [Inject]
    private DialogJsInterop DialogInterop { get; set; } = default!;

    [Inject]
    private FloatingJsInterop FloatingInterop { get; set; } = default!;

    [CascadingParameter]
    private DialogContext Context { get; set; } = default!;

    /// <summary>
    /// Child content of the dialog.
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
    /// Behavior when Escape key is pressed.
    /// </summary>
    [Parameter]
    public EscapeKeyBehavior EscapeKeyBehavior { get; set; } = EscapeKeyBehavior.Close;

    /// <summary>
    /// Behavior when clicking outside the dialog content.
    /// Note: This is typically handled by DialogOverlay, but this provides
    /// an additional hook for programmatic control.
    /// </summary>
    [Parameter]
    public OutsideClickBehavior OutsideClickBehavior { get; set; } = OutsideClickBehavior.Close;

    /// <summary>
    /// Callback invoked when a click outside the dialog is detected.
    /// </summary>
    [Parameter]
    public EventCallback OnInteractOutside { get; set; }

    /// <summary>
    /// Callback invoked when Escape key is pressed.
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
    private DotNetObjectReference<SmDialogContent>? _dotNetRef;
    private string? _escapeKeyListenerId;
    private bool _isInitialized;
    private bool _isInitializing; // Guard against concurrent OnAfterRenderAsync calls
    private bool _isDisposed;
    private bool _wasOpen;
    private bool _animationWatcherRegistered;
    private bool _scrollLocked;

    private string DataState => Context.IsOpen ? "open" : "closed";

    /// <summary>
    /// CSS custom properties for nested dialog styling.
    /// </summary>
    private string CssVariables =>
        $"--summit-dialog-depth: {Context.Depth}; --summit-dialog-nested-count: {Context.NestedOpenCount};";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!RendererInfo.IsInteractive) return;

        if (Context.IsOpen && !_isInitialized && !_isInitializing)
        {
            // Set guard flag immediately to prevent concurrent initialization
            _isInitializing = true;

            try
            {
                // Cancel any pending animation watcher if reopening
                if (Context.IsContentAnimatingClosed)
                {
                    await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
                    Context.IsContentAnimatingClosed = false;
                }
                _animationWatcherRegistered = false;

                Context.RegisterContent(_elementRef);
                _dotNetRef ??= DotNetObjectReference.Create(this);

                // Lock scroll if enabled
                if (PreventScroll && !_scrollLocked)
                {
                    await DialogInterop.LockScrollAsync();
                    _scrollLocked = true;
                }

                // Register Escape key handler
                if (EscapeKeyBehavior != EscapeKeyBehavior.Ignore || OnEscapeKeyDown.HasDelegate)
                {
                    _escapeKeyListenerId = await FloatingInterop.RegisterEscapeKeyAsync(
                        _dotNetRef,
                        nameof(HandleEscapeKey));
                }

                _isInitialized = true;

                // Focus the content if not using FocusTrap (FocusTrap handles focus automatically)
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
            // Immediately unregister escape key so parent dialogs can receive escape events
            // This must happen before animations complete to avoid blocking the escape key stack
            if (!string.IsNullOrEmpty(_escapeKeyListenerId))
            {
                await FloatingInterop.UnregisterEscapeKeyAsync(_escapeKeyListenerId);
                _escapeKeyListenerId = null;
            }

            // Start waiting for close animations to complete
            _animationWatcherRegistered = true;
            Context.IsContentAnimatingClosed = true;
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
            // Cleanup Escape key listener
            if (!string.IsNullOrEmpty(_escapeKeyListenerId))
            {
                await FloatingInterop.UnregisterEscapeKeyAsync(_escapeKeyListenerId);
                _escapeKeyListenerId = null;
            }

            // Unlock scroll if we locked it
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

        // Immediately unregister escape key BEFORE closing so parent dialogs 
        // can receive the next escape event without waiting for re-render
        if (!string.IsNullOrEmpty(_escapeKeyListenerId))
        {
            await FloatingInterop.UnregisterEscapeKeyAsync(_escapeKeyListenerId);
            _escapeKeyListenerId = null;
        }

        await OnEscapeKeyDown.InvokeAsync();

        if (EscapeKeyBehavior == EscapeKeyBehavior.Close)
        {
            await Context.CloseAsync();
        }
    }

    /// <summary>
    /// Called from JavaScript when all close animations have completed.
    /// </summary>
    [JSInvokable]
    public async Task OnCloseAnimationsComplete()
    {
        if (_isDisposed) return;

        Context.IsContentAnimatingClosed = false;

        // Only cleanup if still in closed state (user might have reopened during animation)
        if (!Context.IsOpen)
        {
            await CleanupAsync();

            // Return focus to trigger
            await FloatingInterop.FocusElementAsync(Context.TriggerElement);

            await OnCloseAutoFocus.InvokeAsync();
        }

        Context.RaiseStateChanged();
        // Trigger re-render to potentially remove element from DOM
        // (only if overlay is also done animating)
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Cancel any pending animation watcher
        if (Context.IsContentAnimatingClosed)
        {
            try
            {
                await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
            }
            catch (JSDisconnectedException)
            {
                // Ignore
            }
            Context.IsContentAnimatingClosed = false;
        }

        await CleanupAsync();
        _dotNetRef?.Dispose();
    }
}
