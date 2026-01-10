using Microsoft.JSInterop;

namespace SummitUI.Docs.Client.Services;

/// <summary>
/// JavaScript interop service for search keyboard shortcut (Cmd+K / Ctrl+K).
/// </summary>
public sealed class SearchJsInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private DotNetObjectReference<SearchJsInterop>? _dotNetRef;
    private Action? _onSearchShortcut;

    public SearchJsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _moduleTask = new Lazy<Task<IJSObjectReference>>(() =>
            jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./search.min.js").AsTask());
    }

    /// <summary>
    /// Initialize the search keyboard shortcut listener.
    /// </summary>
    /// <param name="onSearchShortcut">Callback when search shortcut is pressed.</param>
    public async ValueTask InitAsync(Action onSearchShortcut)
    {
        _onSearchShortcut = onSearchShortcut;
        _dotNetRef = DotNetObjectReference.Create(this);
        
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("init", _dotNetRef);
    }

    /// <summary>
    /// Called from JavaScript when search shortcut is pressed.
    /// </summary>
    [JSInvokable]
    public void OnSearchShortcut()
    {
        _onSearchShortcut?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;
                await module.InvokeVoidAsync("dispose");
                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Safe to ignore, JS resources are cleaned up by the browser
        }
        finally
        {
            _dotNetRef?.Dispose();
        }
    }
}
