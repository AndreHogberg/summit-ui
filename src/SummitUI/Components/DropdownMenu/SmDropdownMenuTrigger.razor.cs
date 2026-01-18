using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// Trigger button that toggles the dropdown menu open/closed.
/// Supports the AsChild pattern for rendering custom elements.
/// </summary>
public partial class SmDropdownMenuTrigger : ComponentBase, IAsyncDisposable
{
    [Inject]
    private DropdownMenuJsInterop JsInterop { get; set; } = default!;

    [CascadingParameter]
    private DropdownMenuContext Context { get; set; } = default!;

    /// <summary>
    /// When true, the component will not render a wrapper element.
    /// Instead, it passes attributes via context to the child element.
    /// The child must apply @attributes="context.Attrs" for proper functionality.
    /// </summary>
    [Parameter]
    public bool AsChild { get; set; }

    /// <summary>
    /// Child content. When AsChild is true, receives an AsChildContext with attributes to apply.
    /// </summary>
    [Parameter]
    public RenderFragment<AsChildContext>? ChildContent { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private bool _isInitialized;
    private bool _isDisposed;
    private AsChildContext _context = default!;

    private string DataState => Context.IsOpen ? "open" : "closed";

    protected override void OnInitialized()
    {
        _context = new AsChildContext
        {
            Attrs = BuildAttributes(),
            RefCallback = el => _elementRef = el
        };
    }

    protected override void OnParametersSet()
    {
        // Rebuild attributes when parameters change
        _context = new AsChildContext
        {
            Attrs = BuildAttributes(),
            RefCallback = el => _elementRef = el
        };
    }

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

    private IReadOnlyDictionary<string, object> BuildAttributes()
    {
        var attrs = new Dictionary<string, object>
        {
            ["id"] = $"{Context.MenuId}-trigger",
            ["aria-haspopup"] = "menu",
            ["aria-expanded"] = Context.IsOpen.ToString().ToLowerInvariant(),
            ["data-state"] = DataState,
            ["data-summit-dropdown-menu-trigger"] = true,
            ["onclick"] = EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync),
            ["onkeydown"] = EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync)
        };

        // Add type for button elements (only when not AsChild)
        if (!AsChild)
        {
            attrs["type"] = "button";
        }

        // Only add aria-controls when open
        if (Context.IsOpen)
        {
            attrs["aria-controls"] = Context.MenuId;
        }

        // Merge additional attributes (consumer attributes win)
        if (AdditionalAttributes is not null)
        {
            foreach (var (key, value) in AdditionalAttributes)
            {
                attrs[key] = value;
            }
        }

        return attrs;
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
