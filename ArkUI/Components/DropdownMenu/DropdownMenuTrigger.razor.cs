using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// Trigger button that toggles the dropdown menu open/closed.
/// </summary>
public partial class DropdownMenuTrigger : ComponentBase
{
    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

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

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        await Context.ToggleAsync();
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        // Note: Enter and Space are NOT handled here.
        // The browser's default behavior for buttons triggers onclick for these keys,
        // which calls HandleClickAsync. Handling them here would cause double-toggle.
        switch (args.Key)
        {
            case "ArrowDown":
                // Open menu and focus first item
                if (!Context.IsOpen)
                {
                    await Context.OpenAsync();
                }
                break;
            case "ArrowUp":
                // Open menu and focus last item
                if (!Context.IsOpen)
                {
                    await Context.OpenAsync();
                }
                break;
        }
    }

    private string DataState => Context.IsOpen ? "open" : "closed";
}
