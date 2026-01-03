using ArkUI.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace ArkUI;

/// <summary>
/// Accordion content panel. Renders with role="region".
/// Only renders when the associated item is expanded (unless ForceMount is true).
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
    private bool _wasExpanded;

    private bool IsExpanded => Context.IsExpanded(ItemContext.Value);
    private string DataState => IsExpanded ? "open" : "closed";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Set CSS variable for content height when expanded (for animations)
        if (IsExpanded && !_wasExpanded)
        {
            await JsInterop.SetContentHeightAsync(_elementRef);
        }

        _wasExpanded = IsExpanded;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!IsExpanded && !ForceMount) return;

        builder.OpenElement(0, As);
        builder.AddAttribute(1, "role", "region");
        builder.AddAttribute(2, "id", Context.GetContentId(ItemContext.Value));
        builder.AddAttribute(3, "aria-labelledby", Context.GetTriggerId(ItemContext.Value));
        builder.AddAttribute(4, "data-state", DataState);
        builder.AddAttribute(5, "data-orientation", Context.Orientation.ToString().ToLowerInvariant());
        builder.AddAttribute(6, "data-ark-accordion-content", true);

        if (!IsExpanded)
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
        // Cleanup if needed
        GC.SuppressFinalize(this);
        await Task.CompletedTask;
    }
}
