using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Button to navigate to the next month.
/// </summary>
public class CalendarNextButton : ComponentBase
{
    [CascadingParameter]
    private CalendarContext Context { get; set; } = default!;

    /// <summary>
    /// The child content (button label).
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Accessible label for the button.
    /// </summary>
    [Parameter] public string? AriaLabel { get; set; }

    /// <summary>
    /// Additional attributes to apply to the button element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnInitialized()
    {
        Context.OnStateChanged += StateHasChanged;
    }

    private void HandleClick(MouseEventArgs args)
    {
        if (Context.Disabled || !Context.CanNavigateNext) return;

        // NextMonth() will call RootComponent.OnMonthChanged() to update month name
        Context.NextMonth();
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Use fixed sequence numbers for stable render tree diffing
        var isDisabled = Context.Disabled || !Context.CanNavigateNext;

        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "type", "button");
        builder.AddAttribute(2, "data-summit-calendar-next-button", true);
        builder.AddAttribute(3, "aria-label", AriaLabel ?? "Next month");
        builder.AddAttribute(4, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
        builder.AddAttribute(5, "disabled", isDisabled ? (object)true : null);
        builder.AddAttribute(6, "aria-disabled", isDisabled ? "true" : null);

        builder.AddMultipleAttributes(7, AdditionalAttributes);

        if (ChildContent != null)
        {
            builder.AddContent(8, ChildContent);
        }
        else
        {
            builder.AddContent(8, "\u203A"); // Single right-pointing angle quotation mark
        }

        builder.CloseElement();
    }
}
