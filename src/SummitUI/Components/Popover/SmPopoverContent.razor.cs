using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// The floating content panel of the popover with positioning logic.
/// Uses FloatingUI for positioning while all event handling is done in Blazor.
/// </summary>
public partial class SmPopoverContent : IAsyncDisposable
{
    [Inject]
    private FloatingJsInterop FloatingInterop { get; set; } = default!;

    [CascadingParameter]
    private PopoverContext Context { get; set; } = default!;

    /// <summary>
    /// Child content of the popover.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Preferred placement side relative to the trigger.
    /// </summary>
    [Parameter]
    public Side Side { get; set; } = Side.Bottom;

    /// <summary>
    /// Offset from the trigger element in pixels.
    /// </summary>
    [Parameter]
    public int SideOffset { get; set; }

    /// <summary>
    /// Alignment along the side axis.
    /// </summary>
    [Parameter]
    public Align Align { get; set; } = Align.Center;

    /// <summary>
    /// Offset for alignment in pixels.
    /// </summary>
    [Parameter]
    public int AlignOffset { get; set; }

    /// <summary>
    /// Whether to avoid collisions with viewport boundaries.
    /// </summary>
    [Parameter]
    public bool AvoidCollisions { get; set; } = true;

    /// <summary>
    /// Padding from viewport edges for collision detection.
    /// </summary>
    [Parameter]
    public int CollisionPadding { get; set; } = 8;

    /// <summary>
    /// Whether to trap focus within the popover content.
    /// Defaults to the value of PopoverRoot.Modal.
    /// </summary>
    [Parameter]
    public bool? TrapFocus { get; set; }

    /// <summary>
    /// Behavior when Escape key is pressed.
    /// </summary>
    [Parameter]
    public EscapeKeyBehavior EscapeKeyBehavior { get; set; } = EscapeKeyBehavior.Close;

    /// <summary>
    /// Behavior when clicking outside the popover.
    /// </summary>
    [Parameter]
    public OutsideClickBehavior OutsideClickBehavior { get; set; } = OutsideClickBehavior.Close;

    /// <summary>
    /// Callback invoked when a click outside the popover is detected.
    /// </summary>
    [Parameter]
    public EventCallback OnInteractOutside { get; set; }

    /// <summary>
    /// Callback invoked when Escape key is pressed.
    /// </summary>
    [Parameter]
    public EventCallback OnEscapeKeyDown { get; set; }

    /// <summary>
    /// Callback invoked after popover opens and focuses.
    /// </summary>
    [Parameter]
    public EventCallback OnOpenAutoFocus { get; set; }

    /// <summary>
    /// Callback invoked when popover closes and returns focus.
    /// </summary>
    [Parameter]
    public EventCallback OnCloseAutoFocus { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private DotNetObjectReference<SmPopoverContent>? _dotNetRef;
    private string? _floatingInstanceId;
    private string? _outsideClickListenerId;
    private string? _escapeKeyListenerId;
    private bool _isPositioned;
    private bool _isPositioning; // Guard against concurrent OnAfterRenderAsync calls
    private bool _isDisposed;
    private bool _wasOpen;
    private bool _animationWatcherRegistered;

    private string DataState => Context.IsOpen ? "open" : "closed";

    private bool EffectiveTrapFocus => TrapFocus ?? Context.Modal;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!RendererInfo.IsInteractive) return;

        if (Context.IsOpen && !_isPositioned && !_isPositioning)
        {
            // Set guard flag immediately to prevent concurrent initialization
            _isPositioning = true;

            try
            {
                // Cancel any pending animation watcher if reopening
                if (Context.IsAnimatingClosed)
                {
                    await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
                    Context.IsAnimatingClosed = false;
                }
                _animationWatcherRegistered = false; // Reset for next close cycle

                Context.RegisterContent(_elementRef);
                _dotNetRef ??= DotNetObjectReference.Create(this);

                var options = new FloatingPositionOptions
                {
                    Side = Side.ToString().ToLowerInvariant(),
                    SideOffset = SideOffset,
                    Align = Align.ToString().ToLowerInvariant(),
                    AlignOffset = AlignOffset,
                    AvoidCollisions = AvoidCollisions,
                    CollisionPadding = CollisionPadding
                };

                // Initialize positioning
                _floatingInstanceId = await FloatingInterop.InitializeAsync(
                    Context.TriggerElement,
                    _elementRef,
                    null, // No arrow element for now
                    options);

                // Register outside click handler if needed
                if (OutsideClickBehavior != OutsideClickBehavior.Ignore || OnInteractOutside.HasDelegate)
                {
                    _outsideClickListenerId = await FloatingInterop.RegisterOutsideClickAsync(
                        Context.TriggerElement,
                        _elementRef,
                        _dotNetRef,
                        nameof(HandleOutsideClick));
                }

                // Register Escape key handler if needed
                if (EscapeKeyBehavior != EscapeKeyBehavior.Ignore || OnEscapeKeyDown.HasDelegate)
                {
                    _escapeKeyListenerId = await FloatingInterop.RegisterEscapeKeyAsync(
                        _dotNetRef,
                        nameof(HandleEscapeKey));
                }

                _isPositioned = true;

                // Focus the content if not using FocusTrap (FocusTrap handles focus automatically)
                if (!EffectiveTrapFocus)
                {
                    await FloatingInterop.FocusFirstElementAsync(_elementRef);
                }

                await OnOpenAutoFocus.InvokeAsync();
            }
            finally
            {
                _isPositioning = false;
            }
        }
        else if (!Context.IsOpen && _wasOpen && !_animationWatcherRegistered)
        {
            // Start waiting for close animations to complete
            // Note: Context.IsAnimatingClosed is already set by Root.CloseAsync
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
        if (!_isPositioned) return;

        _isPositioned = false;

        try
        {
            // Cleanup Escape key listener
            if (!string.IsNullOrEmpty(_escapeKeyListenerId))
            {
                await FloatingInterop.UnregisterEscapeKeyAsync(_escapeKeyListenerId);
                _escapeKeyListenerId = null;
            }

            // Cleanup outside click listener
            if (!string.IsNullOrEmpty(_outsideClickListenerId))
            {
                await FloatingInterop.UnregisterOutsideClickAsync(_outsideClickListenerId);
                _outsideClickListenerId = null;
            }

            // Cleanup floating positioning
            if (!string.IsNullOrEmpty(_floatingInstanceId))
            {
                await FloatingInterop.DestroyAsync(_floatingInstanceId);
                _floatingInstanceId = null;
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
    /// Called from JavaScript when an outside click is detected.
    /// </summary>
    [JSInvokable]
    public async Task HandleOutsideClick()
    {
        if (_isDisposed || !Context.IsOpen) return;

        await OnInteractOutside.InvokeAsync();

        if (OutsideClickBehavior == OutsideClickBehavior.Close)
        {
            await Context.CloseAsync();
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
            await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
            Context.IsAnimatingClosed = false;
        }

        await CleanupAsync();
        _dotNetRef?.Dispose();
    }
}
