using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Button to clear all selected values in the combobox.
/// </summary>
/// <typeparam name="TValue">The type of the combobox value.</typeparam>
public class ComboboxClear<TValue> : ComponentBase, IDisposable where TValue : notnull
{
    [CascadingParameter]
    private ComboboxContext<TValue> Context { get; set; } = default!;

    /// <summary>
    /// Content of the clear button.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "button".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "button";

    /// <summary>
    /// Whether to hide the button when no items are selected.
    /// Defaults to true.
    /// </summary>
    [Parameter]
    public bool HideWhenEmpty { get; set; } = true;

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

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
        // Optionally hide when no selections
        if (HideWhenEmpty && Context.SelectedValues.Count == 0)
        {
            return;
        }

        builder.OpenElement(0, As);
        
        if (As == "button")
        {
            builder.AddAttribute(1, "type", "button");
        }
        
        builder.AddAttribute(2, "aria-label", "Clear all selections");
        builder.AddAttribute(3, "data-summit-combobox-clear", "");
        
        if (Context.Disabled)
        {
            builder.AddAttribute(4, "aria-disabled", "true");
            builder.AddAttribute(5, "disabled", true);
            builder.AddAttribute(6, "data-disabled", "");
        }
        
        if (Context.SelectedValues.Count == 0)
        {
            builder.AddAttribute(7, "data-empty", "");
        }
        
        builder.AddMultipleAttributes(8, AdditionalAttributes);
        builder.AddAttribute(9, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        builder.AddEventStopPropagationAttribute(10, "onclick", true);
        builder.AddContent(11, ChildContent);
        builder.CloseElement();
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Context.Disabled) return;

        await Context.ClearAsync();
    }

    public void Dispose()
    {
        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }
    }
}
