using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI.Base;

/// <summary>
/// Base class for simple portal components that render children in a fixed/absolute position container.
/// Portals help avoid z-index and overflow issues from nested DOM structures.
/// </summary>
/// <typeparam name="TContext">The context type that implements IPortalContext.</typeparam>
public abstract class SimplePortalBase<TContext> : ComponentBase, IDisposable
    where TContext : class, IPortalContext
{
    /// <summary>
    /// The cascading context from the parent component.
    /// </summary>
    [CascadingParameter]
    protected TContext Context { get; set; } = default!;

    /// <summary>
    /// Content to render in the portal.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Optional custom container ID. Defaults to context's PortalId.
    /// </summary>
    [Parameter]
    public string? ContainerId { get; set; }

    private bool _isSubscribed;
    private bool _isDisposed;

    /// <summary>
    /// Gets the actual container ID to use (custom or from context).
    /// </summary>
    protected string ActualContainerId => ContainerId ?? Context.PortalId;

    /// <summary>
    /// Gets the data attribute name for the portal element (e.g., "data-summit-select-portal").
    /// </summary>
    protected abstract string DataAttribute { get; }

    /// <summary>
    /// Gets the CSS position style ("fixed" or "absolute"). Defaults to "absolute".
    /// </summary>
    protected virtual string Position => "absolute";

    /// <summary>
    /// Gets the z-index value. Defaults to 9999.
    /// </summary>
    protected virtual int ZIndex => 9999;

    /// <summary>
    /// Gets the full style string for the portal container.
    /// Override for custom positioning like full-screen overlays.
    /// </summary>
    protected virtual string ContainerStyle => $"position: {Position}; top: 0; left: 0; z-index: {ZIndex}; pointer-events: none;";

    /// <summary>
    /// Whether to wrap content in a pointer-events container. Defaults to true.
    /// </summary>
    protected virtual bool UsePointerEventsWrapper => true;

    /// <summary>
    /// Gets the style for the inner pointer-events wrapper. Override for full-screen dialogs.
    /// </summary>
    protected virtual string InnerWrapperStyle => "pointer-events: auto;";

    /// <summary>
    /// Override to add additional attributes to the portal element.
    /// </summary>
    protected virtual void AddCustomAttributes(RenderTreeBuilder builder, ref int sequence)
    {
        // Base implementation does nothing - override to add custom attributes
    }

    protected override void OnInitialized()
    {
        // Subscribe to context state changes for animation awareness
        Context.OnStateChanged += HandleStateChanged;
        _isSubscribed = true;
    }

    private async void HandleStateChanged()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Only render when open or during close animation
        if (!Context.IsOpen && !Context.IsAnimatingClosed) return;

        var sequence = 0;

        builder.OpenElement(sequence++, "div");
        builder.AddAttribute(sequence++, "id", ActualContainerId);
        builder.AddAttribute(sequence++, DataAttribute, "");
        builder.AddAttribute(sequence++, "style", ContainerStyle);

        // Allow derived classes to add custom attributes
        AddCustomAttributes(builder, ref sequence);

        if (UsePointerEventsWrapper)
        {
            builder.OpenElement(sequence++, "div");
            builder.AddAttribute(sequence++, "style", InnerWrapperStyle);
            builder.AddContent(sequence++, ChildContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddContent(sequence++, ChildContent);
        }

        builder.CloseElement();
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (_isSubscribed)
        {
            Context.OnStateChanged -= HandleStateChanged;
        }
    }
}
