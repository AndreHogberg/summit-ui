using Microsoft.JSInterop;

namespace SummitUI.Docs.Client.Services;

/// <summary>
/// JavaScript interop service for search keyboard shortcut (Cmd+K / Ctrl+K).
/// </summary>
public sealed class SearchJsInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private Task<IJSObjectReference>? _moduleTask;
    private DotNetObjectReference<SearchJsInterop>? _dotNetRef;
    private Action? _onSearchShortcut;
    private bool _isInitialized;

    public SearchJsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    private Task<IJSObjectReference> GetModuleAsync()
    {
        return _moduleTask ??= _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./search.min.js").AsTask();
    }

    /// <summary>
    /// Initialize the search keyboard shortcut listener.
    /// </summary>
    /// <param name="onSearchShortcut">Callback when search shortcut is pressed.</param>
    public async ValueTask InitAsync(Action onSearchShortcut)
    {
        _onSearchShortcut = onSearchShortcut;
        
        if (_isInitialized)
            return;
        
        _dotNetRef = DotNetObjectReference.Create(this);
        
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("init", _dotNetRef);
        _isInitialized = true;
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
        if (!_isInitialized)
            return;
            
        _isInitialized = false;
        
        try
        {
            if (_moduleTask is { IsCompleted: true, IsFaulted: false })
            {
                var module = await _moduleTask;
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
            _moduleTask = null;
            _dotNetRef?.Dispose();
            _dotNetRef = null;
        }
    }
}
