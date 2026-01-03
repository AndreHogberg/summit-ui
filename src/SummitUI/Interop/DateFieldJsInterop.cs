using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Interop;

public class DateFieldJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());

    public async ValueTask InitializeSegmentAsync(ElementReference element, DotNetObjectReference<DateFieldSegment> dotNetHelper)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("dateField_initializeSegment", element, dotNetHelper);
    }

    public async ValueTask DestroySegmentAsync(ElementReference element)
    {
        if (_moduleTask.IsValueCreated)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.InvokeVoidAsync("dateField_destroySegment", element);
            }
            catch (JSDisconnectedException)
            {
                // Ignored
            }
        }
    }

    /// <summary>
    /// Gets localized segment labels from the browser using Intl.DisplayNames.
    /// </summary>
    /// <param name="locale">The locale to use (e.g., "en-US", "sv-SE").</param>
    /// <returns>Dictionary of segment labels keyed by segment type name.</returns>
    public async ValueTask<Dictionary<string, string>> GetSegmentLabelsAsync(string locale)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<Dictionary<string, string>>("dateField_getSegmentLabels", locale);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Ignored
            }
        }
    }
}
