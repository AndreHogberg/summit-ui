using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Visual indicator for a radio item. Renders only when the parent item is checked,
/// unless ForceMount is true.
/// </summary>
public partial class SmRadioGroupIndicator
{
    /// <summary>
    /// The cascaded item context from RadioGroupItem.
    /// </summary>
    [CascadingParameter]
    private RadioGroupItemContext? ItemContext { get; set; }

    /// <summary>
    /// When true, the indicator is always rendered regardless of checked state.
    /// Useful for animations that need the element to exist.
    /// </summary>
    [Parameter]
    public bool ForceMount { get; set; }

    /// <summary>
    /// Child content to render inside the indicator.
    /// Typically a check icon or dot.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the indicator element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Whether the indicator should be displayed.
    /// </summary>
    private bool ShouldDisplay => ForceMount || (ItemContext?.Checked ?? false);

    /// <summary>
    /// The data-state attribute value.
    /// </summary>
    private string DataState => (ItemContext?.Checked ?? false) ? "checked" : "unchecked";
}
