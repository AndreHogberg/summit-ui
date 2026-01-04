using Microsoft.AspNetCore.Components;

namespace SummitUI.Docs.Design;

/// <summary>
/// The root table component that provides a responsive wrapper and base table styling.
/// </summary>
public partial class SuTable : ComponentBase
{
    /// <summary>
    /// The content of the table (typically SuTableHeader and SuTableBody).
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the table element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private const string WrapperClasses = "overflow-x-auto rounded-lg border border-su-border";
    private const string TableClasses = "w-full text-sm";

    private string FinalClass => SuStyles.Cn(TableClasses, UserClass);

    private string? UserClass => AdditionalAttributes?.TryGetValue("class", out var cls) == true ? cls?.ToString() : null;

    private IReadOnlyDictionary<string, object>? OtherAttributes =>
        AdditionalAttributes?.Where(x => x.Key != "class").ToDictionary(x => x.Key, x => x.Value);
}
