using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// A single menu item within the dropdown menu.
/// Implements menuitem role with full ARIA support.
/// </summary>
public partial class DropdownMenuItem : ComponentBase
{
    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

    /// <summary>
    /// Whether this item is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Callback invoked when this item is selected.
    /// </summary>
    [Parameter]
    public EventCallback OnSelect { get; set; }

    /// <summary>
    /// Callback invoked when this item is clicked.
    /// </summary>
    [Parameter]
    public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private bool IsHighlighted { get; set; }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Disabled) return;

        await OnClick.InvokeAsync(args);
        await OnSelect.InvokeAsync();
        await Context.SelectItemAsync();
    }

    private void HandleMouseEnter()
    {
        if (!Disabled)
        {
            IsHighlighted = true;
        }
    }

    private void HandleMouseLeave()
    {
        IsHighlighted = false;
    }
}
