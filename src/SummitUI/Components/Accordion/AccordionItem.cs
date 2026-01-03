using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Individual accordion item container.
/// Contains AccordionHeader, AccordionTrigger, and AccordionContent.
/// </summary>
public class AccordionItem : ComponentBase
{
    [CascadingParameter]
    private AccordionContext Context { get; set; } = default!;

    /// <summary>
    /// Unique value identifying this accordion item. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public string Value { get; set; } = "";

    /// <summary>
    /// Child content (AccordionHeader and AccordionContent).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// Whether this specific item is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private AccordionItemContext _itemContext = null!;

    private bool IsExpanded => Context.IsExpanded(Value);
    private bool IsDisabled => Disabled || Context.Disabled;
    private string DataState => IsExpanded ? "open" : "closed";

    protected override void OnInitialized()
    {
        _itemContext = new AccordionItemContext(Value)
        {
            Disabled = IsDisabled
        };
    }

    protected override void OnParametersSet()
    {
        if (_itemContext is not null)
        {
            _itemContext.Disabled = IsDisabled;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "data-state", DataState);
        builder.AddAttribute(2, "data-orientation", Context.Orientation.ToString().ToLowerInvariant());
        builder.AddAttribute(3, "data-ark-accordion-item", true);

        if (IsDisabled)
        {
            builder.AddAttribute(4, "data-disabled", true);
        }

        builder.AddMultipleAttributes(5, AdditionalAttributes);

        // Cascade item context to children
        builder.OpenComponent<CascadingValue<AccordionItemContext>>(6);
        builder.AddComponentParameter(7, "Value", _itemContext);
        builder.AddComponentParameter(8, "IsFixed", false);
        builder.AddComponentParameter(9, "ChildContent", ChildContent);
        builder.CloseComponent();

        builder.CloseElement();
    }
}
