using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SummitUI.Interop;
using SummitUI.Utilities;

namespace SummitUI;

/// <summary>
/// Accordion trigger button that toggles the associated content panel.
/// Handles keyboard navigation including arrow keys, Home, and End.
/// Supports the AsChild pattern for rendering custom elements.
/// </summary>
public partial class SmAccordionTrigger : ComponentBase, IAsyncDisposable
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
    /// When true, the component will not render a wrapper element.
    /// Instead, it passes attributes via context to the child element.
    /// The child must apply @attributes="context.Attrs" for proper functionality.
    /// </summary>
    [Parameter]
    public bool AsChild { get; set; }

    /// <summary>
    /// Child content. When AsChild is true, receives an AsChildContext with attributes to apply.
    /// </summary>
    [Parameter]
    public RenderFragment<AsChildContext>? ChildContent { get; set; }

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
    private string Orientation => Context.Orientation.ToString().ToLowerInvariant();
    private string TriggerId => Context.GetTriggerId(ItemContext.Value);
    private string ContentId => Context.GetContentId(ItemContext.Value);

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

    private IReadOnlyDictionary<string, object> BuildAttributes()
    {
        var attrs = new Dictionary<string, object>
        {
            ["type"] = "button",
            ["id"] = TriggerId,
            ["aria-expanded"] = IsExpanded.ToString().ToLowerInvariant(),
            ["aria-controls"] = ContentId,
            ["data-state"] = DataState,
            ["data-orientation"] = Orientation,
            ["data-summit-accordion-trigger"] = true,
            ["onclick"] = EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync),
            ["onkeydown"] = EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync)
        };

        if (IsDisabled)
        {
            attrs["disabled"] = true;
            attrs["data-disabled"] = true;
        }

        // Merge additional attributes (consumer attributes win)
        if (AdditionalAttributes is not null)
        {
            foreach (var (key, value) in AdditionalAttributes)
            {
                attrs[key] = value;
            }
        }

        return attrs;
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
