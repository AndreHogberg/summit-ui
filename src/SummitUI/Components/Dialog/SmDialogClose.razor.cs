using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Close button for the dialog.
/// Supports the AsChild pattern for rendering custom elements.
/// </summary>
public partial class SmDialogClose : ComponentBase
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
    private AsChildContext _context = default!;

    protected override void OnInitialized()
    {
        _context = new AsChildContext
        {
            Attrs = BuildAttributes()
        };
    }

    private IReadOnlyDictionary<string, object> BuildAttributes()
    {
        var attrs = new Dictionary<string, object>
        {
            ["type"] = "button",
            ["aria-label"] = AriaLabel ?? Localizer["Dialog_CloseLabel"],
            ["data-summit-dialog-close"] = true,
            ["onclick"] = EventCallback.Factory.Create(this, HandleClickAsync)
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

    private async Task HandleClickAsync()
    {
        await Context.CloseAsync();
    }
}
