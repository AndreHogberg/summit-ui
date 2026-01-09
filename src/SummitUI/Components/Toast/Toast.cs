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
/// An individual toast notification. Renders as an ARIA alertdialog.
/// </summary>
/// <typeparam name="TContent">User-defined toast content type.</typeparam>
/// <remarks>
/// <para>
/// Each toast is a non-modal alertdialog with proper ARIA attributes.
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

        builder.OpenElement(seq++, "div");
        builder.AddAttribute(seq++, "role", "alertdialog");
        builder.AddAttribute(seq++, "aria-modal", "false");
        builder.AddAttribute(seq++, "data-toast-key", ToastData.Key);
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
