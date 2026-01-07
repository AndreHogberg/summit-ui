namespace SummitUI;

/// <summary>
/// Provides localized placeholder strings for date field segments.
/// Based on Chrome/Firefox &lt;input type="date"&gt; implementations and React Aria.
/// </summary>
public static class LocalePlaceholders
{
    private static readonly Dictionary<string, (string Year, string Month, string Day)> Placeholders = new()
    {
        ["ach"] = ("mwaka", "dwe", "nino"),
        ["af"] = ("jjjj", "mm", "dd"),
        ["am"] = ("ዓዓዓዓ", "ሚሜ", "ቀቀ"),
        ["an"] = ("aaaa", "mm", "dd"),
        ["ar"] = ("سنة", "شهر", "يوم"),
        ["ast"] = ("aaaa", "mm", "dd"),
        ["az"] = ("iiii", "aa", "gg"),
        ["be"] = ("гггг", "мм", "дд"),
        ["bg"] = ("гггг", "мм", "дд"),
        ["bn"] = ("yyyy", "মিমি", "dd"),
        ["br"] = ("bbbb", "mm", "dd"),
        ["bs"] = ("gggg", "mm", "dd"),
        ["ca"] = ("aaaa", "mm", "dd"),
        ["cak"] = ("jjjj", "ii", "q'q'"),
        ["ckb"] = ("ساڵ", "مانگ", "ڕۆژ"),
        ["cs"] = ("rrrr", "mm", "dd"),
        ["cy"] = ("bbbb", "mm", "dd"),
        ["da"] = ("åååå", "mm", "dd"),
        ["de"] = ("jjjj", "mm", "tt"),
        ["dsb"] = ("llll", "mm", "źź"),
        ["el"] = ("εεεε", "μμ", "ηη"),
        ["en"] = ("yyyy", "mm", "dd"),
        ["eo"] = ("jjjj", "mm", "tt"),
        ["es"] = ("aaaa", "mm", "dd"),
        ["et"] = ("aaaa", "kk", "pp"),
        ["eu"] = ("uuuu", "hh", "ee"),
        ["fa"] = ("سال", "ماه", "روز"),
        ["ff"] = ("hhhh", "ll", "ññ"),
        ["fi"] = ("vvvv", "kk", "pp"),
        ["fr"] = ("aaaa", "mm", "jj"),
        ["fy"] = ("jjjj", "mm", "dd"),
        ["ga"] = ("bbbb", "mm", "ll"),
        ["gd"] = ("bbbb", "mm", "ll"),
        ["gl"] = ("aaaa", "mm", "dd"),
        ["he"] = ("שנה", "חודש", "יום"),
        ["hr"] = ("gggg", "mm", "dd"),
        ["hsb"] = ("llll", "mm", "dd"),
        ["hu"] = ("éééé", "hh", "nn"),
        ["ia"] = ("aaaa", "mm", "dd"),
        ["id"] = ("tttt", "bb", "hh"),
        ["it"] = ("aaaa", "mm", "gg"),
        ["ja"] = ("年", "月", "日"),
        ["ka"] = ("წწწწ", "თთ", "რრ"),
        ["kk"] = ("жжжж", "аа", "кк"),
        ["kn"] = ("ವವವವ", "ಮಿಮೀ", "ದಿದಿ"),
        ["ko"] = ("연도", "월", "일"),
        ["lb"] = ("jjjj", "mm", "dd"),
        ["lo"] = ("ປປປປ", "ດດ", "ວວ"),
        ["lt"] = ("mmmm", "mm", "dd"),
        ["lv"] = ("gggg", "mm", "dd"),
        ["meh"] = ("aaaa", "mm", "dd"),
        ["ml"] = ("വർഷം", "മാസം", "തീയതി"),
        ["ms"] = ("tttt", "mm", "hh"),
        ["nb"] = ("åååå", "mm", "dd"),
        ["nl"] = ("jjjj", "mm", "dd"),
        ["nn"] = ("åååå", "mm", "dd"),
        ["no"] = ("åååå", "mm", "dd"),
        ["oc"] = ("aaaa", "mm", "jj"),
        ["pl"] = ("rrrr", "mm", "dd"),
        ["pt"] = ("aaaa", "mm", "dd"),
        ["rm"] = ("oooo", "mm", "dd"),
        ["ro"] = ("aaaa", "ll", "zz"),
        ["ru"] = ("гггг", "мм", "дд"),
        ["sc"] = ("aaaa", "mm", "dd"),
        ["scn"] = ("aaaa", "mm", "jj"),
        ["sk"] = ("rrrr", "mm", "dd"),
        ["sl"] = ("llll", "mm", "dd"),
        ["sr"] = ("гггг", "мм", "дд"),
        ["sv"] = ("åååå", "mm", "dd"),
        ["szl"] = ("rrrr", "mm", "dd"),
        ["tg"] = ("сссс", "мм", "рр"),
        ["th"] = ("ปปปป", "ดด", "วว"),
        ["tr"] = ("yyyy", "aa", "gg"),
        ["uk"] = ("рррр", "мм", "дд"),
        ["zh"] = ("年", "月", "日")
    };

    private static readonly (string Year, string Month, string Day) DefaultPlaceholder = ("yyyy", "mm", "dd");

    /// <summary>
    /// Gets localized placeholder strings for date field segments.
    /// </summary>
    /// <param name="locale">The locale (e.g., "en-US", "sv-SE", "de-DE").</param>
    /// <returns>A dictionary containing placeholders for year, month, day, hour, and minute.</returns>
    public static Dictionary<string, string> GetPlaceholders(string locale)
    {
        // Extract the language code from the locale (e.g., "sv-SE" → "sv")
        var lang = locale.Split('-')[0].ToLowerInvariant();

        var placeholders = Placeholders.TryGetValue(lang, out var localeData)
            ? localeData
            : DefaultPlaceholder;

        return new Dictionary<string, string>
        {
            ["year"] = placeholders.Year,
            ["month"] = placeholders.Month,
            ["day"] = placeholders.Day,
            ["hour"] = "––",
            ["minute"] = "––"
        };
    }
}
