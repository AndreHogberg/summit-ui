using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// The calendar grid table. Implements role="grid" for accessibility.
/// </summary>
public partial class SmCalendarGrid : IAsyncDisposable
{
    private ElementReference _elementRef;
    private bool _initialized;

    [CascadingParameter]
    private CalendarContext Context { get; set; } = default!;

    [Inject] private CalendarJsInterop JsInterop { get; set; } = default!;

    /// <summary>
    /// The child content.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Custom ARIA description for screen reader instructions.
    /// If not provided, uses the localized default from <see cref="ISummitUILocalizer"/>.
    /// </summary>
    [Parameter] public string? AriaDescription { get; set; }

    /// <summary>
    /// Additional attributes to apply to the table element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Only run JS interop when interactive
        if (!RendererInfo.IsInteractive) return;

        // Initialize keyboard navigation support (preventDefault for navigation keys)
        if (!_initialized)
        {
            await JsInterop.InitializeCalendarAsync(_elementRef);
            _initialized = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_initialized)
        {
            try
            {
                await JsInterop.DestroyCalendarAsync(_elementRef);
            }
            catch (JSDisconnectedException)
            {
                // Safe to ignore, JS resources are cleaned up by the browser
            }
        }
    }
}
