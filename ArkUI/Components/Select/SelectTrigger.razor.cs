using ArkUI.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace ArkUI.Components.Select;

/// <summary>
/// Trigger button that opens/closes the select dropdown.
/// Implements combobox role with full ARIA support.
/// </summary>
public partial class SelectTrigger : ComponentBase, IAsyncDisposable
{
    [Inject]
    private SelectJsInterop JsInterop { get; set; } = default!;

    [CascadingParameter]
    private SelectContext Context { get; set; } = default!;

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

    /// <summary>
    /// Get the element reference for the trigger.
    /// </summary>
    public ElementReference ElementRef => _elementRef;

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
        !string.IsNullOrEmpty(Context.HighlightedValue) 
            ? Context.GetItemId(Context.HighlightedValue) 
            : null;

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

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
