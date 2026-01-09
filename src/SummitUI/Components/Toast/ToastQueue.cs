namespace SummitUI;

/// <summary>
/// A FIFO queue for managing toast notifications.
/// </summary>
/// <typeparam name="TContent">User-defined content type.</typeparam>
/// <remarks>
/// <para>
/// Uses <see cref="Queue{T}"/> internally for FIFO ordering.
/// First toast added is first shown, first to be dismissed.
/// </para>
/// <para>
/// Thread-safe for Blazor Server scenarios.
/// </para>
/// </remarks>
public sealed class ToastQueue<TContent> : IToastQueue<TContent>
{
    private readonly Lock _lock = new();
    private readonly Queue<QueuedToast<TContent>> _queue = new();
    private readonly HashSet<Action> _subscriptions = [];

    /// <summary>
    /// Maximum number of toasts to display at once.
    /// </summary>
    public int MaxVisibleToasts { get; init; } = 5;

    /// <inheritdoc />
    public IReadOnlyList<QueuedToast<TContent>> VisibleToasts
    {
        get
        {
            lock (_lock)
            {
                return _queue.Take(MaxVisibleToasts).ToList();
            }
        }
    }

    /// <inheritdoc />
    public string Add(TContent content, ToastOptions? options = null)
    {
        var key = Guid.NewGuid().ToString("N")[..8];
        var effectiveTimeout = options?.EffectiveTimeout;

        var toast = new QueuedToast<TContent>
        {
            Key = key,
            Content = content,
            Timeout = effectiveTimeout,
            Priority = options?.Priority ?? ToastPriority.Polite,
            CreatedAt = DateTime.UtcNow,
            OnClose = options?.OnClose
        };

        lock (_lock)
        {
            _queue.Enqueue(toast);

            // Start auto-dismiss timer if timeout specified
            if (effectiveTimeout.HasValue)
            {
                var timer = new ToastTimer(() => Close(key), effectiveTimeout.Value);
                toast.Timer = timer;
                timer.Start();
            }
        }

        NotifySubscribers();
        return key;
    }

    /// <inheritdoc />
    public void Close(string key)
    {
        QueuedToast<TContent>? closedToast = null;

        lock (_lock)
        {
            var remaining = new List<QueuedToast<TContent>>();

            while (_queue.Count > 0)
            {
                var toast = _queue.Dequeue();
                if (toast.Key == key)
                {
                    closedToast = toast;
                    toast.Timer?.Dispose();
                }
                else
                {
                    remaining.Add(toast);
                }
            }

            foreach (var toast in remaining)
            {
                _queue.Enqueue(toast);
            }
        }

        closedToast?.OnClose?.Invoke();
        NotifySubscribers();
    }

    /// <inheritdoc />
    public void CloseAll()
    {
        List<QueuedToast<TContent>> closedToasts;

        lock (_lock)
        {
            closedToasts = [.. _queue];

            foreach (var toast in closedToasts)
            {
                toast.Timer?.Dispose();
            }

            _queue.Clear();
        }

        foreach (var toast in closedToasts)
        {
            toast.OnClose?.Invoke();
        }

        NotifySubscribers();
    }

    /// <inheritdoc />
    public void PauseAll()
    {
        lock (_lock)
        {
            foreach (var toast in _queue)
            {
                toast.Timer?.Pause();
            }
        }
    }

    /// <inheritdoc />
    public void ResumeAll()
    {
        lock (_lock)
        {
            foreach (var toast in _queue)
            {
                toast.Timer?.Resume();
            }
        }
    }

    /// <inheritdoc />
    public Action Subscribe(Action callback)
    {
        lock (_lock)
        {
            _subscriptions.Add(callback);
        }

        return () =>
        {
            lock (_lock)
            {
                _subscriptions.Remove(callback);
            }
        };
    }

    private void NotifySubscribers()
    {
        HashSet<Action> subscribers;

        lock (_lock)
        {
            subscribers = [.. _subscriptions];
        }

        foreach (var callback in subscribers)
        {
            try
            {
                callback();
            }
            catch
            {
                // Ignore subscriber errors
            }
        }
    }
}
