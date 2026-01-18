using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// A menu item that opens a submenu when hovered or activated via keyboard.
/// Must be rendered inside DropdownMenuSub.
/// </summary>
public partial class SmDropdownMenuSubTrigger : ComponentBase, IDisposable, IAsyncDisposable
{
    [Inject]
    private DropdownMenuJsInterop JsInterop { get; set; } = default!;

    [CascadingParameter]
    private DropdownMenuContext MenuContext { get; set; } = default!;

    [CascadingParameter]
    private DropdownMenuSubContext SubContext { get; set; } = default!;

    /// <summary>
    /// Gets the parent submenu context (if this is a nested submenu) from SubContext.
    /// Returns null for first-level submenus (direct children of root menu).
    /// </summary>
    private DropdownMenuSubContext? ParentSubContext => SubContext.ParentSubContext;

    /// <summary>
    /// Whether this trigger is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Text value for typeahead search. If not provided, typeahead won't work for this item.
    /// </summary>
    [Parameter]
    public string? TextValue { get; set; }

    /// <summary>
    /// Child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private DotNetObjectReference<SmDropdownMenuSubTrigger>? _dotNetRef;
    private string _itemId = "";
    private bool _isSubscribed;
    private string? _registeredTextValue;
    private bool _isDisposed;
    private bool _isHoverInitialized;
    private System.Threading.Timer? _hoverOpenTimer;
    private System.Threading.Timer? _hoverCloseTimer;

    // Hover delay configuration (in milliseconds)
    private const int OpenDelay = 100;
    private const int CloseDelay = 300;

    /// <summary>
    /// Whether this trigger's submenu is open.
    /// </summary>
    private bool IsOpen => SubContext.IsOpen;

    /// <summary>
    /// Whether this item is currently highlighted in the parent menu.
    /// </summary>
    private bool IsHighlighted
    {
        get
        {
            // Check if highlighted in parent submenu context or root menu context
            if (ParentSubContext != null)
            {
                return ParentSubContext.HighlightedItemId == _itemId;
            }
            return MenuContext.HighlightedItemId == _itemId;
        }
    }

    private string DataState => IsOpen ? "open" : "closed";

    protected override void OnInitialized()
    {
        _itemId = $"{MenuContext.MenuId}-subtrigger-{Guid.NewGuid():N}";

        // Store trigger ID in SubContext for aria-labelledby on SubContent
        SubContext.TriggerId = _itemId;

        // Register with parent context (root menu or parent submenu)
        if (!Disabled)
        {
            if (ParentSubContext != null)
            {
                ParentSubContext.RegisterItem(_itemId);
            }
            else
            {
                MenuContext.RegisterItem(_itemId);
            }
        }

        // Subscribe to state changes from both contexts
        MenuContext.OnStateChanged += HandleStateChanged;
        SubContext.OnStateChanged += HandleStateChanged;
        if (ParentSubContext != null)
        {
            ParentSubContext.OnStateChanged += HandleStateChanged;
        }
        _isSubscribed = true;

        // Register this trigger with the submenu context
        SubContext.RegisterTrigger(_elementRef);
        
        // Register the cancel pending close callback
        SubContext.CancelPendingClose = CancelCloseTimer;
    }

