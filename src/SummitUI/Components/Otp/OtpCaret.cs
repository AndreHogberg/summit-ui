using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace SummitUI;

/// <summary>
/// A fake blinking caret for OTP slots.
/// Use inside OtpSlot to show a caret when the slot is active and empty.
/// </summary>
public class OtpCaret : ComponentBase
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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "data-otp-caret", true);
        
        if (!string.IsNullOrEmpty(Class))
        {
            builder.AddAttribute(2, "class", Class);
        }
        
        builder.AddMultipleAttributes(3, AdditionalAttributes);
        builder.CloseElement();
    }
}
