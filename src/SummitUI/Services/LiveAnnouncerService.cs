using Microsoft.JSInterop;

using SummitUI.Base;

namespace SummitUI.Services;

/// <summary>
/// Implementation of <see cref="ILiveAnnouncer"/> that uses JavaScript interop
/// for reliable screen reader announcements.
/// </summary>
/// <remarks>
/// <para>
/// Inspired by React Aria's LiveAnnouncer pattern.
/// See: https://github.com/adobe/react-spectrum/blob/main/packages/@react-aria/live-announcer
/// Copyright 2020 Adobe. Licensed under Apache 2.0.
/// </para>
/// <para>
/// This implementation follows React Aria's approach:
/// <list type="bullet">
///   <item>Uses vanilla DOM manipulation via JavaScript</item>
///   <item>Creates role="log" elements with aria-relevant="additions"</item>
///   <item>Appends new child nodes rather than replacing content</item>
///   <item>7000ms timeout before clearing announcements</item>
///   <item>100ms delay on first announcement (Safari compatibility)</item>
/// </list>
/// </para>
/// <para>
/// The <see cref="SmLiveAnnouncer"/> component must be placed in the app's layout
/// to initialize the JavaScript module. Without it, announcements will be silently ignored.
/// </para>
/// </remarks>
public sealed class LiveAnnouncerService : JsInteropBase, ILiveAnnouncer
{
    private bool _isInitialized;
    private readonly List<(string Message, string Assertiveness)> _pendingAnnouncements = [];

    public LiveAnnouncerService(IJSRuntime jsRuntime) : base(jsRuntime)
    {
    }

    /// <inheritdoc />
    public string PoliteMessage { get; private set; } = "";

    /// <inheritdoc />
    public string AssertiveMessage { get; private set; } = "";

    /// <inheritdoc />
    public event Action? OnAnnouncementChanged;

    /// <summary>
    /// Initializes the live announcer by creating the DOM elements.
    /// Called by SmLiveAnnouncer on first render.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        if (_isInitialized) return;

        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("liveAnnouncer_initialize");
            _isInitialized = true;

            // Process any pending announcements
            foreach (var (message, assertiveness) in _pendingAnnouncements)
            {
                await AnnounceInternalAsync(message, assertiveness);
            }
            _pendingAnnouncements.Clear();
        }
        catch (JSDisconnectedException)
        {
            // Ignored - Blazor Server circuit disconnected
        }
        catch (ObjectDisposedException)
        {
            // Ignored - JS object reference already disposed
        }
    }

    /// <inheritdoc />
    public void Announce(string message, AnnouncementPriority priority = AnnouncementPriority.Polite)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var assertiveness = priority == AnnouncementPriority.Assertive ? "assertive" : "polite";

        // Update local state for backward compatibility with tests
        if (priority == AnnouncementPriority.Assertive)
            AssertiveMessage = message;
        else
            PoliteMessage = message;

        OnAnnouncementChanged?.Invoke();

        // Fire and forget the async announcement
        _ = AnnounceAsync(message, assertiveness);
    }

    private async Task AnnounceAsync(string message, string assertiveness)
    {
        if (!_isInitialized)
        {
            // Queue for later if not initialized yet
            _pendingAnnouncements.Add((message, assertiveness));
            return;
        }

        await AnnounceInternalAsync(message, assertiveness);
    }

    private async Task AnnounceInternalAsync(string message, string assertiveness)
    {
        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("liveAnnouncer_announce", message, assertiveness);
        }
        catch (JSDisconnectedException)
        {
            // Ignored - Blazor Server circuit disconnected
        }
        catch (ObjectDisposedException)
        {
            // Ignored - JS object reference already disposed
        }
    }

    /// <inheritdoc />
    public void Clear(AnnouncementPriority priority)
    {
        var assertiveness = priority == AnnouncementPriority.Assertive ? "assertive" : "polite";

        if (priority == AnnouncementPriority.Assertive)
            AssertiveMessage = "";
        else
            PoliteMessage = "";

        OnAnnouncementChanged?.Invoke();

        _ = ClearAsync(assertiveness);
    }

    private async Task ClearAsync(string assertiveness)
    {
        if (!_isInitialized) return;

        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("liveAnnouncer_clear", assertiveness);
        }
        catch (JSDisconnectedException)
        {
            // Ignored
        }
        catch (ObjectDisposedException)
        {
            // Ignored
        }
    }

    /// <inheritdoc />
    public void ClearAll()
    {
        PoliteMessage = "";
        AssertiveMessage = "";
        OnAnnouncementChanged?.Invoke();

        _ = ClearAllAsync();
    }

    private async Task ClearAllAsync()
    {
        if (!_isInitialized) return;

        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("liveAnnouncer_clearAll");
        }
        catch (JSDisconnectedException)
        {
            // Ignored
        }
        catch (ObjectDisposedException)
        {
            // Ignored
        }
    }

    /// <summary>
    /// Destroys the live announcer JavaScript component.
    /// Called when SmLiveAnnouncer is disposed.
    /// </summary>
    public async ValueTask DestroyAsync()
    {
        if (!_isInitialized) return;

        try
        {
            var module = await GetModuleAsync();
            await module.InvokeVoidAsync("liveAnnouncer_destroy");
            _isInitialized = false;
        }
        catch (JSDisconnectedException)
        {
            // Ignored
        }
        catch (ObjectDisposedException)
        {
            // Ignored
        }
    }
}
