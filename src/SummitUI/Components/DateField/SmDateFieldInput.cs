using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// Container for date field segments. Generates segments based on format configuration.
/// </summary>
public class SmDateFieldInput : ComponentBase, IDisposable
{
    [CascadingParameter] public DateFieldContext Context { get; set; } = default!;

    [Inject] private ISummitUILocalizer Localizer { get; set; } = default!;

    /// <summary>
    /// Optional custom rendering template for segments.
    /// </summary>
    [Parameter] public RenderFragment<IReadOnlyList<DateFieldSegmentState>>? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private List<DateFieldSegmentState> _segments = new();

    protected override void OnInitialized()
    {
        if (Context == null)
            throw new InvalidOperationException("DateFieldInput must be used within a DateFieldRoot.");

        Context.OnStateChanged += HandleStateChanged;
        
        // Initialize segment labels from localizer
        InitializeSegmentLabels();
        
        RegenerateSegments();
    }

    protected override void OnParametersSet()
    {
        RegenerateSegments();
    }

    /// <summary>
    /// Initializes segment labels from the localizer.
    /// </summary>
    private void InitializeSegmentLabels()
    {
        var labels = new Dictionary<string, string>
        {
            ["year"] = Localizer["DateField_YearLabel"],
            ["month"] = Localizer["DateField_MonthLabel"],
            ["day"] = Localizer["DateField_DayLabel"],
            ["hour"] = Localizer["DateField_HourLabel"],
            ["minute"] = Localizer["DateField_MinuteLabel"],
            ["dayPeriod"] = Localizer["DateField_DayPeriodLabel"]
        };
        Context.SetSegmentLabels(labels);
    }

    private void HandleStateChanged()
    {
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
                builder.OpenComponent<SmDateFieldSegment>(seq++);
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
