namespace SummitUI.Docs.Design;

/// <summary>
/// Size variants for components.
/// </summary>
public enum SuSize
{
    /// <summary>Extra small size.</summary>
    ExtraSmall,
    /// <summary>Small size.</summary>
    Small,
    /// <summary>Medium size (default).</summary>
    Medium,
    /// <summary>Large size.</summary>
    Large,
    /// <summary>Extra large size.</summary>
    ExtraLarge
}

/// <summary>
/// Button style variants.
/// </summary>
public enum SuButtonVariant
{
    /// <summary>Primary button with solid blue background.</summary>
    Primary,
    /// <summary>Secondary button with muted background.</summary>
    Secondary,
    /// <summary>Ghost button with transparent background, hover effect only.</summary>
    Ghost,
    /// <summary>Outline button with border and transparent background.</summary>
    Outline,
    /// <summary>Destructive button for dangerous actions.</summary>
    Destructive,
    /// <summary>Link-style button that looks like a text link.</summary>
    Link
}

/// <summary>
/// Badge style variants.
/// </summary>
public enum SuBadgeVariant
{
    /// <summary>Default badge with muted background.</summary>
    Default,
    /// <summary>Primary badge with blue background.</summary>
    Primary,
    /// <summary>Secondary badge with subtle background.</summary>
    Secondary,
    /// <summary>Outline badge with border only.</summary>
    Outline,
    /// <summary>Destructive badge for error states.</summary>
    Destructive,
    /// <summary>Success badge for positive states.</summary>
    Success,
    /// <summary>Warning badge for caution states.</summary>
    Warning
}

/// <summary>
/// Card style variants.
/// </summary>
public enum SuCardVariant
{
    /// <summary>Default card with border.</summary>
    Default,
    /// <summary>Elevated card with shadow.</summary>
    Elevated,
    /// <summary>Ghost card with no border or shadow.</summary>
    Ghost,
    /// <summary>Interactive card with hover effects.</summary>
    Interactive
}

/// <summary>
/// Alert style variants.
/// </summary>
public enum SuAlertVariant
{
    /// <summary>Default informational alert.</summary>
    Default,
    /// <summary>Informational alert (blue).</summary>
    Info,
    /// <summary>Success alert (green).</summary>
    Success,
    /// <summary>Warning alert (yellow/amber).</summary>
    Warning,
    /// <summary>Destructive/error alert (red).</summary>
    Destructive,
    /// <summary>Tip alert for helpful hints.</summary>
    Tip
}

/// <summary>
/// Heading levels (h1-h6).
/// </summary>
public enum SuHeadingLevel
{
    /// <summary>Heading level 1 (largest).</summary>
    H1 = 1,
    /// <summary>Heading level 2.</summary>
    H2 = 2,
    /// <summary>Heading level 3.</summary>
    H3 = 3,
    /// <summary>Heading level 4.</summary>
    H4 = 4,
    /// <summary>Heading level 5.</summary>
    H5 = 5,
    /// <summary>Heading level 6 (smallest).</summary>
    H6 = 6
}

/// <summary>
/// Text size variants.
/// </summary>
public enum SuTextSize
{
    /// <summary>Extra small text.</summary>
    ExtraSmall,
    /// <summary>Small text.</summary>
    Small,
    /// <summary>Base/default text size.</summary>
    Base,
    /// <summary>Large text.</summary>
    Large,
    /// <summary>Extra large text.</summary>
    ExtraLarge
}

/// <summary>
/// Container max-width variants.
/// </summary>
public enum SuContainerSize
{
    /// <summary>Small container (max-w-sm).</summary>
    Small,
    /// <summary>Medium container (max-w-md).</summary>
    Medium,
    /// <summary>Large container (max-w-lg).</summary>
    Large,
    /// <summary>Extra large container (max-w-xl).</summary>
    ExtraLarge,
    /// <summary>2XL container (max-w-2xl).</summary>
    TwoXl,
    /// <summary>3XL container (max-w-3xl).</summary>
    ThreeXl,
    /// <summary>4XL container (max-w-4xl).</summary>
    FourXl,
    /// <summary>Full width container.</summary>
    Full
}

/// <summary>
/// Divider orientation.
/// </summary>
public enum SuOrientation
{
    /// <summary>Horizontal divider.</summary>
    Horizontal,
    /// <summary>Vertical divider.</summary>
    Vertical
}
