using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using SummitUI.Components.Otp;

namespace SummitUI;

/// <summary>
/// A visual slot for displaying a single character in an OTP input.
/// Use inside OtpRoot to render individual character slots.
/// </summary>
public class SmOtpSlot : ComponentBase
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

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "data-otp-slot", true);
        
        builder.AddAttribute(2, "data-active", Slot.IsActive ? "true" : null);
        
        builder.AddMultipleAttributes(3, AdditionalAttributes);

        // Render character, placeholder, or child content (for caret)
        if (Slot.HasFakeCaret && ChildContent != null)
        {
            // Show child content (typically OtpCaret) when slot is active and empty
            builder.AddContent(4, ChildContent);
        }
        else if (Slot.Char.HasValue)
        {
            // Show the character
            builder.AddContent(5, Slot.Char.Value.ToString());
        }
        else if (Slot.PlaceholderChar.HasValue)
        {
            // Show placeholder
            builder.OpenElement(6, "span");
            builder.AddAttribute(7, "data-otp-placeholder", true);
            builder.AddContent(8, Slot.PlaceholderChar.Value.ToString());
            builder.CloseElement();
        }

        builder.CloseElement();
    }
}
