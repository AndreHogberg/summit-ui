using Microsoft.AspNetCore.Components;
using SummitUI.Components.Otp;

namespace SummitUI;

/// <summary>
/// A visual slot for displaying a single character in an OTP input.
/// Use inside OtpRoot to render individual character slots.
/// </summary>
public partial class SmOtpSlot
{
    /// <summary>
    /// The slot state containing character and active state information.
    /// </summary>
    [Parameter, EditorRequired]
    public OtpSlotState Slot { get; set; } = default!;

    /// <summary>
    /// Content to render inside the slot (e.g., OtpCaret for fake caret).
    /// If not provided, displays the character or placeholder.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional attributes to apply to the slot element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
