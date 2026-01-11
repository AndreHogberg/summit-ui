using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Base;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop for OTP component.
/// Handles selection tracking, focus management, and password manager detection.
/// </summary>
public sealed class OtpJsInterop(IJSRuntime jsRuntime) : JsInteropBase(jsRuntime)
{
    /// <summary>
    /// Initializes the OTP input with event listeners for selection tracking.
    /// </summary>
    /// <param name="inputElement">The hidden input element.</param>
    /// <param name="containerElement">The container element.</param>
    /// <param name="dotNetRef">The .NET object reference for callbacks.</param>
    /// <param name="maxLength">The maximum length of the OTP.</param>
    public async ValueTask InitializeAsync<T>(
        ElementReference inputElement,
        ElementReference containerElement,
        DotNetObjectReference<T> dotNetRef,
        int maxLength) where T : class
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("otp_initialize", inputElement, containerElement, dotNetRef, maxLength);
        }
        catch (JSDisconnectedException)
        {
            // Ignored - browser cleaned up resources
        }
    }

    /// <summary>
    /// Destroys the OTP input event listeners.
    /// </summary>
    /// <param name="element">The hidden input element.</param>
    public async ValueTask DestroyAsync(ElementReference element)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("otp_destroy", element);
        }
        catch (JSDisconnectedException)
        {
            // Ignored - browser cleaned up resources
        }
    }

    /// <summary>
    /// Focuses the OTP input and sets appropriate selection.
    /// </summary>
    /// <param name="element">The hidden input element.</param>
    /// <param name="maxLength">The maximum length.</param>
    public async ValueTask FocusAsync(ElementReference element, int maxLength)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("otp_focus", element, maxLength);
        }
        catch (JSDisconnectedException)
        {
            // Ignored
        }
    }

    /// <summary>
    /// Sets the selection range on the input.
    /// </summary>
    /// <param name="element">The hidden input element.</param>
    /// <param name="start">Selection start position.</param>
    /// <param name="end">Selection end position.</param>
    public async ValueTask SetSelectionAsync(ElementReference element, int start, int end)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("otp_setSelection", element, start, end);
        }
        catch (JSDisconnectedException)
        {
            // Ignored
        }
    }
}
