using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Close button for the dialog.
/// Supports the AsChild pattern for rendering custom elements.
/// </summary>
public class SmDialogClose : ComponentBase
{
    [CascadingParameter]
    private DialogContext Context { get; set; } = default!;

    [Inject]
    private ISummitUILocalizer Localizer { get; set; } = default!;

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
    /// Accessible label for the close button.
    /// If not provided, uses the localized default from <see cref="ISummitUILocalizer"/>.
    /// </summary>
    [Parameter]
    public string? AriaLabel { get; set; }

    /// <summary>
    /// Additional HTML attributes.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;

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
        var attrs = new Dictionary<string, object>();

        // Add type for button elements
        if (As == "button" && !AsChild)
        {
            attrs["type"] = "button";
        }

        attrs["aria-label"] = AriaLabel ?? Localizer["Dialog_CloseLabel"];
        attrs["data-summit-dialog-close"] = true;
        attrs["onclick"] = EventCallback.Factory.Create<MouseEventArgs>(this, HandleClickAsync);

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

    private async Task HandleClickAsync()
    {
        await Context.CloseAsync();
    }
}
