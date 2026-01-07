using Microsoft.JSInterop;

namespace SummitUI.Docs.Client.Services;

/// <summary>
/// JavaScript interop service for theme management (dark/light mode).
/// </summary>
public sealed class ThemeJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./theme.min.js").AsTask());

    /// <summary>
    /// Gets the current theme preference ('light', 'dark', or 'system').
    /// </summary>
    public async ValueTask<string> GetThemeAsync()
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("getTheme");
    }

    /// <summary>
    /// Sets the theme preference and applies it immediately.
    /// </summary>
    /// <param name="theme">The theme to set: 'light', 'dark', or 'system'.</param>
    public async ValueTask SetThemeAsync(string theme)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("setTheme", theme);
    }

    /// <summary>
    /// Toggles between light and dark themes.
    /// If currently on system preference, will toggle to the opposite of the current effective theme.
    /// </summary>
    /// <returns>The new theme ('light' or 'dark').</returns>
    public async ValueTask<string> ToggleThemeAsync()
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("toggleTheme");
    }

    /// <summary>
    /// Checks if dark mode is currently active (either explicit or via system preference).
    /// </summary>
    public async ValueTask<bool> IsDarkModeAsync()
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<bool>("isDarkMode");
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Safe to ignore, JS resources are cleaned up by the browser
        }
    }
}
