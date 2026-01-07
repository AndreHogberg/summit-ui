using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// The calendar grid table. Implements role="grid" for accessibility.
/// </summary>
public class CalendarGrid : ComponentBase, IAsyncDisposable
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
    /// Additional attributes to apply to the table element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Initialize keyboard navigation support (preventDefault for navigation keys)
        if (!_initialized)
        {
            await JsInterop.InitializeCalendarAsync(_elementRef);
            _initialized = true;
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;

        builder.OpenElement(seq++, "table");
        builder.AddAttribute(seq++, "id", Context.GridId);
        // Use role="application" to ensure screen readers pass through all keyboard events
        // This is necessary because the grid contains interactive buttons that need arrow key navigation
        // Note: role="application" should be used sparingly as it disables screen reader shortcuts
        builder.AddAttribute(seq++, "role", "application");
        builder.AddAttribute(seq++, "aria-roledescription", "calendar grid");
        builder.AddAttribute(seq++, "aria-labelledby", Context.HeadingId);
        builder.AddAttribute(seq++, "aria-description", "Use arrow keys to navigate dates, Enter or Space to select");
        builder.AddAttribute(seq++, "data-summit-calendar-grid", true);
        builder.AddMultipleAttributes(seq++, AdditionalAttributes);
        builder.AddElementReferenceCapture(seq++, elementRef => _elementRef = elementRef);

        if (ChildContent != null)
        {
            builder.AddContent(seq++, ChildContent);
        }

        builder.CloseElement();
    }

    public async ValueTask DisposeAsync()
    {
        if (_initialized)
        {
            await JsInterop.DestroyCalendarAsync(_elementRef);
        }
    }
}
