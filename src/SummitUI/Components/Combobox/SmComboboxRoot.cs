using System.Linq.Expressions;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Root component that manages the state of the multi-select combobox.
/// Provides cascading context to child components.
/// </summary>
/// <typeparam name="TValue">The type of the combobox values.</typeparam>
public class SmComboboxRoot<TValue> : ComponentBase, IAsyncDisposable where TValue : notnull
{
    /// <summary>
    /// Child content containing ComboboxTrigger, ComboboxContent, etc.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Controlled selected values. When provided, component operates in controlled mode.
    /// </summary>
    [Parameter]
    public IReadOnlyCollection<TValue>? Values { get; set; }

    /// <summary>
    /// Default selected values for uncontrolled mode.
    /// </summary>
    [Parameter]
    public IReadOnlyCollection<TValue>? DefaultValues { get; set; }

    /// <summary>
    /// Callback when selected values change.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyCollection<TValue>> ValuesChanged { get; set; }

    /// <summary>
    /// Expression identifying the bound values (for EditForm validation).
    /// </summary>
    [Parameter]
    public Expression<Func<IReadOnlyCollection<TValue>?>>? ValuesExpression { get; set; }

    /// <summary>
    /// Callback invoked when values change (alternative to ValuesChanged for non-binding scenarios).
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyCollection<TValue>> OnValuesChange { get; set; }

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
    /// Whether the combobox is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Whether the combobox is required (for form validation).
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Whether the combobox is in an invalid state (for error styling).
    /// </summary>
    [Parameter]
    public bool Invalid { get; set; }

    /// <summary>
    /// Form field name for hidden input (enables form submission).
    /// </summary>
    [Parameter]
    public string? Name { get; set; }

    /// <summary>
    /// Whether to close the popup after an item is selected/deselected.
    /// Defaults to false for multi-select behavior.
    /// </summary>
    [Parameter]
    public bool CloseOnSelect { get; set; } = false;

    /// <summary>
    /// Cascading EditContext for form integration.
    /// </summary>
    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    private readonly ComboboxContext<TValue> _context = new();
    private HashSet<TValue> _internalValues = new();
    private bool _internalOpen;
    private bool _isDisposed;
    private FieldIdentifier? _fieldIdentifier;

    /// <summary>
    /// Effective selected values (controlled or uncontrolled).
    /// </summary>
    private HashSet<TValue> EffectiveValues => Values is not null 
        ? new HashSet<TValue>(Values) 
        : _internalValues;

    /// <summary>
    /// Effective open state (controlled or uncontrolled).
    /// </summary>
    private bool IsOpen => Open ?? _internalOpen;

    protected override void OnInitialized()
    {
        if (DefaultValues is not null)
        {
            _internalValues = new HashSet<TValue>(DefaultValues);
        }
        _internalOpen = DefaultOpen;

        // Set up EditContext field identifier for validation
        if (EditContext is not null && ValuesExpression is not null)
        {
            _fieldIdentifier = FieldIdentifier.Create(ValuesExpression);
        }

        SyncContextValues();
        _context.IsOpen = IsOpen;
        _context.Disabled = Disabled;
        _context.Required = Required;
        _context.Invalid = Invalid;

        _context.ToggleAsync = ToggleAsync;
        _context.OpenAsync = OpenAsync;
        _context.CloseAsync = CloseAsync;
        _context.ToggleItemByKeyAsync = ToggleItemByKeyAsync;
        _context.SelectItemByKeyAsync = SelectItemByKeyAsync;
        _context.DeselectItemByKeyAsync = DeselectItemByKeyAsync;
        _context.DeselectValueAsync = DeselectValueAsync;
        _context.ClearAsync = ClearAsync;
        _context.SetHighlightedKeyAsync = SetHighlightedKeyAsync;
        _context.SetFilterTextAsync = SetFilterTextAsync;
        _context.RegisterTrigger = RegisterTrigger;
        _context.RegisterInput = RegisterInput;
        _context.RegisterContent = RegisterContent;
        _context.NotifyStateChanged = () => StateHasChanged();
    }

    protected override void OnParametersSet()
    {
        SyncContextValues();
        _context.Disabled = Disabled;
        _context.Required = Required;
        _context.Invalid = Invalid;

        // Only update IsOpen from external Open parameter if it's controlled
        if (Open.HasValue)
        {
            _context.IsOpen = Open.Value;
        }
    }

