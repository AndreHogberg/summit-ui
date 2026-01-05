using SummitUI.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace SummitUI;

/// <summary>
/// Accordion content panel. Renders with role="region".
/// Only renders when the associated item is expanded (unless ForceMount is true).
/// Uses animation-aware presence management to wait for CSS animations before hiding.
/// </summary>
public class AccordionContent : ComponentBase, IAsyncDisposable
{
    [CascadingParameter]
    private AccordionContext Context { get; set; } = default!;

    [CascadingParameter]
    private AccordionItemContext ItemContext { get; set; } = default!;

    [Inject]
    private AccordionJsInterop JsInterop { get; set; } = default!;

    /// <summary>
    /// Panel content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// When true, content is always rendered in the DOM (useful for animations).
    /// Defaults to false.
    /// </summary>
    [Parameter]
    public bool ForceMount { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private DotNetObjectReference<AccordionContent>? _dotNetRef;
    private bool _wasExpanded;
    private bool _shouldRender;
    private bool _isHidden;
    private bool _isFirstRender = true;
    private bool _disposed;

    private bool IsExpanded => Context.IsExpanded(ItemContext.Value);
    private string DataState => IsExpanded ? "open" : "closed";

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
    }

    /// <summary>
    /// Called from JavaScript when all CSS animations have completed.
    /// </summary>
    [JSInvokable]
    public async Task OnAnimationsComplete()
    {
        if (_disposed) return;

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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Determine if we should render based on presence state
        var shouldRenderNow = IsExpanded || ForceMount || _shouldRender;
        
        if (!shouldRenderNow) return;

        // Ensure we render when expanded
        if (IsExpanded && !_shouldRender)
        {
            _shouldRender = true;
        }

        builder.OpenElement(0, As);
        builder.AddAttribute(1, "role", "region");
        builder.AddAttribute(2, "id", Context.GetContentId(ItemContext.Value));
        builder.AddAttribute(3, "aria-labelledby", Context.GetTriggerId(ItemContext.Value));
        builder.AddAttribute(4, "data-state", DataState);
        builder.AddAttribute(5, "data-orientation", Context.Orientation.ToString().ToLowerInvariant());
        builder.AddAttribute(6, "data-summit-accordion-content", true);

        // On first render or when we know content should be hidden, add hidden attribute
        // The JS will manage this attribute dynamically for animation-aware hiding
        if (_isHidden && (_isFirstRender || !IsExpanded))
        {
            builder.AddAttribute(7, "hidden", true);
        }

        builder.AddMultipleAttributes(8, AdditionalAttributes);
        builder.AddElementReferenceCapture(9, elementRef => _elementRef = elementRef);
        builder.AddContent(10, ChildContent);
        builder.CloseElement();
    }

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        
        // Only call JS interop if we're in an interactive context
        // During SSR, RendererInfo.IsInteractive is false and JS isn't available
        if (RendererInfo.IsInteractive && _elementRef.Id != null)
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
}
