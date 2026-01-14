using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Trigger button that opens the dialog when clicked.
/// </summary>
public class SmDialogTrigger : ComponentBase
{
    [CascadingParameter]
    private DialogContext Context { get; set; } = default!;

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

    private string DataState => Context.IsOpen ? "open" : "closed";

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
        builder.AddAttribute(1, "type", As == "button" ? "button" : null);
        builder.AddAttribute(2, "aria-haspopup", "dialog");
        builder.AddAttribute(3, "aria-expanded", Context.IsOpen.ToString().ToLowerInvariant());
        builder.AddAttribute(4, "aria-controls", Context.DialogId);
        builder.AddAttribute(5, "data-state", DataState);
        builder.AddAttribute(6, "data-summit-dialog-trigger", true);
        builder.AddMultipleAttributes(7, AdditionalAttributes);
        builder.AddAttribute(8, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        builder.AddAttribute(9, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        builder.AddElementReferenceCapture(10, elementRef => _elementRef = elementRef);
        builder.AddContent(11, ChildContent);
        builder.CloseElement();
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        await Context.OpenAsync();
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        // Only handle Enter/Space for non-button elements.
        // Button elements automatically fire a click event on Enter/Space,
        // so the click handler will take care of opening.
        if (As != "button" && args.Key is "Enter" or " ")
        {
            await Context.OpenAsync();
        }
    }
}
