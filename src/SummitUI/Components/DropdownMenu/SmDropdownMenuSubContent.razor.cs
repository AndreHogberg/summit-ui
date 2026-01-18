using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// The floating content panel of a submenu with positioning logic.
/// Positioned relative to the SubTrigger element.
/// </summary>
public partial class SmDropdownMenuSubContent : ComponentBase, IAsyncDisposable
{
    [Inject]
    private FloatingJsInterop FloatingInterop { get; set; } = default!;

    [CascadingParameter]
    private DropdownMenuContext MenuContext { get; set; } = default!;

    [CascadingParameter]
    private DropdownMenuSubContext SubContext { get; set; } = default!;

    /// <summary>
    /// Child content of the submenu.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Preferred placement side relative to the sub trigger.
    /// Defaults to Right for LTR, Left for RTL.
    /// </summary>
    [Parameter]
    public Side? Side { get; set; }

    /// <summary>
    /// Offset from the trigger element in pixels.
    /// </summary>
    [Parameter]
    public int SideOffset { get; set; } = 2;

    /// <summary>
    /// Alignment along the side axis.
    /// </summary>
    [Parameter]
    public Align Align { get; set; } = Align.Start;

    /// <summary>
    /// Offset for alignment in pixels.
    /// </summary>
    [Parameter]
    public int AlignOffset { get; set; } = -4;

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
    /// Callback invoked when a click outside the submenu is detected.
    /// </summary>
    [Parameter]
    public EventCallback OnInteractOutside { get; set; }

    /// <summary>
    /// Callback invoked when Escape key is pressed.
    /// </summary>
    [Parameter]
    public EventCallback OnEscapeKeyDown { get; set; }

    /// <summary>
    /// Callback invoked after submenu opens and focuses.
    /// </summary>
    [Parameter]
    public EventCallback OnOpenAutoFocus { get; set; }

    /// <summary>
    /// Callback invoked when submenu closes and returns focus.
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
    private DotNetObjectReference<SmDropdownMenuSubContent>? _dotNetRef;
    private string? _floatingInstanceId;
    private string? _escapeKeyListenerId;
    private bool _isPositioned;
    private bool _isPositioning;
    private bool _isDisposed;
    private bool _wasOpen;
    private bool _animationWatcherRegistered;

    // Typeahead state
    private string _typeaheadBuffer = "";
    private System.Timers.Timer? _typeaheadTimer;
    private const int TypeaheadDelay = 500;

    // Map item IDs to labels for typeahead
    private readonly Dictionary<string, string> _itemLabels = new();
    
    // Items sorted by DOM order for keyboard navigation
    private string[]? _orderedItems;

    private string DataState => SubContext.IsOpen ? "open" : "closed";

    /// <summary>
    /// Computed side based on Dir if not explicitly set.
    /// </summary>
    private Side EffectiveSide => Side ?? (MenuContext.Dir == "rtl" ? SummitUI.Side.Left : SummitUI.Side.Right);

    protected override void OnInitialized()
    {
        // Register label callbacks
        SubContext.RegisterItemLabel = RegisterItemLabel;
        SubContext.UnregisterItemLabel = UnregisterItemLabel;
        SubContext.FocusTriggerAsync = FocusTriggerAsync;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_isDisposed) return;
        if (!RendererInfo.IsInteractive) return;

        if (SubContext.IsOpen && !_isPositioned && !_isDisposed && !_isPositioning)
        {
            _isPositioning = true;

            try
            {
                // Cancel any pending animation watcher if reopening
                if (SubContext.IsAnimatingClosed)
                {
                    await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
                    SubContext.IsAnimatingClosed = false;
                }
                _animationWatcherRegistered = false;

                SubContext.RegisterContent(_elementRef);
                SubContext.HandleKeyDownAsync = HandleKeyFromItemAsync;
                _dotNetRef ??= DotNetObjectReference.Create(this);

                if (_isDisposed)
                {
                    _dotNetRef?.Dispose();
                    _dotNetRef = null;
                    return;
                }

                var options = new FloatingPositionOptions
                {
                    Side = EffectiveSide.ToString().ToLowerInvariant(),
                    SideOffset = SideOffset,
                    Align = Align.ToString().ToLowerInvariant(),
                    AlignOffset = AlignOffset,
                    AvoidCollisions = AvoidCollisions,
                    CollisionPadding = CollisionPadding
                };

                // Initialize positioning relative to sub trigger
                _floatingInstanceId = await FloatingInterop.InitializeAsync(
                    SubContext.TriggerElement,
                    _elementRef,
                    null,
                    options);

                // Register Escape key handler - closes just this submenu
                _escapeKeyListenerId = await FloatingInterop.RegisterEscapeKeyAsync(
                    _dotNetRef,
                    nameof(HandleEscapeKey));

                _isPositioned = true;

                // Wait for child items to register
                // This loop handles the case where OnAfterRenderAsync is called before child components initialize
                var maxAttempts = 10;
                for (var i = 0; i < maxAttempts && SubContext.RegisteredItems.Count == 0; i++)
                {
                    await Task.Delay(10);
                    await Task.Yield();
                }
                
                // Get items in DOM order for keyboard navigation
                await RefreshOrderedItemsAsync();
                
                // Highlight and focus first item
                await HighlightFirstItemAsync();

                await OnOpenAutoFocus.InvokeAsync();
            }
            finally
            {
                _isPositioning = false;
            }
        }
        else if (!SubContext.IsOpen && _wasOpen && !_animationWatcherRegistered)
        {
            _animationWatcherRegistered = true;
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await FloatingInterop.WaitForAnimationsCompleteAsync(
                _elementRef,
                _dotNetRef,
                nameof(OnCloseAnimationsComplete));
        }

