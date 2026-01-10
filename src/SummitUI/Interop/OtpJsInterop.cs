using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SummitUI.Interop;

/// <summary>
/// JavaScript interop for OTP component.
/// Handles selection tracking, focus management, and password manager detection.
/// </summary>
public sealed class OtpJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/SummitUI/summitui.js").AsTask());

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
            var module = await _moduleTask.Value;
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
            var module = await _moduleTask.Value;
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
            var module = await _moduleTask.Value;
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
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("otp_setSelection", element, start, end);
        }
        catch (JSDisconnectedException)
        {
            // Ignored
        }
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
                // Ignored - browser cleaned up resources
            }
        }
    }
}
