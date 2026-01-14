using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Trigger button that toggles the popover open/closed.
/// </summary>
public class SmPopoverTrigger : ComponentBase
{
    [CascadingParameter]
    private PopoverContext Context { get; set; } = default!;

    /// <summary>
    /// Child content (typically button text/icon).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "button".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "button";

    /// <summary>
    /// Additional HTML attributes to apply to the element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Context.RegisterTrigger(_elementRef);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "aria-haspopup", "dialog");
        builder.AddAttribute(2, "aria-expanded", Context.IsOpen.ToString().ToLowerInvariant());
        builder.AddAttribute(3, "aria-controls", Context.PopoverId);
        builder.AddAttribute(4, "data-state", DataState);
        builder.AddAttribute(5, "data-summit-popover-trigger", true);
        builder.AddMultipleAttributes(6, AdditionalAttributes);
        builder.AddAttribute(7, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        builder.AddAttribute(8, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        builder.AddElementReferenceCapture(9, (elementRef) => { _elementRef = elementRef; });
        builder.AddContent(10, ChildContent);
        builder.CloseElement();
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        await Context.ToggleAsync();
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        // Only handle Enter/Space for non-button elements.
        // Button elements automatically fire a click event on Enter/Space,
        // so the click handler will take care of toggling.
        if (As != "button" && args.Key is "Enter" or " ")
        {
            await Context.ToggleAsync();
        }
    }

    private string DataState => Context.IsOpen ? "open" : "closed";
}
