using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// A scrollbar track component. Add a ScrollAreaThumb inside this component.
/// Use the orientation parameter to specify vertical or horizontal scrolling.
/// </summary>
public partial class SmScrollAreaScrollbar : ComponentBase, IAsyncDisposable
{
    private void RenderContent(RenderTreeBuilder builder)
    {
        if (ShouldRenderContent)
        {
            builder.OpenElement(0, As);
            builder.AddAttribute(1, "id", Context!.GetScrollbarId(Orientation));
            builder.AddAttribute(2, "data-summit-scroll-area-scrollbar", "true");
            builder.AddAttribute(3, "data-orientation", Orientation.ToString().ToLowerInvariant());
            builder.AddAttribute(4, "data-state", _currentState);
            builder.AddAttribute(5, "data-summit-scroll-area-scrollbar-y", Orientation == ScrollAreaOrientation.Vertical ? true : null);
            builder.AddAttribute(6, "data-summit-scroll-area-scrollbar-x", Orientation == ScrollAreaOrientation.Horizontal ? true : null);
            builder.AddMultipleAttributes(7, AdditionalAttributes);
            builder.AddElementReferenceCapture(8, elemRef => _elementRef = elemRef);
            builder.OpenComponent<CascadingValue<SmScrollAreaScrollbar>>(9);
            builder.AddComponentParameter(10, "Value", this);
            builder.AddComponentParameter(11, "IsFixed", true);
            builder.AddComponentParameter(12, "ChildContent", ChildContent);
            builder.CloseComponent();
            builder.CloseElement();
        }
    }

    [Inject]
    private ScrollAreaJsInterop JsInterop { get; set; } = default!;

    /// <summary>
    /// The scroll area context provided by ScrollAreaRoot.
    /// </summary>
    [CascadingParameter]
    public ScrollAreaContext? Context { get; set; }

    /// <summary>
    /// Child content (typically ScrollAreaThumb).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The orientation of this scrollbar.
    /// Defaults to Vertical.
    /// </summary>
    [Parameter]
    public ScrollAreaOrientation Orientation { get; set; } = ScrollAreaOrientation.Vertical;

    /// <summary>
    /// Whether to force mount the scrollbar regardless of visibility state.
    /// Useful for CSS animations.
    /// </summary>
    [Parameter]
    public bool ForceMount { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// Additional HTML attributes to apply.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef = default!;
    private ElementReference _thumbRef;
    private bool _isRegistered;
    private string? _lastAttemptedInstanceId;
    private string _currentState = "hidden";

    /// <summary>
    /// Internal property to allow ScrollAreaThumb to register its element reference.
    /// </summary>
    internal ElementReference ThumbRef
    {
        get => _thumbRef;
        set
        {
            _thumbRef = value;
            _ = TryRegisterAsync();
        }
    }

    /// <summary>
    /// Internal property to check if this scrollbar has an element reference.
    /// </summary>
    internal bool HasElementRef => _elementRef.Id is not null;

    private bool ShouldRenderContent
    {
        get
        {
            if (Context is null) return false;

            var state = Context.GetScrollbarState(Orientation);
            var shouldShow = Context.ShouldShowScrollbar(Orientation);

            // Always render on first pass (before JS initialization) to allow thumb registration
            // After initialization, respect ForceMount and visibility rules
            var hasInitialized = Context.InstanceId is not null;
            
            if (hasInitialized && !ForceMount && !shouldShow && state == "hidden")
            {
                return false;
            }

            _currentState = shouldShow ? "visible" : "hidden";
            return true;
        }
    }

    protected override void OnInitialized()
    {
        if (Context is not null)
        {
            Context.RegisterScrollbar(Orientation);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Try to register when InstanceId becomes available.
        // Track the last attempted InstanceId to avoid infinite retries.
        if (!_isRegistered && 
            Context?.InstanceId is not null && 
            Context.InstanceId != _lastAttemptedInstanceId)
        {
            _lastAttemptedInstanceId = Context.InstanceId;
            await TryRegisterAsync();
        }
    }

    private async Task TryRegisterAsync()
    {
        // We need both the scrollbar element and thumb element, plus a JS instance
        if (_isRegistered || Context?.InstanceId is null)
            return;

        if (_elementRef.Id is null || _thumbRef.Id is null)
            return;

        var orientation = Orientation.ToString().ToLowerInvariant();
        await JsInterop.RegisterScrollbarAsync(Context.InstanceId, orientation, _elementRef, _thumbRef);
        _isRegistered = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isRegistered && Context?.InstanceId is not null)
        {
            var orientation = Orientation.ToString().ToLowerInvariant();
            await JsInterop.UnregisterScrollbarAsync(Context.InstanceId, orientation);
        }

        Context?.UnregisterScrollbar(Orientation);
    }
}
