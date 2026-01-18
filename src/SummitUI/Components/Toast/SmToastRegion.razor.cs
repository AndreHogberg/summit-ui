using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// A container for toast notifications. Renders as an ARIA landmark region.
/// </summary>
/// <typeparam name="TContent">User-defined toast content type.</typeparam>
/// <remarks>
/// <para>
/// The ToastRegion is an ARIA landmark region labeled for screen readers.
/// It injects <see cref="IToastQueue{TContent}"/> from DI and subscribes to changes.
/// </para>
/// <para>
/// By default, ToastRegion renders content in a portal appended to document.body,
/// avoiding CSS stacking context issues from parent transforms or filters.
/// Set <see cref="UsePortal"/> to false to render inline.
/// </para>
/// <para>
/// Users provide a template via <see cref="ToastTemplate"/> to render each toast.
/// This gives full control over toast appearance while SummitUI handles accessibility.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;ToastRegion TContent="MyToast" AriaLabel="Notifications"&gt;
///     &lt;ToastTemplate Context="toast"&gt;
///         &lt;Toast Toast="toast"&gt;
///             &lt;span&gt;@toast.Content.Title&lt;/span&gt;
///             &lt;ToastCloseButton&gt;x&lt;/ToastCloseButton&gt;
///         &lt;/Toast&gt;
///     &lt;/ToastTemplate&gt;
/// &lt;/ToastRegion&gt;
/// </code>
/// </example>
public partial class SmToastRegion<TContent> : IAsyncDisposable
{
    [Inject]
    private IToastQueue<TContent> Queue { get; set; } = default!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    /// <summary>
    /// Accessible label for the toast region. Required for screen readers.
    /// </summary>
    /// <remarks>
    /// This should be localized by the user. SummitUI does not provide default text.
    /// Example: "Notifications", "Aviseringar" (Swedish), "通知" (Chinese)
    /// </remarks>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Template for rendering each toast.
    /// </summary>
    [Parameter]
    public RenderFragment<QueuedToast<TContent>>? ToastTemplate { get; set; }

    /// <summary>
    /// Function to get the screen reader announcement text from toast content.
    /// If not provided, aria-live announcements are disabled.
    /// </summary>
    /// <remarks>
    /// This allows users to control what text is announced by screen readers.
    /// Example: content => content.Description
    /// </remarks>
    [Parameter]
    public Func<TContent, string>? GetAnnouncementText { get; set; }

    /// <summary>
    /// Keyboard shortcut to focus the toast region.
    /// </summary>
    /// <remarks>
    /// Specify as array of key codes. Examples:
    /// <list type="bullet">
    /// <item><description>["F8"] - F8 key alone</description></item>
    /// <item><description>["altKey", "KeyT"] - Alt+T</description></item>
    /// </list>
    /// If null, no hotkey is registered.
    /// </remarks>
    [Parameter]
    public string[]? Hotkey { get; set; }

    /// <summary>
    /// Whether to render the toast region in a portal appended to body.
    /// Default is true to avoid CSS stacking context issues.
    /// </summary>
    [Parameter]
    public bool UsePortal { get; set; } = true;

    /// <summary>
    /// Additional attributes to apply to the region element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private Action? _unsubscribe;
    private bool _isHovered;
    private bool _isFocused;
    private ElementReference _regionRef = default!;
    private ToastJsInterop? _jsInterop;
    private DotNetObjectReference<SmToastRegion<TContent>>? _dotNetRef;
    private bool _hotkeyRegistered;
    private bool _portalCreated;
    private readonly string _portalId = $"summit-toast-portal-{Guid.NewGuid():N}";

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _unsubscribe = Queue.Subscribe(() => InvokeAsync(StateHasChanged));
        _jsInterop = new ToastJsInterop(JsRuntime);
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Only run JS interop when interactive
        if (!RendererInfo.IsInteractive) return;

        if (UsePortal && !_portalCreated && _jsInterop is not null)
        {
            try
            {
                await _jsInterop.CreatePortalAsync(_portalId);
                _portalCreated = true;
                // Re-render to remove data-portal-pending attribute
                StateHasChanged();
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, ignore
            }
        }

        if (Hotkey is { Length: > 0 } && !_hotkeyRegistered && _jsInterop is not null && _dotNetRef is not null)
        {
            await _jsInterop.RegisterHotkeyAsync(_regionRef, Hotkey, DotNetObjectReference.Create((object)this), nameof(OnHotkeyPressed));
            _hotkeyRegistered = true;
        }
    }

    /// <summary>
    /// Called when the hotkey is pressed. Used by JS interop.
    /// </summary>
    [JSInvokable]
    public void OnHotkeyPressed()
    {
        // Focus is handled by JS, this is just for any .NET-side handling
    }

    private void HandleMouseEnter(MouseEventArgs e)
    {
        _isHovered = true;
        Queue.PauseAll();
    }

    private void HandleMouseLeave(MouseEventArgs e)
    {
        _isHovered = false;
        if (!_isFocused)
        {
            Queue.ResumeAll();
        }
    }

    private void HandleFocusIn(FocusEventArgs e)
    {
        _isFocused = true;
        Queue.PauseAll();
    }

    private void HandleFocusOut(FocusEventArgs e)
    {
        _isFocused = false;
        if (!_isHovered)
        {
            Queue.ResumeAll();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _unsubscribe?.Invoke();
        _dotNetRef?.Dispose();

        if (_jsInterop is not null)
        {
            try
            {
                if (_hotkeyRegistered)
                {
                    await _jsInterop.UnregisterHotkeyAsync(_regionRef);
                }

                if (_portalCreated)
                {
                    await _jsInterop.DestroyPortalAsync(_portalId);
                }
            }
            catch (JSDisconnectedException)
            {
                // Safe to ignore, JS resources are cleaned up by the browser
            }

            await _jsInterop.DisposeAsync();
        }
    }
}
