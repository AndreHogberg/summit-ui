using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// Trigger button that opens/closes the select dropdown.
/// Implements combobox role with full ARIA support.
/// </summary>
/// <typeparam name="TValue">The type of the select value.</typeparam>
public class SelectTrigger<TValue> : ComponentBase, IAsyncDisposable where TValue : notnull
{
    [Inject]
    private SelectJsInterop JsInterop { get; set; } = default!;

    [CascadingParameter]
    private SelectContext<TValue> Context { get; set; } = default!;

    /// <summary>
    /// Child content (typically SelectValue).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "button".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "button";

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
    private bool _isRegistered;
    private bool _isDisposed;
    private bool _isSubscribed;

    /// <summary>
    /// Get the element reference for the trigger.
    /// </summary>
    public ElementReference ElementRef => _elementRef;

    protected override void OnInitialized()
    {
        // Subscribe to context state changes to re-render when HighlightedKey changes
        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Context.RegisterTrigger(_elementRef);

            // Register trigger with JS to prevent default scroll on arrow keys
            await JsInterop.RegisterTriggerAsync(_elementRef);
            _isRegistered = true;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "role", "combobox");
        builder.AddAttribute(2, "id", Context.TriggerId);
        if (As == "button")
        {
            builder.AddAttribute(3, "type", "button");
        }
        builder.AddAttribute(4, "aria-haspopup", "listbox");
        builder.AddAttribute(5, "aria-expanded", Context.IsOpen.ToString().ToLowerInvariant());
        builder.AddAttribute(6, "aria-controls", Context.ContentId);
        if (!string.IsNullOrEmpty(HighlightedItemId))
        {
            builder.AddAttribute(7, "aria-activedescendant", HighlightedItemId);
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
            builder.AddAttribute(13, "disabled", true);
        }
        builder.AddAttribute(14, "data-state", DataState);
        if (Context.Disabled)
        {
            builder.AddAttribute(15, "data-disabled", "");
        }
        if (Context.Invalid)
        {
            builder.AddAttribute(16, "data-invalid", "");
        }
        if (Context.Value is null)
        {
            builder.AddAttribute(17, "data-placeholder", "");
        }
        builder.AddMultipleAttributes(18, AdditionalAttributes);
        builder.AddAttribute(19, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        builder.AddAttribute(20, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        builder.AddElementReferenceCapture(21, (elementRef) => { _elementRef = elementRef; });
        builder.AddContent(22, ChildContent);
        builder.CloseElement();
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (Context.Disabled) return;
        await Context.ToggleAsync();
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (Context.Disabled) return;

        // When open, keyboard navigation is handled by JavaScript
        // We only handle opening here
        if (Context.IsOpen)
        {
            // Let JavaScript handle all navigation when open
            return;
        }

        switch (args.Key)
        {
            case "Enter":
            case " ":
                // Toggle dropdown
                await Context.ToggleAsync();
                break;

            case "ArrowDown":
            case "ArrowUp":
                // Open dropdown if closed
                await Context.OpenAsync();
                break;
        }
    }

    private string DataState => Context.IsOpen ? "open" : "closed";

    private string? HighlightedItemId =>
        !string.IsNullOrEmpty(Context.HighlightedKey)
            ? Context.GetItemId(Context.HighlightedKey)
            : null;

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Unsubscribe from context state changes
        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }

        if (_isRegistered)
        {
            try
            {
                await JsInterop.UnregisterTriggerAsync(_elementRef);
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, ignore
            }
        }
    }
}
