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

    protected override void OnInitialized()
    {
        if (Context == null) 
            throw new InvalidOperationException("DateFieldInput must be used within a DateFieldRoot.");
        
        Context.OnStateChanged += HandleStateChanged;
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
            
            var labels = await JsInterop.GetSegmentLabelsAsync(browserLocale);
            Context.SetSegmentLabels(labels);
            
            var designators = await JsInterop.GetDayPeriodDesignatorsAsync(browserLocale);
            Context.SetDayPeriodDesignators(designators.Am, designators.Pm);
            
            StateHasChanged();
        }
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
