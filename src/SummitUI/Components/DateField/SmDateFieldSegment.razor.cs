using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using SummitUI.Interop;

namespace SummitUI;

/// <summary>
/// Renders a single segment of the date field (day, month, year, hour, etc.).
/// Handles keyboard interaction for increment/decrement and navigation.
/// </summary>
public partial class SmDateFieldSegment : ComponentBase, IAsyncDisposable
{
    [Inject] private DateFieldJsInterop JsInterop { get; set; } = default!;

    [Parameter, EditorRequired] public DateFieldSegmentState Segment { get; set; } = default!;
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object>? AdditionalAttributes { get; set; }

    [CascadingParameter] public DateFieldContext Context { get; set; } = default!;

    private ElementReference _elementRef;
    private DotNetObjectReference<SmDateFieldSegment>? _dotNetHelper;
    private bool _isDisposed;

    private bool SegmentHasValue => Context.SegmentHasValue(Segment.Type);
    private string SegmentText => DateFieldUtils.FormatSegmentValue(Segment.Type, Context);
    private string AriaLabel => Context.GetSegmentLabel(Segment.Type);
    private int Min => DateFieldUtils.GetSegmentMin(Segment.Type, Context);
    private int Max => DateFieldUtils.GetSegmentMax(Segment.Type, Context);
    private int? ValueNow => GetValueNow();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Segment.Type != DateFieldSegmentType.Literal && !_isDisposed)
        {
            _dotNetHelper = DotNetObjectReference.Create(this);

            // Check again after creating the reference in case we were disposed during the await
            if (_isDisposed)
            {
                _dotNetHelper.Dispose();
                return;
            }

            try
            {
                await JsInterop.InitializeSegmentAsync(_elementRef, _dotNetHelper);
            }
            catch (ObjectDisposedException)
            {
                // Component was disposed during the JS call, safe to ignore
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected, safe to ignore
            }
        }
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
        if (_isDisposed) return;
        _isDisposed = true;

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
            catch (ObjectDisposedException)
            {
                // Safe to ignore, already disposed
            }
        }
    }
}
