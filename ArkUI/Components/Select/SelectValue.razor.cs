using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Select;

/// <summary>
/// Displays the currently selected value or a placeholder.
/// </summary>
public partial class SelectValue : ComponentBase
{
    [CascadingParameter]
    private SelectContext Context { get; set; } = default!;

    /// <summary>
    /// Placeholder text to display when no value is selected.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// Optional custom content to render instead of the automatic value display.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Whether the placeholder is currently being displayed.
    /// </summary>
    private bool IsPlaceholder => string.IsNullOrEmpty(Context.Value);

    /// <summary>
    /// The text to display (selected label or placeholder).
    /// </summary>
    private string? DisplayText => 
        !string.IsNullOrEmpty(Context.SelectedLabel) 
            ? Context.SelectedLabel 
            : !string.IsNullOrEmpty(Context.Value)
                ? Context.Value
                : Placeholder;
}
