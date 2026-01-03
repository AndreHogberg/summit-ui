using SummitUI.Interop;
using SummitUI.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace SummitUI;

/// <summary>
/// Accordion trigger button that toggles the associated content panel.
/// Handles keyboard navigation including arrow keys, Home, and End.
/// </summary>
public class AccordionTrigger : ComponentBase, IAsyncDisposable
{
    [CascadingParameter]
    private AccordionContext Context { get; set; } = default!;

    [CascadingParameter]
    private AccordionItemContext ItemContext { get; set; } = default!;

    [Inject]
    private SummitUtilities SummitUtilities { get; set; } = default!;

    [Inject]
    private AccordionJsInterop JsInterop { get; set; } = default!;

    /// <summary>
    /// Child content (trigger label).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "button".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "button";

    /// <summary>
    /// Additional HTML attributes to apply.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private bool _isRegistered;
    private bool _isJsInitialized;
    private bool _isDisposed;
    private bool? _cachedIsRtl;

    private bool IsExpanded => Context.IsExpanded(ItemContext.Value);
    private bool IsDisabled => ItemContext.Disabled || Context.Disabled;
    private string DataState => IsExpanded ? "open" : "closed";

    protected override void OnInitialized()
    {
        // Register this trigger with the context
        Context.RegisterTrigger(ItemContext.Value, IsDisabled);
        _isRegistered = true;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && RendererInfo.IsInteractive && !_isDisposed)
        {
            await JsInterop.RegisterTriggerAsync(_elementRef);
            _isJsInitialized = true;
        }
    }

    protected override void OnParametersSet()
    {
        // Update disabled state in context when parameters change
        if (_isRegistered)
        {
            Context.UpdateTriggerDisabled(ItemContext.Value, IsDisabled);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "type", "button");
        builder.AddAttribute(2, "id", Context.GetTriggerId(ItemContext.Value));
        builder.AddAttribute(3, "aria-expanded", IsExpanded.ToString().ToLowerInvariant());
        builder.AddAttribute(4, "aria-controls", Context.GetContentId(ItemContext.Value));
        builder.AddAttribute(5, "data-state", DataState);
        builder.AddAttribute(6, "data-orientation", Context.Orientation.ToString().ToLowerInvariant());
        builder.AddAttribute(7, "data-ark-accordion-trigger", true);

        if (IsDisabled)
        {
            builder.AddAttribute(8, "disabled", true);
            builder.AddAttribute(9, "data-disabled", true);
        }

        builder.AddMultipleAttributes(10, AdditionalAttributes);
        builder.AddAttribute(11, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        builder.AddAttribute(12, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        builder.AddElementReferenceCapture(13, elementRef => _elementRef = elementRef);
        builder.AddContent(14, ChildContent);
        builder.CloseElement();
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (IsDisabled) return;
        await Context.ToggleItemAsync(ItemContext.Value);
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (IsDisabled) return;

        // Determine navigation keys based on orientation and RTL
        var isVertical = Context.Orientation == AccordionOrientation.Vertical;

        string? targetValue = null;

        switch (args.Key)
        {
            case "ArrowUp" when isVertical:
            case "ArrowLeft" when !isVertical && !await IsRtlAsync():
            case "ArrowRight" when !isVertical && await IsRtlAsync():
                // Navigate to previous
                targetValue = Context.GetNavigationTarget(ItemContext.Value, -1);
                break;

            case "ArrowDown" when isVertical:
            case "ArrowRight" when !isVertical && !await IsRtlAsync():
            case "ArrowLeft" when !isVertical && await IsRtlAsync():
                // Navigate to next
                targetValue = Context.GetNavigationTarget(ItemContext.Value, 1);
                break;

            case "Home":
                // Navigate to first
                targetValue = Context.GetFirstTriggerValue();
                break;

            case "End":
                // Navigate to last
                targetValue = Context.GetLastTriggerValue();
                break;

            default:
                // Enter and Space are handled natively by the <button> element
                return;
        }

        if (targetValue is not null && targetValue != ItemContext.Value)
        {
            await Context.FocusTriggerAsync(targetValue);
        }
    }

    private async Task<bool> IsRtlAsync()
    {
        _cachedIsRtl ??= await SummitUtilities.IsRtlAsync();
        return _cachedIsRtl.Value;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (_isRegistered)
        {
            Context.UnregisterTrigger(ItemContext.Value);
            _isRegistered = false;
        }

        if (_isJsInitialized)
        {
            try
            {
                await JsInterop.UnregisterTriggerAsync(_elementRef);
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, ignore
            }
            catch (ObjectDisposedException)
            {
                // Component already disposed, ignore
            }
        }

        GC.SuppressFinalize(this);
    }
}
