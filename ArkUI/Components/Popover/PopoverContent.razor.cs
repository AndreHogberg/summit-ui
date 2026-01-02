using ArkUI.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArkUI.Components.Popover;

/// <summary>
/// The floating content panel of the popover with positioning logic.
/// </summary>
public partial class PopoverContent : ComponentBase, IAsyncDisposable
{
    [Inject]
    private PopoverJsInterop JsInterop { get; set; } = default!;

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
    /// Preferred placement side relative to the trigger.
    /// </summary>
    [Parameter]
    public PopoverSide Side { get; set; } = PopoverSide.Bottom;

    /// <summary>
    /// Offset from the trigger element in pixels.
    /// </summary>
    [Parameter]
    public int SideOffset { get; set; }

    /// <summary>
    /// Alignment along the side axis.
    /// </summary>
    [Parameter]
    public PopoverAlign Align { get; set; } = PopoverAlign.Center;

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
    /// </summary>
    [Parameter]
    public bool TrapFocus { get; set; }

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
    private DotNetObjectReference<PopoverContent>? _dotNetRef;
    private bool _isPositioned;
    private bool _isDisposed;

    private string DataState => Context.IsOpen ? "open" : "closed";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Context.IsOpen && !_isPositioned)
        {
            Context.RegisterContent(_elementRef);
            _dotNetRef ??= DotNetObjectReference.Create(this);

            var options = new PopoverPositionOptions
            {
                Side = Side.ToString().ToLowerInvariant(),
                SideOffset = SideOffset,
                Align = Align.ToString().ToLowerInvariant(),
                AlignOffset = AlignOffset,
                AvoidCollisions = AvoidCollisions,
                CollisionPadding = CollisionPadding,
                TrapFocus = TrapFocus,
                CloseOnEscape = EscapeKeyBehavior == EscapeKeyBehavior.Close,
                CloseOnOutsideClick = OutsideClickBehavior == OutsideClickBehavior.Close
            };

            await JsInterop.InitializePopoverAsync(
                Context.TriggerElement,
                _elementRef,
                Context.ArrowElement,
                _dotNetRef,
                options);

            _isPositioned = true;
            await OnOpenAutoFocus.InvokeAsync();
        }
        else if (!Context.IsOpen && _isPositioned)
        {
            await CleanupPositioningAsync();
            await OnCloseAutoFocus.InvokeAsync();
        }
    }

    private async Task CleanupPositioningAsync()
    {
        if (_isPositioned)
        {
            _isPositioned = false;
            try
            {
                await JsInterop.DestroyPopoverAsync(_elementRef);
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
    }

    /// <summary>
    /// Called from JavaScript when an outside click is detected.
    /// </summary>
    [JSInvokable]
    public async Task HandleOutsideClick()
    {
        if (_isDisposed) return;

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
        if (_isDisposed) return;

        await OnEscapeKeyDown.InvokeAsync();

        if (EscapeKeyBehavior == EscapeKeyBehavior.Close)
        {
            await Context.CloseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        await CleanupPositioningAsync();
        _dotNetRef?.Dispose();
    }
}
