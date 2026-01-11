using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
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
///             &lt;ToastCloseButton&gt;×&lt;/ToastCloseButton&gt;
///         &lt;/Toast&gt;
///     &lt;/ToastTemplate&gt;
/// &lt;/ToastRegion&gt;
/// </code>
/// </example>
public class ToastRegion<TContent> : ComponentBase, IAsyncDisposable
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
    private ElementReference _regionRef;
    private ToastJsInterop? _jsInterop;
    private DotNetObjectReference<ToastRegion<TContent>>? _dotNetRef;
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

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var visibleToasts = Queue.VisibleToasts;

        // Always render the main region container so hotkey registration works
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "id", _portalId);
        builder.AddAttribute(2, "role", "region");
        builder.AddAttribute(3, "aria-label", AriaLabel);
        builder.AddAttribute(4, "tabindex", "-1");
        
        // Hide element until portal moves it to body - use inline style to avoid CSS dependency
        // Use opacity: 0 and pointer-events: none to hide without affecting layout
        if (UsePortal && !_portalCreated)
        {
            builder.AddAttribute(5, "data-portal-pending", "true");
            builder.AddAttribute(6, "style", "opacity: 0; pointer-events: none;");
        }
        
        builder.AddMultipleAttributes(7, AdditionalAttributes);
        builder.AddAttribute(8, "onmouseenter", EventCallback.Factory.Create<MouseEventArgs>(this, HandleMouseEnter));
        builder.AddAttribute(9, "onmouseleave", EventCallback.Factory.Create<MouseEventArgs>(this, HandleMouseLeave));
        builder.AddAttribute(10, "onfocusin", EventCallback.Factory.Create<FocusEventArgs>(this, HandleFocusIn));
        builder.AddAttribute(11, "onfocusout", EventCallback.Factory.Create<FocusEventArgs>(this, HandleFocusOut));
        builder.AddElementReferenceCapture(12, el => _regionRef = el);

        if (visibleToasts.Count > 0)
        {
            // Cascading value for Toast components to access the queue
            builder.OpenComponent<CascadingValue<Action<string>>>(20);
            builder.AddComponentParameter(21, "Name", "CloseToast");
            builder.AddComponentParameter(22, "Value", (Action<string>)Queue.Close);
            builder.AddComponentParameter(23, "IsFixed", true);
            builder.AddComponentParameter(24, "ChildContent", (RenderFragment)(childBuilder =>
            {
                // Render each visible toast using the template
                // Use a keyed fragment wrapper for stable identity when list changes
                foreach (var toast in visibleToasts)
                {
                    if (ToastTemplate is not null)
                    {
                        childBuilder.OpenElement(0, "div");
                        childBuilder.AddAttribute(1, "style", "display: contents;");
                        childBuilder.SetKey(toast.Key);
                        childBuilder.AddContent(2, ToastTemplate(toast));
                        childBuilder.CloseElement();
                    }
                }
            }));
            builder.CloseComponent();
        }

        builder.CloseElement();

        // Aria-live regions for screen reader announcements
        RenderLiveRegions(builder);
    }

    private void RenderLiveRegions(RenderTreeBuilder builder)
    {
        // Only render live regions if user provided a way to get announcement text
        if (GetAnnouncementText is null)
        {
            return;
        }

        var seq = 1000;

        // Polite announcements
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "aria-live", "polite");
        builder.AddAttribute(seq++, "aria-atomic", "true");
        builder.AddAttribute(seq++, "style", "position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px; overflow: hidden; clip: rect(0, 0, 0, 0); white-space: nowrap; border: 0;");
        
        var politeToasts = Queue.VisibleToasts.Where(t => t.Priority == ToastPriority.Polite).ToList();
        if (politeToasts.Count > 0)
        {
            var latest = politeToasts[^1];
            builder.AddContent(seq++, GetAnnouncementText(latest.Content));
        }
        builder.CloseElement();

        // Assertive announcements
        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "aria-live", "assertive");
        builder.AddAttribute(seq++, "aria-atomic", "true");
        builder.AddAttribute(seq++, "style", "position: absolute; width: 1px; height: 1px; padding: 0; margin: -1px; overflow: hidden; clip: rect(0, 0, 0, 0); white-space: nowrap; border: 0;");
        
        var assertiveToasts = Queue.VisibleToasts.Where(t => t.Priority == ToastPriority.Assertive).ToList();
        if (assertiveToasts.Count > 0)
        {
            var latest = assertiveToasts[^1];
            builder.AddContent(seq++, GetAnnouncementText(latest.Content));
        }
        builder.CloseElement();
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
