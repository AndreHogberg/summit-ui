using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// A label component for DateField that provides proper accessibility association.
/// </summary>
public partial class SmDateFieldLabel : ComponentBase
{
    [CascadingParameter] public DateFieldContext Context { get; set; } = default!;

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnInitialized()
    {
        if (Context == null)
            throw new InvalidOperationException("DateFieldLabel must be used within a DateFieldRoot.");
    }
}
