using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// The interactive day button within a calendar cell.
/// Handles selection and keyboard navigation focus.
/// </summary>
public class CalendarDay : ComponentBase
{
    private ElementReference _elementRef;

    [CascadingParameter]
    private CalendarContext Context { get; set; } = default!;

    [CascadingParameter]
    private CalendarCellContext CellContext { get; set; } = default!;

    [Inject] private CalendarJsInterop JsInterop { get; set; } = default!;

    /// <summary>
    /// Custom content for the day button. If not provided, displays the day number.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional attributes to apply to the button element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnInitialized()
    {
        Context.OnStateChanged += HandleStateChanged;
    }

    private void HandleStateChanged()
    {
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Only set browser focus if:
        // 1. This date is the focused date in the context
        // 2. The context indicates we should programmatically focus (keyboard navigation occurred)
        // This prevents focus stealing between multiple calendars on the same page
        if (Context.ShouldFocus && Context.IsFocused(CellContext.Date))
        {
            Context.ClearShouldFocus();
            await JsInterop.FocusDateAsync(_elementRef);
        }
    }

    private async Task HandleClick(MouseEventArgs args)
    {
        if (CellContext.IsDisabled || CellContext.IsUnavailable) return;

        await Context.SelectDateAsync(CellContext.Date);
    }

    private async Task HandleKeyDown(KeyboardEventArgs args)
    {
        // Handle keyboard navigation directly on the day button
        // This ensures screen readers receive the events (they focus on buttons, not the grid)
        // Note: Works with or without RootComponent - uses CalendarContext methods directly

        switch (args.Key)
        {
            case "ArrowUp":
                Context.MoveFocusWeeks(-1);
                break;
            case "ArrowDown":
                Context.MoveFocusWeeks(1);
                break;
            case "ArrowLeft":
                Context.MoveFocus(-1);
                break;
            case "ArrowRight":
                Context.MoveFocus(1);
                break;
            case "Home":
                Context.FocusStartOfWeek();
                break;
            case "End":
                Context.FocusEndOfWeek();
                break;
            case "PageUp":
                if (args.ShiftKey)
                {
                    Context.PreviousYear();
                }
                else
                {
                    Context.PreviousMonth();
                }
                break;
            case "PageDown":
                if (args.ShiftKey)
                {
                    Context.NextYear();
                }
                else
                {
                    Context.NextMonth();
                }
                break;
            case "Enter":
            case " ":
                await Context.SelectDateAsync(Context.FocusedDate);
                break;
            default:
                // Key not handled, don't prevent default
                return;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Use fixed sequence numbers for stable render tree diffing
        // IMPORTANT: Never use dynamic seq++ with conditional blocks
        var date = CellContext.Date;
        var isSelected = Context.IsSelected(date);
        var isToday = Context.IsToday(date);
        // Get IsFocused directly from Context to ensure we have the latest state
        var isFocused = Context.IsFocused(date);
        var isOutsideMonth = Context.IsOutsideMonth(date);
        var isUnavailable = Context.IsDateUnavailable(date);
        var isDisabled = Context.Disabled;

        // Use roving tabindex: only the focused date is in tab order
        var tabIndex = isFocused ? 0 : -1;

        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "type", "button");
        builder.AddAttribute(2, "data-summit-calendar-day", true);
        builder.AddAttribute(3, "data-date", date.ToString("yyyy-MM-dd"));
        builder.AddAttribute(4, "tabindex", tabIndex);

        // ARIA attributes - always add or use null to remove
        builder.AddAttribute(5, "aria-selected", isSelected ? "true" : null);
        builder.AddAttribute(6, "aria-disabled", (isUnavailable || isDisabled) ? "true" : null);
        builder.AddAttribute(7, "aria-current", isToday ? "date" : null);

        // Accessible label with full date (localized for the current calendar system)
        var ariaLabel = Context.GetLocalizedDateString(date);
        if (isToday) ariaLabel += " (today)";
        if (isSelected) ariaLabel += " (selected)";
        builder.AddAttribute(8, "aria-label", ariaLabel);

        // Data attributes for styling - always add or use null to remove
        builder.AddAttribute(9, "data-state", isSelected ? "selected" : null);
        builder.AddAttribute(10, "data-today", isToday ? (object)true : null);
        builder.AddAttribute(11, "data-focused", isFocused ? (object)true : null);
        builder.AddAttribute(12, "data-outside-month", isOutsideMonth ? (object)true : null);
        builder.AddAttribute(13, "data-unavailable", isUnavailable ? (object)true : null);
        builder.AddAttribute(14, "data-disabled", isDisabled ? (object)true : null);

        // Disable the button if unavailable
        builder.AddAttribute(15, "disabled", (isUnavailable || isDisabled) ? (object)true : null);

        // Event handlers
        builder.AddAttribute(16, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
        builder.AddAttribute(17, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDown));

        builder.AddMultipleAttributes(18, AdditionalAttributes);
        builder.AddElementReferenceCapture(19, elementRef => _elementRef = elementRef);

        // Content: day number (converted for current calendar system) or custom content
        if (ChildContent != null)
        {
            builder.AddContent(20, ChildContent);
        }
        else
        {
            // Display the day number converted to the current calendar system
            builder.AddContent(20, Context.GetDisplayDay(date).ToString());
        }

        builder.CloseElement();
    }
}
