using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Trigger button that opens/closes the date picker calendar popover.
/// Designed to be placed inside DatePickerField for inline styling.
/// </summary>
public class DatePickerTrigger : ComponentBase
{
    [CascadingParameter]
    private DatePickerContext Context { get; set; } = default!;

    /// <summary>
    /// Child content (typically an icon or button text).
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
            // Register as trigger for focus management
            Context.PopoverContext.RegisterTrigger(_elementRef);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Context == null)
            throw new InvalidOperationException("DatePickerTrigger must be used within a DatePickerRoot.");

        var isDisabled = Context.Disabled || Context.ReadOnly;

        builder.OpenElement(0, As);

        // ARIA attributes for accessibility
        builder.AddAttribute(1, "type", As == "button" ? "button" : null);
        builder.AddAttribute(2, "aria-haspopup", "dialog");
        builder.AddAttribute(3, "aria-expanded", Context.IsOpen.ToString().ToLowerInvariant());
        builder.AddAttribute(4, "aria-controls", Context.PopoverContext.PopoverId);
        builder.AddAttribute(5, "aria-label", "Open calendar");

        // Data attributes for styling
        builder.AddAttribute(6, "data-state", DataState);
        builder.AddAttribute(7, "data-summit-datepicker-trigger", true);

        // Disabled state
        if (isDisabled)
        {
            builder.AddAttribute(8, "disabled", true);
            builder.AddAttribute(9, "aria-disabled", "true");
            builder.AddAttribute(10, "data-disabled", "");
        }

        // Additional attributes (allows custom styling)
        builder.AddMultipleAttributes(11, AdditionalAttributes);

        // Event handlers
        if (!isDisabled)
        {
            builder.AddAttribute(12, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
            builder.AddAttribute(13, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        }

        // Element reference capture
        builder.AddElementReferenceCapture(14, elementRef => _elementRef = elementRef);

        // Content
        builder.AddContent(15, ChildContent);

        builder.CloseElement();
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        await Context.ToggleAsync();
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        // Only handle Enter/Space for non-button elements.
        // Button elements automatically fire a click event on Enter/Space.
        if (As != "button" && args.Key is "Enter" or " ")
        {
            await Context.ToggleAsync();
        }
    }

    private string DataState => Context.IsOpen ? "open" : "closed";
}
