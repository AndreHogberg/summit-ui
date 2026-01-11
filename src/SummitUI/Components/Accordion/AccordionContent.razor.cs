using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SummitUI.Interop;

namespace SummitUI;

/// <summary>
///     Accordion content panel. Renders with role="region".
///     Only renders when the associated item is expanded (unless ForceMount is true).
///     Uses animation-aware presence management to wait for CSS animations before hiding.
/// </summary>
public partial class AccordionContent : ComponentBase, IAsyncDisposable
{
    private bool _disposed;
    private DotNetObjectReference<AccordionContent>? _dotNetRef;

    private ElementReference _elementRef;
    private bool _isFirstRender = true;
    private bool _isHidden;
    private bool _shouldRender;
    private bool _wasExpanded;

    [CascadingParameter] private AccordionContext Context { get; set; } = default!;

    [CascadingParameter] private AccordionItemContext ItemContext { get; set; } = default!;

    [Inject] private AccordionJsInterop JsInterop { get; set; } = default!;

    /// <summary>
    ///     Panel content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    ///     When true, content is always rendered in the DOM (useful for animations).
    ///     Defaults to false.
    /// </summary>
    [Parameter]
    public bool ForceMount { get; set; }

    /// <summary>
    ///     Additional HTML attributes to apply.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private bool IsExpanded => Context.IsExpanded(ItemContext.Value);
    private string DataState => IsExpanded ? "open" : "closed";
    private string Orientation => Context.Orientation.ToString().ToLowerInvariant();
    private string ContentId => Context.GetContentId(ItemContext.Value);
    private string TriggerId => Context.GetTriggerId(ItemContext.Value);

    private bool ShouldRenderNow => IsExpanded || ForceMount || _shouldRender;
    private bool ShouldBeHidden => _isHidden && (_isFirstRender || !IsExpanded);

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        Debug.Assert(_elementRef.Id is not null, "_elementRef was indeed null");
        // Only call JS interop if we're in an interactive context
        // During SSR, RendererInfo.IsInteractive is false and JS isn't available
        if (RendererInfo.IsInteractive)
        {
            try
            {
                await JsInterop.CancelAnimationWatcherAsync(_elementRef);
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, ignore
            }
        }

        _dotNetRef?.Dispose();
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        // Initialize render state based on current expanded state
        _shouldRender = IsExpanded || ForceMount;
        _isHidden = !IsExpanded;
        _wasExpanded = IsExpanded;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isFirstRender = false;
            _dotNetRef = DotNetObjectReference.Create(this);
        }

        // Handle state transitions
        if (IsExpanded && !_wasExpanded)
        {
            // Opening: immediately show and measure content
            _isHidden = false;
            await JsInterop.RemoveHiddenAsync(_elementRef);
            await JsInterop.SetContentHeightAsync(_elementRef);
        }
        else if (!IsExpanded && _wasExpanded)
        {
            // Closing: wait for animations to complete before hiding
            await JsInterop.SetContentHeightAsync(_elementRef);

            if (_dotNetRef != null)
            {
                await JsInterop.WaitForAnimationsCompleteAsync(_elementRef, _dotNetRef, nameof(OnAnimationsComplete));
            }
            else
            {
                // Fallback: hide immediately if no callback available
                await HideContent();
            }
        }

        _wasExpanded = IsExpanded;

        // Ensure we render when expanded
        if (IsExpanded && !_shouldRender)
        {
            _shouldRender = true;
        }
    }

    /// <summary>
    ///     Called from JavaScript when all CSS animations have completed.
    /// </summary>
    [JSInvokable]
    public async Task OnAnimationsComplete()
    {
        if (_disposed)
        {
            return;
        }

        // Only hide if still in closed state (user might have reopened during animation)
        if (!IsExpanded)
        {
            await HideContent();
        }
    }

    private async Task HideContent()
    {
        _isHidden = true;
        await JsInterop.SetHiddenAsync(_elementRef);

        // If not force mounted, we can stop rendering entirely
        if (!ForceMount)
        {
            _shouldRender = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}
