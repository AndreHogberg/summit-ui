using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// A component that reactively evaluates a CSS media query and provides the match state to its children.
/// Useful for responsive layouts that need to adapt based on viewport size or other media features.
/// </summary>
/// <example>
/// <code>
/// &lt;MediaQuery Query="(min-width: 800px)" Context="IsLarge"&gt;
///     @if (IsLarge)
///     {
///         &lt;DesktopLayout /&gt;
///     }
///     else
///     {
///         &lt;MobileLayout /&gt;
///     }
/// &lt;/MediaQuery&gt;
/// </code>
/// </example>
public partial class SmMediaQuery : IAsyncDisposable
{
    [Inject]
    private MediaQueryJsInterop JsInterop { get; set; } = default!;

    /// <summary>
    /// The CSS media query string to evaluate.
    /// Examples: "(min-width: 800px)", "(prefers-color-scheme: dark)", "(orientation: landscape)"
    /// </summary>
    [Parameter, EditorRequired]
    public string Query { get; set; } = default!;

    /// <summary>
    /// The child content that receives the current match state as a boolean.
    /// Use the Context attribute to name the parameter (e.g., Context="IsLarge").
    /// </summary>
    [Parameter]
    public RenderFragment<bool>? ChildContent { get; set; }

    /// <summary>
    /// Callback invoked when the media query match state changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> OnChange { get; set; }

    /// <summary>
    /// The initial value to use before JavaScript evaluates the media query.
    /// This is useful for SSR scenarios where JavaScript is not available.
    /// Default: false
    /// </summary>
    [Parameter]
    public bool InitialValue { get; set; }

    private string _listenerId = default!;
    private bool _matches;
    private bool _isInitialized;
    private bool _isDisposed;
    private DotNetObjectReference<SmMediaQuery>? _dotNetRef;

    protected override void OnInitialized()
    {
        _listenerId = $"summit-media-query-{Guid.NewGuid():N}";
        _matches = InitialValue;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && RendererInfo.IsInteractive && !_isDisposed)
        {
            await InitializeAsync();
        }
    }

    private async Task InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            var initialMatch = await JsInterop.RegisterAsync(_listenerId, Query, _dotNetRef);

            _isInitialized = true;

            // Only update if the value changed from InitialValue
            if (initialMatch != _matches)
            {
                _matches = initialMatch;
                await OnChange.InvokeAsync(_matches);
                StateHasChanged();
            }
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }

    /// <summary>
    /// Called from JavaScript when the media query match state changes.
    /// </summary>
    [JSInvokable]
    public async Task OnMediaQueryChanged(bool matches)
    {
        if (_isDisposed || matches == _matches) return;

        _matches = matches;
        await OnChange.InvokeAsync(_matches);
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (_isInitialized)
        {
            try
            {
                await JsInterop.UnregisterAsync(_listenerId);
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, JS resources cleaned up by browser
            }
            catch (ObjectDisposedException)
            {
                // Runtime disposed, ignore
            }
        }

        _dotNetRef?.Dispose();
    }
}
