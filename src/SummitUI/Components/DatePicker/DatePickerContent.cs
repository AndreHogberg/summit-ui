using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// The floating content panel of the date picker (calendar popover).
/// Uses FloatingUI for positioning, anchored to the DatePickerField element.
/// This extends PopoverContent to use the Field element as the anchor instead of the trigger.
/// </summary>
public class DatePickerContent : ComponentBase, IAsyncDisposable
{
    [Inject]
    private FloatingJsInterop FloatingInterop { get; set; } = default!;

    [CascadingParameter]
    private DatePickerContext DatePickerContext { get; set; } = default!;

    [CascadingParameter]
    private PopoverContext Context { get; set; } = default!;

    /// <summary>
    /// Child content of the popover.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// Preferred placement side relative to the field.
    /// </summary>
    [Parameter]
    public Side Side { get; set; } = Side.Bottom;

    /// <summary>
    /// Offset from the field element in pixels.
    /// </summary>
    [Parameter]
    public int SideOffset { get; set; }

    /// <summary>
    /// Alignment along the side axis.
    /// </summary>
    [Parameter]
    public Align Align { get; set; } = Align.Start;

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
    /// Defaults to the value of DatePickerRoot.Modal.
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
    private DotNetObjectReference<DatePickerContent>? _dotNetRef;
    private string? _floatingInstanceId;
    private string? _outsideClickListenerId;
    private string? _escapeKeyListenerId;
    private bool _isPositioned;
    private bool _isPositioning;
    private bool _isDisposed;
    private bool _wasOpen;
    private bool _animationWatcherRegistered;

    private string DataState => Context.IsOpen ? "open" : "closed";

    private bool EffectiveTrapFocus => TrapFocus ?? Context.Modal;

    protected override void OnInitialized()
    {
        // Subscribe to context state changes to handle controlled open/close
        Context.OnStateChanged += HandleContextStateChanged;
    }

    private void HandleContextStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Only render when open or during close animation so CSS animate-out classes can run
        if (!Context.IsOpen && !Context.IsAnimatingClosed) return;

        builder.OpenElement(0, As);
        builder.AddAttribute(1, "id", Context.PopoverId);
        builder.AddAttribute(2, "role", "dialog");
        builder.AddAttribute(3, "aria-modal", EffectiveTrapFocus.ToString().ToLowerInvariant());
        builder.AddAttribute(4, "data-state", DataState);
        builder.AddAttribute(5, "data-side", Side.ToString().ToLowerInvariant());
        builder.AddAttribute(6, "data-align", Align.ToString().ToLowerInvariant());
        builder.AddAttribute(7, "data-summit-datepicker-content", true);
        builder.AddAttribute(8, "tabindex", "-1");
        // Use visibility: hidden initially to prevent flash in top-left corner before JS positioning
        builder.AddAttribute(9, "style", "position: absolute; visibility: hidden;");
        builder.AddMultipleAttributes(10, AdditionalAttributes);
        builder.AddElementReferenceCapture(11, elementRef => _elementRef = elementRef);

        // Wrap content in FocusTrap if enabled
        if (EffectiveTrapFocus)
        {
            builder.OpenComponent<FocusTrap>(12);
            builder.AddComponentParameter(13, "IsActive", Context.IsOpen);
            builder.AddComponentParameter(14, "AutoFocus", true);
            builder.AddComponentParameter(15, "ReturnFocus", true);
            builder.AddComponentParameter(16, "ChildContent", (RenderFragment)(childBuilder => childBuilder.AddContent(0, ChildContent)));
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

        if (Context.IsOpen && !_isPositioned && !_isPositioning)
        {
            _isPositioning = true;

            try
            {
                // Cancel any pending animation watcher if reopening
                if (Context.IsAnimatingClosed)
                {
                    await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
                    Context.IsAnimatingClosed = false;
                }
                _animationWatcherRegistered = false;

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

                // Initialize positioning using the FIELD element as anchor (not trigger)
                // This is the key difference from PopoverContent
                _floatingInstanceId = await FloatingInterop.InitializeAsync(
                    DatePickerContext.FieldElement, // Use field as anchor
                    _elementRef,
                    null, // No arrow element
                    options);

                // Register outside click handler if needed
                if (OutsideClickBehavior != OutsideClickBehavior.Ignore || OnInteractOutside.HasDelegate)
                {
                    _outsideClickListenerId = await FloatingInterop.RegisterOutsideClickAsync(
                        DatePickerContext.FieldElement, // Include field in "inside" check
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

                // Focus the content if not using FocusTrap
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
            if (!string.IsNullOrEmpty(_escapeKeyListenerId))
            {
                await FloatingInterop.UnregisterEscapeKeyAsync(_escapeKeyListenerId);
                _escapeKeyListenerId = null;
            }

            if (!string.IsNullOrEmpty(_outsideClickListenerId))
            {
                await FloatingInterop.UnregisterOutsideClickAsync(_outsideClickListenerId);
                _outsideClickListenerId = null;
            }

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

        // Only cleanup if still in closed state
        if (!Context.IsOpen)
        {
            await CleanupAsync();

            // Return focus to trigger
            await FloatingInterop.FocusElementAsync(Context.TriggerElement);

            await OnCloseAutoFocus.InvokeAsync();
        }

        Context.RaiseStateChanged();
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Unsubscribe from context state changes
        Context.OnStateChanged -= HandleContextStateChanged;

        if (Context.IsAnimatingClosed)
        {
            await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
            Context.IsAnimatingClosed = false;
        }

        await CleanupAsync();
        _dotNetRef?.Dispose();
    }
}
