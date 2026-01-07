using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// The floating content panel of the select with positioning logic.
/// Implements listbox role with keyboard navigation support.
/// All event handling is done in Blazor, with FloatingUI for positioning only.
/// </summary>
/// <typeparam name="TValue">The type of the select value.</typeparam>
public class SelectContent<TValue> : ComponentBase, IAsyncDisposable where TValue : notnull
{
    [Inject]
    private FloatingJsInterop FloatingInterop { get; set; } = default!;

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
    /// Behavior when clicking outside the select.
    /// </summary>
    [Parameter]
    public OutsideClickBehavior OutsideClickBehavior { get; set; } = OutsideClickBehavior.Close;

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
    private string? _floatingInstanceId;
    private string? _outsideClickListenerId;
    private string? _escapeKeyListenerId;
    private bool _isPositioned;
    private bool _isPositioning; // Guard against concurrent OnAfterRenderAsync calls
    private bool _isDisposed;
    private bool _isSubscribed;
    private bool _wasOpen;
    private bool _animationWatcherRegistered;

    // Typeahead state
    private string _typeaheadBuffer = "";
    private System.Timers.Timer? _typeaheadTimer;
    private const int TypeaheadDelay = 500;

    private string DataState => Context.IsOpen ? "open" : "closed";

    // Use visibility: hidden initially to prevent flash in top-left corner before JS positioning
    // JS will set visibility: visible after first position calculation
    private string ContentStyle => "position: absolute; z-index: 50; visibility: hidden; outline: none;";

    protected override void OnInitialized()
    {
        // Subscribe to context state changes
        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;

        // Register focus callback early so it's available when Close is called
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
                    CollisionPadding = CollisionPadding,
                    ConstrainSize = true // Select needs size constraints
                };

                // Initialize positioning
                _floatingInstanceId = await FloatingInterop.InitializeAsync(
                    Context.TriggerElement,
                    _elementRef,
                    null,
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

                // Focus the content so it receives keyboard events
                await FloatingInterop.FocusElementAsync(_elementRef);

                // Highlight selected item or first item
                var selectedKey = Context.GetKeyForValue(Context.Value);
                if (!string.IsNullOrEmpty(selectedKey))
                {
                    await Context.SetHighlightedKeyAsync(selectedKey);
                    // Scroll to the selected item
                    await ScrollToHighlightedItemAsync();
                }
                else
                {
                    // Highlight first item
                    await HighlightFirstItemAsync();
                }
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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Keep element in DOM during close animation so CSS animate-out classes can run
        if (!Context.IsOpen && !Context.IsAnimatingClosed) return;

        builder.OpenElement(0, As);
        builder.AddAttribute(1, "role", "listbox");
        builder.AddAttribute(2, "id", Context.ContentId);
        builder.AddAttribute(3, "tabindex", "-1");
        builder.AddAttribute(4, "aria-labelledby", Context.TriggerId);
        builder.AddAttribute(5, "aria-activedescendant", GetActiveDescendantId());
        builder.AddAttribute(6, "data-state", DataState);
        builder.AddAttribute(7, "data-summit-select-content", "");
        builder.AddAttribute(8, "style", ContentStyle);
        builder.AddMultipleAttributes(9, AdditionalAttributes);
        builder.AddAttribute(10, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        builder.AddEventPreventDefaultAttribute(11, "onkeydown", true);
        builder.AddAttribute(12, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleContentClickAsync));
        builder.AddAttribute(13, "onmouseover", EventCallback.Factory.Create<MouseEventArgs>(this, HandleContentMouseOverAsync));
        builder.AddElementReferenceCapture(14, (elementRef) => { _elementRef = elementRef; });
        builder.AddContent(15, ChildContent);
        builder.CloseElement();
    }

    private string? GetActiveDescendantId()
    {
        return !string.IsNullOrEmpty(Context.HighlightedKey)
            ? Context.GetItemId(Context.HighlightedKey)
            : null;
    }

    private Task HandleContentClickAsync(MouseEventArgs args)
    {
        // Event delegation - find which item was clicked via JS
        // For now, items handle their own clicks via data attributes
        // This is handled by the SelectItem component
        return Task.CompletedTask;
    }

    private Task HandleContentMouseOverAsync(MouseEventArgs args)
    {
        // Event delegation - handled by individual items
        // Mouse over highlighting is done via JS callback or item-level events
        return Task.CompletedTask;
    }

    private async Task CleanupAsync()
    {
        if (!_isPositioned) return;

        _isPositioned = false;

        // Clear typeahead
        ClearTypeahead();

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
            // Root.CloseAsync will focus the trigger before closing
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
            // Note: Focus return to trigger is handled in the close handlers
            // (HandleEscapeKey, SelectHighlightedItemAsync, HandleKeyDownAsync Tab)
            // before Context.CloseAsync() is called, because the component
            // may be unmounted before OnAfterRenderAsync runs.
        }

        // Notify Portal and trigger re-render to remove element from DOM
        Context.RaiseStateChanged();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Handle keyboard navigation in C#.
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
                await HighlightFirstItemAsync();
                break;

            case "End":
                await HighlightLastItemAsync();
                break;

            case "Enter":
            case " ":
                await SelectHighlightedItemAsync();
                break;

            case "Tab":
                // Root.CloseAsync will focus the trigger before closing
                await Context.CloseAsync();
                break;

