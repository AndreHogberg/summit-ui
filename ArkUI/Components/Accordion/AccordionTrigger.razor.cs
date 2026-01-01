using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ArkUI.Components.Accordion;

/// <summary>
/// Accordion trigger button that toggles the associated content panel.
/// </summary>
public partial class AccordionTrigger : ComponentBase
{
    [CascadingParameter]
    private AccordionContext Context { get; set; } = default!;

    [CascadingParameter]
    private AccordionItemContext ItemContext { get; set; } = default!;

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

    private bool IsExpanded => Context.IsExpanded(ItemContext.Value);
    private bool IsDisabled => ItemContext.Disabled || Context.Disabled;
    private string DataState => IsExpanded ? "open" : "closed";

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (IsDisabled) return;
        await Context.ToggleItemAsync(ItemContext.Value);
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (IsDisabled) return;

        // Enter and Space toggle the item
        if (args.Key is "Enter" or " ")
        {
            await Context.ToggleItemAsync(ItemContext.Value);
        }
    }
}
