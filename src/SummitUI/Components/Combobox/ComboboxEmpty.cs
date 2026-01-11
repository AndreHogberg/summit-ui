using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Displays content when no items match the current filter.
/// </summary>
/// <typeparam name="TValue">The type of the combobox value.</typeparam>
public class ComboboxEmpty<TValue> : ComponentBase, IDisposable where TValue : notnull
{
    [CascadingParameter]
    private ComboboxContext<TValue> Context { get; set; } = default!;

    /// <summary>
    /// Content to display when no items match.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

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
        // Only render if there are registered items but none match the filter
        if (!HasVisibleItems() && Context.ItemRegistry.Count > 0)
        {
            builder.OpenElement(0, As);
            builder.AddAttribute(1, "data-summit-combobox-empty", "");
            builder.AddAttribute(2, "role", "presentation");
            builder.AddMultipleAttributes(3, AdditionalAttributes);
            builder.AddContent(4, ChildContent);
            builder.CloseElement();
        }
    }

    private bool HasVisibleItems()
    {
        foreach (var key in Context.ItemRegistry.Keys)
        {
            if (Context.MatchesFilter(key))
            {
                return true;
            }
        }
        return false;
    }

    public void Dispose()
    {
        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }
    }
}
