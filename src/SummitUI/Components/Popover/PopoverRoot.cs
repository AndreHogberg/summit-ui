using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// Root component that manages the state of the popover.
/// Provides cascading context to child components.
/// </summary>
public class PopoverRoot : ComponentBase, IAsyncDisposable
{
    [Inject] private PopoverJsInterop JsInterop { get; set; } = default!;
    [Inject] private PopoverService PopoverService { get; set; } = default!;

    /// <summary>
    /// Child content containing PopoverTrigger, PopoverContent, etc.
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
    /// Callback invoked when popover opens.
    /// </summary>
    [Parameter]
    public EventCallback OnOpen { get; set; }

    /// <summary>
    /// Callback invoked when popover closes.
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// Whether the popover is modal (traps focus when open).
    /// </summary>
    [Parameter]
    public bool Modal { get; set; } = true;

    private readonly PopoverContext _context = new();
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
        _context.RegisterTrigger = RegisterTrigger;
        _context.RegisterContent = RegisterContent;
        _context.NotifyStateChanged = () => StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        // Sync context with current open state
        _context.IsOpen = IsOpen;
        _context.Modal = Modal;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<PopoverContext>>(0);
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

        // Only uncontrolled popovers participate in automatic close behavior
        // Controlled popovers are managed entirely by the parent component
        if (Open is null)
        {
            await PopoverService.RegisterOpenAsync(_context);
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

        // Only uncontrolled popovers participate in automatic close behavior
        if (Open is null)
        {
            PopoverService.Unregister(_context);
            _internalOpen = false;
        }

        _context.IsOpen = false;
        _context.IsAnimatingClosed = true; // Set BEFORE StateHasChanged so Portal stays rendered
        await OpenChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
        StateHasChanged();
        _context.RaiseStateChanged();
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

        // Only unregister uncontrolled popovers from service
        if (Open is null)
        {
            PopoverService.Unregister(_context);
        }

        await Task.CompletedTask;
    }
}
