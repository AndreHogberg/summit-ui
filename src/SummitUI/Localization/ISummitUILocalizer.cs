namespace SummitUI;

/// <summary>
/// Provides localized strings for SummitUI component accessibility labels and screen reader announcements.
/// </summary>
/// <remarks>
/// <para>
/// SummitUI uses this interface to retrieve localized strings for ARIA labels, screen reader
/// announcements, and other accessibility-related text. The library ships with English defaults.
/// </para>
/// <para>
/// To provide translations, you can either:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// Create resource files named <c>SummitUIResources.{culture}.resx</c> in your application's
/// Resources folder (e.g., <c>SummitUIResources.fr.resx</c> for French).
/// </description>
/// </item>
/// <item>
/// <description>
/// Register a custom <see cref="ISummitUILocalizer"/> implementation in your DI container.
/// </description>
/// </item>
/// </list>
/// </remarks>
/// <example>
/// <para>Custom implementation example:</para>
/// <code>
/// public class MySummitUILocalizer : ISummitUILocalizer
/// {
///     private readonly Dictionary&lt;string, string&gt; _translations = new()
///     {
///         ["Dialog_CloseLabel"] = "Fermer la boîte de dialogue",
///         ["Calendar_NextMonthLabel"] = "Mois suivant",
///         // ... other translations
///     };
///
///     public string this[string key] => 
///         _translations.TryGetValue(key, out var value) ? value : key;
///
///     public string this[string key, params object[] arguments] => 
///         string.Format(this[key], arguments);
/// }
///
/// // Register in Program.cs (after AddSummitUI)
/// builder.Services.AddSingleton&lt;ISummitUILocalizer, MySummitUILocalizer&gt;();
/// </code>
/// </example>
public interface ISummitUILocalizer
{
    /// <summary>
    /// Gets the localized string for the specified resource key.
    /// </summary>
    /// <param name="key">The resource key to look up.</param>
    /// <returns>The localized string, or the key itself if no translation is found.</returns>
    string this[string key] { get; }

    /// <summary>
    /// Gets the localized string for the specified resource key with format arguments.
    /// </summary>
    /// <param name="key">The resource key to look up.</param>
    /// <param name="arguments">Arguments to format into the localized string.</param>
    /// <returns>The formatted localized string, or the key itself if no translation is found.</returns>
    string this[string key, params object[] arguments] { get; }
}
