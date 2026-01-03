using SummitUI.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace SummitUI;

/// <summary>
/// The floating content panel of the dropdown menu with positioning logic.
/// All event handling is done in Blazor, with FloatingUI for positioning only.
/// </summary>
public class DropdownMenuContent : ComponentBase, IAsyncDisposable
{
    [Inject]
    private FloatingJsInterop FloatingInterop { get; set; } = default!;

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
    private string? _floatingInstanceId;
    private string? _outsideClickListenerId;
    private string? _escapeKeyListenerId;
    private bool _isPositioned;
    private bool _isDisposed;

    // Typeahead state
    private string _typeaheadBuffer = "";
    private System.Timers.Timer? _typeaheadTimer;
    private const int TypeaheadDelay = 500;

    // Map item IDs to labels for typeahead
    private readonly Dictionary<string, string> _itemLabels = new();

    private string DataState => Context.IsOpen ? "open" : "closed";

    protected override void OnInitialized()
    {
        // Register label callbacks early so items can register during their OnParametersSet
        Context.RegisterItemLabel = RegisterItemLabel;
        Context.UnregisterItemLabel = UnregisterItemLabel;
        Context.FocusTriggerAsync = FocusTriggerAsync;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_isDisposed) return;
        if (!RendererInfo.IsInteractive) return;

        if (Context.IsOpen && !_isPositioned && !_isDisposed)
        {
            Context.RegisterContent(_elementRef);
            Context.HandleKeyDownAsync = HandleKeyFromItemAsync;
            _dotNetRef ??= DotNetObjectReference.Create(this);

            // Check again after creating the reference in case of race condition
            if (_isDisposed)
            {
                _dotNetRef?.Dispose();
                _dotNetRef = null;
                return;
            }

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

            // Highlight and focus first item
            await HighlightFirstItemAsync();
            
            await OnOpenAutoFocus.InvokeAsync();
        }
        else if (!Context.IsOpen && _isPositioned)
        {
            await CleanupAsync();
            // Note: Focus return to trigger is handled in the close handlers
            // (HandleEscapeKey, HandleKeyFromItemAsync Tab) before Context.CloseAsync()
            // is called, because the component may be unmounted before OnAfterRenderAsync runs.
            
            await OnCloseAutoFocus.InvokeAsync();
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!Context.IsOpen && !ForceMount) return;

