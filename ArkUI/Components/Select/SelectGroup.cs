using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ArkUI;

/// <summary>
/// Groups related select items together with an optional label.
/// </summary>
/// <typeparam name="TValue">The type of the select value.</typeparam>
public class SelectGroup<TValue> : ComponentBase where TValue : notnull
{
    [CascadingParameter]
    private SelectContext<TValue> Context { get; set; } = default!;

    /// <summary>
    /// Child content (SelectGroupLabel and SelectItem components).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private readonly string _groupId = Guid.NewGuid().ToString("N");
    private SelectGroupContext _groupContext = default!;

    /// <summary>
    /// ID for the group label element (for aria-labelledby).
    /// </summary>
    private string LabelId => Context.GetGroupLabelId(_groupId);

    protected override void OnInitialized()
    {
        _groupContext = new SelectGroupContext
        {
            GroupId = _groupId,
            LabelId = LabelId
        };
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<SelectGroupContext>>(0);
        builder.AddComponentParameter(1, "Value", _groupContext);
        builder.AddComponentParameter(2, "IsFixed", true);
        builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(childBuilder =>
        {
            childBuilder.OpenElement(0, "div");
            childBuilder.AddAttribute(1, "role", "group");
            childBuilder.AddAttribute(2, "aria-labelledby", LabelId);
            childBuilder.AddAttribute(3, "data-ark-select-group", "");
            childBuilder.AddMultipleAttributes(4, AdditionalAttributes);
            childBuilder.AddContent(5, ChildContent);
            childBuilder.CloseElement();
        }));
        builder.CloseComponent();
    }
}

/// <summary>
/// Context for select group, passed to child components.
/// </summary>
public sealed class SelectGroupContext
{
    /// <summary>
    /// Unique identifier for this group.
    /// </summary>
    public string GroupId { get; init; } = default!;

    /// <summary>
    /// ID for the group label element.
    /// </summary>
    public string LabelId { get; init; } = default!;
}
