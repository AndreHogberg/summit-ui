namespace SummitUI.Docs.Client;

/// <summary>
/// Toast content type for the SummitUI documentation site.
/// </summary>
public record DocsToastContent
{
    /// <summary>
    /// Optional title displayed prominently.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Main message or description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Visual variant for styling ("default", "success", "error", "warning").
    /// </summary>
    public string Variant { get; init; } = "default";

    /// <summary>
    /// Optional action button.
    /// </summary>
    public DocsToastAction? Action { get; init; }
}

/// <summary>
/// An action button that can be shown on a toast.
/// </summary>
public record DocsToastAction
{
    /// <summary>
    /// Button label text.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Callback when the action is clicked.
    /// </summary>
    public Action? OnClick { get; init; }

    /// <summary>
    /// Accessible description of what the action does.
    /// </summary>
    public string? AltText { get; init; }
}

/// <summary>
/// Extension methods for showing toasts in the docs site.
/// </summary>
public static class DocsToastExtensions
{
    /// <summary>
    /// Show a simple toast with just a message.
    /// </summary>
    public static string Show(this IToastQueue<DocsToastContent> queue, string message, int? timeout = 5000)
    {
        return queue.Add(new DocsToastContent { Description = message }, new ToastOptions { Timeout = timeout });
    }

    /// <summary>
    /// Show a toast with a title and description.
    /// </summary>
    public static string Show(this IToastQueue<DocsToastContent> queue, string title, string description, int? timeout = 5000)
    {
        return queue.Add(new DocsToastContent { Title = title, Description = description }, new ToastOptions { Timeout = timeout });
    }

    /// <summary>
    /// Show a success toast.
    /// </summary>
    public static string Success(this IToastQueue<DocsToastContent> queue, string title, string? description = null, int? timeout = 5000)
    {
        return queue.Add(new DocsToastContent { Title = title, Description = description, Variant = "success" }, new ToastOptions { Timeout = timeout });
    }

    /// <summary>
    /// Show an error toast.
    /// </summary>
    public static string Error(this IToastQueue<DocsToastContent> queue, string title, string? description = null, int? timeout = 8000)
    {
        return queue.Add(new DocsToastContent { Title = title, Description = description, Variant = "error" }, new ToastOptions { Timeout = timeout, Priority = ToastPriority.Assertive });
    }

    /// <summary>
    /// Show a warning toast.
    /// </summary>
    public static string Warning(this IToastQueue<DocsToastContent> queue, string title, string? description = null, int? timeout = 6000)
    {
        return queue.Add(new DocsToastContent { Title = title, Description = description, Variant = "warning" }, new ToastOptions { Timeout = timeout });
    }

    /// <summary>
    /// Show a toast with an action button.
    /// </summary>
    public static string ShowWithAction(this IToastQueue<DocsToastContent> queue, string title, string description, DocsToastAction action, int? timeout = null)
    {
        return queue.Add(new DocsToastContent { Title = title, Description = description, Action = action }, new ToastOptions { Timeout = timeout });
    }
}
