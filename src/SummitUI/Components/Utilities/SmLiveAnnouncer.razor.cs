using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Services;

namespace SummitUI;

/// <summary>
/// Initializes the live announcer JavaScript module for screen reader announcements.
/// Place this component once at the root of your application (e.g., MainLayout.razor).
/// </summary>
/// <remarks>
/// <para>
/// This component initializes the JavaScript-based live announcer that uses vanilla DOM
/// manipulation for reliable screen reader announcements. It follows React Aria's pattern:
/// </para>
/// <list type="bullet">
///   <item>Uses <c>role="log"</c> with <c>aria-relevant="additions"</c></item>
///   <item>Appends new child nodes rather than replacing content</item>
///   <item>7000ms timeout before clearing announcements</item>
///   <item>100ms delay on first announcement (Safari compatibility)</item>
/// </list>
/// <para>
/// Components throughout your application can inject <see cref="ILiveAnnouncer"/> and call
/// <see cref="ILiveAnnouncer.Announce"/> to make announcements to screen readers.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// @* In MainLayout.razor or App.razor *@
/// &lt;SmLiveAnnouncer /&gt;
/// 
/// &lt;main&gt;
///     @Body
/// &lt;/main&gt;
/// </code>
/// </example>
public partial class SmLiveAnnouncer : IAsyncDisposable
{
    [Inject]
    private ILiveAnnouncer Announcer { get; set; } = default!;

    private bool _initialized;

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Only run JS interop when interactive
        if (!RendererInfo.IsInteractive) return;

        if (firstRender && !_initialized)
        {
            _initialized = true;

            // Initialize the JavaScript live announcer
            if (Announcer is LiveAnnouncerService service)
            {
                await service.InitializeAsync();
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_initialized && Announcer is LiveAnnouncerService service)
        {
            try
            {
                await service.DestroyAsync();
            }
            catch (JSDisconnectedException)
            {
                // Safe to ignore, JS resources are cleaned up by the browser
            }
        }
    }
}
