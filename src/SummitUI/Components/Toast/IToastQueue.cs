namespace SummitUI;

/// <summary>
/// A queue for managing toast notifications. Generic over content type.
/// </summary>
/// <typeparam name="TContent">User-defined content type (can be anything).</typeparam>
/// <remarks>
/// <para>
/// This interface follows the React Aria toast pattern where users define their own
/// content type. SummitUI provides the queue mechanics and accessibility features,
/// while users have full control over the content structure.
/// </para>
/// <para>
/// Register in DI as a singleton, then inject wherever you need to show toasts.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 1. Define your content type
/// public record MyToast(string Title, string? Description = null);
/// 
/// // 2. Register in DI
/// builder.Services.AddToastQueue&lt;MyToast&gt;();
/// 
/// // 3. Inject and use
/// @inject IToastQueue&lt;MyToast&gt; Toasts
/// 
/// Toasts.Add(new("File saved!", "Your changes are safe."));
/// </code>
/// </example>
public interface IToastQueue<TContent>
{
    /// <summary>
    /// Add a toast to the queue.
    /// </summary>
    /// <param name="content">User-defined content.</param>
    /// <param name="options">Optional settings (timeout, priority, onClose).</param>
    /// <returns>A key that can be used to close the toast programmatically.</returns>
    string Add(TContent content, ToastOptions? options = null);

    /// <summary>
    /// Close a specific toast by key.
    /// </summary>
    /// <param name="key">The key returned from <see cref="Add"/>.</param>
    void Close(string key);

    /// <summary>
    /// Close all toasts immediately.
    /// </summary>
    void CloseAll();

    /// <summary>
    /// Pause all auto-dismiss timers (e.g., on hover/focus).
    /// </summary>
    void PauseAll();

    /// <summary>
    /// Resume all auto-dismiss timers.
    /// </summary>
    void ResumeAll();

    /// <summary>
    /// Currently visible toasts.
    /// </summary>
    IReadOnlyList<QueuedToast<TContent>> VisibleToasts { get; }

    /// <summary>
    /// Maximum number of toasts to display at once.
    /// Additional toasts are queued until visible ones are closed.
    /// </summary>
    int MaxVisibleToasts { get; }

    /// <summary>
    /// Subscribe to queue changes. Returns unsubscribe action.
    /// </summary>
    /// <param name="callback">Called whenever the queue changes.</param>
    /// <returns>An action to unsubscribe.</returns>
    Action Subscribe(Action callback);
}
