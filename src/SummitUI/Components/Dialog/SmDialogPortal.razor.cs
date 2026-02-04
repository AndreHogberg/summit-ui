using Microsoft.AspNetCore.Components.Rendering;

using SummitUI.Base;

namespace SummitUI;

/// <summary>
/// Renders dialog children in a fixed-position container at the top of the stacking context.
/// This avoids z-index and overflow issues that can occur with nested DOM structures.
/// Uses depth-based z-index calculation for proper nested dialog stacking.
/// </summary>
public partial class SmDialogPortal : SimplePortalBase<DialogContext>
{
    /// <inheritdoc />
    protected override string DataAttribute => "data-summit-dialog-portal";

    /// <inheritdoc />
    protected override string Position => "fixed";

    /// <summary>
    /// Z-index calculated based on dialog depth (base 9999, increment by 10 per depth level).
    /// </summary>
    protected override int ZIndex => 9999 + (Context.Depth * 10);

    /// <summary>
    /// Full-screen container for dialog overlay positioning.
    /// </summary>
    protected override string ContainerStyle =>
        $"position: {Position}; top: 0; left: 0; right: 0; bottom: 0; z-index: {ZIndex}; pointer-events: none;";

    /// <summary>
    /// Inner wrapper covers full viewport to ensure overlay and dialog work correctly when nested.
    /// </summary>
    protected override string InnerWrapperStyle =>
        "pointer-events: auto; position: fixed; top: 0; left: 0; right: 0; bottom: 0;";

    /// <inheritdoc />
    protected override void AddCustomAttributes(RenderTreeBuilder builder, ref int sequence)
    {
        builder.AddAttribute(sequence++, "data-depth", Context.Depth);
    }
}
