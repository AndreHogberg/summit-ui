using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// A visual indicator for the checkbox state.
/// Only renders when the checkbox is checked or indeterminate (unless ForceMount is true).
/// Use this to render custom check marks or indeterminate icons.
/// </summary>
public class CheckboxIndicator : ComponentBase
{
    /// <summary>
    /// The checkbox context from the parent CheckboxRoot via cascading parameter.
    /// </summary>
    [CascadingParameter]
    private CheckboxContext? CascadedContext { get; set; }

    /// <summary>
    /// Optional explicit context parameter (overrides cascaded context).
    /// </summary>
    [Parameter]
    public CheckboxContext? Context { get; set; }

    /// <summary>
    /// Child content to render inside the indicator.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// When true, the indicator is always rendered regardless of checked state.
    /// Useful for CSS-based animations.
    /// </summary>
    [Parameter]
    public bool ForceMount { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the indicator element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// The effective context (parameter takes precedence over cascaded).
    /// </summary>
    private CheckboxContext EffectiveContext => Context ?? CascadedContext ?? new CheckboxContext { State = CheckedState.Unchecked };

    /// <summary>
    /// Whether the indicator should be displayed.
    /// </summary>
    private bool ShouldDisplay => ForceMount || EffectiveContext.IsChecked || EffectiveContext.IsIndeterminate;

    /// <summary>
    /// The data-state attribute value.
    /// </summary>
    private string DataState => EffectiveContext.State switch
    {
        CheckedState.Checked => "checked",
        CheckedState.Indeterminate => "indeterminate",
        _ => "unchecked"
    };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!ShouldDisplay) return;

        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "data-summit-checkbox-indicator", "");
        builder.AddAttribute(2, "data-state", DataState);
        builder.AddMultipleAttributes(3, AdditionalAttributes);

        if (ChildContent is not null)
        {
            builder.AddContent(4, ChildContent);
        }

        builder.CloseElement();
    }
}
