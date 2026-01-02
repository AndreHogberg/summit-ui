using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// A checkbox menu item that can be toggled.
/// </summary>
public partial class DropdownMenuCheckboxItem : ComponentBase
{
    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

    /// <summary>
    /// The checked state of the checkbox.
    /// </summary>
    [Parameter]
    public bool Checked { get; set; }

    /// <summary>
    /// Callback when checked state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }

    /// <summary>
    /// The indeterminate state of the checkbox.
    /// </summary>
    [Parameter]
    public bool Indeterminate { get; set; }

    /// <summary>
    /// Callback when indeterminate state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IndeterminateChanged { get; set; }

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
    /// Child content with checkbox context.
    /// </summary>
    [Parameter]
    public RenderFragment<DropdownMenuCheckboxItemContext>? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private bool IsHighlighted { get; set; }

    private string AriaChecked => Indeterminate ? "mixed" : Checked.ToString().ToLowerInvariant();
    private string DataState => Indeterminate ? "indeterminate" : (Checked ? "checked" : "unchecked");

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Disabled) return;

        // Clear indeterminate state on click
        if (Indeterminate)
        {
            Indeterminate = false;
            await IndeterminateChanged.InvokeAsync(false);
        }

        // Toggle checked state
        Checked = !Checked;
        await CheckedChanged.InvokeAsync(Checked);
        await OnSelect.InvokeAsync();

        // Don't close menu for checkbox items
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

/// <summary>
/// Context passed to checkbox item child content.
/// </summary>
public class DropdownMenuCheckboxItemContext
{
    /// <summary>
    /// Whether the checkbox is checked.
    /// </summary>
    public bool Checked { get; init; }

    /// <summary>
    /// Whether the checkbox is in indeterminate state.
    /// </summary>
    public bool Indeterminate { get; init; }
}
