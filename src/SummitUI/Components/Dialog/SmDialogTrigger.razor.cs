using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SummitUI;

/// <summary>
/// Trigger button that opens the dialog when clicked.
/// Supports the AsChild pattern for rendering custom elements.
/// </summary>
public partial class SmDialogTrigger : ComponentBase
{
    [CascadingParameter]
    private DialogContext Context { get; set; } = default!;

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
    /// Additional HTML attributes to apply to the element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }

    private ElementReference _elementRef;
    private AsChildContext _asChildContext = default!;

    private string DataState => Context.IsOpen ? "open" : "closed";

    protected override void OnInitialized()
    {
        _asChildContext = new AsChildContext
        {
            Attrs = BuildAttributes(),
            RefCallback = el => _elementRef = el
        };
    }

    protected override void OnParametersSet()
    {
        // Update the AsChildContext with current attributes
        _asChildContext = new AsChildContext
        {
            Attrs = BuildAttributes(),
            RefCallback = el => _elementRef = el
        };
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            Context.RegisterTrigger(_elementRef);
        }
    }

    private IReadOnlyDictionary<string, object> BuildAttributes()
    {
        var attrs = new Dictionary<string, object>
        {
            ["type"] = "button",
            ["aria-haspopup"] = "dialog",
            ["aria-expanded"] = Context.IsOpen.ToString().ToLowerInvariant(),
            ["aria-controls"] = Context.DialogId,
            ["data-state"] = DataState,
            ["data-summit-dialog-trigger"] = true,
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
        await Context.OpenAsync();
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs args)
    {
        // Enter/Space are handled natively by button elements
        if (args.Key is "Enter" or " ")
        {
            await Context.OpenAsync();
        }
    }
}
