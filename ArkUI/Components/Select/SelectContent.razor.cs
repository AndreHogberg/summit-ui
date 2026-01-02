using ArkUI.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArkUI.Components.Select;

/// <summary>
/// The floating content panel of the select with positioning logic.
/// Implements listbox role with keyboard navigation support.
/// </summary>
/// <typeparam name="TValue">The type of the select value.</typeparam>
public partial class SelectContent<TValue> : ComponentBase, IAsyncDisposable where TValue : notnull
{
    [Inject]
    private SelectJsInterop JsInterop { get; set; } = default!;

    [CascadingParameter]
    private SelectContext<TValue> Context { get; set; } = default!;

    /// <summary>
    /// Child content of the select (typically SelectViewport with items).
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
    public SelectSide Side { get; set; } = SelectSide.Bottom;

    /// <summary>
    /// Offset from the trigger element in pixels.
    /// </summary>
    [Parameter]
    public int SideOffset { get; set; } = 4;

    /// <summary>
    /// Alignment along the side axis.
    /// </summary>
    [Parameter]
    public SelectAlign Align { get; set; } = SelectAlign.Start;

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
    public SelectEscapeKeyBehavior EscapeKeyBehavior { get; set; } = SelectEscapeKeyBehavior.Close;

    /// <summary>
    /// Behavior when clicking outside the select.
    /// </summary>
    [Parameter]
    public SelectOutsideClickBehavior OutsideClickBehavior { get; set; } = SelectOutsideClickBehavior.Close;

    /// <summary>
    /// Callback invoked when a click outside the select is detected.
    /// </summary>
    [Parameter]
    public EventCallback OnInteractOutside { get; set; }

    /// <summary>
    /// Callback invoked when Escape key is pressed.
    /// </summary>
    [Parameter]
    public EventCallback OnEscapeKeyDown { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private DotNetObjectReference<SelectContent<TValue>>? _dotNetRef;
    private bool _isPositioned;
    private bool _isDisposed;
    private bool _isSubscribed;

    private string DataState => Context.IsOpen ? "open" : "closed";

    // Use visibility: hidden initially to prevent flash in top-left corner before JS positioning
    // JS will set visibility: visible after first position calculation
    private string ContentStyle => "position: absolute; z-index: 50; visibility: hidden;";

    protected override void OnInitialized()
    {
        // Subscribe to context state changes
        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Context.IsOpen && !_isPositioned)
        {
            Context.RegisterContent(_elementRef);
            _dotNetRef ??= DotNetObjectReference.Create(this);

            // Get the string key for the currently selected value
            var selectedKey = Context.GetKeyForValue(Context.Value);

            var options = new SelectPositionOptions
            {
                Side = Side.ToString().ToLowerInvariant(),
                SideOffset = SideOffset,
                Align = Align.ToString().ToLowerInvariant(),
                AlignOffset = AlignOffset,
                AvoidCollisions = AvoidCollisions,
                CollisionPadding = CollisionPadding,
                CloseOnEscape = EscapeKeyBehavior == SelectEscapeKeyBehavior.Close,
                CloseOnOutsideClick = OutsideClickBehavior == SelectOutsideClickBehavior.Close,
                SelectedValue = selectedKey
            };

            await JsInterop.InitializeSelectAsync(
                Context.TriggerElement,
                _elementRef,
                _dotNetRef,
                options);

            _isPositioned = true;
        }
        else if (!Context.IsOpen && _isPositioned)
        {
            await CleanupPositioningAsync();
        }
    }

    private async Task CleanupPositioningAsync()
    {
        if (_isPositioned)
        {
            _isPositioned = false;
            try
            {
                await JsInterop.DestroySelectAsync(_elementRef);
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
    /// Called from JavaScript when an item is selected.
    /// The key is the string key used in data-value attribute.
    /// </summary>
    [JSInvokable]
    public async Task HandleItemSelect(string key, string? label)
    {
        if (_isDisposed) return;

        // Look up the TValue from the key and call SelectItemByKeyAsync
        await Context.SelectItemByKeyAsync(key);
    }

    /// <summary>
    /// Called from JavaScript when an outside click is detected.
    /// </summary>
    [JSInvokable]
    public async Task HandleOutsideClick()
    {
        if (_isDisposed) return;

        await OnInteractOutside.InvokeAsync();

        if (OutsideClickBehavior == SelectOutsideClickBehavior.Close)
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

        if (EscapeKeyBehavior == SelectEscapeKeyBehavior.Close)
        {
            await Context.CloseAsync();
        }
    }

    /// <summary>
    /// Called from JavaScript when the select should close (e.g., Tab key).
    /// </summary>
    [JSInvokable]
    public async Task HandleClose()
    {
        if (_isDisposed) return;

        await Context.CloseAsync();
    }

    /// <summary>
    /// Called from JavaScript when highlighted item changes.
    /// </summary>
    [JSInvokable]
    public async Task HandleHighlightChange(string key)
    {
        if (_isDisposed) return;

        await Context.SetHighlightedKeyAsync(key);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Unsubscribe from context events
        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }

        await CleanupPositioningAsync();
        _dotNetRef?.Dispose();
    }
}
