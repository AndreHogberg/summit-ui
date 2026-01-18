using Microsoft.AspNetCore.Components;

namespace SummitUI;

/// <summary>
/// A fake blinking caret for OTP slots.
/// Use inside OtpSlot to show a caret when the slot is active and empty.
/// </summary>
public partial class SmOtpCaret
{
    /// <summary>
    /// The CSS class for the caret element.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Additional attributes to apply to the caret element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IDictionary<string, object>? AdditionalAttributes { get; set; }
}
