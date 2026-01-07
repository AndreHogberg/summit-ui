using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// Trigger button that toggles the dropdown menu open/closed.
/// </summary>
public class DropdownMenuTrigger : ComponentBase, IAsyncDisposable
{
    [Inject]
    private DropdownMenuJsInterop JsInterop { get; set; } = default!;

    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

    /// <summary>
    /// Child content (typically button text/icon).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render. Defaults to "button".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "button";

    /// <summary>
    /// Additional HTML attributes to apply to the element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private bool _isInitialized;
    private bool _isDisposed;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Context.RegisterTrigger(_elementRef);

            if (RendererInfo.IsInteractive && !_isDisposed)
            {
                await JsInterop.InitializeTriggerAsync(_elementRef);
                _isInitialized = true;
            }
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, As);
        builder.AddAttribute(1, "id", $"{Context.MenuId}-trigger");
        builder.AddAttribute(2, "type", As == "button" ? "button" : null);
        builder.AddAttribute(3, "aria-haspopup", "menu");
        builder.AddAttribute(4, "aria-expanded", Context.IsOpen.ToString().ToLowerInvariant());
        builder.AddAttribute(5, "aria-controls", Context.IsOpen ? Context.MenuId : null);
        builder.AddAttribute(6, "data-state", DataState);
        builder.AddAttribute(7, "data-summit-dropdown-menu-trigger", true);
        builder.AddMultipleAttributes(8, AdditionalAttributes);
        builder.AddAttribute(9, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync));
        builder.AddAttribute(10, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync));
        builder.AddElementReferenceCapture(11, (elementRef) => { _elementRef = elementRef; });
        builder.AddContent(12, ChildContent);
        builder.CloseElement();
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        await Context.ToggleAsync();
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        // Note: Enter and Space are NOT handled here.
        // The browser's default behavior for buttons triggers onclick for these keys,
        // which calls HandleClickAsync. Handling them here would cause double-toggle.
        switch (args.Key)
        {
            case "ArrowDown":
                // Open menu and focus first item
                if (!Context.IsOpen)
                {
                    await Context.OpenAsync();
                }
                break;
            case "ArrowUp":
                // Open menu and focus last item
                if (!Context.IsOpen)
                {
                    await Context.OpenAsync();
                }
                break;
        }
    }

    private string DataState => Context.IsOpen ? "open" : "closed";

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (_isInitialized)
        {
            try
            {
                await JsInterop.DestroyTriggerAsync(_elementRef);
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, ignore
            }
            catch (ObjectDisposedException)
            {
                // Component already disposed, ignore
            }
        }
    }
}
