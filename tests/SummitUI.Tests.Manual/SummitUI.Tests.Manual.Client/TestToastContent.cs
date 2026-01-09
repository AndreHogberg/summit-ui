namespace SummitUI.Tests.Manual.Client;

/// <summary>
/// Toast content type for manual tests.
/// </summary>
public record TestToastContent
{
    public string? Title { get; init; }
    public string Description { get; init; } = "";
    public string Variant { get; init; } = "default";
    public TestToastAction? Action { get; init; }
}

/// <summary>
/// Action configuration for test toasts.
/// </summary>
public record TestToastAction
{
    public string Label { get; init; } = "";
    public string AltText { get; init; } = "";
    public Action? OnClick { get; init; }
}

/// <summary>
/// Extension methods for IToastQueue&lt;TestToastContent&gt;.
/// </summary>
public static class TestToastExtensions
{
    public static string Show(this IToastQueue<TestToastContent> queue, string description)
        => queue.Add(new TestToastContent { Description = description });

    public static string Show(this IToastQueue<TestToastContent> queue, string title, string description)
        => queue.Add(new TestToastContent { Title = title, Description = description });

    public static string ShowWithAction(
        this IToastQueue<TestToastContent> queue,
        string title,
        string description,
        TestToastAction action)
        => queue.Add(new TestToastContent
        {
            Title = title,
            Description = description,
            Action = action
        });

    public static string Warning(this IToastQueue<TestToastContent> queue, string title, string description)
        => queue.Add(new TestToastContent
        {
            Title = title,
            Description = description,
            Variant = "warning"
        });
}
