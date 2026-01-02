using Microsoft.AspNetCore.Components;

namespace ArkUI.Components.Select;

/// <summary>
/// A selectable option within the select dropdown.
/// Implements option role with full ARIA support.
/// </summary>
/// <typeparam name="TValue">The type of the select value.</typeparam>
public partial class SelectItem<TValue> : ComponentBase, IDisposable where TValue : notnull
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
    /// Data state for styling (checked or unchecked).
    /// </summary>
    private string DataState => IsSelected ? "checked" : "unchecked";

    protected override void OnInitialized()
    {
        RegisterWithContext();
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
            // Update the value and label in case they changed
            Context.RegisterItem(currentKey, Value, EffectiveLabel);
        }
    }

    private void RegisterWithContext()
    {
        var key = EffectiveKey;
        Context.RegisterItem(key, Value, EffectiveLabel);
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

    public void Dispose()
    {
        UnregisterFromContext();
    }
}
