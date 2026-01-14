using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// The floating content panel of the combobox with positioning logic.
/// Implements listbox role with keyboard navigation support for multi-select.
/// </summary>
/// <typeparam name="TValue">The type of the combobox value.</typeparam>
public class SmComboboxContent<TValue> : ComponentBase, IAsyncDisposable where TValue : notnull
{
    [Inject]
    private FloatingJsInterop FloatingInterop { get; set; } = default!;

    [CascadingParameter]
    private ComboboxContext<TValue> Context { get; set; } = default!;

    /// <summary>
    /// Child content of the combobox (typically ComboboxViewport with items).
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
    public Side Side { get; set; } = Side.Bottom;

    /// <summary>
    /// Offset from the trigger element in pixels.
    /// </summary>
    [Parameter]
    public int SideOffset { get; set; } = 4;

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
    /// Behavior when Escape key is pressed.
    /// </summary>
    [Parameter]
    public EscapeKeyBehavior EscapeKeyBehavior { get; set; } = EscapeKeyBehavior.Close;

    /// <summary>
    /// Behavior when clicking outside the combobox.
    /// </summary>
    [Parameter]
    public OutsideClickBehavior OutsideClickBehavior { get; set; } = OutsideClickBehavior.Close;

    /// <summary>
    /// Callback invoked when a click outside the combobox is detected.
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
    private DotNetObjectReference<SmComboboxContent<TValue>>? _dotNetRef;
    private string? _floatingInstanceId;
    private string? _outsideClickListenerId;
    private string? _escapeKeyListenerId;
    private bool _isPositioned;
    private bool _isPositioning;
    private bool _isDisposed;
    private bool _isSubscribed;
    private bool _wasOpen;
    private bool _animationWatcherRegistered;

    private string DataState => Context.IsOpen ? "open" : "closed";

    // Use visibility: hidden initially to prevent flash in top-left corner before JS positioning
    private string ContentStyle => "position: absolute; z-index: 50; visibility: hidden; outline: none;";

    protected override void OnInitialized()
    {
        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;

        // Register focus callback early
        Context.FocusTriggerAsync = FocusTriggerAsync;
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!RendererInfo.IsInteractive) return;

        if (Context.IsOpen && !_isPositioned && !_isPositioning)
        {
            _isPositioning = true;

            try
            {
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
                    CollisionPadding = CollisionPadding,
                    ConstrainSize = true
                };

                // Initialize positioning - use trigger element as reference
                _floatingInstanceId = await FloatingInterop.InitializeAsync(
                    Context.TriggerElement,
                    _elementRef,
                    null,
                    options);

                // Register outside click handler
                if (OutsideClickBehavior != OutsideClickBehavior.Ignore || OnInteractOutside.HasDelegate)
                {
                    // Include both trigger and input elements in the "inside" check
                    _outsideClickListenerId = await FloatingInterop.RegisterOutsideClickAsync(
                        Context.TriggerElement,
                        _elementRef,
                        _dotNetRef,
                        nameof(HandleOutsideClick));
                }

                // Register Escape key handler (only if no input, as input handles its own Escape)
                if (!Context.HasInput && (EscapeKeyBehavior != EscapeKeyBehavior.Ignore || OnEscapeKeyDown.HasDelegate))
                {
                    _escapeKeyListenerId = await FloatingInterop.RegisterEscapeKeyAsync(
                        _dotNetRef,
                        nameof(HandleEscapeKey));
                }

                _isPositioned = true;

                // Focus the content only if there's no input (select-only mode)
                if (!Context.HasInput)
                {
                    await FloatingInterop.FocusElementAsync(_elementRef);
                }

                // Highlight first visible item
                await HighlightFirstVisibleItemAsync();
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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!Context.IsOpen && !Context.IsAnimatingClosed) return;

        builder.OpenElement(0, As);
        builder.AddAttribute(1, "role", "listbox");
        builder.AddAttribute(2, "id", Context.ContentId);
        builder.AddAttribute(3, "tabindex", "-1");
        builder.AddAttribute(4, "aria-multiselectable", "true");
        builder.AddAttribute(5, "aria-labelledby", Context.HasInput ? Context.InputId : Context.TriggerId);
        builder.AddAttribute(6, "aria-activedescendant", GetActiveDescendantId());
        builder.AddAttribute(7, "data-state", DataState);
        builder.AddAttribute(8, "data-summit-combobox-content", "");
        builder.AddAttribute(9, "style", ContentStyle);
        builder.AddMultipleAttributes(10, AdditionalAttributes);

        // Only handle keyboard events if there's no input (select-only mode)
        if (!Context.HasInput)
        {
            builder.AddAttribute(11, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
            builder.AddEventPreventDefaultAttribute(12, "onkeydown", true);
        }

        builder.AddElementReferenceCapture(13, (elementRef) => { _elementRef = elementRef; });
        builder.AddContent(14, ChildContent);
        builder.CloseElement();
    }

