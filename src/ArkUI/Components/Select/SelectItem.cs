using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace ArkUI;

/// <summary>
/// A selectable option within the select dropdown.
/// Implements option role with full ARIA support.
/// </summary>
/// <typeparam name="TValue">The type of the select value.</typeparam>
public class SelectItem<TValue> : ComponentBase, IDisposable where TValue : notnull
{
    [CascadingParameter]
    private SelectContext<TValue> Context { get; set; } = default!;

    /// <summary>
    /// The value of this item (required, used for selection).
    /// </summary>
    [Parameter, EditorRequired]
    public TValue Value { get; set; } = default!;

    /// <summary>
    /// Optional string key for JS interop. Defaults to Value.ToString().
    /// Use this when Value.ToString() is not unique or not suitable for DOM attributes.
    /// </summary>
    [Parameter]
    public string? Key { get; set; }

    /// <summary>
    /// The label of this item (used for typeahead and display).
    /// If not provided, falls back to Value.ToString().
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Whether this item is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Callback invoked when this item is selected.
    /// </summary>
    [Parameter]
    public EventCallback OnSelect { get; set; }

    /// <summary>
    /// Child content (typically SelectItemText).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private string? _registeredKey;
    private bool _isSubscribed;

    /// <summary>
    /// The effective key used for JS interop and item identification.
    /// </summary>
    private string EffectiveKey => Key ?? Value?.ToString() ?? "";

    /// <summary>
    /// Effective label (explicit Label parameter or fallback to Value.ToString()).
    /// </summary>
    private string EffectiveLabel => Label ?? Value?.ToString() ?? "";

    /// <summary>
    /// Whether this item is currently selected.
    /// </summary>
    private bool IsSelected => Context.Value is not null && 
                               EqualityComparer<TValue>.Default.Equals(Context.Value, Value);

    /// <summary>
    /// Whether this item is currently highlighted (keyboard navigation).
    /// </summary>
    private bool IsHighlighted => Context.HighlightedKey == EffectiveKey;

    /// <summary>
    /// Data state for styling (checked or unchecked).
    /// </summary>
    private string DataState => IsSelected ? "checked" : "unchecked";

    protected override void OnInitialized()
    {
        RegisterWithContext();
        
        // Subscribe to context state changes for highlighting updates
        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override void OnParametersSet()
    {
        // Re-register if key changed
        var currentKey = EffectiveKey;
        if (_registeredKey != currentKey)
        {
            UnregisterFromContext();
            RegisterWithContext();
        }
        else
        {
            // Update the value, label, and disabled state in case they changed
            Context.RegisterItem(currentKey, Value, EffectiveLabel, Disabled);
        }
    }

    private void RegisterWithContext()
    {
        var key = EffectiveKey;
        Context.RegisterItem(key, Value, EffectiveLabel, Disabled);
        _registeredKey = key;
    }

    private void UnregisterFromContext()
    {
        if (_registeredKey is not null)
        {
            Context.UnregisterItem(_registeredKey);
            _registeredKey = null;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "role", "option");
        builder.AddAttribute(2, "id", Context.GetItemId(EffectiveKey));
        builder.AddAttribute(3, "aria-selected", IsSelected ? "true" : "false");
        if (Disabled)
        {
            builder.AddAttribute(4, "aria-disabled", "true");
        }
        builder.AddAttribute(5, "data-ark-select-item", "");
        builder.AddAttribute(6, "data-value", EffectiveKey);
        builder.AddAttribute(7, "data-label", EffectiveLabel);
        builder.AddAttribute(8, "data-state", DataState);
        if (Disabled)
        {
            builder.AddAttribute(9, "data-disabled", "");
        }
        if (IsHighlighted)
        {
            builder.AddAttribute(10, "data-highlighted", "");
        }
        builder.AddAttribute(11, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        if (!Disabled)
        {
            builder.AddEventPreventDefaultAttribute(12, "onclick", true);
        }
        builder.AddAttribute(13, "onmouseenter", EventCallback.Factory.Create<MouseEventArgs>(this, HandleMouseEnterAsync));
        builder.AddMultipleAttributes(14, AdditionalAttributes);
        builder.AddContent(15, ChildContent);
        builder.CloseElement();
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Disabled) return;

        await Context.SelectItemByKeyAsync(EffectiveKey);
        await OnSelect.InvokeAsync();
    }

    private async Task HandleMouseEnterAsync()
    {
        if (Disabled) return;

        await Context.SetHighlightedKeyAsync(EffectiveKey);
    }

    public void Dispose()
    {
        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }
        UnregisterFromContext();
    }
}