            default:
                // Handle typeahead for printable characters
                if (args.Key.Length == 1 && !args.CtrlKey && !args.MetaKey && !args.AltKey)
                {
                    HandleTypeahead(args.Key);
                }
                break;
        }
    }

    /// <summary>
    /// Handle item click.
    /// </summary>
    private async Task HandleItemClickAsync(MouseEventArgs args, string key)
    {
        if (_isDisposed) return;

        // Check if item is disabled
        if (IsItemDisabled(key)) return;

        await Context.SelectItemByKeyAsync(key);
    }

    /// <summary>
    /// Handle mouse enter on item.
    /// </summary>
    private async Task HandleItemMouseEnterAsync(string key)
    {
        if (_isDisposed) return;

        // Check if item is disabled
        if (IsItemDisabled(key)) return;

        await Context.SetHighlightedKeyAsync(key);
    }

    private bool IsItemDisabled(string key)
    {
        return Context.DisabledRegistry.TryGetValue(key, out var disabled) && disabled;
    }

    private async Task HighlightNextItemAsync()
    {
        var keys = GetItemKeys();
        if (keys.Count == 0) return;

        var currentIndex = string.IsNullOrEmpty(Context.HighlightedKey)
            ? -1
            : keys.IndexOf(Context.HighlightedKey);

        // Find next non-disabled item, looping around
        var startIndex = currentIndex;
        var nextIndex = currentIndex;
        do
        {
            nextIndex++;
            if (nextIndex >= keys.Count)
            {
                nextIndex = 0; // Loop to start
            }

            // If we've looped all the way around, stop (all items disabled)
            if (nextIndex == startIndex && startIndex != -1)
            {
                return;
            }
        } while (IsItemDisabled(keys[nextIndex]) && nextIndex != currentIndex);

        // Don't highlight if the found item is disabled (all items disabled case)
        if (IsItemDisabled(keys[nextIndex])) return;

        await Context.SetHighlightedKeyAsync(keys[nextIndex]);
        await ScrollToHighlightedItemAsync();
    }

    private async Task HighlightPreviousItemAsync()
    {
        var keys = GetItemKeys();
        if (keys.Count == 0) return;

        var currentIndex = string.IsNullOrEmpty(Context.HighlightedKey)
            ? keys.Count
            : keys.IndexOf(Context.HighlightedKey);

        // Find previous non-disabled item, looping around
        var startIndex = currentIndex;
        var prevIndex = currentIndex;
        do
        {
            prevIndex--;
            if (prevIndex < 0)
            {
                prevIndex = keys.Count - 1; // Loop to end
            }

            // If we've looped all the way around, stop (all items disabled)
            if (prevIndex == startIndex && startIndex != keys.Count)
            {
                return;
            }
        } while (IsItemDisabled(keys[prevIndex]) && prevIndex != currentIndex);

        // Don't highlight if the found item is disabled (all items disabled case)
        if (IsItemDisabled(keys[prevIndex])) return;

        await Context.SetHighlightedKeyAsync(keys[prevIndex]);
        await ScrollToHighlightedItemAsync();
    }

    private async Task HighlightFirstItemAsync()
    {
        var keys = GetItemKeys();
        if (keys.Count == 0) return;

        // Find first non-disabled item
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

    private async Task HighlightLastItemAsync()
    {
        var keys = GetItemKeys();
        if (keys.Count == 0) return;

        // Find last non-disabled item
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

    private async Task SelectHighlightedItemAsync()
    {
        if (string.IsNullOrEmpty(Context.HighlightedKey)) return;

        // Root.CloseAsync will focus the trigger before closing
        await Context.SelectItemByKeyAsync(Context.HighlightedKey);
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
            // Use JS to scroll the highlighted item into view
            await FloatingInterop.ScrollItemIntoViewAsync(_elementRef, Context.HighlightedKey);
        }
        catch (JSDisconnectedException)
        {
            // Ignore
        }
    }

    private List<string> GetItemKeys()
    {
        // Get keys from context registry
        // Note: This returns keys in insertion order (Dictionary preserves insertion order in .NET)
        return Context.ItemRegistry.Keys.ToList();
    }

    private void HandleTypeahead(string character)
    {
        // Clear existing timeout
        _typeaheadTimer?.Stop();
        _typeaheadTimer?.Dispose();

        // Add character to buffer
        _typeaheadBuffer += character.ToLowerInvariant();

        // Find matching item
        var matchingKey = FindMatchingItemKey(_typeaheadBuffer);
        if (matchingKey != null)
        {
            _ = InvokeAsync(async () =>
            {
                await Context.SetHighlightedKeyAsync(matchingKey);
                await ScrollToHighlightedItemAsync();
            });
        }

        // Clear buffer after delay
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

    private string? FindMatchingItemKey(string search)
    {
        foreach (var kvp in Context.LabelRegistry)
        {
            var label = kvp.Value.ToLowerInvariant();
            if (label.StartsWith(search, StringComparison.OrdinalIgnoreCase))
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

        // Cancel any pending animation watcher
        if (Context.IsAnimatingClosed)
        {
            await FloatingInterop.CancelAnimationWatcherAsync(_elementRef);
            Context.IsAnimatingClosed = false;
        }

        // Unsubscribe from context events
        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }

        ClearTypeahead();
        await CleanupAsync();
        _dotNetRef?.Dispose();
    }
}
