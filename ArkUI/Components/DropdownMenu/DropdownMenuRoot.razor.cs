using ArkUI.Interop;
using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// Root component that manages the state of the dropdown menu.
/// Provides cascading context to child components.
/// </summary>
public partial class DropdownMenuRoot : ComponentBase, IAsyncDisposable
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
        _context.ToggleAsync = ToggleAsync;
        _context.OpenAsync = OpenAsync;
        _context.CloseAsync = CloseAsync;
        _context.SelectItemAsync = SelectItemAsync;
        _context.RegisterTrigger = RegisterTrigger;
        _context.RegisterContent = RegisterContent;
        _context.NotifyStateChanged = () => StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        // Sync context with current open state
        _context.IsOpen = IsOpen;
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
        await OpenChanged.InvokeAsync(true);
        await OnOpen.InvokeAsync();
        StateHasChanged();
    }

    private async Task CloseAsync()
    {
        if (!IsOpen) return;

        if (Open is null)
        {
            _internalOpen = false;
        }

        _context.IsOpen = false;
        await OpenChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
        StateHasChanged();
    }

    private async Task SelectItemAsync()
    {
        // Close the menu after an item is selected
        await CloseAsync();
    }

    private void RegisterTrigger(ElementReference element)
    {
        _context.TriggerElement = element;
    }

    private void RegisterContent(ElementReference element)
    {
        _context.ContentElement = element;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Cleanup is handled by individual components
        await Task.CompletedTask;
    }
}
