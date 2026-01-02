using ArkUI.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ArkUI.Components.Accordion;

/// <summary>
/// Accordion trigger button that toggles the associated content panel.
/// Handles keyboard navigation including arrow keys, Home, and End.
/// </summary>
public partial class AccordionTrigger : ComponentBase, IDisposable
{
    [CascadingParameter]
    private AccordionContext Context { get; set; } = default!;

    [CascadingParameter]
    private AccordionItemContext ItemContext { get; set; } = default!;

    [Inject]
    private ArkUtilities ArkUtilities { get; set; } = default!;

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

    protected override void OnParametersSet()
    {
        // Update disabled state in context when parameters change
        if (_isRegistered)
        {
            Context.UpdateTriggerDisabled(ItemContext.Value, IsDisabled);
        }
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
        _cachedIsRtl ??= await ArkUtilities.IsRtlAsync();
        return _cachedIsRtl.Value;
    }

    public void Dispose()
    {
        if (_isRegistered)
        {
            Context.UnregisterTrigger(ItemContext.Value);
            _isRegistered = false;
        }
        GC.SuppressFinalize(this);
    }
}
