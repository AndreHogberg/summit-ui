using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Tab content panel. Renders with role="tabpanel".
/// Only renders when the associated tab is active.
/// </summary>
public partial class SmTabsContent
{
    [CascadingParameter]
    private TabsContext Context { get; set; } = default!;

    /// <summary>
    /// Value matching the associated TabsTrigger. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public string Value { get; set; } = "";

    /// <summary>
    /// Panel content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private bool IsActive => Context.Value == Value;
    private string DataState => IsActive ? "active" : "inactive";
}
