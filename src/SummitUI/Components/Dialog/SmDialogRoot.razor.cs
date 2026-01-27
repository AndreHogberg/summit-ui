using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Root component that manages the state of the dialog.
/// Provides cascading context to child components and supports nested dialogs.
/// </summary>
public partial class SmDialogRoot : ComponentBase
{
    /// <summary>
    /// Parent dialog context if this dialog is nested within another dialog.
    /// </summary>
    [CascadingParameter]
    private DialogContext? ParentDialogContext { get; set; }

    /// <summary>
    /// Child content containing DialogTrigger, DialogPortal, etc.
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
    /// Callback invoked when dialog opens.
    /// </summary>
    [Parameter]
    public EventCallback OnOpen { get; set; }

    /// <summary>
    /// Callback invoked when dialog closes.
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    private readonly DialogContext _context = new();
    private bool _internalOpen;
    private bool _hasNotifiedParentOpen;

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

        // Set up nested dialog tracking
        if (ParentDialogContext is not null)
        {
            _context.ParentDialog = ParentDialogContext;
            _context.Depth = ParentDialogContext.Depth + 1;
        }
        else
        {
            _context.Depth = 0;
        }

        // Set up callbacks for child dialogs to notify us
        _context.IncrementNestedCount = () =>
        {
            _context.NestedOpenCount++;
            _context.RaiseStateChanged();
            StateHasChanged();
        };

        _context.DecrementNestedCount = () =>
        {
            _context.NestedOpenCount = Math.Max(0, _context.NestedOpenCount - 1);
            _context.RaiseStateChanged();
            StateHasChanged();
        };
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

        // Notify parent dialog that we're opening (for nested dialog tracking)
        if (ParentDialogContext is not null && !_hasNotifiedParentOpen)
        {
            ParentDialogContext.IncrementNestedCount();
            _hasNotifiedParentOpen = true;
        }

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

        // Notify parent dialog that we're closing (for nested dialog tracking)
        if (ParentDialogContext is not null && _hasNotifiedParentOpen)
        {
            ParentDialogContext.DecrementNestedCount();
            _hasNotifiedParentOpen = false;
        }

        if (Open is null)
        {
            _internalOpen = false;
        }

        _context.IsOpen = false;
        // Keep content/overlay rendered during close so animations can run
        _context.IsContentAnimatingClosed = true;
        _context.IsOverlayAnimatingClosed = true;
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
}
