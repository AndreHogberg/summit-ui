using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using SummitUI.Services;

namespace SummitUI;

/// <summary>
/// Container for date field segments. Generates segments based on format configuration.
/// </summary>
public class DateFieldInput : ComponentBase, IDisposable
{
    [Inject] private CalendarProvider CalendarProvider { get; set; } = default!;

    [CascadingParameter] public DateFieldContext Context { get; set; } = default!;

    /// <summary>
    /// Optional custom rendering template for segments.
    /// </summary>
    [Parameter] public RenderFragment<IReadOnlyList<DateFieldSegmentState>>? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private List<DateFieldSegmentState> _segments = new();
    private bool _initialized;
    private CalendarSystem _lastCalendarSystem;
    private DateTime? _lastValueForCalendarInfo;

    protected override void OnInitialized()
    {
        if (Context == null)
            throw new InvalidOperationException("DateFieldInput must be used within a DateFieldRoot.");

        Context.OnStateChanged += HandleStateChanged;
        _lastCalendarSystem = Context.CalendarSystem;
        
        // Initialize segment labels from culture
        InitializeSegmentLabels();
        
        RegenerateSegments();
    }

    protected override void OnParametersSet()
    {
        // Refresh calendar info if calendar system or value changed
        var needsCalendarRefresh = 
            Context.CalendarSystem != _lastCalendarSystem ||
            !DateTime.Equals(Context.EffectiveDateTime, _lastValueForCalendarInfo);

        if (needsCalendarRefresh)
        {
            RefreshCalendarInfo();
        }

        RegenerateSegments();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && !_initialized)
        {
            _initialized = true;
            // Calendar info is already computed synchronously in OnParametersSet
        }
    }

    /// <summary>
    /// Initializes segment labels from the culture's DateTimeFormatInfo.
    /// </summary>
    private void InitializeSegmentLabels()
    {
        // Use English labels as fallback - segment labels are accessibility text
        // and don't need complex localization for the date field to function correctly.
        // The Culture's AM/PM designators are already used from DateTimeFormat.
        var labels = new Dictionary<string, string>
        {
            ["year"] = "Year",
            ["month"] = "Month",
            ["day"] = "Day",
            ["hour"] = "Hour",
            ["minute"] = "Minute",
            ["dayPeriod"] = "AM/PM"
        };
        Context.SetSegmentLabels(labels);
    }

    /// <summary>
    /// Refreshes the calendar info using the CalendarProvider for the current date and calendar system.
    /// </summary>
    private void RefreshCalendarInfo()
    {
        _lastCalendarSystem = Context.CalendarSystem;
        _lastValueForCalendarInfo = Context.EffectiveDateTime;

        // For Gregorian calendar, clear the cached info (uses direct DateTime values)
        if (Context.CalendarSystem == CalendarSystem.Gregorian)
        {
            Context.ClearCalendarInfo();
            return;
        }

        // Convert the Gregorian date to the target calendar system
        var effectiveDate = DateOnly.FromDateTime(Context.EffectiveDateTime);
        var monthInfo = CalendarProvider.GetCalendarMonthInfo(effectiveDate, Context.CalendarSystem);

        Context.SetCalendarInfo(
            monthInfo.Year,
            monthInfo.Month,
            monthInfo.Day,
            monthInfo.Era,
            monthInfo.DaysInMonth,
            monthInfo.MonthsInYear);
    }

    private void HandleStateChanged()
    {
        // Refresh calendar info if needed
        var needsCalendarRefresh = 
            Context.CalendarSystem != _lastCalendarSystem ||
            !DateTime.Equals(Context.EffectiveDateTime, _lastValueForCalendarInfo);

        if (needsCalendarRefresh && Context.CalendarSystem != CalendarSystem.Gregorian)
        {
            RefreshCalendarInfo();
        }

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
