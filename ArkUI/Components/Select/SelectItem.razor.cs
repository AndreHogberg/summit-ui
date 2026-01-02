using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Select;

/// <summary>
/// A selectable option within the select dropdown.
/// Implements option role with full ARIA support.
/// </summary>
public partial class SelectItem : ComponentBase
{
    [CascadingParameter]
    private SelectContext Context { get; set; } = default!;

    /// <summary>
    /// The value of this item (required, used for selection).
    /// </summary>
    [Parameter, EditorRequired]
    public string Value { get; set; } = default!;

    /// <summary>
    /// The label of this item (used for typeahead and display).
    /// If not provided, falls back to text content.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

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
    /// Child content (typically SelectItemText).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Whether this item is currently selected.
    /// </summary>
    private bool IsSelected => Context.Value == Value;

    /// <summary>
    /// Data state for styling (checked or unchecked).
    /// </summary>
    private string DataState => IsSelected ? "checked" : "unchecked";

    /// <summary>
    /// Effective label (explicit Label parameter or fallback to Value).
    /// </summary>
    private string EffectiveLabel => Label ?? Value;
}
