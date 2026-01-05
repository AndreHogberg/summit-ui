using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Interop;

/// <summary>
/// Represents the AM/PM designators.
/// </summary>
public record DayPeriodDesignators(string Am, string Pm);

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
    /// Gets the browser's current locale (navigator.language).
    /// </summary>
    /// <returns>The browser locale string (e.g., "en-US", "sv-SE").</returns>
    public async ValueTask<string> GetBrowserLocaleAsync()
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string>("dateField_getBrowserLocale");
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

    /// <summary>
    /// Gets localized AM/PM designators from the browser using Intl.DateTimeFormat.
    /// </summary>
    /// <param name="locale">The locale to use (e.g., "en-US", "sv-SE").</param>
    /// <returns>The AM and PM designator strings.</returns>
    public async ValueTask<DayPeriodDesignators> GetDayPeriodDesignatorsAsync(string locale)
    {
        var module = await _moduleTask.Value;
        var result = await module.InvokeAsync<Dictionary<string, string>>("dateField_getDayPeriodDesignators", locale);
        return new DayPeriodDesignators(
            result.GetValueOrDefault("am", "AM"),
            result.GetValueOrDefault("pm", "PM")
        );
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
