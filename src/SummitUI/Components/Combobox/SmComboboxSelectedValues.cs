using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Model for a selected item that includes the value, label, and a delegate to deselect it.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public sealed class ComboboxSelectedItem<TValue> where TValue : notnull
{
    /// <summary>
    /// The selected value.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// The display label for this value (if available from registration).
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Delegate to deselect this item. Call this to remove the item from the selection.
    /// </summary>
    public Func<Task> DeselectAsync { get; }

    public ComboboxSelectedItem(TValue value, string label, Func<Task> deselectAsync)
    {
        Value = value;
        Label = label;
        DeselectAsync = deselectAsync;
    }
}

/// <summary>
/// Exposes the currently selected values as a RenderFragment with deselect capability.
/// Use with Context to access the list of selected items.
/// </summary>
/// <typeparam name="TValue">The type of the combobox value.</typeparam>
public class SmComboboxSelectedValues<TValue> : ComponentBase, IDisposable where TValue : notnull
{
    [CascadingParameter]
    private ComboboxContext<TValue> Context { get; set; } = default!;

    /// <summary>
    /// Child content receiving a list of selected items with deselect delegates.
    /// Use with Context="items" to access the IReadOnlyList&lt;ComboboxSelectedItem&lt;TValue&gt;&gt;.
    /// </summary>
    [Parameter]
    public RenderFragment<IReadOnlyList<ComboboxSelectedItem<TValue>>>? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes for the wrapper element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// HTML element to render as wrapper. Defaults to no wrapper (fragment only).
    /// Set to "div", "span", etc. to wrap the content.
    /// </summary>
    [Parameter]
    public string? As { get; set; }

    private bool _isSubscribed;

    protected override void OnInitialized()
    {
        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var selectedItems = BuildSelectedItems();
        var content = ChildContent?.Invoke(selectedItems);

        if (content is null) return;

        if (string.IsNullOrEmpty(As))
        {
            // No wrapper element, render content directly
            builder.AddContent(0, content);
        }
        else
        {
            // Wrap in specified element
            builder.OpenElement(0, As);
            builder.AddAttribute(1, "data-summit-combobox-selected-values", "");
            builder.AddMultipleAttributes(2, AdditionalAttributes);
            builder.AddContent(3, content);
            builder.CloseElement();
        }
    }

    private IReadOnlyList<ComboboxSelectedItem<TValue>> BuildSelectedItems()
    {
        var items = new List<ComboboxSelectedItem<TValue>>();

        foreach (var value in Context.SelectedValues)
        {
            // Skip null values (shouldn't happen with notnull constraint, but be safe)
            if (value is null) continue;
            
            var label = Context.ValueToLabelRegistry.TryGetValue(value, out var l) 
                ? l 
                : value.ToString() ?? "";

            // Capture value for closure
            var capturedValue = value;
            
            var item = new ComboboxSelectedItem<TValue>(
                capturedValue,
                label,
                () => Context.DeselectValueAsync(capturedValue)
            );
            
            items.Add(item);
        }

        return items.AsReadOnly();
    }

    public void Dispose()
    {
        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }
    }
}
