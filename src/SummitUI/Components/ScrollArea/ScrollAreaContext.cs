using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Cascading context shared between scroll area sub-components.
/// Provides state and callbacks for coordinating viewport, scrollbars, and thumb components.
/// </summary>
public sealed class ScrollAreaContext
{
    /// <summary>
    /// Unique identifier for this scroll area instance.
    /// </summary>
    public string ScrollAreaId { get; }

    /// <summary>
    /// The visibility behavior of scrollbars.
    /// </summary>
    public ScrollAreaType Type { get; internal set; } = ScrollAreaType.Hover;

    /// <summary>
    /// Delay in milliseconds before hiding scrollbars (for Hover and Scroll types).
    /// </summary>
    public int ScrollHideDelay { get; internal set; } = 600;

    /// <summary>
    /// Text direction (ltr or rtl). Affects scrollbar positioning.
    /// </summary>
    public string Dir { get; internal set; } = "ltr";

    /// <summary>
    /// Reference to the viewport element, set by ScrollAreaViewport.
    /// </summary>
    public ElementReference ViewportRef { get; internal set; }

    /// <summary>
    /// Indicates whether the viewport reference has been set.
    /// </summary>
    public bool HasViewportRef { get; internal set; }

    /// <summary>
    /// Tracks whether a vertical scrollbar is registered.
    /// </summary>
    public bool HasVerticalScrollbar { get; internal set; }

    /// <summary>
    /// Tracks whether a horizontal scrollbar is registered.
    /// </summary>
    public bool HasHorizontalScrollbar { get; internal set; }

    /// <summary>
    /// Current visibility state of the vertical scrollbar.
    /// </summary>
    public string VerticalScrollbarState { get; internal set; } = "hidden";

    /// <summary>
    /// Current visibility state of the horizontal scrollbar.
    /// </summary>
    public string HorizontalScrollbarState { get; internal set; } = "hidden";

    /// <summary>
    /// Whether vertical content overflows (content height > viewport height).
    /// </summary>
    public bool HasVerticalOverflow { get; internal set; }

    /// <summary>
    /// Whether horizontal content overflows (content width > viewport width).
    /// </summary>
    public bool HasHorizontalOverflow { get; internal set; }

    /// <summary>
    /// Callback to notify the root component that state has changed.
    /// </summary>
    public Action NotifyStateChanged { get; internal set; } = () => { };

    /// <summary>
    /// Callback to set viewport reference from the viewport component.
    /// </summary>
    public Action<ElementReference> SetViewportRef { get; internal set; } = _ => { };

    /// <summary>
    /// Callback to register a scrollbar with the context.
    /// </summary>
    public Action<ScrollAreaOrientation> RegisterScrollbar { get; internal set; } = _ => { };

    /// <summary>
    /// Callback to unregister a scrollbar from the context.
    /// </summary>
    public Action<ScrollAreaOrientation> UnregisterScrollbar { get; internal set; } = _ => { };

    /// <summary>
    /// Callback invoked when the scroll area needs to initialize JS interop.
    /// Returns the instance ID for JS operations.
    /// </summary>
    public Func<Task<string?>> InitializeAsync { get; internal set; } = () => Task.FromResult<string?>(null);

    /// <summary>
    /// The JS instance ID for this scroll area, set after initialization.
    /// </summary>
    public string? InstanceId { get; internal set; }

    /// <summary>
    /// Callback to scroll the viewport to a specific position.
    /// Parameters: orientation, position (0-1 ratio)
    /// </summary>
    public Func<ScrollAreaOrientation, double, Task> ScrollToPositionAsync { get; internal set; } = 
        (_, _) => Task.CompletedTask;

    public ScrollAreaContext()
    {
        ScrollAreaId = $"summit-scroll-area-{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Generates a unique ID for the viewport element.
    /// </summary>
    public string GetViewportId() => $"{ScrollAreaId}-viewport";

    /// <summary>
    /// Generates a unique ID for a scrollbar element.
    /// </summary>
    public string GetScrollbarId(ScrollAreaOrientation orientation) => 
        $"{ScrollAreaId}-scrollbar-{orientation.ToString().ToLowerInvariant()}";

    /// <summary>
    /// Generates a unique ID for a thumb element.
    /// </summary>
    public string GetThumbId(ScrollAreaOrientation orientation) => 
        $"{ScrollAreaId}-thumb-{orientation.ToString().ToLowerInvariant()}";

    /// <summary>
    /// Gets the visibility state for a scrollbar based on orientation.
    /// </summary>
    public string GetScrollbarState(ScrollAreaOrientation orientation) =>
        orientation == ScrollAreaOrientation.Vertical ? VerticalScrollbarState : HorizontalScrollbarState;

    /// <summary>
    /// Updates the scrollbar visibility state.
    /// </summary>
    internal void SetScrollbarState(ScrollAreaOrientation orientation, string state)
    {
        if (orientation == ScrollAreaOrientation.Vertical)
            VerticalScrollbarState = state;
        else
            HorizontalScrollbarState = state;
    }

    /// <summary>
    /// Updates overflow state from JS measurements.
    /// </summary>
    internal void SetOverflowState(bool hasVerticalOverflow, bool hasHorizontalOverflow)
    {
        HasVerticalOverflow = hasVerticalOverflow;
        HasHorizontalOverflow = hasHorizontalOverflow;
    }

    /// <summary>
    /// Determines if a scrollbar should be visible based on type and overflow.
    /// </summary>
    public bool ShouldShowScrollbar(ScrollAreaOrientation orientation)
    {
        var hasOverflow = orientation == ScrollAreaOrientation.Vertical 
            ? HasVerticalOverflow 
            : HasHorizontalOverflow;

        return Type switch
        {
            ScrollAreaType.Always => true,
            ScrollAreaType.Auto => hasOverflow,
            ScrollAreaType.Hover => hasOverflow,
            ScrollAreaType.Scroll => hasOverflow,
            _ => false
        };
    }
}
