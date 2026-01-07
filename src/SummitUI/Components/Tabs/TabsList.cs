using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

using SummitUI.Utilities;

namespace SummitUI;

/// <summary>
/// Container for tab triggers. Renders with role="tablist".
/// Handles keyboard navigation for arrow keys, Home, and End.
/// </summary>
public class TabsList : ComponentBase, IAsyncDisposable
{
    [Inject] private SummitUtilities Utilities { get; set; } = default!;

    /// <summary>
    /// The tabs context from the parent TabsRoot.
    /// </summary>
    [CascadingParameter]
    public TabsContext Context { get; set; } = default!;

    /// <summary>
    /// Child content (TabsTrigger components).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render as.
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// Additional HTML attributes to apply.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private bool? _cachedIsRtl;

    protected override void OnInitialized()
    {
        // Register the focus callback with the context
        Context.FocusTriggerByIdAsync = FocusTriggerByIdAsync;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "role", "tablist");
        builder.AddAttribute(2, "aria-orientation", Context.Orientation.ToString().ToLowerInvariant());
        builder.AddAttribute(3, "data-orientation", Context.Orientation.ToString().ToLowerInvariant());
        builder.AddAttribute(4, "data-summit-tabs-list", true);
        builder.AddMultipleAttributes(5, AdditionalAttributes);
        builder.AddAttribute(6, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        builder.AddElementReferenceCapture(7, elementRef => _elementRef = elementRef);
        builder.AddContent(8, ChildContent);
        builder.CloseElement();
    }

    /// <summary>
    /// Handles keyboard navigation within the tabs list.
    /// </summary>
    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        // Only handle navigation keys
        if (args.Key is not ("ArrowLeft" or "ArrowRight" or "ArrowUp" or "ArrowDown" or "Home" or "End"))
            return;

        var currentValue = Context.Value;
        if (string.IsNullOrEmpty(currentValue))
            return;

        var currentIndex = Context.GetCurrentTriggerIndex(currentValue);
        if (currentIndex < 0)
            return;

        // Cache RTL check
        _cachedIsRtl ??= await Utilities.IsRtlAsync();
        var isRtl = _cachedIsRtl.Value;

        TabTriggerInfo? targetTrigger = null;

        // Determine which keys are "previous" and "next" based on orientation
        var isHorizontal = Context.Orientation == TabsOrientation.Horizontal;
        var prevKey = isHorizontal ? "ArrowLeft" : "ArrowUp";
        var nextKey = isHorizontal ? "ArrowRight" : "ArrowDown";

        switch (args.Key)
        {
            case var key when key == prevKey:
                var prevIndex = Context.GetNextTriggerIndex(currentIndex, -1, isRtl);
                targetTrigger = Context.GetTriggerAtIndex(prevIndex);
                break;

            case var key when key == nextKey:
                var nextIndex = Context.GetNextTriggerIndex(currentIndex, 1, isRtl);
                targetTrigger = Context.GetTriggerAtIndex(nextIndex);
                break;

            case "Home":
                targetTrigger = Context.GetFirstTrigger();
                break;

            case "End":
                targetTrigger = Context.GetLastTrigger();
                break;
        }

        if (targetTrigger != null && targetTrigger.Value != currentValue)
        {
            // Focus the target trigger
            var triggerId = Context.GetTriggerId(targetTrigger.Value);
            await Utilities.FocusElementByIdAsync(triggerId);

            // In auto mode, also activate the tab
            if (Context.ActivationMode == TabsActivationMode.Auto)
            {
                await Context.ActivateTabAsync(targetTrigger.Value);
            }
        }
    }

    /// <summary>
    /// Focuses a trigger element by its ID.
    /// </summary>
    private async ValueTask FocusTriggerByIdAsync(string triggerId)
    {
        await Utilities.FocusElementByIdAsync(triggerId);
    }

    public ValueTask DisposeAsync()
    {
        // Clean up the focus callback
        if (Context.FocusTriggerByIdAsync == FocusTriggerByIdAsync)
        {
            Context.FocusTriggerByIdAsync = null;
        }

        return ValueTask.CompletedTask;
    }
}
