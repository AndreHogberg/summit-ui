using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// A backdrop overlay that covers the page when the dialog is open.
/// Clicking the overlay typically closes the dialog.
/// Supports nested dialog styling via data attributes and CSS custom properties.
/// </summary>
public class DialogOverlay : ComponentBase
{
    [CascadingParameter]
    private DialogContext Context { get; set; } = default!;

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
    /// Whether clicking the overlay closes the dialog. Defaults to true.
    /// </summary>
    [Parameter]
    public bool CloseOnClick { get; set; } = true;

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string DataState => Context.IsOpen ? "open" : "closed";

    /// <summary>
    /// CSS custom properties for nested dialog styling.
    /// </summary>
    private string CssVariables =>
        $"--summit-dialog-depth: {Context.Depth}; --summit-dialog-nested-count: {Context.NestedOpenCount};";

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Only render when open or during close animation so CSS animate-out classes can run
        if (!Context.IsOpen && !Context.IsAnimatingClosed) return;

        builder.OpenElement(0, As);
        builder.AddAttribute(1, "data-state", DataState);
        builder.AddAttribute(2, "data-summit-dialog-overlay", true);
        builder.AddAttribute(3, "aria-hidden", "true");

        // Add nested dialog data attributes
        if (Context.IsNested)
        {
            builder.AddAttribute(4, "data-nested", true);
        }

        if (Context.HasNestedOpen)
        {
            builder.AddAttribute(5, "data-nested-open", true);
        }

        builder.AddAttribute(6, "style", CssVariables);
        builder.AddMultipleAttributes(7, AdditionalAttributes);
        builder.AddAttribute(8, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        builder.AddContent(9, ChildContent);
        builder.CloseElement();
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        await OnClick.InvokeAsync(args);

        if (CloseOnClick)
        {
            await Context.CloseAsync();
        }
    }
}
