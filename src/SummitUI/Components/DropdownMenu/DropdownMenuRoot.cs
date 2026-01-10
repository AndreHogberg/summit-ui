using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// Root component that manages the state of the dropdown menu.
/// Provides cascading context to child components.
/// </summary>
public class DropdownMenuRoot : ComponentBase, IAsyncDisposable
{
    [Inject] private DropdownMenuJsInterop JsInterop { get; set; } = default!;

    /// <summary>
    /// Child content containing DropdownMenuTrigger, DropdownMenuContent, etc.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Controlled open state. When provided, component operates in controlled mode.
    /// </summary>
    [Parameter]
    public bool? Open { get; set; }

    /// <summary>
    /// Default open state for uncontrolled mode.
    /// </summary>
    [Parameter]
    public bool DefaultOpen { get; set; }

    /// <summary>
    /// Callback when open state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Callback invoked when menu opens.
    /// </summary>
    [Parameter]
    public EventCallback OnOpen { get; set; }

    /// <summary>
    /// Callback invoked when menu closes.
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// Whether the dropdown menu is modal (traps focus when open).
    /// </summary>
    [Parameter]
    public bool Modal { get; set; } = true;

    /// <summary>
    /// The reading direction. Used for submenu arrow key navigation.
    /// </summary>
    [Parameter]
    public string Dir { get; set; } = "ltr";

    private readonly DropdownMenuContext _context = new();
    private bool _internalOpen;
    private bool _isDisposed;

    /// <summary>
    /// Effective open state (controlled or uncontrolled).
    /// </summary>
    private bool IsOpen => Open ?? _internalOpen;

    protected override void OnInitialized()
    {
        _internalOpen = DefaultOpen;
        _context.IsOpen = IsOpen;
        _context.Dir = Dir;
        _context.ToggleAsync = ToggleAsync;
        _context.OpenAsync = OpenAsync;
        _context.CloseAsync = CloseAsync;
        _context.SelectItemAsync = SelectItemAsync;
        _context.SetHighlightedItemAsync = SetHighlightedItemAsync;
        _context.RegisterTrigger = RegisterTrigger;
        _context.RegisterContent = RegisterContent;
        _context.RegisterItem = RegisterItem;
        _context.UnregisterItem = UnregisterItem;
        _context.NotifyStateChanged = () => StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        // Sync context with current open state and Dir
        _context.IsOpen = IsOpen;
        _context.Dir = Dir;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<DropdownMenuContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "IsFixed", false);
        builder.AddComponentParameter(3, "ChildContent", ChildContent);
        builder.CloseComponent();
    }

    private async Task ToggleAsync()
    {
        if (IsOpen)
            await CloseAsync();
        else
            await OpenAsync();
    }

    private async Task OpenAsync()
    {
        if (IsOpen) return;

        if (Open is null)
        {
            _internalOpen = true;
        }

        _context.IsOpen = true;
        _context.HighlightedItemId = null; // Reset highlight on open
        await OpenChanged.InvokeAsync(true);
        await OnOpen.InvokeAsync();
        StateHasChanged();
    }

    private async Task CloseAsync()
    {
        if (!IsOpen) return;

        // Close all submenus first
        await _context.CloseAllSubMenusAsync();

        // Focus trigger BEFORE closing, as content may be unmounted after close
        await _context.FocusTriggerAsync();

        if (Open is null)
        {
            _internalOpen = false;
        }

        _context.IsOpen = false;
        _context.IsAnimatingClosed = true; // Set BEFORE StateHasChanged so Portal stays rendered
        _context.HighlightedItemId = null;
        await OpenChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
        StateHasChanged();
        _context.RaiseStateChanged();
    }

    private async Task SelectItemAsync()
    {
        // Close the menu after an item is selected
        await CloseAsync();
    }

    private Task SetHighlightedItemAsync(string? itemId)
    {
        if (_context.HighlightedItemId != itemId)
        {
            _context.HighlightedItemId = itemId;
            _context.RaiseStateChanged();
        }
        return Task.CompletedTask;
    }

    private void RegisterTrigger(ElementReference element)
    {
        _context.TriggerElement = element;
    }

    private void RegisterContent(ElementReference element)
    {
        _context.ContentElement = element;
    }

    private void RegisterItem(string itemId)
    {
        if (!_context.RegisteredItems.Contains(itemId))
        {
            _context.RegisteredItems.Add(itemId);
        }
    }

    private void UnregisterItem(string itemId)
    {
        _context.RegisteredItems.Remove(itemId);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Cleanup is handled by individual components
        await Task.CompletedTask;
    }
}
