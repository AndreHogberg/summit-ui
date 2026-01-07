using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// Container for date field segments. Generates segments based on format configuration.
/// </summary>
public class DateFieldInput : ComponentBase, IDisposable
{
    [Inject] private DateFieldJsInterop JsInterop { get; set; } = default!;

    [CascadingParameter] public DateFieldContext Context { get; set; } = default!;

    /// <summary>
    /// Optional custom rendering template for segments.
    /// </summary>
    [Parameter] public RenderFragment<IReadOnlyList<DateFieldSegmentState>>? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private List<DateFieldSegmentState> _segments = new();
    private bool _localizationLoaded;
    private CalendarSystem _lastCalendarSystem;
    private DateTime? _lastValueForCalendarInfo;

    protected override void OnInitialized()
    {
        if (Context == null)
            throw new InvalidOperationException("DateFieldInput must be used within a DateFieldRoot.");

        Context.OnStateChanged += HandleStateChanged;
        _lastCalendarSystem = Context.CalendarSystem;
        RegenerateSegments();
    }

    protected override void OnParametersSet()
    {
        RegenerateSegments();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_localizationLoaded)
        {
            _localizationLoaded = true;

            // Get browser locale and use it for segment labels and AM/PM designators
            var browserLocale = await JsInterop.GetBrowserLocaleAsync();

            // If Locale was not explicitly set, use browser locale
            if (Context.Locale == "en-US" && browserLocale != "en-US")
            {
                Context.SetDetectedLocale(browserLocale);
            }

            // Auto-detect format based on locale if not explicitly set
            if (!Context.HasExplicitFormat)
            {
                var localeFormat = await JsInterop.GetLocaleDateFormatAsync(Context.Locale);
                Context.SetDetectedFormat(localeFormat.DateFormat);
                RegenerateSegments();
            }

            var labels = await JsInterop.GetSegmentLabelsAsync(Context.Locale);
            Context.SetSegmentLabels(labels);

            var designators = await JsInterop.GetDayPeriodDesignatorsAsync(Context.Locale);
            Context.SetDayPeriodDesignators(designators.Am, designators.Pm);

            // Fetch initial calendar info
            await RefreshCalendarInfoAsync();

            StateHasChanged();
        }
        else if (_localizationLoaded)
        {
            // Check if we need to refresh calendar info (value or calendar system changed)
            var needsCalendarRefresh = 
                Context.CalendarSystem != _lastCalendarSystem ||
                !DateTime.Equals(Context.EffectiveDateTime, _lastValueForCalendarInfo);

            if (needsCalendarRefresh && Context.CalendarSystem != CalendarSystem.Gregorian)
            {
                await RefreshCalendarInfoAsync();
                StateHasChanged();
            }
        }
    }

    /// <summary>
    /// Refreshes the calendar info from JavaScript for the current date and calendar system.
    /// </summary>
    private async Task RefreshCalendarInfoAsync()
    {
        _lastCalendarSystem = Context.CalendarSystem;
        _lastValueForCalendarInfo = Context.EffectiveDateTime;

        // Only fetch if not Gregorian (Gregorian uses direct C# values)
        if (Context.CalendarSystem == CalendarSystem.Gregorian)
        {
            Context.ClearCalendarInfo();
            return;
        }

        var effectiveDate = Context.EffectiveDateTime;
        
        var calendarInfo = await JsInterop.GetCalendarDateInfoAsync(
            effectiveDate.Year,
            effectiveDate.Month,
            effectiveDate.Day,
            Context.CalendarSystem);

        Context.SetCalendarInfo(
            calendarInfo.Year,
            calendarInfo.Month,
            calendarInfo.Day,
            calendarInfo.Era,
            calendarInfo.DaysInMonth,
            calendarInfo.MonthsInYear);
    }

    private void HandleStateChanged()
    {
        // Only regenerate if necessary (segments structure doesn't change often)
        StateHasChanged();
    }

    private void RegenerateSegments()
    {
        _segments = DateFieldUtils.GetSegments(Context);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "role", "presentation");
        builder.AddMultipleAttributes(2, AdditionalAttributes);

        if (ChildContent != null)
        {
            // Custom template rendering
            builder.AddContent(3, ChildContent(_segments));
        }
        else
        {
            // Default rendering - render each segment
            var seq = 4;
            foreach (var segment in _segments)
            {
                builder.OpenComponent<DateFieldSegment>(seq++);
                builder.AddAttribute(seq++, "Segment", segment);
                builder.SetKey(segment.Id);
                builder.CloseComponent();
            }
        }

        builder.CloseElement();
    }

    public void Dispose()
    {
        if (Context != null)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }
    }
}
