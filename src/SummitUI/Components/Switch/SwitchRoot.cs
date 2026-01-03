using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

public class SwitchRoot : ComponentBase
{
    /// <summary>
    /// The controlled checked state. When provided, the component operates in controlled mode.
    /// </summary>
    [Parameter] public bool? Checked { get; set; }
    
    /// <summary>
    /// The default checked state for uncontrolled mode.
    /// </summary>
    [Parameter] public bool DefaultChecked { get; set; }
    
    [Parameter] public EventCallback<bool> CheckedChanged { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private bool _internalChecked;
    private bool IsControlled => Checked.HasValue;

    private bool IsChecked => IsControlled ? (Checked ?? false) : _internalChecked;

    protected override void OnInitialized()
    {
        _internalChecked = DefaultChecked;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var context = new SwitchContext
        {
            IsChecked = IsChecked,
            IsDisabled = Disabled
        };

        builder.OpenComponent<CascadingValue<SwitchContext>>(0);
        builder.AddComponentParameter(1, "Value", context);
        builder.AddComponentParameter(2, "IsFixed", false);
        builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(childBuilder =>
        {
            childBuilder.OpenElement(0, "button");
            childBuilder.AddAttribute(1, "type", "button");
            childBuilder.AddAttribute(2, "role", "switch");
            childBuilder.AddAttribute(3, "aria-checked", IsChecked ? "true" : "false");
            childBuilder.AddAttribute(4, "data-state", IsChecked ? "checked" : "unchecked");
            
            if (Disabled)
            {
                childBuilder.AddAttribute(5, "disabled", true);
                childBuilder.AddAttribute(6, "aria-disabled", "true");
                childBuilder.AddAttribute(7, "data-disabled", "");
            }
            
            childBuilder.AddAttribute(8, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClick));
            childBuilder.AddMultipleAttributes(9, AdditionalAttributes);
            childBuilder.AddContent(10, ChildContent);
            childBuilder.CloseElement();

            if (!string.IsNullOrEmpty(Name))
            {
                childBuilder.OpenElement(11, "input");
                childBuilder.AddAttribute(12, "type", "hidden");
                childBuilder.AddAttribute(13, "name", Name);
                childBuilder.AddAttribute(14, "value", IsChecked ? (Value ?? "on") : "");
                childBuilder.AddAttribute(15, "disabled", Disabled);
                childBuilder.AddAttribute(16, "required", Required);
                childBuilder.CloseElement();
            }
        }));
        builder.CloseComponent();
    }

    private async Task HandleClick(MouseEventArgs args)
    {
        if (Disabled) return;
        
        var newChecked = !IsChecked;

        if (!IsControlled)
        {
            _internalChecked = newChecked;
            StateHasChanged();
        }

        await CheckedChanged.InvokeAsync(newChecked);
    }
}
