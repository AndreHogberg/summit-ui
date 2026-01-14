using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Displays the currently selected value or a placeholder.
/// </summary>
/// <typeparam name="TValue">The type of the select value.</typeparam>
public class SmSelectValue<TValue> : ComponentBase where TValue : notnull
{
    [CascadingParameter]
    private SelectContext<TValue> Context { get; set; } = default!;

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
    private bool IsPlaceholder => Context.Value is null;

    /// <summary>
    /// The text to display (selected label or placeholder).
    /// </summary>
    private string? DisplayText =>
        !string.IsNullOrEmpty(Context.SelectedLabel)
            ? Context.SelectedLabel
            : Context.Value is not null
                ? Context.Value.ToString()
                : Placeholder;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "data-summit-select-value", "");
        if (IsPlaceholder)
        {
            builder.AddAttribute(2, "data-placeholder", "");
        }
        if (ChildContent is not null)
        {
            builder.AddContent(3, ChildContent);
        }
        else
        {
            builder.AddContent(4, DisplayText);
        }
        builder.CloseElement();
    }
}
