using TailwindMerge;

namespace SummitUI.Docs.Design;

/// <summary>
/// Utility class for managing Tailwind CSS class names with merge support.
/// Provides a centralized way to combine and resolve conflicting Tailwind classes.
/// </summary>
public static class SuStyles
{
    private static readonly TwMerge TwMerge = new();

    /// <summary>
    /// Merges Tailwind CSS classes, resolving conflicts automatically.
    /// Similar to the cn() function from shadcn/ui.
    /// </summary>
    /// <param name="classNames">The CSS class names to merge.</param>
    /// <returns>A merged string of CSS classes with conflicts resolved, or null if all inputs are null/empty.</returns>
    /// <example>
    /// <code>
    /// // Later classes override earlier ones for the same property
    /// var classes = SuStyles.Cn("bg-red-500", "bg-blue-500"); // Returns "bg-blue-500"
    /// 
    /// // Different properties are preserved
    /// var classes2 = SuStyles.Cn("p-4", "m-2", "p-6"); // Returns "m-2 p-6"
    /// </code>
    /// </example>
    public static string? Cn(params string?[] classNames)
        => TwMerge.Merge(classNames);

    /// <summary>
    /// Conditionally includes a class based on a boolean condition.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="trueClass">The class to include when condition is true.</param>
    /// <param name="falseClass">The class to include when condition is false (optional).</param>
    /// <returns>The appropriate class based on the condition.</returns>
    public static string? When(bool condition, string? trueClass, string? falseClass = null)
        => condition ? trueClass : falseClass;
}
