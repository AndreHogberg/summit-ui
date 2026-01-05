using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// Renders a single segment of the date field (day, month, year, hour, etc.).
/// Handles keyboard interaction for increment/decrement and navigation.
/// </summary>
public class DateFieldSegment : ComponentBase, IAsyncDisposable
{
    [Inject] private DateFieldJsInterop JsInterop { get; set; } = default!;

    [Parameter, EditorRequired] public DateFieldSegmentState Segment { get; set; } = default!;
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }
    
    [CascadingParameter] public DateFieldContext Context { get; set; } = default!;

    private ElementReference _elementRef;
    private DotNetObjectReference<DateFieldSegment>? _dotNetHelper;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Segment.Type != DateFieldSegmentType.Literal)
        {
            _dotNetHelper = DotNetObjectReference.Create(this);
            await JsInterop.InitializeSegmentAsync(_elementRef, _dotNetHelper);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Segment.Type == DateFieldSegmentType.Literal)
        {
            BuildLiteralSegment(builder);
        }
        else
        {
            BuildEditableSegment(builder);
        }
    }

    private void BuildLiteralSegment(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "aria-hidden", "true");
        builder.AddAttribute(2, "data-segment", "literal");
        builder.AddMultipleAttributes(3, AdditionalAttributes);
        builder.AddContent(4, Segment.LiteralValue);
        builder.CloseElement();
    }

    private void BuildEditableSegment(RenderTreeBuilder builder)
    {
        var segmentHasValue = Context.SegmentHasValue(Segment.Type);
        var segmentText = DateFieldUtils.FormatSegmentValue(Segment.Type, Context);
        var ariaLabel = Context.GetSegmentLabel(Segment.Type);
        var min = DateFieldUtils.GetSegmentMin(Segment.Type, Context);
        var max = DateFieldUtils.GetSegmentMax(Segment.Type, Context);
        var valueNow = GetValueNow();
        
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "role", "spinbutton");
        builder.AddAttribute(2, "aria-label", ariaLabel);
        builder.AddAttribute(3, "aria-valuemin", min);
        builder.AddAttribute(4, "aria-valuemax", max);
        
        if (valueNow.HasValue)
        {
            builder.AddAttribute(5, "aria-valuenow", valueNow.Value);
        }
        
        builder.AddAttribute(6, "aria-valuetext", segmentText);
        builder.AddAttribute(7, "tabindex", Context.Disabled ? "-1" : "0");
        builder.AddAttribute(8, "data-segment", Segment.Type.ToString().ToLowerInvariant());
        builder.AddAttribute(9, "inputmode", Segment.Type == DateFieldSegmentType.DayPeriod ? "text" : "numeric");
        
        if (Context.Disabled) builder.AddAttribute(10, "data-disabled", "");
        if (Context.ReadOnly) builder.AddAttribute(11, "data-readonly", "");
        if (!segmentHasValue) builder.AddAttribute(12, "data-placeholder", "");
        
        builder.AddMultipleAttributes(13, AdditionalAttributes);
        builder.AddElementReferenceCapture(14, el => _elementRef = el);
        
        // Wrap text content in aria-hidden span to prevent screen readers from 
        // reading both aria-valuetext and the visible text content
        builder.OpenElement(15, "span");
        builder.AddAttribute(16, "aria-hidden", "true");
        builder.AddContent(17, segmentText);
        builder.CloseElement();
        
        builder.CloseElement();
    }

    /// <summary>
    /// Called from JavaScript when ArrowUp is pressed.
    /// </summary>
    [JSInvokable]
    public async Task IncrementSegment()
    {
        await Context.IncrementSegmentAsync(Segment.Type);
    }

    /// <summary>
    /// Called from JavaScript when ArrowDown is pressed.
    /// </summary>
    [JSInvokable]
    public async Task DecrementSegment()
    {
        await Context.DecrementSegmentAsync(Segment.Type);
    }

    /// <summary>
    /// Called from JavaScript when a numeric value is typed.
    /// </summary>
    [JSInvokable]
    public async Task SetSegmentValue(int value)
    {
        await Context.SetSegmentValueAsync(Segment.Type, value);
    }

    /// <summary>
    /// Called from JavaScript when Backspace/Delete is pressed to clear the segment.
    /// </summary>
    [JSInvokable]
    public async Task ClearSegment()
    {
        await Context.ClearSegmentAsync(Segment.Type);
    }

    /// <summary>
    /// Called from JavaScript when 'a' or 'p' is pressed on DayPeriod segment.
    /// </summary>
    [JSInvokable]
    public async Task SetDayPeriod(string period)
    {
        await Context.SetDayPeriodAsync(period);
    }

    private int? GetValueNow()
    {
        if (Segment.Type == DateFieldSegmentType.DayPeriod)
        {
            return null; // DayPeriod doesn't have a numeric value
        }
        
        // Check if this specific segment has a value
        if (!Context.SegmentHasValue(Segment.Type))
        {
            return null;
        }
        
        return Context.GetSegmentValue(Segment.Type);
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetHelper?.Dispose();
        if (Segment.Type != DateFieldSegmentType.Literal)
        {
            try
            {
                await JsInterop.DestroySegmentAsync(_elementRef);
            }
            catch (JSDisconnectedException)
            {
                // Safe to ignore, JS resources are cleaned up by the browser
            }
        }
    }
}
