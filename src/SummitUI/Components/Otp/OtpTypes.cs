namespace SummitUI.Components.Otp;

/// <summary>
/// Represents the state of a single OTP slot for rendering.
/// </summary>
public sealed class OtpSlotState
{
    /// <summary>
    /// The index of this slot (0-based).
    /// </summary>
    public int Index { get; init; }
    
    /// <summary>
    /// The character in this slot, or null if empty.
    /// </summary>
    public char? Char { get; init; }
    
    /// <summary>
    /// The placeholder character for this slot, or null if not showing placeholder.
    /// </summary>
    public char? PlaceholderChar { get; init; }
    
    /// <summary>
    /// Whether this slot is currently selected/active.
    /// </summary>
    public bool IsActive { get; init; }
    
    /// <summary>
    /// Whether this slot should show a fake caret (IsActive and no character).
    /// </summary>
    public bool HasFakeCaret => IsActive && Char == null;
}

/// <summary>
/// Context provided to custom OTP slot rendering.
/// </summary>
public sealed class OtpRenderContext
{
    /// <summary>
    /// The list of slot states for rendering.
    /// </summary>
    public required IReadOnlyList<OtpSlotState> Slots { get; init; }
    
    /// <summary>
    /// Whether the OTP input is currently focused.
    /// </summary>
    public bool IsFocused { get; init; }
    
    /// <summary>
    /// Whether the mouse is hovering over the OTP input.
    /// </summary>
    public bool IsHovering { get; init; }
}

/// <summary>
/// Text alignment options for the OTP input.
/// </summary>
public enum OtpTextAlign
{
    /// <summary>
    /// Align text/selection to the left (default).
    /// </summary>
    Left,
    
    /// <summary>
    /// Align text/selection to the center.
    /// </summary>
    Center,
    
    /// <summary>
    /// Align text/selection to the right.
    /// </summary>
    Right
}
