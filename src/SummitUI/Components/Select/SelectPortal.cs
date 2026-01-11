using SummitUI.Base;

namespace SummitUI;

/// <summary>
/// Renders children in a fixed-position container to avoid z-index and overflow issues.
/// Content is visually "portaled" to the top of the stacking context.
/// </summary>
/// <typeparam name="TValue">The type of the select value.</typeparam>
public class SelectPortal<TValue> : SimplePortalBase<SelectContext<TValue>> where TValue : notnull
{
    /// <inheritdoc />
    protected override string DataAttribute => "data-summit-select-portal";
}
