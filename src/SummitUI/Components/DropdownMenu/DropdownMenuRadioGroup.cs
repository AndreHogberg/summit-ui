using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// A group of radio menu items where only one can be selected at a time.
/// </summary>
public class DropdownMenuRadioGroup : ComponentBase
{
    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

    /// <summary>
    /// The currently selected value.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Callback when value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Accessible label for the group.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Child content (DropdownMenuRadioItem components).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private readonly DropdownMenuRadioGroupContext _radioContext = new();

    protected override void OnInitialized()
    {
        _radioContext.Value = Value;
        _radioContext.OnValueChangeAsync = HandleValueChangeAsync;
    }

    protected override void OnParametersSet()
    {
        _radioContext.Value = Value;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<DropdownMenuRadioGroupContext>>(0);
        builder.AddComponentParameter(1, "Value", _radioContext);
        builder.AddComponentParameter(2, "IsFixed", false);
        builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(childBuilder =>
        {
            childBuilder.OpenElement(0, "div");
            childBuilder.AddAttribute(1, "role", "group");
            childBuilder.AddAttribute(2, "aria-label", AriaLabel);
            childBuilder.AddAttribute(3, "data-summit-dropdown-menu-radio-group", "");
            childBuilder.AddMultipleAttributes(4, AdditionalAttributes);
            childBuilder.AddContent(5, ChildContent);
            childBuilder.CloseElement();
        }));
        builder.CloseComponent();
    }

    private async Task HandleValueChangeAsync(string value)
    {
        Value = value;
        _radioContext.Value = value;
        await ValueChanged.InvokeAsync(value);
        StateHasChanged();
    }
}
