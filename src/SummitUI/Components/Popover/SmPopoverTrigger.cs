using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Trigger button that toggles the popover open/closed.
/// Supports the AsChild pattern for rendering custom elements.
/// </summary>
public class SmPopoverTrigger : ComponentBase
{
    [CascadingParameter]
    private PopoverContext Context { get; set; } = default!;

    /// <summary>
    /// When true, the component will not render a wrapper element.
    /// Instead, it passes attributes via context to the child element.
    /// The child must apply @attributes="context.Attrs" for proper functionality.
    /// </summary>
    [Parameter]
    public bool AsChild { get; set; }

    /// <summary>
    /// Child content. When AsChild is true, receives an AsChildContext with attributes to apply.
    /// </summary>
    [Parameter]
    public RenderFragment<AsChildContext>? ChildContent { get; set; }

    /// <summary>
    /// HTML element to render when AsChild is false. Defaults to "button".
    /// </summary>
    [Parameter]
    public string As { get; set; } = "button";

    /// <summary>
    /// Additional HTML attributes to apply to the element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;

    private string DataState => Context.IsOpen ? "open" : "closed";

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Context.RegisterTrigger(_elementRef);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var context = new AsChildContext
        {
            Attrs = BuildAttributes(),
            RefCallback = el => _elementRef = el
        };

        if (AsChild)
        {
            // Render only the child content with context - no wrapper element
            builder.AddContent(0, ChildContent?.Invoke(context));
        }
        else
        {
            // Render wrapper element
            builder.OpenElement(0, As);
            builder.AddMultipleAttributes(1, context.Attrs);
            builder.AddElementReferenceCapture(2, el => _elementRef = el);
            builder.AddContent(3, ChildContent?.Invoke(context));
            builder.CloseElement();
        }
    }

    private IReadOnlyDictionary<string, object> BuildAttributes()
    {
        var attrs = new Dictionary<string, object>
        {
            ["aria-haspopup"] = "dialog",
            ["aria-expanded"] = Context.IsOpen.ToString().ToLowerInvariant(),
            ["aria-controls"] = Context.PopoverId,
            ["data-state"] = DataState,
            ["data-summit-popover-trigger"] = true,
            ["onclick"] = EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync),
            ["onkeydown"] = EventCallback.Factory.Create<KeyboardEventArgs>(this, HandleKeyDownAsync)
        };

        // Merge additional attributes (consumer attributes win)
        if (AdditionalAttributes is not null)
        {
            foreach (var (key, value) in AdditionalAttributes)
            {
                attrs[key] = value;
            }
        }

        return attrs;
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        await Context.ToggleAsync();
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        // Only handle Enter/Space for non-button elements.
        // Button elements automatically fire a click event on Enter/Space,
        // so the click handler will take care of toggling.
        if (As != "button" && args.Key is "Enter" or " ")
        {
            await Context.ToggleAsync();
        }
    }
}
