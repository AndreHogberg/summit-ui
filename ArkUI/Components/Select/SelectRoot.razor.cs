using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Select;

/// <summary>
/// Root component that manages the state of the select.
/// Provides cascading context to child components.
/// </summary>
public partial class SelectRoot : ComponentBase, IAsyncDisposable
{
    /// <summary>
    /// Child content containing SelectTrigger, SelectContent, etc.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Controlled selected value. When provided, component operates in controlled mode.
    /// </summary>
    [Parameter]
    public string? Value { get; set; }

    /// <summary>
    /// Default selected value for uncontrolled mode.
    /// </summary>
    [Parameter]
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Callback when selected value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> ValueChanged { get; set; }

    /// <summary>
    /// Callback invoked when value changes (alternative to ValueChanged for non-binding scenarios).
    /// </summary>
    [Parameter]
    public EventCallback<string?> OnValueChange { get; set; }

    /// <summary>
    /// Controlled open state. When provided, component operates in controlled mode for open/close.
    /// </summary>
    [Parameter]
    public bool? Open { get; set; }

    /// <summary>
    /// Default open state for uncontrolled mode.
    /// </summary>
    [Parameter]
    public bool DefaultOpen { get; set; }

    /// <summary>
    /// Callback when open state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>
    /// Whether the select is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Whether the select is required (for form validation).
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Whether the select is in an invalid state (for error styling).
    /// </summary>
    [Parameter]
    public bool Invalid { get; set; }

    /// <summary>
    /// Form field name for hidden input (enables form submission).
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    private readonly SelectContext _context = new();
    private string? _internalValue;
    private bool _internalOpen;
    private bool _isDisposed;
    private DateTime _lastSelectionCloseTime; // Timestamp to debounce re-opening

    /// <summary>
    /// Effective selected value (controlled or uncontrolled).
    /// </summary>
    private string? EffectiveValue => Value ?? _internalValue;

    /// <summary>
    /// Effective open state (controlled or uncontrolled).
    /// </summary>
    private bool IsOpen => Open ?? _internalOpen;

    protected override void OnInitialized()
    {
        _internalValue = DefaultValue;
        _internalOpen = DefaultOpen;
        
        _context.Value = EffectiveValue;
        _context.IsOpen = IsOpen;
        _context.Disabled = Disabled;
        _context.Required = Required;
        _context.Invalid = Invalid;
        
        _context.ToggleAsync = ToggleAsync;
        _context.OpenAsync = OpenAsync;
        _context.CloseAsync = CloseAsync;
        _context.SelectItemAsync = SelectItemAsync;
        _context.SetHighlightedValueAsync = SetHighlightedValueAsync;
        _context.RegisterTrigger = RegisterTrigger;
        _context.RegisterContent = RegisterContent;
        _context.NotifyStateChanged = () => StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        // Only sync from external parameters, not internal state
        // Internal state (_internalOpen) is managed by Open/Close/Toggle methods
        _context.Value = EffectiveValue;
        _context.Disabled = Disabled;
        _context.Required = Required;
        _context.Invalid = Invalid;
        
        // Only update IsOpen from external Open parameter if it's controlled
        if (Open.HasValue)
        {
            _context.IsOpen = Open.Value;
        }
        
        // For uncontrolled mode, DON'T touch IsOpen here - it's managed by Open/Close methods
        // The previous code `_context.IsOpen = _internalOpen` was causing issues because
        // OnParametersSet can be called during the CloseAsync flow (due to parent re-render
        // from ValueChanged), and at that point we'd be overwriting the context state.
    }

    private async Task ToggleAsync()
    {
        if (Disabled) return;
        
        // Prevent re-opening within 100ms of a selection close
        // This handles the race condition where keyboard selection (via JS) and
        // Blazor's keydown handler both fire, and we need to prevent the re-open
        var timeSinceClose = (DateTime.UtcNow - _lastSelectionCloseTime).TotalMilliseconds;
        if (timeSinceClose < 100)
        {
            return;
        }
        
        if (IsOpen)
            await CloseAsync();
        else
            await OpenAsync();
    }

    private async Task OpenAsync()
    {
        // Prevent re-opening within 100ms of a selection close
        var timeSinceClose = (DateTime.UtcNow - _lastSelectionCloseTime).TotalMilliseconds;
        if (timeSinceClose < 100)
        {
            return;
        }
        
        if (Disabled || IsOpen) return;

        if (Open is null)
        {
            _internalOpen = true;
        }

        _context.IsOpen = true;
        await OpenChanged.InvokeAsync(true);
        StateHasChanged();
    }

    private async Task CloseAsync()
    {
        if (!IsOpen) return;

        if (Open is null)
        {
            _internalOpen = false;
        }

        _context.IsOpen = false;
        _context.HighlightedValue = null;
        await OpenChanged.InvokeAsync(false);
        StateHasChanged();
        _context.RaiseStateChanged();
    }

    private async Task SelectItemAsync(string value, string? label)
    {
        if (Disabled) return;
        
        // Update internal state for uncontrolled mode
        if (Value is null)
        {
            _internalValue = value;
        }

        _context.Value = value;
        _context.SelectedLabel = label;
        
        // IMPORTANT: Close BEFORE firing value changed events
        // This ensures _internalOpen is false when parent re-renders (due to ValueChanged)
        // and OnParametersSet runs on this component
        await CloseAsync();
        
        // Set timestamp to prevent immediate re-opening from keyboard event handlers
        // The keyboard event (Enter/Space) that triggered selection will also fire
        // Blazor's onkeydown handler after this async method returns
        _lastSelectionCloseTime = DateTime.UtcNow;
        
        await ValueChanged.InvokeAsync(value);
        await OnValueChange.InvokeAsync(value);
    }

    private Task SetHighlightedValueAsync(string? value)
    {
        _context.HighlightedValue = value;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private void RegisterTrigger(ElementReference element)
    {
        _context.TriggerElement = element;
    }

    private void RegisterContent(ElementReference element)
    {
        _context.ContentElement = element;
    }

    public ValueTask DisposeAsync()
    {
        if (_isDisposed) return ValueTask.CompletedTask;
        _isDisposed = true;

        // Cleanup is handled by individual components
        return ValueTask.CompletedTask;
    }
}