    private void SyncContextValues()
    {
        _context.SelectedValues.Clear();
        foreach (var value in EffectiveValues)
        {
            _context.SelectedValues.Add(value);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<ComboboxContext<TValue>>>(0);
        builder.AddComponentParameter(1, "Value", _context);
        builder.AddComponentParameter(2, "IsFixed", false);
        builder.AddComponentParameter(3, "ChildContent", (RenderFragment)(childBuilder =>
        {
            childBuilder.AddContent(0, ChildContent);

            // Render hidden inputs for form submission
            if (!string.IsNullOrEmpty(Name))
            {
                var index = 1;
                foreach (var value in EffectiveValues)
                {
                    childBuilder.OpenElement(index++, "input");
                    childBuilder.AddAttribute(index++, "type", "hidden");
                    childBuilder.AddAttribute(index++, "name", Name);
                    childBuilder.AddAttribute(index++, "value", value?.ToString());
                    childBuilder.CloseElement();
                }
            }
        }));
        builder.CloseComponent();
    }

    private async Task ToggleAsync()
    {
        if (Disabled) return;

        if (IsOpen)
            await CloseAsync();
        else
            await OpenAsync();
    }

    private async Task OpenAsync()
    {
        if (Disabled || IsOpen) return;

        // Clear the item registry before opening to ensure items register in correct DOM order.
        _context.ClearItemRegistry();

        if (Open is null)
        {
            _internalOpen = true;
        }

        _context.IsOpen = true;
        
        // Highlight first visible item when opening (will be set after items render)
        // Note: Items register on render, so we trigger state change which will re-render
        // and the filter will auto-highlight first item if any filter is applied
        
        await OpenChanged.InvokeAsync(true);
        StateHasChanged();
    }

    private async Task CloseAsync()
    {
        if (!IsOpen) return;

        // Focus trigger or input BEFORE closing, as content may be unmounted after close
        if (_context.HasInput)
        {
            await _context.FocusInputAsync();
        }
        else
        {
            await _context.FocusTriggerAsync();
        }

        if (Open is null)
        {
            _internalOpen = false;
        }

        _context.IsOpen = false;
        _context.IsAnimatingClosed = true; // Set BEFORE StateHasChanged so Portal stays rendered
        _context.HighlightedKey = null;
        _context.FilterText = ""; // Clear filter on close
        await OpenChanged.InvokeAsync(false);
        StateHasChanged();
        _context.RaiseStateChanged();
    }

    private async Task ToggleItemByKeyAsync(string key)
    {
        if (_context.ItemRegistry.TryGetValue(key, out var value))
        {
            if (_context.SelectedValues.Contains(value))
            {
                await DeselectValueInternalAsync(value);
            }
            else
            {
                await SelectValueInternalAsync(value);
            }
        }
    }

    private async Task SelectItemByKeyAsync(string key)
    {
        if (_context.ItemRegistry.TryGetValue(key, out var value))
        {
            await SelectValueInternalAsync(value);
        }
    }

    private async Task DeselectItemByKeyAsync(string key)
    {
        if (_context.ItemRegistry.TryGetValue(key, out var value))
        {
            await DeselectValueInternalAsync(value);
        }
    }

    private Task DeselectValueAsync(TValue value)
    {
        return DeselectValueInternalAsync(value);
    }

    private async Task SelectValueInternalAsync(TValue value)
    {
        if (Disabled) return;
        if (_context.SelectedValues.Contains(value)) return;

        // Update internal state for uncontrolled mode
        if (Values is null)
        {
            _internalValues.Add(value);
        }

        _context.SelectedValues.Add(value);

        await NotifyValuesChangedAsync();

        if (CloseOnSelect)
        {
            await CloseAsync();
        }
        else
        {
            // Clear filter after selection for a fresh slate
            if (_context.HasInput && !string.IsNullOrEmpty(_context.FilterText))
            {
                _context.FilterText = "";
                HighlightFirstVisibleItem();
            }
            
            // Focus input after selection (multi-select keeps dropdown open)
            if (_context.HasInput)
            {
                await _context.FocusInputAsync();
            }
            StateHasChanged();
            _context.RaiseStateChanged();
        }
    }

    private async Task DeselectValueInternalAsync(TValue value)
    {
        if (Disabled) return;
        if (!_context.SelectedValues.Contains(value)) return;

        // Update internal state for uncontrolled mode
        if (Values is null)
        {
            _internalValues.Remove(value);
        }

        _context.SelectedValues.Remove(value);

        await NotifyValuesChangedAsync();

        if (CloseOnSelect)
        {
            await CloseAsync();
        }
        else
        {
            // Focus input after deselection (multi-select keeps dropdown open)
            if (_context.HasInput)
            {
                await _context.FocusInputAsync();
            }
            StateHasChanged();
            _context.RaiseStateChanged();
        }
    }

    private async Task ClearAsync()
    {
        if (Disabled) return;
        if (_context.SelectedValues.Count == 0) return;

        // Update internal state for uncontrolled mode
        if (Values is null)
        {
            _internalValues.Clear();
        }

        _context.SelectedValues.Clear();

        await NotifyValuesChangedAsync();
        StateHasChanged();
        _context.RaiseStateChanged();
    }

    private async Task NotifyValuesChangedAsync()
    {
        var values = _context.SelectedValues.ToList().AsReadOnly();
        await ValuesChanged.InvokeAsync(values);
        await OnValuesChange.InvokeAsync(values);

        // Notify EditContext of the change for validation
        if (EditContext is not null && _fieldIdentifier.HasValue)
        {
            EditContext.NotifyFieldChanged(_fieldIdentifier.Value);
        }
    }

    private Task SetHighlightedKeyAsync(string? key)
    {
        if (_context.HighlightedKey != key)
        {
            _context.HighlightedKey = key;
            _context.RaiseStateChanged();
        }
        return Task.CompletedTask;
    }

    private Task SetFilterTextAsync(string text)
    {
        if (_context.FilterText != text)
        {
            _context.FilterText = text;
            
            // Auto-highlight first visible item when filter changes
            HighlightFirstVisibleItem();
            
            _context.RaiseStateChanged();
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Highlights the first visible (non-disabled) item in the filtered list.
    /// </summary>
    private void HighlightFirstVisibleItem()
    {
        string? firstVisibleKey = null;
        foreach (var key in _context.ItemRegistry.Keys)
        {
            if (_context.MatchesFilter(key) && 
                (!_context.DisabledRegistry.TryGetValue(key, out var disabled) || !disabled))
            {
                firstVisibleKey = key;
                break;
            }
        }
        _context.HighlightedKey = firstVisibleKey;
    }

    private void RegisterTrigger(ElementReference element)
    {
        _context.TriggerElement = element;
    }

    private void RegisterInput(ElementReference element)
    {
        _context.InputElement = element;
        _context.HasInput = true;
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
