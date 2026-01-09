namespace SummitUI;

/// <summary>
/// Options for toast behavior (not content).
/// </summary>
/// <remarks>
/// These options control how the toast behaves, not what it displays.
/// Content is entirely user-defined via the generic <c>TContent</c> type.
/// </remarks>
public sealed record ToastOptions
{
    /// <summary>
    /// Auto-dismiss timeout in milliseconds.
    /// Minimum 5000ms is enforced for accessibility (screen reader users need time).
    /// Null means no auto-dismiss (user must close manually).
    /// </summary>
    /// <remarks>
    /// WCAG recommends at least 5 seconds for users to read and comprehend notifications.
    /// If a value less than 5000 is provided, it will be clamped to 5000.
    /// </remarks>
    public int? Timeout { get; init; }

    /// <summary>
    /// Priority for screen reader announcements.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description><see cref="ToastPriority.Polite"/> - Announced at next graceful opportunity</description></item>
    /// <item><description><see cref="ToastPriority.Assertive"/> - Announced immediately, interrupting current speech</description></item>
    /// </list>
    /// Use <see cref="ToastPriority.Assertive"/> sparingly - only for critical errors or important alerts.
    /// </remarks>
    public ToastPriority Priority { get; init; } = ToastPriority.Polite;

    /// <summary>
    /// Callback when the toast is closed (by user action or timeout).
    /// </summary>
    public Action? OnClose { get; init; }

    /// <summary>
    /// Gets the effective timeout, clamped to the minimum of 5000ms for accessibility.
    /// </summary>
    internal int? EffectiveTimeout => Timeout.HasValue ? Math.Max(Timeout.Value, 5000) : null;
}

/// <summary>
/// Priority level for screen reader announcements.
/// </summary>
public enum ToastPriority
{
    /// <summary>
    /// Announced at next graceful opportunity (aria-live="polite").
    /// Use for non-critical notifications.
    /// </summary>
    Polite,

    /// <summary>
    /// Announced immediately, interrupting current speech (aria-live="assertive").
    /// Use sparingly - only for critical errors or important alerts.
    /// </summary>
    Assertive
}