        _wasOpen = SubContext.IsOpen;
    }

    private Task HandlePointerEnterAsync(PointerEventArgs args)
    {
        // When pointer enters content, cancel the trigger's close timer
        // This prevents the submenu from closing when moving from trigger to content
        SubContext.CancelPendingClose();
        return Task.CompletedTask;
    }

    private Task HandlePointerLeaveAsync(PointerEventArgs args)
    {
        // Pointer left content - the SubTrigger's close timer will handle closing
        // if pointer doesn't re-enter the trigger or content
        return Task.CompletedTask;
    }

    private async Task CleanupAsync()
    {
        if (!_isPositioned) return;

        _isPositioned = false;
        ClearTypeahead();

        try
        {
            if (!string.IsNullOrEmpty(_escapeKeyListenerId))
            {
                await FloatingInterop.UnregisterEscapeKeyAsync(_escapeKeyListenerId);
                _escapeKeyListenerId = null;
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

    internal void RegisterItemLabel(string itemId, string label)
    {
        _itemLabels[itemId] = label;
    }

    internal void UnregisterItemLabel(string itemId)
    {
        _itemLabels.Remove(itemId);
    }

    /// <summary>
    /// Refresh the cached DOM-ordered items list.
    /// Call this when the submenu opens or when items change.
    /// </summary>
    private async Task RefreshOrderedItemsAsync()
    {
        try
        {
            var registeredIds = SubContext.RegisteredItems.ToArray();
            _orderedItems = await FloatingInterop.GetMenuItemsInDomOrderAsync(_elementRef, registeredIds);
        }
        catch (JSDisconnectedException)
        {
            // Fall back to registration order
            _orderedItems = SubContext.RegisteredItems.ToArray();
        }
    }

    /// <summary>
    /// Get items in DOM order for navigation.
    /// Falls back to registration order if DOM order is not available.
    /// </summary>
    private IReadOnlyList<string> GetOrderedItems()
    {
        if (_orderedItems != null) return _orderedItems;
        return SubContext.RegisteredItems;
    }
    
    /// <summary>
    /// Find the index of an item ID in the list.
    /// </summary>
    private static int IndexOf(IReadOnlyList<string> items, string id)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i] == id) return i;
        }
        return -1;
    }

    /// <summary>
    /// Called from JavaScript when Escape key is pressed.
    /// Closes only this submenu, not the parent menu.
    /// </summary>
    [JSInvokable]
    public async Task HandleEscapeKey()
    {
        if (_isDisposed || !SubContext.IsOpen) return;

        await OnEscapeKeyDown.InvokeAsync();
        await SubContext.CloseAsync();
    }

    /// <summary>
    /// Called from JavaScript when all close animations have completed.
    /// </summary>
    [JSInvokable]
    public async Task OnCloseAnimationsComplete()
    {
        if (_isDisposed) return;

        SubContext.IsAnimatingClosed = false;

        if (!SubContext.IsOpen)
        {
            await CleanupAsync();
            await OnCloseAutoFocus.InvokeAsync();
        }

        SubContext.RaiseStateChanged();
        await InvokeAsync(StateHasChanged);
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        await HandleKeyFromItemAsync(args.Key);
    }

    private async Task HandleKeyFromItemAsync(string key)
    {
        if (_isDisposed || !SubContext.IsOpen) return;

        var isLtr = MenuContext.Dir == "ltr";
        var closeKey = isLtr ? "ArrowLeft" : "ArrowRight";

        switch (key)
        {
            case "ArrowDown":
                await HighlightNextItemAsync();
                break;

            case "ArrowUp":
                await HighlightPreviousItemAsync();
                break;

            case var k when k == closeKey:
                // Close this submenu and focus trigger
                await SubContext.CloseAsync();
                break;

            case "Home":
                await HighlightFirstItemAsync();
                break;

            case "End":
                await HighlightLastItemAsync();
                break;

            case "Enter":
            case " ":
                await ActivateHighlightedItemAsync();
                break;

            case "Tab":
                // Close entire menu tree
                await MenuContext.CloseAsync();
                break;

            default:
                // Handle typeahead for printable characters
                if (key.Length == 1)
                {
                    HandleTypeahead(key);
                }
                break;
        }
    }

    private async Task HighlightNextItemAsync()
    {
        var items = GetOrderedItems();
        if (items.Count == 0) return;

        var currentIndex = string.IsNullOrEmpty(SubContext.HighlightedItemId)
            ? -1
            : IndexOf(items, SubContext.HighlightedItemId);

        var nextIndex = currentIndex + 1;
        if (nextIndex >= items.Count)
        {
            nextIndex = Loop ? 0 : items.Count - 1;
        }

        await SubContext.SetHighlightedItemAsync(items[nextIndex]);
        await FocusHighlightedItemAsync();
    }

    private async Task HighlightPreviousItemAsync()
    {
        var items = GetOrderedItems();
        if (items.Count == 0) return;

        var currentIndex = string.IsNullOrEmpty(SubContext.HighlightedItemId)
            ? items.Count
            : IndexOf(items, SubContext.HighlightedItemId);

        var prevIndex = currentIndex - 1;
        if (prevIndex < 0)
        {
            prevIndex = Loop ? items.Count - 1 : 0;
        }

        await SubContext.SetHighlightedItemAsync(items[prevIndex]);
        await FocusHighlightedItemAsync();
    }

    private async Task HighlightFirstItemAsync()
    {
        var items = GetOrderedItems();
        if (items.Count == 0) return;
        
        await SubContext.SetHighlightedItemAsync(items[0]);
        await FocusHighlightedItemAsync();
    }

    private async Task HighlightLastItemAsync()
    {
        var items = GetOrderedItems();
        if (items.Count == 0) return;

        await SubContext.SetHighlightedItemAsync(items[^1]);
        await FocusHighlightedItemAsync();
    }

    private async Task ActivateHighlightedItemAsync()
    {
        if (!string.IsNullOrEmpty(SubContext.HighlightedItemId))
        {
            try
            {
                await FloatingInterop.ClickElementByIdAsync(SubContext.HighlightedItemId);
            }
            catch (JSDisconnectedException)
            {
                // Ignore
            }
        }
    }

    private async Task FocusHighlightedItemAsync()
    {
        if (string.IsNullOrEmpty(SubContext.HighlightedItemId)) return;

        try
        {
            await FloatingInterop.FocusElementByIdAsync(SubContext.HighlightedItemId);
        }
        catch (JSDisconnectedException)
        {
            // Ignore
        }
    }

    private async Task FocusTriggerAsync()
    {
        try
        {
            await FloatingInterop.FocusElementAsync(SubContext.TriggerElement);
        }
        catch (JSDisconnectedException)
        {
            // Ignore
        }
    }

    private void HandleTypeahead(string character)
    {
        _typeaheadTimer?.Stop();
        _typeaheadTimer?.Dispose();

        _typeaheadBuffer += character.ToLowerInvariant();

        var matchingItemId = FindMatchingItemId(_typeaheadBuffer);
        if (matchingItemId != null)
        {
            _ = InvokeAsync(async () =>
            {
                await SubContext.SetHighlightedItemAsync(matchingItemId);
                await FocusHighlightedItemAsync();
            });
        }

        _typeaheadTimer = new System.Timers.Timer(TypeaheadDelay);
        _typeaheadTimer.Elapsed += (_, _) =>
        {
            _typeaheadBuffer = "";
            _typeaheadTimer?.Dispose();
            _typeaheadTimer = null;
        };
        _typeaheadTimer.AutoReset = false;
        _typeaheadTimer.Start();
    }

    private string? FindMatchingItemId(string search)
    {
        foreach (var kvp in _itemLabels)
        {
            if (kvp.Value.StartsWith(search, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Key;
            }
        }
        return null;
    }

    private void ClearTypeahead()
    {
        _typeaheadBuffer = "";
        _typeaheadTimer?.Stop();
        _typeaheadTimer?.Dispose();
        _typeaheadTimer = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (SubContext.IsAnimatingClosed)
        {
            await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
            SubContext.IsAnimatingClosed = false;
        }

        ClearTypeahead();
        await CleanupAsync();
        _dotNetRef?.Dispose();
    }
}
