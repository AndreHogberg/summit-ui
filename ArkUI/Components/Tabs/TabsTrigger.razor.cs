using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ArkUI.Components.Tabs;

/// <summary>
/// Individual tab trigger button. Renders with role="tab".
/// </summary>
public partial class TabsTrigger : ComponentBase
{
    [CascadingParameter]
    private TabsContext Context { get; set; } = default!;

    /// <summary>
    /// Unique value identifying this tab. Required.
    /// </summary>
    [Parameter, EditorRequired]
    public string Value { get; set; } = "";

    /// <summary>
    /// Child content (tab label).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "button".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "button";

    /// <summary>
    /// Whether this tab is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;

    private bool IsActive => Context.Value == Value;
    private string DataState => IsActive ? "active" : "inactive";
    private int TabIndex => IsActive ? 0 : -1;

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Disabled) return;
        await Context.ActivateTabAsync(Value);
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (Disabled) return;

        // Enter and Space activate in both auto and manual modes
        if (args.Key is "Enter" or " ")
        {
            await Context.ActivateTabAsync(Value);
        }
    }

    private async Task HandleFocusAsync(FocusEventArgs args)
    {
        if (Disabled) return;

        // In auto mode, activate tab when focused via keyboard navigation
        if (Context.ActivationMode == TabsActivationMode.Auto)
        {
            await Context.ActivateTabAsync(Value);
        }
    }
}
