using ArkUI.Interop;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArkUI.Components.Tabs;

/// <summary>
/// Container for tab triggers. Renders with role="tablist".
/// </summary>
public partial class TabsList : ComponentBase, IAsyncDisposable
{
    [Inject] private TabsJsInterop JsInterop { get; set; } = default!;

    /// <summary>
    /// The tabs context from the parent TabsRoot.
    /// </summary>
    [CascadingParameter]
    public TabsContext Context { get; set; } = default!;

    /// <summary>
    /// Child content (TabsTrigger components).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render as.
    /// </summary>
    [Parameter]
    public string As { get; set; } = "div";

    /// <summary>
    /// Additional HTML attributes to apply.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private DotNetObjectReference<TabsList>? _dotNetRef;
    private bool _isInitialized;
    private bool _isDisposed;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_isInitialized)
        {
            _isInitialized = true;
            _dotNetRef = DotNetObjectReference.Create(this);

            try
            {
                await JsInterop.InitializeAsync(
                    _elementRef,
                    _dotNetRef,
                    new TabsNavigationOptions
                    {
                        Orientation = Context.Orientation.ToString().ToLowerInvariant(),
                        Loop = Context.Loop,
                        ActivationMode = Context.ActivationMode.ToString().ToLowerInvariant()
                    });
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, ignore
            }
        }
    }

    /// <summary>
    /// Called from JavaScript when a tab should be activated via keyboard navigation.
    /// </summary>
    [JSInvokable]
    public async Task HandleTabActivation(string value)
    {
        await Context.ActivateTabAsync(value);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (_isInitialized)
        {
            try
            {
                await JsInterop.DestroyAsync(_elementRef);
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, ignore
            }
        }

        _dotNetRef?.Dispose();
    }
}
