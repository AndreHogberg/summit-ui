using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ArkUI.Components.DropdownMenu;

/// <summary>
/// A radio menu item within a radio group.
/// </summary>
public partial class DropdownMenuRadioItem : ComponentBase
{
    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

    [CascadingParameter]
    private DropdownMenuRadioGroupContext RadioContext { get; set; } = default!;

    /// <summary>
    /// The value of this radio item.
    /// </summary>
    [Parameter, EditorRequired]
    public string Value { get; set; } = default!;

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
    /// Child content with radio context.
    /// </summary>
    [Parameter]
    public RenderFragment<DropdownMenuRadioItemContext>? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private bool IsHighlighted { get; set; }
    private bool IsSelected => RadioContext.Value == Value;
    private string DataState => IsSelected ? "checked" : "unchecked";

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Disabled) return;

        await RadioContext.OnValueChangeAsync(Value);
        await OnSelect.InvokeAsync();

        // Don't close menu for radio items
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
/// Context passed to radio item child content.
/// </summary>
public class DropdownMenuRadioItemContext
{
    /// <summary>
    /// Whether the radio item is selected.
    /// </summary>
    public bool Checked { get; init; }
}
