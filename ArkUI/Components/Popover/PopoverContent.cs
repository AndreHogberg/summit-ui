using ArkUI.Components.Utilities;
using ArkUI.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace ArkUI.Components.Popover;

/// <summary>
/// The floating content panel of the popover with positioning logic.
/// Uses FloatingUI for positioning while all event handling is done in Blazor.
/// </summary>
public class PopoverContent : ComponentBase, IAsyncDisposable
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
    private string? _floatingInstanceId;
    private string? _outsideClickListenerId;
    private string? _escapeKeyListenerId;
    private bool _isPositioned;
    private bool _isDisposed;

    private string DataState => Context.IsOpen ? "open" : "closed";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "id", Context.PopoverId);
        builder.AddAttribute(2, "role", "dialog");
        builder.AddAttribute(3, "aria-modal", TrapFocus.ToString().ToLowerInvariant());
        builder.AddAttribute(4, "data-state", DataState);
        builder.AddAttribute(5, "data-side", Side.ToString().ToLowerInvariant());
        builder.AddAttribute(6, "data-align", Align.ToString().ToLowerInvariant());
        builder.AddAttribute(7, "data-ark-popover-content", true);
        builder.AddAttribute(8, "tabindex", "-1");
        // Use visibility: hidden initially to prevent flash in top-left corner before JS positioning
        // JS will set visibility: visible after first position calculation
        builder.AddAttribute(9, "style", "position: absolute; top: 0; left: 0; visibility: hidden;");
        builder.AddMultipleAttributes(10, AdditionalAttributes);
        builder.AddElementReferenceCapture(11, (elementRef) => { _elementRef = elementRef; });
        
        // Wrap content in FocusTrap if enabled
        if (TrapFocus)
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
        if (Context.IsOpen && !_isPositioned)
        {
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
            if (!TrapFocus)
            {
                await FloatingInterop.FocusFirstElementAsync(_elementRef);
            }
            
            await OnOpenAutoFocus.InvokeAsync();
        }
        else if (!Context.IsOpen && _isPositioned)
        {
            await CleanupAsync();
            
            // Return focus to trigger
            await FloatingInterop.FocusElementAsync(Context.TriggerElement);
            
            await OnCloseAutoFocus.InvokeAsync();
        }
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

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        await CleanupAsync();
        _dotNetRef?.Dispose();
    }
}
