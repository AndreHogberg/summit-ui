namespace SummitUI.Services;

/// <summary>
/// Priority levels for screen reader announcements.
/// </summary>
public enum AnnouncementPriority
{
    /// <summary>
    /// Polite announcements wait for the user to finish their current task.
    /// Use for non-urgent information like selection confirmations.
    /// Maps to aria-live="polite".
    /// </summary>
    Polite,

    /// <summary>
    /// Assertive announcements interrupt the user immediately.
    /// Use sparingly for urgent information like errors or time-sensitive alerts.
    /// Maps to aria-live="assertive".
    /// </summary>
    Assertive
}

/// <summary>
/// Service for making announcements to screen readers via ARIA live regions.
/// </summary>
/// <remarks>
/// <para>
/// This service provides a centralized way to announce dynamic content changes
/// to assistive technologies. Components can inject this service to announce
/// state changes without managing their own live regions.
/// </para>
/// <para>
/// To use this service, place <see cref="SmLiveAnnouncer"/> once at your app's
/// root level (e.g., in MainLayout.razor or App.razor).
/// </para>
/// <para>
/// If the service is not registered or the component is not placed, components
/// should fall back gracefully (announcements are simply not made).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In a component
/// [Inject]
/// private ILiveAnnouncer? Announcer { get; set; }
/// 
/// private void OnSelectionChanged(string label)
/// {
///     Announcer?.Announce($"{label} selected");
/// }
/// </code>
/// </example>
public interface ILiveAnnouncer
{
    /// <summary>
    /// Announces a message to screen readers.
    /// </summary>
    /// <param name="message">The message to announce. Should be localized by the caller.</param>
    /// <param name="priority">
    /// The urgency level. Polite waits for the user, assertive interrupts immediately.
    /// Defaults to <see cref="AnnouncementPriority.Polite"/>.
    /// </param>
    void Announce(string message, AnnouncementPriority priority = AnnouncementPriority.Polite);

    /// <summary>
    /// Clears the current announcement for the specified priority.
    /// </summary>
    /// <param name="priority">The priority level to clear.</param>
    void Clear(AnnouncementPriority priority);

    /// <summary>
    /// Clears all announcements (both polite and assertive).
    /// </summary>
    void ClearAll();

    /// <summary>
    /// Event raised when any announcement changes.
    /// The <see cref="SmLiveAnnouncer"/> component subscribes to this event.
    /// </summary>
    event Action? OnAnnouncementChanged;

    /// <summary>
    /// Gets the current polite announcement text.
    /// </summary>
    string PoliteMessage { get; }

    /// <summary>
    /// Gets the current assertive announcement text.
    /// </summary>
    string AssertiveMessage { get; }
}
