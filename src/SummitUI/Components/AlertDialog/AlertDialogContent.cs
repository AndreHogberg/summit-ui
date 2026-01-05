using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
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
public class AlertDialogContent : ComponentBase, IAsyncDisposable
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
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

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
    private bool _isDisposed;
    private bool _wasOpen;
    private bool _animationWatcherRegistered;
    private bool _scrollLocked;

    private string DataState => Context.IsOpen ? "open" : "closed";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Only render when open or during close animation
        if (!Context.IsOpen && !Context.IsAnimatingClosed) return;

        builder.OpenElement(0, As);
        builder.AddAttribute(1, "id", Context.DialogId);
        builder.AddAttribute(2, "role", "alertdialog");
        builder.AddAttribute(3, "aria-modal", TrapFocus.ToString().ToLowerInvariant());
        builder.AddAttribute(4, "aria-labelledby", Context.TitleId);
        builder.AddAttribute(5, "aria-describedby", Context.DescriptionId);
        builder.AddAttribute(6, "data-state", DataState);
        builder.AddAttribute(7, "data-summit-alertdialog-content", true);
        builder.AddAttribute(8, "tabindex", "-1");

        if (Context.Options.IsDestructive)
        {
            builder.AddAttribute(9, "data-destructive", "");
        }

        builder.AddMultipleAttributes(10, AdditionalAttributes);
        builder.AddElementReferenceCapture(11, elementRef => _elementRef = elementRef);

        // Wrap content in FocusTrap if enabled
        if (TrapFocus)
        {
            builder.OpenComponent<FocusTrap>(12);
            builder.AddComponentParameter(13, "IsActive", Context.IsOpen);
            builder.AddComponentParameter(14, "AutoFocus", true);
            builder.AddComponentParameter(15, "ReturnFocus", true);
            // For destructive dialogs, focus the cancel button first
            if (Context.Options.IsDestructive)
            {
                builder.AddComponentParameter(16, "InitialFocusSelector", "[data-summit-alertdialog-cancel]");
            }
            builder.AddComponentParameter(17, "ChildContent", (RenderFragment)(childBuilder => childBuilder.AddContent(0, ChildContent)));
            builder.CloseComponent();
        }
        else
        {
            builder.AddContent(12, ChildContent);
        }

        builder.CloseElement();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!RendererInfo.IsInteractive) return;

        if (Context.IsOpen && !_isInitialized)
        {
            // Cancel any pending animation watcher if reopening
            if (Context.IsAnimatingClosed)
            {
                await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
                Context.IsAnimatingClosed = false;
                _isInitialized = false;
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

            // Register Escape key handler if allowed
            if (Context.Options.AllowEscapeClose || OnEscapeKeyDown.HasDelegate)
            {
                _escapeKeyListenerId = await FloatingInterop.RegisterEscapeKeyAsync(
                    _dotNetRef,
                    nameof(HandleEscapeKey));
            }

            _isInitialized = true;

            // Focus is handled by FocusTrap if enabled
            if (!TrapFocus)
            {
                await FloatingInterop.FocusFirstElementAsync(_elementRef);
            }

            await OnOpenAutoFocus.InvokeAsync();
        }
        else if (!Context.IsOpen && _wasOpen && !_animationWatcherRegistered)
        {
            // Start waiting for close animations to complete
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

        // Only cleanup if still in closed state
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

        // Cancel any pending animation watcher
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
