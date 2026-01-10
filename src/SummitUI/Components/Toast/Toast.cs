using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace SummitUI;

/// <summary>
/// Swipe direction for toast dismissal.
/// </summary>
public enum SwipeDirection
{
    /// <summary>Swipe left to dismiss.</summary>
    Left,
    /// <summary>Swipe right to dismiss.</summary>
    Right,
    /// <summary>Swipe up to dismiss.</summary>
    Up,
    /// <summary>Swipe down to dismiss.</summary>
    Down
}

/// <summary>
/// Urgency level for toast notifications, affecting aria-live announcements.
/// </summary>
public enum ToastUrgency
{
    /// <summary>
    /// Polite announcements (aria-live="polite"). Background, non-critical messages.
    /// </summary>
    Polite,
    /// <summary>
    /// Assertive announcements (aria-live="assertive"). Foreground, important messages.
    /// </summary>
    Assertive
}

/// <summary>
/// An individual toast notification. Renders as an ARIA live region with role="status".
/// </summary>
/// <typeparam name="TContent">User-defined toast content type.</typeparam>
/// <remarks>
/// <para>
/// Each toast is an ARIA live region with proper accessibility attributes including
/// aria-live (assertive/polite), aria-atomic, and keyboard focusability.
/// Users provide their own content via ChildContent.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;Toast Toast="toast" class="bg-blue-600 p-4 rounded-lg"&gt;
///     &lt;span&gt;@toast.Content.Title&lt;/span&gt;
///     &lt;ToastCloseButton&gt;×&lt;/ToastCloseButton&gt;
/// &lt;/Toast&gt;
/// </code>
/// </example>
public class Toast<TContent> : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private IToastQueue<TContent> Queue { get; set; } = default!;

    /// <summary>
    /// The queued toast data including key and content.
    /// </summary>
    [Parameter, EditorRequired]
    public QueuedToast<TContent> ToastData { get; set; } = default!;

    /// <summary>
    /// Child content for the toast. Users provide their own markup.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The swipe direction that will dismiss the toast. Null to disable swipe.
    /// </summary>
    [Parameter]
    public SwipeDirection? SwipeDirection { get; set; }

    /// <summary>
    /// Distance in pixels required to trigger swipe dismissal. Default is 50.
    /// </summary>
    [Parameter]
    public int SwipeThreshold { get; set; } = 50;

    /// <summary>
    /// Urgency level for screen reader announcements. Default is Assertive.
    /// </summary>
    [Parameter]
    public ToastUrgency Urgency { get; set; } = ToastUrgency.Assertive;

    /// <summary>
    /// ID of the element that labels this toast (for aria-labelledby).
    /// </summary>
    [Parameter]
    public string? AriaLabelledBy { get; set; }

    /// <summary>
    /// ID of the element that describes this toast (for aria-describedby).
    /// </summary>
    [Parameter]
    public string? AriaDescribedBy { get; set; }

    /// <summary>
    /// Additional attributes to apply to the toast element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private ToastJsInterop? _jsInterop;
    private DotNetObjectReference<Toast<TContent>>? _dotNetRef;
    private bool _swipeRegistered;

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;
        var ariaLive = Urgency == ToastUrgency.Assertive ? "assertive" : "polite";

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "role", "status");
        builder.AddAttribute(seq++, "aria-live", ariaLive);
        builder.AddAttribute(seq++, "aria-atomic", "true");
        builder.AddAttribute(seq++, "data-summit-toast-root", "");
        builder.AddAttribute(seq++, "data-state", "open");
        builder.AddAttribute(seq++, "data-toast-key", ToastData.Key);
        builder.AddAttribute(seq++, "class", "toast-root");

        if (SwipeDirection.HasValue)
        {
            builder.AddAttribute(seq++, "data-swipe-direction", SwipeDirection.Value.ToString().ToLowerInvariant());
        }

        if (!string.IsNullOrEmpty(AriaLabelledBy))
        {
            builder.AddAttribute(seq++, "aria-labelledby", AriaLabelledBy);
        }

        if (!string.IsNullOrEmpty(AriaDescribedBy))
        {
            builder.AddAttribute(seq++, "aria-describedby", AriaDescribedBy);
        }

        builder.AddMultipleAttributes(seq++, AdditionalAttributes);
        builder.AddElementReferenceCapture(seq++, el => _elementRef = el);
        builder.SetKey(ToastData.Key);

        // Cascading value for ToastCloseButton to access the toast key
        builder.OpenComponent<CascadingValue<string>>(seq++);
        builder.AddComponentParameter(seq++, "Name", "ToastKey");
        builder.AddComponentParameter(seq++, "Value", ToastData.Key);
        builder.AddComponentParameter(seq++, "IsFixed", true);
        builder.AddComponentParameter(seq++, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && SwipeDirection.HasValue)
        {
            _jsInterop = new ToastJsInterop(JsRuntime);
            _dotNetRef = DotNetObjectReference.Create(this);

            var direction = SwipeDirection.Value.ToString().ToLowerInvariant();
            await _jsInterop.RegisterSwipeAsync(_elementRef, direction, SwipeThreshold, _dotNetRef!);
            _swipeRegistered = true;
        }
    }

    /// <summary>
    /// Called by JS when swipe completes past threshold.
    /// </summary>
    [JSInvokable]
    public void HandleSwipeEnd(double x, double y)
    {
        Queue.Close(ToastData.Key);
    }

    /// <summary>
    /// Called by JS when swipe starts.
    /// </summary>
    [JSInvokable]
    public void HandleSwipeStart() { }

    /// <summary>
    /// Called by JS during swipe movement.
    /// </summary>
    [JSInvokable]
    public void HandleSwipeMove(double x, double y) { }

    /// <summary>
    /// Called by JS when swipe is cancelled.
    /// </summary>
    [JSInvokable]
    public void HandleSwipeCancel() { }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_swipeRegistered && _jsInterop is not null)
            {
                await _jsInterop.UnregisterSwipeAsync(_elementRef);
            }
            if (_jsInterop is not null)
            {
                await _jsInterop.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Safe to ignore, JS resources are cleaned up by the browser
        }

        _dotNetRef?.Dispose();
    }
}
