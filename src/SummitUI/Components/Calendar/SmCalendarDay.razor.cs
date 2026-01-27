using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// The interactive day button within a calendar cell.
/// Handles selection and keyboard navigation focus.
/// </summary>
public partial class SmCalendarDay
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
                    Context.MoveFocusYears(-1);
                }
                else
                {
                    Context.MoveFocusMonths(-1);
                }
                break;
            case "PageDown":
                if (args.ShiftKey)
                {
                    Context.MoveFocusYears(1);
                }
                else
                {
                    Context.MoveFocusMonths(1);
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
}
