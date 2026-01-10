namespace SummitUI.Docs.Client.Services.Search;

/// <summary>
/// Represents a searchable document in the search index.
/// </summary>
public sealed class SearchDocument
{
    /// <summary>
    /// The URL path of the document (e.g., "/docs/accordion").
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// The title of the document (e.g., "Accordion").
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// A brief description of the document content.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The category this document belongs to (e.g., "Components", "Guides").
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Additional searchable keywords/content.
    /// </summary>
    public string[]? Keywords { get; init; }

    /// <summary>
    /// Pre-computed lowercase tokens for efficient searching.
    /// </summary>
    internal string[] TitleTokens { get; set; } = [];

    /// <summary>
    /// Pre-computed lowercase tokens from description.
    /// </summary>
    internal string[] DescriptionTokens { get; set; } = [];

    /// <summary>
    /// Pre-computed lowercase tokens from keywords.
    /// </summary>
    internal string[] KeywordTokens { get; set; } = [];
}

/// <summary>
/// Represents a search result with relevance score.
/// </summary>
public sealed class SearchResult
{
    /// <summary>
    /// The matched document.
    /// </summary>
    public required SearchDocument Document { get; init; }

    /// <summary>
    /// Relevance score (higher is more relevant).
    /// </summary>
    public int Score { get; init; }

    /// <summary>
    /// Which field(s) matched the query.
    /// </summary>
    public MatchType MatchType { get; init; }
}

/// <summary>
/// Indicates which fields matched a search query.
/// </summary>
[Flags]
public enum MatchType
{
    None = 0,
    Title = 1,
    Description = 2,
    Keywords = 4
}
