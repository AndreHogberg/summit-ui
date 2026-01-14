using SummitUI.Base;

namespace SummitUI;

/// <summary>
/// Renders children in a fixed-position container to avoid z-index and overflow issues.
/// Content is visually "portaled" to the top of the stacking context.
/// </summary>
public class SmPopoverPortal : SimplePortalBase<PopoverContext>
{
    /// <inheritdoc />
    protected override string DataAttribute => "data-summit-popover-portal";

    /// <inheritdoc />
    protected override string Position => "fixed";
}