        builder.OpenElement(0, As);
        builder.AddAttribute(1, "id", Context.MenuId);
        builder.AddAttribute(2, "role", "menu");
        builder.AddAttribute(3, "aria-orientation", "vertical");
        builder.AddAttribute(4, "aria-labelledby", $"{Context.MenuId}-trigger");
        builder.AddAttribute(5, "aria-activedescendant", Context.HighlightedItemId);
        builder.AddAttribute(6, "data-state", DataState);
        builder.AddAttribute(7, "data-side", Side.ToString().ToLowerInvariant());
        builder.AddAttribute(8, "data-align", Align.ToString().ToLowerInvariant());
        builder.AddAttribute(9, "data-ark-dropdown-menu-content", true);
        builder.AddAttribute(10, "tabindex", "-1");
        // Use visibility: hidden initially to prevent flash in top-left corner before JS positioning
        // JS will set visibility: visible after first position calculation
        builder.AddAttribute(11, "style", "position: absolute; top: 0; left: 0; outline: none; visibility: hidden;");
        builder.AddMultipleAttributes(12, AdditionalAttributes);
        builder.AddAttribute(13, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        builder.AddEventPreventDefaultAttribute(14, "onkeydown", true);
        builder.AddEventStopPropagationAttribute(15, "onkeydown", true);
        builder.AddElementReferenceCapture(16, (elementRef) => { _elementRef = elementRef; });
        builder.AddContent(17, ChildContent);
        builder.CloseElement();
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
    /// Register an item's label for typeahead.
    /// </summary>
    internal void RegisterItemLabel(string itemId, string label)
    {
        _itemLabels[itemId] = label;
    }

    /// <summary>
    /// Unregister an item's label.
    /// </summary>
    internal void UnregisterItemLabel(string itemId)
    {
        _itemLabels.Remove(itemId);
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
    /// Handle keyboard navigation in C#.
    /// </summary>
    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        await HandleKeyFromItemAsync(args.Key);
    }

    /// <summary>
    /// Handle keyboard navigation from items (when they have focus).
    /// </summary>
    private async Task HandleKeyFromItemAsync(string key)
    {
        if (_isDisposed || !Context.IsOpen) return;

        switch (key)
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
                await ActivateHighlightedItemAsync();
                break;

            case "Tab":
                // Root.CloseAsync will focus the trigger before closing
                await Context.CloseAsync();
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
        var items = Context.RegisteredItems;
        if (items.Count == 0) return;

        var currentIndex = string.IsNullOrEmpty(Context.HighlightedItemId)
            ? -1
            : items.IndexOf(Context.HighlightedItemId);

        var nextIndex = currentIndex + 1;
        if (nextIndex >= items.Count)
        {
            nextIndex = Loop ? 0 : items.Count - 1;
        }

        await Context.SetHighlightedItemAsync(items[nextIndex]);
        await FocusHighlightedItemAsync();
    }

    private async Task HighlightPreviousItemAsync()
    {
        var items = Context.RegisteredItems;
        if (items.Count == 0) return;

        var currentIndex = string.IsNullOrEmpty(Context.HighlightedItemId)
            ? items.Count
            : items.IndexOf(Context.HighlightedItemId);

        var prevIndex = currentIndex - 1;
        if (prevIndex < 0)
        {
            prevIndex = Loop ? items.Count - 1 : 0;
        }

        await Context.SetHighlightedItemAsync(items[prevIndex]);
        await FocusHighlightedItemAsync();
    }

    private async Task HighlightFirstItemAsync()
    {
        var items = Context.RegisteredItems;
        if (items.Count == 0) return;

        await Context.SetHighlightedItemAsync(items[0]);
        await FocusHighlightedItemAsync();
    }

    private async Task HighlightLastItemAsync()
    {
        var items = Context.RegisteredItems;
        if (items.Count == 0) return;

        await Context.SetHighlightedItemAsync(items[^1]);
        await FocusHighlightedItemAsync();
    }

    private async Task ActivateHighlightedItemAsync()
    {
        // The highlighted item will handle its own click via the context
        // We just need to trigger the activation
        // This is done by simulating a click on the highlighted element via JS
        if (!string.IsNullOrEmpty(Context.HighlightedItemId))
        {
            try
            {
                await FloatingInterop.ClickElementByIdAsync(Context.HighlightedItemId);
            }
            catch (JSDisconnectedException)
            {
                // Ignore
            }
        }
    }

    private async Task ScrollToHighlightedItemAsync()
    {
        if (string.IsNullOrEmpty(Context.HighlightedItemId)) return;

        try
        {
            await FloatingInterop.ScrollElementIntoViewByIdAsync(Context.HighlightedItemId);
        }
        catch (JSDisconnectedException)
        {
            // Ignore
        }
    }

    private async Task FocusHighlightedItemAsync()
    {
        if (string.IsNullOrEmpty(Context.HighlightedItemId)) return;

        try
        {
            await FloatingInterop.FocusElementByIdAsync(Context.HighlightedItemId);
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
            await FloatingInterop.FocusElementAsync(Context.TriggerElement);
        }
        catch (JSDisconnectedException)
        {
            // Ignore
        }
    }

    private void HandleTypeahead(string character)
    {
        // Clear existing timeout
        _typeaheadTimer?.Stop();
        _typeaheadTimer?.Dispose();

        // Add character to buffer
        _typeaheadBuffer += character.ToLowerInvariant();

        // Find matching item
        var matchingItemId = FindMatchingItemId(_typeaheadBuffer);
        if (matchingItemId != null)
        {
            _ = InvokeAsync(async () =>
            {
                await Context.SetHighlightedItemAsync(matchingItemId);
                await FocusHighlightedItemAsync();
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

        ClearTypeahead();
        await CleanupAsync();
        _dotNetRef?.Dispose();
    }
}
