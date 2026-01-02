using ArkUI.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// The floating content panel of the dropdown menu with positioning logic.
/// </summary>
public partial class DropdownMenuContent : ComponentBase, IAsyncDisposable
{
    [Inject]
    private DropdownMenuJsInterop JsInterop { get; set; } = default!;

    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

    /// <summary>
    /// Child content of the dropdown menu.
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
    public DropdownMenuSide Side { get; set; } = DropdownMenuSide.Bottom;

    /// <summary>
    /// Offset from the trigger element in pixels.
    /// </summary>
    [Parameter]
    public int SideOffset { get; set; } = 4;

    /// <summary>
    /// Alignment along the side axis.
    /// </summary>
    [Parameter]
    public DropdownMenuAlign Align { get; set; } = DropdownMenuAlign.Start;

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
    /// Behavior when Escape key is pressed.
    /// </summary>
    [Parameter]
    public EscapeKeyBehavior EscapeKeyBehavior { get; set; } = EscapeKeyBehavior.Close;

    /// <summary>
    /// Behavior when clicking outside the menu.
    /// </summary>
    [Parameter]
    public OutsideClickBehavior OutsideClickBehavior { get; set; } = OutsideClickBehavior.Close;

    /// <summary>
    /// Callback invoked when a click outside the menu is detected.
    /// </summary>
    [Parameter]
    public EventCallback OnInteractOutside { get; set; }

    /// <summary>
    /// Callback invoked when Escape key is pressed.
    /// </summary>
    [Parameter]
    public EventCallback OnEscapeKeyDown { get; set; }

    /// <summary>
    /// Callback invoked after menu opens and focuses.
    /// </summary>
    [Parameter]
    public EventCallback OnOpenAutoFocus { get; set; }

    /// <summary>
    /// Callback invoked when menu closes and returns focus.
    /// </summary>
    [Parameter]
    public EventCallback OnCloseAutoFocus { get; set; }

    /// <summary>
    /// Whether to loop keyboard navigation.
    /// </summary>
    [Parameter]
    public bool Loop { get; set; } = true;

    /// <summary>
    /// Whether to force mount the content (for animations).
    /// </summary>
    [Parameter]
    public bool ForceMount { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private DotNetObjectReference<DropdownMenuContent>? _dotNetRef;
    private bool _isPositioned;
    private bool _isDisposed;

    private string DataState => Context.IsOpen ? "open" : "closed";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!RendererInfo.IsInteractive) return;

        if (Context.IsOpen && !_isPositioned)
        {
            Context.RegisterContent(_elementRef);
            _dotNetRef ??= DotNetObjectReference.Create(this);

            var options = new DropdownMenuPositionOptions
            {
                Side = Side.ToString().ToLowerInvariant(),
                SideOffset = SideOffset,
                Align = Align.ToString().ToLowerInvariant(),
                AlignOffset = AlignOffset,
                AvoidCollisions = AvoidCollisions,
                CollisionPadding = CollisionPadding,
                CloseOnEscape = EscapeKeyBehavior == EscapeKeyBehavior.Close,
                CloseOnOutsideClick = OutsideClickBehavior == OutsideClickBehavior.Close,
                Loop = Loop
            };

            await JsInterop.InitializeDropdownMenuAsync(
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
            try
            {
                await JsInterop.DestroyDropdownMenuAsync(_elementRef);
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, ignore
            }
            _isPositioned = false;
        }
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        switch (args.Key)
        {
            case "Escape":
                if (EscapeKeyBehavior == EscapeKeyBehavior.Close)
                {
                    await OnEscapeKeyDown.InvokeAsync();
                    await Context.CloseAsync();
                }
                break;
            case "Tab":
                // Close menu on Tab to move focus out
                await Context.CloseAsync();
                break;
        }
    }

    /// <summary>
    /// Called from JavaScript when an outside click is detected.
    /// </summary>
    [JSInvokable]
    public async Task HandleOutsideClick()
    {
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
        await OnEscapeKeyDown.InvokeAsync();

        if (EscapeKeyBehavior == EscapeKeyBehavior.Close)
        {
            await Context.CloseAsync();
        }
    }

    /// <summary>
    /// Called from JavaScript when an item is selected.
    /// </summary>
    [JSInvokable]
    public async Task HandleItemSelect(string value)
    {
        await Context.SelectItemAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        await CleanupPositioningAsync();
        _dotNetRef?.Dispose();
    }
}
