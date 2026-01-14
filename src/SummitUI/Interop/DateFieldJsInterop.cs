using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Base;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop for DateField component.
/// Handles keyboard event interception for segment navigation.
/// </summary>
/// <remarks>
/// Date conversion and locale formatting have been moved to <see cref="Services.CalendarProvider"/> 
/// and <see cref="Services.CalendarFormatter"/> which use .NET's System.Globalization.
/// </remarks>
public class DateFieldJsInterop(IJSRuntime jsRuntime) : JsInteropBase(jsRuntime)
{
    /// <summary>
    /// Initializes keyboard handling for a date field segment.
    /// </summary>
    public async ValueTask InitializeSegmentAsync(ElementReference element, DotNetObjectReference<SmDateFieldSegment> dotNetHelper)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("dateField_initializeSegment", element, dotNetHelper);
    }

    /// <summary>
    /// Cleans up keyboard handling for a date field segment.
    /// </summary>
    public async ValueTask DestroySegmentAsync(ElementReference element)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("dateField_destroySegment", element);
        }
        catch (JSDisconnectedException)
        {
            // Ignored - Blazor Server circuit disconnected
        }
        catch (ObjectDisposedException)
        {
            // Ignored - JS object reference already disposed (WebAssembly)
        }
    }
}
