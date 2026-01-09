namespace SummitUI;

/// <summary>
/// Represents a toast in the queue with user-defined content.
/// </summary>
/// <typeparam name="TContent">User-defined content type.</typeparam>
public sealed class QueuedToast<TContent>
{
    /// <summary>
    /// Unique key for this toast. Use this to close the toast programmatically.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// User-defined content.
    /// </summary>
    public required TContent Content { get; init; }

    /// <summary>
    /// Auto-dismiss timeout in milliseconds, or null for manual close only.
    /// </summary>
    public int? Timeout { get; init; }

    /// <summary>
    /// Screen reader announcement priority.
    /// </summary>
    public ToastPriority Priority { get; init; }

    /// <summary>
    /// When the toast was added to the queue.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Internal timer for auto-dismiss. Managed by <see cref="ToastQueue{TContent}"/>.
    /// </summary>
    internal ToastTimer? Timer { get; set; }

    /// <summary>
    /// Callback when the toast is closed.
    /// </summary>
    internal Action? OnClose { get; init; }
}

/// <summary>
/// A pausable timer for toast auto-dismiss.
/// </summary>
internal sealed class ToastTimer : IDisposable
{
    private readonly Action _callback;
    private readonly int _duration;
    private Timer? _timer;
    private DateTime _startTime;
    private int _remainingMs;
    private bool _isPaused;
    private bool _isDisposed;

    public ToastTimer(Action callback, int durationMs)
    {
        _callback = callback;
        _duration = durationMs;
        _remainingMs = durationMs;
    }

    public void Start()
    {
        if (_isDisposed) return;
        _startTime = DateTime.UtcNow;
        _timer = new Timer(_ => _callback(), null, _remainingMs, Timeout.Infinite);
    }

    public void Pause()
    {
        if (_isDisposed || _isPaused || _timer is null) return;

        _isPaused = true;
        _timer.Change(Timeout.Infinite, Timeout.Infinite);

        var elapsed = (int)(DateTime.UtcNow - _startTime).TotalMilliseconds;
        _remainingMs = Math.Max(0, _remainingMs - elapsed);
    }

    public void Resume()
    {
        if (_isDisposed || !_isPaused || _timer is null) return;

        _isPaused = false;
        _startTime = DateTime.UtcNow;
        _timer.Change(_remainingMs, Timeout.Infinite);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _timer?.Dispose();
        _timer = null;
    }
}
