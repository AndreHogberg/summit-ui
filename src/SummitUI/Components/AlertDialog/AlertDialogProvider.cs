using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Provider component that enables alert dialogs triggered by <see cref="IAlertDialogService"/>.
/// Place this component once at the root of your application (e.g., in MainLayout or App.razor).
/// 
/// This component requires explicit child content defining the dialog structure.
/// Use AlertDialogPortal, AlertDialogOverlay, AlertDialogContent, AlertDialogTitle,
/// AlertDialogDescription, AlertDialogConfirm, and AlertDialogCancel to build the UI.
/// </summary>
/// <example>
/// <code>
/// &lt;AlertDialogProvider Context="alert"&gt;
///     &lt;AlertDialogPortal&gt;
///         &lt;AlertDialogOverlay class="my-overlay" /&gt;
///         &lt;AlertDialogContent class="my-content"&gt;
///             &lt;AlertDialogTitle /&gt;
///             &lt;AlertDialogDescription /&gt;
///             &lt;div class="actions"&gt;
///                 &lt;AlertDialogCancel class="btn-cancel" /&gt;
///                 &lt;AlertDialogConfirm class="btn-confirm" /&gt;
///             &lt;/div&gt;
///         &lt;/AlertDialogContent&gt;
///     &lt;/AlertDialogPortal&gt;
/// &lt;/AlertDialogProvider&gt;
/// </code>
/// </example>
public class AlertDialogProvider : ComponentBase, IDisposable
{
    [Inject]
    private IAlertDialogService AlertDialogService { get; set; } = default!;

    /// <summary>
    /// Template for rendering the alert dialog UI. Required - no default provided.
    /// The context parameter provides access to the current dialog state and options.
    /// </summary>
    [Parameter]
    public RenderFragment<AlertDialogContext>? ChildContent { get; set; }

    private readonly AlertDialogContext _context = new();
    private AlertDialogRequest? _currentRequest;
    private bool _disposed;

    protected override void OnInitialized()
    {
        if (AlertDialogService is AlertDialogService service)
        {
            service.OnShow += HandleShow;
        }

        // Wire up context callbacks
        _context.Complete = HandleComplete;
        _context.NotifyStateChanged = () => InvokeAsync(StateHasChanged);
        _context.RegisterContent = el => _context.ContentElement = el;
    }

    private void HandleShow(AlertDialogRequest request)
    {
        _currentRequest = request;
        _context.IsOpen = true;
        _context.IsAnimatingClosed = false;
        _context.Message = request.Message;
        _context.Options = request.Options;
        InvokeAsync(StateHasChanged);
    }

    private void HandleComplete(bool result)
    {
        if (_currentRequest is null) return;

        _currentRequest.CompletionSource.TrySetResult(result);
        _context.IsOpen = false;
        _context.IsAnimatingClosed = true; // Allow animations to complete
        _currentRequest = null;
        StateHasChanged();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (ChildContent is null)
        {
            throw new InvalidOperationException(
                "AlertDialogProvider requires ChildContent to define the dialog structure. " +
                "Use AlertDialogPortal, AlertDialogOverlay, AlertDialogContent, AlertDialogTitle, " +
                "AlertDialogDescription, AlertDialogConfirm, and AlertDialogCancel components.");
        }

        // Always render CascadingValue so child components can access context
        builder.OpenComponent<CascadingValue<AlertDialogContext>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "ChildContent", ChildContent.Invoke(_context));
        builder.CloseComponent();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (AlertDialogService is AlertDialogService service)
        {
            service.OnShow -= HandleShow;
        }

        // Complete any pending request with false (cancelled)
        _currentRequest?.CompletionSource.TrySetResult(false);
    }
}