    private string? GetActiveDescendantId()
    {
        return !string.IsNullOrEmpty(Context.HighlightedKey)
            ? Context.GetItemId(Context.HighlightedKey)
            : null;
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

        if (!Context.IsOpen)
        {
            await CleanupAsync();
        }

        Context.RaiseStateChanged();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Handle keyboard navigation (only used in select-only mode without input).
    /// </summary>
    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (_isDisposed || !Context.IsOpen) return;

        switch (args.Key)
        {
            case "ArrowDown":
                await HighlightNextItemAsync();
                break;

            case "ArrowUp":
                await HighlightPreviousItemAsync();
                break;

            case "Home":
                await HighlightFirstVisibleItemAsync();
                break;

            case "End":
                await HighlightLastVisibleItemAsync();
                break;

            case "Enter":
            case " ":
                await ToggleHighlightedItemAsync();
                break;

            case "Tab":
                await Context.CloseAsync();
                break;
        }
    }

    private async Task HighlightNextItemAsync()
    {
        var keys = GetVisibleItemKeys();
        if (keys.Count == 0) return;

        var currentIndex = string.IsNullOrEmpty(Context.HighlightedKey)
            ? -1
            : keys.IndexOf(Context.HighlightedKey);

        var startIndex = currentIndex;
        var nextIndex = currentIndex;
        do
        {
            nextIndex++;
            if (nextIndex >= keys.Count)
            {
                nextIndex = 0;
            }
            if (nextIndex == startIndex && startIndex != -1) return;
        } while (IsItemDisabled(keys[nextIndex]) && nextIndex != currentIndex);

        if (IsItemDisabled(keys[nextIndex])) return;

        await Context.SetHighlightedKeyAsync(keys[nextIndex]);
        await ScrollToHighlightedItemAsync();
    }

    private async Task HighlightPreviousItemAsync()
    {
        var keys = GetVisibleItemKeys();
        if (keys.Count == 0) return;

        var currentIndex = string.IsNullOrEmpty(Context.HighlightedKey)
            ? keys.Count
            : keys.IndexOf(Context.HighlightedKey);

        var startIndex = currentIndex;
        var prevIndex = currentIndex;
        do
        {
            prevIndex--;
            if (prevIndex < 0)
            {
                prevIndex = keys.Count - 1;
            }
            if (prevIndex == startIndex && startIndex != keys.Count) return;
        } while (IsItemDisabled(keys[prevIndex]) && prevIndex != currentIndex);

        if (IsItemDisabled(keys[prevIndex])) return;

        await Context.SetHighlightedKeyAsync(keys[prevIndex]);
        await ScrollToHighlightedItemAsync();
    }

    private async Task HighlightFirstVisibleItemAsync()
    {
        var keys = GetVisibleItemKeys();
        for (var i = 0; i < keys.Count; i++)
        {
            if (!IsItemDisabled(keys[i]))
            {
                await Context.SetHighlightedKeyAsync(keys[i]);
                await ScrollToHighlightedItemAsync();
                return;
            }
        }
    }

    private async Task HighlightLastVisibleItemAsync()
    {
        var keys = GetVisibleItemKeys();
        for (var i = keys.Count - 1; i >= 0; i--)
        {
            if (!IsItemDisabled(keys[i]))
            {
                await Context.SetHighlightedKeyAsync(keys[i]);
                await ScrollToHighlightedItemAsync();
                return;
            }
        }
    }

    private async Task ToggleHighlightedItemAsync()
    {
        if (string.IsNullOrEmpty(Context.HighlightedKey)) return;

        await Context.ToggleItemByKeyAsync(Context.HighlightedKey);
    }

    private async Task FocusTriggerAsync()
    {
        try
        {
            await FloatingInterop.FocusElementAsync(Context.TriggerElement);
        }
        catch (JSDisconnectedException)
        {
            // Ignore
        }
    }

    private async Task ScrollToHighlightedItemAsync()
    {
        if (string.IsNullOrEmpty(Context.HighlightedKey)) return;

        try
        {
            await FloatingInterop.ScrollItemIntoViewAsync(_elementRef, Context.HighlightedKey);
        }
        catch (JSDisconnectedException)
        {
            // Ignore
        }
    }

    private List<string> GetVisibleItemKeys()
    {
        return Context.ItemRegistry.Keys
            .Where(key => Context.MatchesFilter(key))
            .ToList();
    }

    private bool IsItemDisabled(string key)
    {
        return Context.DisabledRegistry.TryGetValue(key, out var disabled) && disabled;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (Context.IsAnimatingClosed)
        {
            await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
            Context.IsAnimatingClosed = false;
        }

        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }

        await CleanupAsync();
        _dotNetRef?.Dispose();
    }
}
