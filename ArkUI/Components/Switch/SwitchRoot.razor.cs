using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ArkUI.Components.Switch;

public partial class SwitchRoot : ComponentBase
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

    private SwitchContext Context => new()
    {
        IsChecked = IsChecked,
        IsDisabled = Disabled
    };
}
