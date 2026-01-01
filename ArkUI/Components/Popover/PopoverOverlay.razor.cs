using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ArkUI.Components.Popover;

/// <summary>
/// Optional backdrop overlay for modal-like popover behavior.
/// Clicking the overlay typically closes the popover.
/// </summary>
public partial class PopoverOverlay : ComponentBase
{
    [CascadingParameter]
    private PopoverContext Context { get; set; } = default!;

    /// <summary>
    /// Optional child content for the overlay.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// Callback when the overlay is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Whether clicking the overlay closes the popover. Defaults to true.
    /// </summary>
    [Parameter]
    public bool CloseOnClick { get; set; } = true;

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string DataState => Context.IsOpen ? "open" : "closed";

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        await OnClick.InvokeAsync(args);

        if (CloseOnClick)
        {
            await Context.CloseAsync();
        }
    }
}
