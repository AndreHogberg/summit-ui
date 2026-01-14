using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Individual tab trigger button. Renders with role="tab".
/// Registers with the TabsContext for keyboard navigation support.
/// </summary>
public class SmTabsTrigger : ComponentBase, IDisposable
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
    private bool _previousDisabled;
    private string _previousValue = "";
    private bool _isRegistered;

    private bool IsActive => Context.Value == Value;
    private string DataState => IsActive ? "active" : "inactive";
    private int TabIndex => IsActive ? 0 : -1;

    protected override void OnParametersSet()
    {
        // Update registration if Value changed
        if (_isRegistered && _previousValue != Value)
        {
            Context.UnregisterTrigger(_previousValue);
            _isRegistered = false;
        }

        // Update disabled state if it changed
        if (_isRegistered && _previousDisabled != Disabled)
        {
            Context.UpdateTriggerDisabled(Value, Disabled);
        }

        _previousDisabled = Disabled;
        _previousValue = Value;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        // Register with context after first render when ElementReference is available
        if (!_isRegistered)
        {
            Context.RegisterTrigger(Value, _elementRef, Disabled);
            _isRegistered = true;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "role", "tab");
        builder.AddAttribute(2, "id", Context.GetTriggerId(Value));
        builder.AddAttribute(3, "aria-selected", IsActive.ToString().ToLowerInvariant());
        builder.AddAttribute(4, "aria-controls", Context.GetContentId(Value));
        builder.AddAttribute(5, "tabindex", TabIndex);
        builder.AddAttribute(6, "data-state", DataState);
        builder.AddAttribute(7, "data-orientation", Context.Orientation.ToString().ToLowerInvariant());
        builder.AddAttribute(8, "data-summit-tabs-trigger", true);
        builder.AddAttribute(9, "data-value", Value);

        if (Disabled)
        {
            builder.AddAttribute(10, "aria-disabled", "true");
            builder.AddAttribute(11, "data-disabled", true);
        }

        builder.AddMultipleAttributes(12, AdditionalAttributes);
        builder.AddAttribute(13, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        builder.AddAttribute(14, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        builder.AddAttribute(15, "onfocus", EventCallback.Factory.Create<FocusEventArgs>(this, HandleFocusAsync));
        builder.AddElementReferenceCapture(16, elementRef => _elementRef = elementRef);
        builder.AddContent(17, ChildContent);
        builder.CloseElement();
    }

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
        // Note: Arrow keys, Home, End are handled by TabsList
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

    public void Dispose()
    {
        if (_isRegistered)
        {
            Context.UnregisterTrigger(Value);
            _isRegistered = false;
        }
    }
}
