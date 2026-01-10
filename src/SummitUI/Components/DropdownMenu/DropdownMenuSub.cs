using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Container component for a submenu within a dropdown menu.
/// Manages submenu open/close state and provides context to SubTrigger and SubContent.
/// </summary>
public class DropdownMenuSub : ComponentBase, IDisposable
{
    [CascadingParameter]
    private DropdownMenuContext MenuContext { get; set; } = default!;

    [CascadingParameter]
    private DropdownMenuSubContext? ParentSubContext { get; set; }

    /// <summary>
    /// Child content containing DropdownMenuSubTrigger and DropdownMenuSubContent.
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
    /// Callback invoked when submenu opens.
    /// </summary>
    [Parameter]
    public EventCallback OnOpen { get; set; }

    /// <summary>
    /// Callback invoked when submenu closes.
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    private DropdownMenuSubContext _context = default!;
    private bool _internalOpen;
    private bool _isDisposed;

    /// <summary>
    /// Effective open state (controlled or uncontrolled).
    /// </summary>
    private bool IsOpen => Open ?? _internalOpen;

    protected override void OnInitialized()
    {
        _context = new DropdownMenuSubContext(MenuContext);
        _internalOpen = DefaultOpen;

        // Register with the central registry - this reliably establishes parent-child relationships
        // even when cascading parameters don't propagate correctly through portals/dynamic content
        var parentSubMenuId = ParentSubContext?.SubMenuId;
        MenuContext.RegisterSubContext(_context, parentSubMenuId);

        _context.Dir = MenuContext.Dir;

        // Wire up context callbacks
        _context.IsOpen = IsOpen;
        _context.OpenAsync = OpenAsync;
        _context.CloseAsync = CloseAsync;
        _context.CloseAllAsync = CloseAllAsync;
        _context.SetHighlightedItemAsync = SetHighlightedItemAsync;
        _context.RegisterTrigger = RegisterTrigger;
        _context.RegisterContent = RegisterContent;
        _context.RegisterItem = RegisterItem;
        _context.UnregisterItem = UnregisterItem;
        _context.NotifySubMenuOpeningAsync = HandleNestedSubMenuOpeningAsync;
    }

    protected override void OnParametersSet()
    {
        // Sync context with current open state
        _context.IsOpen = IsOpen;
        _context.Dir = MenuContext.Dir;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Provide the submenu context to children
        builder.OpenComponent<CascadingValue<DropdownMenuSubContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "IsFixed", false);
        builder.AddComponentParameter(3, "ChildContent", ChildContent);
        builder.CloseComponent();
    }

    private async Task OpenAsync()
    {
        if (IsOpen) return;

        // Notify parent to close sibling submenus
        if (ParentSubContext != null)
        {
            await ParentSubContext.NotifySubMenuOpeningAsync(_context);
        }
        else
        {
            await MenuContext.NotifySubMenuOpeningAsync(_context);
        }

        if (Open is null)
        {
            _internalOpen = true;
        }

        _context.IsOpen = true;
        _context.HighlightedItemId = null;
        await OpenChanged.InvokeAsync(true);
        await OnOpen.InvokeAsync();
        StateHasChanged();
    }

    private async Task CloseAsync()
    {
        if (!IsOpen) return;

        // First close any nested submenus
        if (_context.ActiveNestedSubContext != null)
        {
            await _context.ActiveNestedSubContext.CloseAllAsync();
            _context.ActiveNestedSubContext = null;
        }

        // Focus the trigger BEFORE closing
        await _context.FocusTriggerAsync();

        if (Open is null)
        {
            _internalOpen = false;
        }

        _context.IsOpen = false;
        _context.IsAnimatingClosed = true;
        _context.HighlightedItemId = null;
        await OpenChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
        StateHasChanged();
        _context.RaiseStateChanged();
    }

    private async Task CloseAllAsync()
    {
        // Close nested submenus first
        if (_context.ActiveNestedSubContext != null)
        {
            await _context.ActiveNestedSubContext.CloseAllAsync();
            _context.ActiveNestedSubContext = null;
        }

        // Then close this submenu (without focusing trigger - we're closing the whole tree)
        if (!IsOpen) return;

        if (Open is null)
        {
            _internalOpen = false;
        }

        _context.IsOpen = false;
        _context.IsAnimatingClosed = true;
        _context.HighlightedItemId = null;
        await OpenChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
        StateHasChanged();
        _context.RaiseStateChanged();
    }

    private async Task HandleNestedSubMenuOpeningAsync(DropdownMenuSubContext openingContext)
    {
        // Close currently active nested submenu if different
        if (_context.ActiveNestedSubContext != null && _context.ActiveNestedSubContext != openingContext)
        {
            await _context.ActiveNestedSubContext.CloseAllAsync();
        }

        _context.ActiveNestedSubContext = openingContext;
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

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Unregister from central registry
        MenuContext.UnregisterSubContext(_context.SubMenuId);

        // Clear from parent's active submenu if we're it
        if (_context.ParentSubContext != null && _context.ParentSubContext.ActiveNestedSubContext == _context)
        {
            _context.ParentSubContext.ActiveNestedSubContext = null;
        }
        else if (MenuContext.ActiveSubContext == _context)
        {
            MenuContext.ActiveSubContext = null;
        }
    }
}
