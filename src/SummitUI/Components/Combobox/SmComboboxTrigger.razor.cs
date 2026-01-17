using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Trigger element that opens/closes the combobox popup.
/// Can contain ComboboxSelectedValues, ComboboxInput, and other content.
/// </summary>
/// <typeparam name="TValue">The type of the combobox value.</typeparam>
public partial class SmComboboxTrigger<TValue> : ComponentBase, IDisposable where TValue : notnull
{
    [CascadingParameter]
    private ComboboxContext<TValue> Context { get; set; } = default!;

    /// <summary>
    /// Child content (typically ComboboxSelectedValues and/or ComboboxInput).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Direct aria-label for the trigger.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// ID of an external label element to associate with the trigger.
    /// </summary>
    [Parameter]
    public string? AriaLabelledBy { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private bool _isDisposed;
    private bool _isSubscribed;

    /// <summary>
    /// Get the element reference for the trigger.
    /// </summary>
    public ElementReference ElementRef => _elementRef;

    protected override void OnInitialized()
    {
        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Context.RegisterTrigger(_elementRef);
        }
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Context.Disabled) return;

        if (Context.HasInput)
        {
            await Context.OpenAsync();
            await Context.FocusInputAsync();
        }
        else
        {
            await Context.ToggleAsync();
        }
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (Context.Disabled) return;

        // Only handle keys when there's no input (select-only mode)
        if (Context.HasInput) return;

        // When closed, handle opening
        if (!Context.IsOpen)
        {
            switch (args.Key)
            {
                case "Enter":
                case " ":
                case "ArrowDown":
                case "ArrowUp":
                    await Context.OpenAsync();
                    break;
            }
        }
    }

    private string DataState => Context.IsOpen ? "open" : "closed";

    private string? HighlightedItemId =>
        !string.IsNullOrEmpty(Context.HighlightedKey)
            ? Context.GetItemId(Context.HighlightedKey)
            : null;

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }
    }
}
