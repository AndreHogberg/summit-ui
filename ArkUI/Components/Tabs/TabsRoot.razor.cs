using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Tabs;

/// <summary>
/// Root component that manages the state of the tabs.
/// Provides cascading context to child components.
/// </summary>
public partial class TabsRoot : ComponentBase
{
    /// <summary>
    /// Child content containing TabsList and TabsContent components.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Controlled active tab value. When provided, component operates in controlled mode.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Default active tab value for uncontrolled mode.
    /// </summary>
    [Parameter]
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Callback when active tab value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Callback invoked when a tab is activated.
    /// </summary>
    [Parameter]
    public EventCallback<string?> OnValueChange { get; set; }

    /// <summary>
    /// Orientation of the tabs (affects keyboard navigation).
    /// </summary>
    [Parameter]
    public TabsOrientation Orientation { get; set; } = TabsOrientation.Horizontal;

    /// <summary>
    /// Activation mode for tabs.
    /// Auto: activates on focus. Manual: requires click or Enter/Space.
    /// </summary>
    [Parameter]
    public TabsActivationMode ActivationMode { get; set; } = TabsActivationMode.Auto;

    /// <summary>
    /// Whether keyboard navigation loops from last to first and vice versa.
    /// </summary>
    [Parameter]
    public bool Loop { get; set; } = true;

    private readonly TabsContext _context = new();
    private string? _internalValue;

    /// <summary>
    /// Effective active tab value (controlled or uncontrolled).
    /// </summary>
    private string? ActiveValue => Value ?? _internalValue;

    protected override void OnInitialized()
    {
        _internalValue = DefaultValue;
        _context.Value = ActiveValue;
        _context.Orientation = Orientation;
        _context.ActivationMode = ActivationMode;
        _context.Loop = Loop;
        _context.ActivateTabAsync = ActivateTabAsync;
        _context.NotifyStateChanged = () => StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        // Sync context with current state
        _context.Value = ActiveValue;
        _context.Orientation = Orientation;
        _context.ActivationMode = ActivationMode;
        _context.Loop = Loop;
    }

    private async Task ActivateTabAsync(string value)
    {
        if (ActiveValue == value) return;

        // Update internal state for uncontrolled mode
        if (Value is null)
        {
            _internalValue = value;
        }

        _context.Value = value;
        await ValueChanged.InvokeAsync(value);
        await OnValueChange.InvokeAsync(value);
        StateHasChanged();
    }
}
