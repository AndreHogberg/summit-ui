using SummitUI.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace SummitUI;

/// <summary>
/// The main content panel of the dialog with focus trapping, scroll locking,
/// and keyboard event handling.
/// </summary>
public class DialogContent : ComponentBase, IAsyncDisposable
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
    private DotNetObjectReference<DialogContent>? _dotNetRef;
    private string? _escapeKeyListenerId;
    private bool _isInitialized;
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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Only render when open or during close animation so CSS animate-out classes can run
        if (!Context.IsOpen && !Context.IsAnimatingClosed) return;

        builder.OpenElement(0, As);
        builder.AddAttribute(1, "id", Context.DialogId);
        builder.AddAttribute(2, "role", "dialog");
        builder.AddAttribute(3, "aria-modal", TrapFocus.ToString().ToLowerInvariant());
        builder.AddAttribute(4, "aria-labelledby", Context.TitleId);
        builder.AddAttribute(5, "aria-describedby", Context.DescriptionId);
        builder.AddAttribute(6, "data-state", DataState);
        builder.AddAttribute(7, "data-summit-dialog-content", true);
        builder.AddAttribute(8, "tabindex", "-1");

        // Add nested dialog data attributes
        if (Context.IsNested)
        {
            builder.AddAttribute(9, "data-nested", true);
        }

        if (Context.HasNestedOpen)
        {
            builder.AddAttribute(10, "data-nested-open", true);
        }

        builder.AddAttribute(11, "style", CssVariables);
        builder.AddMultipleAttributes(12, AdditionalAttributes);
        builder.AddElementReferenceCapture(13, elementRef => _elementRef = elementRef);

        // Wrap content in FocusTrap if enabled
        if (TrapFocus)
        {
            builder.OpenComponent<FocusTrap>(14);
            builder.AddComponentParameter(15, "IsActive", Context.IsOpen);
            builder.AddComponentParameter(16, "AutoFocus", true);
            builder.AddComponentParameter(17, "ReturnFocus", true);
            builder.AddComponentParameter(18, "ChildContent", (RenderFragment)(childBuilder => childBuilder.AddContent(0, ChildContent)));
            builder.CloseComponent();
        }
        else
        {
            builder.AddContent(14, ChildContent);
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

        Context.IsAnimatingClosed = false;

        // Only cleanup if still in closed state (user might have reopened during animation)
        if (!Context.IsOpen)
        {
            await CleanupAsync();

            // Return focus to trigger
            await FloatingInterop.FocusElementAsync(Context.TriggerElement);

            await OnCloseAutoFocus.InvokeAsync();
        }

        Context.RaiseStateChanged();
        // Trigger re-render to remove element from DOM now that animation is complete
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
