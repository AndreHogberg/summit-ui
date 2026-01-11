using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Trigger element that opens/closes the combobox popup.
/// Can contain ComboboxSelectedValues, ComboboxInput, and other content.
/// </summary>
/// <typeparam name="TValue">The type of the combobox value.</typeparam>
public class ComboboxTrigger<TValue> : ComponentBase, IDisposable where TValue : notnull
{
    [CascadingParameter]
    private ComboboxContext<TValue> Context { get; set; } = default!;

    /// <summary>
    /// Child content (typically ComboboxSelectedValues and/or ComboboxInput).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "div".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

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
        // Subscribe to context state changes
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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "id", Context.TriggerId);
        
        // Only add role="combobox" if there's no input (select-only mode)
        // When there's an input, the input element gets the combobox role
        if (!Context.HasInput)
        {
            builder.AddAttribute(2, "role", "combobox");
            builder.AddAttribute(3, "tabindex", "0");
            builder.AddAttribute(4, "aria-haspopup", "listbox");
            builder.AddAttribute(5, "aria-expanded", Context.IsOpen.ToString().ToLowerInvariant());
            builder.AddAttribute(6, "aria-controls", Context.ContentId);
            if (!string.IsNullOrEmpty(HighlightedItemId))
            {
                builder.AddAttribute(7, "aria-activedescendant", HighlightedItemId);
            }
        }
        
        if (!string.IsNullOrEmpty(AriaLabel))
        {
            builder.AddAttribute(8, "aria-label", AriaLabel);
        }
        if (!string.IsNullOrEmpty(AriaLabelledBy))
        {
            builder.AddAttribute(9, "aria-labelledby", AriaLabelledBy);
        }
        if (Context.Required)
        {
            builder.AddAttribute(10, "aria-required", "true");
        }
        if (Context.Invalid)
        {
            builder.AddAttribute(11, "aria-invalid", "true");
        }
        if (Context.Disabled)
        {
            builder.AddAttribute(12, "aria-disabled", "true");
        }
        
        builder.AddAttribute(13, "data-state", DataState);
        builder.AddAttribute(14, "data-summit-combobox-trigger", "");
        
        if (Context.Disabled)
        {
            builder.AddAttribute(15, "data-disabled", "");
        }
        if (Context.Invalid)
        {
            builder.AddAttribute(16, "data-invalid", "");
        }
        if (Context.SelectedValues.Count == 0)
        {
            builder.AddAttribute(17, "data-placeholder", "");
        }
        
        builder.AddMultipleAttributes(18, AdditionalAttributes);
        builder.AddAttribute(19, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        
        // Only handle keyboard events if there's no input (select-only mode)
        if (!Context.HasInput)
        {
            builder.AddAttribute(20, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        }
        
        builder.AddElementReferenceCapture(21, (elementRef) => { _elementRef = elementRef; });
        builder.AddContent(22, ChildContent);
        builder.CloseElement();
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Context.Disabled) return;
        
        // If there's an input, clicking the trigger should focus the input and open
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
        // When there's an input, it handles keyboard events
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
