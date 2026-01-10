namespace SummitUI.Docs.Client.Services.Search;

/// <summary>
/// A simple, efficient search service using token matching with weighted scoring.
/// Designed for small-to-medium documentation sites (~50 pages).
/// </summary>
public sealed class SearchService
{
    private readonly List<SearchDocument> _documents = [];
    private bool _isIndexed;

    // Scoring weights
    private const int ExactTitleMatchScore = 100;
    private const int TitleStartsWithScore = 50;
    private const int TitleContainsScore = 20;
    private const int DescriptionExactScore = 15;
    private const int DescriptionContainsScore = 5;
    private const int KeywordExactScore = 25;
    private const int KeywordContainsScore = 10;

    /// <summary>
    /// Adds a document to the search index.
    /// </summary>
    public void AddDocument(SearchDocument document)
    {
        // Pre-tokenize for efficient searching
        document.TitleTokens = Tokenize(document.Title);
        document.DescriptionTokens = Tokenize(document.Description);
        document.KeywordTokens = document.Keywords?
            .SelectMany(Tokenize)
            .ToArray() ?? [];

        _documents.Add(document);
        _isIndexed = true;
    }

    /// <summary>
    /// Adds multiple documents to the search index.
    /// </summary>
    public void AddDocuments(IEnumerable<SearchDocument> documents)
    {
        foreach (var doc in documents)
        {
            AddDocument(doc);
        }
    }

    /// <summary>
    /// Searches the index and returns ranked results.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="maxResults">Maximum number of results to return.</param>
    /// <returns>Ranked search results.</returns>
    public IReadOnlyList<SearchResult> Search(string query, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || !_isIndexed)
        {
            return [];
        }

        var queryTokens = Tokenize(query);
        if (queryTokens.Length == 0)
        {
            return [];
        }

        var queryLower = query.Trim().ToLowerInvariant();
        var results = new List<SearchResult>();

        foreach (var document in _documents)
        {
            var (score, matchType) = CalculateScore(document, queryTokens, queryLower);
            
            if (score > 0)
            {
                results.Add(new SearchResult
                {
                    Document = document,
                    Score = score,
                    MatchType = matchType
                });
            }
        }

        return results
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.Document.Title)
            .Take(maxResults)
            .ToList();
    }

    /// <summary>
    /// Clears the search index.
    /// </summary>
    public void Clear()
    {
        _documents.Clear();
        _isIndexed = false;
    }

    private (int Score, MatchType MatchType) CalculateScore(
        SearchDocument document, 
        string[] queryTokens, 
        string queryLower)
    {
        var score = 0;
        var matchType = MatchType.None;
        var titleLower = document.Title.ToLowerInvariant();

        // Check title matches (highest priority)
        if (titleLower == queryLower)
        {
            // Exact title match
            score += ExactTitleMatchScore;
            matchType |= MatchType.Title;
        }
        else if (titleLower.StartsWith(queryLower, StringComparison.Ordinal))
        {
            // Title starts with query
            score += TitleStartsWithScore;
            matchType |= MatchType.Title;
        }
        else
        {
            // Check token matches in title
            foreach (var queryToken in queryTokens)
            {
                foreach (var titleToken in document.TitleTokens)
                {
                    if (titleToken == queryToken)
                    {
                        score += TitleContainsScore * 2; // Exact token match
                        matchType |= MatchType.Title;
                    }
                    else if (titleToken.StartsWith(queryToken, StringComparison.Ordinal))
                    {
                        score += TitleContainsScore;
                        matchType |= MatchType.Title;
                    }
                    else if (titleToken.Contains(queryToken, StringComparison.Ordinal))
                    {
                        score += TitleContainsScore / 2;
                        matchType |= MatchType.Title;
                    }
                }
            }
        }

        // Check description matches
        foreach (var queryToken in queryTokens)
        {
            foreach (var descToken in document.DescriptionTokens)
            {
                if (descToken == queryToken)
                {
                    score += DescriptionExactScore;
                    matchType |= MatchType.Description;
                }
                else if (descToken.Contains(queryToken, StringComparison.Ordinal))
                {
                    score += DescriptionContainsScore;
                    matchType |= MatchType.Description;
                }
            }
        }

        // Check keyword matches
        foreach (var queryToken in queryTokens)
        {
            foreach (var keywordToken in document.KeywordTokens)
            {
                if (keywordToken == queryToken)
                {
                    score += KeywordExactScore;
                    matchType |= MatchType.Keywords;
                }
                else if (keywordToken.Contains(queryToken, StringComparison.Ordinal))
                {
                    score += KeywordContainsScore;
                    matchType |= MatchType.Keywords;
                }
            }
        }

        return (score, matchType);
    }

    /// <summary>
    /// Tokenizes text into lowercase words for searching.
    /// </summary>
    private static string[] Tokenize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return text
            .ToLowerInvariant()
            .Split([' ', '-', '_', '.', ',', '/', '(', ')', '[', ']'], 
                   StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length > 1) // Filter out single characters
            .Distinct()
            .ToArray();
    }
}