    protected override void OnParametersSet()
    {
        // Register/update text value for typeahead if changed
        if (!Disabled && TextValue != _registeredTextValue)
        {
            if (_registeredTextValue != null)
            {
                if (ParentSubContext != null)
                {
                    ParentSubContext.UnregisterItemLabel(_itemId);
                }
                else
                {
                    MenuContext.UnregisterItemLabel(_itemId);
                }
            }

            if (!string.IsNullOrEmpty(TextValue))
            {
                if (ParentSubContext != null)
                {
                    ParentSubContext.RegisterItemLabel(_itemId, TextValue);
                }
                else
                {
                    MenuContext.RegisterItemLabel(_itemId, TextValue);
                }
            }

            _registeredTextValue = TextValue;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_isDisposed) return;
        if (!RendererInfo.IsInteractive) return;

        if (firstRender)
        {
            SubContext.RegisterTrigger(_elementRef);
            _dotNetRef = DotNetObjectReference.Create(this);

            // Initialize hover intent via JS
            await JsInterop.InitializeSubTriggerAsync(_elementRef, _dotNetRef, OpenDelay, CloseDelay);
            _isHoverInitialized = true;
        }
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    private Task HandleClickAsync(MouseEventArgs args)
    {
        if (Disabled) return Task.CompletedTask;

        // Click toggles the submenu
        if (IsOpen)
        {
            return SubContext.CloseAsync();
        }
        return SubContext.OpenAsync();
    }

    private async Task HandlePointerEnterAsync(PointerEventArgs args)
    {
        if (Disabled) return;

        // Cancel any pending close timer on this trigger
        CancelCloseTimer();

        // Also cancel the parent trigger's close timer (if nested)
        // This handles the case where pointer moves from parent trigger to this (child) trigger
        ParentSubContext?.CancelPendingClose();

        // Highlight this item in the parent menu
        if (ParentSubContext != null)
        {
            await ParentSubContext.SetHighlightedItemAsync(_itemId);
        }
        else
        {
            await MenuContext.SetHighlightedItemAsync(_itemId);
        }

        // Start open timer
        StartOpenTimer();
    }

    private Task HandlePointerLeaveAsync(PointerEventArgs args)
    {
        if (Disabled) return Task.CompletedTask;

        // Cancel any pending open timer
        CancelOpenTimer();

        // Start close timer (will be cancelled if pointer enters content)
        StartCloseTimer();

        return Task.CompletedTask;
    }

    private Task HandlePointerMoveAsync(PointerEventArgs args)
    {
        // Cancel close timer on move within trigger
        CancelCloseTimer();
        return Task.CompletedTask;
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (Disabled) return;

        var isLtr = MenuContext.Dir == "ltr";
        var openKey = isLtr ? "ArrowRight" : "ArrowLeft";
        var closeKey = isLtr ? "ArrowLeft" : "ArrowRight";

        switch (args.Key)
        {
            case var key when key == openKey:
                // Open submenu and focus first item
                await SubContext.OpenAsync();
                break;

            case "Enter":
            case " ":
                // Open submenu on Enter/Space
                await SubContext.OpenAsync();
                break;

            default:
                // Delegate other keys to parent menu for navigation
                if (ParentSubContext != null)
                {
                    await ParentSubContext.HandleKeyDownAsync(args.Key);
                }
                else
                {
                    await MenuContext.HandleKeyDownAsync(args.Key);
                }
                break;
        }
    }

    /// <summary>
    /// Called from JavaScript when hover intent triggers open.
    /// </summary>
    [JSInvokable]
    public async Task OnHoverIntentOpen()
    {
        if (_isDisposed || Disabled) return;
        await SubContext.OpenAsync();
    }

    /// <summary>
    /// Called from JavaScript when hover intent triggers close.
    /// </summary>
    [JSInvokable]
    public async Task OnHoverIntentClose()
    {
        if (_isDisposed || Disabled) return;
        await SubContext.CloseAsync();
    }

    /// <summary>
    /// Called from SubContent to cancel the close timer when pointer enters content.
    /// </summary>
    public void CancelPendingClose()
    {
        CancelCloseTimer();
    }

    private void StartOpenTimer()
    {
        CancelOpenTimer();
        _hoverOpenTimer = new System.Threading.Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                if (!_isDisposed && !Disabled && !IsOpen)
                {
                    await SubContext.OpenAsync();
                }
            });
        }, null, OpenDelay, Timeout.Infinite);
    }

    private void StartCloseTimer()
    {
        CancelCloseTimer();
        _hoverCloseTimer = new System.Threading.Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                if (!_isDisposed && !Disabled && IsOpen)
                {
                    await SubContext.CloseAsync();
                }
            });
        }, null, CloseDelay, Timeout.Infinite);
    }

    private void CancelOpenTimer()
    {
        _hoverOpenTimer?.Dispose();
        _hoverOpenTimer = null;
    }

    private void CancelCloseTimer()
    {
        _hoverCloseTimer?.Dispose();
        _hoverCloseTimer = null;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        CancelOpenTimer();
        CancelCloseTimer();

        if (_isSubscribed)
        {
            MenuContext.OnStateChanged -= HandleStateChanged;
            SubContext.OnStateChanged -= HandleStateChanged;
            if (ParentSubContext != null)
            {
                ParentSubContext.OnStateChanged -= HandleStateChanged;
            }
        }

        if (!Disabled)
        {
            if (ParentSubContext != null)
            {
                ParentSubContext.UnregisterItem(_itemId);
            }
            else
            {
                MenuContext.UnregisterItem(_itemId);
            }
        }

        if (_registeredTextValue != null)
        {
            if (ParentSubContext != null)
            {
                ParentSubContext.UnregisterItemLabel(_itemId);
            }
            else
            {
                MenuContext.UnregisterItemLabel(_itemId);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;

        try
        {
            if (_isHoverInitialized)
            {
                await JsInterop.DestroySubTriggerAsync(_elementRef);
            }
        }
        catch (JSDisconnectedException)
        {
            // Safe to ignore
        }

        _dotNetRef?.Dispose();
        Dispose();
    }
}
