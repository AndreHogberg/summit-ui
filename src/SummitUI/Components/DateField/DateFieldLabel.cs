using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// A label component for DateField that provides proper accessibility association.
/// </summary>
public class DateFieldLabel : ComponentBase
{
    [CascadingParameter] public DateFieldContext Context { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Context == null)
            throw new InvalidOperationException("DateFieldLabel must be used within a DateFieldRoot.");

        builder.OpenElement(0, "label");
        builder.AddAttribute(1, "id", Context.LabelId);
        builder.AddAttribute(2, "for", Context.Id);
        builder.AddMultipleAttributes(3, AdditionalAttributes);
        builder.AddContent(4, ChildContent);
        builder.CloseElement();
    }
}
