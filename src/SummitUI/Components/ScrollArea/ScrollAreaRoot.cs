using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

using SummitUI.Interop;
using SummitUI.Utilities;

namespace SummitUI;

/// <summary>
/// Root component that contains all scroll area parts.
/// Manages state and coordinates initialization between viewport and scrollbars.
/// </summary>
public class ScrollAreaRoot : ComponentBase, IAsyncDisposable
{
    [Inject]
    private ScrollAreaJsInterop JsInterop { get; set; } = default!;

    [Inject]
    private SummitUtilities SummitUtilities { get; set; } = default!;

    /// <summary>
    /// Child content containing ScrollAreaViewport, ScrollAreaScrollbar, etc.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The visibility behavior of scrollbars.
    /// Defaults to Hover (show on hover, hide after delay).
    /// </summary>
    [Parameter]
    public ScrollAreaType Type { get; set; } = ScrollAreaType.Hover;

    /// <summary>
    /// Delay in milliseconds before hiding scrollbars (for Hover and Scroll types).
    /// Defaults to 600ms.
    /// </summary>
    [Parameter]
    public int ScrollHideDelay { get; set; } = 600;

    /// <summary>
    /// Text direction. When set to "rtl", the vertical scrollbar appears on the left.
    /// If null, direction is auto-detected from the DOM.
    /// </summary>
    [Parameter]
    public string? Dir { get; set; }

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

    private readonly ScrollAreaContext _context = new();
    private DotNetObjectReference<ScrollAreaRoot>? _dotNetRef;
    private string? _instanceId;
    private bool _isInitialized;
    private string _effectiveDir = "ltr";

    protected override void OnInitialized()
    {
        // Wire up context callbacks
        _context.SetViewportRef = SetViewportRef;
        _context.RegisterScrollbar = RegisterScrollbar;
        _context.UnregisterScrollbar = UnregisterScrollbar;
        _context.InitializeAsync = InitializeJsAsync;
        _context.NotifyStateChanged = () => StateHasChanged();

        SyncContext();
    }

    protected override void OnParametersSet()
    {
        SyncContext();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Detect RTL if not explicitly set
            if (Dir is null)
            {
                _effectiveDir = await SummitUtilities.IsRtlAsync() ? "rtl" : "ltr";
            }
            else
            {
                _effectiveDir = Dir;
            }

            _context.Dir = _effectiveDir;
        }
    }

    private void SyncContext()
    {
        _context.Type = Type;
        _context.ScrollHideDelay = ScrollHideDelay;
        _context.Dir = Dir ?? _effectiveDir;
    }

    private void SetViewportRef(ElementReference viewportRef)
    {
        _context.ViewportRef = viewportRef;
        _context.HasViewportRef = true;

        // Attempt to initialize if we haven't already
        _ = TryInitializeAsync();
    }

    private void RegisterScrollbar(ScrollAreaOrientation orientation)
    {
        if (orientation == ScrollAreaOrientation.Vertical)
            _context.HasVerticalScrollbar = true;
        else
            _context.HasHorizontalScrollbar = true;
    }

    private void UnregisterScrollbar(ScrollAreaOrientation orientation)
    {
        if (orientation == ScrollAreaOrientation.Vertical)
            _context.HasVerticalScrollbar = false;
        else
            _context.HasHorizontalScrollbar = false;
    }

    private async Task<string?> InitializeJsAsync()
    {
        return await TryInitializeAsync();
    }

    private async Task<string?> TryInitializeAsync()
    {
        if (_isInitialized || !_context.HasViewportRef)
            return _instanceId;

        _dotNetRef = DotNetObjectReference.Create(this);

        var options = new ScrollAreaOptions
        {
            Type = Type.ToString().ToLowerInvariant(),
            ScrollHideDelay = ScrollHideDelay,
            Dir = _context.Dir
        };

        _instanceId = await JsInterop.InitializeAsync(_context.ViewportRef, options, _dotNetRef);
        _context.InstanceId = _instanceId;
        _isInitialized = true;

        // Trigger a re-render so scrollbars can register with the now-available instance
        StateHasChanged();

        return _instanceId;
    }

    /// <summary>
    /// Called from JavaScript when overflow state changes.
    /// </summary>
    [JSInvokable]
    public void OnOverflowChanged(bool hasVerticalOverflow, bool hasHorizontalOverflow)
    {
        _context.SetOverflowState(hasVerticalOverflow, hasHorizontalOverflow);
        _context.NotifyStateChanged();
    }

    /// <summary>
    /// Called from JavaScript when scrollbar visibility state changes.
    /// </summary>
    [JSInvokable]
    public void OnScrollbarStateChanged(string orientation, string state)
    {
        var orient = orientation.Equals("vertical", StringComparison.OrdinalIgnoreCase)
            ? ScrollAreaOrientation.Vertical
            : ScrollAreaOrientation.Horizontal;

        _context.SetScrollbarState(orient, state);
        _context.NotifyStateChanged();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;
        builder.OpenElement(seq++, As);

        // Apply additional attributes first
        builder.AddMultipleAttributes(seq++, AdditionalAttributes);

        // Core attributes
        builder.AddAttribute(seq++, "data-summit-scroll-area-root", true);
        builder.AddAttribute(seq++, "dir", _context.Dir);

        // The root needs position: relative for absolute positioned scrollbars
        // and overflow: hidden to prevent double scrollbars
        var existingStyle = AdditionalAttributes?.TryGetValue("style", out var styleObj) == true
            ? styleObj?.ToString() ?? ""
            : "";

        var rootStyles = "position: relative; overflow: hidden;";
        var combinedStyle = string.IsNullOrEmpty(existingStyle)
            ? rootStyles
            : $"{existingStyle}; {rootStyles}";

        builder.AddAttribute(seq++, "style", combinedStyle);

        // Cascading context
        builder.OpenComponent<CascadingValue<ScrollAreaContext>>(seq++);
        builder.AddComponentParameter(seq++, "Value", _context);
        builder.AddComponentParameter(seq++, "IsFixed", false);
        builder.AddComponentParameter(seq++, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }

    public async ValueTask DisposeAsync()
    {
        if (_instanceId is not null)
        {
            await JsInterop.DestroyAsync(_instanceId);
        }

        _dotNetRef?.Dispose();
    }
}
