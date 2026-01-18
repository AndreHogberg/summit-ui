using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// Close button for the popover.
/// </summary>
public partial class SmPopoverClose
{
    [CascadingParameter]
    private PopoverContext Context { get; set; } = default!;

    [Inject]
    private ISummitUILocalizer Localizer { get; set; } = default!;

    /// <summary>
    /// Child content (typically button text/icon).
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

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

    private string EffectiveAriaLabel => AriaLabel ?? Localizer["Popover_CloseLabel"];

    private async Task HandleClickAsync()
    {
        await Context.CloseAsync();
    }
}
